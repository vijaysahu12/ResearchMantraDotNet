using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using Microsoft.Data.SqlClient;
using static RM.Model.Models.SubscriptionModel;

namespace RM.MService.Services
{
    public class SubscriptionPlanService(KingResearchContext repository)
    {
        private readonly KingResearchContext _context = repository;
        private readonly ApiCommonResponseModel _responseModel = new();
        public async Task<ApiCommonResponseModel> GetSubscriptionById(SubscriptionRequestModel request)
        {
            var _finalResponse = new SubscriptionPlanGroupModel
            {
                SubscriptionPlans = new()
            };

            if (request == null && (request.ProductId == 0 && request.SubscriptionPlanId == 0))
            {
                _responseModel.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _responseModel.Message = "Bad Request because param object is null";
                return _responseModel;
            }
            else
            {
                var groupList = new SubscriptionPlanResponseModel
                {
                    SubscriptionDurations = new()
                };
                var subscriptionPlans = await GetSubscriptionPlanWithProduct(request);
                if (request.ProductId > 0 && request.SubscriptionPlanId == 0)
                {
                    // Grouping and transforming data into SubscriptionPlanGroup
                    var groupedByProductIds = subscriptionPlans.GroupBy(s => s.ProductId)
                        .Select(group => new
                        {
                            ProductId = group.Key,
                            Plans = group.ToList()
                        });
                    var defaultCoupon = _context.CouponsM.FirstOrDefault(x => x.IsVisible && x.IsActive);
                    foreach (var product in groupedByProductIds)
                    {
                        groupList.PaymentGatewayName = "INSTAMOJO";//"INSTAMOJO OR PHONEPE"
                        groupList.ProductId = product.ProductId;
                        groupList.SubscriptionPlanId = product.Plans.First().SubscriptionPlanId;
                        groupList.Name = product.Plans.First().PlanName.Trim();
                        product.Plans.ForEach(item =>
                        {
                            groupList.SubscriptionDurations.Add(new SubscriptionDurationDetail
                            {
                                ActualPrice = item.ActualPrice,
                                CouponCode = item.CouponCode,
                                DiscountPrice = item.DiscountPrice,
                                ExpireOn = item.ExpireOn,
                                IsRecommended = item.IsRecommended,
                                Months = item.Months,
                                NetPayment = item.NetPayment,
                                PerMonth = item.PerMonth,
                                SubscriptionDurationId = item.SubscriptionDurationId,
                                SubscriptionDurationName = item.SubscriptionDurationName,
                                SubscriptionMappingId = item.SubscriptionMappingId,
                                DefaultCouponDiscount = defaultCoupon != null ? (defaultCoupon.DiscountInPrice != null ? defaultCoupon.DiscountInPrice : (defaultCoupon.DiscountInPercentage != null
                                ? (decimal?)Math.Round(item.NetPayment * (defaultCoupon.DiscountInPercentage.Value / 100m), 2) : null)) : null
                            });
                        });
                    }
                }
                else if (request.ProductId == 0 && request.SubscriptionPlanId >= 0)
                {
                    // Grouping and transforming data into SubscriptionPlanGroup
                    var groupedByProductIds = subscriptionPlans.GroupBy(s => s.SubscriptionPlanId)
                        .Select(group => new
                        {
                            SubscriptionPlanId = group.Key,
                            ProductName = string.Join(", ", group.Select(s => s.ProductName).Distinct()),
                            Plans = group.ToList()
                        });


                    foreach (var plan in groupedByProductIds)
                    {
                        groupList.SubscriptionPlanId = plan.SubscriptionPlanId;
                        groupList.Name = $"{plan.Plans.First().PlanName.Trim()} : {plan.ProductName}";
                        plan.Plans.ForEach(item =>
                        {
                            groupList.SubscriptionDurations.Add(new SubscriptionDurationDetail
                            {
                                ActualPrice = item.ActualPrice,
                                CouponCode = item.CouponCode,
                                DiscountPrice = item.DiscountPrice,
                                ExpireOn = item.ExpireOn,
                                IsRecommended = item.IsRecommended,
                                Months = item.Months,
                                NetPayment = item.NetPayment,
                                PerMonth = item.PerMonth,
                                SubscriptionDurationName = item.SubscriptionDurationName
                            });
                        });
                    }
                }
                if (groupList.SubscriptionDurations != null && groupList.SubscriptionDurations.Count > 0)
                {
                    _finalResponse.SubscriptionPlans.Add(groupList);
                }
                _responseModel.Data = _finalResponse.SubscriptionPlans;
                _responseModel.StatusCode = System.Net.HttpStatusCode.OK;
                return _responseModel;
            }
        }
        private async Task<List<SubscriptionPlanWithProductSP>> GetSubscriptionPlanWithProduct(SubscriptionRequestModel request)
        {
            var sqlParameters = new List<SqlParameter>
            {
              new SqlParameter { ParameterName = "ProductId", Value = request.ProductId == 0 ? DBNull.Value:  request.ProductId ,SqlDbType = System.Data.SqlDbType.Int},
              new SqlParameter { ParameterName = "SubscriptionPlanId ", Value = request.SubscriptionPlanId == 0 ? DBNull.Value :  request.SubscriptionPlanId ,SqlDbType = System.Data.SqlDbType.Int},
              new SqlParameter { ParameterName = "MobileUserKey", Value = request.MobileUserKey == Guid.Empty ? DBNull.Value :  request.MobileUserKey ,SqlDbType = System.Data.SqlDbType.UniqueIdentifier},
              new SqlParameter { ParameterName = "DeviceType", Value = request.DeviceType ==  null ? DBNull.Value :  request.DeviceType ,SqlDbType = System.Data.SqlDbType.VarChar, Size = 15}
            };
            List<SubscriptionPlanWithProductSP> procedureResponse = await _context.SqlQueryToListAsync<SubscriptionPlanWithProductSP>(ProcedureCommonSqlParametersText.GetSubscriptionPlanWithProduct, sqlParameters.ToArray());
            return procedureResponse;
        }
    }
}