using System;

namespace RM.Database.KingResearchContext;

public partial class LeadsShadow
{
    public long AuditId { get; set; }

    public long Id { get; set; }

    public string FullName { get; set; }

    public string MobileNumber { get; set; }

    public string EmailId { get; set; }

    public string ServiceKey { get; set; }

    public string LeadTypeKey { get; set; }

    public string LeadSourceKey { get; set; }

    public string Remarks { get; set; }

    public byte? IsStudent { get; set; }

    public byte? IsCustomer { get; set; }

    public byte? IsSpam { get; set; }

    public byte? IsWon { get; set; }

    public byte? IsDisabled { get; set; }

    public byte? IsDelete { get; set; }

    public Guid? PublicKey { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }

    public string AssignedTo { get; set; }

    public string AuditAction { get; set; }

    public DateTime AuditDate { get; set; }

    public string AuditUser { get; set; }

    public string AuditApp { get; set; }
}
