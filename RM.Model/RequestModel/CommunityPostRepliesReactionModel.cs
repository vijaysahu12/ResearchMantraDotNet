namespace RM.Model.RequestModel
{
    public class CommunityPostRepliesReactionModel
    {
        public int EntityId { get; set; } // Id of the post or reply
        public int UserId { get; set; }
        public bool IsLike { get; set; }
        public bool IsHeart { get; set; }
        public bool IsDislike { get; set; }
        public bool IsPost { get; set; } // Indicates whether it's a post or reply reaction
    }
}
