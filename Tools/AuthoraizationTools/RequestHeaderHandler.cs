using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Tools.AuthoraizationTools
{
    public static class RequestHeaderHandler
    {
        public static string GetUserAgent(this HttpContext context)
        {
            var currentUserAgent = context.Request.Headers["User-Agent"].ToString();
            return currentUserAgent;
        }
        public static string[] ipHeaders = [
            "X-Forwarded-For",
            "X-Real-IP",
            "Forwarded",
            "CF-Connecting-IP",
            "True-Client-IP",
            "X-Cluster-Client-IP",
            "Fastly-Client-IP",
            "X-Client-IP"
        ];
        public static string GetIP(this HttpContext context)
        {
            string clientIp = "";

            foreach (var header in ipHeaders)
            {
                if (context.Request.Headers[header].Count > 0)
                {
                    clientIp = context.Request.Headers[header].ToString();
                    break;
                }
            }
            if (string.IsNullOrEmpty(clientIp))
            {
                clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "";
            }

            return clientIp;
        }
    }
}
