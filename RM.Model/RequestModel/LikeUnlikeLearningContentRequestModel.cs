using System;

namespace RM.Model.RequestModel
{
    public class LikeUnlikeLearningContentRequestModel
    {
        public int Id { get; set; }
        public bool IsLiked { get; set; }
        public Guid UserKey { get; set; }
    }
}
