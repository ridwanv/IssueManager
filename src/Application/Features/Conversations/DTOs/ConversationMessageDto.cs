// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CleanArchitecture.Blazor.Application.Features.Conversations.DTOs;

public class ConversationMessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string BotFrameworkConversationId { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? ToolCallId { get; set; }
    public string? ToolCalls { get; set; }
    public string? ImageType { get; set; }
    public string? ImageData { get; set; }
    public string? Attachments { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ChannelId { get; set; }
    public bool IsEscalated { get; set; }
    public string TenantId { get; set; } = default!;
}

public class ConversationMessageCreateDto
{
    public string BotFrameworkConversationId { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? ToolCallId { get; set; }
    public string? ToolCalls { get; set; }
    public string? ImageType { get; set; }
    public string? ImageData { get; set; }
    public string? Attachments { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? ChannelId { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? ConversationChannelData { get; set; } // Full ConversationReference JSON for Bot Framework routing
    
    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ConversationMessage, ConversationMessageDto>().ReverseMap();
            CreateMap<ConversationMessageCreateDto, ConversationMessage>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ConversationId, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => src.Timestamp ?? DateTime.UtcNow))
                .ForMember(dest => dest.IsEscalated, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.Conversation, opt => opt.Ignore())
                .ForMember(dest => dest.AttachmentEntities, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModified, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DomainEvents, opt => opt.Ignore());
        }
    }
}
