using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace RM.API.Models.Mail
{
    public class MailRequest
    {
        public string ToEmail { get; set; }
        public string? CcEmail { get; set; }
        public string? BccEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }
    public class MailSettings
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Security { get; set; }
    }
    public class MailSettingsMarketManthan
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Security { get; set; }
    }
}
