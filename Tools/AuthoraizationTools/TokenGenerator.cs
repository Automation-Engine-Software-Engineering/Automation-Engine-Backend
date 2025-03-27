using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Tools.TextTools;
using FrameWork.ExeptionHandler.ExeptionModel;
using System.Data;

namespace Tools.AuthoraizationTools
{
    public class TokenGenerator
    {
        private readonly IConfiguration _configuration;

        public TokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(string username,string role)
        {
            var token = GenerateToken("JWTSettings:AccessTokenSecret",DateTime.UtcNow.AddMinutes(15),new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Role, role));
            return token;
        }
        public string GenerateRefreshToken()
        {
            var token = GenerateToken("JWTSettings:RefreshTokenSecret", DateTime.UtcNow.AddMonths(1));
            return token;
        }
        private string GenerateToken(string secretConfigPath DateTime expires, params Claim[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var issuer = _configuration["JWTSettings:Issuer"];
            if (issuer.IsNullOrEmpty())
                throw new CustomException("دامنه یافت نشد");

            var audience = _configuration["JWTSettings:Audience"];
            if (audience.IsNullOrEmpty())
                throw new CustomException("دامنه یافت نشد");

            // خواندن مقدار JWTSecret از فایل appsettings.json
            var secretKey = _configuration[secretConfigPath];
            if (secretKey.IsNullOrEmpty())
                throw new CustomException("کلید خصوصی یافت نشد");
            var key = Encoding.UTF8.GetBytes(secretKey ?? "");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires, // مدت زمان Access Token
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                NotBefore = DateTime.UtcNow,
                Audience = audience,//دریافت کننده
                Issuer = issuer,// صادرکننده

            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        private ClaimsPrincipal ValidateRefreshToken(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("your_refresh_secret_key");

            try
            {
                var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.Zero // تأخیر زمانی مجاز
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
