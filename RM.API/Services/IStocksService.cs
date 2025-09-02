using RM.Database.Extension;
using RM.Database.ResearchMantraContext;
using RM.Model;
using RM.Model.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface IStocksService
    {
        Task<ApiCommonResponseModel> ManageNFOContracts(NFOModel contracts);
        Task<ApiCommonResponseModel> ManageNSEContracts(NSEModel contracts);
    }

    public class StocksService : IStocksService
    {
        private readonly ResearchMantraContext _context;

        public StocksService(ResearchMantraContext context)
        {
            _context = context;
        }

        ApiCommonResponseModel responseModel = new();
        public async Task<ApiCommonResponseModel> ManageNFOContracts(NFOModel contracts)
        {
            try
            {

                var c = new List<StockImports>();
                if (contracts != null)
                {
                    var contractList = contracts.NFO;

                    List<StockImports> finalOutPut = contractList.Select(item => new StockImports
                    {
                        Token = item.token,
                        Exch = item.exch,
                        LotSize = item.lot_size,
                        Symbol = item.symbol,
                        OptionType = item.option_type,
                        StrikePrice =    (item.strike_price),
                        TradingSymbol = item.trading_symbol ?? item.symbol,
                        ContractDate = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(item.expiry_date)).UtcDateTime,
                        Month = null// Convert.ToDateTime(DateTimeOffset.FromUnixTimeMilliseconds((long)item.expiry_date).UtcDateTime).ToString("MMM")
                    }).ToList();

                    if (finalOutPut.Count > 0)
                    {
                        await _context.StockImport.AddRangeAsync(finalOutPut.OrderBy(item => item.ContractDate));
                        await _context.SaveChangesAsync();
                        _ = await _context.SqlQueryToListAsync<ImportBulkStocks>("Exec ImportBlulkStocks null");
                    }

                    responseModel.Data = null;
                    return responseModel;

                }


            }
            catch (Exception ex)
            {
                var dd = ex;
                responseModel.Message = ex.Message;
                return responseModel;
            }

            return new ApiCommonResponseModel();
        }


        public async Task<ApiCommonResponseModel> ManageNSEContracts(NSEModel contracts)
        {

            try
            {
                if (contracts != null)
                {
                    var contractList = contracts.NSE;
                    List<StockImports> finalOutPut = contractList.Select(item => new StockImports
                    {

                        Token = item.token,
                        Exch = item.exch,
                        LotSize = item.lot_size,
                        Symbol = item.symbol,
                        OptionType = item.option_type,
                        StrikePrice =     (item.strike_price),
                        TradingSymbol = item.trading_symbol ?? item.symbol,
                        ContractDate = DateTime.Now,
                        Month = null
                    }).ToList();
                    if (finalOutPut.Count > 0)
                    {
                        await _context.StockImport.AddRangeAsync(finalOutPut.OrderBy(item => item.ContractDate));
                        await _context.SaveChangesAsync();
                        _ = await _context.SqlQueryToListAsync<ImportBulkStocks>("Exec ImportBlulkStocks null");
                    }
                }


            }
            catch (Exception ex)
            {

            }

            return new ApiCommonResponseModel();
        }







    }
}
