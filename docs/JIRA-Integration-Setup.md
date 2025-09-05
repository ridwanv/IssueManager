# JIRA Integration Setup Guide

## Overview
The JIRA integration provides bidirectional synchronization between the Issue Manager system and JIRA. When issues are created in the system (via WhatsApp or web), they are automatically created in JIRA. When issues are updated in JIRA, the local system is updated and WhatsApp notifications are sent to reporters.

## Features Implemented

### Phase 1: Issue → JIRA (✅ Complete)
- Automatic JIRA issue creation when local issues are created
- Similarity checking prevents duplicate JIRA issues
- JIRA metadata stored locally (JiraKey, JiraUrl, timestamps)
- Non-blocking error handling (local issue creation succeeds even if JIRA fails)

### Phase 2: JIRA → WhatsApp (✅ Complete)
- JIRA webhook endpoint for receiving updates
- Automatic WhatsApp notifications when JIRA issues are updated
- Status, priority, and assignee changes trigger notifications
- Configurable field mapping between JIRA and local system

## Configuration

### 1. JIRA API Settings
Add to `appsettings.json`:

```json
{
  "JiraConfiguration": {
    "BaseUrl": "https://your-company.atlassian.net",
    "Username": "your-email@company.com",
    "ApiToken": "your-jira-api-token",
    "ProjectKey": "SUP",
    "DefaultIssueType": "Task",
    "Enabled": true,
    "WebhookSecret": "your-webhook-secret",
    "FieldMappings": {
      "Priority": {
        "Low": "Low",
        "Medium": "Medium", 
        "High": "High",
        "Critical": "Highest"
      },
      "Status": {
        "New": "To Do",
        "InProgress": "In Progress",
        "Resolved": "Done",
        "Closed": "Done"
      }
    }
  }
}
```

### 2. JIRA API Token Setup
1. Go to https://id.atlassian.com/manage-profile/security/api-tokens
2. Create a new API token
3. Add the token to your configuration

### 3. Webhook Configuration
1. In JIRA, go to Settings → System → Webhooks
2. Create a new webhook with URL: `https://your-domain.com/api/jirawebhook/issue-updated`
3. Select events: Issue Updated, Issue Deleted
4. Add the webhook secret to your configuration

### 4. Database Migration
Apply the migration to add JIRA fields to the Issue entity:

```bash
cd src/Migrators/Migrators.SqLite
dotnet ef database update
```

## API Endpoints

### JIRA Webhook Endpoint
- **URL**: `/api/jirawebhook/issue-updated`
- **Method**: POST
- **Purpose**: Receives updates from JIRA and triggers local updates + WhatsApp notifications

### Health Check
- **URL**: `/api/jirawebhook/health`
- **Method**: GET
- **Purpose**: Monitors webhook endpoint availability

## Event Flow

### Issue Creation Flow
1. User creates issue via WhatsApp → `IssueIntakePlugin.cs`
2. `CreateIssueCommandHandler` creates local issue
3. `IssueCreatedEvent` triggers `IssueCreatedJiraHandler`
4. JIRA issue created via `JiraService`
5. Local issue updated with JIRA metadata

### JIRA Update Flow  
1. JIRA issue updated → Webhook sent to `/api/jirawebhook/issue-updated`
2. `JiraWebhookController` processes webhook
3. `UpdateIssueCommand` updates local issue
4. `IssueUpdatedFromJiraEvent` triggers `IssueUpdatedFromJiraWhatsAppHandler`
5. WhatsApp notification sent to reporter

## Components

### Core Services
- `JiraService`: JIRA REST API client
- `JiraConfiguration`: Configuration model
- `JiraWebhookController`: Webhook endpoint

### Event Handlers
- `IssueCreatedJiraHandler`: Creates JIRA issues
- `IssueUpdatedFromJiraWhatsAppHandler`: Sends WhatsApp notifications

### Commands
- `UpdateIssueCommand`: Handles JIRA webhook updates
- `UpdateIssueCommandHandler`: Processes field updates and triggers events

### Models
- `JiraIssue`: JIRA issue representation
- `JiraField`: Field definitions
- `JiraWebhookPayload`: Webhook data structure

## Testing

### Manual Testing
1. Create an issue via WhatsApp
2. Verify JIRA issue created
3. Update the JIRA issue
4. Verify local issue updated
5. Verify WhatsApp notification sent

### Health Checks
- Monitor webhook endpoint: `/api/jirawebhook/health`
- Check JIRA connectivity via health checks
- Monitor logs for integration errors

## Error Handling

### JIRA API Failures
- Local issue creation continues even if JIRA fails
- Retry logic with exponential backoff
- Detailed error logging

### Webhook Failures
- Webhook signature validation
- Graceful handling of malformed payloads
- Non-blocking error handling (WhatsApp notifications don't fail issue updates)

## Security

### JIRA API
- API token authentication
- HTTPS required
- Rate limiting compliance

### Webhooks
- Signature validation (configurable)
- HTTPS required
- Input sanitization

## Monitoring

### Logs
- JIRA API calls and responses
- Webhook processing
- Error tracking
- Performance metrics

### Metrics
- JIRA integration success/failure rates
- Webhook processing times
- WhatsApp notification delivery

## Troubleshooting

### Common Issues
1. **JIRA API authentication errors**: Check API token and permissions
2. **Webhook not receiving updates**: Verify webhook URL and JIRA configuration
3. **WhatsApp notifications not sent**: Check WhatsApp Bot service status
4. **Field mapping errors**: Verify field mappings in configuration

### Debug Steps
1. Check application logs
2. Verify JIRA API connectivity
3. Test webhook endpoint manually
4. Validate configuration settings
