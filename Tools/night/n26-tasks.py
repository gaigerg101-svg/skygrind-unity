#!/usr/bin/env python3
"""NIGHT26: add U3-40 and U3-40.1 .. U3-40.9 to tasks.json after the last U3-39 line (single-line items, file's line endings kept)."""
import json, io

PATH = "tasks.json"
titles = [
    ("U3-40", "NIGHT26: THE FIRST THREE LEVELS (platforming, chase boss, weapons, level select)"),
    ("U3-40.1", "NIGHT26: start and the Editor gate"),
    ("U3-40.2", "NIGHT26: the shared commit and builders"),
    ("U3-40.3", "NIGHT26: build rounds"),
    ("U3-40.4", "NIGHT26: freeze and verification"),
    ("U3-40.5", "NIGHT26: docs and close"),
]
raw = io.open(PATH, "r", encoding="utf-8", newline="").read()
nl = "\r\n" if "\r\n" in raw else "\n"
lines = raw.split(nl)
last = max(i for i, l in enumerate(lines) if '"id": "U3-39.' in l)
new = []
for tid, title in titles:
    item = {"id": tid, "stage": "U3", "title": title, "why": "GG: the first three levels (2026-10-08).",
            "acceptance": ["see NIGHT26.md"], "check": {"how": "NIGHT26-LOG.md close table", "expect": "pass"},
            "status": "not_started", "evidence": None, "owner": "agent", "blockedBy": [], "decisions": [], "notes": ""}
    new.append("    " + json.dumps(item, ensure_ascii=False) + ",")
lines[last + 1:last + 1] = new
io.open(PATH, "w", encoding="utf-8", newline="").write(nl.join(lines))
json.load(io.open(PATH, encoding="utf-8"))
print("tasks.json parses; added", len(new))
