using party.cli.Tests.TestHelpers;
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
            new ServerListCommandHandler(new StubServerService(), new ConsoleOutput()),
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
            new ServerListCommandHandler(serverService, new ConsoleOutput()),
            new ConfigCommandHandler(new StubConfigService(), new ConsoleOutput()));

        var result = await rootCommand.Parse(["servers", "list", "--country-id", "74"]).InvokeAsync();

        Assert.Equal(0, result);
        Assert.Equal(74, serverService.LastCountryId);
    }

    private static RootCommand CreateRootCommand()
    {
        return CliCommands.Create(
            new ServerListCommandHandler(new StubServerService(), new ConsoleOutput()),
            new ConfigCommandHandler(new StubConfigService(), new ConsoleOutput()));
    }
}
