using System;
using partycli.Application.Interfaces;

namespace partycli.Presentation.Cli;

public class ConfigCommandHandler
{
    private readonly IConfigService configService;
    private readonly ConsoleOutput consoleOutput;

    public ConfigCommandHandler(IConfigService configService, ConsoleOutput consoleOutput)
    {
        this.configService = configService;
        this.consoleOutput = consoleOutput;
    }

    public int Handle(string name, string value)
    {
        try
        {
            configService.SetValue(name, value);
            consoleOutput.WriteLine($"Changed {name} to {value}");

            return 0;
        }
        catch (Exception exception)
        {
            consoleOutput.WriteError(exception.Message);

            return 1;
        }
    }
}
