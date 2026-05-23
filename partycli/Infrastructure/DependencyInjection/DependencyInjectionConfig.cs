using System.CommandLine;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using partycli.Application.Interfaces;
using partycli.Application.Services;
using partycli.Infrastructure.Configuration;
using partycli.Infrastructure.NordVpn;
using partycli.Infrastructure.Storage;
using partycli.Presentation.Cli;

namespace partycli.Infrastructure.DependencyInjection;

public static class DependencyInjectionConfig
{
    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        RegisterConfiguration(services);
        RegisterInfrastructure(services);
        RegisterApplication(services);
        RegisterPresentation(services);

        return services.BuildServiceProvider();
    }

    private static void RegisterConfiguration(IServiceCollection services)
    {
        services.AddSingleton(AppConfiguration.Load());
    }

    private static void RegisterInfrastructure(IServiceCollection services)
    {
        services.AddSingleton<HttpClient>();
        services.AddSingleton<ISettingsStorage, SettingsStorage>();
        services.AddSingleton<IServerFilterCatalog, NordVpnServerFilterCatalog>();
        services.AddSingleton<IServerRepository>(serviceProvider =>
        {
            var httpClient = serviceProvider.GetRequiredService<HttpClient>();
            var configuration = serviceProvider.GetRequiredService<AppConfiguration>();

            return new NordVpnServerRepository(
                httpClient,
                configuration.NordVpn.ServersEndpoint);
        });
    }

    private static void RegisterApplication(IServiceCollection services)
    {
        services.AddSingleton<IActionLogger, LoggerService>();
        services.AddSingleton<IServerService, ServerService>();
        services.AddSingleton<IConfigService, ConfigService>();
    }

    private static void RegisterPresentation(IServiceCollection services)
    {
        services.AddSingleton<ConsoleOutput>();
        services.AddSingleton<ServerListCommandHandler>();
        services.AddSingleton<ConfigCommandHandler>();
        services.AddSingleton<RootCommand>(serviceProvider =>
        {
            var serverListCommandHandler = serviceProvider.GetRequiredService<ServerListCommandHandler>();
            var configCommandHandler = serviceProvider.GetRequiredService<ConfigCommandHandler>();

            return CliCommands.Create(serverListCommandHandler, configCommandHandler);
        });
    }
}
