using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RM.API.Helpers
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            logger = loggerFactory.CreateLogger<RequestResponseLoggingMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();

            byte[] buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            _ = await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            string requestBody = Encoding.UTF8.GetString(buffer);
            _ = context.Request.Body.Seek(0, SeekOrigin.Begin);

            logger.LogInformation(requestBody);

            Stream originalBodyStream = context.Response.Body;

            using MemoryStream responseBody = new();
            context.Response.Body = responseBody;

            await next(context);

            _ = context.Response.Body.Seek(0, SeekOrigin.Begin);
            string response = await new StreamReader(context.Response.Body).ReadToEndAsync();
            _ = context.Response.Body.Seek(0, SeekOrigin.Begin);

            logger.LogInformation(response);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}
