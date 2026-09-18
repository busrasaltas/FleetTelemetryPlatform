using FleetTelemetryPlatform.Data;
using FleetTelemetryPlatform.Jobs;
using FleetTelemetryPlatform.Messaging;
using FleetTelemetryPlatform.Middleware;
using FleetTelemetryPlatform.Repositories;
using FleetTelemetryPlatform.Repositoriess;
using FleetTelemetryPlatform.Services;
using FleetTelemetryPlatform.Workers;
using Hangfire;
using Hangfire.Dashboard;
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

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();
builder.Services.AddScoped<OfflineDeviceDetectionJob>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "sql-server")
    .AddRedis(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379", name: "redis")
    .AddRabbitMQ(
    factory: _ => new RabbitMQ.Client.ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMq:Host"] ?? "localhost"
    }.CreateConnectionAsync(),
    name: "rabbitmq");


var app = builder.Build();


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
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        // Docker port forwarding makes this request non-local to the container.
        // This bypass exists only in the Development environment.
        Authorization = new[] { new DevelopmentDashboardAuthorizationFilter() }
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

RecurringJob.AddOrUpdate<OfflineDeviceDetectionJob>(
    "offline-device-detection",
    job => job.ExecuteAsync(),
    "*/1 * * * *");

app.Run();

sealed class DevelopmentDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) =>
        context.GetHttpContext().RequestServices
            .GetRequiredService<IHostEnvironment>()
            .IsDevelopment();
}
