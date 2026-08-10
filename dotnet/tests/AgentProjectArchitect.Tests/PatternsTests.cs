using AgentProjectArchitect.Core;

namespace AgentProjectArchitect.Tests;

public class PatternsRegistryTests
{
    [Fact]
    public void AllPatternsHaveRolesThatExist()
    {
        foreach (var p in Patterns.All.Values)
            foreach (var (role, _) in p.ForceRoles)
                Assert.True(Roles.All.ContainsKey(role), $"pattern {p.Id} forces unknown role {role}");
    }

    [Fact]
    public void AllMinProfilesAreValid()
    {
        foreach (var p in Patterns.All.Values)
            if (p.MinProfile != null)
                Assert.True(ArchitectureProfileCatalog.Profiles.ContainsKey(p.MinProfile));
    }

    [Fact]
    public void ChoicesCoverEveryPattern()
    {
        var choiceIds = Patterns.Choices().Select(c => c.Id).ToHashSet();
        Assert.Equal(Patterns.All.Keys.ToHashSet(), choiceIds);
    }

    [Fact]
    public void UnknownPatternIdFallsBackToAuto()
    {
        var p = Patterns.Get("does-not-exist");
        Assert.Equal("auto", p.Id);
    }
}

public class PatternIntegrationTests
{
    private static Requirements BaseReq(Action<Requirements>? configure = null)
    {
        var req = new Requirements { Name = "t", Objective = "test" };
        configure?.Invoke(req);
        return req;
    }

    [Fact]
    public void PlanExecuteReviewForcesPlannerAndEvaluator()
    {
        var req = BaseReq(r => { r.Size = "tiny"; r.Risk = "low"; r.LoopPattern = "plan-execute-review"; });
        var a = ArchitectureRecommender.Recommend(req);
        Assert.Contains("planner", a.AgentNames());
        Assert.Contains("evaluator", a.AgentNames());
    }

    [Fact]
    public void DebateCriticForcesAlwaysOnCritic()
    {
        var req = BaseReq(r => { r.Size = "tiny"; r.Risk = "low"; r.LoopPattern = "debate-critic"; });
        var a = ArchitectureRecommender.Recommend(req);
        var critic = a.Agents.First(x => x.Role == "critic");
        Assert.Equal("always", critic.Mode);
    }

    [Fact]
    public void SwarmParallelEnforcesMinimumProfileSize()
    {
        var req = BaseReq(r => { r.Size = "tiny"; r.Risk = "low"; r.LoopPattern = "swarm-parallel"; });
        var a = ArchitectureRecommender.Recommend(req);
        var collaborativeSize = ArchitectureProfileCatalog.Build("collaborative").Agents.Count;
        Assert.True(a.Agents.Count >= collaborativeSize);
    }

    [Fact]
    public void AutoPatternDoesNotAlterArchitecture()
    {
        var withAuto = ArchitectureRecommender.Recommend(BaseReq(r => { r.Size = "small"; r.Risk = "low"; r.LoopPattern = "auto"; }));
        var withNone = ArchitectureRecommender.Recommend(BaseReq(r => { r.Size = "small"; r.Risk = "low"; }));
        Assert.Equal(withNone.AgentNames(), withAuto.AgentNames());
    }

    [Fact]
    public void LoopPatternRecordedOnArchitecture()
    {
        var req = BaseReq(r => r.LoopPattern = "human-in-the-loop");
        var a = ArchitectureRecommender.Recommend(req);
        Assert.Equal("human-in-the-loop", a.LoopPattern);
    }
}
