// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using IssueManager.Bot.Middleware;
using IssueManager.Bot.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Globalization;

namespace IssueManager.Bot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public async void ConfigureServices(IServiceCollection services)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
                services.AddSingleton(configuration);

                services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
                });

                // Add health checks for monitoring
                services.AddHealthChecks();

                // Configure HttpClient for IssueManager API
                services.AddHttpClient<IssueManagerApiClient>(client =>
                {
                    var baseUrl = configuration.GetValue<string>("IssueManagerApi:BaseUrl") ?? "https://localhost:7001";
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("User-Agent", "IssueManager-Bot/1.0");
                    
                    // Add timeout configuration
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

                // Register the IssueManager API client as singleton to match IBot lifetime
                services.AddSingleton<IssueManagerApiClient>();

                // Add Application Insights telemetry
                services.AddApplicationInsightsTelemetry(configuration);

                // Add authentication services
                services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.Authority = configuration.GetValue<string>("Authentication:Authority");
                        options.RequireHttpsMetadata = false; // Set based on configuration
                        options.Audience = configuration.GetValue<string>("Authentication:Audience");
                    });

                // Add CORS policy for webhook endpoints
                services.AddCors(options =>
                {
                    options.AddPolicy("WebhookPolicy", policy =>
                    {
                        policy.WithOrigins(configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? new[] { "*" })
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });

                // Create the Bot Framework Authentication to be used with the Bot Adapter.
                services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

                // Create the Bot Adapter with error handling enabled.
                services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

                string userAssignedClientId = configuration.GetValue<string>("MicrosoftAppId");
                var credential = new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId }
                );

                AzureOpenAIClient aoaiClient;
                var aoaiApiKey = configuration.GetValue<string>("AZURE_OPENAI_API_KEY");
                if (string.IsNullOrEmpty(aoaiApiKey))
                {
                    aoaiClient = new AzureOpenAIClient(
                        endpoint: new Uri(configuration.GetValue<string>("AZURE_OPENAI_API_ENDPOINT")),
                        credential: credential
                    );
                }
                else
                {
                    aoaiClient = new AzureOpenAIClient(
                        endpoint: new Uri(configuration.GetValue<string>("AZURE_OPENAI_API_ENDPOINT")),
                        credential: new ApiKeyCredential(aoaiApiKey)
                    );
                }
                services.AddSingleton(aoaiClient);

                Phi phiClient = new Phi(
                    configuration.GetValue<string>("AZURE_AI_PHI_DEPLOYMENT_ENDPOINT"),
                    configuration.GetValue<string>("AZURE_AI_PHI_DEPLOYMENT_KEY")
                );

                services.AddSingleton(phiClient);

                IStorage storage = new MemoryStorage();
                
                // Create the User state passing in the storage layer.
                var userState = new UserState(storage);
                services.AddSingleton(userState);
                
                
                // Create the Conversation state passing in the storage layer.
                var conversationState = new ConversationState(storage);
                services.AddSingleton(conversationState);

                // Add the Login dialog
                services.AddSingleton<LoginDialog>();

                // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
                switch (configuration.GetValue<string>("GEN_AI_IMPLEMENTATION"))
                {
                    case "semantic-kernel":
                        services.AddSingleton<IBot, IssueManager.Bot.Bots.SemanticKernelBot<LoginDialog>>();
                        break;
                    default:
                        throw new Exception("Invalid engine type");
                }
                
                services.AddHttpClient();
            }
            catch (Exception)
            {
                throw;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                //.UseCors("WebhookPolicy")
                //.UseMiddleware<WhatsAppSignatureVerificationMiddleware>()
                //.UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    //endpoints.MapHealthChecks("/healthcheck");
                });

            // app.UseHttpsRedirection();
        }
    }
}
