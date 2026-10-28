#!/usr/bin/env python3
"""NIGHT25: add U3-39 and U3-39.1 .. U3-39.9 to tasks.json after the last U3-38 line (single-line items, file's line endings kept)."""
import json, io

PATH = "tasks.json"
titles = [
    ("U3-39", "NIGHT25: THE TEACHING CAMPAIGN (bug fixes, pulse, tutorial, ten levels)"),
    ("U3-39.1", "NIGHT25: start and the Editor gate"),
    ("U3-39.2", "NIGHT25: the shared commit"),
    ("U3-39.3", "NIGHT25: baseline suite, smoke, GG's screenshots reproduced"),
    ("U3-39.4", "NIGHT25: integration rounds"),
    ("U3-39.5", "NIGHT25: the contract rows"),
    ("U3-39.6", "NIGHT25: level reviews"),
    ("U3-39.7", "NIGHT25: freeze and verification"),
    ("U3-39.8", "NIGHT25: carry-overs"),
    ("U3-39.9", "NIGHT25: docs and close"),
]
raw = io.open(PATH, "r", encoding="utf-8", newline="").read()
nl = "\r\n" if "\r\n" in raw else "\n"
lines = raw.split(nl)
last = max(i for i, l in enumerate(lines) if '"id": "U3-38.' in l)
new = []
for tid, title in titles:
    item = {"id": tid, "stage": "U3", "title": title, "why": "GG: a teaching campaign (2026-10-08).",
            "acceptance": ["see NIGHT25.md"], "check": {"how": "NIGHT25-LOG.md close table", "expect": "pass"},
            "status": "not_started", "evidence": None, "owner": "agent", "blockedBy": [], "decisions": [], "notes": ""}
    new.append("    " + json.dumps(item, ensure_ascii=False) + ",")
lines[last + 1:last + 1] = new
io.open(PATH, "w", encoding="utf-8", newline="").write(nl.join(lines))
json.load(io.open(PATH, encoding="utf-8"))
print("tasks.json parses; added", len(new))
