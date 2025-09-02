using Google.Api.Gax.ResourceNames;
using RM.API.Services;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.Entity.Infrastructure.Interception;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KycController : ControllerBase
    {
        private readonly IKycService _ikycService;
        private readonly IWebHostEnvironment _env;
        public KycController(IKycService ikycService, IWebHostEnvironment env)
        {
            _ikycService = ikycService;
            _env = env;
        }
        [HttpPost("GetKycDetails")]
        public async Task<IActionResult> Get(QueryValues query)
        {
            var response = await _ikycService.GetKycDetails(query);

            return Ok(response);
        }

        [AllowAnonymous]
        [DisableRequestSizeLimit]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitRpf([FromForm] RpfSubmission form)
        {

            Guid LocalId = new Guid();


            var entity = new RpfForm
            {
                Id = LocalId,
                Name = form.Name,
                Pan = form.Pan,
                Aadhaar = form.Aadhaar,
                Email = form.Email,
                CountryCode = form.CountryCode,
                Mobile = form.Mobile,
                Q1 = form.Q1,
                Q2 = form.Q2,
                Q3 = form.Q3,
                Q4 = form.Q4,
                Q5 = form.Q5,
                Q6 = form.Q6,
                Q7 = form.Q7,
                Q8 = form.Q8,
                Q9 = form.Q9,
                Q10 = form.Q10,
                Q11 = form.Q11,
                Q12 = form.Q12,
                Q13 = form.Q13,
                Q14 = form.Q14,
                Q14OtherText = form.Q14OtherText,
                Q15 = form.Q15,
                Q16 = form.Q16,
                Q17 = form.Q17,
                Q18 = form.Q18,
                Q19 = form.Q19,
                Q20 = form.Q20,
                Q21 = form.Q21,
                Q22 = form.Q22,
                RiskConsent = form.RiskConsent,
                SupportConsent = form.SupportConsent,
                CreatedOn = form.CreatedOn,
                AdviserConsent = form.AdviserConsent,
                DeclarationConsent = form.DeclarationConsent
            };

            ApiCommonResponseModel response = await _ikycService.SubmitRpfAsync(entity, form.signatureFront, form.signatureEnd);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)

            {
                return BadRequest(new { message = "Failed to submit KYC." + response.Message });
            }
            return Ok(response);
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folderName, string id)
        {
            string sharedKycDirectory = @"D:\SharedFiles\KycPdf";
            if (!Directory.Exists(sharedKycDirectory))
            {
                Directory.CreateDirectory(sharedKycDirectory);
            }

            var fileName = $"{id}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(sharedKycDirectory, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("uploads", folderName, fileName);

        }
    }
}