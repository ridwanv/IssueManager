#!/bin/bash

# WhatsApp Bot Service Deployment Script (Linux/Mac)
set -e

# Default values
ENVIRONMENT=""
RESOURCE_GROUP=""
APP_SERVICE_NAME=""
APP_SERVICE_PLAN=""
LOCATION="East US"
SKIP_BUILD=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -g|--resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        -a|--app-service)
            APP_SERVICE_NAME="$2"
            shift 2
            ;;
        -p|--app-service-plan)
            APP_SERVICE_PLAN="$2"
            shift 2
            ;;
        -l|--location)
            LOCATION="$2"
            shift 2
            ;;
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 -e <environment> [options]"
            echo "Options:"
            echo "  -e, --environment        Environment (Development|Staging|Production)"
            echo "  -g, --resource-group     Azure resource group name"
            echo "  -a, --app-service        Azure app service name"
            echo "  -p, --app-service-plan   Azure app service plan name"
            echo "  -l, --location           Azure location (default: East US)"
            echo "  --skip-build             Skip the build and publish step"
            echo "  -h, --help               Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Validate required parameters
if [ -z "$ENVIRONMENT" ]; then
    echo "Error: Environment parameter is required. Use -e <environment>"
    echo "Valid environments: Development, Staging, Production"
    exit 1
fi

# Validate environment
case $ENVIRONMENT in
    Development|Staging|Production)
        ;;
    *)
        echo "Error: Invalid environment. Must be Development, Staging, or Production"
        exit 1
        ;;
esac

# Set default values based on environment
if [ -z "$RESOURCE_GROUP" ]; then
    RESOURCE_GROUP="issuemanager-$(echo $ENVIRONMENT | tr '[:upper:]' '[:lower:]')"
fi

if [ -z "$APP_SERVICE_NAME" ]; then
    APP_SERVICE_NAME="issuemanager-bot-$(echo $ENVIRONMENT | tr '[:upper:]' '[:lower:]')"
fi

if [ -z "$APP_SERVICE_PLAN" ]; then
    APP_SERVICE_PLAN="issuemanager-plan-$(echo $ENVIRONMENT | tr '[:upper:]' '[:lower:]')"
fi

# Configuration
PROJECT_PATH="src/Bot/IssueManager.Bot.csproj"
PUBLISH_PATH="publish/bot-service"

echo "Starting WhatsApp Bot Service Deployment for $ENVIRONMENT"

# Step 1: Build and publish the Bot service
if [ "$SKIP_BUILD" = false ]; then
    echo "Building Bot service..."
    
    dotnet clean $PROJECT_PATH
    dotnet restore $PROJECT_PATH
    dotnet build $PROJECT_PATH --configuration Release
    
    echo "Publishing Bot service..."
    dotnet publish $PROJECT_PATH --configuration Release --output $PUBLISH_PATH --no-build
fi

# Step 2: Azure CLI deployment
echo "Deploying to Azure..."

# Check if logged into Azure
if ! az account show > /dev/null 2>&1; then
    echo "Please login to Azure CLI first: az login"
    exit 1
fi

AZ_SUBSCRIPTION=$(az account show --query "name" -o tsv)
echo "Using Azure subscription: $AZ_SUBSCRIPTION"

# Create resource group if it doesn't exist
echo "Ensuring resource group exists: $RESOURCE_GROUP"
az group create --name $RESOURCE_GROUP --location "$LOCATION" --only-show-errors > /dev/null

# Create App Service Plan if it doesn't exist
echo "Ensuring App Service Plan exists: $APP_SERVICE_PLAN"
if ! az appservice plan show --name $APP_SERVICE_PLAN --resource-group $RESOURCE_GROUP > /dev/null 2>&1; then
    case $ENVIRONMENT in
        Production)
            SKU="P1v2"
            ;;
        Staging)
            SKU="S1"
            ;;
        *)
            SKU="F1"
            ;;
    esac
    az appservice plan create --name $APP_SERVICE_PLAN --resource-group $RESOURCE_GROUP --sku $SKU --only-show-errors > /dev/null
fi

# Create Web App if it doesn't exist
echo "Ensuring Web App exists: $APP_SERVICE_NAME"
if ! az webapp show --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP > /dev/null 2>&1; then
    az webapp create --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP --plan $APP_SERVICE_PLAN --runtime "DOTNETCORE|9.0" --only-show-errors > /dev/null
    
    # Configure Web App settings
    az webapp config set --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP --always-on true --only-show-errors > /dev/null
fi

# Deploy the application
echo "Deploying application files..."
pushd $PUBLISH_PATH > /dev/null

# Create zip file for deployment
zip -r deploy.zip . > /dev/null
az webapp deployment source config-zip --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP --src deploy.zip > /dev/null

popd > /dev/null

# Step 3: Configure environment-specific application settings
echo "Configuring application settings..."

SETTINGS="WEBSITE_RUN_FROM_PACKAGE=1 ASPNETCORE_ENVIRONMENT=$ENVIRONMENT"

case $ENVIRONMENT in
    Production)
        SETTINGS="$SETTINGS Logging__LogLevel__Default=Warning"
        ;;
    *)
        SETTINGS="$SETTINGS Logging__LogLevel__Default=Information"
        ;;
esac

SETTINGS="$SETTINGS WhatsApp__VerifyToken=REPLACE_WITH_ACTUAL_TOKEN"

az webapp config appsettings set --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP --settings $SETTINGS --only-show-errors > /dev/null

# Step 4: Restart the application
echo "Restarting Web App..."
az webapp restart --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP --only-show-errors > /dev/null

# Get the application URL
APP_URL=$(az webapp show --name $APP_SERVICE_NAME --resource-group $RESOURCE_GROUP --query "defaultHostName" -o tsv)

echo "Deployment completed successfully!"
echo "Bot Service URL: https://$APP_URL"
echo "Health Check: https://$APP_URL/health"
echo "Webhook Endpoint: https://$APP_URL/webhook"

# Test health endpoint
echo "Testing health endpoint..."
sleep 30  # Give the app time to start

if curl -s --max-time 30 "https://$APP_URL/health" > /dev/null; then
    echo "Health check successful"
else
    echo "Health check failed (this may be expected during initial deployment)"
fi

echo "Deployment script completed!"