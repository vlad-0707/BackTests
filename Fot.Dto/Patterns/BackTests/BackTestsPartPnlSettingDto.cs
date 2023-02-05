namespace Fot.Dto.Patterns.BackTests;

[DataContract]
public class BackTestsPartPnlSettingDto
{
	[DataMember(Name = "id")]
	public int Id { get; set; }

	[DataMember(Name = "backTestPartId")]
	public int BackTestPartId { get; set; }

	[DataMember(Name = "checkAfterMilliseconds")]
	public long CheckAfterMilliseconds { get; set; }
}
