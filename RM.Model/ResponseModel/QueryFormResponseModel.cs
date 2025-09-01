using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.ResponseModel
{
    public class QueryFormResponseModel
    {
        public int Id { get; set; }
        public Guid MobileUserId { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ProductName { get; set; }
        public string Mobile { get; set; }
        public string Questions { get; set; }
        public string ScreenshotUrl { get; set; }
        public int QueryRelatedTo { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public int RemarksCount { get; set; }
        public int IsActive { get; set; }
        [NotMapped]
        public string QueryRelatedToName { get; set; }
    }
}
