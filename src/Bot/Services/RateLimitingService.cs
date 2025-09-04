using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IssueManager.Bot.Services
{
    /// <summary>
    /// Service for managing API rate limits and quotas for WhatsApp Business API
    /// </summary>
    public class RateLimitingService
    {
        private readonly ILogger<RateLimitingService> _logger;
        private readonly IConfiguration _configuration;
        
        // Rate limiting storage
        private readonly ConcurrentDictionary<string, TokenBucket> _inboundBuckets = new();
        private readonly ConcurrentDictionary<string, TokenBucket> _outboundBuckets = new();
        
        // Circuit breaker state
        private readonly ConcurrentDictionary<string, CircuitBreakerState> _circuitBreakers = new();
        
        // Configuration values
        private readonly int _maxInboundRequestsPerSecond;
        private readonly int _maxOutboundRequestsPerSecond;
        private readonly int _maxDailyOutboundMessages;
        private readonly TimeSpan _circuitBreakerTimeout;

        public RateLimitingService(ILogger<RateLimitingService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            // Load configuration with WhatsApp API defaults
            _maxInboundRequestsPerSecond = configuration.GetValue<int>("RateLimit:InboundRequestsPerSecond", 100);
            _maxOutboundRequestsPerSecond = configuration.GetValue<int>("RateLimit:OutboundRequestsPerSecond", 1000);
            _maxDailyOutboundMessages = configuration.GetValue<int>("RateLimit:DailyOutboundMessages", 250000);
            _circuitBreakerTimeout = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimit:CircuitBreakerTimeoutMinutes", 5));
        }

        /// <summary>
        /// Checks if an inbound webhook request is allowed under rate limits
        /// </summary>
        /// <param name="clientIdentifier">Client identifier (IP address, etc.)</param>
        /// <returns>True if request is allowed</returns>
        public bool IsInboundRequestAllowed(string clientIdentifier)
        {
            try
            {
                var bucket = _inboundBuckets.GetOrAdd(clientIdentifier, _ => 
                    new TokenBucket(_maxInboundRequestsPerSecond, TimeSpan.FromSeconds(1)));

                var allowed = bucket.TryConsume();
                
                if (!allowed)
                {
                    _logger.LogWarning("Inbound rate limit exceeded for client {ClientId}", clientIdentifier);
                }

                return allowed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking inbound rate limit for {ClientId}", clientIdentifier);
                return false; // Fail closed for security
            }
        }

        /// <summary>
        /// Checks if an outbound API call is allowed under rate limits and quota
        /// </summary>
        /// <param name="apiEndpoint">API endpoint identifier</param>
        /// <returns>True if call is allowed</returns>
        public bool IsOutboundCallAllowed(string apiEndpoint = "whatsapp-api")
        {
            try
            {
                // Check circuit breaker first
                if (!IsCircuitBreakerClosed(apiEndpoint))
                {
                    _logger.LogWarning("Circuit breaker is open for {ApiEndpoint} - blocking call", apiEndpoint);
                    return false;
                }

                // Check per-second rate limit
                var bucket = _outboundBuckets.GetOrAdd(apiEndpoint, _ => 
                    new TokenBucket(_maxOutboundRequestsPerSecond, TimeSpan.FromSeconds(1)));

                if (!bucket.TryConsume())
                {
                    _logger.LogWarning("Outbound rate limit exceeded for {ApiEndpoint}", apiEndpoint);
                    return false;
                }

                // Check daily quota (simplified - in production would use persistent storage)
                if (!CheckDailyQuota(apiEndpoint))
                {
                    _logger.LogWarning("Daily quota exceeded for {ApiEndpoint}", apiEndpoint);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking outbound rate limit for {ApiEndpoint}", apiEndpoint);
                return false; // Fail closed for safety
            }
        }

        /// <summary>
        /// Records a successful API call for quota tracking
        /// </summary>
        /// <param name="apiEndpoint">API endpoint identifier</param>
        public void RecordSuccessfulCall(string apiEndpoint = "whatsapp-api")
        {
            try
            {
                // Reset circuit breaker on successful call
                ResetCircuitBreaker(apiEndpoint);
                
                _logger.LogDebug("Recorded successful API call for {ApiEndpoint}", apiEndpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording successful call for {ApiEndpoint}", apiEndpoint);
            }
        }

        /// <summary>
        /// Records a failed API call for circuit breaker logic
        /// </summary>
        /// <param name="apiEndpoint">API endpoint identifier</param>
        /// <param name="exception">Optional exception that caused the failure</param>
        public void RecordFailedCall(string apiEndpoint, Exception? exception = null)
        {
            try
            {
                var circuitBreaker = _circuitBreakers.GetOrAdd(apiEndpoint, _ => new CircuitBreakerState());
                
                circuitBreaker.RecordFailure();
                
                if (circuitBreaker.ShouldTripCircuitBreaker())
                {
                    circuitBreaker.TripCircuitBreaker(_circuitBreakerTimeout);
                    _logger.LogWarning("Circuit breaker tripped for {ApiEndpoint} due to {FailureCount} consecutive failures", 
                        apiEndpoint, circuitBreaker.ConsecutiveFailures);
                }

                _logger.LogWarning(exception, "Recorded failed API call for {ApiEndpoint}, consecutive failures: {FailureCount}", 
                    apiEndpoint, circuitBreaker.ConsecutiveFailures);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording failed call for {ApiEndpoint}", apiEndpoint);
            }
        }

        /// <summary>
        /// Gets current rate limit status for monitoring
        /// </summary>
        /// <returns>Rate limit status information</returns>
        public RateLimitStatus GetStatus()
        {
            return new RateLimitStatus
            {
                InboundBucketsCount = _inboundBuckets.Count,
                OutboundBucketsCount = _outboundBuckets.Count,
                CircuitBreakersCount = _circuitBreakers.Count,
                OpenCircuitBreakers = _circuitBreakers
                    .Where(cb => cb.Value.IsOpen)
                    .Select(cb => cb.Key)
                    .ToList(),
                MaxInboundRequestsPerSecond = _maxInboundRequestsPerSecond,
                MaxOutboundRequestsPerSecond = _maxOutboundRequestsPerSecond,
                MaxDailyOutboundMessages = _maxDailyOutboundMessages
            };
        }

        private bool IsCircuitBreakerClosed(string apiEndpoint)
        {
            if (!_circuitBreakers.TryGetValue(apiEndpoint, out var circuitBreaker))
                return true; // No circuit breaker means it's closed

            return !circuitBreaker.IsOpen;
        }

        private void ResetCircuitBreaker(string apiEndpoint)
        {
            if (_circuitBreakers.TryGetValue(apiEndpoint, out var circuitBreaker))
            {
                circuitBreaker.RecordSuccess();
            }
        }

        private bool CheckDailyQuota(string apiEndpoint)
        {
            // Simplified daily quota check - in production, this would use persistent storage
            // For now, we'll just return true and rely on WhatsApp API to enforce quotas
            return true;
        }
    }

    /// <summary>
    /// Token bucket for rate limiting
    /// </summary>
    public class TokenBucket
    {
        private readonly int _maxTokens;
        private readonly TimeSpan _refillInterval;
        private int _tokens;
        private DateTime _lastRefill;
        private readonly object _lock = new();

        public TokenBucket(int maxTokens, TimeSpan refillInterval)
        {
            _maxTokens = maxTokens;
            _refillInterval = refillInterval;
            _tokens = maxTokens;
            _lastRefill = DateTime.UtcNow;
        }

        public bool TryConsume(int tokensRequested = 1)
        {
            lock (_lock)
            {
                RefillTokens();
                
                if (_tokens >= tokensRequested)
                {
                    _tokens -= tokensRequested;
                    return true;
                }
                
                return false;
            }
        }

        private void RefillTokens()
        {
            var now = DateTime.UtcNow;
            var elapsed = now - _lastRefill;
            
            if (elapsed >= _refillInterval)
            {
                _tokens = _maxTokens;
                _lastRefill = now;
            }
        }
    }

    /// <summary>
    /// Circuit breaker state management
    /// </summary>
    public class CircuitBreakerState
    {
        private readonly int _failureThreshold;
        private int _consecutiveFailures;
        private DateTime? _tripTime;
        private TimeSpan _timeout;

        public CircuitBreakerState(int failureThreshold = 5)
        {
            _failureThreshold = failureThreshold;
        }

        public int ConsecutiveFailures => _consecutiveFailures;
        public bool IsOpen => _tripTime.HasValue && DateTime.UtcNow < _tripTime.Value + _timeout;

        public void RecordFailure()
        {
            Interlocked.Increment(ref _consecutiveFailures);
        }

        public void RecordSuccess()
        {
            Interlocked.Exchange(ref _consecutiveFailures, 0);
            _tripTime = null;
        }

        public bool ShouldTripCircuitBreaker()
        {
            return _consecutiveFailures >= _failureThreshold && !IsOpen;
        }

        public void TripCircuitBreaker(TimeSpan timeout)
        {
            _timeout = timeout;
            _tripTime = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Rate limiting status for monitoring
    /// </summary>
    public class RateLimitStatus
    {
        public int InboundBucketsCount { get; set; }
        public int OutboundBucketsCount { get; set; }
        public int CircuitBreakersCount { get; set; }
        public List<string> OpenCircuitBreakers { get; set; } = new();
        public int MaxInboundRequestsPerSecond { get; set; }
        public int MaxOutboundRequestsPerSecond { get; set; }
        public int MaxDailyOutboundMessages { get; set; }
    }
}