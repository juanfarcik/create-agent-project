"""Runtime adapters (Section 52).

The core project (.agent/, .project/, AGENTS.md) is runtime-independent.
Adapters generate only what a given runtime requires on top of that core —
they never duplicate the project model.
"""

from __future__ import annotations

from pathlib import Path

from .models import Architecture, Requirements
from .roles import ROLES

TOOL_MAP_CLAUDE = {
    "read": "Read",
    "write": "Write, Edit",
    "delegate": "Agent",
    "web_search": "WebSearch, WebFetch",
    "execute": "Bash",
}


def _claude_tools(abstract_tools: list) -> str:
    seen = []
    for t in abstract_tools:
        for tool in TOOL_MAP_CLAUDE.get(t, t).split(", "):
            if tool not in seen:
                seen.append(tool)
    return ", ".join(seen)


def generate_claude_code(root: Path, req: Requirements, arch: Architecture) -> None:
    agents_dir = root / ".claude" / "agents"
    agents_dir.mkdir(parents=True, exist_ok=True)

    for agent in arch.agents:
        role = ROLES.get(agent.role)
        if not role:
            continue
        tools = _claude_tools(role["tools"])
        body = (root / ".agent" / "prompts" / f"{agent.role}.md").read_text(encoding="utf-8")
        content = f"""---
name: {agent.role}
description: {role["description"]}
tools: {tools}
---

{body}"""
        (agents_dir / f"{agent.role}.md").write_text(content, encoding="utf-8")

    claude_md = root / "CLAUDE.md"
    claude_md.write_text(
        f"""# {req.name}

See `AGENTS.md` for full instructions — this file exists only because
Claude Code looks for `CLAUDE.md` by convention.

Subagents for this project live in `.claude/agents/`, generated from
`.agent/prompts/`. Do not edit them directly; edit the source prompt and
regenerate with `agent-project new --adapter claude-code` (or re-run the
generator) if you need to change a role.
""",
        encoding="utf-8",
    )


def generate_codex_cli(root: Path, req: Requirements, arch: Architecture) -> None:
    """Codex CLI (and most emerging agent CLIs) already read AGENTS.md by
    convention, so there is nothing to duplicate here. This just drops a
    marker file so `validate` can confirm the adapter was requested."""
    codex_dir = root / ".codex"
    codex_dir.mkdir(parents=True, exist_ok=True)
    (codex_dir / "NOTES.md").write_text(
        f"""# Codex CLI

This project uses `AGENTS.md` at the project root as its instruction
entry point — Codex CLI reads it natively, no adapter files required.

Role prompts are available at `.agent/prompts/<role>.md` if you want to
paste them into a task manually.
""",
        encoding="utf-8",
    )


def generate_opencode(root: Path, req: Requirements, arch: Architecture) -> None:
    agents_dir = root / ".opencode" / "agent"
    agents_dir.mkdir(parents=True, exist_ok=True)

    for agent in arch.agents:
        role = ROLES.get(agent.role)
        if not role:
            continue
        body = (root / ".agent" / "prompts" / f"{agent.role}.md").read_text(encoding="utf-8")
        content = f"""---
description: {role["description"]}
mode: {"primary" if agent.role == "orchestrator" else "subagent"}
---

{body}"""
        (agents_dir / f"{agent.role}.md").write_text(content, encoding="utf-8")

    opencode_json = root / "opencode.json"
    if not opencode_json.exists():
        opencode_json.write_text(
            '{\n  "$schema": "https://opencode.ai/config.json"\n}\n', encoding="utf-8"
        )


ADAPTERS = {
    "claude-code": generate_claude_code,
    "opencode": generate_opencode,
    "codex-cli": generate_codex_cli,
}


def generate(root: Path, req: Requirements, arch: Architecture) -> list:
    if req.runtime in ("all", "both"):
        runtimes = list(ADAPTERS.keys())
    else:
        runtimes = [req.runtime]
    generated = []
    for r in runtimes:
        fn = ADAPTERS.get(r)
        if fn:
            fn(root, req, arch)
            generated.append(r)
    return generated
