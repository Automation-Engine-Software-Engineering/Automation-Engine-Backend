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
using FrameWork.Model.DTO;

namespace Tools.AuthoraizationTools
{
    public class TokenGenerator
    {
        private readonly IConfiguration _configuration;

        public TokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(string userId, string? role)
        {
            var claims = new List<Claim> { new Claim("UserId", userId) };//TODO : ClaimsEnum
            if (role != null)
                claims.Add(new Claim("RoleId", role));//TODO : ClaimsEnum

            var token = GenerateToken("JWTSettings:AccessTokenSecret", DateTime.UtcNow.AddMinutes(15), claims);
            return token;
        }
        public string GenerateRefreshToken(string userId, string? role)
        {
            var claims = new List<Claim> { new Claim("UserId", userId) };//TODO : ClaimsEnum
            if (role != null)
                claims.Add(new Claim("RoleId", role));//TODO : ClaimsEnum

            var token = GenerateToken("JWTSettings:RefreshTokenSecret", DateTime.UtcNow.AddMonths(1), claims);
            return token;
        }
        private string GenerateToken(string secretConfigPath, DateTime expires, List<Claim>? claims = null)
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
        public string? GetClaimFromToken(string inputToken, string claimType) // TODO : ClaimsEnum.UserId.ToString() change to ClaimsEnum
        {
            var token = inputToken.Trim();
            if (token.Contains("Bearer "))
                token = inputToken.Substring("Bearer ".Length);

            var jwtHandler = new JwtSecurityTokenHandler();
            if (!jwtHandler.CanReadToken(token))
                return null;

            var jwtToken = jwtHandler.ReadJwtToken(token);
            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType);
            return claim?.Value;
        }
        public ClaimsPrincipal ValidateToken(string token, bool isRefreshToken = false)
        {
            var refreshToken = token.Trim();
            if (refreshToken.Contains("Bearer "))
                refreshToken = token.Substring("Bearer ".Length);

            var tokenHandler = new JwtSecurityTokenHandler();
            string tokenSecret = "";
            if (isRefreshToken)
                tokenSecret = "JWTSettings:RefreshTokenSecret";
            else
                tokenSecret = "JWTSettings:AccessTokenSecret";

            // خواندن مقدار JWTSecret از فایل appsettings.json
            var secretKey = _configuration[tokenSecret] ?? throw new CustomException("کلید خصوصی یافت نشد");
            var issuer = _configuration["JWTSettings:Issuer"];
            var audience = _configuration["JWTSettings:Audience"];

            var key = Encoding.UTF8.GetBytes(secretKey ?? "");

            try
            {
                var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey ?? "")),
                    
                    ClockSkew = TimeSpan.Zero // تأخیر زمانی مجاز
                },out _);
                //TokenClaims
                return principal;
            }
            catch(Exception e)
            {
                throw new CustomException(new ValidationDto<string>(false, "Authentication", "NotAuthorized", refreshToken).GetMessage(403));
            }
        }
    }
}
