using System;

namespace RM.Database.KingResearchContext;

public partial class Service
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public decimal? ServiceCost { get; set; }

    public string ServiceTypeKey { get; set; }

    public string ServiceCategoryKey { get; set; }

    public byte? IsDisabled { get; set; }

    public byte? IsDelete { get; set; }

    public Guid? PublicKey { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }
}
