using System;
using System.IO;
using Newtonsoft.Json;

namespace partycli.Infrastructure.Configuration;

public class AppConfiguration
{
    public NordVpnConfiguration NordVpn { get; set; }

    public static AppConfiguration Load()
    {
        return Load(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
    }

    public static AppConfiguration Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException("Configuration file appsettings.json was not found.");
        }

        var json = File.ReadAllText(filePath);
        var configuration = JsonConvert.DeserializeObject<AppConfiguration>(json);

        if (string.IsNullOrWhiteSpace(configuration?.NordVpn?.ServersEndpoint))
        {
            throw new InvalidOperationException("Missing configuration value 'nordVpn.serversEndpoint'.");
        }

        return configuration;
    }
}

public class NordVpnConfiguration
{
    public string ServersEndpoint { get; set; }
}
