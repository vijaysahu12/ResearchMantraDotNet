using System;

namespace RM.Model.ResponseModel
{
    public class BlogModel
    {
        public class CommentDto
        {
            public string ObjectId { get; set; }
            public string Content { get; set; }
            public string Mention { get; set; }
            public string CreatorName { get; set; }
            public bool IsActive { get; set; }
            public bool IsDelete { get; set; }
            public DateTime? ModifiedOn { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public class ReplyDto
        {
            public string ObjectId { get; set; }
            public string Content { get; set; }
            public string Mention { get; set; }
            public string CreatorName { get; set; }
            public bool IsActive { get; set; }
            public bool IsDelete { get; set; }
            public DateTime? ModifiedOn { get; set; }
            public DateTime CreatedOn { get; set; }
        }
       

    }
}