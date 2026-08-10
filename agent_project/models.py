"""Domain model. Runtime-independent concepts only (Section 72).

Kept intentionally small: this is what the CLI needs to reason about
requirements -> architecture -> scaffold. Not a general framework.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import List, Optional


# ---------------------------------------------------------------------------
# Requirements (Section 6/7) — what the user needs, expressed without jargon
# ---------------------------------------------------------------------------

@dataclass
class Requirements:
    name: str
    objective: str
    domain: str = "general"          # general | software | research | creative | business | ops
    definition_of_done: str = ""
    context: str = ""
    constraints: str = ""

    size: str = "small"              # tiny | small | medium | large
    lifetime: str = "session"        # session | days | weeks | long-running
    autonomy: str = "collaborative"  # human | collaborative | mostly-autonomous | autonomous
    risk: str = "low"                # low | medium | high | critical

    budget_profile: str = "hobby"    # free | ultra-low | hobby | balanced | quality-first | custom
    execution_mode: str = "interactive"  # interactive | agent-loop | scheduled | continuous | event-driven

    runtime: str = "claude-code"     # claude-code | opencode | codex-cli | all
    human_involvement: str = "important-decisions"  # none | exceptions | important-decisions | per-phase | per-action

    schedule: Optional[str] = None   # e.g. "daily 08:00, max 30m, max $0.50/day"
    experience_level: str = "beginner"  # beginner | tech

    loop_pattern: str = "auto"       # see agent_project.patterns.LOOP_PATTERNS


# ---------------------------------------------------------------------------
# Architecture (Section 10/72)
# ---------------------------------------------------------------------------

@dataclass
class AgentSpec:
    role: str
    mode: str = "always"   # always | on-demand
    model_tier: str = "balanced"  # cheap | balanced | strong


@dataclass
class Architecture:
    profile: str
    agents: List[AgentSpec] = field(default_factory=list)
    memory: str = "filesystem"
    human_gates: List[str] = field(default_factory=list)
    checkpoints: bool = False
    complexity: str = "LOW"       # LOW | MEDIUM | HIGH | VERY HIGH
    est_calls_per_run: str = "1-4"
    est_context: str = "LOW"
    est_cost: str = "LOW"
    notes: List[str] = field(default_factory=list)
    loop_pattern: str = "auto"

    def agent_names(self) -> List[str]:
        return [a.role for a in self.agents]
