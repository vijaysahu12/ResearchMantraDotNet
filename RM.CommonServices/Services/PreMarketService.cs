
using RM.ChatGPT;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.RequestModel.Notification;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static RM.Database.MongoDbContext.PreMarketReport;

namespace RM.CommonServices.Services
{
    public class PreAndPostMarketService
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<PreMarketReport.PreMarketCollection> _collection;
        private readonly IMongoCollection<PostMarketReport.PostMarketCollection> _postCollection;
        private readonly StockInsightService _insightService;
        private readonly IMobileNotificationService _notificationService;

        public PreAndPostMarketService(IOptions<MongoDBSettings> mongoDBSettings, StockInsightService insightService, IMobileNotificationService notificationService)
        {
            var client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _collection = _database.GetCollection<PreMarketReport.PreMarketCollection>("PreMarketData");
            _postCollection = _database.GetCollection<PostMarketReport.PostMarketCollection>("PostMarketData");
            _insightService = insightService;
            _notificationService = notificationService;
        }

        public async Task<ApiCommonResponseModel> GetPaginatedMarketDataAsync(int pageNumber, int pageSize = 10)
        {
            var apiCommon = new ApiCommonResponseModel();
            try
            {
                // Step 1: Fetch distinct dates
                var distinctDatesCursor = await _collection.DistinctAsync<DateTime>(
                    field: "CreatedOn",
                    filter: FilterDefinition<PreMarketReport.PreMarketCollection>.Empty);

                // Enumerate the cursor to get the list of distinct dates
                var distinctDates = await distinctDatesCursor.ToListAsync();

                // Normalize the distinct dates to only include the date part (e.g., YYYY-MM-DD)
                var uniqueDates = distinctDates.Select(date => date.Date).Distinct();

                // Step 2: Fetch the latest record for each distinct date
                var latestRecords = new List<PreMarketReport.PreMarketCollection>();

                foreach (var date in uniqueDates)
                {
                    // Filter to find records for the specific date range
                    var filter = Builders<PreMarketReport.PreMarketCollection>.Filter.And(
                        Builders<PreMarketReport.PreMarketCollection>.Filter.Gte(x => x.CreatedOn, date.ToString("yyyy-MM-dd")),
                        Builders<PreMarketReport.PreMarketCollection>.Filter.Lt(x => x.CreatedOn, date.AddDays(1).ToString("yyyy-MM-dd"))
                    );

                    // Get the latest record for the date
                    var latestRecord = await _collection.Find(filter)
                                                        .SortByDescending(x => x.CreatedOn)
                                                        .FirstOrDefaultAsync();

                    if (latestRecord != null)
                    {
                        latestRecords.Add(latestRecord);
                    }
                }

                // Step 3: Map the data to the response model
                var summaries = latestRecords.Select(data => new PreMarketReport.PreMarketSummaryResponseModel
                {
                    Id = data.Id,
                    CreatedOn = Convert.ToDateTime(data.CreatedOn),
                    Nifty = data.IndicatorSupport.FirstOrDefault(x => string.Equals(x.Name, "Nifty", StringComparison.OrdinalIgnoreCase))?.Close.ToString("F2") ?? "N/A",
                    BNF = data.IndicatorSupport.FirstOrDefault(x => string.Equals(x.Name, "Banknifty", StringComparison.OrdinalIgnoreCase))?.Close.ToString("F2") ?? "N/A",
                    Vix = data.IndiaVix?.Close.ToString("F2") ?? "N/A",
                    HotNews = data.NewsBulletins?.Bulletins?.FirstOrDefault() ?? "No news available",
                    Diis = data.FiiDiiData.FirstOrDefault(x => x.Name == "DII")?.NetValue ?? "N/A",
                    Fiis = data.FiiDiiData.FirstOrDefault(x => x.Name == "FII")?.NetValue ?? "N/A",
                    Oi = data.SupportResistance?.Pivot.ToString("F2") ?? "N/A",
                    ButtonText = "Read More"

                })
                .OrderByDescending(x => x.CreatedOn)
                .Skip((pageNumber - 1) * pageSize)   // Apply pagination
                .Take(pageSize)
                .ToList();


                apiCommon.Data = summaries;
                apiCommon.Total = summaries.Count;
                apiCommon.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                apiCommon.Data = null;
                apiCommon.StatusCode = HttpStatusCode.InternalServerError;
                apiCommon.Message = $"An error occurred: {ex.Message}\n{ex.StackTrace}";
            }
            return apiCommon;
        }

