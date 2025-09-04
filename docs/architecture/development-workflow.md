# Development Workflow

## Prerequisites

```bash
# Install .NET 9 SDK
winget install Microsoft.DotNet.SDK.9

# Install Node.js for front-end tooling
winget install OpenJS.NodeJS

# Install SQL Server LocalDB (for development)
winget install Microsoft.SQLServer.2022.LocalDB

# Install Azure CLI (for cloud services)
winget install Microsoft.AzureCLI

# Install Bot Framework Emulator (for bot testing)
winget install Microsoft.BotFrameworkEmulator

# Install Docker Desktop (for containerized services)
winget install Docker.DockerDesktop

# Verify installations
dotnet --version          # Should show 9.0.x
node --version           # Should show 18.x or higher
sqlcmd -?               # Should show SQL Server command line tool
az --version            # Should show Azure CLI version
```

## Initial Setup

```bash
# Clone the repository
git clone <repository-url> IssueManager
cd IssueManager

# Restore .NET packages for all projects
dotnet restore

# Set up user secrets for development
dotnet user-secrets init --project src/Server.UI
dotnet user-secrets set "DatabaseSettings:ConnectionString" "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=IssueManagerDB;Integrated Security=True" --project src/Server.UI

dotnet user-secrets init --project src/Bot
dotnet user-secrets set "BotFramework:MicrosoftAppId" "your-bot-app-id" --project src/Bot
dotnet user-secrets set "BotFramework:MicrosoftAppPassword" "your-bot-app-password" --project src/Bot

# Run database migrations
dotnet ef database update --project src/Migrators/Migrators.MSSQL --startup-project src/Server.UI

# Seed initial data (optional)
dotnet run --project src/Server.UI -- --seed-data

# Install npm packages for UI tooling
npm install --prefix src/Server.UI/wwwroot

# Set up development certificates
dotnet dev-certs https --trust
```

## Development Commands

```bash
# Start all services in development mode
dotnet run --project src/Server.UI --environment Development
# UI available at: https://localhost:5001

# Start Bot service separately (in new terminal)
dotnet run --project src/Bot --environment Development
# Bot webhook at: https://localhost:5002/api/messages

# Watch mode for automatic rebuilds during development
dotnet watch --project src/Server.UI

# Run specific database migration
dotnet ef migrations add AddNewFeature --project src/Migrators/Migrators.MSSQL --startup-project src/Server.UI

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Application.UnitTests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults

# Build for production
dotnet build --configuration Release

# Publish applications
dotnet publish src/Server.UI --configuration Release --output ./publish/ui
dotnet publish src/Bot --configuration Release --output ./publish/bot
```
