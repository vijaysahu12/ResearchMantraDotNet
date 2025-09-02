using System;

namespace RM.Database.ResearchMantraContext
{
    public class LearningContentM
    {
        public int Id { get; set; }
        public int MaterialId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? FirstImage { get; set; }
        public string? SecondImage { get; set; }
        public string? ThumbnailImageUrl { get; set; }
        public string? ListImageUrl { get; set; }
        public string? AttachmentDescription { get; set; }
        public string? AttachmentType { get; set; }
        public string? AttachmentTitle { get; set; }
        public string? Attachment { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}