using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.DB.Tables
{
    public class PurchaseOrderStatusRequestModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public long LeadId { get; set; }
        [Required]
        public string ClientName { get; set; }
        [Required]
        public string Mobile { get; set; }
        [Required]
        public string? CountryCode { get; set; }
        [Required]
        public string Email { get; set; }
        public string DOB { get; set; }
        public string Remark { get; set; }
        [Required]
        public DateTime PaymentDate { get; set; }
        [Required]
        public int ModeOfPayment { get; set; }
        public string BankName { get; set; }
        public string TransasctionReference { get; set; }
        public string Pan { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string TransactionRecipt { get; set; }
        public string Product { get; set; }
        public decimal NetAmount { get; set; } // disabled and will be calculate on paid Amount
        public decimal PaidAmount { get; set; }
        public int Status { get; set; }
        public string ActionBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid PublicKey { get; set; }
    }

    public class PurchaseOrderResponseSPModel
    {

        public int Id { get; set; }
        public long LeadId { get; set; }
        public string ClientName { get; set; }
        public string? CountryCode { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string? DOB { get; set; }
        public string Remark { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ModeOfPayment { get; set; }
        public int ModeOfPaymentId { get; set; }
        public string BankName { get; set; }
        public string Pan { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string TransactionRecipt { get; set; }
        public string TransasctionReference { get; set; }
        public int ServiceId { get; set; }
        public string Service { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public int Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string AssignedTo { get; set; }
        public string StatusName { get; set; }
        public string ServiceName { get; set; }
        public string? ActionBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid PublicKey { get; set; }
        public Guid LeadPublickKey { get; set; }

        public string LeadSourceName { get; set; }
    }

    public enum EmailTemplatesEnum
    {
        LTC = 1,
        SubscriptionExpired = 2
    }


    public enum PurchaseOrdersStatusEnum
    {
        Active = 1
        , Inactive = 2
        , Pending = 3
        , Completed = 4
        , Failed = 5
        , Cancelled = 6
        , OnHold = 7
        , Archived = 8
        , Draft = 9
        , Approved = 10
        , Rejected = 11
        , PaymentReceived = 12
        , PendingApproval = 13
        , Scheduled = 14
        , Overdue = 15
        , Urgent = 16
        , Normal = 17
        , High = 18
        , Low = 19
        , Open = 20
        , Closed = 21
        , Fresh = 22
        , Followup = 23
        , Customer = 24
    }

    public enum ActivityTypeEnum
    {
        LeadModified = 1,
        LeadAssigned = 2,
        LeadUnassigned = 3,
        LeadSelfassigned = 4,
        LeadAllocated = 5,
        PoCreated = 6,
        PoModified = 7,
        PoApproved = 8,
        PoRejected = 9,
        PoPending = 10,
        PoRegenerated = 11,
        CreatedLead = 12,
        UpdatedLead = 13,
        DeletedLead = 14,
        LTCCreated = 15,
        FollowUpReminderAdded = 16,
        EnquiryAdded = 17,
        ServiceModified = 18,
        FollowUpReminderEdited = 19,
        FollowUpReminderDeleted = 20,
        LeadPulled = 21,
        ReportPublished = 22,
        ReportUnpublished = 23,
        FreeTrailActivated = 24,
        FreeTrailDeactivated = 25,
        FreeTrailExtended = 26,
        MarkAsComplete = 27,
        FreeTrailDeleted = 28
    }

    public static class EnumFinder
    {
        public static PurchaseOrdersStatusEnum FindEnumByIdForPO(int targetId)
        {
            foreach (PurchaseOrdersStatusEnum enumValue in Enum.GetValues(typeof(PurchaseOrdersStatusEnum)))
            {
                if ((int)enumValue == targetId)
                    return enumValue;
            }
            return default; // Return a default value if the enum is not found.
        }
        public static ActivityTypeEnum FindEnumByIdForActivityType(int targetId)
        {
            foreach (ActivityTypeEnum enumValue in Enum.GetValues(typeof(ActivityTypeEnum)))
            {
                if ((int)enumValue == targetId)
                    return enumValue;
            }
            return default; // Return a default value if the enum is not found.
        }


    }

    public enum NotificationScreenEnum
    {
        None,
        Lead,
        Customer,
        PurchaseOrder,
        Dashboard,
        AllocateLeads,
        SpecificLead
    }
}
