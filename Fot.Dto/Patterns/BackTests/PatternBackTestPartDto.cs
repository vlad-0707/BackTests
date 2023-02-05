using Fot.Common.Patterns;

namespace Fot.Dto.Patterns.BackTests;

[DataContract]
public class PatternBackTestPartDto : ChangePatternBackTestPartRequest
{
	[DataMember(Name = "id")]
	public int Id { get; set; }
	[DataMember(Name = "backTestId")]
	public int BackTestId { get; set; }
	[DataMember(Name = "statusId")]
	public BackTestStatusEnum StatusId { get; set; }
	[DataMember(Name = "completionTime")]
	public DateTime? CompletionTime { get; set; }
	[DataMember(Name = "lastCalcTime")]
	public DateTime? LastCalcTime { get; set; }
	[DataMember(Name = "lastCalcTimestamp")]
	public long? LastCalcTimestamp { get; set; }

	[DataMember(Name = "results")]
	public BackTestResultDto[] Results { get; set; } = Array.Empty<BackTestResultDto>();
}
