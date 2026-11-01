import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location(
    "coach_history", Path(__file__).with_name("verify-pro-coach-history.py"))
history = importlib.util.module_from_spec(spec)
spec.loader.exec_module(history)


class CoachHistoryTests(unittest.TestCase):
    def test_equal_results_share_the_middle_rank(self):
        self.assertEqual(50, history.percentile(8, [3, 8, 8, 13]))
        self.assertEqual(50, history.percentile(8, [13, 8, 3, 8], reverse=True))

    def test_scoring_scale_has_bounded_endpoints(self):
        self.assertEqual(20, history.percentile(3, [3, 8, 13]))
        self.assertEqual(80, history.percentile(13, [3, 8, 13]))
        self.assertEqual(80, history.percentile(99, [3, 8, 13]))
        self.assertEqual(20, history.percentile(13, [3, 8, 13], reverse=True))

    def test_tied_games_are_not_losses(self):
        result = history.stats([(20, 20), (10, 7), (7, 10)])
        self.assertEqual((3, 1, 1), (result["Games"], result["Wins"], result["Losses"]))


if __name__ == "__main__":
    unittest.main()
