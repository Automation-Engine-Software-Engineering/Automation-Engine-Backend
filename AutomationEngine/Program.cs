using DataLayer.DbContext;
using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Services;
using System.Text;
using System.Threading.RateLimiting;
using Tools.AuthoraizationTools;
using Serilog.Settings.Configuration;
using Tools.LoggingTools;
using AutomationEngine.CustomMiddlewares.Security;
using AutomationEngine.CustomMiddlewares.Configuration;
using AutomationEngine.CustomMiddlewares.Extensions;

var builder = WebApplication.CreateBuilder(args);

var audience = builder.Configuration["JWTSettings:Audience"] ?? throw new CustomException("AppSettings", "JWTSettings");
var headers = RequestHeaderHandler.ipHeaders.ToList();
headers.AddRange(["Content-Type", "Authorization", "User-Agent"]);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Console.WriteLine($"Current Environment: {environment}");

builder.Services.AddAntiforgery();
builder.Services.AddCustomAntiforgery();
builder.Services.ConfigureRateLimiter(builder.Configuration);
builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationLogging(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.SQLDatabaseConfig(builder.Configuration);
builder.Services.AddSwaggerConfig();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});


var app = builder.Build();

app.UseSerilogRequestLogging();

app.ConfigureMiddlewares(builder.Configuration);
app.ConfigureCors(headers.ToArray(), audience);

app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();