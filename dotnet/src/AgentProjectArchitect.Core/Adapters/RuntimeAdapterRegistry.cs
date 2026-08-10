namespace AgentProjectArchitect.Core;

/// <summary>
/// Resolves <see cref="Requirements.Runtime"/> to the <see cref="IRuntimeAdapter"/>(s)
/// that should run. Depends only on the <see cref="IRuntimeAdapter"/> abstraction
/// (Dependency Inversion) — callers never touch concrete adapter classes.
/// </summary>
public sealed class RuntimeAdapterRegistry
{
    private readonly IReadOnlyList<IRuntimeAdapter> _adapters;

    public RuntimeAdapterRegistry(IEnumerable<IRuntimeAdapter> adapters)
    {
        _adapters = adapters.ToList();
    }

    /// <summary>The built-in adapters this tool ships with.</summary>
    public static RuntimeAdapterRegistry Default { get; } = new(new IRuntimeAdapter[]
    {
        new ClaudeCodeAdapter(),
        new OpenCodeAdapter(),
        new CodexCliAdapter(),
    });

    /// <summary>Runs every adapter <paramref name="req"/>.Runtime resolves to, returns their ids.</summary>
    public List<string> Generate(string root, Requirements req, Architecture arch)
    {
        var wantedIds = req.Runtime is "all" or "both"
            ? _adapters.Select(a => a.Id).ToList()
            : new List<string> { req.Runtime };

        var generated = new List<string>();
        foreach (var id in wantedIds)
        {
            var adapter = _adapters.FirstOrDefault(a => a.Id == id);
            if (adapter is null) continue;

            adapter.Generate(root, req, arch);
            generated.Add(adapter.Id);
        }
        return generated;
    }
}
