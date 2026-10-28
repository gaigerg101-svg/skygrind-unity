"""Frame look meter (wave-49 spike). Usage: python -I lookmeter.py <label>=<png> ...
Per frame: mean value (HSV V), share of dark (V<0.25), mid, bright (V>0.75) pixels, mean saturation, share of strongly
saturated pixels (S>0.5 and V>0.3), share of cyan pixels (hue 170-200 deg, S>0.35, V>0.35), and hue spread (number of
30-degree hue bins holding >= 3 % of the saturated pixels). The HUD band (top 12 % and bottom 18 %) is cut out."""
import sys
import numpy as np
from PIL import Image

for arg in sys.argv[1:]:
    label, _, path = arg.partition("=")
    im = np.asarray(Image.open(path).convert("RGB"), dtype=np.float32) / 255.0
    h0, h1 = int(im.shape[0] * 0.12), int(im.shape[0] * 0.82)
    im = im[h0:h1]
    r, g, b = im[..., 0], im[..., 1], im[..., 2]
    mx, mn = im.max(-1), im.min(-1)
    v = mx
    s = np.where(mx > 0, (mx - mn) / np.maximum(mx, 1e-6), 0)
    d = mx - mn + 1e-6
    hue = np.where(mx == r, (g - b) / d % 6, np.where(mx == g, (b - r) / d + 2, (r - g) / d + 4)) * 60
    sat = (s > 0.5) & (v > 0.3)
    cyan = (hue >= 170) & (hue <= 200) & (s > 0.35) & (v > 0.35)
    bins = np.histogram(hue[sat], bins=12, range=(0, 360))[0]
    spread = int((bins >= 0.03 * max(1, sat.sum())).sum())
    print(f"{label}: V {v.mean():.2f} | dark {100*(v<0.25).mean():.0f} % mid {100*((v>=0.25)&(v<=0.75)).mean():.0f} % bright {100*(v>0.75).mean():.0f} % | S {s.mean():.2f} saturated {100*sat.mean():.0f} % | cyan {100*cyan.mean():.1f} % | hue bins {spread}")
