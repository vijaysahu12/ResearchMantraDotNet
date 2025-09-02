using RM.API.Dtos;
using RM.API.Interfaces;
using RM.Database.ResearchMantraContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public UserAuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            try
            {
                // check if user exists
                User userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

                if (userFromRepo == null)
                {
                    return Unauthorized();
                }
                //assigned file by converting to byt array
                if (userFromRepo.UserImage != null && userFromRepo.UserImage != "" && System.IO.File.Exists(userFromRepo.UserImage))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(userFromRepo.UserImage);
                    userFromRepo.UserImage = Convert.ToBase64String(bytes);
                }
                else
                {
                    userFromRepo.UserImage = null;
                }

                string roleName = await _repo.GetRoleName(Guid.Parse(userFromRepo.RoleKey));

                // if user exists then create token  
                //now we create 2 claims one with user id & other one is username to identify the user
                Claim[] claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userFromRepo.PublicKey.ToString()),
                    new Claim(ClaimTypes.Name, userFromRepo.FirstName),
                    new Claim(ClaimTypes.Email, userFromRepo.EmailId),
                    new Claim(ClaimTypes.Role, userFromRepo.RoleKey),
                    new Claim(ClaimTypes.PrimarySid, userFromRepo.Id.ToString()),
                    new Claim("userPublicKey", userFromRepo.PublicKey.ToString()),
                    new Claim("roleName", roleName)
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

                JwtSecurityTokenHandler tokenHandler = new();

                //creation of token using the token descriptor
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

                return Ok(new
                {
                    token = tokenHandler.WriteToken(token),
                    publicKey = userFromRepo.PublicKey.ToString(),
                    Image = userFromRepo.UserImage,
                    UserId = userFromRepo.Id
                });

            }
            catch (Exception ex)
            {
                FileHelper.WriteToFile("Exception", ex.ToString());
                throw;
            }
        }
    }
}
