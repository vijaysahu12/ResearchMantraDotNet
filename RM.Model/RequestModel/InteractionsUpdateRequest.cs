namespace RM.Model.RequestModel
{
    public class InteractionsUpdateRequest
    {

        //public int LikeCountDelta { get; set; }
        //public int HeartCountDelta { get; set; }
        public string ReplyContent { get; set; }
        //public bool Unlike { get; set; }
        public string Hashtags { get; set; }
        public int? ParentReplyId { get; set; }
        public string Mentions { get; set; }
        //public string Type { get; set; }
        //public int Value { get; set; }
        public int UserId { get; set; }
    }

}
