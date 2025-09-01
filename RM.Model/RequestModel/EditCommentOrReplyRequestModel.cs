using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class EditCommentOrReplyRequestModel
    {
        [Required]
        public string ObjectId { get; set; }
        [Required]
        public string UserObjectId { get; set; }
        public string NewContent { get; set; }
        public string NewMention { get; set; }
        public string Type { get; set; }
    }
}
