namespace BlazorServer.Hubs;

public class UserConnectionManager
{
    public Dictionary<string, string> userConnections { get; } = new Dictionary<string, string>();
    public Dictionary<string, byte[]> userPublicKeys { get; } = new Dictionary<string, byte[]>();

}