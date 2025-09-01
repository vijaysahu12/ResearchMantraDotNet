using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    public class EditBlogRequestModel
    {
        [Required]
        public string BlogId { get; set; }
        [Required]
        public string UserObjectId { get; set; }
        public string NewContent { get; set; }

        public string NewHashtag { get; set; }
    }
}
