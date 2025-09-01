using RM.Database.KingResearchContext;
using RM.Model;
using Microsoft.EntityFrameworkCore;
using System.Net;

public interface ICategoryService
{
    Task<ApiCommonResponseModel> GetCategories();
    Task<ApiCommonResponseModel> GetStrategies(string strategyName);

}
public class CategoryService : ICategoryService
{
    private readonly KingResearchContext _dbContext;
    ApiCommonResponseModel responseModel = new();

    public CategoryService(KingResearchContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiCommonResponseModel> GetCategories()
    {
        var categories = await _dbContext.ProductCategoriesM.Where(item => item.IsActive == true && (item.IsDelete == false || item.IsDelete == null)).Select(item => new
        {
            item.Id,
            item.Name,
            item.GroupName,
            item.Description,
        }).ToListAsync();

        responseModel.Message = "Data Fetch Successfull.";
        responseModel.Data = categories;
        responseModel.StatusCode = HttpStatusCode.OK;
        return responseModel;
    }

    public async Task<ApiCommonResponseModel> GetStrategies(string strategyName)
    {
        //        select p.Name, P.code, p.Price , pm.Name as PCName from ProductsM  as p
        //INNER JOIN ProductCategoriesM as pm on p.CategoryId = pm.Id where pm.GroupName = 'strategy'

        var result = await _dbContext.ProductsM.Join(_dbContext.ProductCategoriesM, product => product.CategoryID, productsCategM => productsCategM.Id, (product, productsCategM) =>
        new
        {
            product.Id,
            product.Name,
            product.Code,
            productsCategM.GroupName,
            product.IsActive,
            product.IsDeleted,
        }).Where(filterItem => filterItem.GroupName == strategyName && !filterItem.IsDeleted && filterItem.IsActive).OrderBy(filterItem => filterItem.Name).ToListAsync();

        var allEntry = new
        {
            Id = 0,
            Name = "All",
            Code = "",
            GroupName = strategyName,
            IsActive = true,
            IsDeleted = false
        };
        result.Insert(0, allEntry!);



        //var categories = await _dbContext.ProductCategoriesM.Where(item => item.IsActive == true && (item.IsDelete == false || item.IsDelete == null)).Select(item => new
        //{
        //    item.Id,
        //    item.Name,
        //    item.GroupName,
        //    item.Description,
        //}).ToListAsync();

        responseModel.Message = "Data Fetch Successfull.";
        responseModel.Data = result;
        responseModel.StatusCode = HttpStatusCode.OK;
        return responseModel;
    }
}
