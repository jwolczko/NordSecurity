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
        Assert.DoesNotContain(rootCommand.Subcommands, command => command.Name == "config");
    }

    [Fact]
    public async Task CreateServersListCommandInvokesCountryHandler()
    {
        var serverService = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer("fr1", 20, "online") }
        };
        using var console = new ConsoleCapture();
        var rootCommand = CliCommands.Create(CreateServerListCommandHandler(serverService));

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
        var rootCommand = CliCommands.Create(CreateServerListCommandHandler(serverService));

        var result = await rootCommand.Parse(["servers", "list", "--protocol", "TCP"]).InvokeAsync();

        Assert.Equal(0, result);
        Assert.Equal(5, serverService.LastProtocolId);
    }

    [Fact]
    public async Task CreateServersListCommandCombinesCountryAndProtocolFilters()
    {
        var serverService = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer("fr-tcp1", 20, "online") }
        };
        using var console = new ConsoleCapture();
        var rootCommand = CliCommands.Create(CreateServerListCommandHandler(serverService));

        var result = await rootCommand.Parse(["servers", "list", "--country", "france", "--protocol", "TCP"]).InvokeAsync();

        Assert.Equal(0, result);
        Assert.Equal(74, serverService.LastCountryId);
        Assert.Equal(5, serverService.LastProtocolId);
    }

    private static RootCommand CreateRootCommand()
    {
        return CliCommands.Create(CreateServerListCommandHandler(new StubServerService()));
    }

    private static ServerListCommandHandler CreateServerListCommandHandler(StubServerService serverService)
    {
        return new ServerListCommandHandler(serverService, new ConsoleOutput(), new NordVpnServerFilterCatalog());
    }
}
