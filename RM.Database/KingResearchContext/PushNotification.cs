using System;
using System.ComponentModel.DataAnnotations;

namespace RM.Database.KingResearchContext.Tables;

public partial class PushNotification
{
    [Key]

    public int Id { get; set; }
    public Guid Userkey { get; set; }
    public string Message { get; set; }
    public DateTime? ReadDate { get; set; }
    public bool? IsRead { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsImportant { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }

    public Guid CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
}