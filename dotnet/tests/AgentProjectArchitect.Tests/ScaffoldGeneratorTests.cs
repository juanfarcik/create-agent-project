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

        // Component subfolders are conditional — only the ones the catalog
        // includes for this requirements/architecture combo should exist.
        foreach (var d in ProjectComponentCatalog.Decide(req, arch))
        {
            var exists = Directory.Exists(Path.Combine(root, ".project", d.Id));
            Assert.True(exists == d.Included, $"{d.Id}: expected included={d.Included} but exists={exists}");
        }

        foreach (var agent in arch.Agents)
            Assert.True(File.Exists(Path.Combine(root, ".agent", "prompts", $"{agent.Role}.md")));
    }

    [Fact]
    public void TinyProjectSkipsSpecsAndReferences()
    {
        var req = new Requirements { Name = "tiny-demo", Objective = "Write a haiku", Size = "tiny" };
        var arch = ArchitectureRecommender.Recommend(req);
        var root = Path.Combine(_tmp, "tiny-demo");
        ScaffoldGenerator.Generate(root, req, arch);

        Assert.False(Directory.Exists(Path.Combine(root, ".project", "specs")));
        Assert.False(Directory.Exists(Path.Combine(root, ".project", "references")));
        Assert.True(Directory.Exists(Path.Combine(root, ".project", "outputs")));
    }

    [Fact]
    public void ReadmeExplainsWhichComponentsWereIncludedAndWhy()
    {
        var req = new Requirements { Name = "demo-explain", Objective = "Do the thing", Size = "tiny" };
        var arch = ArchitectureRecommender.Recommend(req);
        var root = Path.Combine(_tmp, "demo-explain");
        ScaffoldGenerator.Generate(root, req, arch);

        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        Assert.Contains("✓ `.project/outputs/`", readme);
        Assert.Contains("○ `.project/specs/`", readme);
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

    [Fact]
    public void GeneratedMarkdownFilesHaveTypedFrontmatter()
    {
        var req = new Requirements { Name = "demo4", Objective = "Do the thing", Domain = "general" };
        var arch = ArchitectureRecommender.Recommend(req);
        var root = Path.Combine(_tmp, "demo4");
        ScaffoldGenerator.Generate(root, req, arch);

        var goal = File.ReadAllText(Path.Combine(root, ".project", "goal.md"));
        Assert.StartsWith("---\ntype: goal\npurpose:", goal);

        var agents = File.ReadAllText(Path.Combine(root, "AGENTS.md"));
        Assert.StartsWith("---\ntype: agent-instructions\npurpose:", agents);
    }

    [Fact]
    public void OutputsReadmeIsDomainAware()
    {
        var creativeReq = new Requirements { Name = "demo5", Objective = "Write a novel", Domain = "creative" };
        var creativeArch = ArchitectureRecommender.Recommend(creativeReq);
        var creativeRoot = Path.Combine(_tmp, "demo5");
        ScaffoldGenerator.Generate(creativeRoot, creativeReq, creativeArch);

        var softwareReq = new Requirements { Name = "demo6", Objective = "Ship an app", Domain = "software" };
        var softwareArch = ArchitectureRecommender.Recommend(softwareReq);
        var softwareRoot = Path.Combine(_tmp, "demo6");
        ScaffoldGenerator.Generate(softwareRoot, softwareReq, softwareArch);

        var creativeOutputs = File.ReadAllText(Path.Combine(creativeRoot, ".project", "outputs", "README.md"));
        var softwareOutputs = File.ReadAllText(Path.Combine(softwareRoot, ".project", "outputs", "README.md"));

        Assert.Contains("chapters, tracks, mockups", creativeOutputs);
        Assert.Contains("architecture decision records", softwareOutputs);
        Assert.NotEqual(creativeOutputs, softwareOutputs);
    }

    [Fact]
    public void DoesNotCreateEmptyPlaceholderDirectories()
    {
        var req = new Requirements { Name = "demo7", Objective = "Do the thing" };
        var arch = ArchitectureRecommender.Recommend(req);
        var root = Path.Combine(_tmp, "demo7");
        ScaffoldGenerator.Generate(root, req, arch);

        Assert.False(Directory.Exists(Path.Combine(root, ".agent", "adapters")));
        Assert.False(Directory.Exists(Path.Combine(root, ".agent", "schemas")));
    }
}
