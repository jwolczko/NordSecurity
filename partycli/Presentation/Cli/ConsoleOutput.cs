using System;
using System.Collections.Generic;
using partycli.Domain;

namespace partycli.Presentation.Cli;

public class ConsoleOutput
{
    public void WriteServers(IReadOnlyList<Server> servers)
    {
        Console.WriteLine("Server list:");

        foreach (var server in servers)
        {
            Console.WriteLine($"Name: {server.Name}, Load: {server.Load}, Status: {server.Status}");
        }

        Console.WriteLine($"Total servers: {servers.Count}");
    }

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteError(string message)
    {
        Console.Error.WriteLine($"Error: {message}");
    }
}
