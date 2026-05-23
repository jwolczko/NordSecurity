using System;
using System.Collections.Generic;
using partycli.Application.Interfaces;
using partycli.Domain;

namespace partycli.Application.Services;

public class LoggerService : IActionLogger
{
    private readonly IStateStorage stateStorage;

    public LoggerService(IStateStorage stateStorage)
    {
        this.stateStorage = stateStorage;
    }

    public void Log(string action)
    {
        var currentLog = new List<LogEntry>(stateStorage.GetLogs());

        currentLog.Add(new LogEntry
        {
            Action = action,
            Time = DateTime.Now
        });

        stateStorage.SaveLogs(currentLog);
    }
}
