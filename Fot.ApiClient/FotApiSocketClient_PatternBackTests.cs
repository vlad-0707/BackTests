using Fot.Dto;
using Fot.Dto.Patterns.BackTests;

using Microsoft.AspNetCore.SignalR.Client;
namespace Fot.ApiClient;

public partial class FotApiSocketClient
{
	public event Func<int, Task> StopBackTestRequested = _ => Task.CompletedTask;
	public event Func<int, Task> StartBackTestRequested = _ => Task.CompletedTask;
	public event Func<int, Task> ResetBackTestRequested = _ => Task.CompletedTask;

	private void SubscribeToPatternBackTests()
	{
		Сonnection.On<int>(SocketEndpoints.BackTests.StopBackTest, OnStopBackTest);
		Сonnection.On<int>(SocketEndpoints.BackTests.StartBackTest, OnStartBackTest);
		Сonnection.On<int>(SocketEndpoints.BackTests.ResetBackTest, OnResetBackTest);
	}

	private void OnStopBackTest(int id) => _ = Task.Run(() => StopBackTestRequested.Invoke(id));
	private void OnStartBackTest(int id) => _ = Task.Run(() => StartBackTestRequested.Invoke(id));
	private void OnResetBackTest(int id) => _ = Task.Run(() => ResetBackTestRequested.Invoke(id));

	public void SendBackTestPartState(BackTestPartStateDto partDto)
		=> SendData(SocketEndpoints.BackTests.BackTestPartStateChanged, partDto);

	public void SendBackTestState(BackTestStateDto backTestStateDto)
		=> SendData(SocketEndpoints.BackTests.BackTestStateChanged, backTestStateDto);
}
