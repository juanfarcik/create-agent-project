import unittest

from agent_project import architecture as arch_mod, patterns as patterns_mod
from agent_project.models import Requirements
from agent_project.roles import ROLES


class TestPatternsRegistry(unittest.TestCase):
    def test_all_patterns_have_roles_that_exist(self):
        for p in patterns_mod.PATTERNS.values():
            for role, _mode in p.force_roles:
                self.assertIn(role, ROLES, f"pattern {p.id} forces unknown role {role}")

    def test_all_min_profiles_are_valid(self):
        for p in patterns_mod.PATTERNS.values():
            if p.min_profile:
                self.assertIn(p.min_profile, arch_mod.PROFILES)

    def test_choices_cover_every_pattern(self):
        self.assertEqual(set(pid for pid, _ in patterns_mod.choices()), set(patterns_mod.PATTERNS.keys()))

    def test_unknown_pattern_id_falls_back_to_auto(self):
        p = patterns_mod.get("does-not-exist")
        self.assertEqual(p.id, "auto")


class TestPatternIntegration(unittest.TestCase):
    def base_req(self, **overrides) -> Requirements:
        req = Requirements(name="t", objective="test")
        for k, v in overrides.items():
            setattr(req, k, v)
        return req

    def test_plan_execute_review_forces_planner_and_evaluator(self):
        req = self.base_req(size="tiny", risk="low", loop_pattern="plan-execute-review")
        a = arch_mod.recommend(req)
        self.assertIn("planner", a.agent_names())
        self.assertIn("evaluator", a.agent_names())

    def test_debate_critic_forces_always_on_critic(self):
        req = self.base_req(size="tiny", risk="low", loop_pattern="debate-critic")
        a = arch_mod.recommend(req)
        critic = next(x for x in a.agents if x.role == "critic")
        self.assertEqual(critic.mode, "always")

    def test_swarm_parallel_enforces_minimum_profile_size(self):
        req = self.base_req(size="tiny", risk="low", loop_pattern="swarm-parallel")
        a = arch_mod.recommend(req)
        collaborative_size = len(arch_mod.build_profile("collaborative").agents)
        self.assertGreaterEqual(len(a.agents), collaborative_size)

    def test_auto_pattern_does_not_alter_architecture(self):
        req_auto = self.base_req(size="small", risk="low", loop_pattern="auto")
        req_none = self.base_req(size="small", risk="low")
        self.assertEqual(
            arch_mod.recommend(req_auto).agent_names(),
            arch_mod.recommend(req_none).agent_names(),
        )

    def test_loop_pattern_recorded_on_architecture(self):
        req = self.base_req(loop_pattern="human-in-the-loop")
        a = arch_mod.recommend(req)
        self.assertEqual(a.loop_pattern, "human-in-the-loop")


if __name__ == "__main__":
    unittest.main()
