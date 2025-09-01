using System;

namespace RM.Model.RequestModel
{
    public class UpdateBlogStatusRequestModel
    {
        public string BlogId { get; set; }
        public bool BlogStatus { get; set; }
        public Guid? LoggedInUser { get; set; }
    }
}