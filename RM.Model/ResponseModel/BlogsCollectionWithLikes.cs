namespace RM.Model.ResponseModel
{
    public class BlogsCollectionWithLikes
    {
        public long TotalComments { get; set; }
        public long TotalLikes { get; set; }
        public bool UserHasLiked { get; set; }
    }
}
