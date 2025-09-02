using System.ComponentModel.DataAnnotations;
namespace RM.Database.ResearchMantraContext;
public partial class LeadsImport
{
    public long Id { get; set; }

    [StringLength(200, ErrorMessage = "FullName cannot exceed 200 characters.")]
    public string FullName { get; set; }

    [StringLength(50, ErrorMessage = "Gender cannot exceed 50 characters.")]
    public string Gender { get; set; }

    [StringLength(10, ErrorMessage = "CountryCode cannot exceed 10 characters.")]
    public string CountryCode { get; set; }

    [Required(ErrorMessage = "MobileNumber is required.")]
    [StringLength(100, ErrorMessage = "MobileNumber cannot exceed 100 characters.")]
    public string MobileNumber { get; set; }

    [StringLength(200, ErrorMessage = "EmailId cannot exceed 200 characters.")]
    [EmailAddress(ErrorMessage = "Invalid EmailId format.")]
    public string EmailId { get; set; }

    [StringLength(2000, ErrorMessage = "Remarks cannot exceed 2000 characters.")]
    public string Remarks { get; set; }

    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters.")]
    public string City { get; set; }

    [StringLength(50, ErrorMessage = "Marking cannot exceed 50 characters.")]
    public string Marking { get; set; }
}