using RM.API.Dtos;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace RM.API.Helpers
{
    public class TokenAnalyser
    {
        public TokenVariables FetchTokenValues(ClaimsIdentity identity)
        {
            if (identity == null)
            {
                return null;
            }

            var tokenVariables = new TokenVariables();
            var claims = identity.Claims;

            var publicKeyClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            var firstNameClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            var emailClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            var roleClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            var primarySidClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.PrimarySid);

            tokenVariables.PublicKey = publicKeyClaim?.Value;
            tokenVariables.FirstName = firstNameClaim?.Value;
            tokenVariables.EmailId = emailClaim?.Value;
            tokenVariables.RoleKey = roleClaim?.Value;

            if (primarySidClaim?.Value != null && int.TryParse(primarySidClaim.Value, out int id))
            {
                tokenVariables.Id = id;
            }

            return tokenVariables;
        }

        //public string GetTokenValue()
        //{
        //    var identity = HttpContext.User.Identity as ClaimsIdentity;
        //    if (identity != null)
        //    {
        //        IEnumerable<Claim> claims = identity.Claims;
        //        string PrimarySid = claims.Where(x => x.Type == ClaimTypes.PrimarySid).FirstOrDefault().Value;
        //    }
        //}
    }

    public static class TokenAnalyserStatic
    {
        public static TokenVariables FetchTokenPart2(HttpContext context)
        {
            if (context == null || context.User == null || context.User.Identity is not ClaimsIdentity identity)
            {
                return null;
            }

            try
            {
                var claims = identity.Claims;

                // Helper function to safely get claim value
                string GetClaimValue(string claimType) =>
                    claims.FirstOrDefault(x => x.Type == claimType)?.Value;

                TokenVariables tokenVariables = new()
                {
                    PublicKey = GetClaimValue(ClaimTypes.NameIdentifier),
                    FirstName = GetClaimValue(ClaimTypes.Name),
                    EmailId = GetClaimValue(ClaimTypes.Email),
                    RoleKey = GetClaimValue(ClaimTypes.Role)
                };

                // Parsing PrimarySid (if it exists)
                string primarySid = GetClaimValue(ClaimTypes.PrimarySid);
                if (!string.IsNullOrEmpty(primarySid) && int.TryParse(primarySid, out int id))
                {
                    tokenVariables.Id = id;
                }
                else
                {
                    tokenVariables.Id = 0; // Default value or handle as per your requirement
                }

                return tokenVariables;
            }
            catch (Exception ex)
            {
                // Provide meaningful error messages for easier debugging
                throw new UnauthorizedAccessException("Failed to fetch token claims. Unauthorized access.", ex);
            }
        }
    }
}