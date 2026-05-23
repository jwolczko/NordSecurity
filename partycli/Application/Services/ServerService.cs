using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using partycli.Application.Interfaces;
using partycli.Domain;

namespace partycli.Application.Services;

public class ServerService : IServerService
{
    private const string ServerListSettingName = "serverlist";

    private readonly IServerRepository serverRepository;
    private readonly ISettingsStorage settingsStorage;
    private readonly IActionLogger logger;

    public ServerService(
        IServerRepository serverRepository,
        ISettingsStorage settingsStorage,
        IActionLogger logger)
    {
        this.serverRepository = serverRepository;
        this.settingsStorage = settingsStorage;
        this.logger = logger;
    }

    public async Task<IReadOnlyList<Server>> FetchAllAsync(CancellationToken cancellationToken)
    {
        var servers = await serverRepository.GetServersAsync(VpnServerQuery.All(), cancellationToken);
        SaveServers(servers, "Saved all servers");

        return servers;
    }

    public async Task<IReadOnlyList<Server>> FetchByCountryAsync(int countryId, CancellationToken cancellationToken)
    {
        var servers = await serverRepository.GetServersAsync(VpnServerQuery.ByCountry(countryId), cancellationToken);
        SaveServers(servers, $"Saved servers for country {countryId}");

        return servers;
    }

    public async Task<IReadOnlyList<Server>> FetchByProtocolAsync(int protocolId, CancellationToken cancellationToken)
    {
        var servers = await serverRepository.GetServersAsync(VpnServerQuery.ByProtocol(protocolId), cancellationToken);
        SaveServers(servers, $"Saved servers for protocol {protocolId}");

        return servers;
    }

    public IReadOnlyList<Server> GetLocal()
    {
        var serializedServers = settingsStorage.GetValue(ServerListSettingName);

        if (string.IsNullOrWhiteSpace(serializedServers))
        {
            return new List<Server>();
        }

        return JsonConvert.DeserializeObject<List<Server>>(serializedServers) ?? new List<Server>();
    }

    private void SaveServers(IReadOnlyList<Server> servers, string action)
    {
        settingsStorage.SetValue(ServerListSettingName, JsonConvert.SerializeObject(servers));
        logger.Log($"{action}. Total servers: {servers.Count}");
    }
}
