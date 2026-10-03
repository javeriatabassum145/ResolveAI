var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Allow React frontend to call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("ReactApp");

app.MapControllers();

app.Run();