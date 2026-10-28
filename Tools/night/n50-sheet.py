# NIGHT50 G6 before / after contact sheet (the lead's). One row per campaign level: BEFORE = the grade off (NIGHT49's look on that level, the
# n50-r1 a349 combat-off frame), AFTER = the freeze's state (the grade on only where listed: n48-foundry and n47-spire; elsewhere the same
# frame as before). Uses the saved reel frames (no Player). Writes Logs/n50/sheet/sheet.png and prints the level table.
#   python -I Tools/night/n50-sheet.py
import os
import sys

from PIL import Image, ImageDraw

SRC = os.path.join("Logs", "feel", "n50-r1")
OUT = os.path.join("Logs", "n50", "sheet")
LEVELS = ["n48-foundry", "n47-iceline", "n47-kiln", "n47-span", "n47-spire"]
FREEZE_ON = {"n48-foundry", "n47-spire"}  # DECIDE50 D-12


def frame(level, side):
    p = os.path.join(SRC, "v-" + level, "a349", "shot-N49-V-" + level + "-combat-" + side + ".png")
    if not os.path.isfile(p):
        sys.exit("missing frame: " + p)
    return Image.open(p).convert("RGB")


def main():
    os.makedirs(OUT, exist_ok=True)
    tiles = []
    for level in LEVELS:
        before = frame(level, "off")
        after = frame(level, "on") if level in FREEZE_ON else before
        w, h = 480, 270
        row = Image.new("RGB", (w * 2 + 10, h + 24), (20, 20, 24))
        row.paste(before.resize((w, h)), (0, 24))
        row.paste(after.resize((w, h)), (w + 10, 24))
        d = ImageDraw.Draw(row)
        d.text((4, 4), level + "  BEFORE (grade off)", fill=(230, 230, 230))
        d.text((w + 14, 4), level + "  AFTER (" + ("grade on" if level in FREEZE_ON else "unchanged") + ")", fill=(230, 230, 230))
        tiles.append(row)
    sheet = Image.new("RGB", (tiles[0].width, sum(t.height for t in tiles) + 10 * (len(tiles) - 1)), (20, 20, 24))
    y = 0
    for t in tiles:
        sheet.paste(t, (0, y))
        y += t.height + 10
    sheet.save(os.path.join(OUT, "sheet.png"))
    for level in LEVELS:
        print(level, "changed" if level in FREEZE_ON else "unchanged (same frame before and after)")
    print("wrote", os.path.join(OUT, "sheet.png"))


if __name__ == "__main__":
    main()
