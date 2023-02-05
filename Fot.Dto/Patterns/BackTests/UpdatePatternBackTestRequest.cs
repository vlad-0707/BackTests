namespace Fot.Dto.Patterns.BackTests;

[DataContract]
public class UpdatePatternBackTestRequest
	: ChangePatternBackTestRequest
{
	[DataMember(Name = "id")]
	public int Id { get; set; }

	[DataMember(Name = "parts")]
	public UpdatePatternBackTestPartRequest[] Parts { get; set; } = Array.Empty<UpdatePatternBackTestPartRequest>();
}
