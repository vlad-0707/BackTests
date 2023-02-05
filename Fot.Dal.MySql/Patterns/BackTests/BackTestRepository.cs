using Fot.Dal.MySql.Repositories;
using Fot.Dal.Patterns.BackTests;

namespace Fot.Dal.MySql.Patterns.BackTests;

public class BackTestRepository : BaseRepository, IBackTestRepository
{
	public BackTestRepository(
		ILogger<BackTestRepository> logger,
		FotDbContext context)
		: base(logger, context)
	{
	}

	private const string DeletePnlsQuery = @"
		DELETE FROM BackTestResultPnls
		WHERE BackTestResultId IN
			(SELECT r.Id
			FROM BackTestResults AS r
				INNER JOIN BackTestParts AS p
					ON p.Id = r.BackTestPartId
			WHERE p.BackTestId = @p0)";

	private const string DeleteResultsQuery = @"
		DELETE FROM BackTestResults 
		WHERE BackTestPartId IN
		(SELECT Id
		FROM BackTestParts
		WHERE BackTestId = @p0)";

	private const string ResetBackTestPartQuery = @"
		UPDATE BackTestParts
		SET StatusId = 7, CompletionTime = null, LastCalcTime = null, LastCalcTimestamp = null
		WHERE BackTestId = @p0";

	public Task RemoveResultPnlsAsync(int backTestId)
	{
		return Context.Database.ExecuteSqlRawAsync(DeletePnlsQuery, backTestId);
	}

	public Task RemoveResultsAsync(int backTestId)
	{
		return Context.Database.ExecuteSqlRawAsync(DeleteResultsQuery, backTestId);
	}

	public Task ResetBackTestPartAsync(int backTestId)
	{
		return Context.Database.ExecuteSqlRawAsync(ResetBackTestPartQuery, backTestId);
	}
}