        public async Task<PreMarketReport.PreMarketCollection> GetPreMarketDataByDateAsync(DateTime date)
        {
            var filter = Builders<PreMarketReport.PreMarketCollection>.Filter.Eq("CreatedOn", date.Date.ToString("yyyy-MM-dd"));
            var marketData = await _collection.Find(filter).FirstOrDefaultAsync();

            if (marketData != null)
            {
                Console.WriteLine("Market data retrieved successfully.");
            }
            else
            {
                Console.WriteLine("No market data found for the specified date.");
            }

            return marketData;
        }

        public async Task<ApiCommonResponseModel> InsertMarketDataAsync(PreMarketCollection marketDataNew)
        {
            var response = new ApiCommonResponseModel();

            var validationContext = new ValidationContext(marketDataNew);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(marketDataNew, validationContext, results, true);

            if (!isValid)
            {
                var errors = results.Select(r => r.ErrorMessage).ToList();
                response.Data = errors;
                response.Message = "Validation failed";

                await _notificationService.SendNotificationToMobile(new Model.RequestModel.Notification.NotificationToMobileRequestModel
                {
                    Body = "Not able to upate beacuse the Pre Market Report Data is not valid",
                    Mobile = "9411122233,8885417635,7000863437",
                    ScreenName = "",
                    Title = "Pre-Market Invalid Data",
                    Topic = "ANNOUNCEMENT"
                });

                return response;
            }

            // then proceed to insert logic as before...

            try
            {
                var insights = await _insightService.GetDailyStockInsightsAsync();
                if (insights?.Any() == true)
                {
                    marketDataNew.NewsBulletins.Bulletins = insights;
                }

                await _collection.InsertOneAsync(marketDataNew);

                await _notificationService.SendNotificationToMobile(new Model.RequestModel.Notification.NotificationToMobileRequestModel
                {
                    Body = "Pre Market Report Updated Successfully",
                    Mobile = "9411122233,8885417635,7000863437",
                    ScreenName = "",
                    Title = "Pre-Market Success",
                    Topic = "ANNOUNCEMENT"
                });

                response.Data = true;
                response.Message = "Import PreMarketData was successful.";
            }
            catch (Exception ex)
            {
                // Optional: Log the exception
                response.Data = false;
                response.Message = "Failed to import PreMarketData.";
                //response.Errors = new List<string> { ex.Message };

                // Try sending failure notification
                try
                {
                    await _notificationService.SendNotificationToMobile(new Model.RequestModel.Notification.NotificationToMobileRequestModel
                    {
                        Body = "Pre Market Report update failed",
                        Mobile = "9411122233,8885417635,7000863437",
                        ScreenName = "",
                        Title = "Pre-Market Failed",
                        Topic = "ANNOUNCEMENT"
                    });
                }
                catch (Exception notifyEx)
                {
                    // Optionally log notification failure
                    response.Data = ("Notification failed: " + notifyEx.Message);
                }
            }

            return response;
        }

        public async Task<ApiCommonResponseModel> GetPreMarketDataByObjectIdAsync(string objectId)
        {
            var apiCommon = new ApiCommonResponseModel();

            if (!ObjectId.TryParse(objectId, out var objectIdParsed))
            {
                Console.WriteLine("Invalid ObjectId format.");
                return null;
            }

            var filter = Builders<PreMarketReport.PreMarketCollection>.Filter.Eq("_id", objectIdParsed);
            var marketData = await _collection.Find(filter).FirstOrDefaultAsync();

            if (marketData != null)
            {
                Console.WriteLine("Market data retrieved successfully.");
            }
            else
            {
                Console.WriteLine("No market data found for the specified date.");
            }
            apiCommon.Data = marketData;
            apiCommon.StatusCode = HttpStatusCode.OK;

            return apiCommon;
        }

