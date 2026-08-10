using System.Diagnostics.CodeAnalysis;

namespace AgentProjectArchitect.Core;

/// <summary>
/// What the user needs, expressed without agent-architecture jargon.
/// Mutable record so the wizard/optimizer can adjust fields in place.
/// </summary>
public sealed class Requirements
{
    public required string Name { get; set; }
    public required string Objective { get; set; }
    public string Domain { get; set; } = "general"; // general | software | research | creative | business | ops
    public string DefinitionOfDone { get; set; } = "";
    public string Context { get; set; } = "";
    public string Constraints { get; set; } = "";

    public string Size { get; set; } = "small";              // tiny | small | medium | large
    public string Lifetime { get; set; } = "session";          // session | days | weeks | long-running
    public string Autonomy { get; set; } = "collaborative";    // human | collaborative | mostly-autonomous | autonomous
    public string Risk { get; set; } = "low";                  // low | medium | high | critical

    public string BudgetProfile { get; set; } = "hobby";       // free | ultra-low | hobby | balanced | quality-first | custom
    public string ExecutionMode { get; set; } = "interactive"; // interactive | agent-loop | scheduled | continuous | event-driven

    // agnostic (default — base only, no vendor extras) | claude-code | opencode | codex-cli | all.
    // The generated AGENTS.md + .agent/ + .project/ core works with any of these (or any future
    // AGENTS.md-reading CLI) regardless of this value — it only controls whether vendor-specific
    // native extras (e.g. Claude Code subagents) get generated on top of that agnostic base.
    public string Runtime { get; set; } = "agnostic";
    public string HumanInvolvement { get; set; } = "important-decisions";

    public string? Schedule { get; set; }
    public string ExperienceLevel { get; set; } = "beginner";  // beginner | tech

    public string LoopPattern { get; set; } = "auto";
}

/// <summary>One role's participation in an <see cref="Architecture"/>: which role, how it's invoked, and at what model tier.</summary>
public sealed class AgentSpec
{
    public required string Role { get; set; }
    public string Mode { get; set; } = "always";       // always | on-demand
    public string ModelTier { get; set; } = "balanced"; // cheap | balanced | strong

    public AgentSpec() { }

    [SetsRequiredMembers]
    public AgentSpec(string role, string mode = "always", string modelTier = "balanced")
    {
        Role = role;
        Mode = mode;
        ModelTier = modelTier;
    }
}

/// <summary>
/// How agents help accomplish a project: which roles exist, how they're
/// invoked, what needs human approval, and the estimated cost/complexity.
/// Produced by <see cref="ArchitectureRecommender"/> from a <see cref="Requirements"/>,
/// optionally refined by <see cref="ArchitectureOptimizer"/>.
/// </summary>
public sealed class Architecture
{
    public required string Profile { get; set; }
    public List<AgentSpec> Agents { get; set; } = new();
    public string Memory { get; set; } = "filesystem";
    public List<string> HumanGates { get; set; } = new();
    public bool Checkpoints { get; set; }
    public string Complexity { get; set; } = "LOW";
    public string EstCallsPerRun { get; set; } = "1-4";
    public string EstContext { get; set; } = "LOW";
    public string EstCost { get; set; } = "LOW";
    public List<string> Notes { get; set; } = new();
    public string LoopPattern { get; set; } = "auto";

    public List<string> AgentNames() => Agents.Select(a => a.Role).ToList();

    /// <summary>Deep-enough copy for building a variant without mutating the source.</summary>
    public Architecture Clone() => new()
    {
        Profile = Profile,
        Agents = Agents.Select(a => new AgentSpec(a.Role, a.Mode, a.ModelTier)).ToList(),
        Memory = Memory,
        HumanGates = new List<string>(HumanGates),
        Checkpoints = Checkpoints,
        Complexity = Complexity,
        EstCallsPerRun = EstCallsPerRun,
        EstContext = EstContext,
        EstCost = EstCost,
        Notes = new List<string>(Notes),
        LoopPattern = LoopPattern,
    };
}
