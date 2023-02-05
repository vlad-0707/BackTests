#nullable disable
using Fot.Common.Models;

namespace Fot.Common;

public abstract class CodedExceptionBase : Exception
{
	public abstract ResponseCode Code { get; }
}
