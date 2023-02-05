#nullable disable
using System.Linq.Expressions;

namespace Fot.Common.Models.Filtering;

public class Filter<T>
{
	public Filter(Expression<Func<T, bool>> expression)
	{
		Expression = expression;
	}

	public Expression<Func<T, bool>> Expression { get; set; }
}
