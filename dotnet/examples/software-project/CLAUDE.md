# software-project

See `AGENTS.md` for full instructions — this file exists only because
Claude Code looks for `CLAUDE.md` by convention.

Subagents for this project live in `.claude/agents/`, generated from
`.agent/prompts/`. Do not edit them directly; edit the source prompt and
regenerate if you need to change a role.

The project's work pattern is also packaged as a Skill under
`.claude/skills/` — invoke it explicitly if you want the loop
procedure followed deliberately rather than just referenced from
`AGENTS.md`.