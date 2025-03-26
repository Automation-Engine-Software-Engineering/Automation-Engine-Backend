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
        private string GenerateToken(string secretConfigPath, string frontDomainConfigPath, string backendDomainConfigPath, DateTime expires, params Claim[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var backendDomain = _configuration[backendDomainConfigPath];
            if (backendDomain.IsNullOrEmpty())
                throw new CustomException("دامنه یافت نشد");

            var frontDomain = _configuration[frontDomainConfigPath];
            if (frontDomain.IsNullOrEmpty())
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
                Audience = frontDomain,
                Issuer = backendDomain,
                
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
