using LTWebAPi.Data;
using LTWebAPi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);
// ===== SERILOG =====
var logger = new LoggerConfiguration()
    .WriteTo.Console()                                          
    .WriteTo.File("Logs/Book_Log.txt", rollingInterval: RollingInterval.Minute)  
    .MinimumLevel.Information()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
// ===== END SERILOG =====

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký Repository
builder.Services.AddScoped<IPublisherRepository, SQLPublisherRepository>();
builder.Services.AddScoped<IAuthorRepository, SQLAuthorRepository>();
builder.Services.AddScoped<IBookRepository, SQLBookRepository>();

var app = builder.Build();

// Chỉ bật Swagger khi đang Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LT Web API v1");
        c.RoutePrefix = "swagger"; // truy cập: https://localhost:xxxx/swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();