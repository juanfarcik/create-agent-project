using AgentProjectArchitect.Core;

namespace AgentProjectArchitect.Tests;

public class ScaffoldGeneratorTests : IDisposable
{
    private readonly string _tmp = Path.Combine(Path.GetTempPath(), "apa-test-" + Guid.NewGuid());

    public void Dispose()
    {
        if (Directory.Exists(_tmp)) Directory.Delete(_tmp, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void GenerateCreatesExpectedLayout()
    {
        var req = new Requirements { Name = "demo", Objective = "Do the thing", Domain = "general" };
        var arch = ArchitectureRecommender.Recommend(req);
        var root = Path.Combine(_tmp, "demo");
        ScaffoldGenerator.Generate(root, req, arch);

        string[] expected =
        {
            "AGENTS.md", "README.md", "GETTING_STARTED.md",
            ".agent/project.yaml", ".agent/architecture.yaml", ".agent/policies.yaml",
            ".project/goal.md", ".project/state.md", ".project/backlog.md",
            ".project/decisions.md", ".project/learnings.md", ".project/constraints.md",
            ".project/resources.md", ".project/metrics.md",
        };
        foreach (var f in expected)
            Assert.True(File.Exists(Path.Combine(root, f.Replace('/', Path.DirectorySeparatorChar))), $"missing {f}");

        foreach (var sub in ScaffoldGenerator.ProjectSubdirs)
            Assert.True(Directory.Exists(Path.Combine(root, ".project", sub)));

        foreach (var agent in arch.Agents)
            Assert.True(File.Exists(Path.Combine(root, ".agent", "prompts", $"{agent.Role}.md")));
    }

    [Fact]
    public void GeneratedObjectiveAppearsInGoalAndAgentsMd()
    {
        var req = new Requirements { Name = "demo2", Objective = "UNIQUE_OBJECTIVE_STRING", Domain = "general" };
        var arch = ArchitectureRecommender.Recommend(req);
        var root = Path.Combine(_tmp, "demo2");
        ScaffoldGenerator.Generate(root, req, arch);

        Assert.Contains("UNIQUE_OBJECTIVE_STRING", File.ReadAllText(Path.Combine(root, ".project", "goal.md")));
        Assert.Contains(req.Domain, File.ReadAllText(Path.Combine(root, "AGENTS.md")));
    }

    [Fact]
    public void ProjectYamlRoundTripsThroughYamlLoader()
    {
        var req = new Requirements
        {
            Name = "demo3", Objective = "Ship it", Domain = "software",
            Size = "medium", Risk = "medium", BudgetProfile = "balanced", Schedule = null,
        };
        var arch = ArchitectureRecommender.Recommend(req);
        var root = Path.Combine(_tmp, "demo3");
        ScaffoldGenerator.Generate(root, req, arch);

        var loadedReq = YamlLoader.LoadRequirements(root);
        Assert.Equal(req.Name, loadedReq.Name);
        Assert.Equal(req.Objective, loadedReq.Objective);
        Assert.Equal(req.Domain, loadedReq.Domain);
        Assert.Equal(req.Size, loadedReq.Size);
        Assert.Null(loadedReq.Schedule);

        var loadedArch = YamlLoader.LoadArchitecture(root);
        Assert.Equal(arch.Profile, loadedArch.Profile);
        Assert.Equal(arch.AgentNames().OrderBy(x => x), loadedArch.AgentNames().OrderBy(x => x));
    }
}
