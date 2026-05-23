using partycli.Application.Interfaces;

namespace partycli.Application.Services;

public class ConfigService : IConfigService
{
    private readonly ISettingsStorage settingsStorage;
    private readonly IActionLogger logger;

    public ConfigService(ISettingsStorage settingsStorage, IActionLogger logger)
    {
        this.settingsStorage = settingsStorage;
        this.logger = logger;
    }

    public void SetValue(string name, string value)
    {
        var normalizedName = NormalizeName(name);

        settingsStorage.SetValue(normalizedName, value);
        logger.Log($"Changed {normalizedName} to {value}");
    }

    private static string NormalizeName(string name)
    {
        return name.Replace("-", string.Empty);
    }
}
