"""Interactive wizard with exactly two entry points (Section 5):

- `simple()`   — for people who don't know or care about "agent architecture".
                 Writers, designers, musicians, researchers, anyone with a
                 goal. A handful of plain-language questions, everything
                 else defaults sensibly.
- `advanced()` — for people who want to set size/risk/lifetime/execution
                 mode/schedule/budget explicitly.

Both funnel into the same `Requirements` model and the same deterministic
recommendation engine — the only difference is how much you're asked.
"""

from __future__ import annotations

import re

from . import patterns as patterns_mod
from .models import Requirements

# A short, friendly subset for Simple mode — the full list (with
# descriptions) is only shown in Advanced mode via patterns.choices().
SIMPLE_PATTERN_CHOICES = [
    ("auto", "Not sure — let the tool decide"),
    ("interactive", "I'll be driving — ask before each thing"),
    ("agent-in-the-loop", "Let it work through a to-do list on its own, check with me only if stuck"),
    ("human-in-the-loop", "Propose each step and wait for my OK"),
    ("scheduled-digest", "Run on a schedule (e.g. every morning) and give me a report"),
]


def _slugify(text: str) -> str:
    slug = re.sub(r"[^a-z0-9]+", "-", text.strip().lower()).strip("-")
    return slug[:40] or "my-project"


def _ask(question: str, default: str = "") -> str:
    suffix = f" [{default}]" if default else ""
    value = input(f"{question}{suffix}: ").strip()
    return value or default


def _choose(question: str, options: list, default_idx: int = 0) -> str:
    print(f"\n{question}")
    for i, (val, label) in enumerate(options, 1):
        marker = " (default)" if i - 1 == default_idx else ""
        print(f"  {i}. {label}{marker}")
    raw = input(f"> [{default_idx + 1}]: ").strip()
    if not raw:
        return options[default_idx][0]
    try:
        idx = int(raw) - 1
        if 0 <= idx < len(options):
            return options[idx][0]
    except ValueError:
        pass
    return options[default_idx][0]


# Friendly, persona-oriented labels shown to the user; internal `domain`
# values are what the architecture engine actually branches on, so a
# writer, a designer, and a musician all map to the same "creative"
# handling without needing separate engine logic.
DOMAIN_CHOICES = [
    ("software", "Building software / an app"),
    ("writing", "Writing (book, blog, docs, scripts)"),
    ("design", "Design (visual, product, UX)"),
    ("creative", "Music, art, video, or other creative work"),
    ("research", "Research / learning about something"),
    ("business", "Business / strategy / market analysis"),
    ("ops", "Operations / recurring tasks"),
    ("general", "Not sure / something else"),
]

_DOMAIN_TO_ENGINE = {
    "writing": "creative",
    "design": "creative",
    "creative": "creative",
}


def _engine_domain(chosen: str) -> str:
    return _DOMAIN_TO_ENGINE.get(chosen, chosen)


def _idx_of(options: list, value: str, fallback: int) -> int:
    for i, (val, _label) in enumerate(options):
        if val == value:
            return i
    return fallback


def _runtime_choice() -> str:
    return _choose("Where will you open this project?", [
        ("claude-code", "Claude Code"),
        ("opencode", "OpenCode"),
        ("codex-cli", "Codex CLI"),
        ("all", "Not sure yet — generate for all of them"),
    ], default_idx=3)


# ---------------------------------------------------------------------------
# Simple — for people who don't know anything about agent architecture
# ---------------------------------------------------------------------------

def simple() -> Requirements:
    print("\n== Let's set up your project ==")
    print("(Just answer in plain language — we'll figure out the rest.)\n")

    objective = _ask("What do you want to accomplish?")
    name = _ask("Give it a short name", _slugify(objective) if objective else "my-project")
    domain_choice = _choose("What kind of project is this?", DOMAIN_CHOICES)
    dod = _ask("How will you know it's done? (optional)")
    pattern_id = _choose("How should it work with you?", SIMPLE_PATTERN_CHOICES)
    runtime = _runtime_choice()

    pattern = patterns_mod.get(pattern_id)
    overrides = pattern.overrides

    return Requirements(
        name=_slugify(name),
        objective=objective or "Not yet defined — refine in .project/goal.md",
        domain=_engine_domain(domain_choice),
        definition_of_done=dod,
        size="small", lifetime="session",
        autonomy=overrides.get("autonomy", "collaborative"),
        risk="low", budget_profile="hobby",
        execution_mode=overrides.get("execution_mode", "interactive"),
        runtime=runtime,
        human_involvement=overrides.get("human_involvement", "important-decisions"),
        loop_pattern=pattern_id,
        experience_level="beginner",
    )


