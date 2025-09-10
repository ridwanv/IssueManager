using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Server.UI.Hubs;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Blazor.Server.UI.Services.SignalR
{
    public class SignalRConnectionService : IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SignalRConnectionService> _logger;
        private HubConnection? _hubConnection;
        private Task? _connectionTask;
        private readonly object _lock = new object();
        private volatile bool _isDisposed = false;

        public SignalRConnectionService(IServiceProvider serviceProvider, ILogger<SignalRConnectionService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public HubConnection? HubConnection => _hubConnection;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected && !_isDisposed;

        public Task EnsureConnectedAsync()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SignalRConnectionService));
            }

            lock (_lock)
            {
                if (_connectionTask == null || _connectionTask.IsFaulted)
                {
                    _connectionTask = InitializeSignalRConnection();
                }
            }
            return _connectionTask;
        }

        private async Task InitializeSignalRConnection()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SignalRConnectionService));
            }

            try
            {
                // Create a scope to access scoped services
                using var scope = _serviceProvider.CreateScope();
                var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

                // Build the hub URL manually since NavigationManager might not be initialized
                var httpContext = httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    throw new InvalidOperationException("HttpContext is not available");
                }

                var request = httpContext.Request;
                var scheme = request.Scheme;
                var host = request.Host;
                var hubUrl = new Uri($"{scheme}://{host}{ISignalRHub.Url}");
                
                _logger.LogInformation("SignalR: Initializing connection to {HubUrl}", hubUrl);

                // Configure cookies for authentication
                var uri = new UriBuilder(hubUrl);
                var container = new CookieContainer();
                foreach (var c in httpContext.Request.Cookies)
                {
                    var sanitizedValue = Uri.EscapeDataString(c.Value);
                    container.Add(new Cookie(c.Key, sanitizedValue)
                    {
                        Domain = uri.Host,
                        Path = "/"
                    });
                }

                // Dispose existing connection if any
                if (_hubConnection != null)
                {
                    await _hubConnection.DisposeAsync();
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

        public async Task<bool> TryInvokeAsync(string methodName, params object[] args)
        {
            if (_isDisposed || _hubConnection == null)
            {
                return false;
            }

            try
            {
                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.SendAsync(methodName, args);
                    return true;
                }
            }
            catch (ObjectDisposedException)
            {
                _logger.LogWarning("Attempted to invoke {MethodName} on disposed SignalR connection", methodName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking {MethodName} on SignalR connection", methodName);
            }

            return false;
        }

        public async ValueTask DisposeAsync()
        {
            _isDisposed = true;
            
            if (_hubConnection is not null)
            {
                try
                {
                    await _hubConnection.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error disposing SignalR connection");
                }
            }
        }
    }
}
