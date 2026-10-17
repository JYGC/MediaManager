# Align the repository with the layered architecture

Goal: make the code match the layer chain in `CLAUDE.md` (API → Application → Service → Repository → Store, each layer depending only on the one directly below) and the target mapping in `CLAUDE-project.md`, and bring the code in line with the self-documenting-code and TDD rules in `CLAUDE.md`.

Baseline measured before any change: `dotnet build OffPeakMediaFetcher.sln` → 0 errors; `dotnet test` → 128 passed (49 contract, 42 unit, 35 integration, 2 E2E).

## Decisions needed before design

Each row has a recommended default. The requirements below are written assuming the defaults; tell me which to change.

| # | Question | Recommended default | Alternative and its cost |
|---|---|---|---|
| D1 | Must the UI go through the Application layer for every operation, or only where orchestration exists? | **Strict.** API depends on Application only. Every UI/CLI operation has an Application use case; some will be thin pass-throughs. This is the literal reading of the `CLAUDE.md` rule. | **Pragmatic.** UI modules keep calling `MediaManager.Services2` directly for plain reads/updates. Application is used only for startup, the CLI `metadata`/`videos` flows and download-now. Fewer classes, but a documented exception to the rule. |
| D2 | Rename the Domain project `OPMF.Entities` → `MediaManager.Models` and add `MediaManager.Dtos` now? | **No. Keep `OPMF.Entities`; add no DTO project in this change.** A rename breaks every existing database (see "Verified risk" below). No boundary needs a DTO today (`ChannelServices.GetAll` copies a `Channel` into an identical `Channel`). | **Rename now,** with a startup migration that rewrites `_type` in stored documents. Touches every project and test, adds a one-way migration (older builds can no longer read the migrated file). |
| D3 | What is the status of `OPMF.Settings`, `OPMF.Filesystem`, `OPMF.Logging`, `OPMF.TextLogging`? They are not in the mapping. | **Cross-cutting infrastructure,** referenceable from any layer, documented as such in `CLAUDE-project.md`. Only unused and upward references are removed. | **Strict.** Store clients may not log or read settings; they report failures upward. That changes how `IDownloader` / `IMetadataFetcher` behave, which the contract tests freeze. |
| D4 | How far does the self-documenting-code sweep go? | **All production code,** as a final phase after the structural moves so the diffs stay reviewable. Public members covered by contract tests keep their names. | **Only code created or moved by this change.** |

Assumptions (veto any of these):
- A1. Same-layer references are allowed (for example `MetadataServices` → `ITaskRunnerServices`). The rule restricts reaching past the layer directly below.
- A2. Test projects are exempt from the layer rules. They set up data through Store types today.
- A3. The contract tests are frozen. Per `CLAUDE-project.md`, a contract failure means updating consumers, not the test, so no contract-tested public interface changes shape.

### Verified risk behind D2

LiteDB stores the CLR type name for any document whose runtime type differs from the collection's declared type. `YoutubeMetadataFetcher` inserts `YoutubeMetadata` / `YoutubeChannel` instances into base-typed collections, so existing databases contain `_type: "OPMF.Entities.YoutubeMetadata, OPMF.Entities"`. I reproduced this with a throwaway LiteDB 5.0.15 project. A document with an unresolvable `_type` fails on read with `LiteException: Type 'OPMF.Entities.YoutubeMetadata, OPMF.Entities' not found in current domain`. So the namespace and assembly name `OPMF.Entities` are part of the stored-data contract.

## Current state vs. target

