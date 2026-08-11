# Architecture

Technical reference for how this codebase is put together — for
contributors, and for anyone deciding whether to build on top of it.
For *why* each idea exists and where it came from, see
[`REFERENCES.md`](REFERENCES.md). This document is about *how* it's
implemented.

## 1. What this system is, precisely

A deterministic pipeline:

```
Requirements  ──▶  Architecture  ──▶  files on disk
 (what the        (which agent        (AGENTS.md, .agent/,
  user wants)       roles, how         project/, optional
                     they work)         vendor adapters)
```

No step in that pipeline calls an LLM, a network service, or anything
non-deterministic. Given the same `Requirements`, the output is always
the same `Architecture` and always the same files. This is a design
constraint, not an accident (see §9).

## 2. Project layout

```
dotnet/
├── src/
│   ├── AgentProjectArchitect.Core/    # domain model + rules engine + generator — zero Console I/O
│   │   ├── Models.cs                   # Requirements, AgentSpec, Architecture
│   │   ├── Roles.cs                    # Role library (data)
│   │   ├── Patterns.cs                 # Work pattern library (data)
│   │   ├── Api.cs                      # Preview() / BuildProject() — the programmatic seam
│   │   ├── YamlLoader.cs               # .agent/*.yaml -> domain objects
│   │   ├── Architecture/
│   │   │   ├── ArchitectureProfileCatalog.cs   # 9 built-in profiles (data)
│   │   │   ├── ArchitectureRecommender.cs      # Requirements -> Architecture
│   │   │   ├── ArchitectureOptimizer.cs        # complexity reduction
│   │   │   ├── ArchitectureCostEstimator.cs    # qualitative cost warning
│   │   │   └── LoopPatternApplier.cs           # pattern -> structural guarantees
│   │   ├── Adapters/
│   │   │   ├── IRuntimeAdapter.cs              # Strategy interface
│   │   │   ├── ClaudeCodeAdapter.cs
│   │   │   ├── OpenCodeAdapter.cs
│   │   │   ├── CodexCliAdapter.cs
│   │   │   └── RuntimeAdapterRegistry.cs       # resolves Requirements.Runtime -> adapters
│   │   └── Scaffold/
│   │       ├── ScaffoldGenerator.cs            # writes every file
│   │       └── ProjectComponentCatalog.cs      # which project/ subfolders to create, and why
│   └── AgentProjectArchitect.Cli/     # console wizard + command dispatch — all Console I/O lives here
│       ├── Program.cs                  # command dispatch, argument parsing, output formatting
│       ├── Wizard.cs                   # Simple() / Advanced() interactive flows
│       ├── CliHelp.cs                  # --help text, separate from execution logic
│       └── ArchitectureComparisonView.cs  # `compare` table rendering (presentation, not domain)
├── tests/AgentProjectArchitect.Tests/  # xUnit, one file per Core module roughly
├── examples/
│   ├── GenerateExamples/               # regenerates the 5 committed reference projects
│   └── <five example projects>/        # committed output, used as living documentation
├── scripts/publish.sh                  # builds self-contained release binaries
├── docs/
│   ├── ARCHITECTURE.md                 # this file
│   └── REFERENCES.md                   # provenance + trends self-assessment
└── Directory.Build.props, .editorconfig  # shared build config (nullable, analyzers, version)
```

**The Core/Cli split is the single most load-bearing architectural
decision in this codebase.** `AgentProjectArchitect.Core` has no
dependency on `Console`, no `Console.ReadLine`/`WriteLine` anywhere in
it — verify with `grep -rn Console src/AgentProjectArchitect.Core/`
(should return nothing). Everything interactive lives in
`AgentProjectArchitect.Cli`. This means a future web frontend calls
`Api.Preview()` / `Api.BuildProject()` directly and needs to write zero
new domain logic — only a new presentation layer, the same way the CLI
is a presentation layer over the same two calls.

## 3. Domain model (`Models.cs`)

```csharp
class Requirements {
    string Name, Objective, Domain, DefinitionOfDone, Context, Constraints;
    string Size, Lifetime, Autonomy, Risk;
    string BudgetProfile, ExecutionMode;
    string Runtime, HumanInvolvement;
    string? Schedule;
    string ExperienceLevel, LoopPattern;
}

class AgentSpec { string Role, Mode, ModelTier; }

class Architecture {
    string Profile;
    List<AgentSpec> Agents;
    string Memory;
    List<string> HumanGates;
    bool Checkpoints;
    string Complexity, EstCallsPerRun, EstContext, EstCost;
    List<string> Notes;
    string LoopPattern;
}
```

