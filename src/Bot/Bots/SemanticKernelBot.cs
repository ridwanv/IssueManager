// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Plugins;
using IssueManager.Bot.Services;

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

        public SemanticKernelBot(IConfiguration config, ConversationState conversationState, UserState userState, AzureOpenAIClient aoaiClient, T dialog, IssueManagerApiClient apiClient): base(config, conversationState, userState, dialog)
        {
            _instructions = config["LLM_INSTRUCTIONS"];
            _welcomeMessage = config.GetValue("LLM_WELCOME_MESSAGE", "Hello and welcome to the Semantic Kernel Bot Dotnet!");
            _endpoint = config["AZURE_OPENAI_API_ENDPOINT"];
            _deployment = config["AZURE_OPENAI_DEPLOYMENT_NAME"];
            _apiKey = config["AZURE_OPENAI_API_KEY"];
            _credential = new DefaultAzureCredential();
            _apiClient = apiClient;
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

            // Enforce login
            var loggedIn = await HandleLogin(turnContext, cancellationToken);
            if (!loggedIn)
            {
                return false;
            }

            // Add user message to history
            conversationData.AddTurn("user", turnContext.Activity.Text);

            var kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(_deployment, _endpoint, _apiKey)
                .Build();

            // Add custom plugins with API client instead of MediatR
            kernel.Plugins.Add(kernel.CreatePluginFromObject(new IssueIntakePlugin(conversationData, turnContext, _apiClient)));

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

            // Save conversation state
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);

            // Send response to user
            await turnContext.SendActivityAsync(MessageFactory.Text(response.Content), cancellationToken);

            return true;
        }
    }
}