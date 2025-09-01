using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Services;
using RM.CommonServices.Helpers;
using RM.CommonServices.Services;
using RM.Database.KingResearchContext;
using RM.Model.Common;
using RM.Model.RequestModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using RM.MService.Services;


namespace RM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        private readonly IBlogService _blogService;

        public BlogController(MongoDbService mongoDbService, IBlogService blogService)
        {
            _mongoDbService = mongoDbService;
            _blogService = blogService;
        }


            [HttpPost("Blogs")]
            public async Task<IActionResult> GetBlogs(QueryValues query)
            {
                //var response = await _mongoDbService.GetAllBlogs(query);
                var response = await _blogService.GetAllBlogs(query);
                return Ok(response);
            }


        [HttpPost("ManageUserPostPermission")]
        public async Task<IActionResult> ManageUserPostPermission([FromBody] RestrictUserRequestModel request)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var loggedInUser))
            {
                return Unauthorized(new { Message = "Invalid or missing user token." });
            }

            request.LoggedInUser = loggedInUser;

            var result = await _blogService.ManageUserPostPermissionAsync(request);
            return Ok(result);
        }



        [HttpDelete("Blog/{id}")]
        public async Task<IActionResult> DeleteBlog(string id)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var loggedInUser))
            {
                return Unauthorized(new { Message = "Invalid or missing user token." });
            }

            var response = await _blogService.DeleteBlogAsync(id, loggedInUser);

            return Ok(response);
        }

        [HttpPost("ManageBlogStatus")]
        public async Task<IActionResult> ManageBlogStatus([FromBody] UpdateBlogStatusRequestModel request)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var loggedInUser))
            {
                return Unauthorized(new { Message = "Invalid or missing user token." });
            }

            request.LoggedInUser = loggedInUser;

            return Ok(await _blogService.ManageBlogStatusAsync(request));
        }

        [HttpPost("ManageBlogPinStatus")]
        public async Task<IActionResult> ManageBlogPinStatus([FromBody] UpdatePinnedStatusRequestModel request)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var loggedInUser))
            {
                return Unauthorized(new { Message = "Invalid or missing user token." });
            }

            request.LoggedInUser = loggedInUser;

            return Ok(await _blogService.ManageBlogPinStatusAsync(request));
        }

        [HttpPost("CreateBlogPost")]
        public async Task<IActionResult> CreatePost([FromForm] CommunityPostRequestModel model)
        {
            var response = await _blogService.CreateBlogForCrmPostAsync(model);
            return Ok(response);
        }



        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment(PostCommentRequestModel request)
        {
            if (!Guid.TryParse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"), out var loggedInUser))
            {
                return Unauthorized(new { Message = "Invalid or missing user token." });
            }

            return Ok(await _mongoDbService.AddBlogComment(request, loggedInUser));
}
    
    }
}
