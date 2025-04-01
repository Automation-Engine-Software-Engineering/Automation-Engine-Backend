using DataLayer.DbContext;
using FrameWork.ExeptionHandler.CustomMiddleware;
using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services;
using System.Text;
using Tools.AuthoraizationTools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var audience = builder.Configuration["JWTSettings:Audience"] ?? throw new CustomException("Audience در appsettings یافت نشد");
var accessTokenSecret = builder.Configuration["JWTSettings:AccessTokenSecret"] ?? throw new CustomException("Audience در appsettings یافت نشد");
var issuer = builder.Configuration["JWTSettings:Issuer"] ?? throw new CustomException("Issuer در appsettings یافت نشد");



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

builder.Services.AddScoped<DataLayer.DbContext.Context>();
builder.Services.AddScoped<DynamicDbContext>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IEntityService, EntityService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IWorkFlowService, WorkFlowService>();
builder.Services.AddScoped<IWorkFlowUserService, WorkFlowUserService>();
builder.Services.AddScoped<IWorkFlowRoleService, WorkFlowRoleService>();
builder.Services.AddScoped<IRoleUserService, RoleUserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IHtmlService, HtmlService>();
builder.Services.AddSingleton<TokenGenerator>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var headers = RequestHeaderHandler.ipHeaders.ToList();
headers.AddRange(["Content-Type", "Authorization", "User-Agent"]);
builder.Services.AddCors(options => options.AddPolicy("PublishPolicy",
builder =>
{
    builder.WithHeaders(headers.ToArray())
           .WithMethods("GET", "POST")
           .WithOrigins(audience)
           .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
}));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseCors("PublishPolicy");
app.UseMiddleware<CustomMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();

app.Run();
