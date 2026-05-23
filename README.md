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

The original version of the application kept most of its logic inside `Program.cs`. A single class was responsible for argument parsing, calling the NordVPN API, saving configuration, logging actions, deserializing JSON, and printing results to the console. This structure made the application harder to extend, harder to test, and harder to adapt to future requirements.

The goal of the refactoring was to introduce a lightweight Clean Architecture-inspired structure, preserve the existing CLI behavior, and prepare the codebase for further development.

### Main Changes

1. **Simplified `Program.cs`**

   `Program.cs` now uses a classic structure with a `Main` method. The `Main` method no longer contains business logic. Its only responsibility is to build the dependency injection container, resolve the root CLI command, and invoke the command-line parser.

2. **Introduced Application Layers**

   The code was split into several logical areas:

   - `Domain` - domain models such as `Server`, `LogEntry`, and `VpnServerQuery`.
   - `Application` - application interfaces and services such as `ServerService`, `ConfigService`, and `LoggerService`.
   - `Infrastructure` - technical implementations such as the NordVPN API client, configuration loading, and settings storage.
   - `Presentation/Cli` - CLI command definitions, command handlers, and console output.

3. **Replaced Manual Argument Parsing with `System.CommandLine`**

   The manual state machine based on `if` statements and direct iteration over `args` was replaced with `System.CommandLine`. This gives the application clearer command definitions, built-in `--help` support, validation, and a better foundation for future CLI growth.

   Existing commands were preserved:

   - `partycli server_list`
   - `partycli server_list --france`
   - `partycli server_list --TCP`
   - `partycli server_list --local`

   A more extensible command structure was also added:

   - `partycli servers list`
   - `partycli servers list --country france`
   - `partycli servers list --protocol TCP`
   - `partycli servers list --local`

4. **Added Dependency Injection**

   The project now uses `Microsoft.Extensions.DependencyInjection`. Dependency registration was moved to `DependencyInjectionConfig`. The container creates application services, repositories, configuration objects, CLI handlers, and the root `RootCommand`.

   This makes classes depend on abstractions instead of concrete implementations and improves testability.

5. **Migrated from `App.config` and `Properties.Settings` to JSON files**

   The old settings mechanism based on `App.config`, `Settings.settings`, and `Settings.Designer.cs` was removed. Static application configuration is now stored in `appsettings.json`, while runtime state is stored separately in `partycli-state.json`.

   `appsettings.json` contains:

   - `nordVpn.serversEndpoint` - the NordVPN API endpoint.

   `partycli-state.json` contains:

   - `serverlist` - the stored server list,
   - `log` - stored application logs.

6. **Moved Endpoints to Configuration**

   The `https://api.nordvpn.com/v1/servers` URL is no longer hardcoded in the repository class. It was moved to `appsettings.json`, and `NordVpnServerRepository` receives it through its constructor. Changing the API endpoint no longer requires modifying repository code.

7. **Separated Responsibilities**

   The logic was split into smaller classes:

   - `NordVpnServerRepository` handles only NordVPN API communication.
   - `NordVpnServerFilterCatalog` maps user-friendly filter names to NordVPN API identifiers.
   - `SettingsStorage` handles only JSON runtime-state persistence.
   - `ServerService` handles server-list use cases.
   - `LoggerService` handles action logging.
   - `ConfigService` handles configuration updates.
   - `CliCommands` defines the CLI command structure.
   - `ServerListCommandHandler` and `ConfigCommandHandler` handle user actions.
   - `ConsoleOutput` centralizes console output.

8. **Introduced Asynchronous API Calls**

   API calls now use `async`/`await`. Blocking calls such as `.Result` were removed, which improves scalability and avoids common async-related issues.

9. **Improved String Readability**

   String concatenation was replaced with string interpolation, for example `$"Total servers: {servers.Count}"`. This makes messages easier to read and maintain.

10. **Added Tests**

    The repository contains a `party.cli.Tests` project with tests for the application, infrastructure, and presentation layers. The tests cover application services, CLI handlers, configuration loading, settings storage, and the NordVPN repository.

11. **Removed Legacy Project Metadata**

    The SDK-style project no longer keeps manual assembly metadata in `AssemblyInfo.cs`. Version metadata is now defined in the project file, which reduces legacy project scaffolding.

### Before and After

| Before Refactoring | After Refactoring |
| --- | --- |
| Most logic lived in `Program.cs`. | `Program.cs` only starts the application. |
| Arguments were parsed manually. | Arguments are parsed by `System.CommandLine`. |
| The API endpoint was hardcoded. | The endpoint is stored in `appsettings.json`. |
| Configuration used `App.config` and `Properties.Settings`. | Configuration uses `appsettings.json`, while persisted runtime state uses `partycli-state.json`. |
| Responsibilities were mixed together. | Code is split into `Domain`, `Application`, `Infrastructure`, and `Presentation`. |
| Dependencies were created directly in `Program.cs`. | Dependencies are registered in a DI container. |
| HTTP calls used blocking `.Result`. | HTTP calls use `async`/`await`. |
| Testing individual parts was harder. | Classes depend on interfaces and are easier to test. |

### Final Result

The application keeps its original behavior while becoming more modular, readable, and ready for future changes. Adding a new command, replacing the storage provider, changing the API, or modifying console output can now be done in focused, well-separated parts of the codebase instead of one large `Main` method.
