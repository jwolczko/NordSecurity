using System;
using System.IO;
using Newtonsoft.Json.Linq;
using partycli.Infrastructure.Storage;

namespace party.cli.Tests.Infrastructure;

public class SettingsStorageTests
{
    [Fact]
    public void GetValueWhenFileDoesNotExistCreatesDefaultFileAndReturnsEmptyString()
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(directoryPath, "appsettings.json");
        var storage = new SettingsStorage(filePath);

        var value = storage.GetValue("serverlist");

        Assert.Equal(string.Empty, value);
        Assert.True(File.Exists(filePath));
        var json = JObject.Parse(File.ReadAllText(filePath));
        Assert.Equal(string.Empty, json["serverlist"]?.ToString());
        Assert.Equal(string.Empty, json["log"]?.ToString());

        Directory.Delete(directoryPath, true);
    }

    [Fact]
    public void SetValueUpdatesSelectedSetting()
    {
        var filePath = CreateTempSettingsFile();

        try
        {
            var storage = new SettingsStorage(filePath);

            storage.SetValue("serverlist", "saved-value");

            var json = JObject.Parse(File.ReadAllText(filePath));
            Assert.Equal("saved-value", json["serverlist"]?.ToString());
            Assert.Equal(string.Empty, json["log"]?.ToString());
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void GetValueWhenSettingDoesNotExistReturnsEmptyString()
    {
        var filePath = CreateTempSettingsFile();

        try
        {
            var storage = new SettingsStorage(filePath);

            var value = storage.GetValue("missing");

            Assert.Equal(string.Empty, value);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void GetValueWhenJsonIsInvalidThrowsInvalidOperationException()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(filePath, "{invalid");

        try
        {
            var storage = new SettingsStorage(filePath);

            var exception = Assert.Throws<InvalidOperationException>(() => storage.GetValue("serverlist"));

            Assert.Equal("Couldn't read setting 'serverlist' from appsettings.json.", exception.Message);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private static string CreateTempSettingsFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(
            filePath,
            """
            {
              "serverlist": "",
              "log": ""
            }
            """);
        return filePath;
    }
}
