using RM.API.Dtos;
using RM.API.Interfaces;
using RM.API.Models;
using RM.API.Models.Mobile;
using RM.Database.ResearchMantraContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
namespace RM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileAuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly ResearchMantraContext _context;

        public MobileAuthController(IAuthRepository repo, IConfiguration config, ResearchMantraContext context)
        {
            _repo = repo;
            _config = config;
            _context = context;
        }


        [AllowAnonymous]
        [HttpPost("MobileLogin")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            CommonResponse commonResponse = new();

            try
            {
                MobileUserResponse userFromRepo = null;

                userFromRepo = await _repo.MobileUserSignIn(userForLoginDto.Username.ToLower());

                if (userFromRepo == null)
                {
                    return Unauthorized();
                }

                // if user exists then create token 
                //now we create 2 claims one with user id & other one is username to identify the user
                Claim[] claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userFromRepo.PublicKey.ToString()),
                    new Claim(ClaimTypes.Name, userFromRepo.FullName),
                    new Claim(ClaimTypes.MobilePhone, userFromRepo.MobileNumber),
                    new Claim(ClaimTypes.Email, userFromRepo.EmailId),
                    new Claim(ClaimTypes.Role,userFromRepo.RoleKey ?? "user"),
                    new Claim(ClaimTypes.PrimarySid, userFromRepo.Id.ToString())
                };

                // now we are creating a signing in key 
                SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

                // encrypting the key 
                SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512Signature);

                // creating our token with passing our claim subject, expirty details & signing credentials
                SecurityTokenDescriptor tokenDescriptor = new()
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = creds
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = (JwtSecurityToken)tokenHandler.CreateToken(tokenDescriptor);
                jwtToken.Header["kid"] = "my-signing-key-1234";  // Replace with your key identifier

                //creation of token using the token descriptor
                commonResponse.Data = jwtToken;
                commonResponse.StatusCode = StatusCodes.Status200OK;
                commonResponse.Message = "Login Successfull";
                return Ok(commonResponse);
            }
            catch (Exception ex)
            {
                commonResponse.StatusCode = StatusCodes.Status500InternalServerError;
                commonResponse.Message = "Internal Exception:" + ex.ToString();
                return Ok(commonResponse);
            }
        }

        [AllowAnonymous]

        [HttpPost]
        [Route("MobileSignUp")]
        public async Task<ActionResult<User>> MobileUserSignUp(MobileRequest mobileRequest)
        {
            CommonResponse response = new();
            try
            {
                Lead leadExists = await _context.Leads.Where(lea => lea.MobileNumber == mobileRequest.Mobile).FirstOrDefaultAsync();

                if (mobileRequest.RequestType == "BeforeOTPVerfication")
                {
                    string gui = new Guid().ToString();

                    if (leadExists == null)
                    {
                        leadExists = new Lead
                        {
                            CreatedOn = DateTime.Now,
                            ModifiedOn = DateTime.Now,
                            MobileNumber = mobileRequest.Mobile,
                            CreatedBy = gui,
                            ModifiedBy = gui
                        };
                        _ = _context.Leads.Add(leadExists);
                        _ = await _context.SaveChangesAsync();
                    }

                    if (leadExists != null && leadExists.PublicKey == Guid.Empty)
                    {
                        Lead dd = await _context.Leads.Where(item => item.Id == leadExists.Id).FirstOrDefaultAsync();
                        leadExists.PublicKey = dd.PublicKey;
                    }
                    MobileUser mobileUserDto = await _context.MobileUsers.FirstOrDefaultAsync(l => l.LeadKey == leadExists.PublicKey);
                    if (mobileUserDto == null)
                    {
                        mobileUserDto = new MobileUser
                        {
                            LeadKey = leadExists.PublicKey,
                            IsActive = true,
                            IsDelete = false,
                            OneTimePassword = "123456",
                            CreatedOn = DateTime.Now,
                            ModifiedOn = DateTime.Now,
                            IsOtpVerified = false,
                            MobileToken = "NA",
                            DeviceType = "NA"
                        };
                        _ = _context.MobileUsers.Add(mobileUserDto);
                    }
                    else
                    {
                        mobileUserDto.OneTimePassword = "123456";
                        mobileUserDto.IsOtpVerified = false;
                        mobileUserDto.ModifiedOn = DateTime.Now;
                        _ = _context.MobileUsers.Update(mobileUserDto);

                    }
                    _ = await _context.SaveChangesAsync();
                    response.StatusCode = StatusCodes.Status200OK;
                    response.Message = "Enter the OTP To Continue";
                    return Ok(response);
                }
                else if (mobileRequest.RequestType == "AfterOTPVerfication")
                {
                    Lead lead = await _context.Leads.FirstOrDefaultAsync(le => le.Id == leadExists.Id);

                    lead.FullName = mobileRequest.FullName;
                    lead.EmailId = mobileRequest.Email;
                    _ = _context.Leads.Update(lead);


                    MobileUser mobile = await _context.MobileUsers.FirstOrDefaultAsync(item => item.LeadKey == lead.PublicKey);

                    if (mobile != null)
                    {
                        _ = _context.MobileUsers.Update(mobile);
                    }

                    _ = await _context.SaveChangesAsync();
                    response.Message = "Successfully Completed the signup process.";
                }
                return Ok(response);

            }
            catch (Exception)
            {
                response.Message = "Exception while creating new user.";
                response.StatusCode = StatusCodes.Status500InternalServerError;
                return BadRequest(response);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("OTPVerification")]
        public async Task<IActionResult> OtpVerification(string mobile, string otp)
        {
            CommonResponse commonResponse = new();
            var result = await (from o in _context.Leads
                          join u in _context.MobileUsers
                          on o.PublicKey equals u.LeadKey
                          where
                          o.MobileNumber == mobile &&
                          u.OneTimePassword == otp &&
                          u.IsDelete == false &&
                          u.IsActive == true
                          select new
                          {
                              u.IsOtpVerified,
                              u.LeadKey,
                              o.MobileNumber
                          }
                          ).FirstOrDefaultAsync();


            if (result != null && result.MobileNumber.Length > 0)
            {

                MobileUser mobileUser = await _context.MobileUsers.Where(le => le.LeadKey == result.LeadKey).FirstOrDefaultAsync();


                mobileUser.IsOtpVerified = true;
                mobileUser.IsActive = true;
                mobileUser.IsDelete = false;
                mobileUser.ModifiedOn = DateTime.Now;
                _ = _context.MobileUsers.Update(mobileUser);
                _ = await _context.SaveChangesAsync();

                var lead = await _context.Leads.Where(lead => lead.PublicKey == mobileUser.LeadKey).Select(item => new
                {
                    item.EmailId,
                    item.MobileNumber,
                    item.IsDisabled,
                    item.PublicKey
                }).FirstOrDefaultAsync();

                commonResponse.Message = "OTP Verified";
                commonResponse.StatusCode = StatusCodes.Status200OK;
                commonResponse.Data = lead;
                return Ok(commonResponse);
            }
            else
            {
                commonResponse.Message = "OTP Verification Failed";
                commonResponse.StatusCode = StatusCodes.Status404NotFound;
                return NotFound(commonResponse);
            }
        }

    }
}
