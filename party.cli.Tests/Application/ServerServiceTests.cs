using Newtonsoft.Json;
using party.cli.Tests.TestHelpers;
using partycli.Application.Services;
using partycli.Domain;

namespace party.cli.Tests.Application;

public class ServerServiceTests
{
    [Fact]
    public async Task FetchAllAsyncRequestsAllServersStoresSerializedResultAndLogsAction()
    {
        var servers = new[] { TestData.CreateServer() };
        var repository = new RecordingServerRepository(servers);
        var storage = new InMemorySettingsStorage();
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        var result = await service.FetchAllAsync(CancellationToken.None);

        Assert.Same(servers, result);
        var query = Assert.Single(repository.Queries);
        Assert.Null(query.CountryId);
        Assert.Null(query.ProtocolId);
        var savedServers = JsonConvert.DeserializeObject<Server[]>(storage.GetValue("serverlist"));
        Assert.NotNull(savedServers);
        Assert.Single(savedServers);
        Assert.Single(logger.Actions);
        Assert.Equal("Saved all servers. Total servers: 1", logger.Actions[0]);
    }

    [Fact]
    public async Task FetchByCountryAsyncUsesCountryQuery()
    {
        var repository = new RecordingServerRepository();
        var storage = new InMemorySettingsStorage();
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        await service.FetchByCountryAsync(74, CancellationToken.None);

        var query = Assert.Single(repository.Queries);
        Assert.Equal(74, query.CountryId);
        Assert.Null(query.ProtocolId);
        Assert.Equal("Saved servers for country 74. Total servers: 0", Assert.Single(logger.Actions));
    }

    [Fact]
    public async Task FetchByProtocolAsyncUsesProtocolQuery()
    {
        var repository = new RecordingServerRepository();
        var storage = new InMemorySettingsStorage();
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        await service.FetchByProtocolAsync(5, CancellationToken.None);

        var query = Assert.Single(repository.Queries);
        Assert.Equal(5, query.ProtocolId);
        Assert.Null(query.CountryId);
        Assert.Equal("Saved servers for protocol 5. Total servers: 0", Assert.Single(logger.Actions));
    }

    [Fact]
    public void GetLocalWhenStorageIsEmptyReturnsEmptyList()
    {
        var repository = new RecordingServerRepository();
        var storage = new InMemorySettingsStorage();
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        var result = service.GetLocal();

        Assert.Empty(result);
    }

    [Fact]
    public void GetLocalWhenStorageContainsSerializedServersReturnsDeserializedList()
    {
        var expected = new[] { TestData.CreateServer("uk1", 15, "online") };
        var repository = new RecordingServerRepository();
        var storage = new InMemorySettingsStorage();
        storage.SetValue("serverlist", JsonConvert.SerializeObject(expected));
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        var result = service.GetLocal();

        var server = Assert.Single(result);
        Assert.Equal("uk1", server.Name);
        Assert.Equal(15, server.Load);
        Assert.Equal("online", server.Status);
    }
}
