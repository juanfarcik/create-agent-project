"""Architecture profiles, recommendation engine, and optimizer.

Deterministic and rule-based on purpose (Section 70): a basic project must
be generated without calling an LLM. Requirements -> architecture is a
lookup + a small set of adjustment rules, not a model call.
"""

from __future__ import annotations

from typing import List

from . import patterns as patterns_mod
from .models import AgentSpec, Architecture, Requirements

SIZE_ORDER = ["tiny", "small", "medium", "large"]
RISK_ORDER = ["low", "medium", "high", "critical"]
AUTONOMY_ORDER = ["human", "collaborative", "mostly-autonomous", "autonomous"]
LIFETIME_ORDER = ["session", "days", "weeks", "long-running"]


def _idx(order: List[str], value: str, default: int = 0) -> int:
    try:
        return order.index(value)
    except ValueError:
        return default


# ---------------------------------------------------------------------------
# Built-in profiles (Section 10)
# ---------------------------------------------------------------------------

def _profile_minimal() -> Architecture:
    return Architecture(
        profile="minimal",
        agents=[AgentSpec("orchestrator", "always", "balanced")],
        memory="filesystem",
        human_gates=["irreversible actions"],
        checkpoints=False,
        complexity="LOW",
        est_calls_per_run="1-4",
        est_context="LOW",
        est_cost="LOW",
        notes=["Single agent handles everything. No specialization needed."],
    )


def _profile_lean() -> Architecture:
    return Architecture(
        profile="lean",
        agents=[
            AgentSpec("orchestrator", "always", "balanced"),
            AgentSpec("researcher", "on-demand", "cheap"),
            AgentSpec("critic", "on-demand", "balanced"),
        ],
        memory="filesystem",
        human_gates=["irreversible actions", "budget threshold"],
        checkpoints=False,
        complexity="LOW",
        est_calls_per_run="4-8",
        est_context="LOW",
        est_cost="LOW",
        notes=["One orchestrator plus specialists invoked only when useful."],
    )


def _profile_collaborative() -> Architecture:
    return Architecture(
        profile="collaborative",
        agents=[
            AgentSpec("orchestrator", "always", "strong"),
            AgentSpec("researcher", "always", "cheap"),
            AgentSpec("analyst", "always", "balanced"),
            AgentSpec("critic", "on-demand", "balanced"),
        ],
        memory="filesystem",
        human_gates=["irreversible actions", "budget threshold", "conflicting agent results"],
        checkpoints=True,
        complexity="MEDIUM",
        est_calls_per_run="10-20",
        est_context="MEDIUM",
        est_cost="MEDIUM",
        notes=["Supervisor + workers. Justified when parallel work has real value."],
    )


def _profile_research() -> Architecture:
    return Architecture(
        profile="research",
        agents=[
            AgentSpec("orchestrator", "always", "balanced"),
            AgentSpec("researcher", "always", "cheap"),
            AgentSpec("analyst", "always", "balanced"),
            AgentSpec("critic", "on-demand", "balanced"),
        ],
        memory="filesystem",
        human_gates=["irreversible actions"],
        checkpoints=False,
        complexity="LOW",
        est_calls_per_run="5-10",
        est_context="LOW",
        est_cost="LOW",
        notes=["Researcher -> Analyst -> Critic -> Synthesis, for evidence-heavy work."],
    )


def _profile_autonomous_loop() -> Architecture:
    return Architecture(
        profile="autonomous-loop",
        agents=[
            AgentSpec("orchestrator", "always", "balanced"),
            AgentSpec("researcher", "on-demand", "cheap"),
            AgentSpec("evaluator", "always", "balanced"),
        ],
        memory="filesystem",
        human_gates=["irreversible actions", "budget threshold"],
        checkpoints=True,
        complexity="MEDIUM",
        est_calls_per_run="4-8/run",
        est_context="LOW",
        est_cost="LOW-MEDIUM",
        notes=["Persistent state + scheduled/continuous execution with checkpoints."],
    )


