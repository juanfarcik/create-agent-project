# Getting Started with software-project

Architecture: **software-standard** — 6 role(s):

  - orchestrator (always, balanced)
  - architect (on-demand, strong)
  - coder (always, balanced)
  - tester (always, cheap)
  - code-reviewer (always, balanced)
  - bi-analyst (on-demand, cheap)

## Steps

1. `cd` into this directory.
2. Open it with your agent CLI of choice (or any AGENTS.md-compatible runtime —
   Claude Code, OpenCode, and Codex CLI all read `AGENTS.md`/`CLAUDE.md`
   at the project root by convention; nothing else to wire up).
3. The runtime reads `AGENTS.md` on session start. It in turn points to:
   - `.project/goal.md` / `state.md` / `backlog.md` / `constraints.md` for
     current project reality
   - `.agent/architecture.yaml` + `.agent/policies.yaml` for the agent
     topology and budget/permission policy
   - `.agent/prompts/<role>.md` for each configured role's instructions
     (already materialized as native subagents under `.claude/agents/` or
     `.opencode/agent/` if that runtime was selected)
4. Drive it directly ("implement the next backlog item", "run the
   evaluator on this artifact") or let the orchestrator decide the next
   action on its own if `execution_mode` allows it.
5. Inspect/adjust the architecture at any point:
   ```
   python3 agent-project.py status       .
   python3 agent-project.py architecture . --recommend
   python3 agent-project.py optimize     . --apply
   python3 agent-project.py validate     .
   ```
6. Edit `.agent/architecture.yaml` or `.agent/policies.yaml` directly for
   fine-grained control, then re-run `validate`. If you add/remove agent
   roles by hand, regenerate the runtime adapter files (`.claude/agents/`,
   `.opencode/agent/`) so they stay in sync with `.agent/prompts/`.
