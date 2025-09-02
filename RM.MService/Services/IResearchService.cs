using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.ResponseModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;

namespace RM.MService.Services
{
    public interface IResearchService
    {
        Task<ApiCommonResponseModel> GetBaskets(Guid loggedInUser);
        Task<ApiCommonResponseModel> GetCompanies(QueryValues param);
        Task<ApiCommonResponseModel> GetMessage(int id);
        Task<ApiCommonResponseModel> ManageComment(QueryValues request);
        Task<ApiCommonResponseModel> GetCompanyReport(QueryValues param);
    }

    public class ResearchService : IResearchService
    {
        private ApiCommonResponseModel responseModel = new();
        private readonly ResearchMantraContext _dbContext;

        public ResearchService(ResearchMantraContext context)
        {
            _dbContext = context;
        }

        public async Task<ApiCommonResponseModel> GetBaskets(Guid loggedInUser)
        {
            //responseModel.Data = _dbContext.BasketsM.Where(item => item.IsActive == true && item.IsDelete == false).Select(item => new { item.Id, item.Title, item.IsFree, item.Description }).ToList();
            var sqlParameters = new[]
            {
                new SqlParameter("@UserId", loggedInUser)
            };

            var spResult = await _dbContext.SqlQueryToListAsync<GetBasketsMSpResponseModel>(ProcedureCommonSqlParametersText.GetBasketsM, sqlParameters);

            responseModel.Data = spResult.OrderBy(c => c.SortOrder).ToList();

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        /// <summary>
        /// Id: BasketId
        /// PrimaryKey : AllPE || ALLMARKETCAP
        /// SecondaryKey : MarketCap: LESSTHEN500 || GREATERTHEN500  && PE: LESSTHEN40 || GREATERTHEN40
        /// SearchText : null
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<ApiCommonResponseModel> GetCompanies(QueryValues param)
        {
            SqlParameter sqlOutputParameter = new()
            {
                ParameterName = "UserHasResearch",
                Value = DBNull.Value,
                Direction = ParameterDirection.Output,
                SqlDbType = SqlDbType.Bit
            };

            List<SqlParameter> sqlParameters = new()
            {
                new SqlParameter { ParameterName = "LoggedInUser", Value = Guid.Parse(param.LoggedInUser)  ,SqlDbType = SqlDbType.UniqueIdentifier},
                new SqlParameter { ParameterName = "BasketId", Value = param.Id,SqlDbType = SqlDbType.Int},
                new SqlParameter { ParameterName = "SearchText", Value = param.SearchText == null || param.SearchText.Trim() == ""     ? DBNull.Value : param.SearchText ,SqlDbType = SqlDbType.VarChar, Size = 100},
                new SqlParameter { ParameterName = "PrimaryKey", Value = param.PrimaryKey == null ||  param.PrimaryKey.Trim() == "" ? DBNull.Value : param.PrimaryKey ,SqlDbType = SqlDbType.VarChar, Size = 100},
                new SqlParameter { ParameterName = "SecondaryKey", Value = param.SecondaryKey == null || param.SecondaryKey.Trim() == "" ? DBNull.Value : param.SecondaryKey ,SqlDbType = SqlDbType.VarChar, Size = 100},
                sqlOutputParameter
            };

            var spResult = await _dbContext.SqlQueryAsync<GetCompaniesSpResponseModel>(ProcedureCommonSqlParametersText.GetCompaniesM, sqlParameters.ToArray());

            responseModel.Data = new { companyData = spResult, hasResearchProduct = sqlOutputParameter.Value };
            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        private IQueryable<CompanyDetailM> GetCompaniesFilter(QueryValues param)
        {
            IQueryable<CompanyDetailM> queryableData = _dbContext.CompanyDetailM.Where(item => item.BasketId == param.Id && item.IsPublished == true).AsQueryable();

            if (!string.IsNullOrEmpty(param.SearchText))
            {
                queryableData = queryableData.Where(item => item.Name.Contains(param.SearchText));
            }
            else
            {
                if (!string.IsNullOrEmpty(param.PrimaryKey) || !string.IsNullOrEmpty(param.SecondaryKey))
                {
                    queryableData = queryableData.Where(item =>
                    (string.IsNullOrEmpty(param.PrimaryKey) ||
                    (param.PrimaryKey == "GREATERTHAN500" && item.MarketCap >= 500) ||
                    (param.PrimaryKey == "LESSTHAN500" && item.MarketCap < 500)) &&
                    (string.IsNullOrEmpty(param.SecondaryKey) ||
                    (param.SecondaryKey == "LESSTHAN40" && item.PE < 40) ||
                    (param.SecondaryKey == "GREATERTHAN40" && item.PE >= 40)));
                }
            }

            return queryableData;
        }

        /// <summary>
        /// Id: BasketId
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<ApiCommonResponseModel> GetCompanyReport(QueryValues param)
        {
            bool userHasResearchProduct = false;
            var query = from b in _dbContext.MyBucketM //ToDo: add join with mybucket table and check if user has purchased research product to avoid two db calls here.
                        join p in _dbContext.ProductsM on b.ProductId equals p.Id
                        where b.MobileUserKey == Guid.Parse(param.LoggedInUser)
                              && p.Code.ToLower() == "research"
                              && b.EndDate.HasValue && b.EndDate.Value.Date >= DateTime.Now.Date
                        select b.Id;
            var queryResult = await query.ToListAsync();
            userHasResearchProduct = queryResult.Count != 0;

            IQueryable<CompanyDetailM> queryableData = GetCompaniesFilter(param);

            var companyCount = 0;

            if (!userHasResearchProduct)
            {
                queryableData = queryableData.Where(c => c.IsFree);
                companyCount = await queryableData.CountAsync();
            }
            else
            {
                companyCount = await queryableData.CountAsync();
            }

            //var companyDetailM = _dbContext.CompanyDetailM.Where(item => item.BasketId == param.Id).AsQueryable();
            int skip = (param.PageNumber - 1);

            var company = await queryableData
                .Where(c => c.IsPublished == true)
                .OrderByDescending(x => x.IsFree)
                .ThenByDescending(x => x.CreatedOn).Skip(skip)
                .Take(1)
             //.Where(c => c.Id == companyId)
             .Select(c => new
             {
                 CompanyId = c.Id,
                 Name = c.Name,
                 Description = c.Description,
                 ChartUrl = c.ChartImageUrl,
                 OtherUrl = c.OtherImage,
                 WebsiteUrl = c.WebsiteUrl,
                 Symbol = c.Symbol,
                 c.PublishDate,
                 c.FaceValue,
                 c.CurrentPrice,
                 c.ProfitGrowth,
                 c.PromotersHolding,
                 c.NetWorth,
                 CommentCount = (from message in _dbContext.CompanyDetailMessageM
                                 join user in _dbContext.MobileUsers on message.ModifiedBy equals user.PublicKey
                                 where message.CompanyDetailId == c.Id && message.IsActive
                                 select message).Count()
             }).FirstOrDefaultAsync();

            if (company == null)
            {
                return null;
            }

            var companyType = await _dbContext.CompanyTypeM
                .Where(ct => ct.CompanyId == company.CompanyId)
                .Select(ct => new CompanyTypeM
                {
                    Id = ct.Id,
                    CompanyId = ct.CompanyId,
                    Symbol = ct.Symbol,
                    LTOPUptrend = ct.LTOPUptrend,
                    STOPOpUpTrend = ct.STOPOpUpTrend,
                    FuturisticSector = ct.FuturisticSector,
                    HNIInstitutionalPromotersBuy = ct.HNIInstitutionalPromotersBuy,
                    SpecialSituations = ct.SpecialSituations,
                    FutureVisibility = ct.FutureVisibility
                }).FirstOrDefaultAsync();

            var lastOneYearMonthlyPrices = await _dbContext.LastOneYearMonthlyPriceM
                .Where(lm => lm.CompanyId == company.CompanyId)
                .Select(lm => new LastOneYearMonthlyPriceM
                {
                    Id = lm.Id,
                    CompanyId = lm.CompanyId,
                    Symbol = lm.Symbol,
                    Month = lm.Month,
                    Price = lm.Price
                }).ToListAsync();

            var lastTenYearSales = await _dbContext.LastTenYearSalesM
                .Where(ls => ls.CompanyId == company.CompanyId)
                .Select(ls => new LastTenYearSalesM
                {
                    Id = ls.Id,
                    CompanyId = ls.CompanyId,
                    Year = ls.Year,
                    //Symbol = ls.Symbol,
                    Sales = ls.Sales,
                    OpProfit = ls.OpProfit,
                    NetProfit = ls.NetProfit,
                    OTM = ls.OTM,
                    NPM = ls.NPM,
                    PromotersPercent = ls.PromotersPercent
                }).ToListAsync();

            //return companyDetails;
            responseModel.Data = new CompanyDetailsVM
            {
                CompanyId = company.CompanyId,
                CompanyCount = companyCount,
                Symbol = company.Symbol,
                Name = company.Name,
                Description = company.Description,
                ChartUrl = company.ChartUrl,
                OtherUrl = company.OtherUrl,
                WebsiteUrl = company.WebsiteUrl,
                CompanyType = companyType,
                CommentCount = company.CommentCount,
                LastOneYearMonthlyPrices = lastOneYearMonthlyPrices,
                LastTenYearSales = lastTenYearSales,
                PublishDate = company.PublishDate,
                FaceValue = company.FaceValue,
                PromotersHolding = company.PromotersHolding,
                ProfitGrowth = company.ProfitGrowth,
                CurrentPrice = company.CurrentPrice
            };

            responseModel.StatusCode = HttpStatusCode.OK;
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> GetMessage(int myCompanyDetailId)
        {
            responseModel.StatusCode = HttpStatusCode.OK;
            responseModel.Message = "Success";

            responseModel.Data = await (from company in _dbContext.CompanyDetailMessageM
                                        join user in _dbContext.MobileUsers on company.ModifiedBy equals user.PublicKey
                                        where company.CompanyDetailId == myCompanyDetailId && company.IsActive
                                        orderby company.ModifiedOn descending
                                        select new
                                        {
                                            Id = company.Id,
                                            Message = company.Message,
                                            ModifiedBy = user.FullName,
                                            ModifiedOn = company.ModifiedOn,
                                            PublicKey = user.PublicKey
                                        }).ToListAsync();
            return responseModel;
        }

        public async Task<ApiCommonResponseModel> ManageComment(QueryValues request)
        {
            // add comment
            if (string.IsNullOrEmpty(request.SecondaryKey))
            {
                var comment = new CompanyDetailMessageM
                {
                    CompanyDetailId = request.Id,
                    IsActive = true,
                    Message = request.PrimaryKey,
                    ModifiedOn = DateTime.Now,
                    ModifiedBy = Guid.Parse(request.LoggedInUser)
                };

                _dbContext.CompanyDetailMessageM.Add(comment);
                await _dbContext.SaveChangesAsync();

                var commentId = comment.Id;

                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Added Successfully.";
                responseModel.Data = commentId;
                return responseModel;
            }
            // edit action
            if (request.SecondaryKey.Equals("edit", StringComparison.OrdinalIgnoreCase))
            {
                var comment = _dbContext.CompanyDetailMessageM.Where(c => c.Id == request.Id && c.IsActive == true).FirstOrDefault();
                if (comment is null)
                {
                    responseModel.Message = "Comment Not Found";
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    return responseModel;
                }
                ;

                if (DateTime.Now - comment.ModifiedOn > TimeSpan.FromMinutes(30))
                {
                    responseModel.StatusCode = HttpStatusCode.Forbidden;
                    responseModel.Message = "Cannot Edit.";
                    return responseModel;
                }

                comment.ModifiedOn = DateTime.Now;
                comment.Message = request.PrimaryKey;

                await _dbContext.SaveChangesAsync();

                responseModel.StatusCode = HttpStatusCode.OK;
                responseModel.Message = "Edit Successfull.";
                return responseModel;
            }
            //delete action
            if (request.SecondaryKey.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                var comment = _dbContext.CompanyDetailMessageM.Where(c => c.Id == request.Id && c.IsActive == true).FirstOrDefault();

                if (comment is null)
                {
                    responseModel.Message = "Comment Not Found";
                    responseModel.StatusCode = HttpStatusCode.NotFound;
                    return responseModel;
                }

                if (DateTime.Now - comment.ModifiedOn > TimeSpan.FromMinutes(30))
                {
                    responseModel.StatusCode = HttpStatusCode.Forbidden;
                    responseModel.Message = "Cannot Delete.";
                    return responseModel;
                }

                comment.IsActive = false;
                comment.ModifiedOn = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                responseModel.Message = "Delete Successfull.";
                responseModel.StatusCode = HttpStatusCode.OK;
                return responseModel;
            }

            return responseModel;
        }
    }

    public class CompanyDetailsVM
    {
        public int CompanyId { get; set; }
        public string? Symbol { get; set; }
        public int CompanyCount { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ChartUrl { get; set; }
        public string OtherUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public CompanyTypeM CompanyType { get; set; }
        public int CommentCount { get; set; } = 0;
        public List<LastOneYearMonthlyPriceM> LastOneYearMonthlyPrices { get; set; }
        public List<LastTenYearSalesM> LastTenYearSales { get; set; }
        public DateTime? PublishDate { get; set; }
        public decimal? FaceValue { get; internal set; }
        public decimal? PromotersHolding { get; internal set; }
        public decimal? ProfitGrowth { get; internal set; }
        public decimal? CurrentPrice { get; internal set; }
    }
}