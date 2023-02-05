#nullable disable
using System.Linq.Expressions;

namespace Fot.Common.Models.Filtering;

public class FilterList<T>
{
	public FilterList<T> Add(Expression<Func<T, bool>> expression)
	{
		_filterList.Add(new Filter<T>(expression));
		return this;
	}

	public List<Filter<T>> Get()
	{
		return _filterList.ToList();
	}

	private readonly List<Filter<T>> _filterList = new List<Filter<T>>();
}
