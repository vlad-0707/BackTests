#nullable disable
namespace Fot.Common.Models.Filtering;

public class QueryParams<T>
{

	public QueryParams(Paging paging)
	{
		Paging = paging;
	}

	public QueryParams(FilterList<T> filters, Paging paging, SortList<T> sorts)
	{
		Sorts = sorts;
		Filters = filters;
		Paging = paging;
	}

	public QueryParams(FilterList<T> filters)
	{
		Filters = filters;
	}

	public FilterList<T> Filters { get; set; }
	public SortList<T> Sorts { get; set; }
	public Paging Paging { get; set; }
}
