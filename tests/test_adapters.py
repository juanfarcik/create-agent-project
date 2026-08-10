import shutil
import tempfile
import unittest
from pathlib import Path

from agent_project import adapters, architecture as arch_mod, scaffold
from agent_project.models import Requirements


class TestAdapters(unittest.TestCase):
    def setUp(self):
        self.tmp = Path(tempfile.mkdtemp())
        self.addCleanup(shutil.rmtree, self.tmp, ignore_errors=True)

    def _build(self, runtime):
        req = Requirements(name="demo", objective="Do the thing", domain="software",
                            size="small", risk="low", runtime=runtime)
        arch = arch_mod.recommend(req)
        root = self.tmp / f"demo-{runtime}"
        scaffold.generate(root, req, arch)
        return root, req, arch

    def test_claude_code_adapter(self):
        root, req, arch = self._build("claude-code")
        generated = adapters.generate(root, req, arch)
        self.assertEqual(generated, ["claude-code"])
        self.assertTrue((root / "CLAUDE.md").exists())
        for a in arch.agents:
            f = root / ".claude" / "agents" / f"{a.role}.md"
            self.assertTrue(f.exists())
            content = f.read_text()
            self.assertTrue(content.startswith("---\nname: "))
            self.assertIn("tools:", content)

    def test_opencode_adapter(self):
        root, req, arch = self._build("opencode")
        generated = adapters.generate(root, req, arch)
        self.assertEqual(generated, ["opencode"])
        self.assertTrue((root / "opencode.json").exists())
        for a in arch.agents:
            self.assertTrue((root / ".opencode" / "agent" / f"{a.role}.md").exists())

    def test_codex_cli_adapter(self):
        root, req, arch = self._build("codex-cli")
        generated = adapters.generate(root, req, arch)
        self.assertEqual(generated, ["codex-cli"])
        self.assertTrue((root / ".codex" / "NOTES.md").exists())

    def test_all_generates_every_adapter(self):
        root, req, arch = self._build("all")
        generated = adapters.generate(root, req, arch)
        self.assertEqual(set(generated), {"claude-code", "opencode", "codex-cli"})
        self.assertTrue((root / "CLAUDE.md").exists())
        self.assertTrue((root / "opencode.json").exists())
        self.assertTrue((root / ".codex" / "NOTES.md").exists())


if __name__ == "__main__":
    unittest.main()
