"""Programmatic entry point — no terminal I/O, no interactive prompts.

This is the seam a future UI (web form, GUI, anything else) is meant to
call instead of going through the CLI's wizard. `cli.py`'s `new` command
is just one caller of `build_project`; a web backend would be another,
constructing `Requirements` from form data instead of `input()`.

Keeping this free of print()/input() is deliberate — every function here
must work identically whether it's called from a terminal, an HTTP
handler, or a test.
"""

from __future__ import annotations

from pathlib import Path
from typing import List, Optional

from . import adapters, architecture as arch_mod, scaffold
from .models import Architecture, Requirements


def preview(req: Requirements) -> Architecture:
    """Requirements -> recommended Architecture. No side effects."""
    return arch_mod.recommend(req)


def build_project(
    root: Path,
    req: Requirements,
    arch: Optional[Architecture] = None,
    *,
    optimize: bool = False,
) -> "BuildResult":
    """Generate a complete project on disk and return what was produced.

    If `arch` is omitted, it's derived from `req` via `preview()`. Pass an
    already-customized `Architecture` (e.g. after optimize/try-another in
    a UI) to generate exactly that instead of recomputing it.
    """
    if arch is None:
        arch = preview(req)
    if optimize:
        arch = arch_mod.optimize(arch, req)

    scaffold.generate(root, req, arch)
    generated_adapters = adapters.generate(root, req, arch)

    return BuildResult(root=root, requirements=req, architecture=arch, adapters=generated_adapters)


class BuildResult:
    def __init__(self, root: Path, requirements: Requirements, architecture: Architecture, adapters: List[str]):
        self.root = root
        self.requirements = requirements
        self.architecture = architecture
        self.adapters = adapters
