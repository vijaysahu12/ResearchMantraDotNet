using System;

namespace RM.Model.RequestModel
{
    public class UpdatePinnedStatusRequestModel
    {
        public string Id { get; set; }
        public bool IsPinned { get; set; }
        public Guid? LoggedInUser { get; set; }
    }
}