# create-agent-project

[![.NET CI](https://github.com/juanfarcik/create-agent-project/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/juanfarcik/create-agent-project/actions/workflows/dotnet-ci.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)

> Working name. Once this has real usage, it'll get a proper product
> name — `create-agent-project` is deliberately literal for now (same
> naming spirit as `create-react-app`/`create-next-app`: it says exactly
> what it does, nothing more).

Domain-agnostic scaffold generator for agentic projects. It asks what
you're trying to accomplish in plain language, designs the **smallest
architecture that fits** (from a single agent to a full multi-role team),
and generates a portable project you open directly with **Claude Code**,
**OpenCode**, **Codex CLI**, or any other `AGENTS.md`-compatible runtime.

It is not another multi-agent framework. It doesn't run agents — it
designs the workspace they run in, and gets out of the way.

This is a personal, GPLv3-licensed research project — an open exploration
of how individuals (not just engineering teams) can work with agentic
CLIs on their own projects. If you're looking for agentic orchestration
at company scale, that's a different, commercial problem — see
[Tikra](https://www.tikra.team/) ("AI Teammates for Engineering Teams"),
by the same author.

## Who this is for

- **Individuals working solo** — hobbyists, freelancers, researchers,
  writers, indie developers. Not engineering teams, not company-scale
  orchestration.
- **Any domain that produces file-based output**, not just software:
  writing a book, running a research process, planning a business,
  making music or art, as much as building an app.
- **People with no agent-architecture experience** — the wizard's
  Simple mode asks in plain language, defaults sensibly, and never
  requires understanding what "multi-agent" even means. There's also
  an Advanced mode for full explicit control if you want it.
- **Anyone who wants to avoid vendor lock-in** — the generated base
  works with Claude Code, OpenCode, Codex CLI, or any future tool that
  reads `AGENTS.md`, without regenerating anything.

**What this is not:** an agent framework, an orchestrator, or anything
that runs agents itself. It designs the workspace an agent works in,
then gets out of the way — see `docs/ARCHITECTURE.md` for exactly
where its responsibility ends.

## Why

Most "AI agent templates" start from the architecture (how many agents,
which topology) and force the project into it. This tool starts from
requirements — size, risk, autonomy, budget, lifetime — and derives the
architecture. A 30-minutes-a-day research task gets one agent. A
high-stakes software launch gets an architect, coder, tester, QA, code
reviewer, and human approval gates. Nothing is generated that the project
doesn't need.

## Install

Three ways to get `create-agent-project`, in order of how much you want to deal
with:

### 1. Download a release binary (no .NET install needed)

Grab the archive for your platform from
[Releases](https://github.com/juanfarcik/create-agent-project/releases),
extract it, and run the binary directly:

```bash
tar -xzf create-agent-project-osx-arm64.tar.gz   # or unzip create-agent-project-win-x64.zip
./AgentProjectArchitect.Cli --help
```

Self-contained and single-file — the whole .NET runtime is bundled in,
nothing else to install. (This is *not* Native AOT — see the note below
for why, and what that would take.)

### 2. Build the binary yourself

Requires the .NET 8 SDK.

```bash
cd dotnet
./scripts/publish.sh              # current OS/arch only
./scripts/publish.sh --all        # every supported platform
./scripts/publish.sh osx-arm64    # one specific platform
```

Binaries land in `dotnet/publish/`. This is exactly what the release
workflow (`.github/workflows/release.yml`) runs on every `v*` tag push.

### 3. Run from source

```bash
cd dotnet
dotnet build
dotnet test
dotnet run --project src/AgentProjectArchitect.Cli -- new
```

### Why not Native AOT?

`dotnet publish -p:PublishAot=true` compiles, but `validate` /
`architecture` / `optimize` — anything that reads `.agent/*.yaml` back —
crashes at runtime ("Exception during deserialization"), because
YamlDotNet's default (de)serializer relies on reflection, which AOT
trimming removes. Self-contained + single-file (what `publish.sh` does)
gives the same "no separate runtime install" outcome without that
breakage. Fixing AOT properly means switching to YamlDotNet's
source-generated static context (`StaticSerializerBuilder`/
`StaticDeserializerBuilder`) — tracked as a known limitation,
contributions welcome.

## Wizard

Exactly two paths, same as the design brief:

- **Simple** (`new --simple`) — for non-technical creators (writers,
  designers, musicians, researchers, anyone). A handful of plain-language
  questions, everything else defaults sensibly.
- **Advanced** (`new --advanced`) — full explicit control over size,
  risk, lifetime, execution mode, schedule, budget, and work pattern, plus
  an optional step to add specific specialist roles on-demand beyond what
  the rules engine recommends (the engine's picks always win by default —
  this is for when you already know you want, say, a `risk-reviewer`
  available even though the requirements didn't trigger one automatically).

**The generated base is CLI-agnostic by default.** `AGENTS.md` +
`.agent/` + `.project/` work with any agentic CLI that reads
`AGENTS.md` — Claude Code, OpenCode, Codex CLI, or one that doesn't
exist yet — with zero vendor-specific files. Picking a specific runtime
(`--runtime claude-code`, etc.) only adds *optional native extras* on
top (e.g. Claude Code subagents under `.claude/agents/`) — it never
changes what the agnostic core contains.

## What gets generated

```
my-project/
├── AGENTS.md                  # entry point every runtime reads first
├── GETTING_STARTED.md          # tailored walkthrough (beginner or technical)
├── .agent/                     # the architecture — how agents help
│   ├── project.yaml
│   ├── architecture.yaml
│   ├── policies.yaml
│   └── prompts/<role>.md
├── .project/                    # the project — what you're trying to do
│   ├── goal.md                    # stable, high-level objective + Definition of Done
│   ├── specs/                     # one file per feature/deliverable — see docs/REFERENCES.md
│   ├── references/                # the human's own source material (style guides, links, ...)
│   ├── outputs/                    # the actual durable output — domain-aware README seeded in
│   ├── state.md, backlog.md, decisions.md, learnings.md, constraints.md, ...
│   └── research/ plans/ experiments/ reviews/ checkpoints/ telemetry/
├── .claude/agents/*.md          # generated if targeting Claude Code
├── .claude/skills/<pattern>/    # generated if targeting Claude Code and a work pattern was picked
├── .opencode/agent/*.md         # generated if targeting OpenCode
└── .codex/                      # generated if targeting Codex CLI
```

Every generated `.md` file carries a small YAML frontmatter block (`type`,
`purpose`) so it can be classified without reading the body — see
[`docs/REFERENCES.md`](docs/REFERENCES.md) for why. Vendor folders
(`.claude/`, `.opencode/`, `.codex/`) are opt-in extras on top of the
agnostic base, not generated by default (see below).

**The `.project/` subfolders shown above aren't generated unconditionally.**
Each one has a concrete inclusion rule — `research/` only appears if a
researcher role is in the architecture, `checkpoints/` only if the
architecture uses checkpoints, `telemetry/` only for long-lived or
scheduled projects, and so on. A trivial project gets 2-3 of the 9
possible folders; a large multi-role software project gets most of
them. `new` shows the full ✓/○ list with a one-line reason for each
before writing anything to disk — see `ProjectComponentCatalog` in
`docs/REFERENCES.md`.

## CLI commands

```bash
create-agent-project new [path] [--simple|--advanced] [--runtime <agnostic|claude-code|opencode|codex-cli|all>]
create-agent-project validate      <path>
create-agent-project status        <path>
create-agent-project architecture  <path> [--recommend]
create-agent-project optimize      <path> [--apply]
create-agent-project compare
create-agent-project templates
create-agent-project patterns
create-agent-project --version
create-agent-project <command> --help    # detailed help for any command
```

The commands above assume the binary is on your `PATH` as `create-agent-project`
(rename it after downloading/building, or add an alias). Running it
un-renamed, it's `AgentProjectArchitect.Cli` / `./AgentProjectArchitect.Cli`
depending on install method.

## Architecture profiles & work patterns

Same catalogue as the design brief: 9 architecture profiles (`minimal`
through `software-high-reliability`) and 11 work patterns
(`agent-in-the-loop`, `human-in-the-loop`, `debate-critic`,
`swarm-parallel`, ...), all deterministic — `create-agent-project templates`
and `create-agent-project patterns` list them.

**None of these were invented here.** Every pattern traces to a real
paper, project, or established prior art — see
[`docs/REFERENCES.md`](docs/REFERENCES.md) for the actual source of
each one, plus an honest assessment of where this project is and isn't
aligned with current agentic-system trends (including real gaps like
no MCP integration and no evaluation harness).

## Project layout

```
dotnet/
├── src/
│   ├── AgentProjectArchitect.Core/     # domain model + rules engine + generator (no I/O)
│   │   ├── Architecture/                # ArchitectureProfileCatalog, ArchitectureRecommender,
│   │   │                                 # ArchitectureOptimizer, ArchitectureCostEstimator, LoopPatternApplier
│   │   ├── Adapters/                    # IRuntimeAdapter + one class per runtime + registry
│   │   ├── Scaffold/                    # ScaffoldGenerator
│   │   ├── Models.cs, Roles.cs, Patterns.cs, YamlLoader.cs, Api.cs
│   │   └── ...
│   └── AgentProjectArchitect.Cli/      # console wizard + command dispatch (all Console I/O lives here)
├── tests/AgentProjectArchitect.Tests/
└── examples/                            # 5 reference projects + regeneration tool
```

**Design notes for contributors:**

- `AgentProjectArchitect.Core` has zero `Console` I/O — every future
  frontend (a web UI, a GUI) calls `Api.Preview` / `Api.BuildProject`
  directly instead of the CLI's wizard. See `Api.cs`.
- Runtime adapters are a Strategy pattern (`IRuntimeAdapter`), not a
  switch statement — adding a new agentic CLI's support means adding one
  class and registering it in `RuntimeAdapterRegistry`, no existing
  adapter code changes (Open/Closed Principle).
- The architecture engine is split by single responsibility:
  `ArchitectureProfileCatalog` is pure data, `ArchitectureRecommender`
  turns requirements into an architecture, `ArchitectureOptimizer` trims
  it, `ArchitectureCostEstimator`/`LoopPatternApplier` are the two
  focused helpers the recommender composes. None of them know how to
  print anything — presentation (e.g. the `compare` table) lives in the
  CLI project.
- Nullable reference types + .NET analyzers (`AnalysisLevel=latest-recommended`)
  are enabled repo-wide via `Directory.Build.props`; the build is
  warning-clean.

## Regenerating the reference examples

```bash
dotnet run --project examples/GenerateExamples
```

## Documentation

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — technical reference:
  module-by-module design, the recommendation/optimizer algorithms,
  design invariants, testing strategy.
- [`docs/REFERENCES.md`](docs/REFERENCES.md) — where every pattern,
  architecture idea, and practice actually comes from, plus an honest
  self-assessment against current trends (including real gaps).

## License & governance

GPLv3 — see [LICENSE](LICENSE). Governance model (who decides what gets
merged) is separate from the license — see [GOVERNANCE.md](GOVERNANCE.md).
Contributions welcome — see [CONTRIBUTING.md](CONTRIBUTING.md).
