using System.Threading;
using System.Threading.Tasks;
using party.cli.Tests.TestHelpers;
using partycli.Presentation.Cli;

namespace party.cli.Tests.Presentation;

public class ServerListCommandHandlerTests
{
    [Fact]
    public async Task HandleAsyncWhenMultipleSourcesAreSelectedReturnsErrorWithoutCallingService()
    {
        var service = new StubServerService();
        var handler = new ServerListCommandHandler(service, new ConsoleOutput());
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: true, countryId: 74, protocolId: null, CancellationToken.None);

        Assert.Equal(1, result);
        Assert.False(service.GetLocalCalled);
        Assert.False(service.FetchAllCalled);
        Assert.Contains("Error: Use only one server source at a time.", console.Error.ToString());
    }

    [Fact]
    public async Task HandleAsyncWhenLocalStorageIsEmptyReturnsError()
    {
        var service = new StubServerService();
        var handler = new ServerListCommandHandler(service, new ConsoleOutput());
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: true, countryId: null, protocolId: null, CancellationToken.None);

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
        var handler = new ServerListCommandHandler(service, new ConsoleOutput());
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: false, countryId: 74, protocolId: null, CancellationToken.None);

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
        var handler = new ServerListCommandHandler(service, new ConsoleOutput());
        using var console = new ConsoleCapture();

        await handler.HandleLegacyAsync(local: false, france: true, tcp: false, CancellationToken.None);
        Assert.Equal(74, service.LastCountryId);

        service = new StubServerService
        {
            RemoteServers = new[] { TestData.CreateServer() }
        };
        handler = new ServerListCommandHandler(service, new ConsoleOutput());

        await handler.HandleLegacyAsync(local: false, france: false, tcp: true, CancellationToken.None);
        Assert.Equal(5, service.LastProtocolId);
    }

    [Fact]
    public async Task HandleAsyncWhenServiceThrowsWritesErrorAndReturnsOne()
    {
        var service = new StubServerService
        {
            ExceptionToThrow = new InvalidOperationException("request failed")
        };
        var handler = new ServerListCommandHandler(service, new ConsoleOutput());
        using var console = new ConsoleCapture();

        var result = await handler.HandleAsync(local: false, countryId: null, protocolId: null, CancellationToken.None);

        Assert.Equal(1, result);
        Assert.Contains("Error: request failed", console.Error.ToString());
    }
}
