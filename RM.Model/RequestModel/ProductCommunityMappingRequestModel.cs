using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.RequestModel
{
    public class ProductCommunityMappingRequestModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CommunityId { get; set; }
        public int CreatedBy { get; set; }
        public string Action { get; set; }
        public int DurationInDays { get; set; }
    }
}
