# create-agent-project

A **deterministic scaffold generator for agentic AI projects.** No LLM
decides the architecture — a rules engine reads your requirements
(size, risk, domain, autonomy) and picks the smallest agent setup that
actually fits, from one agent to a full multi-role team. The output is
a portable, CLI-agnostic project structure (`AGENTS.md` + `.agent/` +
`.project/`) you open directly with **Claude Code**, **OpenCode**,
**Codex CLI**, or any other `AGENTS.md`-reading tool — no vendor
lock-in by default.

**Who it's for:** one person, working alone, on anything — code, a
book, a research project, a business plan — who wants a solid
structure to start working with an AI CLI, without having to learn
"agent architecture" first.

*(This is a personal, open research project, not a company. Its
author also works professionally on agentic orchestration for
engineering teams — a separate, commercial effort, unrelated to what
this repo does.)*

## Quick start

```bash
# Download a release binary (no .NET install needed) from:
# https://github.com/juanfarcik/create-agent-project/releases
tar -xzf create-agent-project-<your-platform>.tar.gz
./AgentProjectArchitect.Cli new
```

Answer a few plain-language questions, get a project folder. Open it
with Claude Code, OpenCode, or Codex CLI and start talking to it.

**The project lives in [`dotnet/`](dotnet/).** See
[`dotnet/README.md`](dotnet/README.md) for every other install option,
full usage, architecture profiles, and work patterns.

## License & governance

GPLv3 — see [LICENSE](LICENSE). Governance model (who decides what gets
merged) — see [`dotnet/GOVERNANCE.md`](dotnet/GOVERNANCE.md).
Contributions welcome — see [`dotnet/CONTRIBUTING.md`](dotnet/CONTRIBUTING.md).
