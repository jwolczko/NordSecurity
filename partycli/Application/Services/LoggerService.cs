using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using partycli.Application.Interfaces;
using partycli.Domain;

namespace partycli.Application.Services;

public class LoggerService : IActionLogger
{
    private const string LogSettingName = "log";

    private readonly ISettingsStorage settingsStorage;

    public LoggerService(ISettingsStorage settingsStorage)
    {
        this.settingsStorage = settingsStorage;
    }

    public void Log(string action)
    {
        var currentLog = LoadCurrentLog();

        currentLog.Add(new LogEntry
        {
            Action = action,
            Time = DateTime.Now
        });

        settingsStorage.SetValue(LogSettingName, JsonConvert.SerializeObject(currentLog));
    }

    private List<LogEntry> LoadCurrentLog()
    {
        var serializedLog = settingsStorage.GetValue(LogSettingName);

        if (string.IsNullOrWhiteSpace(serializedLog))
        {
            return new List<LogEntry>();
        }

        return JsonConvert.DeserializeObject<List<LogEntry>>(serializedLog) ?? new List<LogEntry>();
    }
}
