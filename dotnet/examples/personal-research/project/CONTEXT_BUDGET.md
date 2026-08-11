---
type: context-budget
purpose: "Conventions and optional integrations for keeping token usage low"
---

# Context budget

This tool generates a structure, it doesn't run anything — so it can't
compress tokens for you at runtime. What it *can* do is generate a
structure that keeps context small by construction, and point at where
to plug in more if this project's budget profile (ultra-low)
needs it.

## What this structure already does for you

- **`state.md` over history.** Agents are told (see `AGENTS.md`) to read
  `state.md` for current reality, not to replay the whole conversation
  or every file in `decisions.md`/`learnings.md` — those are logs to
  append to, not context to reload in full each time.
- **Hierarchical `AGENTS.md`.** Claude Code and compatible runtimes load
  memory files per-directory, not the whole tree at once. If a subfolder
  under `project/outputs/` grows real substructure, its own nested
  `AGENTS.md` (per the convention documented in the root `AGENTS.md`)
  scopes context to that subtree instead of pulling in everything.
- **Skills instead of always-loaded instructions.** If this project uses
  a work pattern, its Claude Code Skill (`.claude/skills/<pattern>/`, when
  targeting that runtime) is loaded on demand, not stuffed into every
  turn — see the [Claude Code Skills docs](https://code.claude.com/docs/en/skills).

## Where to plug in more, if you need it

None of the below is generated or run by this tool — these are real,
external projects you can wire in yourself if this project's volume of
context genuinely needs it:

- **[LLMLingua / LLMLingua-2](https://github.com/microsoft/LLMLingua)**
  (Microsoft Research) — compresses a prompt by dropping low-information
  tokens before it's sent, with published benchmarks on the trade-off.
  Useful if you're pasting large documents into `project/references/` and
  then into a prompt verbatim.
- **Prompt caching** — if your runtime's underlying API supports it (e.g.
  [Anthropic's prompt caching](https://docs.claude.com/en/docs/build-with-claude/prompt-caching)),
  put your most stable, rarely-changing content (`goal.md`, `constraints.md`)
  first and your fast-changing content (`state.md`) last, so the stable
  prefix can be cached across turns.

Don't add either unless you've actually hit a real cost or context-window
problem — see `project/metrics.md` for tracking whether you have.
