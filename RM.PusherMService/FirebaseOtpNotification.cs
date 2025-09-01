using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace RM.NotificationService
{
    public class FirebaseOtpNotification
    {
        private const string FirebaseCredentialPath = "google-services.json"; // Replace with your Firebase Admin SDK JSON file path
        readonly string projectRoot = AppDomain.CurrentDomain.BaseDirectory; // Get project root path
        public async Task SendOTPNotificationAsync(string recipient, string otp)
        {
            string assetsPath = Path.Combine(projectRoot, "assets", FirebaseCredentialPath);

            FirebaseApp app = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(FirebaseCredentialPath)
            });

            if (app == null)
            {
                throw new Exception("Error initializing Firebase app.");
            }

            var message = new Message()
            {
                Token = recipient, // Recipient's FCM token
                Notification = new Notification
                {
                    Title = "OTP Notification",
                    Body = $"Your OTP is: {otp}"
                }
            };

            FirebaseMessaging messaging = FirebaseMessaging.DefaultInstance;
            try
            {
                string response = await messaging.SendAsync(message);
                ////Console.WriteLine("Message sent successfully: " + response);
            }
            catch (FirebaseMessagingException ex)
            {
                ////Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
    }

    public class FirebasePushNotification
    {
        private const string FirebaseCredentialPath = "Assets/pushnotification-VijayAccount.json"; // Replace with your Firebase Admin SDK JSON file path

        public async Task SendPushNotificationAsync(string deviceToken, string title, string body)
        {
            FirebaseApp app = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(FirebaseCredentialPath)
            });

            if (app == null)
            {
                throw new Exception("Error initializing Firebase app.");
            }

            var message = new Message()
            {
                Token = deviceToken, // Recipient's FCM token
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            FirebaseMessaging messaging = FirebaseMessaging.DefaultInstance;
            try
            {
                string response = await messaging.SendAsync(message);
                ////Console.WriteLine("Message sent successfully: " + response);
            }
            catch (FirebaseMessagingException ex)
            {
                ////Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        //////ussage
        ////    string deviceToken = "recipient_fcm_token"; // Replace with the recipient's FCM token
        ////string title = "Your Notification Title";
        ////string body = "Your Notification Body";

        ////string pathToFirebaseSDKJson = "path_to_your_firebase_admin_sdk_json_file.json";
        ////var firebasePushNotification = new FirebasePushNotification();

        ////await firebasePushNotification.SendPushNotificationAsync(deviceToken, title, body);
    }
}
