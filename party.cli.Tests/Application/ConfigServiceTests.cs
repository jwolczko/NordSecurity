using party.cli.Tests.TestHelpers;
using partycli.Application.Services;

namespace party.cli.Tests.Application;

public class ConfigServiceTests
{
    [Fact]
    public void SetValueRemovesHyphensBeforeSavingAndLogging()
    {
        var storage = new InMemorySettingsStorage();
        var logger = new RecordingLogger();
        var service = new ConfigService(storage, logger);

        service.SetValue("server-list", "value.json");

        Assert.Equal("value.json", storage.GetValue("serverlist"));
        Assert.Equal(string.Empty, storage.GetValue("server-list"));
        Assert.Single(logger.Actions);
        Assert.Equal("Changed serverlist to value.json", logger.Actions[0]);
    }
}
