# NIGHT71 builder F (L4, T-105 "the f3-f4 source, frame by frame"): read a153's saved wash crops (FeelReel.N53.Polish.cs: CropUp48 of a
# 0.4 h square round the centre at 1:1, 432 px at 1080p; FeelReel.N48.Lineup.cs:177) and split the WASH box (Wash49: the centre 20 % of the
# frame each way, 384 x 216 px at 1920 x 1080, so x 24..408 and y 108..324 of the crop) into colour classes, each class's share of the box's
# mean luma (Rec. 709 on the sRGB values, as Wash49). Read only: prints.
# usage: python -I wash-classes.py <a153 dir> [<a153 dir> ...]
import colorsys
import os
import sys

from PIL import Image

W = H = 432
BX0, BX1 = 216 - 192, 216 + 192  # 384 px of a 1920 frame
BY0, BY1 = 216 - 108, 216 + 108  # 216 px of a 1080 frame


def classes(path):
    im = Image.open(path).convert('RGB')
    if im.size != (W, H):
        return None
    px = im.load()
    n = 0
    tot = {'all': 0.0, 'warm (red-orange-yellow, lit)': 0.0, 'white / grey-white': 0.0, 'cyan': 0.0, 'dark (< 0.15)': 0.0, 'other': 0.0}
    cnt = {k: 0 for k in tot}
    for y in range(BY0, BY1):
        for x in range(BX0, BX1):
            r, g, b = px[x, y]
            lum = (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255.0
            h, s, v = colorsys.rgb_to_hsv(r / 255.0, g / 255.0, b / 255.0)
            hd = h * 360
            if v < 0.15:
                k = 'dark (< 0.15)'
            elif s < 0.25 and v > 0.6:
                k = 'white / grey-white'
            elif (hd <= 60 or hd >= 345) and s >= 0.45:
                k = 'warm (red-orange-yellow, lit)'
            elif 160 <= hd <= 210 and s >= 0.3:
                k = 'cyan'
            else:
                k = 'other'
            tot['all'] += lum
            tot[k] += lum
            cnt[k] += 1
            n += 1
    return {k: (tot[k] / n, cnt[k] / n) for k in tot}


for d in sys.argv[1:]:
    print(f'== {d}')
    for side in ('off', 'on'):
        for k in range(1, 9):
            p = os.path.join(d, f'shot-N30-S53-wash-{side}-f{k}-box.png')
            if not os.path.exists(p):
                continue
            c = classes(p)
            if c is None:
                print(f'  {side} f{k}: not a 432 px crop')
                continue
            parts = ', '.join(f'{name} {lum:.3f} ({share * 100:.0f} % of px)' for name, (lum, share) in c.items() if name != 'all')
            print(f'  {side} f{k}: box luma {c["all"][0]:.3f} = {parts}')
