using System;

namespace RM.Model.ResponseModel
{
    public class FollowUpReminderResponseModel
    {
        public int Id { get; set; }
        public Guid AssignedTo { get; set; }
        public string FullName { get; set; }
        public string Comments { get; set; }
        public DateTime NotifyDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class UntouchedLeadsResponseModel
    {

        public Guid PublicKey { get; set; }
        public string FullName { get; set; }
        public Guid AssignedTo { get; set; }
        public int DateDiff { get; set; }
        public string Comments { get; set; }
        public Guid AdminRole { get; set; }
    }

}
