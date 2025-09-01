//using RM.Model.ResponseModel;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace RM.API.Services.Quartz.Jobs
//{
//    public class CustomerServiceExpiryReminderJob : IJob
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private IConfiguration _Configuration { get; }

//        public CustomerServiceExpiryReminderJob(IServiceProvider serviceProvider, IConfiguration configuration)
//        {
//            _serviceProvider = serviceProvider;
//            _Configuration = configuration;

//        }
//        public async Task Execute(IJobExecutionContext context)
//        {
//            using IServiceScope scope = _serviceProvider.CreateScope();

//            ISchedulerService schedulerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
//            //var _exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLog>();
//            //await _exceptionLog.Logs("Job Triggered at " + DateTime.Now.ToString(), "IJob.PartnerSessionJob");

//            List<ExpiredServiceResponseModel> expiredList = new();
//            string days = _Configuration.GetSection("AppSettings:ServiceExpiryNotificationBefore").Value;
//            await schedulerService.NotifyOnExpiredServices(days);
//        }
//    }
//}
