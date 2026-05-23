using partycli.Domain;
using partycli.Application.Interfaces;
using System.Net;

namespace party.cli.Tests.TestHelpers;

internal class InMemoryStateStorage : IStateStorage
{
    private IReadOnlyList<Server> servers = Array.Empty<Server>();
    private IReadOnlyList<LogEntry> logs = Array.Empty<LogEntry>();

    public IReadOnlyList<Server> GetServers()
    {
        return servers;
    }

    public void SaveServers(IReadOnlyList<Server> servers)
    {
        this.servers = servers;
    }

    public IReadOnlyList<LogEntry> GetLogs()
    {
        return logs;
    }

    public void SaveLogs(IReadOnlyList<LogEntry> logs)
    {
        this.logs = logs;
    }
}

internal class RecordingLogger : IActionLogger
{
    public List<string> Actions { get; } = new();

    public void Log(string action)
    {
        Actions.Add(action);
    }
}

internal class RecordingServerRepository : IServerRepository
{
    private readonly IReadOnlyList<Server> serversToReturn;

    public RecordingServerRepository(IReadOnlyList<Server>? serversToReturn = null)
    {
        this.serversToReturn = serversToReturn ?? Array.Empty<Server>();
    }

    public List<VpnServerQuery> Queries { get; } = new();

    public Task<IReadOnlyList<Server>> GetServersAsync(VpnServerQuery query, CancellationToken cancellationToken)
    {
        Queries.Add(query);
        return Task.FromResult(serversToReturn);
    }
}

internal class StubServerService : IServerService
{
    public IReadOnlyList<Server> LocalServers { get; set; } = Array.Empty<Server>();
    public IReadOnlyList<Server> RemoteServers { get; set; } = Array.Empty<Server>();
    public Exception? ExceptionToThrow { get; set; }
    public int? LastCountryId { get; private set; }
    public int? LastProtocolId { get; private set; }
    public bool FetchAllCalled { get; private set; }
    public bool GetLocalCalled { get; private set; }

    public Task<IReadOnlyList<Server>> FetchAllAsync(CancellationToken cancellationToken)
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        FetchAllCalled = true;
        return Task.FromResult(RemoteServers);
    }

    public Task<IReadOnlyList<Server>> FetchAsync(
        int? countryId,
        int? protocolId,
        CancellationToken cancellationToken)
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        LastCountryId = countryId;
        LastProtocolId = protocolId;
        return Task.FromResult(RemoteServers);
    }

    public IReadOnlyList<Server> GetLocal()
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        GetLocalCalled = true;
        return LocalServers;
    }
}

internal class ConsoleCapture : IDisposable
{
    private readonly TextWriter originalOut;
    private readonly TextWriter originalError;

    public ConsoleCapture()
    {
        originalOut = Console.Out;
        originalError = Console.Error;
        Output = new StringWriter();
        Error = new StringWriter();

        Console.SetOut(Output);
        Console.SetError(Error);
    }

    public StringWriter Output { get; }

    public StringWriter Error { get; }

    public void Dispose()
    {
        Console.SetOut(originalOut);
        Console.SetError(originalError);
        Output.Dispose();
        Error.Dispose();
    }
}

internal class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage response;

    public RecordingHttpMessageHandler(HttpResponseMessage response)
    {
        this.response = response;
    }

    public Uri? LastRequestUri { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequestUri = request.RequestUri;
        return Task.FromResult(response);
    }
}

internal static class TestData
{
    public static Server CreateServer(string name = "pl1", int load = 42, string status = "online")
    {
        return new Server
        {
            Name = name,
            Load = load,
            Status = status
        };
    }

    public static HttpResponseMessage CreateJsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };
    }
}
