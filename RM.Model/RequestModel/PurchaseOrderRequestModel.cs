using System;

namespace RM.Model.RequestModel
{
    public class PurchaseOrderRequestModel

    {
        public int Id { get; set; }

        public long LeadId { get; set; }
        public string ClientName { get; set; }

        public string Mobile { get; set; }

        public string Email { get; set; }

        public string Dob { get; set; }

        public string Remark { get; set; }

        public DateTime? PaymentDate { get; set; }

        public int? ModeOfPayment { get; set; }

        public string BankName { get; set; }

        public string Pan { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public string TransactionRecipt { get; set; }

        public string TransasctionReference { get; set; }

        public int? ServiceId { get; set; }

        public string Service { get; set; }

        public decimal? NetAmount { get; set; }

        public decimal? PaidAmount { get; set; }

        public int? Status { get; set; }

        public Guid? ActionBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public Guid? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? PublicKey { get; set; }
        public bool? IsExpired { get; set; }
        public DateTime? LTCDate { get; set; }
        public DateTime? PaymentActionDate { get; set; }
        public int? PaymentStatusId { get; set; }
    }

}
