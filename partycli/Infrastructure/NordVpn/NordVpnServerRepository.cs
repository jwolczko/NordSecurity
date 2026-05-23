using Newtonsoft.Json;
using partycli.Application.Interfaces;
using partycli.Domain;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace partycli.Infrastructure.NordVpn;

public class NordVpnServerRepository : IServerRepository
{
    private readonly HttpClient httpClient;
    private readonly string serversEndpoint;

    public NordVpnServerRepository(HttpClient httpClient, string serversEndpoint)
    {
        this.httpClient = httpClient;
        this.serversEndpoint = serversEndpoint;
    }

    public async Task<IReadOnlyList<Server>> GetServersAsync(
        VpnServerQuery query,
        CancellationToken cancellationToken)
    {
        var requestUri = BuildRequestUri(query);
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonConvert.DeserializeObject<List<Server>>(responseString) ?? new List<Server>();
    }

    private string BuildRequestUri(VpnServerQuery query)
    {
        var filters = new List<string>();

        if (query.ProtocolId.HasValue)
        {
            filters.Add($"filters[servers_technologies][id]={query.ProtocolId.Value}");
        }

        if (query.CountryId.HasValue)
        {
            filters.Add($"filters[country_id]={query.CountryId.Value}");
        }

        if (filters.Count == 0)
        {
            return serversEndpoint;
        }

        return $"{serversEndpoint}?{string.Join("&", filters)}";
    }
}
