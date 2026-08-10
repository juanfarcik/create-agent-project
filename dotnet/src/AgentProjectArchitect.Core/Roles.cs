namespace AgentProjectArchitect.Core;

/// <summary>
/// Generic, domain-agnostic role library. Each role defines purpose,
/// responsibilities, required/excluded context, allowed tools (abstract,
/// mapped to runtime tools by adapters), and escalation conditions. Roles
/// are selected dynamically by the architecture engine — nothing here is
/// software-specific except the software-domain roles at the bottom.
/// </summary>
public sealed record Role(
    string Description,
    IReadOnlyList<string> Responsibilities,
    IReadOnlyList<string> RequiredContext,
    IReadOnlyList<string> ExcludedContext,
    IReadOnlyList<string> Tools,
    IReadOnlyList<string> EscalateWhen);

public static class Roles
{
    public static readonly IReadOnlyDictionary<string, Role> All = new Dictionary<string, Role>
    {
        ["orchestrator"] = new Role(
            "Coordinates the project and decides the next highest-value action.",
            new[]
            {
                "read current project state before acting",
                "identify the gap between current state and Definition of Done",
                "decide whether specialized help is needed",
                "delegate only when delegation adds value",
                "update .project/state.md and .project/backlog.md after meaningful work",
            },
            new[] { ".project/goal.md", ".project/state.md", ".project/backlog.md" },
            new[] { "full conversation history of other agents" },
            new[] { "read", "write", "delegate" },
            new[] { "irreversible action", "budget threshold reached", "conflicting results" }),

        ["researcher"] = new Role(
            "Reduces uncertainty by gathering evidence and comparing alternatives.",
            new[]
            {
                "investigate unknowns relevant to the current task",
                "distinguish facts from assumptions",
                "cite sources or reasoning",
            },
            new[] { ".project/goal.md", "current task" },
            new[] { ".project/decisions.md history unrelated to the task" },
            new[] { "read", "web_search", "write" },
            new[] { "evidence is contradictory or unobtainable" }),

        ["planner"] = new Role(
            "Turns objectives into small, incremental, executable plans.",
            new[]
            {
                "decompose the current goal into dependencies and milestones",
                "define acceptance criteria per milestone",
                "prefer incremental plans over large upfront plans",
            },
            new[] { ".project/goal.md", ".project/backlog.md" },
            Array.Empty<string>(),
            new[] { "read", "write" },
            new[] { "scope is unclear or contradicts constraints" }),

        ["analyst"] = new Role(
            "Transforms information into conclusions, separating observation from inference.",
            new[]
            {
                "analyze available data relevant to the task",
                "quantify when possible",
                "state uncertainty explicitly",
            },
            new[] { "task inputs", "relevant artifacts" },
            new[] { "unrelated project history" },
            new[] { "read", "write" },
            new[] { "conclusion materially changes project direction" }),

        ["critic"] = new Role(
            "Challenges assumptions and identifies weaknesses, on demand.",
            new[]
            {
                "look for weak reasoning, hidden risk, unnecessary complexity",
                "propose concrete alternatives, not just objections",
            },
            new[] { "artifact under review", ".project/constraints.md" },
            Array.Empty<string>(),
            new[] { "read", "write" },
            new[] { "never — critic output feeds back to the orchestrator" }),

        ["evaluator"] = new Role(
            "Independently verifies whether work actually meets the Definition of Done.",
            new[]
            {
                "never trust self-reported completion",
                "check the artifact against .project/goal.md's Definition of Done",
                "return PASS/FAIL with required changes",
            },
            new[] { ".project/goal.md", "artifact under evaluation" },
            Array.Empty<string>(),
            new[] { "read", "write" },
            new[] { "repeated failure on the same criteria" }),

        ["domain-expert"] = new Role(
            "Provides specialized domain knowledge relevant to the project.",
            new[]
            {
                "distinguish established knowledge from assumption",
                "flag when external expertise is required",
            },
            new[] { "current task", ".project/context.md" },
            Array.Empty<string>(),
            new[] { "read", "write" },
            new[] { "claim requires certified/external expertise" }),

        ["executor"] = new Role(
            "Performs concrete domain-specific work required by the project.",
            new[]
            {
                "understand objective and constraints before acting",
                "report what changed, what remains, and any risk introduced",
            },
            new[] { "current task", "relevant artifacts" },
            Array.Empty<string>(),
            new[] { "read", "write", "execute" },
            new[] { "action is irreversible or outside granted permissions" }),

        ["creative-director"] = new Role(
            "Maintains creative vision and coherence across creative work.",
            new[]
            {
                "protect the creative direction",
                "prevent generic or derivative output",
                "balance creativity against real constraints",
            },
            new[] { ".project/context.md", "prior creative artifacts" },
            Array.Empty<string>(),
            new[] { "read", "write" },
            new[] { "direction change affects the whole project" }),

        ["risk-reviewer"] = new Role(
            "Identifies safety, privacy, security, and irreversibility risks.",
            new[]
            {
                "look for irreversible consequences and unauthorized actions",
                "escalate high-risk decisions to the human",
            },
            new[] { "artifact or action under review", ".project/constraints.md" },
            Array.Empty<string>(),
            new[] { "read", "write" },
            new[] { "risk is high or irreversible" }),

        // -- Software-domain roles (only used when Requirements.Domain == "software") --

        ["architect"] = new Role(
            "Defines technical structure, technology choices, and tradeoffs before implementation.",
            new[]
            {
                "translate the objective into a technical design (components, boundaries, data flow)",
                "choose technologies/patterns and justify tradeoffs",
                "flag design decisions that are expensive to reverse",
                "keep the design as simple as the requirements allow — no speculative abstraction",
            },
            new[] { ".project/goal.md", ".project/constraints.md", "existing codebase structure" },
            new[] { "unrelated business/creative context" },
            new[] { "read", "write" },
            new[] { "a design choice materially affects cost, security, or is hard to reverse" }),

        ["coder"] = new Role(
            "Implements the code required to satisfy a task, following the current design.",
            new[]
            {
                "implement the smallest correct change that satisfies the task",
                "follow existing codebase conventions instead of introducing new ones",
                "avoid unrelated refactors, premature abstractions, or speculative features",
                "report what changed and why",
            },
            new[] { "current task", "relevant existing code", "architecture notes if any" },
            new[] { "full project history unrelated to the task" },
            new[] { "read", "write", "execute" },
            new[] { "the task requires a design decision not yet made, or touches out-of-scope code" }),

        ["tester"] = new Role(
            "Writes and runs automated tests, and reports failures with a concrete repro.",
            new[]
            {
                "write tests for new/changed behavior, including edge cases",
                "run the test suite and report pass/fail with evidence",
                "do not mark work done because tests were written — they must pass",
            },
            new[] { "changed code", "acceptance criteria for the task" },
            Array.Empty<string>(),
            new[] { "read", "write", "execute" },
            new[] { "a failure looks environmental/flaky rather than a real regression" }),

        ["qa-reviewer"] = new Role(
            "Independently checks the product against acceptance criteria, beyond automated tests.",
            new[]
            {
                "exercise the golden path and realistic edge cases as a user would",
                "verify behavior against .project/goal.md's Definition of Done, not just 'tests pass'",
                "report concrete repro steps for any defect found",
            },
            new[] { ".project/goal.md", "artifact/build under review" },
            Array.Empty<string>(),
            new[] { "read", "write", "execute" },
            new[] { "a defect blocks the Definition of Done" }),

        ["code-reviewer"] = new Role(
            "Reviews code changes for correctness, security, and unnecessary complexity before merge.",
            new[]
            {
                "check the diff for correctness, security issues (OWASP-style), and simplification opportunities",
                "flag over-engineering as readily as bugs",
                "do not approve blindly — return concrete required changes when needed",
            },
            new[] { "the diff/change under review", ".project/constraints.md" },
            new[] { "unrelated parts of the codebase" },
            new[] { "read", "write" },
            new[] { "a security or data-loss risk is found" }),

        ["bi-analyst"] = new Role(
            "Analyzes product/usage data and metrics to inform decisions, on demand.",
            new[]
            {
                "turn raw data/metrics into decision-relevant conclusions",
                "separate observed data from inference",
                "flag when the data available is insufficient to conclude",
            },
            new[] { "relevant metrics/data sources", "the question being asked" },
            new[] { "unrelated project history" },
            new[] { "read", "write" },
            new[] { "data suggests a direction change for the project" }),
    };

    /// <summary>All registered role ids.</summary>
    public static IReadOnlyList<string> Names => All.Keys.ToList();
}
