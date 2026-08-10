// Regenerates the reference examples committed under dotnet/examples/.
// Run from dotnet/: dotnet run --project examples/GenerateExamples
//
// Each example demonstrates a different requirements profile resolving to
// a different architecture — not every example is multi-agent on purpose:
// the point is to show the range, minimal to full team.

using AgentProjectArchitect.Core;

var examplesDir = AppContext.BaseDirectory;
// Walk up from bin/Debug/net8.0/ to examples/GenerateExamples/, then to examples/.
var root = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "examples"));

var examples = new List<Requirements>
{
    new()
    {
        Name = "personal-research",
        Objective = "Every morning, spend 30 minutes researching experimental jazz production techniques and leave a concise report.",
        Domain = "research",
        DefinitionOfDone = "A short daily report exists under .project/outputs/ with at least one concrete, actionable technique.",
        Size = "tiny", Lifetime = "long-running", Autonomy = "mostly-autonomous",
        Risk = "low", BudgetProfile = "ultra-low", ExecutionMode = "scheduled",
        Runtime = "claude-code", HumanInvolvement = "none",
        Schedule = "daily 08:00, max 30 minutes, max $0.50/day",
        ExperienceLevel = "beginner",
    },
    new()
    {
        Name = "creative-project",
        Objective = "Create a six-track experimental album.",
        Domain = "creative",
        DefinitionOfDone = "Six mixed tracks, mastered, with a short release plan.",
        Size = "medium", Lifetime = "weeks", Autonomy = "collaborative",
        Risk = "low", BudgetProfile = "hobby", ExecutionMode = "interactive",
        Runtime = "claude-code", HumanInvolvement = "important-decisions",
        ExperienceLevel = "beginner",
    },
    new()
    {
        Name = "business-research",
        Objective = "Research a market opportunity for a new product, analyze competitors, estimate financial viability, and produce a strategy that keeps updating as new information arrives.",
        Domain = "business",
        DefinitionOfDone = "A strategy document exists with market sizing, competitor analysis, financial viability estimate, and it is kept current in .project/outputs/.",
        Size = "medium", Lifetime = "weeks", Autonomy = "collaborative",
        Risk = "medium", BudgetProfile = "balanced", ExecutionMode = "agent-loop",
        Runtime = "claude-code", HumanInvolvement = "important-decisions",
        ExperienceLevel = "tech",
    },
    new()
    {
        Name = "software-project",
        Objective = "Build and ship a small SaaS billing dashboard.",
        Domain = "software",
        DefinitionOfDone = "Dashboard is deployed, automated tests pass, and QA has verified the golden path plus edge cases.",
        Size = "medium", Lifetime = "weeks", Autonomy = "collaborative",
        Risk = "medium", BudgetProfile = "balanced", ExecutionMode = "interactive",
        Runtime = "all", HumanInvolvement = "per-phase",
        ExperienceLevel = "tech",
    },
    new()
    {
        Name = "autonomous-daily-agent",
        Objective = "Continuously monitor a project's backlog, pick the highest-value next action, and execute it every day without supervision unless something risky comes up.",
        Domain = "ops",
        DefinitionOfDone = "Backlog trends toward zero; every action taken is logged in .project/decisions.md.",
        Size = "small", Lifetime = "long-running", Autonomy = "autonomous",
        Risk = "medium", BudgetProfile = "hobby", ExecutionMode = "continuous",
        Runtime = "claude-code", HumanInvolvement = "exceptions",
        ExperienceLevel = "tech",
    },
};

foreach (var req in examples)
{
    var exampleRoot = Path.Combine(root, req.Name);
    if (Directory.Exists(exampleRoot)) Directory.Delete(exampleRoot, recursive: true);
    var result = Api.BuildProject(exampleRoot, req);
    Console.WriteLine($"{req.Name,-24} -> {result.Architecture.Profile} ({result.Architecture.Agents.Count} agents)");
}
