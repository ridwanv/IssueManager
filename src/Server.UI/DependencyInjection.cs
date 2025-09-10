using System.Net.Http.Headers;
using CleanArchitecture.Blazor.Application;
using CleanArchitecture.Blazor.Application.Common.Constants.Localization;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Infrastructure.Services.Identity;
using CleanArchitecture.Blazor.Infrastructure.BackgroundJobs;
using CleanArchitecture.Blazor.Server.UI.Hubs;
using CleanArchitecture.Blazor.Server.UI.Middlewares;
using CleanArchitecture.Blazor.Server.UI.Services;
using CleanArchitecture.Blazor.Server.UI.Services.Identity;
using CleanArchitecture.Blazor.Server.UI.Services.JsInterop;
using CleanArchitecture.Blazor.Server.UI.Services.Layout;
using CleanArchitecture.Blazor.Server.UI.Services.Navigation;
using CleanArchitecture.Blazor.Server.UI.Services.Notifications;
using CleanArchitecture.Blazor.Server.UI.Services.UserPreferences;
using IssueManager.Server.UI.Services;
using Hangfire;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using QuestPDF;
using QuestPDF.Infrastructure;
using CleanArchitecture.Blazor.Server.UI.Services.SignalR;



namespace CleanArchitecture.Blazor.Server.UI;

