namespace RM.Model.ResponseModel
{
    public class GetCouponsSpResponse
    {
        public string CouponName { get; set; }
        public string Description { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? DiscountPercentage { get; set; }
    }
}
