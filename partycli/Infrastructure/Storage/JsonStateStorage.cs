using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using partycli.Application.Interfaces;
using partycli.Domain;

namespace partycli.Infrastructure.Storage;

public class JsonStateStorage : IStateStorage
{
    private readonly string filePath;

    public JsonStateStorage()
        : this(Path.Combine(AppContext.BaseDirectory, "partycli-state.json"))
    {
    }

    public JsonStateStorage(string filePath)
    {
        this.filePath = filePath;
    }

    public IReadOnlyList<Server> GetServers()
    {
        return LoadState().ServerList;
    }

    public void SaveServers(IReadOnlyList<Server> servers)
    {
        var state = LoadState();
        state.ServerList = new List<Server>(servers);

        SaveState(state);
    }

    public IReadOnlyList<LogEntry> GetLogs()
    {
        return LoadState().Log;
    }

    public void SaveLogs(IReadOnlyList<LogEntry> logs)
    {
        var state = LoadState();
        state.Log = new List<LogEntry>(logs);

        SaveState(state);
    }

    private PartyCliState LoadState()
    {
        try
        {
            EnsureStateFileExists();

            var json = File.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new PartyCliState();
            }

            return DeserializeState(json);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Couldn't read application state.", exception);
        }
    }

    private void SaveState(PartyCliState state)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(state, Formatting.Indented));
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Couldn't save application state.", exception);
        }
    }

    private void EnsureStateFileExists()
    {
        if (File.Exists(filePath))
        {
            return;
        }

        SaveState(new PartyCliState());
    }

    private static PartyCliState DeserializeState(string json)
    {
        var stateJson = JObject.Parse(json);

        return new PartyCliState
        {
            ServerList = ReadList<Server>(stateJson, "ServerList", "serverlist"),
            Log = ReadList<LogEntry>(stateJson, "Log", "log")
        };
    }

    private static List<T> ReadList<T>(JObject stateJson, string currentPropertyName, string legacyPropertyName)
    {
        var currentValue = stateJson[currentPropertyName];

        if (currentValue is JArray currentArray)
        {
            return currentArray.ToObject<List<T>>() ?? new List<T>();
        }

        var legacyValue = stateJson[legacyPropertyName];

        if (legacyValue?.Type == JTokenType.String)
        {
            var legacyJson = legacyValue.ToString();

            if (string.IsNullOrWhiteSpace(legacyJson))
            {
                return new List<T>();
            }

            return JsonConvert.DeserializeObject<List<T>>(legacyJson) ?? new List<T>();
        }

        return new List<T>();
    }

    private sealed class PartyCliState
    {
        public List<Server> ServerList { get; set; } = new();

        public List<LogEntry> Log { get; set; } = new();
    }
}
