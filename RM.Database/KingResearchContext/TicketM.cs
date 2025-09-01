using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RM.Database.KingResearchContext
{
    public class TicketM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [Column(TypeName = "char(3)")]
        public string TicketType { get; set; } //Technical, Sales, Other

        [Column(TypeName = "char(10)")]
        public string Priority { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string Subject { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Description { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Comment { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }
        public char? Status { get; set; } //Open /Closed

        [Required]
        public Guid CreatedBy { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public Guid? ModifiedBy { get; set; }

        public string? Images { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}