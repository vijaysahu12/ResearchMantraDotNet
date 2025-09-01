using System;

namespace RM.Database.KingResearchContext
{
    public class BlogComments
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string Comment { get; set; }
        public string Mention { get; set; }
        public int? ParentCommentId { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }
    }
}
