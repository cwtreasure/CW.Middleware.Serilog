namespace CW.Middleware.Serilog
{
    using System;
    using System.Collections.Generic;
    using global::Serilog.Core;
    using Microsoft.AspNetCore.Http;

    public class SerilogRequestOptions
    {
        /// <summary>
        /// Set a trace id to httpContext.Items
        /// </summary>
        public string TraceIdName { get; set; } = "trace-id";

        /// <summary>
        /// Set a user ip to httpContext.Items
        /// </summary>
        public string UserIpName { get; set; } = "userIp";

        /// <summary>
        /// Serilog custom trace-id Property Enricher name in template
        /// </summary>
        public string TraceIdEnricherName { get; set; } = "cTraceId";

        /// <summary>
        /// Serilog custom user-ip Property Enricher name in template
        /// </summary>
        public string UserIpEnricherName { get; set; } = "UserIp";

        /// <summary>
        /// Should log request and response message
        /// </summary>
        public bool ShouldLogRequestAndResponse { get; set; } = false;

        /// <summary>
        /// Build Serilog Event Enricher with httpcontext
        /// </summary>
        public Func<HttpContext, List<ILogEventEnricher>> BuildEventEnricher { get; set; }
    }
}
