using Fot.Common.Patterns;

namespace Fot.Dto.Patterns.BackTests;

[DataContract]
public class PatternBackTestDto
{
	[DataMember(Name = "id")]
	public int Id { get; set; }
	[DataMember(Name = "userId")]
	public int UserId { get; set; }
	[DataMember(Name = "patternId")]
	public int PatternId { get; set; }

	[DataMember(Name = "since")]
	public DateTime? Since { get; set; }
	[DataMember(Name = "until")]
	public DateTime? Until { get; set; }
	[DataMember(Name = "startTime")]
	public DateTime? StartTime { get; set; }
	[DataMember(Name = "statusId")]
	public BackTestStatusEnum StatusId { get; set; }

	[DataMember(Name = "createdAt")]
	public DateTime CreatedAt { get; set; }
	[DataMember(Name = "updatedAt")]
	public DateTime UpdatedAt { get; set; }

	[DataMember(Name = "pattern")]
	public PatternDto? Pattern { get; set; }

	[DataMember(Name = "parts")]
	public PatternBackTestPartDto[]? Parts { get; set; }
}
