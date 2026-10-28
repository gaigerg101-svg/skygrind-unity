"""NIGHT74 builder L (L3): the offline model that sized crown74's fill (UnityTuning.N74CrownFill*), run on the n73 freeze build's THE CROWN frames.
A model, not a Player frame: the lead's ON / OFF pair of the n74 build (Tools/night/n74-crown.py) is the measure.
What it does, per frame (display luma 0-1 of the 8-bit bytes, lit72's weights 0.299 / 0.587 / 0.114):
  1. the frame already carries lit72's display lift (f = lit72(p), p the luma before it): p is recovered by inverting lit72's monotone curve with its own numbers
     (A 0.12, l0 0.12, l1 0.42, gain cap 9: UnityTuning.N72L.cs);
  2. crown74's fill (Ceiling27.shader: the display-space add of FILL x weight, weight = 1 - smoothstep(L0, L1, p)) is added to p, lit72 is applied again;
  3. the region 900-1300 x 150-450's mean luma before and after, and its share under 30.
It treats EVERY pixel of the region as ceiling (the shader only touches the ceiling, and the region is mostly ceiling plus the ring's sparkles: the windup frame is the least sure).
  python -I Tools/night/n74-crown-model.py [<frames dir>] [scale ...]
The default frames dir is the n73 freeze's bp / boss63 folder of the main tree (read only); the default scales 1.0 1.25 1.5 2.0 (1.25 is the shipped fill: UnityTuning.N74CrownFill* = 0.12 / 0.125 / 0.20 x 1.25).
"""
import os
import sys

import numpy as np
from PIL import Image

DEFAULT_DIR = "C:/Users/gaige/dev/SKYGRIND-unity/Logs/feel/n73-freeze/bp/boss63"
FILL = np.array([0.12, 0.125, 0.20])   # the unscaled fill; UnityTuning.N74CrownFill* is this x 1.25
L0, L1 = 0.20, 0.60                    # UnityTuning.N74CrownL0 / L1
A, LL0, LL1, KMAX = 0.12, 0.12, 0.42, 9.0   # lit72's numbers (UnityTuning.N72L.cs: L72LitA, L72LitL0, L72LitL1, L72LitKMax)
NAMES = ("idle", "flinch", "windup", "tell", "stagger")


def smooth(e0, e1, x):
    t = np.clip((x - e0) / (e1 - e0), 0, 1)
    return t * t * (3 - 2 * t)


def lit72(p):
    w = 1 - smooth(LL0, LL1, p)
    k = np.minimum(KMAX, (p + A * w) / np.maximum(p, 1e-4))
    return np.minimum(1.0, p * k)


PS = np.linspace(0, 1, 4001)
FS = lit72(PS)


def inverse(f):
    return np.interp(f, FS, PS)


def main(argv):
    d = DEFAULT_DIR
    scales = []
    for a in argv:
        if os.path.isdir(a):
            d = a
        else:
            scales.append(float(a))
    scales = scales or [1.0, 1.25, 1.5, 2.0]
    for scale in scales:
        print("fill scale %.2f (fill %.3f / %.3f / %.3f)" % (scale, *(FILL * scale)))
        worst = 1e9
        for name in NAMES:
            im = np.asarray(Image.open(os.path.join(d, "shot-N30-B63-crown37-%s.png" % name)).convert("RGB"), dtype=np.float32) / 255.0
            f = 0.299 * im[..., 0] + 0.587 * im[..., 1] + 0.114 * im[..., 2]
            p = inverse(f)
            w = 1 - smooth(L0, L1, p)
            fill_l = float(np.dot([0.299, 0.587, 0.114], FILL * scale))
            f2 = lit72(np.clip(p + fill_l * w, 0, 1))
            r0, r1 = f[150:450, 900:1300] * 255, f2[150:450, 900:1300] * 255
            delta = r1.mean() - r0.mean()
            worst = min(worst, delta)
            print("  %-8s region mean luma %.1f -> %.1f (%+.1f) | region dark<30 %.1f %% -> %.1f %%" % (name, r0.mean(), r1.mean(), delta, 100 * (r0 < 30).mean(), 100 * (r1 < 30).mean()))
        print("  worst %+.1f (the bar: +25)" % worst)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
