# WhatsApp Bot Service Deployment Script
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Development", "Staging", "Production")]
    [string]$Environment,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName = "issuemanager-$($Environment.ToLower())",
    
    [Parameter(Mandatory=$false)]
    [string]$AppServiceName = "issuemanager-bot-$($Environment.ToLower())",
    
    [Parameter(Mandatory=$false)]
    [string]$AppServicePlan = "issuemanager-plan-$($Environment.ToLower())",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "East US",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild
)

Write-Host "Starting WhatsApp Bot Service Deployment for $Environment" -ForegroundColor Green

# Configuration
$ProjectPath = "src/Bot/IssueManager.Bot.csproj"
$PublishPath = "publish/bot-service"

try {
    # Step 1: Build and publish the Bot service
    if (-not $SkipBuild) {
        Write-Host "Building Bot service..." -ForegroundColor Yellow
        
        dotnet clean $ProjectPath
        if ($LASTEXITCODE -ne 0) { throw "Clean failed" }
        
        dotnet restore $ProjectPath
        if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
        
        dotnet build $ProjectPath --configuration Release
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
        
        Write-Host "Publishing Bot service..." -ForegroundColor Yellow
        dotnet publish $ProjectPath --configuration Release --output $PublishPath --no-build
        if ($LASTEXITCODE -ne 0) { throw "Publish failed" }
    }

    # Step 2: Azure CLI deployment
    Write-Host "Deploying to Azure..." -ForegroundColor Yellow
    
    # Check if logged into Azure
    $azContext = az account show --query "name" -o tsv 2>$null
    if (-not $azContext) {
        Write-Host "Please login to Azure CLI first: az login" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Using Azure subscription: $azContext" -ForegroundColor Cyan
    
    # Create resource group if it doesn't exist
    Write-Host "Ensuring resource group exists: $ResourceGroupName" -ForegroundColor Yellow
    az group create --name $ResourceGroupName --location $Location --only-show-errors
    
    # Create App Service Plan if it doesn't exist
    Write-Host "Ensuring App Service Plan exists: $AppServicePlan" -ForegroundColor Yellow
    $planExists = az appservice plan show --name $AppServicePlan --resource-group $ResourceGroupName --query "name" -o tsv 2>$null
    if (-not $planExists) {
        $sku = if ($Environment -eq "Production") { "P1v2" } elseif ($Environment -eq "Staging") { "S1" } else { "F1" }
        az appservice plan create --name $AppServicePlan --resource-group $ResourceGroupName --sku $sku --only-show-errors
    }
    
    # Create Web App if it doesn't exist
    Write-Host "Ensuring Web App exists: $AppServiceName" -ForegroundColor Yellow
    $appExists = az webapp show --name $AppServiceName --resource-group $ResourceGroupName --query "name" -o tsv 2>$null
    if (-not $appExists) {
        az webapp create --name $AppServiceName --resource-group $ResourceGroupName --plan $AppServicePlan --runtime "DOTNETCORE|9.0" --only-show-errors
        
        # Configure Web App settings
        az webapp config set --name $AppServiceName --resource-group $ResourceGroupName --always-on true --only-show-errors
    }
    
    # Deploy the application
    Write-Host "Deploying application files..." -ForegroundColor Yellow
    Push-Location $PublishPath
    try {
        az webapp deployment source config-zip --name $AppServiceName --resource-group $ResourceGroupName --src "$(Get-Location | Select-Object -ExpandProperty Path | ForEach-Object { Join-Path $_ 'deploy.zip' })" 2>$null
        
        # If zip deployment fails, try alternative method
        if ($LASTEXITCODE -ne 0) {
            # Create zip file for deployment
            Compress-Archive -Path ".\*" -DestinationPath "deploy.zip" -Force
            az webapp deployment source config-zip --name $AppServiceName --resource-group $ResourceGroupName --src "deploy.zip"
        }
    }
    finally {
        Pop-Location
    }
    
    # Step 3: Configure environment-specific application settings
    Write-Host "Configuring application settings..." -ForegroundColor Yellow
    
    $appSettings = @{
        "WEBSITE_RUN_FROM_PACKAGE" = "1"
        "ASPNETCORE_ENVIRONMENT" = $Environment
        "Logging__LogLevel__Default" = if ($Environment -eq "Production") { "Warning" } else { "Information" }
        "WhatsApp__VerifyToken" = "REPLACE_WITH_ACTUAL_TOKEN"
    }
    
    $settingsJson = ($appSettings.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join " "
    az webapp config appsettings set --name $AppServiceName --resource-group $ResourceGroupName --settings $settingsJson --only-show-errors
    
    # Step 4: Restart the application
    Write-Host "Restarting Web App..." -ForegroundColor Yellow
    az webapp restart --name $AppServiceName --resource-group $ResourceGroupName --only-show-errors
    
    # Get the application URL
    $appUrl = az webapp show --name $AppServiceName --resource-group $ResourceGroupName --query "defaultHostName" -o tsv
    
    Write-Host "Deployment completed successfully!" -ForegroundColor Green
    Write-Host "Bot Service URL: https://$appUrl" -ForegroundColor Cyan
    Write-Host "Health Check: https://$appUrl/health" -ForegroundColor Cyan
    Write-Host "Webhook Endpoint: https://$appUrl/webhook" -ForegroundColor Cyan
    
    # Test health endpoint
    Write-Host "Testing health endpoint..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30  # Give the app time to start
    
    try {
        $healthResponse = Invoke-RestMethod -Uri "https://$appUrl/health" -Method Get -TimeoutSec 30
        Write-Host "Health check successful: $($healthResponse.Status)" -ForegroundColor Green
    }
    catch {
        Write-Host "Health check failed (this may be expected during initial deployment): $($_.Exception.Message)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "Deployment script completed!" -ForegroundColor Green