/// <summary>
/// Provides dependency injection configuration for the server UI.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds server UI services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddServerUI(this IServiceCollection services, IConfiguration config, IWebHostEnvironment? environment = null)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents(options =>
            {
                // Always show detailed errors in development, and optionally in other environments for debugging
                options.DetailedErrors = environment?.IsDevelopment() ?? true; // Changed to true for debugging
                options.DisconnectedCircuitMaxRetained = 100;
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
                options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
                options.MaxBufferedUnacknowledgedRenderBatches = 10;
            })
            .AddHubOptions(options => 
            {
                options.MaximumReceiveMessageSize = 64 * 1024;
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            });
        services.AddCascadingAuthenticationState();
  
        services.AddMudServices(config =>
        {
            MudGlobal.InputDefaults.ShrinkLabel = true;
            //MudGlobal.InputDefaults.Variant = Variant.Outlined;
            //MudGlobal.ButtonDefaults.Variant = Variant.Outlined;
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
            config.SnackbarConfiguration.NewestOnTop = false;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 3000;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
           
            // we're currently planning on deprecating `PreventDuplicates`, at least to the end dev. however,
            // we may end up wanting to instead set it as internal because the docs project relies on it
            // to ensure that the Snackbar always allows duplicates. disabling the warning for now because
            // the project is set to treat warnings as errors.
#pragma warning disable 0618
            config.SnackbarConfiguration.PreventDuplicates = false;
#pragma warning restore 0618
        });
        services.AddMudPopoverService();
        services.AddMudBlazorSnackbar();
        services.AddMudBlazorDialog();


        services.AddScoped<LocalizationCookiesMiddleware>()
            .Configure<RequestLocalizationOptions>(options =>
            {
    
                options.AddSupportedUICultures(LocalizationConstants.SupportedLanguages.Select(x => x.Code).ToArray());
                options.AddSupportedCultures(LocalizationConstants.SupportedLanguages.Select(x => x.Code).ToArray());
                options.DefaultRequestCulture = new RequestCulture(LocalizationConstants.DefaultLanguageCode);
                options.FallBackToParentUICultures = true;
            })
            .AddLocalization(options => options.ResourcesPath = LocalizationConstants.ResourcesPath);

        services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseInMemoryStorage())
            .AddHangfireServer()
            .AddMvc();

        services.AddControllers();

        // Add Swagger/OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "IssueManager UI API",
                Version = "v1",
                Description = "REST API for Issue Management System (UI Project)",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "IssueManager Team",
                    Email = "support@issuemanager.com"
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

        // Add CORS for API endpoints
        services.AddCors(options =>
        {
            options.AddPolicy("ApiCorsPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        
        services.AddScoped<IApplicationHubWrapper, ServerHubWrapper>()
            .AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 64 * 1024;
                options.AddFilter<UserContextHubFilter>();
                
                // Enhanced connection settings for better reliability
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.MaximumParallelInvocationsPerClient = 1;
                options.EnableDetailedErrors = environment?.IsDevelopment() ?? true; // Enable detailed errors for debugging
            });
        
      
        
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddHealthChecks();


        services.AddHttpClient("ocr", (serviceProvider, c) =>
        {
            var aiSettings = serviceProvider.GetRequiredService<IAISettings>();
            c.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent");
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            c.DefaultRequestHeaders.Add("x-goog-api-key", aiSettings.GeminiApiKey);
           
        });
        services.AddHttpContextAccessor();
        services.AddScoped<HubClient>();
        services.AddSingleton<SignalRConnectionService>();
        services
            .AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>()
            .AddScoped<LayoutService>()
            .AddScoped<DialogServiceHelper>()
            .AddScoped<IPermissionHelper, PermissionHelper>()
            .AddScoped<UserPermissionAssignmentService>()
            .AddScoped<RolePermissionAssignmentService>()
            .AddScoped<BlazorDownloadFileService>()
            .AddScoped<IUserPreferencesService, UserPreferencesService>()
            .AddScoped<IMenuService, MenuService>()
            .AddScoped<InMemoryNotificationService>()
            .AddScoped<EscalationNavigationService>()
            .AddScoped<INotificationService>(sp =>
            {
                var service = sp.GetRequiredService<InMemoryNotificationService>();
                service.Preload();
                return service;
            });


        return services;
    }

    /// <summary>
    /// Configures the server pipeline.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The configured web application.</returns>
    public static WebApplication ConfigureServer(this WebApplication app, IConfiguration config)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
            
            // Enable Swagger in development
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IssueManager UI API v1");
                c.RoutePrefix = "swagger"; // Access Swagger UI at /swagger
                c.DocumentTitle = "IssueManager UI API Documentation";
            });
        }
        else
        {
            app.UseExceptionHandler("/Error", true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        
        app.UseStatusCodePagesWithRedirects("/404");
        app.MapHealthChecks("/health");
        app.UseAuthentication();
        app.UseAuthorization();
        
        // Enable CORS for API endpoints
        app.UseCors("ApiCorsPolicy");
        
        app.UseAntiforgery();
        app.UseHttpsRedirection();
        
        // Configure static files with proper caching headers
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                // For development, disable caching of static files to avoid refresh issues
                if (app.Environment.IsDevelopment())
                {
                    ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                    ctx.Context.Response.Headers.Pragma = "no-cache";
                    ctx.Context.Response.Headers.Expires = "-1";
                }
                else
                {
                    // In production, cache static assets for 1 hour
                    const int durationInSeconds = 60 * 60; // 1 hour
                    ctx.Context.Response.Headers.CacheControl = $"public,max-age={durationInSeconds}";
                }
            }
        });
        
        app.MapStaticAssets();
        

        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), @"Files")))
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), @"Files"));



        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Files")),
            RequestPath = new PathString("/Files")
        });

        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(LocalizationConstants.DefaultLanguageCode)
            .AddSupportedCultures(LocalizationConstants.SupportedLanguages.Select(x => x.Code).ToArray())
            .AddSupportedUICultures(LocalizationConstants.SupportedLanguages.Select(x => x.Code).ToArray());

        // Remove AcceptLanguageHeaderRequestCultureProvider to prevent the browser's Accept-Language header from taking effect
        var acceptLanguageProvider = localizationOptions.RequestCultureProviders
            .OfType<AcceptLanguageHeaderRequestCultureProvider>()
            .FirstOrDefault();
        if (acceptLanguageProvider != null)
        {
            localizationOptions.RequestCultureProviders.Remove(acceptLanguageProvider);
        }
        app.UseRequestLocalization(localizationOptions);
        app.UseMiddleware<LocalizationCookiesMiddleware>();
        app.UseExceptionHandler();
        
        // Enhanced WebSocket configuration for better SignalR reliability - MUST come before SignalR mapping
        app.UseWebSockets(new WebSocketOptions()
        {
            KeepAliveInterval = TimeSpan.FromSeconds(15),
            AllowedOrigins = { "*" } // Allow all origins in development
        });
        
        app.UseHangfireDashboard("/jobs", new DashboardOptions
        {
            Authorization = new[] { new HangfireDashboardAuthorizationFilter() },
            AsyncAuthorization = new[] { new HangfireDashboardAsyncAuthorizationFilter() }
        });
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AllowAnonymous(); // Allow anonymous access to prevent auth issues
        app.MapHub<ServerHub>(ISignalRHub.Url, options =>
        {
            options.Transports =
                Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
        });
        
        // Map API controllers
        app.MapControllers();

        //QuestPDF License configuration
        Settings.License = LicenseType.Community;

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();
        app.UseForwardedHeaders();
        
        // Add connection debugging middleware in development
        if (app.Environment.IsDevelopment())
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/_blazor"))
                {
                    Console.WriteLine($"Blazor SignalR request: {context.Request.Method} {context.Request.Path}");
                }
                await next();
            });
        }
        
        // Schedule background jobs
        ProcessCompletedConversationsJob.ScheduleRecurringJob();
       
        return app;
    }
}
