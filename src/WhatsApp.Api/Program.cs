using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WhatsApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File("logs/whatsapp-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddHttpClient();

// Add WhatsApp services
builder.Services.AddScoped<WhatsAppApiService>();
builder.Services.AddScoped<WhatsAppMessageParser>();
builder.Services.AddScoped<DirectLineService>();

// Add DirectLine background service
builder.Services.AddHostedService<WhatsAppDirectLineService>();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add CORS for webhook
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebhookPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? new[] { "*" })
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("WebhookPolicy");
app.UseRouting();
app.MapControllers();

try
{
    Log.Information("Starting WhatsApp.Api");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "WhatsApp.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}