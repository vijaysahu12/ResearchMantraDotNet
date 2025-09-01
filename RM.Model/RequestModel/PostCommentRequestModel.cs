using System.ComponentModel.DataAnnotations;

namespace RM.Model.RequestModel
{
    //public class PostCommentRequestModel
    //{
    //    [Required]
    //    public string Comment { get; set; }
    //    public string Mention { get; set; }
    //    [Required]
    //    public string BlogId { get; set; }
    //    public string? ParentCommentId { get; set; }
    //    [Required]
    //    public Guid CreatedBy { get; set; }
    //}


    public class PostCommentRequestModel
    {
        [Required]
        public string BlogId { get; set; }
        public string Mention { get; set; }

        [Required]
        public string Comment { get; set; }

        [Required]
        public string UserObjectId { get; set; }
    }


}
