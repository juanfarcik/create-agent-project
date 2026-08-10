# Agentic Project Architect (Python prototype — see [dotnet/](dotnet/) for the maintained version)

> **This Python implementation is the original prototype.** Active
> development has moved to the C#/.NET port in [`dotnet/`](dotnet/),
> which is the version to build on going forward (GPLv3, same design,
> same test coverage). This directory is kept for reference and will be
> phased out.

Domain-agnostic scaffold generator for agentic projects. It asks what
you're trying to accomplish in plain language, designs the **smallest
architecture that fits** (from a single agent to a full multi-role team),
and generates a portable project you open directly with **Claude Code**,
**OpenCode**, **Codex CLI**, or any other `AGENTS.md`-compatible runtime.

It is not another multi-agent framework. It doesn't run agents — it
designs the workspace they run in, and gets out of the way.

## Why

Most "AI agent templates" start from the architecture (how many agents,
which topology) and force the project into it. This tool starts from
requirements — size, risk, autonomy, budget, lifetime — and derives the
architecture. A 30-minutes-a-day research task gets one agent. A
high-stakes software launch gets an architect, coder, tester, QA, code
reviewer, and human approval gates. Nothing is generated that the project
doesn't need.

## Install

```bash
pip install -e .
# or, without installing:
python3 agent-project.py new
```

Requires Python 3.9+. No dependencies.

## Quick start

```bash
agent-project new
```

The wizard has exactly two paths:

- **Simple** (`--simple`) — for non-technical creators (writers, designers,
  musicians, researchers, anyone). A handful of plain-language questions,
  everything else defaults sensibly. This is not just for programmers —
  Claude Code, OpenCode, and similar CLIs work equally well for any
  project that produces file-based output.
- **Advanced** (`--advanced`) — full explicit control over size, risk,
  lifetime, execution mode, schedule, and budget.

Either way you get an architecture preview with an explanation before
anything is written to disk. Then:

```bash
cd my-project
claude   # or opencode, or codex — they all read AGENTS.md automatically
```

See the generated `GETTING_STARTED.md` inside each project for exact
next steps, tailored to whether you picked the beginner or technical flow.

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
│   ├── goal.md, state.md, backlog.md, decisions.md, constraints.md, ...
│   └── outputs/ research/ plans/ reviews/ checkpoints/ telemetry/
├── .claude/agents/*.md      # generated if targeting Claude Code
├── .opencode/agent/*.md     # generated if targeting OpenCode
└── .codex/                  # generated if targeting Codex CLI
```

`.agent/` (how) and `.project/` (what) are kept separate on purpose —
agents are workers, project state is the source of truth.

## Managing a generated project

```bash
agent-project validate      <path>              # check consistency
agent-project status        <path>               # current state + metrics
agent-project architecture  <path> --recommend    # current vs. what requirements now suggest
agent-project optimize      <path> [--apply]      # explain and strip unjustified complexity
agent-project compare                              # table of all built-in profiles
agent-project templates                            # list built-in architecture profiles
agent-project patterns                             # list built-in work patterns
```

## Built-in architecture profiles

| Profile | Agents | When |
|---|---|---|
| `minimal` | 1 | simple, low-risk, low-budget |
| `lean` | 2-3 | some specialization is useful |
| `research` | 3-4 | evidence gathering, synthesis |
| `collaborative` | 4 | parallel work has real value |
| `autonomous-loop` | 3 | scheduled/continuous execution |
| `high-reliability` | 7 | high risk, needs review + human gates |
| `software-lean` | up to 4 | small software project |
| `software-standard` | up to 6 | typical product build |
| `software-high-reliability` | up to 9 | production software, high stakes |

Software profiles bring domain-specific roles: `architect`, `coder`,
`tester`, `qa-reviewer`, `code-reviewer`, `bi-analyst`.

## Work patterns — how the agent actually operates with you

Separate from *which roles exist* is *how they work with you*. This is
selectable explicitly in both wizard modes (`agent_project/patterns.py`):

| Pattern | What it means |
|---|---|
| `interactive` | You drive every turn; the agent acts only when asked |
| `agent-in-the-loop` | Autonomous think→act→observe loop; you're consulted on exceptions |
| `human-in-the-loop` | Agent proposes each step, waits for your approval |
| `human-on-the-loop` | Agent runs continuously and autonomously; you supervise asynchronously |
| `plan-execute-review` | Dedicated planning pass, then execution, then independent review |
| `debate-critic` | A critic always challenges the result before it's final |
| `reflexion-self-critique` | Single agent self-critiques its own output before reporting done |
| `swarm-parallel` | Independent subtasks run in parallel, then get consolidated |
| `blackboard` | Agents act opportunistically on shared state instead of explicit handoff |
| `scheduled-digest` | Runs on a fixed schedule, produces a report each time |
| `reactive-event-driven` | Runs only when triggered by an external event |

Picking a pattern shapes the wizard's suggested defaults (execution mode,
autonomy, human involvement) *and* gives structural guarantees the
architecture engine enforces regardless of those — e.g. `debate-critic`
always keeps a critic always-on, `swarm-parallel` never generates fewer
than a `collaborative`-sized team. `agent-project patterns` lists all of
them with descriptions.

Requirements are always evaluated first (`agent_project/architecture.py:recommend`) —
the CLI never calls an LLM to decide architecture. Deterministic, free,
inspectable.

## Development

```bash
python3 -m unittest discover -s tests -v
```

## Design principles

1. Minimum architecture required for the objective — never more.
2. Project state (`.project/`) is the source of truth, not conversation.
3. Agents get only the context they need, not everything.
4. Never trust self-reported completion — evaluate against Definition of Done.
5. Keep the generator itself deterministic and dependency-free.

## License

GPLv3 — see [LICENSE](LICENSE).
