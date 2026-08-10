using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AgentProjectArchitect.Core;

/// <summary>
/// Generates the portable project scaffold:
///   AGENTS.md, README.md, GETTING_STARTED.md
///   .agent/{project.yaml, architecture.yaml, policies.yaml, prompts/&lt;role&gt;.md}
///   .project/{goal,context,state,backlog,decisions,learnings,constraints,resources,metrics}.md
///   .project/{outputs,research,plans,experiments,reviews,checkpoints,telemetry}/
/// Runtime adapters (.claude/, .opencode/, .codex/) are generated separately.
/// </summary>
public static class ScaffoldGenerator
{
    /// <summary>Subdirectories created under <c>.project/</c> for durable, categorized output.</summary>
    public static readonly string[] ProjectSubdirs =
        {
            "specs", "references", "outputs", "research", "plans",
            "experiments", "reviews", "checkpoints", "telemetry",
        };

    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .DisableAliases()
        .Build();

    /// <summary>Writes the full portable scaffold (AGENTS.md, .agent/, .project/) under <paramref name="root"/>.</summary>
    public static void Generate(string root, Requirements req, Architecture arch)
    {
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, ".agent", "prompts"));
        Directory.CreateDirectory(Path.Combine(root, ".project"));
        foreach (var sub in ProjectSubdirs)
            Directory.CreateDirectory(Path.Combine(root, ".project", sub));

        WriteMd(root, "AGENTS.md", "agent-instructions",
            "Entry point every agent runtime reads first", AgentsMd(req, arch));
        WriteMd(root, "README.md", "readme",
            "Human-facing project overview", ReadmeMd(req, arch));
        WriteMd(root, "GETTING_STARTED.md", "getting-started",
            "Step-by-step onboarding for the human", GettingStartedMd(req, arch));

        Write(root, Path.Combine(".agent", "project.yaml"), ProjectYaml(req));
        Write(root, Path.Combine(".agent", "architecture.yaml"), ArchitectureYaml(arch));
        Write(root, Path.Combine(".agent", "policies.yaml"), PoliciesYaml(req, arch));

        foreach (var agent in arch.Agents)
        {
            if (Roles.All.TryGetValue(agent.Role, out var role))
            {
                WriteMd(root, Path.Combine(".agent", "prompts", $"{agent.Role}.md"), "role-prompt",
                    $"Instructions for the {agent.Role} role", RolePrompt(agent.Role, role));
            }
        }

        WriteMd(root, Path.Combine(".project", "goal.md"), "goal",
            "Objective and Definition of Done for this project", GoalMd(req));
        WriteMd(root, Path.Combine(".project", "context.md"), "context",
            "Durable background facts agents should remember", ContextMd(req));
        WriteMd(root, Path.Combine(".project", "state.md"), "state",
            "Current project status — reality now, not history", StateMd());
        WriteMd(root, Path.Combine(".project", "backlog.md"), "backlog",
            "Actionable next work", BacklogMd());
        WriteMd(root, Path.Combine(".project", "decisions.md"), "decisions",
            "Log of deliberate, non-trivial choices", DecisionsMd());
        WriteMd(root, Path.Combine(".project", "learnings.md"), "learnings",
            "Patterns, pitfalls, and preferences discovered while working", LearningsMd());
        WriteMd(root, Path.Combine(".project", "constraints.md"), "constraints",
            "Human approval gates and limits for this project", ConstraintsMd(req, arch));
        WriteMd(root, Path.Combine(".project", "resources.md"), "resources",
            "External resources in use by this project", ResourcesMd());
        WriteMd(root, Path.Combine(".project", "metrics.md"), "metrics",
            "Iteration, budget, and task counters", MetricsMd(req));
        WriteMd(root, Path.Combine(".project", "outputs", "README.md"), "outputs-guide",
            "What kind of durable output belongs in this folder", OutputsReadme(req));
        WriteMd(root, Path.Combine(".project", "specs", "README.md"), "specs-guide",
            "How to write a spec for a feature or deliverable", SpecsReadme());
        WriteMd(root, Path.Combine(".project", "references", "README.md"), "references-guide",
            "Where the human's own source material lives", ReferencesReadme());
    }

    private static void Write(string root, string relative, string content)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content.TrimEnd() + "\n");
    }

    /// <summary>
    /// Writes a markdown file with a small YAML frontmatter block (type +
    /// one-line purpose) prepended, so a file can be classified by a tool
    /// or an LLM without reading its full body — the same technique
    /// static-site generators and note-taking tools use for machine-
    /// readable structure on top of human-readable markdown.
    /// </summary>
    private static void WriteMd(string root, string relative, string type, string purpose, string content)
    {
        var frontmatter = $"""
            ---
            type: {type}
            purpose: "{purpose.Replace("\"", "\\\"")}"
            ---
            """;
        Write(root, relative, frontmatter + "\n\n" + content.TrimStart());
    }

    // -------------------------------------------------------------
    // AGENTS.md
    // -------------------------------------------------------------

    private static string AgentsMd(Requirements req, Architecture arch)
    {
        var agentLines = string.Join("\n", arch.Agents.Select(a =>
            $"- `{a.Role}` ({a.Mode}, {a.ModelTier}) — {(Roles.All.TryGetValue(a.Role, out var r) ? r.Description : "")}"));
        var gates = arch.HumanGates.Count > 0
            ? string.Join("\n", arch.HumanGates.Select(g => $"- {g}"))
            : "- none configured";
        var pattern = Patterns.Get(arch.LoopPattern);

        return $$"""
# {{req.Name}} — Agent Instructions

This is a **{{req.Domain}}** project. Architecture: **{{arch.Profile}}**.
Work pattern: **{{pattern.Label}}**.

Do not assume this is a software project unless the domain says so.

## Start here, every session

1. Read `.project/goal.md` — objective and Definition of Done.
2. Read `.project/state.md` — current reality (not history).
3. Read `.project/backlog.md` — actionable next work.
4. Read `.project/constraints.md` before acting.
5. Read `.project/learnings.md` — patterns, pitfalls, and preferences picked
   up in earlier sessions. Add to it when you learn something that will
   matter next time; don't let it become a chronological dump.

## First session: clarify before you commit

If `.project/goal.md` reads thin, vague, or like a surface-level feature
request, don't start building yet. Ask 2-3 sharp questions that surface
what the person is *actually* trying to accomplish — the real pain, not
just the literal words. It's common for what someone describes ("a daily
report") to be a narrower slice of something bigger they haven't
articulated yet. Reflect that back, let them correct you, then update
`.project/goal.md` with what you actually agreed on before writing
anything else. This only needs to happen once — don't re-interrogate an
established goal on every session.

## When you're not sure

Do not guess on a decision that's expensive to reverse (architecture,
scope, what "done" means, deleting/overwriting something). Stop and ask.
Guessing wrong costs more than one extra question ever does. This does
not apply to small, reversible, in-scope choices — decide those yourself
and note why in `.project/decisions.md` if it's non-obvious.

## Stay disciplined

Four failure modes to actively avoid, in order of how often they derail
a project:

1. **Wrong assumptions** — verify instead of guessing (see above).
2. **Overcomplexity** — the smallest thing that satisfies the Definition
   of Done beats a more "proper" version nobody asked for.
3. **Orthogonal edits** — touch what the task requires, not what you
   noticed in passing. Note unrelated issues in the backlog instead.
4. **Imperative over declarative** — prefer stating the desired end state
   and letting the approach follow, over a rigid step list that breaks
   the moment reality differs slightly.

(These four are widely circulated as "Karpathy's AI coding rules" — see
this project's `docs/REFERENCES.md` for the actual attribution chain,
which is more nuanced than that name implies.)

## Loop — {{pattern.Label}}

{{pattern.Description}}

```
{{pattern.LoopDiagram}}
```

Do not stop merely because one task finished. Continue until the
Definition of Done is met, a blocker needs a human, or budget/iteration
limits (`.project/metrics.md`) are reached.

## Agents available

{{agentLines}}

Role prompts live in `.agent/prompts/<role>.md`. Delegate only when a role
adds real value — a single agent handling everything is often correct.

## Human approval required for

{{gates}}

Record every non-trivial decision in `.project/decisions.md`, anything
worth remembering for next time in `.project/learnings.md`, and every
durable output under `.project/outputs/` (conversation is not the output).

## Growing the structure: nested AGENTS.md

If `.project/outputs/` grows real substructure (chapters, modules,
tracks, whatever the project's unit of work is), and a subfolder
accumulates its own context that doesn't belong in the project-wide
files above, drop a small `AGENTS.md` inside that subfolder explaining
just that subset. Claude Code and similar runtimes read `AGENTS.md`
hierarchically — the closer file adds to, not replaces, this one. Use
this when a subfolder's context would otherwise bloat this file or
`.project/context.md`; don't create one preemptively for every folder.

## Safety phrases

The person you're working with doesn't need to edit YAML to change how
careful you're being. If they say something like:

- **"be careful"** — slow down, double-check before anything destructive
  or hard to undo, prefer read-only steps first.
- **"don't touch anything outside [folder/file]"** — treat everything
  else as read-only until they say otherwise.
- **"stop"** / **"wait"** — stop mid-action and report state; don't
  continue the loop until they respond.

Honor these immediately, without asking for confirmation first.

## Architecture details

See `.agent/architecture.yaml` for the full agent/topology config and
`.agent/policies.yaml` for budget and permission policy. Run
`agent-project validate` after manual edits.

## Where this structure comes from

This project was scaffolded by Agentic Project Architect
(GPLv3, https://github.com/juanfarcik/agent-project-architect). Nothing
above was invented from scratch — the work pattern, role list, and
"stay disciplined" guidance all trace to real sources (papers, other
open-source projects, established prior art), documented in that repo's
`docs/REFERENCES.md`.
""";
    }

    private static string ReadmeMd(Requirements req, Architecture arch) => $$"""
# {{req.Name}}

{{req.Objective}}

Generated by Agentic Project Architect. Architecture: **{{arch.Profile}}**
({{arch.Agents.Count}} agent(s), estimated cost: {{arch.EstCost}}).

## Layout

- `AGENTS.md` — entry point for any agent runtime (Claude Code, OpenCode, ...)
- `.agent/` — architecture, policies, prompts (the "how")
- `.project/` — goal, state, backlog, decisions, outputs (the "what")

## Working on this project

Open this directory with your agent CLI and let it read `AGENTS.md` first.
""";

    private static string GettingStartedMd(Requirements req, Architecture arch)
    {
        var runtimeLabel = req.Runtime switch
        {
            "claude-code" => "Claude Code",
            "opencode" => "OpenCode",
            "codex-cli" => "Codex CLI",
            "all" => "your agent CLI of choice",
            "agnostic" => "whichever agentic CLI you have — Claude Code, OpenCode, Codex CLI, or any other that reads AGENTS.md",
            _ => req.Runtime,
        };

        if (req.ExperienceLevel == "beginner")
        {
            return $$"""
# Getting Started with {{req.Name}}

You don't need to know anything about "agents" or "prompts" to use this.
The project already has everything set up — you just need to open it and
start talking to it.

## Steps

1. Open this folder in your editor (e.g. VS Code).
2. Open a terminal in this folder and start {{runtimeLabel}} there
   (e.g. run `claude` for Claude Code, or the equivalent for your tool).
3. Your assistant will automatically read `AGENTS.md` first — that file
   tells it what this project is and what to do. You don't need to paste
   anything.
4. Just type what you want in plain language, for example:
   - "Get started" / "What's the current state of the project?"
   - "Do the next most useful thing"
   - "Show me what's been done so far"
5. The assistant will keep track of progress for you in the `.project/`
   folder. You can check `.project/state.md` anytime to see where things
   stand, or `.project/outputs/` to see what's been produced.
6. It will ask you before doing anything risky or irreversible — that's
   expected, just answer yes/no.
7. If it starts going somewhere you don't want, you can say **"stop"**,
   **"be careful"**, or **"don't touch anything outside [some folder]"**
   at any time — it's instructed to listen to those immediately.

That's it. If you ever feel lost, just say "explain the current state of
this project" and it will summarize it for you.
""";
        }

        var agentLines = string.Join("\n", arch.Agents.Select(a => $"  - {a.Role} ({a.Mode}, {a.ModelTier})"));
        return $$"""
# Getting Started with {{req.Name}}

Architecture: **{{arch.Profile}}** — {{arch.Agents.Count}} role(s):

{{agentLines}}

## Steps

1. `cd` into this directory.
2. Open it with {{runtimeLabel}} (or any AGENTS.md-compatible runtime —
   Claude Code, OpenCode, and Codex CLI all read `AGENTS.md`/`CLAUDE.md`
   at the project root by convention; nothing else to wire up).
3. The runtime reads `AGENTS.md` on session start. It in turn points to:
   - `.project/goal.md` / `state.md` / `backlog.md` / `constraints.md` for
     current project reality
   - `.agent/architecture.yaml` + `.agent/policies.yaml` for the agent
     topology and budget/permission policy
   - `.agent/prompts/<role>.md` for each configured role's instructions
4. Drive it directly ("implement the next backlog item", "run the
   evaluator on this artifact") or let the orchestrator decide the next
   action on its own if `execution_mode` allows it.
5. Inspect/adjust the architecture with the `agent-project` CLI
   (`status`, `architecture --recommend`, `optimize --apply`, `validate`).
6. Edit `.agent/architecture.yaml` or `.agent/policies.yaml` directly for
   fine-grained control, then re-run `validate`.
""";
    }

    // -------------------------------------------------------------
    // .agent/*.yaml
    // -------------------------------------------------------------

    private static string ProjectYaml(Requirements req)
    {
        var data = new Dictionary<string, object?>
        {
            ["project"] = new Dictionary<string, object?>
            {
                ["name"] = req.Name,
                ["domain"] = req.Domain,
                ["objective"] = req.Objective,
                ["definition_of_done"] = req.DefinitionOfDone,
            },
            ["requirements"] = new Dictionary<string, object?>
            {
                ["size"] = req.Size,
                ["lifetime"] = req.Lifetime,
                ["autonomy"] = req.Autonomy,
                ["risk"] = req.Risk,
                ["budget_profile"] = req.BudgetProfile,
                ["execution_mode"] = req.ExecutionMode,
                ["human_involvement"] = req.HumanInvolvement,
                ["schedule"] = req.Schedule ?? "",
                ["loop_pattern"] = req.LoopPattern,
            },
            ["runtime"] = req.Runtime,
            ["experience_level"] = req.ExperienceLevel,
        };
        return YamlSerializer.Serialize(data);
    }

    /// <summary>Serializes an <see cref="Architecture"/> to the same YAML shape <see cref="Generate"/> writes — used by the CLI's optimize --apply to write back a modified architecture.</summary>
    public static string ArchitectureYaml(Architecture arch)
    {
        var data = new Dictionary<string, object?>
        {
            ["architecture"] = new Dictionary<string, object?>
            {
                ["profile"] = arch.Profile,
                ["loop_pattern"] = arch.LoopPattern,
                ["memory"] = arch.Memory,
                ["checkpoints"] = arch.Checkpoints,
                ["complexity"] = arch.Complexity,
                ["estimated"] = new Dictionary<string, object?>
                {
                    ["calls_per_run"] = arch.EstCallsPerRun,
                    ["context"] = arch.EstContext,
                    ["cost"] = arch.EstCost,
                },
                ["agents"] = arch.Agents.Select(a => new Dictionary<string, object?>
                {
                    ["role"] = a.Role,
                    ["mode"] = a.Mode,
                    ["model_tier"] = a.ModelTier,
                }).ToList(),
                ["human_gates"] = arch.HumanGates,
                ["notes"] = arch.Notes,
            },
        };
        return YamlSerializer.Serialize(data);
    }

    private static string PoliciesYaml(Requirements req, Architecture arch)
    {
        var data = new Dictionary<string, object?>
        {
            ["budget"] = new Dictionary<string, object?>
            {
                ["profile"] = req.BudgetProfile,
                ["warn_at_pct"] = 70,
                ["throttle_at_pct"] = 85,
                ["stop_at_pct"] = 100,
            },
            ["permissions"] = new Dictionary<string, object?>
            {
                ["spend_money"] = false,
                ["contact_external_people"] = false,
                ["publish_externally"] = false,
                ["perform_irreversible_actions"] = false,
            },
            ["failure_policy"] = new Dictionary<string, object?>
            {
                ["on_failure_1"] = "retry",
                ["on_failure_2"] = "try alternative strategy",
                ["on_failure_3"] = "reassess",
                ["on_repeated_failure"] = "escalate to human",
            },
            ["human_gates"] = arch.HumanGates,
        };
        return YamlSerializer.Serialize(data);
    }

    private static string RolePrompt(string roleId, Role role)
    {
        var resp = string.Join("\n", role.Responsibilities.Select(r => $"- {r}"));
        var reqCtx = role.RequiredContext.Count > 0
            ? string.Join("\n", role.RequiredContext.Select(c => $"- {c}")) : "- (task-scoped)";
        var excCtx = role.ExcludedContext.Count > 0
            ? string.Join("\n", role.ExcludedContext.Select(c => $"- {c}")) : "- (none)";
        var esc = string.Join("\n", role.EscalateWhen.Select(e => $"- {e}"));
        var tools = string.Join(", ", role.Tools);

        return $$"""
# Role: {{roleId}}

{{role.Description}}

## Responsibilities

{{resp}}

## Required context

{{reqCtx}}

## Do NOT pull in

{{excCtx}}

## Allowed tools

{{tools}}

## Escalate to the orchestrator/human when

{{esc}}

Report back with: what was done, what changed, what remains, confidence,
and any assumptions made.
""";
    }

    // -------------------------------------------------------------
    // .project/*.md
    // -------------------------------------------------------------

    private static string GoalMd(Requirements req) => $"""
# Goal

## Objective

{req.Objective}

## Definition of Done

{(string.IsNullOrWhiteSpace(req.DefinitionOfDone) ? "(define this before starting non-trivial work)" : req.DefinitionOfDone)}

## Domain

{req.Domain}
""";

    private static string ContextMd(Requirements req) => $"""
# Project Context

{(string.IsNullOrWhiteSpace(req.Context) ? "(no initial context provided)" : req.Context)}

This file holds durable facts agents should remember. Do not use it for
temporary task state — that belongs in `state.md`.
""";

    private static string StateMd() => """
# Current State

Status: NOT_STARTED
Current objective: Understand the project and establish the initial state.
Current task: Initial analysis.
Blockers: none
Known risks: none
Last decision: none
Next action: Read goal.md and constraints.md, then propose the first task.

(This file reflects current reality only. History belongs in telemetry.)
""";

    private static string BacklogMd() => """
# Backlog

## P0

- [ ] Understand project objective and Definition of Done
- [ ] Identify unknowns
- [ ] Define first actionable milestone

## P1

## P2

## Completed
""";

    private static string DecisionsMd() => """
# Decisions

No decisions recorded yet. Each entry should include: decision, date,
context, alternatives considered, rationale, consequences, reversibility.

Decisions are things deliberately chosen. Facts discovered along the way
(what worked, what didn't, preferences) belong in `learnings.md` instead.
""";

    private static string LearningsMd() => """
# Learnings

Patterns, pitfalls, and preferences picked up while working on this
project — distinct from `decisions.md` (deliberate choices) and
`state.md` (current status). Keep entries short and specific enough to
be useful next session; prune ones that stop being true.

## Patterns that worked

## Pitfalls to avoid

## Preferences noticed
""";

    private static string ConstraintsMd(Requirements req, Architecture arch)
    {
        var gates = arch.HumanGates.Count > 0
            ? string.Join("\n", arch.HumanGates.Select(g => $"- {g}"))
            : "- (none — low-risk, small-scale project)";
        return $"""
# Constraints

{(string.IsNullOrWhiteSpace(req.Constraints) ? "(no explicit constraints provided)" : req.Constraints)}

## Human approval required for

{gates}

This list scales with the project (see `.agent/architecture.yaml`) — a
small personal project only gets the gates that actually apply to it.
""";
    }

    private static string ResourcesMd() => """
# Resources

Record important external resources here as they become part of the
project. Prefer existing resources before requesting new ones.
""";

    private static string MetricsMd(Requirements req) => $"""
# Metrics

Iterations: 0
Budget profile: {req.BudgetProfile}
Estimated cost so far: 0
Tasks completed: 0
Tasks failed: 0
Reviews passed: 0
Reviews failed: 0
Project status: NOT_STARTED
""";

    private static readonly Dictionary<string, string> OutputsGuidanceByDomain = new()
    {
        ["software"] = "Design docs, architecture decision records, and release notes. " +
            "Source code itself lives in the project's normal source layout (e.g. `src/`) — " +
            "this scaffold doesn't dictate where; only durable *decisions and docs about* the " +
            "code belong here, not the code.",
        ["research"] = "Findings and synthesized reports — one file per finding or report, " +
            "dated. Raw notes-in-progress can live here too, but promote anything conclusive " +
            "out of draft form before calling it done.",
        ["creative"] = "Finished or feedback-ready creative work — chapters, tracks, mockups, " +
            "scenes, whatever the medium's actual unit of work is. Work-in-progress drafts are " +
            "fine here too; this isn't a \"final only\" folder, it's the durable-artifact folder " +
            "as opposed to conversation.",
        ["business"] = "Strategy documents, market analysis, financial estimates — anything " +
            "meant to inform a real decision, not exploratory scratch work.",
        ["ops"] = "Digests, run reports, and logs of completed automated actions — one file " +
            "per run or per period, not one giant append-only log.",
        ["general"] = "Whatever durable output this project produces. If it doesn't fit " +
            "cleanly in one file per unit of work, that's a sign to reconsider the structure, " +
            "not to make one file bigger and bigger.",
    };

    private static string OutputsReadme(Requirements req)
    {
        var guidance = OutputsGuidanceByDomain.GetValueOrDefault(req.Domain, OutputsGuidanceByDomain["general"]);
        return $"""
# Outputs

{guidance}

This folder is the actual point of the project — everything else in
`.project/` and `.agent/` exists to help produce what goes here.
Conversation with the agent is not the output; what's saved in this
folder is.

See `.project/goal.md` for what "done" means for this project's outputs,
and the "Growing the structure" section in the root `AGENTS.md` for when
a subfolder here should get its own `AGENTS.md`.
""";
    }

    private static string SpecsReadme() => """
# Specs

`goal.md` stays high-level and stable — the project's overall objective
and Definition of Done. As soon as the project has more than one
distinct feature or deliverable, each one gets its own spec file here
instead of piling more detail into `goal.md`.

One file per feature/deliverable, named for what it covers (e.g.
`login-flow.md`, `chapter-3.md`, `pricing-page.md`). Each spec should
cover, briefly:

- **What** — the concrete thing being built/written/produced
- **Why** — the real need behind it (see "clarify before you commit" in
  the root `AGENTS.md` — don't skip straight to *what* without this)
- **Acceptance criteria** — how to know this specific piece is done,
  distinct from the project's overall Definition of Done
- **Status** — draft / ready / in progress / done

Keep specs small and disposable — a spec for a feature that's done is
historical record, not something to keep editing. This mirrors how
spec-driven agentic workflows (e.g. GitHub's spec-kit, Kiro) separate
"what to build" from "how" (`plans/`) and "the work itself"
(`outputs/`) — see this project's `docs/REFERENCES.md`.

Don't create a spec for trivial one-off tasks — that's what
`backlog.md` is for. Specs are for anything substantial enough that
"what does done look like" needs to be written down before starting.
""";

    private static string ReferencesReadme() => """
# References

This is where the human's own source material lives — things the agent
should treat as ground truth or inspiration, not generate itself:
style guides, brand guidelines, source documents, prior work, links,
research the human already did before this project started.

Drop files here directly (text, markdown, whatever). If something here
gets used as the basis for a decision or a spec, note that connection
in `decisions.md` or the relevant file under `specs/` — a reference
sitting unused in this folder isn't doing anything; citing it where
it's actually applied is what makes it useful context instead of noise.

This is different from `.project/context.md` (durable facts the agent
itself has learned/confirmed) and `.project/resources.md` (external
tools/services the project depends on) — this folder is source material
the human brought in from outside.
""";
}
