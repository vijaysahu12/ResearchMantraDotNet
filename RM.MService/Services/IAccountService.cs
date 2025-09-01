using RM.BlobStorage;
using RM.CommonServices;
using RM.CommonServices.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel;
using RM.Model.ResponseModel;
using RM.NotificationService;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RM.MService.Services
{
    public interface IAccountService
    {
        Task<ApiCommonResponseModel> DeleteUser(string userKey);

        object Login(string userName, string password);

        object Logout();

        Task<ApiCommonResponseModel> GetUserDetails(string mobileUserKey);

        Task<ApiCommonResponseModel> OtpLogin(string mobileNumber, string countryCode);

        Task<ApiCommonResponseModel> OtpLoginVerificationAndGetSubscription(
            MobileOtpVerificationRequestModel paramRequest);

        Task<ApiCommonResponseModel> ManageUserDetails(ManageUserDetailsRequestModel queryValues);

        Task<ApiCommonResponseModel> GetSubscriptionTopics(string userKey, string? updateVersion);

        Task<bool> DeleteImage(string filename);

        Task<ApiCommonResponseModel> ManagePartnerAccount(PartnerAccountsMDetailRequestModel request);

        Task<ApiCommonResponseModel> GetDematAccount();

        Task<ApiCommonResponseModel> RemoveProfileImage(Guid publickey);

        Task<ApiCommonResponseModel> GetPartnerNames();

        Task<ApiCommonResponseModel> GetDematAccountDetails(Guid mobileUserKey);

        Task<ApiCommonResponseModel> Logout(Guid userKey, string fcmToken);

        Task<ApiCommonResponseModel> GetNewApiVersionMessage(string version);

        Task<ApiCommonResponseModel> GetFreeTrial(Guid userKey);

        Task<ApiCommonResponseModel?> ActivateFreeTrial(Guid mobileUserKey);

        Task<ApiCommonResponseModel?> GetMyActiveSubscription(Guid mobileUserKey);

        Task<ApiCommonResponseModel> UpdateFcmToken(UpdateFcmTokenRequestModel request);

        Task<ApiCommonResponseModel?> AccountDelete(AccountDeleteRequestModel request);

        Task<ApiCommonResponseModel> GetDeleteStatement();

        Task<ApiCommonResponseModel> RefreshToken(string refreshToken, Guid mobileUserKey, string deviceType, string version);
    }

    public class AccountService : IAccountService
    {
        private readonly KingResearchContext _context;
        private readonly ApiCommonResponseModel responseModel = new();
        private readonly MongoDbService _mongoService;
        private readonly IMobileNotificationService _mobileNotificationService;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly int _tokenExpiryInDays = 20;
        private readonly IMongoRepository<Log> _log;
        private readonly IMongoRepository<ExceptionLog> _exception;
        public AccountService(KingResearchContext context, IConfiguration config,
            MongoDbService mongoService, IMobileNotificationService mobileNotificationService,
            IAzureBlobStorageService azureBlobStorageService, IMongoRepository<Log> mongoRepo, IMongoRepository<ExceptionLog> exception, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _mongoService = mongoService;
            _mobileNotificationService = mobileNotificationService;
            _azureBlobStorageService = azureBlobStorageService;
            _log = mongoRepo;
            _emailService = emailService;
            _exception = exception;
        }

        public object Login(string userName, string password)
        {
            MobileUser? userFromRepo = _context.MobileUsers
                .Where(item => item.Mobile == userName && item.Password == password).FirstOrDefault();
            responseModel.Data = userFromRepo;

            if (userFromRepo is null)
            {
                responseModel.StatusCode = HttpStatusCode.Unauthorized;
                responseModel.Message = "Invalid Username and password";
            }
            else
            {
                Claim[] claims = new[]
                {
                    new Claim(ClaimTypes.Name, userFromRepo.FullName),
                    new Claim(ClaimTypes.Email, userFromRepo.EmailId),
                    new Claim(ClaimTypes.PrimarySid, userFromRepo.Id.ToString()),
                    new Claim("userPublicKey", userFromRepo.PublicKey.ToString()),
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
                SecurityToken token = (JwtSecurityToken)tokenHandler.CreateToken(tokenDescriptor);

                var result = new

                {
                    token = tokenHandler.WriteToken(token),
                    publicKey = userFromRepo.PublicKey.ToString(),
                    Image = userFromRepo.FullName
                };

                responseModel.Data = result;
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Success";
            }

            return responseModel;
        }

        public object Logout()
        {
            throw new NotImplementedException();
        }

        public async Task<ApiCommonResponseModel> GetUserDetails(string mobileUserKey)
        {
            if (Guid.TryParse(mobileUserKey, out Guid guidTry))
            {
                var result = await _context.MobileUsers
                .Where(c => c.PublicKey == guidTry && c.IsDelete == false)
                .Select(c => new
                {
                    c.FullName,
                    c.EmailId,
                    c.Mobile,
                    c.Gender,
                    c.City,
                    c.Dob,
                    c.PublicKey,
                    ProfileImage = c.ProfileImage != null ? _config["Azure:ImageUrlSuffix"] + c.ProfileImage : null
                })
                .FirstOrDefaultAsync();

                if (result == null)
                {
                    responseModel.Message = "User Not Found";
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    return responseModel;
                }
                responseModel.Message = "Data Fetched Successfully.";
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = result;
            }
            else
            {
                responseModel.Message = "Mobile Details Not Found..";
                responseModel.StatusCode = HttpStatusCode.BadRequest;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> OtpLogin(string mobileNumber, string countryCode)
        {
            string? createdBy = _config.GetSection("AppSettings:DefaultAdmin").Value;

            SqlParameter otp = new()
            {
                ParameterName = "Otp",
                SqlDbType = SqlDbType.VarChar,
                Size = 6,
                Direction = ParameterDirection.Output,
            };

            List<SqlParameter> sqlParameters =
            [
                new SqlParameter
                {
                    ParameterName = "MobileNumber",
                    Value = mobileNumber == "" ? DBNull.Value : mobileNumber,
                    SqlDbType = SqlDbType.VarChar
                },
                new SqlParameter
                {
                    ParameterName = "CreatedBy",
                    Value = Guid.Parse(createdBy),
                    SqlDbType = SqlDbType.UniqueIdentifier
                },
                new SqlParameter
                {
                    ParameterName = "ModifiedBy",
                    Value = Guid.Parse(createdBy),
                    SqlDbType = SqlDbType.UniqueIdentifier
                },
                new SqlParameter { ParameterName = "DeviceType", Value = "Android", SqlDbType = SqlDbType.VarChar },
                new SqlParameter { ParameterName = "CountryCode", Value = countryCode, SqlDbType = SqlDbType.VarChar },
                otp
            ];

            OtpLogin result =
                await _context.SqlQueryFirstOrDefaultAsync2<OtpLogin>(ProcedureCommonSqlParametersText.OtpLogin, sqlParameters.ToArray());
            result.ProfileImage = result.ProfileImage != null ? _config["Azure:ImageUrlSuffix"] + result.ProfileImage : null;

            if (result.Result.ToUpper() == "USERDELETED")
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = result.Message;
            }

            if (result.Result.ToUpper() == "OTPLIMITREACHED")
            {
                responseModel.StatusCode = HttpStatusCode.Forbidden;
                responseModel.Message = result.Message;
            }

            if (result.Result.ToUpper() == "OTPSENT")
            {
                try
                {
                    if (_config["AppSettings:AllowOTPSent"] == "true")
                    {
                        SendOTP(mobileNumber, otp.Value.ToString()!, countryCode);
                    }
                    await _mongoService.DeleteSelfAccountRequestedData(mobileNumber); //Delete from mongodb if he already requested for self account's delete
                }
                catch (Exception ex)
                {
                    await _exception.AddAsync(new ExceptionLog
                    {
                        CreatedOn = DateTime.Now,
                        InnerException = ex.InnerException?.Message,
                        Message = ex.Message,
                        RequestBody = $"MobileNumber: {mobileNumber}, CountryCode: {countryCode}",
                        Source = "OTPSENT",
                        StackTrace = ex.StackTrace
                    });
                }

                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = result.Message;
            }

            if (result.Result.ToUpper() == "REGISTERED")
            {
                SendOTP(mobileNumber, otp.Value.ToString()!, countryCode);
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = result.Message;
            }

            responseModel.Message = result.Message;
            responseModel.Data = result;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> OtpLoginVerificationAndGetSubscription(MobileOtpVerificationRequestModel paramRequest)
        {
            SqlParameter oldDeviceFcmToken = new()
            {
                ParameterName = "oldDeviceFcmToken",
                Direction = ParameterDirection.Output,
                SqlDbType = SqlDbType.VarChar,
                Size = 1000
            };

            #region otp verificatoin through sp [OtpLoginVerification]

            List<SqlParameter> sqlParameters =
            [
                new SqlParameter
                {
                    ParameterName = "MobileUserKey",
                    Value = paramRequest.MobileUserKey == "" ? DBNull.Value : Guid.Parse(paramRequest.MobileUserKey),
                    SqlDbType = SqlDbType.UniqueIdentifier
                },
                new SqlParameter
                {
                    ParameterName = "FirebaseFcmToken",
                    Value = string.IsNullOrEmpty(paramRequest.FirebaseFcmToken)
                        ? DBNull.Value
                        : paramRequest.FirebaseFcmToken,
                    SqlDbType = SqlDbType.VarChar
                },
                new SqlParameter
                {
                    ParameterName = "DeviceType",
                    Value = paramRequest.DeviceType == "" ? DBNull.Value : paramRequest.DeviceType,
                    SqlDbType = SqlDbType.VarChar
                },
                new SqlParameter
                { ParameterName = "Otp", Value = paramRequest.Otp, SqlDbType = SqlDbType.VarChar, Size = 6 },
                oldDeviceFcmToken
            ];

            OtpLoginVerificationResponse result =
                await _context.SqlQueryFirstOrDefaultAsync2<OtpLoginVerificationResponse>(ProcedureCommonSqlParametersText.OtpLoginVerificationAndGetSubscription, sqlParameters.ToArray());

            #endregion otp verificatoin through sp [OtpLoginVerification]

            // if new device is used to login then fcm token is going to change and if that changes we will send a notification to the old device which will logout from the device
            if (oldDeviceFcmToken.Value != null && !string.IsNullOrEmpty(oldDeviceFcmToken.Value.ToString()))
            {
                await _mobileNotificationService.SendNotificationToTokenToLogoutFromOldDevice(oldDeviceFcmToken.Value.ToString());
            }

            if (result.Result?.ToUpper() == "VERIFIEDUSER")
            {
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = result.Message;

                //Revoke all the old token
                //await _context.UserRefreshTokenM.Where(token => token.MobileUserKey == result.MobileUserKey && token.IsRevoked == false)
                //    .ExecuteUpdateAsync(update => update.SetProperty(token => token.IsRevoked, true));

                string token = GenerateJWTToken(result.MobileUserKey.ToString(), result.LeadKey, result.MobileUserId, paramRequest.DeviceType, paramRequest.Version, _tokenExpiryInDays);// JWT Token Generate
                var newRefreshToken = GenerateRefreshToken();

                if (string.IsNullOrEmpty(newRefreshToken))
                {
                    await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = "RefreshToken is null ", Source = "RefreshToken", Category = "JWT" });
                }
                var refreshTokenEntry = await _context.UserRefreshTokenM.Where(x => x.MobileUserKey == result.MobileUserKey).FirstOrDefaultAsync();

                if (refreshTokenEntry != null)
                {
                    refreshTokenEntry.RefreshToken = newRefreshToken;
                    refreshTokenEntry.IssuedAt = DateTime.Now;
                    refreshTokenEntry.ExpiresAt = DateTime.Now.AddDays(20);
                    refreshTokenEntry.IsRevoked = false;
                    _context.UserRefreshTokenM.Update(refreshTokenEntry);
                }
                else
                {
                    _ = _context.UserRefreshTokenM.Add(new UserRefreshTokenM
                    {
                        MobileUserKey = result.MobileUserKey,
                        RefreshToken = newRefreshToken,
                        IssuedAt = DateTime.Now,
                        ExpiresAt = DateTime.Now.AddDays(20),
                        DeviceType = paramRequest.DeviceType,
                        IsRevoked = false,
                    });

                }

                _ = await _context.SaveChangesAsync();
                if (string.IsNullOrEmpty(newRefreshToken))
                {
                    await _log.AddAsync(new Log { CreatedOn = DateTime.Now, Message = $"NewRefreshToken Not Generated for {result.MobileNumber}", Source = "JWT", Category = "JWT" });
                }
                responseModel.Data = new
                {
                    publicKey = result.MobileUserKey,
                    mobileUserId = result.MobileUserId,
                    name = result.FullName,
                    Token = token,
                    RefreshToken = newRefreshToken,
                    AccessToken = token,
                    profileImage = result.ProfileImage,
                    SubscriptionTopics = result.EventSubscription,
                    result.IsExistingUser,
                    result.Gender,
                    result.IsFreeTrialActivated,
                    ExpiresAt = DateTime.Now.AddDays(20)
                };
            }
            else if (result.Result?.ToUpper() == "OTPFAILED")
            {
                responseModel.StatusCode = HttpStatusCode.Unauthorized;
                responseModel.Message = result.Message;
                responseModel.Data = null;
            }
            else if (result.Result?.ToUpper() == "USERDELETED")
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = result.Message;
                responseModel.Data = null;
            }
            else if (result.Result.ToUpper() == "RETRYATTEMPTLIMITREACHED")
            {
                responseModel.StatusCode = HttpStatusCode.Forbidden;
                responseModel.Message = result.Message;
                responseModel.Data = null;
            }
            else if (result.Result is null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = "User Not Found.";
                responseModel.Data = null;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManageUserDetails(ManageUserDetailsRequestModel obj)
        {
            MobileUser? existingUserWithEmail;
            MobileUser? mobileUserDetails =
                await _context.MobileUsers.FirstOrDefaultAsync(item => item.PublicKey == Guid.Parse(obj.PublicKey) && item.IsDelete == false);
            if (mobileUserDetails != null)
            {
                existingUserWithEmail = await _context.MobileUsers.FirstOrDefaultAsync(item =>
                    item.EmailId == obj.EmailId && item.PublicKey != mobileUserDetails.PublicKey && item.IsDelete == false);
            }
            else
            {
                responseModel.Message = "User Not Found.";
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }

            if (existingUserWithEmail != null)
            {
                responseModel.Message = "EmailId already exists.";
                responseModel.StatusCode = HttpStatusCode.BadRequest;
                return responseModel;
            }

            int maxAllowedSizeInBytes = 31457280;
            bool profileImageExists = false;

            if (obj.ProfileImage != null)
            {
                profileImageExists = true;
                if (obj.ProfileImage.Length > maxAllowedSizeInBytes)
                {
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    responseModel.Message = $"File Size cannot be more than {maxAllowedSizeInBytes / 1048576} MB";
                    return responseModel;
                }
            }

            if (mobileUserDetails is { IsOtpVerified: true, IsActive: true })
            {
                if (string.IsNullOrEmpty(mobileUserDetails.FullName?.Trim()))
                {
                    //try
                    //{
                    await SendWhatsappWelcomeAsync(
                        mobileUserDetails.Mobile,
                        obj.FullName,
                        mobileUserDetails.CountryCode);
                    await _emailService.SendWelcomeEmailAsync(obj.EmailId, obj.FullName);
                    //}
                    //catch (Exception ex)
                    //{
                    //    await _exception.AddAsync(new ExceptionLog
                    //    {
                    //        Source = "ManageUserDetails",
                    //        RequestBody = JsonSerializer.Serialize(obj),
                    //        Message = ex.Message,
                    //        InnerException = ex.InnerException?.Message,
                    //        StackTrace = ex.StackTrace,
                    //        CreatedOn = DateTime.Now
                    //    });
                    //}
                }
                if (mobileUserDetails.ProfileImage is not null && profileImageExists)
                    _ = await _azureBlobStorageService.DeleteImage(mobileUserDetails.ProfileImage);

                mobileUserDetails.FullName = obj.FullName;
                mobileUserDetails.City = obj.City;
                mobileUserDetails.Gender = obj.Gender;
                mobileUserDetails.Dob = DateTime.Parse(obj.Dob);
                mobileUserDetails.EmailId = obj.EmailId;
                mobileUserDetails.ProfileImage = profileImageExists
                    ? await _azureBlobStorageService.UploadImage(obj.ProfileImage!)
                    : mobileUserDetails.ProfileImage;
                Lead? leadDetails =
                                      await _context.Leads.FirstOrDefaultAsync(item => item.MobileNumber == obj.Mobile);

                if (leadDetails != null)
                {
                    leadDetails.FullName = mobileUserDetails.FullName;
                    leadDetails.EmailId = mobileUserDetails.EmailId;
                    leadDetails.Gender = mobileUserDetails.Gender == "male" ? "M" : "F";

                    if (mobileUserDetails.LeadKey == Guid.Empty)
                    {
                        mobileUserDetails.LeadKey = leadDetails?.PublicKey ?? Guid.Empty;
                    }
                }
                await _context.SaveChangesAsync();

                // update profile image in mongo
                _ = await _mongoService.UpdateProfileImage(mobileUserDetails.PublicKey, mobileUserDetails.ProfileImage);

                responseModel.Message = "Update Successfull.";
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            else if (mobileUserDetails is not null &&
                     (mobileUserDetails.IsActive == false && mobileUserDetails.IsDelete == true))
            {
                responseModel.Message = "User deleted.";
                responseModel.StatusCode = HttpStatusCode.NotFound;
            }
            else if (mobileUserDetails is not null && mobileUserDetails.IsOtpVerified == false &&
                     mobileUserDetails.IsActive == true)
            {
                responseModel.Message = "Mobile Number is not Verified.";
                responseModel.StatusCode = HttpStatusCode.Forbidden;
            }

            return responseModel;
        }
        private async Task SendWhatsappWelcomeAsync(string mobileNumber, string userName, string countrycode)
        {
            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            var titleCaseUserName = textInfo.ToTitleCase(userName.ToLower());

            var options = new RestClientOptions(_config["GupShup:Url"]!);
            var restClient = new RestClient(options);
            var request = new RestRequest("", Method.Post);

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("accept", "application/json");
            request.AddHeader("apikey", _config["GupShup:ApiKey"]!);
            request.AddParameter("source", _config["GupShup:GupShupMobile"]);
            request.AddParameter("src.name", _config["GupShup:Appname"]);
            request.AddParameter("channel", "whatsapp");

            var messageJson = $"{{\"image\":{{\"link\":\"{_config["GupShup:WelcomeMessageMediaLink"]}\"}},\"type\":\"image\"}}";
            request.AddParameter("message", messageJson);

            // Use the title-cased userName in the template
            var templateJson = $"{{\"id\":\"{_config["GupShup:WelcomeMessageTemplateId"]}\",\"params\":[\"{titleCaseUserName}\"]}}";
            request.AddParameter("template", templateJson);
            request.AddParameter("destination", $"{countrycode}{mobileNumber}");

            try
            {
                var response = await restClient.PostAsync(request);

                // Log the response status and content
                if (!response.IsSuccessful)
                {
                    await _log.AddAsync(new Log
                    {
                        Source = "SendWhatsappWelcomeAsync_Failed",
                        Message = $"Failed to send welcome message to {mobileNumber}. Response: {response.Content}",
                        Category = "Whatsapp",
                        CreatedOn = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                await _exception.AddAsync(new ExceptionLog
                {
                    Source = "OTP",
                    RequestBody = $"MobileNumber: {mobileNumber}, OTP: {userName}, CountryCode: {countrycode}",
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace,
                    CreatedOn = DateTime.Now
                });
            }
        }

        private string GenerateJWTToken(string publicKey, string? leadKey, long mobileUserId, string deviceType, string version, int tokenExpiryInDays)
        {
            Claim[] claims =
 {
              new Claim(ClaimTypes.Role, publicKey),
              new Claim("userPublicKey", publicKey),
              new Claim("userLeadKey", leadKey ?? ""),
              new Claim("deviceType", deviceType ?? ""),
              new Claim("version", version ?? ""),
              new Claim(ClaimTypes.PrimarySid, mobileUserId.ToString()),
          };

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value!));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512Signature);
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = creds,
                Expires = DateTime.Now.AddDays(_tokenExpiryInDays)
            };
            JwtSecurityTokenHandler tokenHandler = new();
            JwtSecurityToken token = (JwtSecurityToken)tokenHandler.CreateToken(tokenDescriptor);
            token.Header["kid"] = "my-signing-key-1234";
            return tokenHandler.WriteToken(token);


        }

        public async Task<ApiCommonResponseModel> DeleteUser(string userKey)
        {
            MobileUser? result =
                await _context.MobileUsers.FirstOrDefaultAsync(item => item.PublicKey == Guid.Parse(userKey));

            if (result == null)
            {
                responseModel.Message = "User Not Found.";
                responseModel.StatusCode = HttpStatusCode.NotFound;
                return responseModel;
            }

            _ = result.IsDelete = true;
            _ = result.IsActive = false;

            await _context.SaveChangesAsync();

            responseModel.Message = "User Deleted Successfully";
            responseModel.StatusCode = HttpStatusCode.NoContent;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetSubscriptionTopics(string userKey, string? updateVersion)
        {
            // Check if userKey is a valid Guid and updateVersion is not null
            if (!string.IsNullOrEmpty(updateVersion) && Guid.TryParse(userKey, out Guid publicKey))
            {
                var item = await _context.MobileUsers
                    .Where(m => m.PublicKey == publicKey)
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = "User not Found.";
                    return responseModel;
                }

                // Update device version
                item.DeviceVersion = updateVersion;
                item.ModifiedOn = DateTime.Now;

                await _context.SaveChangesAsync();
            }

            if (Guid.TryParse(userKey, out Guid userKeyNew))
            {
                List<SqlParameter> sqlParameters =
                [
                    new SqlParameter
                    {
                        ParameterName = "mobileUserKey",
                        Value = userKeyNew,
                        SqlDbType = SqlDbType.UniqueIdentifier
                    }
                ];

                List<GetUserTopicsResponseModel> FreeTrialResponse =
                    await _context.SqlQueryToListAsync<GetUserTopicsResponseModel>(ProcedureCommonSqlParametersText.GetUserTopics, sqlParameters.ToArray());

                responseModel.Data = FreeTrialResponse.FirstOrDefault();
                responseModel.Message = "Data Fetched Successfully";
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                responseModel.Data = null;
                responseModel.Message = "Unauthorized Users.";
                responseModel.StatusCode = HttpStatusCode.Unauthorized;
            }
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManagePartnerAccount(PartnerAccountsMDetailRequestModel request)
        {
            PartnerAccountsM? existingRecord =
                await _context.PartnerAccountsM.FirstOrDefaultAsync(p => p.CreatedBy == request.CreatedBy);

            if (existingRecord != null)
            {
                // If the existing record has the same CreatedBy, update it; otherwise, do not allow the operation
                if (existingRecord.CreatedBy == request.CreatedBy)
                {
                    // Update the existing record
                    existingRecord.PartnerName = request.PartnerName;
                    existingRecord.API = request.API;
                    existingRecord.BrokerId = null;
                    existingRecord.GenerateToken = null;
                    existingRecord.MobileUserId = null;
                    existingRecord.PartnerId = request.PartnerId;
                    existingRecord.PartnerIdSecond = null;
                    existingRecord.SecretKey = request.SecretKey;
                    existingRecord.ModifiedBy = request.CreatedBy;
                    existingRecord.ModifiedOn = DateTime.Now;

                    _context.PartnerAccountsM.Update(existingRecord);
                }
                else
                {
                    // Existing record found with different CreatedBy, do not allow the operation
                    responseModel.StatusCode = HttpStatusCode.BadRequest;
                    responseModel.Message = "MobileUserId exists.";
                    return responseModel;
                }
            }
            else
            {
                PartnerAccountsM partnerAccountsM = new()
                {
                    PartnerName = request.PartnerName,
                    API = request.API,
                    BrokerId = null,
                    GenerateToken = null,
                    MobileUserId = null,
                    PartnerId = request.PartnerId,
                    PartnerIdSecond = null,
                    SecretKey = request.SecretKey,
                    IsActive = true,
                    IsDelete = false,
                    CreatedBy = request.CreatedBy,
                    CreatedOn = DateTime.Now
                };

                _context.PartnerAccountsM.Add(partnerAccountsM);
            }

            await _context.SaveChangesAsync();

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Saved Successfully.";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetDematAccount()
        {
            responseModel.Data = await _context.PartnerAccountsM.ToListAsync();
            responseModel.Message = "Data Fetched Successfully.";
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }
        public async void SendOTP(string mobileNumber, string otp, string countryCode)
        {
            var options = new RestClientOptions("https://api.gupshup.io/wa/api/v1/template/msg");
            var client = new RestClient(options);

            var request = new RestRequest("", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("apikey", _config["GupShup:ApiKey"]!);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            var templateJson = $"{{\"id\":\"{_config["GupShup:OtpTemplateId"]}\",\"params\":[\"{otp}\"]}}";
            request.AddParameter("template", templateJson);
            request.AddParameter("source", _config["GupShup:GupShupMobile"]);
            request.AddParameter("src.name", _config["GupShup:Appname"]);
            request.AddParameter("destination", $"{countryCode}{mobileNumber.Trim()}");
            request.AddParameter("channel", "whatsapp");
            try
            {
                var response = await client.PostAsync(request);

                if (response == null)
                {
                    await _log.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = $"Failed to send OTP to {mobileNumber}. Response is null.",
                        Source = "SendOTP_Failed_Response_Null",
                        Category = "Whatsapp"
                    });
                }
                else if (response.IsSuccessful)
                {

                    //await _log.AddAsync(new Log
                    //{
                    //    CreatedOn = DateTime.Now,
                    //    Message = $"OTP sent successfully to {mobileNumber}. Response : {JsonSerializer.Serialize(response)}",
                    //    Source = "SendOTP_Success",
                    //    Category = "Whatsapp"
                    //});

                }
                else
                {

                    await _log.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = $"Failed to send OTP to {mobileNumber}. Response : {JsonSerializer.Serialize(response)}",
                        Source = "SendOTP_Failed",
                        Category = "Whatsapp"
                    });


                }
            }
            catch (Exception ex)
            {
                await _exception.AddAsync(new ExceptionLog
                {
                    Source = "OTP",
                    RequestBody = $"MobileNumber: {mobileNumber}, OTP: {otp}, CountryCode: {countryCode}",
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace,
                    CreatedOn = DateTime.Now
                });

            }
        }
        public async Task<ApiCommonResponseModel> RemoveProfileImage(Guid publickey)
        {
            MobileUser? mobileUser = await _context.MobileUsers
                .Where(item => item.PublicKey == publickey && item.IsActive == true && item.IsDelete == false)
                .FirstOrDefaultAsync();
            if (mobileUser is null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = "User Not Found";
                return responseModel;
            }

            if (mobileUser.ProfileImage is not null)
            {
                _ = await _azureBlobStorageService.DeleteImage(mobileUser.ProfileImage);
            }

            mobileUser.ProfileImage = null;
            mobileUser.ModifiedOn = DateTime.Now;
            await _context.SaveChangesAsync();

            // Update data in mongo db
            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter
                {
                    ParameterName = "MobileUserKey", Value = publickey,
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier
                },
            };

            GetMobileUserDetailsSpResponseModel result =
                await _context.SqlQueryFirstOrDefaultAsync2<GetMobileUserDetailsSpResponseModel>(
                    ProcedureCommonSqlParametersText.GetMobileUserDetails, sqlParameters.ToArray());

            RM.Model.MongoDbCollection.User mongoUserDocument = new()
            {
                FullName = result.Fullname,
                PublicKey = result.PublicKey,
                ProfileImage = result.ProfileImage,
                Gender = result.Gender,
                CanCommunityPost = result.HasActiveProduct
            };

            _ = await _mongoService.AddUser(mongoUserDocument);

            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Successfull.";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetPartnerNames()
        {
            List<string> partnerNamesList = await _context.PartnerNamesM
                .Where(item => item.IsActive == true && item.IsDelete == false).Select(item => item.Name).ToListAsync();
            responseModel.Data = partnerNamesList;
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Data Fetched Successfully.";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetDematAccountDetails(Guid mobileUserKey)
        {
            var dematAccountDetails = await _context.PartnerAccountsM.Where(item => item.CreatedBy == mobileUserKey)
                .Select(item => new
                { item.Id, PartnerId = item.PartnerId.ToString(), item.API, item.SecretKey, item.PartnerName })
                .FirstOrDefaultAsync();
            responseModel.Data = dematAccountDetails;
            responseModel.Message = "Successfull.";
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> Logout(Guid userKey, string fcmToken)
        {
            MobileUser? userDetails = await _context.MobileUsers.Where(c => c.PublicKey == userKey).FirstOrDefaultAsync();
            if (userDetails is not null) // && userDetails.FirebaseFcmToken?.Trim() == fcmToken.Trim())
            {
                userDetails.FirebaseFcmToken = null;
                userDetails.ModifiedOn = DateTime.Now;

                _ = await _context.SaveChangesAsync();
                responseModel.Message = "Successfull";
                responseModel.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                responseModel.Message = "User Not Found";
                responseModel.StatusCode = HttpStatusCode.NotFound;
            }

            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetNewApiVersionMessage(string version)
        {
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Success";
            Setting? res = await _context.Settings
                .Where(item => item.Code == (string.IsNullOrEmpty(version) ? "newVersion" : version) && item.IsActive)
                .FirstOrDefaultAsync();
            responseModel.Data = res;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetFreeTrial(Guid userKey)
        {
            List<SqlParameter> sqlParameters =
            [
                new SqlParameter
                { ParameterName = "MobileUserKey", Value = userKey, SqlDbType = SqlDbType.UniqueIdentifier },
            ];
            List<GetFreeTrialResponseModel> resposneTrial =
                await _context.SqlQueryToListAsync<GetFreeTrialResponseModel>(
                    ProcedureCommonSqlParametersText.GetFreeTrialM, sqlParameters.ToArray());

            if (resposneTrial is not null)
            {
                string? distinctTrial = resposneTrial.Distinct().Select(item => item.Name).FirstOrDefault();
                int DaysInNumber = resposneTrial.Distinct().Select(item => item.DaysInNumber).FirstOrDefault();

                GetFreeTrialRootResponseModel finalResponse = new()
                {
                    TrialName = distinctTrial,
                    TrialInDays = DaysInNumber,
                    Data = []
                };

                foreach (GetFreeTrialResponseModel item in resposneTrial)
                {
                    finalResponse.Data.Add(item.ProductName);
                }

                responseModel.Data = finalResponse;
            }

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ActivateFreeTrial(Guid mobileUserKey)
        {
            List<SqlParameter> sqlParameters =
            [
                new SqlParameter
                { ParameterName = "MobileUserKey", Value = mobileUserKey, SqlDbType = SqlDbType.UniqueIdentifier },
            ];
            ActivateFreeTrialResponseModel responseTrial =
                await _context.SqlQueryFirstOrDefaultAsync2<ActivateFreeTrialResponseModel>(
                    ProcedureCommonSqlParametersText.ActivateFreeTrialM, sqlParameters.ToArray());

            responseModel.Data = responseTrial;
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel?> GetMyActiveSubscription(Guid mobileUserKey)
        {
            responseModel.Data = await (from my in _context.MyBucketM
                                        join pr in _context.ProductsM on my.ProductId equals pr.Id
                                        where my.MobileUserKey == mobileUserKey
                                              && my.IsActive
                                              && !my.IsExpired
                                              && my.EndDate.Value.Date >= DateTime.Now.Date
                                        select new
                                        {
                                            myBucketId = my.Id,
                                            productId = pr.Id,
                                            productName = pr.Name,
                                            productCode = pr.Code
                                        }).ToListAsync();
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "success";
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> UpdateFcmToken(UpdateFcmTokenRequestModel request)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var mobileUser = await _context.MobileUsers
                    .Where(x => x.PublicKey == request.MobileUserKey)
                    .FirstOrDefaultAsync();

                if (mobileUser == null)
                {
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    responseModel.Message = "User Not Found.";
                    return responseModel;
                }

                // Update existing users with the same FcmToken
                await _context.MobileUsers
                    .Where(item => item.FirebaseFcmToken == request.FcmToken && item.PublicKey != request.MobileUserKey)
                    .ExecuteUpdateAsync(s => s.SetProperty(b => b.FirebaseFcmToken, (string)null));

                // Update the current user's FcmToken
                mobileUser.FirebaseFcmToken = request.FcmToken;
                mobileUser.ModifiedOn = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Update Successful.";
            }
            return responseModel;
        }

        public async Task<ApiCommonResponseModel?> AccountDelete(AccountDeleteRequestModel request)
        {
            var mobileUser = await _context.MobileUsers.Where(x => x.PublicKey == request.MobileUserKey)
                .FirstOrDefaultAsync();
            if (mobileUser == null)
            {
                responseModel.StatusCode = HttpStatusCode.NotFound;
                responseModel.Message = "User Not Found.";
                return responseModel;
            }
            else
            {
                mobileUser.SelfDeleteRequest = true;
                mobileUser.SelfDeleteRequestDate = DateTime.Now.AddDays(30);
                mobileUser.SelfDeleteReason = request.Reason;
                _ = await _context.SaveChangesAsync();
                return await _mongoService.AddSelfDeleteAccountRequestData(request, mobileUser);
            }
        }

        public async Task<ApiCommonResponseModel> GetDeleteStatement()
        {
            responseModel.Data = await _context.DeleteStatementM.Where(x => x.IsDelete == false)
                .Select(x => x.Statement).ToListAsync();
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        async Task<bool> IAccountService.DeleteImage(string filename)
        {
            _ = await _azureBlobStorageService.DeleteImage(filename);
            return true;
        }

        public async Task<ApiCommonResponseModel> RefreshToken(string currentRefreshToken, Guid mobileUserKey, string deviceType, string version)
        {
            if (string.IsNullOrEmpty(currentRefreshToken) || Guid.Empty == mobileUserKey)
            {
                await _log.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"CurrentRefreshToken is null for mobile {mobileUserKey}, token:{currentRefreshToken} and deviceType:{deviceType}, version:{version}",
                    Source = "RefreshToken",
                    Category = "JWT"
                });

                return new ApiCommonResponseModel
                {
                    Message = "Invalid refresh token or user identifier.",
                    StatusCode = HttpStatusCode.Unauthorized
                };
            }

            var dateTimeNow = DateTime.Now;
            var refreshTokenEntry = await _context.UserRefreshTokenM
                .FirstOrDefaultAsync(x => x.MobileUserKey == mobileUserKey && x.RefreshToken == currentRefreshToken);

            if (refreshTokenEntry == null)
            {
                await _log.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"refreshTokenEntry not found for mobile {mobileUserKey}, token:{currentRefreshToken} and deviceType:{deviceType}, version:{version}",
                    Source = "RefreshToken",
                    Category = "JWT"
                });

                return new ApiCommonResponseModel
                {
                    Message = "Invalid refresh token.",
                    StatusCode = HttpStatusCode.Unauthorized
                };
            }

            // Fetch user details
            var user = await _context.MobileUsers
                .FirstOrDefaultAsync(x => x.PublicKey == mobileUserKey && (x.IsDelete == null || x.IsDelete == false));

            if (user == null)
            {
                return new ApiCommonResponseModel
                {
                    Message = "User Not Found.",
                    StatusCode = HttpStatusCode.Unauthorized
                };
            }

            user.DeviceType = deviceType;
            user.DeviceVersion = version;

            var newAccessToken = GenerateJWTToken(user.PublicKey.ToString(), user.LeadKey?.ToString(), user.Id, deviceType, version, _tokenExpiryInDays);

            string finalRefreshToken = currentRefreshToken;

            if (refreshTokenEntry == null)
            {
                finalRefreshToken = GenerateRefreshToken();

                _context.UserRefreshTokenM.Add(new UserRefreshTokenM
                {
                    MobileUserKey = mobileUserKey,
                    RefreshToken = finalRefreshToken,
                    IssuedAt = dateTimeNow,
                    ExpiresAt = dateTimeNow.AddDays(20),
                    DeviceType = deviceType,
                    IsRevoked = false,
                    ModifiedOn = dateTimeNow
                });

                await _log.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"New Token For: {mobileUserKey} is {finalRefreshToken}",
                    Source = "RefreshToken",
                    Category = "RefreshToken"
                });
            }
            else
            {
                refreshTokenEntry.DeviceType = deviceType;
                refreshTokenEntry.ModifiedOn = dateTimeNow;
                refreshTokenEntry.ExpiresAt = dateTimeNow.AddDays(_tokenExpiryInDays);
                refreshTokenEntry.IsRevoked = false;
            }

            await _context.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                Data = new
                {
                    accessToken = newAccessToken,
                    refreshToken = finalRefreshToken,
                    ExpiresAt = dateTimeNow.AddDays(20)
                },
                StatusCode = HttpStatusCode.OK,
                Message = "Access token generated successfully."
            };
        }


        public static string GenerateRefreshToken(int length = 64) // Increased default length
        {
            const string chars = "ABCDEFGHIJKLMVWX012345YZabcdeNOPQRSTUfghijklmnopqrstuvwxyz6789";
            var tokenBuilder = new StringBuilder(length);
            using (var rng = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                for (int i = 0; i < length; i++)
                {
                    tokenBuilder.Append(chars[randomBytes[i] % chars.Length]);
                }
            }
            var base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenBuilder.ToString()));
            //Optional sanitization. Remove if not needed.
            var sanitizedToken = new StringBuilder(base64Token.Length);
            foreach (char c in base64Token)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sanitizedToken.Append(c);
                }
            }
            return sanitizedToken.ToString();
        }

    }
}