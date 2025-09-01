using RM.Database.KingResearchContext;
using RM.Model.DB.Tables;
using System;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface IActivityService
    {
        Task UserLog(string userKey, Guid? leadKey, ActivityTypeEnum ActivityType, string? description);
        Task LeadLog(string leadKey, string userKey, ActivityTypeEnum ActivityType, string source = null, string destination = null, string description = null);
    }

    public class ActivityService : IActivityService
    {
        private readonly KingResearchContext _context;

        public ActivityService(KingResearchContext context)
        {
            _context = context;
        }

        public async Task UserLog(string userKey, Guid? leadKey, ActivityTypeEnum ActivityType, string? description)
        {
            UserActivity userActivity = new()
            {
                PublicKey = Guid.Parse(userKey),
                CreatedOn = DateTime.Now,
                LeadKey = leadKey,
                ActivityType = (int)ActivityType,
                Description = description
            };
            _ = _context.UserActivity.Add(userActivity);
            _ = await _context.SaveChangesAsync();
        }

        public async Task LeadLog(string leadKey, string userKey, ActivityTypeEnum ActivityType, string source = null, string destination = null, string description = null)
        {
            LeadActivity userActivity = new()
            {
                LeadKey = Guid.Parse(leadKey),
                Message = ActivityType.ToString(),
                CreatedOn = DateTime.Now,
                CreatedBy = Guid.Parse(userKey),
                Source = source is not null ? Guid.Parse(source) : null,
                Destination = destination is not null ? Guid.Parse(destination) : null,
                ActivityType = (int)ActivityType,
                Description = description
            };
            _ = _context.LeadActivity.Add(userActivity);
            _ = await _context.SaveChangesAsync();
        }
    }
}
