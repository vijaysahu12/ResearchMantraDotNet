using RM.Database.ResearchMantraContext;
using RM.Model.ResponseModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using static RM.Database.ResearchMantraContext.ScreenerTables;
using static RM.Model.ResponseModel.ScreenerModel;

namespace RM.API.Services
{

    internal interface IScreenerService
    {
        Task<IEnumerable<ScreenerCategoryM>> GetAllAsync();
        Task<ScreenerCategoryM> GetByIdAsync(int id);
        Task<ScreenerCategoryM> AddAsync(ScreenerCategoryM category);
        Task<ScreenerCategoryM> UpdateAsync(ScreenerCategoryM category);
        Task<bool> DeleteAsync(int id);
    }

    public class ScreenerServiceApi : IScreenerService
    {
        private readonly ResearchMantraContext _dbContext;

        public ScreenerServiceApi(ResearchMantraContext repository)
        {
            _dbContext = repository;
        }
        public async Task<IEnumerable<ScreenerCategoryM>> GetAllAsync()
        {
            return await _dbContext.ScreenerCategoryM.ToListAsync();
        }

        public async Task<ScreenerCategoryM> GetByIdAsync(int id)
        {
            return await _dbContext.ScreenerCategoryM.FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<ScreenerCategoryM> AddAsync(ScreenerCategoryM category)
        {
            _dbContext.ScreenerCategoryM.Add(category);
            await _dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<ScreenerCategoryM> UpdateAsync(ScreenerCategoryM category)
        {
            _dbContext.ScreenerCategoryM.Update(category);
            await _dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _dbContext.ScreenerCategoryM.FirstOrDefaultAsync(item => item.Id == id);
            result.IsActive = false;
            _ = _dbContext.ScreenerCategoryM.Update(result);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public List<ScreenerCategoryModel> GetScreenerDataForMobileApplication()
        {
            // Example data fetched from stored procedure
            #region Fetch Data From SP 
            List<ScreenerModel.GetScreenerDetailssP> procedureResponse = new()
            {
            new ScreenerModel.GetScreenerDetailssP
            {
                ScreenerId = 1,
                ScreenerName = "52-Week High",
                Code = "52W_HIGH",
                ScreenerDescription = "Stocks at their 52-week high levels",
                ScreenerIsActive = true,
                CategoryId = 1,
                CategoryName = "Breakout",
                CategoryDescription = "Screeners for stocks breaking out of key levels",
                CategoryIsActive = true
            },
            new ScreenerModel.GetScreenerDetailssP
            {
                ScreenerId = 2,
                ScreenerName = "Volume Buzzer",
                Code = "VOL_BUZZ",
                ScreenerDescription = "Highlights unusual volume activity",
                ScreenerIsActive = true,
                CategoryId = 2,
                CategoryName = "Volume Buzzer",
                CategoryDescription = "Screeners for unusual volume activity",
                CategoryIsActive = true
            }
        };
            #endregion

            // Simplify the data into categories and screeners
            var simplifiedData = SimplifyData(procedureResponse);

            // Display the results
            foreach (var category in simplifiedData)
            {
                Console.WriteLine($"Category: {category.CategoryName} (ID: {category.CategoryId})");
                Console.WriteLine($"Description: {category.CategoryDescription}");
                Console.WriteLine("Screeners:");
                foreach (var screener in category.Screeners)
                {
                    Console.WriteLine($"  - {screener.Name} (Code: {screener.Code})");
                }
                Console.WriteLine();
            }

            return simplifiedData;
        }

        private static List<ScreenerCategoryModel> SimplifyData(List<GetScreenerDetailssP> procedureResponse)
        {
            // Group data by CategoryId and map to SimplifiedCategoryModel
            return procedureResponse
                .GroupBy(r => new { r.CategoryId, r.CategoryName, r.CategoryDescription, r.SubscriptionStatus })
                .Select(g => new ScreenerCategoryModel
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    CategoryDescription = g.Key.CategoryDescription,
                    SubscriptionStatus = g.Key.SubscriptionStatus,
                    Screeners = g.Select(s => new ScreenerModel.Screener
                    {
                        Id = s.ScreenerId,
                        Name = s.ScreenerName,
                        Code = s.Code,
                        ScreenerDescription = s.ScreenerDescription
                    }).ToList()
                })
                .ToList();
        }

    }
}
