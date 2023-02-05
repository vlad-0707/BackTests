namespace Fot.Dal.Models.Patterns.BackTests;

public class BackTestResultDal
{
	public int Id { get; set; }
	public int BackTestPartId { get; set; }
	public long StartTimestamp { get; set; }
	public decimal? StartOpen { get; set; }
	public decimal? StartHigh { get; set; }
	public decimal? StartLow { get; set; }
	public decimal? StartClose { get; set; }
	public decimal? StartVolume { get; set; }

	public long EndTimestamp { get; set; }
	public decimal? EndOpen { get; set; }
	public decimal? EndHigh { get; set; }
	public decimal? EndLow { get; set; }
	public decimal? EndClose { get; set; }
	public decimal? EndVolume { get; set; }

	public long? StopBaseCandleTimestamp { get; set; }
	public decimal? StopBaseCandleOpen { get; set; }
	public decimal? StopBaseCandleHigh { get; set; }
	public decimal? StopBaseCandleLow { get; set; }
	public decimal? StopBaseCandleClose { get; set; }
	public decimal? StopBaseCandleVolume { get; set; } 

	private BackTestPartDal? _backTestPartDal;
	public BackTestPartDal? BackTestPart
	{
		set => _backTestPartDal = value;
		get => _backTestPartDal; //?? throw new EntityNotLoadedException<BackTestPartDal>(nameof(BackTestPart));
	}

	public override string ToString()
	{
		return $"BackTestResult [ {Id} ], BackTestPartId: {BackTestPartId}, StartTimestamp: {StartTimestamp}, EndTimestamp: {EndTimestamp}";
	}
}
