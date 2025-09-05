// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Builder;
using System.Text.Json;
using System.Text.RegularExpressions;
using IssueManager.Bot.Services;

namespace IssueManager.Bot.Plugins;

public class HumanEscalationPlugin
{
    private readonly ConversationData _conversationData;
    private readonly ITurnContext _turnContext;
    private readonly IssueManagerApiClient _apiClient;
    
    public HumanEscalationPlugin(ConversationData conversationData, ITurnContext turnContext, IssueManagerApiClient apiClient)
    {
        _conversationData = conversationData;
        _turnContext = turnContext;
        _apiClient = apiClient;
    }

    [KernelFunction]
    [Description("Escalates the conversation to a human agent when the bot cannot help or user requests human support")]
    public async Task<string> EscalateToHuman(
        [Description("The reason for escalation")] string reason,
        [Description("Brief summary of what the user needs help with")] string userNeedsSummary = "")
    {
        try
        {
            // Check if already escalated
            if (_conversationData.IsEscalated)
            {
                return "This conversation has already been escalated to a human agent. Please wait while we connect you.";
            }

            // Initiate escalation in local state first
            _conversationData.InitiateEscalation($"{reason}. User needs: {userNeedsSummary}");
            
            // Get conversation transcript for agent context
            var transcript = _conversationData.GetConversationTranscript();
            
            // Get WhatsApp phone number from the conversation
            var whatsAppPhone = _turnContext.Activity.From?.Id;
            
            // Call the API to persist the escalation to the database
            var escalationResult = await _apiClient.EscalateConversationAsync(
                _turnContext.Activity.Conversation.Id,
                $"{reason}. User needs: {userNeedsSummary}",
                transcript,
                whatsAppPhone
            );
            
            if (escalationResult.Succeeded)
            {
                // Store the database conversation ID for future reference
                _conversationData.ConversationEntityId = escalationResult.Data;
                
                return $"I understand you need human assistance. Let me connect you with one of our support agents right away.\n\n" +
                       $"✅ Escalation initiated\n" +
                       $"🔄 Finding available agent...\n" +
                       $"📋 Your conversation history has been shared for context\n\n" +
                       $"An agent will join this conversation shortly to help you with: {userNeedsSummary}";
            }
            else
            {
                // API call failed, but keep local escalation state
                return $"I understand you need human assistance. Your request has been noted and we're working to connect you with an agent.\n\n" +
                       $"⚠️ There was a technical issue, but don't worry - we'll make sure you get help.\n" +
                       $"📱 Please stay in this chat and an agent will assist you soon.\n\n" +
                       $"Issue: {userNeedsSummary}";
            }
        }
        catch (Exception)
        {
            // Log the error but don't expose technical details to user
            return $"I understand you need human assistance. Your escalation request has been registered.\n\n" +
                   $"📱 Please stay in this chat and an agent will connect with you shortly.\n" +
                   $"🔄 If no agent responds within a few minutes, please try saying 'human help' again.\n\n" +
                   $"Issue: {userNeedsSummary}";
        }
    }

