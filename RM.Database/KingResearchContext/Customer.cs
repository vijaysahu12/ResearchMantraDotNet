using System;

namespace RM.Database.KingResearchContext;

public partial class Customer
{
    public long Id { get; set; }

    //public string CustomerTypeKey { get; set; }

    //public string SegmentKey { get; set; }

    public decimal? TotalPurchases { get; set; }

    public string Remarks { get; set; }

    public string LeadKey { get; set; }

    public byte? IsDelete { get; set; }

    public Guid? PublicKey { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }
    public Guid? PurchaseOrderKey { get; set; }
}
