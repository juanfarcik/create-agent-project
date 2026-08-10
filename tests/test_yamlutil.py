import unittest

from agent_project.yamlutil import dump, load


class TestYamlRoundTrip(unittest.TestCase):
    def test_scalars(self):
        data = {"name": "hello", "count": 3, "ratio": 1.5, "flag": True, "empty": None}
        self.assertEqual(load(dump(data)), data)

    def test_nested_dict(self):
        data = {"a": {"b": {"c": "deep"}}}
        self.assertEqual(load(dump(data)), data)

    def test_list_of_scalars(self):
        data = {"items": ["one", "two", "three"]}
        self.assertEqual(load(dump(data)), data)

    def test_empty_list(self):
        data = {"items": []}
        self.assertEqual(load(dump(data)), data)

    def test_list_of_dicts(self):
        data = {"agents": [
            {"role": "orchestrator", "mode": "always", "model_tier": "balanced"},
            {"role": "coder", "mode": "on-demand", "model_tier": "cheap"},
        ]}
        self.assertEqual(load(dump(data)), data)

    def test_values_with_colons_and_leading_space(self):
        data = {"notes": ["Optimizer changes:", "  - removed always-on 'critic'"]}
        self.assertEqual(load(dump(data)), data)

    def test_empty_string_value(self):
        data = {"schedule": ""}
        self.assertEqual(load(dump(data)), data)

    def test_realistic_architecture_yaml(self):
        data = {
            "architecture": {
                "profile": "software-standard",
                "memory": "filesystem",
                "checkpoints": True,
                "complexity": "MEDIUM",
                "estimated": {"calls_per_run": "10-25", "context": "MEDIUM", "cost": "MEDIUM"},
                "agents": [
                    {"role": "orchestrator", "mode": "always", "model_tier": "balanced"},
                    {"role": "architect", "mode": "on-demand", "model_tier": "strong"},
                ],
                "human_gates": ["irreversible actions", "budget threshold"],
                "notes": ["Standard product build."],
            },
        }
        self.assertEqual(load(dump(data)), data)


if __name__ == "__main__":
    unittest.main()
