using System;

namespace RM.Database.KingResearchContext
{
    public class ProductLikesM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        //public string Type { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
