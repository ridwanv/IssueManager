// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable enable
using Plugins;
using IssueManager.Bot.Services;
using IssueManager.Bot.Plugins;
using IssueManager.Bot.Models;

namespace IssueManager.Bot.Bots
{
    public class SemanticKernelBot<T> : StateManagementBot<T> where T : Dialog
    {
        private readonly string _instructions;
        private readonly string _welcomeMessage;
        private readonly string _endpoint;
        private readonly string _deployment;
        private readonly string _apiKey;
        private readonly TokenCredential _credential;
        private readonly IssueManagerApiClient _apiClient;
        private readonly ConversationHandoffService _handoffService;

        public SemanticKernelBot(IConfiguration config, ConversationState conversationState, UserState userState, AzureOpenAIClient aoaiClient, T dialog, IssueManagerApiClient apiClient): base(config, conversationState, userState, dialog)
        {
            _instructions = config["LLM_INSTRUCTIONS"] ?? "You are a helpful assistant.";
            _welcomeMessage = config.GetValue("LLM_WELCOME_MESSAGE", "Hello and welcome to the Semantic Kernel Bot Dotnet!");
            _endpoint = config["AZURE_OPENAI_API_ENDPOINT"] ?? throw new InvalidOperationException("AZURE_OPENAI_API_ENDPOINT is required");
            _deployment = config["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is required");
            _apiKey = config["AZURE_OPENAI_API_KEY"] ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is required");
            _credential = new DefaultAzureCredential();
            _apiClient = apiClient;
            _handoffService = new ConversationHandoffService();
        }

        // Modify onMembersAdded as needed
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(_welcomeMessage), cancellationToken);
                }
            }
            // Log in at the start of the conversation
            await HandleLogin(turnContext, cancellationToken);
        }

        protected override async Task<bool> OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Load conversation state
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await conversationStateAccessors.GetAsync(
                turnContext, () => new ConversationData()
                {
                    History = new List<ConversationTurn>() {
                        new() { Role = "system", Message = _instructions }
                    }
                });
                
            // Handle Bot Framework handoff events
            if (turnContext.Activity.Type == ActivityTypes.Event)
            {
                var handled = await _handoffService.HandleHandoffEvent(turnContext, conversationData);
                if (handled)
                {
                    await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                    return true;
                }
            }

            // Enforce login
            var loggedIn = await HandleLogin(turnContext, cancellationToken);
            if (!loggedIn)
            {
                return false;
            }

            // Add user message to history
            conversationData.AddTurn("user", turnContext.Activity.Text);
            
            // Store user message in database via API
            await StoreMessageInDatabase(turnContext, "user", turnContext.Activity.Text);
            
            // Check for escalation intent before processing with AI
            var escalationPlugin = new HumanEscalationPlugin(conversationData, turnContext, _apiClient);
            var shouldEscalate = escalationPlugin.DetectEscalationIntent(turnContext.Activity.Text);
            
            if (shouldEscalate && !conversationData.IsEscalated)
            {
                await escalationPlugin.EscalateToHuman("User requested human assistance", 
                    "User expressed need for human support");
                    
                // Create and send Bot Framework handoff initiation event
                var handoffEvent = await _handoffService.CreateHandoffInitiation(
                    turnContext, conversationData, 
                    "User requested human assistance", 
                    "User expressed need for human support");
                await turnContext.SendActivityAsync(handoffEvent, cancellationToken);
                
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                
                var escalationMessage = "I understand you'd like to speak with a human agent. Let me connect you right away. " +
                                      "An agent will join this conversation shortly with full context of our discussion.";
                await turnContext.SendActivityAsync(MessageFactory.Text(escalationMessage), cancellationToken);
                
                // Store escalation message in database
                await StoreMessageInDatabase(turnContext, "assistant", escalationMessage);
                
                return true;
            }

            var kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(_deployment, _endpoint, _apiKey)
                .Build();

            // Add custom plugins with API client instead of MediatR
            kernel.Plugins.Add(kernel.CreatePluginFromObject(new IssueIntakePlugin(conversationData, turnContext, _apiClient)));
            kernel.Plugins.Add(kernel.CreatePluginFromObject(new HumanEscalationPlugin(conversationData, turnContext, _apiClient)));

            // Create ChatCompletionService
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Execute conversation
            var chatHistory = new ChatHistory();
            foreach (var turn in conversationData.History)
            {
                switch (turn.Role)
                {
                    case "system":
                        chatHistory.AddSystemMessage(turn.Message);
                        break;
                    case "user":
                        chatHistory.AddUserMessage(turn.Message);
                        break;
                    case "assistant":
                        chatHistory.AddAssistantMessage(turn.Message);
                        break;
                }
            }

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.7,
                MaxTokens = 2000
            };

            var response = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                kernel,
                cancellationToken);

            // Add response to conversation history
            conversationData.AddTurn("assistant", response.Content);
            
            // Check for escalation triggers after AI response
            var escalationTriggers = escalationPlugin.CheckEscalationTriggers();
            if (!string.IsNullOrEmpty(escalationTriggers) && !conversationData.IsEscalated)
            {
                // Add escalation suggestion to response
                var enhancedResponse = response.Content + 
                    "\n\n💡 *Would you like me to connect you with a human agent for more personalized assistance?*";
                await turnContext.SendActivityAsync(MessageFactory.Text(enhancedResponse), cancellationToken);
                
                // Store enhanced response in database
                await StoreMessageInDatabase(turnContext, "assistant", enhancedResponse);
            }
            else
            {
                // Send normal response to user
                await turnContext.SendActivityAsync(MessageFactory.Text(response.Content), cancellationToken);
                
                // Store assistant response in database
                await StoreMessageInDatabase(turnContext, "assistant", response.Content ?? string.Empty);
            }

            // Save conversation state
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            return true;
        }

        /// <summary>
        /// Stores a conversation message in the database via the API
        /// </summary>
        /// <param name="turnContext">The turn context</param>
        /// <param name="role">The message role (user, assistant, system)</param>
        /// <param name="content">The message content</param>
        /// <param name="toolCallId">Optional tool call ID</param>
        /// <param name="toolCalls">Optional tool calls data</param>
        /// <param name="imageType">Optional image MIME type</param>
        /// <param name="imageData">Optional base64 image data</param>
        private async Task StoreMessageInDatabase(
            ITurnContext turnContext, 
            string role, 
            string content, 
            string? toolCallId = null, 
            string? toolCalls = null,
            string? imageType = null,
            string? imageData = null)
        {
            try
            {
                var message = new ConversationMessageCreateDto
                {
                    BotFrameworkConversationId = turnContext.Activity.Conversation.Id,
                    Role = role,
                    Content = content ?? string.Empty,
                    ToolCallId = toolCallId,
                    ToolCalls = toolCalls,
                    ImageType = imageType,
                    ImageData = imageData,
                    Timestamp = DateTime.UtcNow,
                    UserId = turnContext.Activity.From?.Id,
                    UserName = turnContext.Activity.From?.Name,
                    ChannelId = turnContext.Activity.ChannelId
                };

                var result = await _apiClient.AddConversationMessageAsync(message);
                
                if (!result.Succeeded)
                {
                    // Log error but don't stop the conversation
                    Console.WriteLine($"Failed to store message in database: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't stop the conversation
                Console.WriteLine($"Exception storing message in database: {ex.Message}");
            }
        }
    }
}