using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RM.API.Services.Quartz
{
    public class PartnerScheduler : IHostedService
    {
        public IScheduler Scheduler { get; set; }
        public readonly IJobFactory _jobFactory;
        private readonly JobMetadata _jobMetaData;
        private readonly ISchedulerFactory _schedulerFactory;

        public PartnerScheduler(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, JobMetadata jobMetadata)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _jobMetaData = jobMetadata;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Creating Schedular 
            Scheduler = await _schedulerFactory.GetScheduler();
            Scheduler.JobFactory = _jobFactory;
            // Create Job 
            IJobDetail jobDetail = CreateJob(_jobMetaData);
            // Create Trigger 
            ITrigger trigger = CreateTrigger(_jobMetaData);
            // Schedule JOb 
            _ = await Scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
            await Scheduler.Start(cancellationToken);
            // Start The Scheduler 

        }

        private ITrigger CreateTrigger(JobMetadata jobMetaData)
        {
            return
                TriggerBuilder.Create()
                .WithIdentity(jobMetaData.JobId.ToString())
                .WithCronSchedule(jobMetaData.CronExpression)
                .WithDescription(jobMetaData.JobName)
                .Build();
        }

        private IJobDetail CreateJob(JobMetadata jobMetaData)
        {
            return
                JobBuilder.Create(jobMetaData.JobType)
                .WithIdentity(jobMetaData.JobId.ToString())
                .WithDescription(jobMetaData.JobName)
                .Build();

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
