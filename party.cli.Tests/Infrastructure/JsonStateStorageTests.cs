using Newtonsoft.Json.Linq;
using party.cli.Tests.TestHelpers;
using partycli.Domain;
using partycli.Infrastructure.Storage;

namespace party.cli.Tests.Infrastructure;

public class JsonStateStorageTests
{
    [Fact]
    public void GetServersWhenFileDoesNotExistCreatesDefaultFileAndReturnsEmptyList()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(directoryPath, "partycli-state.json");
        var storage = new JsonStateStorage(filePath);

        var servers = storage.GetServers();

        Assert.Empty(servers);
        Assert.True(File.Exists(filePath));
        var json = JObject.Parse(File.ReadAllText(filePath));
        Assert.Empty(json["ServerList"]!);
        Assert.Empty(json["Log"]!);

        Directory.Delete(directoryPath, true);
    }

    [Fact]
    public void SaveServersUpdatesServerListWithoutClearingLog()
    {
        var filePath = CreateTempStateFile();

        try
        {
            var storage = new JsonStateStorage(filePath);

            storage.SaveServers(new[] { TestData.CreateServer("fr1", 12, "online") });

            var servers = storage.GetServers();
            var server = Assert.Single(servers);
            Assert.Equal("fr1", server.Name);
            Assert.Single(storage.GetLogs());
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveLogsUpdatesLogWithoutClearingServerList()
    {
        var filePath = CreateTempStateFile();

        try
        {
            var storage = new JsonStateStorage(filePath);

            storage.SaveLogs(new[]
            {
                new LogEntry
                {
                    Action = "new action",
                    Time = new DateTime(2026, 5, 23, 12, 0, 0)
                }
            });

            Assert.Single(storage.GetLogs());
            Assert.Single(storage.GetServers());
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void GetServersWhenStateUsesLegacyStringFormatMigratesInMemory()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(
            filePath,
            """
            {
              "serverlist": "[{\"Name\":\"legacy1\",\"Load\":4,\"Status\":\"online\"}]",
              "log": "[{\"Action\":\"legacy action\",\"Time\":\"2026-05-23T10:00:00\"}]"
            }
            """);

        try
        {
            var storage = new JsonStateStorage(filePath);

            var server = Assert.Single(storage.GetServers());
            var logEntry = Assert.Single(storage.GetLogs());

            Assert.Equal("legacy1", server.Name);
            Assert.Equal("legacy action", logEntry.Action);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void GetServersWhenJsonIsInvalidThrowsInvalidOperationException()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(filePath, "{invalid");

        try
        {
            var storage = new JsonStateStorage(filePath);

            var exception = Assert.Throws<InvalidOperationException>(() => storage.GetServers());

            Assert.Equal("Couldn't read application state.", exception.Message);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private static string CreateTempStateFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(
            filePath,
            """
            {
              "ServerList": [
                {
                  "Name": "stored1",
                  "Load": 7,
                  "Status": "online"
                }
              ],
              "Log": [
                {
                  "Action": "existing action",
                  "Time": "2026-05-23T10:00:00"
                }
              ]
            }
            """);
        return filePath;
    }
}
