# Scratch (builder B, NIGHT71 L3): per frame of a boss63 telemetry.jsonl, the boss bar's hint and hud47's fight line as FeelProbe recorded them
# (path, alpha, text), printing each change. Usage: python -I orders.py <telemetry.jsonl> [needle ...]
import json, sys

path = sys.argv[1]
needles = sys.argv[2:] or ["Hint", "Fight line 47"]
last = {}
with open(path, encoding="utf-8") as f:
    for line in f:
        try:
            d = json.loads(line)
        except Exception:
            continue
        if d.get("type") != "frame":
            continue
        fr, t, held = d.get("f"), d.get("t"), d.get("held")
        seen = {}
        for u in d.get("ui", []):
            p = u.get("path") or ""
            for n in needles:
                if p.endswith(n) or ("/" + n + "/") in p and p.endswith("/Main"):
                    key = n
                    txt = u.get("text")
                    a = u.get("alpha")
                    if txt is None:
                        continue
                    seen[key] = (txt, a, p)
        for n in needles:
            cur = seen.get(n)
            sig = (cur[0], round(cur[1] or 0, 2) > 0) if cur else None
            if last.get(n) != sig:
                print("f%04d t%6.2f %s %-14s %s" % (fr, t or 0, "H" if held else " ", n[:14], "-" if not cur else ("'%s' a=%.2f (%s)" % (cur[0], cur[1] or 0, cur[2]))))
                last[n] = sig
