# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Changes (spec-driven work)

> This approach is based on [Kiro's spec methodology](https://kiro.dev/docs/specs/). The [Requirements-First workflow](https://kiro.dev/docs/specs/feature-specs/requirements-first/) is the standard used here: specify system behaviour before making architectural decisions.

Non-trivial features and bug fixes are tracked as a **change** — a folder at `.claude/changes/<change-name>/` containing up to three spec files. Use specs for anything complex, costly to get wrong, or requiring iterative design. Skip specs for exploratory/prototype work.

### Spec files

**`requirements.md`** — the *what*. Organise by feature area (H2) and user story group (H3). Each requirement uses EARS notation:

```
WHEN <condition> THE SYSTEM SHALL <action>
```

Example:
```
## Channel Management

### Add channel
WHEN a user submits a valid new-channel form THE SYSTEM SHALL create the channel record and queue an initial metadata fetch.
WHEN a user submits a channel URL that already exists THE SYSTEM SHALL display a "Channel already tracked" error.
```

Also cover edge cases and error-handling scenarios.

**`design.md`** — the *how*. Sections: system architecture and components, sequence diagrams, data models and interfaces, error-handling approach, testing strategy.

**`tasks.md`** — the *steps*. Discrete, trackable implementation tasks, each with a clear description, expected outcome, and any dependencies. Mark tasks required vs optional. Work through independent tasks first, then dependent ones in order.

**`bugfix.md`** — replaces `requirements.md` for bug fixes. Three sections using their own notation:

```
## Current Behavior (Defect)
WHEN <condition> THEN the system <incorrect behavior>

## Expected Behavior (Correct)
WHEN <condition> THEN the system SHALL <correct behavior>

## Unchanged Behavior (Regression Prevention)
WHEN <condition> THEN the system SHALL CONTINUE TO <existing behavior>
```

The "Unchanged Behavior" section is the key addition — explicitly locking down what must not change prevents regressions. The `design.md` for a bugfix includes root cause analysis; `tasks.md` includes tests that verify the bug is fixed and unchanged behavior is preserved.

### Workflow

**Feature:**
1. Create `requirements.md` and agree on it before writing `design.md`.
2. Create `design.md` and agree on it before writing `tasks.md`.
3. Execute `tasks.md` one task at a time, marking each done as you go.

**Bugfix:**
1. Create `bugfix.md` (current / expected / unchanged behavior) and agree on it.
2. Create `design.md` including root cause analysis.
3. Create and execute `tasks.md`, including tests for fix and regression prevention.

Before starting any non-trivial feature, refactor, or bug fix, check `.claude/changes/` for an existing change folder. If none exists, create one and start with `requirements.md` (feature) or `bugfix.md` (bug).

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

The codebase is being migrated from the flat `OPMF.*` libraries toward the layered architecture defined in [Layered Architecture](#layered-architecture) below. New code must follow the target patterns; existing `OPMF.*` code is updated incrementally.

Several `MediaManager.*` folders exist on disk (`MediaManager.Models`, `MediaManager.Dtos`, `MediaManager.Repositories`, `MediaManager.Services`, `MediaManager.Database`, `MediaManager.Database.Migration`, `MediaManager.Logging`, `MediaManager.TaskRunner`, `MediaManager.Types`, `MediaManager.Initializations`) but currently contain no source files and no `.csproj` — they are reserved slots for the target layered design. **Only `MediaManager.Services2` is wired into the solution today.** When introducing one of the reserved layers, add the `.csproj`, register it in `OffPeakMediaFetcher.sln`, and update this document.

**Legacy → target mapping (current state):**

- The two executables (`OffPeakMediaFetcher`, `MediaManagerUI`) act as the API layer — they parse args / handle UI events and delegate downward.
- Business logic that has already been moved lives in `MediaManager.Services2/`. Logic still in `OPMF.Downloader.YTDownloader/`, `OPMF.SiteAdapter.Youtube/`, etc. should be migrated when touched.
- Persistence currently lives directly in `OPMF.Database/` (LiteDB collections used by services). The target is to hide LiteDB behind `MediaManager.Repositories/`; until then, treat `OPMF.Database` as the Store layer.

## Layered Architecture

All code must follow a layered architecture with clear separation of concerns. Each layer may only depend on the layer directly below it.

| Layer | Responsibility |
|---|---|
| **API** | Entry-point handlers (CLI argument dispatch, UI event handlers), request/response mapping, input validation |
| **Application** | Use-case orchestration; coordinates services without containing business logic |
| **Service** | Business logic and domain rules |
| **Repository** | Data access abstraction; hides persistence details from services |
| **Store** | Persistence (LiteDB collections, external API clients) |

Optional: **Domain** (pure entities and value objects, no dependencies), **DTO/Schema** (typed data transfer objects at layer boundaries).

**Target mapping:**

| Layer | OffPeakMediaFetcher target |
|---|---|
| **API** | `OffPeakMediaFetcher/` (CLI), `MediaManagerUI/` (Razor components + WPF host) |
| **Application** | `MediaManager.Initializations/` and equivalent orchestration code — to be introduced |
| **Service** | `MediaManager.Services2/` (and any future `MediaManager.Services/`) |
| **Repository** | `MediaManager.Repositories/` — to be introduced |
| **Store** | LiteDB (currently via `OPMF.Database/`), YouTube Data API (`OPMF.SiteAdapter.Youtube/`), yt-dlp (`OPMF.Downloader.YTDownloader/`) |
| **Domain** | `OPMF.Entities/` (target: `MediaManager.Models/`) |
| **DTO** | Target: `MediaManager.Dtos/` |

### References

1. [Microsoft Learn — Infrastructure Persistence Layer Design](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
2. [From Request to Database: Three-Layer Architecture in API Development](https://konstantinmb.medium.com/from-request-to-database-understanding-the-three-layer-architecture-in-api-development-1c44c973c7af)
3. [Repository Pattern with Layered Architecture](https://medium.com/@leadcoder/repository-pattern-with-layered-architecture-35f7b9182ebf)
4. [Layered Architecture Template for REST APIs (Java/Spring Boot)](https://kamilmazurek.pl/layered-architecture-template)
5. [Service–Repository Pattern in Action](https://medium.com/@albinaji.official/service-repository-pattern-in-action-0db4bb9a474b)
6. [Repository and Services Pattern in a Multilayered Architecture](https://www.vodovnik.com/repository-and-services-pattern-in-a-multilayered-architecture/)
7. [Clean Architecture — Incorporating Repository Pattern](https://medium.com/@bert.oneill/clean-architecture-incorporating-repository-pattern-388742e0b54e)
8. [Unpacking Clean Architecture Layers — Domain, Application, Infrastructure Services](https://www.dandoescode.com/blog/unpacking-the-layers-of-clean-architecture-domain-application-and-infrastructure-services)
9. [Clean Architecture: Understanding the Infrastructure and Persistence Layers](https://dev.to/moh_moh701/what-is-clean-architecture-understanding-the-infrastructure-and-persistence-layers-2pca)
10. [clean-architecture-api-boilerplate (GitHub)](https://github.com/luizomf/clean-architecture-api-boilerplate/blob/master/README.md)
11. [Clean Architecture Design Guide for Backend API Developments](https://naskay.com/blog/clean-architecture-design-guide-for-backend-api-developments/)

## Development rules

- Always read a file before editing it.
- LiteDB has no schema migrations; structural changes to stored documents must include a backfill/upgrade path executed at startup (target: `MediaManager.Database.Migration/`) — never mutate the `.db` file by hand.
- No speculative abstractions — only build what is needed now. The empty `MediaManager.*` folders are placeholders, not invitations to scaffold layers without a concrete use.
- When adding a new `.csproj`, register it in `OffPeakMediaFetcher.sln` (including all six build configurations: Debug/Release × Any CPU/x64/x86) and update the architecture diagram in this file.
- Format C# with `dotnet format` before committing.
- Code style: [Microsoft C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions), [.NET runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md). Razor components in `MediaManagerUI/` follow the [ASP.NET Core Razor conventions](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor).

## Testing

### Mandate

**Unit tests must be written before the implementation code they cover (TDD).** Write the test, watch it fail, then write the minimum code to make it pass.

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

### In specs

`tasks.md` must include explicit test tasks. For features: unit tests as the first task for each class, followed by contract / integration / E2E tasks as the surface area demands. For bugfixes: tasks must include a test that reproduces the bug before it is fixed, plus regression tests drawn from `bugfix.md`'s "Unchanged Behavior" section.

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
