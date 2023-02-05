#nullable disable
namespace Fot.Common.Models;

public enum ResponseCode
{
	Success = 0,
	UnknownError = 1,
	HttpError = 2,
	UnconfirmedEmail = 3,
	AccessDenied = 4,
	Timeout = 5,
	QsConnectorError = 6
}
