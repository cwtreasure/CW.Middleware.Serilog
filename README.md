# CW.Middleware.Serilog

 Serilog middleware

# How to use

## instaill nuget package

```
dotnet add package CW.Middleware.Serilog
```

## configure

```cs
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
    
    // ...
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseSerilogRequest();

    // ...
}
```
