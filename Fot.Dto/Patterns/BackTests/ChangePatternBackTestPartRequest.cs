namespace Fot.Dto.Patterns.BackTests;

[DataContract]
public abstract class ChangePatternBackTestPartRequest
{
	[DataMember(Name = "symbolId")]
	public int SymbolId { get; set; }

	[DataMember(Name = "timeFrameId")]
	public TimeFrameEnum TimeFrameId { get; set; }

	[DataMember(Name = "pnlSettings")]
	public BackTestsPartPnlSettingDto[] PnlSettings { get; set; } = Array.Empty<BackTestsPartPnlSettingDto>();
}
