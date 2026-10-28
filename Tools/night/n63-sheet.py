# NIGHT63 builder V (the lead's tool for GG's report; tooling, no game change): contact sheets of same-run pairs, one sheet per row.
# For each pair: the ON frame `<...>/<side>/<scen>/<shot>.png` and its OFF twin `<...>/<side>-off/<scen>/<shot>.png` (D1's same-run layout; an
# explicit OFF path may be given instead), side by side, ON | OFF, each half downscaled to 960 px wide, under a one-line caption: the row, the
# pose's label, and the changed px (from the reel's toggle line when given; else measured here with n62-gun-yellow.py's rule: a pixel whose
# largest channel moves more than 24 of 255, said "measured here"). Plain PIL text (PIL's bundled default font), dark grey caption bands.
# Writes Logs/n63/sheets/<row>.png (or --out <dir>); reads only the frames named. Imports only numpy and PIL (python -I safe).
#
#   python -I Tools/night/n63-sheet.py --row <row> --pair <on.png> "<label>" [<changed px>] [--pair ...] [--row <row2> --pair ...] [--out <dir>]
#       each --row starts a sheet; the --pair lines after it belong to it. <changed px> is optional ('-' or omitted: measured here).
#       --off <off.png> right after a --pair's arguments names its OFF frame explicitly.
#   python -I Tools/night/n63-sheet.py --spec <file> [--out <dir>]
#       one pair per line, tab-separated: <row> <TAB> <on.png> <TAB> <label> [<TAB> <changed px> [<TAB> <off.png>]]; blank lines and lines
#       starting with # skipped; a sheet per row in first-seen order.
#   python -I Tools/night/n63-sheet.py --selftest
#       the planted cases (PASS / FAIL each; exit 1 on a FAIL), written into a temporary folder that is removed after
#
# e.g. python -I Tools/night/n63-sheet.py --row bossMotion63 --pair Logs/feel/n63-b5/b1w/boss63/shot-N30-B63-wick-stagger.png "THE WICK, stagger slump" 61234
import os
import shutil
import sys
import tempfile

import numpy as np
from PIL import Image, ImageDraw, ImageFont

HALF_W = 960          # each half's width on the sheet (px)
GAP = 8               # between the halves and between the pose bands (px)
CAPTION_H = 40        # the caption band's height (px)
TAG_H = 28            # the ON / OFF tag band under the caption (px)
BG = (24, 24, 28)     # the sheet's background
CAPTION_BG = (40, 40, 46)
TEXT = (235, 235, 235)
TAG_ON = (120, 220, 235)
TAG_OFF = (200, 200, 200)
DIFF = 24             # n62-gun-yellow.py's changed-pixel rule
DEFAULT_OUT = os.path.join("Logs", "n63", "sheets")


def off_twin(on_path):
    """<...>/<side>/<scen>/<shot>.png -> <...>/<side>-off/<scen>/<shot>.png (D1's same-run layout)."""
    norm = os.path.normpath(on_path)
    shot_dir, shot = os.path.split(norm)
    side_dir, scen = os.path.split(shot_dir)
    parent, side = os.path.split(side_dir)
    if not side or not scen:
        raise ValueError("cannot find <side>/<scen>/<shot> in " + on_path)
    return os.path.join(parent, side + "-off", scen, shot)


def changed_px(a, b):
    if a.shape != b.shape:
        raise ValueError("the two frames differ in size: %s vs %s" % (a.shape, b.shape))
    return int((np.abs(a.astype(np.int16) - b.astype(np.int16)).max(axis=-1) > DIFF).sum())


def font(size):
    try:
        return ImageFont.load_default(size=size)
    except TypeError:  # an old PIL without the sized default
        return ImageFont.load_default()


def half(img):
    w, h = img.size
    nh = max(1, int(round(h * HALF_W / float(w))))
    return img.resize((HALF_W, nh), Image.LANCZOS)


