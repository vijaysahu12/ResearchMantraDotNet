using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.ResponseModel
{
    public class UpdateMyBucketResponseModel
    {
        public long Id { get; set; }
        public int BucketProductId { get; set; }
        public string ProductName { get; set; }
        public Guid PublicKey { get; set; }
        public Guid MobileUserPublicKey { get; set; }
        public string Reason { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartDate { get; set; }
    }
    public class AddPurchaseOrderDetailsRequestModel
    {
        public int Id { get; set; }
        public Guid PublicKey { get; set; }
        public long? LeadId { get; set; }
        public string ClientName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string DOB { get; set; }
        public string Remark { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int? ModeOfPayment { get; set; }
        public string BankName { get; set; }
        public string Pan { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Reason { get; set; }
        public string TransasctionReference { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public Guid? CouponKey { get; set; }
        public decimal? CouponDiscountAmount { get; set; }
        public int? CouponDiscountPercent { get; set; }
        public int? Status { get; set; }
        public Guid? ActionBy { get; set; }
        public int? PaymentStatusId { get; set; }
        public DateTime? PaymentActionDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? KycApproved { get; set; }
        public DateTime? KycApprovedDate { get; set; }
        public int? SubscriptionMappingId { get; set; }
        public string TransactionId { get; set; }
        public string Invoice { get; set; }
    }

}