        // public async Task GeneratePreMarketPulsePdfAsync(DateTime date, string outputFilePath)
        // {
        //     var filter = Builders<PreMarketReport.PreMarketCollection>.Filter.Empty;
        //     var sort = Builders<PreMarketReport.PreMarketCollection>.Sort.Descending("CreatedOn");
        //     var marketData = await _collection.Find(filter).Sort(sort).Limit(1).FirstOrDefaultAsync();

        //     if (marketData == null)
        //     {
        //         Console.WriteLine("No market data found for the specified date.");
        //         return;
        //     }

        //     using (var stream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        //     {
        //         var document = new Document();
        //         var writer = PdfWriter.GetInstance(document, stream);
        //         document.Open();

        //         var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
        //         document.Add(new Paragraph("Pre-Market King Research Analysis", titleFont));
        //         document.Add(new Paragraph($"Release Date: {DateTime.Now:ddd, dd MMM yyyy}", titleFont));
        //         document.Add(new Paragraph("\n"));

        //         document.Add(CreateDataTable("Global Indices", marketData.GlobalIndices.Data, writer));
        //         document.Add(new Paragraph("\n"));

        //         document.Add(CreateDataTable("Indian Indices", marketData.IndianIndices.Data, writer));
        //         document.Add(new Paragraph("\n"));

        //         document.Add(CreateCommoditiesTable(marketData.Commodities));
        //         document.Add(new Paragraph(" "));

        //         document.Add(CreateFiiDiiDataTable(marketData.FiiDiiData, writer));
        //         document.Add(new Paragraph(" "));
        //         document.Add(new Paragraph(" "));
        //         document.Add(new Paragraph(" "));

        //         document.Add(CreateMarketBulletinsTable(marketData.NewsBulletins.Bulletins));
        //         document.Add(new Paragraph("\n"));

        //         document.Add(new Paragraph("Technical Analysis", titleFont));
        //         document.Add(new Paragraph("Nifty50:", titleFont));
        //         document.Add(new Paragraph("• RSI indicator shows a bearish crossover, supporting negative sentiment.", titleFont));
        //         document.Add(new Paragraph("• Nifty closed at a five-month low of 23,349.90.", titleFont));
        //         document.Add(new Paragraph("\n"));

        //         document.Add(CreateFiiDiiDataTable(marketData.FiiDiiData, writer));
        //         document.Add(new Paragraph("\n"));

        //         var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, BaseColor.GRAY);
        //         document.Add(new Paragraph("www.premarketpulse.com | Contact: +91 8618318166", footerFont));

        //         AddWatermark(writer, "King Research Academy");

        //         document.Close();
        //         Console.WriteLine($"PDF generated successfully at {outputFilePath}");
        //     }
        // }

        // private PdfPTable CreateDataTable(string title, List<PreMarketReport.IndexData> indices, PdfWriter writer)
        // {
        //     var table = new PdfPTable(4) { WidthPercentage = 100 };
        //     table.SetWidths(new float[] { 40f, 20f, 20f, 20f });

        //     var mailTitleHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, Font.BOLD, BaseColor.BLUE);
        //     var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
        //     var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

        //     var titleCell = new PdfPCell(new Phrase(title, mailTitleHeaderFont))
        //     {
        //         Colspan = 4,
        //         BackgroundColor = BaseColor.WHITE,
        //         HorizontalAlignment = Element.ALIGN_LEFT,
        //         Padding = 2,
        //         PaddingBottom = 10,
        //         Border = Rectangle.BOTTOM_BORDER,
        //         BorderColor = BaseColor.BLACK,
        //         BorderWidthBottom = 2
        //     };
        //     table.AddCell(titleCell);