| Layer | Target | Today |
|---|---|---|
| API | `OffPeakMediaFetcher`, `MediaManagerUI` | CLI `VideoFetcher` / `MetadataFetcher` contain orchestration and business logic (file moving, queue selection, status flagging) and use `DatabaseAdapter`, `YTDownloader` and `YoutubeMetadataFetcher` directly. UI `Program` / `MainWindow` call Settings, Filesystem, Logging and TextLogging directly. UI modules call Service-layer types directly. |
| Application | `MediaManager.Initializations` (to be introduced) | Does not exist. The startup sequence (settings → folders → config) is duplicated in both `Program.cs` files. |
| Service | `MediaManager.Services2` | Calls the static `DatabaseAdapter.AccessDbAdapter` (Store) directly. `ChannelMetadataServices.GetVideoByUrl` instantiates `YoutubeVideoMetadataGetter` (a Store client) directly. |
| Repository | `MediaManager.Repositories` (to be introduced) | Does not exist. |
| Store | `OPMF.Database`, `OPMF.SiteAdapter.Youtube`, `OPMF.Downloader.YTDownloader` | Present. `OPMF.SiteAdapter.Youtube` references `OPMF.Database` without using it. `OPMF.Filesystem` references seven projects and uses none of them directly. |
| Domain | `OPMF.Entities` | Present (see D2). |

## Requirements

### Layer dependencies

- WHEN a project in the API layer declares a project reference THE SYSTEM SHALL reference only Application-layer projects, Domain, and cross-cutting infrastructure.
- WHEN a project in the Application layer declares a project reference THE SYSTEM SHALL reference only Service-layer projects, Domain, and cross-cutting infrastructure.
- WHEN a project in the Service layer declares a project reference THE SYSTEM SHALL reference only Repository-layer projects, Domain, and cross-cutting infrastructure.
- WHEN a project in the Repository layer declares a project reference THE SYSTEM SHALL reference only Store-layer projects, Domain, and cross-cutting infrastructure.
- WHEN a project in the Store layer declares a project reference THE SYSTEM SHALL NOT reference any API, Application, Service or Repository project.
- WHEN a project declares a project reference that its code does not use THE SYSTEM SHALL NOT keep that reference. Known cases: `OPMF.SiteAdapter.Youtube` → `OPMF.Database`, and all seven current references of `OPMF.Filesystem` (it only uses `OPMF.Settings`, which it reaches transitively today and will reference directly).
- WHEN the architecture contract test runs THE SYSTEM SHALL fail if any non-test assembly references an assembly in a layer other than its own, the one directly below, Domain, or cross-cutting infrastructure.

### Application layer (`MediaManager.Initializations`)

#### Startup
- WHEN either executable starts THE SYSTEM SHALL run read-only-settings initialization, application-folder establishment and config establishment, in that order, through one Application-layer entry point shared by both executables.
- WHEN startup initialization throws THE SYSTEM SHALL record an `OPMFError` log entry and write the exception message to standard output, as today.

#### Fetch metadata (`metadata`)
- WHEN the metadata use case runs THE SYSTEM SHALL load every channel that is not blacklisted, fetch new metadata for those channels, insert only metadata that is not already stored, and update each channel's `LastCheckedOut` and `LastActivityDate`.
- WHEN the metadata use case fails THE SYSTEM SHALL log an `OPMFError` that carries the channels and metadata collected so far, then rethrow.

#### Download videos (`videos [siteId]`)
- WHEN the videos use case runs without a site id THE SYSTEM SHALL download every metadata item whose status is `ToDownload`.
- WHEN the videos use case runs with a site id THE SYSTEM SHALL download only the metadata item with that site id.
- WHEN a download queue is selected THE SYSTEM SHALL mark every queued item as being downloaded before downloading starts.
- WHEN downloading finishes THE SYSTEM SHALL persist each item's status, clear its being-downloaded flag, ensure the video output folder exists, and move every file with the configured video or subtitle extension from the download folder to the output folder, skipping files whose name already exists there.
- WHEN the videos use case fails THE SYSTEM SHALL log an `OPMFError` that carries the queued metadata, then rethrow.

