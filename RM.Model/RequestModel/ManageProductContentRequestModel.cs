using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace RM.Model.RequestModel
{
    public class ManageProductContentRequestModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? AttachmentType { get; set; }
        public string? Attachment { get; set; }
        public Guid CreatedBy { get; set; }
        public IFormFile? ListImage { get; set; }
        public IFormFile? ThumbnailImage { get; set; }
        public List<IFormFile>? Screenshots { get; set; } // ⬅️ Multiple screenshots
        public List<string>? AspectRatios { get; set; } // ⬅️ Corresponding aspect ratios
        public string? ExistingScreenshots { get; set; } // JSON string from Angular
        public List<IFormFile> NewScreenshots { get; set; } // newly added images
        public string? DeletedScreenshots { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}