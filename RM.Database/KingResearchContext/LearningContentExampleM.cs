using System;

namespace RM.Database.ResearchMantraContext
{
    public class LearningContentExampleM
    {
        public int Id { get; set; }
        public int ContentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
