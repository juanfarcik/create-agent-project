namespace AgentProjectArchitect.Core;

/// <summary>
/// Removes unjustified complexity from an architecture given requirements
/// (Section 13 of the design brief). Roles that actually produce the
/// project's output (<see cref="CoreDoerRoles"/>) are never removed — only
/// demoted to on-demand — so the architecture stays capable of doing the
/// work even under a tight budget.
/// </summary>
public static class ArchitectureOptimizer
{
    private static readonly string[] SizeOrder = { "tiny", "small", "medium", "large" };
    private static readonly string[] RiskOrder = { "low", "medium", "high", "critical" };

    private static readonly HashSet<string> CoreDoerRoles =
        new() { "orchestrator", "researcher", "executor", "coder", "evaluator" };

    private static int IndexOf(string[] order, string value, int fallback)
    {
        var i = Array.IndexOf(order, value);
        return i >= 0 ? i : fallback;
    }

    public static Architecture Optimize(Architecture arch, Requirements req)
    {
        var removed = new List<string>();
        var agents = arch.Agents.Select(a => new AgentSpec(a.Role, a.Mode, a.ModelTier)).ToList();

        var risk = IndexOf(RiskOrder, req.Risk, 0);
        var size = IndexOf(SizeOrder, req.Size, 1);
        var cheapBudget = req.BudgetProfile is "free" or "ultra-low" or "hobby";

        DemoteReviewRolesForLowRisk(agents, risk, size, removed);
        var trimmedAgents = DemoteNonCoreRolesUnderTightBudget(agents, cheapBudget, removed);
        DowngradeModelTierUnderTightBudget(trimmedAgents, cheapBudget, removed);
        var checkpoints = DisableCheckpointsForSingleSession(arch.Checkpoints, req.Lifetime, removed);

        return BuildOptimizedArchitecture(arch, trimmedAgents, checkpoints, cheapBudget, removed);
    }

    /// <summary>Rule 1: drop always-on critic/evaluator to on-demand for low-risk, small projects.</summary>
    private static void DemoteReviewRolesForLowRisk(List<AgentSpec> agents, int risk, int size, List<string> removed)
    {
        if (risk != 0 || size > 1) return;

        foreach (var a in agents)
        {
            if (a.Role is "critic" or "evaluator" && a.Mode == "always")
            {
                a.Mode = "on-demand";
                removed.Add($"set '{a.Role}' to on-demand (low risk, small project)");
            }
        }
    }

    /// <summary>
    /// Rule 2: demote (never remove) underused specialist roles when
    /// budget is tight. <see cref="CoreDoerRoles"/> are never fully
    /// removed — only demoted — so the architecture stays capable of
    /// doing the work.
    /// </summary>
    private static List<AgentSpec> DemoteNonCoreRolesUnderTightBudget(List<AgentSpec> agents, bool cheapBudget, List<string> removed)
    {
        if (!cheapBudget) return agents;

        var result = new List<AgentSpec>();
        foreach (var a in agents)
        {
            if (CoreDoerRoles.Contains(a.Role) || a.Mode == "on-demand")
            {
                result.Add(a); // core roles kept; on-demand agents cost nothing until invoked
            }
            else
            {
                a.Mode = "on-demand";
                result.Add(a);
                removed.Add($"set always-on '{a.Role}' to on-demand (tight budget, low marginal value)");
            }
        }
        return result;
    }

    /// <summary>Rule 3: downgrade model tier for non-critical roles under tight budget.</summary>
    private static void DowngradeModelTierUnderTightBudget(List<AgentSpec> agents, bool cheapBudget, List<string> removed)
    {
        if (!cheapBudget) return;

        foreach (var a in agents)
        {
            if (a.Role is not ("orchestrator" or "evaluator" or "risk-reviewer") && a.ModelTier == "strong")
            {
                a.ModelTier = "balanced";
                removed.Add($"downgraded '{a.Role}' model tier to balanced");
            }
        }
    }

    /// <summary>Rule 4: strip checkpoints if the project is a single session.</summary>
    private static bool DisableCheckpointsForSingleSession(bool checkpoints, string lifetime, List<string> removed)
    {
        if (lifetime != "session" || !checkpoints) return checkpoints;

        removed.Add("disabled checkpoints (single-session project)");
        return false;
    }

    private static Architecture BuildOptimizedArchitecture(
        Architecture original, List<AgentSpec> agents, bool checkpoints, bool cheapBudget, List<string> removed)
    {
        var optimized = new Architecture
        {
            Profile = original.Profile,
            Agents = agents,
            Memory = original.Memory,
            HumanGates = new List<string>(original.HumanGates),
            Checkpoints = checkpoints,
            Complexity = agents.Count <= 2 ? "LOW" : original.Complexity,
            EstCallsPerRun = original.EstCallsPerRun,
            EstContext = original.EstContext,
            EstCost = cheapBudget ? "LOW" : original.EstCost,
            Notes = new List<string>(original.Notes),
            LoopPattern = original.LoopPattern,
        };

        if (removed.Count > 0)
        {
            optimized.Notes.Add("Optimizer changes:");
            optimized.Notes.AddRange(removed.Select(r => $"  - {r}"));
        }
        else
        {
            optimized.Notes.Add("Optimizer: architecture already minimal for these requirements.");
        }

        return optimized;
    }
}