        //     table.AddCell(new PdfPCell(new Phrase("Name", headerFont)) { BackgroundColor = BaseColor.GRAY });
        //     table.AddCell(new PdfPCell(new Phrase("LTP", headerFont)) { BackgroundColor = BaseColor.GRAY });
        //     table.AddCell(new PdfPCell(new Phrase("Change", headerFont)) { BackgroundColor = BaseColor.GRAY });
        //     table.AddCell(new PdfPCell(new Phrase("Change %", headerFont)) { BackgroundColor = BaseColor.GRAY });

        //     foreach (var indexData in indices)
        //     {
        //         table.AddCell(new PdfPCell(new Phrase(indexData.Name, dataFont)));
        //         table.AddCell(new PdfPCell(new Phrase(indexData.Close.ToString("0.00"), dataFont)));

        //         table.AddCell(new PdfPCell(new Phrase(indexData.ChangePercentage.ToString("0.00") + "%", dataFont))
        //         {
        //             BackgroundColor = indexData.ChangePercentage < 0 ? BaseColor.RED : BaseColor.GREEN
        //         });
        //     }

        //     return table;
        // }

        // private PdfPTable CreateFiiDiiDataTable(List<PreMarketReport.FiiDiiData> fiiDiiData, PdfWriter writer)
        // {
        //     var table = new PdfPTable(3) { WidthPercentage = 100 };
        //     table.SetWidths(new float[] { 40f, 30f, 30f });

        //     var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
        //     var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

        //     table.AddCell(new PdfPCell(new Phrase("Name", headerFont)) { BackgroundColor = BaseColor.GRAY });
        //     table.AddCell(new PdfPCell(new Phrase("Net Value", headerFont)) { BackgroundColor = BaseColor.GRAY });

        //     foreach (var fiiDii in fiiDiiData)
        //     {
        //         table.AddCell(new PdfPCell(new Phrase(fiiDii.Name, dataFont)));
        //         table.AddCell(new PdfPCell(new Phrase(fiiDii.NetValue, dataFont)));
        //     }

        //     return table;
        // }

        // private void AddWatermark(PdfWriter writer, string watermarkText)
        // {
        //     var content = writer.DirectContentUnder;
        //     var font = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.LIGHT_GRAY);
        //     var phrase = new Phrase(watermarkText, font);

        //     var pageSize = writer.PageSize;
        //     float xStep = 150f;
        //     float yStep = 150f;

        //     for (float x = 0; x < pageSize.Width; x += xStep)
        //     {
        //         for (float y = 0; y < pageSize.Height; y += yStep)
        //         {
        //             ColumnText.ShowTextAligned(content, Element.ALIGN_CENTER, phrase, x, y, 30);
        //         }
        //     }
        // }

        // private PdfPTable CreateCommoditiesTable(PreMarketReport.Commodities commodities)
        // {
        //     var table = new PdfPTable(4) { WidthPercentage = 100 };
        //     var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
        //     var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

        //     var titleCell = new PdfPCell(new Phrase("Commodities", headerFont))
        //     {
        //         Colspan = 4,
        //         BackgroundColor = BaseColor.GRAY,
        //         HorizontalAlignment = Element.ALIGN_CENTER
        //     };
        //     table.AddCell(titleCell);

        //     table.AddCell(new PdfPCell(new Phrase("Name", headerFont)) { BackgroundColor = BaseColor.GRAY });
        //     table.AddCell(new PdfPCell(new Phrase("LTP", headerFont)) { BackgroundColor = BaseColor.GRAY });
        //     table.AddCell(new PdfPCell(new Phrase("Change", headerFont)) { BackgroundColor = BaseColor.GRAY });
        //     table.AddCell(new PdfPCell(new Phrase("Change %", headerFont)) { BackgroundColor = BaseColor.GRAY });

        //     AddCommodityRow(table, "Gold", commodities.Commodity.GOLD, dataFont);
        //     AddCommodityRow(table, "Silver", commodities.Commodity.SILVER, dataFont);
        //     AddCommodityRow(table, "Crude Oil", commodities.Commodity.CRUDEOIL, dataFont);

