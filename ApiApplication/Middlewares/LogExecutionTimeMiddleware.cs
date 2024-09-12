using ApiApplication.Attributes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ApiApplication.Middlewares
{
    public class LogExecutionTimeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogExecutionTimeMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public LogExecutionTimeMiddleware(RequestDelegate next, ILogger<LogExecutionTimeMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_env.IsDevelopment())
            {
                var endpoint = context.GetEndpoint();

                if (endpoint != null)
                {
                    var methodInfo = endpoint.Metadata
                        .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
                        .FirstOrDefault()?
                        .MethodInfo;

                    if (methodInfo != null && methodInfo.GetCustomAttribute<LogExecutionTimeAttribute>() != null)
                    {
                        var className = methodInfo.DeclaringType?.Name ?? "Unknown";
                        var methodName = methodInfo.Name;

                        var stopwatch = Stopwatch.StartNew();
                        try
                        {
                            await _next(context); // Continue processing the request
                        }
                        finally
                        {
                            stopwatch.Stop();
                            _logger.LogInformation($"Method {methodName} of class {className} executed in {stopwatch.Elapsed.TotalMilliseconds} ms");
                        }
                    }
                    else
                    {
                        await _next(context); // Continue processing the request 
                    }
                }
            }
            else
            {
                await _next(context); // Continue processing the request 
            }
        }
    }
}
