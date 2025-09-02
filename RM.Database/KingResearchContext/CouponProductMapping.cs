namespace RM.Database.ResearchMantraContext
{
    public class CouponProductMappingM
    {
        public int Id { get; set; }
        public int CouponID { get; set; }
        public int? ProductID { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }
}

