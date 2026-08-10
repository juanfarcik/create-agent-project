# Contributing

This project is GPLv3-licensed and BDFL-governed — see
[GOVERNANCE.md](GOVERNANCE.md) for what that means in practice.
Contributions are genuinely wanted; the maintainer has final say on what
gets merged and why.

## Setup

```bash
cd dotnet
dotnet build
dotnet test
```

1. Fork, branch, make your change.
2. Run `dotnet test` — all tests must pass.
3. Add tests for new behavior — especially anything touching
   `ArchitectureEngine.cs` (recommendation/optimizer rules) or
   `ScaffoldGenerator.cs`/`YamlLoader.cs` (generation and round-trip
   correctness), since those are the parts most likely to silently break.
4. Keep `AgentProjectArchitect.Core` free of `Console` I/O — it's the
   seam a future web UI calls directly (see `Api.cs`). Interactive
   prompts belong in `AgentProjectArchitect.Cli` only.
5. Keep the generator deterministic — no network calls or LLM calls in
   the core recommendation/generation path.
6. Open a PR describing what changed and why.

## Adding a new role

Add it to `Roles.cs` (`Roles.All`) with description, responsibilities,
required/excluded context, tools, and escalation conditions. Reference it
from an architecture profile in `ArchitectureEngine.cs`. A test already
asserts every built-in profile only references roles that exist
(`ProfileTests.AllProfileRolesExistInRoleLibrary`).

## Adding a new work pattern

Add a `LoopPattern` entry to `Patterns.cs` (`Patterns.All`) and add its id
to `Patterns.Choices()`'s display order. If it forces roles on, they must
exist in `Roles.cs` — a test asserts this automatically.

## Adding a new runtime adapter

Add a `Generate<Runtime>(root, req, arch)` method to
`RuntimeAdapters.cs` and register it in the adapter registry. It should
only generate what that runtime needs on top of the runtime-independent
core (`AGENTS.md`, `.agent/`, `.project/`) — never duplicate the project
model.
