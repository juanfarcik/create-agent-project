---
type: agent-instructions
purpose: "Entry point every agent runtime reads first"
---

# autonomous-daily-agent — Agent Instructions

This is a **ops** project. Architecture: **autonomous-loop**.
Work pattern: **Auto (let the tool decide)**.

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

(These four are widely circulated as "Karpathy's AI coding rules" — see
this project's `docs/REFERENCES.md` for the actual attribution chain,
which is more nuanced than that name implies.)

## Loop — Auto (let the tool decide)

Derived from size/risk/lifetime/execution mode — no explicit pattern forced.

```
GOAL -> STATE -> GAP -> ACTION -> RESULT -> EVALUATE -> STATE UPDATE
```

Do not stop merely because one task finished. Continue until the
Definition of Done is met, a blocker needs a human, or budget/iteration
limits (`.project/metrics.md`) are reached.

## Agents available

- `orchestrator` (always, balanced) — Coordinates the project and decides the next highest-value action.
- `researcher` (on-demand, cheap) — Reduces uncertainty by gathering evidence and comparing alternatives.
- `evaluator` (always, balanced) — Independently verifies whether work actually meets the Definition of Done.

Role prompts live in `.agent/prompts/<role>.md`. Delegate only when a role
adds real value — a single agent handling everything is often correct.

## Human approval required for

- budget threshold
- irreversible actions

Record every non-trivial decision in `.project/decisions.md`, anything
worth remembering for next time in `.project/learnings.md`, and every
durable output under `.project/outputs/` (conversation is not the output).

## Growing the structure: nested AGENTS.md

If `.project/outputs/` grows real substructure (chapters, modules,
tracks, whatever the project's unit of work is), and a subfolder
accumulates its own context that doesn't belong in the project-wide
files above, drop a small `AGENTS.md` inside that subfolder explaining
just that subset. Claude Code and similar runtimes read `AGENTS.md`
hierarchically — the closer file adds to, not replaces, this one. Use
this when a subfolder's context would otherwise bloat this file or
`.project/context.md`; don't create one preemptively for every folder.

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
`create-agent-project validate` after manual edits.

## Where this structure comes from

This project was scaffolded by Agentic Project Architect
(GPLv3, https://github.com/juanfarcik/create-agent-project). Nothing
above was invented from scratch — the work pattern, role list, and
"stay disciplined" guidance all trace to real sources (papers, other
open-source projects, established prior art), documented in that repo's
`docs/REFERENCES.md`.
