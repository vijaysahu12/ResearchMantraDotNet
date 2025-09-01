using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerformanceController : ControllerBase
    {
        private readonly IPerformanceService _service;

        public PerformanceController(IPerformanceService service)
        {
            _service = service;
        }

        /// <summary>
        /// For Mobile Application , this will help to get the perfromance based on topic.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>

        [AllowAnonymous]
        [HttpGet("GetPerformance")]
        public async Task<IActionResult> Get([FromQuery][Required] string code)
        {
            var result = await _service.GetPerformance(code);
            return Ok(result);
        }


        [Authorize]
        [HttpGet("GetPerformanceHeader")]
        public IActionResult GetPerformanceHeader()
        {
            var result = _service.GetPerformanceHeader();
            return Ok(result);
        }


    }
}
