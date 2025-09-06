using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Server.UI.Hubs;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Blazor.Server.UI.Services.SignalR
{
    public class SignalRConnectionService : IAsyncDisposable
    {
        private readonly NavigationManager _navigationManager;
        private readonly ILogger<SignalRConnectionService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HubConnection? _hubConnection;
        private Task? _connectionTask;
        private readonly object _lock = new object();

        public SignalRConnectionService(NavigationManager navigationManager, ILogger<SignalRConnectionService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _navigationManager = navigationManager;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public HubConnection? HubConnection => _hubConnection;

        public Task EnsureConnectedAsync()
        {
            lock (_lock)
            {
                if (_connectionTask == null)
                {
                    _connectionTask = InitializeSignalRConnection();
                }
            }
            return _connectionTask;
        }

        private async Task InitializeSignalRConnection()
        {
            try
            {
                var hubUrl = _navigationManager.ToAbsoluteUri(ISignalRHub.Url);
                _logger.LogInformation("SignalR: Initializing connection to {HubUrl}", hubUrl);

                // Configure cookies for authentication
                var uri = new UriBuilder(hubUrl);
                var container = new CookieContainer();
                if (_httpContextAccessor.HttpContext != null)
                {
                    foreach (var c in _httpContextAccessor.HttpContext.Request.Cookies)
                    {
                        var sanitizedValue = Uri.EscapeDataString(c.Value);
                        container.Add(new Cookie(c.Key, sanitizedValue)
                        {
                            Domain = uri.Host,
                            Path = "/"
                        });
                    }
                }

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options =>
                    {
                        options.SkipNegotiation = false;
                        options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                           Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                        options.Cookies = container;
                        options.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    })
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                    .ConfigureLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Information);
                    })
                    .Build();

                _hubConnection.Closed += (error) =>
                {
                    _logger.LogWarning(error, "SignalR connection closed.");
                    return Task.CompletedTask;
                };

                try
                {
                    await _hubConnection.StartAsync();
                    _logger.LogInformation("SignalR connection started successfully. ConnectionId: {ConnectionId}", _hubConnection.ConnectionId);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "A network error occurred while starting the SignalR connection.");
                    if (ex.InnerException is System.Net.Http.HttpRequestException innerEx)
                    {
                        _logger.LogError(innerEx, "Inner HTTP request exception details.");
                    }
                    throw;
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "An invalid operation occurred while starting the SignalR connection. This can happen if negotiation fails.");
                    // Attempt to get more details if possible, though negotiation responses are not always easy to capture here.
                    throw;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize and start SignalR connection.");
                // Reset the task so a retry can happen.
                lock (_lock)
                {
                    _connectionTask = null;
                }
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
