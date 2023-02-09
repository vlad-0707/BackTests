using DocumentFormat.OpenXml.InkML;

using Fot.Dal;
using Fot.Dal.Models.Markets;

using System.Linq;

namespace Fot.Bll.Common.Market.Candles;

public class CandleLoader
{
	public CandleLoader(
		IMapper mapper,
		IExchangeStorage storage,
		IServiceProvider serviceProvider)
	{
		_mapper = mapper;
		_storage = storage;
		_serviceProvider = serviceProvider;
	}

	private readonly IMapper _mapper;
	private readonly IExchangeStorage _storage;
	private readonly IServiceProvider _serviceProvider;

	public async Task<FtCandle[]> GetCandlesAsync(int symbolId, ExchangeEnum exchangeId, long? since, long? until, TimeFrameEnum timeFrame, Func<bool> needStop)
	{
		var symbol = _serviceProvider.GetRequired<IMarketStorage>()
			.GetSymbol(symbolId, exchangeId);

		// добавить смену ТФ если больше на меньше меняем
		if (symbol.Dal.FirstCandleTimestamp is not null
			 && (since is null || since < symbol.Dal.FirstCandleTimestamp))
		{
			since = symbol.Dal.FirstCandleTimestamp - 1;
		};

		using var scope = _serviceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequired<IFotContext>();

		var candles = await context
			.Set<CandleDal>()
			.AsNoTracking()
			.Where(x => x.TimeFrameId == timeFrame
							&& x.SymbolId == symbolId
							&& (!since.HasValue || x.Timestamp > since)
							&& (!until.HasValue || x.Timestamp < until))
			.ToArrayAsync()
			.CAF();

		if (candles.Length == 0)
		{
			if (needStop())
			{
				return Array.Empty<FtCandle>();
			}

			return await GetListFromApiAsync(context, symbol, timeFrame, since, until, needStop);
		}

		var candlesArray = _mapper.Map<FtCandle[]>(candles);

		if (needStop())
		{
			return Array.Empty<FtCandle>();
		}

		var allCandles = await CheckCandlesInRangeAsync(context, candles, symbol, timeFrame, since, until, needStop);

		allCandles.AddRange(candlesArray);

		return allCandles.OrderBy(x => x.Timestamp).ToArray();
	}

	private async Task<FtCandle[]> GetListFromApiAsync(IFotContext context, SymbolBll symbol, TimeFrameEnum timeFrame, long? since, long? until, Func<bool> needStop)
	{
		var exchange = _storage.GetExchange(symbol.ExchangeId);

		var candles = await exchange.PublicExchangeApi.GetCandleListHistoryAsync(symbol.Name, timeFrame, since, until);

		var capacity = since.HasValue && until.HasValue
			? (until - since) / ((long)timeFrame * 1000) + 1
			: null;

		var list = capacity.HasValue
			? new List<FtCandle>((int)capacity.Value)
			: new List<FtCandle>();

		while (candles.Length > 0)
		{
			if (needStop())
			{
				return Array.Empty<FtCandle>();
			}
			await AddAndSaveCandlesAsync(candles, symbol.Dal.Id, timeFrame, context);

			list.AddRange(candles);
			until = candles.Last().Timestamp - 1;

			//Так как запросов 10 в секунду (точнее 20 в 2 сек)
			await Task.Delay(100);

			candles = await exchange.PublicExchangeApi.GetCandleListHistoryAsync(symbol.Name, timeFrame, since, until);
		}

		await SetFirstCandleAsync(symbol, timeFrame, list, since);

		list.Reverse();

		return list.ToArray();
	}

	private async Task<List<FtCandle>> CheckCandlesInRangeAsync(IFotContext context , CandleDal[] candles, SymbolBll symbol, TimeFrameEnum timeFrame, long? since, long? until, Func<bool> needStop)
	{
		CandleDal? prevCandle = null;
		List<FtCandle> allCandles = new();
		var targetSpace = (long)timeFrame * 1000;

		foreach (var candleDal in candles)
		{
			if (needStop())
			{
				return Array.Empty<FtCandle>().ToList();
			}

			if (prevCandle is not null && candleDal.Timestamp - prevCandle.Timestamp != targetSpace)
			{
				var arr = await GetListFromApiAsync(context, symbol, timeFrame, prevCandle.Timestamp, candleDal.Timestamp, needStop);
				allCandles.AddRange(arr);
			}

			prevCandle = candleDal;
		}

		var firstCandle = candles.First();
		var lastCandle = candles.Last();

		if (!since.HasValue || firstCandle.Timestamp - targetSpace > since + 1)
		{
			var arr = await GetListFromApiAsync(context, symbol, timeFrame, since, firstCandle.Timestamp, needStop);
			allCandles.AddRange(arr);
		}

		if (!until.HasValue || lastCandle.Timestamp + targetSpace < until)
		{
			var arr = await GetListFromApiAsync(context, symbol, timeFrame, lastCandle.Timestamp, until, needStop);
			allCandles.AddRange(arr);
		}

		return allCandles;
	}

	private Task AddAndSaveCandlesAsync(FtCandle[] newCandles, int symbolId, TimeFrameEnum timeFrame, IFotContext context)
	{
		foreach (var candle in newCandles)
		{
			var cand = _mapper.Map<CandleDal>(candle);
			cand.SymbolId = symbolId;
			cand.TimeFrameId = timeFrame;

			context.Add(cand);
		}

		return context.SaveChangesAsync();
	}

	private async Task SetFirstCandleAsync(SymbolBll symbol, TimeFrameEnum timeFrame, List<FtCandle> list, long? since)
	{
		var targetSpace = (long)timeFrame * 1000;

		if (symbol.Dal.FirstCandleTimestamp is not null && symbol.Dal.FirstCandleTimeFrameId <= timeFrame
		    || since is not null && list.Last().Timestamp - targetSpace <= since)
		{
			return;
		}

		using var scope = _serviceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequired<IFotContext>();

		var symbolDal = await context
			.Set<SymbolDal>()
			.SingleAsync(x => x.Id == symbol.Dal.Id)
			.CAF();

		symbol.Dal.FirstCandleTimestamp = symbolDal.FirstCandleTimestamp = list.Last().Timestamp;
		symbol.Dal.FirstCandleTimeFrameId = symbolDal.FirstCandleTimeFrameId = timeFrame;

		await context.SaveChangesAsync();
	}
}
