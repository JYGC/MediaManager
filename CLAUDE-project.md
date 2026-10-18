# CLAUDE-project.md

Project-specific guidance for OffPeakMediaFetcher. General workflow, architecture, and testing rules live in [CLAUDE.md](CLAUDE.md).

## What is OffPeakMediaFetcher

OffPeakMediaFetcher downloads videos (currently YouTube) during off-peak hours and manages the resulting media library. The solution ships two executables that share a common set of class libraries:

- **`OffPeakMediaFetcher`** — a .NET 8 console app invoked by Windows Task Scheduler. Takes one of two arguments: `metadata` (fetch new video metadata from tracked channels via the YouTube Data API) or `videos` (download queued videos via `youtube-dl` with `yt-dlp` as fallback, wrapped through NYoutubeDL).
- **`MediaManagerUI`** — a .NET 8 WPF + Blazor Hybrid desktop app (MudBlazor components hosted in WebView2) for browsing the library, managing tracked channels, viewing logs, and configuring scheduled tasks.

Persistence is a local LiteDB (embedded NoSQL) database. There is no server component.

## Architecture

```
OffPeakMediaFetcher/          API — Task Scheduler CLI (videos | metadata); Program.Main only composes
MediaManagerUI/               API — WPF host; Program, App and MainWindow only compose
MediaManagerUI.Components/    API — Razor class library: layout, pages, shared components, modules (MudBlazor)

MediaManager.Initializations/ Application — use cases, entry-point use case, Application DTO converter
MediaManager.Services2/       Service — ChannelServices, MetadataServices, LogServices, TaskRunnerServices,
                              ChannelMetadataServices, NewVideoMetadataFetchServices, VideoDownloadServices
MediaManager.Repositories/    Repository — the only code that turns Store models into Repository DTOs

OPMF.Database/                Store — LiteDB collections (YoutubeMetadata, YoutubeChannel, OPMFLog)
OPMF.Logging/                 Store — database log writer
OPMF.SiteAdapter/             Store — abstract video-site adapter
OPMF.SiteAdapter.Youtube/     Store — YouTube Data API (Google.Apis.YouTube.v3)
OPMF.Downloader/              Store — abstract downloader interface
OPMF.Downloader.YTDownloader/ Store — youtube-dl / yt-dlp downloader (NYoutubeDL)

OPMF.Entities/                Domain — database models (Metadata, Channel, OPMFLog)
MediaManager.Dtos/            DTO — Repository, Service and Application DTOs (one namespace each)

OPMF.Settings/                Cross-cutting — config file and settings
OPMF.Filesystem/              Cross-cutting — folder layout and creation of missing folders
OPMF.TextLogging/             Cross-cutting — text-file log

tests/
  OPMF.UnitTests/             xUnit — every class in isolation, through seams; bUnit for Razor components
  OPMF.ContractTests/         xUnit — boundary contracts and the layered-architecture checks
  OPMF.IntegrationTests/      xUnit — repositories and services against real LiteDB
  OPMF.E2ETests/              xUnit — drives the OffPeakMediaFetcher executable end-to-end
```

### Layer mapping

| Layer | Projects |
|---|---|
| **API** | `OffPeakMediaFetcher/`, `MediaManagerUI/`, `MediaManagerUI.Components/` |
| **Application** | `MediaManager.Initializations/` |
| **Service** | `MediaManager.Services2/` |
| **Repository** | `MediaManager.Repositories/` |
| **Store** | `OPMF.Database/`, `OPMF.Logging/`, `OPMF.SiteAdapter/`, `OPMF.SiteAdapter.Youtube/`, `OPMF.Downloader/`, `OPMF.Downloader.YTDownloader/` |
| **Domain** | `OPMF.Entities/` |
| **DTO** | `MediaManager.Dtos/` |
| **Cross-cutting** | `OPMF.Settings/`, `OPMF.Filesystem/`, `OPMF.TextLogging/` |

`tests/OPMF.ContractTests/LayeredArchitectureContractTests.cs` holds this map in code and fails the build on a violation. Adding a project means adding it to that map.

### Rules the architecture tests enforce

- **Direction.** API uses Application; Application uses Service; Service uses Repository; Repository uses Store and Domain. A layer never uses a layer above it or skips one. Every layer may use DTOs and cross-cutting projects. A layer reaches the Store only through the layer below it, and the Domain only from Repository downwards.
- **DTOs sit on the boundaries.** API and Application exchange `MediaManager.Dtos.Application`; Application and Service exchange `…Services`; Service and Repository exchange `…Repositories`. A layer uses only the DTO namespaces of the boundaries directly above and below it, and converts between them.
- **Database models stay below the Repositories.** Only Repository and Store code uses types from `OPMF.Entities`.
- **Cross-cutting projects** (`OPMF.Settings`, `OPMF.Filesystem`, `OPMF.TextLogging`) reference no layer.
- **No static state.** No production type has a non-`readonly` static field.

### Composition and testability

- Each layer registers its own services in an `Add…Layer` extension method (`AddMediaManagerApplicationLayer`, `AddMediaManagerServiceLayer`, `AddMediaManagerRepositoryLayer`). The only composition roots are `Program.Main` of the CLI, `Program.Main` and `App` of the UI host, and the `MainWindow` constructor.
- Nothing outside a composition root or a thin adapter touches the file system, console, network, process, thread or clock directly. Use the seams instead: `TimeProvider`, a `TextWriter` for standard output, `IDatabaseConnectionOpener`, and the downloader interfaces (`IUrlToFileDownloader`, `IVideoDownloaderExecutableRunner`, `IInterruptibleDownloadThreadStarter`, `ICurrentThreadWaiter`, `IConsoleRowOverwriter`), `IYoutubeDataApi`, `IExternalProcessStarter`, `IMissingFolderCreator`.
- The thin adapters that only forward to those APIs (`WebClientUrlToFileDownloader`, `NYoutubeDLVideoDownloaderExecutableRunner`, `InterruptibleDownloadThreadStarter`, `ThreadSleepCurrentThreadWaiter`, `ConsoleRowOverwriter`, `GoogleSignIn`, `YoutubeDataApi`, `ExternalProcessStarter`) and the composition roots are exempt from unit tests.

