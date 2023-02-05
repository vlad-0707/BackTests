#nullable disable
namespace Fot.BLL.Configs;

public class FotApiConfig
{
	public string SocketUrl { get; set; }
	public string ApiBaseUrl { get; set; }
	public bool SocketEnableAutoSendPing { get; set; }
	public int SocketAutoSendPingInterval { get; set; }
	public int SocketWaitUntilReconnectMls { get; set; }
}
