# Changelog

## 0.1.0

Initial release.

- Requirements-driven architecture recommendation engine (deterministic,
  no LLM calls), 9 built-in profiles including software-specific ones
  (`software-lean`, `software-standard`, `software-high-reliability`)
  with `architect`, `coder`, `tester`, `qa-reviewer`, `code-reviewer`,
  `bi-analyst` roles.
- Architecture optimizer that demotes/removes unjustified complexity
  under tight budgets, without ever stripping a project's core producer
  role (e.g. `coder` is never fully removed from a software project).
- Portable scaffold generator: `AGENTS.md`, `.agent/` (architecture),
  `.project/` (state), `GETTING_STARTED.md` tailored to beginner or
  technical experience level.
- Runtime adapters for Claude Code, OpenCode, and Codex CLI.
- CLI: `new`, `validate`, `status`, `architecture`, `optimize`,
  `compare`, `templates`.
- Dependency-free YAML read/write for the tool's own config shapes.
- Test suite (`tests/`) covering the YAML round-trip, recommendation
  engine, optimizer, scaffold generation, adapters, and CLI commands.
