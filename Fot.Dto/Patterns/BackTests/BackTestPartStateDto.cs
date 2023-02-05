using Fot.Common.Patterns;

namespace Fot.Dto.Patterns.BackTests;

[DataContract]
public class BackTestPartStateDto
{
	[DataMember(Name = "id")]
	public int Id { get; set; }
	[DataMember(Name = "userId")]
	public int UserId { get; set; }
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

	public override string ToString()
	{
		return $"backTestId {BackTestId}, partId {Id}, statusId {StatusId}, completionTime {CompletionTime}, lastCalcTime {LastCalcTime}, lastCalcTimestamp {LastCalcTimestamp}";
	}
}
