using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using partycli.Application.Interfaces;
using partycli.Domain;

namespace partycli.Application.Services;

public class ServerService : IServerService
{
    private readonly IServerRepository serverRepository;
    private readonly IStateStorage stateStorage;
    private readonly IActionLogger logger;

    public ServerService(
        IServerRepository serverRepository,
        IStateStorage stateStorage,
        IActionLogger logger)
    {
        this.serverRepository = serverRepository;
        this.stateStorage = stateStorage;
        this.logger = logger;
    }

    public async Task<IReadOnlyList<Server>> FetchAllAsync(CancellationToken cancellationToken)
    {
        var servers = await serverRepository.GetServersAsync(VpnServerQuery.All(), cancellationToken);
        SaveServers(servers, "Saved all servers");

        return servers;
    }

    public async Task<IReadOnlyList<Server>> FetchAsync(
        int? countryId,
        int? protocolId,
        CancellationToken cancellationToken)
    {
        var servers = await serverRepository.GetServersAsync(
            VpnServerQuery.WithFilters(countryId, protocolId),
            cancellationToken);
        SaveServers(servers, BuildFilteredAction(countryId, protocolId));

        return servers;
    }

    public IReadOnlyList<Server> GetLocal()
    {
        return stateStorage.GetServers();
    }

    private void SaveServers(IReadOnlyList<Server> servers, string action)
    {
        stateStorage.SaveServers(servers);
        logger.Log($"{action}. Total servers: {servers.Count}");
    }

    private static string BuildFilteredAction(int? countryId, int? protocolId)
    {
        if (countryId.HasValue && protocolId.HasValue)
        {
            return $"Saved servers for country {countryId.Value} and protocol {protocolId.Value}";
        }

        if (countryId.HasValue)
        {
            return $"Saved servers for country {countryId.Value}";
        }

        if (protocolId.HasValue)
        {
            return $"Saved servers for protocol {protocolId.Value}";
        }

        return "Saved all servers";
    }
}
