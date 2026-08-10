namespace AgentProjectArchitect.Core;

/// <summary>
/// Flags an architecture whose model-tier weight is disproportionate to
/// the requested budget profile. Qualitative on purpose (Section 56 of
/// the design brief acknowledges real token/cost simulation is future
/// work) — this exists to catch obviously oversized architectures, not to
/// predict an exact bill.
/// </summary>
public static class ArchitectureCostEstimator
{
    private static readonly Dictionary<string, int> ModelTierWeight = new()
    {
        ["cheap"] = 1,
        ["balanced"] = 2,
        ["strong"] = 3,
    };

    private static readonly Dictionary<string, int> BudgetCap = new()
    {
        ["free"] = 0,
        ["ultra-low"] = 1,
        ["hobby"] = 2,
        ["balanced"] = 3,
        ["quality-first"] = 4,
        ["custom"] = 4,
    };

    public static void Estimate(Architecture arch, Requirements req)
    {
        var weight = arch.Agents.Sum(a => ModelTierWeight.GetValueOrDefault(a.ModelTier, 2));
        var cap = BudgetCap.GetValueOrDefault(req.BudgetProfile, 2);

        if (weight > cap * 3 + 3)
        {
            arch.Notes.Add(
                $"WARNING: architecture weight ({weight}) is high for budget profile " +
                $"'{req.BudgetProfile}'. Consider --optimize.");
        }
    }
}
