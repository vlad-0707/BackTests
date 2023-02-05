#nullable disable
using System.Linq.Expressions;

namespace Fot.Common.Models.Filtering;

public class Sort<T, TKey>
{
	public Sort(Expression<Func<T, TKey>> expression, bool byDescending)
	{
		Expression = expression;
		ByDescending = byDescending;
	}

	public Expression<Func<T, TKey>> Expression { get; set; }
	public bool ByDescending { get; set; }
}
