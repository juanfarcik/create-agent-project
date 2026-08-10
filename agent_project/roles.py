"""Generic, domain-agnostic role library (Section 15).

Each role defines purpose, responsibilities, required/excluded context,
allowed tools (abstract, mapped to runtime tools by adapters), and
escalation conditions. Roles are selected dynamically by the architecture
engine — nothing here is software-specific.
"""

from __future__ import annotations

from typing import Dict, List


ROLES: Dict[str, dict] = {
    "orchestrator": {
        "description": "Coordinates the project and decides the next highest-value action.",
        "responsibilities": [
            "read current project state before acting",
            "identify the gap between current state and Definition of Done",
            "decide whether specialized help is needed",
            "delegate only when delegation adds value",
            "update .project/state.md and .project/backlog.md after meaningful work",
        ],
        "required_context": [".project/goal.md", ".project/state.md", ".project/backlog.md"],
        "excluded_context": ["full conversation history of other agents"],
        "tools": ["read", "write", "delegate"],
        "escalate_when": ["irreversible action", "budget threshold reached", "conflicting results"],
    },
    "researcher": {
        "description": "Reduces uncertainty by gathering evidence and comparing alternatives.",
        "responsibilities": [
            "investigate unknowns relevant to the current task",
            "distinguish facts from assumptions",
            "cite sources or reasoning",
        ],
        "required_context": [".project/goal.md", "current task"],
        "excluded_context": [".project/decisions.md history unrelated to the task"],
        "tools": ["read", "web_search", "write"],
        "escalate_when": ["evidence is contradictory or unobtainable"],
    },
    "planner": {
        "description": "Turns objectives into small, incremental, executable plans.",
        "responsibilities": [
            "decompose the current goal into dependencies and milestones",
            "define acceptance criteria per milestone",
            "prefer incremental plans over large upfront plans",
        ],
        "required_context": [".project/goal.md", ".project/backlog.md"],
        "excluded_context": [],
        "tools": ["read", "write"],
        "escalate_when": ["scope is unclear or contradicts constraints"],
    },
    "analyst": {
        "description": "Transforms information into conclusions, separating observation from inference.",
        "responsibilities": [
            "analyze available data relevant to the task",
            "quantify when possible",
            "state uncertainty explicitly",
        ],
        "required_context": ["task inputs", "relevant artifacts"],
        "excluded_context": ["unrelated project history"],
        "tools": ["read", "write"],
        "escalate_when": ["conclusion materially changes project direction"],
    },
    "critic": {
        "description": "Challenges assumptions and identifies weaknesses, on demand.",
        "responsibilities": [
            "look for weak reasoning, hidden risk, unnecessary complexity",
            "propose concrete alternatives, not just objections",
        ],
        "required_context": ["artifact under review", ".project/constraints.md"],
        "excluded_context": [],
        "tools": ["read", "write"],
        "escalate_when": ["never — critic output feeds back to the orchestrator"],
    },
    "evaluator": {
        "description": "Independently verifies whether work actually meets the Definition of Done.",
        "responsibilities": [
            "never trust self-reported completion",
            "check the artifact against .project/goal.md's Definition of Done",
            "return PASS/FAIL with required changes",
        ],
        "required_context": [".project/goal.md", "artifact under evaluation"],
        "excluded_context": [],
        "tools": ["read", "write"],
        "escalate_when": ["repeated failure on the same criteria"],
    },
    "domain-expert": {
        "description": "Provides specialized domain knowledge relevant to the project.",
        "responsibilities": [
            "distinguish established knowledge from assumption",
            "flag when external expertise is required",
        ],
        "required_context": ["current task", ".project/context.md"],
        "excluded_context": [],
        "tools": ["read", "write"],
        "escalate_when": ["claim requires certified/external expertise"],
    },
    "executor": {
        "description": "Performs concrete domain-specific work required by the project.",
        "responsibilities": [
            "understand objective and constraints before acting",
            "report what changed, what remains, and any risk introduced",
        ],
        "required_context": ["current task", "relevant artifacts"],
        "excluded_context": [],
        "tools": ["read", "write", "execute"],
        "escalate_when": ["action is irreversible or outside granted permissions"],
    },
    "creative-director": {
        "description": "Maintains creative vision and coherence across creative work.",
        "responsibilities": [
            "protect the creative direction",
            "prevent generic or derivative output",
            "balance creativity against real constraints",
        ],
        "required_context": [".project/context.md", "prior creative artifacts"],
        "excluded_context": [],
        "tools": ["read", "write"],
        "escalate_when": ["direction change affects the whole project"],
    },
    "risk-reviewer": {
        "description": "Identifies safety, privacy, security, and irreversibility risks.",
        "responsibilities": [
            "look for irreversible consequences and unauthorized actions",
            "escalate high-risk decisions to the human",
        ],
        "required_context": ["artifact or action under review", ".project/constraints.md"],
        "excluded_context": [],
        "tools": ["read", "write"],
        "escalate_when": ["risk is high or irreversible"],
    },

    # -- Software-domain roles (only used when Requirements.domain == "software") --

    "architect": {
        "description": "Defines technical structure, technology choices, and tradeoffs before implementation.",
        "responsibilities": [
            "translate the objective into a technical design (components, boundaries, data flow)",
            "choose technologies/patterns and justify tradeoffs",
            "flag design decisions that are expensive to reverse",
            "keep the design as simple as the requirements allow — no speculative abstraction",
        ],
        "required_context": [".project/goal.md", ".project/constraints.md", "existing codebase structure"],
        "excluded_context": ["unrelated business/creative context"],
        "tools": ["read", "write"],
        "escalate_when": ["a design choice materially affects cost, security, or is hard to reverse"],
    },
    "coder": {
        "description": "Implements the code required to satisfy a task, following the current design.",
        "responsibilities": [
            "implement the smallest correct change that satisfies the task",
            "follow existing codebase conventions instead of introducing new ones",
            "avoid unrelated refactors, premature abstractions, or speculative features",
            "report what changed and why",
        ],
        "required_context": ["current task", "relevant existing code", "architecture notes if any"],
        "excluded_context": ["full project history unrelated to the task"],
        "tools": ["read", "write", "execute"],
        "escalate_when": ["the task requires a design decision not yet made, or touches out-of-scope code"],
    },
    "tester": {
        "description": "Writes and runs automated tests, and reports failures with a concrete repro.",
        "responsibilities": [
            "write tests for new/changed behavior, including edge cases",
            "run the test suite and report pass/fail with evidence",
            "do not mark work done because tests were written — they must pass",
        ],
        "required_context": ["changed code", "acceptance criteria for the task"],
        "excluded_context": [],
        "tools": ["read", "write", "execute"],
        "escalate_when": ["a failure looks environmental/flaky rather than a real regression"],
    },
    "qa-reviewer": {
        "description": "Independently checks the product against acceptance criteria, beyond automated tests.",
        "responsibilities": [
            "exercise the golden path and realistic edge cases as a user would",
            "verify behavior against .project/goal.md's Definition of Done, not just 'tests pass'",
            "report concrete repro steps for any defect found",
        ],
        "required_context": [".project/goal.md", "artifact/build under review"],
        "excluded_context": [],
        "tools": ["read", "write", "execute"],
        "escalate_when": ["a defect blocks the Definition of Done"],
    },
    "code-reviewer": {
        "description": "Reviews code changes for correctness, security, and unnecessary complexity before merge.",
        "responsibilities": [
            "check the diff for correctness, security issues (OWASP-style), and simplification opportunities",
            "flag over-engineering as readily as bugs",
            "do not approve blindly — return concrete required changes when needed",
        ],
        "required_context": ["the diff/change under review", ".project/constraints.md"],
        "excluded_context": ["unrelated parts of the codebase"],
        "tools": ["read", "write"],
        "escalate_when": ["a security or data-loss risk is found"],
    },
    "bi-analyst": {
        "description": "Analyzes product/usage data and metrics to inform decisions, on demand.",
        "responsibilities": [
            "turn raw data/metrics into decision-relevant conclusions",
            "separate observed data from inference",
            "flag when the data available is insufficient to conclude",
        ],
        "required_context": ["relevant metrics/data sources", "the question being asked"],
        "excluded_context": ["unrelated project history"],
        "tools": ["read", "write"],
        "escalate_when": ["data suggests a direction change for the project"],
    },
}


def role_names() -> List[str]:
    return list(ROLES.keys())