def _profile_high_reliability() -> Architecture:
    return Architecture(
        profile="high-reliability",
        agents=[
            AgentSpec("orchestrator", "always", "strong"),
            AgentSpec("planner", "always", "strong"),
            AgentSpec("researcher", "always", "cheap"),
            AgentSpec("executor", "always", "balanced"),
            AgentSpec("critic", "always", "strong"),
            AgentSpec("evaluator", "always", "strong"),
            AgentSpec("risk-reviewer", "always", "strong"),
        ],
        memory="filesystem",
        human_gates=[
            "irreversible actions", "budget threshold", "external publication",
            "conflicting agent results", "final deliverable",
        ],
        checkpoints=True,
        complexity="HIGH",
        est_calls_per_run="20-40",
        est_context="HIGH",
        est_cost="HIGH",
        notes=["Planner + Workers + Critic + Evaluator + human gates. Use only when justified."],
    )


def _profile_software_lean() -> Architecture:
    return Architecture(
        profile="software-lean",
        agents=[
            AgentSpec("orchestrator", "always", "balanced"),
            AgentSpec("coder", "always", "balanced"),
            AgentSpec("tester", "on-demand", "cheap"),
            AgentSpec("code-reviewer", "on-demand", "balanced"),
        ],
        memory="filesystem",
        human_gates=["irreversible actions"],
        checkpoints=False,
        complexity="LOW",
        est_calls_per_run="4-10",
        est_context="LOW",
        est_cost="LOW",
        notes=["Small software project: one implementer, tests/review invoked when useful."],
    )


def _profile_software_standard() -> Architecture:
    return Architecture(
        profile="software-standard",
        agents=[
            AgentSpec("orchestrator", "always", "balanced"),
            AgentSpec("architect", "on-demand", "strong"),
            AgentSpec("coder", "always", "balanced"),
            AgentSpec("tester", "always", "cheap"),
            AgentSpec("code-reviewer", "always", "balanced"),
            AgentSpec("bi-analyst", "on-demand", "cheap"),
        ],
        memory="filesystem",
        human_gates=["irreversible actions", "budget threshold"],
        checkpoints=True,
        complexity="MEDIUM",
        est_calls_per_run="10-25",
        est_context="MEDIUM",
        est_cost="MEDIUM",
        notes=["Standard product build: design on-demand, implementation with tests and review as gates."],
    )


def _profile_software_high_reliability() -> Architecture:
    return Architecture(
        profile="software-high-reliability",
        agents=[
            AgentSpec("orchestrator", "always", "strong"),
            AgentSpec("architect", "always", "strong"),
            AgentSpec("planner", "always", "balanced"),
            AgentSpec("coder", "always", "balanced"),
            AgentSpec("tester", "always", "cheap"),
            AgentSpec("qa-reviewer", "always", "balanced"),
            AgentSpec("code-reviewer", "always", "strong"),
            AgentSpec("risk-reviewer", "on-demand", "strong"),
            AgentSpec("bi-analyst", "on-demand", "cheap"),
        ],
        memory="filesystem",
        human_gates=[
            "irreversible actions", "budget threshold", "external publication",
            "final deliverable", "production deploys",
        ],
        checkpoints=True,
        complexity="HIGH",
        est_calls_per_run="25-50",
        est_context="HIGH",
        est_cost="HIGH",
        notes=["Full software team: design, build, automated tests, QA, and code review gates."],
    )


PROFILES = {
    "minimal": _profile_minimal,
    "lean": _profile_lean,
    "collaborative": _profile_collaborative,
    "research": _profile_research,
    "autonomous-loop": _profile_autonomous_loop,
    "high-reliability": _profile_high_reliability,
    "software-lean": _profile_software_lean,
    "software-standard": _profile_software_standard,
    "software-high-reliability": _profile_software_high_reliability,
}


def build_profile(name: str) -> Architecture:
    if name not in PROFILES:
        raise ValueError(f"Unknown architecture profile: {name}")
    return PROFILES[name]()


# ---------------------------------------------------------------------------
# Recommendation engine (Section 9) — requirements -> profile
# ---------------------------------------------------------------------------

