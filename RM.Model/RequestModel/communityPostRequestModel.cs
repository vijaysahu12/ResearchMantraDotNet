using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace RM.Model.RequestModel
{
    public class CommunityPostRequestModel
    {
        public string? Content { get; set; }
        public List<string> AspectRatios { get; set; }

        public IFormFileCollection Images { get; set; }

        public string UserMobileNumber { get; set; }
        public string Hashtag { get; set; }
    }
}