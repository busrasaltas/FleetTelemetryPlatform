using FleetTelemetryPlatform.Data;
using FleetTelemetryPlatform.Messaging;
using FleetTelemetryPlatform.Middleware;
using FleetTelemetryPlatform.Repositories;
using FleetTelemetryPlatform.Repositoriess;
using FleetTelemetryPlatform.Services;
using FleetTelemetryPlatform.Workers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Service Registrations (Dependency Injection) ---

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Swagger UI - visual API testing interface
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext - connection string is read from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registered as Scoped to provide a dedicated DbContext per HTTP request.
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceService, DeviceService>();

builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

builder.Services.AddHostedService<TelemetryConsumerService>();

builder.Services.AddScoped<ICommandService, CommandService>();

var app = builder.Build();

// Register first to catch exceptions from all subsequent middleware and endpoints.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// --- Automatic Migrations ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    const int maxAttempts = 10;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            Console.WriteLine($"Database not ready (attempt {attempt}/{maxAttempts}): {ex.Message}. Retrying in 5s...");
            Thread.Sleep(5000);
        }
    }
} 

// --- HTTP Request Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();