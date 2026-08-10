namespace AgentProjectArchitect.Core;

/// <summary>
/// Codex CLI (and most emerging agent CLIs) already reads <c>AGENTS.md</c>
/// at the project root by convention, so there is nothing to duplicate
/// here — this just drops a marker file so <c>validate</c> can confirm
/// the adapter was requested.
/// </summary>
public sealed class CodexCliAdapter : IRuntimeAdapter
{
    public string Id => "codex-cli";

    public void Generate(string root, Requirements req, Architecture arch)
    {
        var codexDir = Path.Combine(root, ".codex");
        Directory.CreateDirectory(codexDir);
        File.WriteAllText(Path.Combine(codexDir, "NOTES.md"), """
            # Codex CLI

            This project uses `AGENTS.md` at the project root as its instruction
            entry point — Codex CLI reads it natively, no adapter files required.

            Role prompts are available at `.agent/prompts/<role>.md` if you want to
            paste them into a task manually.
            """);
    }
}
