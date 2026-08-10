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
2. Run `dotnet test` — all tests must pass (50 as of this writing; the
   number only matters in that it shouldn't go down without a reason).
3. Add tests for new behavior — especially anything touching
   `ArchitectureRecommender.cs`/`ArchitectureOptimizer.cs` (recommendation
   and optimizer rules), `ProjectComponentCatalog.cs` (folder inclusion
   rules), or `ScaffoldGenerator.cs`/`YamlLoader.cs` (generation and
   round-trip correctness) — those are the parts most likely to silently
   break.
4. Keep `AgentProjectArchitect.Core` free of `Console` I/O — it's the
   seam a future web UI calls directly (see `Api.cs`). Interactive
   prompts belong in `AgentProjectArchitect.Cli` only.
5. Keep the generator deterministic — no network calls or LLM calls in
   the core recommendation/generation path.
6. If you add or cite an external idea (a paper, a project, a
   convention), add it to `docs/REFERENCES.md` with a real, checked
   link — see that file's own rule at the top for what counts.
7. Open a PR describing what changed and why, and update
   `CHANGELOG.md`'s `[Unreleased]` section as part of the same PR (see
   `CHANGELOG.md`'s own note — it's updated per-change, not batched).

## Adding a new role

Add it to `Roles.cs` (`Roles.All`) with description, responsibilities,
required/excluded context, tools, and escalation conditions. Reference it
from an architecture profile in `ArchitectureProfileCatalog.cs`. A test
already asserts every built-in profile only references roles that exist
(`ProfileTests.AllProfileRolesExistInRoleLibrary`).

## Adding a new work pattern

Add a `LoopPattern` entry to `Patterns.cs` (`Patterns.All`) and add its id
to `Patterns.Choices()`'s display order. If it forces roles on, they must
exist in `Roles.cs` — a test asserts this automatically
(`PatternsRegistryTests.AllPatternsHaveRolesThatExist`).

## Adding a new runtime adapter

Add a class implementing `IRuntimeAdapter` (see `Adapters/ClaudeCodeAdapter.cs`
for the shape) and register it in `RuntimeAdapterRegistry.Default`'s list.
This is a Strategy pattern deliberately — adding a runtime should never
require editing another adapter's code. It should only generate what
that runtime needs on top of the runtime-independent core (`AGENTS.md`,
`.agent/`, `.project/`) — never duplicate the project model.

## Adding a new `.project/` component

Add a `ProjectComponent` entry to `ProjectComponentCatalog.Components`
with an `Include` predicate (against `Requirements`/`Architecture`) and a
`Reason` function explaining the decision either way — the CLI's `new`
preview and the generated `README.md` both surface that reason verbatim,
so write it for a human reading it, not just as an internal comment.

## Documentation you're expected to keep in sync

- `CHANGELOG.md` — every notable change, not just releases.
- `docs/REFERENCES.md` — every non-obvious idea's real source.
- `docs/ARCHITECTURE.md` — update if you change a module's responsibility,
  add a new one, or change how the layers (Core / Cli) relate.
