using System.Threading;
using System.Threading.Tasks;
using party.cli.Tests.TestHelpers;
using partycli.Infrastructure.NordVpn;
using partycli.Presentation.Cli;

namespace party.cli.Tests.Presentation;

public class ServerListCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncWhenLocalIsCombinedWithRemoteFilterReturnsErrorWithoutCallingService()
    {
        var service = new StubServerService();
        var handler = CreateHandler(service);
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: true, country: "france", protocol: null, CancellationToken.None);

        Assert.Equal(1, result);
        Assert.False(service.GetLocalCalled);
        Assert.False(service.FetchAllCalled);
        Assert.Contains("Error: Use --local without country or protocol filters.", console.Error.ToString());
    }

    [Fact]
    public async Task HandleAsyncWhenLocalStorageIsEmptyReturnsError()
    {
        var service = new StubServerService();
        var handler = CreateHandler(service);
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: true, country: null, protocol: null, CancellationToken.None);

        Assert.Equal(1, result);
        Assert.True(service.GetLocalCalled);
        Assert.Contains("Error: There are no server data in local storage.", console.Error.ToString());
    }

    [Fact]
    public async Task HandleAsyncWhenCountryFilterIsUsedFetchesServersAndPrintsThem()
    {
        var service = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer("fr1", 18, "online") }
        };
        var handler = CreateHandler(service);
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: false, country: "france", protocol: null, CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Equal(74, service.LastCountryId);
        Assert.Contains("Name: fr1, Load: 18, Status: online", console.Output.ToString());
    }

    [Fact]
    public async Task HandleLegacyAsyncMapsFlagsToCountryAndProtocolIds()
    {
        var service = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer() }
        };
        var handler = CreateHandler(service);
        using var console = new ConsoleCapture();

        await handler.HandleLegacyAsync(local: false, france: true, tcp: false, CancellationToken.None);
        Assert.Equal(74, service.LastCountryId);

        service = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer() }
        };
        handler = CreateHandler(service);

        await handler.HandleLegacyAsync(local: false, france: false, tcp: true, CancellationToken.None);
        Assert.Equal(5, service.LastProtocolId);
    }

    [Fact]
    public async Task HandleAsyncWhenProtocolNameIsUsedFetchesServersByMappedProtocolId()
    {
        var service = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer("tcp1", 11, "online") }
        };
        var handler = CreateHandler(service);
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: false, country: null, protocol: "TCP", CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Equal(5, service.LastProtocolId);
        Assert.Contains("Name: tcp1, Load: 11, Status: online", console.Output.ToString());
    }

    [Fact]
    public async Task HandleAsyncWhenCountryAndProtocolAreUsedFetchesServersByBothFilters()
    {
        var service = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer("fr-tcp1", 9, "online") }
        };
        var handler = CreateHandler(service);
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: false, country: "france", protocol: "TCP", CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Equal(74, service.LastCountryId);
        Assert.Equal(5, service.LastProtocolId);
        Assert.Contains("Name: fr-tcp1, Load: 9, Status: online", console.Output.ToString());
    }

    [Fact]
    public async Task HandleAsyncWhenCountryNameIsUsedFetchesServersByMappedCountryId()
    {
        var service = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer("fr1", 11, "online") }
        };
        var handler = CreateHandler(service);
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: false, country: "france", protocol: null, CancellationToken.None);

        Assert.Equal(0, result);
        Assert.Equal(74, service.LastCountryId);
        Assert.Contains("Name: fr1, Load: 11, Status: online", console.Output.ToString());
    }

    [Fact]
    public async Task HandleAsyncWhenUnsupportedCountryIsUsedReturnsFriendlyError()
    {
        var service = new StubServerService();
        var handler = CreateHandler(service);
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: false, country: "atlantis", protocol: null, CancellationToken.None);

        Assert.Equal(1, result);
        Assert.Null(service.LastCountryId);
        Assert.Contains("Error: Unsupported country 'atlantis'. Supported countries:", console.Error.ToString());
    }

    [Fact]
    public async Task HandleAsyncWhenUnsupportedProtocolIsUsedReturnsFriendlyError()
    {
        var service = new StubServerService();
        var handler = CreateHandler(service);
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: false, country: null, protocol: "PPTP", CancellationToken.None);

        Assert.Equal(1, result);
        Assert.Null(service.LastProtocolId);
        Assert.Contains("Error: Unsupported protocol 'PPTP'. Supported protocols:", console.Error.ToString());
    }

    [Fact]
    public async Task HandleAsyncWhenServiceThrowsWritesErrorAndReturnsOne()
    {
        var service = new StubServerService
        {
            ExceptionToThrow = new InvalidOperationException("request failed")
        };
        var handler = CreateHandler(service);
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: false, country: null, protocol: null, CancellationToken.None);

        Assert.Equal(1, result);
        Assert.Contains("Error: request failed", console.Error.ToString());
    }

    private static ServerListCommandHandler CreateHandler(StubServerService service)
    {
        return new ServerListCommandHandler(service, new ConsoleOutput(), new NordVpnServerFilterCatalog());
    }
}
