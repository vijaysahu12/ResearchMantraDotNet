using System;

namespace RM.Database.ResearchMantraContext
{
    public class LearningProductM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ListImageUrl { get; set; }
        public string? ThumbnailImageUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }
}
