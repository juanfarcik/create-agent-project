# Contributing

1. Fork, branch, make your change.
2. Run the test suite: `python3 -m unittest discover -s tests -v`.
3. Add tests for new behavior — especially for anything touching
   `architecture.py` (recommendation/optimizer rules) or `yamlutil.py`
   (round-trip correctness), since those are the parts most likely to
   silently break.
4. Keep the generator dependency-free and deterministic — no network
   calls or LLM calls in the core recommendation/generation path.
5. Open a PR describing what changed and why.

## Adding a new role

Add it to `agent_project/roles.py` (`ROLES` dict) with description,
responsibilities, required/excluded context, tools, and escalation
conditions. Reference it from an architecture profile in
`agent_project/architecture.py`. Add a test asserting the profile
references only roles that exist (`tests/test_architecture.py` already
checks this for all built-in profiles).

## Adding a new runtime adapter

Add a `generate_<runtime>(root, req, arch)` function to
`agent_project/adapters.py` and register it in `ADAPTERS`. It should
only generate what that runtime needs on top of the runtime-independent
core (`AGENTS.md`, `.agent/`, `.project/`) — never duplicate the project
model.
