using AgentProjectArchitect.Core;

namespace AgentProjectArchitect.Tests;

public class ProjectComponentCatalogTests
{
    private static Requirements BaseReq(Action<Requirements>? configure = null)
    {
        var req = new Requirements { Name = "t", Objective = "test" };
        configure?.Invoke(req);
        return req;
    }

    [Fact]
    public void OutputsIsAlwaysIncluded()
    {
        var tiny = BaseReq(r => r.Size = "tiny");
        var decisions = ProjectComponentCatalog.Decide(tiny, ArchitectureRecommender.Recommend(tiny));
        Assert.True(decisions.Single(d => d.Id == "outputs").Included);
    }

    [Fact]
    public void TinyProjectSkipsSpecsAndReferences()
    {
        var req = BaseReq(r => r.Size = "tiny");
        var decisions = ProjectComponentCatalog.Decide(req, ArchitectureRecommender.Recommend(req));
        Assert.False(decisions.Single(d => d.Id == "specs").Included);
        Assert.False(decisions.Single(d => d.Id == "references").Included);
    }

    [Fact]
    public void ResearchFolderFollowsResearcherRolePresence()
    {
        var withResearcher = BaseReq(r => r.Domain = "research");
        var withoutResearcher = BaseReq(r => { r.Domain = "software"; r.Size = "tiny"; r.Risk = "low"; });

        var withDecision = ProjectComponentCatalog.Decide(withResearcher, ArchitectureRecommender.Recommend(withResearcher));
        var withoutDecision = ProjectComponentCatalog.Decide(withoutResearcher, ArchitectureRecommender.Recommend(withoutResearcher));

        Assert.True(withDecision.Single(d => d.Id == "research").Included);
        Assert.False(withoutDecision.Single(d => d.Id == "research").Included);
    }

    [Fact]
    public void PlansFollowsePlannerRoleOrPlanExecuteReviewPattern()
    {
        var req = BaseReq(r => r.LoopPattern = "plan-execute-review");
        var decisions = ProjectComponentCatalog.Decide(req, ArchitectureRecommender.Recommend(req));
        Assert.True(decisions.Single(d => d.Id == "plans").Included);
    }

    [Fact]
    public void CheckpointsFolderMatchesArchitectureCheckpointsFlag()
    {
        var scheduled = BaseReq(r => r.ExecutionMode = "scheduled");
        var arch = ArchitectureRecommender.Recommend(scheduled);
        var decisions = ProjectComponentCatalog.Decide(scheduled, arch);
        Assert.Equal(arch.Checkpoints, decisions.Single(d => d.Id == "checkpoints").Included);
    }

    [Fact]
    public void TelemetryFollowsLifetimeOrPersistentExecutionMode()
    {
        var longRunning = BaseReq(r => r.Lifetime = "long-running");
        var oneOff = BaseReq(r => { r.Lifetime = "session"; r.ExecutionMode = "interactive"; });

        var longDecision = ProjectComponentCatalog.Decide(longRunning, ArchitectureRecommender.Recommend(longRunning));
        var oneOffDecision = ProjectComponentCatalog.Decide(oneOff, ArchitectureRecommender.Recommend(oneOff));

        Assert.True(longDecision.Single(d => d.Id == "telemetry").Included);
        Assert.False(oneOffDecision.Single(d => d.Id == "telemetry").Included);
    }

    [Fact]
    public void EveryDecisionHasANonEmptyReason()
    {
        var req = BaseReq();
        var decisions = ProjectComponentCatalog.Decide(req, ArchitectureRecommender.Recommend(req));
        Assert.All(decisions, d => Assert.False(string.IsNullOrWhiteSpace(d.Reason)));
    }
}
