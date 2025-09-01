using DocumentFormat.OpenXml.Bibliography;
using RM.API.Dtos;
using RM.API.Models.Constants;
using RM.API.Models.Reports;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.Common;
using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface IPartnerService
    {
        /// <summary>
        /// PrimaryKey == MobileNumber
        /// </summary>
        Task<ApiCommonResponseModel> GetById(long id);

        /// <summary>
        /// PrimaryKey == MobileNumber
        /// </summary>
        Task<ApiCommonResponseModel> GetAll(QueryValues param);
        Task<ApiCommonResponseModel> GetPartnerComments(QueryValues queryValues);


        ApiCommonResponseModel GetPartnerAccountsAndComments(long id);

        /// <summary>
        /// This Method is only through CRM
        /// </summary>
        ApiCommonResponseModel Manage(TokenVariables user, AllParnterAccountCode param);

        /// <summary>
        /// This method is only we are using through outside APIs like (kingresearch website) or partner.kingresearch.co.in
        /// </summary>
        ApiCommonResponseModel Add(PartnerAccountRequestModel param);

        Task<ApiCommonResponseModel> GetPartnerAccountsSummaryReport(QueryValues query);

        Task<ApiCommonResponseModel> GetPartnerCountAsync(QueryValues queryValues);

        Task<ApiCommonResponseModel> GetPartnerStatusCountAsync(QueryValues queryValues);

        ApiCommonResponseModel Delete(long id, TokenVariables loginUser);
        Task<object> GetPartnerReferralLinks();
        object GetByMobile(string mobileNumber);
    }

    public class PartnerService : IPartnerService
    {
        private readonly KingResearchContext _context;
        private readonly ApiCommonResponseModel apiCommonResponse = new();

        public PartnerService(KingResearchContext context)
        {
            _context = context;
        }

        public async Task<ApiCommonResponseModel> GetById(long id)
        {
            var partnerAccount = await _context.PartnerAccounts.Where(item => item.Id == id).FirstOrDefaultAsync();
            return apiCommonResponse;
        }

        public async Task<ApiCommonResponseModel> GetAll(QueryValues queryValues)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            try
            {
                SqlParameter parameterOutValue = new()
                {
                    ParameterName = "product_count",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };

                SqlParameter parameterreturnValue = new()
                {
                    ParameterName = "returnValue",
                    Direction = System.Data.ParameterDirection.Output,
                    SqlDbType = System.Data.SqlDbType.Int,
                };
                SqlParameter[] sqlParameters = new[]
                {
                new SqlParameter
                {
                    ParameterName = "IsPaging",
                    Value = queryValues.IsPaging,
                    SqlDbType = System.Data.SqlDbType.Bit,
                },
                new SqlParameter
                {
                    ParameterName = "PageSize",
                    Value = queryValues.PageSize,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                new SqlParameter
                    {
                        ParameterName = "PageNumber ",
                        Value = queryValues.PageNumber ,
                        SqlDbType = System.Data.SqlDbType.Int,
                    },
                new SqlParameter
                    {
                        ParameterName = "FromDate",
                        Value = queryValues.FromDate?? Convert.DBNull,
                        SqlDbType = System.Data.SqlDbType.DateTime,
                    },
                new SqlParameter
                    {
                        ParameterName = "ToDate",
                        Value = queryValues.ToDate ?? Convert.DBNull,
                        SqlDbType = System.Data.SqlDbType.DateTime,
                    }

                ,
                new SqlParameter
                    {
                        ParameterName = "SearchText",
                        Value = string.IsNullOrEmpty(queryValues.SearchText) ?  Convert.DBNull : queryValues.SearchText,
                        SqlDbType = System.Data.SqlDbType.VarChar,
                    }
                ,
                new SqlParameter
                    {
                        ParameterName = "SortExpression",
                        Value = string.IsNullOrEmpty(queryValues.SortExpression) ?  Convert.DBNull : queryValues.SortExpression,
                        SqlDbType = System.Data.SqlDbType.VarChar,
                    }
                ,
                new SqlParameter
                    {
                        ParameterName = "PartnerWith",
                        Value =string.IsNullOrEmpty(queryValues.ThirdKey) ?  Convert.DBNull : queryValues.ThirdKey  ,
                        SqlDbType = System.Data.SqlDbType.VarChar,
                    }
                ,
                new SqlParameter
                    {
                        ParameterName = "StatusType",
                        Value = queryValues.FourthKey == "" ?  Convert.DBNull : queryValues.FourthKey,
                        SqlDbType = System.Data.SqlDbType.Int,
                    },
                new SqlParameter
                    {
                        ParameterName = "AssignedTo",
                        Value = queryValues.FifthKey == "" ?  0 : Convert.ToInt32(queryValues.FifthKey),
                        SqlDbType = System.Data.SqlDbType.Int,
                    },
                 new SqlParameter
                                {
                                    ParameterName = "PartnerType",
                                    Value = string.IsNullOrEmpty(queryValues.SixthKey)
                                                ? DBNull.Value
                                                : queryValues.SixthKey,
                                    SqlDbType = System.Data.SqlDbType.Int,
                                },

                parameterOutValue,
                parameterreturnValue
            };
                OutputParameter<int> returnValue = null;
                List<PartnerAccountsSP> dd = await _context.SqlQueryToListAsync<PartnerAccountsSP>
                    ("EXEC  @returnValue = GetPartnerAccountsFlat @IsPaging  , @PageSize , @PageNumber  , @FromDate  , @ToDate  , @SearchText  ,   @SortExpression , @PartnerWith   , @StatusType , @AssignedTo ,@PartnerType, @product_count OUTPUT", sqlParameters);
                //dd = dd.ToList();

                //int overallCount = (int)parameterOverallCount.Value;
                object totalRecords = parameterOutValue.Value;

                returnValue?.SetValue(parameterreturnValue.Value);

                apiCommonResponse.Data = dd;
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Total = Convert.ToInt32(totalRecords);

                return apiCommonResponse;
            }
            catch (Exception ex)
            {
                apiCommonResponse.Message = ex.ToString();
                apiCommonResponse.Data = null;
                apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
            }

            return apiCommonResponse;
        }


        public async Task<ApiCommonResponseModel> GetPartnerComments(QueryValues queryValues)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            try
            {
                SqlParameter parameterOutValue = new()
                {
                    ParameterName = "product_count",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };

                SqlParameter parameterreturnValue = new()
                {
                    ParameterName = "returnValue",
                    Direction = System.Data.ParameterDirection.Output,
                    SqlDbType = System.Data.SqlDbType.Int,
                };

                SqlParameter[] sqlParameters = new[]
                {
            new SqlParameter
            {
                ParameterName = "IsPaging",
                Value = queryValues.IsPaging,
                SqlDbType = System.Data.SqlDbType.Bit,
            },
            new SqlParameter
            {
                ParameterName = "PageSize",
                Value = queryValues.PageSize,
                SqlDbType = System.Data.SqlDbType.Int,
            },
            new SqlParameter
            {
                ParameterName = "PageNumber",
                Value = queryValues.PageNumber,
                SqlDbType = System.Data.SqlDbType.Int,
            },
            new SqlParameter
            {
                ParameterName = "FromDate",
                Value = queryValues.FromDate ?? (object)DBNull.Value,
                SqlDbType = System.Data.SqlDbType.Date,
            },
            new SqlParameter
            {
                ParameterName = "ToDate",
                Value = queryValues.ToDate ?? (object)DBNull.Value,
                SqlDbType = System.Data.SqlDbType.Date,
            },
            new SqlParameter
            {
                ParameterName = "SearchText",
                Value = string.IsNullOrEmpty(queryValues.SearchText) ? (object)DBNull.Value : queryValues.SearchText,
                SqlDbType = System.Data.SqlDbType.VarChar,
            },
            parameterOutValue,
            parameterreturnValue
        };

                OutputParameter<int> returnValue = null;

                // Map to a DTO like PartnerCommentsSP (you can create a model similar to PartnerAccountsSP)
                List<PartnerCommentsSP> comments = await _context.SqlQueryToListAsync<PartnerCommentsSP>(
                    "EXEC @returnValue = Sp_GetPartnerAccountsWithComments @IsPaging, @PageSize, @PageNumber, @FromDate, @ToDate, @SearchText, @product_count OUTPUT",
                    sqlParameters
                );

                object totalRecords = parameterOutValue.Value;
                returnValue?.SetValue(parameterreturnValue.Value);

                apiCommonResponse.Data = comments;
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Total = Convert.ToInt32(totalRecords);

                return apiCommonResponse;
            }
            catch (Exception ex)
            {
                apiCommonResponse.Message = ex.ToString();
                apiCommonResponse.Data = null;
                apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
            }

            return apiCommonResponse;
        }
        public ApiCommonResponseModel GetPartnerAccountsAndComments(long partnerId)
        {
            PartnerCommnetResponseModel objResponse = new();
            try
            {
                var comments = (from pc in _context.PartnerComments
                                join pa in _context.PartnerAccounts on pc.PartnerId equals pa.Id
                                join us in _context.Users on pc.CreatedBy equals us.Id
                                select new
                                {
                                    pa.Id,
                                    pa.PublicKey,
                                    pc.Comments,
                                    pc.CreatedOn,
                                    us.FirstName,
                                    us.LastName
                                })
                               .Where(item => item.Id == partnerId)
                               .OrderByDescending(orderby => orderby.CreatedOn)
                               .ToList();

                var activities = (from pa in _context.PartnerAccountActivities
                                  join pad in _context.PartnerAccountDetails on pa.PartnerAccountDetailId equals pad.Id
                                  join user in _context.Users on pa.CreatedBy equals user.Id
                                  where pad.PartnerAccountId == partnerId
                                  select new
                                  {
                                      pad.PartnerCode,
                                      pa.Comments,
                                      pa.CreatedOn,
                                      user.FirstName,
                                      user.LastName
                                  })
                                 .OrderByDescending(o => o.CreatedOn)
                                 .ToList();

                if (comments.Any() || activities.Any())
                {
                    var partnerCommentTemp = (from pc in _context.PartnerAccountActivities
                                              join pad in _context.PartnerAccountDetails on pc.PartnerAccountDetailId equals pad.Id
                                              join pa in _context.PartnerAccounts on pad.PartnerAccountId equals pa.Id
                                              join us in _context.Users on pc.CreatedBy equals us.Id
                                              select new
                                              {
                                                  pa.Id,
                                                  pa.PublicKey,
                                                  pad.PartnerCode,
                                                  pc.Comments,
                                                  pc.CreatedOn,
                                                  us.FirstName,
                                                  us.LastName
                                              }
                               ).Where(item => item.Id == partnerId).OrderByDescending(orderby => orderby.CreatedOn).ToList();

                    //if (partnerCommentTemp is not null && partnerCommentTemp.Count > 0)
                    //{
                    //    objResponse.Details = _context.PartnerAccountDetails.Where(item => item.PartnerAccountId == partnerId).ToList();
                    //}

                }

                objResponse.Comments = comments;
                objResponse.Activities = activities;

                apiCommonResponse.StatusCode = HttpStatusCode.OK;
                apiCommonResponse.Data = objResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return apiCommonResponse;
        }


        public ApiCommonResponseModel Manage(TokenVariables user, AllParnterAccountCode param)
        {
            try
            {
                DateTime modifiedOn = DateTime.Now;
                Guid partnerKey = Guid.NewGuid();
                long partnerId = 0;

                #region Add or Update PartnerAccount

                var partnerAccount = _context.PartnerAccounts.FirstOrDefault(p => p.Id == param.Id);
                bool isNewPartner = partnerAccount == null;

                if (isNewPartner)
                {
                    var leadKey = _context.Leads
                        .Where(l => l.MobileNumber == param.MobileNumber)
                        .Select(l => l.PublicKey)
                        .FirstOrDefault();

                    partnerAccount = new PartnerAccount
                    {
                        FullName = param.FullName,
                        MobileNumber = param.MobileNumber,
                        EmailId = param.EmailId,
                        City = param.City,
                        Source = param.Source,
                        AssignedTo = param.AssignedTo,
                        TelegramId = param.TelegramId,
                        Brokerage = param.Brokerage,
                        LeadKey = leadKey,
                        Remarks = param.Remarks,
                        IsDelete = param.IsDelete,
                        IsDisabled = param.IsDisabled,
                        PublicKey = partnerKey,
                        CreatedIpAddress = param.CreatedIpAddress,
                        CreatedBy = param.CreatedBy,
                        CreatedOn = modifiedOn,
                        ModifiedBy = param.ModifiedBy,
                        ModifiedOn = modifiedOn,
                    };

                    _context.PartnerAccounts.Add(partnerAccount);
                    apiCommonResponse.Message = "Successfully Created";
                }
                else
                {
                    partnerId = partnerAccount.Id;
                    partnerKey = partnerAccount.PublicKey;

                    if (partnerAccount.LeadKey == null)
                    {
                        partnerAccount.LeadKey = _context.Leads
                            .Where(l => l.MobileNumber == param.MobileNumber)
                            .Select(l => l.PublicKey)
                            .FirstOrDefault();
                    }

                    if (param.Status.HasValue && partnerAccount.Status != param.Status)
                    {
                        string statusString = Enum.GetName(typeof(PartnerAccountStatus), param.Status);
                        if (!string.IsNullOrEmpty(statusString))
                        {
                            _context.PartnerAccountActivities.Add(new PartnerAccountActivity
                            {
                                Comments = $"Status changed to: {statusString}",
                                CreatedBy = user.Id,
                                CreatedOn = modifiedOn,
                                PartnerAccountDetailId = 0
                            });
                        }
                    }

                    partnerAccount.FullName = param.FullName;
                    partnerAccount.EmailId = param.EmailId;
                    partnerAccount.MobileNumber = param.MobileNumber;
                    partnerAccount.City = param.City;
                    partnerAccount.TelegramId = param.TelegramId;
                    partnerAccount.AssignedTo = param.AssignedTo;
                    partnerAccount.Remarks = param.Remarks;
                    partnerAccount.Status = param.Status;
                    partnerAccount.Source = param.Source;
                    partnerAccount.ModifiedOn = modifiedOn;
                    partnerAccount.ModifiedBy = param.ModifiedBy;

                    apiCommonResponse.Message = "Successfully Updated";
                }

                #endregion

                #region Add Partner Comment

                if (!string.IsNullOrEmpty(param.Remarks))
                {
                    bool commentExistsToday = _context.PartnerComments
                        .Any(c => c.PartnerId == partnerAccount.Id && c.Comments == param.Remarks);

                    if (!commentExistsToday)
                    {
                        _context.PartnerComments.Add(new PartnerComment
                        {
                            Comments = param.Remarks,
                            CreatedBy = user.Id,
                            CreatedOn = modifiedOn,
                            PartnerId = partnerAccount.Id
                        });
                    }
                }

                #endregion

                #region Add Default PartnerActivity (if needed)

                bool activityExistsToday = _context.PartnerAccountActivities
                    .Any(a => a.PartnerAccountDetailId == 0 && a.Comments == param.Remarks && a.CreatedOn.Value.Date == modifiedOn.Date);

                if (!activityExistsToday && !string.IsNullOrEmpty(param.Remarks))
                {
                    _context.PartnerAccountActivities.Add(new PartnerAccountActivity
                    {
                        Comments = param.Remarks,
                        CreatedBy = user.Id,
                        CreatedOn = modifiedOn,
                        PartnerAccountDetailId = 0
                    });
                }

                #endregion

                #region Manage PartnerAccountDetails

                if (param?.PartnerDetails != null)
                {
                    foreach (var sourceDetail in param.PartnerDetails)
                    {
                        var newStatusName = sourceDetail.StatusId.HasValue
                            ? Enum.GetName(typeof(PartnerAccountStatus), sourceDetail.StatusId.Value)
                            : "";

                        if (sourceDetail.partnerAccountDetailId == 0)
                        {
                            if (!string.IsNullOrWhiteSpace(sourceDetail.PartnerCode) && sourceDetail.StatusId.HasValue)
                            {
                                if (!string.IsNullOrEmpty(sourceDetail.ClientId))
                                {
                                    var detail = new PartnerAccountDetails
                                    {
                                        CreatedBy = Guid.Parse(user.PublicKey),
                                        CreatedOn = modifiedOn,
                                        ModifiedBy = Guid.Parse(user.PublicKey),
                                        ModifiedOn = modifiedOn,
                                        PartnerCId = sourceDetail.ClientId,
                                        IsVerified = sourceDetail.IsVerified,
                                        PartnerWith = sourceDetail.PartnerWith,
                                        AssignedTo=sourceDetail.AssignedTo,
                                        StatusId = sourceDetail.StatusId,
                                        PartnerCode = sourceDetail.PartnerCode,
                                        PartnerAccountId = param.Id
                                    };

                                    _context.PartnerAccountDetails.Add(detail);
                                    _context.SaveChanges();

                                    _context.PartnerAccountActivities.Add(new PartnerAccountActivity
                                    {
                                        Comments = $"Status changed to: {newStatusName}",
                                        CreatedBy = user.Id,
                                        CreatedOn = modifiedOn,
                                        PartnerAccountDetailId = detail.Id
                                    });
                                }
                            }

                        }
                        else
                        {
                            var destinationDetail = _context.PartnerAccountDetails.FirstOrDefault(d => d.Id == sourceDetail.partnerAccountDetailId);

                            if (destinationDetail != null)
                            {
                                bool isChanged =
                                    destinationDetail.PartnerWith != sourceDetail.PartnerWith ||
                                    destinationDetail.IsVerified != sourceDetail.IsVerified ||
                                    destinationDetail.AssignedTo != sourceDetail.AssignedTo ||
                                    !string.Equals(destinationDetail.PartnerCId?.Trim(), sourceDetail.ClientId?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                                    (destinationDetail.StatusId ?? 0) != (sourceDetail.StatusId ?? 0);

                                if (isChanged)
                                {
                                    int? oldStatusId = destinationDetail.StatusId ?? 0;
                                    int? newStatusId = sourceDetail.StatusId ?? 0;

                                    string oldStatusName = Enum.GetName(typeof(PartnerAccountStatus), oldStatusId);
                                    string resolvedNewStatusName = Enum.GetName(typeof(PartnerAccountStatus), newStatusId);

                                    // Only add activity if status is different
                                    if (oldStatusId != newStatusId)
                                    {
                                        _context.PartnerAccountActivities.Add(new PartnerAccountActivity
                                        {
                                            Comments = $"[{sourceDetail.ClientId}] Status changed from {(oldStatusName ?? oldStatusId.ToString())} to {(resolvedNewStatusName ?? newStatusId.ToString())}",
                                            CreatedBy = user.Id,
                                            CreatedOn = modifiedOn,
                                            PartnerAccountDetailId = destinationDetail.Id
                                        });
                                    }

                                    // Update fields
                                    destinationDetail.PartnerWith = sourceDetail.PartnerWith;
                                    destinationDetail.PartnerCId = sourceDetail.ClientId;
                                    destinationDetail.IsVerified = sourceDetail.IsVerified;
                                    destinationDetail.AssignedTo = sourceDetail.AssignedTo;
                                    destinationDetail.StatusId = sourceDetail.StatusId;
                                    destinationDetail.ModifiedOn = modifiedOn;
                                    destinationDetail.ModifiedBy = Guid.Parse(user.PublicKey);
                                }
                            }
                        }

                    }
                }

                #endregion

                _context.SaveChanges();
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                // Add optional logging here
                apiCommonResponse.StatusCode = HttpStatusCode.InternalServerError;
                apiCommonResponse.Message = "An unexpected error occurred.";
            }

            return apiCommonResponse;
        }

        public ApiCommonResponseModel Add(PartnerAccountRequestModel param)
        {
            var partnerAccount = _context.PartnerAccounts.Where(item => item.MobileNumber == param.MobileNumber).FirstOrDefault();
            param.Remarks = param.Remarks.Trim();

            Guid partnerKey = Guid.NewGuid();
            long partnerId = 0;
            DateTime modifiedOn = DateTime.Now;


            var commentTemp = new PartnerComment
            {

                Comments = param.Remarks,
                CreatedBy = 6,
                CreatedOn = modifiedOn
            };

            if (partnerAccount == null)
            {
                var partnerNew = new PartnerAccount
                {
                    FullName = param.FullName,
                    MobileNumber = param.MobileNumber,
                    EmailId = param.EmailId,
                    City = param.City,
                    Source = param.Source,
                    AssignedTo = param.AssignedTo,
                    TelegramId = param.TelegramId,
                    Brokerage = param.Brokerage,
                    LeadKey = param.LeadKey,
                    Remarks = param.Remarks,
                    IsDelete = param.IsDelete,
                    IsDisabled = param.IsDisabled,
                    PublicKey = partnerKey,
                    CreatedIpAddress = param.CreatedIpAddress,
                    CreatedBy = param.CreatedBy,
                    CreatedOn = modifiedOn,
                    ModifiedBy = param.ModifiedBy,
                    ModifiedOn = modifiedOn,
                    Status = param.Status ?? 0
                };
                _context.PartnerAccounts.Add(partnerNew);
                apiCommonResponse.Message = "Successfully Added";
                _context.SaveChanges();
                // After saving, we can get the Id of the newly created partner
                partnerId = partnerNew.Id;

            }
            else
            {
                partnerId= partnerAccount.Id;
            }

            commentTemp.PartnerId = partnerId;
            _context.PartnerComments.Add(commentTemp);

            //else
            //{
            //    partnerId = partnerAccount.Id;
            //    partnerAccount.ModifiedOn = modifiedOn;
            //    partnerAccount.Remarks = param.Remarks;
            //    partnerAccount.Status = 0;

            //    partnerAccount.IsDelete = 0;

            //    var existingComment = _context.PartnerComments
            //        .Where(c => c.PartnerId == partnerId).FirstOrDefault();

            //    if (existingComment == null)
            //    {
            //        var comment = new PartnerComment
            //        {
            //            PartnerId = partnerId,
            //            Comments = param.Remarks,
            //            CreatedBy = 6,
            //            CreatedOn = modifiedOn
            //        };
            //        _context.PartnerComments.Add(comment);
            //    }
            //    else
            //    {
            //        if (DateTime.Now.Date != existingComment.CreatedOn.Value.Date)
            //        {
            //            var comment = new PartnerComment
            //            {
            //                PartnerId = partnerId,
            //                Comments = existingComment.Comments,
            //                CreatedBy = 6,
            //                CreatedOn = modifiedOn
            //            };
            //            _context.PartnerComments.Add(comment);
            //        }
            //    }
            //}

            //we added this logic to make sure for which partner account client has been requestd for.. suppos he requesting for punch then in partnerAccountDetails will make one dummy entry for partnerCode = 'punch';

            var partnerCode = GetPartnerShortName(param.Remarks);
            var partnerAccountDetails = _context.PartnerAccountDetails.Where(item => item.PartnerAccountId == partnerId && item.PartnerCode == partnerCode ).ToList();
            //bool isPartnerCodeExists = partnerAccountDetails.Any(item => item.PartnerCode == partnerCode);

            if (partnerAccountDetails.Count() == 0)
            {
                PartnerAccountDetails partnerAccountDestination = new()
                {
                    PartnerAccountId = partnerId,
                    PartnerCode = partnerCode,
                    CreatedOn = modifiedOn,
                    ModifiedOn = modifiedOn,
                    PartnerCId = param.Remarks,
                    Remarks = param.Remarks,
                    StatusId = param.Status ?? 0
                };

                _context.PartnerAccountDetails.Add(partnerAccountDestination);

            }

            _context.SaveChanges();
            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            return apiCommonResponse;
        }

        private static string GetPartnerShortName(string remarks)
        {
            remarks = remarks.ToUpper();
            return remarks switch
            {
                "ANGEL BROKING" => "angel",
                "FYERS" => "fyer",
                "ALICEBLUE" => "ant",
                "EDELWISS" => "edel",
                "ZERODHA" => "kite",
                "DHAN" => "dhan",
                "PUNCH TRADE" => "punch",
                "DELTA EXCHANGE" => "delta",
                _ => ""
            };
        }

        public async Task<ApiCommonResponseModel> GetPartnerAccountsSummaryReport(QueryValues queryValues)
        {
            SqlParameter BenchMark = new()
            {
                ParameterName = "BenchMark",
                Direction = System.Data.ParameterDirection.Output,
                SqlDbType = System.Data.SqlDbType.Int,
            };

            SqlParameter[] sqlParameters = new[]
            {
                new SqlParameter
                    {
                        ParameterName = "@PartnerName",
                        Value = queryValues.FourthKey == "" ?  Convert.DBNull : queryValues.FourthKey,
                        SqlDbType = System.Data.SqlDbType.VarChar,
                    },

                 new SqlParameter
                    {
                        ParameterName = "AssignedTo",
                        Value = queryValues.PrimaryKey == "" ?  0 : Convert.ToInt32(queryValues.PrimaryKey),
                        SqlDbType = System.Data.SqlDbType.Int,
                    },
                new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = queryValues.FromDate  ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Date,
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = queryValues.ToDate  ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Date,
                },
                 
                BenchMark,
            };
            PartnerSummaryReportRootResponse response = new();

            List<PartnerSummaryReportResponse> result = await _context.SqlQueryToListAsync<PartnerSummaryReportResponse>
            ("EXEC [GetPartnerAccountsSummaryReport] @PartnerName ={0} , @AssignedTo ={1}, @FromDate ={2}, @ToDate ={3} , @BenchMark = {4} OUTPUT", sqlParameters);
            response.responseData = result;
            response.BenchMark = Convert.ToInt32(BenchMark.Value);
            response.MinValue = result.FirstOrDefault().Date.ToString();
            response.MaxValue = result.Max(t => t.Date).ToString();

            apiCommonResponse.Data = response;
            apiCommonResponse.Message = "Success";
            apiCommonResponse.StatusCode = HttpStatusCode.OK;

            return apiCommonResponse;
        }

        public async Task<ApiCommonResponseModel> GetPartnerCountAsync(QueryValues queryValues)
        {
            SqlParameter[] sqlParameters = new[]
          {
                new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = queryValues.FromDate  ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Date,
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = queryValues.ToDate  ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Date,
                }
            };
            var result = await _context.SqlQueryToListAsync<PartnerCountResponse>(
        "EXEC [GetPartnerCountByDate] @FromDate = {0}, @ToDate = {1}",
        sqlParameters);

            return new ApiCommonResponseModel
            {
                Data = result,
                Message = "Partner count retrieved successfully.",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ApiCommonResponseModel> GetPartnerStatusCountAsync(QueryValues queryValues)
        {
            SqlParameter[] sqlParameters = new[]
          {
                new SqlParameter
                {
                    ParameterName = "@FromDate",
                    Value = queryValues.FromDate  ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Date,
                },
                new SqlParameter
                {
                    ParameterName = "@ToDate",
                    Value = queryValues.ToDate  ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Date,
                },
                 new SqlParameter
                    {
                        ParameterName = "AssignedTo",
                        Value = queryValues.PrimaryKey == "" ?  0 : Convert.ToInt32(queryValues.PrimaryKey),
                        SqlDbType = System.Data.SqlDbType.Int,
                    },
                  new SqlParameter
                    {
                        ParameterName = "@PartnerName",
                        Value = queryValues.FourthKey == "" ?  Convert.DBNull : queryValues.FourthKey,
                        SqlDbType = System.Data.SqlDbType.VarChar,
                    },
            };
            var result = await _context.SqlQueryToListAsync<PartnerStatusCount>(
        "EXEC [GetPartnerAccountStatusCount] @FromDate = {0}, @ToDate = {1},@AssignedTo={2}, @PartnerName ={3} ",
        sqlParameters);

            return new ApiCommonResponseModel
            {
                Data = result,
                Message = "Partner count retrieved successfully.",
                StatusCode = HttpStatusCode.OK
            };
        }

        public ApiCommonResponseModel Delete(long id, TokenVariables UserKey)
        {
            if (UserKey is null)
            {
                apiCommonResponse.StatusCode = HttpStatusCode.Unauthorized;
            }
            PartnerAccount checkifExists = _context.PartnerAccounts.Where(item => item.Id == id).FirstOrDefault();

            if (checkifExists == null)
            {
                apiCommonResponse.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                checkifExists.IsDelete = 1;
                checkifExists.ModifiedBy = UserKey.PublicKey;
                checkifExists.ModifiedOn = DateTime.Now;
                _context.Entry(checkifExists).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.SaveChanges();
                apiCommonResponse.StatusCode = HttpStatusCode.OK;
            }
            return apiCommonResponse;
        }

        private List<PartnerAccountDetails> CreatePartnerAccountDetails(AllParnterAccountCode param, DateTime modifiedOn)
        {
            var list = new List<PartnerAccountDetails>();

            if (param.PartnerDetails != null)
            {
                foreach (var detail in param.PartnerDetails)
                {
                    //  Only include when ClientId is valid
                    if (!string.IsNullOrWhiteSpace(detail.ClientId))
                    {
                        list.Add(new PartnerAccountDetails
                        {
                            PartnerAccountId = param.Id,
                            PartnerCode = detail.PartnerCode.ToLower(),
                            PartnerCId = detail.ClientId.Trim(),
                            IsVerified = detail.IsVerified,
                            PartnerWith = detail.PartnerWith,
                            Remarks = param.Remarks,
                            StatusId = detail.StatusId ?? 0, // or throw if you require this
                            CreatedOn = modifiedOn,
                            ModifiedBy = Guid.Parse(param.ModifiedBy),
                            CreatedBy = Guid.Parse(param.CreatedBy),
                            ModifiedOn = modifiedOn
                        });
                    }
                }
            }

            return list;
        }


        public async Task<object> GetPartnerReferralLinks()
        {
            return await _context.PartnerNamesM.Where(item => item.IsActive && !item.IsDelete).ToListAsync();
        }

        public object GetByMobile(string mobileNumber)
        {
            var partner = _context.PartnerAccounts
               .Where(p => p.MobileNumber == mobileNumber && (p.IsDelete == 0 || p.IsDelete == null))
               .FirstOrDefault();

            // ✅ Check here before accessing partner.Id
            if (partner == null)
                return (new { statusCode = 404, message = "Partner not found" });

            var detail = _context.PartnerAccountDetails
                .Where(joined => joined.PartnerAccountId == partner.Id)
                .ToList();

            var comments = (from pc in _context.PartnerComments
                            join pa in _context.PartnerAccounts on pc.PartnerId equals pa.Id
                            join us in _context.Users on pc.CreatedBy equals us.Id
                            where pa.Id == partner.Id
                            select new
                            {
                                pa.Id,
                                pa.PublicKey,
                                pc.Comments,
                                pc.CreatedOn,
                                us.FirstName,
                                us.LastName
                            })
                                  .OrderByDescending(orderby => orderby.CreatedOn)
                                  .ToList();

            var activities = (from pa in _context.PartnerAccountActivities
                              join pad in _context.PartnerAccountDetails on pa.PartnerAccountDetailId equals pad.Id
                              join user in _context.Users on pa.CreatedBy equals user.Id
                              where pad.PartnerAccountId == partner.Id
                              select new
                              {
                                  pad.PartnerCode,
                                  pa.Comments,
                                  pa.CreatedOn,
                                  user.FirstName,
                                  user.LastName
                              })
                                    .OrderByDescending(o => o.CreatedOn)
                                    .ToList();

            return new
            {
                statusCode = 200,
                data = new
                {
                    account = partner,
                    detail = detail,
                    comments = comments,
                    activities = activities
                }
            };
        }
    }

    public class OutputParameter<TValue>
    {
        /// <summary>
        /// Defines the _valueSet.
        /// </summary>
        private bool _valueSet = false;

        /// <summary>
        /// Defines the _value.
        /// </summary>
        public TValue _value;

        /// <summary>
        /// Gets the Value.
        /// </summary>
        public TValue Value => !_valueSet ? throw new InvalidOperationException("Value not set.") : _value;

        /// <summary>
        /// The SetValue.
        /// </summary>
        /// <param name="value">The value<see cref="object"/>.</param>
        public void SetValue(object value)
        {
            _valueSet = true;

            _value = null == value || Convert.IsDBNull(value) ? default : (TValue)value;
        }
    }
}