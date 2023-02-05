using Fot.Dal.Models.Patterns.BackTests;

namespace Fot.Bll.Common.Patterns.BackTests;

public interface IBackTestProcessor
{
	IBackTestProcessor Init(PatternBll pattern, BackTestPartDal part, Func<bool> needStopFunc);
	Task TryProcessPartAsync();
}
