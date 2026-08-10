import unittest

from agent_project import architecture as arch_mod
from agent_project.models import Requirements
from agent_project.roles import ROLES


class TestProfiles(unittest.TestCase):
    def test_all_profiles_buildable(self):
        for name in arch_mod.PROFILES:
            a = arch_mod.build_profile(name)
            self.assertTrue(a.agents, f"{name} has no agents")

    def test_all_profile_roles_exist_in_role_library(self):
        for name in arch_mod.PROFILES:
            a = arch_mod.build_profile(name)
            for agent in a.agents:
                self.assertIn(agent.role, ROLES, f"profile {name} references unknown role {agent.role}")

    def test_unknown_profile_raises(self):
        with self.assertRaises(ValueError):
            arch_mod.build_profile("does-not-exist")


class TestRecommend(unittest.TestCase):
    def base_req(self, **overrides) -> Requirements:
        req = Requirements(name="t", objective="test")
        for k, v in overrides.items():
            setattr(req, k, v)
        return req

    def test_tiny_low_risk_is_minimal(self):
        req = self.base_req(size="tiny", risk="low", domain="general")
        a = arch_mod.recommend(req)
        self.assertEqual(a.profile, "minimal")

    def test_scheduled_execution_forces_checkpoints(self):
        req = self.base_req(size="tiny", risk="low", execution_mode="scheduled")
        a = arch_mod.recommend(req)
        self.assertTrue(a.checkpoints)

    def test_high_risk_adds_risk_reviewer(self):
        req = self.base_req(size="small", risk="high")
        a = arch_mod.recommend(req)
        self.assertIn("risk-reviewer", a.agent_names())
        self.assertIn("high-risk decisions", a.human_gates)

    def test_large_high_risk_is_high_reliability(self):
        req = self.base_req(size="large", risk="critical")
        a = arch_mod.recommend(req)
        self.assertEqual(a.profile, "high-reliability")

    def test_research_domain_uses_research_profile(self):
        req = self.base_req(domain="research")
        a = arch_mod.recommend(req)
        self.assertEqual(a.profile, "research")

    def test_software_domain_uses_software_profiles(self):
        small = arch_mod.recommend(self.base_req(domain="software", size="tiny", risk="low"))
        self.assertEqual(small.profile, "software-lean")
        self.assertIn("coder", small.agent_names())

        big = arch_mod.recommend(self.base_req(domain="software", size="large", risk="critical"))
        self.assertEqual(big.profile, "software-high-reliability")
        for role in ("architect", "coder", "tester", "qa-reviewer", "code-reviewer"):
            self.assertIn(role, big.agent_names())

    def test_autonomy_shapes_human_gates_not_agent_count(self):
        req = self.base_req(autonomy="autonomous", size="tiny", risk="low")
        a = arch_mod.recommend(req)
        self.assertEqual(a.profile, "minimal")
        self.assertIn("irreversible actions", a.human_gates)


class TestOptimize(unittest.TestCase):
    def test_optimize_never_increases_agent_count(self):
        req = Requirements(name="t", objective="x", size="tiny", risk="low", budget_profile="ultra-low")
        arch = arch_mod.build_profile("high-reliability")
        optimized = arch_mod.optimize(arch, req)
        self.assertLessEqual(len(optimized.agents), len(arch.agents))

    def test_optimize_is_idempotent_on_minimal(self):
        req = Requirements(name="t", objective="x", size="tiny", risk="low", budget_profile="hobby", lifetime="session")
        arch = arch_mod.build_profile("minimal")
        arch.checkpoints = False
        optimized = arch_mod.optimize(arch, req)
        self.assertEqual(len(optimized.agents), len(arch.agents))

    def test_optimize_disables_checkpoints_for_single_session(self):
        req = Requirements(name="t", objective="x", lifetime="session")
        arch = arch_mod.build_profile("autonomous-loop")
        arch.checkpoints = True
        optimized = arch_mod.optimize(arch, req)
        self.assertFalse(optimized.checkpoints)


if __name__ == "__main__":
    unittest.main()
