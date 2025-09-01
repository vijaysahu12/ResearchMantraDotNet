using System;

namespace RM.Database.KingResearchContext;

public partial class CustomerTag
{
    public int Id { get; set; }

    public string CustomerKey { get; set; }

    public string TagKey { get; set; }

    public byte? IsDelete { get; set; }

    public Guid? PublicKey { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }
}
