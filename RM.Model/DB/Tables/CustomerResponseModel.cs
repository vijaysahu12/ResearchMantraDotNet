using System;

namespace RM.Model.DB.Tables
{
    public class CustomerResponseModel
    {
        public int Id { get; set; }
        public Guid LeadPublicKey { get; set; }
        public string LeadName { get; set; }
        public string MobileNumber { get; set; }
        public string EmailId { get; set; }
        public string AssignedToName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PaidAmount { get; set; }
        public int DaysToGo { get; set; }
        public string Status { get; set; }
        public string PrStatusName { get; set; }
        public int StatusId { get; set; }
    }

    public class FilterValues
    {
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
        public string ThirdKey { get; set; }
        public string FourthKey { get; set; }
        public string FifthKey { get; set; }
        public string SearchText { get; set; }
        public int IsPaging { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public string SortOrder { get; set; }
        public string SortExpression { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string AssignedTo { get; set; }

    }
}
