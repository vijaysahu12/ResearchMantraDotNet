using System;

namespace RM.Model.RequestModel
{
    public class SetRemindersRequestModel
    {
        public int Id { get; set; }
        public Guid LeadKey { get; set; }
        public string Comments { get; set; }
        public DateTime NotifyDate { get; set; }
        //public Guid CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public string Action { get; set; }

    }
}
