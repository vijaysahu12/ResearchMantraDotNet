using RM.API.Models.Mail;
using RM.Database.KingResearchContext;
using RM.Model;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
        Task<bool> SendEmailAsyncMarketManthan(MailRequest mailRequest);
        Task<ApiCommonResponseModel> SendEmailAsyncTwo(MailRequest mailRequest);
    }
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly MailSettingsMarketManthan _mailSettingsMarketManthan;
        private readonly KingResearchContext _context;

        public MailService(IOptions<MailSettings> mailSettings, IOptions<MailSettingsMarketManthan> mailSettingsMarketManthan, KingResearchContext context)
        {
            _mailSettings = mailSettings.Value;
            _mailSettingsMarketManthan = mailSettingsMarketManthan.Value;
            _context = context;

        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            try
            {
                MimeMessage email = new()
                {
                    Sender = MailboxAddress.Parse(_mailSettings.Mail)
                };
                email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));

                if (!string.IsNullOrEmpty(mailRequest.CcEmail))
                {
                    email.Cc.Add(MailboxAddress.Parse(mailRequest.CcEmail));
                }
                if (!string.IsNullOrEmpty(mailRequest.BccEmail))
                {
                    email.Bcc.Add(MailboxAddress.Parse(mailRequest.BccEmail));
                }
                email.Subject = mailRequest.Subject;
                BodyBuilder builder = new();
                if (mailRequest.Attachments != null)
                {
                    byte[] fileBytes;
                    foreach (Microsoft.AspNetCore.Http.IFormFile file in mailRequest.Attachments)
                    {
                        if (file.Length > 0)
                        {
                            using (MemoryStream ms = new())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                            _ = builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                        }
                    }
                }
                builder.HtmlBody = mailRequest.Body;
                email.Body = builder.ToMessageBody();
                using SmtpClient smtp = new();
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, false);//SecureSocketOptions.StartTls
                smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
                _ = await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
            catch (Exception)
            {
            }
        }
        public async Task<bool> SendEmailAsyncMarketManthan(MailRequest mailRequest)
        {
            try
            {

                MimeMessage email = new()
                {
                    From = { new MailboxAddress("Market Manthan 2024", _mailSettingsMarketManthan.Mail) },
                    Sender = MailboxAddress.Parse(_mailSettingsMarketManthan.Mail),
                };


                email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));

                if (!string.IsNullOrEmpty(mailRequest.CcEmail))
                {
                    email.Cc.Add(MailboxAddress.Parse(mailRequest.CcEmail));
                }
                if (!string.IsNullOrEmpty(mailRequest.BccEmail))
                {
                    email.Bcc.Add(MailboxAddress.Parse(mailRequest.BccEmail));
                }
                email.Subject = mailRequest.Subject;
                BodyBuilder builder = new();


                builder.Attachments.Add("Images\\MarketManthan2024_AlgoTraders1.png");
                builder.Attachments.Add("Images\\KingResearchLogo.png");
                builder.Attachments.Add("Images\\MarketManthan2024_KingResearch_.pdf");

                builder.HtmlBody = mailRequest.Body;
                email.Body = builder.ToMessageBody();
                using SmtpClient smtp = new();
                smtp.Connect(_mailSettingsMarketManthan.Host, _mailSettingsMarketManthan.Port, false);//SecureSocketOptions.StartTls
                smtp.Authenticate(_mailSettingsMarketManthan.Mail, _mailSettingsMarketManthan.Password);
                var response = await smtp.SendAsync(email);
                smtp.Disconnect(true);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<ApiCommonResponseModel> SendEmailAsyncTwo(MailRequest mailRequest)
        {
            ApiCommonResponseModel response = new()
            {
                StatusCode = HttpStatusCode.OK
            };
            try
            {
                MimeMessage email = new();

                email.From.Add(new MailboxAddress("Sende Name", _mailSettings.Mail));
                email.To.Add(new MailboxAddress("Receiver Name", mailRequest.ToEmail));

                email.Subject = mailRequest.Subject;// "Testing out email sending";
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = mailRequest.Body ?? ""
                };

                using SmtpClient smtp = new();
                smtp.Connect(_mailSettings.Host, _mailSettings.Port, false);

                // Note: only needed if the SMTP server requires authentication
                //smtp.Authenticate("sp0301149@gmail.com", "fsxczzweleptounf");
                smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);

                _ = await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {

                response.Message = ex.InnerException.ToString();
                response.Data = ex.ToString();
                response.StatusCode = HttpStatusCode.InternalServerError;
            }
            return response;
        }
    }
}
