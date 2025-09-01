using System;

namespace RM.API.Models
{
    public class GetUserActivitySpResponseModel
    {
        public long SlNo { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public Guid PublicKey { get; set; }
        public string? ActivityMessage { get; set; }
        public DateTime CreatedOn { get; set; }
        public string MobileNumber { get; set; }
        public string? Message { get; set; }
    }
}