#### UI operations (D1 = strict)
- WHEN the UI lists channels, updates a channel's blacklist flag, lists warning or error logs, lists new / queued / searched videos, updates a video's status, clears a video's being-downloaded flag, fetches a single video by URL, or requests download-now THE SYSTEM SHALL perform the operation through an Application-layer use case.
- WHEN download-now is requested THE SYSTEM SHALL flag the metadata as being downloaded and start `OffPeakMediaFetcher.exe videos <siteId>` as a separate process.

### Service layer (`MediaManager.Services2`)

- WHEN a service reads or writes persisted data THE SYSTEM SHALL do so only through Repository interfaces, never through `DatabaseAdapter` or any LiteDB type.
- WHEN a service needs the video site or the downloader THE SYSTEM SHALL use a Repository interface, never a concrete Store class.
- WHEN a service assembles `ChannelMetadata` pairs THE SYSTEM SHALL keep that join logic in the service.
- WHEN `GetVideoByUrl` is called for a video that is not stored THE SYSTEM SHALL fetch it from the site, store its metadata and channel, and return the pair.
- WHEN `GetVideoByUrl` cannot retrieve the video THE SYSTEM SHALL throw `Exception("Failed to get video")`.
- WHEN business rules currently in the CLI (queue selection, status and flag updates, output-file moving) are moved THE SYSTEM SHALL place them in `MediaManager.Services2`.

### Repository layer (`MediaManager.Repositories`)

- WHEN the project is introduced THE SYSTEM SHALL provide repository interfaces and implementations for channels, metadata, logs, site access (metadata fetch and single-video lookup) and video downloading, built over the existing Store types.
- WHEN a repository method runs THE SYSTEM SHALL open the LiteDB connection for the duration of that call only and dispose it, as `DatabaseAdapter.AccessDbAdapter` does today.
- WHEN a database call throws THE SYSTEM SHALL append the exception to the text log and rethrow, as today.
- WHEN a repository interface is declared THE SYSTEM SHALL NOT expose any LiteDB type in its signatures.

### Store layer

- WHEN the refactor completes THE SYSTEM SHALL keep the public shape of every Store interface covered by a contract test (`IDatabaseCollection`, `IChannelDbCollection`, `IMetadataDbCollection`, `IDownloader`, `IMetadataFetcher`, `ISiteChannelFinder`, `ISiteVideoMetadataGetter`).
- WHEN the refactor completes THE SYSTEM SHALL keep the collection names `YoutubeMetadata`, `YoutubeChannel`, `OPMFLog` and the stored document shapes.

### API layer

- WHEN the CLI receives no arguments THE SYSTEM SHALL print `Appropriate arguments required.` and exit with code 0.
- WHEN the CLI receives an unknown first argument THE SYSTEM SHALL print `Invalid argument.` and exit with code 0.
- WHEN the CLI receives `videos` or `metadata` THE SYSTEM SHALL only dispatch to the matching Application use case.
- WHEN an executable builds its object graph THE SYSTEM SHALL do so without the API project referencing any project below the Application layer.
- WHEN a Razor component or UI module needs behaviour THE SYSTEM SHALL depend only on Application-layer types and Domain types.

### Domain and DTO (D2 default)

- WHEN the change completes THE SYSTEM SHALL keep `OPMF.Entities` as the Domain project with its namespace and assembly name unchanged.
- WHEN the change completes THE SYSTEM SHALL NOT add `MediaManager.Models` or `MediaManager.Dtos`.

If D2 = rename: WHEN the application starts against a database written before the rename THE SYSTEM SHALL rewrite stored `_type` values idempotently via `MediaManager.Database.Migration` before any read, and SHALL read every existing document without data loss.

### Self-documenting code (D4 default: all production code)

