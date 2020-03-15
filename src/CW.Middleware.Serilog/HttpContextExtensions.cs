namespace CW.Middleware.Serilog
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Linq;
    using System.Net;

    internal static class HttpContextExtensions
    {
        public static string GetUserOrCurrentIp(this HttpContext context)
        {
            try
            {
                var ip = context.GetUserIp();

                if (string.IsNullOrWhiteSpace(ip))
                {
                    foreach (var iPA in Dns.GetHostAddresses(Dns.GetHostName()))
                    {
                        if (iPA.AddressFamily.ToString() == "InterNetwork")
                        {
                            ip = iPA.ToString();
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(ip))
                    {
                        ip = "127.0.0.1";
                    }
                }

                return ip;
            }
            catch (Exception)
            {
                return "127.0.0.1";
            }
        }

        public static string GetUserIp(this HttpContext context)
        {
            try
            {
                var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                if (string.IsNullOrEmpty(ip))
                {
                    ip = context.Connection.RemoteIpAddress?.ToString();
                }

                if (string.IsNullOrWhiteSpace(ip))
                {
                    ip = context.Request.Headers["REMOTE_ADDR"].FirstOrDefault();
                }

                return ip;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
