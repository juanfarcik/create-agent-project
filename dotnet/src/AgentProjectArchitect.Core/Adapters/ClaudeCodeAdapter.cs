namespace AgentProjectArchitect.Core;

/// <summary>Generates <c>.claude/agents/*.md</c> subagents and a thin <c>CLAUDE.md</c> pointer.</summary>
public sealed class ClaudeCodeAdapter : IRuntimeAdapter
{
    public string Id => "claude-code";

    private static readonly Dictionary<string, string> ToolMap = new()
    {
        ["read"] = "Read",
        ["write"] = "Write, Edit",
        ["delegate"] = "Agent",
        ["web_search"] = "WebSearch, WebFetch",
        ["execute"] = "Bash",
    };

    public void Generate(string root, Requirements req, Architecture arch)
    {
        var agentsDir = Path.Combine(root, ".claude", "agents");
        Directory.CreateDirectory(agentsDir);

        foreach (var agent in arch.Agents)
        {
            if (!Roles.All.TryGetValue(agent.Role, out var role)) continue;

            var tools = MapTools(role.Tools);
            var body = File.ReadAllText(Path.Combine(root, ".agent", "prompts", $"{agent.Role}.md"));
            var content = $"""
                ---
                name: {agent.Role}
                description: {role.Description}
                tools: {tools}
                ---

                {body}
                """;
            File.WriteAllText(Path.Combine(agentsDir, $"{agent.Role}.md"), content);
        }

        File.WriteAllText(Path.Combine(root, "CLAUDE.md"), $"""
            # {req.Name}

            See `AGENTS.md` for full instructions — this file exists only because
            Claude Code looks for `CLAUDE.md` by convention.

            Subagents for this project live in `.claude/agents/`, generated from
            `.agent/prompts/`. Do not edit them directly; edit the source prompt and
            regenerate if you need to change a role.
            """);
    }

    private static string MapTools(IReadOnlyList<string> abstractTools)
    {
        var seen = new List<string>();
        foreach (var tool in abstractTools)
        {
            var mapped = ToolMap.TryGetValue(tool, out var m) ? m : tool;
            foreach (var t in mapped.Split(", "))
            {
                if (!seen.Contains(t)) seen.Add(t);
            }
        }
        return string.Join(", ", seen);
    }
}
