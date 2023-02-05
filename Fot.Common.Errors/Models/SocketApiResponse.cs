#nullable disable
using System.Runtime.Serialization;

namespace Fot.Common.Models;

public class SocketApiResponse : SocketApiResponse<object>
{
	public SocketApiResponse()
	{

	}

	public SocketApiResponse(object data, string message = null) : base(data, message)
	{
	}

	public SocketApiResponse(ResponseCode code, string message = null) : base(code, message)
	{

	}
}

[DataContract]
public class SocketApiResponse<T> : ApiResponse<T>
{
	public SocketApiResponse()
	{

	}

	public SocketApiResponse(T data, string message = null) : base(data, message)
	{
	}

	public SocketApiResponse(ResponseCode code, string message = null) : base(code, message)
	{

	}

	[DataMember(Name = "userId")]
	public int UserId { get; set; }
	[DataMember(Name = "uid")]
	public string Uid { get; set; }
}
