using RM.CommonServices;
using RM.CommonServices.Services;
using RM.Database.MongoDbContext;
using RM.MService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RM.MobileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreMarketController : ControllerBase
    {
        private readonly PreAndPostMarketService _marketService;

        public PreMarketController(PreAndPostMarketService marketService)
        {
             _marketService = marketService;
        }

        [HttpPost("ManagePreMarketData")]
        public async Task<IActionResult> Post(PreMarketReport.PreMarketCollection collection)
        {
            var result = await _marketService.InsertMarketDataAsync(collection);
            return Ok(result);
        }

        [HttpGet("GetAllPreMarket")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _marketService.GetPaginatedMarketDataAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("GetByDateTime")]
        public async Task<IActionResult> Get(DateTime dateTime)
        {
            var result = await _marketService.GetPreMarketDataByDateAsync(dateTime);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("GetPreMarketById")]
        public async Task<IActionResult> Get(string objectId)
        {
            var result = await _marketService.GetPreMarketDataByObjectIdAsync(objectId);
            return Ok(result);
        }


        [HttpPut]
        public async Task<IActionResult> Put()
        {
            string _directoryPath = @"D:\\SymbolData\pdfFileName" + DateTime.Now.ToString("ddMMYYYYhhmmss") + ".pdf";
            // await _marketService.GeneratePreMarketPulsePdfAsync(DateTime.Now, _directoryPath);
            return Ok();
        }
 

        //Method to post market data
        [HttpPost("ManagePostMarketData")]
        public async Task<IActionResult> ManagePostMarketData(PostMarketReport.PostMarketCollection data)
        {
            var res = await _marketService.ManagePostMarketData(data);
            return Ok(res);
        }

        [HttpGet("GetPostMarketById")]
        public async Task<IActionResult> GetPostMarketBYId(string objectId)
        {
            var result = await _marketService.GetPostMarketDataByObjectIdAsync(objectId);
            return Ok(result);
        }

        [HttpGet("GetAllPostMarketData")]
        public async Task<IActionResult> GetAllPostMarketData(int pageNumber = 1, int pageSize = 10)
        {
            var res = await _marketService.GetPaginatedPostMarketDataAsync(pageNumber, pageSize);
            return Ok(res);
        }

    }
}
