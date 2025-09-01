namespace RM.Model.RequestModel
{
    public class ReportBlogRequestModel
    {
        public string BlogId { get; set; }
        public string ReportedBy { get; set; }
        public string ReasonId { get; set; }
        public string? Description { get; set; }
    }
}
