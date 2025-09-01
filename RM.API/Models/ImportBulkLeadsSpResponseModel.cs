using Microsoft.EntityFrameworkCore;

namespace RM.API.Models
{
    [Keyless]
    public class ImportBulkLeadsSpResponseModel
    {

        public string FullName { get; set; }
        public string? Gender { get; set; }
        public string? EmailId { get; set; }
        public string MobileNumber { get; set; }
        public string? City { get; set; }
        public string Action { get; set; }
        public string CountryCode { get; set; }
    }
}
