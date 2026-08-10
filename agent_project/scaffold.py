"""Generates the portable project scaffold (Section 46).

Layout:

    AGENTS.md
    README.md
    .agent/{project.yaml, architecture.yaml, policies.yaml}
    .agent/prompts/<role>.md
    .project/{goal,context,state,backlog,decisions,constraints,resources,metrics}.md
    .project/{outputs,research,plans,experiments,reviews,checkpoints,telemetry}/
    runtime adapters (.claude/, .opencode/) generated separately via adapters.py
"""

from __future__ import annotations

from pathlib import Path

from . import patterns as patterns_mod
from .models import Architecture, Requirements
from .roles import ROLES
from .yamlutil import dump

PROJECT_SUBDIRS = [
    "outputs", "research", "plans", "experiments", "reviews", "checkpoints", "telemetry",
]


def generate(root: Path, req: Requirements, arch: Architecture) -> None:
    root.mkdir(parents=True, exist_ok=True)
    (root / ".agent" / "prompts").mkdir(parents=True, exist_ok=True)
    (root / ".agent" / "adapters").mkdir(parents=True, exist_ok=True)
    (root / ".agent" / "schemas").mkdir(parents=True, exist_ok=True)
    (root / ".project").mkdir(parents=True, exist_ok=True)
    for sub in PROJECT_SUBDIRS:
        (root / ".project" / sub).mkdir(parents=True, exist_ok=True)

    _write(root / "AGENTS.md", _agents_md(req, arch))
    _write(root / "README.md", _readme_md(req, arch))
    _write(root / "GETTING_STARTED.md", _getting_started_md(req, arch))

    _write(root / ".agent" / "project.yaml", _project_yaml(req))
    _write(root / ".agent" / "architecture.yaml", _architecture_yaml(arch))
    _write(root / ".agent" / "policies.yaml", _policies_yaml(req, arch))

    for agent in arch.agents:
        role = ROLES.get(agent.role)
        if role:
            _write(root / ".agent" / "prompts" / f"{agent.role}.md", _role_prompt(agent.role, role))

    _write(root / ".project" / "goal.md", _goal_md(req))
    _write(root / ".project" / "context.md", _context_md(req))
    _write(root / ".project" / "state.md", _state_md())
    _write(root / ".project" / "backlog.md", _backlog_md())
    _write(root / ".project" / "decisions.md", _decisions_md())
    _write(root / ".project" / "learnings.md", _learnings_md())
    _write(root / ".project" / "constraints.md", _constraints_md(req, arch))
    _write(root / ".project" / "resources.md", _resources_md())
    _write(root / ".project" / "metrics.md", _metrics_md(req))


def _write(path: Path, content: str) -> None:
    path.write_text(content.rstrip() + "\n", encoding="utf-8")


# ---------------------------------------------------------------------------
# AGENTS.md — concise navigation layer (Section 47), not a knowledge dump
# ---------------------------------------------------------------------------

def _agents_md(req: Requirements, arch: Architecture) -> str:
    agent_lines = "\n".join(
        f"- `{a.role}` ({a.mode}, {a.model_tier}) — {ROLES.get(a.role, {}).get('description', '')}"
        for a in arch.agents
    )
    gates = "\n".join(f"- {g}" for g in arch.human_gates) or "- none configured"
    pattern = patterns_mod.get(arch.loop_pattern)

    return f"""# {req.name} — Agent Instructions

This is a **{req.domain}** project. Architecture: **{arch.profile}**.
Work pattern: **{pattern.label}**.

Do not assume this is a software project unless the domain says so.

## Start here, every session

1. Read `.project/goal.md` — objective and Definition of Done.
2. Read `.project/state.md` — current reality (not history).
3. Read `.project/backlog.md` — actionable next work.
4. Read `.project/constraints.md` before acting.
5. Read `.project/learnings.md` — patterns, pitfalls, and preferences picked
   up in earlier sessions. Add to it when you learn something that will
   matter next time; don't let it become a chronological dump.

## First session: clarify before you commit

If `.project/goal.md` reads thin, vague, or like a surface-level feature
request, don't start building yet. Ask 2-3 sharp questions that surface
what the person is *actually* trying to accomplish — the real pain, not
just the literal words. It's common for what someone describes ("a daily
report") to be a narrower slice of something bigger they haven't
articulated yet. Reflect that back, let them correct you, then update
`.project/goal.md` with what you actually agreed on before writing
anything else. This only needs to happen once — don't re-interrogate an
established goal on every session.

## When you're not sure

Do not guess on a decision that's expensive to reverse (architecture,
scope, what "done" means, deleting/overwriting something). Stop and ask.
Guessing wrong costs more than one extra question ever does. This does
not apply to small, reversible, in-scope choices — decide those yourself
and note why in `.project/decisions.md` if it's non-obvious.

## Stay disciplined

Four failure modes to actively avoid, in order of how often they derail
a project:

1. **Wrong assumptions** — verify instead of guessing (see above).
2. **Overcomplexity** — the smallest thing that satisfies the Definition
   of Done beats a more "proper" version nobody asked for.
3. **Orthogonal edits** — touch what the task requires, not what you
   noticed in passing. Note unrelated issues in the backlog instead.
4. **Imperative over declarative** — prefer stating the desired end state
   and letting the approach follow, over a rigid step list that breaks
   the moment reality differs slightly.

## Loop — {pattern.label}

{pattern.description}

```
{pattern.loop_diagram}
```

Do not stop merely because one task finished. Continue until the
Definition of Done is met, a blocker needs a human, or budget/iteration
limits (`.project/metrics.md`) are reached.

## Agents available

{agent_lines}

Role prompts live in `.agent/prompts/<role>.md`. Delegate only when a role
adds real value — a single agent handling everything is often correct.

## Human approval required for

{gates}

Record every non-trivial decision in `.project/decisions.md`, anything
worth remembering for next time in `.project/learnings.md`, and every
durable output under `.project/outputs/` (conversation is not the output).

## Safety phrases

The person you're working with doesn't need to edit YAML to change how
careful you're being. If they say something like:

- **"be careful"** — slow down, double-check before anything destructive
  or hard to undo, prefer read-only steps first.
- **"don't touch anything outside [folder/file]"** — treat everything
  else as read-only until they say otherwise.
- **"stop"** / **"wait"** — stop mid-action and report state; don't
  continue the loop until they respond.

Honor these immediately, without asking for confirmation first.

## Architecture details

See `.agent/architecture.yaml` for the full agent/topology config and
`.agent/policies.yaml` for budget and permission policy. Run
`agent-project validate` after manual edits.
"""


