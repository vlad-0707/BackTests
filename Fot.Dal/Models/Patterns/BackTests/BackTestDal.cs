using Fot.Common.Patterns;
using Fot.Dal.Models.Users;

namespace Fot.Dal.Models.Patterns.BackTests;

public class BackTestDal
{
	public int Id { get; set; }
	public int PatternId { get; set; }
	public int UserId { get; set; }

	public DateTime? Since { get; set; }
	public DateTime? Until { get; set; }
	public DateTime? StartTime { get; set; }
	public BackTestStatusEnum StatusId { get; set; }

	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }


	private PatternDal? _pattern;
	public PatternDal Pattern
	{
		set => _pattern = value;
		get => _pattern ?? throw new EntityNotLoadedException<PatternDal>(nameof(Pattern));
	}

	private AspNetUserDal? _user;
	public AspNetUserDal User
	{
		set => _user = value;
		get => _user ?? throw new EntityNotLoadedException<AspNetUserDal>(nameof(User));
	}

	private BackTestStatusDal? _status;
	public BackTestStatusDal Status
	{
		set => _status = value;
		get => _status ?? throw new EntityNotLoadedException<BackTestStatusDal>(nameof(Status));
	}

	public ICollection<BackTestPartDal> Parts { get; set; } = new HashSet<BackTestPartDal>();

	public override string ToString()
	{
		return $"BackTest: Id [{Id}], PatternId {PatternId}, Since {Since}, Until {Until}, StartTime {StartTime}, StatusId {StatusId}";
	}
}
