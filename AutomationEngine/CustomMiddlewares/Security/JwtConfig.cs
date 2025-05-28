using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationEngine.CustomMiddlewares.Security
{
    public static class JwtConfig
    {
        public static void ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var exceptionJWTSettings = new CustomException("AppSettings", "JWTSettings");
            var audience = configuration["JWTSettings:Audience"];
            var accessTokenSecret = configuration["JWTSettings:AccessTokenSecret"] ?? throw exceptionJWTSettings;
            var issuer = configuration["JWTSettings:Issuer"];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessTokenSecret))
                    };
                    options.RequireHttpsMetadata = true;
                });
        }
    }
}
