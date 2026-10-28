#!/usr/bin/env python3
"""NIGHT24: turn design.n24.<row>.enabled on / off in Design/unity-config.json (text edit, the file's layout kept).
   python Tools/night/n24-rows.py on padY shotgunTune ...     |     python Tools/night/n24-rows.py off all
   Prints the rows' states after. The lead then runs Tools/night/n24-sync.sh."""
import io, re, sys, json

PATH = "Design/unity-config.json"
mode = sys.argv[1]
rows = sys.argv[2:]
text = io.open(PATH, "r", encoding="utf-8", newline="").read()
d = json.loads(text)
names = [k for k in d["design"]["n24"] if k != "_why"]
if rows == ["all"]:
    rows = names
want = "true" if mode == "on" else "false"
start = text.index('"n24": {')
for r in rows:
    # the row's block starts at "<r>": { after the n24 start; its first "enabled" is the row's own switch (set right after _why)
    m = re.compile(r'("%s": \{)' % re.escape(r)).search(text, start)
    if not m:
        print("no row", r)
        continue
    e = re.compile(r'("enabled": )(true|false)').search(text, m.end())
    text = text[:e.start()] + e.group(1) + want + text[e.end():]
io.open(PATH, "w", encoding="utf-8", newline="").write(text)
d = json.loads(text)
print({k: d["design"]["n24"][k]["enabled"] for k in names})
