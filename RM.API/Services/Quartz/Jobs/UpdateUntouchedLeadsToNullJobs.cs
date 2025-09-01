using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Threading.Tasks;

namespace RM.API.Services.Quartz.Jobs
{
    public class UpdateUntouchedLeadsToNullJobs : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private IConfiguration _Configuration { get; }

        public UpdateUntouchedLeadsToNullJobs(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _Configuration = configuration;
            _serviceProvider = serviceProvider;

        }
        public async Task Execute(IJobExecutionContext context)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            ISchedulerService partnerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
            //var _exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLog>();
            //await _exceptionLog.Logs("Job Triggered at " + DateTime.Now.ToString(), "IJob.PartnerSessionJob");


            await partnerService.UpdateUntouchedLeadsToNull();
        }
    }
}