using RM.API.Models.Mail;
using RM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly IMailService mailService;
        public MailController(IMailService mailService)
        {
            this.mailService = mailService;
        }
        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendMail(MailRequest request)
        {
            try
            {
                await mailService.SendEmailAsync(request);
                //await mailService.SendEmailAsyncTwo(request);
                return Ok();
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}