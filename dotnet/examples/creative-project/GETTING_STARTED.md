---
type: getting-started
purpose: "Step-by-step onboarding for the human"
---

# Getting Started with creative-project

You don't need to know anything about "agents" or "prompts" to use this.
The project already has everything set up — you just need to open it and
start talking to it.

## Steps

1. Open this folder in your editor (e.g. VS Code).
2. Open a terminal in this folder and start Claude Code there
   (e.g. run `claude` for Claude Code, or the equivalent for your tool).
3. Your assistant will automatically read `AGENTS.md` first — that file
   tells it what this project is and what to do. You don't need to paste
   anything.
4. Just type what you want in plain language, for example:
   - "Get started" / "What's the current state of the project?"
   - "Do the next most useful thing"
   - "Show me what's been done so far"
5. The assistant will keep track of progress for you in the `project/`
   folder. You can check `project/state.md` anytime to see where things
   stand, or `project/outputs/` to see what's been produced.
6. It will ask you before doing anything risky or irreversible — that's
   expected, just answer yes/no.
7. If it starts going somewhere you don't want, you can say **"stop"**,
   **"be careful"**, or **"don't touch anything outside [some folder]"**
   at any time — it's instructed to listen to those immediately.

That's it. If you ever feel lost, just say "explain the current state of
this project" and it will summarize it for you.
