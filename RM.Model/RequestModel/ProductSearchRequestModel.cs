namespace RM.Model.RequestModel
{
    public class ProductSearchRequestModel
    {
        public string SearchText { get; set; }
        public int? Category { get; set; }
        public int? Status { get; set; }
    }
}