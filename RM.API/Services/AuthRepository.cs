using RM.API.Dtos;
using RM.API.Interfaces;
using RM.API.Models;
using RM.API.Models.Mobile;
using RM.Database.ResearchMantraContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ResearchMantraContext _gurujiDbContext;
        public AuthRepository(ResearchMantraContext gurujiDbContext)
        {
            _gurujiDbContext = gurujiDbContext;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await _gurujiDbContext.Users.FirstOrDefaultAsync(x => x.EmailId == username);
            if (user == null)
            {
                return null;
            }
            else if ((user.IsDelete == 1 || user.IsDelete is null) && user.IsDisabled == 1)
            {
                return null;
            }

            else
            {
                try
                {
                    //byte[] passwordHash, passwordSalt;
                    //this.CreatePasswordHash(password, out passwordHash, out passwordSalt);

                    ////user.Password = password;
                    ////var res = await this.UpdatePasswordHash(user);
                    //user.PasswordHash = passwordHash;
                    //user.PasswordSalt = passwordSalt;
                    //await _gurujiDbContext.SaveChangesAsync();
                    if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                    {

                        return null;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return user;

        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                //here it is checking if the computedhash with the old password salt & db saved Computer hash both are same. 
                // i mean new password string comptued hash & db saved one. 
                var commputedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));


                for (int i = 0; i < commputedHash.Length; i++)
                {
                    if (commputedHash[i] != passwordHash[i])
                        return false;
                }
            }

            return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordSalt = passwordSalt;
            user.PasswordHash = passwordHash;

            await _gurujiDbContext.Users.AddAsync(user);
            await _gurujiDbContext.SaveChangesAsync();

            return user;

        }

        public async Task<User> UpdatePasswordHash(User user)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(user.Password, out passwordHash, out passwordSalt);

            user.PasswordSalt = passwordSalt;
            user.PasswordHash = passwordHash;

            return user;

        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }

        }

        public async Task<bool> UserExists(string username)
        {
            if (await _gurujiDbContext.Users.AnyAsync(x => x.EmailId == username || x.MobileNumber == username))
                return true;
            else
                return false;
        }

        public async Task<MobileUserResponse> MobileUserSignIn(string userName)
        {
            try
            {
                var result = await (from lead in _gurujiDbContext.Leads
                              join mobile in _gurujiDbContext.MobileUsers
                              on lead.PublicKey equals mobile.LeadKey
                              select new MobileUserResponse
                              {
                                  Id = lead.Id,
                                  PublicKey = lead.PublicKey,
                                  MobileNumber = lead.MobileNumber,
                                  EmailId = lead.EmailId,
                                  FullName = lead.FullName,
                                  Gender = Convert.ToChar(lead.Gender),
                                  Pincode = lead.PinCode,
                                  IMEI = mobile.Imei,
                                  ProfileImage = lead.ProfileImage
                              }
                              ).FirstOrDefaultAsync();

                if (result == null)
                    return null;

                return result;

            }
            catch (Exception ex)
            {

                return null;
            }
        }


       

        public async Task<long> MobileUserSignUp(MobileUserDto mobileRequest)
        {
            try
            {
                var dd = new MobileUser();
                _gurujiDbContext.MobileUsers.Add(dd);
                await _gurujiDbContext.SaveChangesAsync();

                return mobileRequest.MobileUserId;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<string> GetRoleName(Guid roleKey)
        {
            var result = await _gurujiDbContext.Roles.Where(item => item.PublicKey == roleKey).FirstOrDefaultAsync();
            return result.Name;
        }


        //public async Task<ActionResult<User>> GetUserById(int UserId)
        //{
        //    var userParam = new SqlParameter("@Key", UserId);
        //    var user = await _gurujiDbContext.Users.FromSqlRaw($"sp_GetUsersbyKey", userParam).ToListAsync();
        //    //return user;

        //}
    }
}
