using party.cli.Tests.TestHelpers;
using partycli.Presentation.Cli;

namespace party.cli.Tests.Presentation;

public class ConsoleOutputTests
{
    [Fact]
    public void WriteServersPrintsHeaderServersAndTotalCount()
    {
        using var console = new ConsoleCapture();
        var output = new ConsoleOutput();

        output.WriteServers(new[] { TestData.CreateServer("pl1", 22, "online") });

        var text = console.Output.ToString();
        Assert.Contains("Server list:", text);
        Assert.Contains("Name: pl1, Load: 22, Status: online", text);
        Assert.Contains("Total servers: 1", text);
    }

    [Fact]
    public void WriteLinePrintsMessage()
    {
        using var console = new ConsoleCapture();
        var output = new ConsoleOutput();

        output.WriteLine("hello");

        Assert.Contains("hello", console.Output.ToString());
    }

    [Fact]
    public void WriteErrorPrintsErrorPrefix()
    {
        using var console = new ConsoleCapture();
        var output = new ConsoleOutput();

        output.WriteError("failure");

        Assert.Contains("Error: failure", console.Error.ToString());
    }
}
