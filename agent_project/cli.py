"""CLI entry point.

Implements Section 66's priority subset: new, validate, status,
architecture, optimize, plus compare/templates since they are cheap and
support the "requirements before architecture" workflow (Section 9).
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

from . import api, architecture as arch_mod, patterns as patterns_mod, scaffold, wizard
from .models import AgentSpec, Architecture, Requirements
from .roles import ROLES
from .yamlutil import load


# ---------------------------------------------------------------------------
# Requirements <-> project.yaml round-trip
# ---------------------------------------------------------------------------

def _load_requirements(root: Path) -> Requirements:
    path = root / ".agent" / "project.yaml"
    if not path.exists():
        print(f"error: {path} not found. Is this an agent-project directory?", file=sys.stderr)
        sys.exit(1)
    data = load(path.read_text(encoding="utf-8"))
    p, r = data.get("project", {}), data.get("requirements", {})
    return Requirements(
        name=p.get("name", root.name),
        objective=p.get("objective", ""),
        domain=p.get("domain", "general"),
        definition_of_done=p.get("definition_of_done", ""),
        size=r.get("size", "small"),
        lifetime=r.get("lifetime", "session"),
        autonomy=r.get("autonomy", "collaborative"),
        risk=r.get("risk", "low"),
        budget_profile=r.get("budget_profile", "hobby"),
        execution_mode=r.get("execution_mode", "interactive"),
        runtime=data.get("runtime", "claude-code"),
        human_involvement=r.get("human_involvement", "important-decisions"),
        schedule=r.get("schedule") or None,
        experience_level=data.get("experience_level", "beginner"),
        loop_pattern=r.get("loop_pattern", "auto"),
    )


def _load_architecture(root: Path) -> Architecture:
    path = root / ".agent" / "architecture.yaml"
    if not path.exists():
        print(f"error: {path} not found.", file=sys.stderr)
        sys.exit(1)
    data = load(path.read_text(encoding="utf-8")).get("architecture", {})
    est = data.get("estimated", {})
    agents = [
        AgentSpec(a.get("role"), a.get("mode", "always"), a.get("model_tier", "balanced"))
        for a in data.get("agents", [])
    ]
    return Architecture(
        profile=data.get("profile", "custom"),
        agents=agents,
        memory=data.get("memory", "filesystem"),
        human_gates=data.get("human_gates", []) or [],
        checkpoints=bool(data.get("checkpoints", False)),
        complexity=data.get("complexity", "LOW"),
        est_calls_per_run=est.get("calls_per_run", "?"),
        est_context=est.get("context", "?"),
        est_cost=est.get("cost", "?"),
        notes=data.get("notes", []) or [],
        loop_pattern=data.get("loop_pattern", "auto"),
    )


# ---------------------------------------------------------------------------
# Preview / confirm
# ---------------------------------------------------------------------------

def _print_architecture(arch: Architecture, title: str = "Recommended Architecture") -> None:
    pattern = patterns_mod.get(arch.loop_pattern)
    print(f"\n{title}\n")
    print(f"Architecture: {arch.profile.upper()}")
    print(f"Work pattern: {pattern.label}\n")
    print("Agents:")
    for a in arch.agents:
        print(f"  {a.role} ({a.mode}, {a.model_tier})")
    print(f"\nMemory: {arch.memory}")
    print(f"Checkpoints: {arch.checkpoints}")
    print("\nHuman approval required for:")
    for g in arch.human_gates:
        print(f"  - {g}")
    print("\nEstimated:")
    print(f"  complexity: {arch.complexity}")
    print(f"  agent calls: {arch.est_calls_per_run}")
    print(f"  context: {arch.est_context}")
    print(f"  cost: {arch.est_cost}")
    if arch.notes:
        print("\nNotes:")
        for n in arch.notes:
            print(f"  {n}")
    print()


# ---------------------------------------------------------------------------
# Commands
# ---------------------------------------------------------------------------

def cmd_new(args: argparse.Namespace) -> None:
    if args.advanced:
        req = wizard.advanced()
    elif args.simple:
        req = wizard.simple()
    else:
        print("How do you want to set this up?\n")
        print("  1. Simple — I don't know/care about agent architecture, just ask me the basics")
        print("  2. Advanced — let me configure size, risk, execution mode, budget, etc.")
        choice = input("> [1]: ").strip() or "1"
        req = wizard.advanced() if choice == "2" else wizard.simple()

    if args.runtime:
        req.runtime = args.runtime

    arch = api.preview(req)
    _print_architecture(arch)

    while True:
        choice = input("[G]enerate / [C]ustomize (optimize) / [T]ry another / [A]bort: ").strip().lower()
        if choice in ("g", ""):
            break
        if choice == "c":
            arch = arch_mod.optimize(arch, req)
            _print_architecture(arch, "Optimized Architecture")
            continue
        if choice == "t":
            name = input(f"Profile ({', '.join(arch_mod.PROFILES)}): ").strip()
            if name in arch_mod.PROFILES:
                arch = arch_mod.build_profile(name)
                _print_architecture(arch, f"Architecture: {name}")
            continue
        if choice == "a":
            print("Aborted.")
            return

    root = Path(args.path or req.name)
    result = api.build_project(root, req, arch)
    print(f"\nGenerated project at: {result.root.resolve()}")
    print(f"Runtime adapters: {', '.join(result.adapters) if result.adapters else 'none'}")
    print(f"Next: open {result.root}/AGENTS.md with your agent runtime.")


def cmd_validate(args: argparse.Namespace) -> None:
    root = Path(args.path)
    problems = []

    for required in ["AGENTS.md", ".agent/project.yaml", ".agent/architecture.yaml", ".project/goal.md", ".project/state.md"]:
        if not (root / required).exists():
            problems.append(f"missing {required}")

    if not problems:
        req = _load_requirements(root)
        arch = _load_architecture(root)
        for a in arch.agents:
            if a.role not in ROLES:
                problems.append(f"unknown role in architecture.yaml: {a.role}")
            elif not (root / ".agent" / "prompts" / f"{a.role}.md").exists():
                problems.append(f"missing prompt for role: {a.role}")
        if not req.objective:
            problems.append("project.yaml: objective is empty")

    if problems:
        print(f"INVALID — {len(problems)} problem(s):")
        for p in problems:
            print(f"  - {p}")
        sys.exit(1)
    print("VALID")


def cmd_status(args: argparse.Namespace) -> None:
    root = Path(args.path)
    for f in [".project/state.md", ".project/metrics.md"]:
        p = root / f
        if p.exists():
            print(f"--- {f} ---")
            print(p.read_text(encoding="utf-8"))


def cmd_architecture(args: argparse.Namespace) -> None:
    root = Path(args.path)
    current = _load_architecture(root)
    _print_architecture(current, "Current Architecture")
    if args.recommend:
        req = _load_requirements(root)
        recommended = arch_mod.recommend(req)
        _print_architecture(recommended, "Recommended (from current requirements)")


def cmd_optimize(args: argparse.Namespace) -> None:
    root = Path(args.path)
    req = _load_requirements(root)
    current = _load_architecture(root)
    optimized = arch_mod.optimize(current, req)
    _print_architecture(optimized, "Optimized Architecture")
    if args.apply:
        (root / ".agent" / "architecture.yaml").write_text(
            scaffold._architecture_yaml(optimized).rstrip() + "\n", encoding="utf-8"
        )
        print("Applied. Re-run `validate` and regenerate adapters if agent roles changed.")


def cmd_compare(_args: argparse.Namespace) -> None:
    print(arch_mod.compare_table())


def cmd_templates(_args: argparse.Namespace) -> None:
    for name, factory in arch_mod.PROFILES.items():
        a = factory()
        roles = ", ".join(a.agent_names())
        print(f"{name:<18} agents: {roles}")


def cmd_patterns(_args: argparse.Namespace) -> None:
    for pid, _label in patterns_mod.choices():
        p = patterns_mod.PATTERNS[pid]
        print(f"{p.id:<24} {p.label}")
        print(f"  {p.description}")
        if p.force_roles:
            print(f"  guarantees: {', '.join(f'{r} always-on' for r, m in p.force_roles)}")
        if p.min_profile:
            print(f"  minimum architecture: {p.min_profile}")
        print()


def build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(prog="agent-project", description="Agentic Project Architect")
    sub = p.add_subparsers(dest="command", required=True)

    p_new = sub.add_parser("new", help="Create a new agentic project")
    p_new.add_argument("path", nargs="?", help="Target directory (default: project name)")
    p_new.add_argument("--simple", "--quick", dest="simple", action="store_true",
                        help="Minimal questions, sensible defaults — for non-technical users")
    p_new.add_argument("--advanced", "--guided", dest="advanced", action="store_true",
                        help="Full explicit control over size, risk, execution mode, budget, etc.")
    p_new.add_argument("--runtime", choices=["claude-code", "opencode", "codex-cli", "all"])
    p_new.set_defaults(func=cmd_new)

    p_val = sub.add_parser("validate", help="Validate a generated project")
    p_val.add_argument("path")
    p_val.set_defaults(func=cmd_validate)

    p_status = sub.add_parser("status", help="Show current project state")
    p_status.add_argument("path")
    p_status.set_defaults(func=cmd_status)

    p_arch = sub.add_parser("architecture", help="Show current/recommended architecture")
    p_arch.add_argument("path")
    p_arch.add_argument("--recommend", action="store_true", help="Also show what current requirements would recommend")
    p_arch.set_defaults(func=cmd_architecture)

    p_opt = sub.add_parser("optimize", help="Suggest architecture simplifications")
    p_opt.add_argument("path")
    p_opt.add_argument("--apply", action="store_true", help="Write the optimized architecture back to architecture.yaml")
    p_opt.set_defaults(func=cmd_optimize)

    p_cmp = sub.add_parser("compare", help="Compare built-in architecture profiles")
    p_cmp.set_defaults(func=cmd_compare)

    p_tpl = sub.add_parser("templates", help="List built-in architecture templates")
    p_tpl.set_defaults(func=cmd_templates)

    p_pat = sub.add_parser("patterns", help="List built-in agentic work patterns (agent-in-the-loop, human-in-the-loop, swarm, ...)")
    p_pat.set_defaults(func=cmd_patterns)

    return p


def main(argv=None) -> None:
    parser = build_parser()
    args = parser.parse_args(argv)
    args.func(args)


if __name__ == "__main__":
    main()
