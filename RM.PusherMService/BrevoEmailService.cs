
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.MongoDbCollection;
using RM.Model.RequestModel.MobileApi;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using System;
using System.Data;
using System.Reflection;
using static MongoDB.Driver.WriteConcern;
using BrevoModel = sib_api_v3_sdk.Model; // Alias for sib_api_v3_sdk.Model to avoid Task conflict

namespace RM.NotificationService
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string userName);
        Task SendOrderConfirmationEmailAsync(string toEmail, string userName, string productName, string productCategory);
        Task SendPaymentSuccessEmailAsync(string toEmail, string userName, string mobile, string accessStartDate, string ProductCode);

        Task<ApiCommonResponseModel> SendConfirmationActiveNotification(string toEmail, string userName, MobileUser mobileUser, string productItem, string accessStartDate, string amount, string validityInDays, PurchaseOrderMRequestModel purchaseRequest);
    }

    public class BrevoEmailService : IEmailService
    {
        private readonly ApiCommonResponseModel responseModel = new();

        private readonly BrevoSettings _settings;
        private readonly TransactionalEmailsApi _apiInstance;
        private readonly IMongoRepository<Log> _mongoRepo;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly IConfiguration _config;
        private readonly ResearchMantraContext _context;

        public BrevoEmailService(
            IOptions<BrevoSettings> settings,
            IMongoRepository<Log> mongoRepo,
            IConfiguration config,
            ResearchMantraContext context)
        {
            _settings = settings.Value;
            _mongoRepo = mongoRepo;
            _config = config;
            _context = context;

            // Configure the Brevo API client
            Configuration.Default.AddApiKey("api-key", _settings.ApiKey);
            _apiInstance = new TransactionalEmailsApi();

            _senderEmail = _config["MailSettings:Mail"]!;
            _senderName = _config["MailSettings:DisplayName"]!;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            try
            {
                var sender = new BrevoModel.SendSmtpEmailSender(_senderName, _senderEmail);
                var to = new List<BrevoModel.SendSmtpEmailTo> { new BrevoModel.SendSmtpEmailTo(toEmail) };
                var email = new BrevoModel.SendSmtpEmail
                {
                    Sender = sender,
                    To = to,
                    Subject = $"Welcome to {_senderName}!",
                    HtmlContent = $"<h1>Welcome, {userName}!</h1><p>We’re excited to have you on board. Enjoy your journey with us!</p>"
                };


                await SendEmailAsync(email, toEmail);
            }
            catch (Exception ex)
            {
                await _mongoRepo.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"Exception while sending welcome email to {toEmail}: {ex.Message}",
                    Source = "SendWelcomeEmailAsync",
                    Category = "Email"
                });
            }
        }

        public async Task SendOrderConfirmationEmailAsync(string toEmail, string userName, string productName, string productCategory)
        {
            try
            {
                await _mongoRepo.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"Initiating order confirmation email. To: {toEmail}, User: {userName}, Product: {productName}, Category: {productCategory}",
                    Source = "SendOrderConfirmationEmailAsync",
                    Category = "Email"
                });

                var sqlParameters = new SqlParameter[]
                {
                    new SqlParameter { ParameterName = "Category", Value = productCategory, SqlDbType = SqlDbType.VarChar, Size = 50 },
                    new SqlParameter { ParameterName = "CustomerName", Value = userName, SqlDbType = SqlDbType.NVarChar, Size = 255 },
                    new SqlParameter { ParameterName = "ServiceName", Value = productName, SqlDbType = SqlDbType.NVarChar, Size = 255 }
                };

                var emailTemplates = await _context.SqlQueryToListAsync<EmailTemplate>(
                    "EXEC GenerateEmailContent @Category, @CustomerName, @ServiceName", sqlParameters.ToArray()
                );

                var emailContent = emailTemplates.FirstOrDefault();

                if (emailContent != null)
                {
                    var sender = new BrevoModel.SendSmtpEmailSender(_senderName, _senderEmail);
                    var to = new List<BrevoModel.SendSmtpEmailTo> { new BrevoModel.SendSmtpEmailTo(toEmail) };
                    var email = new BrevoModel.SendSmtpEmail
                    {
                        Sender = sender,
                        To = to,
                        Subject = emailContent.EmailSubject,
                        HtmlContent = emailContent.EmailBody
                    };

                    await _mongoRepo.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = $"Sending order confirmation email to {toEmail} with subject: {emailContent.EmailSubject}",
                        Source = "SendOrderConfirmationEmailAsync",
                        Category = "Email"
                    });

                    await SendEmailAsync(email, toEmail);
                }
                else
                {
                    var errorMsg = $"Failed to retrieve email template for category: {productCategory}";

                    await _mongoRepo.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = errorMsg,
                        Source = "SendOrderConfirmationEmailAsync",
                        Category = "Email"
                    });

                    throw new Exception(errorMsg); // Ensure the method either completes or throws
                }
            }
            catch (Exception ex)
            {
                await _mongoRepo.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"Exception while sending email to {toEmail}: {ex.Message}",
                    Source = "SendOrderConfirmationEmailAsync",
                    Category = "Email"
                });

                throw;
            }
        }
        public async Task SendPaymentSuccessEmailAsync(string toEmail, string userName, string mobile, string accessStartDate, string ProductCode)
        {
            try
            {
                if (!string.IsNullOrEmpty(ProductCode) && ProductCode == "BREAKFASTSTRATEGY")
                {
                    await _mongoRepo.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = $"Initiating payment success email. To: {toEmail}, User: {userName}, Mobile: {mobile}",
                        Source = "SendPaymentSuccessEmailAsync",
                        Category = "Email"
                    });

                    var sender = new BrevoModel.SendSmtpEmailSender(_senderName, _senderEmail);
                    var to = new List<BrevoModel.SendSmtpEmailTo> { new BrevoModel.SendSmtpEmailTo(toEmail) };


                    var email = new BrevoModel.SendSmtpEmail
                    {
                        Sender = sender,
                        To = to,
                        Subject = "✅ Congratulations! – Your smart trading journey begins!",
                        HtmlContent = $@"
                                <p>Dear {userName},</p>
                                <p>Welcome aboard! 🎉</p>

                                <p>You’ve just unlocked the power of <strong>15-minute trading</strong> with the <strong>Breakfast Strategy</strong>—designed for professionals, entrepreneurs, and busy individuals like you. Now, trade smartly without sitting in front of charts all day.</p>

                                <p><strong>✅ Your purchase linked to {mobile} is confirmed!</strong> 🚀 Your access begins on <strong>{accessStartDate}</strong>.</p>

                                <p>As an early bird, you’ve unlocked exclusive benefits:</p>

                                <ul>
                                    <li>🎥 3 months access to recorded course modules</li>
                                    <li>🔍 3 months access to the Breakfast Scanner</li>
                                    <li>❓ Submit doubts via query form for live session discussion</li>
                                    <li>🧑‍💻 Up to 3 live trading sessions access</li>
                                    <li>📼 Access to live session recordings</li>
                                    <li>🌐 Lifetime access to the Breakfast Community</li>
                                </ul>

                                <p><strong>Quick Start Guide:</strong></p>
                                <ul>
                                    <li>📱 <strong>Download the App:</strong><br>
                                        Android: <a href='https://bit.ly/446laEw'>Play Store</a> | 
                                        iOS: <a href='https://bit.ly/4jM6DSY'>App Store</a>
                                    </li>
                                    <li>👤 <strong>Sign Up:</strong> Use your registered mobile number: {mobile}</li>
                                    <li>📚 <strong>Course Access:</strong> From {accessStartDate}, go to ‘Strategies’ → Select ‘Breakfast Strategy’</li>
                                    <li>🔎 <strong>Scanner Access:</strong> Find it on the homepage or within the course section</li>
                                    <li>📅 <strong>Live Sessions:</strong><br>
                                        Note your doubts as you go through the course<br>
                                        Submit via: Breakfast Strategy → Upcoming events → Query Form
                                    </li>
                                </ul>

                                <p><strong>🛠️ Need Help?</strong><br>
                                📞 Contact us at <a href='tel:+916300292841'>+91 6300292841</a><br>
                                📧 <a href='mailto:support-app@kingresearch.co.in'>support-app@kingresearch.co.in</a></p>

                                <p>Wishing you an amazing journey as a smart, confident trader.<br>
                                <strong>Remember: Learn and then earn!</strong></p>

                                <p>— Support Team<br>King Research Academy</p>"
                    };

                    await _mongoRepo.AddAsync(new Log
                    {
                        CreatedOn = DateTime.Now,
                        Message = $"Sending payment success email to {toEmail} with subject: {email.Subject}",
                        Source = "SendPaymentSuccessEmailAsync",
                        Category = "Email"
                    });

                    await SendEmailAsync(email, toEmail);
                }

            }
            catch (Exception ex)
            {
                await _mongoRepo.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"Exception while sending payment success email to {toEmail}: {ex.Message}",
                    Source = "SendPaymentSuccessEmailAsync",
                    Category = "Email"
                });

            }
        }

        private async Task SendEmailAsync(BrevoModel.SendSmtpEmail email, string recipientEmail)
        {
            try
            {
                await _mongoRepo.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"Initiating email send. Recipient: {recipientEmail}, Subject: {email.Subject}",
                    Source = "SendEmailAsync",
                    Category = "Email"
                });

                var result = await _apiInstance.SendTransacEmailAsync(email);

                await _mongoRepo.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"Email sent successfully to {recipientEmail}. Message ID: {result.MessageId}",
                    Source = "SendEmailAsync",
                    Category = "Email"
                });
            }
            catch (Exception ex)
            {
                await _mongoRepo.AddAsync(new Log
                {
                    CreatedOn = DateTime.Now,
                    Message = $"Exception occurred while sending email to {recipientEmail}: {ex.Message}",
                    Source = "SendEmailAsync",
                    Category = "Email"
                });

                throw new Exception("Failed to send email", ex);
            }
        }

        public async Task<ApiCommonResponseModel> SendConfirmationActiveNotification(string toEmail, string userName, MobileUser mobileUser, string productItem , string accessStartDate, string amount, string ValidityInDays, PurchaseOrderMRequestModel purchaseRequest)
        {
            if (mobileUser == null)
            {
                responseModel.StatusCode = System.Net.HttpStatusCode.BadRequest;
                responseModel.Message = "Mobile user not found.";
                return responseModel;
            }

            // Log initiation
            await _mongoRepo.AddAsync(new Log
            {
                CreatedOn = DateTime.Now,
                Message = $"Initiating activation confirmation email. To: {toEmail}, User: {userName}, Mobile: {mobileUser.Mobile}",
                Source = "SendActivationSuccessEmailAsync",
                Category = "Email"
            });

            // 1. Define shared invoice directory (must exist or will be created)
            string sharedInvoiceDirectory = @"D:\SharedFiles\Invoices"; // common folder for both APIs

            if (!Directory.Exists(sharedInvoiceDirectory))
                Directory.CreateDirectory(sharedInvoiceDirectory);
            // 2. Generate invoice File Name
            string invoiceFileName = $"Invoice_{DateTime.Now:yyyyMMddHHmm}_{mobileUser.Mobile}.pdf";

            // 3. Build unique invoice file name and full path
            string invoicePath = Path.Combine(sharedInvoiceDirectory, invoiceFileName);

            //  4. Generate the PDF invoice
            decimal parsedAmount = decimal.Parse(amount);
            // await GenerateStyledInvoicePdfAsync(userName, mobileUser, productItem, invoiceFileName, DateTime.Parse(accessStartDate), parsedAmount, invoicePath);

            // Update Invoice File Name in PurchaseOrdersM Table (SQL)
            var purchaseOrder = await _context.PurchaseOrdersM
                .FirstOrDefaultAsync(p => p.ActionBy == mobileUser.PublicKey && p.TransasctionReference == purchaseRequest.MerchantTransactionId);

            if (purchaseOrder != null)
            {
                purchaseOrder.Invoice = invoiceFileName; // or invoicePath
                await _context.SaveChangesAsync();
            }

            //  5. Convert to base64
            byte[] pdfBytes = await File.ReadAllBytesAsync(invoicePath);
            string base64Pdf = Convert.ToBase64String(pdfBytes);

            //  6. Build email with attachment
            var sender = new BrevoModel.SendSmtpEmailSender(_senderName, _senderEmail);
            var to = new List<BrevoModel.SendSmtpEmailTo> { new BrevoModel.SendSmtpEmailTo(toEmail) };

            var email = new BrevoModel.SendSmtpEmail
            {
                Sender = sender,
                To = to,
                Subject = "We Received Your Payment Successfully",
                HtmlContent = $@"
                            <p>Hi {userName},</p>

                            <p>Your journey to smarter trading just got an upgrade. We are pleased to confirm your purchase and welcome you into the <strong>King Research</strong> community.</p>

                            <p><strong>Here are the details of your successful transaction:</strong></p>
                            <ul>
                                <li><strong>Product Purchased:</strong> {productItem}</li>
                                <li><strong>Registered Mobile:</strong> {mobileUser.Mobile}</li>
                                <li><strong>Date of Purchase:</strong> {accessStartDate}</li>
                                <li><strong>Validity:</strong> {ValidityInDays}</li>
                            </ul>

                            <p>You now have exclusive access to insights and tools that will help you trade smarter and grow faster.</p>

                            <p><strong>Need help with setup or have any questions?</strong> We’re just a call or email away!</p>
                            <ul>
                                <li><strong>Support Mobile:</strong> <a href='tel:+919000406184'>+91 9000406184</a></li>
                                <li><strong>Support Email:</strong> <a href='mailto:support@kingresearch.co.in'>support@kingresearch.co.in</a></li>
                            </ul>

                            <p>We’re here to support your growth every single day!</p>

                            <p>Regards,<br/>
                            <strong>Team King Research</strong></p>",
                Attachment = new List<BrevoModel.SendSmtpEmailAttachment>
                {
                    new BrevoModel.SendSmtpEmailAttachment(
                        url: null,
                        content: pdfBytes,
                        name: $"Invoice_{invoiceFileName}.pdf"
                    )
                }
            };

            // Log before sending
            await _mongoRepo.AddAsync(new Log
            {
                CreatedOn = DateTime.Now,
                Message = $"Sending activation confirmation email to {toEmail} with subject: {email.Subject}. InvoiceNumber : {invoiceFileName} InvoicePath : {invoicePath}",
                Source = "SendActivationSuccessEmailAsync",
                Category = "Email"
            });

            // Send the email
            await SendEmailAsync(email, toEmail);

            return new ApiCommonResponseModel
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Message = "Activation confirmation email sent successfully.",
                Data = new { InvoicePath = invoicePath, InvoiceNumber = invoiceFileName }
            };
        }

        // public async Task GenerateStyledInvoicePdfAsync(string userName, MobileUser mobileUser, string productName, string invoiceNumber, DateTime accessStartDate, decimal amount, string outputPath)
        // {
        //     decimal taxRate = 0.18m;
        //     decimal baseAmount = Math.Round(amount / (1 + taxRate), 2);
        //     decimal taxAmount = amount - baseAmount;
        //     string userState = mobileUser.City ?? "NA";
        //     string gstCode = GstStateCodes.StateCodeMap.TryGetValue(userState, out var code) ? code : "NA";

        //     using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        //     var document = new Document(PageSize.A4, 36, 36, 36, 36);
        //     var writer = PdfWriter.GetInstance(document, stream);
        //     document.Open();

        //     var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        //     var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

        //     var fullPageBorderTable = new PdfPTable(1) { WidthPercentage = 100 };
        //     var outerTable = new PdfPTable(1) { WidthPercentage = 100 };
        //     var borderWrapper = new PdfPCell { Border = Rectangle.NO_BORDER, Padding = 0f };

        //     var contentTable = new PdfPTable(1) { WidthPercentage = 100 };

        //     // === Title ===
        //     var title = new Paragraph("Tax Invoice", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
        //     {
        //         Alignment = Element.ALIGN_CENTER,
        //         SpacingAfter = 10f
        //     };
        //     contentTable.AddCell(new PdfPCell(title) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });

        //     // === Company & Invoice Info ===
        //     var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
        //     headerTable.SetWidths(new float[] { 70f, 30f });

        //     var companyInfo = "KING RESEARCH ACADEMY\n" +
        //                       "PLOT NO.19&41-13, 2B AND 2D, 406B, Jain Sadguru Images\n" +
        //                       "Capital Park,\n" +
        //                       "Madhapur Village, Serilingampally, Rangareddy, Hyderabad\n" +
        //                       "GSTIN/UIN: 36BLPPS3144G1ZS\n" +
        //                       "State Name : Telangana, Code : 36";
        //     headerTable.AddCell(CreateBox(companyInfo, normal));

        //     var rightTable = new PdfPTable(1);
        //     rightTable.AddCell(CreateBox($"Invoice No.: {invoiceNumber}", normal));
        //     rightTable.AddCell(CreateBox($"Dated: {DateTime.Now:dd-MMM-yy}", normal));
        //     rightTable.AddCell(CreateBox("Delivery Note:", normal));
        //     rightTable.AddCell(CreateBox("Mode/Terms of Payment:", normal));
        //     rightTable.AddCell(CreateBox("Buyer's Order No.:\nDated:", normal));
        //     rightTable.AddCell(CreateBox("Dispatch Doc No.:\nDelivery Note Date:", normal));
        //     rightTable.AddCell(CreateBox("Dispatched Through:\nDestination:", normal));
        //     rightTable.AddCell(CreateBox("Terms of Delivery:", normal));

        //     headerTable.AddCell(new PdfPCell(rightTable) { Border = Rectangle.BOX });
        //     contentTable.AddCell(new PdfPCell(headerTable) { Border = Rectangle.NO_BORDER });

        //     // === Consignee and Buyer ===
        //     var partyTable = new PdfPTable(2) { WidthPercentage = 100 };
        //     partyTable.SetWidths(new float[] { 50f, 50f });
        //     partyTable.AddCell(CreateBox("Consignee (Ship to)", bold));
        //     partyTable.AddCell(CreateBox("Buyer (Bill to)", bold));
        //     partyTable.AddCell(CreateBox(userName + "\nState Name : " + userState + ", State Code : " + gstCode, normal));
        //     partyTable.AddCell(CreateBox(userName + "\nState Name : " + userState + ", State Code : " + gstCode, normal));
        //     contentTable.AddCell(new PdfPCell(partyTable) { Border = Rectangle.NO_BORDER });

        //     // === Product Table ===
        //     var itemTable = new PdfPTable(9) { WidthPercentage = 100 };
        //     itemTable.SetWidths(new float[] { 5, 20, 10, 5, 10, 10, 10, 10, 10 });
        //     string[] headers = { "Sl No.", "Description of Services", "HSN/SAC", "Qty", "Rate", "Discount", "Gross Amt", "IGST", "Amount" };
        //     foreach (var h in headers)
        //         itemTable.AddCell(CreateHeaderCell(h, bold));

        //     itemTable.AddCell(CreateBox("1", normal));
        //     itemTable.AddCell(CreateBox(productName, normal));
        //     itemTable.AddCell(CreateBox("-", normal));
        //     itemTable.AddCell(CreateBox("1", normal));
        //     itemTable.AddCell(CreateBox(baseAmount.ToString("0.00"), normal));
        //     itemTable.AddCell(CreateBox("0.00", normal));
        //     itemTable.AddCell(CreateBox(baseAmount.ToString("0.00"), normal));
        //     itemTable.AddCell(CreateBox("18%", normal));
        //     itemTable.AddCell(CreateBox(amount.ToString("0.00"), normal));

        //     contentTable.AddCell(new PdfPCell(itemTable) { Border = Rectangle.NO_BORDER });

        //     // === Amount in Words ===
        //     contentTable.AddCell(CreateBox("Amount Chargeable (in words): INR " + NumberToWordsConverter.Convert(amount) + " Only", bold));

        //     // === Tax Table ===
        //     var taxTable = new PdfPTable(5) { WidthPercentage = 100 };
        //     taxTable.SetWidths(new float[] { 20, 20, 20, 20, 20 });
        //     taxTable.AddCell(CreateHeaderCell("HSN/SAC", bold));
        //     taxTable.AddCell(CreateHeaderCell("Taxable Value", bold));
        //     taxTable.AddCell(CreateHeaderCell("IGST Rate", bold));
        //     taxTable.AddCell(CreateHeaderCell("IGST Amount", bold));
        //     taxTable.AddCell(CreateHeaderCell("Total", bold));
        //     taxTable.AddCell(CreateBox("-", normal));
        //     taxTable.AddCell(CreateBox(baseAmount.ToString("0.00"), normal));
        //     taxTable.AddCell(CreateBox("18%", normal));
        //     taxTable.AddCell(CreateBox(taxAmount.ToString("0.00"), normal));
        //     taxTable.AddCell(CreateBox(taxAmount.ToString("0.00"), normal));
        //     contentTable.AddCell(new PdfPCell(taxTable) { Border = Rectangle.NO_BORDER });

        //     // === Total Summary ===
        //     contentTable.AddCell(new PdfPCell(new Phrase("Tax Amount (in words): INR " + NumberToWordsConverter.Convert(taxAmount) + " Only", bold)) { Border = Rectangle.NO_BORDER });

        //     var summaryTable = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT };
        //     summaryTable.SetWidths(new float[] { 60, 40 });
        //     summaryTable.AddCell(CreateBox("Base Amount", normal));
        //     summaryTable.AddCell(CreateBox(baseAmount.ToString("0.00"), normal));
        //     summaryTable.AddCell(CreateBox("Tax Amount", normal));
        //     summaryTable.AddCell(CreateBox(taxAmount.ToString("0.00"), normal));
        //     summaryTable.AddCell(CreateBox("Total Amount", bold));
        //     summaryTable.AddCell(CreateBox(amount.ToString("0.00"), bold));
        //     contentTable.AddCell(new PdfPCell(summaryTable) { Border = Rectangle.NO_BORDER });

        //     // === Declaration and Signature ===
        //     var footerTable = new PdfPTable(2) { WidthPercentage = 100 };
        //     footerTable.SetWidths(new float[] { 70, 30 });
        //     footerTable.AddCell(CreateBox("Declaration:\nWe declare that this invoice shows the actual price of the goods described and that all particulars are true and correct.", normal));
        //     footerTable.AddCell(CreateBox("for KING RESEARCH ACADEMY\n\n\nAuthorised Signatory", normal));
        //     contentTable.AddCell(new PdfPCell(footerTable) { Border = Rectangle.NO_BORDER });

        //     // === Footer Note ===
        //     contentTable.AddCell(new PdfPCell(new Phrase("This is a system-generated invoice and does not require a physical signature.", FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8)))
        //     {
        //         Border = Rectangle.NO_BORDER,
        //         HorizontalAlignment = Element.ALIGN_CENTER,
        //         Colspan = 2
        //     });

        //     borderWrapper.AddElement(contentTable);
        //     outerTable.AddCell(borderWrapper);

        //     var fullPageBorderCell = new PdfPCell(outerTable) { Border = Rectangle.BOX, Padding = 10f };
        //     fullPageBorderTable.AddCell(fullPageBorderCell);

        //     document.Add(fullPageBorderTable);
        //     document.Close();
        // }

        // private PdfPCell CreateBox(string text, Font font, int border = Rectangle.BOX, int alignment = Element.ALIGN_LEFT)
        // {
        //     return new PdfPCell(new Phrase(text, font))
        //     {
        //         Border = border,
        //         Padding = 5,
        //         HorizontalAlignment = alignment
        //     };
        // }

        // private PdfPCell CreateHeaderCell(string text, Font font)
        // {
        //     return new PdfPCell(new Phrase(text, font))
        //     {
        //         BackgroundColor = BaseColor.LIGHT_GRAY,
        //         Padding = 5,
        //         HorizontalAlignment = Element.ALIGN_CENTER
        //     };
        // }

        public static class NumberToWordsConverter
        {
            private static readonly string[] units = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
            private static readonly string[] teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            private static readonly string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            public static string Convert(decimal amount)
            {
                long rupees = (long)Math.Floor(amount);
                int paise = (int)Math.Round((amount - rupees) * 100);

                string result = "";

                if (rupees > 0)
                    result += Convert(rupees) + (rupees == 1 ? " rupee" : " rupees");

                if (paise > 0)
                {
                    if (rupees > 0)
                        result += " and ";
                    result += Convert(paise) + (paise == 1 ? " paisa" : " paise");
                }

                if (string.IsNullOrEmpty(result))
                    result = "zero rupees";

                return result.Trim();
            }

            public static string Convert(long number)
            {
                if (number == 0)
                    return "zero";

                if (number < 0)
                    return "minus " + Convert(-number);

                string words = "";

                if ((number / 10000000) > 0)
                {
                    words += Convert(number / 10000000) + " crore ";
                    number %= 10000000;
                }

                if ((number / 100000) > 0)
                {
                    words += Convert(number / 100000) + " lakh ";
                    number %= 100000;
                }

                if ((number / 1000) > 0)
                {
                    words += Convert(number / 1000) + " thousand ";
                    number %= 1000;
                }

                if ((number / 100) > 0)
                {
                    words += Convert(number / 100) + " hundred ";
                    number %= 100;
                }

                if (number > 0)
                {
                    if (!string.IsNullOrEmpty(words))
                        words += "and ";

                    if (number < 10)
                        words += units[number];
                    else if (number < 20)
                        words += teens[number - 10];
                    else
                    {
                        words += tens[number / 10];
                        if ((number % 10) > 0)
                            words += "-" + units[number % 10];
                    }
                }

                return words.Trim();
            }
        }

        public static class GstStateCodes
        {
            public static readonly Dictionary<string, string> StateCodeMap = new(StringComparer.OrdinalIgnoreCase)
            {
                { "Jammu and Kashmir", "01" },
                { "Himachal Pradesh", "02" },
                { "Punjab", "03" },
                { "Chandigarh", "04" },
                { "Uttarakhand", "05" },
                { "Haryana", "06" },
                { "Delhi", "07" },
                { "Rajasthan", "08" },
                { "Uttar Pradesh", "09" },
                { "Bihar", "10" },
                { "Sikkim", "11" },
                { "Arunachal Pradesh", "12" },
                { "Nagaland", "13" },
                { "Manipur", "14" },
                { "Mizoram", "15" },
                { "Tripura", "16" },
                { "Meghalaya", "17" },
                { "Assam", "18" },
                { "West Bengal", "19" },
                { "Jharkhand", "20" },
                { "Odisha", "21" },
                { "Chhattisgarh", "22" },
                { "Madhya Pradesh", "23" },
                { "Gujarat", "24" },
                { "Daman and Diu", "25" },
                { "Dadra and Nagar Haveli", "26" },
                { "Maharashtra", "27" },
                { "Andhra Pradesh", "28" },
                { "Karnataka", "29" },
                { "Goa", "30" },
                { "Lakshadweep", "31" },
                { "Kerala", "32" },
                { "Tamil Nadu", "33" },
                { "Puducherry", "34" },
                { "Andaman and Nicobar Islands", "35" },
                { "Telangana", "36" },
                { "Andhra Pradesh (New)", "37" },
            };
        }
    }
}