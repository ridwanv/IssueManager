# Deployment Architecture

## Deployment Strategy

**Frontend Deployment:**
- **Platform:** Azure App Service (Windows)
- **Build Command:** `dotnet publish src/Server.UI --configuration Release --output ./publish/ui`
- **Output Directory:** `./publish/ui`
- **CDN/Edge:** Azure CDN with static asset caching and global distribution

**Backend Deployment:**
- **Platform:** Azure App Service (Windows) - Same as frontend for Blazor Server
- **Build Command:** `dotnet publish src/Bot --configuration Release --output ./publish/bot`
- **Deployment Method:** Azure DevOps CI/CD pipeline with staging slots

## Environments

| Environment | Frontend URL | Backend URL | Purpose |
|-------------|-------------|-------------|---------|
| Development | https://localhost:5001 | https://localhost:5002 | Local development and testing |
| Staging | https://issuemanager-staging.azurewebsites.net | https://issuemanager-bot-staging.azurewebsites.net | Pre-production testing and validation |
| Production | https://issuemanager.azurewebsites.net | https://issuemanager-bot.azurewebsites.net | Live production environment |
