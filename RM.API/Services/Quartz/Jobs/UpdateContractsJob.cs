using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Threading.Tasks;

//namespace RM.API.Services.Quartz.Jobs
//{
//    public class UpdateContractsJob : IJob
//    {
//        private readonly IServiceProvider _serviceProvider;


//        public UpdateContractsJob(IServiceProvider serviceProvider)
//        {
//            _serviceProvider = serviceProvider;

//        }
//        public async Task Execute(IJobExecutionContext context)
//        {
//            using IServiceScope scope = _serviceProvider.CreateScope();

//            ISchedulerService updateContracts = scope.ServiceProvider.GetRequiredService<ISchedulerService>();
//            //var _exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLog>();
//            //await _exceptionLog.Logs("Job Triggered at " + DateTime.Now.ToString(), "IJob.PartnerSessionJob");


//            await updateContracts.UpdateContracts();
//        }
//    }
//}