def _readme_md(req: Requirements, arch: Architecture) -> str:
    return f"""# {req.name}

{req.objective}

Generated by Agentic Project Architect. Architecture: **{arch.profile}**
({len(arch.agents)} agent(s), estimated cost: {arch.est_cost}).

## Layout

- `AGENTS.md` — entry point for any agent runtime (Claude Code, OpenCode, ...)
- `.agent/` — architecture, policies, prompts (the "how")
- `.project/` — goal, state, backlog, decisions, outputs (the "what")

## Working on this project

With Claude Code, open this directory and let it read `AGENTS.md` first.
Runtime-specific adapters (if generated) live under `.claude/` / `.opencode/`.

## Managing this project

```
python3 agent-project.py status   .
python3 agent-project.py validate .
python3 agent-project.py architecture .
python3 agent-project.py optimize .
```
"""


def _getting_started_md(req: Requirements, arch: Architecture) -> str:
    runtime_label = {
        "claude-code": "Claude Code", "opencode": "OpenCode",
        "codex-cli": "Codex CLI", "all": "your agent CLI of choice",
    }.get(req.runtime, req.runtime)

    if req.experience_level == "beginner":
        return f"""# Getting Started with {req.name}

You don't need to know anything about "agents" or "prompts" to use this.
The project already has everything set up — you just need to open it and
start talking to it.

## Steps

1. Open this folder in your editor (e.g. VS Code).
2. Open a terminal in this folder and start {runtime_label} there
   (e.g. run `claude` for Claude Code, or the equivalent for your tool).
3. Your assistant will automatically read `AGENTS.md` first — that file
   tells it what this project is and what to do. You don't need to paste
   anything.
4. Just type what you want in plain language, for example:
   - "Get started" / "What's the current state of the project?"
   - "Do the next most useful thing"
   - "Show me what's been done so far"
5. The assistant will keep track of progress for you in the `.project/`
   folder. You can check `.project/state.md` anytime to see where things
   stand, or `.project/outputs/` to see what's been produced.
6. It will ask you before doing anything risky or irreversible — that's
   expected, just answer yes/no.
7. If it starts going somewhere you don't want, you can say **"stop"**,
   **"be careful"**, or **"don't touch anything outside [some folder]"**
   at any time — it's instructed to listen to those immediately.

That's it. If you ever feel lost, just say "explain the current state of
this project" and it will summarize it for you.
"""

    agent_lines = "\n".join(f"  - {a.role} ({a.mode}, {a.model_tier})" for a in arch.agents)
    return f"""# Getting Started with {req.name}

Architecture: **{arch.profile}** — {len(arch.agents)} role(s):

{agent_lines}

## Steps

1. `cd` into this directory.
2. Open it with {runtime_label} (or any AGENTS.md-compatible runtime —
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
"""


# ---------------------------------------------------------------------------
# .agent/*.yaml
# ---------------------------------------------------------------------------

def _project_yaml(req: Requirements) -> str:
    return dump({
        "project": {
            "name": req.name,
            "domain": req.domain,
            "objective": req.objective,
            "definition_of_done": req.definition_of_done,
        },
        "requirements": {
            "size": req.size,
            "lifetime": req.lifetime,
            "autonomy": req.autonomy,
            "risk": req.risk,
            "budget_profile": req.budget_profile,
            "execution_mode": req.execution_mode,
            "human_involvement": req.human_involvement,
            "schedule": req.schedule or "",
            "loop_pattern": req.loop_pattern,
        },
        "runtime": req.runtime,
        "experience_level": req.experience_level,
    })


