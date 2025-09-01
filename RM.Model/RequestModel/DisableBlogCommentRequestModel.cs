using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class DisableBlogCommentRequestModel
    {
        [Required]
        public string BlogId { get; set; }
        [Required]
        public string CreatedByObjectId { get; set; }
    }
}
