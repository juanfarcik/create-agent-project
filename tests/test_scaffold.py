import shutil
import tempfile
import unittest
from pathlib import Path

from agent_project import architecture as arch_mod
from agent_project import scaffold
from agent_project.models import Requirements


class TestScaffold(unittest.TestCase):
    def setUp(self):
        self.tmp = Path(tempfile.mkdtemp())
        self.addCleanup(shutil.rmtree, self.tmp, ignore_errors=True)

    def test_generate_creates_expected_layout(self):
        req = Requirements(name="demo", objective="Do the thing", domain="general")
        arch = arch_mod.recommend(req)
        root = self.tmp / "demo"
        scaffold.generate(root, req, arch)

        for f in [
            "AGENTS.md", "README.md", "GETTING_STARTED.md",
            ".agent/project.yaml", ".agent/architecture.yaml", ".agent/policies.yaml",
            ".project/goal.md", ".project/state.md", ".project/backlog.md",
            ".project/decisions.md", ".project/learnings.md", ".project/constraints.md",
            ".project/resources.md", ".project/metrics.md",
        ]:
            self.assertTrue((root / f).exists(), f"missing {f}")

        for sub in scaffold.PROJECT_SUBDIRS:
            self.assertTrue((root / ".project" / sub).is_dir())

        for agent in arch.agents:
            self.assertTrue((root / ".agent" / "prompts" / f"{agent.role}.md").exists())

    def test_generated_objective_appears_in_goal_and_agents_md(self):
        req = Requirements(name="demo2", objective="UNIQUE_OBJECTIVE_STRING", domain="general")
        arch = arch_mod.recommend(req)
        root = self.tmp / "demo2"
        scaffold.generate(root, req, arch)

        self.assertIn("UNIQUE_OBJECTIVE_STRING", (root / ".project" / "goal.md").read_text())
        self.assertIn(req.domain, (root / "AGENTS.md").read_text())

    def test_project_yaml_round_trips_through_cli_loader(self):
        from agent_project.cli import _load_requirements, _load_architecture

        req = Requirements(
            name="demo3", objective="Ship it", domain="software",
            size="medium", risk="medium", budget_profile="balanced",
            schedule=None,
        )
        arch = arch_mod.recommend(req)
        root = self.tmp / "demo3"
        scaffold.generate(root, req, arch)

        loaded_req = _load_requirements(root)
        self.assertEqual(loaded_req.name, req.name)
        self.assertEqual(loaded_req.objective, req.objective)
        self.assertEqual(loaded_req.domain, req.domain)
        self.assertEqual(loaded_req.size, req.size)
        self.assertIsNone(loaded_req.schedule)

        loaded_arch = _load_architecture(root)
        self.assertEqual(loaded_arch.profile, arch.profile)
        self.assertEqual(sorted(loaded_arch.agent_names()), sorted(arch.agent_names()))


if __name__ == "__main__":
    unittest.main()
