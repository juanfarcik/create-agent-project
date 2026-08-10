using System.Text.RegularExpressions;
using AgentProjectArchitect.Core;

namespace AgentProjectArchitect.Cli;

/// <summary>
/// Interactive wizard with exactly two entry points: Simple (plain
/// language, smart defaults) and Advanced (full explicit control).
/// </summary>
public static class Wizard
{
    private static string Slugify(string text)
    {
        var slug = Regex.Replace(text.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        if (slug.Length > 40) slug = slug[..40];
        return string.IsNullOrEmpty(slug) ? "my-project" : slug;
    }

    private static string Ask(string question, string defaultValue = "")
    {
        var suffix = defaultValue.Length > 0 ? $" [{defaultValue}]" : "";
        Console.Write($"{question}{suffix}: ");
        var value = (Console.ReadLine() ?? "").Trim();
        return value.Length > 0 ? value : defaultValue;
    }

    private static string Choose(string question, (string Value, string Label)[] options, int defaultIdx = 0)
    {
        Console.WriteLine($"\n{question}");
        for (var i = 0; i < options.Length; i++)
        {
            var marker = i == defaultIdx ? " (default)" : "";
            Console.WriteLine($"  {i + 1}. {options[i].Label}{marker}");
        }
        Console.Write($"> [{defaultIdx + 1}]: ");
        var raw = (Console.ReadLine() ?? "").Trim();
        if (raw.Length == 0) return options[defaultIdx].Value;
        if (int.TryParse(raw, out var idx) && idx >= 1 && idx <= options.Length)
            return options[idx - 1].Value;
        return options[defaultIdx].Value;
    }

    private static int IndexOf((string Value, string Label)[] options, string value, int fallback)
    {
        for (var i = 0; i < options.Length; i++)
            if (options[i].Value == value) return i;
        return fallback;
    }

    // Friendly, persona-oriented labels; internal domain values are what
    // the architecture engine branches on, so a writer, a designer, and a
    // musician all map to "creative" without separate engine logic.
    private static readonly (string Value, string Label)[] DomainChoices =
    {
        ("software", "Building software / an app"),
        ("writing", "Writing (book, blog, docs, scripts)"),
        ("design", "Design (visual, product, UX)"),
        ("creative", "Music, art, video, or other creative work"),
        ("research", "Research / learning about something"),
        ("business", "Business / strategy / market analysis"),
        ("ops", "Operations / recurring tasks"),
        ("general", "Not sure / something else"),
    };

    private static readonly Dictionary<string, string> DomainToEngine = new()
    {
        ["writing"] = "creative",
        ["design"] = "creative",
        ["creative"] = "creative",
    };

    private static string EngineDomain(string chosen) =>
        DomainToEngine.TryGetValue(chosen, out var d) ? d : chosen;

    private static readonly (string Value, string Label)[] RuntimeChoices =
    {
        ("agnostic", "Just give me the base — works with any agentic CLI, no extras"),
        ("claude-code", "Claude Code (adds native subagents)"),
        ("opencode", "OpenCode (adds native subagents)"),
        ("codex-cli", "Codex CLI"),
        ("all", "Generate the native extras for all of them"),
    };

    private static string RuntimeChoice() => Choose(
        "Which CLI do you want extra native integration for? (the base works with any of them either way)",
        RuntimeChoices, 0);

    private static readonly (string Value, string Label)[] SimplePatternChoices =
    {
        ("auto", "Not sure — let the tool decide"),
        ("interactive", "I'll be driving — ask before each thing"),
        ("agent-in-the-loop", "Let it work through a to-do list on its own, check with me only if stuck"),
        ("human-in-the-loop", "Propose each step and wait for my OK"),
        ("scheduled-digest", "Run on a schedule (e.g. every morning) and give me a report"),
    };

    // -------------------------------------------------------------
    // Simple
    // -------------------------------------------------------------

    public static Requirements Simple()
    {
        Console.WriteLine("\n== Let's set up your project ==");
        Console.WriteLine("(Just answer in plain language — we'll figure out the rest.)\n");

        var objective = Ask("What do you want to accomplish?");
        var name = Ask("Give it a short name", objective.Length > 0 ? Slugify(objective) : "my-project");
        var domainChoice = Choose("What kind of project is this?", DomainChoices);
        var dod = Ask("How will you know it's done? (optional)");
        var patternId = Choose("How should it work with you?", SimplePatternChoices);
        var runtime = RuntimeChoice();

        var pattern = Patterns.Get(patternId);
        var overrides = pattern.Overrides;

        return new Requirements
        {
            Name = Slugify(name),
            Objective = objective.Length > 0 ? objective : "Not yet defined — refine in .project/goal.md",
            Domain = EngineDomain(domainChoice),
            DefinitionOfDone = dod,
            Size = "small",
            Lifetime = "session",
            Autonomy = overrides.GetValueOrDefault("autonomy", "collaborative"),
            Risk = "low",
            BudgetProfile = "hobby",
            ExecutionMode = overrides.GetValueOrDefault("execution_mode", "interactive"),
            Runtime = runtime,
            HumanInvolvement = overrides.GetValueOrDefault("human_involvement", "important-decisions"),
            LoopPattern = patternId,
            ExperienceLevel = "beginner",
        };
    }

    // -------------------------------------------------------------
    // Advanced
    // -------------------------------------------------------------

    public static Requirements Advanced()
    {
        Console.WriteLine("\n== Advanced setup ==");
        Console.WriteLine("(Every field below shapes the generated architecture.)\n");

        var objective = Ask("What do you want to accomplish?");
        var name = Ask("Project name", objective.Length > 0 ? Slugify(objective) : "my-project");
        var domainChoice = Choose("Project domain", DomainChoices);
        var dod = Ask("Definition of Done");
        var context = Ask("Initial context (optional)");
        var constraints = Ask("Constraints (optional)");

        Console.WriteLine("\nWork patterns (agent-in-the-loop, human-in-the-loop, swarm, debate, ...):");
        foreach (var (pid, _) in Patterns.Choices())
            Console.WriteLine($"  - {pid}: {Patterns.All[pid].Description}");
        var patternChoices = Patterns.Choices().Select(c => (c.Id, c.Label)).ToArray();
        var patternId = Choose("Work pattern", patternChoices);
        var pattern = Patterns.All[patternId];
        var overrides = pattern.Overrides;
        Console.WriteLine($"(Picking sensible defaults below for '{pattern.Label}' — override anything you like.)");

        var size = Choose("Project size", new[]
        {
            ("tiny", "Personal / tiny"), ("small", "Small"),
            ("medium", "Medium"), ("large", "Large"),
        }, 1);

        var lifetime = Choose("Project lifetime", new[]
        {
            ("session", "One session"), ("days", "Several days"),
            ("weeks", "Several weeks"), ("long-running", "Long-running"),
        });

        var autonomyOptions = new[]
        {
            ("human", "Mostly human"),
            ("collaborative", "Collaborative"),
            ("mostly-autonomous", "Mostly autonomous"),
            ("autonomous", "Fully autonomous"),
        };
        var autonomy = Choose("Desired autonomy", autonomyOptions,
            IndexOf(autonomyOptions, overrides.GetValueOrDefault("autonomy", "collaborative"), 1));

        var risk = Choose("Risk", new[]
        {
            ("low", "Low"), ("medium", "Medium"),
            ("high", "High"), ("critical", "Critical"),
        });

        var executionOptions = new[]
        {
            ("interactive", "Interactive"), ("agent-loop", "Agent-in-a-loop"),
            ("scheduled", "Scheduled"), ("continuous", "Continuous"),
            ("event-driven", "Event-driven"),
        };
        var executionMode = Choose("Execution mode", executionOptions,
            IndexOf(executionOptions, overrides.GetValueOrDefault("execution_mode", "interactive"), 0));

        string? schedule = null;
        if (executionMode == "scheduled")
            schedule = Ask("Schedule (e.g. 'daily 08:00, max 30m, max $0.50/day')");

        var budget = Choose("Budget / cost preference", new[]
        {
            ("free", "Free / local"),
            ("ultra-low", "Ultra low cost"),
            ("hobby", "Hobby"),
            ("balanced", "Balanced"),
            ("quality-first", "Quality first"),
        }, 2);

        var humanOptions = new[]
        {
            ("none", "None unless failure"),
            ("exceptions", "On exceptions"),
            ("important-decisions", "On important decisions"),
            ("per-phase", "Approval per phase"),
            ("per-action", "Approval per action"),
        };
        var human = Choose("Human involvement", humanOptions,
            IndexOf(humanOptions, overrides.GetValueOrDefault("human_involvement", "important-decisions"), 2));

        var runtime = RuntimeChoice();

        return new Requirements
        {
            Name = Slugify(name),
            Objective = objective,
            Domain = EngineDomain(domainChoice),
            DefinitionOfDone = dod,
            Context = context,
            Constraints = constraints,
            LoopPattern = patternId,
            Size = size,
            Lifetime = lifetime,
            Autonomy = autonomy,
            Risk = risk,
            ExecutionMode = executionMode,
            Schedule = schedule,
            BudgetProfile = budget,
            Runtime = runtime,
            HumanInvolvement = human,
            ExperienceLevel = "tech",
        };
    }
}
