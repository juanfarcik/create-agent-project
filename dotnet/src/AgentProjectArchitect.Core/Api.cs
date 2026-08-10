namespace AgentProjectArchitect.Core;

/// <summary>
/// Programmatic entry point — no console I/O, no interactive prompts.
/// This is the seam a future UI (web form, GUI, anything else) is meant
/// to call instead of going through the CLI's wizard.
/// </summary>
public static class Api
{
    /// <summary>Requirements -> recommended Architecture. No side effects.</summary>
    public static Architecture Preview(Requirements req) => ArchitectureRecommender.Recommend(req);

    public sealed class BuildResult
    {
        public required string Root { get; init; }
        public required Requirements Requirements { get; init; }
        public required Architecture Architecture { get; init; }
        public required List<string> Adapters { get; init; }
    }

    /// <summary>
    /// Generate a complete project on disk and return what was produced.
    /// If <paramref name="arch"/> is omitted, it's derived from
    /// <paramref name="req"/> via <see cref="Preview"/>.
    /// </summary>
    public static BuildResult BuildProject(string root, Requirements req, Architecture? arch = null, bool optimize = false)
    {
        arch ??= Preview(req);
        if (optimize) arch = ArchitectureOptimizer.Optimize(arch, req);

        ScaffoldGenerator.Generate(root, req, arch);
        var generatedAdapters = RuntimeAdapterRegistry.Default.Generate(root, req, arch);

        return new BuildResult { Root = root, Requirements = req, Architecture = arch, Adapters = generatedAdapters };
    }
}
