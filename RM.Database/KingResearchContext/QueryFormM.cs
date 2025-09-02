using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Database.ResearchMantraContext
{
    public class QueryFormM
    {
        public int Id { get; set; }
        public Guid MobileUserId { get; set; }
        public int ProductId { get; set; }
        public string Questions { get; set; }
        public string? ScreenshotUrl { get; set; }
        public int QueryRelatedTo { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class QueryFormRemarks
    {
        public int Id { get; set; }
        public int QueryId { get; set; }
        public string Remarks { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
    }


}
