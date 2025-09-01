using System;

namespace RM.Database.KingResearchContext;

public partial class Lead
{
    public long Id { get; set; }

    public Guid PublicKey { get; set; }

    public string FullName { get; set; }

    public string Gender { get; set; }

    public string? CountryCode { get; set; }
    public string MobileNumber { get; set; }
    public string? AlternateMobileNumber { get; set; }

    public string EmailId { get; set; }

    public string ProfileImage { get; set; }

    public string PriorityStatus { get; set; }

    public string AssignedTo { get; set; }

    public string ServiceKey { get; set; }

    public string LeadTypeKey { get; set; }

    public string LeadSourceKey { get; set; }

    public string Remarks { get; set; }

    public byte? IsDisabled { get; set; }

    public byte? IsDelete { get; set; }

    public DateTime CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public byte? IsSpam { get; set; }

    public byte? IsWon { get; set; }

    public DateTime ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }

    public string City { get; set; }

    public string PinCode { get; set; }
    public int? StatusId { get; set; }
    public bool? Favourite { get; set; }
    public Guid? PurchaseOrderKey { get; set; }
}