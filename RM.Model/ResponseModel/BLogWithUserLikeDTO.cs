using RM.Model.MongoDbCollection;

namespace RM.Model.ResponseModel
{
    public class BlogWithUserLikeDTO
    {
        public Blog Blog { get; set; }
        public bool UserHasLiked { get; set; }
    }
}
