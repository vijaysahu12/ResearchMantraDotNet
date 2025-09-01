namespace RM.API.Models
{
    public class GetLeadsForWhatsappMessageModel
    {
        public long LeadId { get; set; }
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
    }
}
