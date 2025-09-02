using System;

namespace RM.Database.ResearchMantraContext
{
    public class LearningContentLikesM
    {
        public long Id { get; set; }
        public long LearningContentId { get; set; }
        public Guid UserKey { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsLiked { get; set; }
    }
}
