using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class MobileUserBucketRequestModel
    {
        public bool IsPaging { get; set; } 
        public int PageSize { get; set; }
        public int PageNumber { get; set; } 
        public string SearchText { get; set; } 
    }

}
