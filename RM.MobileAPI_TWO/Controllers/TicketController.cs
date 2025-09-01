using RM.Model.RequestModel;
using RM.Model.RequestModel.MobileApi;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid mobileUserKey)
        {
            var result = await _ticketService.Get(mobileUserKey);
            return Ok(result);
        }
        [AllowAnonymous]

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] ManageTicketRequestModel objReqeust)
        {
            var result = await _ticketService.Add(objReqeust);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("GetSupportMobile")]
        public IActionResult GetSupportMobile()
        {
            return Ok(_ticketService.GetSupportMobile());
        }

        [HttpPost("AddTicketComment")]
        public async Task<IActionResult> AddTicketComment([FromForm] AddTicketCommentRequestModel request)
        {
            //var mobileUserId = Guid.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            return Ok(await _ticketService.AddTicketComment(request));
        }

        [HttpPost("GetTicketDetails")]
        public async Task<IActionResult> GetTicketDetails([FromQuery][Required] int id)
        {
            return Ok(await _ticketService.GetTicketDetails(id));
        }
        [AllowAnonymous]
        [HttpGet("GetTicketImage")]
        public IActionResult GetTicketImage(string imageName)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Ticket-Images", imageName);

            var imageFileName = Path.GetFileName(imagePath);
            var imageExtension = Path.GetExtension(imageFileName);

            if (System.IO.File.Exists(imagePath))
            {
                var imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return File(imageBytes, $"image/{imageExtension.TrimStart('.').ToLower()}");
            }

            return NotFound($"Image '{imageName}' not found.");
        }
    }
}