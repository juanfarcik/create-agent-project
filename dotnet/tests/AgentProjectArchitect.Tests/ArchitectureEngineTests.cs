using AgentProjectArchitect.Core;

namespace AgentProjectArchitect.Tests;

public class ProfileTests
{
    [Fact]
    public void AllProfilesAreBuildable()
    {
        foreach (var name in ArchitectureProfileCatalog.Profiles.Keys)
        {
            var a = ArchitectureProfileCatalog.Build(name);
            Assert.NotEmpty(a.Agents);
        }
    }

    [Fact]
    public void AllProfileRolesExistInRoleLibrary()
    {
        foreach (var name in ArchitectureProfileCatalog.Profiles.Keys)
        {
            var a = ArchitectureProfileCatalog.Build(name);
            foreach (var agent in a.Agents)
                Assert.True(Roles.All.ContainsKey(agent.Role), $"profile {name} references unknown role {agent.Role}");
        }
    }

    [Fact]
    public void UnknownProfileThrows()
    {
        Assert.Throws<ArgumentException>(() => ArchitectureProfileCatalog.Build("does-not-exist"));
    }
}

public class RecommendTests
{
    private static Requirements BaseReq(Action<Requirements>? configure = null)
    {
        var req = new Requirements { Name = "t", Objective = "test" };
        configure?.Invoke(req);
        return req;
    }

    [Fact]
    public void TinyLowRiskIsMinimal()
    {
        var req = BaseReq(r => { r.Size = "tiny"; r.Risk = "low"; r.Domain = "general"; });
        var a = ArchitectureRecommender.Recommend(req);
        Assert.Equal("minimal", a.Profile);
    }

    [Fact]
    public void ScheduledExecutionForcesCheckpoints()
    {
        var req = BaseReq(r => { r.Size = "tiny"; r.Risk = "low"; r.ExecutionMode = "scheduled"; });
        var a = ArchitectureRecommender.Recommend(req);
        Assert.True(a.Checkpoints);
    }

    [Fact]
    public void HighRiskAddsRiskReviewer()
    {
        var req = BaseReq(r => { r.Size = "small"; r.Risk = "high"; });
        var a = ArchitectureRecommender.Recommend(req);
        Assert.Contains("risk-reviewer", a.AgentNames());
        Assert.Contains("high-risk decisions", a.HumanGates);
    }

    [Fact]
    public void LargeCriticalRiskIsHighReliability()
    {
        var req = BaseReq(r => { r.Size = "large"; r.Risk = "critical"; });
        var a = ArchitectureRecommender.Recommend(req);
        Assert.Equal("high-reliability", a.Profile);
    }

    [Fact]
    public void ResearchDomainUsesResearchProfile()
    {
        var req = BaseReq(r => r.Domain = "research");
        var a = ArchitectureRecommender.Recommend(req);
        Assert.Equal("research", a.Profile);
    }

    [Fact]
    public void SoftwareDomainUsesSoftwareProfiles()
    {
        var small = ArchitectureRecommender.Recommend(BaseReq(r => { r.Domain = "software"; r.Size = "tiny"; r.Risk = "low"; }));
        Assert.Equal("software-lean", small.Profile);
        Assert.Contains("coder", small.AgentNames());

        var big = ArchitectureRecommender.Recommend(BaseReq(r => { r.Domain = "software"; r.Size = "large"; r.Risk = "critical"; }));
        Assert.Equal("software-high-reliability", big.Profile);
        foreach (var role in new[] { "architect", "coder", "tester", "qa-reviewer", "code-reviewer" })
            Assert.Contains(role, big.AgentNames());
    }

    [Fact]
    public void AutonomyShapesHumanGatesNotAgentCount()
    {
        var req = BaseReq(r => { r.Autonomy = "autonomous"; r.Size = "tiny"; r.Risk = "low"; });
        var a = ArchitectureRecommender.Recommend(req);
        Assert.Equal("minimal", a.Profile);
        Assert.Contains("irreversible actions", a.HumanGates);
    }

    [Fact]
    public void CreativeDomainSwapsAnalystForCreativeDirector()
    {
        var req = BaseReq(r => { r.Domain = "creative"; r.Size = "medium"; r.Lifetime = "weeks"; });
        var a = ArchitectureRecommender.Recommend(req);
        Assert.Contains("creative-director", a.AgentNames());
        Assert.DoesNotContain("analyst", a.AgentNames());
    }
}

public class OptimizeTests
{
    [Fact]
    public void OptimizeNeverIncreasesAgentCount()
    {
        var req = new Requirements { Name = "t", Objective = "x", Size = "tiny", Risk = "low", BudgetProfile = "ultra-low" };
        var arch = ArchitectureProfileCatalog.Build("high-reliability");
        var optimized = ArchitectureOptimizer.Optimize(arch, req);
        Assert.True(optimized.Agents.Count <= arch.Agents.Count);
    }

    [Fact]
    public void OptimizeNeverRemovesCoreDoerRole()
    {
        var req = new Requirements { Name = "t", Objective = "x", Domain = "software", Size = "tiny", Risk = "low", BudgetProfile = "ultra-low" };
        var arch = ArchitectureProfileCatalog.Build("software-high-reliability");
        var optimized = ArchitectureOptimizer.Optimize(arch, req);
        Assert.Contains("coder", optimized.AgentNames());
        var coder = optimized.Agents.First(a => a.Role == "coder");
        Assert.Equal("always", coder.Mode);
    }

    [Fact]
    public void OptimizeIsIdempotentOnMinimal()
    {
        var req = new Requirements { Name = "t", Objective = "x", Size = "tiny", Risk = "low", BudgetProfile = "hobby", Lifetime = "session" };
        var arch = ArchitectureProfileCatalog.Build("minimal");
        arch.Checkpoints = false;
        var optimized = ArchitectureOptimizer.Optimize(arch, req);
        Assert.Equal(arch.Agents.Count, optimized.Agents.Count);
    }

    [Fact]
    public void OptimizeDisablesCheckpointsForSingleSession()
    {
        var req = new Requirements { Name = "t", Objective = "x", Lifetime = "session" };
        var arch = ArchitectureProfileCatalog.Build("autonomous-loop");
        arch.Checkpoints = true;
        var optimized = ArchitectureOptimizer.Optimize(arch, req);
        Assert.False(optimized.Checkpoints);
    }
}
