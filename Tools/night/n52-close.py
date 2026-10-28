# NIGHT52 close table (the lead's; from n50-close.py): prints the close table from Tools/night/n52-rows.json, one line per row, in full.
#   python -I Tools/night/n52-close.py            -> the table (Markdown), for NIGHT52-LOG.md
# A row's `state` (DONE / GREEN, SHORT with its fallback, OFF with its reason), `passes` (true only after its check printed),
# `evidence_paths` and `note` are written by the lead (Tools/night/n52-flip.cjs); this script only reads them. Exit 1 if a row has no state.
import json
import os
import sys

ROWS = os.path.join("Tools", "night", "n52-rows.json")


def main():
    with open(ROWS, encoding="utf8") as f:
        rows = json.load(f)["rows"]
    print("| row | title | check | state | passes | evidence | note |")
    print("| --- | --- | --- | --- | --- | --- | --- |")
    missing = []
    for r in rows:
        state = r.get("state", "")
        if not state:
            missing.append(r["id"])
        ev = "; ".join(r.get("evidence_paths", [])) or "-"
        note = r.get("note", "") or "-"
        print(f"| {r['id']} | {r['title']} | {r['check']} | {state or '(none)'} | {str(r.get('passes', False)).lower()} | {ev} | {note} |")
    if missing:
        print("rows without a state:", ", ".join(missing))
        sys.exit(1)


if __name__ == "__main__":
    main()