Every field on `Requirements` is a plain string (not an enum) with the
valid values documented in a comment next to it. This is deliberate: the
same strings round-trip through YAML (`YamlLoader`), through the CLI's
`--runtime <value>` flag, and through the wizard's `Choose()` helper's
`(value, label)` tuples, with no enum-to-string mapping layer anywhere.
The cost is that an invalid string silently falls through to a fallback
value in `ArchitectureRecommender` (via `IndexOf(..., fallback)`) instead
of throwing — a deliberate trade-off toward "never crash on a malformed
`.agent/project.yaml`" over "fail loudly on invalid input." `validate`
is the intended place to catch structural problems, not silent parsing.

`Architecture.Agents` is a mutable `List<AgentSpec>`, not an immutable
collection — `ArchitectureOptimizer`, `LoopPatternApplier`, and the
CLI's "add additional roles" step all mutate it in place. `Architecture`
itself is a mutable class for the same reason: profiles are built fresh
per call (`ArchitectureProfileCatalog.Build` returns a new instance from
a `Func<Architecture>` factory) specifically so mutation is always safe
and never accidentally shares state between two `Recommend()` calls.

## 4. The recommendation pipeline

```
ArchitectureRecommender.Recommend(Requirements)
  │
  ├─ software domain?  → pick software-lean / -standard / -high-reliability
  │                       by (size, risk) score, then Finalize()
  │
  ├─ research domain?  → research profile
  ├─ score ≤ 1, risk=0 → minimal
  ├─ score ≤ 2         → lean
  ├─ risk ≥ 2 or score ≥ 5 → high-reliability
  ├─ size ≥ 2          → collaborative
  ├─ else              → lean
  │
  ├─ creative domain?  → swap "analyst" role for "creative-director"
  │
  └─ Finalize(arch, req, risk, lifetime)
       ├─ scheduled/continuous/event-driven or long lifetime → upgrade to
       │   autonomous-loop if still minimal/lean; set Checkpoints=true
       ├─ risk ≥ 2 → ensure risk-reviewer present; add "high-risk decisions" gate
       ├─ autonomy → adds gates (never changes agent count)
       ├─ LoopPatternApplier.Apply(arch, req)
       │   ├─ pattern has MinProfile? upgrade arch if too small
       │   ├─ pattern has ForceRoles? set mode or add AgentSpec for each
       │   └─ pattern has Note? append to arch.Notes; set arch.LoopPattern
       └─ ArchitectureCostEstimator.Estimate(arch, req)
           └─ sum(model-tier weights) vs budget-profile cap → warning note
```

`score = size_index + risk_index + (lifetime_index >= 2 ? 1 : 0)` is the
single scalar the profile-selection thresholds are written against. It
is intentionally crude (three ordinal dimensions summed) rather than a
weighted model — the profiles are coarse buckets (9 of them), so a
precise score would be false precision. If you're tuning thresholds,
change the `if`/`else if` chain in `Recommend()` directly; there's no
separate scoring configuration to keep in sync with it.

**Both branches (software and non-software) converge on the same
`Finalize()` call.** This is what guarantees loop-pattern guarantees,
risk gates, and cost warnings apply uniformly regardless of domain —
`Finalize` doesn't know or care which branch got it there.

## 5. The optimizer (`ArchitectureOptimizer.cs`)

Four rules, applied in order, each appending a human-readable line to
`removed` (surfaced in `Architecture.Notes` on the returned instance):

1. **Demote review roles under low risk + small size.** `critic`/
   `evaluator` go from `always` to `on-demand` if `risk == 0 && size ≤
   1`. Doesn't touch other roles.
2. **Demote (never remove) non-core roles under tight budget.**
   `CoreDoerRoles = {orchestrator, researcher, executor, coder,
   evaluator}` are untouchable at this step — always kept as-is. Every
   other `always`-mode role gets demoted to `on-demand` (not deleted)
   when `BudgetProfile` is `free`/`ultra-low`/`hobby`. This is the fix
   for a real bug caught during development: an earlier version of this
   rule *removed* non-core roles outright, which could strip `coder`
   from a software project under a tight budget, leaving an architecture
   incapable of producing anything. The current behavior — demote, never
   delete a capability — is load-bearing; don't regress it without
   re-reading `ArchitectureOptimizerTests.OptimizeNeverRemovesCoreDoerRole`.
