using Fot.ApiClient;
using Fot.Bll.Common.Market.Candles;
using Fot.Bll.Common.Patterns.Steps;
using Fot.Common.Patterns;
using Fot.Dal;
using Fot.Dal.Models.Patterns;
using Fot.Dal.Models.Patterns.BackTests;
using Fot.Dto.Patterns.BackTests;

namespace Fot.Bll.Common.Patterns.BackTests;

public class BackTestProcessor : IBackTestProcessor
{
	public BackTestProcessor(
		IMapper mapper,
		IFotContext context,
		IServiceProvider serviceProvider,
		FotApiSocketClient socketClient,
		ILogger<BackTestProcessor> logger,
		CandleLoader candleLoader)
	{
		_mapper = mapper;
		_logger = logger;
		_context = context;
		_socketClient = socketClient;
		_candleLoader = candleLoader;
		_serviceProvider = serviceProvider;
	}

	private readonly IMapper _mapper;
	private readonly IFotContext _context;
	private readonly CandleLoader _candleLoader;
	private readonly FotApiSocketClient _socketClient;
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<BackTestProcessor> _logger;

	private readonly List<BackTestResultBll> _results = new();
	private readonly BackTestPartStateDto _partStateDto = new();

	private SymbolBll _symbol;
	private PatternBll _pattern;
	private BackTestDal _backTest;
	private BackTestPartDal _part;
	private Func<bool> _needStopFunc;
	private ICollection<BackTestsPartPnlSettingDal> _pnlSettings;

	private LinkedListNode<CandleBll> _currentCandle;

	private string? _logId;
	private string LogId => _logId ??= $"PT#{_pattern.Id}BT#{_part.BackTestId}P#{_part.Id}S#{_symbol.Id}. ";

	public IBackTestProcessor Init(PatternBll pattern, BackTestPartDal part, Func<bool> needStopFunc)
	{
		_part = part;
		_pattern = pattern;
		_backTest = part.BackTest;
		_pnlSettings = part.PnlSettings;
		_partStateDto.UserId = _backTest.UserId;

		_needStopFunc = needStopFunc;

		//TODO: переделать так, чтобы работать с неактивными инструментами
		_symbol = _serviceProvider.GetRequired<IMarketStorage>().GetSymbol(_part.SymbolId, ExchangeEnum.OKEx);

		_logger.LogTrace("{LogId} Проинициализирован бэктест паттерна {_pattern.Name} для символа  {_symbol.Name} и ТФ {_part.TimeFrameId}",
			LogId, _pattern.Name, _symbol.Name, _part.TimeFrameId);

		return this;
	}

