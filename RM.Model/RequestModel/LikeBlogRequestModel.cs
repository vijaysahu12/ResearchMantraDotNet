using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class LikeBlogRequestModel
    {
        [Required]
        public string UserObjectId { get; set; }
        [Required]
        public string BlogId { get; set; }
        [Required]
        public bool IsLiked { get; set; }
    }
}
