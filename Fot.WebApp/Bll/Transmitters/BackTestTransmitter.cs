using Fot.Bll.Common.Patterns.BackTests;
using Fot.Dto.Patterns.BackTests;

namespace Fot.WebApp.Bll.Transmitters;

public class BackTestTransmitter : BaseTransmitter, IBackTestTransmitter
{
	public BackTestTransmitter(
		ILogger<BackTestTransmitter> logger,
		IServiceProvider serviceProvider)
		: base(logger, serviceProvider)
	{
	}

	public Task StartBackTestAsync(int id)
		=> TrySendToConsoleAsync(SocketEndpoints.BackTests.StartBackTest, id);

	public Task StopBackTestAsync(int id)
		=> TrySendToConsoleAsync(SocketEndpoints.BackTests.StopBackTest, id);

	public Task ResetBackTestAsync(int id)
		=> TrySendToConsoleAsync(SocketEndpoints.BackTests.ResetBackTest, id);

	public Task PartStateChanged(BackTestPartStateDto partDto)
		=> TrySendToUser(partDto.UserId, SocketEndpoints.BackTests.BackTestPartStateChanged, partDto);

	public Task BackTestStateChanged(BackTestStateDto backTestDto)
		=> TrySendToUser(backTestDto.UserId, SocketEndpoints.BackTests.BackTestStateChanged, backTestDto);
}
