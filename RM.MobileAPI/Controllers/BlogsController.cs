using RM.Model.RequestModel;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BlogsController(IBlogService blogService) : ControllerBase
    {
        private readonly IBlogService blogService = blogService;

        /// <summary>
        /// To save a blog in the blogs collection when ever a user posts any blog.
        /// </summary>
        [HttpPost("Add")]
        public async Task<IActionResult> PostBlog([FromForm] PostBlogRequestModel obj)
        {
            return Ok(await blogService.PostBlog(obj));
        }


        /// <summary>
        /// To fetch the list of blogs for the user with pagination.
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetBlogs([Required] int pageNumber, [Required] int pageSize, [Required] Guid publicKey)
        {
            return Ok(await blogService.GetBlogsAsync(pageNumber, pageSize, publicKey));
        }

        /// <summary>
        /// If the user want to disable the comment section of the blog.
        /// </summary>
        [HttpPost("DisableBlogComment")]
        public async Task<IActionResult> DisableBlogComment(DisableBlogCommentRequestModel request)
        {
            return Ok(await blogService.DisableBlogComment(request));
        }

        /// <summary>
        /// To delete a blog (soft delete).
        /// </summary>
        [HttpDelete("DeleteBlog")]
        public async Task<IActionResult> DeleteBlog([Required] string blogId, [Required] string userId)
        {
            return Ok(await blogService.DeleteBlog(blogId, userId));
        }

        /// <summary>
        /// To post a comment on the blog. This will add a document in the comments collection for the blog.
        /// </summary>
        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment(PostCommentRequestModel request)
        {
            return Ok(await blogService.AddComment(request));
        }

        /// <summary>
        /// To post a comment on the blog. This will add a document in the reply collection for the comment.
        /// </summary>
        [HttpPost("CommentReply")]
        public async Task<IActionResult> CommentReply(CommentReplyRequestModel request)
        {
            return Ok(await blogService.CommentReply(request));
        }

        /// <summary>
        /// To fetch the comments for the blog with pagination.
        /// </summary>
        [HttpGet("GetComments")]
        public async Task<IActionResult> GetComments([Required] string blogId, [Required] int pageNumber, [Required] int pageSize)
        {
            return Ok(await blogService.GetComments(blogId, pageNumber, pageSize));
        }

        [HttpGet("GetReplies")]
        public async Task<IActionResult> GetReplies([Required] string commentId)
        {
            return Ok(await blogService.GetReplies(commentId));
        }

        [HttpDelete("DeleteCommentOrReply")]
        public async Task<IActionResult> DeleteCommentOrReply([Required] string objectId, [Required] string userId, [Required] string type)
        {
            return Ok(await blogService.DeleteCommentOrReply(objectId, userId, type));
        }

        [HttpGet("GetImage")]
        [AllowAnonymous]
        public IActionResult GetBlogImage(string imageName)
        {
            var rootDirectory = Directory.GetCurrentDirectory();
            var imagePath = Path.Combine(rootDirectory, "Assets", "Blog-images", imageName);

            var imageFileName = Path.GetFileName(imagePath);
            var imageExtension = Path.GetExtension(imageFileName);

            if (System.IO.File.Exists(imagePath))
            {
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, $"image/{imageExtension.TrimStart('.').ToLower()}");
            }
            else
            {
                return NotFound($"Image not found.");
            }
        }

        [HttpPost("Like")]
        public async Task<IActionResult> LikeBlog([FromBody] LikeBlogRequestModel request)
        {
            return Ok(await blogService.LikeBlog(request.UserObjectId, request.BlogId, request.IsLiked));
        }

        [HttpPost("EditBlog")]
        public async Task<IActionResult> EditBlog(EditBlogRequestModel request)
        {
            return Ok(await blogService.EditBlog(request));
        }

        [HttpPost("EditCommentOrReply")]
        public async Task<IActionResult> EditCommentOrReply(EditCommentOrReplyRequestModel request)
        {
            return Ok(await blogService.EditCommentOrReply(request));
        }

        [HttpGet("DisableCommunityPostForUser")]
        public async Task<IActionResult> DisableCommunityPostForUser(Guid userKey)
        {
            return Ok(await blogService.DisableCommunityPostForUser(userKey));
        }

        /// <summary>
        /// To report a blog for posting something irrelevant 
        /// If a blog gets reported 5 times then the user won't be able to post anything in the blogs screen and a notification will be sent to the user regarding the action.
        /// </summary>
        [HttpPost("ReportBlog")]
        public async Task<IActionResult> ReportBlog([FromBody] ReportBlogRequestModel request)
        {
            return Ok(await blogService.ReportBlog(request));
        }

        /// <summary>
        /// While reporting a blog or user the statements for the reason can be fetched from here.
        /// </summary>
        [HttpGet("GetReportReason")]
        public async Task<IActionResult> GetReportReason()
        {
            return Ok(await blogService.GetReportReason());
        }

        [HttpPost("BlockUser")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequestModel request)
        {
            return Ok(await blogService.BlockUser(request));
        }

        [HttpGet("GetBlockedUser")]
        public async Task<IActionResult> GetBlockedUser(string? mobileUserKey)
        {
            return Ok(await blogService.GetBlockedUser(mobileUserKey));
        }
    }
}