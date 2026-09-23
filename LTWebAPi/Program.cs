var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Web API v1");
    c.RoutePrefix = "swagger"; // Đường dẫn truy cập sẽ là /swagger
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();