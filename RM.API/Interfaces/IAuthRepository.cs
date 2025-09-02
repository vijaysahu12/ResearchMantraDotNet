using RM.API.Dtos;
using RM.API.Models;
using RM.Database.ResearchMantraContext;
using System;
using System.Threading.Tasks;

namespace RM.API.Interfaces
{
    public interface IAuthRepository
    {
        //registering a user
        Task<User> Register(User user, string password);

        Task<User> UpdatePasswordHash(User user);
        // login method 
        Task<User> Login(string username, string password);

        Task<bool> UserExists(string username);
        Task<long> MobileUserSignUp(MobileUserDto mobileRequest);

        Task<MobileUserResponse> MobileUserSignIn(string userName);

        Task<string> GetRoleName(Guid roleKey);

    }
}
