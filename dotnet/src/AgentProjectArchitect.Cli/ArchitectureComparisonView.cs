namespace AgentProjectArchitect.Cli;

/// <summary>
/// Renders the built-in profile comparison table for the <c>compare</c>
/// command. Console-formatted text is a presentation concern, so it lives
/// in the CLI project, not <c>AgentProjectArchitect.Core</c>.
/// </summary>
public static class ArchitectureComparisonView
{
    private static readonly (string Architecture, string Agents, string Cost, string Complexity, string Reliability)[] Rows =
    {
        ("Architecture", "Agents", "Cost", "Complexity", "Reliability"),
        ("minimal", "1", "$", "LOW", "MEDIUM"),
        ("lean", "2-3", "$$", "LOW", "HIGH"),
        ("research", "3-4", "$$", "LOW", "HIGH"),
        ("collaborative", "4", "$$$", "MEDIUM", "HIGH"),
        ("autonomous-loop", "3", "$$-$$$", "MEDIUM", "HIGH"),
        ("high-reliability", "7", "$$$$", "HIGH", "VERY HIGH"),
    };

    public static string Render()
    {
        var widths = new int[5];
        foreach (var r in Rows)
        {
            var cells = new[] { r.Architecture, r.Agents, r.Cost, r.Complexity, r.Reliability };
            for (var i = 0; i < 5; i++) widths[i] = Math.Max(widths[i], cells[i].Length);
        }

        var lines = Rows.Select(r =>
        {
            var cells = new[] { r.Architecture, r.Agents, r.Cost, r.Complexity, r.Reliability };
            return string.Join("  ", cells.Select((c, i) => c.PadRight(widths[i])));
        });

        return string.Join("\n", lines);
    }
}
