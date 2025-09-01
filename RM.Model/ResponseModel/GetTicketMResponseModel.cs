using System;

namespace RM.Model.ResponseModel
{
    public class GetTicketMResponseModel
    {
        public long SlNo { get; set; }
        public long Id { get; set; }
        public long UserId { get; set; }
        public Guid UserPublicKey { get; set; }
        public string CreatedBy { get; set; }
        public string TicketType { get; set; }
        public string Priority { get; set; }
        public string Subject { get; set; }
        public string? Description { get; set; }
        public string? Comment { get; set; }
        public string CommentsJson { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? Images { get; set; }

        public string? Mobile { get; set; }
        public string Status { get; set; }
    }
}
