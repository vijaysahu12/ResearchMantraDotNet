
namespace RM.API.Dtos
{
    public class DashboardDto
    {
        //public int Accounts { get; set; }
        //public int Leads { get; set; }
        //public int Customers { get; set; }
        //public int CallPerformance { get; set; }
        public int FreshPartners { get; set; }
        public int PendingPartners { get; set; }
        public int AcceptedPartners { get; set; }
        public int RejectedPartners { get; set; }
        public int TotalPartners { get; set; }
    }
}
