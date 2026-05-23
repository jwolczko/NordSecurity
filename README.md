# My solution of Technical interview tasks for .net developer role
## Current project specification
In current repository you will find project which after build will make console app named `partycli.exe` that shows and saves servers received from API:  

Currently this console application has following functions:  

- This should fetch servers from API, store them in persistent data store and display each server (name, load, status) and total number of servers in the console:  
`partycli.exe server_list`

- This should fetch specific country (France) servers from API, store them in persistent data store and display each server (name, load, status) and total number of servers in the console:  
`partycli.exe server_list --france` 

- This should fetch specific TCP protocol servers from API, store them in persistent data store and display each server (name, load, status) and total number of servers in the console:  
`partycli.exe server_list --TCP`

- This should fetch servers from persistent data store and display each server (name, load, status) and total number of servers in the console:  
`partycli.exe server_list --local`

## Task

`partycli.exe` for now it’s simple console app and written without having in mind that it could grow in the near future into enterprise grade cli monster:

1. There might be more parameters for the app.
2. Persistent data store provider/storage type/libraries might change.
3. Servers might be displayed differently in the console or even displayed with colors.
4. Different API might be chosen.

>[!NOTE]
>It should be fairly easy to adapt current app code to the upcoming requirements. So choose your architecture wisely!

> [!TIP]
>Your goal is to improve this code, make it more robust, scalable, maintainable,testable - just easier to work with.

> [!IMPORTANT]
>All code modifications must be made within the current repository.

### Few simple requirements
- :ballot_box_with_check: Refactor existing application  
- :ballot_box_with_check: Write high quality, scalable, maintainable,testable code  
- :ballot_box_with_check: Try to follow modern .NET development practices  
- :ballot_box_with_check: Don't reinvent the wheel! If you find a nice library/framework that can make your life easier use it!  
- :ballot_box_with_check: Have fun!

## Change Report

### Refactoring Goal

The first version of the application was small, but too much work happened in `Program.cs`. Argument parsing, NordVPN API calls, JSON handling, logging, persistence, and console output were all tied together in one entry point. That was acceptable for a short exercise, but it would quickly become awkward once new commands, filters, output formats, or storage options were added.

The refactoring keeps the application lightweight while giving it a clearer structure. The code is now easier to navigate, easier to test, and better prepared for small future changes without turning the CLI into an over-engineered solution.

### Main Changes

1. **Smaller `Program.cs`**

   `Program.cs` now acts only as the application entry point. The `Main` method builds the dependency injection container, resolves the root command, and starts command parsing. NordVPN-specific behavior and command handling live outside of `Main`.

2. **Clearer project structure**

   The code is grouped into a few focused areas:

   - `Domain` - simple models such as `Server`, `LogEntry`, and `VpnServerQuery`.
   - `Application` - application services and interfaces.
   - `Infrastructure` - NordVPN API access, configuration, filter mapping, and JSON state storage.
   - `Presentation/Cli` - command definitions, handlers, and console output.

   This is a lightweight Clean Architecture-style split, not a heavy framework around a small console app.

3. **Command parsing with `System.CommandLine`**

   Manual argument parsing was replaced with `System.CommandLine`. Command definitions are easier to read, and help output is handled by the library.

   The old command style is still supported:

   - `partycli server_list`
   - `partycli server_list --france`
   - `partycli server_list --TCP`
   - `partycli server_list --local`

   A newer, more explicit command style is also available:

   - `partycli servers list`
   - `partycli servers list --country france`
   - `partycli servers list --protocol TCP`
   - `partycli servers list --country france --protocol TCP`
   - `partycli servers list --local`

4. **Dependency injection**

   Object creation was moved to `DependencyInjectionConfig`, using `Microsoft.Extensions.DependencyInjection`. Services now receive their dependencies through constructors, which keeps classes simpler and makes them easier to test.

5. **Configuration and runtime state**

   The old `App.config` and `Properties.Settings` setup was removed. Static configuration is stored in `appsettings.json`, while runtime data is stored separately in `partycli-state.json`.

   `appsettings.json` contains:

   - `nordVpn.serversEndpoint` - the NordVPN API endpoint.

   `partycli-state.json` contains:

   - the last stored server list,
   - application log entries.

   This keeps application configuration separate from data created while the program is running.

6. **Endpoint moved out of code**

   The NordVPN API URL is no longer hardcoded in the repository. `NordVpnServerRepository` receives the endpoint from configuration, so changing the API address does not require changing repository code.

7. **User-friendly filters**

   The CLI no longer exposes NordVPN numeric identifiers. Users can pass readable values such as:

   - `--country france`
   - `--protocol TCP`

   `NordVpnServerFilterCatalog` maps those values to the identifiers expected by the NordVPN API.

8. **Typed JSON state storage**

   Runtime persistence is handled by `JsonStateStorage`. Application services work with typed models instead of manually serializing and deserializing JSON. The storage also understands the previous state-file format, so older local data can still be read.

9. **Asynchronous API calls**

   HTTP calls now use `async`/`await`. Blocking calls such as `.Result` were removed.

10. **Cleaner strings**

    Plain string concatenation was replaced with interpolation where it made the code easier to read, for example `$"Total servers: {servers.Count}"`.

11. **Tests**

    The `party.cli.Tests` project covers the main application paths: services, CLI handlers, configuration loading, state storage, and the NordVPN repository. It also checks combined country and protocol filtering.

12. **Removed unnecessary pieces**

    The `config` command was removed because runtime state is now managed by a dedicated storage component. The manually maintained `AssemblyInfo.cs` file was also removed, with version metadata kept in the SDK-style project file instead.

### Before and After

- Instead of keeping most logic in `Program.cs`, the file now only starts the application.
- Instead of manual argument parsing, commands are handled by `System.CommandLine`.
- Instead of a hardcoded API URL, the endpoint is stored in `appsettings.json`.
- Instead of mixing configuration with runtime data, the application uses `appsettings.json` and `partycli-state.json`.
- Instead of exposing country and protocol IDs in the CLI, users provide names such as `france` and `TCP`.
- Instead of serializing JSON directly in services, state persistence is handled by `JsonStateStorage`.
- Instead of blocking HTTP calls, the code uses `async`/`await`.

### Result

The application keeps the same core behavior, but the code is no longer concentrated in one file. Adding a new filter, changing the API, replacing storage, or adjusting console output can now be done in focused parts of the codebase.
