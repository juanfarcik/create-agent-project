"""Agentic work/loop patterns — the current common ways people structure
how an agent (or set of agents) actually operates, independent of which
roles exist.

This is deliberately explicit and user-selectable (not just inferred),
because "how autonomous / how supervised / how parallel should this feel"
is a decision people increasingly want to make directly, not have guessed
for them. Each pattern:

- suggests sane defaults for execution_mode / autonomy / human_involvement
  (used to pre-select wizard defaults — the user's explicit answers to
  those questions always win over the suggestion)
- may force certain roles to be present/always-on (a structural guarantee
  that holds regardless of what execution_mode ends up being)
- may set a floor on architecture size (a pattern that needs parallel
  workers doesn't make sense as a 1-agent "minimal" architecture)
- carries a short loop diagram used in the generated AGENTS.md so the
  agent (and the human) understand the operating model, not just the
  role list
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Dict, List, Optional, Tuple


@dataclass
class LoopPattern:
    id: str
    label: str
    description: str
    overrides: Dict[str, str] = field(default_factory=dict)
    force_roles: List[Tuple[str, str]] = field(default_factory=list)  # (role, mode)
    min_profile: Optional[str] = None
    note: Optional[str] = None
    loop_diagram: str = "GOAL -> STATE -> GAP -> ACTION -> RESULT -> EVALUATE -> STATE UPDATE"


PATTERNS: Dict[str, LoopPattern] = {
    p.id: p for p in [
        LoopPattern(
            id="auto",
            label="Auto (let the tool decide)",
            description="Derived from size/risk/lifetime/execution mode — no explicit pattern forced.",
        ),
        LoopPattern(
            id="interactive",
            label="Interactive",
            description="You drive every turn; the agent acts only when asked. The default for most sessions.",
            overrides={"execution_mode": "interactive", "autonomy": "collaborative"},
            loop_diagram="YOU ASK -> AGENT ACTS -> AGENT REPORTS -> YOU ASK",
        ),
        LoopPattern(
            id="agent-in-the-loop",
            label="Agent-in-the-loop",
            description=(
                "The agent runs its own think -> act -> observe loop autonomously across "
                "many steps; you're consulted on exceptions and irreversible actions, not every step."
            ),
            overrides={"execution_mode": "agent-loop", "autonomy": "mostly-autonomous", "human_involvement": "exceptions"},
            loop_diagram="THINK -> ACT -> OBSERVE -> THINK  (repeat until goal/blocker/budget limit)",
        ),
        LoopPattern(
            id="human-in-the-loop",
            label="Human-in-the-loop",
            description="The agent proposes each significant step and waits for your explicit approval before acting.",
            overrides={"human_involvement": "per-action", "autonomy": "collaborative"},
            loop_diagram="AGENT PROPOSES -> HUMAN APPROVES/EDITS -> AGENT ACTS -> AGENT PROPOSES NEXT",
        ),
        LoopPattern(
            id="human-on-the-loop",
            label="Human-on-the-loop (supervisory)",
            description=(
                "The agent acts autonomously and continuously; you monitor asynchronously "
                "and can step in, but it does not wait for you by default."
            ),
            overrides={"execution_mode": "continuous", "autonomy": "mostly-autonomous", "human_involvement": "exceptions"},
            loop_diagram="AGENT RUNS CONTINUOUSLY -> LOGS EVERY ACTION -> HUMAN REVIEWS ASYNC -> INTERVENES IF NEEDED",
        ),
        LoopPattern(
            id="plan-execute-review",
            label="Plan -> Execute -> Review",
            description="A dedicated planning pass before any execution, then an independent review pass after.",
            force_roles=[("planner", "always"), ("evaluator", "always")],
            loop_diagram="PLAN -> EXECUTE -> REVIEW -> (revise plan if REVIEW fails) -> EXECUTE -> ...",
        ),
        LoopPattern(
            id="debate-critic",
            label="Debate / Critic",
            description="Before finalizing, a critic actively challenges the result and proposes alternatives.",
            force_roles=[("critic", "always")],
            loop_diagram="PROPOSE -> CRITIQUE -> REVISE -> CRITIQUE -> ... -> CONVERGE",
        ),
        LoopPattern(
            id="reflexion-self-critique",
            label="Reflexion (self-critique)",
            description=(
                "A single agent generates, critiques its own output against the Definition "
                "of Done, and revises before reporting done — no separate critic agent."
            ),
            note="Self-critique loop: after producing an artifact, re-read it against the "
                 "Definition of Done and revise before reporting done. Do this at least once "
                 "per artifact, even without being asked.",
            loop_diagram="GENERATE -> SELF-CRITIQUE -> REVISE -> (repeat once) -> REPORT",
        ),
        LoopPattern(
            id="swarm-parallel",
            label="Swarm (parallel workers)",
            description="Independent subtasks run in parallel across specialists, then get consolidated.",
            min_profile="collaborative",
            note="Parallel execution: independent tasks may be delegated concurrently. "
                 "The orchestrator consolidates results and resolves conflicts before "
                 "reporting — never merge conflicting outputs silently.",
            loop_diagram="ORCHESTRATOR SPLITS WORK -> WORKERS RUN IN PARALLEL -> ORCHESTRATOR CONSOLIDATES",
        ),
        LoopPattern(
            id="blackboard",
            label="Blackboard (shared state, opportunistic)",
            description=(
                "Agents don't hand off directly — they read/write a shared project state and "
                "act whenever they have something useful to contribute, in any order."
            ),
            note="Blackboard coordination: do not wait for explicit handoff. Check "
                 "`.project/state.md` and `.project/backlog.md` for anything you can usefully "
                 "act on, act, then update state for others.",
            loop_diagram="SHARED STATE <-> AGENT A / AGENT B / AGENT C  (each acts opportunistically)",
        ),
        LoopPattern(
            id="scheduled-digest",
            label="Scheduled digest",
            description="Runs on a fixed schedule (e.g. daily) and produces a digest/report each run.",
            overrides={"execution_mode": "scheduled"},
            loop_diagram="TRIGGER (schedule) -> RUN -> PRODUCE ARTIFACT -> STOP UNTIL NEXT TRIGGER",
        ),
        LoopPattern(
            id="reactive-event-driven",
            label="Reactive / event-driven",
            description="Runs only when triggered by an external event (a file change, a webhook, a new item).",
            overrides={"execution_mode": "event-driven"},
            loop_diagram="EVENT -> HANDLE -> UPDATE STATE -> WAIT FOR NEXT EVENT",
        ),
    ]
}


def get(pattern_id: str) -> LoopPattern:
    return PATTERNS.get(pattern_id, PATTERNS["auto"])


def choices() -> List[Tuple[str, str]]:
    """(id, label) pairs in a sensible display order for wizards."""
    order = [
        "auto", "interactive", "agent-in-the-loop", "human-in-the-loop",
        "human-on-the-loop", "plan-execute-review", "debate-critic",
        "reflexion-self-critique", "swarm-parallel", "blackboard",
        "scheduled-digest", "reactive-event-driven",
    ]
    return [(pid, PATTERNS[pid].label) for pid in order]