	public async Task TryProcessPartAsync()
	{
		try
		{
			_part = await _context.Set<BackTestPartDal>()
				.SingleAsync(x => x.Id == _part.Id);

			await SetPartStatusAsync(BackTestStatusEnum.Running);

			await ProcessPartAsync();

			var isComplete = !_needStopFunc();
			_part.CompletionTime = isComplete ? DateTime.Now : null;
			await SetPartStatusAsync(isComplete ? BackTestStatusEnum.Finish : BackTestStatusEnum.Stopped);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Ошибка с BackTestPart {_part}: {e.GetFullTextWithInnerExcludeLite()}",
				_part, e.GetFullTextWithInnerExcludeLite());

			await SetPartStatusAsync(BackTestStatusEnum.Error);
		}
	}

	private async Task ProcessPartAsync()
	{
		var candles = await GetCandlesForPartAsync();
		if (candles.Length <= 1)
		{
			return;
		}
		return;

		_pattern.ClearPotencialAutoIncidents();

		var linkedList = new LinkedList<CandleBll>(candles.Take(300).Select(x => new CandleBll(x)));
		await AnalyzeAsync(linkedList.Last);

		foreach (var candle in candles.Skip(300))
		{
			var old = linkedList.First;
			linkedList.RemoveFirst();

			old.Value.ChangeCandle(candle);
			linkedList.AddLast(old);

			await AnalyzeAsync(linkedList.Last);
		}

		if (!_results.Any())
		{
			return;
		}

		var maxCheckAfterMilliseconds = _pnlSettings.Max(x => x.CheckAfterMilliseconds);
		var maxPnlTimestamp = _results.Max(x => x.Dal.EndTimestamp) + maxCheckAfterMilliseconds;

		if (maxPnlTimestamp < candles.Last().Timestamp)
		{
			return;
		}

		var until = maxPnlTimestamp + (long)_part.TimeFrameId;

		candles = await _candleLoader.GetCandlesAsync(_symbol.Dal.Id, _symbol.ExchangeId, candles.Last().Timestamp,
			until, _part.TimeFrameId, _needStopFunc);

		foreach (var candle in candles)
		{
			if (_needStopFunc())
			{
				return;
			}

			var old = linkedList.First;
			linkedList.RemoveFirst();

			old.Value.ChangeCandle(candle);
			linkedList.AddLast(old);

			_currentCandle = linkedList.Last;
			await AnalyzeResultsAsync();
		}
	}

	private Task<FtCandle[]> GetCandlesForPartAsync()
	{
		var since = _backTest.Since?.ToUnixTimeMilliseconds() - 1;
		var until = _backTest.Until?.ToUnixTimeMilliseconds();

		return _candleLoader.GetCandlesAsync(_part.SymbolId, _symbol.ExchangeId, since, until, _part.TimeFrameId, _needStopFunc);
	}

	private async Task AnalyzeAsync(LinkedListNode<CandleBll> candle)
	{
		await AnalyzePatternBllAsync(candle);
		await AnalyzeResultsAsync();

		_part.LastCalcTime = DateTime.Now;
		_part.LastCalcTimestamp = candle.Value.Candle.Timestamp;
		await _context.SaveChangesAsync();

		SendPartStateToSocket();
	}

	private async Task SetPartStatusAsync(BackTestStatusEnum status)
	{
		_part.StatusId = status;
		await _context.SaveChangesAsync();

		SendPartStateToSocket(true);
	}

	private DateTime? _lastSendTime;

	private void SendPartStateToSocket(bool forceSend = false)
	{
		if (!forceSend
			 && _lastSendTime.HasValue
			 && (DateTime.Now - _lastSendTime.Value).TotalSeconds < 1)
		{
			return;
		}

		_mapper.Map(_part, _partStateDto);

		_socketClient.SendBackTestPartState(_partStateDto);
		_lastSendTime = DateTime.Now;
	}

	private async Task AnalyzePatternBllAsync(LinkedListNode<CandleBll> candle)
	{
		_currentCandle = candle;
		var firstStep = _pattern.Steps.First!;

		AnalyzeZeroStep(firstStep);

		foreach (var pAutoIncident in _pattern.PotentialAutoIncidents.ToArray())
		{
			await AnalyzePotentialAutoIncidentAsync(pAutoIncident).CAF();
		}
	}

	private async Task AnalyzeResultsAsync()
	{
		foreach (var resultBll in _results)
		{
			await CreatePnlResultAsync(resultBll);
		}
	}

	private async Task CreatePnlResultAsync(BackTestResultBll result)
	{
		var min = result.PnlSettings.FirstOrDefault();

		if (min is null
			|| result.Dal.EndTimestamp + min.CheckAfterMilliseconds > _currentCandle.Value.Candle.Timestamp)
		{
			return;
		}

		var pnlDalResult = new BackTestResultPnlDal
		{
			LongResult = _currentCandle.Value.Candle.Open / result.Candle.Close,
			BackTestResultId = result.Dal.Id,
			BackTestSettingId = min.Id
		};

		_context.Add(pnlDalResult);
		await _context.SaveChangesAsync();

		result.PnlSettings.Remove(min);

		if (!result.PnlSettings.Any())
		{
			_results.Remove(result);
		}
	}

	private void AnalyzeZeroStep(LinkedListNode<AbstractStep> step)
	{
		var candle = _currentCandle.Value;
		if (candle.PatternSendList.ContainsKey(_pattern.Id))
		{
			return;
		}

		var result = step.Value.CheckAsZeroStep(_currentCandle, _symbol);
		if (result.Status != PotentialAutoIncidentStatusEnum.Success)
		{
			return;
		}

		var nextStep = step.Value.NeedCloseCandle && _currentCandle.Next is null ? step : step.Next!;

		var dal = new PotentialAutoIncidentDal
		{
			SymbolId = _symbol.Id,
			StartTimestamp = candle.Candle.Timestamp,
			StatusId = PotentialAutoIncidentStatusEnum.InProgress,
			StepId = nextStep.Value.Dal.Id,
			LowerLevelZone = result.LowerLevelZone!.Value,
			UpperLevelZone = result.UpperLevelZone!.Value,
			PrefStepTimestamp = candle.Candle.Timestamp
		};

		var pAutoIncident = _serviceProvider.GetRequired<PotentialAutoIncidentBll>()
			.Init(dal, _symbol, _currentCandle, nextStep,_pattern)
			.SetLastAnalyzeCandle(_currentCandle);

		_pattern.PotentialAutoIncidents.Add(pAutoIncident);

		if (pAutoIncident.FirstCandle is not null
			&& pAutoIncident.StopCandle is null
			&& _pattern.Dal.StopBaseCandle == PatternStopBaseCandleEnum.NextFirst
			&& _currentCandle.Value.Candle.Timestamp > pAutoIncident.FirstCandle.Timestamp)
		{
			pAutoIncident.StopCandle = _currentCandle.Value.Candle;
		}

		//_logger.LogTrace("{LogId} Найден потенциальный автоинцидент. Начало: {dal.StartTimestamp}",
		//	LogId, dal.StartTimestamp);
	}

	private async Task AnalyzePotentialAutoIncidentAsync(PotentialAutoIncidentBll pAutoIncident)
	{
		var isClose = pAutoIncident.CheckIsCloseCandleIfNeeded();
		if (isClose.HasValue)
		{
			if (!isClose.Value)
			{
				return;
			}
			var finished = await MoveStepOrFinishPotentialAutoIncidentAsync(pAutoIncident).CAF();
			if (finished)
			{
				return;
			}
		}

		var result = pAutoIncident.CheckCurrentStep();
		switch (result.Status)
		{
			case PotentialAutoIncidentStatusEnum.InProgress:
				//if (result.CandleFinish!.Next is not null)
				//{
				//	pAutoIncident.SetLastAnalyzeCandle(result.CandleFinish!);
				//}
				return;

			case PotentialAutoIncidentStatusEnum.Failed or PotentialAutoIncidentStatusEnum.Cancelled:

				//_logger.LogTrace("{LogId} Потенциальный автоинцидент {result.Status}",
				//	LogId, result.Status);
				_pattern.PotentialAutoIncidents.Remove(pAutoIncident);
				return;
		}

		if (result.CandleFinish is null)
		{
			throw new LiteException($"Из успешного шага в инциденте #{pAutoIncident.Id} не вернули свечу");
		}

		pAutoIncident.SetLastStepCandle(result.CandleFinish);

		if (pAutoIncident.CurrentStep.Value.NeedCloseCandle && result.CandleFinish!.Next is null)
		{
			pAutoIncident.SetLastAnalyzeCandle(result.CandleFinish);
			return;
		}

		await MoveStepOrFinishPotentialAutoIncidentAsync(pAutoIncident).CAF();
	}

	private async Task<bool> MoveStepOrFinishPotentialAutoIncidentAsync(PotentialAutoIncidentBll pAutoIncident)
	{
		//Если не налл - значит мы сделали переход на новый шаг
		if (pAutoIncident.MoveStep() != null)
		{
			//_logger.LogTrace("{LogId} Потенциальный автоинцидент переход на шаг: {pAutoIncident.StepId}",
			//	LogId, pAutoIncident.StepId);
			return false;
		}

		//Если налл - значит это последний - ура, нашли инцидент
		_pattern.PotentialAutoIncidents.Remove(pAutoIncident);

		await SaveBackTestResults(pAutoIncident).CAF();
		return true;
	}

	private async Task SaveBackTestResults(PotentialAutoIncidentBll pAutoIncident)
	{
		using var scope = _serviceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequired<IFotContext>();

		var stopCandle = FindCandleForStop(pAutoIncident);

		pAutoIncident.LastCandle = pAutoIncident.LastStepCandle!.Value.Candle;

		if (pAutoIncident.LastCandle is not null
			&& pAutoIncident.StopCandle is null
			&& _pattern.Dal.StopBaseCandle == PatternStopBaseCandleEnum.NextLast
			&& _currentCandle.Value.Candle.Timestamp > pAutoIncident.LastCandle.Timestamp)
		{
			pAutoIncident.StopCandle = _currentCandle.Value.Candle;
		}

		var result = new BackTestResultDal
		{
			StartLow = pAutoIncident.FirstCandle.Low,
			StartHigh = pAutoIncident.FirstCandle.High,
			StartOpen = pAutoIncident.FirstCandle.Open,
			StartClose = pAutoIncident.FirstCandle.Close,
			StartVolume = pAutoIncident.FirstCandle.Volume,
			StartTimestamp = pAutoIncident.FirstCandle.Timestamp,

			EndLow = pAutoIncident.LastCandle.Low,
			EndOpen = pAutoIncident.LastCandle.Open,
			EndHigh = pAutoIncident.LastCandle.High,
			EndClose = pAutoIncident.LastCandle.Close,
			EndVolume = pAutoIncident.LastCandle.Volume,
			EndTimestamp = pAutoIncident.LastCandle.Timestamp,

			BackTestPartId = _part.Id,
			StopBaseCandleLow = stopCandle.Low,
			StopBaseCandleOpen = stopCandle.Open,
			StopBaseCandleHigh = stopCandle.High,
			StopBaseCandleClose = stopCandle.Close,
			StopBaseCandleVolume = stopCandle.Volume,
			StopBaseCandleTimestamp = stopCandle.Timestamp,
			//StartTimestamp = pAutoIncident.Dal.StartTimestamp,
			
		};

		_logger.LogTrace("{LogId} Потенциальный автоинцидент успешный в диапазоне: {result.StartTimestamp}-{result.EndTimestamp}",
			LogId, result.StartTimestamp, result.EndTimestamp);

		context.Add(result);
		await context.SaveChangesAsync();

		var stopPrice = GetStopPrice(result);

		var resultBll = _serviceProvider.GetRequired<BackTestResultBll>().Init(result, _part, _pnlSettings, pAutoIncident.LastStepCandle.Value.Candle);
		_results.Add(resultBll);

	}

	private FtCandle? FindCandleForStop(PotentialAutoIncidentBll pAutoIncident)
	{
		if (_pattern.Dal.StopBaseCandle == PatternStopBaseCandleEnum.PrevLast)
		{
			return pAutoIncident.StopCandle = _currentCandle.Previous.Value.Candle;
		}
		else if (_pattern.Dal.StopBaseCandle == PatternStopBaseCandleEnum.Last)
		{
			return pAutoIncident.StopCandle = _currentCandle.Value.Candle;
		}

		return pAutoIncident.StopCandle;
	}

	private decimal? GetStopPrice(BackTestResultDal result)
	{
		//надо взять базовую цену(negativePrice)
		//положительную цену
		//негативную цену
		//цена стопа = базовая цена + (пол - нег) * стопдев

		var basePrice = GetStopPrice(result, _pattern.Dal.StopDeviationBaseLine);
		var positivePrice = GetStopPrice(result, _pattern.Dal.StopDeviationPositiveOperand);
		var negativePrice = GetStopPrice(result, _pattern.Dal.StopDeviationNegativeOperand);
		
		return basePrice + (positivePrice - negativePrice) * _pattern.Dal.StopDeviation;
	}

	private decimal? GetStopPrice(BackTestResultDal result, BaseLineEnum? patternStopDeviation)
	{
			switch (patternStopDeviation)
			{
				case BaseLineEnum.None:
					break;   //доделать

				case BaseLineEnum.Open:
					return result.StopBaseCandleOpen;

				case BaseLineEnum.Close:
					return result.StopBaseCandleClose;

				case BaseLineEnum.Low:
					return result.StopBaseCandleLow;

				case BaseLineEnum.High:
					return result.StopBaseCandleHigh;

				case BaseLineEnum.Top:
					return Math.Max(result.StopBaseCandleOpen.Value, result.StopBaseCandleClose.Value);

				case BaseLineEnum.Bottom:
					return Math.Min(result.StopBaseCandleOpen.Value, result.StopBaseCandleClose.Value);

				case BaseLineEnum.HLMedian:
					return (result.StopBaseCandleHigh - result.StopBaseCandleLow) / 2;

				case BaseLineEnum.TBMedian:
					return Math.Abs(Math.Max(result.StopBaseCandleOpen.Value, result.StopBaseCandleClose.Value)
						- Math.Min(result.StopBaseCandleOpen.Value, result.StopBaseCandleClose.Value));
					
				case BaseLineEnum.Other:
					break;

				default:
					break;
			}
		
		return null;
	}
	
}
