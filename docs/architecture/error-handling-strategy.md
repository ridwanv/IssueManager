# Error Handling Strategy

## Error Flow
```mermaid
sequenceDiagram
    participant UI as Blazor Component
    participant API as Controller
    participant App as Application Handler
    participant Infra as Infrastructure Service
    
    UI->>API: HTTP Request
    API->>App: Command/Query via MediatR
    App->>Infra: Service call
    Infra-->>App: Exception thrown
    App->>App: Catch & wrap in Result<T>
    App-->>API: Result.Failure(message)
    API->>API: Map to HTTP status code
    API-->>UI: HTTP error response
    UI->>UI: Display user-friendly message
    UI->>UI: Log technical details
```

## Error Response Format
```typescript
interface ApiError {
  error: {
    code: string;           // ERROR_ISSUE_NOT_FOUND
    message: string;        // User-friendly message
    details?: Record<string, any>; // Validation errors
    timestamp: string;      // ISO 8601 timestamp
    requestId: string;      // Correlation ID for logs
  };
}
```
