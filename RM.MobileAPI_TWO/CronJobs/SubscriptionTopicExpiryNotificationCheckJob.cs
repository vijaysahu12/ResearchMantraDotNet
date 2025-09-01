//using Quartz;

namespace RM.MobileAPI.CronJobs
{
    public class SubscriptionTopicExpiryNotificationCheckJob(IServiceProvider serviceProvider) //: IJob
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        //public async Task Execute(IJobExecutionContext context)
        //{

        //    using IServiceScope scope = _serviceProvider.CreateScope();

        //    IMobileNotificationService mobileNotificationService = scope.ServiceProvider.GetRequiredService<IMobileNotificationService>();

        //    await mobileNotificationService.SendSubscriptionExpiryCheckNotification();
        //}
    }
}
