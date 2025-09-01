using RM.Model.MongoDbCollection;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class CreateCommunity
    {
        public string Id { get; set; }
        public IFormFileCollection Images { get; set; }
        public List<string> AspectRatios { get; set; }
        public string Url { get; set; }
        public IFormFile ImageUrl { get; set; }
        public int ProductId { get; set; }
        //public CommunityPostTypeEnum PostTypeId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string IsApproved { get; set; }
        public bool IsDelete { get; set; }
    }
}
