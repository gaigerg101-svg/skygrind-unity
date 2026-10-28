# NIGHT55 builder R (P1-doorDark, critic-ggproxy #9): the flat-frame meter. For every frame of a reel folder, the luma standard deviation
# (Rec. 601 luma, 0-1) over the WORLD area: the HUD's corners and the gun's quadrant are masked out (they stay detailed when the world is one
# flat colour, as on n54-freeze vk verbs37 f0950). A frame is FLAT when its world luma std < 0.03 (the critic's threshold).
#   python -I Tools/night/n55-r-flat.py <frames dir> [<frames dir> ...]   e.g. Logs/feel/n54-freeze/vk/verbs37
# Prints one line per flat frame and a total per folder; exit 1 when any frame is flat. Needs Pillow (PIL).
import os, sys
from PIL import Image

TH = 0.03


def world_std(path):
    im = Image.open(path).convert('L')
    w, h = im.size
    im = im.resize((w // 8, h // 8))
    w, h = im.size
    px = im.load()
    vals = []
    for y in range(h):
        for x in range(w):
            fx, fy = x / w, y / h
            if fy < 0.25 and (fx < 0.25 or fx > 0.70):  # the counter plate, the style meter
                continue
            if fy < 0.22 and 0.40 < fx < 0.60:  # the zone plate and the hint line
                continue
            if fy > 0.55 and fx > 0.52:  # the gun and the weapon plate
                continue
            if fy > 0.85 and fx < 0.30:  # the health bar
                continue
            vals.append(px[x, y] / 255.0)
    n = len(vals)
    m = sum(vals) / n
    return (sum((v - m) ** 2 for v in vals) / n) ** 0.5, m


bad_all = 0
for d in sys.argv[1:]:
    fs = sorted(f for f in os.listdir(d) if f.lower().endswith(('.jpg', '.png')))
    bad = []
    lo = None
    for f in fs:
        s, m = world_std(os.path.join(d, f))
        if lo is None or s < lo[0]:
            lo = (s, f)
        if s < TH:
            bad.append((f, s, m))
    for f, s, m in bad:
        print('  FLAT %s/%s world luma std %.4f (mean %.3f)' % (d, f, s, m))
    print('n55-r-flat %s: %d frames, %d flat (std < %.2f); lowest %.4f at %s' % (d, len(fs), len(bad), TH, lo[0] if lo else -1, lo[1] if lo else '-'))
    bad_all += len(bad)
sys.exit(1 if bad_all else 0)
