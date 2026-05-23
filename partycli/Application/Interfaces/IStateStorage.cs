using System.Collections.Generic;
using partycli.Domain;

namespace partycli.Application.Interfaces;

public interface IStateStorage
{
    IReadOnlyList<Server> GetServers();

    void SaveServers(IReadOnlyList<Server> servers);

    IReadOnlyList<LogEntry> GetLogs();

    void SaveLogs(IReadOnlyList<LogEntry> logs);
}
