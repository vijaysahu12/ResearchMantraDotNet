using DocumentFormat.OpenXml.Bibliography;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.ResponseModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface IKycService
    {
        Task<string> GenerateAndSavePdfAsync(RpfForm form, IFormFile? signatureFrontFile, IFormFile signatureEndFile);
        Task<ApiCommonResponseModel> GetKycDetails(QueryValues query);
        Task<ApiCommonResponseModel> SubmitRpfAsync(RpfForm form, IFormFile signatureFront, IFormFile signatureEnd);
    }


    public class KycService : IKycService
    {
        private readonly KingResearchContext _context;
        private readonly ApiCommonResponseModel apiCommonResponse = new();

        public KycService(KingResearchContext context)
        {
            _context = context;
        }

        public async Task<ApiCommonResponseModel> GetKycDetails(QueryValues request)
        {
            try
            {
                SqlParameter parameterOutValue = new()
                {
                    ParameterName = "TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output,
                };


                List<SqlParameter> sqlParameters = new()
                {
                    new SqlParameter { ParameterName = "PageSize", Value = request.PageSize ,SqlDbType = SqlDbType.Int},
                    new SqlParameter { ParameterName = "PageNumber", Value = request.PageNumber  ,SqlDbType = SqlDbType.Int},
                    new SqlParameter { ParameterName = "poStatus",    Value = string.IsNullOrEmpty(request.PrimaryKey) ? DBNull.Value : request.PrimaryKey ,SqlDbType = SqlDbType.Int},
                    new SqlParameter { ParameterName = "FromDate", Value = request.FromDate == null ? DBNull.Value  : request.FromDate ,SqlDbType = SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "ToDate", Value = request.ToDate == null ? DBNull.Value : request.ToDate ,SqlDbType = SqlDbType.DateTime},
                    new SqlParameter { ParameterName = "SearchText", Value = request.SearchText == null ? DBNull.Value : request.SearchText ,SqlDbType = SqlDbType.VarChar, Size = 100},
                    parameterOutValue
                };


                apiCommonResponse.Data = await _context.SqlQueryToListAsync<KycResponseModel>(ProcedureCommonSqlParametersText.GetKycDetails, sqlParameters.ToArray());
                var TotalCount = Convert.ToInt32(parameterOutValue.Value);

                if (apiCommonResponse.Data is null)
                {
                    apiCommonResponse.Data = null;
                    apiCommonResponse.StatusCode = HttpStatusCode.OK;
                    apiCommonResponse.Message = "No Activity For This Search Found.";
                }
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Message = "Data Fetched Successfully.";
            }
            catch (Exception ex)
            {
                var dd = ex;
            }

            return apiCommonResponse;

        }

        public async Task<ApiCommonResponseModel> SubmitRpfAsync(RpfForm form, IFormFile? signatureFront, IFormFile signatureEnd)
        {
            try
            {

                if( _context.RpfForms.FirstOrDefault(x => x.Mobile == form.Mobile || x.Email.ToLower() == form.Email.ToLower()) != null)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "Your email or phone number is already there in a record. \nPlease try using another number or email"
                    };
                }
                
                var lead = _context.Leads.FirstOrDefault(x => x.MobileNumber == form.Mobile);

                if (lead == null)
                {
                    lead = new Lead
                    {
                        PublicKey = Guid.NewGuid(),
                        FullName = form.Name,
                        MobileNumber = form.Mobile,
                        EmailId = form.Email,
                        Remarks = "Added from Advisory form",
                        CreatedOn = DateTime.Now,
                        // Explicitly null or default for the rest
                        Gender = null,
                        CountryCode = null,
                        AlternateMobileNumber = null,
                        ProfileImage = null,
                        PriorityStatus = null,
                        AssignedTo = null,
                        ServiceKey = "625c783f-871a-ef11-b261-88b133b31c8f",
                        LeadTypeKey = null,
                        LeadSourceKey = null,
                        IsDisabled = null,
                        IsDelete = null,
                        CreatedBy = "bb74d26f-aa28-eb11-bee5-00155d53687a",
                        IsSpam = null,
                        IsWon = null,
                        ModifiedOn = DateTime.Now,
                        ModifiedBy = "bb74d26f-aa28-eb11-bee5-00155d53687a",
                        City = null,
                        PinCode = null,
                        StatusId = 22,
                        Favourite = null,
                        PurchaseOrderKey = null
                    };

                    _context.Leads.Add(lead);
                    await _context.SaveChangesAsync();
                }

                string pdfFileLocation = "";
                pdfFileLocation = await GenerateAndSavePdfAsync(form, signatureFront, signatureEnd);

                form.PdfFileLocation = pdfFileLocation;

                // Step 4: Save KYC form regardless
                _context.RpfForms.Add(form);
                await _context.SaveChangesAsync();

                // Step 5: Return response
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Risk Profile Form submitted successfully.",
                    Data = pdfFileLocation
                };
            }
            catch(Exception e)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Issues in Submitting Risk Profile Form"
                };
            }
        }
            

        public async Task<string> GenerateAndSavePdfAsync(RpfForm form, IFormFile signatureFrontFile, IFormFile signatureEndFile)
        {
            string sharedKycDirectory = @"D:\SharedFiles\CustomerRpfForms";
            if (!Directory.Exists(sharedKycDirectory)) Directory.CreateDirectory(sharedKycDirectory);

            string pdfFileName = $"kyc_{DateTime.Now:yyyyMMddHHmm}_{form.Mobile}.pdf";
            string fullPath = Path.Combine(sharedKycDirectory, pdfFileName);

            byte[] signatureFrontBytes = null, signatureEndBytes = null;
            if (signatureFrontFile != null)
            {
                using var ms = new MemoryStream();
                await signatureFrontFile.CopyToAsync(ms);
                signatureFrontBytes = ms.ToArray();
            }
            if (signatureEndFile != null)
            {
                using var ms = new MemoryStream();
                await signatureEndFile.CopyToAsync(ms);
                signatureEndBytes = ms.ToArray();
            }

            var headingFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD);
            var subHeadingFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD); // For "Contact Details: -" etc.
            var normalFont = new Font(Font.FontFamily.HELVETICA, 10);
            var smallItalicFont = new Font(Font.FontFamily.HELVETICA, 8, Font.ITALIC);
            var noteFont = new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD); // For "NOTE: -"

            Document doc = new Document(PageSize.A4, 50, 50, 50, 50);
            using var stream = new FileStream(fullPath, FileMode.Create);
            var writer = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            // --- Form Header ---
            doc.Add(new Paragraph("Risk Profile Form", new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD)) { Alignment = Element.ALIGN_CENTER });
            doc.Add(new Paragraph("\nAs per SEBI norms KYC is mandatory for all clients. After subscribing us, submit this KYC form. Without submitting this Agreement or KYC form your service will not start.", normalFont));
            doc.Add(new Paragraph("\n"));

            // --- Basic Info ---
            doc.Add(new Paragraph("Name of the Applicant: " + form.Name, normalFont));
            doc.Add(new Paragraph("\n")); // Space after Name

            // PAN and Aadhaar in a two-column layout using PdfPTable
            PdfPTable panAadhaarTable = new PdfPTable(2);
            panAadhaarTable.WidthPercentage = 100;
            panAadhaarTable.SetWidths(new float[] { 0.5f, 0.5f });
            panAadhaarTable.DefaultCell.Border = PdfPCell.NO_BORDER;
            panAadhaarTable.SpacingAfter = 10f; // Add space after the table

            panAadhaarTable.AddCell(new Phrase("PAN Number: " + form.Pan, normalFont));
            panAadhaarTable.AddCell(new Phrase("Aadhaar Number: " + form.Aadhaar, normalFont));
            doc.Add(panAadhaarTable);

            // --- Contact Details ---
            doc.Add(new Paragraph("Contact Details: -", subHeadingFont));
            doc.Add(new Paragraph("\n"));

            // Mobile and Email in a two-column layout
            PdfPTable contactTable = new PdfPTable(2);
            contactTable.WidthPercentage = 100;
            contactTable.SetWidths(new float[] { 0.5f, 0.5f });
            contactTable.DefaultCell.Border = PdfPCell.NO_BORDER;
            contactTable.SpacingAfter = 10f;

            contactTable.AddCell(new Phrase("Mobile Number: " + form.CountryCode + " " + form.Mobile, normalFont));
            contactTable.AddCell(new Phrase("Email ID: " + form.Email, normalFont));
            doc.Add(contactTable);

            doc.Add(new LineSeparator()); // Horizontal line

            // --- Declaration ---
            AddOptionsWithCheckboxes(doc, writer, [form.DeclarationConsent, ""], new[] {"YES"}, normalFont);
            doc.Add(new Paragraph("Declaration", subHeadingFont));
            doc.Add(new Paragraph("I hereby declare that, I'm above 18 years of age and  the details furnished above are true and correct to the best of my knowledge and belief and I undertake to inform you of any changes therein, immediately. In case any of the above information is found to be false or untrue or misleading or misrepresenting. I am aware that I may be held liable for it.", normalFont));
            doc.Add(new Paragraph("\n"));

            

            // --- Disclaimer & Terms ---
            doc.Add(new LineSeparator()); // Horizontal line
            doc.Add(new Paragraph("Disclaimer & Terms", subHeadingFont));
            var disclaimerList = new List();
            disclaimerList.Add(new ListItem("The RESEARCHMANTRA Advisor respects and values the right to privacy of each and every individual. By becoming a client, you have a promise from our side that we shall remain loyal to all our clients and non-clients whose information reside with us. The privacy policy of RESEARCHMANTRA applies to both current and former clients.", normalFont));
            disclaimerList.Add(new ListItem("Your information, whether public or private, will not be sold, rented, exchanged, transferred or given to any firm or individual for any reason without your consent.", normalFont));
            disclaimerList.Add(new ListItem("Your information will be used SOLELY for providing the services you have subscribed to, and not for any other purposes.", normalFont));
            disclaimerList.Add(new ListItem("The information you provide represents your identity with us. If any changes occur, you must notify us by phone or email.", normalFont));
            disclaimerList.Add(new ListItem("By filling out this form, you agree to comply with and be bound by the terms and conditions of use, along with those posted on the website www.researchmantra.in, which together with our privacy policy govern our relationship with you.", normalFont));
            disclaimerList.Add(new ListItem("By signing the form, you agree to provide a valid mobile number and consent to receive calls and SMS, even if registered on the National “DO NOT DISTURB” registry.", normalFont));
            disclaimerList.Add(new ListItem("RESEARCHMANTRA, as a merchant, shall not be liable for any loss or damage resulting from the decline of authorization for any transaction if the cardholder exceeded the limit set by our acquiring bank.", normalFont));
            doc.Add(disclaimerList);
            doc.Add(new Paragraph("\n"));
            doc.Add(new LineSeparator()); // Horizontal line

            // --- Questions ---
            var questions = new List<(string Number, string Text, string Answer, string[] Options)>
        {
            ("1.", "Would you invest where a small return is earned associated with small risk instead of a high return associated with high risk?", form.Q1, new[] { "Strongly Prefer", "Prefer", "Indifferent", "Do not Prefer", "Strongly do not prefer" }),
            ("2.", "When market is not performing well would like to invest in more risky investment instead of less risky investment to earn high returns?", form.Q2, new[] { "Strongly Prefer", "Prefer", "Indifferent", "Do not Prefer", "Strongly do not prefer" }),
            ("3.", "Very high risk is associated with very high returns, High risk is associated with high returns, medium risk is associated with medium returns? What risk can you bear (Not Prefer)?", form.Q3, new[] { "Very High", "High", "Medium" }),
            ("4.", "What is the duration of investment you are looking forward to keep invested?", form.Q4, new[] {
                "Short term positional (from 1 month to 6 month) [5]",
                "Long term positional (from 1 year) [0]",
                "Intraday [10]" }),
            ("5.", "In which of the following market segment have you traded previously? (Can select multiple if needed)", form.Q5, new[] { "Equity", "Derivative", "Commodity", "Forex", "All", "NA" }),
            ("6.", "What is your experience with Equity investment?", form.Q6, new[] { "Extensive experience", "Moderate experience", "Very less experience", "No experience" }),
            // Add other questions here similarly (Q7 to Q22)
            ("7.", "What is your experience with Commodity Investment?", form.Q7, new[] { "Extensive experience", "Moderate experience", "Very less experience", "No experience" }),
            ("8.", "What is your experience with Forex Investment?", form.Q8, new[] { "Extensive experience", "Moderate experience", "Very less experience", "No experience" }),
            ("9.", "What is your Age group?", form.Q9, new[] { "Under 35", "36-45", "46-55", "56-60", "60+" }),
            ("10.", "Investment Goal?", form.Q10, new[] { "Capital Appreciation [0]", "Regular Income [20]", "Capital Appreciation and Regular income [10]" }),
            ("11.", "Proposed Investment amount?", form.Q11, new[] { "less than 1 lakh", "1 - 2 lakhs", "2 - 5 lakhs", "More than 5 lakhs" }),
            ("12.", "Gross Annual Income details?", form.Q12, new[] { "Below 1 Lakh", "1-5 Lakh", "5-10 lakh", "10-25 lakh", "25 Lakh" }),
            ("13.", "Sources of Income, Primary income?", form.Q13, new[] { "Salary", "Business", "None" }),
            ("14.", "Secondary Sources? (Can select multiple if needed.)", form.Q14, new[] { "Royalties", "Rental", "Dividend", "Other specify" }),
            ("15.", "Market Value of portfolio held?", form.Q15, new[] { "less than 1 lakh", "1 - 2 lakhs", "2 - 5 lakhs", "More than 5 lakhs" }),
            ("16.", "Investment experience?", form.Q16, new[] { "Less than 3 years", "3 - 5 years", "More than 5 years" }),
            ("17.", "How many dependents do you financially supports?", form.Q17, new[] { "None", "1 - 3", "4+" }),
            ("18.", "What is the size of your Emergency Fund?", form.Q18, new[] { "Less than 1-month income", "1-3-month income", "3-6-month income", "More than 6-month income", "Do Not Have" }),
            ("19.", "What is your experience with investment in past?", form.Q19, new[] { "Very good [20]", "Good [15]", "Moderate [10]", "Bad [5]", "Very bad [0]" }),
            ("20.", "What percentage of monthly income is Allocated to pay off debt [ All EMI's]?", form.Q20, new[] { "None [20]", "Between 0- 20% [15]", "Between 20 - 35% [10]", "Between 35 - 50% [5]", "More than 50 % [0]" }),
            ("21.", "Occupation (Please select the appropriate)?", form.Q21, new[] { "Private sector", "Public sector", "Government sector", "Business", "Professional", "Agriculturalist", "Retired", "House Wife/Husband", "Student", "Dealer", "Self-Employee", "Others" }),
            ("22.", "Are you any of the following, or are directly or indirectly related to any of the following?", form.Q22, new[] { "Civil Servant", "Politician", "Current or former Head of State", "Bureaucrat (Tax Authorities, Foreign Services, IAS etc.)", "Current or Former MP/MLA/MLC", "Connected to Media", "Connected to any Company/ Promotor Group/ Group of companies listed on any stock exchange", "None" })
        };

            foreach (var q in questions)
            {
                AddQuestion(doc, writer, q.Number, q.Text, q.Answer, q.Options, normalFont);
            }

            // --- Additional Consent Questions ---
            doc.Add(new Paragraph("\n")); // Add some space

            // Risk Consent Question
            doc.Add(new Paragraph("Above mentioned services fall into a risk category chosen by you. If you agree to the above then click “YES” If you do not agree then please click “NO”.", normalFont));
            AddOptionsWithCheckboxes(doc, writer, [form.RiskConsent, ""], new[] { "YES", "NO" }, normalFont);
            doc.Add(new Paragraph("\n"));

            // Telephonic Support Consent Question
            doc.Add(new Paragraph("NOTE: -", noteFont)); // "NOTE: -" heading
            doc.Add(new Paragraph("If you are willing to get Telephonic Support instead of SMS of our services then all the responsibility of loss or profit whether big or small, through holding or intraday market, will only be yours or not of the firm or any person associated with the firm. If you agree to the then click “YES”. If you do not agree then please click “No”.", normalFont));
            AddOptionsWithCheckboxes(doc, writer, [form.SupportConsent, ""], new[] { "YES", "NO" }, normalFont);
            doc.Add(new Paragraph("\n"));

            // --- Investment Risk Profile Section ---
            doc.Add(new Paragraph("Investment Risk Profile", headingFont));
            doc.Add(new Paragraph("In setting up an investment portfolio suitable for you, your financial adviser will ask you a series of questions about your financial and lifestyle goals. Using this information, plus details of current assets, liabilities and income, your adviser can determine what level of risk or exposure you are prepared to tolerate in relation to fluctuation in the market place - and the level that makes sense for your stage in life. From this an appropriate mix of assets can be allocated to your investment portfolio.", normalFont));
            doc.Add(new Paragraph("\n"));

            // Create the table for Investment Risk Profile
            PdfPTable riskProfileTable = new PdfPTable(2); // 2 columns
            riskProfileTable.WidthPercentage = 100; // Table occupies 100% of the page width
            riskProfileTable.SetWidths(new float[] { 0.30f, 0.70f }); // Column widths: 30% for first, 70% for second

            // Table Header
            PdfPCell headerCell1 = new PdfPCell(new Phrase("YOUR TOTAL SCORE", normalFont));
            headerCell1.BackgroundColor = new BaseColor(242, 242, 242); // #f2f2f2
            headerCell1.Padding = 5;
            riskProfileTable.AddCell(headerCell1);

            PdfPCell headerCell2 = new PdfPCell(new Phrase("YOUR INVESTMENT PORTFOLIO", normalFont));
            headerCell2.BackgroundColor = new BaseColor(242, 242, 242); // #f2f2f2
            headerCell2.Padding = 5;
            riskProfileTable.AddCell(headerCell2);

            // Table Rows
            riskProfileTable.AddCell(new PdfPCell(new Phrase("Submit", normalFont)) { Padding = 5 });
            riskProfileTable.AddCell(new PdfPCell(new Phrase("Your investment portfolio should have bias towards high capital growth and you have little need for income. You are prepared to accept more higher degree of volatility and risk. Your primary concern is to accumulate assets over medium to long term.", normalFont)) { Padding = 5 });

            riskProfileTable.AddCell(new PdfPCell(new Phrase("HIGH GROWTH INVESTORS [161- 200]", normalFont)) { Padding = 5 });
            riskProfileTable.AddCell(new PdfPCell(new Phrase("Your investment portfolio should have focus on investment growth with some need for income. You are ready to take calculated risk to achieve better returns.", normalFont)) { Padding = 5 });

            riskProfileTable.AddCell(new PdfPCell(new Phrase("MEDIUM GROWTH INVESTORS [81- 160]", normalFont)) { Padding = 5 });
            riskProfileTable.AddCell(new PdfPCell(new Phrase("You prefer capital preservation over high returns. You are risk-averse and favor stable, income-generating investments with minimal market fluctuation.", normalFont)) { Padding = 5 });

            doc.Add(riskProfileTable); // Add the table to the document
            doc.Add(new Paragraph("\n")); // Add some space after the table
            AddOptionsWithCheckboxes(doc, writer, [form.AdviserConsent, ""], new[] { "YES" }, normalFont);
            doc.Add(new Paragraph("As your ADVISER, “I” and ‘YOU” confirm that we have worked together to determine your risk profile and the investment options that are best suited on your investment style.", normalFont));
            // --- Final Signature ---
            doc.Add(new Paragraph("Please upload your Signature in .jpeg or .png or .heif format.(The size should be under 10MB)", normalFont));
            doc.Add(new Paragraph("\nSignature:", headingFont));
            if (signatureEndBytes != null)
            {
                try
                {
                    var img = iTextSharp.text.Image.GetInstance(signatureEndBytes);
                    img.ScaleToFit(150f, 75f);
                    img.Alignment = Element.ALIGN_CENTER;
                    doc.Add(img);
                    doc.Add(new Paragraph("(Final Signature)", normalFont) { Alignment = Element.ALIGN_CENTER });
                }
                catch (Exception ex)
                {
                    doc.Add(new Paragraph("Error loading final signature image: " + ex.Message, smallItalicFont));
                }
            }
            else
            {
                doc.Add(new Paragraph("(Final Signature - Not Provided)", smallItalicFont));
            }
            doc.Add(new Paragraph("\nSubmitted On: " + form.CreatedOn?.ToLocalTime().ToString("f") + " IST", smallItalicFont)
            {
                Alignment = Element.ALIGN_RIGHT
            });

            doc.Close();
            return fullPath;
        }

        private void AddQuestion(Document doc, PdfWriter writer, string number, string text, string selectedAnswer, string[] options, Font font)
        {
            // Add the question text
            doc.Add(new Paragraph($"{number} {text}", font));

            // Handle Q5 (multiple selection) differently if needed
            if (number == "5.")
            {
                // For Q5, selectedAnswer will be a comma-separated string, e.g., "Equity,Derivative"
                // Split it into individual selections
                string[] selectedOptions = string.IsNullOrEmpty(selectedAnswer) ? new string[0] : selectedAnswer.Split(',');
                AddOptionsWithCheckboxes(doc, writer, selectedOptions, options, font);
            }
            else if (number == "14.")
            {
                string[] selectedOptions = string.IsNullOrEmpty(selectedAnswer) ? new string[0] : selectedAnswer.Split(',');

                // Define the base predefined options
                List<string> optionsList = new List<string> { "Royalties", "Rental", "Dividend", "Other specify" };

                // Add any extra options not in base list
                foreach (var sel in selectedOptions)
                {
                    var trimmed = sel.Trim();
                    if (!optionsList.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
                    {
                        optionsList.Add(trimmed);
                    }
                }

                AddOptionsWithCheckboxes(doc, writer, selectedOptions, optionsList.ToArray(), font);
            }
            else
            {
                // For single-select questions, pass the single selected answer in an array
                AddOptionsWithCheckboxes(doc, writer, new string[] { selectedAnswer }, options, font);
            }

            // Add a small space after all options for the question
            doc.Add(new Paragraph("\n"));
        }

        // Overload for single selected answer (most radio buttons)
        private void AddOptionsWithCheckboxes(Document doc, PdfWriter writer, string[] selectedAnswers, string[] options, Font font)
        {
            PdfContentByte cb = writer.DirectContent;
            float boxSize = 8f;
            float textIndent = 15f;

            foreach (var option in options)
            {
                Phrase optionPhrase = new Phrase(option, font);
                Paragraph optionParagraph = new Paragraph(optionPhrase);
                optionParagraph.IndentationLeft = textIndent;

                doc.Add(optionParagraph);

                float y = writer.GetVerticalPosition(true);
                float checkboxX = doc.LeftMargin;
                float checkboxY = y + (font.Size / 2) - (boxSize / 2); // Vertically centers the box with the text

                cb.SaveState();
                cb.SetColorStroke(BaseColor.BLACK);
                cb.SetLineWidth(0.5f);

                cb.Rectangle(checkboxX, checkboxY, boxSize, boxSize);
                cb.Stroke();

                // Check if the current option is in the list of selected answers
                if (selectedAnswers != null && Array.Exists(selectedAnswers, s => s.Trim().Equals(option.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    // Tick Mark Coordinates
                    float tickStartX = checkboxX + boxSize * 0.2f;  // Bottom-left inside box
                    float tickStartY = checkboxY + boxSize * 0.4f;

                    float tickMidX = checkboxX + boxSize * 0.4f;    // Middle
                    float tickMidY = checkboxY + boxSize * 0.2f;

                    float tickEndX = checkboxX + boxSize * 0.75f;   // Upper right
                    float tickEndY = checkboxY + boxSize * 0.8f;
                    cb.MoveTo(tickStartX, tickStartY);  // Start at bottom-left
                    cb.LineTo(tickMidX, tickMidY);      // Go up slightly left
                    cb.LineTo(tickEndX, tickEndY);      // Finish upward right
                    cb.Stroke();
                }
                cb.RestoreState();
            }
        }
    }

}