namespace AgentProjectArchitect.Core;

/// <summary>
/// The built-in architecture profiles (Section 10 of the design brief) —
/// from a single agent up to a full software team. Pure data: each
/// profile is a factory so callers always get a fresh, independently
/// mutable <see cref="Architecture"/> instance.
/// </summary>
public static class ArchitectureProfileCatalog
{
    public static readonly IReadOnlyDictionary<string, Func<Architecture>> Profiles =
        new Dictionary<string, Func<Architecture>>
        {
            ["minimal"] = () => new Architecture
            {
                Profile = "minimal",
                Agents = { new AgentSpec("orchestrator", "always", "balanced") },
                HumanGates = { "irreversible actions" },
                Complexity = "LOW",
                EstCallsPerRun = "1-4",
                EstContext = "LOW",
                EstCost = "LOW",
                Notes = { "Single agent handles everything. No specialization needed." },
            },
            ["lean"] = () => new Architecture
            {
                Profile = "lean",
                Agents =
                {
                    new AgentSpec("orchestrator", "always", "balanced"),
                    new AgentSpec("researcher", "on-demand", "cheap"),
                    new AgentSpec("critic", "on-demand", "balanced"),
                },
                HumanGates = { "irreversible actions", "budget threshold" },
                Complexity = "LOW",
                EstCallsPerRun = "4-8",
                EstContext = "LOW",
                EstCost = "LOW",
                Notes = { "One orchestrator plus specialists invoked only when useful." },
            },
            ["collaborative"] = () => new Architecture
            {
                Profile = "collaborative",
                Agents =
                {
                    new AgentSpec("orchestrator", "always", "strong"),
                    new AgentSpec("researcher", "always", "cheap"),
                    new AgentSpec("analyst", "always", "balanced"),
                    new AgentSpec("critic", "on-demand", "balanced"),
                },
                HumanGates = { "irreversible actions", "budget threshold", "conflicting agent results" },
                Checkpoints = true,
                Complexity = "MEDIUM",
                EstCallsPerRun = "10-20",
                EstContext = "MEDIUM",
                EstCost = "MEDIUM",
                Notes = { "Supervisor + workers. Justified when parallel work has real value." },
            },
            ["research"] = () => new Architecture
            {
                Profile = "research",
                Agents =
                {
                    new AgentSpec("orchestrator", "always", "balanced"),
                    new AgentSpec("researcher", "always", "cheap"),
                    new AgentSpec("analyst", "always", "balanced"),
                    new AgentSpec("critic", "on-demand", "balanced"),
                },
                HumanGates = { "irreversible actions" },
                Complexity = "LOW",
                EstCallsPerRun = "5-10",
                EstContext = "LOW",
                EstCost = "LOW",
                Notes = { "Researcher -> Analyst -> Critic -> Synthesis, for evidence-heavy work." },
            },
            ["autonomous-loop"] = () => new Architecture
            {
                Profile = "autonomous-loop",
                Agents =
                {
                    new AgentSpec("orchestrator", "always", "balanced"),
                    new AgentSpec("researcher", "on-demand", "cheap"),
                    new AgentSpec("evaluator", "always", "balanced"),
                },
                HumanGates = { "irreversible actions", "budget threshold" },
                Checkpoints = true,
                Complexity = "MEDIUM",
                EstCallsPerRun = "4-8/run",
                EstContext = "LOW",
                EstCost = "LOW-MEDIUM",
                Notes = { "Persistent state + scheduled/continuous execution with checkpoints." },
            },
            ["high-reliability"] = () => new Architecture
            {
                Profile = "high-reliability",
                Agents =
                {
                    new AgentSpec("orchestrator", "always", "strong"),
                    new AgentSpec("planner", "always", "strong"),
                    new AgentSpec("researcher", "always", "cheap"),
                    new AgentSpec("executor", "always", "balanced"),
                    new AgentSpec("critic", "always", "strong"),
                    new AgentSpec("evaluator", "always", "strong"),
                    new AgentSpec("risk-reviewer", "always", "strong"),
                },
                HumanGates =
                {
                    "irreversible actions", "budget threshold", "external publication",
                    "conflicting agent results", "final deliverable",
                },
                Checkpoints = true,
                Complexity = "HIGH",
                EstCallsPerRun = "20-40",
                EstContext = "HIGH",
                EstCost = "HIGH",
                Notes = { "Planner + Workers + Critic + Evaluator + human gates. Use only when justified." },
            },
            ["software-lean"] = () => new Architecture
            {
                Profile = "software-lean",
                Agents =
                {
                    new AgentSpec("orchestrator", "always", "balanced"),
                    new AgentSpec("coder", "always", "balanced"),
                    new AgentSpec("tester", "on-demand", "cheap"),
                    new AgentSpec("code-reviewer", "on-demand", "balanced"),
                },
                HumanGates = { "irreversible actions" },
                Complexity = "LOW",
                EstCallsPerRun = "4-10",
                EstContext = "LOW",
                EstCost = "LOW",
                Notes = { "Small software project: one implementer, tests/review invoked when useful." },
            },
            ["software-standard"] = () => new Architecture
            {
                Profile = "software-standard",
                Agents =
                {
                    new AgentSpec("orchestrator", "always", "balanced"),
                    new AgentSpec("architect", "on-demand", "strong"),
                    new AgentSpec("coder", "always", "balanced"),
                    new AgentSpec("tester", "always", "cheap"),
                    new AgentSpec("code-reviewer", "always", "balanced"),
                    new AgentSpec("bi-analyst", "on-demand", "cheap"),
                },
                HumanGates = { "irreversible actions", "budget threshold" },
                Checkpoints = true,
                Complexity = "MEDIUM",
                EstCallsPerRun = "10-25",
                EstContext = "MEDIUM",
                EstCost = "MEDIUM",
                Notes = { "Standard product build: design on-demand, implementation with tests and review as gates." },
            },
            ["software-high-reliability"] = () => new Architecture
            {
                Profile = "software-high-reliability",
                Agents =
                {
                    new AgentSpec("orchestrator", "always", "strong"),
                    new AgentSpec("architect", "always", "strong"),
                    new AgentSpec("planner", "always", "balanced"),
                    new AgentSpec("coder", "always", "balanced"),
                    new AgentSpec("tester", "always", "cheap"),
                    new AgentSpec("qa-reviewer", "always", "balanced"),
                    new AgentSpec("code-reviewer", "always", "strong"),
                    new AgentSpec("risk-reviewer", "on-demand", "strong"),
                    new AgentSpec("bi-analyst", "on-demand", "cheap"),
                },
                HumanGates =
                {
                    "irreversible actions", "budget threshold", "external publication",
                    "final deliverable", "production deploys",
                },
                Checkpoints = true,
                Complexity = "HIGH",
                EstCallsPerRun = "25-50",
                EstContext = "HIGH",
                EstCost = "HIGH",
                Notes = { "Full software team: design, build, automated tests, QA, and code review gates." },
            },
        };

    /// <summary>Builds a fresh instance of the named profile.</summary>
    /// <exception cref="ArgumentException">The profile name isn't registered.</exception>
    public static Architecture Build(string name)
    {
        if (!Profiles.TryGetValue(name, out var factory))
        {
            throw new ArgumentException($"Unknown architecture profile: {name}", nameof(name));
        }
        return factory();
    }
}
