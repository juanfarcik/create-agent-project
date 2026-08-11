---
type: outputs-guide
purpose: "What kind of durable output belongs in this folder"
---

# Outputs

Digests, run reports, and logs of completed automated actions — one file per run or per period, not one giant append-only log.

This folder is the actual point of the project — everything else in
`project/` and `.agent/` exists to help produce what goes here.
Conversation with the agent is not the output; what's saved in this
folder is.

See `project/goal.md` for what "done" means for this project's outputs,
and the "Growing the structure" section in the root `AGENTS.md` for when
a subfolder here should get its own `AGENTS.md`.
