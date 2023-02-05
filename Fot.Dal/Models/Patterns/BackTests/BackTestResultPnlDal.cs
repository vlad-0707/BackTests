namespace Fot.Dal.Models.Patterns.BackTests;

public class BackTestResultPnlDal
{
	public int Id { get; set; }
	public int BackTestResultId { get; set; }
	public int BackTestSettingId { get; set; }
	public decimal LongResult { get; set; }

	public long? ResultTimestamp { get; set; }
	public decimal? ResultOpen { get; set; }
	public decimal? ResultHigh { get; set; }
	public decimal? ResultLow { get; set; }
	public decimal? ResultClose { get; set; }
	public decimal? ResultVolume { get; set; }

	public decimal? High { get; set; }
	public long? HighTimestamp { get; set; }
	public decimal? Low { get; set; }
	public long? LowTimestamp { get; set; }

	private BackTestResultDal? _result;
	public BackTestResultDal Result
	{
		set => _result = value;
		get => _result ?? throw new EntityNotLoadedException<PatternDal>(nameof(Result));
	}

	private BackTestsPartPnlSettingDal? _setting;
	public BackTestsPartPnlSettingDal Setting
	{
		set => _setting = value;
		get => _setting ?? throw new EntityNotLoadedException<PatternDal>(nameof(Setting));
	}
}
