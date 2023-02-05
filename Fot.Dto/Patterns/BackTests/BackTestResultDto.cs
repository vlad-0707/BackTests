namespace Fot.Dto.Patterns.BackTests;

[DataContract]
public class BackTestResultDto
{
	[DataMember(Name = "id")]
	public int Id { get; set; }

	[DataMember(Name = "backTestPartId")]
	public int BackTestPartId { get; set; }

	[DataMember(Name = "startTimestamp")]
	public long StartTimestamp { get; set; }

	[DataMember(Name = "endTimestamp")]
	public long EndTimestamp { get; set; }
}
