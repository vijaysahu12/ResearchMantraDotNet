using System;

namespace RM.Database.KingResearchContext
{
    public class LearningMaterialM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }

        public Guid? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}