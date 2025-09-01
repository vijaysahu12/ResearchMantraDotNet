using System;

namespace RM.Database.KingResearchContext
{
    public class PostReactions
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public bool IsLike { get; set; }
        public bool IsHeart { get; set; }
        public bool IsDislike { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
    }

}
