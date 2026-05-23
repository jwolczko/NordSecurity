using party.cli.Tests.TestHelpers;
using partycli.Presentation.Cli;

namespace party.cli.Tests.Presentation;

public class ConfigCommandHandlerTests
{
    [Fact]
    public void HandleWhenConfigurationSucceedsWritesConfirmationAndReturnsZero()
    {
        var service = new StubConfigService();
        var handler = new ConfigCommandHandler(service, new ConsoleOutput());
        using var console = new ConsoleCapture();

        var result = handler.Handle("serverlist", "servers.json");

        Assert.Equal(0, result);
        Assert.Equal("serverlist", service.LastName);
        Assert.Equal("servers.json", service.LastValue);
        Assert.Contains("Changed serverlist to servers.json", console.Output.ToString());
    }

    [Fact]
    public void HandleWhenConfigurationFailsWritesErrorAndReturnsOne()
    {
        var service = new StubConfigService
        {
            ExceptionToThrow = new InvalidOperationException("failure")
        };
        var handler = new ConfigCommandHandler(service, new ConsoleOutput());
        using var console = new ConsoleCapture();

        var result = handler.Handle("serverlist", "servers.json");

        Assert.Equal(1, result);
        Assert.Contains("Error: failure", console.Error.ToString());
    }
}
