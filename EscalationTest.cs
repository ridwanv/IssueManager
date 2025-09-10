using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Blazor.Application.Features.Conversations.Commands.EscalateConversation;
using CleanArchitecture.Blazor.Infrastructure.Persistence;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using MediatR;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing escalation functionality...");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("src/Server.UI/appsettings.json", optional: false)
            .Build();

        var services = new ServiceCollection();
        
        // Add basic services
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=src/Server.UI/BlazorDashboardDb.db"));
        
        services.AddMediatR(typeof(EscalateConversationCommand).Assembly);
        
        var serviceProvider = services.BuildServiceProvider();
        
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        // Create a test conversation
        Console.WriteLine("Creating test conversation...");
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Subject = "Test Escalation Conversation",
            Status = ConversationStatus.Open,
            Priority = ConversationPriority.Medium,
            CreatedBy = "test-user",
            Created = DateTime.UtcNow,
            TenantId = "default-tenant"
        };
        
        dbContext.Conversations.Add(conversation);
        await dbContext.SaveChangesAsync();
        
        Console.WriteLine($"Created conversation with ID: {conversation.Id}");
        
        // Test escalation
        Console.WriteLine("Testing escalation command...");
        var command = new EscalateConversationCommand
        {
            ConversationId = conversation.Id,
            EscalationReason = "Test escalation for debugging",
            Priority = ConversationPriority.High
        };
        
        try
        {
            var result = await mediator.Send(command);
            Console.WriteLine($"Escalation result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Escalation failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("Test completed.");
    }
}
