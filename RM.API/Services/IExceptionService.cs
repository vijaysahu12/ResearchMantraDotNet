using RM.Database.ResearchMantraContext;
using RM.Database.ResearchMantraContext.Tables;
using RM.Model;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RM.API.Services
{
    public interface IExceptionService
    {
        public Task<ApiCommonResponseModel> ExceptionLog(Exception ex);
        public Task<ApiCommonResponseModel> ExceptionLog();
        public Task<ApiCommonResponseModel> ExceptionLog(ExceptionLog logParam);
    }

    public class ExceptionService : IExceptionService
    {
        public ResearchMantraContext _context;
        public ExceptionService(ResearchMantraContext context)
        {
            _context = context;

        }
        public async Task<ApiCommonResponseModel> ExceptionLog(Exception ex)
        {
            ExceptionLog log = new();
            _ = await _context.ExceptionLogs.AddAsync(log);
            _ = await _context.SaveChangesAsync();
            ApiCommonResponseModel result = new()
            {
                StatusCode = HttpStatusCode.OK
            };
            return result;
        }

        public Task<ApiCommonResponseModel> ExceptionLog()
        {
            throw new NotImplementedException();
        }

        public async Task<ApiCommonResponseModel> ExceptionLog(ExceptionLog logParam)
        {
            _ = await _context.ExceptionLogs.AddAsync(logParam);
            _ = await _context.SaveChangesAsync();
            ApiCommonResponseModel result = new()
            {
                StatusCode = HttpStatusCode.OK
            };
            return result;
        }
    }
}
