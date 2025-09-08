using CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationById;
using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Server.UI.Services.SignalR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Localization;
using MediatR;
using MudBlazor;

namespace CleanArchitecture.Blazor.Server.UI.Components.Issues;

public partial class IssueConversationTabComponent : ComponentBase, IAsyncDisposable
{
    [Parameter] public Guid IssueId { get; set; }
    [Parameter] public int? ConversationId { get; set; }

    [Inject] public SignalRConnectionService SignalRService { get; set; } = default!;
    [Inject] public IStringLocalizer<IssueConversationTabComponent> L { get; set; } = default!;

    private ConversationDetailsDto? _conversationDetails;
    private bool _loading = true;
    private bool _customerTyping = false;
    private bool _sendingMessage = false;
    private HubConnection? _hubConnection;

    protected override async Task OnParametersSetAsync()
    {
        if (ConversationId.HasValue && (_conversationDetails == null || _conversationDetails.Conversation.Id != ConversationId.Value))
        {
            await LoadConversation();
            await InitializeSignalR();
        }
        else if (!ConversationId.HasValue)
        {
            _conversationDetails = null;
            _loading = false;
        }
    }

    private async Task LoadConversation()
    {
        if (!ConversationId.HasValue) return;

        _loading = true;
        try
        {
            var result = await Mediator.Send(new GetConversationByIdQuery(ConversationId.Value.ToString()));
            
            if (result.Succeeded)
            {
                _conversationDetails = result.Data;
            }
            else
            {
                Snackbar.Add(result.Errors?.FirstOrDefault() ?? L["Failed to load conversation"], MudBlazor.Severity.Error);
                _conversationDetails = null;
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["Error loading conversation: {0}", ex.Message], MudBlazor.Severity.Error);
            _conversationDetails = null;
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    private async Task InitializeSignalR()
    {
        if (!ConversationId.HasValue) return;

        try
        {
            await SignalRService.EnsureConnectedAsync();
            _hubConnection = SignalRService.HubConnection;

            if (_hubConnection != null)
            {
                // Subscribe to real-time message updates
                _hubConnection.On<ConversationMessageDto>("NewMessageReceived", OnNewMessageReceived);
                
                // Subscribe to typing indicators
                _hubConnection.On<string, bool>("CustomerTyping", OnCustomerTyping);
                
                // Join conversation group for targeted updates
                await _hubConnection.InvokeAsync("JoinConversationGroup", ConversationId.ToString());
                
                // Join issue-conversation group for combined updates
                await _hubConnection.InvokeAsync("JoinIssueConversationGroup", IssueId, ConversationId.ToString());
            }
        }
        catch (Exception)
        {
            // Log error but don't fail the component loading
            // SignalR errors should not prevent conversation display
        }
    }

    private async Task HandleNewMessage(string message)
    {
        if (!ConversationId.HasValue || _conversationDetails == null) return;

        _sendingMessage = true;
        StateHasChanged();

        try
        {
            // The message sending will be handled by the MessageInputComponent
            // This is just a placeholder for any additional issue-specific logic
        }
        finally
        {
            _sendingMessage = false;
            StateHasChanged();
        }
    }

    private async Task OnNewMessageReceived(ConversationMessageDto message)
    {
        if (_conversationDetails != null && message.ConversationId == ConversationId)
        {
            // Add the new message to the collection
            _conversationDetails.Messages ??= new List<ConversationMessageDto>();
            _conversationDetails.Messages.Add(message);
            
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnCustomerTyping(string conversationId, bool isTyping)
    {
        if (conversationId == ConversationId?.ToString())
        {
            _customerTyping = isTyping;
            await InvokeAsync(StateHasChanged);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null && ConversationId.HasValue)
        {
            try
            {
                // Leave SignalR groups
                // Intentionally NOT leaving conversation group to allow global notifications
                // await _hubConnection.InvokeAsync("LeaveConversationGroup", ConversationId.ToString());
                await _hubConnection.InvokeAsync("LeaveIssueConversationGroup", IssueId, ConversationId.ToString());
                
                // Unsubscribe from events
                _hubConnection.Remove("NewMessageReceived");
                _hubConnection.Remove("CustomerTyping");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}