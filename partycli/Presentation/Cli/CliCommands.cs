using System.CommandLine;

namespace partycli.Presentation.Cli;

public static class CliCommands
{
    public static RootCommand Create(
        ServerListCommandHandler serverListCommandHandler,
        ConfigCommandHandler configCommandHandler)
    {
        var rootCommand = new RootCommand("NordVPN server CLI");

        rootCommand.Subcommands.Add(CreateLegacyServerListCommand(serverListCommandHandler));
        rootCommand.Subcommands.Add(CreateServersCommand(serverListCommandHandler));
        rootCommand.Subcommands.Add(CreateConfigCommand(configCommandHandler));

        return rootCommand;
    }

    private static Command CreateLegacyServerListCommand(ServerListCommandHandler handler)
    {
        var command = new Command("server_list", "Fetch, save and display NordVPN servers");
        var localOption = new Option<bool>("--local") { Description = "Display servers saved locally" };
        var franceOption = new Option<bool>("--france") { Description = "Fetch servers from France" };
        var tcpOption = new Option<bool>("--TCP") { Description = "Fetch servers that support TCP" };

        command.Options.Add(localOption);
        command.Options.Add(franceOption);
        command.Options.Add(tcpOption);

        command.SetAction((parseResult, cancellationToken) =>
        {
            var local = parseResult.GetValue(localOption);
            var france = parseResult.GetValue(franceOption);
            var tcp = parseResult.GetValue(tcpOption);

            return handler.HandleLegacyAsync(local, france, tcp, cancellationToken);
        });

        return command;
    }

    private static Command CreateServersCommand(ServerListCommandHandler handler)
    {
        var serversCommand = new Command("servers", "Manage NordVPN servers");
        var listCommand = new Command("list", "Fetch, save and display NordVPN servers");
        var localOption = new Option<bool>("--local") { Description = "Display servers saved locally" };
        var countryIdOption = new Option<int?>("--country-id") { Description = "Fetch servers by NordVPN country id" };
        var protocolIdOption = new Option<int?>("--protocol-id") { Description = "Fetch servers by NordVPN protocol id" };

        listCommand.Options.Add(localOption);
        listCommand.Options.Add(countryIdOption);
        listCommand.Options.Add(protocolIdOption);

        listCommand.SetAction((parseResult, cancellationToken) =>
        {
            var local = parseResult.GetValue(localOption);
            var countryId = parseResult.GetValue(countryIdOption);
            var protocolId = parseResult.GetValue(protocolIdOption);

            return handler.HandleAsync(local, countryId, protocolId, cancellationToken);
        });

        serversCommand.Subcommands.Add(listCommand);

        return serversCommand;
    }

    private static Command CreateConfigCommand(ConfigCommandHandler handler)
    {
        var command = new Command("config", "Set application configuration values");
        var nameArgument = new Argument<string>("name")
        {
            Description = "Setting name, for example serverlist or log"
        };
        var valueArgument = new Argument<string>("value")
        {
            Description = "Setting value"
        };

        command.Arguments.Add(nameArgument);
        command.Arguments.Add(valueArgument);

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArgument);
            var value = parseResult.GetValue(valueArgument);

            return handler.Handle(name, value);
        });

        return command;
    }
}
