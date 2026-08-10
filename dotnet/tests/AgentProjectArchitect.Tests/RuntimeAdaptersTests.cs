using AgentProjectArchitect.Core;

namespace AgentProjectArchitect.Tests;

public class RuntimeAdaptersTests : IDisposable
{
    private readonly string _tmp = Path.Combine(Path.GetTempPath(), "apa-test-" + Guid.NewGuid());

    public void Dispose()
    {
        if (Directory.Exists(_tmp)) Directory.Delete(_tmp, recursive: true);
        GC.SuppressFinalize(this);
    }

    private (string Root, Requirements Req, Architecture Arch) Build(string runtime)
    {
        var req = new Requirements
        {
            Name = "demo", Objective = "Do the thing", Domain = "software",
            Size = "small", Risk = "low", Runtime = runtime,
        };
        var arch = ArchitectureRecommender.Recommend(req);
        var root = Path.Combine(_tmp, $"demo-{runtime}");
        ScaffoldGenerator.Generate(root, req, arch);
        return (root, req, arch);
    }

    [Fact]
    public void ClaudeCodeAdapter()
    {
        var (root, req, arch) = Build("claude-code");
        var generated = RuntimeAdapterRegistry.Default.Generate(root, req, arch);
        Assert.Equal(new List<string> { "claude-code" }, generated);
        Assert.True(File.Exists(Path.Combine(root, "CLAUDE.md")));
        foreach (var a in arch.Agents)
        {
            var f = Path.Combine(root, ".claude", "agents", $"{a.Role}.md");
            Assert.True(File.Exists(f));
            var content = File.ReadAllText(f);
            Assert.StartsWith("---\nname: ", content);
            Assert.Contains("tools:", content);
        }
    }

    [Fact]
    public void ClaudeCodeAdapterSkipsSkillWhenPatternIsAuto()
    {
        var (root, req, arch) = Build("claude-code");
        Assert.Equal("auto", arch.LoopPattern);
        RuntimeAdapterRegistry.Default.Generate(root, req, arch);
        Assert.False(Directory.Exists(Path.Combine(root, ".claude", "skills")));
    }

    [Fact]
    public void ClaudeCodeAdapterGeneratesSkillForExplicitPattern()
    {
        var req = new Requirements
        {
            Name = "demo", Objective = "Do the thing", Runtime = "claude-code",
            LoopPattern = "debate-critic",
        };
        var arch = ArchitectureRecommender.Recommend(req);
        var root = Path.Combine(_tmp, "demo-skill");
        ScaffoldGenerator.Generate(root, req, arch);
        RuntimeAdapterRegistry.Default.Generate(root, req, arch);

        var skillPath = Path.Combine(root, ".claude", "skills", "debate-critic", "SKILL.md");
        Assert.True(File.Exists(skillPath));
        var content = File.ReadAllText(skillPath);
        Assert.StartsWith("---\nname: debate-critic\n", content);
        Assert.Contains("PROPOSE -> CRITIQUE", content);
    }

    [Fact]
    public void OpenCodeAdapter()
    {
        var (root, req, arch) = Build("opencode");
        var generated = RuntimeAdapterRegistry.Default.Generate(root, req, arch);
        Assert.Equal(new List<string> { "opencode" }, generated);
        Assert.True(File.Exists(Path.Combine(root, "opencode.json")));
        foreach (var a in arch.Agents)
            Assert.True(File.Exists(Path.Combine(root, ".opencode", "agent", $"{a.Role}.md")));
    }

    [Fact]
    public void CodexCliAdapter()
    {
        var (root, req, arch) = Build("codex-cli");
        var generated = RuntimeAdapterRegistry.Default.Generate(root, req, arch);
        Assert.Equal(new List<string> { "codex-cli" }, generated);
        Assert.True(File.Exists(Path.Combine(root, ".codex", "NOTES.md")));
    }

    [Fact]
    public void AllGeneratesEveryAdapter()
    {
        var (root, req, arch) = Build("all");
        var generated = RuntimeAdapterRegistry.Default.Generate(root, req, arch);
        Assert.Equal(new HashSet<string> { "claude-code", "opencode", "codex-cli" }, generated.ToHashSet());
        Assert.True(File.Exists(Path.Combine(root, "CLAUDE.md")));
        Assert.True(File.Exists(Path.Combine(root, "opencode.json")));
        Assert.True(File.Exists(Path.Combine(root, ".codex", "NOTES.md")));
    }
}
