using Fot.Common.Patterns;

namespace Fot.Dal.Models.Patterns.BackTests;

public class BackTestStatusDal
{
	public BackTestStatusEnum Id { get; set; } = BackTestStatusEnum.Unknown;
	public string Name { get; set; } = "Unknown";
}
