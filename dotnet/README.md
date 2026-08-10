# Agentic Project Architect (.NET)

[![.NET CI](https://github.com/juanfarcik/agent-project-architect/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/juanfarcik/agent-project-architect/actions/workflows/dotnet-ci.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)

Domain-agnostic scaffold generator for agentic projects. It asks what
you're trying to accomplish in plain language, designs the **smallest
architecture that fits** (from a single agent to a full multi-role team),
and generates a portable project you open directly with **Claude Code**,
**OpenCode**, **Codex CLI**, or any other `AGENTS.md`-compatible runtime.

It is not another multi-agent framework. It doesn't run agents — it
designs the workspace they run in, and gets out of the way.

This is the C#/.NET port — the canonical, actively maintained
implementation going forward. A Python prototype exists at the repo root
for reference but is no longer where development happens.

This is a personal, GPLv3-licensed research project — an open exploration
of how individuals (not just engineering teams) can work with agentic
CLIs on their own projects. If you're looking for agentic orchestration
at company scale, that's a different, commercial problem — see
[Tikra](https://www.tikra.team/) ("AI Teammates for Engineering Teams"),
by the same author.

## Why

Most "AI agent templates" start from the architecture (how many agents,
which topology) and force the project into it. This tool starts from
requirements — size, risk, autonomy, budget, lifetime — and derives the
architecture. A 30-minutes-a-day research task gets one agent. A
high-stakes software launch gets an architect, coder, tester, QA, code
reviewer, and human approval gates. Nothing is generated that the project
doesn't need.

## Requirements

.NET 8 SDK (LTS). No other dependencies to build/run the CLI.

## Build & test

```bash
cd dotnet
dotnet build
dotnet test
```

## Run

```bash
dotnet run --project src/AgentProjectArchitect.Cli -- new
```

Or build once and use the binary directly:

```bash
dotnet publish src/AgentProjectArchitect.Cli -c Release -o out
./out/AgentProjectArchitect.Cli new
```

For a single-file binary that doesn't require installing the .NET
runtime, publish self-contained (no AOT):

```bash
dotnet publish src/AgentProjectArchitect.Cli -c Release -r <RID> \
  --self-contained -p:PublishSingleFile=true -o out
```

(`<RID>` e.g. `osx-arm64`, `linux-x64`, `win-x64`.)

**Native AOT (`-p:PublishAot=true`) does not work yet.** It compiles, but
`validate`/`architecture`/`optimize` — anything that reads `.agent/*.yaml`
back — crashes at runtime ("Exception during deserialization"), because
YamlDotNet's default (de)serializer relies on reflection, which trimming
removes. Fixing this means switching to YamlDotNet's source-generated
static context (`StaticSerializerBuilder`/`StaticDeserializerBuilder`,
see their AOT docs) — tracked as a known limitation, contributions welcome.

## Wizard

Exactly two paths, same as the design brief:

- **Simple** (`new --simple`) — for non-technical creators (writers,
  designers, musicians, researchers, anyone). A handful of plain-language
  questions, everything else defaults sensibly.
- **Advanced** (`new --advanced`) — full explicit control over size,
  risk, lifetime, execution mode, schedule, budget, and work pattern.

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
├── AGENTS.md              # entry point every runtime reads first
├── GETTING_STARTED.md      # tailored walkthrough (beginner or technical)
├── .agent/                 # the architecture — how agents help
│   ├── project.yaml
│   ├── architecture.yaml
│   ├── policies.yaml
│   └── prompts/<role>.md
├── .project/                # the project — what you're trying to do
│   ├── goal.md, state.md, backlog.md, decisions.md, learnings.md, constraints.md, ...
│   └── outputs/ research/ plans/ reviews/ checkpoints/ telemetry/
├── .claude/agents/*.md      # generated if targeting Claude Code
├── .opencode/agent/*.md     # generated if targeting OpenCode
└── .codex/                  # generated if targeting Codex CLI
```

## CLI commands

```bash
agent-project new [path] [--simple|--advanced] [--runtime <agnostic|claude-code|opencode|codex-cli|all>]
agent-project validate      <path>
agent-project status        <path>
agent-project architecture  <path> [--recommend]
agent-project optimize      <path> [--apply]
agent-project compare
agent-project templates
agent-project patterns
```

## Architecture profiles & work patterns

Same catalogue as the design brief: 9 architecture profiles (`minimal`
through `software-high-reliability`) and 11 work patterns
(`agent-in-the-loop`, `human-in-the-loop`, `debate-critic`,
`swarm-parallel`, ...), all deterministic — `agent-project templates`
and `agent-project patterns` list them.

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

## License & governance

GPLv3 — see [LICENSE](LICENSE). Governance model (who decides what gets
merged) is separate from the license — see [GOVERNANCE.md](GOVERNANCE.md).
Contributions welcome — see [CONTRIBUTING.md](CONTRIBUTING.md).
