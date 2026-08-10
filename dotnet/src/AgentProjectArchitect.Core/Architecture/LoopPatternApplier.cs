namespace AgentProjectArchitect.Core;

/// <summary>
/// Applies the structural guarantees of an explicitly chosen work pattern
/// (agent-in-the-loop / human-in-the-loop / debate / swarm / ...) to an
/// <see cref="Architecture"/> — independent of execution_mode/autonomy,
/// which only seed wizard defaults. A pattern's role and topology
/// guarantees hold regardless of how those were ultimately set.
/// </summary>
public static class LoopPatternApplier
{
    public static Architecture Apply(Architecture arch, Requirements req)
    {
        var pattern = Patterns.Get(req.LoopPattern);
        if (pattern.Id == "auto") return arch;

        if (pattern.MinProfile != null && arch.Agents.Count < ArchitectureProfileCatalog.Build(pattern.MinProfile).Agents.Count)
        {
            var bigger = ArchitectureProfileCatalog.Build(pattern.MinProfile);
            bigger.Checkpoints = arch.Checkpoints;
            bigger.HumanGates = arch.HumanGates;
            bigger.Notes = arch.Notes;
            arch = bigger;
        }

        foreach (var (role, mode) in pattern.ForceRoles)
        {
            var existing = arch.Agents.FirstOrDefault(a => a.Role == role);
            if (existing != null)
            {
                existing.Mode = mode;
            }
            else
            {
                arch.Agents.Add(new AgentSpec(role, mode, "balanced"));
            }
        }

        if (pattern.Note != null)
        {
            arch.Notes.Add($"Pattern ({pattern.Label}): {pattern.Note}");
        }

        arch.LoopPattern = pattern.Id;
        return arch;
    }
}
