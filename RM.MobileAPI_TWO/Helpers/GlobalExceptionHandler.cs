using RM.CommonServices.Services;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.MongoDbCollection;
using System.Net;
using System.Text;

namespace RM.MobileAPI.Helpers
{
    public class GlobalExceptionMiddlewareNew(RequestDelegate next, IServiceProvider serviceProvider)
    {
        private readonly RequestDelegate _next = next;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Enable buffering if not already enabled
                if (!context.Request.Body.CanSeek)
                {
                    context.Request.EnableBuffering();
                }
                await _next(context);
            }
            catch (Exception ex)
            {
                using var scope = _serviceProvider.CreateScope();
                
                var _mongoRepository = scope.ServiceProvider.GetRequiredService<IMongoRepository<ExceptionLog>>();
                var requestBody = await GetRequestBodyAsync(context);
                await _mongoRepository.AddAsync(new ExceptionLog
                {
                    CreatedOn = DateTime.Now,
                    InnerException = ex.InnerException?.Message,
                    Message = ex.Message,
                    RequestBody = requestBody,
                    Source = "global",
                    StackTrace = ex.StackTrace
                });

                HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

                if (ex.Message.Contains("Unauthorized"))
                {
                    statusCode = HttpStatusCode.Unauthorized;
                }
                // Respond with a consistent format
                var response = new ApiCommonResponseModel
                {
                    StatusCode = statusCode,
                    Message = ex.Message,
                    Data = null,
                    Exceptions = new { ex.Message, ex.StackTrace }
                };

                context.Response.StatusCode = (int)statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);

            }
        }

        private static async Task<string> GetRequestBodyAsync(HttpContext context)
        {
            string body = "";
            try
            {
                // Enable buffering if not already enabled
                if (!context.Request.Body.CanSeek)
                {
                    context.Request.EnableBuffering();
                }

                if (context.Request.Method.ToUpper() == "GET")
                {
                    body = context.Request.QueryString.ToString();
                }
                else
                {
                    context.Request.Body.Position = 0; // Reset the stream position to the beginning
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0; // Reset again for potential further use
                }
                return body;
            }
            catch (Exception e)
            {
                // Handle any exceptions that occur while reading the body
                Console.WriteLine($"Failed to read request body: {e.Message}");
                return "Could not read request body.";
            }
        }
    }

}


