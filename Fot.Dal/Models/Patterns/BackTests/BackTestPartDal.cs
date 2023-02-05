using Fot.Common.Patterns;
using Fot.Dal.Models.Markets;

namespace Fot.Dal.Models.Patterns.BackTests;

public class BackTestPartDal
{
	public int Id { get; set; }
	public int BackTestId { get; set; }
	public int SymbolId { get; set; }
	public TimeFrameEnum TimeFrameId { get; set; }

	public BackTestStatusEnum StatusId { get; set; }
	public DateTime? CompletionTime { get; set; }
	public DateTime? LastCalcTime { get; set; }
	public long? LastCalcTimestamp { get; set; }


	private BackTestDal? _backTest;
	public BackTestDal BackTest
	{
		set => _backTest = value;
		get => _backTest ?? throw new EntityNotLoadedException<BackTestDal>(nameof(BackTest));
	}

	private BackTestStatusDal? _status;
	public BackTestStatusDal Status
	{
		set => _status = value;
		get => _status ?? throw new EntityNotLoadedException<BackTestStatusDal>(nameof(Status));
	}

	private SymbolDal? _symbol;
	public SymbolDal Symbol
	{
		set => _symbol = value;
		get => _symbol ?? throw new EntityNotLoadedException<SymbolDal>(nameof(Symbol));
	}

	private TimeFrameDal? _timeFrame;
	public TimeFrameDal TimeFrame
	{
		set => _timeFrame = value;
		get => _timeFrame ?? throw new EntityNotLoadedException<TimeFrameDal>(nameof(TimeFrame));
	}

	public ICollection<BackTestResultDal> Results { get; set; } = new HashSet<BackTestResultDal>();

	public ICollection<BackTestsPartPnlSettingDal> PnlSettings { get; set; } = new HashSet<BackTestsPartPnlSettingDal>();

	public override string ToString()
	{
		return $"BackTestPart: Id [{Id}], BackTestId {BackTestId}, SymbolId {SymbolId}, TimeFrameId {TimeFrameId}, StatusId {StatusId}, CompletionTime {CompletionTime}, " +
			$"LastCalcTime {LastCalcTime}, LastCalcTimestamp {LastCalcTimestamp}";
	}
}
