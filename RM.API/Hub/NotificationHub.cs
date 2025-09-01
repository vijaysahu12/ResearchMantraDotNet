using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RM.API.Hub
{
    [Authorize]
    public class NotificationHub : Hub<INotificationHubClient>
    {
        private static readonly ConcurrentDictionary<string, string> _connectedUsers = new();

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = _connectedUsers.SingleOrDefault((c) => c.Key == Context.ConnectionId).Key;
            if (connectionId != null)
            {
                _ = _connectedUsers.TryRemove(connectionId, out string keyToRemove);
                await Clients.Others.UserLogout(connectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendNotification(List<string> message)
        {
            await Clients.All.SendNotification(message);
        }
        public async Task ReceiveMessage(string receiverKey, string fromUser, string message)
        {
            if (_connectedUsers.TryGetValue(receiverKey, out string receiverConnId))
            {
                string sender = _connectedUsers.SingleOrDefault((c) => c.Key == Context.ConnectionId).Value;
                if (!string.IsNullOrEmpty(receiverKey) && sender != receiverConnId && _connectedUsers.ContainsKey(receiverConnId))
                {
                    _ = _connectedUsers.TryGetValue(sender, out string client);
                }
                await Clients.Clients(receiverConnId).ReceiveMessage("ReceiveMessage", receiverKey, message);
            }
        }
        public void UserLogin(string userKey)
        {
            string connectionId = Context.ConnectionId;
            if (!_connectedUsers.ContainsKey(connectionId))
            {
                _ = _connectedUsers.TryAdd(connectionId, userKey);
            }
        }
        public void SendMessage(string userKey, string message)
        {
            //_connectedUsers.TryGetValue(userKey, out var receiverConnId);
            //var sender = _connectedUsers.SingleOrDefault((c) => c.Key == Context.ConnectionId).Value;
            //if (!string.IsNullOrEmpty(userKey))
            //{
            //    await Clients.Clients(receiverConnId).ReceiveMessage("ReceiveMessage", sender, message);
            //}
        }

        public async Task SendToAll(string receiverKey, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", receiverKey, message);
        }


        public async Task BroadcastMessage(string receiver, string message)
        {
            string sender = _connectedUsers.SingleOrDefault((c) => c.Key == Context.ConnectionId).Value;
            if (!string.IsNullOrEmpty(receiver))
            {
                await Clients.Others.ReceiveMessage("ReceiveMessage", sender, message);
            }
        }
        public async void Logout()
        {
            string userName = _connectedUsers.SingleOrDefault((c) => c.Key == Context.ConnectionId).Value;
            if (userName != null)
            {
                _ = _connectedUsers.TryRemove(userName, out string client);
                _ = Context.Items.Remove(Context.ConnectionId, out object currentname);
                await Clients.Others.UserLogout(userName);
                ////Console.WriteLine($"<> {userName} logout");
            }
        }
    }
}