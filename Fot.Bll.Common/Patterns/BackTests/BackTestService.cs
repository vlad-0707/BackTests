using Fot.ApiClient;
using Fot.Common.Patterns;
using Fot.Dal;
using Fot.Dal.Models.Patterns;
using Fot.Dal.Models.Patterns.BackTests;
using Fot.Dal.Patterns.BackTests;
using Fot.Dto.Patterns.BackTests;

using FT.Extending.Services;

namespace Fot.Bll.Common.Patterns.BackTests;

public class BackTestService : AbstractService
{
	public BackTestService(
		IMapper mapper,
		ILogger<BackTestService> logger,
		FotApiSocketClient socketClient,
		IServiceProvider serviceProvider
		)
		: base(serviceProvider, logger)
	{
		_mapper = mapper;
		_socketClient = socketClient;

		_socketClient.StopBackTestRequested += TryStopBackTestAsync;
		_socketClient.StartBackTestRequested += TryStartBackTestAsync;
		_socketClient.ResetBackTestRequested += TryResetBackTestAsync;
	}

	private readonly IMapper _mapper;
	private readonly FotApiSocketClient _socketClient;
	//Сразу инитим закрытый семафор, чтобы пока мы не начали обработку запросы пользователя ждали
	private readonly SemaphoreSlim _runningSemaphore = new(0, 1);

	private bool _needStop = false;

	public override Type[] InitializeDependenciesTypes
		=> new[] { typeof(IMarketStorage) };

	protected override async Task ExecuteServiceAsync()
	{
		try
		{
			await RunNextAsync();
		}
		finally
		{
			_runningSemaphore.Release();
		}
	}

	#region Start

	private async Task TryStartBackTestAsync(int id)
	{
		try
		{
			await StartBackTestAsync(id);
		}
		catch (Exception e)
		{
			Logger.LogError("Error on start back-test #{id}: {e.GetFullTextWithInnerExcludeLite()}",
				id, e.GetFullTextWithInnerExcludeLite());
		}
	}

	private async Task StartBackTestAsync(int id)
	{
		bool canRun;
		BackTestDal? backTest;

		await _runningSemaphore.WaitAsync().CAF();
		try
		{
			using var scope = ServiceProvider.CreateScope();
			var context = scope.ServiceProvider.GetRequired<IFotContext>();

			backTest = await context
				.Set<BackTestDal>()
				.SingleOrDefaultAsync(x => x.Id == id)
				.CAF();

			if (backTest is null)
			{
				Logger.LogError("Бэк-тест по ИД = {id} не найден", id);
				return;
			}

			if (backTest.StatusId == BackTestStatusEnum.Running)
			{
				Logger.LogTrace("Бэк-тест {backTest} уже запущен", backTest);
				return;
			}

			canRun = ! await context
				.Set<BackTestDal>()
				.AnyAsync(x => x.StatusId == BackTestStatusEnum.Running);

			if (canRun)
			{
				backTest.StartTime = DateTime.Now;
				backTest.StatusId = BackTestStatusEnum.Running;

				_socketClient.SendBackTestState(_mapper.Map<BackTestStateDto>(backTest));
			}
			else
			{
				backTest.StatusId = BackTestStatusEnum.Wait;

				_socketClient.SendBackTestState(_mapper.Map<BackTestStateDto>(backTest));
			}

			await context.SaveChangesAsync();
		}
		finally
		{
			_runningSemaphore.Release();
		}

		if (canRun)
		{
			_ = Task.Run(() => TryProcessBackTestAsync(backTest));
		}
	}

	private async Task TryProcessBackTestAsync(BackTestDal backTest)
	{
		try
		{
			await ProcessBackTestAsync(backTest);
		}
		catch (Exception e)
		{
			Logger.LogError(e, "Ошибка c BackTest {backTest}: {e.GetFullTextWithInnerExcludeLite()}",
				backTest, e.GetFullTextWithInner());

			await FinishAndRunNextAsync(backTest, BackTestStatusEnum.Error);
		}
	}

	private async Task ProcessBackTestAsync(BackTestDal backTest)
	{
		using (var scope = ServiceProvider.CreateScope())
		{
			var context = scope.ServiceProvider.GetRequired<IFotContext>();

			backTest = await context.Set<BackTestDal>()
				.AsNoTracking()
				.Include(x => x.Parts)
					.ThenInclude(x => x.Symbol)
				.Include(x => x.Parts)
					.ThenInclude(x => x.PnlSettings)
				.SingleAsync(x => x.Id == backTest.Id)
				.CAF();
		}

		var pattern = await GetPattern(backTest.PatternId);
		var patternBll = await ServiceProvider.GetRequired<PatternBll>().InitAsync(pattern).CAF();

		foreach (var part in backTest.Parts)
		{
			if (IsStopState)
			{
				return;
			}

			if (_needStop)
			{
				break;
			}

			await ProcessPartAsync(patternBll, part);
		}

		await FinishAndRunNextAsync(backTest, _needStop ? BackTestStatusEnum.Stopped : BackTestStatusEnum.Finish);
	}

	private async Task RunNextAsync()
	{
		try
		{
			var backTest = await GetRunningBackTestAsync();
			if (backTest is not null)
				_ = Task.Run(() => TryProcessBackTestAsync(backTest));
		}
		catch (Exception e)
		{
			Logger.LogError(e, "Ошибка поиска следующего для запуска бэктеста {e.GetFullTextWithInnerExcludeLite()}",
				e.GetFullTextWithInnerExcludeLite());
		}
	}

