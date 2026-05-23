using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using partycli.Infrastructure.DependencyInjection;

namespace partycli;

public static class Program
{
    private static async Task<int> Main(string[] args)
    {
        using var serviceProvider = DependencyInjectionConfig.BuildServiceProvider();
        var rootCommand = serviceProvider.GetRequiredService<RootCommand>();

        return await rootCommand.Parse(args).InvokeAsync();
    }
}
