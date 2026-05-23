using System.CommandLine;

namespace partycli.Presentation.Cli;

public static class CliCommands
{
    public static RootCommand Create(ServerListCommandHandler serverListCommandHandler)
    {
        var rootCommand = new RootCommand("NordVPN server CLI");

        rootCommand.Subcommands.Add(CreateLegacyServerListCommand(serverListCommandHandler));
        rootCommand.Subcommands.Add(CreateServersCommand(serverListCommandHandler));

        return rootCommand;
    }

    private static Command CreateLegacyServerListCommand(ServerListCommandHandler handler)
    {
        var command = new Command("server_list", "Legacy command for fetching, saving and displaying NordVPN servers");
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
        var countryOption = new Option<string>("--country") { Description = "Fetch servers by country name, for example france" };
        var protocolOption = new Option<string>("--protocol") { Description = "Fetch servers by protocol name, for example TCP" };

        listCommand.Options.Add(localOption);
        listCommand.Options.Add(countryOption);
        listCommand.Options.Add(protocolOption);

        listCommand.SetAction((parseResult, cancellationToken) =>
        {
            var local = parseResult.GetValue(localOption);
            var country = parseResult.GetValue(countryOption);
            var protocol = parseResult.GetValue(protocolOption);

            return handler.HandleAsync(local, country, protocol, cancellationToken);
        });

        serversCommand.Subcommands.Add(listCommand);

        return serversCommand;
    }

}
