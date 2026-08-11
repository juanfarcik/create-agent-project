namespace AgentProjectArchitect.Core;

/// <summary>
/// A runtime adapter generates only what a given agentic CLI (Claude Code,
/// OpenCode, Codex CLI, ...) needs on top of the runtime-independent core
/// (<c>AGENTS.md</c>, <c>.agent/</c>, <c>project/</c>) — it never
/// duplicates the project model.
///
/// Strategy pattern: adding support for a new runtime means adding a new
/// class that implements this interface and registering it in
/// <see cref="RuntimeAdapterRegistry"/> — no existing adapter code changes
/// (Open/Closed Principle).
/// </summary>
public interface IRuntimeAdapter
{
    /// <summary>The <see cref="Requirements.Runtime"/> value this adapter handles.</summary>
    string Id { get; }

    /// <summary>Writes this runtime's files under <paramref name="root"/>.</summary>
    void Generate(string root, Requirements req, Architecture arch);
}
