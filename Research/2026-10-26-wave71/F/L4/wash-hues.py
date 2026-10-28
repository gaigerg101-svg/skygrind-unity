# NIGHT71 builder F (L4): the hue spread of the WASH box's lit pixels (value > 0.35, not white: saturation >= 0.12) per frame, in 30-degree
# bins, each bin's share of the box's mean luma; for the frames named. Read only: prints. Box as wash-classes.py (a153 crops, 1:1).
# usage: python -I wash-hues.py <png> [<png> ...]
import colorsys
import sys

from PIL import Image

BX0, BX1, BY0, BY1 = 24, 408, 108, 324
for p in sys.argv[1:]:
    im = Image.open(p).convert('RGB')
    px = im.load()
    n = 0
    bins = [0.0] * 12
    for y in range(BY0, BY1):
        for x in range(BX0, BX1):
            r, g, b = px[x, y]
            n += 1
            h, s, v = colorsys.rgb_to_hsv(r / 255.0, g / 255.0, b / 255.0)
            if v <= 0.35 or s < 0.12:
                continue
            bins[int(h * 12) % 12] += (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255.0
    print(p.split('/')[-1] + ': ' + ' '.join(f'{i * 30}-{i * 30 + 30} {bins[i] / n:.3f}' for i in range(12)))
