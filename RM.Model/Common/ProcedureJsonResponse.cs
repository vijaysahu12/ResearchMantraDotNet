using System;
using System.Collections.Generic;

namespace RM.Model.Common
{
    public class GetProcedureJsonResponse
    {
        public string JsonData { get; set; }
    }

    public class JsonResponseModel : GetProcedureJsonResponse
    {
        public int TotalCount { get; set; }
    }

    public class KycJsonResponseModel : GetProcedureJsonResponse
    {
        public int TotalCount { get; set; }
        public List<KycItem>? FullData { get; set; }
        public List<string>? ServiceNames { get; set; }
    }

    public class KycItem
    {
        public int RowId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public bool? IsRpfApproved { get; set; }
        public string? Status { get; set; }
        public DateTime? RpfApprovedOn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string FilePath { get; set; }
        public string? ServiceName { get; set; }
        public string? Id { get; set; }
    }
}
