namespace AgentProjectArchitect.Core;

/// <summary>
/// Agentic work/loop patterns — explicit, user-selectable ways an agent
/// (or set of agents) operates, independent of which roles exist.
///
/// None of these were invented for this project — each traces to a real
/// paper or established project (ReAct for agent-in-the-loop, Reflexion
/// for self-critique, multiagent debate research for debate-critic,
/// blackboard architecture from 1980s AI, etc). See
/// <c>docs/REFERENCES.md</c> at the repo root for the actual source of
/// every pattern below.
/// </summary>
public sealed record LoopPattern(
    string Id,
    string Label,
    string Description,
    IReadOnlyDictionary<string, string> Overrides,
    IReadOnlyList<(string Role, string Mode)> ForceRoles,
    string? MinProfile,
    string? Note,
    string LoopDiagram);

public static class Patterns
{
    private static readonly IReadOnlyDictionary<string, string> NoOverrides =
        new Dictionary<string, string>();

    private static readonly IReadOnlyList<(string, string)> NoForcedRoles =
        Array.Empty<(string, string)>();

    public static readonly IReadOnlyDictionary<string, LoopPattern> All = new List<LoopPattern>
    {
        new("auto", "Auto (let the tool decide)",
            "Derived from size/risk/lifetime/execution mode — no explicit pattern forced.",
            NoOverrides, NoForcedRoles, null, null,
            "GOAL -> STATE -> GAP -> ACTION -> RESULT -> EVALUATE -> STATE UPDATE"),

        new("interactive", "Interactive",
            "You drive every turn; the agent acts only when asked. The default for most sessions.",
            new Dictionary<string, string> { ["execution_mode"] = "interactive", ["autonomy"] = "collaborative" },
            NoForcedRoles, null, null,
            "YOU ASK -> AGENT ACTS -> AGENT REPORTS -> YOU ASK"),

        new("agent-in-the-loop", "Agent-in-the-loop",
            "The agent runs its own think -> act -> observe loop autonomously across many steps; " +
            "you're consulted on exceptions and irreversible actions, not every step.",
            new Dictionary<string, string> { ["execution_mode"] = "agent-loop", ["autonomy"] = "mostly-autonomous", ["human_involvement"] = "exceptions" },
            NoForcedRoles, null, null,
            "THINK -> ACT -> OBSERVE -> THINK  (repeat until goal/blocker/budget limit)"),

        new("human-in-the-loop", "Human-in-the-loop",
            "The agent proposes each significant step and waits for your explicit approval before acting.",
            new Dictionary<string, string> { ["human_involvement"] = "per-action", ["autonomy"] = "collaborative" },
            NoForcedRoles, null, null,
            "AGENT PROPOSES -> HUMAN APPROVES/EDITS -> AGENT ACTS -> AGENT PROPOSES NEXT"),

        new("human-on-the-loop", "Human-on-the-loop (supervisory)",
            "The agent acts autonomously and continuously; you monitor asynchronously and can step in, " +
            "but it does not wait for you by default.",
            new Dictionary<string, string> { ["execution_mode"] = "continuous", ["autonomy"] = "mostly-autonomous", ["human_involvement"] = "exceptions" },
            NoForcedRoles, null, null,
            "AGENT RUNS CONTINUOUSLY -> LOGS EVERY ACTION -> HUMAN REVIEWS ASYNC -> INTERVENES IF NEEDED"),

        new("plan-execute-review", "Plan -> Execute -> Review",
            "A dedicated planning pass before any execution, then an independent review pass after.",
            NoOverrides,
            new List<(string, string)> { ("planner", "always"), ("evaluator", "always") },
            null, null,
            "PLAN -> EXECUTE -> REVIEW -> (revise plan if REVIEW fails) -> EXECUTE -> ..."),

        new("debate-critic", "Debate / Critic",
            "Before finalizing, a critic actively challenges the result and proposes alternatives.",
            NoOverrides,
            new List<(string, string)> { ("critic", "always") },
            null, null,
            "PROPOSE -> CRITIQUE -> REVISE -> CRITIQUE -> ... -> CONVERGE"),

        new("reflexion-self-critique", "Reflexion (self-critique)",
            "A single agent generates, critiques its own output against the Definition of Done, and " +
            "revises before reporting done — no separate critic agent.",
            NoOverrides, NoForcedRoles, null,
            "Self-critique loop: after producing an artifact, re-read it against the Definition of " +
            "Done and revise before reporting done. Do this at least once per artifact, even without " +
            "being asked.",
            "GENERATE -> SELF-CRITIQUE -> REVISE -> (repeat once) -> REPORT"),

        new("swarm-parallel", "Swarm (parallel workers)",
            "Independent subtasks run in parallel across specialists, then get consolidated.",
            NoOverrides, NoForcedRoles, "collaborative",
            "Parallel execution: independent tasks may be delegated concurrently. The orchestrator " +
            "consolidates results and resolves conflicts before reporting — never merge conflicting " +
            "outputs silently.",
            "ORCHESTRATOR SPLITS WORK -> WORKERS RUN IN PARALLEL -> ORCHESTRATOR CONSOLIDATES"),

        new("blackboard", "Blackboard (shared state, opportunistic)",
            "Agents don't hand off directly — they read/write a shared project state and act whenever " +
            "they have something useful to contribute, in any order.",
            NoOverrides, NoForcedRoles, null,
            "Blackboard coordination: do not wait for explicit handoff. Check `project/state.md` and " +
            "`project/backlog.md` for anything you can usefully act on, act, then update state for others.",
            "SHARED STATE <-> AGENT A / AGENT B / AGENT C  (each acts opportunistically)"),

        new("scheduled-digest", "Scheduled digest",
            "Runs on a fixed schedule (e.g. daily) and produces a digest/report each run.",
            new Dictionary<string, string> { ["execution_mode"] = "scheduled" },
            NoForcedRoles, null, null,
            "TRIGGER (schedule) -> RUN -> PRODUCE ARTIFACT -> STOP UNTIL NEXT TRIGGER"),

        new("reactive-event-driven", "Reactive / event-driven",
            "Runs only when triggered by an external event (a file change, a webhook, a new item).",
            new Dictionary<string, string> { ["execution_mode"] = "event-driven" },
            NoForcedRoles, null, null,
            "EVENT -> HANDLE -> UPDATE STATE -> WAIT FOR NEXT EVENT"),
    }.ToDictionary(p => p.Id);

    /// <summary>Looks up a pattern by id, falling back to <c>"auto"</c> for unknown ids.</summary>
    public static LoopPattern Get(string id) => All.TryGetValue(id, out var p) ? p : All["auto"];

    /// <summary>(id, label) pairs in a sensible display order for wizards.</summary>
    public static IReadOnlyList<(string Id, string Label)> Choices()
    {
        string[] order =
        {
            "auto", "interactive", "agent-in-the-loop", "human-in-the-loop",
            "human-on-the-loop", "plan-execute-review", "debate-critic",
            "reflexion-self-critique", "swarm-parallel", "blackboard",
            "scheduled-digest", "reactive-event-driven",
        };
        return order.Select(id => (id, All[id].Label)).ToList();
    }
}
