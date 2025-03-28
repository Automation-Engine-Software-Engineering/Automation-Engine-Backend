using DataLayer.Context;
using FrameWork.ExeptionHandler.CustomMiddleware;
using Microsoft.EntityFrameworkCore;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options => options.AddPolicy("MyPolicy",
builder =>
{
    builder.AllowAnyHeader()
           .AllowAnyMethod()
           .SetIsOriginAllowed((host) => true)
           .AllowCredentials();
}));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add-Migration InitialCreate -DbContext DbContext
//Update-Database InitialCreate -DbContext DbContext
builder.Services.AddDbContext<DataLayer.Context.DbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("Basic")));

//Add-Migration InitialCreate -DbContext DynamicDbContext
//Update-Database InitialCreate -DbContext DynamicDbContext
builder.Services.AddDbContext<DynamicDbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("Dynamic")));
           
builder.Services.AddScoped<DataLayer.Context.DbContext>();
builder.Services.AddScoped<DynamicDbContext>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IEntityService, EntityService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IWorkFlowService, WorkFlowService>();
builder.Services.AddScoped<IWorkFlowUserService, WorkFlowUserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IHtmlService, HtmlService>();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(x => true));
app.UseCors("MyPolicy");
app.UseMiddleware<CustomMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();

app.Run();
