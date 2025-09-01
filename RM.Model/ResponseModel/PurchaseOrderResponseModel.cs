using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RM.Model.ResponseModel
{
    public class PurchaseOrderResponseModel
    {
        public int Id { get; set; }
        public long LeadId { get; set; }
        public string ClientName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string? DOB { get; set; }
        public string Remark { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ModeOfPayment { get; set; }
        public string BankName { get; set; }
        public string Pan { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string TransasctionReference { get; set; }
        public int ProductId { get; set; }
        public string Product { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string StatusName { get; set; }
        public string ProductName { get; set; }
        public string? ActionBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? PublicKey { get; set; }
        public int? DaysToGo { get; set; }

        [NotMapped]
        public string? InvoiceFileName { get; set; }

        [NotMapped]
        public string? InvoiceUrl { get; set; }
    }
}