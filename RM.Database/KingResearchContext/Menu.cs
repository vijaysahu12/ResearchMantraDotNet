using System;

namespace RM.Database.ResearchMantraContext;

public partial class Menu
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Url { get; set; }

    public int? ParentId { get; set; }

    public int? SortOrder { get; set; }

    public byte? IsDisabled { get; set; }

    public byte? IsDelete { get; set; }

    public Guid? PublicKey { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string ModifiedBy { get; set; }

    public int? IsLhs { get; set; }
}
