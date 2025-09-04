# External APIs

## WhatsApp Business API

- **Purpose:** Enable WhatsApp channel integration for issue intake and customer communication
- **Documentation:** https://developers.facebook.com/docs/whatsapp/cloud-api
- **Base URL(s):** https://graph.facebook.com/v18.0/
- **Authentication:** Bearer token with WhatsApp Business Account access
- **Rate Limits:** 1000 messages per second, 250,000 messages per day (varies by tier)

**Key Endpoints Used:**
- `POST /{phone-number-id}/messages` - Send messages to customers
- `GET /{phone-number-id}/media/{media-id}` - Download media attachments
- `POST /webhook` - Receive incoming messages (webhook setup)

**Integration Notes:** Webhook verification required, media downloads need virus scanning before storage, message templates must be pre-approved for automated responses

## Azure Communication Services API

- **Purpose:** Alternative WhatsApp integration option with enterprise-grade reliability and Azure native integration
- **Documentation:** https://docs.microsoft.com/en-us/azure/communication-services/
- **Base URL(s):** https://{resource-name}.communication.azure.com/
- **Authentication:** Azure Active Directory or access key authentication
- **Rate Limits:** 100 requests per second per resource, higher limits available

**Key Endpoints Used:**
- `POST /messages` - Send WhatsApp messages
- `GET /messages/{messageId}` - Get message delivery status
- `POST /webhook` - Receive message events

**Integration Notes:** Native Azure integration simplifies authentication and monitoring, supports advanced message types including interactive buttons and lists

## Semantic Kernel / Azure OpenAI API

- **Purpose:** AI-powered conversation processing, intent recognition, and automated response generation for bot interactions
- **Documentation:** https://docs.microsoft.com/en-us/semantic-kernel/
- **Base URL(s):** https://{resource}.openai.azure.com/
- **Authentication:** API key or Azure Active Directory token
- **Rate Limits:** Varies by model and tier (typically 60K tokens per minute)

**Key Endpoints Used:**
- `POST /openai/deployments/{deployment-id}/chat/completions` - Process conversations
- `POST /openai/deployments/{deployment-id}/embeddings` - Generate embeddings for semantic search

**Integration Notes:** Requires prompt engineering for issue extraction, content filtering for safety, token usage monitoring for cost management
