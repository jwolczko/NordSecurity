using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using partycli.Application.Interfaces;

namespace partycli.Infrastructure.Storage;

public class SettingsStorage : ISettingsStorage
{
    private readonly string filePath;

    public SettingsStorage()
        : this(Path.Combine(AppContext.BaseDirectory, "appsettings.json"))
    {
    }

    public SettingsStorage(string filePath)
    {
        this.filePath = filePath;
    }

    public string GetValue(string name)
    {
        try
        {
            var settings = LoadSettings();

            return settings.TryGetValue(name, out var value)
                ? value.ToString()
                : string.Empty;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Couldn't read setting '{name}' from appsettings.json.", exception);
        }
    }

    public void SetValue(string name, string value)
    {
        try
        {
            var settings = LoadSettings();

            settings[name] = value;
            SaveSettings(settings);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Couldn't save setting '{name}' to appsettings.json.", exception);
        }
    }

    private JObject LoadSettings()
    {
        EnsureSettingsFileExists();

        var json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return CreateDefaultSettings();
        }

        return JObject.Parse(json);
    }

    private void SaveSettings(JObject settings)
    {
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, settings.ToString(Formatting.Indented));
    }

    private void EnsureSettingsFileExists()
    {
        if (File.Exists(filePath))
        {
            return;
        }

        SaveSettings(CreateDefaultSettings());
    }

    private static JObject CreateDefaultSettings()
    {
        return new JObject
        {
            ["serverlist"] = string.Empty,
            ["log"] = string.Empty
        };
    }
}
