namespace RM.Model.ResponseModel
{
    public class CommunityPostLikeDislikeResponseModel
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        //public string Type { get; set; } // Like or Dislike
        //public int Value { get; set; } // 1 for Like, -1 for Dislike
    }
}
