using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class CommentReplyRequestModel
    {
        [Required]
        public string Reply { get; set; }
        [Required]
        public string UserObjectId { get; set; }
        [Required]
        public string CommentId { get; set; }
        public string Mention { get; set; }

    }
}
