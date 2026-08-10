using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AgentProjectArchitect.Core;

/// <summary>
/// Reads the project.yaml / architecture.yaml files this tool generates
/// back into domain objects. A real YAML parser (YamlDotNet) — unlike the
/// Python port's hand-rolled one, we don't need to avoid the dependency here.
/// </summary>
public static class YamlLoader
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>Reads <c>.agent/project.yaml</c> back into a <see cref="Requirements"/>.</summary>
    /// <exception cref="FileNotFoundException">project.yaml is missing under <paramref name="root"/>.</exception>
    public static Requirements LoadRequirements(string root)
    {
        var path = Path.Combine(root, ".agent", "project.yaml");
        if (!File.Exists(path))
            throw new FileNotFoundException($"{path} not found. Is this an create-agent-project directory?");

        var doc = Deserializer.Deserialize<ProjectYamlDoc>(File.ReadAllText(path)) ?? new ProjectYamlDoc();
        var p = doc.Project ?? new ProjectSection();
        var r = doc.Requirements ?? new RequirementsSection();

        return new Requirements
        {
            Name = p.Name ?? Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar)),
            Objective = p.Objective ?? "",
            Domain = p.Domain ?? "general",
            DefinitionOfDone = p.DefinitionOfDone ?? "",
            Size = r.Size ?? "small",
            Lifetime = r.Lifetime ?? "session",
            Autonomy = r.Autonomy ?? "collaborative",
            Risk = r.Risk ?? "low",
            BudgetProfile = r.BudgetProfile ?? "hobby",
            ExecutionMode = r.ExecutionMode ?? "interactive",
            Runtime = doc.Runtime ?? "claude-code",
            HumanInvolvement = r.HumanInvolvement ?? "important-decisions",
            Schedule = string.IsNullOrEmpty(r.Schedule) ? null : r.Schedule,
            ExperienceLevel = doc.ExperienceLevel ?? "beginner",
            LoopPattern = r.LoopPattern ?? "auto",
        };
    }

    /// <summary>Reads <c>.agent/architecture.yaml</c> back into an <see cref="Architecture"/>.</summary>
    /// <exception cref="FileNotFoundException">architecture.yaml is missing under <paramref name="root"/>.</exception>
    public static Architecture LoadArchitecture(string root)
    {
        var path = Path.Combine(root, ".agent", "architecture.yaml");
        if (!File.Exists(path))
            throw new FileNotFoundException($"{path} not found.");

        var doc = Deserializer.Deserialize<ArchitectureYamlDoc>(File.ReadAllText(path)) ?? new ArchitectureYamlDoc();
        var a = doc.Architecture ?? new ArchitectureSection();
        var est = a.Estimated ?? new EstimatedSection();

        return new Architecture
        {
            Profile = a.Profile ?? "custom",
            Agents = (a.Agents ?? new()).Select(x => new AgentSpec(x.Role ?? "", x.Mode ?? "always", x.ModelTier ?? "balanced")).ToList(),
            Memory = a.Memory ?? "filesystem",
            HumanGates = a.HumanGates ?? new(),
            Checkpoints = a.Checkpoints,
            Complexity = a.Complexity ?? "LOW",
            EstCallsPerRun = est.CallsPerRun ?? "?",
            EstContext = est.Context ?? "?",
            EstCost = est.Cost ?? "?",
            Notes = a.Notes ?? new(),
            LoopPattern = a.LoopPattern ?? "auto",
        };
    }

    // Shapes matching the serialized YAML (underscored naming convention).
    private sealed class ProjectYamlDoc
    {
        public ProjectSection? Project { get; set; }
        public RequirementsSection? Requirements { get; set; }
        public string? Runtime { get; set; }
        public string? ExperienceLevel { get; set; }
    }

    private sealed class ProjectSection
    {
        public string? Name { get; set; }
        public string? Domain { get; set; }
        public string? Objective { get; set; }
        public string? DefinitionOfDone { get; set; }
    }

    private sealed class RequirementsSection
    {
        public string? Size { get; set; }
        public string? Lifetime { get; set; }
        public string? Autonomy { get; set; }
        public string? Risk { get; set; }
        public string? BudgetProfile { get; set; }
        public string? ExecutionMode { get; set; }
        public string? HumanInvolvement { get; set; }
        public string? Schedule { get; set; }
        public string? LoopPattern { get; set; }
    }

    private sealed class ArchitectureYamlDoc
    {
        public ArchitectureSection? Architecture { get; set; }
    }

    private sealed class ArchitectureSection
    {
        public string? Profile { get; set; }
        public string? LoopPattern { get; set; }
        public string? Memory { get; set; }
        public bool Checkpoints { get; set; }
        public string? Complexity { get; set; }
        public EstimatedSection? Estimated { get; set; }
        public List<AgentSection>? Agents { get; set; }
        public List<string>? HumanGates { get; set; }
        public List<string>? Notes { get; set; }
    }

    private sealed class EstimatedSection
    {
        public string? CallsPerRun { get; set; }
        public string? Context { get; set; }
        public string? Cost { get; set; }
    }

    private sealed class AgentSection
    {
        public string? Role { get; set; }
        public string? Mode { get; set; }
        public string? ModelTier { get; set; }
    }
}
