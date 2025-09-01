using RM.API.Services;
using RM.CommonServices.Helpers;
using RM.Model.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LearningMaterialController : ControllerBase
    {
        private readonly ILearningMaterialService _materialService;

        public LearningMaterialController(ILearningMaterialService researchService)
        {
            _materialService = researchService;
        }

        [HttpGet("GetLearningMaterialCategory")]
        public async Task<IActionResult> GetLearningMaterialCategory()
        {
            return Ok(await _materialService.GetLearningMaterialCategory());
        }

        [HttpGet("GetLearningMaterialItemBasedOnCategoryId")]
        public IActionResult GetLearningMaterialItemBasedOnCategoryId(int Id)
        {
            return Ok(_materialService.GetLearningMaterialItemBasedOnCategoryId(Id));
        }

        [HttpPost("ManageLearningCategory")]
        public async Task<IActionResult> ManageLearningCategory([FromForm] LearningMaterialModel.ManageLearningCategoryRequestModel request)
        {
            var loginUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _materialService.ManageLearningCategory(request, loginUser));
        }

        [HttpPost("UpdateCategoryStatus")]
        public async Task<IActionResult> UpdateCategoryStatus([FromBody] LearningMaterialModel.ManageLearningCategoryRequestModel request)
        {
            request.ModifiedBy = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _materialService.UpdateCategoryStatus(request));
        }

        [HttpPost("ManageLearningContent")]
        public async Task<IActionResult> ManageLearningContent([FromForm] LearningMaterialModel.ManageLearningContentRequestModel request)
        {
            request.LoggedUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            return Ok(await _materialService.ManageLearningContent(request));
        }

        [HttpGet("LearningMaterialContentActiveToggle/{learningMaterialContentId}")]
        public async Task<IActionResult> LearningMaterialContentActiveToggle(int learningMaterialContentId)
        {
            return Ok(await _materialService.LearningMaterialContentActiveToggle(learningMaterialContentId));
        }

        [HttpGet("GetImage")]
        [AllowAnonymous]
        public IActionResult GetImage(string imageName)
        {
            string rootDirectory = Directory.GetCurrentDirectory();
            string imagePath = Path.Combine(rootDirectory, "Assets", "Products", imageName);

            string imageFileName = Path.GetFileName(imagePath);
            string imageExtension = Path.GetExtension(imageFileName);

            if (System.IO.File.Exists(imagePath))
            {
                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, $"image/{imageExtension.TrimStart('.').ToLower()}");
            }
            else
            {
                return NotFound($"Image '{imageName}' not found.");
            }
        }
    }
}