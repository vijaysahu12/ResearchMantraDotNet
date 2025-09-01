using System;

namespace RM.Model.RequestModel
{
    public class GetPerformanceRequestModel
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
    }
}
