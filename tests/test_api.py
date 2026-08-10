import shutil
import tempfile
import unittest
from pathlib import Path

from agent_project import api
from agent_project.models import Requirements


class TestApi(unittest.TestCase):
    """These exercise the exact seam a future non-CLI frontend (e.g. a web
    UI) would call: build a Requirements object with no terminal I/O
    involved, get a project on disk back."""

    def setUp(self):
        self.tmp = Path(tempfile.mkdtemp())
        self.addCleanup(shutil.rmtree, self.tmp, ignore_errors=True)

    def test_preview_has_no_side_effects(self):
        req = Requirements(name="demo", objective="Do a thing")
        arch = api.preview(req)
        self.assertFalse((self.tmp / "demo").exists())
        self.assertTrue(arch.agents)

    def test_build_project_from_requirements_only(self):
        req = Requirements(name="demo", objective="Do a thing", domain="creative")
        result = api.build_project(self.tmp / "demo", req)
        self.assertTrue((result.root / "AGENTS.md").exists())
        self.assertEqual(result.requirements.name, "demo")
        self.assertIn("claude-code", result.adapters)

    def test_build_project_accepts_precomputed_architecture(self):
        req = Requirements(name="demo", objective="Do a thing")
        arch = api.preview(req)
        arch.notes.append("UI-customized before generation")
        result = api.build_project(self.tmp / "demo", req, arch)
        self.assertIn("UI-customized before generation",
                       (self.tmp / "demo" / ".agent" / "architecture.yaml").read_text())

    def test_build_project_can_optimize_inline(self):
        req = Requirements(name="demo", objective="Do a thing", size="large",
                            risk="critical", budget_profile="ultra-low")
        result = api.build_project(self.tmp / "demo", req, optimize=True)
        self.assertTrue(any("Optimizer" in n for n in result.architecture.notes))


if __name__ == "__main__":
    unittest.main()
