#!/usr/bin/env python3
# NIGHT73 (CLAUDE.md rule 8: red-orange is enemies and danger only). The share of a frame's pixels in the danger hue band, the lead's meter for
# the kiln arena's orange (n72-play bk frames: walls, floor and ceiling read red-orange around THE KILN WYRM).
#   python -I Tools/night/n73-hue.py <image> [<image> ...]
# Per image: danger share (hue 0-40 or 345-360 deg, saturation >= 0.35, value >= 0.20), the HUD bands excluded (top 12 % and bottom 18 % of
# the frame), and the mean luma; then the mean over the images. The band is NIGHT54's danger family (L-097), written here, never moved after a run.
import sys
from PIL import Image

def share(p):
    im = Image.open(p).convert('RGB').resize((480, 270))
    hsv = im.convert('HSV')
    w, h = im.size
    y0, y1 = int(h * 0.12), int(h * 0.82)
    n = d = 0
    lum = 0.0
    px, ph = im.load(), hsv.load()
    for y in range(y0, y1):
        for x in range(w):
            hh, s, v = ph[x, y]
            deg = hh * 360.0 / 255.0
            n += 1
            r, g, b = px[x, y]
            lum += 0.2126 * r + 0.7152 * g + 0.0722 * b
            if (deg <= 40 or deg >= 345) and s >= 0.35 * 255 and v >= 0.20 * 255:
                d += 1
    return 100.0 * d / n, lum / n

if __name__ == '__main__':
    tot = []
    for p in sys.argv[1:]:
        s, l = share(p)
        tot.append(s)
        print('%-90s danger %5.1f %%  luma %5.1f' % (p, s, l))
    if tot:
        print('mean danger share %.1f %% over %d frames' % (sum(tot) / len(tot), len(tot)))