    [KernelFunction]
    [Description("Detects if the user is requesting human assistance or expressing frustration")]
    public bool DetectEscalationIntent(
        [Description("The user's message to analyze")] string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return false;

        var message = userMessage.ToLowerInvariant();
        
        // Direct human requests
        var humanRequestPatterns = new[]
        {
            @"\b(speak|talk|chat)\s+(?:to|with)\s+(?:a\s+)?(?:human|person|agent|representative|someone|operator)\b",
            @"\b(?:human|person|agent|representative|operator|someone)\s+(?:please|help|now)\b",
            @"\bget\s+(?:me\s+)?(?:a\s+)?(?:human|person|agent|representative)\b",
            @"\b(?:transfer|connect|escalate)\s+(?:me\s+)?(?:to\s+)?(?:a\s+)?(?:human|person|agent)\b",
            @"\bi\s+(?:need|want|require)\s+(?:a\s+)?(?:human|person|agent|representative)\b",
            @"\b(?:customer\s+)?(?:service|support)\s+(?:agent|representative|person|human)\b"
        };

        // Frustration indicators
        var frustrationPatterns = new[]
        {
            @"\b(?:this\s+)?(?:is\s+)?(?:not\s+)?(?:working|helpful|useful|good)\b",
            @"\b(?:frustrated|annoyed|angry|upset|mad)\b",
            @"\b(?:i\s+)?(?:can't|cannot|unable)\s+(?:do|understand|figure)\b",
            @"\b(?:this\s+)?(?:doesn't|does\s+not|won't|will\s+not)\s+(?:work|help|make\s+sense)\b",
            @"\b(?:waste\s+of\s+time|useless|stupid|dumb)\b",
            @"\b(?:give\s+up|had\s+enough|done\s+with\s+this)\b"
        };

        // Bot limitation acknowledgment
        var botLimitationPatterns = new[]
        {
            @"\byou\s+(?:can't|cannot|don't|do\s+not)\s+(?:help|understand|do|handle)\b",
            @"\b(?:you're|you\s+are)\s+(?:not\s+)?(?:helping|useful|understanding)\b",
            @"\b(?:i\s+)?(?:need|want)\s+(?:real\s+)?(?:help|assistance|support)\b"
        };

        // Check all patterns
        var allPatterns = humanRequestPatterns.Concat(frustrationPatterns).Concat(botLimitationPatterns);
        
        foreach (var pattern in allPatterns)
        {
            if (Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
        }

        // Check for excessive questions/confusion
        var questionCount = Regex.Matches(message, @"\?").Count;
        if (questionCount >= 3)
        {
            return true;
        }

        return false;
    }

    [KernelFunction]
    [Description("Checks conversation confidence and escalation triggers")]
    public string CheckEscalationTriggers()
    {
        var triggers = new List<string>();
        
        // Check conversation length - long conversations may need human help
        if (_conversationData.History.Count > 20)
        {
            triggers.Add("Long conversation detected");
        }
        
        // Check for repeated similar user messages (user stuck in loop)
        var userMessages = _conversationData.History
            .Where(h => h.Role == "user" && !string.IsNullOrEmpty(h.Message))
            .TakeLast(6)
            .Select(h => h.Message.ToLowerInvariant())
            .ToList();
            
        if (userMessages.Count >= 3)
        {
            var similarMessages = 0;
            for (int i = 1; i < userMessages.Count; i++)
            {
                var similarity = CalculateStringSimilarity(userMessages[i], userMessages[i-1]);
                if (similarity > 0.7) // 70% similarity threshold
                {
                    similarMessages++;
                }
            }
            
            if (similarMessages >= 2)
            {
                triggers.Add("User appears stuck in conversation loop");
            }
        }
        
        // Check if conversation has been going on too long without resolution
        var conversationDuration = _conversationData.GetIssueIntakeDuration();
        if (_conversationData.IsIssueIntakeActive && conversationDuration.TotalMinutes > 15)
        {
            triggers.Add("Issue intake taking longer than expected");
        }
        
        return triggers.Any() ? $"Escalation recommended: {string.Join(", ", triggers)}" : "";
    }
    
    private double CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0;
            
        var longer = str1.Length > str2.Length ? str1 : str2;
        var shorter = str1.Length > str2.Length ? str2 : str1;
        
        if (longer.Length == 0)
            return 1.0;
            
        var editDistance = CalculateLevenshteinDistance(longer, shorter);
        return (longer.Length - editDistance) / (double)longer.Length;
    }
    
    private int CalculateLevenshteinDistance(string str1, string str2)
    {
        var matrix = new int[str1.Length + 1, str2.Length + 1];
        
        for (int i = 0; i <= str1.Length; i++)
            matrix[i, 0] = i;
            
        for (int j = 0; j <= str2.Length; j++)
            matrix[0, j] = j;
            
        for (int i = 1; i <= str1.Length; i++)
        {
            for (int j = 1; j <= str2.Length; j++)
            {
                var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }
        
        return matrix[str1.Length, str2.Length];
    }
}