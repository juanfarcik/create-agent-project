# Changelog

Format loosely follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Full C#/.NET 8 port of the Python prototype — the canonical
  implementation going forward. Same domain model, rules engine, and
  scaffold generator, ported idiomatically (records, pattern matching,
  `IRuntimeAdapter` strategy pattern) rather than transliterated.
- `AgentProjectArchitect.Core` — zero-`Console`-I/O class library:
  domain model, 9 architecture profiles, 11 work patterns, deterministic
  recommendation engine, optimizer, scaffold generator, runtime adapters
  (Claude Code, OpenCode, Codex CLI), and `Api.Preview`/`Api.BuildProject`
  as the seam a future web UI calls directly.
- `AgentProjectArchitect.Cli` — console wizard (Simple/Advanced) and
  command dispatch (`new`, `validate`, `status`, `architecture`,
  `optimize`, `compare`, `templates`, `patterns`).
- 35 xUnit tests covering the recommendation engine, optimizer, pattern
  integration, scaffold generation + YAML round-trip, and runtime adapters.
- `examples/GenerateExamples` regenerates the 5 reference example projects.
- GPLv3 license, BDFL governance model (`GOVERNANCE.md`), Contributor
  Covenant code of conduct, security policy, issue/PR templates, and
  Dependabot config for a professional OSS baseline.
- `Directory.Build.props` + `.editorconfig`: nullable reference types,
  .NET analyzers (`AnalysisLevel=latest-recommended`), enforced code
  style, XML documentation generation for the public API surface.

### Changed from the Python prototype

- Runtime adapters (`ClaudeCodeAdapter`, `OpenCodeAdapter`, `CodexCliAdapter`)
  implement `IRuntimeAdapter` and are resolved through
  `RuntimeAdapterRegistry` — adding a new runtime means adding a class,
  not editing a shared registry function (Open/Closed Principle).
- The architecture engine is split by responsibility instead of one
  module: `ArchitectureProfileCatalog` (data), `ArchitectureRecommender`
  (requirements -> architecture), `ArchitectureOptimizer` (complexity
  reduction), `ArchitectureCostEstimator` and `LoopPatternApplier`
  (single-purpose helpers `ArchitectureRecommender` composes).
- Table-formatting presentation logic (`compare`'s output) moved out of
  `Core` into the CLI project — it's a display concern, not domain logic.
- YAML read/write uses YamlDotNet instead of a hand-rolled parser (the
  Python version avoided the dependency deliberately; in .NET a mature
  library is the better default).
