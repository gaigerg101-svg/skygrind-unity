# NIGHT50 close table (the lead's): prints the close table from Tools/night/n50-rows.json, one line per row, in full.
#   python -I Tools/night/n50-close.py            -> the table (markdown), for NIGHT50-LOG.md
# A row's `state` (GREEN, SHORT, OFF, or the lead's wording), `passes` (true only after its check printed), `evidence` (paths) and
# `note` are written in the JSON by the lead; this script only reads them. Exit 1 if a row has no state.
import json
import os
import sys

ROWS = os.path.join("Tools", "night", "n50-rows.json")


def main():
    with open(ROWS, encoding="utf8") as f:
        rows = json.load(f)["rows"]
    print("| row | title | state | passes | evidence | note |")
    print("| --- | --- | --- | --- | --- | --- |")
    missing = []
    for r in rows:
        state = r.get("state", "")
        if not state:
            missing.append(r["id"])
        ev = "; ".join(r.get("evidence_paths", [])) or "-"
        note = r.get("note", "") or "-"
        print(f"| {r['id']} | {r['title']} | {state or '(none)'} | {str(r.get('passes', False)).lower()} | {ev} | {note} |")
    if missing:
        print("rows without a state:", ", ".join(missing))
        sys.exit(1)


if __name__ == "__main__":
    main()
