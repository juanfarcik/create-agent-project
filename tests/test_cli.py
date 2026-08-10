import contextlib
import io
import shutil
import tempfile
import unittest
from argparse import Namespace
from pathlib import Path

from agent_project import adapters, architecture as arch_mod, cli, scaffold
from agent_project.models import Requirements
from agent_project.yamlutil import load


class TestCli(unittest.TestCase):
    def setUp(self):
        self.tmp = Path(tempfile.mkdtemp())
        self.addCleanup(shutil.rmtree, self.tmp, ignore_errors=True)

        self.req = Requirements(
            name="demo", objective="Ship the thing", domain="software",
            definition_of_done="It ships", size="large", risk="critical",
            budget_profile="ultra-low",
        )
        self.arch = arch_mod.recommend(self.req)
        self.root = self.tmp / "demo"
        scaffold.generate(self.root, self.req, self.arch)
        adapters.generate(self.root, self.req, self.arch)

    def _capture(self, fn, *args, **kwargs):
        buf = io.StringIO()
        with contextlib.redirect_stdout(buf):
            fn(*args, **kwargs)
        return buf.getvalue()

    def test_validate_passes_on_freshly_generated_project(self):
        out = self._capture(cli.cmd_validate, Namespace(path=str(self.root)))
        self.assertIn("VALID", out)

    def test_validate_fails_on_missing_files(self):
        shutil.rmtree(self.root / ".project")
        with self.assertRaises(SystemExit):
            cli.cmd_validate(Namespace(path=str(self.root)))

    def test_status_prints_state_and_metrics(self):
        out = self._capture(cli.cmd_status, Namespace(path=str(self.root)))
        self.assertIn("NOT_STARTED", out)
        self.assertIn("Budget profile", out)

    def test_architecture_recommend_matches_generated(self):
        out = self._capture(cli.cmd_architecture, Namespace(path=str(self.root), recommend=True))
        self.assertIn("Current Architecture", out)
        self.assertIn("Recommended", out)
        self.assertIn(self.arch.profile.upper(), out)

    def test_optimize_apply_writes_back_valid_yaml(self):
        cli.cmd_optimize(Namespace(path=str(self.root), apply=True))
        data = load((self.root / ".agent" / "architecture.yaml").read_text())
        self.assertIn("architecture", data)
        # project must still validate after optimization (roles/prompts stay consistent
        # only if optimize never introduces a role without a prompt file)
        optimized_roles = {a["role"] for a in data["architecture"]["agents"]}
        for role in optimized_roles:
            self.assertTrue((self.root / ".agent" / "prompts" / f"{role}.md").exists())

    def test_compare_and_templates_run_without_error(self):
        out1 = self._capture(cli.cmd_compare, Namespace())
        self.assertIn("Architecture", out1)
        out2 = self._capture(cli.cmd_templates, Namespace())
        self.assertIn("software-lean", out2)


if __name__ == "__main__":
    unittest.main()
