namespace Microsoft.AspNetCore.Builder
{
    using CW.Middleware.Serilog;

    public static class SerilogRequestMiddlewareExtensions
    {
        /// <summary>
        /// Use Serilog Request Middleware
        /// </summary>
        /// <param name="builder">IApplicationBuilder</param>
        /// <returns>IApplicationBuilder</returns>
        public static IApplicationBuilder UseSerilogRequest(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SerilogRequestMiddleware>();
        }
    }
}
