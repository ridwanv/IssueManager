using CleanArchitecture.Blazor.Application;
using CleanArchitecture.Blazor.Application.Common.Constants.Localization;
using CleanArchitecture.Blazor.Infrastructure;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);



// Add localization services
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.AddSupportedUICultures(LocalizationConstants.SupportedLanguages.Select(x => x.Code).ToArray());
    options.AddSupportedCultures(LocalizationConstants.SupportedLanguages.Select(x => x.Code).ToArray());
    options.DefaultRequestCulture = new RequestCulture(LocalizationConstants.DefaultLanguageCode);
    options.FallBackToParentUICultures = true;
})
.AddLocalization(options => options.ResourcesPath = LocalizationConstants.ResourcesPath);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var apiSettings = builder.Configuration.GetSection("ApiSettings");
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = apiSettings["Title"] ?? "IssueManager API",
        Version = apiSettings["Version"] ?? "v1",
        Description = apiSettings["Description"] ?? "REST API for Issue Management System",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = apiSettings["ContactName"] ?? "IssueManager Team",
            Email = apiSettings["ContactEmail"] ?? "support@issuemanager.com"
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS policy for API access
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() 
                        ?? new[] { "https://localhost:7000", "https://localhost:7002" };
    
    options.AddPolicy("ApiCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IssueManager API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the root
        c.DocumentTitle = "IssueManager API Documentation";
    });
}

// Configure localization
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(LocalizationConstants.DefaultLanguageCode)
    .AddSupportedCultures(LocalizationConstants.SupportedLanguages.Select(x => x.Code).ToArray())
    .AddSupportedUICultures(LocalizationConstants.SupportedLanguages.Select(x => x.Code).ToArray());

app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();
app.UseCors("ApiCorsPolicy");

// Add authentication if required
var authSettings = builder.Configuration.GetSection("Authentication");
if (authSettings.GetValue<bool>("RequireAuthentication"))
{
    app.UseAuthentication();
}

app.UseAuthorization();

// Map health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
