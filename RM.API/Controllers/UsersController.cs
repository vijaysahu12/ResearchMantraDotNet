using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Interfaces;
using RM.API.Models;
using RM.CommonServices.Helpers;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using RM.Model.RequestModel;
using RM.ChatGPT;

namespace RM.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApiCommonResponseModel responseModel = new();
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly ResearchMantraContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly StockInsightService _insightService;


        public UsersController(IAuthRepository repo, IConfiguration config, ResearchMantraContext context, IWebHostEnvironment environment, StockInsightService insightService)
        {
            _repo = repo;
            _config = config;
            _context = context;
            _environment = environment;
            _insightService = insightService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ApiCommonResponseModel> GetUsers()
        {

            //var users = await (from u in _context.Users
            //                   from r in _context.Roles
            //                   where ((u.RoleKey == r.PublicKey.ToString()) && ((u.IsDelete == 0) || (u.IsDelete == null) && ( u.IsDisabled == null) || (u.IsDisabled == 0)))
            //                   orderby u.FirstName ascending
            //                   select new { Id = u.Id, RoleKey = r.Name, FirstName = u.FirstName, LastName = u.LastName, MobileNumber = u.MobileNumber, EmailId = u.EmailId, PublicKey = u.PublicKey, IsDisabled = u.IsDisabled }).ToListAsync();

            //return Ok(users.OrderBy(user => user.FirstName).ToList());

            List<SqlParameter> sqlParameters = new()
            {
               new SqlParameter { ParameterName = "SearchText", Value =  DBNull.Value , SqlDbType = System.Data.SqlDbType.VarChar,Size = 100},
            };
            responseModel.Message = "Users Fetched";
            responseModel.Data = await _context.SqlQueryAsync<JsonResponseModel>(ProcedureCommonSqlParametersText.GetUsers, sqlParameters.ToArray());
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;

        }


        [HttpPost("GetFilterUsers")]
        public async Task<ApiCommonResponseModel> GetFilterUsers(QueryValues queryValues)
        {

            List<SqlParameter> sqlParameters = new()
            {
               new SqlParameter { ParameterName = "SearchText", Value = queryValues.PrimaryKey == null ? DBNull.Value : queryValues.PrimaryKey, SqlDbType = System.Data.SqlDbType.VarChar,Size = 100},
            };
            responseModel.Message = "Users Fetched";
            responseModel.Data = await _context.SqlQueryAsync<JsonResponseModel>(ProcedureCommonSqlParametersText.GetUsers, sqlParameters.ToArray());
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }



        [HttpGet]
        [Route("getAnalysts")]
        public async Task<ActionResult> GetAnalystUsers()
        {


            List<SqlParameter> sqlParameters = new()
            {
               new SqlParameter { ParameterName = "Category", Value = "analyst", SqlDbType = System.Data.SqlDbType.VarChar,Size = 50},
            };
            responseModel.Message = "Analysts Fetched";
            responseModel.Data = await _context.SqlQueryAsync<GetProcedureJsonResponse>(ProcedureCommonSqlParametersText.GetAnalysts, sqlParameters.ToArray());
            responseModel.StatusCode = HttpStatusCode.OK;


            //var users = await (from u in _context.Users
            //                   from r in _context.UserMappings
            //                   where ((u.PublicKey == r.UserKey) && (u.IsDelete == 0 || u.IsDelete == null) && r.UserType == "analyst")
            //                   orderby u.FirstName ascending
            //                   select new { Id = u.Id, FirstName = u.FirstName, LastName = u.LastName, PublicKey = u.PublicKey }).ToListAsync();

            return Ok(responseModel);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            if (Guid.TryParse(id, out Guid userKey))
            {
                User user = await _context.Users.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

                //assigned file by converting to byt array
                if (user != null && user.UserImage != null && user.UserImage != "")
                {
                    if (user.UserImage != null && System.IO.File.Exists(user.UserImage))
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(user.UserImage);
                        user.UserImage = Convert.ToBase64String(bytes);
                    }
                    else
                    {
                        user.UserImage = null;
                    }
                }
                return user == null ? (ActionResult<User>)NotFound() : (ActionResult<User>)user;
            }
            else
            {
                return BadRequest();
            }
        }

        // PUT: api/Users/5

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, User user)
        {
            var Id = id.ToLower();
            if (Id != user.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<User> entry = _context.Entry(user);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            if (string.IsNullOrEmpty(user.Password) || (user.Password.Length < 3))
            {
                entry.Property(e => e.PasswordHash).IsModified = false;
                entry.Property(e => e.PasswordSalt).IsModified = false;
            }
            else
            {
                user = await _repo.UpdatePasswordHash(user);
            }

            user.Password = null;
            TokenVariables tokenVariables = TokenAnalyserStatic.FetchTokenPart2(HttpContext);
            user.ModifiedOn = DateTime.Now;
            user.ModifiedBy = tokenVariables.PublicKey.ToString();
            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            var roleNameRaw = await _context.Roles
        .Where(r => r.PublicKey == Guid.Parse(user.RoleKey))
        .Select(r => r.Name)
        .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(roleNameRaw))
            {
                return BadRequest("Invalid role");
            }

            var roleNames = roleNameRaw
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim());

            var mappedRoles = new List<string>();

            foreach (var role in roleNames)
            {
                if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) )
                {
                    mappedRoles.Add("BDE");
                    mappedRoles.Add("analyst");
                }
                else if(role.Equals("GlobleAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("GlobleAdmin");
                    mappedRoles.Add("BDE");
                    mappedRoles.Add("analyst");
                }
                else if (role.Equals("Calls Provider", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("analyst");
                }
                else if (role.Equals("Sales Lead", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("BDE");
                    mappedRoles.Add("Sales Lead");
                }
                else
                {
                    mappedRoles.Add(role);
                }
            }

            var existingMappings = await _context.UserMappings
                .Where(um => um.UserKey == user.PublicKey)
                .ToListAsync();

            foreach (var existing in existingMappings)
            {
                if (mappedRoles.Contains(existing.UserType))
                {
                    // Reactivate if present
                    existing.IsActive = true;
                    existing.IsDelete = false;
                }
                else
                {
                    // Deactivate if not part of current roles
                    existing.IsActive = false;
                    existing.IsDelete = true;
                }

                _context.UserMappings.Update(existing);
            }

            foreach (var role in mappedRoles.Distinct())
            {
                if (!existingMappings.Any(em => em.UserType == role))
                {
                    _context.UserMappings.Add(new UserMappings
                    {
                        UserKey = user.PublicKey,
                        UserType = role,
                        IsActive = true,
                        IsDelete = false
                    });
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Users

        //[HttpPost("register")]
        //public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        //{
        //    //var createdUser = await _repo.Register(user, user.FirstName);
        //    //return Ok(createdUser);

        //    // validate request below 2 codes are required if APICONTROLLER is not being used
        //    //Register([FromBody]UserForRegisterDto userForRegisterDto)
        //    //if (!ModelState.IsValid) return BadRequest(ModelState);

        //    // convert the text to lowercase
        //    userForRegisterDto.EmailId = userForRegisterDto.EmailId.ToLower();
        //    //check if this users exists
        //    if (await _repo.UserExists(userForRegisterDto.EmailId))
        //        return BadRequest("Username already exists");

        //    //create new user with the username
        //    var userToCreate = new User
        //    {
        //        EmailId = userForRegisterDto.EmailId
        //    };
        //    //now pass the created user & password to Register method
        //    var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

        //    return Ok(createdUser);
        //}
        [HttpGet("GetActiveSalesLeads")]
        public async Task<IActionResult> GetActiveSalesLeads()
        {
            var salesLeads = await (
                from user in _context.Users
                join mapping in _context.UserMappings on user.PublicKey equals mapping.UserKey
                where mapping.UserType == "Sales Lead"
                      && (user.IsDisabled == null || user.IsDisabled == 0)
                      && mapping.IsActive && !mapping.IsDelete
                select new
                {
                    user.Id,
                    user.PublicKey,
                    FullName = user.FirstName + " " + user.LastName
                }
            ).Distinct().ToListAsync();

            // Now fetch all BDEs with their SupervisorId
            var allBDEs = await (
                from user in _context.Users
                join mapping in _context.UserMappings on user.PublicKey equals mapping.UserKey
                where mapping.UserType == "BDE"
                      && (user.IsDisabled == null || user.IsDisabled == 0)
                      && mapping.IsActive && !mapping.IsDelete
                select new
                {
                   
                    user.Id,
                    user.PublicKey,
                    FullName = user.FirstName + " " + user.LastName,
                    SupervisorId = user.SupervisorId
                }
            ).ToListAsync();

            // Merge BDEs under each Sales Lead
            var result = salesLeads.Select(sl => new
            {
                sl.Id,
                sl.PublicKey,
                sl.FullName,
                BDEs = allBDEs
                    .Where(b => b.SupervisorId == sl.Id)
                    .Select(b => new
                    {   b.SupervisorId,
                        b.Id,
                        b.PublicKey,
                        b.FullName
                    })
                    .ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpGet("GetUnassignedBDE")]
        public async Task<IActionResult> GetUnassignedBDE()
        {
            var bdes = await (
                from user in _context.Users
                where (user.IsDisabled == null || user.IsDisabled == 0)
                      && user.SupervisorId == null
                      && (
                          // Only include users who have exactly 1 active, undeleted mapping
                          (from m in _context.UserMappings
                           where m.UserKey == user.PublicKey && m.IsActive && !m.IsDelete
                           select m).Count() == 1
                      )
                join mapping in _context.UserMappings
                    on user.PublicKey equals mapping.UserKey
                where mapping.IsActive && !mapping.IsDelete
                select new
                {
                    user.Id,
                    user.PublicKey,
                    FullName = user.FirstName + " " + user.LastName,
                    UserType = mapping.UserType
                }
            ).ToListAsync();

            return Ok(bdes);
        }

        [HttpPost("AssignBDEsToLead")]
        public async Task<IActionResult> AssignBDEsToLead([FromBody] AssignBDERequest request)
        {
            var users = await _context.Users
                .Where(u => request.BdeUserIds.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                user.SupervisorId = request.SalesLeadId; // will be null if unassigned
                user.ModifiedOn = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "BDEs updated successfully." });
        }



        //&& u.SupervisorId == null

        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromForm] UserForRegisterDto userForRegisterDto)
        {
            // convert the text to lowercase
            userForRegisterDto.EmailId = userForRegisterDto.EmailId.ToLower();
            //check if this users exists
            if (await _repo.UserExists(userForRegisterDto.EmailId))
            {
                return BadRequest("Username already exists");
            }
            //check image
            string filepath = "";
            if (userForRegisterDto.file != null)
            {
                if (userForRegisterDto.file.ContentType.Contains("image"))
                {
                    string extension = Path.GetExtension(userForRegisterDto.file.FileName).ToLower();
                    if (extension is ".png" or ".jpg" or ".jpeg" or ".gif")
                    {
                        string fName = userForRegisterDto.file.FileName.Split('.')[0] + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        string filename = string.Concat(fName, extension);
                        string basePath = Path.Combine(_environment.ContentRootPath, "Image");
                        filepath = Path.Combine(_environment.ContentRootPath, "Image/" + fName + extension);
                        if (Directory.Exists(basePath) == false)
                        {
                            _ = Directory.CreateDirectory(basePath);
                        }
                        using FileStream inputStream = new(Path.Combine(basePath, filename), FileMode.Create);
                        try
                        {
                            await userForRegisterDto.file.CopyToAsync(inputStream);
                        }
                        catch (Exception)
                        {
                            //ex.Log();
                            //return ResponseInfo.Error("Failed to save image");
                        }
                    }
                    else
                    {
                        return BadRequest("Invalid file");
                    }
                }
                else
                {
                    return BadRequest("image not uploaded");
                }

            }
            //create new user with the username
            User userToCreate = new()
            {
                EmailId = userForRegisterDto.EmailId,
                FirstName = userForRegisterDto.FirstName,
                LastName = userForRegisterDto.LastName,
                MobileNumber = userForRegisterDto.MobileNumber,
                Doj = userForRegisterDto.DOJ,
                Address = userForRegisterDto.Address,
                RoleKey = userForRegisterDto.RoleKey,
                Gender = userForRegisterDto.Gender,
                UserImage = filepath,
            };

            //now pass the created user & password to Register method
            User createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            var roleNameRaw = await _context.Roles
     .Where(r => r.PublicKey == Guid.Parse(userForRegisterDto.RoleKey))
     .Select(r => r.Name)
     .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(roleNameRaw))
            {
                return BadRequest("Invalid role key");
            }


            var roleNames = roleNameRaw
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .ToList();

            List<string> mappedRoles = new();

            foreach (var role in roleNames)
            {
                if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("BDE");
                    mappedRoles.Add("analyst");
                }
                else if (role.Equals("GlobleAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("GlobleAdmin");
                    mappedRoles.Add("BDE");
                    mappedRoles.Add("analyst");
                }
                else if (role.Equals("Calls Provider", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("analyst");
                }
                else if (role.Equals("Sales Lead", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("BDE");
                    mappedRoles.Add("Sales Lead");
                }
                else
                {
                    mappedRoles.Add(role);
                }
            }


            foreach (var mappedRole in mappedRoles.Distinct()) // Optional: avoid duplicates
            {
                var userMapping = new UserMappings
                {
                    UserKey = createdUser.PublicKey,
                    UserType = mappedRole,
                    IsActive = true,
                    IsDelete = false
                };

                _context.UserMappings.Add(userMapping);
            }
            await _context.SaveChangesAsync();


            return Ok(createdUser);
            //_context.Users.Add(user);
            //await _context.SaveChangesAsync();
            //return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        [HttpPost]
        [Route("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(User user)
        {
            ApiCommonResponseModel vv = new();
            if (user != null && user.PublicKey != null && user.Password != null)
            {
                User res = _context.Users.Where(item => item.PublicKey == user.PublicKey).FirstOrDefault();
                User createdUser = await _repo.UpdatePasswordHash(user);
                res.PasswordHash = user.PasswordHash;
                res.PasswordSalt = user.PasswordSalt;
                _ = await _context.SaveChangesAsync();

                vv.Message = "Succesfully Updated Password...";
                vv.StatusCode = HttpStatusCode.OK;

            }
            else
            {
                vv.StatusCode = HttpStatusCode.BadRequest;
                vv.Message = "Invalid User or Password, Please Try Again";
            }
            return Ok(vv);
        }
        [HttpGet("ToggleUserStatus/{id}")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {


            // Find the user by id
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "User not found.",
                    Data = null
                });
            }

            // Toggle the IsDisabled field (0 = Active, 1 = Inactive)
            user.IsDisabled = user.IsDisabled.HasValue ? (user.IsDisabled.Value == 0 ? (byte)1 : (byte)0) : (byte)0;
            user.ModifiedOn = DateTime.UtcNow;
            // user.ModifiedBy = loggedUser;  // Track who made the change
            await _context.SaveChangesAsync();
            // Save changes to the database
            var roleNameRaw = await _context.Roles
        .Where(r => r.PublicKey == Guid.Parse(user.RoleKey))
        .Select(r => r.Name)
        .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(roleNameRaw))
            {
                return BadRequest(new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Role not found for the user.",
                    Data = null
                });
            }

            // 🧠 Expand mapped roles (Admin → BDE + analyst)
            var roleNames = roleNameRaw
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim());

            var mappedRoles = new List<string>();

            foreach (var role in roleNames)
            {
                if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("Admin");
                    mappedRoles.Add("BDE");
                    mappedRoles.Add("analyst");
                }
                else if (role.Equals("GlobleAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("GlobleAdmin");
                    mappedRoles.Add("BDE");
                    mappedRoles.Add("analyst");
                }
                else if (role.Equals("Calls Provider", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("Calls Provider");
                    mappedRoles.Add("analyst");
                }
                else if (role.Equals("Sales Lead", StringComparison.OrdinalIgnoreCase))
                {
                    mappedRoles.Add("BDE");
                    mappedRoles.Add("Sales Lead");
                }
                else
                {
                    mappedRoles.Add(role);
                }
            }


            // 📦 Get UserMappings and update status
            var userMappings = await _context.UserMappings
                .Where(um => um.UserKey == user.PublicKey)
                .ToListAsync();

            foreach (var mapping in userMappings)
            {
                bool isExactBDE = mappedRoles.Contains("BDE") && mapping.UserType == "BDE"; // exact match for BDE
                bool isOtherRole = mappedRoles.Any(r =>
                    !r.Equals("BDE", StringComparison.Ordinal) &&
                    r.Equals(mapping.UserType, StringComparison.OrdinalIgnoreCase)); // case-insensitive for others

                if (isExactBDE || isOtherRole)
                {
                    mapping.IsActive = user.IsDisabled == 0;
                }
                else
                {
                    mapping.IsActive = false;
                }
            }



            await _context.SaveChangesAsync();

            return Ok(new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "User status toggled successfully.",
                Data = new
                {
                    IsDisabled = user.IsDisabled,
                    User = user,
                    UserMappingsUpdated = userMappings.Select(m => new { m.UserType, m.IsActive, m.IsDelete })
                }
            });

        }

        [HttpPost("checkMobileNumber")]
        public IActionResult CheckMobileNumber([FromBody] CheckMobileNumberRequestModel requestModel)
        {
            var response = new ApiCommonResponseModel();

            // 1. Validate mobile number
            if (string.IsNullOrWhiteSpace(requestModel.MobileNumber))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Message = "Mobile number is required.";
                response.Data = null;
                response.Exceptions = null;
                return Ok(response);
            }

            // 2. Normalize the input mobile number
            string cleanedMobileNumber = requestModel.MobileNumber.Replace(" ", "").Trim();

            // 3. Determine if it's a new user
            bool isNewUser = requestModel.MobileUserKey == Guid.Empty;
            Guid userKey = requestModel.MobileUserKey;

            // 4. Check if the number exists (exclude current user when editing)
            bool exists = _context.Users.Any(u =>
                  u.MobileNumber.Replace(" ", "").Trim() == cleanedMobileNumber &&
                  (isNewUser || u.PublicKey != userKey) &&
                  (u.IsDelete == 0 || u.IsDelete == null)
              );



            if (exists)
            {
                response.StatusCode = HttpStatusCode.Conflict;
                response.Message = "This mobile number is already registered.";
                response.Data = new { Exists = true };
                response.Exceptions = null;
                return Ok(response);
            }

            // 5. If not exists, return success
            response.StatusCode = HttpStatusCode.OK;
            response.Message = "Mobile number is available.";
            response.Data = new { Exists = false };
            response.Exceptions = null;

            return Ok(response);
        }

        [HttpPost]
        [Route("UploadUserImage")]
        public async Task<ActionResult<User>> UploadUserImage([FromForm] UserImageDto img)
        {
            if (img.file != null)
            {
                if (img.file.ContentType.Contains("image"))
                {
                    string extension = Path.GetExtension(img.file.FileName).ToLower();
                    if (extension is ".png" or ".jpg" or ".jpeg" or ".gif")
                    {
                        string fName = img.file.FileName.Split('.')[0] + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        string filename = string.Concat(fName, extension);
                        string basePath = Path.Combine(_environment.ContentRootPath, "Image");
                        string filepath = Path.Combine(_environment.ContentRootPath, "Image/" + fName + extension);
                        if (Directory.Exists(basePath) == false)
                        {
                            _ = Directory.CreateDirectory(basePath);
                        }
                        User user = await _context.Users.FindAsync(img.Id);
                        if (System.IO.File.Exists(user.UserImage))
                        {
                            System.IO.File.Delete(user.UserImage);
                        }

                        using FileStream inputStream = new(Path.Combine(basePath, filename), FileMode.Create);
                        try
                        {
                            await img.file.CopyToAsync(inputStream);

                            user.UserImage = filepath;
                            _context.Entry(user).State = EntityState.Modified;
                            _ = await _context.SaveChangesAsync();
                            //}
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        return BadRequest("Invalid file");
                    }
                }
                else
                {
                    return BadRequest("image not uploaded");
                }

            }

            return Ok();
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.PublicKey.ToString() == id);

        }


        [HttpGet("SearchUser/{searchText}")]
        public async Task<ActionResult> searchUsers(string searchText)
        {
            ApiCommonResponseModel responseModel = new();
            if (searchText == "")
            {
                responseModel.Message = "Empty Search";
                responseModel.Data = _context.Users.ToListAsync();
                responseModel.StatusCode = HttpStatusCode.OK;

            }
            //var usersList = await _context.Users.Where( item => item.FirstName == searchText || item.LastName == searchText || item.MobileNumber == searchText || item.EmailId == searchText).ToListAsync();
            var usersList = await (from u in _context.Users
                                   from r in _context.Roles
                                   where ((u.RoleKey == r.PublicKey.ToString()) && (u.FirstName == searchText)) || (u.LastName == searchText) || (u.MobileNumber == searchText)
                                   orderby u.FirstName ascending
                                   select new { u.Id, RoleKey = r.Name, u.FirstName, u.LastName, u.MobileNumber, u.EmailId, u.PublicKey, u.IsDisabled }).ToListAsync();
            if (usersList.Count != 0)
            {
                responseModel.Message = "User Found";
                responseModel.Data = usersList;
                responseModel.StatusCode = HttpStatusCode.OK;
                return Ok(responseModel);
            }
            else
            {
                responseModel.Message = "No User Found";
                responseModel.Data = null;
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return Ok(responseModel);
            }
        }

        [HttpPost("GetUserActivity")]
        public async Task<ActionResult> GetUserActivity(QueryValues queryValues)

        {
            TokenAnalyser tokenAnalyser = new();

            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }

            string loginUser = tokenVariables.PublicKey;
            string loginUserRole = tokenVariables.RoleKey;
            queryValues.RoleKey = loginUserRole;
            var userTemp = await _context.Users.Where(item => item.PublicKey == Guid.Parse(loginUser)).FirstOrDefaultAsync();
            var roles = await _context.Roles.Where(item => item.PublicKey == Guid.Parse(userTemp.RoleKey)).FirstOrDefaultAsync();
            ApiCommonResponseModel responseModel = new();

            SqlParameter sqlOutputParameter = new SqlParameter()
            {
                ParameterName = "TotalCount",
                Value = DBNull.Value,
                Direction = ParameterDirection.Output,
                SqlDbType = SqlDbType.Int
            };

            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter { ParameterName = "PageSize", Value = queryValues.PageSize ,SqlDbType = SqlDbType.Int},
                new SqlParameter { ParameterName = "PageNumber", Value = queryValues.PageNumber  ,SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "PrimaryKey",    Value = queryValues.PrimaryKey =="" ? DBNull.Value :Guid.Parse( queryValues.PrimaryKey) ,SqlDbType = SqlDbType.UniqueIdentifier},
                new SqlParameter {ParameterName = "SecondaryKey",Value = string.IsNullOrEmpty(queryValues.SecondaryKey) ? DBNull.Value : queryValues.SecondaryKey,SqlDbType = SqlDbType.VarChar,Size = 50},
                new SqlParameter { ParameterName = "FromDate", Value = queryValues.FromDate == null ? DBNull.Value  : queryValues.FromDate ,SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "ToDate", Value = queryValues.ToDate == null ? DBNull.Value : queryValues.ToDate ,SqlDbType = SqlDbType.DateTime},
                new SqlParameter { ParameterName = "LoggedInUser",  Value = loginUser   ,SqlDbType = SqlDbType.VarChar, Size = 50},

                new SqlParameter { ParameterName = "SearchText", Value = queryValues.SearchText == null ? DBNull.Value : queryValues.SearchText ,SqlDbType = SqlDbType.VarChar, Size = 50},
                sqlOutputParameter
            };

            _context.Database.SetCommandTimeout(180); // Set the timeout to 3 minutes for the query

            responseModel.Data = new { Data = await _context.SqlQueryToListAsync<GetUserActivitySpResponseModel>(ProcedureCommonSqlParametersText.GetUserActivity, sqlParameters.ToArray()), TotalCount = sqlOutputParameter.Value };

            if (responseModel.Data is not null)
            {
                responseModel.Message = "Data Fetched Successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            else if (responseModel.Data is null)
            {
                responseModel.Message = "No Records available for current query";
                responseModel.StatusCode = HttpStatusCode.OK;
            }

            return Ok(responseModel);


        }

        [HttpPost("GetEmployeeWorkStatus")]
        public async Task<ActionResult> GetEmployeeWorkStatus(GetEmployeeWorkStatusRequestModel request)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            int userId = tokenVariables.Id;

            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter { ParameterName = "SuperVisorId", Value = userId, SqlDbType = SqlDbType.Int },
                new SqlParameter { ParameterName = "StartDate", Value = request.StartDate, SqlDbType = SqlDbType.DateTime },
                new SqlParameter { ParameterName = "EndDate", Value = request.EndDate, SqlDbType = SqlDbType.DateTime },
            };


            responseModel.Data = await _context.SqlQueryToListAsync<GetEmployeeWorkStatusSpResponse>(ProcedureCommonSqlParametersText.GetEmployeeWorkStatus, sqlParameters.ToArray());

            if (responseModel.Data is not null)
            {
                responseModel.Message = "Data Fetched Successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            else if (responseModel.Data is null)
            {
                responseModel.Message = "No Records available for current query";
                responseModel.StatusCode = HttpStatusCode.OK;
            }

            return Ok(responseModel);
        }

        [HttpDelete("delete/{publicKey}")]
        public async Task<IActionResult> Delete(Guid publicKey)
        {
            Guid loggedInUser = Guid.Parse(UserClaimsHelper.GetClaimValue(User, "userPublicKey"));
            if (publicKey == Guid.Empty)
            {
                return BadRequest("Invalid PublicKey.");
            }

            var item = await _context.Users.FirstOrDefaultAsync(s => s.PublicKey == publicKey);
            if (item == null)
            {
                return NotFound("User not found.");
            }

            item.IsDelete = 1;
            //item.IsDisabled = 1;
            item.ModifiedOn = DateTime.Now;
            item.ModifiedBy = loggedInUser.ToString();
            var userMappings = await _context.UserMappings
             .Where(um => um.UserKey == publicKey)
             .ToListAsync();

            foreach (var mapping in userMappings)
            {
                mapping.IsActive = false;
                mapping.IsDelete = true;
            }


            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content for successful deletion
        }


        [HttpGet("GetMarketInsights")]
        public async Task<IActionResult> GetMarketInsights()
        {
            var response = await _insightService.GetDailyStockInsightsAsync();
            return Ok(response);
        }
    }
}
