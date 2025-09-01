using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Threading.Tasks;

namespace RM.API.Services.Quartz.Jobs
{
    public class LeadsFollowUpReminder : IJob
    {

        private readonly IServiceProvider _serviceProvider;
        private IConfiguration _Configuration { get; }
        public LeadsFollowUpReminder(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _Configuration = configuration;

        }

        public async Task Execute(IJobExecutionContext context)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            ISchedulerService schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
            // incomplete : for now it is sending notification to all 
            await schedulerService.NotifyFollowUpReminder();
        }
    }
}
