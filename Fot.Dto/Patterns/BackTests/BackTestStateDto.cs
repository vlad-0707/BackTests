using Fot.Common.Patterns;

namespace Fot.Dto.Patterns.BackTests;

public class BackTestStateDto
{
	[DataMember(Name = "id")]
	public int Id { get; set; }
	[DataMember(Name = "userId")]
	public int UserId { get; set; }
	[DataMember(Name = "startTime")]
	public DateTime? StartTime { get; set; }
	[DataMember(Name = "statusId")]
	public BackTestStatusEnum StatusId { get; set; }
	[DataMember(Name = "updatedAt")]
	public DateTime UpdatedAt { get; set; }
}
