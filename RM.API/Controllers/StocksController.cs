using RM.API.Services;
using RM.CommonServices;
using RM.Database.Extension;
using RM.Database.KingResearchContext;
using RM.Model;
using RM.Model.Common;
using RM.Model.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly KingResearchContext _context;
        private readonly IStocksService _stocksService;
        private readonly StockMarketContractsService _stockMarketContractsService;
        //to read   c drive file C:\MyFiles
        private readonly string _directoryPath = @"D:\\SymbolData";
        public StocksController(KingResearchContext context, IStocksService stocksService, StockMarketContractsService _IStockMarketContractsService)
        {
            _context = context;
            _stocksService = stocksService;
            _stockMarketContractsService = _IStockMarketContractsService;
        }

        // GET: api/Stocks
        private string GetContentType(string fileExtension)
        {
            // Determine content type based on file extension
            return fileExtension.ToLower() switch
            {
                ".txt" => "text/plain",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream",
            };
        }
        // GET: api/Stocks
        [HttpGet]
        public async Task<ActionResult> GetStocks()
        {
            try
            {
                var res = _stockMarketContractsService.FilterData("a");
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/Stocks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Stock>> GetStocks(string id)
        {
            var filepath = Path.Combine(_directoryPath, "symboldata.txt");

            var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            var stockList = System.Text.Json.JsonSerializer.DeserializeAsyncEnumerable<SymbolList>(fileStream);

            var dd = stockList.ToJson();
            int count = 0;
            await foreach (var stock in stockList)
            {
                if (stock.Tsym.StartsWith(id))
                {
                    count++;
                }
            }
            return Ok(dd);
        }




        // PUT: api/Stocks/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStocks(int id, Stock stocks)
        {
            if (id != stocks.Id)
            {
                return BadRequest();
            }

            _context.Entry(stocks).State = EntityState.Modified;
            _ = _context.Entry(stocks);


            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StocksExists(id))
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

        // POST: api/Stocks
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Stock>> PostStocks(Stock stocks)
        {

            Stock isExists = await _context.Stocks.FirstOrDefaultAsync(item => item.Name == stocks.Name);
            if (isExists != null)
            {
                return Ok(HttpStatusCode.Ambiguous);
            }

            _ = _context.Stocks.Add(stocks);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetStocks", new { id = stocks.Id }, stocks);
        }

        private bool StocksExists(int id)
        {
            return _context.Stocks.Any(e => e.Id == id);
        }

        [AllowAnonymous]
        [HttpGet("GetNFOContracts")]
        public async Task<IActionResult> GetNFOContracts()
        {
            ApiCommonResponseModel apiCommonResponse = new();
            HttpClientHandler handler = new()
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };
            using (HttpClient httpClient = new(handler))
            {
                List<string> contractsList = new()
                {
                    "NFO",
                    "NSE"
                };

                foreach (string exchange in contractsList)
                {

                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                    // Define the URL you want to send the GET request to
                    string apiUrl = "https://v2api.aliceblueonline.com/restpy/contract_master?exch=" + exchange.ToUpper();
                    // Send the GET request and get the response
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                    // Check if the request was successful (status code 200)
                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();

                        if (apiUrl.Contains("NFO"))
                        {
                            NFOModel objNSO = JsonConvert.DeserializeObject<NFOModel>(responseData);
                            await _stocksService.ManageNFOContracts(objNSO);

                        }
                        else if (apiUrl.Contains("NSE"))
                        {
                            NSEModel objNSO = JsonConvert.DeserializeObject<NSEModel>(responseData);
                            await _stocksService.ManageNSEContracts(objNSO);

                        }
                    }
                }
            }
            return Ok(apiCommonResponse);
        }


        [HttpPost("GetFilteredStocks")]
        public async Task<ApiCommonResponseModel> GetFilterdStocks(QueryValues queryValues)
        {
            ApiCommonResponseModel responseModel = new()
            {
                Data = await _stockMarketContractsService.FilterData(queryValues.PrimaryKey),
                StatusCode = HttpStatusCode.OK
            };
            return responseModel;
        }

        [HttpGet("FreshContractsBinding")]
        public async Task<IActionResult> FreshContractBinding()
        {
            await _stockMarketContractsService.LoadStockDetailsFromFile(true);
            return Ok();
        }

        // GET: api/Stocks/5
        [HttpGet]
        [Route("GetFilteredStocksForCallPerformance")]
        public async Task<IActionResult> GetFilteredStocksForCallPerformance(string name)
        {
            SqlParameter[] sqlParameters = new[]
            {
               new SqlParameter { ParameterName = "searchText",Value = name ,SqlDbType = System.Data.SqlDbType.VarChar},
            };

            List<ContractResponseModel> stocks = await _context.SqlQueryToListAsync<ContractResponseModel>("GetFilteredContracts @searchText = {0}", sqlParameters);

            return Ok(stocks);
        }
    }
}