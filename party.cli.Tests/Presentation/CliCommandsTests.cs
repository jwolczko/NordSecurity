using party.cli.Tests.TestHelpers;
using partycli.Infrastructure.NordVpn;
using partycli.Presentation.Cli;
using System.CommandLine;

namespace party.cli.Tests.Presentation;

public class CliCommandsTests
{
    [Fact]
    public void CreateRegistersExpectedTopLevelCommands()
    {
        var rootCommand = CreateRootCommand();

        Assert.Contains(rootCommand.Subcommands, command => command.Name == "server_list");
        Assert.Contains(rootCommand.Subcommands, command => command.Name == "servers");
        Assert.Contains(rootCommand.Subcommands, command => command.Name == "config");
    }

    [Fact]
    public async Task CreateConfigCommandInvokesHandlerWithProvidedArguments()
    {
        var configService = new StubConfigService();
        using var console = new ConsoleCapture();
        var rootCommand = CliCommands.Create(
            CreateServerListCommandHandler(new StubServerService()),
            new ConfigCommandHandler(configService, new ConsoleOutput()));

        var result = await rootCommand.Parse(["config", "server-list", "value.json"]).InvokeAsync();

        Assert.Equal(0, result);
        Assert.Equal("server-list", configService.LastName);
        Assert.Equal("value.json", configService.LastValue);
    }

    [Fact]
    public async Task CreateServersListCommandInvokesCountryHandler()
    {
        var serverService = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer("fr1", 20, "online") }
        };
        using var console = new ConsoleCapture();
        var rootCommand = CliCommands.Create(
            CreateServerListCommandHandler(serverService),
            new ConfigCommandHandler(new StubConfigService(), new ConsoleOutput()));

        var result = await rootCommand.Parse(["servers", "list", "--country", "france"]).InvokeAsync();

        Assert.Equal(0, result);
        Assert.Equal(74, serverService.LastCountryId);
    }

    [Fact]
    public async Task CreateServersListCommandInvokesProtocolHandler()
    {
        var serverService = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer("tcp1", 20, "online") }
        };
        using var console = new ConsoleCapture();
        var rootCommand = CliCommands.Create(
            CreateServerListCommandHandler(serverService),
            new ConfigCommandHandler(new StubConfigService(), new ConsoleOutput()));

        var result = await rootCommand.Parse(["servers", "list", "--protocol", "TCP"]).InvokeAsync();

        Assert.Equal(0, result);
        Assert.Equal(5, serverService.LastProtocolId);
    }

    private static RootCommand CreateRootCommand()
    {
        return CliCommands.Create(
            CreateServerListCommandHandler(new StubServerService()),
            new ConfigCommandHandler(new StubConfigService(), new ConsoleOutput()));
    }

    private static ServerListCommandHandler CreateServerListCommandHandler(StubServerService serverService)
    {
        return new ServerListCommandHandler(serverService, new ConsoleOutput(), new NordVpnServerFilterCatalog());
    }
}
