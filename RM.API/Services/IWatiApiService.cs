using RM.API.Models;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Model;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface IWatiApiService
    {
        Task<ApiCommonResponseModel> SendTemplateMessageMarketManthan(int numberOfLeads, string loggedInUser);
        Task<ApiCommonResponseModel> SendTextToNumber(string mobileNumber, string message);
        Task<ApiCommonResponseModel> GetTemplateName();
        Task<ApiCommonResponseModel> GetRemainingMessagesToSendCount();
        //Task<ApiCommonResponseModel> SendBulkMessages(QueryValues queryValues);
    }

    public class WatiApiService : IWatiApiService
    {
        private readonly IConfiguration _configuration;
        private readonly ResearchMantraContext _context;
        readonly string watiBaseUrl = "";
        readonly string templateNameUrl = "";
        readonly string sendTemplateMessageUrl = "";
        readonly string templateName = "";
        readonly string sendSessionMessage = "";
        readonly string authorizationToken = "";

        public WatiApiService(ResearchMantraContext context, IConfiguration config)
        {
            _context = context;
            _configuration = config;
            watiBaseUrl = _configuration.GetSection("Wati:Baseurl").Value;
            sendTemplateMessageUrl = _configuration.GetSection("Wati:SendTemplateMessageUrl").Value;
            templateName = _configuration.GetSection("Wati:TemplateName").Value;
            authorizationToken = _configuration.GetSection("Wati:BearerToken").Value;
            sendSessionMessage = _configuration.GetSection("Wati:SendSessionMessage").Value;
            watiBaseUrl = _configuration.GetSection("Wati:Baseurl").Value;
            templateNameUrl = _configuration.GetSection("Wati:TemplateNameUrl").Value;
        }

        ApiCommonResponseModel responseModel = new();

        //send template message to number
        public async Task<ApiCommonResponseModel> SendTemplateMessageMarketManthan(int numberOfLeads, string loggedInUser)
        {
            try
            {

                //var presentDateSentMessagesId = await _context.WatiSentMessageLogs.Where(item => item.CreatedOn.Date == DateTime.Today).Select(item => item.LeadId).ToListAsync();


                SqlParameter[] sqlParameters = new[]
                {
                        new SqlParameter { ParameterName = "NumberOfLeads",      Value = numberOfLeads ,SqlDbType = System.Data.SqlDbType.Int},
                };
                var leadsToSendWhatsapp = await _context.SqlQueryToListAsync<GetLeadsForWhatsappMessageModel>(ProcedureCommonSqlParametersText.GetLeadsForWhatsappMessage, sqlParameters);


                //if (leadsToSendWhatsapp.Count >= 1000)
                //{
                //    responseModel.Message = "Daily limit has been reached.";
                //    responseModel.StatusCode = HttpStatusCode.Forbidden;
                //    return responseModel;
                //}



                #region execute the sp with pageSize of 1000 numbers
                //SqlParameter parameterOutValue = new()
                //{
                //    ParameterName = "TotalCount",
                //    SqlDbType = SqlDbType.Int,
                //    Direction = ParameterDirection.Output,
                //};


                //SqlParameter[] sqlParameters = new[]
                //{
                //    new SqlParameter { ParameterName = "IsPaging",      Value = queryValues.IsPaging,SqlDbType = System.Data.SqlDbType.Int},
                //    new SqlParameter { ParameterName = "PageSize",      Value = 1000 - presentDateSentMessagesId.Count,SqlDbType = System.Data.SqlDbType.Int},
                //    new SqlParameter { ParameterName = "PageNumber ",   Value = 1 ,SqlDbType = System.Data.SqlDbType.Int},
                //    new SqlParameter { ParameterName = "SortOrder",     Value = queryValues.SortOrder == "" ?  "DESC" : queryValues.SortOrder,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                //    new SqlParameter { ParameterName = "SortExpression",Value = queryValues.SortExpression == "" ?  DBNull.Value: Convert.ToString(queryValues.SortExpression),SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                //    new SqlParameter { ParameterName = "FromDate",      Value = queryValues.FromDate ?? Convert.DBNull ,SqlDbType = System.Data.SqlDbType.DateTime},
                //    new SqlParameter { ParameterName = "ToDate",        Value = queryValues.ToDate  ??  Convert.DBNull ,SqlDbType = System.Data.SqlDbType.DateTime},
                //    new SqlParameter { ParameterName = "PrimaryKey",    Value = queryValues.PrimaryKey == "" ?  Convert.DBNull : queryValues.PrimaryKey,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                //    new SqlParameter { ParameterName = "SecondaryKey",  Value = queryValues.SecondaryKey  == "" ?  Convert.DBNull : queryValues.SecondaryKey,SqlDbType = SqlDbType.VarChar,Size = 50},
                //    new SqlParameter { ParameterName = "ThirdKey",      Value = queryValues.ThirdKey == "" ?  Convert.DBNull : queryValues.ThirdKey  ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                //    new SqlParameter { ParameterName = "FourthKey",     Value = queryValues.FourthKey == "" ?  Convert.DBNull : queryValues.FourthKey,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                //    new SqlParameter { ParameterName = "FifthKey",      Value = queryValues.FifthKey == "" ?  DBNull.Value : queryValues.FifthKey,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                //    new SqlParameter { ParameterName = "CreatedBy",     Value = queryValues.RequestedBy == "" ?  DBNull.Value : queryValues.RequestedBy,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                //    new SqlParameter { ParameterName = "AssignedTo",    Value = queryValues.AssignedTo == "" ?  DBNull.Value : queryValues.AssignedTo,SqlDbType = System.Data.SqlDbType.VarChar, Size = 50},
                //    new SqlParameter { ParameterName = "SearchText",    Value = queryValues.SearchText == "" ?  DBNull.Value : queryValues.SearchText , SqlDbType = System.Data.SqlDbType.VarChar, Size = 100},
                //    parameterOutValue
                //};
                //List<GetProcedureJsonResponse> junkleads = await _context.SqlQueryToListAsync<GetProcedureJsonResponse>(ProcedureCommonSqlParametersText.GetJunkLeads, sqlParameters);

                ////var watiTemplate =  _context.WatiTemplates.Where(item => item.TemplateName == queryValues.SixthKey).First();

                //JsonResponseModel spResult = new()
                //{
                //    JsonData = junkleads.FirstOrDefault()?.JsonData,
                //    TotalCount = Convert.ToInt32(parameterOutValue.Value)
                //};

                //List<Lead> leadListToSendMessages = JsonConvert.DeserializeObject<List<Lead>>(spResult.JsonData);

                #endregion

                //List<Lead> leadListToSendMessages = _context.Leads
                //        .Where(lead => !presentDateSentMessagesId.Contains(lead.Id))
                //    .OrderByDescending(lead => lead.CreatedOn)
                //    .Take(250 - presentDateSentMessagesId.Count)
                //    .ToList();


                int sentMessageCount = 0;

                using (HttpClient client = new())
                {

                    client.DefaultRequestHeaders.Add("Authorization", authorizationToken);
                    foreach (var lead in leadsToSendWhatsapp)
                    {

                        #region api call to wati for sending message // uncomment this to send messages

                        // in request body we need to give values for the parameters in the template (in wati) as name and value 
                        var requestBody = new
                        {
                            template_name = "marketmanthan09",
                            broadcast_name = "string",
                            //parameters = new[]
                            //{
                            //    new { name = "name", value = lead.FullName }
                            //}
                        };

                        string jsonBody = JsonConvert.SerializeObject(requestBody);

                        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");


                        var url = watiBaseUrl + sendTemplateMessageUrl + "91" + "8895804280"; //ToDo: change the hardcoded number to lead.MobileNumber

                        HttpResponseMessage response = await client.PostAsync(url, content);
                        #endregion

                        if (response.StatusCode.ToString() == "OK")
                        {
                            sentMessageCount++;
                            //this should be inside status.ok of the wati api call
                            WatiSentMessageLog watiSentMessageLog = new()
                            {
                                WatiId = 1,
                                LeadId = (int)lead.LeadId,
                                CreatedOn = DateTime.Now,
                                CreatedBy = Guid.Parse("3CA214D0-8CB8-EB11-AAF2-00155D53687A")
                            };
                            _context.WatiSentMessageLogs.Add(watiSentMessageLog);
                        }



                    }
                    await _context.SaveChangesAsync();
                }

                responseModel.Message = "No. of message sent = " + sentMessageCount;
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;

            }
            catch (Exception e)
            {
                responseModel.Message = e.Message;
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }


        // send simple message to number 
        public async Task<ApiCommonResponseModel> SendTextToNumber(string mobileNubmer, string message)
        {
            // get the url, bearer token and base url 



            string url = watiBaseUrl + sendSessionMessage + $"/{mobileNubmer}?messageText={message}";

            using (HttpClient client = new())
            {
                try
                {
                    // need to use the provided authorization token from wati
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationToken);

                    HttpResponseMessage response = await client.PostAsync(url, null);
                    if (response.StatusCode.ToString() == "OK")
                    {
                        responseModel.Message = "Message Sent Successfully.";
                        responseModel.StatusCode = HttpStatusCode.OK;
                        return responseModel;
                    }
                    else
                    {
                        responseModel.Message = "Error while sending message.";
                        responseModel.StatusCode = HttpStatusCode.OK;
                        return responseModel;
                    }
                }
                catch (HttpRequestException ex)
                {
                    responseModel.Message = "Error while sending message.";
                    responseModel.StatusCode = HttpStatusCode.OK;
                    return responseModel;
                }
            }
        }

        //public Task<ApiCommonResponseModel> SendWhatsappMessageInBulk(QueryValues filters)
        //{
        //    //ToDo: get the settings for wati 
        //    var watiSettingvalue = _context.Settings.Where(item => item.Code == filters.SixthKey).FirstOrDefault();
        //    //ToDO: Call The What's Template Body based on name 
        //    //WatiSettingResponseModel objRequest = Newtonsoft 

        //    //ToDO: Call the SP to get the filter data , also calculate the total unique numbers for the day 
        //    var watiSettingdssdfvalue = _context.WatiMessageSentLogs.Where(item => item.CreatedOn == DateTime.Now).FirstOrDefault();

        //    // Validation Make sure to not repeat the same template to send the same user, twice for the day
        //    //ToDo: Foreach on filter leads and replace the template body's receiver name if not then blank 
        //    //
        //}





        public async Task<ApiCommonResponseModel> GetTemplateName()
        {

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", authorizationToken);


                    HttpResponseMessage response = await client.GetAsync(templateNameUrl);


                    if (response.StatusCode.ToString() == "OK")
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                        List<string> elementNames = new List<string>();

                        foreach (var template in jsonResponse.messageTemplates)
                        {
                            string elementName = template.elementName;
                            string status = template.status;

                            if (status == "APPROVED")
                            {
                                elementNames.Add(elementName);
                            }

                            elementNames.Add(elementName);
                        }

                        responseModel.Message = "Template Name Fetched.";
                        responseModel.StatusCode = HttpStatusCode.OK;
                        responseModel.Data = elementNames;
                        return responseModel;
                    }
                    else
                    {
                        responseModel.Message = "Error while fetching data.";
                        responseModel.StatusCode = HttpStatusCode.InternalServerError;
                        return responseModel;
                    }

                }
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }


        }

        public async Task<ApiCommonResponseModel> GetRemainingMessagesToSendCount()
        {
            try
            {
                var presentDaySentMessages = _context.WatiSentMessageLogs.Count(item => item.CreatedOn.Date == DateTime.Today);
                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Data = presentDaySentMessages;
                responseModel.Message = "Data Fetched Successfully.";
                return responseModel;
            }
            catch (Exception ex)
            {
                responseModel.Message = ex.Message;
                responseModel.StatusCode = HttpStatusCode.InternalServerError;
                return responseModel;
            }
        }
    }


}
