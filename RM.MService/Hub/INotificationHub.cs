using RM.Database.ResearchMantraContext;
using Microsoft.AspNetCore.SignalR;

namespace RM.MService.Hub
{
    public interface INotificationHub
    {

    }

    public class NotificationHub : Hub<INotificationHub>
    {
        private readonly TokenService _tokenService;
        private readonly ResearchMantraContext _context;



        public NotificationHub(TokenService tokenService, ResearchMantraContext context)
        {
            _tokenService = tokenService;
            _context = context;

        }

        // runs when a connection is established
        public override async Task OnConnectedAsync()
        {

            try
            {

                string userId = _tokenService.AccessFactoryToken;
                string connectionId = Context.ConnectionId;

                //Dictionary<string, List<string>> userConnectionData = GetUserDataFromFile();

                WebSocketConnection connection = new()
                {
                    UserId = userId,
                    ConnectionId = connectionId,
                    CreatedOn = DateTime.Now
                };

                _context.WebSocketConnections.Add(connection);
                await _context.SaveChangesAsync();
                await base.OnConnectedAsync();

            }
            catch (Exception ex)
            {

            }
        }
        // runs when a connection is lost
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string userId = _tokenService.AccessFactoryToken;
            string connectionId = Context.ConnectionId;

            var connectionToRemove = _context.WebSocketConnections.SingleOrDefault(item => item.ConnectionId == connectionId);
            if (connectionToRemove != null)
            {
                _context.WebSocketConnections.Remove(connectionToRemove);
                await _context.SaveChangesAsync();
            }
            await base.OnDisconnectedAsync(exception);
        }

    }

    public class TokenService
    {
        public string AccessFactoryToken { get; set; }
    }
}