        //     return table;
        // }

        // private void AddCommodityRow(PdfPTable table, string name, PreMarketReport.CommodityData commodity, Font dataFont)
        // {
        //     table.AddCell(new PdfPCell(new Phrase(name, dataFont)));
        //     if (commodity != null)
        //     {

        //         table.AddCell(new PdfPCell(new Phrase(commodity.ChangePercentage.ToString("0.00") + "%", dataFont))
        //         {
        //             BackgroundColor = commodity.ChangePercentage < 0 ? BaseColor.RED : BaseColor.GREEN
        //         });
        //     }
        // }
        // private PdfPTable CreateMarketBulletinsTable(List<string> bulletins)
        // {
        //     var table = new PdfPTable(2) { WidthPercentage = 100 };
        //     var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
        //     var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

        //     table.SetWidths(new float[] { 5f, 95f });

        //     var titleCell = new PdfPCell(new Phrase("Market Bulletins", headerFont))
        //     {
        //         Colspan = 2,
        //         BackgroundColor = BaseColor.GRAY,
        //         HorizontalAlignment = Element.ALIGN_CENTER,
        //         Padding = 8,
        //         PaddingTop = 10,
        //         PaddingBottom = 10
        //     };
        //     table.AddCell(titleCell);

        //     for (int i = 0; i < bulletins.Count; i++)
        //     {
        //         var prefix = (i + 1).ToString();
        //         table.AddCell(new PdfPCell(new Phrase(prefix, dataFont))
        //         {
        //             HorizontalAlignment = Element.ALIGN_RIGHT,
        //             Padding = 5
        //         });

        //         table.AddCell(new PdfPCell(new Phrase(bulletins[i], dataFont))
        //         {
        //             HorizontalAlignment = Element.ALIGN_LEFT,
        //             Padding = 5
        //         });
        //     }

        //     return table;
        // }

        public async Task<ApiCommonResponseModel> ManagePostMarketData(PostMarketReport.PostMarketCollection postMarket)
        {
            var response = new ApiCommonResponseModel();

            // Validate the model manually
            var validationContext = new ValidationContext(postMarket);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(postMarket, validationContext, results, true);

            if (!isValid)
            {
                var errors = results.Select(r => r.ErrorMessage).ToList();
                response.Data = errors;
                response.Message = "Validation failed.";

                response.StatusCode = HttpStatusCode.BadRequest;



                await _notificationService.SendNotificationToMobile(new NotificationToMobileRequestModel
                {
                    Body = "Post Market Report update failed due to invalid data.",
                    Mobile = "9411122233,8885417635,7000863437",
                    ScreenName = "",
                    Title = "Post-Market Invalid Data",
                    Topic = "ANNOUNCEMENT"
                });

                return response;
            }

            try
            {
                var formattedDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                postMarket.CreatedOn = formattedDateTime;

                await _postCollection.InsertOneAsync(postMarket);

                await _notificationService.SendNotificationToMobile(new NotificationToMobileRequestModel
                {
                    Body = "Post Market Report updated successfully.",
                    Mobile = "9411122233,8885417635,7000863437",
                    ScreenName = "",
                    Title = "Post-Market Success",
                    Topic = "ANNOUNCEMENT"
                });
                response.StatusCode = HttpStatusCode.OK;
                response.Data = true;
                response.Message = "Post Market data inserted successfully.";
            }
            catch (Exception ex)
            {
                response.Data = ex.Message;
                response.Message = "Failed to insert Post Market data.";
                response.StatusCode = HttpStatusCode.InternalServerError;
                //response.Errors = new List<string> { ex.Message };

                try
                {
                    await _notificationService.SendNotificationToMobile(new NotificationToMobileRequestModel
                    {
                        Body = "Post Market Report update failed.",
                        Mobile = "9411122233,8885417635,7000863437",
                        ScreenName = "",
                        Title = "Post-Market Failure",
                        Topic = "ANNOUNCEMENT"
                    });
                }
                catch (Exception notifyEx)
                {
                    response.Message = "Not able to send push notification to users ";
                    response.Data = "Notification failed: " + notifyEx.Message;
                    response.StatusCode = HttpStatusCode.InternalServerError;
                }
            }

            return response;
        }


