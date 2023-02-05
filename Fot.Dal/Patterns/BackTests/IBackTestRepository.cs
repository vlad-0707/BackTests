namespace Fot.Dal.Patterns.BackTests;

public interface IBackTestRepository
{
	Task RemoveResultPnlsAsync(int backTestId);
	Task RemoveResultsAsync(int backTestId);
	Task ResetBackTestPartAsync(int backTestId);
}
