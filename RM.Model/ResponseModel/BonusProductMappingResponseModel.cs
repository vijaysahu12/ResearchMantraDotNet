using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RM.Model.ResponseModel
{
    public class BonusProductMappingResponseModel
    {
        public int? Id { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? BonusProductId { get; set; }
        public int? DurationInDays { get; set; }
        public string? BonusProductName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
