using System;

namespace RM.Database.ResearchMantraContext;

public partial class Pincode
{
    public long Id { get; set; }

    public string Pincode1 { get; set; }

    public string Area { get; set; }

    public string Division { get; set; }

    public string District { get; set; }

    public string State { get; set; }

    public byte? IsDisabled { get; set; }

    public byte? IsDelete { get; set; }

    public Guid? PublicKey { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }
}
