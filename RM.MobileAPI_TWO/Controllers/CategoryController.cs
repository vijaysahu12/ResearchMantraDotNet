using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RM.MobileAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]

        [Route("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _categoryService.GetCategories());
        }

        [HttpGet]
        [Route("GetStrategies")]
        public async Task<IActionResult> GetStrategies(string strategyName)
        {
            return Ok(await _categoryService.GetStrategies(strategyName));
        }
    }
}