def _architecture_yaml(arch: Architecture) -> str:
    return dump({
        "architecture": {
            "profile": arch.profile,
            "loop_pattern": arch.loop_pattern,
            "memory": arch.memory,
            "checkpoints": arch.checkpoints,
            "complexity": arch.complexity,
            "estimated": {
                "calls_per_run": arch.est_calls_per_run,
                "context": arch.est_context,
                "cost": arch.est_cost,
            },
            "agents": [
                {"role": a.role, "mode": a.mode, "model_tier": a.model_tier}
                for a in arch.agents
            ],
            "human_gates": arch.human_gates,
            "notes": arch.notes,
        },
    })


def _policies_yaml(req: Requirements, arch: Architecture) -> str:
    return dump({
        "budget": {
            "profile": req.budget_profile,
            "warn_at_pct": 70,
            "throttle_at_pct": 85,
            "stop_at_pct": 100,
        },
        "permissions": {
            "spend_money": False,
            "contact_external_people": False,
            "publish_externally": False,
            "perform_irreversible_actions": False,
        },
        "failure_policy": {
            "on_failure_1": "retry",
            "on_failure_2": "try alternative strategy",
            "on_failure_3": "reassess",
            "on_repeated_failure": "escalate to human",
        },
        "human_gates": arch.human_gates,
    })


def _role_prompt(role_id: str, role: dict) -> str:
    resp = "\n".join(f"- {r}" for r in role["responsibilities"])
    req_ctx = "\n".join(f"- {c}" for c in role["required_context"]) or "- (task-scoped)"
    exc_ctx = "\n".join(f"- {c}" for c in role["excluded_context"]) or "- (none)"
    esc = "\n".join(f"- {e}" for e in role["escalate_when"])
    tools = ", ".join(role["tools"])

    return f"""# Role: {role_id}

{role["description"]}

## Responsibilities

{resp}

## Required context

{req_ctx}

## Do NOT pull in

{exc_ctx}

## Allowed tools

{tools}

## Escalate to the orchestrator/human when

{esc}

Report back with: what was done, what changed, what remains, confidence,
and any assumptions made.
"""


# ---------------------------------------------------------------------------
# .project/*.md — concise, structured, non-chronological (Section 40/41)
# ---------------------------------------------------------------------------

def _goal_md(req: Requirements) -> str:
    return f"""# Goal

## Objective

{req.objective}

## Definition of Done

{req.definition_of_done or "(define this before starting non-trivial work)"}

## Domain

{req.domain}
"""


def _context_md(req: Requirements) -> str:
    return f"""# Project Context

{req.context or "(no initial context provided)"}

This file holds durable facts agents should remember. Do not use it for
temporary task state — that belongs in `state.md`.
"""


def _state_md() -> str:
    return """# Current State

Status: NOT_STARTED
Current objective: Understand the project and establish the initial state.
Current task: Initial analysis.
Blockers: none
Known risks: none
Last decision: none
Next action: Read goal.md and constraints.md, then propose the first task.

(This file reflects current reality only. History belongs in telemetry.)
"""


def _backlog_md() -> str:
    return """# Backlog

## P0

- [ ] Understand project objective and Definition of Done
- [ ] Identify unknowns
- [ ] Define first actionable milestone

## P1

## P2

## Completed
"""


def _decisions_md() -> str:
    return """# Decisions

No decisions recorded yet. Each entry should include: decision, date,
context, alternatives considered, rationale, consequences, reversibility.

Decisions are things deliberately chosen. Facts discovered along the way
(what worked, what didn't, preferences) belong in `learnings.md` instead.
"""


def _learnings_md() -> str:
    return """# Learnings

Patterns, pitfalls, and preferences picked up while working on this
project — distinct from `decisions.md` (deliberate choices) and
`state.md` (current status). Keep entries short and specific enough to
be useful next session; prune ones that stop being true.

## Patterns that worked

## Pitfalls to avoid

## Preferences noticed
"""


def _constraints_md(req: Requirements, arch: Architecture) -> str:
    gates = "\n".join(f"- {g}" for g in arch.human_gates) or "- (none — low-risk, small-scale project)"
    return f"""# Constraints

{req.constraints or "(no explicit constraints provided)"}

## Human approval required for

{gates}

This list scales with the project (see `.agent/architecture.yaml`) — a
small personal project only gets the gates that actually apply to it.
"""


def _resources_md() -> str:
    return """# Resources

Record important external resources here as they become part of the
project. Prefer existing resources before requesting new ones.
"""


def _metrics_md(req: Requirements) -> str:
    return f"""# Metrics

Iterations: 0
Budget profile: {req.budget_profile}
Estimated cost so far: 0
Tasks completed: 0
Tasks failed: 0
Reviews passed: 0
Reviews failed: 0
Project status: NOT_STARTED
"""
