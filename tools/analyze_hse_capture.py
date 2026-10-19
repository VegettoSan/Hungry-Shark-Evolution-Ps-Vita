"""Summarize an owner-controlled Editor capture, without attributing FPS to Vita."""
import argparse
import csv
import json
import math
from pathlib import Path
from statistics import mean


def percentile(values, fraction):
    values = sorted(values)
    index = (len(values) - 1) * fraction
    low = int(index)
    high = min(low + 1, len(values) - 1)
    return values[low] + (values[high] - values[low]) * (index - low)


def summarize(path):
    with path.open(newline="", encoding="utf-8-sig") as stream:
        rows = [{key: float(value) for key, value in row.items()} for row in csv.DictReader(stream)]
    if not rows:
        raise ValueError("Empty capture")
    times = [row["frame_ms"] for row in rows]
    if any(not math.isfinite(value) or value < 0 for value in times):
        raise ValueError("Invalid frame time")
    result = {
        "classification": "Editor observation; not Vita FPS or a matched performance comparison",
        "samples": len(times), "mean_ms": mean(times),
        "p50_ms": percentile(times, .5), "p95_ms": percentile(times, .95),
        "p99_ms": percentile(times, .99), "max_ms": max(times),
        "frames_above_33_333_ms": sum(value > 1000 / 30 for value in times),
        "eats_first": rows[0]["eats"], "eats_last": rows[-1]["eats"],
        "gc_collections": sum(row["gc"] for row in rows),
        "capture_seconds": rows[-1]["seconds"] - rows[0]["seconds"],
    }
    for name in ("x", "y", "mono_bytes", "allocated_bytes", "reserved_bytes"):
        valid = [row[name] for row in rows if math.isfinite(row[name])]
        result[name + "_range"] = [min(valid), max(valid)] if valid else None
    for name in ("main_ns", "render_ns", "physics_ns"):
        valid = [row[name] / 1e6 for row in rows if row[name] >= 0]
        result[name.replace("_ns", "_ms")] = {
            "available_samples": len(valid), "mean": mean(valid) if valid else None,
            "p95": percentile(valid, .95) if valid else None,
        }
    # Correlation windows only. Loading, renderer, AI and effects need the raw call tree.
    result["feeding_frames"] = [
        {"frame": row["frame"], "frame_ms": row["frame_ms"], "eats": row["eats"]}
        for previous, row in zip(rows, rows[1:]) if row["eats"] > previous["eats"]
    ]
    return result


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("csv", type=Path)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    report = json.dumps(summarize(args.csv), indent=2, allow_nan=False) + "\n"
    if args.output:
        args.output.write_text(report, encoding="utf-8")
    else:
        print(report, end="")
