

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Jaxx.WebApi.Shared.Infrastructure
{
    // https://weblog.west-wind.com/posts/2020/Feb/26/Working-with-IWebHostEnvironment-and-IHostingEnvironment-in-dual-targeted-NET-Core-Projects
    public sealed class JsonExceptionMiddleware
    {
        public const string DefaultErrorMessage = "A server error occurred.";

        private readonly IHostEnvironment _env;
        private readonly JsonSerializer _serializer;
        private readonly ILogger _logger;

        public JsonExceptionMiddleware(IHostEnvironment env, ILogger<JsonExceptionMiddleware> logger)
        {
            _env = env;
            _logger = logger;

            _serializer = new JsonSerializer();
            _serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (ex == null) return;

            var error = BuildError(ex, _env);
            _logger.LogError(ex.Message);
            _logger.LogError(ex.StackTrace);

            using (var writer = new StreamWriter(context.Response.Body))
            {
                _serializer.Serialize(writer, error);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }

        private static ApiError BuildError(Exception ex, IHostEnvironment env)
        {
            var error = new ApiError();

            if (env.IsDevelopment())
            {
                error.Message = ex.Message;
                error.Detail = ex.StackTrace;
            }
            else
            {
                error.Message = DefaultErrorMessage;
                error.Detail = ex.Message;
            }

            return error;
        }
    }
}
