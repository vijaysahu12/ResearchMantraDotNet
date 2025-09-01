using System;

namespace RM.Model.RequestModel
{
    public class GetTicketReqeustModel
    {
        public string? Status { get; set; }
        public string? TicketType { get; set; }
        public string? Priority { get; set; }
        public string? SearchText { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