# ---------------------------------------------------------------------------
# Advanced — full explicit control
# ---------------------------------------------------------------------------

def advanced() -> Requirements:
    print("\n== Advanced setup ==")
    print("(Every field below shapes the generated architecture.)\n")

    objective = _ask("What do you want to accomplish?")
    name = _ask("Project name", _slugify(objective) if objective else "my-project")
    domain_choice = _choose("Project domain", DOMAIN_CHOICES)
    dod = _ask("Definition of Done")
    context = _ask("Initial context (optional)")
    constraints = _ask("Constraints (optional)")

    print("\nWork patterns (agent-in-the-loop, human-in-the-loop, swarm, debate, ...):")
    for pid, _label in patterns_mod.choices():
        print(f"  - {pid}: {patterns_mod.PATTERNS[pid].description}")
    pattern_id = _choose("Work pattern", patterns_mod.choices())
    pattern = patterns_mod.PATTERNS[pattern_id]
    overrides = pattern.overrides
    print(f"(Picking sensible defaults below for '{pattern.label}' — override anything you like.)")

    size = _choose("Project size", [
        ("tiny", "Personal / tiny"), ("small", "Small"),
        ("medium", "Medium"), ("large", "Large"),
    ], default_idx=1)

    lifetime = _choose("Project lifetime", [
        ("session", "One session"), ("days", "Several days"),
        ("weeks", "Several weeks"), ("long-running", "Long-running"),
    ])

    autonomy_options = [
        ("human", "Mostly human"),
        ("collaborative", "Collaborative"),
        ("mostly-autonomous", "Mostly autonomous"),
        ("autonomous", "Fully autonomous"),
    ]
    autonomy = _choose("Desired autonomy", autonomy_options,
                        default_idx=_idx_of(autonomy_options, overrides.get("autonomy", "collaborative"), 1))

    risk = _choose("Risk", [
        ("low", "Low"), ("medium", "Medium"),
        ("high", "High"), ("critical", "Critical"),
    ])

    execution_options = [
        ("interactive", "Interactive"), ("agent-loop", "Agent-in-a-loop"),
        ("scheduled", "Scheduled"), ("continuous", "Continuous"),
        ("event-driven", "Event-driven"),
    ]
    execution_mode = _choose("Execution mode", execution_options,
                              default_idx=_idx_of(execution_options, overrides.get("execution_mode", "interactive"), 0))

    schedule = None
    if execution_mode == "scheduled":
        schedule = _ask("Schedule (e.g. 'daily 08:00, max 30m, max $0.50/day')")

    budget = _choose("Budget / cost preference", [
        ("free", "Free / local"),
        ("ultra-low", "Ultra low cost"),
        ("hobby", "Hobby"),
        ("balanced", "Balanced"),
        ("quality-first", "Quality first"),
    ], default_idx=2)

    human_options = [
        ("none", "None unless failure"),
        ("exceptions", "On exceptions"),
        ("important-decisions", "On important decisions"),
        ("per-phase", "Approval per phase"),
        ("per-action", "Approval per action"),
    ]
    human = _choose("Human involvement", human_options,
                     default_idx=_idx_of(human_options, overrides.get("human_involvement", "important-decisions"), 2))

    runtime = _runtime_choice()

    return Requirements(
        name=_slugify(name), objective=objective, domain=_engine_domain(domain_choice),
        definition_of_done=dod, context=context, constraints=constraints,
        loop_pattern=pattern_id,
        size=size, lifetime=lifetime, autonomy=autonomy, risk=risk,
        execution_mode=execution_mode, schedule=schedule,
        budget_profile=budget, runtime=runtime, human_involvement=human,
        experience_level="tech",
    )


# Backwards-compatible aliases (older CLI code / scripts may import these).
quick_start = simple
guided = advanced
