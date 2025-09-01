using System;

namespace RM.Model.RequestModel
{
    public class GetLearningContentList
    {
        public int Id { get; set; }
        public Guid UserKey { get; set; }
    }
}
