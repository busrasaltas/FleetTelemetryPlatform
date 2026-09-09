using Microsoft.EntityFrameworkCore;
using FleetTelemetryPlatform.Data;
using FleetTelemetryPlatform.Repositories;
using FleetTelemetryPlatform.Services;
using FleetTelemetryPlatform.Repositoriess;

var builder = WebApplication.CreateBuilder(args);

// --- Service Registrations (Dependency Injection) ---

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// DbContext - connection string is read from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registered as Scoped to provide a dedicated DbContext per HTTP request.
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceService, DeviceService>();

var app = builder.Build();

// --- Automatic Migrations ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// --- HTTP Request Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();