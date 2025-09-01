using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class QueryFormRequestModel
    {
        public int Id { get; set; }
        public Guid MobileUserId { get; set; }
        public string ProductId { get; set; }
        public string QueryDescription { get; set; }
        public IFormFile? ScreenshotUrl { get; set; }
        public int QueryCategory { get; set; }
        public string? Remarks { get; set; }
        public int CreatedBy { get; set; }
        public int Status { get; set; }
    }
}
