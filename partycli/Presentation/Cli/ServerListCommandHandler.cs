using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using partycli.Application.Interfaces;
using partycli.Domain;

namespace partycli.Presentation.Cli;

public class ServerListCommandHandler
{
    private readonly IServerService serverService;
    private readonly ConsoleOutput consoleOutput;
    private readonly IServerFilterCatalog filterCatalog;

    public ServerListCommandHandler(
        IServerService serverService,
        ConsoleOutput consoleOutput,
        IServerFilterCatalog filterCatalog)
    {
        this.serverService = serverService;
        this.consoleOutput = consoleOutput;
        this.filterCatalog = filterCatalog;
    }

    public Task<int> HandleLegacyAsync(
        bool local,
        bool france,
        bool tcp,
        CancellationToken cancellationToken)
    {
        var country = france ? "france" : null;
        var protocol = tcp ? "TCP" : null;

        return HandleAsync(local, country, protocol, cancellationToken);
    }

    public async Task<int> HandleAsync(
        bool local,
        string country,
        string protocol,
        CancellationToken cancellationToken)
    {
        try
        {
            if (local && HasRemoteFilter(country, protocol))
            {
                consoleOutput.WriteError("Use --local without country or protocol filters.");

                return 1;
            }

            if (!TryResolveCountryId(country, out var countryId) ||
                !TryResolveProtocolId(protocol, out var protocolId))
            {
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

        if (countryId.HasValue || protocolId.HasValue)
        {
            return serverService.FetchAsync(countryId, protocolId, cancellationToken);
        }

        return serverService.FetchAllAsync(cancellationToken);
    }

    private bool TryResolveCountryId(string country, out int? countryId)
    {
        countryId = null;

        if (string.IsNullOrWhiteSpace(country))
        {
            return true;
        }

        if (filterCatalog.TryGetCountryId(country, out var resolvedCountryId))
        {
            countryId = resolvedCountryId;

            return true;
        }

        consoleOutput.WriteError($"Unsupported country '{country}'. Supported countries: {filterCatalog.GetSupportedCountries()}.");

        return false;
    }

    private bool TryResolveProtocolId(string protocol, out int? protocolId)
    {
        protocolId = null;

        if (string.IsNullOrWhiteSpace(protocol))
        {
            return true;
        }

        if (filterCatalog.TryGetProtocolId(protocol, out var resolvedProtocolId))
        {
            protocolId = resolvedProtocolId;

            return true;
        }

        consoleOutput.WriteError($"Unsupported protocol '{protocol}'. Supported protocols: {filterCatalog.GetSupportedProtocols()}.");

        return false;
    }

    private static bool HasRemoteFilter(string country, string protocol)
    {
        return !string.IsNullOrWhiteSpace(country) ||
               !string.IsNullOrWhiteSpace(protocol);
    }
}
