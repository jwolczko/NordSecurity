using System.Threading;
using System.Threading.Tasks;
using party.cli.Tests.TestHelpers;
using partycli.Domain;
using partycli.Infrastructure.NordVpn;

namespace party.cli.Tests.Infrastructure;

public class NordVpnServerRepositoryTests
{
    [Fact]
    public async Task GetServersAsyncWithoutFiltersUsesBaseEndpoint()
    {
        var handler = new RecordingHttpMessageHandler(TestData.CreateJsonResponse("[]"));
        var client = new HttpClient(handler);
        var repository = new NordVpnServerRepository(client, "https://api.nordvpn.com/v1/servers");

        await repository.GetServersAsync(VpnServerQuery.All(), CancellationToken.None);

        Assert.Equal("https://api.nordvpn.com/v1/servers", handler.LastRequestUri!.ToString());
    }

    [Fact]
    public async Task GetServersAsyncWithCountryAndProtocolAddsBothFilters()
    {
        var handler = new RecordingHttpMessageHandler(TestData.CreateJsonResponse("[]"));
        var client = new HttpClient(handler);
        var repository = new NordVpnServerRepository(client, "https://api.nordvpn.com/v1/servers");
        var query = new VpnServerQuery
        {
            CountryId = 74,
            ProtocolId = 5
        };

        await repository.GetServersAsync(query, CancellationToken.None);

        Assert.Equal(
            "https://api.nordvpn.com/v1/servers?filters[servers_technologies][id]=5&filters[country_id]=74",
            handler.LastRequestUri!.ToString());
    }

    [Fact]
    public async Task GetServersAsyncDeserializesServersFromResponse()
    {
        var handler = new RecordingHttpMessageHandler(
            TestData.CreateJsonResponse("[{\"name\":\"pl1\",\"load\":11,\"status\":\"online\"}]"));
        var client = new HttpClient(handler);
        var repository = new NordVpnServerRepository(client, "https://api.nordvpn.com/v1/servers");

        var result = await repository.GetServersAsync(VpnServerQuery.All(), CancellationToken.None);

        var server = Assert.Single(result);
        Assert.Equal("pl1", server.Name);
        Assert.Equal(11, server.Load);
        Assert.Equal("online", server.Status);
    }
}
