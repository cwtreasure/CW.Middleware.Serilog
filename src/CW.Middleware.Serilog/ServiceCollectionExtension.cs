namespace Microsoft.Extensions.DependencyInjection
{
    using CW.Middleware.Serilog;
    using System;

    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Add Serilog Request
        /// </summary>
        /// <param name="services">services</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSerilogRequest(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSerilogRequest(x => new SerilogRequestOptions());

            return services;
        }

        /// <summary>
        /// Add Serilog Request
        /// </summary>
        /// <param name="services">services</param>
        /// <param name="configure">configure</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddSerilogRequest(this IServiceCollection services, Action<SerilogRequestOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            services.Configure(configure);

            return services;
        }
    }
}
