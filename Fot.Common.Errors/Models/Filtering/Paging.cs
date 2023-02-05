#nullable disable
namespace Fot.Common.Models.Filtering;

public class Paging
{
	public Paging()
	{

	}

	public Paging(int page, int perPage)
	{
		Page = page;
		PerPage = perPage;
	}

	public int Page { get; set; }
	public int PerPage { get; set; }
	public int Total { get; set; }
}
