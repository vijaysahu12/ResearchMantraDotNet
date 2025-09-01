using Microsoft.AspNetCore.Http;

namespace RM.API.Models
{
    public class TicketRequestModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public IFormFileCollection Images { get; set; }
        public string Message { get; set; }

    }
}
