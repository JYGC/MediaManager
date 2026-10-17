# CLAUDE-project.md

Project-specific guidance for OffPeakMediaFetcher. General workflow, architecture, and testing rules live in [CLAUDE.md](CLAUDE.md).

## What is OffPeakMediaFetcher

OffPeakMediaFetcher downloads videos (currently YouTube) during off-peak hours and manages the resulting media library. The solution ships two executables that share a common set of class libraries:

- **`OffPeakMediaFetcher`** — a .NET 8 console app invoked by Windows Task Scheduler. Takes one of two arguments: `metadata` (fetch new video metadata from tracked channels via the YouTube Data API) or `videos` (download queued videos via `yt-dlp`, wrapped through NYoutubeDL).
- **`MediaManagerUI`** — a .NET 8 WPF + Blazor Hybrid desktop app (MudBlazor components hosted in WebView2) for browsing the library, managing tracked channels, viewing logs, and configuring scheduled tasks.

Persistence is a local LiteDB (embedded NoSQL) database. There is no server component.

## Architecture

```
OffPeakMediaFetcher/        Entry point — Task Scheduler CLI (videos | metadata)
MediaManagerUI/             Entry point — WPF + Blazor Hybrid GUI (MudBlazor)

OPMF.Entities/              Domain models (Metadata, Channel, OPMFError)
OPMF.Settings/              JSON-backed config / settings access
OPMF.Filesystem/            Folder layout and path helpers
OPMF.Logging/               Logger abstraction (facade)
OPMF.TextLogging/           Text-file logger implementation
OPMF.Database/              LiteDB collections (YoutubeMetadata, YoutubeChannel, OPMFLog)
OPMF.SiteAdapter/           Abstract video-site adapter
OPMF.SiteAdapter.Youtube/   YouTube adapter using Google.Apis.YouTube.v3
OPMF.Downloader/            Abstract downloader interface
OPMF.Downloader.YTDownloader/  Concrete downloader using NYoutubeDL (yt-dlp wrapper)

MediaManager.Services2/     New service layer — ChannelServices, MetadataServices,
                            LogServices, TaskRunnerServices, ChannelMetadataServices

tests/
  OPMF.UnitTests/           xUnit — isolated units (entities, settings)
  OPMF.ContractTests/       xUnit — boundary contracts (database, downloader, site adapter)
  OPMF.IntegrationTests/    xUnit — multi-component flows including Services2
  OPMF.E2ETests/            xUnit — drives the OffPeakMediaFetcher executable end-to-end
```

### Key patterns

The codebase is being migrated from the flat `OPMF.*` libraries toward the layered architecture defined in [CLAUDE.md — Layered Architecture](CLAUDE.md#layered-architecture). New code must follow the target patterns; existing `OPMF.*` code is updated incrementally.

Several `MediaManager.*` folders exist on disk (`MediaManager.Models`, `MediaManager.Dtos`, `MediaManager.Repositories`, `MediaManager.Services`, `MediaManager.Database`, `MediaManager.Database.Migration`, `MediaManager.Logging`, `MediaManager.TaskRunner`, `MediaManager.Types`, `MediaManager.Initializations`) but currently contain no source files and no `.csproj` — they are reserved slots for the target layered design. **Only `MediaManager.Services2` is wired into the solution today.** When introducing one of the reserved layers, add the `.csproj`, register it in `OffPeakMediaFetcher.sln`, and update this document.

**Legacy → target mapping (current state):**

- The two executables (`OffPeakMediaFetcher`, `MediaManagerUI`) act as the API layer — they parse args / handle UI events and delegate downward.
- Business logic that has already been moved lives in `MediaManager.Services2/`. Logic still in `OPMF.Downloader.YTDownloader/`, `OPMF.SiteAdapter.Youtube/`, etc. should be migrated when touched.
- Persistence currently lives directly in `OPMF.Database/` (LiteDB collections used by services). The target is to hide LiteDB behind `MediaManager.Repositories/`; until then, treat `OPMF.Database` as the Store layer.

### Layer target mapping

| Layer | OffPeakMediaFetcher target |
|---|---|
| **API** | `OffPeakMediaFetcher/` (CLI), `MediaManagerUI/` (Razor components + WPF host) |
| **Application** | `MediaManager.Initializations/` and equivalent orchestration code — to be introduced |
| **Service** | `MediaManager.Services2/` (and any future `MediaManager.Services/`) |
| **Repository** | `MediaManager.Repositories/` — to be introduced |
| **Store** | LiteDB (currently via `OPMF.Database/`), YouTube Data API (`OPMF.SiteAdapter.Youtube/`), yt-dlp (`OPMF.Downloader.YTDownloader/`) |
| **Domain** | `OPMF.Entities/` (target: `MediaManager.Models/`) |
| **DTO** | Target: `MediaManager.Dtos/` |

## Development rules

- LiteDB has no schema migrations; structural changes to stored documents must include a backfill/upgrade path executed at startup (target: `MediaManager.Database.Migration/`) — never mutate the `.db` file by hand.
- The empty `MediaManager.*` folders are placeholders, not invitations to scaffold layers without a concrete use.
- When adding a new `.csproj`, register it in `OffPeakMediaFetcher.sln` (including all six build configurations: Debug/Release × Any CPU/x64/x86) and update the architecture diagram in this file.
- Format C# with `dotnet format` before committing.
- Code style: [Microsoft C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions), [.NET runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md). Razor components in `MediaManagerUI/` follow the [ASP.NET Core Razor conventions](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor).

## Testing

### Test types

| Type | Project | Scope | When required |
|---|---|---|---|
| **Unit** | `tests/OPMF.UnitTests/` | Single class or method in isolation | Always — written first |
| **Contract** | `tests/OPMF.ContractTests/` | Boundary contracts: database collections, downloader interface, site-adapter interface — verifies the shape stays stable | When adding or changing a public interface, LiteDB collection, or external-API integration |
| **Integration** | `tests/OPMF.IntegrationTests/` | Multiple components together (e.g. service + database, service + downloader) against real LiteDB | Always for non-trivial cross-component interactions |
| **E2E** | `tests/OPMF.E2ETests/` | Drives the built `OffPeakMediaFetcher.exe` end-to-end with real arguments | Always for changes to the CLI dispatch or scheduled-task behaviour |

### Conventions

- Framework: **xUnit 2.6.6** across all test projects, with `coverlet.collector` for coverage.
- All test projects live under `tests/` — there are no in-source test files alongside production code.
- Integration tests use a real LiteDB instance backed by a per-test temporary file; **do not mock LiteDB**.
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

`MediaManagerUI` targets `net8.0-windows` (WPF + WebView2 + MudBlazor) and only builds on Windows.

## Tech stack — quick reference

- **Runtime:** .NET 8.0 (`net8.0` for libraries and the CLI, `net8.0-windows` for the WPF UI)
- **Database:** LiteDB 5.0.15 (embedded, file-backed)
- **YouTube metadata:** Google.Apis.YouTube.v3 1.58.0.2874
- **Video download:** NYoutubeDL 0.11.2 (wraps `yt-dlp`; `yt-dlp` must be on `PATH` at runtime)
- **UI:** WPF host + `Microsoft.AspNetCore.Components.WebView.Wpf` 8.0.100 + MudBlazor 8.0.0
- **JSON:** Newtonsoft.Json 13.0.2
- **Tests:** xUnit 2.6.6 + coverlet.collector 6.0.0