### Downloader

`YTDownloader` downloads each video with the executable named by `ExecutableTriedFirstToDownloadVideos` in the config file (`youtube-dl` or `yt-dlp`). When that attempt fails, it tries the other executable for the same video (with `--no-continue`) and logs a warning. Both executables are downloaded into the application folder when missing, so nothing has to be on `PATH`.

## Development rules

- LiteDB has no schema migrations; structural changes to stored documents must include a backfill/upgrade path executed at startup — never mutate the `.db` file by hand.
- Empty `MediaManager.*` folders on disk (`Models`, `Services`, `Database`, `Database.Migration`, `Logging`, `TaskRunner`, `Types`) are placeholders, not invitations to scaffold layers without a concrete use.
- When adding a new `.csproj`, register it in `OffPeakMediaFetcher.sln` (including all six build configurations: Debug/Release × Any CPU/x64/x86) and update the architecture diagram in this file.
- Format C# with `dotnet format` before committing.
- Code style: [Microsoft C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions), [.NET runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md). Razor components in `MediaManagerUI.Components/` follow the [ASP.NET Core Razor conventions](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor).

## Testing

### Test types

| Type | Project | Scope | When required |
|---|---|---|---|
| **Unit** | `tests/OPMF.UnitTests/` | Single class or method in isolation, through its seams | Always — written first |
| **Contract** | `tests/OPMF.ContractTests/` | Boundary contracts: database collections, downloader interface, site-adapter interface, repository / service / application interfaces, and the layered-architecture checks — verifies the shape stays stable | When adding or changing a public interface, LiteDB collection, or external-API integration |
| **Integration** | `tests/OPMF.IntegrationTests/` | Multiple components together (e.g. service + database, service + downloader) against real LiteDB | Always for non-trivial cross-component interactions |
| **E2E** | `tests/OPMF.E2ETests/` | Drives the built `OffPeakMediaFetcher.exe` end-to-end with real arguments | Always for changes to the CLI dispatch or scheduled-task behaviour |

### Conventions

- Framework: **xUnit 2.6.6** across all test projects, with `coverlet.collector` for coverage.
- All test projects live under `tests/` — there are no in-source test files alongside production code.
- Integration tests use a real LiteDB instance backed by a per-test temporary file; **do not mock LiteDB**.
- Unit tests touch no real file, console, network, process, thread or clock: substitute the seams (`FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`, a `StringWriter`, fakes of the downloader and API interfaces). Code that needs a database uses an in-memory `LiteDatabase` through the `InMemoryDatabaseConnectionOpener` test support class. The only exemptions are the thin adapters and composition roots listed above.
- Razor components are unit-tested with bUnit 1.40.0 and MudBlazor 8.0.0 (add `MudPopoverProvider` / `MudDialogProvider` for pickers and dialogs).
- Contract tests should fail loudly when an interface or stored-document shape changes — that is the signal to update consumers, not the test.

### Commands

```sh
dotnet test                                       # all test projects
dotnet test tests/OPMF.UnitTests                  # one project
dotnet test --filter FullyQualifiedName~ChannelServices   # one class/method
dotnet test --collect:"XPlat Code Coverage"       # with coverage
```

## Build and run

Run from the solution root:

```sh
dotnet restore                                    # restore NuGet packages
dotnet build OffPeakMediaFetcher.sln              # build all projects
dotnet build -c Release OffPeakMediaFetcher.sln   # release build
dotnet format                                     # apply code style
dotnet test                                       # run all tests
```

### Running the CLI (Task Scheduler entry point)

```sh
dotnet run --project OffPeakMediaFetcher -- metadata   # fetch new YouTube metadata
dotnet run --project OffPeakMediaFetcher -- videos     # download queued videos
```

In production these are invoked by Windows Task Scheduler against the published `OffPeakMediaFetcher.exe`.

### Running the GUI

```sh
dotnet run --project MediaManagerUI                    # launch the WPF + Blazor app
```

`MediaManagerUI` targets `net8.0-windows` (WPF + WebView2) and only builds on Windows; `MediaManagerUI.Components` targets `net8.0`.

## Tech stack — quick reference

- **Runtime:** .NET 8.0 (`net8.0` for libraries and the CLI, `net8.0-windows` for the WPF UI)
- **Database:** LiteDB 5.0.15 (embedded, file-backed)
- **YouTube metadata:** Google.Apis.YouTube.v3 1.58.0.2874
- **Video download:** NYoutubeDL 0.11.2 (wraps `youtube-dl` and `yt-dlp`, which the app downloads itself when missing)
- **UI:** WPF host + `Microsoft.AspNetCore.Components.WebView.Wpf` 8.0.100 + MudBlazor 8.0.0 (Razor class library `MediaManagerUI.Components`)
- **Dependency injection:** Microsoft.Extensions.DependencyInjection 8.0
- **JSON:** Newtonsoft.Json 13.0.2
- **Tests:** xUnit 2.6.6 + coverlet.collector 6.0.0 + bUnit 1.40.0 + Microsoft.Extensions.TimeProvider.Testing 8.10.0 (unit tests)
