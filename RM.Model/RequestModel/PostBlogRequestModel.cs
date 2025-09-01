using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class PostBlogRequestModel
    {
        public string Content { get; set; }
        public string Hashtag { get; set; }
        [Required]
        public string UserObjectId { get; set; }
        public IFormFileCollection Images { get; set; }
        public List<string> AspectRatios { get; set; }
    }
}
