using System;

namespace RM.Model.RequestModel
{
    public class RestrictUserRequestModel
    {
        public string BlogId { get; set; }
        public string CreatedBy { get; set; }
        public bool CanCommunityPost { get; set; }
        public Guid? LoggedInUser { get; set; }
    }
}