namespace Fot.Dto.Patterns.BackTests;

[DataContract]
public class CreatePatternBackTestRequest
	: ChangePatternBackTestRequest
{
	[DataMember(Name = "patternId")]
	public int PatternId { get; set; }

	[DataMember(Name = "parts")]
	public CreatePatternBackTestPartRequest[] Parts { get; set; } = Array.Empty<CreatePatternBackTestPartRequest>();
}
