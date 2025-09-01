using RM.CommonServices.Helpers;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.RequestModel;
using RM.Model.ResponseModel;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;
using System.Security.Claims;
namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculatorController : ControllerBase
    {
        private readonly IOtherService _otherSerivce;
        private readonly KingResearchContext _context;

        public CalculatorController(IOtherService otherSerivce, KingResearchContext kingResearchContext)
        {
            _otherSerivce = otherSerivce;
            _context = kingResearchContext;
        }
        [AllowAnonymous]
        [HttpPost("GetFuturePlans")]
        public IActionResult GetFuturePlans()
        {
            var apiCommon = new ApiCommonResponseModel();

            apiCommon.Data = new List<object>
                {
                    new
                    {
                        PlanName = "Retirement Plans",
                        Id = 1,
                        AgeTitle = "Your future, your plan!",
                        AgeLabel = "Enter your age to start planning.",
                        RetirementAge = 60,
                        RetirementTitle = "Retirement Age",
                        CurrentExpensesTitle = "Retirement planning made easy! See your future expenses now!",
                        CurrentExpensesLabel = "Enter your current monthly expenses to get started.",
                        visible= false,
                        apiName = "CalculateMyFuture"
                    },
                    new
                    {
                        visible= true,
                        PlanName = "SIP Calculator",
                        apiName = "CalculateSIP",
                        Id = 2
                    },
                    new
                    {
                        visible= true,
                        apiName = "",
                        PlanName = "Future Child Plan - Coming soon ",
                        Id = 2
                    },
                    new
                    {
                        visible= true,
                        apiName = "Calculate Risk Reward",
                        PlanName = "Risk Reward Calculator",
                        Id = 2
                    },
                    // new
                    //{
                    //    visible= true,
                    //    apiName = "Trading Journal Screen",
                    //    PlanName = "Trading Journal Screen",
                    //    Id = 2
                    //},
                };
            apiCommon.StatusCode = HttpStatusCode.OK;

            return Ok(apiCommon);
        }

        [AllowAnonymous]
        [HttpPost("CalculateMyFuture")]
        public IActionResult CalculateMyFuture([FromBody] FuturePlanningRequest request)
        {
            var apiCommon = new ApiCommonResponseModel();

            if (request == null)
            {
                apiCommon.StatusCode = HttpStatusCode.BadRequest;
                apiCommon.Message = "Invalid Request";
            }

            if (!ModelState.IsValid || request.CurrentAge > 90 || request.RetirementAge > 90 || request.InterestRate > 50)
            {
                apiCommon.StatusCode = HttpStatusCode.BadRequest;
                apiCommon.Message = "Provide valid input";
            }

            apiCommon = CalculateFuturePlan(request);
            return Ok(apiCommon);
        }

        [NonAction]
        private ApiCommonResponseModel CalculateFuturePlan(FuturePlanningRequest request)
        {
            //int currentAge, int retirementAge, double currentMonthlyExpense, double interestRate
            int currentAge = request.CurrentAge;// 40;
            int retirementAge = request.RetirementAge;// 60;
            double currentMonthlyExpense = request.CurrentMonthlyExpense;// 20000; // 20K expense
            double inflationRate = request.InterestRate / 100;// 0.06; // 6% annual interest
            int yearsToRetirement = (retirementAge - currentAge);
            int howManyYearsLefttoTouch60 = 60 - 40;
            int anyCurrentInvestment = request.AnyCurrentInvestment;// 100000;
            int projectedGrowthInterval = 3;
            var obj = new FuturePlanningService();

            // Example 1: Future Value of a Lump Sum Investment
            double fv1 = obj.FutureValue(0.06, yearsToRetirement, 0, currentMonthlyExpense, 0);
            fv1 = Math.Round(fv1);
            Console.WriteLine($"Future Value (Lump Sum): {fv1:C}"); // Output: e.g., $1,489.85

            double futureMonthlyExpenseAtAgeOf60 = obj.FutureValue(inflationRate, yearsToRetirement, 0, currentMonthlyExpense, 0);
            futureMonthlyExpenseAtAgeOf60 = Math.Abs(Math.Round(futureMonthlyExpenseAtAgeOf60));

            double capitalNeededAt60 = obj.PresentValue(.07 / 12, 30 * 12, futureMonthlyExpenseAtAgeOf60, 0);
            capitalNeededAt60 = Math.Abs(Math.Round(capitalNeededAt60));


            //Now Calculate the final MonthlySIP based on any existing investment 
            var listWithExistingInvestment = new List<ListOfInvestmentPlans>
            { new ListOfInvestmentPlans
                {
                    PlanName = "Interest Rate_ Debt_Bank_7%",
                    InterestRate = 7,
                    MonthlySipAmount =currentMonthlyExpense.ToString("0.0"),
                    ProjectedGrowths = []
                },
                new ListOfInvestmentPlans
                {
                    PlanName = "Interest Rate_ Next Nifty_15%",
                    InterestRate = 15,
                    MonthlySipAmount = currentMonthlyExpense.ToString("0.0"),
                    ProjectedGrowths = []
                },
                new ListOfInvestmentPlans
                {
                    PlanName = "Interest Rate_ Next Nifty_16%",
                    InterestRate = 16,
                    MonthlySipAmount = currentMonthlyExpense.ToString("0.0"),
                    ProjectedGrowths = []
                },
                new ListOfInvestmentPlans
                {
                    PlanName = "Interest Rate_ Midcaps_Smallcaps_17%",
                    InterestRate = 17,
                    MonthlySipAmount = currentMonthlyExpense.ToString("0.0"),
                    ProjectedGrowths = []
                }
            };

            var listWithoutExistingInvestment = listWithExistingInvestment;

            foreach (var item in listWithExistingInvestment)
            {
                //item.InterestRate /= 100; //divide by 100
                var sip = obj.Payment(item.InterestRate / 12, howManyYearsLefttoTouch60 * 12, anyCurrentInvestment, capitalNeededAt60, 0);
                item.MonthlySipAmount = Math.Abs(Math.Round(sip)).ToString("N2");
                for (var i = currentAge; i <= retirementAge; i += projectedGrowthInterval)
                {
                    item.ProjectedGrowths.Add(new ProjectedGrowth
                    {
                        Amount = 10000,//ToDo TO calculate the amount
                        Year = i
                    });
                }
            }

            foreach (var item in listWithoutExistingInvestment)
            {
                //item.InterestRate /= 100; //divide by 100
                var sip = obj.Payment(item.InterestRate / 12, howManyYearsLefttoTouch60 * 12, 0, capitalNeededAt60, 0);
                item.MonthlySipAmount = Math.Abs(Math.Round(sip)).ToString("N2");
                for (var i = currentAge; i <= retirementAge; i += projectedGrowthInterval)
                {
                    item.ProjectedGrowths.Add(new ProjectedGrowth
                    {
                        Amount = 10000,
                        Year = i
                    });
                }
            }

            var apicommonResponse = new ApiCommonResponseModel();
            var result = new
            {
                currentAge = currentAge,
                inflationRate = inflationRate,
                yearsToRetirement = yearsToRetirement,
                howManyYearsLefttoTouch60 = howManyYearsLefttoTouch60,
                currentMonthlyExpense = currentMonthlyExpense.ToString("N2"),
                futureMonthlyExpenseAtAgeOf60 = futureMonthlyExpenseAtAgeOf60.ToString("N2"),
                capitalNeededAt60 = capitalNeededAt60.ToString("N2"),
                anyCurrentInvestment = anyCurrentInvestment == 0 ? "No" : "Yes , " + anyCurrentInvestment.ToString("N2"),
                InvestmentPlansWithExistingInvestment = listWithExistingInvestment,
                InvestmentPlansWithoutAnyExistingInvestment = listWithoutExistingInvestment,
                summaryLabel1 = "To Acheive your goal, you have to invest amount from today onwards ",
                summaryLabel2 = $"Projected wealth growth (Every {projectedGrowthInterval} years)",
                AllowPdf = false
            };

            apicommonResponse.Data = result;
            apicommonResponse.StatusCode = System.Net.HttpStatusCode.OK;
            return apicommonResponse;
        }// end block 

        [HttpPost("CalculateSIP")]
        public IActionResult CalculateSIP([FromBody] SIPRequest request)
        {
            if (request.MonthlyInvestment <= 0 || request.InvestmentPeriod <= 0)
            {
                return BadRequest("All input values must be greater than zero.");
            }


            List<SIPProjection> projections = new List<SIPProjection>();
            double totalInvested = 0;
            //var obj = new FuturePlanningService();

            double monthlySipAmount = request.MonthlyInvestment;
            double totalFutureValue = 0.0;
            double annualIncreaseRate = (double)request.IncrementalRate / 100; // e.g., 10%
            double averageReturn = (double)request.AnnualReturns / 100;
            double monthlyReturnRate = (double)averageReturn / 12;
            double previousFutureValue = 0.0;

            for (int year = 1; year <= request.InvestmentPeriod; year++)
            {
                double yearlyInvestment = monthlySipAmount * 12;
                totalInvested += yearlyInvestment;

                double futureValueForYear = 0;
                var obj = new FuturePlanningService();

                if (year == 1)
                {
                    futureValueForYear = obj.FutureValue(monthlyReturnRate, 1 * 12, monthlySipAmount, 0, 1);
                }
                else
                {
                    futureValueForYear = obj.FutureValue(monthlyReturnRate, 1 * 12, monthlySipAmount, previousFutureValue, 1);
                }
                //for (int month = 1; month <= 12; month++)
                //{
                //    int monthsLeft = (request.InvestmentPeriod - year) * 12 + (12 - month + 1);
                //    futureValueForYear += monthlySipAmount * Math.Pow(1 + monthlyReturnRate, monthsLeft);
                //}

                //   = FV(E8 %/ 12, 1 * 12, -F8, 0, 1)
                previousFutureValue =Math.Abs(futureValueForYear);

                totalFutureValue = futureValueForYear;

                projections.Add(new SIPProjection
                {
                    Duration = year,
                    SIPAmount = Math.Round(monthlySipAmount),
                    InvestedAmount = Math.Round(yearlyInvestment),
                    TotalInvestedAmount = Math.Round(totalInvested),
                    FutureValue = Math.Round(futureValueForYear),
                    IncrementalRateInFuture = request.IncrementalRate
                });

                // 💡 Increment SIP *after* this year’s future value is calculated
                monthlySipAmount *= (1 + annualIncreaseRate);
            }


            var response = new SIPResponse
            {
                ExpectedAmount = Math.Round(totalFutureValue, 2),
                AmountInvested = Math.Round(totalInvested, 2),
                WealthGain = Math.Round(totalFutureValue - totalInvested, 2),
                IncrementalRateInFuture = 0.0,
                ProjectedSipReturnsTable = projections
            };

            return Ok(new ApiCommonResponseModel
            {
                Data = response,
                StatusCode = HttpStatusCode.OK,
                Message = "SIP calculation completed. (Expected amount is the nominal future value, inflation adjustment applied only in the projection table)."
            });
        }


        [HttpPost("CalculateRiskReward")]
        public IActionResult CalculateRiskReward([FromBody] TradeSettings request)
        {
            if (request == null)
            {
                return BadRequest(new ApiCommonResponseModel
                {
                    Data = null,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid request."
                });
            }

            bool hasInvalidInputs = request.CapitalAmount <= 0 ||
                                    request.EntryPrice <= 0 ||
                                    request.StopLoss <= 0 ||
                                    request.TargetPrice <= 0 ||
                                    request.RiskFactor <= 0;

            if (hasInvalidInputs)
            {
                return BadRequest(new ApiCommonResponseModel
                {
                    Data = null,
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "All input values must be valid and greater than zero."
                });
            }

            double riskAmount = request.CapitalAmount * (request.RiskFactor / 100);
            int recommendedQuantity;
            double riskRewardRatio;
            double profitAndLoss;

            if (request.IsBuy)
            {
                if (request.EntryPrice <= request.StopLoss || request.TargetPrice <= request.EntryPrice)
                {
                    return BadRequest(new ApiCommonResponseModel
                    {
                        Data = null,
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "For Buy: Entry must be > StopLoss and Target > Entry."
                    });
                }

                recommendedQuantity = (int)Math.Floor(riskAmount / (request.EntryPrice - request.StopLoss));
                profitAndLoss = (request.TargetPrice - request.EntryPrice) * recommendedQuantity;
                riskRewardRatio = (request.TargetPrice - request.EntryPrice) / (request.EntryPrice - request.StopLoss);
            }
            else // Sell
            {
                if (request.StopLoss <= request.EntryPrice || request.EntryPrice <= request.TargetPrice)
                {
                    return BadRequest(new ApiCommonResponseModel
                    {
                        Data = null,
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "For Sell: StopLoss must be > Entry and Entry > Target."
                    });
                }

                recommendedQuantity = (int)Math.Floor(riskAmount / (request.StopLoss - request.EntryPrice));
                profitAndLoss = (request.EntryPrice - request.TargetPrice) * recommendedQuantity;
                riskRewardRatio = (request.EntryPrice - request.TargetPrice) / (request.StopLoss - request.EntryPrice);
            }

            var result = new RRC
            {
                RiskAmount = Math.Round(riskAmount, 2),
                RecommendedQuantity = recommendedQuantity,
                TargetPrice = request.TargetPrice,
                ProfitAndLoss = Math.Round(profitAndLoss, 2),
                RiskRewardRatio = $"1:{Math.Round(riskRewardRatio, 1)}"
            };

            return Ok(new ApiCommonResponseModel
            {
                Data = result,
                StatusCode = HttpStatusCode.OK,
                Message = "Risk-reward calculation completed."
            });
        }

        [HttpPost("CreateTradeJournal")]
        public async Task<ApiCommonResponseModel> CreateTradeEntry([FromBody] TradeJournal tradeJournal)
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            if (tradeJournal == null)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid input data.",
                    Data = null
                };
            }

            if (tradeJournal?.Id == null || tradeJournal.Id == 0) // Create New Trade Journal
            {
                tradeJournal.CreatedOn = DateTime.Now;
                tradeJournal.CreatedBy = userPublicKey;
                tradeJournal.IsActive = true;
                tradeJournal.IsDeleted = false;

                _context.TradeJounal.Add(tradeJournal);
                await _context.SaveChangesAsync();

                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Trade Journal created successfully.",
                    Data = new
                    {
                        tradeJournal.Id
                    }
                };
            }
            else // Update Existing Trade Journal
            {
                var existingTradeJournal = await _context.TradeJounal.FindAsync(tradeJournal.Id);

                if (existingTradeJournal == null || existingTradeJournal.IsDeleted)
                {
                    return new ApiCommonResponseModel
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = "Trade Journal not found.",
                        Data = null
                    };
                }

                // Update properties
                existingTradeJournal.StartDate = tradeJournal.StartDate;
                existingTradeJournal.Symbol = tradeJournal.Symbol;
                existingTradeJournal.BuySellButton = tradeJournal.BuySellButton;
                existingTradeJournal.CapitalAmount = tradeJournal.CapitalAmount;
                existingTradeJournal.RiskPercentage = tradeJournal.RiskPercentage;
                existingTradeJournal.RiskAmount = tradeJournal.RiskAmount;
                existingTradeJournal.EntryPrice = tradeJournal.EntryPrice;
                existingTradeJournal.StopLoss = tradeJournal.StopLoss;
                existingTradeJournal.Target1 = tradeJournal.Target1;
                existingTradeJournal.Target2 = tradeJournal.Target2;
                existingTradeJournal.PositionSize = tradeJournal.PositionSize;
                existingTradeJournal.ActualExitPrice = tradeJournal.ActualExitPrice;
                existingTradeJournal.ProfitLoss = tradeJournal.ProfitLoss;
                existingTradeJournal.RiskReward = tradeJournal.RiskReward;
                existingTradeJournal.Notes = tradeJournal.Notes;
                existingTradeJournal.ModifiedBy = userPublicKey;
                existingTradeJournal.ModifiedOn = DateTime.Now;

                await _context.SaveChangesAsync();

                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Trade Journal updated successfully.",
                    Data = existingTradeJournal
                };
            }
        }

        [HttpGet("GetPagedTradeJournal")]
        public async Task<ApiCommonResponseModel> GetPagedTradeJournal([FromQuery] QueryValues queryValues)
        {
            var apiCommonResponse = new ApiCommonResponseModel();

            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@MobileUserKey", queryValues.PrimaryKey),
                new SqlParameter("@PageNumber", (object?)queryValues.PageNumber ?? DBNull.Value),
                new SqlParameter("@PageSize", (object?)queryValues.PageSize ?? DBNull.Value),
                new SqlParameter("@FromDate", (object?)queryValues.FromDate?.Date ?? DBNull.Value),
                new SqlParameter("@ToDate", (object?)queryValues.ToDate?.Date.AddDays(1).AddMilliseconds(-1) ?? DBNull.Value),
                new SqlParameter
                {
                    ParameterName = "@TotalCount",
                    SqlDbType = SqlDbType.Int,
                    Direction = ParameterDirection.Output
                }
            };

            var tradeJournalList = await _context.SqlQueryToListAsync<TradeJournalDto>(
                "EXEC GetTradeJournalsByMobileUserKey @MobileUserKey, @PageNumber, @PageSize, @FromDate, @ToDate, @TotalCount OUTPUT",
                sqlParameters.ToArray()
            );

            int totalRecords = sqlParameters
                .FirstOrDefault(p => p.ParameterName == "@TotalCount")?.Value is int count ? count : 0;

            apiCommonResponse.StatusCode = HttpStatusCode.OK;
            apiCommonResponse.Total = totalRecords;
            apiCommonResponse.Data = tradeJournalList;

            return apiCommonResponse;
        }


        //[HttpPut("Update/{id}")]
        //public async Task<ApiCommonResponseModel> UpdateTradeEntry(int id, [FromBody] TradeJournal updatedTradeJournal)
        //{
        //    int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

        //    // Check if the trade entry exists
        //    var existingTradeJournal = await _context.TradeJounal.FindAsync(id);
        //    if (existingTradeJournal == null || existingTradeJournal.IsDeleted)
        //    {
        //        return new ApiCommonResponseModel
        //        {
        //            StatusCode = HttpStatusCode.OK,
        //            Message = "Trade Journal not found.",
        //            Data = null
        //        };
        //    }

        //    // Update properties
        //    existingTradeJournal.StartDate = updatedTradeJournal.StartDate;
        //    existingTradeJournal.Symbol = updatedTradeJournal.Symbol;
        //    existingTradeJournal.BuySellButton = updatedTradeJournal.BuySellButton;
        //    existingTradeJournal.CapitalAmount = updatedTradeJournal.CapitalAmount;
        //    existingTradeJournal.RiskPercentage = updatedTradeJournal.RiskPercentage;
        //    existingTradeJournal.RiskAmount = updatedTradeJournal.RiskAmount;
        //    existingTradeJournal.EntryPrice = updatedTradeJournal.EntryPrice;
        //    existingTradeJournal.StopLoss = updatedTradeJournal.StopLoss;
        //    existingTradeJournal.Target1 = updatedTradeJournal.Target1;
        //    existingTradeJournal.Target2 = updatedTradeJournal.Target2;
        //    existingTradeJournal.PositionSize = updatedTradeJournal.PositionSize;
        //    existingTradeJournal.ActualExitPrice = updatedTradeJournal.ActualExitPrice;
        //    existingTradeJournal.ProfitLoss = updatedTradeJournal.ProfitLoss;
        //    existingTradeJournal.RiskReward = updatedTradeJournal.RiskReward;
        //    existingTradeJournal.Notes = updatedTradeJournal.Notes;
        //    existingTradeJournal.ModifiedBy = userPublicKey;
        //    existingTradeJournal.ModifiedOn = DateTime.Now;

        //    // Save changes to the database
        //    await _context.SaveChangesAsync();

        //    return new ApiCommonResponseModel
        //    {
        //        StatusCode = HttpStatusCode.OK,
        //        Message = "Trade Journal updated successfully.",
        //        Data = existingTradeJournal
        //    };
        //}

        [HttpDelete("Delete/{id}")]
        public async Task<ApiCommonResponseModel> DeleteTradeEntry(int id)
        {
            int userPublicKey = int.Parse(UserClaimsHelper.GetClaimValue(User, ClaimTypes.PrimarySid));

            // Find the trade entry by ID
            var tradeJournal = await _context.TradeJounal.FindAsync(id);

            if (tradeJournal == null || tradeJournal.IsDeleted)
            {
                return new ApiCommonResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Trade Journal not found.",
                    Data = null
                };
            }

            // Mark as deleted by setting IsDeleted to true
            tradeJournal.IsDeleted = true;
            tradeJournal.IsActive = false;
            tradeJournal.ModifiedBy = userPublicKey;
            tradeJournal.ModifiedOn = DateTime.Now;

            // Save changes
            await _context.SaveChangesAsync();

            return new ApiCommonResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Message = "Trade Journal as deleted successfully.",
                Data = tradeJournal
            };
        }




    }
}

