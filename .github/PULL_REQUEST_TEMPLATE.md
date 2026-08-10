## What changed and why

<!-- The "why" matters more than the "what" — the diff already shows what changed. -->

## Checklist

- [ ] `dotnet test` passes locally
- [ ] Added/updated tests for the behavior change (especially if touching
      `ArchitectureRecommender`, `ArchitectureOptimizer`, `Patterns`, or
      `ScaffoldGenerator`/`YamlLoader` round-trip)
- [ ] `AgentProjectArchitect.Core` still has no `Console` I/O
- [ ] No network/LLM calls introduced in the core recommendation/generation path
- [ ] Ran `dotnet run --project examples/GenerateExamples` if a change
      affects what gets generated, and committed the regenerated `examples/`

## Related issue

Closes #
