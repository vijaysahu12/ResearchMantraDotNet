using RM.CommonServices.Services;
using RM.Database.MongoDbContext;
using RM.Model;
using RM.Model.MongoDbCollection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RM.API.Helpers
{
    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }



    public class GlobalExceptionMiddlewareNew
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider; 


        public GlobalExceptionMiddlewareNew(RequestDelegate next, IServiceProvider serviceProvider )
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

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
                var requestBody = await GetRequestBodyAsync(context);
                using var scope = _serviceProvider.CreateScope();

                var _exception = scope.ServiceProvider.GetRequiredService<IMongoRepository<ExceptionLog>>();

                await _exception.AddAsync(new ExceptionLog
                {
                    StackTrace = ex.StackTrace,
                    CreatedOn = DateTime.Now,
                    InnerException = ex.InnerException?.Message,
                    Message = ex.InnerException?.Message,
                    RequestBody = requestBody,
                    Source = "GlobalExceptionMiddlewareNew"
                });

                HttpStatusCode statusCode = System.Net.HttpStatusCode.InternalServerError;

                if (ex.Message.Contains("Unauthorized"))
                {
                    statusCode = System.Net.HttpStatusCode.Unauthorized;
                }
                // Respond with a consistent format
                var response = new ApiCommonResponseModel
                {
                    StatusCode = statusCode,
                    Message = ex.InnerException == null ? ex.Message : ex.InnerException?.Message,
                    Data = null,
                    Exceptions = new { ex.Message, ex.InnerException, ex.StackTrace }
                };

                context.Response.StatusCode = (int)statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(response);

            }
        }

        private static async Task<string> GetRequestBodyAsync(HttpContext context)
        {
            try
            {
                // Enable buffering if not already enabled
                if (!context.Request.Body.CanSeek)
                {
                    context.Request.EnableBuffering();
                }

                context.Request.Body.Position = 0; // Reset the stream position to the beginning

                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                string body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset again for potential further use
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


