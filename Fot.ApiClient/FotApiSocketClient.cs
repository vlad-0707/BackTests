#nullable disable
using Fot.BLL.Configs;
using Fot.Dto;
using Fot.Dto.Messages;
using FT.Extending;
using FT.Extending.Extenders;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fot.ApiClient;

public partial class FotApiSocketClient : IAsyncDisposable
{
	public FotApiSocketClient(
		IOptions<FotApiConfig> options,
		IServiceProvider serviceProvider,
		ILogger<FotApiSocketClient> logger)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
		_socketClientConfig = options.Value;
	}

	//Internal for module
	internal HubConnection Сonnection { get; private set; }
	public HubConnectionState State => Сonnection?.State ?? HubConnectionState.Disconnected;

	public event Func<Exception, Task> ConnectionClosed = _ => Task.CompletedTask;
	public event Func<Task> ConnectionReconnected = () => Task.CompletedTask;

	private bool _isConnect;
	private readonly FotApiConfig _socketClientConfig;
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<FotApiSocketClient> _logger;

	private void SubscribeAll()
	{
		SubscribeEvents();
		SubscribeUsers();
		SubscribeAccounts();
		SubscribeStrategies();
		SubscribeOrders();
		SubscribeServices();
		SubscribeCharts();
		SubscribeDotnetCounters();
		SubscribeExpressions();
		SubscribeMarkets();
		SubscribePatterns();
		SubscribeSymbols();
		SubscribePatternSteps();
		SubscribeToPatternBackTests();


		Сonnection.Reconnected += OnReconnected;
		Сonnection.Closed += TryReconnectAsync;
	}

	private async Task OnReconnected(string arg)
	{
		await ConnectionReconnected.Invoke().CAF();
	}

	#region Connection

	public async Task TryConnectAsCheckerAsync()
	{
		await TryConnectAsync().CAF();
		RegisterChecker();
	}

	public async Task TryConnectAsync()
	{
		while (!_isConnect)
		{
			try
			{
				await ConnectAsync().CAF();
			}
			catch (Exception e)
			{
				_logger.LogError("{nameof(FotApiSocketClient)} connection failed. {e.GetShortTextWithInner()}", nameof(FotApiSocketClient), e.GetShortTextWithInner());
				await Task.Delay(_socketClientConfig.SocketWaitUntilReconnectMls).CAF();
			}
		}
	}

	private async Task ConnectAsync()
	{
		await CreateConnectionAsync().CAF();
		_logger.LogInformation("Try connect to socket");
		await Сonnection.StartAsync(CancellationToken.None).CAF();

		_isConnect = true;

		_logger.LogInformation("Socket connected!");
	}

	private async Task CreateConnectionAsync()
	{
		var oldConnection = Сonnection;

		_logger.LogInformation("Try create connection");
		Сonnection = new HubConnectionBuilder()
			.WithUrl(_socketClientConfig.SocketUrl,
				options =>
				{
					options.TransportMaxBufferSize = 200L * 1024 * 1024;
				})
			.AddNewtonsoftJsonProtocol()
			.Build();

		SubscribeAll();

		if (oldConnection != null)
		{
			oldConnection.Closed -= TryReconnectAsync;
			oldConnection.Reconnected -= OnReconnected;
			await oldConnection.DisposeAsync().CAF();
		}
	}

	private async Task TryReconnectAsync(Exception exception)
	{
		await ConnectionClosed.Invoke(exception);
		_logger.LogError("{nameof(FotApiSocketClient)} conneсtion on closed: {exception.GetShortTextWithInner()}", nameof(FotApiSocketClient), exception.GetShortTextWithInner());
		_isConnect = false;
		await TryConnectAsync().CAF();
		await ConnectionReconnected.Invoke().CAF();
	}

	public void RegisterChecker()
	{
		Сonnection.InvokeAsync(SocketEndpoints.Applications.RegisterCheckService);
	}

	#endregion

	public void SendMessage(MessageDto message)
	{
		SendData(SocketEndpoints.ReceiveMessage, message);
	}

	internal void SendData<T>(string endpoint, T data, Action startAction = null, Action endAction = null, Action finalyAction = null)
	{
		_ = Task.Run(async () => await TrySendDataAsync(endpoint, data, startAction, endAction, finalyAction).CAF());
	}

	private async Task TrySendDataAsync<T>(string endpoint, T data, Action startAction, Action endAction, Action finalyAction)
	{
		try
		{
			if (Сonnection == null)
			{
				return;
			}

			startAction?.Invoke();
			await Сonnection.InvokeAsync(endpoint, data).CAF();
			endAction?.Invoke();
		}
		catch
		{
			//_logger.LogError($"Send data to {endpoint}: {e.GetFullTextWithInnerExcludeLite()}");
		}
		finally
		{
			finalyAction?.Invoke();
		}
	}

	public ValueTask DisposeAsync()
	{
		if (Сonnection is null)
		{
			return ValueTask.CompletedTask;
		}

		Сonnection.Reconnected -= OnReconnected;
		Сonnection.Closed -= TryReconnectAsync;

		return Сonnection.DisposeAsync();
	}
}
