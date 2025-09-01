//using RM.Model.ResponseModel;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace RM.API.Services.Quartz.Jobs
//{
//    public class UnTochedLeadAssignedToNullReminderJob : IJob
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private IConfiguration _Configuration { get; }

//        public UnTochedLeadAssignedToNullReminderJob(IServiceProvider serviceProvider, IConfiguration configuration)
//        {
//            _Configuration = configuration;
//            _serviceProvider = serviceProvider;

//        }
//        public async Task Execute(IJobExecutionContext context)
//        {
//            using IServiceScope scope = _serviceProvider.CreateScope();

//            ISchedulerService partnerService = scope.ServiceProvider.GetRequiredService<ISchedulerService>();

//            List<ExpiredServiceResponseModel> expiredList = new();

//            string days = _Configuration.GetSection("AppSettings:UntochedLeadsJobs").Value;
//            await partnerService.GetUntouchedLeads();
//        }
//    }
//}
