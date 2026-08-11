using System.Reflection;

namespace AgentProjectArchitect.Cli;

/// <summary>
/// All --help text lives here, separate from command dispatch/execution,
/// so <c>Program.cs</c> stays about running commands, not describing them.
/// </summary>
public static class CliHelp
{
    public static string Version =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";

    public const string Banner = "create-agent-project — Agentic Project Architect";

    public static string TopLevel => $$"""
        {{Banner}} (v{{Version}})

        Domain-agnostic scaffold generator for agentic projects. Designs the
        smallest agent architecture that fits your requirements, then generates
        a portable project structure — AGENTS.md + .agent/ + project/ — that
        works with Claude Code, OpenCode, Codex CLI, or any other
        AGENTS.md-reading CLI, with zero vendor lock-in by default.

        Usage:
          create-agent-project <command> [arguments] [options]

        Commands:
          new           Create a new agentic project (interactive wizard)
          validate      Check a generated project's consistency
          status        Show current project state and metrics
          architecture  Show current architecture, optionally vs. a fresh recommendation
          optimize      Suggest (or apply) complexity reductions
          compare       Compare the built-in architecture profiles
          templates     List the built-in architecture profiles
          patterns      List the built-in agentic work patterns

        Options:
          -h, --help     Show help (add after any command for command-specific help)
          -v, --version  Show version

        Run 'create-agent-project <command> --help' for details on a specific command.
        Full documentation: https://github.com/juanfarcik/create-agent-project
        """;

    private static readonly Dictionary<string, string> CommandHelp = new()
    {
        ["new"] = """
            Usage: create-agent-project new [path] [options]

            Create a new agentic project through an interactive wizard, then
            generate it at [path] (default: the project's slugified name).

            Options:
              --simple              Minimal questions, sensible defaults (default if
                                     no mode flag is given and you answer "1" at the prompt)
              --advanced             Full explicit control over size, risk, lifetime,
                                     execution mode, budget, work pattern, and roles
              --runtime <value>      agnostic (default) | claude-code | opencode | codex-cli | all
                                     Controls only optional native extras (e.g. Claude
                                     Code subagents/skills) layered on top of the
                                     agnostic AGENTS.md + .agent/ + project/ base —
                                     it never changes what the agnostic base contains.

            Before anything is written, you'll see:
              - the recommended architecture (agents, human approval gates, cost)
              - the project structure preview (which project/ subfolders will be
                created and why, e.g. "research/ skipped — no researcher role")
              - a chance to [G]enerate / [C]ustomize (optimize) / [T]ry another
                profile / [A]bort

            Examples:
              create-agent-project new
              create-agent-project new my-book --simple
              create-agent-project new ./billing-service --advanced --runtime claude-code
            """,

        ["validate"] = """
            Usage: create-agent-project validate <path>

            Checks a generated project for consistency: required files present,
            every role in .agent/architecture.yaml exists in the role library and
            has a matching prompt file, and project.yaml has a non-empty objective.

            Exit code 0 and prints VALID if everything checks out; otherwise
            prints each problem found and exits 1.

            Example:
              create-agent-project validate ./my-project
            """,

        ["status"] = """
            Usage: create-agent-project status <path>

            Prints project/state.md and project/metrics.md — current reality
            and iteration/budget counters. Does not modify anything.

            Example:
              create-agent-project status ./my-project
            """,

        ["architecture"] = """
            Usage: create-agent-project architecture <path> [--recommend]

            Shows the architecture currently saved in .agent/architecture.yaml.

            Options:
              --recommend   Also compute what the rules engine would recommend
                            right now from .agent/project.yaml's requirements, and
                            show it alongside the saved one. Useful after a
                            project's requirements have changed (bigger scope,
                            different domain, higher risk) to see whether the
                            architecture should change too.

            Example:
              create-agent-project architecture ./my-project --recommend
            """,

        ["optimize"] = """
            Usage: create-agent-project optimize <path> [--apply]

            Re-runs the optimizer against the project's current requirements and
            architecture, and prints what it would remove/demote and why (or
            confirms the architecture is already minimal for its requirements).

            Options:
              --apply   Write the optimized architecture back to
                        .agent/architecture.yaml. Run `validate` afterward, and
                        regenerate runtime adapters (re-run `new` or hand-edit
                        .claude/agents/, .opencode/agent/) if roles changed.

            Example:
              create-agent-project optimize ./my-project --apply
            """,

        ["compare"] = """
            Usage: create-agent-project compare

            Prints a comparison table of every built-in architecture profile:
            agent count, relative cost, complexity, reliability.
            """,

        ["templates"] = """
            Usage: create-agent-project templates

            Lists every built-in architecture profile with its agent roster.
            """,

        ["patterns"] = """
            Usage: create-agent-project patterns

            Lists every built-in work pattern (agent-in-the-loop, human-in-the-loop,
            debate-critic, swarm-parallel, ...) with its description and any
            structural guarantees it enforces (forced roles, minimum architecture size).
            """,
    };

    public static bool TryGetCommandHelp(string command, out string help) =>
        CommandHelp.TryGetValue(command, out help!);
}
