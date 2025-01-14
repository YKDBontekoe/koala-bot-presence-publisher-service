﻿using Koala.ActivityGameHandlerService.Options;
using Koala.ActivityGameHandlerService.Repositories;
using Koala.ActivityGameHandlerService.Repositories.Interfaces;
using Koala.ActivityGameHandlerService.Services;
using Koala.ActivityGameHandlerService.Services.Interfaces;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Koala.ActivityGameHandlerService;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var host = Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) =>
                {
                    var env = context.HostingEnvironment;

                    builder
                        .SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.Development.json", true, true)
                        .AddEnvironmentVariables();
                }
            )
            .ConfigureServices(ConfigureDelegate)
            .UseConsoleLifetime()
            .Build();

        await host.RunAsync();
    }

    private static void ConfigureDelegate(HostBuilderContext hostContext, IServiceCollection services)
    {
        ConfigureOptions(services, hostContext.Configuration);
        ConfigureServiceBus(services);
        ConfigureCache(services);
        ConfigureRepositories(services);
        ConfigureServices(services);
        ConfigureHostedServices(services);
    }

    // Configure options for the application to use in the services
    private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<ServiceBusOptions>(configuration.GetSection(ServiceBusOptions.ServiceBus));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.Redis));
        services.Configure<SteamOptions>(configuration.GetSection(SteamOptions.Steam));
    }

    // Configure the Azure Service Bus client with the connection string
    private static void ConfigureServiceBus(IServiceCollection services)
    {
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(services.BuildServiceProvider().GetRequiredService<IOptions<ServiceBusOptions>>().Value.ConnectionString);
        });
    }
    
    // Configure Cache
    private static void ConfigureCache(IServiceCollection services)
    {
        services.AddDistributedMemoryCache();
    }
    
    // Configure repositories
    private static void ConfigureRepositories(IServiceCollection services)
    {
        services.AddTransient<ICacheRepository, CacheRepository>();
        services.AddTransient<ISteamRepository, SteamRepository>();
        services.AddTransient<IScraperRepository, SteamScraperRepository>();
    }
    
    // Configure services
    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<ISteamService, SteamService>();
        services.AddTransient<IMessageService, AzureMessageService>();
    }
    
    // Configure the hosted services
    private static void ConfigureHostedServices(IServiceCollection services)
    {
        services.AddHostedService<SteamGameCacheUpdateWorker>();
        services.AddHostedService<ActivityGameHandlerWorker>();
    }
}