namespace AgentProjectArchitect.Core;

/// <summary>
/// Generates <c>.claude/agents/*.md</c> subagents, a <c>.claude/skills/&lt;pattern&gt;/SKILL.md</c>
/// for the project's chosen work pattern, and a thin <c>CLAUDE.md</c> pointer.
///
/// Subagents and Skills are different mechanisms: a subagent is delegated
/// to and runs in an isolated context; a Skill is loaded into the current
/// agent's own context to follow a specific procedure. Roles map naturally
/// to subagents (delegation). The work pattern — a procedure, not a role —
/// maps naturally to a Skill instead.
/// </summary>
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

        GenerateWorkPatternSkill(root, arch);

        File.WriteAllText(Path.Combine(root, "CLAUDE.md"), $"""
            # {req.Name}

            See `AGENTS.md` for full instructions — this file exists only because
            Claude Code looks for `CLAUDE.md` by convention.

            Subagents for this project live in `.claude/agents/`, generated from
            `.agent/prompts/`. Do not edit them directly; edit the source prompt and
            regenerate if you need to change a role.

            The project's work pattern is also packaged as a Skill under
            `.claude/skills/` — invoke it explicitly if you want the loop
            procedure followed deliberately rather than just referenced from
            `AGENTS.md`.
            """);
    }

    private static void GenerateWorkPatternSkill(string root, Architecture arch)
    {
        var pattern = Patterns.Get(arch.LoopPattern);
        if (pattern.Id == "auto") return; // nothing distinct enough to package as a procedure

        var skillDir = Path.Combine(root, ".claude", "skills", pattern.Id);
        Directory.CreateDirectory(skillDir);

        var content = $"""
            ---
            name: {pattern.Id}
            description: {pattern.Description}
            ---

            # {pattern.Label}

            {pattern.Description}

            ```
            {pattern.LoopDiagram}
            ```

            Use this skill when you want to follow this project's chosen work
            pattern deliberately, step by step, rather than loosely. See the
            root `AGENTS.md` for the full project context this pattern operates
            within (roles, human approval gates, constraints).
            """;
        File.WriteAllText(Path.Combine(skillDir, "SKILL.md"), content);
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
