using Fot.Dto.Patterns.BackTests;

namespace Fot.Bll.Common.Patterns.BackTests;
public interface IBackTestTransmitter
{
	Task StartBackTestAsync(int id);
	Task StopBackTestAsync(int id);
	Task ResetBackTestAsync(int id);
	Task PartStateChanged(BackTestPartStateDto partDto);
	Task BackTestStateChanged(BackTestStateDto backTestDto);
}
