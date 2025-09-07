using CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;
using CleanArchitecture.Blazor.Application.Features.Conversations.Queries.GetConversationInsights;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MediatR;
using MudBlazor;

namespace CleanArchitecture.Blazor.Server.UI.Components.Conversations;

public partial class ConversationInsightsComponent : ComponentBase
{
    [Parameter] public int ConversationId { get; set; }

    [Inject] public IStringLocalizer<ConversationInsightsComponent> L { get; set; } = default!;

    private ConversationInsightDto? _insights;
    private bool _loading = true;

    protected override async Task OnParametersSetAsync()
    {
        if (ConversationId > 0)
        {
            await LoadInsights();
        }
    }

    private async Task LoadInsights()
    {
        _loading = true;
        
        try
        {
            var query = new GetConversationInsightsByIdQuery(ConversationId);
            var result = await Mediator.Send(query);

            if (result.Succeeded)
            {
                _insights = result.Data;
            }
            else
            {
                Snackbar.Add(L["Failed to load conversation insights"], MudBlazor.Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add(L["Error loading insights: {0}", ex.Message], MudBlazor.Severity.Error);
        }
        finally
        {
            _loading = false;
        }
    }

    private Color GetSentimentColor(string sentimentLabel)
    {
        return sentimentLabel.ToLower() switch
        {
            "positive" => Color.Success,
            "negative" => Color.Error,
            "neutral" => Color.Info,
            _ => Color.Default
        };
    }

    private string GetSentimentIcon(string sentimentLabel)
    {
        return sentimentLabel.ToLower() switch
        {
            "positive" => Icons.Material.Filled.SentimentVerySatisfied,
            "negative" => Icons.Material.Filled.SentimentVeryDissatisfied,
            "neutral" => Icons.Material.Filled.SentimentNeutral,
            _ => Icons.Material.Filled.Help
        };
    }

    private double GetSentimentProgressValue(decimal sentimentScore)
    {
        // Convert from -1.0 to 1.0 scale to 0 to 100 scale
        return (double)((sentimentScore + 1) * 50);
    }
}