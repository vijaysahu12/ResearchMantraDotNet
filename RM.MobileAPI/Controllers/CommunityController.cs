using RM.CommonService;
using RM.CommonServices.Helpers;
using RM.Model;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.MService.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly CommunityPostService _communityPostService;

        public CommunityController(CommunityPostService communityPostService)
        {
            _communityPostService = communityPostService;
        }

        [HttpPost("GetCommunity")]
        public async Task<IActionResult> Get([FromBody] GetCommunityRequestModel request)
        {
            var userId = long.TryParse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid), out var id) ? id : 0;
            request.LoggedInUserId = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));


            var response = await _communityPostService.Get(request, userId);
            return Ok(response);
        }

        [HttpGet("GetCommunityDetails")]
        public async Task<IActionResult> GetCommunityDetails()
        {
            _ = long.TryParse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid), out long loggedInUser);
            var response = await _communityPostService.GetCommunityDetails(loggedInUser);
            return Ok(response);
        }

        [HttpPost("v2/GetCommunity")]
        public async Task<IActionResult> GetV2([FromBody] GetCommunityRequestModel request)
        {
            var loggedInUser = long.TryParse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid), out var id) ? id : 0;

            var response = await _communityPostService.GetV2(request, loggedInUser);

            return Ok(response);
        }


        [HttpPost("CreateBlogCommunity")]
        public async Task<IActionResult> CreateBlogCommunity([FromForm] CreateCommunity request)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid Request, please provide valid values"
                });
            }

            _ = int.TryParse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid), out int mobileUserId);

            // Clean all string properties (like Id, Title, Content, Url, IsApproved)
            SanitizeStrings(request);

            var response = await _communityPostService.ManageBlogCommunity(request, mobileUserId);
            return Ok(response);
        }

        [HttpPut("Delete/{id}")]
        public async Task<IActionResult> DeleteCommunityPost(string id)
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _communityPostService.DeleteCommunityPost(id, userPublicKey);

            return Ok(result);
        }


        [HttpGet("GetCommunityPostTypes")]
        public IActionResult GetCommunityPostTypesList()
        {
            var postTypes = _communityPostService.FetchCommunityPostTypes();  // Calling public wrapper
            return Ok(new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Community post types fetched successfully.",
                Data = postTypes,
                Exceptions = null
            });
        }

        [HttpPost("LikeBlog")]
        public async Task<IActionResult> LikeBlog([FromBody] LikeBlogRequestModel request)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var userKey))
            {
                return Ok(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid user key."
                });
            }

            var result = await _communityPostService.LikeBlogCommunity(userKey.ToString(), request.BlogId, request.IsLiked);
            return Ok(result);
        }

        [HttpGet("GetComments")]
        public async Task<IActionResult> GetComments([Required] string blogId, [Required] int pageNumber, [Required] int pageSize)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var userKey))
            {
                return Ok(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid user key."
                });
            }

            var comments = await _communityPostService.GetComments(blogId, pageNumber, pageSize);
            return Ok(comments);
        }


        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment(PostCommentRequestModel request)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var userKey))
            {
                return Ok(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid user key."
                });
            }
            //request.UserObjectId = userKey.ToString();

            var result = await _communityPostService.AddComment(request);
            return Ok(result);
        }

        [HttpPost("CommentReply")]
        public async Task<IActionResult> CommentReply(CommentReplyRequestModel request)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var userKey))
            {
                return Ok(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid user key."
                });
            }

            var result = await _communityPostService.CommentReply(request);
            return Ok(result);
        }


        [HttpGet("GetReplies")]
        public async Task<IActionResult> GetReplies([Required] string commentId)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var userKey))
            {
                return Ok(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid user key."
                });
            }

            var result = await _communityPostService.GetReplies(commentId);

            return Ok(result);
        }

        [HttpPost("EditCommentOrReply")]
        public async Task<IActionResult> EditCommentOrReply(EditCommentOrReplyRequestModel request)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var userKey))
            {
                return Ok(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid user key."
                });
            }

            var result = await _communityPostService.EditCommentOrReply(request);

            return Ok(result);
        }

        [HttpDelete("DeleteCommentOrReply")]
        public async Task<IActionResult> DeleteCommentOrReply([Required] string objectId, [Required] string userId, [Required] string type)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var userKey))
            {
                return Ok(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid user key."
                });
            }
            var result = await _communityPostService.DeleteCommentOrReply(objectId, userId, type);
            return Ok(result);
        }

        [HttpPost("BlockUser")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequestModel request)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var userKey))
            {
                return Ok(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid user key."
                });
            }
            request.UserKey = userKey;
            return Ok(await _communityPostService.BlockUser(request));
        }

        [HttpPost("DisableCommunityComment")]
        public async Task<IActionResult> DisableBlogComment(DisableBlogCommentRequestModel request)
        {
            return Ok(await _communityPostService.DisableBlogComment(request));
        }

        /// <summary>
        /// To report a blog for posting something irrelevant 
        /// /// If a blog gets reported 5 times then the user won't be able to post anything in the blogs screen and a notification will be sent to the user regarding the action.
        /// </summary>
        [HttpPost("ReportBlog")]
        public async Task<IActionResult> ReportBlog([FromBody] ReportBlogRequestModel request)
        {
            _ = int.TryParse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid), out int mobileUserId);

            if (!ModelState.IsValid)
            {
                return Ok(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid Request, please provide valid values"
                });
            }
            return Ok(await _communityPostService.ReportBlog(request,mobileUserId));
        }






        // Helper method to sanitize input
        private void SanitizeStrings(object obj)
        {
            var stringProperties = obj.GetType()
                                      .GetProperties()
                                      .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

            foreach (var prop in stringProperties)
            {
                var value = (string?)prop.GetValue(obj);
                if (string.IsNullOrWhiteSpace(value) || value.Trim().ToLower() == "string")
                {
                    prop.SetValue(obj, null);
                }
            }
        }

    }

}