	private async Task<BackTestDal?> GetRunningBackTestAsync()
	{
		using var scope = ServiceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequired<IFotContext>();

		var backTest = await context
			.Set<BackTestDal>()
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.StatusId == BackTestStatusEnum.Running)
			.CAF();

		if (backTest is not null)
		{
			return backTest;
		}

		backTest = await context
			.Set<BackTestDal>()
			.FirstOrDefaultAsync(x => x.StatusId == BackTestStatusEnum.Wait)
			.CAF();

		if (backTest is not null)
		{
			backTest.StartTime = DateTime.Now;
			backTest.StatusId = BackTestStatusEnum.Running;
			await context.SaveChangesAsync();
		}

		return backTest;
	}

	private async Task ProcessPartAsync(PatternBll pattern, BackTestPartDal part)
	{
		using var scope = ServiceProvider.CreateScope();
		await scope.ServiceProvider
			.GetRequired<IBackTestProcessor>()
			.Init(pattern, part, () => IsStopState || _needStop)
			.TryProcessPartAsync();
	}

	private async Task<PatternDal> GetPattern(int patternId)
	{
		using var scope = ServiceProvider.CreateScope();

		return await scope.ServiceProvider
			       .GetRequired<IPatternExpressionManager>()
			       .GetWithExpressionAsync(patternId, true)
		       ?? throw new LiteException($"Не найден паттерн с ИД = {patternId}");
	}


	#endregion

	#region Stop

	private async Task TryStopBackTestAsync(int id)
	{
		try
		{
			await StopBackTestAsync(id);
		}
		catch (Exception e)
		{
			Logger.LogError("Error on stop back-test #{id}: {e.GetFullTextWithInnerExcludeLite()}",
				id, e.GetFullTextWithInnerExcludeLite());
		}
	}

	private async Task StopBackTestAsync(int id)
	{
		using var scope = ServiceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequired<IFotContext>();

		//Остановку в БД тоже надо синхронить, чтобы 
		await _runningSemaphore.WaitAsync();

		try
		{
			//Запущенный кешим и тогда проверять достаточно из кеша сравнением ИД,
			var backTest = await context
				.Set<BackTestDal>()
				.FirstOrDefaultAsync(x => x.Id == id && (x.StatusId == BackTestStatusEnum.Running || x.StatusId == BackTestStatusEnum.Wait))
				.CAF();

			switch (backTest?.StatusId)
			{
				case BackTestStatusEnum.Running:
					// а то стопнуть может не тот, что передан ИД
					_needStop = true;

					break;
				case BackTestStatusEnum.Wait:
					backTest.StatusId = BackTestStatusEnum.Stopped;
					await context.SaveChangesAsync();

					_socketClient.SendBackTestState(_mapper.Map<BackTestStateDto>(backTest));
					break;
			}
		}
		finally
		{
			_runningSemaphore.Release();
		}
	}

	private async Task FinishAndRunNextAsync(BackTestDal backTest, BackTestStatusEnum status)
	{
		await _runningSemaphore.WaitAsync();

		try
		{
			using (var scope = ServiceProvider.CreateScope())
			{
				var context = scope.ServiceProvider.GetRequired<IFotContext>();

				backTest = await context.Set<BackTestDal>()
					.SingleAsync(x => x.Id == backTest.Id);

				backTest.StatusId = status;
				await context.SaveChangesAsync();

				_socketClient.SendBackTestState(_mapper.Map<BackTestStateDto>(backTest));
			}

			await RunNextAsync();
		}
		finally
		{
			_needStop = false;
			_runningSemaphore.Release();
		}
	}

	#endregion

	private async Task TryResetBackTestAsync(int id)
	{
		using var scope = ServiceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequired<IFotContext>();

		BackTestDal? backTest = null;

		await _runningSemaphore.WaitAsync();

		try
		{
			backTest = await context
				.Set<BackTestDal>()
				.Include(x => x.Parts)
				.FirstOrDefaultAsync(x => x.Id == id)
				.CAF();

			if (backTest is null)
			{
				Logger.LogWarning("Бэк-тест c Id #{id} не найден", id);
				return;
			}

			if (backTest.StatusId is not (BackTestStatusEnum.Error or BackTestStatusEnum.Finish
			    or BackTestStatusEnum.Stopped))
			{
				Logger.LogWarning("Бэк-тест c Id #{id} имеет статус, который нельзя сбросить", id);
				return;
			}

			backTest.StartTime = null;
			backTest.StatusId = BackTestStatusEnum.Dropped;

			var backTestRepository = scope.ServiceProvider.GetRequired<IBackTestRepository>();
			await backTestRepository.RemoveResultPnlsAsync(id);
			await backTestRepository.RemoveResultsAsync(id);
			await backTestRepository.ResetBackTestPartAsync(id);

			await context.SaveChangesAsync();
			Logger.LogTrace("Бэк-тест {backTest.Id} сброшен", backTest.Id);

			_socketClient.SendBackTestState(_mapper.Map<BackTestStateDto>(backTest));
		}
		catch (Exception e)
		{
			Logger.LogError(e, "Ошибка сброса состояния {backTest}: {e.GetFullTextWithInnerExcludeLite()}",
				backTest, e.GetFullTextWithInner());
		}
		finally
		{
			_runningSemaphore.Release();
		}
	}

	protected override Task StopServiceAsync()
	{
		_socketClient.StartBackTestRequested -= TryStartBackTestAsync;
		_socketClient.StopBackTestRequested -= TryStopBackTestAsync;
		_socketClient.ResetBackTestRequested -= TryResetBackTestAsync;

		return base.StopServiceAsync();
	}
}
