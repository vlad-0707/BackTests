#nullable disable
using System.Runtime.Serialization;

namespace Fot.Common.Models;

[DataContract]
public class ApiResponse : ApiResponse<object>
{
	public ApiResponse()
	{

	}

	public ApiResponse(object data, string message = null) : base(data, message)
	{
	}

	public ApiResponse(ResponseCode code, string message = null) : base(code, message)
	{

	}
}

[DataContract]
public class ApiResponse<T>
{
	public ApiResponse()
	{

	}

	public ApiResponse(T data, string message = null)
	{
		Code = ResponseCode.Success;
		Message = message;
		Data = data;
	}

	public ApiResponse(ResponseCode code, string message)
	{
		Message = message;
		Code = code;
	}

	[DataMember(Name = "code")]
	public ResponseCode Code { get; set; }
	[DataMember(Name = "message")]
	public string Message { get; set; }
	[DataMember(Name = "data")]
	public T Data { get; set; }
	[DataMember(Name = "isSuccess")]
	public bool IsSuccess => Code == ResponseCode.Success;
}
