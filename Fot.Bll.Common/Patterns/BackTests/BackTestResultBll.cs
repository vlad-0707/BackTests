using Fot.Dal.Models.Patterns.BackTests;

namespace Fot.Bll.Common.Patterns.BackTests;

public class BackTestResultBll
{
	private FtCandle? _candle;
	public FtCandle Candle => _candle ?? throw new NotInitializeException($"{nameof(Candle)}");

	private BackTestPartDal? _part;
	public BackTestPartDal Part => _part ?? throw new NotInitializeException($"{nameof(Part)}");

	private BackTestResultDal? _result;
	public BackTestResultDal Dal => _result ?? throw new NotInitializeException($"{nameof(Dal)}");

	public List<BackTestsPartPnlSettingDal> PnlSettings { get; set; } = new();

	public BackTestResultBll Init(BackTestResultDal result, BackTestPartDal part, ICollection<BackTestsPartPnlSettingDal> pnlSettings, FtCandle candle)
	{
		_part = part;
		_result = result;
		_candle = candle;
		PnlSettings = pnlSettings.OrderBy(x => x.CheckAfterMilliseconds).ToList();

		return this;
	}
}
