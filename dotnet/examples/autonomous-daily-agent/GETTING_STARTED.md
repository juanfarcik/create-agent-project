# Getting Started with autonomous-daily-agent

Architecture: **autonomous-loop** — 3 role(s):

  - orchestrator (always, balanced)
  - researcher (on-demand, cheap)
  - evaluator (always, balanced)

## Steps

1. `cd` into this directory.
2. Open it with Claude Code (or any AGENTS.md-compatible runtime —
   Claude Code, OpenCode, and Codex CLI all read `AGENTS.md`/`CLAUDE.md`
   at the project root by convention; nothing else to wire up).
3. The runtime reads `AGENTS.md` on session start. It in turn points to:
   - `.project/goal.md` / `state.md` / `backlog.md` / `constraints.md` for
     current project reality
   - `.agent/architecture.yaml` + `.agent/policies.yaml` for the agent
     topology and budget/permission policy
   - `.agent/prompts/<role>.md` for each configured role's instructions
4. Drive it directly ("implement the next backlog item", "run the
   evaluator on this artifact") or let the orchestrator decide the next
   action on its own if `execution_mode` allows it.
5. Inspect/adjust the architecture with the `agent-project` CLI
   (`status`, `architecture --recommend`, `optimize --apply`, `validate`).
6. Edit `.agent/architecture.yaml` or `.agent/policies.yaml` directly for
   fine-grained control, then re-run `validate`.