def recommend(req: Requirements) -> Architecture:
    size = _idx(SIZE_ORDER, req.size, 1)
    risk = _idx(RISK_ORDER, req.risk, 0)
    autonomy = _idx(AUTONOMY_ORDER, req.autonomy, 1)
    lifetime = _idx(LIFETIME_ORDER, req.lifetime, 0)

    score = size + risk + (lifetime >= 2)

    if req.domain == "software":
        if score <= 1 and risk == 0:
            arch = build_profile("software-lean")
        elif risk >= 2 or score >= 5:
            arch = build_profile("software-high-reliability")
        else:
            arch = build_profile("software-standard")
        return _finalize(arch, req, size, risk, lifetime)

    if req.domain == "research":
        arch = build_profile("research")
    elif score <= 1 and risk == 0:
        arch = build_profile("minimal")
    elif score <= 2:
        arch = build_profile("lean")
    elif risk >= 2 or score >= 5:
        arch = build_profile("high-reliability")
    elif size >= 2:
        arch = build_profile("collaborative")
    else:
        arch = build_profile("lean")

    # Creative work (writing/design/music/art/...) values direction and
    # coherence over generic data analysis — swap in the creative-director
    # role wherever the generic profile would have used an analyst.
    if req.domain == "creative":
        for a in arch.agents:
            if a.role == "analyst":
                a.role = "creative-director"

    return _finalize(arch, req, size, risk, lifetime)


def _finalize(arch: Architecture, req: Requirements, size: int, risk: int, lifetime: int) -> Architecture:
    # Long-running / scheduled / continuous execution needs persistence.
    if req.execution_mode in ("scheduled", "continuous", "event-driven") or lifetime >= 2:
        if arch.profile in ("minimal", "lean"):
            arch = build_profile("autonomous-loop")
        arch.checkpoints = True

    # High/critical risk always adds a risk gate, regardless of profile.
    if risk >= 2:
        if "risk-reviewer" not in arch.agent_names():
            arch.agents.append(AgentSpec("risk-reviewer", "on-demand", "strong"))
        arch.human_gates.append("high-risk decisions")

    # Autonomy shapes human gates, not agent count.
    gate_by_autonomy = {
        "human": ["every action"],
        "collaborative": ["important decisions"],
        "mostly-autonomous": ["irreversible actions", "budget threshold"],
        "autonomous": ["irreversible actions"],
    }
    arch.human_gates = sorted(set(arch.human_gates) | set(gate_by_autonomy.get(req.autonomy, [])))

    arch = _apply_loop_pattern(arch, req)
    _estimate_cost(arch, req)
    return arch


def _apply_loop_pattern(arch: Architecture, req: Requirements) -> Architecture:
    """Apply the structural guarantees of an explicitly chosen work pattern
    (Section: agent-in-the-loop / human-in-the-loop / debate / swarm / ...).
    Independent of execution_mode/autonomy — those only seed wizard
    defaults; a pattern's role and topology guarantees hold regardless."""
    pattern = patterns_mod.get(req.loop_pattern)
    if pattern.id == "auto":
        return arch

    if pattern.min_profile and len(arch.agents) < len(build_profile(pattern.min_profile).agents):
        bigger = build_profile(pattern.min_profile)
        bigger.checkpoints = arch.checkpoints
        bigger.human_gates = arch.human_gates
        bigger.notes = arch.notes
        arch = bigger

    for role, mode in pattern.force_roles:
        existing = next((a for a in arch.agents if a.role == role), None)
        if existing:
            existing.mode = mode
        else:
            arch.agents.append(AgentSpec(role, mode, "balanced"))

    if pattern.note:
        arch.notes.append(f"Pattern ({pattern.label}): {pattern.note}")

    arch.loop_pattern = pattern.id
    return arch


