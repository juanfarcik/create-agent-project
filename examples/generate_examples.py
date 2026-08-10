#!/usr/bin/env python3
"""Regenerates the reference examples committed under examples/.

Run from the repo root: python3 examples/generate_examples.py

Each example demonstrates a different requirements profile resolving to a
different architecture — not every example is multi-agent on purpose
(Section 77): the point is to show the range, minimal to full team.
"""

import shutil
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from agent_project import adapters, architecture as arch_mod, scaffold
from agent_project.models import Requirements

EXAMPLES_DIR = Path(__file__).resolve().parent

EXAMPLES = [
    Requirements(
        name="personal-research",
        objective="Every morning, spend 30 minutes researching experimental "
                   "jazz production techniques and leave a concise report.",
        domain="research",
        definition_of_done="A short daily report exists under .project/outputs/ "
                            "with at least one concrete, actionable technique.",
        size="tiny", lifetime="long-running", autonomy="mostly-autonomous",
        risk="low", budget_profile="ultra-low", execution_mode="scheduled",
        runtime="claude-code", human_involvement="none",
        schedule="daily 08:00, max 30 minutes, max $0.50/day",
        experience_level="beginner",
    ),
    Requirements(
        name="creative-project",
        objective="Create a six-track experimental album.",
        domain="creative",
        definition_of_done="Six mixed tracks, mastered, with a short release plan.",
        size="medium", lifetime="weeks", autonomy="collaborative",
        risk="low", budget_profile="hobby", execution_mode="interactive",
        runtime="claude-code", human_involvement="important-decisions",
        experience_level="beginner",
    ),
    Requirements(
        name="business-research",
        objective="Research a market opportunity for a new product, analyze "
                   "competitors, estimate financial viability, and produce a "
                   "strategy that keeps updating as new information arrives.",
        domain="business",
        definition_of_done="A strategy document exists with market sizing, "
                            "competitor analysis, financial viability estimate, "
                            "and it is kept current in .project/outputs/.",
        size="medium", lifetime="weeks", autonomy="collaborative",
        risk="medium", budget_profile="balanced", execution_mode="agent-loop",
        runtime="claude-code", human_involvement="important-decisions",
        experience_level="tech",
    ),
    Requirements(
        name="software-project",
        objective="Build and ship a small SaaS billing dashboard.",
        domain="software",
        definition_of_done="Dashboard is deployed, automated tests pass, and "
                            "QA has verified the golden path plus edge cases.",
        size="medium", lifetime="weeks", autonomy="collaborative",
        risk="medium", budget_profile="balanced", execution_mode="interactive",
        runtime="all", human_involvement="per-phase",
        experience_level="tech",
    ),
    Requirements(
        name="autonomous-daily-agent",
        objective="Continuously monitor a project's backlog, pick the "
                   "highest-value next action, and execute it every day "
                   "without supervision unless something risky comes up.",
        domain="ops",
        definition_of_done="Backlog trends toward zero; every action taken is "
                            "logged in .project/decisions.md.",
        size="small", lifetime="long-running", autonomy="autonomous",
        risk="medium", budget_profile="hobby", execution_mode="continuous",
        runtime="claude-code", human_involvement="exceptions",
        experience_level="tech",
    ),
]


def main() -> None:
    for req in EXAMPLES:
        root = EXAMPLES_DIR / req.name
        if root.exists():
            shutil.rmtree(root)
        arch = arch_mod.recommend(req)
        scaffold.generate(root, req, arch)
        adapters.generate(root, req, arch)
        print(f"{req.name:<24} -> {arch.profile} ({len(arch.agents)} agents)")


if __name__ == "__main__":
    main()
