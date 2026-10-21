# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Project-specific details — what this repository is, its architecture, layer mapping, test projects, build/run commands, and tech stack — are in @CLAUDE-project.md.

## Changes (spec-driven work)

> This approach is based on [Kiro's spec methodology](https://kiro.dev/docs/specs/). The [Requirements-First workflow](https://kiro.dev/docs/specs/feature-specs/requirements-first/) is the standard used here: specify system behaviour before making architectural decisions.

Non-trivial features and bug fixes are tracked as a **change** — a folder at `docs/changes/<change-name>/` containing up to three spec files. Use specs for anything complex, costly to get wrong, or requiring iterative design. Skip specs for exploratory/prototype work.

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

Before starting any non-trivial feature, refactor, or bug fix, check `docs/changes/` for an existing change folder. If none exists, create one and start with `requirements.md` (feature) or `bugfix.md` (bug).

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
- No speculative abstractions — only build what is needed now.

## Self-documenting code

- Names of variables, parameters, fields, properties, methods, classes, interfaces, and tests must state exactly what they hold or do, so the code reads without explanation.
- Names may be up to 200 words long. Prefer a long, precise name over a short, ambiguous one.
- Structure the code so it explains itself: pull intermediate values, conditions, and steps out into well-named variables and methods instead of describing them in comments.
- Refrain from comments where possible. Use one only when the code itself cannot carry the information, such as why a non-obvious decision was made.

## Testing

### Mandate

**Unit tests must be written before the implementation code they cover (TDD).** Write the test, watch it fail, then write the minimum code to make it pass.

### In specs

`tasks.md` must include explicit test tasks. For features: unit tests as the first task for each class, followed by contract / integration / E2E tasks as the surface area demands. For bugfixes: tasks must include a test that reproduces the bug before it is fixed, plus regression tests drawn from `bugfix.md`'s "Unchanged Behavior" section.
