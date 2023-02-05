namespace Fot.Dal.Models.Patterns.BackTests;

public class BackTestsPartPnlSettingDal
{
	public int Id { get; set; }
	public int BackTestPartId { get; set; }
	public long CheckAfterMilliseconds { get; set; }

	private BackTestPartDal? _part;
	public BackTestPartDal Part
	{
		set => _part = value;
		get => _part ?? throw new EntityNotLoadedException<BackTestPartDal>(nameof(Part));
	}
}
