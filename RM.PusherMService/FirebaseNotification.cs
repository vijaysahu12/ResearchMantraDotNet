using FirebaseAdmin.Messaging;
using RM.Model;
using RM.Model.ResponseModel;
using System.Collections.Concurrent;
using System.Net;

namespace RM.NotificationService
{
    public class FirebaseNotification
    {
        private readonly ApiCommonResponseModel _apiResponse = new();

        public async Task<bool> SendAsync(string title, string body, string fcmToken, string topic = "")
        {
            try
            {
                var msg = new Message
                {
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body,
                    },
                    Android = new AndroidConfig
                    {
                        Notification = new AndroidNotification
                        {
                            Sound = "notification_tone",
                            ChannelId = "kingResearchAcademy",
                            Priority = string.IsNullOrEmpty(topic) ? NotificationPriority.HIGH : NotificationPriority.LOW
                        },
                        Priority = string.IsNullOrEmpty(topic) ? Priority.High : Priority.Normal
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = "notification_tone.wav",
                            ContentAvailable = true,
                        }
                    },
                    Data = new Dictionary<string, string>(),
                    Token = fcmToken
                };

                var fcmResponse = await FirebaseMessaging.DefaultInstance.SendAsync(msg);
                return fcmResponse != null;
            }
            catch (Exception ex)
            {
                // Log error message
                return false;
            }
        }

        public async Task<ApiCommonResponseModel> SendFCMMessage(string title, string body, List<UserListForPushNotificationModel> listOfToken, Dictionary<string, string>? data, string topic = "", int chunkSize = 500, bool withNotificationPayload = true)
        {
            var batchResponses = new ConcurrentBag<BatchResponse>();
            try
            {
                var message = CreateMulticastMessage(title, body, data, topic == "" ? "ANNOUNCEMENT" : topic, withNotificationPayload);


                // Separate new and old devices
                var newDevices = listOfToken.Where(x => !x.OldDevice).ToList();
                var oldDevices = listOfToken.Except(newDevices).ToList();


                var tasks = new List<Task>();
                for (int i = 0; i < oldDevices.Count; i += chunkSize)
                {
                    var chunk = oldDevices.Skip(i).Take(chunkSize).ToList();
                    //await SendChunkAsync(message, chunk, batchResponses);
                    tasks.Add(SendChunkAsync(message, chunk, batchResponses));
                }


                for (int i = 0; i < newDevices.Count; i += chunkSize)
                {
                    var chunk = newDevices.Skip(i).Take(chunkSize).ToList();
                    tasks.Add(SendChunkAsync(message, chunk, batchResponses));
                }

                await Task.WhenAll(tasks);

                // ✅ Extract failed tokens after all batches are done
                var failedReasons = batchResponses
                 .SelectMany(br => br.Responses)
                 .Where(r => !r.IsSuccess)
                 .Select(r => r.Exception?.Message ?? "Unknown error")
                 .ToList(); //ToDo: Debug and fix the logic to log into mongoDb Table


                _apiResponse.Data = new
                {
                    TotalChunks = batchResponses.Count,
                    FailedCunks = failedReasons.Count,
                    Message = "FCM notifications sent"
                };
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.Message = "success";
            }
            catch (Exception ex)
            {
                _apiResponse.Data = null;
                _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                _apiResponse.Message = ex.InnerException == null ? ex.Message : ex.InnerException.ToString();
            }


            return _apiResponse;
        }

        private MulticastMessage CreateMulticastMessage(string title, string body, Dictionary<string, string>? data, string topic, bool withNotificationPayload)
        {
            var message = new MulticastMessage
            {
                Android = new AndroidConfig
                {
                    Notification = new AndroidNotification
                    {
                        Sound = "notification_tone",
                        ChannelId = "kingResearchAcademy",
                        Priority = string.IsNullOrEmpty(topic) ? NotificationPriority.HIGH : NotificationPriority.LOW
                    },
                    Priority = string.IsNullOrEmpty(topic) ? Priority.High : Priority.Normal
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "notification_tone.wav",
                        ContentAvailable = true,
                    }
                },
                Data = data ?? []
            };


            // Add notification payload if required else use data payload for push notification with image or other data
            if (withNotificationPayload)
            {
                var notification = new Notification
                {
                    Title = title,
                    Body = body
                };

                string? imageUrl = data?.GetValueOrDefault("NotificationImage");
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    notification.ImageUrl = imageUrl;
                }

                message.Notification = notification;
            }

            return message;
        }

        public async Task<ApiCommonResponseModel> SendFCMMessageWithoutNotificationBody(string title, string body, List<UserListForPushNotificationModel> listOfToken, Dictionary<string, string>? data, string topic = "", int chunkSize = 500)
        {
            var apiResponse = new ApiCommonResponseModel();
            var batchResponses = new ConcurrentBag<BatchResponse>(); // Thread-safe collection

            try
            {
                // Separate new and old devices
                var newDevices = listOfToken.Where(x => !x.OldDevice).ToList();
                var oldDevices = listOfToken.Except(newDevices).ToList();

                // Process new devices (Silent push)
                if (newDevices.Any())
                {
                    await SendFCMMessageChunks(newDevices, null, data, topic, chunkSize, batchResponses);
                }

                // Process old devices (With notification)
                if (oldDevices.Any())
                {
                    var notification = new Notification
                    {
                        Title = title,
                        Body = body
                    };
                    await SendFCMMessageChunks(oldDevices, notification, data, topic, chunkSize, batchResponses);
                }

                apiResponse.Data = batchResponses.ToList();
                apiResponse.StatusCode = HttpStatusCode.OK;
                apiResponse.Message = "success";
            }
            catch (Exception ex)
            {
                apiResponse.Data = null;
                apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiResponse.Message = ex.InnerException?.ToString() ?? ex.Message;
            }
            return apiResponse;
        }

        private async Task SendFCMMessageChunks(List<UserListForPushNotificationModel> tokens, Notification? notification, Dictionary<string, string>? data, string topic,
            int chunkSize, ConcurrentBag<BatchResponse> batchResponses)
        {
            for (int i = 0; i < tokens.Count; i += chunkSize)
            {
                var chunkTokens = tokens.Skip(i).Take(chunkSize).Select(t => t.FirebaseFcmToken).ToList();

                var message = new MulticastMessage
                {
                    Tokens = chunkTokens,
                    Data = data,
                    Android = new AndroidConfig
                    {
                        Notification = new AndroidNotification
                        {
                            Sound = "notification_tone",
                            ChannelId = "kingResearchAcademy",
                            Priority = string.IsNullOrEmpty(topic) ? NotificationPriority.HIGH : NotificationPriority.LOW
                        },
                        Priority = string.IsNullOrEmpty(topic) ? Priority.High : Priority.Normal
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = "notification_tone.wav",
                            ContentAvailable = true
                        }
                    }
                };

                if (notification is not null)
                {
                    message.Notification = notification;
                }

                var fcmResponse = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                batchResponses.Add(fcmResponse);
            }
        }

        //public async Task<ApiCommonResponseModel> SendFCMMessage(string title, string body, List<NotificationReceiverResponseModel> listOfToken, Dictionary<string, string>? data, string topic = "", int chunkSize = 500, bool validateToken = false)
        //{
        //    var message = new MulticastMessage
        //    {
        //        Notification = new Notification
        //        {
        //            Title = title,
        //            Body = body,
        //        },
        //        Android = new AndroidConfig
        //        {
        //            Notification = new AndroidNotification
        //            {
        //                Sound = "notification_tone",
        //                ChannelId = "kingResearchAcademy",
        //                Priority = string.IsNullOrEmpty(topic) ? NotificationPriority.HIGH : NotificationPriority.LOW
        //            },
        //            Priority = string.IsNullOrEmpty(topic) ? Priority.High : Priority.Normal
        //        },
        //        Apns = new ApnsConfig
        //        {
        //            Aps = new Aps
        //            {
        //                Sound = "notification_tone.wav",
        //                ContentAvailable = true,
        //            }
        //        },
        //        Data = data ?? new Dictionary<string, string>()
        //    };
        //    return message;
        //}

        private static async Task SendChunkAsync(MulticastMessage message, List<UserListForPushNotificationModel> chunk, ConcurrentBag<BatchResponse> batchResponses)
        {
            try
            {
                message.Tokens = [.. chunk.Select(item => item.FirebaseFcmToken)];

                var batch = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                batchResponses.Add(batch);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (log, retry, etc.)
                Console.WriteLine($"Error sending chunk: {ex.Message}");
            }
        }

        public async Task<bool> ValidateFcmTokenAsync(List<string> listOfToken)
        {
            foreach (var token in listOfToken)
            {
                try
                {
                    var message = new Message
                    {
                        Token = token,
                        Notification = null,
                        Data = new Dictionary<string, string>
                        {
                            { "test", "true" }
                        }
                    };

                    await FirebaseMessaging.DefaultInstance.SendAsync(message);
                }
                catch (FirebaseMessagingException ex)
                {
                    if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered || ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
                    {
                        Console.WriteLine($"Invalid Token: {token}, Error: {ex.Message}");
                    }
                    Console.WriteLine($"Error validating token: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
            }
            return true;
        }


        public bool IsOldDevice(string deviceVersion, string deviceType)
        {
            if (string.IsNullOrEmpty(deviceVersion))
            {
                return true; // DeviceVersion is NULL
            }

            var versionParts = deviceVersion.Split('.'); // Splitting version number by "."

            if (versionParts.Length < 3) return true; // Ensure version has at least three parts

            if (int.TryParse(versionParts[0], out int major) &&
                int.TryParse(versionParts[1], out int minor) &&
                int.TryParse(versionParts[2], out int patch))
            {
                if (deviceType == "iOS" && major == 1 && minor == 0 && patch < 15)
                {
                    return true; // Old iOS device
                }

                if (deviceType == "Android" && major == 1 && minor == 0 && patch < 57)
                {
                    return true; // Old Android device
                }
            }

            return false; // Not an old device
        }

        public async Task SendFCMMessage(object title, object body, List<UserListForPushNotificationModel> listOfToken, Dictionary<string, string> data, string topic)
        {
            throw new NotImplementedException();
        }
    }
}