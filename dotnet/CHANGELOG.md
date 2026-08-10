# Changelog

Format loosely follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
Version is the single `<Version>` in `Directory.Build.props`.

## [Unreleased]

Nothing yet — this file is updated as part of every notable change, not
batched at release time.

## [0.1.0] — first tagged release

The C#/.NET port reaching feature parity with (and then exceeding) the
original Python prototype, which has since been removed from this repo.
See `docs/REFERENCES.md` for where every non-obvious idea below actually
comes from — nothing here is presented as invented in-house unless it is.

### Core engine

- Deterministic, rule-based recommendation engine (`ArchitectureRecommender`) —
  no LLM call decides architecture. 9 built-in profiles (`minimal` through
  `software-high-reliability`), including software-specific roles
  (`architect`, `coder`, `tester`, `qa-reviewer`, `code-reviewer`, `bi-analyst`).
- 11 work patterns (`Patterns.cs`) — `agent-in-the-loop`, `human-in-the-loop`,
  `debate-critic`, `swarm-parallel`, etc. — each with structural guarantees
  the engine enforces (forced roles, minimum architecture size) independent
  of execution-mode settings.
- `ArchitectureOptimizer` — demotes/removes unjustified complexity under
  tight budgets without ever fully removing a project's core producer role
  (e.g. `coder` is never stripped from a software project, only demoted).
- `ProjectComponentCatalog` — the file/folder structure itself is now
  proportional to project complexity, the same way agent selection already
  was. Each of 9 optional `.project/` subfolders has a concrete inclusion
  rule (e.g. `research/` only if a researcher role exists, `checkpoints/`
  only if the architecture uses checkpoints). A trivial project gets 2-3
  folders; a large one gets most of them. Shown as a ✓/○ list with reasons
  before anything is written to disk.

### Scaffold generator

- CLI-agnostic by default (`Requirements.Runtime = "agnostic"`) —
  `AGENTS.md` + `.agent/` + `.project/` work with any AGENTS.md-reading
  CLI with zero vendor files. Picking a runtime only adds optional native
  extras on top; it never changes the agnostic core.
- Runtime adapters (Claude Code, OpenCode, Codex CLI) implement
  `IRuntimeAdapter`, resolved through `RuntimeAdapterRegistry` — adding a
  new runtime means adding a class, not editing a shared function.
- Claude Code adapter also generates a `.claude/skills/<pattern>/SKILL.md`
  for the project's chosen work pattern (distinct mechanism from
  subagents — a Skill is a procedure loaded into the current context,
  not a delegate).
- `.project/specs/` (one file per feature/deliverable, spec-driven-development-inspired)
  and `.project/references/` (the human's own source material) —
  both conditional per `ProjectComponentCatalog`.
- Every generated `.md` file carries a small YAML frontmatter block
  (`type`, `purpose`) so it can be classified without reading the body.
- `AGENTS.md` documents the nested-`AGENTS.md`-per-subfolder convention
  explicitly, a "clarify before you commit" first-session behavior, a
  "when you're not sure, stop and ask" confusion protocol, conversational
  safety phrases, and cites the four failure modes it asks the agent to
  avoid — with honest attribution instead of presenting them as ownerless
  wisdom.
- `.project/learnings.md` distinct from `.project/decisions.md` (emergent
  facts vs. deliberate choices).

### Wizard & CLI

- Exactly two entry points: Simple (plain language, smart defaults) and
  Advanced (full explicit control, including an optional step to add
  specific specialist roles on-demand beyond the engine's automatic picks).
- Domain list is persona-oriented (writing, design, music/art, not just
  software) — internally normalized to the same engine domains.
- `new`, `validate`, `status`, `architecture --recommend`,
  `optimize --apply`, `compare`, `templates`, `patterns`.
- `--version` / `-v`, top-level `--help`, and per-command
  `<command> --help` with full usage/options/examples for every command.

### Distribution

- `scripts/publish.sh` — builds self-contained, single-file binaries
  (no .NET runtime install required) for macOS (arm64/x64), Linux
  (x64/arm64), and Windows (x64).
- `.github/workflows/release.yml` — on every `v*` tag push, runs the
  full test suite, builds all platform binaries, and attaches them to a
  GitHub Release.
- Native AOT does not work yet (YamlDotNet's reflection-based
  (de)serializer breaks under trimming) — documented as a known
  limitation with the working alternative, not silently avoided.

### Engineering / OSS baseline

- `AgentProjectArchitect.Core` has zero `Console` I/O — `Api.Preview` /
  `Api.BuildProject` are the seam a future web UI calls directly instead
  of the CLI wizard.
- Nullable reference types + .NET analyzers (`AnalysisLevel=latest-recommended`)
  enabled repo-wide via `Directory.Build.props`; build is warning-clean.
- 50 xUnit tests covering the recommendation engine, optimizer, pattern
  integration, component catalog, scaffold generation + YAML round-trip,
  and runtime adapters.
- GPLv3 license, BDFL governance (`GOVERNANCE.md`, separate from the
  license), Contributor Covenant code of conduct, security policy,
  issue/PR templates, Dependabot config.
- `docs/REFERENCES.md` — every pattern, architecture idea, and
  engineering practice traced to a real, checked source (paper, project,
  or spec), plus an honest self-assessment against current trends
  (aligned: AGENTS.md convergence, Anthropic's context-engineering
  framing; real gaps: no MCP integration, no evaluation harness, no real
  cost telemetry, no template import).
