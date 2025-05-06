using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Tools.TextTools;

namespace Tools.LoggingTools
{
    public class Logging
    {
        private readonly ILogger<Logging> _logger;

        public Logging(ILogger<Logging> logger)
        {
            _logger = logger;
        }
        private void WriteLog(LogLevel logLevel, Exception exception, string message)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    _logger.LogCritical(exception, message);
                    break;
                case LogLevel.Error:
                    _logger.LogError(exception, message);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(exception, message);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(exception, message);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(exception, message);
                    break;
                case LogLevel.Trace:
                    _logger.LogTrace(exception, message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }
        private string CreateFormattedMessage(string title, params (string Key, object Value)[] properties)
        {
            var formattedProperties = string.Join(",", properties.Select(p => @$"""{p.Key}"":""{p.Value}"""));
            return $"{title} {{{formattedProperties}}}";
        }
        public void LogErrorMiddleware(
    Exception exception,
    string message,
    object? logData,
    string requestBody,
    string requestFormData,
    string headers)
        {
            string formattedMessage = CreateFormattedMessage(
                message,
                ("LogData", ConvertToString.ConvertObjectToString(logData)),
                ("RequestBody", requestBody),
                ("RequestFormData", requestFormData),
                ("Headers", headers)
            );

            WriteLog(LogLevel.Critical, exception, formattedMessage);
        }

        public void LogUserLoginSuccess(
            int userId,
            string username,
            string ip,
            string userAgent)
        {
            string formattedMessage = CreateFormattedMessage(
                "User login success",
                ("userId", userId),
                ("username", username),
                ("ip", ip),
                ("userAgent", userAgent)
            );

            WriteLog(LogLevel.Warning, null, formattedMessage);
        }

        public void LogUserChangePassword(
            int userId,
            string username,
            string ip,
            string userAgent)
        {
            string formattedMessage = CreateFormattedMessage(
                "User Change Password",
                ("userId", userId),
                ("username", username),
                ("ip", ip),
                ("userAgent", userAgent)
            );

            WriteLog(LogLevel.Warning, null, formattedMessage);
        }

        public void LogUserLogout(
            int userId,
            string username,
            string ip,
            string userAgent)
        {
            string formattedMessage = CreateFormattedMessage("User Logout",
                ("userId", userId),
                ("username", username),
                ("ip", ip),
                ("userAgent", userAgent));

            WriteLog(LogLevel.Warning, null, formattedMessage);
        }

    }
}
