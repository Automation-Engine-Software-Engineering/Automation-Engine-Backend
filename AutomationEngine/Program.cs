using AutomationEngine.CustomMiddlewares;
using DataLayer.DbContext;
using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services;
using System.Text;
using System.Threading.RateLimiting;
using Tools.AuthoraizationTools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var audience = builder.Configuration["JWTSettings:Audience"] ?? throw new CustomException("Audience در appsettings یافت نشد");
var accessTokenSecret = builder.Configuration["JWTSettings:AccessTokenSecret"] ?? throw new CustomException("Audience در appsettings یافت نشد");
var issuer = builder.Configuration["JWTSettings:Issuer"] ?? throw new CustomException("Issuer در appsettings یافت نشد");
var secure = bool.Parse(builder.Configuration["JWTSettings:Secure"] ?? throw new CustomException("Secure در appsettings یافت نشد"));

var queueLimit = int.Parse(builder.Configuration["RateLimiter:QueueLimit"] ?? throw new CustomException("Issuer در appsettings یافت نشد"));
var permitLimit = int.Parse(builder.Configuration["RateLimiter:PermitLimit"] ?? throw new CustomException("Issuer در appsettings یافت نشد"));
var window = TimeSpan.Parse(builder.Configuration["RateLimiter:Window"] ?? throw new CustomException("Issuer در appsettings یافت نشد"));

var queueLimitLogin = int.Parse(builder.Configuration["RateLimiter:QueueLimitLogin"] ?? throw new CustomException("Issuer در appsettings یافت نشد"));
var permitLimitLogin = int.Parse(builder.Configuration["RateLimiter:PermitLimitLogin"] ?? throw new CustomException("Issuer در appsettings یافت نشد"));
var windowLogin = TimeSpan.Parse(builder.Configuration["RateLimiter:WindowLogin"] ?? throw new CustomException("Issuer در appsettings یافت نشد"));


var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Console.WriteLine($"Current Environment: {environment}");

builder.Services.AddRateLimiter(options =>
{
    // محدودیت سراسری: حداکثر 5 درخواست در هر 10 ثانیه
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.GetIP(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit, // تعداد درخواست مجاز
                Window = window, // بازه زمانی
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = queueLimit,// حداکثر تعداد درخواست در صف
                AutoReplenishment = true
            });
    });
    options.AddPolicy("LoginRateLimit", context =>
       RateLimitPartition.GetFixedWindowLimiter(
           context.GetIP(),
           partition => new FixedWindowRateLimiterOptions
           {
               PermitLimit = permitLimitLogin,
               Window = windowLogin, // بازه زمانی
               QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
               QueueLimit = queueLimitLogin,
               AutoReplenishment = true
           }
       )
   );
    // response of error
    options.OnRejected = (context, token) =>
    {
        var ex = new CustomException<object>(new FrameWork.Model.DTO.ValidationDto<object>(false, "Authentication", "TooManyRequests", null), 429);
        throw ex;
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Automaition Engine", Version = "v1" });

    // تعریف Authorization
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your token in the text input below.\n\nExample: 'Bearer 12345abcdef'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
});
//Add-Migration InitialCreate -Context Context
//Update-Database InitialCreate -Context Context
builder.Services.AddDbContext<DataLayer.DbContext.Context>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("Basic")));

//Add-Migration InitialCreate -DbContext DynamicDbContext
//Update-Database InitialCreate -DbContext DynamicDbContext
builder.Services.AddDbContext<DynamicDbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("Dynamic")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddScoped<Context>();
builder.Services.AddScoped<DynamicDbContext>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IEntityService, EntityService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IWorkflowUserService, WorkflowUserService>();
builder.Services.AddScoped<IWorkflowRoleService, WorkflowRoleService>();
builder.Services.AddScoped<IRoleUserService, RoleUserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IHtmlService, HtmlService>();
builder.Services.AddSingleton<TokenGenerator>();
builder.Services.AddSingleton<EncryptionTool>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var headers = RequestHeaderHandler.ipHeaders.ToList();
headers.AddRange(["Content-Type", "Authorization", "User-Agent"]);

//builder.Services.AddAntiforgery(options =>
//{
//    options.Cookie.Name = "X-CSRF-TOKEN";
//    options.Cookie.HttpOnly = true;
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    //app.UseHsts();
    //app.UseMiddleware<CspMiddleware>();
    if (secure)
        app.UseHttpsRedirection();
    app.UseRateLimiter();
}

app.UseCors(builder =>
{
    if (app.Environment.IsDevelopment())
    {
        // تنظیمات CORS در محیط Development
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin()
            .SetPreflightMaxAge(TimeSpan.FromDays(15));
    }
    else
    {
        builder
                    .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin()
            .SetPreflightMaxAge(TimeSpan.FromDays(15));


        //builder
        //  //.WithHeaders(headers.ToArray())
        //  .WithOrigins(audience)
        //  .WithMethods("GET", "POST")
        //  .AllowAnyOrigin()
        //  .SetPreflightMaxAge(TimeSpan.FromMinutes(15));
    }
});

app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();