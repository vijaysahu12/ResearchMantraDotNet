using System;

namespace RM.API.Models.Reports
{
    public class CallsTopPerformersReportsResponseModel
    {
        public Guid UserKey { get; set; }
        public string Name { get; set; }
        public int TotalCalls { get; set; }
        public decimal TotalRoi { get; set; }
        public int StrategyCount { get; set; }
        public decimal PNL { get; set; }
        public decimal TotalCapital { get; set; }
        public decimal AverageCapital { get; set; }
        public decimal Accuracy { get; set; }
    }
}
