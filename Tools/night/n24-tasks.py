#!/usr/bin/env python3
"""NIGHT24: add U3-38 and U3-38.1 .. U3-38.9 to tasks.json after the last U3-37 line (single-line items, file's line endings kept)."""
import json, io

PATH = "tasks.json"
titles = [
    ("U3-38", "NIGHT24: THE LINEAR TURN (campaign spine, worm boss, removals)"),
    ("U3-38.1", "NIGHT24: start and the Editor gate"),
    ("U3-38.2", "NIGHT24: the shared commit"),
    ("U3-38.3", "NIGHT24: baseline suite and NIGHT23 debts"),
    ("U3-38.4", "NIGHT24: builder slices land (compile rounds)"),
    ("U3-38.5", "NIGHT24: the 13 contract rows"),
    ("U3-38.6", "NIGHT24: switch-lag (hitch tour and fixes)"),
    ("U3-38.7", "NIGHT24: freeze and verification"),
    ("U3-38.8", "NIGHT24: leftovers"),
    ("U3-38.9", "NIGHT24: docs and close"),
]
raw = io.open(PATH, "r", encoding="utf-8", newline="").read()
nl = "\r\n" if "\r\n" in raw else "\n"
lines = raw.split(nl)
last = max(i for i, l in enumerate(lines) if '"id": "U3-37.' in l)
new = []
for tid, title in titles:
    item = {"id": tid, "stage": "U3", "title": title, "why": "GG turned the game linear (2026-10-08).",
            "acceptance": ["see NIGHT24.md"], "check": {"how": "NIGHT24-LOG.md close table", "expect": "pass"},
            "status": "not_started", "evidence": None, "owner": "agent", "blockedBy": [], "decisions": [], "notes": ""}
    new.append("    " + json.dumps(item, ensure_ascii=False) + ",")
lines[last + 1:last + 1] = new
io.open(PATH, "w", encoding="utf-8", newline="").write(nl.join(lines))
json.load(io.open(PATH, encoding="utf-8"))
print("tasks.json parses; added", len(new))
