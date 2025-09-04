// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Common.ExceptionHandlers;
using CleanArchitecture.Blazor.Application.Common.PublishStrategies;
using CleanArchitecture.Blazor.Application.Pipeline;
using CleanArchitecture.Blazor.Application.Pipeline.PreProcessors;
using CleanArchitecture.Blazor.Application.Features.Issues.Commands.Create;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
            loggingBuilder.AddEventSourceLogger();
        });
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Register application services
        services.AddScoped<IIssueReferenceNumberService, IssueReferenceNumberService>();
      
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.NotificationPublisherType = typeof(ChannelBasedNoWaitPublisher);
            config.AddRequestPreProcessor(typeof(IRequestPreProcessor<>), typeof(ValidationPreProcessor<>));
            config.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            config.AddOpenBehavior(typeof(FusionCacheBehaviour<,>));
            config.AddOpenBehavior(typeof(CacheInvalidationBehaviour<,>));

        });
      

        return services;
    }
    
}