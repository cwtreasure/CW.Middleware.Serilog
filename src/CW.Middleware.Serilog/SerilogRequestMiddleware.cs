namespace CW.Middleware.Serilog
{
    using global::Serilog;
    using global::Serilog.Context;
    using global::Serilog.Core;
    using global::Serilog.Core.Enrichers;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    internal class SerilogRequestMiddleware
    {
        private static readonly ILogger Logger = Log.ForContext<SerilogRequestMiddleware>();
        private readonly RequestDelegate _next;
        private readonly SerilogRequestOptions _options;

        public SerilogRequestMiddleware(RequestDelegate next, IOptions<SerilogRequestOptions> optionsAccs)
        {
            _next = next;
            _options = optionsAccs.Value;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // handle head and options methods here.
            if (httpContext.Request.Method.Equals(HttpMethods.Head, StringComparison.OrdinalIgnoreCase)
                || httpContext.Request.Method.Equals(HttpMethods.Options, StringComparison.OrdinalIgnoreCase))
            {
                httpContext.Response.StatusCode = 200;
                return;
            }

            List<ILogEventEnricher> properties = new List<ILogEventEnricher>();

            if (_options.BuildEventEnricher == null)
            {
                var traceId = Guid.NewGuid().ToString("N");
                var userIp = httpContext.GetUserOrCurrentIp();

                if (httpContext.Request.Headers.TryGetValue(_options.TraceIdName, out var tId))
                {
                    var rid = tId.FirstOrDefault().ToString();
                    traceId = rid.Length > 32 ? rid.Substring(0, 32) : rid;
                }

                httpContext.Items.Add(_options.TraceIdName, traceId);
                httpContext.Items.Add(_options.UserIpName, userIp);

                properties.Add(new PropertyEnricher(_options.TraceIdEnricherName, traceId));
                properties.Add(new PropertyEnricher(_options.UserIpEnricherName, userIp));
            }
            else
            {
                var tmpProps = _options.BuildEventEnricher.Invoke(httpContext);
                properties.AddRange(tmpProps);
            }

            if (properties == null || !properties.Any())
            {
                throw new ArgumentNullException("Can not find ILogEventEnricher!");
            }

            using (LogContext.Push(properties.ToArray()))
            {
                if (_options.ShouldLogRequestAndResponse)
                {
                    var reqMsg = await FormatRequest(httpContext.Request);
                    Logger.Information(
                        "Method={0},Path={1},QueryString={2},Body={3}",
                        httpContext.Request.Method, httpContext.Request.Path, httpContext.Request.QueryString.ToString(), reqMsg);

                    var originalBodyStream = httpContext.Response.Body;
                    using var responseBody = new MemoryStream();
                    httpContext.Response.Body = responseBody;
                    var sp = new Stopwatch();
                    sp.Start();
                    await _next(httpContext);
                    sp.Stop();
                    var resMsg = await FormatResponse(httpContext.Response);
                    Logger.Information(
                       "Method={0},Path={1},Cost={2}ms,Response={3}",
                       httpContext.Request.Method, httpContext.Request.Path, sp.ElapsedMilliseconds, resMsg);

                    await responseBody.CopyToAsync(originalBodyStream);
                }
                else
                {
                    await _next(httpContext);
                }
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            request.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Seek(0, SeekOrigin.Begin);
            return text?.Trim();
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            if (response.HasStarted) return string.Empty;
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return text?.Trim();
        }
    }
}
