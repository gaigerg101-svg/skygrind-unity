# NIGHT52 the before / after sheet (the lead's; NIGHT52.md section 7 "the before / after sheet", section 11 "density before / after for
# every rebuilt room"). One row per rebuilt room: BEFORE plan | AFTER plan | BEFORE entry frame | AFTER entry frame, with the room's
# density before -> after printed on the row. Plans: Logs/n52/sheet/plan-<level>-<room>-{before,after}.png (Tools/night/n52-svg-png.sh);
# frames: the level36 reel's "-a" entry frame (the room's spawn view 1.2 s after the entry) from one build for both sides:
#   bash Tools/night/n47-reels.sh <tag> b-<lv>,<old level id>,level36 a-<lv>,<n52 level id>,level36
# Usage: python -I Tools/night/n52-sheet.py <tag> <lv>:<room> ...      e.g. python -I Tools/night/n52-sheet.py n52-freeze kiln:kb2 iceline:g7
# Writes Logs/n52/sheet/sheet.png (and sheet-<n>.png pages of 6 rows) and prints the density table (Markdown).
import json
import os
import sys

from PIL import Image, ImageDraw

OLD_PLAN = {"kiln": "Design/levels/n47/kiln.plan.json", "foundry": "Design/levels/n46/foundry.plan.json", "iceline": "Design/levels/n47/iceline.plan.json",
            "spire": "Design/levels/n47/spire.plan.json", "span": "Design/levels/n47/span.plan.json"}
NEW_PLAN = {lv: "Design/levels/n52/" + lv + ".plan.json" for lv in OLD_PLAN}
OLD_ID = {"kiln": "n47-kiln", "foundry": "n48-foundry", "iceline": "n47-iceline", "spire": "n47-spire", "span": "n47-span"}
OUT = os.path.join("Logs", "n52", "sheet")
W, H = 480, 270  # one cell (a 16:9 frame, a plan letterboxed)


def density(plan_path, room_id):
    p = json.load(open(plan_path, encoding="utf-8"))
    r = next(x for x in p["rooms"] if x["id"] == room_id)
    x0, z0, x1, z1 = r["rect"]
    zone = lambda b: isinstance(b, dict) and b.get("zone") == room_id
    n = sum(1 for k in ("boxes", "ramps", "pads", "grapple") for b in p.get(k, []) if zone(b))
    n += len(r.get("grappleNodes", [])) + len(r.get("pads", []))
    n += sum(1 for q in p.get("props", []) if q.get("room") == room_id and q.get("collider"))
    return n, (x1 - x0), (z1 - z0), n * 100.0 / ((x1 - x0) * (z1 - z0))


def cell(path, label):
    img = Image.new("RGB", (W, H), (12, 12, 20))
    if path and os.path.isfile(path):
        src = Image.open(path).convert("RGB")
        src.thumbnail((W, H - 18))
        img.paste(src, ((W - src.width) // 2, 18 + (H - 18 - src.height) // 2))
    else:
        label += " (MISSING)"
    ImageDraw.Draw(img).text((6, 3), label, fill=(235, 235, 245))
    return img


def main():
    if len(sys.argv) < 3:
        sys.exit("usage: n52-sheet.py <tag> <lv>:<room> ...")
    tag, pairs = sys.argv[1], [a.split(":") for a in sys.argv[2:]]
    os.makedirs(OUT, exist_ok=True)
    rows, table = [], ["| level | room | before | after | pieces / 100 m2 before -> after |", "|---|---|---|---|---|"]
    for lv, room in pairs:
        nb, wb, db, xb = density(OLD_PLAN[lv], room)
        na, wa, da, xa = density(NEW_PLAN[lv], room)
        table.append("| %s | %s | %g x %g m, %d pieces | %g x %g m, %d pieces | %.2f -> %.2f |" % (lv, room, wb, db, nb, wa, da, na, xb, xa))
        fb = os.path.join("Logs", "feel", tag, "b-" + lv, "level36", "shot-N29-N36-level36-" + room + "-a.png")
        fa = os.path.join("Logs", "feel", tag, "a-" + lv, "level36", "shot-N29-N36-level36-" + room + "-a.png")
        row = Image.new("RGB", (W * 4, H), (0, 0, 0))
        row.paste(cell(os.path.join(OUT, "plan-%s-%s-before.png" % (lv, room)), "%s %s BEFORE (%s) %.2f /100 m2" % (lv, room, OLD_ID[lv], xb)), (0, 0))
        row.paste(cell(os.path.join(OUT, "plan-%s-%s-after.png" % (lv, room)), "AFTER (n52-%s) %.2f /100 m2" % (lv, xa)), (W, 0))
        row.paste(cell(fb, "entry BEFORE"), (W * 2, 0))
        row.paste(cell(fa, "entry AFTER"), (W * 3, 0))
        rows.append(row)
    for page in range(0, len(rows), 6):
        chunk = rows[page:page + 6]
        sheet = Image.new("RGB", (W * 4, H * len(chunk)), (0, 0, 0))
        for i, r in enumerate(chunk):
            sheet.paste(r, (0, i * H))
        name = "sheet.png" if page == 0 else "sheet-%d.png" % (page // 6 + 1)
        sheet.save(os.path.join(OUT, name))
        print("wrote " + os.path.join(OUT, name))
    print("\n".join(table))


if __name__ == "__main__":
    main()