def _estimate_cost(arch: Architecture, req: Requirements) -> None:
    tiers = {"cheap": 1, "balanced": 2, "strong": 3}
    weight = sum(tiers.get(a.model_tier, 2) for a in arch.agents)
    budget_cap = {
        "free": 0, "ultra-low": 1, "hobby": 2, "balanced": 3,
        "quality-first": 4, "custom": 4,
    }.get(req.budget_profile, 2)
    if weight > budget_cap * 3 + 3:
        arch.notes.append(
            f"WARNING: architecture weight ({weight}) is high for budget profile "
            f"'{req.budget_profile}'. Consider --optimize."
        )


# ---------------------------------------------------------------------------
# Optimizer (Section 13) — remove unjustified complexity
# ---------------------------------------------------------------------------

def optimize(arch: Architecture, req: Requirements) -> Architecture:
    """Return a new, possibly smaller, Architecture with an explanation trail."""
    removed = []
    agents = list(arch.agents)

    risk = _idx(RISK_ORDER, req.risk, 0)
    size = _idx(SIZE_ORDER, req.size, 1)
    cheap_budget = req.budget_profile in ("free", "ultra-low", "hobby")

    # Rule 1: drop always-on critic/evaluator to on-demand for low-risk, small projects.
    if risk == 0 and size <= 1:
        for a in agents:
            if a.role in ("critic", "evaluator") and a.mode == "always":
                a.mode = "on-demand"
                removed.append(f"set '{a.role}' to on-demand (low risk, small project)")

    # Rule 2: collapse duplicate/underused specialist roles when budget is tight.
    # Roles that actually produce the project's output are never removed —
    # only demoted to on-demand — so the architecture stays capable of doing
    # the work. Pure oversight/review roles can be dropped entirely.
    CORE_DOER_ROLES = {"orchestrator", "researcher", "executor", "coder", "evaluator"}
    if cheap_budget:
        pruned = []
        for a in agents:
            if a.role in CORE_DOER_ROLES or a.mode == "on-demand":
                pruned.append(a)  # core roles kept; on-demand agents cost nothing until invoked
            else:
                a.mode = "on-demand"
                pruned.append(a)
                removed.append(f"set always-on '{a.role}' to on-demand (tight budget, low marginal value)")
        agents = pruned

    # Rule 3: downgrade model tier for non-critical roles under tight budget.
    if cheap_budget:
        for a in agents:
            if a.role not in ("orchestrator", "evaluator", "risk-reviewer") and a.model_tier == "strong":
                a.model_tier = "balanced"
                removed.append(f"downgraded '{a.role}' model tier to balanced")

    # Rule 4: strip checkpoints if the project is a single session.
    checkpoints = arch.checkpoints
    if req.lifetime == "session" and checkpoints:
        checkpoints = False
        removed.append("disabled checkpoints (single-session project)")

    optimized = Architecture(
        profile=arch.profile,
        agents=agents,
        memory=arch.memory,
        human_gates=arch.human_gates,
        checkpoints=checkpoints,
        complexity="LOW" if len(agents) <= 2 else arch.complexity,
        est_calls_per_run=arch.est_calls_per_run,
        est_context=arch.est_context,
        est_cost="LOW" if cheap_budget else arch.est_cost,
        notes=list(arch.notes),
    )
    if removed:
        optimized.notes.append("Optimizer changes:")
        optimized.notes.extend(f"  - {r}" for r in removed)
    else:
        optimized.notes.append("Optimizer: architecture already minimal for these requirements.")
    return optimized


def compare_table() -> str:
    rows = [
        ("Architecture", "Agents", "Cost", "Complexity", "Reliability"),
        ("minimal", "1", "$", "LOW", "MEDIUM"),
        ("lean", "2-3", "$$", "LOW", "HIGH"),
        ("research", "3-4", "$$", "LOW", "HIGH"),
        ("collaborative", "4", "$$$", "MEDIUM", "HIGH"),
        ("autonomous-loop", "3", "$$-$$$", "MEDIUM", "HIGH"),
        ("high-reliability", "7", "$$$$", "HIGH", "VERY HIGH"),
    ]
    widths = [max(len(r[i]) for r in rows) for i in range(5)]
    lines = []
    for r in rows:
        lines.append("  ".join(c.ljust(widths[i]) for i, c in enumerate(r)))
    return "\n".join(lines)
