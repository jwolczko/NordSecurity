using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using partycli.Application.Interfaces;
using partycli.Domain;

namespace partycli.Presentation.Cli;

public class ServerListCommandHandler
{
    private const int FranceCountryId = 74;
    private const int TcpProtocolId = 5;

    private readonly IServerService serverService;
    private readonly ConsoleOutput consoleOutput;

    public ServerListCommandHandler(IServerService serverService, ConsoleOutput consoleOutput)
    {
        this.serverService = serverService;
        this.consoleOutput = consoleOutput;
    }

    public Task<int> HandleLegacyAsync(
        bool local,
        bool france,
        bool tcp,
        CancellationToken cancellationToken)
    {
        int? countryId = france ? FranceCountryId : null;
        int? protocolId = tcp ? TcpProtocolId : null;

        return HandleAsync(local, countryId, protocolId, cancellationToken);
    }

    public async Task<int> HandleAsync(
        bool local,
        int? countryId,
        int? protocolId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!IsValidQuery(local, countryId, protocolId))
            {
                consoleOutput.WriteError("Use only one server source at a time.");

                return 1;
            }

            var servers = await GetServersAsync(local, countryId, protocolId, cancellationToken);

            if (local && servers.Count == 0)
            {
                consoleOutput.WriteError("There are no server data in local storage.");

                return 1;
            }

            consoleOutput.WriteServers(servers);

            return 0;
        }
        catch (Exception exception)
        {
            consoleOutput.WriteError(exception.Message);

            return 1;
        }
    }

    private Task<IReadOnlyList<Server>> GetServersAsync(
        bool local,
        int? countryId,
        int? protocolId,
        CancellationToken cancellationToken)
    {
        if (local)
        {
            return Task.FromResult(serverService.GetLocal());
        }

        if (countryId.HasValue)
        {
            return serverService.FetchByCountryAsync(countryId.Value, cancellationToken);
        }

        if (protocolId.HasValue)
        {
            return serverService.FetchByProtocolAsync(protocolId.Value, cancellationToken);
        }

        return serverService.FetchAllAsync(cancellationToken);
    }

    private static bool IsValidQuery(bool local, int? countryId, int? protocolId)
    {
        var selectedSources = 0;

        if (local)
        {
            selectedSources++;
        }

        if (countryId.HasValue)
        {
            selectedSources++;
        }

        if (protocolId.HasValue)
        {
            selectedSources++;
        }

        return selectedSources <= 1;
    }
}
