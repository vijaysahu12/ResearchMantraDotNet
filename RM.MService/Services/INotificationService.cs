using RM.MService.Hub;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace RM.MService.Services
{
    public interface INotificationService
    {
        Task SendToAllAsync(string message);
        Task SendToSpecific(string receiver, string message, string arg);
        Task SendNotificationToAllActiveUsers(string message);
    }
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToAllAsync(string message)
        {
            await _hubContext.Clients.All.SendAsync("SendMessage", message);
        }


        public async Task SendToSpecific(string receiver, string message, string arg = "PR")
        {

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", arg, receiver.Trim().ToUpper(), message);
        }

        // send to all active connectionIDs
        public async Task SendNotificationToAllActiveUsers(string message)
        {
            List<string> connectionIds = GetConnectionIdsListFromFile();

            foreach (var connectionId in connectionIds)
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("SendMessage", message);
            }
        }

        // temp to get the connectionIDs from file
        List<string> GetConnectionIdsListFromFile()
        {
            try
            {
                string filePath = "connection_ids.txt";

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<string>>(json);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions when reading the file (e.g., log or notify)
                // Example: ////Console.WriteLine("Error reading file: " + ex.Message);
            }

            // If the file doesn't exist or an error occurs, return an empty list
            return new List<string>();
        }





    }
}