3. **Downgrade model tier under tight budget.** Any `strong`-tier role
   except `orchestrator`/`evaluator`/`risk-reviewer` drops to `balanced`.
4. **Strip checkpoints for single-session projects.** Only touches the
   `Checkpoints` flag, independent of the other three rules.

The optimizer always returns a **new** `Architecture` (via
`BuildOptimizedArchitecture`) rather than mutating its input — callers
that want to compare before/after (the CLI's `[C]ustomize` preview loop)
rely on this.

## 6. Project structure proportionality (`ProjectComponentCatalog.cs`)

```csharp
record ProjectComponent(
    string Id,
    Func<Requirements, Architecture, bool> Include,
    Func<Requirements, Architecture, string> Reason);
```

9 components, each a pure function of `(Requirements, Architecture)` to
`bool` (include or not) plus a matching human-readable `Reason` function
that explains *either* outcome (included or skipped) — not just the
positive case. `ProjectComponentCatalog.Decide()` evaluates all 9 and
returns `List<ComponentDecision>`, consumed in three places:

- `ScaffoldGenerator.Generate()` — decides which directories actually
  get created and which conditional README files get written
  (`outputs/README.md`, `specs/README.md`, `references/README.md`).
- `Program.PrintProjectStructure()` — the CLI preview, shown *before*
  generation, re-shown after `[C]ustomize`/`[T]ry another` since those
  can change the `Architecture` the rules read.
- `ScaffoldGenerator.ReadmeMd()` — the same decisions, human-readable,
  persisted into the generated project's own `README.md` so the
  explanation survives past the terminal session.

This mirrors the same "requirements-driven, not activity-driven"
principle already applied to agent selection — see §4 — applied to
folders instead of roles. If you add a 10th component, it must have
both an `Include` and a non-empty `Reason` for both outcomes; a test
(`EveryDecisionHasANonEmptyReason`) enforces the latter.

`outputs` is the only component with an unconditional `(_, _) => true`
— it's the actual point of the project (durable output), never overhead.

## 7. Runtime adapters (`Adapters/`)

Strategy pattern, not a switch statement:

```csharp
interface IRuntimeAdapter {
    string Id { get; }
    void Generate(string root, Requirements req, Architecture arch);
}
```

`RuntimeAdapterRegistry.Default` holds a fixed `IRuntimeAdapter[]` (one
instance each of `ClaudeCodeAdapter`, `OpenCodeAdapter`,
`CodexCliAdapter`). `RuntimeAdapterRegistry.Generate()` resolves
`Requirements.Runtime`:

- `"agnostic"` (default) → resolves to no adapter id → nothing extra
  generated. This is not a special case in the registry code — it's
  simply that no adapter's `Id` equals `"agnostic"`, so the lookup finds
  nothing and the loop does nothing. The agnostic-by-default behavior
  falls directly out of the data, not a conditional.
- `"all"` / `"both"` → every registered adapter's `Id`.
- Any other value → looked up directly; unknown values silently produce
  no adapters (no exception — consistent with the "don't crash on bad
  input" philosophy in §3).

Adding a new runtime (e.g. Cursor, Gemini CLI) means: write a new class
implementing `IRuntimeAdapter`, add one line to `RuntimeAdapterRegistry.Default`'s
list. No existing adapter's code changes (Open/Closed Principle,
literally, not just as a description).

`ClaudeCodeAdapter` is the most involved: it generates
`.claude/agents/<role>.md` per agent (reading the already-written
`.agent/prompts/<role>.md` body and wrapping it in Claude Code subagent
frontmatter), a `CLAUDE.md` pointer, and — if `Architecture.LoopPattern
!= "auto"` — a `.claude/skills/<pattern-id>/SKILL.md`. Subagents and
Skills are deliberately different mechanisms in the generated output:
roles map to subagents (delegation, isolated context); the work pattern
maps to a Skill (a procedure loaded into the current context). See
`ClaudeCodeAdapter`'s class-level doc comment for the reasoning.

## 8. Scaffold generation (`ScaffoldGenerator.cs`)

`Generate(root, req, arch)` is the single entry point. Order of
operations:

1. `ProjectComponentCatalog.Decide()` — compute once, reused throughout.
2. Create `root`, `.agent/prompts/`, `project/`, and only the included
   component subfolders.
3. Write `AGENTS.md`, `README.md`, `GETTING_STARTED.md` (the last one
   branches on `Requirements.ExperienceLevel` — a materially different,
   shorter, jargon-free document for `"beginner"` vs. a technical one).
4. Write `.agent/project.yaml`, `architecture.yaml`, `policies.yaml`
   (via `YamlSerializer`, a `YamlDotNet.Serialization.ISerializer` with
   underscored naming — `BudgetProfile` becomes `budget_profile` on disk).
5. Write one `.agent/prompts/<role>.md` per agent in `arch.Agents`.
6. Write the 9 `project/*.md` core files (always) plus the conditional
   component READMEs (§6).

Every `.md` file goes through `WriteMd(root, relative, type, purpose,
content)`, which prepends a YAML frontmatter block:

```markdown
---
type: goal
purpose: "Objective and Definition of Done for this project"
---

# Goal
...
```

`Write()` (the lower-level helper `WriteMd` calls) does
`content.TrimEnd() + "\n"` — every generated file ends in exactly one
newline, no trailing whitespace, regardless of how the C# raw string
literal that produced it was indented. `.yaml` files go through `Write()`
directly, skipping frontmatter (they're already structured).

`ArchitectureYaml(Architecture)` is `public` (not `private` like the
other serialization helpers) specifically because `Program.CmdOptimize`'s
`--apply` path needs to re-serialize an `Architecture` back to
`.agent/architecture.yaml` outside of a full `Generate()` call — writing
back just the one file, not regenerating the whole project.

## 9. Reading state back (`YamlLoader.cs`)

The inverse of `ProjectYaml`/`ArchitectureYaml`: `LoadRequirements(root)`
and `LoadArchitecture(root)` deserialize `.agent/project.yaml` and
`.agent/architecture.yaml` back into domain objects, using YamlDotNet's
`IDeserializer` with `IgnoreUnmatchedProperties()` (a project.yaml
hand-edited to add an unrecognized key doesn't break loading — it's
just ignored). Every field has a `?? "<fallback>"` — a missing or
malformed value degrades to a sensible default rather than throwing.
This is what makes `validate`, `architecture --recommend`, and
`optimize` resilient to a user hand-editing YAML incorrectly; `validate`
is the command responsible for catching structural problems, not the
loader.

These two functions are the reason `AgentProjectArchitect.Core` depends
on `YamlDotNet` at all (see §12 for why that's a real limitation, not a
free choice).

## 10. The `Api` seam (`Api.cs`)

```csharp
static Architecture Preview(Requirements req);
static BuildResult BuildProject(string root, Requirements req,
    Architecture? arch = null, bool optimize = false);
```

This is deliberately the *entire* public surface a non-CLI frontend is
expected to call. `Preview` has no side effects (safe to call
repeatedly for a live-updating web form). `BuildProject` accepts an
already-computed/customized `Architecture` so a web UI's "customize"
step doesn't need to re-derive it — same pattern the CLI's
`[C]ustomize`/`[T]ry another` loop already uses internally.

`BuildResult` carries back `Root`, `Requirements`, `Architecture`, and
`Adapters` (the list of runtime adapter ids that actually ran) — enough
for a caller to render a "here's what got created" summary without
re-reading the filesystem.

**What a web backend still has to build itself** (not in `Core` today):
writing to a temp directory and zipping for download instead of writing
directly to a server path, an HTTP layer, and translating the
`Architecture`/`ComponentDecision` objects to whatever the frontend's
preview UI needs — none of which requires touching `Core`.

## 11. CLI (`AgentProjectArchitect.Cli/`)

`Program.Main` is a flat dispatch: check `--version`/top-level
`--help` first, then `<command> --help` (checked generically across all
commands before dispatch, via `CliHelp.TryGetCommandHelp`), then a
`switch` on the command name calling one `Cmd*` method each. Every
`Cmd*` method returns a process exit code (`0` success, `1` failure) —
there's no exception-based control flow for expected failure modes
(`validate` failing, an unknown profile name); exceptions are reserved
for genuinely unexpected states and caught once at the top of `Main`.

`Wizard.Simple()` and `Wizard.Advanced()` are the only two entry points
by design (§ in `REFERENCES.md` on why exactly two, not three+).
`Advanced()` returns `(Requirements, List<string> AdditionalRoles)` —
the tuple exists because "add a specific role beyond what the engine
recommends" is a wizard-only customization, not a durable part of
`Requirements` (it doesn't round-trip through `.agent/project.yaml`;
it's applied once, directly to the computed `Architecture`, in
`Program.CmdNew`).

`CliHelp.cs` holding all `--help` text as data (a `Dictionary<string,
string>` plus one big top-level string) instead of scattering
`Console.WriteLine` calls with usage text across each `Cmd*` method is a
deliberate separation — `Cmd*` methods are about *running* a command,
`CliHelp` is about *describing* one. `Program.cs` and `CliHelp.cs`
never need to change in the same PR unless a command's actual behavior
and its documented behavior are both changing.

## 12. Testing strategy

`tests/AgentProjectArchitect.Tests/`, one file roughly per `Core` module:
`ArchitectureEngineTests.cs` (profiles + recommend + optimize, despite
the filename predating the module split — kept as one file since the
three concerns are tightly related and tested together),
`PatternsTests.cs`, `ProjectComponentCatalogTests.cs`,
`ScaffoldGeneratorTests.cs`, `RuntimeAdaptersTests.cs`, `ApiTests.cs`.

Patterns used throughout:

- **Round-trip tests** (`ProjectYamlRoundTripsThroughYamlLoader`) —
  generate, then load back, assert equality. Catches serialization/
  deserialization drift directly instead of testing each direction in
  isolation.
- **Invariant tests over every registered item**
  (`AllProfileRolesExistInRoleLibrary`, `AllPatternsHaveRolesThatExist`,
  `EveryDecisionHasANonEmptyReason`) — iterate the full catalog/registry
  rather than hardcoding one example, so a new profile/pattern/component
  someone adds later is checked automatically without a new test.
- **Regression tests for specific bugs found during development**
  (`OptimizeNeverRemovesCoreDoerRole` — see §5;
  `DoesNotCreateEmptyPlaceholderDirectories` — an earlier version
  created `.agent/adapters/` and `.agent/schemas/` unconditionally and
  never wrote anything into them). These exist specifically so the bug
  can't silently come back.
- **CLI-level smoke tests are manual, not automated.** The interactive
  wizard (`Console.ReadLine`-driven) has no unit test coverage — it was
  verified by hand with scripted stdin during development (see git
  history around the "agnostic base" and "additional roles" changes for
  the actual transcripts). If you're adding wizard behavior, the
  underlying logic it calls (`ArchitectureRecommender`, `Wizard`'s pure
  helpers like `Slugify`/`ChooseMulti`'s parsing) is what should be unit
  tested; the interactive loop itself is not currently covered by
  automation.

## 13. Design invariants (violate these only with a very good reason)

1. `AgentProjectArchitect.Core` contains no `Console` I/O, no network
   calls, no LLM calls — verified by inspection, not by a build-time
   check (a good candidate for a future analyzer rule, currently not
   enforced automatically).
2. `ArchitectureRecommender`/`ArchitectureOptimizer`/`ProjectComponentCatalog`
   are pure functions of their inputs — same `Requirements`/`Architecture`
   in, same result out, every time. No hidden state, no `DateTime.Now`,
   no randomness.
3. The optimizer never fully removes a `CoreDoerRoles` member — demotion
   only (§5). This is tested; don't "simplify" the rule back to deletion.
4. Every `ProjectComponent`'s `Reason` must be non-empty for *both*
   included and excluded outcomes — a component that's silently skipped
   with no explanation defeats the entire point of §6.
5. Runtime adapters never write to the agnostic core's files
   (`AGENTS.md`, `.agent/*`, `project/*`) — they only add new files
   under vendor-specific paths. An adapter that had to edit `AGENTS.md`
   to do its job would be a sign the abstraction is wrong.

## 14. Known architectural limitations

Kept short here on purpose — the full, honest list (with what each gap
actually means and roughly how it'd get fixed) lives in
[`REFERENCES.md`](REFERENCES.md)'s "Where this stands relative to
current trends" section. As of this writing: no MCP integration, no
Claude Code Skills beyond the one work-pattern skill (§7), no real
evaluation harness, no real token/cost telemetry (only a qualitative
weight-vs-budget-cap warning, §4), no import of externally-authored
agent templates, and Native AOT publishing doesn't work (§ in
`README.md`'s install section — YamlDotNet reflection breaks under
trimming).