- WHEN a variable, parameter, field, property, method, class, interface or test is named THE SYSTEM SHALL use a name that states exactly what it holds or does. Names of up to 200 words are acceptable.
- WHEN a comment restates what the code does THE SYSTEM SHALL remove it and let a well-named variable or method carry the information.
- WHEN a comment explains why a non-obvious decision was made (for example "dbToUpdate must be list or it won't update in foreach loop") THE SYSTEM SHALL keep it.
- WHEN an intermediate value, condition or step is hard to read inline THE SYSTEM SHALL extract it into a named variable or method.
- WHEN a public member is covered by a contract test THE SYSTEM SHALL keep its current name and signature, even if the name is not self-documenting.

### Testing

- WHEN a new class is added THE SYSTEM SHALL have unit tests for it written before the implementation, run to failure, then made to pass.
- WHEN a new public interface is added (repository interfaces, Application use cases) THE SYSTEM SHALL add contract tests for it.
- WHEN a service is tested together with its repositories THE SYSTEM SHALL use a real LiteDB file in a per-test temporary folder, not a mock.
- WHEN the architecture contract test is added THE SYSTEM SHALL place it in `tests/OPMF.ContractTests`.
- WHEN existing integration tests construct services THE SYSTEM SHALL change only the construction wiring, not the assertions.
- WHEN the change completes THE SYSTEM SHALL pass all 128 baseline tests, with contract, unit and E2E tests unmodified, plus the new tests.

### Solution hygiene

- WHEN a new `.csproj` is added THE SYSTEM SHALL register it in `OffPeakMediaFetcher.sln` for all six configurations (Debug/Release × Any CPU/x64/x86).
- WHEN the change completes THE SYSTEM SHALL update the architecture diagram and layer mapping in `CLAUDE-project.md`, including the cross-cutting classification from D3.
- WHEN the change completes THE SYSTEM SHALL leave the other reserved `MediaManager.*` folders (`Models`, `Dtos`, `Services`, `Database`, `Database.Migration`, `Logging`, `TaskRunner`, `Types`) without projects.
- WHEN the change completes THE SYSTEM SHALL pass `dotnet format` and build with 0 errors.

## Unchanged behavior (regression prevention)

- WHEN the Debug build runs THE SYSTEM SHALL CONTINUE TO use the `OffPeakMediaFetcherDev` data folder; the Release build SHALL CONTINUE TO use `OffPeakMediaFetcher`.
- WHEN data is read or written THE SYSTEM SHALL CONTINUE TO use `Databases/OPMF.db` with LiteDB shared-connection mode.
- WHEN a database from before this change is opened THE SYSTEM SHALL CONTINUE TO read all channels, metadata and logs.
- WHEN a failure occurs in a fetch use case THE SYSTEM SHALL CONTINUE TO log it with its variables, rethrow, and have the entry point log it again and print the message. This double logging is existing behaviour and is kept.
- WHEN the UI loads video or channel lists THE SYSTEM SHALL CONTINUE TO page in chunks of 10000 and batch status and blacklist updates on a 1 second timer.
- WHEN download-now is requested from the UI THE SYSTEM SHALL CONTINUE TO launch the task runner as a separate process.

## Edge cases and limitations

- WHEN `metadata` or `videos` need Google credentials, network access or `yt-dlp` THE SYSTEM CANNOT be covered by hermetic E2E tests. Those flows are covered by Application-layer tests with fake repositories, and the two existing E2E tests keep covering CLI dispatch errors.
- WHEN the new `MediaManager.Repositories` and `MediaManager.Initializations` projects are created THE SYSTEM SHALL ignore the stale `bin/` and `obj/` leftovers already in those folders from earlier F# projects. They are git-ignored.

## Out of scope (noticed, deliberately not changed)

- `throw e;` in `VideoFetcher` / `MetadataFetcher` resets the stack trace (compiler warning CA2200). Fixing it is a behaviour change.
- Fetch failures are logged twice (once with variables, once by `Program`). Listed under "Unchanged behavior".
- The `YTDownloader` constructor builds a `YoutubeDL` object that is never stored or used.
- Stray folders `FetcherManager/`, `OPMF.Actions/`, `OPMF.Tests/` contain only build leftovers and are not part of this change.
