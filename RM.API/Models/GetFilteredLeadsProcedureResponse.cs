using System;

namespace RM.API.Models
{
    public class GetFilteredLeadsProcedureResponse
    {
        public long? SlNo { get; set; }
        public long? Id { get; set; }
        public string? FullName { get; set; }
        public string? CountryCode { get; set; }
        public string? MobileNumber { get; set; }
        public string? AlternateMobileNumber { get; set; }
        public string? EmailId { get; set; }
        public string? City { get; set; }
        public DateTime? PaymentDate { get; set; }
        public bool? Favourite { get; set; }
        public string? ServiceKey { get; set; }
        public int? ServiceId { get; set; }
        public string? LeadTypeKey { get; set; }
        public int? LeadTypesId { get; set; }
        public int? LeadSourcesId { get; set; }
        public string? LeadSourceKey { get; set; }
        public string? Remark { get; set; }
        public byte? IsSpam { get; set; }
        public byte? IsWon { get; set; }
        public byte? IsDisabled { get; set; }
        public byte? IsDelete { get; set; }
        public Guid? PublicKey { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public string? AssignedTo { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public Guid? PurchaseOrderKey { get; set; }
        public string? PurchaseOrderStatus { get; set; }
        public int? ModeOfPayment { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? NetAmount { get; set; }
        public string? TransactionRecipt { get; set; }
        public string? TransasctionReference { get; set; }
        public int? DaysToGo { get; set; }
        public string? LeadCreatedBy { get; set; }
    }



}
