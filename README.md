# create-agent-project

A **deterministic scaffold generator for agentic AI projects.** No LLM
decides the architecture — a rules engine reads your requirements
(size, risk, domain, autonomy) and picks the smallest agent setup that
actually fits, from one agent to a full multi-role team. The output is
a portable, CLI-agnostic project structure (`AGENTS.md` + `.agent/` +
`.project/`) you open directly with **Claude Code**, **OpenCode**,
**Codex CLI**, or any other `AGENTS.md`-reading tool — no vendor
lock-in by default.

**Who it's for:** individuals working solo — not enterprise teams —
across any domain: code, writing, research, creative work, business
planning. If you're looking for agentic orchestration at company
scale, that's a different, commercial problem this project explicitly
does not try to solve (see `dotnet/README.md` for what does).

**The project lives in [`dotnet/`](dotnet/).** See
[`dotnet/README.md`](dotnet/README.md) for install, usage, architecture
profiles, work patterns, and everything else.

## License & governance

GPLv3 — see [LICENSE](LICENSE). Governance model (who decides what gets
merged) — see [`dotnet/GOVERNANCE.md`](dotnet/GOVERNANCE.md).
Contributions welcome — see [`dotnet/CONTRIBUTING.md`](dotnet/CONTRIBUTING.md).
