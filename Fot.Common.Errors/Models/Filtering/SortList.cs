#nullable disable
using System.Linq.Expressions;

namespace Fot.Common.Models.Filtering;

public class SortList<T>
{
	public int Count => _sortList.Count;

	private readonly List<dynamic> _sortList = new();

	public void Add<TKey>(Expression<Func<T, TKey>> expression, bool byDesc)
	{
		_sortList.Add(new Sort<T, TKey>(expression, byDesc));
	}

	public List<dynamic> Get()
	{
		return _sortList.ToList();
	}
}
