using partycli.Infrastructure.Configuration;

namespace party.cli.Tests.Infrastructure;

public class AppConfigurationTests
{
    [Fact]
    public void LoadWhenConfigurationIsValidReturnsEndpoint()
    {
        var filePath = CreateTempFile(
            """
            {
              "nordVpn": {
                "serversEndpoint": "https://api.nordvpn.com/v1/servers"
              }
            }
            """);

        try
        {
            var configuration = AppConfiguration.Load(filePath);

            Assert.Equal("https://api.nordvpn.com/v1/servers", configuration.NordVpn.ServersEndpoint);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void LoadWhenFileDoesNotExistThrowsInvalidOperationException()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        var exception = Assert.Throws<InvalidOperationException>(() => AppConfiguration.Load(filePath));

        Assert.Equal("Configuration file appsettings.json was not found.", exception.Message);
    }

    [Fact]
    public void LoadWhenEndpointIsMissingThrowsInvalidOperationException()
    {
        var filePath = CreateTempFile("{\"nordVpn\":{}}");

        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() => AppConfiguration.Load(filePath));

            Assert.Equal("Missing configuration value 'nordVpn.serversEndpoint'.", exception.Message);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private static string CreateTempFile(string content)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(filePath, content);
        return filePath;
    }
}
