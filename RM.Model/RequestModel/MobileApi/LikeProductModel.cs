namespace RM.Model.RequestModel.MobileApi
{
    public class LikeProductModel
    {
        public int ProductId { get; set; }
        public int LikeId { get; set; }
        public string CreatedBy { get; set; }
        public string Action { get; set; }
    }
}
