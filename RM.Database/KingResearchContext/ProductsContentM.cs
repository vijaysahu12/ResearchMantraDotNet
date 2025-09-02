using System;

namespace RM.Database.ResearchMantraContext
{
    public class ProductsContentM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? ListImage { get; set; }
        public string? ThumbnailImage { get; set; }
        public string AttachmentType { get; set; }
        public string? Attachment { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? Duration { get; set; }
        public string? Language { get; set; }
        public string? ScreenshotJson { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}