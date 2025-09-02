using System;

namespace RM.Database.ResearchMantraContext;

public partial class CustomerKyc
{
    public long Id { get; set; }

    public string LeadKey { get; set; }

    public string Panurl { get; set; }

    public string Pan { get; set; }

    public bool? Verified { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }

    public bool? IsDelete { get; set; }

    public string Status { get; set; }

    public string Remarks { get; set; }

    public string JsonData { get; set; }
}
