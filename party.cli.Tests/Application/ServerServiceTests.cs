using party.cli.Tests.TestHelpers;
using partycli.Application.Services;

namespace party.cli.Tests.Application;

public class ServerServiceTests
{
    [Fact]
    public async Task FetchAllAsyncRequestsAllServersStoresSerializedResultAndLogsAction()
    {
        var servers = new[] { TestData.CreateServer() };
        var repository = new RecordingServerRepository(servers);
        var storage = new InMemoryStateStorage();
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        var result = await service.FetchAllAsync(CancellationToken.None);

        Assert.Same(servers, result);
        var query = Assert.Single(repository.Queries);
        Assert.Null(query.CountryId);
        Assert.Null(query.ProtocolId);
        Assert.Single(storage.GetServers());
        Assert.Single(logger.Actions);
        Assert.Equal("Saved all servers. Total servers: 1", logger.Actions[0]);
    }

    [Fact]
    public async Task FetchAsyncUsesCountryQuery()
    {
        var repository = new RecordingServerRepository();
        var storage = new InMemoryStateStorage();
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        await service.FetchAsync(countryId: 74, protocolId: null, CancellationToken.None);

        var query = Assert.Single(repository.Queries);
        Assert.Equal(74, query.CountryId);
        Assert.Null(query.ProtocolId);
        Assert.Equal("Saved servers for country 74. Total servers: 0", Assert.Single(logger.Actions));
    }

    [Fact]
    public async Task FetchAsyncUsesProtocolQuery()
    {
        var repository = new RecordingServerRepository();
        var storage = new InMemoryStateStorage();
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        await service.FetchAsync(countryId: null, protocolId: 5, CancellationToken.None);

        var query = Assert.Single(repository.Queries);
        Assert.Equal(5, query.ProtocolId);
        Assert.Null(query.CountryId);
        Assert.Equal("Saved servers for protocol 5. Total servers: 0", Assert.Single(logger.Actions));
    }

    [Fact]
    public void GetLocalWhenStorageIsEmptyReturnsEmptyList()
    {
        var repository = new RecordingServerRepository();
        var storage = new InMemoryStateStorage();
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        var result = service.GetLocal();

        Assert.Empty(result);
    }

    [Fact]
    public void GetLocalWhenStorageContainsSerializedServersReturnsDeserializedList()
    {
        var repository = new RecordingServerRepository();
        var storage = new InMemoryStateStorage();
        storage.SaveServers(new[] { TestData.CreateServer("uk1", 15, "online") });
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        var result = service.GetLocal();

        var server = Assert.Single(result);
        Assert.Equal("uk1", server.Name);
        Assert.Equal(15, server.Load);
        Assert.Equal("online", server.Status);
    }

    [Fact]
    public async Task FetchAsyncUsesCombinedCountryAndProtocolQuery()
    {
        var repository = new RecordingServerRepository();
        var storage = new InMemoryStateStorage();
        var logger = new RecordingLogger();
        var service = new ServerService(repository, storage, logger);

        await service.FetchAsync(countryId: 74, protocolId: 5, CancellationToken.None);

        var query = Assert.Single(repository.Queries);
        Assert.Equal(74, query.CountryId);
        Assert.Equal(5, query.ProtocolId);
        Assert.Equal("Saved servers for country 74 and protocol 5. Total servers: 0", Assert.Single(logger.Actions));
    }
}
