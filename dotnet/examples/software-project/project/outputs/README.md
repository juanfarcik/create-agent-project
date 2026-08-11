---
type: outputs-guide
purpose: "What kind of durable output belongs in this folder"
---

# Outputs

Design docs, architecture decision records, and release notes. Source code itself lives in the project's normal source layout (e.g. `src/`) — this scaffold doesn't dictate where; only durable *decisions and docs about* the code belong here, not the code.

This folder is the actual point of the project — everything else in
`project/` and `.agent/` exists to help produce what goes here.
Conversation with the agent is not the output; what's saved in this
folder is.

See `project/goal.md` for what "done" means for this project's outputs,
and the "Growing the structure" section in the root `AGENTS.md` for when
a subfolder here should get its own `AGENTS.md`.
