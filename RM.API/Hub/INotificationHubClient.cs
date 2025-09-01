using System.Collections.Generic;
using System.Threading.Tasks;

namespace RM.API.Hub
{

    public interface INotificationHubClient
    {
        Task SendNotification(List<string> message);
        Task SendAsync(string to, string from, string message);
        Task ReceiveMessage(string action, string from, string message);
        Task UserLogin(string action, string from);
        Task UserLogout(string userId);
        Task Notification(string action, string sender);
        Task SendToAll(string receiverKey, string senderKey, string message);
    }
}
