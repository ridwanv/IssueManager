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

            // Check for agent session timeouts and handback grace periods
            CheckAgentSessionTimeout(conversationData);

            // Check if this is an agent message FIRST (before checking ShouldBotRespond)
            if (IsAgentMessage(turnContext))
            {
                // Start agent session if not already active
                if (!conversationData.IsAgentActivelyHandling)
                {
                    var agentId = turnContext.Activity.From?.Id ?? "unknown-agent";
                    var agentName = turnContext.Activity.From?.Name ?? "Agent";
                    
                    // Remove "agent:" prefix if present
                    if (agentId.StartsWith("agent:"))
                    {
                        agentId = agentId.Substring(6);
                    }
                    
                    conversationData.StartAgentSession(agentId, agentName);
                    Console.WriteLine($"Started agent session for {agentName} (ID: {agentId})");
                }
                else
                {
                    conversationData.UpdateAgentActivity();
                    Console.WriteLine($"Updated agent activity for existing session");
                }
                
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                
                // Handle as agent message activity
                return await HandleAgentMessageActivity(turnContext, conversationData, cancellationToken);
            }

            // Early exit check: Don't process if agent is actively handling
            if (!conversationData.ShouldBotRespond())
            {
                // Route user message to the active agent
                return await RouteUserMessageToAgent(turnContext, conversationData, cancellationToken);
            }

            // Check if this is an agent message activity (Bot as Proxy pattern)
            if (turnContext.Activity.Type == "agentMessage")
            {
                return await HandleAgentMessageActivity(turnContext, conversationData, cancellationToken);
            }

            // Handle handoff status activities
            if (turnContext.Activity.Type == "handoffStatus")
            {
                return await HandleHandoffStatusActivity(turnContext, conversationData, cancellationToken);
            }

            // Handle agent presence activities
            if (turnContext.Activity.Type == "agentPresence")
            {
                return await HandleAgentPresenceActivity(turnContext, conversationData, cancellationToken);
            }
                
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
                
                // Prepare for agent session (will be started when agent connects)
                conversationData.IsHandoffPending = true;
                conversationData.HandoffInitiatedAt = DateTime.UtcNow;
                
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
        /// Handles agent message activities for Bot as Proxy pattern
        /// </summary>
        private async Task<bool> HandleAgentMessageActivity(ITurnContext<IMessageActivity> turnContext, ConversationData conversationData, CancellationToken cancellationToken)
        {
            try
            {
                // Get agent information from activity From field and Text content
                var agentName = turnContext.Activity.From?.Name ?? "Agent";
                var agentId = turnContext.Activity.From?.Id ?? "unknown-agent";
                var agentMessage = turnContext.Activity.Text;

                // Remove "agent:" prefix if present in agent ID
                if (agentId.StartsWith("agent:"))
                {
                    agentId = agentId.Substring(6);
                }

                if (string.IsNullOrEmpty(agentMessage))
                {
                    Console.WriteLine("Agent message activity missing text content");
                    return false;
                }

                Console.WriteLine($"Processing agent message from {agentName} (ID: {agentId}): {agentMessage}");

                // Bot acts as proxy - format and route agent message to customer
                var formattedMessage = $"**{agentName}**: {agentMessage}";
                
                try
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(formattedMessage), cancellationToken);
                    Console.WriteLine($"Agent message sent successfully via Bot Framework");
                }
                catch (Exception sendEx)
                {
                    Console.WriteLine($"Failed to send agent message via Bot Framework: {sendEx.Message}");
                    // Continue with storing the message even if sending fails
                }

                // Store agent message in database
                await StoreMessageInDatabase(turnContext, "agent", agentMessage, agentName: agentName);

                // Add to conversation history for context preservation
                conversationData.AddTurn("assistant", formattedMessage);

                // Update conversation state to track agent handoff
                conversationData.IsEscalated = true;
                conversationData.AgentName = agentName;

                // Save conversation state
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

                Console.WriteLine($"Agent message processed successfully from {agentName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling agent message activity: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Handles handoff status activities
        /// </summary>
        private async Task<bool> HandleHandoffStatusActivity(ITurnContext<IMessageActivity> turnContext, ConversationData conversationData, CancellationToken cancellationToken)
        {
            try
            {
                var activityValue = turnContext.Activity.Value;
                var statusMessage = turnContext.Activity.Text ?? "Handoff status updated";
                var agentName = turnContext.Activity.From?.Name ?? "Agent";
                var agentId = turnContext.Activity.From?.Id ?? "unknown-agent";

                // Update conversation handoff state
                if (activityValue != null && activityValue.ToString()?.Contains("connected") == true)
                {
                    conversationData.IsEscalated = true;
                    conversationData.AgentName = agentName;
                    
                    // Start agent session
                    conversationData.StartAgentSession(agentId, agentName);
                    Console.WriteLine($"Agent {agentName} connected - started session");
                }
                else if (activityValue != null && activityValue.ToString()?.Contains("completed") == true)
                {
                    conversationData.IsEscalated = false;
                    conversationData.AgentName = null;
                    
                    // End agent session
                    conversationData.EndAgentSession("Handoff completed");
                    Console.WriteLine($"Agent {agentName} disconnected - ended session");
                }
                else if (activityValue != null && activityValue.ToString()?.Contains("handback") == true)
                {
                    // Agent requesting handback to bot
                    conversationData.RequestHandback("Agent requested handback");
                    Console.WriteLine($"Agent {agentName} requested handback");
                }

                // Store handoff status message
                await StoreMessageInDatabase(turnContext, "system", statusMessage);

                // Save conversation state
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

                Console.WriteLine($"Handoff status activity processed: {statusMessage}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling handoff status activity: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Handles agent presence activities (typing indicators)
        /// </summary>
        private async Task<bool> HandleAgentPresenceActivity(ITurnContext<IMessageActivity> turnContext, ConversationData conversationData, CancellationToken cancellationToken)
        {
            try
            {
                var agentName = turnContext.Activity.From?.Name ?? "Agent";
                var isTyping = turnContext.Activity.Value?.ToString()?.Contains("IsTyping\":true") == true;

                // Update agent activity if we have an active session
                if (conversationData.IsAgentActivelyHandling)
                {
                    conversationData.UpdateAgentActivity();
                }

                // Send typing indicator to customer if agent is typing
                if (isTyping)
                {
                    await turnContext.SendActivitiesAsync(new[] { 
                        new Activity { Type = ActivityTypes.Typing } 
                    }, cancellationToken);
                }

                // Save conversation state to persist activity update
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

                Console.WriteLine($"Agent presence activity processed: {agentName} {(isTyping ? "started" : "stopped")} typing");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling agent presence activity: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Enhanced StoreMessageInDatabase method to support agent name parameter
        /// </summary>
        private async Task StoreMessageInDatabase(
            ITurnContext turnContext, 
            string role, 
            string content, 
            string? toolCallId = null, 
            string? toolCalls = null,
            string? imageType = null,
            string? imageData = null,
            string? agentName = null)
        {
            try
            {
                // Capture full ConversationReference for Bot Framework routing
                string? conversationChannelData = null;
                try
                {
                    var conversationReference = turnContext.Activity.GetConversationReference();
                    conversationChannelData = JsonSerializer.Serialize(conversationReference);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the entire operation
                    Console.WriteLine($"Failed to serialize ConversationReference: {ex.Message}");
                }

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
                    UserName = agentName ?? turnContext.Activity.From?.Name,
                    ChannelId = turnContext.Activity.ChannelId,
                    ConversationChannelData = conversationChannelData // Include for conversation creation/update
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

        /// <summary>
        /// Helper method to determine if a message is from an agent
        /// </summary>
        private bool IsAgentMessage(ITurnContext turnContext)
        {
            // Check if activity type indicates agent message
            if (turnContext.Activity.Type == "agentMessage" || 
                turnContext.Activity.Type == "agentPresence")
            {
                return true;
            }

            // Check if the From field indicates this is an agent message (prefixed with "agent:")
            if (!string.IsNullOrEmpty(turnContext.Activity.From?.Id))
            {
                var fromId = turnContext.Activity.From.Id;
                
                if (fromId.StartsWith("agent:"))
                {
                    return true;
                }
            }

            // Check if message has agent identifier in the activity properties
            if (turnContext.Activity.Properties?.ContainsKey("isAgentMessage") == true)
            {
                var agentMessageValue = turnContext.Activity.Properties["isAgentMessage"];
                return agentMessageValue != null && agentMessageValue.ToString() == "true";
            }

            return false;
        }

        /// <summary>
        /// Check and handle agent session timeouts
        /// </summary>
        private void CheckAgentSessionTimeout(ConversationData conversationData)
        {
            if (conversationData.IsAgentSessionTimedOut())
            {
                Console.WriteLine($"Agent session timed out for {conversationData.AgentName}");
                conversationData.EndAgentSession("Agent session timeout");
            }
            
            // Also check handback grace period
            if (conversationData.IsInHandbackGracePeriod())
            {
                // Grace period is still active, keep suppressing bot responses
                Console.WriteLine($"Still in handback grace period for {conversationData.AgentName}");
            }
            else if (conversationData.AgentRequestedHandback)
            {
                // Grace period expired, complete the handback
                Console.WriteLine($"Handback grace period expired, completing handback for {conversationData.AgentName}");
                conversationData.EndAgentSession("Handback grace period completed");
            }
        }

        /// <summary>
        /// Routes user messages to the active agent during agent handoff
        /// </summary>
        private async Task<bool> RouteUserMessageToAgent(ITurnContext<IMessageActivity> turnContext, ConversationData conversationData, CancellationToken cancellationToken)
        {
            try
            {
                var userMessage = turnContext.Activity.Text;
                var userId = turnContext.Activity.From?.Id;
                var userName = turnContext.Activity.From?.Name;

                if (string.IsNullOrEmpty(userMessage))
                {
                    Console.WriteLine("User message is empty, not routing to agent");
                    return true;
                }

                Console.WriteLine($"Routing user message to agent {conversationData.AgentName}: {userMessage}");

                // Store user message in database with proper context
                await StoreMessageInDatabase(turnContext, "user", userMessage);

                // Update agent activity to extend session timeout
                conversationData.UpdateAgentActivity();

                // Here you would implement the actual routing to the agent
                // This could be via:
                // 1. Webhook to agent dashboard
                // 2. SignalR notification
                // 3. Database queue that agents poll
                // 4. Direct API call to agent service
                
                // For now, we'll use a placeholder approach
                await NotifyAgentOfUserMessage(turnContext, conversationData, userMessage, cancellationToken);

                // Save conversation state
                await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

                Console.WriteLine($"User message successfully routed to agent {conversationData.AgentName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error routing user message to agent: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Notifies the agent of a new user message via the appropriate channel
        /// </summary>
        private async Task NotifyAgentOfUserMessage(ITurnContext<IMessageActivity> turnContext, ConversationData conversationData, string userMessage, CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"Notifying agent {conversationData.CurrentAgentId} of user message via API");

                var result = await _apiClient.NotifyAgentOfUserMessageAsync(
                    turnContext.Activity.Conversation.Id,
                    conversationData.CurrentAgentId!,
                    userMessage,
                    turnContext.Activity.From?.Id,
                    turnContext.Activity.From?.Name,
                    turnContext.Activity.ChannelId,
                    NotificationUrgency.Normal);

                if (result.Succeeded)
                {
                    Console.WriteLine($"Agent {conversationData.CurrentAgentId} successfully notified of user message");
                }
                else
                {
                    Console.WriteLine($"Failed to notify agent {conversationData.CurrentAgentId}: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying agent {conversationData.CurrentAgentId} of user message: {ex.Message}");
            }
        }
    }
}