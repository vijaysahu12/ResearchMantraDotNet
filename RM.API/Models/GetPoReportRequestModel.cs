using System;
using System.ComponentModel.DataAnnotations;

namespace RM.API.Models
{
    public class GetPoReportRequestModel
    {
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int StatusId { get; set; }
        public string ClientName { get; set; }
        public int LeadSourceId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