def build_sheet(row, pairs, out_dir):
    """pairs: [(on_path, label, px_or_None, off_path_or_None)]; returns (the sheet's path, [(label, px, measured)])."""
    bands, notes = [], []
    f_cap, f_tag = font(22), font(18)
    for on_path, label, px, off_path in pairs:
        off_path = off_path or off_twin(on_path)
        on_img = Image.open(on_path).convert("RGB")
        off_img = Image.open(off_path).convert("RGB")
        measured = px is None
        if measured:
            px = changed_px(np.asarray(on_img), np.asarray(off_img))
        notes.append((label, px, measured))
        a, b = half(on_img), half(off_img)
        h = max(a.size[1], b.size[1])
        band = Image.new("RGB", (HALF_W * 2 + GAP, CAPTION_H + TAG_H + h), BG)
        d = ImageDraw.Draw(band)
        d.rectangle([0, 0, band.size[0], CAPTION_H], fill=CAPTION_BG)
        share = 100.0 * px / float(on_img.size[0] * on_img.size[1])
        caption = "%s  |  %s  |  changed %s px (%.2f %% of the frame)%s" % (row, label, format(px, ","), share, " measured here" if measured else "")
        d.text((12, (CAPTION_H - 22) // 2), caption, fill=TEXT, font=f_cap)
        d.text((12, CAPTION_H + 4), "ON   " + os.path.basename(on_path), fill=TAG_ON, font=f_tag)
        d.text((HALF_W + GAP + 12, CAPTION_H + 4), "OFF  " + os.path.basename(off_path), fill=TAG_OFF, font=f_tag)
        band.paste(a, (0, CAPTION_H + TAG_H))
        band.paste(b, (HALF_W + GAP, CAPTION_H + TAG_H))
        bands.append(band)
    if not bands:
        raise ValueError("row " + row + ": no pairs")
    total_h = sum(bd.size[1] for bd in bands) + GAP * (len(bands) - 1)
    sheet = Image.new("RGB", (HALF_W * 2 + GAP, total_h), BG)
    y = 0
    for bd in bands:
        sheet.paste(bd, (0, y))
        y += bd.size[1] + GAP
    os.makedirs(out_dir, exist_ok=True)
    safe = "".join(c if c.isalnum() or c in "-_." else "_" for c in row)
    path = os.path.join(out_dir, safe + ".png")
    sheet.save(path)
    return path, notes


def parse_args(argv):
    rows, order, out_dir, i = {}, [], DEFAULT_OUT, 0
    cur = None
    while i < len(argv):
        a = argv[i]
        if a == "--out":
            out_dir = argv[i + 1]
            i += 2
        elif a == "--row":
            cur = argv[i + 1]
            if cur not in rows:
                rows[cur] = []
                order.append(cur)
            i += 2
        elif a == "--pair":
            if cur is None:
                raise ValueError("--pair before any --row")
            on_path, label = argv[i + 1], argv[i + 2]
            i += 3
            px = None
            if i < len(argv) and not argv[i].startswith("--"):
                px = None if argv[i] == "-" else int(argv[i].replace(",", ""))
                i += 1
            off_path = None
            if i < len(argv) and argv[i] == "--off":
                off_path = argv[i + 1]
                i += 2
            rows[cur].append((on_path, label, px, off_path))
        elif a == "--spec":
            with open(argv[i + 1], "r", encoding="utf-8") as fh:
                for line in fh:
                    line = line.rstrip("\r\n")
                    if not line.strip() or line.lstrip().startswith("#"):
                        continue
                    parts = line.split("\t")
                    if len(parts) < 3:
                        raise ValueError("a spec line needs <row> TAB <on.png> TAB <label>: " + line)
                    row, on_path, label = parts[0], parts[1], parts[2]
                    px = None if len(parts) < 4 or parts[3].strip() in ("", "-") else int(parts[3].replace(",", ""))
                    off_path = parts[4] if len(parts) >= 5 and parts[4].strip() else None
                    if row not in rows:
                        rows[row] = []
                        order.append(row)
                    rows[row].append((on_path, label, px, off_path))
            i += 2
        else:
            raise ValueError("unknown argument: " + a)
    return [(r, rows[r]) for r in order], out_dir


def selftest():
    fails = 0

    def check(name, ok, got):
        nonlocal fails
        if not ok:
            fails += 1
        print(("PASS " if ok else "FAIL ") + name + ": " + str(got))

    tmp = tempfile.mkdtemp(prefix="n63-sheet-")
    try:
        rng = np.random.RandomState(7)
        frame = (rng.rand(1080, 1920, 3) * 255).astype(np.uint8)
        for side in ("s1", "s1-off"):
            os.makedirs(os.path.join(tmp, side, "boss63"), exist_ok=True)
        on_p = os.path.join(tmp, "s1", "boss63", "shot-a.png")
        off_p = os.path.join(tmp, "s1-off", "boss63", "shot-a.png")
        Image.fromarray(frame).save(on_p)
        Image.fromarray(frame).save(off_p)
        check("the OFF twin's path", os.path.normpath(off_twin(on_p)) == os.path.normpath(off_p), off_twin(on_p))
        out = os.path.join(tmp, "sheets")
        path, notes = build_sheet("rowA63", [(on_p, "planted: identical frames", None, None)], out)
        sheet = np.asarray(Image.open(path).convert("RGB"))
        top = CAPTION_H + TAG_H
        left = sheet[top:, :HALF_W]
        right = sheet[top:, HALF_W + GAP:HALF_W * 2 + GAP]
        check("two identical frames: the sheet's halves match", left.shape == right.shape and int(np.abs(left.astype(np.int16) - right.astype(np.int16)).max()) == 0, left.shape)
        check("each half 960 px wide, 540 tall from 1920 x 1080", left.shape[1] == 960 and left.shape[0] == 540, left.shape[:2])
        check("identical frames: 0 changed px, measured here", notes[0][1] == 0 and notes[0][2], notes[0])
        changed = frame.copy()
        changed[100:110, 200:210] = ((frame[100:110, 200:210].astype(np.int16) + 128) % 256).astype(np.uint8)  # every channel moves 128 (> 24): 100 changed px
        on2 = os.path.join(tmp, "s1", "boss63", "shot-b.png")
        off2 = os.path.join(tmp, "s1-off", "boss63", "shot-b.png")
        Image.fromarray(changed).save(on2)
        Image.fromarray(frame).save(off2)
        _, notes2 = build_sheet("rowB63", [(on2, "planted: a 10 x 10 change", None, None)], out)
        check("a planted 10 x 10 change: exactly 100 changed px", notes2[0][1] == 100, notes2[0][1])
        _, notes3 = build_sheet("rowC63", [(on2, "given by the toggle line", 4321, None)], out)
        check("a given changed px is printed as given (not measured)", notes3[0][1] == 4321 and not notes3[0][2], notes3[0])
        rows, out_dir = parse_args(["--row", "r1", "--pair", on_p, "pose 1", "1,234", "--pair", on2, "pose 2", "-", "--off", off2, "--row", "r2", "--pair", on_p, "pose 3", "--out", out])
        ok = len(rows) == 2 and rows[0][1][0][2] == 1234 and rows[0][1][1][2] is None and rows[0][1][1][3] == off2 and rows[1][1][0][2] is None and out_dir == out
        check("the command line: rows, pairs, a given px, '-', --off, --out", ok, rows)
        spec = os.path.join(tmp, "spec.txt")
        with open(spec, "w", encoding="utf-8") as fh:
            fh.write("# a comment\n\nr9\t" + on_p + "\tpose 9\t77\n")
        rows2, _ = parse_args(["--spec", spec])
        check("the spec file", len(rows2) == 1 and rows2[0][0] == "r9" and rows2[0][1][0][2] == 77, rows2)
        two, _ = build_sheet("rowD63", [(on_p, "pose 1", None, None), (on2, "pose 2", None, None)], out)
        h2 = Image.open(two).size[1]
        check("two poses: two bands stacked", h2 == 2 * (CAPTION_H + TAG_H + 540) + GAP, h2)
    finally:
        shutil.rmtree(tmp, ignore_errors=True)
    total = 9
    print("selftest: %d / %d PASS" % (total - fails, total))
    return 1 if fails else 0


def main(argv):
    if "--selftest" in argv:
        return selftest()
    if not argv:
        print(__doc__ if __doc__ else "usage: see the file's head")
        return 2
    rows, out_dir = parse_args(argv)
    for row, pairs in rows:
        path, notes = build_sheet(row, pairs, out_dir)
        print("%s: %d poses -> %s" % (row, len(notes), path))
        for label, px, measured in notes:
            print("   %s: changed %s px%s" % (label, format(px, ","), " (measured here)" if measured else " (as given)"))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
