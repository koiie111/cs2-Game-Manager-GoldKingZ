"""Verify saved raw samples and nearest-rank statistics without a game server."""
import hashlib
import json
import math
from pathlib import Path

root = Path(__file__).resolve().parents[1]
directory = root / "docs/performance/measurements"
summary = json.loads((directory / "summary.json").read_text(encoding="utf-8-sig"))
for window in summary["windows"]:
    path = directory / (window["window"] + ".json")
    raw = path.read_bytes()
    assert hashlib.sha256(raw).hexdigest() == window["sha256"], path
    data = json.loads(raw)
    count = data["count"]
    assert count == len(data["ticks"]) == len(data["allocations"]) == window["calls"]
    assert count > 0 and data["frequency"] > 0
    assert all(value >= 0 for value in data["ticks"] + data["allocations"])
    micros = sorted(value / data["frequency"] * 1e6 for value in data["ticks"])
    expected = {"mean_us": sum(micros) / count, "max_us": max(micros),
                "bytes_per_call": sum(data["allocations"]) / count}
    for percentile in (50, 95, 99):
        expected[f"p{percentile}_us"] = micros[math.ceil(count * percentile / 100) - 1]
    for key, value in expected.items():
        assert math.isclose(window[key], value, rel_tol=1e-10, abs_tol=1e-10), (path, key)

lines = (directory / "review-runtime.txt").read_text(encoding="utf-8-sig").splitlines()
windows = [json.loads(line) for line in lines if line.startswith("{")]
assert len(windows) == 24
for recipients in (0, 1, 8, 32):
    for optimized in (False, True):
        group = [w for w in windows if w["recipients"] == recipients and w["optimized"] == optimized]
        assert sorted(w["window"] for w in group) == [0, 1, 2]
        for w in group:
            assert w["synthetic"] and w["connected"] == 32 and w["frames"] == 20000
            assert w["delivered"] == w["frames"] * recipients
            assert w["milliseconds"] >= 0 and w["allocatedBytes"] >= 0
print("Verified 9 idle raw profiles and 24 synthetic runtime windows; no live speedup inferred.")
