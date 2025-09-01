using System;

namespace RM.Model.RequestModel
{
    public class ManageTicketsRequestModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public Guid ModifiedBy { get; set; }
        public string TicketType { get; set; }
        public string Priority { get; set; }
        public char? Status { get; set; }
        public string Comment { get; set; }
        public bool? IsActive { get; set; }
        public string Subject { get; set; }
        public string Action { get; set; }
        public string PhoneNumber { get; set; }
    }

   
}
