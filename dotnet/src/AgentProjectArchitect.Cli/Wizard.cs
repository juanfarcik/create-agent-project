using System.Text.RegularExpressions;
using AgentProjectArchitect.Core;

namespace AgentProjectArchitect.Cli;

/// <summary>
/// Interactive wizard with exactly two entry points: Simple (plain
/// language, smart defaults) and Advanced (full explicit control).
/// Every prompt is available in English or Spanish — pass the language
/// code ("en" or "es") the caller already asked for once, up front.
/// </summary>
public static class Wizard
{
    private static string Slugify(string text)
    {
        var slug = Regex.Replace(text.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        if (slug.Length > 40) slug = slug[..40];
        return string.IsNullOrEmpty(slug) ? "my-project" : slug;
    }

    /// <summary>Asks which language to run the rest of the wizard in. Call this first.</summary>
    public static string AskLanguage()
    {
        Console.WriteLine("Language / Idioma:");
        Console.WriteLine("  1. English (default)");
        Console.WriteLine("  2. Español");
        Console.Write("> [1]: ");
        var raw = (Console.ReadLine() ?? "").Trim();
        return raw == "2" ? "es" : "en";
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

    /// <summary>Pick zero or more options by comma-separated number, e.g. "1,3". Empty = none.</summary>
    private static List<string> ChooseMulti(string question, (string Value, string Label)[] options, string hint)
    {
        Console.WriteLine($"\n{question}");
        for (var i = 0; i < options.Length; i++)
            Console.WriteLine($"  {i + 1}. {options[i].Label}");
        Console.Write($"> ({hint}): ");
        var raw = (Console.ReadLine() ?? "").Trim();
        if (raw.Length == 0) return new List<string>();

        var result = new List<string>();
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out var idx) && idx >= 1 && idx <= options.Length)
                result.Add(options[idx - 1].Value);
        }
        return result;
    }

    private static int IndexOf((string Value, string Label)[] options, string value, int fallback)
    {
        for (var i = 0; i < options.Length; i++)
            if (options[i].Value == value) return i;
        return fallback;
    }

    private static readonly Dictionary<string, string> DomainToEngine = new()
    {
        ["writing"] = "creative",
        ["design"] = "creative",
        ["creative"] = "creative",
    };

    private static string EngineDomain(string chosen) =>
        DomainToEngine.TryGetValue(chosen, out var d) ? d : chosen;

    // Friendly, persona-oriented labels; internal domain values are what
    // the architecture engine branches on, so a writer, a designer, and a
    // musician all map to "creative" without separate engine logic.
    private static (string Value, string Label)[] DomainChoices(string lang) => lang == "es"
        ? new[]
        {
            ("software", "Armar software / una app"),
            ("writing", "Escritura (libro, blog, docs, guiones)"),
            ("design", "Diseño (visual, producto, UX)"),
            ("creative", "Música, arte, video u otro trabajo creativo"),
            ("research", "Investigación / aprender sobre algo"),
            ("business", "Negocio / estrategia / análisis de mercado"),
            ("ops", "Operaciones / tareas recurrentes"),
            ("general", "No estoy seguro / otra cosa"),
        }
        : new[]
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

    private static (string Value, string Label)[] RuntimeChoices(string lang) => lang == "es"
        ? new[]
        {
            ("agnostic", "Solo la base — funciona con cualquier CLI agéntico, sin extras"),
            ("claude-code", "Claude Code (agrega subagents nativos)"),
            ("opencode", "OpenCode (agrega subagents nativos)"),
            ("codex-cli", "Codex CLI"),
            ("all", "Generar los extras nativos para todos"),
        }
        : new[]
        {
            ("agnostic", "Just give me the base — works with any agentic CLI, no extras"),
            ("claude-code", "Claude Code (adds native subagents)"),
            ("opencode", "OpenCode (adds native subagents)"),
            ("codex-cli", "Codex CLI"),
            ("all", "Generate the native extras for all of them"),
        };

    private static string RuntimeChoice(string lang) => Choose(
        lang == "es"
            ? "¿Para qué CLI querés integración nativa extra? (la base funciona con cualquiera igual)"
            : "Which CLI do you want extra native integration for? (the base works with any of them either way)",
        RuntimeChoices(lang), 0);

    // Technical pattern ids shown alongside the plain-language description —
    // so "¿por qué no veo agent-in-the-loop?" has an obvious answer: it's
    // right there in parentheses.
    private static (string Value, string Label)[] SimplePatternChoices(string lang) => lang == "es"
        ? new[]
        {
            ("auto", "No estoy seguro — que decida la herramienta (auto)"),
            ("interactive", "Yo manejo — preguntame antes de cada cosa (interactive)"),
            ("agent-in-the-loop", "Que trabaje solo una lista de tareas, consultame solo si se traba (agent-in-the-loop)"),
            ("human-in-the-loop", "Que proponga cada paso y espere mi OK (human-in-the-loop)"),
            ("scheduled-digest", "Que corra en un horario fijo (ej. cada mañana) y me dé un reporte (scheduled-digest)"),
        }
        : new[]
        {
            ("auto", "Not sure — let the tool decide (auto)"),
            ("interactive", "I'll be driving — ask before each thing (interactive)"),
            ("agent-in-the-loop", "Let it work through a to-do list on its own, check with me only if stuck (agent-in-the-loop)"),
            ("human-in-the-loop", "Propose each step and wait for my OK (human-in-the-loop)"),
            ("scheduled-digest", "Run on a schedule (e.g. every morning) and give me a report (scheduled-digest)"),
        };

    // -------------------------------------------------------------
    // Simple
    // -------------------------------------------------------------

    public static Requirements Simple(string lang = "en")
    {
        string L(string en, string es) => lang == "es" ? es : en;

        Console.WriteLine(L("\n== Let's set up your project ==", "\n== Armemos tu proyecto =="));
        Console.WriteLine(L(
            "(Just answer in plain language — we'll figure out the rest.)\n",
            "(Contestá en lenguaje normal — nosotros nos encargamos del resto.)\n"));

        var objective = Ask(L("What do you want to accomplish?", "¿Qué querés lograr?"));
        var name = Ask(L("Give it a short name", "Ponele un nombre corto"),
            objective.Length > 0 ? Slugify(objective) : "my-project");
        var domainChoice = Choose(L("What kind of project is this?", "¿Qué tipo de proyecto es?"), DomainChoices(lang));
        var dod = Ask(L("How will you know it's done? (optional)", "¿Cómo vas a saber que está terminado? (opcional)"));
        var patternId = Choose(L("How should it work with you?", "¿Cómo debería trabajar con vos?"), SimplePatternChoices(lang));
        var runtime = RuntimeChoice(lang);

        var pattern = Patterns.Get(patternId);
        var overrides = pattern.Overrides;

        return new Requirements
        {
            Name = Slugify(name),
            Objective = objective.Length > 0 ? objective : L(
                "Not yet defined — refine in project/goal.md",
                "Todavía sin definir — completalo en project/goal.md"),
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

    public static (Requirements Requirements, List<string> AdditionalRoles) Advanced(string lang = "en")
    {
        string L(string en, string es) => lang == "es" ? es : en;

        Console.WriteLine(L("\n== Advanced setup ==", "\n== Configuración avanzada =="));
        Console.WriteLine(L(
            "(Every field below shapes the generated architecture.)\n",
            "(Cada campo de abajo define la arquitectura generada.)\n"));

        var objective = Ask(L("What do you want to accomplish?", "¿Qué querés lograr?"));
        var name = Ask(L("Project name", "Nombre del proyecto"), objective.Length > 0 ? Slugify(objective) : "my-project");
        var domainChoice = Choose(L("Project domain", "Dominio del proyecto"), DomainChoices(lang));
        var dod = Ask(L("Definition of Done", "Definición de \"terminado\""));
        var context = Ask(L("Initial context (optional)", "Contexto inicial (opcional)"));
        var constraints = Ask(L("Constraints (optional)", "Restricciones (opcional)"));

        Console.WriteLine(L(
            "\nWork patterns (agent-in-the-loop, human-in-the-loop, swarm, debate, ...):",
            "\nPatrones de trabajo (agent-in-the-loop, human-in-the-loop, swarm, debate, ...):"));
        foreach (var (pid, _) in Patterns.Choices())
            Console.WriteLine($"  - {pid}: {Patterns.All[pid].Description}");
        var patternChoices = Patterns.Choices().Select(c => (c.Id, c.Label)).ToArray();
        var patternId = Choose(L("Work pattern", "Patrón de trabajo"), patternChoices);
        var pattern = Patterns.All[patternId];
        var overrides = pattern.Overrides;
        Console.WriteLine(L(
            $"(Picking sensible defaults below for '{pattern.Label}' — override anything you like.)",
            $"(Los valores por defecto de abajo se ajustan a '{pattern.Label}' — cambiá lo que quieras.)"));

        var size = Choose(L("Project size", "Tamaño del proyecto"), lang == "es"
            ? new[] { ("tiny", "Personal / muy chico"), ("small", "Chico"), ("medium", "Mediano"), ("large", "Grande") }
            : new[] { ("tiny", "Personal / tiny"), ("small", "Small"), ("medium", "Medium"), ("large", "Large") }, 1);

        var lifetime = Choose(L("Project lifetime", "Duración del proyecto"), lang == "es"
            ? new[] { ("session", "Una sola sesión"), ("days", "Varios días"), ("weeks", "Varias semanas"), ("long-running", "De largo plazo") }
            : new[] { ("session", "One session"), ("days", "Several days"), ("weeks", "Several weeks"), ("long-running", "Long-running") });

        var autonomyOptions = lang == "es"
            ? new[]
            {
                ("human", "Mayormente humano"),
                ("collaborative", "Colaborativo"),
                ("mostly-autonomous", "Mayormente autónomo"),
                ("autonomous", "Totalmente autónomo"),
            }
            : new[]
            {
                ("human", "Mostly human"),
                ("collaborative", "Collaborative"),
                ("mostly-autonomous", "Mostly autonomous"),
                ("autonomous", "Fully autonomous"),
            };
        var autonomy = Choose(L("Desired autonomy", "Autonomía deseada"), autonomyOptions,
            IndexOf(autonomyOptions, overrides.GetValueOrDefault("autonomy", "collaborative"), 1));

        var risk = Choose(L("Risk", "Riesgo"), lang == "es"
            ? new[] { ("low", "Bajo"), ("medium", "Medio"), ("high", "Alto"), ("critical", "Crítico") }
            : new[] { ("low", "Low"), ("medium", "Medium"), ("high", "High"), ("critical", "Critical") });

        var executionOptions = lang == "es"
            ? new[]
            {
                ("interactive", "Interactivo"),
                ("agent-loop", "Agente en loop (agent-in-a-loop)"),
                ("scheduled", "Programado (horario fijo)"),
                ("continuous", "Continuo"),
                ("event-driven", "Disparado por eventos"),
            }
            : new[]
            {
                ("interactive", "Interactive"), ("agent-loop", "Agent-in-a-loop"),
                ("scheduled", "Scheduled"), ("continuous", "Continuous"),
                ("event-driven", "Event-driven"),
            };
        var executionMode = Choose(L("Execution mode", "Modo de ejecución"), executionOptions,
            IndexOf(executionOptions, overrides.GetValueOrDefault("execution_mode", "interactive"), 0));

        string? schedule = null;
        if (executionMode == "scheduled")
        {
            schedule = Ask(L(
                "Schedule (e.g. 'daily 08:00, max 30m, max $0.50/day')",
                "Horario (ej. 'todos los días 08:00, máx 30m, máx $0.50/día')"));
        }

        var budget = Choose(L("Budget / cost preference", "Preferencia de presupuesto / costo"), lang == "es"
            ? new[]
            {
                ("free", "Gratis / local"),
                ("ultra-low", "Costo ultra bajo"),
                ("hobby", "Hobby"),
                ("balanced", "Balanceado"),
                ("quality-first", "Calidad primero"),
            }
            : new[]
            {
                ("free", "Free / local"),
                ("ultra-low", "Ultra low cost"),
                ("hobby", "Hobby"),
                ("balanced", "Balanced"),
                ("quality-first", "Quality first"),
            }, 2);

        var humanOptions = lang == "es"
            ? new[]
            {
                ("none", "Ninguna, salvo que falle"),
                ("exceptions", "Ante excepciones"),
                ("important-decisions", "Ante decisiones importantes"),
                ("per-phase", "Aprobación por fase"),
                ("per-action", "Aprobación por acción"),
            }
            : new[]
            {
                ("none", "None unless failure"),
                ("exceptions", "On exceptions"),
                ("important-decisions", "On important decisions"),
                ("per-phase", "Approval per phase"),
                ("per-action", "Approval per action"),
            };
        var human = Choose(L("Human involvement", "Involucramiento humano"), humanOptions,
            IndexOf(humanOptions, overrides.GetValueOrDefault("human_involvement", "important-decisions"), 2));

        var runtime = RuntimeChoice(lang);

        Console.WriteLine(L(
            "\nThe architecture engine already picks roles automatically based on " +
            "everything above. Only add here if you know you want a specific extra " +
            "specialist available (on-demand — it won't run unless invoked):",
            "\nEl motor de arquitectura ya elige roles automáticamente en base a todo " +
            "lo anterior. Agregá acá solo si sabés que querés un especialista extra " +
            "disponible (on-demand — no se ejecuta salvo que se invoque):"));
        var roleOptions = Roles.Names
            .Select(r => (r, $"{r} — {Roles.All[r].Description}"))
            .ToArray();
        var additionalRoles = ChooseMulti(
            L("Additional specialist roles (optional)", "Roles especialistas adicionales (opcional)"),
            roleOptions,
            L("comma-separated numbers, or blank for none", "números separados por coma, o vacío para ninguno"));

        var req = new Requirements
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
        return (req, additionalRoles);
    }
}
