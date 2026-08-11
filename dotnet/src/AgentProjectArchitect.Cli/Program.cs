using AgentProjectArchitect.Core;

namespace AgentProjectArchitect.Cli;

public static class Program
{
    private static readonly string[] Commands =
        { "new", "validate", "status", "architecture", "optimize", "compare", "templates", "patterns" };

    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(CliHelp.TopLevel);
            return 1;
        }

        if (args[0] is "-v" or "--version")
        {
            Console.WriteLine(CliHelp.Version);
            return 0;
        }

        if (args[0] is "-h" or "--help")
        {
            Console.WriteLine(CliHelp.TopLevel);
            return 0;
        }

        var command = args[0];
        var rest = args.Skip(1).ToArray();

        if (Commands.Contains(command) && rest.Any(a => a is "-h" or "--help"))
        {
            Console.WriteLine(CliHelp.TryGetCommandHelp(command, out var help) ? help : CliHelp.TopLevel);
            return 0;
        }

        try
        {
            return command switch
            {
                "new" => CmdNew(rest),
                "validate" => CmdValidate(rest),
                "status" => CmdStatus(rest),
                "architecture" => CmdArchitecture(rest),
                "optimize" => CmdOptimize(rest),
                "compare" => CmdCompare(),
                "templates" => CmdTemplates(),
                "patterns" => CmdPatterns(),
                _ => UnknownCommand(command),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"error: unknown command '{command}'\n");
        Console.WriteLine(CliHelp.TopLevel);
        return 1;
    }

    // -----------------------------------------------------------------
    // new
    // -----------------------------------------------------------------

    private static int CmdNew(string[] args)
    {
        var simple = args.Contains("--simple") || args.Contains("--quick");
        var advanced = args.Contains("--advanced") || args.Contains("--guided");
        string? runtime = null;
        string? path = null;

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--runtime" && i + 1 < args.Length) runtime = args[++i];
            else if (!args[i].StartsWith("--", StringComparison.Ordinal)) path = args[i];
        }

        Requirements req;
        List<string> additionalRoles = new();
        if (advanced) (req, additionalRoles) = Wizard.Advanced();
        else if (simple) req = Wizard.Simple();
        else
        {
            Console.WriteLine("How do you want to set this up?\n");
            Console.WriteLine("  1. Simple — I don't know/care about agent architecture, just ask me the basics");
            Console.WriteLine("  2. Advanced — let me configure size, risk, execution mode, budget, etc.");
            Console.Write("> [1]: ");
            var choice = (Console.ReadLine() ?? "").Trim();
            if (choice == "2") (req, additionalRoles) = Wizard.Advanced();
            else req = Wizard.Simple();
        }

        if (runtime != null) req.Runtime = runtime;

        var arch = Api.Preview(req);
        foreach (var role in additionalRoles)
        {
            if (!arch.AgentNames().Contains(role))
                arch.Agents.Add(new AgentSpec(role, "on-demand", "balanced"));
        }
        PrintArchitecture(arch);
        PrintProjectStructure(req, arch);

        while (true)
        {
            Console.Write("[G]enerate / [C]ustomize (optimize) / [T]ry another / [A]bort: ");
            var choice = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            if (choice is "g" or "") break;
            if (choice == "c")
            {
                arch = ArchitectureOptimizer.Optimize(arch, req);
                PrintArchitecture(arch, "Optimized Architecture");
                PrintProjectStructure(req, arch);
                continue;
            }
            if (choice == "t")
            {
                Console.Write($"Profile ({string.Join(", ", ArchitectureProfileCatalog.Profiles.Keys)}): ");
                var name = (Console.ReadLine() ?? "").Trim();
                if (ArchitectureProfileCatalog.Profiles.ContainsKey(name))
                {
                    arch = ArchitectureProfileCatalog.Build(name);
                    PrintArchitecture(arch, $"Architecture: {name}");
                    PrintProjectStructure(req, arch);
                }
                continue;
            }
            if (choice == "a")
            {
                Console.WriteLine("Aborted.");
                return 0;
            }
        }

        var root = path ?? req.Name;
        var result = Api.BuildProject(root, req, arch);
        Console.WriteLine($"\nGenerated project at: {Path.GetFullPath(result.Root)}");
        Console.WriteLine($"Native runtime extras: {(result.Adapters.Count > 0 ? string.Join(", ", result.Adapters) : "none (agnostic base only — works with any AGENTS.md-reading CLI)")}");
        Console.WriteLine($"Next: open {result.Root}/AGENTS.md with your agent runtime.");
        return 0;
    }

    // -----------------------------------------------------------------
    // validate
    // -----------------------------------------------------------------

    private static int CmdValidate(string[] args)
    {
        var root = RequirePath(args);
        var problems = new List<string>();

        string[] required =
        {
            "AGENTS.md", ".agent/project.yaml", ".agent/architecture.yaml",
            "project/goal.md", "project/state.md",
        };
        foreach (var rel in required)
            if (!File.Exists(Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar))))
                problems.Add($"missing {rel}");

        if (problems.Count == 0)
        {
            var req = YamlLoader.LoadRequirements(root);
            var arch = YamlLoader.LoadArchitecture(root);
            foreach (var a in arch.Agents)
            {
                if (!Roles.All.ContainsKey(a.Role))
                    problems.Add($"unknown role in architecture.yaml: {a.Role}");
                else if (!File.Exists(Path.Combine(root, ".agent", "prompts", $"{a.Role}.md")))
                    problems.Add($"missing prompt for role: {a.Role}");
            }
            if (string.IsNullOrWhiteSpace(req.Objective))
                problems.Add("project.yaml: objective is empty");
        }

        if (problems.Count > 0)
        {
            Console.WriteLine($"INVALID — {problems.Count} problem(s):");
            foreach (var p in problems) Console.WriteLine($"  - {p}");
            return 1;
        }
        Console.WriteLine("VALID");
        return 0;
    }

    // -----------------------------------------------------------------
    // status
    // -----------------------------------------------------------------

    private static int CmdStatus(string[] args)
    {
        var root = RequirePath(args);
        foreach (var f in new[] { "project/state.md", "project/metrics.md" })
        {
            var p = Path.Combine(root, f.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(p))
            {
                Console.WriteLine($"--- {f} ---");
                Console.WriteLine(File.ReadAllText(p));
            }
        }
        return 0;
    }

    // -----------------------------------------------------------------
    // architecture
    // -----------------------------------------------------------------

    private static int CmdArchitecture(string[] args)
    {
        var root = RequirePath(args);
        var recommend = args.Contains("--recommend");

        var current = YamlLoader.LoadArchitecture(root);
        PrintArchitecture(current, "Current Architecture");

        if (recommend)
        {
            var req = YamlLoader.LoadRequirements(root);
            var recommended = ArchitectureRecommender.Recommend(req);
            PrintArchitecture(recommended, "Recommended (from current requirements)");
        }
        return 0;
    }

    // -----------------------------------------------------------------
    // optimize
    // -----------------------------------------------------------------

    private static int CmdOptimize(string[] args)
    {
        var root = RequirePath(args);
        var apply = args.Contains("--apply");

        var req = YamlLoader.LoadRequirements(root);
        var current = YamlLoader.LoadArchitecture(root);
        var optimized = ArchitectureOptimizer.Optimize(current, req);
        PrintArchitecture(optimized, "Optimized Architecture");

        if (apply)
        {
            var yaml = ScaffoldGenerator.ArchitectureYaml(optimized);
            File.WriteAllText(Path.Combine(root, ".agent", "architecture.yaml"), yaml.TrimEnd() + "\n");
            Console.WriteLine("Applied. Re-run `validate` and regenerate adapters if agent roles changed.");
        }
        return 0;
    }

    // -----------------------------------------------------------------
    // compare / templates / patterns
    // -----------------------------------------------------------------

    private static int CmdCompare()
    {
        Console.WriteLine(ArchitectureComparisonView.Render());
        return 0;
    }

    private static int CmdTemplates()
    {
        foreach (var (name, factory) in ArchitectureProfileCatalog.Profiles)
        {
            var a = factory();
            Console.WriteLine($"{name,-18} agents: {string.Join(", ", a.AgentNames())}");
        }
        return 0;
    }

    private static int CmdPatterns()
    {
        foreach (var (pid, _) in Patterns.Choices())
        {
            var p = Patterns.All[pid];
            Console.WriteLine($"{p.Id,-24} {p.Label}");
            Console.WriteLine($"  {p.Description}");
            if (p.ForceRoles.Count > 0)
                Console.WriteLine($"  guarantees: {string.Join(", ", p.ForceRoles.Select(r => $"{r.Role} always-on"))}");
            if (p.MinProfile != null)
                Console.WriteLine($"  minimum architecture: {p.MinProfile}");
            Console.WriteLine();
        }
        return 0;
    }

    // -----------------------------------------------------------------
    // helpers
    // -----------------------------------------------------------------

    private static string RequirePath(string[] args)
    {
        var path = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));
        if (path == null)
        {
            Console.Error.WriteLine("error: missing <path> argument");
            Environment.Exit(1);
        }
        return path!;
    }

    private static void PrintArchitecture(Architecture arch, string title = "Recommended Architecture")
    {
        var pattern = Patterns.Get(arch.LoopPattern);
        Console.WriteLine($"\n{title}\n");
        Console.WriteLine($"Architecture: {arch.Profile.ToUpperInvariant()}");
        Console.WriteLine($"Work pattern: {pattern.Label}\n");
        Console.WriteLine("Agents:");
        foreach (var a in arch.Agents)
            Console.WriteLine($"  {a.Role} ({a.Mode}, {a.ModelTier})");
        Console.WriteLine($"\nMemory: {arch.Memory}");
        Console.WriteLine($"Checkpoints: {arch.Checkpoints}");
        Console.WriteLine("\nHuman approval required for:");
        foreach (var g in arch.HumanGates) Console.WriteLine($"  - {g}");
        Console.WriteLine("\nEstimated:");
        Console.WriteLine($"  complexity: {arch.Complexity}");
        Console.WriteLine($"  agent calls: {arch.EstCallsPerRun}");
        Console.WriteLine($"  context: {arch.EstContext}");
        Console.WriteLine($"  cost: {arch.EstCost}");
        if (arch.Notes.Count > 0)
        {
            Console.WriteLine("\nNotes:");
            foreach (var n in arch.Notes) Console.WriteLine($"  {n}");
        }
        Console.WriteLine();
    }

    private static void PrintProjectStructure(Requirements req, Architecture arch)
    {
        var decisions = ProjectComponentCatalog.Decide(req, arch);
        Console.WriteLine("Project structure (project/):");
        foreach (var d in decisions)
        {
            var mark = d.Included ? "✓" : "○";
            Console.WriteLine($"  {mark} {d.Id,-12} {d.Reason}");
        }
        Console.WriteLine();
    }
}
