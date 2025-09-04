# Monitoring and Observability

## Monitoring Stack
- **Frontend Monitoring:** Application Insights Real User Monitoring (RUM) with custom telemetry for business metrics
- **Backend Monitoring:** Application Insights with structured logging via Serilog, custom performance counters
- **Error Tracking:** Application Insights exception tracking with PII scrubbing and alert rules
- **Performance Monitoring:** Application Insights dependency tracking, database query performance analysis

## Key Metrics
**Frontend Metrics:**
- Core Web Vitals (LCP < 2.5s, FID < 100ms, CLS < 0.1)
- JavaScript errors and unhandled promise rejections
- SignalR connection success rates and message delivery latency
- User interaction funnel metrics (issue creation completion rate)

**Backend Metrics:**
- Request rate per endpoint with P95 response times
- Database connection pool utilization and query execution times
- WhatsApp message processing success rate and AI extraction accuracy
- Authentication failure rate and security event frequency
