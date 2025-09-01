using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RM.Model.ResponseModel
{
    public class GetProductContentMV2SpResponseModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailImage { get; set; }
        public string ListImage { get; set; }
        public string AttachmentType { get; set; }
        public string Attachment { get; set; }
        public string? Language { get; set; }
        public int? AllVideoDuration { get; set; }
        public int TotalVideoCount { get; set; }
        public int TotalChapters { get; set; }

        [JsonIgnore]
        public string? ScreenshotJson { get; set; }
        public List<ScreenshotItem> ScreenshotList { get; set; } = new(); // Populate in C#
    }

    public class ScreenshotItem
    {
        public string Name { get; set; }
        public string AspectRatio { get; set; }
    }
}
