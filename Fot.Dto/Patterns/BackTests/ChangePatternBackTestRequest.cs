namespace Fot.Dto.Patterns.BackTests;

[DataContract]
public class ChangePatternBackTestRequest
{
	[DataMember(Name = "since")]
	public DateTime? Since { get; set; }
	[DataMember(Name = "until")]
	public DateTime? Until { get; set; }
}
