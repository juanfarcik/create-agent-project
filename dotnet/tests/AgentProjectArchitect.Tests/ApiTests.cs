using AgentProjectArchitect.Core;

namespace AgentProjectArchitect.Tests;

/// <summary>
/// Exercises the exact seam a future non-CLI frontend (e.g. a web UI)
/// would call: build a Requirements object with no console I/O involved,
/// get a project on disk back.
/// </summary>
public class ApiTests : IDisposable
{
    private readonly string _tmp = Path.Combine(Path.GetTempPath(), "apa-test-" + Guid.NewGuid());

    public void Dispose()
    {
        if (Directory.Exists(_tmp)) Directory.Delete(_tmp, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void PreviewHasNoSideEffects()
    {
        var req = new Requirements { Name = "demo", Objective = "Do a thing" };
        var arch = Api.Preview(req);
        Assert.False(Directory.Exists(Path.Combine(_tmp, "demo")));
        Assert.NotEmpty(arch.Agents);
    }

    [Fact]
    public void BuildProjectFromRequirementsOnly()
    {
        var req = new Requirements { Name = "demo", Objective = "Do a thing", Domain = "creative" };
        var result = Api.BuildProject(Path.Combine(_tmp, "demo"), req);
        Assert.True(File.Exists(Path.Combine(result.Root, "AGENTS.md")));
        Assert.Equal("demo", result.Requirements.Name);
        // Default runtime is "agnostic": the base (AGENTS.md/.agent/.project)
        // works with any CLI without generating vendor-specific extras.
        Assert.Empty(result.Adapters);
    }

    [Fact]
    public void BuildProjectWithClaudeCodeRuntimeGeneratesNativeExtras()
    {
        var req = new Requirements { Name = "demo", Objective = "Do a thing", Runtime = "claude-code" };
        var result = Api.BuildProject(Path.Combine(_tmp, "demo"), req);
        Assert.Contains("claude-code", result.Adapters);
    }

    [Fact]
    public void BuildProjectAcceptsPrecomputedArchitecture()
    {
        var req = new Requirements { Name = "demo", Objective = "Do a thing" };
        var arch = Api.Preview(req);
        arch.Notes.Add("UI-customized before generation");
        var root = Path.Combine(_tmp, "demo");
        Api.BuildProject(root, req, arch);
        Assert.Contains("UI-customized before generation",
            File.ReadAllText(Path.Combine(root, ".agent", "architecture.yaml")));
    }

    [Fact]
    public void BuildProjectCanOptimizeInline()
    {
        var req = new Requirements
        {
            Name = "demo", Objective = "Do a thing", Size = "large",
            Risk = "critical", BudgetProfile = "ultra-low",
        };
        var result = Api.BuildProject(Path.Combine(_tmp, "demo"), req, optimize: true);
        Assert.Contains(result.Architecture.Notes, n => n.Contains("Optimizer"));
    }
}
