using Newtonsoft.Json;
using party.cli.Tests.TestHelpers;
using partycli.Application.Services;
using partycli.Domain;

namespace party.cli.Tests.Application;

public class LoggerServiceTests
{
    [Fact]
    public void LogWhenLogIsEmptyCreatesNewEntry()
    {
        var storage = new InMemorySettingsStorage();
        var service = new LoggerService(storage);
        var before = DateTime.Now;

        service.Log("Saved all servers");

        var after = DateTime.Now;
        var entries = JsonConvert.DeserializeObject<List<LogEntry>>(storage.GetValue("log"));

        var entry = Assert.Single(entries!);
        Assert.Equal("Saved all servers", entry.Action);
        Assert.InRange(entry.Time, before, after);
    }

    [Fact]
    public void LogWhenLogAlreadyExistsAppendsEntry()
    {
        var existingEntries = new List<LogEntry>
        {
            new()
            {
                Action = "Existing action",
                Time = new DateTime(2026, 5, 23, 10, 0, 0)
            }
        };

        var storage = new InMemorySettingsStorage();
        storage.SetValue("log", JsonConvert.SerializeObject(existingEntries));
        var service = new LoggerService(storage);

        service.Log("New action");

        var entries = JsonConvert.DeserializeObject<List<LogEntry>>(storage.GetValue("log"));

        Assert.NotNull(entries);
        Assert.Equal(2, entries.Count);
        Assert.Equal("Existing action", entries[0].Action);
        Assert.Equal("New action", entries[1].Action);
    }
}