        public async Task<ApiCommonResponseModel> GetPostMarketDataByObjectIdAsync(string objectId)
        {
            var apiCommon = new ApiCommonResponseModel();

            if (!ObjectId.TryParse(objectId, out var objectIdParsed))
            {
                Console.WriteLine("Invalid ObjectId format.");
                return null;
            }

            var filter = Builders<PostMarketReport.PostMarketCollection>.Filter.Eq("_id", objectIdParsed);
            var marketData = await _postCollection.Find(filter).FirstOrDefaultAsync();

            apiCommon.Data = marketData;
            apiCommon.StatusCode = HttpStatusCode.OK;

            return apiCommon;
        }

        public async Task<ApiCommonResponseModel> GetPaginatedPostMarketDataAsync(int pageNumber, int pageSize = 10)
        {
            var apiCommon = new ApiCommonResponseModel();
            try
            {
                // Step 1: Fetch distinct dates while filtering out null CreatedOn
                var validRecordsFilter = Builders<PostMarketReport.PostMarketCollection>.Filter.Exists("CreatedOn", true);

                var distinctDatesCursor = await _postCollection.DistinctAsync<DateTime?>(
                    field: "CreatedOn",
                    filter: validRecordsFilter);

                // Convert nullable dates to a list and exclude null values
                var distinctDates = (await distinctDatesCursor.ToListAsync())
                    .Where(date => date.HasValue)
                    .Select(date => date.Value.Date)
                    .Distinct()
                    .ToList();

                // Step 2: Fetch the latest record for each distinct date
                var latestRecords = new List<PostMarketReport.PostMarketCollection>();

                foreach (var date in distinctDates)
                {
                    // Filter to find records for the specific date range
                    var dateFilter = Builders<PostMarketReport.PostMarketCollection>.Filter.And(
                    validRecordsFilter,
                        Builders<PostMarketReport.PostMarketCollection>.Filter.Gte(x => x.CreatedOn, date.ToString("yyyy-MM-dd")),
                        Builders<PostMarketReport.PostMarketCollection>.Filter.Lt(x => x.CreatedOn, date.AddDays(1).ToString("yyyy-MM-dd"))
                    );

                    // Get the latest record for the date
                    var latestRecord = await _postCollection.Find(dateFilter)
                                                            .SortByDescending(x => x.CreatedOn)
                                                            .FirstOrDefaultAsync();

                    if (latestRecord != null)
                    {
                        latestRecords.Add(latestRecord);
                    }
                }

                // Step 3: Paginate the results
                var paginatedRecords = latestRecords
                    .OrderByDescending(x => x.CreatedOn) // Order by CreatedOn descending
                    .Skip((pageNumber - 1) * pageSize)   // Apply pagination
                    .Take(pageSize)                      // Take the specified number of records
                    .Select(p => new PostMarketReport.PostMarketSummaryResponseModel
                    {
                        Id = p.Id,
                        CreatedOn = p.CreatedOn,
                        BestPerformer = p.BestPerformer.FirstOrDefault(),
                        WorstPerformer = p.WorstPerformer.FirstOrDefault(),
                        VolumeShocker = p.VolumeShocker
                    })
                    .ToList();

                // Step 4: Populate the response model
                apiCommon.Data = paginatedRecords;
                apiCommon.Total = distinctDates.Count(); // Total distinct dates
                apiCommon.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                apiCommon.Data = null;
                apiCommon.StatusCode = HttpStatusCode.InternalServerError;
                apiCommon.Message = $"An error occurred: {ex.Message}\n{ex.StackTrace}";
            }
            return apiCommon;
        }

    }



}