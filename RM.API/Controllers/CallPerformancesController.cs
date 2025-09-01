using AutoMapper;
using RM.API.Dtos;
using RM.API.Helpers;
using RM.API.Models.Reports;
using RM.API.Services;
using RM.Database.Constants;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.RequestModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CallPerformancesController : ControllerBase
    {
        private readonly KingResearchContext _context;
        private readonly ICallPerformanceService _ICallPerformance;

        private readonly IMapper _mapper;
        public CallPerformancesController(KingResearchContext context, IMapper mapper, ICallPerformanceService callPerformance)
        {
            _mapper = mapper;
            _context = context;
            _ICallPerformance = callPerformance;
        }

        [HttpGet]
        public async Task<IActionResult> GetCallPerformance(string Search, string CallBy, string StrategyKey, DateTime StartDate, DateTime EndDate, int PageNumber, int PageSize, string SortOrder, string SortBy)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            object result = await _ICallPerformance.GetCallPerformance(Search, CallBy, StrategyKey, StartDate, EndDate, PageNumber, PageSize, SortOrder, SortBy, tokenVariables);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetCallPerformanceExcel")]
        public async Task<IActionResult> GetCallPerformanceExcel(string Search, string CallBy, string StrategyKey, DateTime StartDate, DateTime EndDate, int PageNumber, int PageSize, string SortOrder, string SortBy)
        {
            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }
            object result = await _ICallPerformance.GetCallPerformanceExcel(Search, CallBy, StrategyKey, StartDate, EndDate, PageNumber, PageSize, SortOrder, SortBy, tokenVariables);
            return Ok(result);
        }

        // GET: api/CallPerformances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CallPerformance>> GetCallPerformance(string id)
        {
            _ = Guid.TryParse(id, out Guid newGuid);

            if (newGuid != Guid.Empty)
            {
                CallPerformance callPerformance = await _ICallPerformance.GetCallById(Guid.Parse(id));
                return Ok(callPerformance ?? new CallPerformance());
            }
            else
            {
                return Ok();
            }
        }

        // PUT: api/CallPerformances/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        // updates a record in callPerformance
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCallPerformance(string id, [FromForm] CallPerformanceDto callPerformance)
        {
            if (id.ToLower() != callPerformance.PublicKey.ToString().ToLower())
            {
                return BadRequest();
            }

            TokenAnalyser tokenAnalyser = new();
            TokenVariables tokenVariables = null;
            if (HttpContext.User.Identity is ClaimsIdentity identity)
            {
                tokenVariables = tokenAnalyser.FetchTokenValues(identity);
            }

            if (callPerformance.ExitPrice is not null and not 0)
            {
                double exitP = 0.0;

                if (callPerformance.TradeType.ToLower() == "buy")
                {
                    exitP = (double)(callPerformance.ExitPrice >= callPerformance.Target1Price ? callPerformance.Target1Price : callPerformance.ExitPrice);
                    callPerformance.Pnl = callPerformance.LotSize * (Convert.ToDecimal(exitP) - (decimal)callPerformance.EntryPrice);
                }
                else
                {
                    exitP = (double)(callPerformance.ExitPrice <= callPerformance.Target1Price ? callPerformance.Target1Price : callPerformance.ExitPrice);
                    callPerformance.Pnl = callPerformance.LotSize * (Convert.ToDecimal(callPerformance.EntryPrice) - Convert.ToDecimal(exitP));
                }
            }
            else
            {
                callPerformance.Pnl = 0;
                callPerformance.Roi = null;
            }

            callPerformance.ModifiedOn = DateTime.Now;
            callPerformance.ModifiedBy = tokenVariables.PublicKey;

            var imageUrl = "";
            if (callPerformance.Image is not null)
            {
                var gdrive = new GdriveImageUpload();
                imageUrl = await gdrive.UploadImageToGDriveAsync(callPerformance.Image);

            }
            CallPerformance callPerformanceEntry = new()
            {
                Id = callPerformance.Id,
                CallByKey = callPerformance.CallByKey,
                CallDate = callPerformance.CallDate,
                CallStatus = callPerformance.CallStatus,
                CreatedBy = callPerformance.CreatedBy,
                CreatedOn = callPerformance.CreatedOn,
                EntryPrice = callPerformance.EntryPrice,
                EntryTime = callPerformance.EntryTime,
                ExitPrice = callPerformance.ExitPrice,
                ExpiryKey = callPerformance.ExpiryKey,
                ImageUrl = callPerformance.Image is not null ? imageUrl : null,
                IsCallActivate = callPerformance.IsCallActivate,
                IsDelete = callPerformance.IsDelete,
                IsDisabled = callPerformance.IsDisabled,
                IsIntraday = callPerformance.IsIntraday,
                IsPublic = callPerformance.IsPublic,
                LotSize = callPerformance.LotSize,
                ModifiedBy = callPerformance.ModifiedBy,
                ModifiedOn = callPerformance.ModifiedOn,
                OptionValue = callPerformance.OptionValue,
                PlottedCapital = callPerformance.PlottedCapital,
                Pnl = callPerformance.Pnl,
                PublicKey = callPerformance.PublicKey,
                Remarks = callPerformance.Remarks,
                ResultHigh = callPerformance.ResultHigh,
                ResultTypeKey = callPerformance.ResultTypeKey,
                Roi = callPerformance.Roi,
                SegmentKey = callPerformance.SegmentKey,
                StockKey = callPerformance.StockKey,
                StopLossPrice = callPerformance.StopLossPrice,
                StrategyKey = callPerformance.StrategyKey,
                Target1Price = callPerformance.Target1Price,
                Target2Price = callPerformance.Target2Price,
                Target3Price = callPerformance.Target3Price,
                TradeType = callPerformance.TradeType,
                TriggerTime = callPerformance.TriggerTime

            };
            _context.Entry(callPerformanceEntry).State = Microsoft.EntityFrameworkCore.EntityState.Modified;



            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CallPerformance> entry = _context.Entry(callPerformanceEntry);
            entry.Property(e => e.Id).IsModified = false; entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;
            if (callPerformance.Image == null)
            {
                entry.Property(e => e.ImageUrl).IsModified = false;
            }
            else
            {
                entry.Property(e => e.ImageUrl).IsModified = true;
            }



            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CallPerformanceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CallPerformances
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<CallPerformance>> PostCallPerformance([FromForm] CallPerformanceRequestModel callPerformance)
        {
            try
            {
                string? imageUrl = "";
                if (callPerformance.Image is not null)
                {
                    var gDrive = new GdriveImageUpload();
                    imageUrl = await gDrive.UploadImageToGDriveAsync(callPerformance.Image);
                }

                TokenAnalyser tokenAnalyser = new();
                TokenVariables tokenVariables = null;
                if (HttpContext.User.Identity is ClaimsIdentity identity)
                {
                    tokenVariables = tokenAnalyser.FetchTokenValues(identity);
                }
                string LoginUser = tokenVariables.PublicKey;
                string LoginUserRole = tokenVariables.RoleKey;


                CallPerformance callPer = new();
                CallPerformance mappingObject = _mapper.Map<CallPerformance>(callPerformance);
                mappingObject.CallByKey = callPerformance.CallByKey;
                mappingObject.CreatedOn = DateTime.Now;
                mappingObject.ModifiedOn = DateTime.Now;
                mappingObject.CreatedBy = LoginUser;
                mappingObject.ModifiedBy = LoginUser;
                mappingObject.IsCallActivate = callPerformance.IsCallActivate;
                mappingObject.IsDelete = false;
                mappingObject.ImageUrl = imageUrl ?? null;


                _ = _context.CallPerformances.Add(mappingObject);
                _ = await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return CreatedAtAction("GetCallPerformance", new { id = callPerformance.Id }, callPerformance);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("report")]
        public async Task<IActionResult> CallPerformance([FromBody] CallPerformanceReportRequest request)
        {
            try
            {
                SqlParameter TotalM2MWithHighPrice = new()
                {
                    ParameterName = "TotalM2MWithHighPrice ",
                    Direction = System.Data.ParameterDirection.Output,
                    SqlDbType = System.Data.SqlDbType.Float,
                };

                SqlParameter TotalM2MWithExitPrice = new()
                {
                    ParameterName = "TotalM2MWithExitPrice ",
                    Direction = System.Data.ParameterDirection.Output,
                    SqlDbType = System.Data.SqlDbType.Float,
                };

                SqlParameter[] sqlParameters = new[]
                {
                new SqlParameter
                {
                    ParameterName = "CallBy",
                    Value = request.CallBy ?? Convert.DBNull ,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                } ,new SqlParameter
                {
                    ParameterName = "StrategyKey",
                    Value = request.StrategyKey ?? Convert.DBNull ,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                },
                TotalM2MWithHighPrice,
                TotalM2MWithExitPrice
            };

                System.Collections.Generic.List<CallPerformanceDto> result = await _context.SqlQueryToListAsync<CallPerformanceDto>(ProcedureCommonSqlParametersText.GetCallPerformanceReport, sqlParameters);
                CallPerformanceReportResponse output = new()
                {
                    PerformanceReport = result,
                    TotalM2MWithExitPrice = Convert.ToDecimal(TotalM2MWithExitPrice.Value),
                    TotalM2MWithHighPrice = Convert.ToDecimal(TotalM2MWithHighPrice.Value)
                };
                return Ok(output);
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }


        [HttpPost]
        [Route("GetCallsTopBuzzerReports")]
        public async Task<IActionResult> GetCallsTopBuzzerReports(CallPerformanceReportRequest request)
        {
            Model.ApiCommonResponseModel result = await _ICallPerformance.GetCallsTopBuzzerReports(request);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetCallsTopPerformersReports")]
        public async Task<IActionResult> GetCallsTopPerformersReports(CallPerformanceReportRequest request)
        {
            Model.ApiCommonResponseModel result = await _ICallPerformance.GetCallsTopPerformersReports(request);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetCallsSummaryReports")]
        public async Task<IActionResult> GetCallsSummaryReports(CallPerformanceReportRequest request)
        {
            Model.ApiCommonResponseModel result = await _ICallPerformance.GetCallsSummaryReports(request);
            return Ok(result);
        }

        private bool CallPerformanceExists(string id)
        {
            return _context.CallPerformances.Any(e => e.PublicKey.ToString() == id);
        }


        [HttpPost("GetCallPerformanceHeatMapData")]
        public async Task<IActionResult> GetCallPerformanceHeatMapData(QueryValues queryValues)
        {
            Model.ApiCommonResponseModel result = await _ICallPerformance.GetCallPerformanceHeatMapData(queryValues);
            return Ok(result);
        }

        [HttpPost("GetCallDetails")]
        public async Task<IActionResult> GetCallDetails(QueryValues queryValues)
        {
            ApiCommonResponseModel responseModel = await _ICallPerformance.GetCallDetails(queryValues);
            return Ok(responseModel);
        }

        [AllowAnonymous]
        [HttpPost("CallPerformanceBySusmita")]
        public async Task<IActionResult> GetCallPerformanceBySusmita()
        {
            var hh = await _context.CallPerformances.Where(item => item.CallByKey == "8DD53404-F2CD-ED11-8110-00155D23D79C" && item.IsDelete != false && item.IsPublic == true).ToListAsync();
            return Ok(hh);
        }
    }
}