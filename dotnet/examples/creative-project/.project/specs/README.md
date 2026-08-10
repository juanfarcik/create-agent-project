---
type: specs-guide
purpose: "How to write a spec for a feature or deliverable"
---

# Specs

`goal.md` stays high-level and stable — the project's overall objective
and Definition of Done. As soon as the project has more than one
distinct feature or deliverable, each one gets its own spec file here
instead of piling more detail into `goal.md`.

One file per feature/deliverable, named for what it covers (e.g.
`login-flow.md`, `chapter-3.md`, `pricing-page.md`). Each spec should
cover, briefly:

- **What** — the concrete thing being built/written/produced
- **Why** — the real need behind it (see "clarify before you commit" in
  the root `AGENTS.md` — don't skip straight to *what* without this)
- **Acceptance criteria** — how to know this specific piece is done,
  distinct from the project's overall Definition of Done
- **Status** — draft / ready / in progress / done

Keep specs small and disposable — a spec for a feature that's done is
historical record, not something to keep editing. This mirrors how
spec-driven agentic workflows (e.g. GitHub's spec-kit, Kiro) separate
"what to build" from "how" (`plans/`) and "the work itself"
(`outputs/`) — see this project's `docs/REFERENCES.md`.

Don't create a spec for trivial one-off tasks — that's what
`backlog.md` is for. Specs are for anything substantial enough that
"what does done look like" needs to be written down before starting.
