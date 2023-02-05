namespace Fot.Dto.Patterns.BackTests;

[DataContract]
public sealed class UpdatePatternBackTestPartRequest
	: ChangePatternBackTestPartRequest
{
	[DataMember(Name = "id")]
	public int Id { get; set; }
}
