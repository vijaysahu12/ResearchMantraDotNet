using RM.CommonService;
using RM.CommonServices.Helpers;
using RM.Model;
using RM.Model.Common;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommunityController : ControllerBase
    {
        private readonly CommunityPostService _communityPostService;
        public CommunityController(CommunityPostService communityPostService)
        {
            _communityPostService = communityPostService;
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] CreateCommunityPostRequest request)
        {
            if (ModelState.IsValid)
            {
                _ = int.TryParse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid), out int loggedInUser);

                request.MobileUserId = loggedInUser;

                var response = await _communityPostService.Manage(request);

                return Ok(response);
            }
            else
            {
                return Ok(new
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid Request, please provide valid values"
                });
            }
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

        [HttpPost("GetCommunity")]

        public async Task<IActionResult> GetAllPosts(QueryValues query)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            Guid loggedInUserkey = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));

            query.LoggedInUser = loggedInUserkey.ToString();

            if (!long.TryParse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid), out long loggedInUser))
            {
                apiCommonResponse.StatusCode = HttpStatusCode.Unauthorized;
                apiCommonResponse.Message = "User is not authorized to access this method.";
                return Ok(apiCommonResponse);
            }

            //query.LoggedInUserId = loggedInUser;

            apiCommonResponse = await _communityPostService.GetAllPostsAsync(query);

            //// Wrap posts in ApiCommonResponseModel for consistency
            //apiCommonResponse.StatusCode = HttpStatusCode.OK;
            //apiCommonResponse.Data = posts;

            return Ok(apiCommonResponse);
        }


        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostById(string postId)
        {
            var post = await _communityPostService.GetPostByIdAsync(postId);

            if (post == null)
            {
                return NotFound(new { message = "Post not found" });
            }
            return Ok(post);
        }

        [HttpPut("Update/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCommunityPost(string id, [FromForm] CommunityPostUpdateModel updateModel)
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            updateModel.ModifiedBy = userPublicKey;

            var updatedPost = await _communityPostService.UpdateCommunityPost(id, updateModel);

            if (updatedPost == null)
            {
                return NotFound(new { message = "Post not found" });
            }

            return Ok(updatedPost);
        }

        [HttpPut("Delete/{id}")]
        public async Task<IActionResult> DeleteCommunityPost(string id)
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _communityPostService.DeleteCommunityPost(id, userPublicKey);

            return Ok(result);
        }

        [HttpPut("UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateCommunityPostStatus(string id)
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            var result = await _communityPostService.UpdateCommunityPostStatus(id, userPublicKey);

            return Ok(result);
        }

    }
}
