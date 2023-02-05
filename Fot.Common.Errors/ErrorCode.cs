#nullable disable
namespace Fot.Common;

public enum ErrorCode
{
	Success = 0,
	UnknownError = 1,
	HttpError = 2,
	UnconfirmedEmail = 3,
	AccessDenied = 4
}
