# References

Every work pattern, architectural idea, and engineering practice in this
project comes from somewhere. This document traces each one to a real
source — a paper, a project, a spec, a book — so anyone can go verify it,
read the original, and understand the actual lineage instead of taking
our word for it.

Rule for this file: **no citation goes in here unless it's checkable.**
Where something is general prior art rather than a specific paper (e.g.
"human-in-the-loop" predates LLMs by decades), that's stated explicitly
rather than dressed up as a citation. Where we adapted an idea instead of
inventing it, that's stated too.

---

## Work patterns (`Patterns.cs`)

| Pattern | Where it actually comes from |
|---|---|
| `agent-in-the-loop` (think → act → observe) | **ReAct: Synergizing Reasoning and Acting in Language Models**, Yao et al., 2022. [arXiv:2210.03629](https://arxiv.org/abs/2210.03629). The think/act/observe loop is the direct descendant of ReAct's prompting framework, now the de facto default loop shape for autonomous LLM agents. |
| `human-in-the-loop` / `human-on-the-loop` | Not an LLM-era invention — these terms come from human-automation interaction research. The canonical origin is Sheridan & Verplank's **levels of automation** (MIT Man-Machine Systems Laboratory, 1978), which is where the in-the-loop / on-the-loop / out-of-the-loop distinction was formalized, decades before generative AI. Practical LLM-agent framing: [LangGraph's human-in-the-loop docs](https://langchain-ai.github.io/langgraph/concepts/human_in_the_loop/). |
| `plan-execute-review` | Practical origin: LangChain's **Plan-and-Execute agents** blog post (2023), [blog.langchain.dev](https://blog.langchain.dev/planning-agents/). Academic grounding: **Plan-and-Solve Prompting**, Wang et al., 2023. [arXiv:2305.04091](https://arxiv.org/abs/2305.04091). |
| `debate-critic` | **Improving Factuality and Reasoning in Language Models through Multiagent Debate**, Du, Li, Torralba, Tenenbaum, Mordatch, 2023. [arXiv:2305.14325](https://arxiv.org/abs/2305.14325). |
| `reflexion-self-critique` | **Reflexion: Language Agents with Verbal Reinforcement Learning**, Shinn, Cassano, Gopinath, Narasimhan, Yao, 2023. [arXiv:2303.11366](https://arxiv.org/abs/2303.11366). |
| `swarm-parallel` | Term popularized by OpenAI's experimental **Swarm** framework (2024), [github.com/openai/swarm](https://github.com/openai/swarm) (since superseded by the OpenAI Agents SDK). The underlying fan-out/fan-in parallel-task-decomposition idea is much older, standard distributed-systems prior art (e.g. MapReduce, Dean & Ghemawat, 2004) — not LLM-specific. |
| `blackboard` | **Blackboard architecture** predates LLMs by four decades. Canonical source: Erman, Hayes-Roth, Lesser, Reddy, **"The Hearsay-II Speech-Understanding System: Integrating Knowledge to Resolve Uncertainty"**, ACM Computing Surveys, 1980. We reused the coordination idea (shared state, opportunistic contribution, no fixed handoff order), not any LLM-specific technique. |
| `scheduled-digest` / `reactive-event-driven` | General software patterns (cron scheduling, event-driven architecture) applied to agents — not agent-specific research, standard prior art. |
| `interactive` | Baseline conversational turn-taking; not attributed to a specific source. |
| `auto` | Internal to this tool (a "let the rules engine decide" default) — not a pattern with external provenance. |

## Architecture profiles & role library (`ArchitectureProfileCatalog.cs`, `Roles.cs`)

- **Supervisor/worker (orchestrator + specialists) topology** — widely used across current multi-agent frameworks: [LangGraph's supervisor pattern](https://langchain-ai.github.io/langgraph/tutorials/multi_agent/agent_supervisor/), [Microsoft AutoGen](https://microsoft.github.io/autogen/), [CrewAI](https://github.com/crewAIInc/crewAI). We didn't invent the topology; we made picking the *smallest* one that fits the requirements the actual product.
- **"Minimum viable architecture" as the design philosophy** — not tied to one paper. It's a direct application of general software-engineering minimalism (YAGNI — Fowler, [martinfowler.com/bliki/Yagni.html](https://martinfowler.com/bliki/Yagni.html)) to agent topology instead of code.

## Engineering practices

- **SOLID principles** — Robert C. Martin, *Agile Software Development: Principles, Patterns, and Practices*, 2002; the SRP/OCP/DIP formulation used here follows his later essays collected at [blog.cleancoder.com](https://blog.cleancoder.com/uncle-bob/2020/10/18/Solid-Relevance.html).
- **Strategy pattern** (`IRuntimeAdapter`) — Gamma, Helm, Johnson, Vlissides ("Gang of Four"), *Design Patterns: Elements of Reusable Object-Oriented Software*, 1994.
- **Semantic Versioning** — [semver.org](https://semver.org/).
- **Keep a Changelog format** — [keepachangelog.com](https://keepachangelog.com/en/1.1.0/).
- **Contributor Covenant** (`CODE_OF_CONDUCT.md`) — [contributor-covenant.org](https://www.contributor-covenant.org/), v2.1.
- **GPLv3** — Free Software Foundation, [gnu.org/licenses/gpl-3.0](https://www.gnu.org/licenses/gpl-3.0.html).

## Runtime conventions this tool generates for

- **`AGENTS.md`** — an open, cross-vendor convention for giving coding agents project instructions, at [agents.md](https://agents.md/). Not invented by this project; we generate to the convention.
- **Claude Code `CLAUDE.md` and subagents (`.claude/agents/*.md`)** — Anthropic's documented Claude Code features. [docs.claude.com](https://docs.claude.com/en/docs/claude-code) (subagents, memory/CLAUDE.md).
- **OpenCode** — open-source AI coding agent, [opencode.ai](https://opencode.ai/).
- **Codex CLI** — OpenAI's coding agent CLI; reads `AGENTS.md` by the same open convention above.

## Ideas adapted from a specific project (with attribution)

Five concrete ideas in `AGENTS.md` generation (the "clarify before you commit"
first-session behavior, the "when you're not sure, stop and ask" confusion
protocol, conversational safety phrases, the `learnings.md` file distinct
from `decisions.md`, and citing the four failure modes below) were adapted,
**not invented here**, from studying **gstack** by Garry Tan —
[github.com/garrytan/gstack](https://github.com/garrytan/gstack) — a much
larger, commercial-grade Claude Code skill pack for engineering teams. We
took the *ideas*, rewrote them for a personal-project, non-execution-engine
scope, and did not copy gstack's code (which is also a different license).

The "four failure modes" (wrong assumptions, overcomplexity, orthogonal
edits, imperative over declarative) are cited in gstack's README as
"Karpathy's AI coding rules," attributed there to a community-compiled
repository: [github.com/forrestchang/andrej-karpathy-skills](https://github.com/forrestchang/andrej-karpathy-skills).
We're passing that attribution through, not claiming direct authorship or
a primary source from Andrej Karpathy himself — the chain of attribution
matters here and we don't want to flatten it.

---

## Where this stands relative to current trends (honest self-assessment)

Written from what's actually true of the *code*, not the ambition. See
also the open gaps already tracked elsewhere in this repo
(`README.md`'s AOT limitation, `dotnet/CHANGELOG.md`).

### Aligned

- **AGENTS.md as the portable entry point** — this is exactly where the
  ecosystem converged (OpenAI, and multiple agent CLIs including Claude
  Code and OpenCode, read/recommend it) — we didn't guess right, we
  followed the convention as it stabilized.
- **"Context engineering" over "prompt engineering"** — our Section-16-derived
  principle (agents get only the context they need, not the whole
  conversation) matches the framing Anthropic itself published in
  ["Effective context engineering for AI agents"](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents) (2025) — we arrived
  at this from first principles before reading that, and it's good
  confirmation, not the source.
- **ReAct-style agent loops and reflection/debate as named, selectable
  patterns** rather than implicit — matches how the field now talks about
  agent design (loop shape as an explicit architectural choice).

### Real gaps, not hidden

- **No MCP (Model Context Protocol) integration.** [Anthropic's MCP](https://modelcontextprotocol.io/),
  released Nov 2024, is now the standard for how agents discover and call
  tools/context sources, adopted by OpenAI and others through 2025. Our
  role prompts describe tools abstractly (`read`, `write`, `execute`,
  `web_search`) and map them to Claude Code tool *names* — we don't
  generate or reference any MCP server config. If a generated project
  needs real external tool access beyond what's built into Claude
  Code/OpenCode/Codex, this project currently has nothing to say about it.
- **No Claude Code Skills (`SKILL.md`) support.** Skills are a distinct,
  newer packaging mechanism from subagents (this very session runs
  inside a harness that uses them) — a reusable capability with its own
  manifest, separate from `.claude/agents/*.md`. We only generate
  subagents. Worth adding as a `Skill`-shaped adapter output eventually.
- **No real evaluation harness.** The `evaluator` role is a *prompt*
  telling an agent to check its own work — there's no structured rubric,
  no integration with an actual eval framework (e.g. promptfoo,
  Braintrust, LangSmith evals). "Agents need evals, not vibes" is a
  well-known 2024-2025 critique of exactly this kind of prompt-only
  self-checking, and it applies to us too.
- **No token/cost telemetry, no real cost simulation.** Flagged
  repeatedly earlier in this project's history and still true: `est_cost`
  is a qualitative LOW/MEDIUM/HIGH label, not a number derived from
  actual model pricing.
- **No import of existing agent templates/architectures** (e.g. reading
  a LangGraph or CrewAI project and normalizing it into our model) —
  originally scoped as a differentiator, never built.

If you're picking this up as a contributor: the gaps above are the
highest-value places to start, in roughly that order.
