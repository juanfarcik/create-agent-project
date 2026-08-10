namespace AgentProjectArchitect.Core;

/// <summary>Generates <c>.opencode/agent/*.md</c> and a minimal <c>opencode.json</c>.</summary>
public sealed class OpenCodeAdapter : IRuntimeAdapter
{
    public string Id => "opencode";

    public void Generate(string root, Requirements req, Architecture arch)
    {
        var agentsDir = Path.Combine(root, ".opencode", "agent");
        Directory.CreateDirectory(agentsDir);

        foreach (var agent in arch.Agents)
        {
            if (!Roles.All.TryGetValue(agent.Role, out var role)) continue;

            var body = File.ReadAllText(Path.Combine(root, ".agent", "prompts", $"{agent.Role}.md"));
            var mode = agent.Role == "orchestrator" ? "primary" : "subagent";
            var content = $"""
                ---
                description: {role.Description}
                mode: {mode}
                ---

                {body}
                """;
            File.WriteAllText(Path.Combine(agentsDir, $"{agent.Role}.md"), content);
        }

        var opencodeJson = Path.Combine(root, "opencode.json");
        if (!File.Exists(opencodeJson))
        {
            File.WriteAllText(opencodeJson, "{\n  \"$schema\": \"https://opencode.ai/config.json\"\n}\n");
        }
    }
}
