using System.ComponentModel.DataAnnotations;

namespace RM.Database.ResearchMantraContext
{
    public class DashboardServiceM
    {
        [Key]
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string ImageUrl { get; set; }
        public string? Badge { get; set; }
    }
}
