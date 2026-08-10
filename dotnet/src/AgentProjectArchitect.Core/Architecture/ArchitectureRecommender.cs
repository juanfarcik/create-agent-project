namespace AgentProjectArchitect.Core;

/// <summary>
/// Requirements -&gt; recommended <see cref="Architecture"/>. Deterministic and
/// rule-based (Section 70 of the design brief): a basic project must be
/// generated without calling an LLM. This is a lookup against
/// <see cref="ArchitectureProfileCatalog"/> plus a small set of adjustment
/// rules, never a model call.
/// </summary>
public static class ArchitectureRecommender
{
    private static readonly string[] SizeOrder = { "tiny", "small", "medium", "large" };
    private static readonly string[] RiskOrder = { "low", "medium", "high", "critical" };
    private static readonly string[] LifetimeOrder = { "session", "days", "weeks", "long-running" };

    private static int IndexOf(string[] order, string value, int fallback)
    {
        var i = Array.IndexOf(order, value);
        return i >= 0 ? i : fallback;
    }

    public static Architecture Recommend(Requirements req)
    {
        var size = IndexOf(SizeOrder, req.Size, 1);
        var risk = IndexOf(RiskOrder, req.Risk, 0);
        var lifetime = IndexOf(LifetimeOrder, req.Lifetime, 0);
        var score = size + risk + (lifetime >= 2 ? 1 : 0);

        Architecture arch;

        if (req.Domain == "software")
        {
            arch = score <= 1 && risk == 0 ? ArchitectureProfileCatalog.Build("software-lean")
                 : risk >= 2 || score >= 5 ? ArchitectureProfileCatalog.Build("software-high-reliability")
                 : ArchitectureProfileCatalog.Build("software-standard");
            return Finalize(arch, req, risk, lifetime);
        }

        if (req.Domain == "research")
        {
            arch = ArchitectureProfileCatalog.Build("research");
        }
        else if (score <= 1 && risk == 0)
        {
            arch = ArchitectureProfileCatalog.Build("minimal");
        }
        else if (score <= 2)
        {
            arch = ArchitectureProfileCatalog.Build("lean");
        }
        else if (risk >= 2 || score >= 5)
        {
            arch = ArchitectureProfileCatalog.Build("high-reliability");
        }
        else if (size >= 2)
        {
            arch = ArchitectureProfileCatalog.Build("collaborative");
        }
        else
        {
            arch = ArchitectureProfileCatalog.Build("lean");
        }

        // Creative work values direction and coherence over generic data
        // analysis — swap in creative-director wherever the generic
        // profile would have used an analyst.
        if (req.Domain == "creative")
        {
            foreach (var a in arch.Agents)
            {
                if (a.Role == "analyst")
                {
                    a.Role = "creative-director";
                }
            }
        }

        return Finalize(arch, req, risk, lifetime);
    }

    private static Architecture Finalize(Architecture arch, Requirements req, int risk, int lifetime)
    {
        // Long-running / scheduled / continuous execution needs persistence.
        if (req.ExecutionMode is "scheduled" or "continuous" or "event-driven" || lifetime >= 2)
        {
            if (arch.Profile is "minimal" or "lean")
            {
                arch = ArchitectureProfileCatalog.Build("autonomous-loop");
            }
            arch.Checkpoints = true;
        }

        // High/critical risk always adds a risk gate, regardless of profile.
        if (risk >= 2)
        {
            if (!arch.AgentNames().Contains("risk-reviewer"))
            {
                arch.Agents.Add(new AgentSpec("risk-reviewer", "on-demand", "strong"));
            }
            arch.HumanGates.Add("high-risk decisions");
        }

        // Autonomy shapes human gates, not agent count.
        var gatesByAutonomy = new Dictionary<string, string[]>
        {
            ["human"] = new[] { "every action" },
            ["collaborative"] = new[] { "important decisions" },
            ["mostly-autonomous"] = new[] { "irreversible actions", "budget threshold" },
            ["autonomous"] = new[] { "irreversible actions" },
        };
        var extraGates = gatesByAutonomy.TryGetValue(req.Autonomy, out var g) ? g : Array.Empty<string>();
        arch.HumanGates = arch.HumanGates.Concat(extraGates).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToList();

        arch = LoopPatternApplier.Apply(arch, req);
        ArchitectureCostEstimator.Estimate(arch, req);
        return arch;
    }
}
