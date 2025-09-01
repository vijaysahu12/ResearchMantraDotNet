using Quartz;
using Quartz.Spi;
using System;

namespace RM.API.Services.Quartz
{
    public class JobFactory : IJobFactory
    {
        public readonly IServiceProvider _serviceProvider;
        public JobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            IJobDetail jobDetails = bundle.JobDetail;
            return (IJob)_serviceProvider.GetService(jobDetails.JobType);
        }

        public void ReturnJob(IJob job)
        {
        }
    }
}
