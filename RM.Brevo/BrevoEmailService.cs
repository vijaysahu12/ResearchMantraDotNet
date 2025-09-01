using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace RM.Brevo
{
    public interface IEmailService
    {
        // Task SendOrderConfirmationEmailAsync(EmailRequest request);

        Task SendWelcomeEmailAsync(string toEmail, string userName);
    }
    public class BrevoEmailService : IEmailService
    {
        private readonly BrevoSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<BrevoEmailService> _logger;
        private readonly IConfiguration _config;


        public BrevoEmailService(IOptions<BrevoSettings> settings, HttpClient httpClient, ILogger<BrevoEmailService> logger, IConfiguration config)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
            _httpClient.BaseAddress = new Uri("https://api.brevo.com/v3/");
            _httpClient.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var senderEmail = _config["MailSettings:Mail"];
            var senderName = _config["MailSettings:DisplayName"];
            var emailPayload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = toEmail } },
                subject = "Welcome to " + senderName + "!",
                htmlContent = $"<h1>Welcome, {userName}!</h1><p>We’re excited to have you on board. Enjoy your journey with us!</p>"
            };

            await SendEmailAsync(emailPayload, toEmail);
        }

        private async Task SendEmailAsync(object emailPayload, string recipientEmail)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(emailPayload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("smtp/email", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {Email}", recipientEmail);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send email to {Email}. Status: {Status}, Error: {Error}",
                        recipientEmail, response.StatusCode, errorContent);
                    throw new Exception($"Failed to send email: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", recipientEmail);
                throw new Exception("Failed to send email", ex);
            }
        }


    }
}
