using DataLayer.Context;
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
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add-Migration InitialCreate -Context Context
//Update-Database InitialCreate -Context Context
builder.Services.AddDbContext<Context>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Add-Migration InitialCreate -Context DynamicDbContext
//Update-Database InitialCreate -Context DynamicDbContext
builder.Services.AddDbContext<DynamicDbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<Context>();
builder.Services.AddScoped<DynamicDbContext>();
builder.Services.AddScoped<IFormService, FormService>();
builder.Services.AddScoped<IEntityService, EntityService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IWorkFlowService, WorkFlowService>();
builder.Services.AddScoped<IWorkFlowUserService, WorkFlowUserService>();
builder.Services.AddScoped<IRoleService, RoleService>();

//builder.Services.AddControllers().AddJsonOptions(options =>
//{
//    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
//});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(x=>true));
app.UseCors("MyPolicy");
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
