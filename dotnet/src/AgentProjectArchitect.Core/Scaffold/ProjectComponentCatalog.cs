namespace AgentProjectArchitect.Core;

/// <summary>Whether a <see cref="ProjectComponent"/> was included, and why — surfaced
/// in the CLI preview before generation and usable anywhere else that wants to
/// explain the structure instead of just producing it.</summary>
public sealed record ComponentDecision(string Id, bool Included, string Reason);

/// <summary>
/// A single optional subfolder under <c>project/</c>. Not every project needs
/// every folder — a trivial one-off task doesn't need <c>plans/</c> if there's
/// no planner, or <c>telemetry/</c> if it only runs once. This is the same
/// "minimum architecture required" principle already applied to agent
/// selection (<see cref="ArchitectureRecommender"/>), applied here to the
/// file/folder structure itself instead of just the agent roster.
/// </summary>
public sealed record ProjectComponent(
    string Id,
    Func<Requirements, Architecture, bool> Include,
    Func<Requirements, Architecture, string> Reason);

public static class ProjectComponentCatalog
{
    private static readonly string[] LongLifetimes = { "weeks", "long-running" };
    private static readonly string[] PersistentExecutionModes = { "scheduled", "continuous", "event-driven" };

    public static readonly IReadOnlyList<ProjectComponent> Components = new List<ProjectComponent>
    {
        new("outputs",
            (_, _) => true,
            (_, _) => "always included — the actual durable output of the project"),

        new("specs",
            (req, _) => req.Size != "tiny",
            (req, _) => req.Size == "tiny"
                ? "skipped — a tiny project's goal.md is enough, no separate feature specs needed"
                : "included — project size suggests more than one deliverable worth specifying separately"),

        new("references",
            (req, _) => req.Size != "tiny",
            (req, _) => req.Size == "tiny"
                ? "skipped — unlikely to need a dedicated place for source material at this size"
                : "included — a place for source material (style guides, prior work, links)"),

        new("research",
            (_, arch) => arch.AgentNames().Contains("researcher"),
            (_, arch) => arch.AgentNames().Contains("researcher")
                ? "included — a researcher role is part of this architecture"
                : "skipped — no researcher role in this architecture"),

        new("plans",
            (req, arch) => arch.AgentNames().Contains("planner") || req.LoopPattern == "plan-execute-review",
            (req, arch) => arch.AgentNames().Contains("planner") || req.LoopPattern == "plan-execute-review"
                ? "included — a planner role or the plan-execute-review work pattern is in play"
                : "skipped — no planner role and no plan-execute-review pattern"),

        new("experiments",
            (req, _) => req.Domain is "research" or "software",
            (req, _) => req.Domain is "research" or "software"
                ? "included — research and software projects both tend to test hypotheses before committing"
                : $"skipped — not typical for a '{req.Domain}' project"),

        new("reviews",
            (_, arch) => arch.AgentNames().Any(r => r is "critic" or "evaluator" or "code-reviewer" or "qa-reviewer"),
            (_, arch) => arch.AgentNames().Any(r => r is "critic" or "evaluator" or "code-reviewer" or "qa-reviewer")
                ? "included — a review-capable role exists in this architecture"
                : "skipped — no critic/evaluator/reviewer role in this architecture"),

        new("checkpoints",
            (_, arch) => arch.Checkpoints,
            (_, arch) => arch.Checkpoints
                ? "included — this architecture uses checkpoints (long-running/scheduled work)"
                : "skipped — this architecture doesn't use checkpoints"),

        new("telemetry",
            (req, _) => LongLifetimes.Contains(req.Lifetime) || PersistentExecutionModes.Contains(req.ExecutionMode),
            (req, _) => LongLifetimes.Contains(req.Lifetime) || PersistentExecutionModes.Contains(req.ExecutionMode)
                ? "included — a long-lived or persistently-running project benefits from iteration history"
                : "skipped — a single-session project has little use for iteration telemetry"),
    };

    /// <summary>Evaluates every component's inclusion rule against these requirements/architecture.</summary>
    public static List<ComponentDecision> Decide(Requirements req, Architecture arch) =>
        Components.Select(c => new ComponentDecision(c.Id, c.Include(req, arch), c.Reason(req, arch))).ToList();
}
