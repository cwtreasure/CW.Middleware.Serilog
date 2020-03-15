namespace CW.Middleware.Serilog.WebApp
{
    using global::Serilog.Core;
    using global::Serilog.Core.Enrichers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSerilogRequest(x =>
            {
                x.BuildEventEnricher = (context) =>
                {
                    var traceId = Guid.NewGuid().ToString("N");

                    if (context.Request.Headers.TryGetValue("trace-id", out var tId))
                    {
                        var rid = tId.FirstOrDefault().ToString();
                        traceId = rid.Length > 32 ? rid.Substring(0, 32) : rid;
                    }

                    context.Items.Add("trace-id", traceId);

                    return new List<ILogEventEnricher>
                    {
                        new PropertyEnricher("cTraceId", traceId)
                    };
                };
                x.ShouldLogRequestAndResponse = true;
                x.TraceIdEnricherName = "cTraceId";
                x.TraceIdName = "trace-id";
                x.UserIpEnricherName = "UserIp";
                x.UserIpName = "userIp";
            });
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequest();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
