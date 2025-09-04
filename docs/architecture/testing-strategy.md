# Testing Strategy

## Testing Pyramid
```
                    E2E Tests (5%)
               Integration Tests (15%)
          Frontend Unit    Backend Unit (80%)
```

## Test Organization

**Frontend Tests:**
```
tests/Server.UI.Tests/
├── Components/          # Blazor component tests
├── Pages/              # Page-level integration tests  
├── Services/           # Service layer unit tests
└── EndToEnd/          # Playwright browser tests
```

**Backend Tests:**
```
tests/Application.UnitTests/
├── Features/Issues/Commands/    # Command handler tests
├── Features/Issues/Queries/     # Query handler tests
├── Common/Behaviours/          # Pipeline behavior tests
└── Services/                   # Application service tests
```

**E2E Tests:**
```
tests/EndToEnd.Tests/
├── UserJourneys/       # Complete user workflow tests
├── WhatsAppIntegration/ # Bot conversation flow tests
└── SecurityTests/      # Authentication and authorization tests
```
