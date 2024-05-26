namespace BlazorServer.Hubs;

public class UserConnectionManager
{
    public Dictionary<string, string> userConnections { get; } = new Dictionary<string, string>();
}