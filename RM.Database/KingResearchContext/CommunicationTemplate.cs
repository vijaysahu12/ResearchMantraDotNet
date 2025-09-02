using System;

namespace RM.Database.ResearchMantraContext;

public partial class CommunicationTemplate
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string ModeKey { get; set; }

    public string Template { get; set; }

    public byte? IsDisabled { get; set; }

    public byte? IsDelete { get; set; }

    public Guid? PublicKey { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }
}
