using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class CommunityPostUpdateModel
    {
        public string Content { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public int ModifiedBy { get; set; }
        public int ProductId { get; set; }
        public int PostTypeId { get; set; }
        public List<IFormFile> Images { get; set; } // Allows Image Upload
        public IFormFile ImageUrl { get; set; } // Allows Image Upload
        public bool IsDelete { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpComingEvent { get; set; }
        public bool IsJoinNowEnabled { get; set; }
        public bool IsQueryFormEnabled { get; set; }
        public string IsApproved { get; set; }
        public List<string> AspectRatios { get; set; }
        public List<IFormFile> NewImages { get; set; }
        public List<string> ImageUrls { get; set; } // Existing images to retain
        public List<string> DeletedImages { get; set; } // Images to delete
    }

}
