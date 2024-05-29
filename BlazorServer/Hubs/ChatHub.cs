using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorServer.Hubs
{


    public class ChatHub : Hub
    {
        private readonly UserConnectionManager _userConnectionManager;

        public ChatHub(UserConnectionManager userConnectionManager)
        {
            _userConnectionManager = userConnectionManager;
        }

        public Task SendMessagePrivate_ChatHubFunction(string connectionId, string user, byte[] secretMessage, byte[] IV)
        {
            return Clients.Client(connectionId).SendAsync("BBBB", user, secretMessage, IV);
        }


        public Task SendPublicKey(byte[] publicKey)
        {
            // Store the public key associated with the connectionId
            _userConnectionManager.userPublicKeys[Context.ConnectionId] = publicKey;
            return Task.CompletedTask;
        }

 

        public Task RequestPublicKey(string connectionId)
        {
            // Send the public key associated with the connectionId
            if (_userConnectionManager.userPublicKeys.TryGetValue(connectionId, out var PublicKey))
            {
                return Clients.Caller.SendAsync("RequestPublicKey", PublicKey);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
 
        // Method to send the list of connected users to the caller
        public Task SendConnectedUsers_ChatHubFunction()
        {
            return Clients.Caller.SendAsync("CCCC", _userConnectionManager.userConnections);
        }

        public override async Task OnConnectedAsync()
        {
            string username = "User" + new Random().Next(1000, 9999);
            string connectionId = Context.ConnectionId;
            lock (_userConnectionManager.userConnections)
            {
                _userConnectionManager.userConnections[connectionId] = username;
            }

            await Clients.Caller.SendAsync("ReceiveUsername", username);
            await Clients.All.SendAsync("CCCC", _userConnectionManager.userConnections);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;
            lock (_userConnectionManager.userConnections)
            {
                _userConnectionManager.userConnections.Remove(connectionId);
                _userConnectionManager.userPublicKeys.Remove(connectionId);
            }

            await Clients.All.SendAsync("CCCC", _userConnectionManager.userConnections);
            await base.OnDisconnectedAsync(exception);
        }
    }
}