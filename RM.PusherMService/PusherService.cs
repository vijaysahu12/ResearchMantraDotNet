using PusherServer;

namespace RM.NotificationService
{
    public interface IPusherChannelService
    {
        public Task Send(string message, string channelName, string eventName);
    }
    public class PusherChannelService : IPusherChannelService
    {

        public async Task HelloWorld()
        {
            var options = new PusherOptions
            {
                Cluster = "ap2",
                Encrypted = true
            };
            var pusher = new Pusher(
                "1714674",
                "34cdcc1ab23eb32bb10e",
                "83f2a935b697d4015afe",
                options);

            var result = await pusher.TriggerAsync(
                 "my-channel",
                 "my-event",
        new { message = "hello world" });

        }

        public async Task Send(string message, string channelName, string eventName)
        {
            var appId = "1714674";
            var apiiKey = "34cdcc1ab23eb32bb10e";
            var apiiSecret = "83f2a935b697d4015afe";
            channelName = "my-channel";
            eventName = "BNZA";
            var options = new PusherOptions
            {
                Cluster = "ap2",
                Encrypted = true
            };
            var pusher = new Pusher(appId, apiiKey, apiiSecret, options);

            var result = await pusher.TriggerAsync(
                 channelName,
                 eventName,
            new { message });
        }
    }
}