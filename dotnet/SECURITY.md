# Security Policy

## Reporting a Vulnerability

This tool generates local files and runs no network services and no
remote code execution paths by design (Section 70: the generator is
deterministic, dependency-minimal, and calls no LLM/network APIs in its
core recommendation/generation path). The realistic attack surface is:

- Malformed/malicious `project.yaml` or `architecture.yaml` fed to
  `create-agent-project validate` / `optimize` / `architecture` on an existing
  directory (YAML deserialization).
- A generated project's files being trusted uncritically by an agentic
  CLI without human review.

If you find a security issue (e.g. a way for a crafted `.agent/*.yaml`
file to cause unintended file writes outside the target project
directory, path traversal in generated file names, or a YamlDotNet
deserialization issue reachable through this tool):

**Please do not open a public issue.** Instead, use GitHub's private
vulnerability reporting for this repository (Security tab → "Report a
vulnerability"), or contact the maintainer directly if that's not
available.

Include:

- A description of the issue and its impact
- Steps to reproduce (a minimal `project.yaml`/`architecture.yaml` or
  CLI invocation is ideal)
- The version/commit you tested against

We'll acknowledge reports within a reasonable timeframe and credit
reporters in the fix's changelog entry unless you prefer otherwise.

## Supported Versions

This project is pre-1.0; only the `main` branch is supported. There is no
long-term support branch yet.
