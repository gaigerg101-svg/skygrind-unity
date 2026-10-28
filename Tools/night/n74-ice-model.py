"""NIGHT74 builder L (leftover 4): the offline model for ice74's stronger lit72 in THE ICEBREAKER's arena, run on the boss63 frames of the n74-t4 bi0 side (ice74 OFF, lit72 ON at A 0.12).
A model, not a Player frame: the lead's ON / OFF pair (Tools/night/n74-ice.py) is the measure. Unlike crown74's and the ambient-probe version's models, this one is exact in its mechanism: it only
re-runs lit72's own curve (Crt29.shader's Lit72 block) with other numbers, and lit72's shipped numbers reproduce the frames (bi0 -> bi: 48 % -> 20 % dark in the Player).
Per frame (display luma 0-1 of the 8-bit bytes, weights 0.299 / 0.587 / 0.114):
  1. the frame carries lit72's lift (f = lit72(p) at A 0.12, l0 0.12, l1 0.42, gain cap 9: UnityTuning.N72L.cs); p, the luma before it, is recovered by inverting that monotone curve;
     THE ICEBREAKER's walls are p = 0.010 (2.2, 2.2, 5.6 of 255): the cap, x9, is why they read 22 and not more;
  2. lit72 is applied again to p with the arena's numbers (A, l1, gain cap): f2 = min(1, p x min(cap, (p + A w(p)) / p)), w = 1 - smoothstep(l0, l1, p);
  3. the dark (< 30) share of the whole frame, the frame's mean luma, and how far the BRIGHT pixels (over 115) moved (the floor must not wash).
  python -I -B Tools/night/n74-ice-model.py [<frames dir>] [A:l1:cap ...]      (default frames: the n74-t4 bi0 side in the main tree, read only; default settings: the shipped 0.12:0.42:9 and some larger)
"""
import os
import sys

import numpy as np
from PIL import Image

DEFAULT_DIR = "C:/Users/gaige/dev/SKYGRIND-unity/Logs/feel/n74-t4/bi0/boss63"
A0, L0, L1_0, K0 = 0.12, 0.12, 0.42, 9.0   # lit72's shipped numbers (UnityTuning.N72L.cs)
NAMES = ("idle", "windup", "stagger", "flinch")


def smooth(e0, e1, x):
    t = np.clip((x - e0) / (e1 - e0), 0, 1)
    return t * t * (3 - 2 * t)


def lit72(p, a, l0, l1, cap):
    w = 1 - smooth(l0, l1, p)
    k = np.minimum(cap, (p + a * w) / np.maximum(p, 1e-4))
    return np.minimum(1.0, p * k)


PS = np.linspace(0, 1, 8001)
FS = lit72(PS, A0, L0, L1_0, K0)


def inverse(f):
    return np.interp(f, FS, PS)


def main(argv):
    d = DEFAULT_DIR
    settings = []
    for a in argv:
        if os.path.isdir(a):
            d = a
        else:
            x, y, z = a.split(":")
            settings.append((float(x), float(y), float(z)))
    settings = settings or [(0.12, 0.42, 9.0), (0.12, 0.42, 20.0), (0.16, 0.42, 20.0), (0.16, 0.50, 24.0), (0.20, 0.50, 30.0)]
    for a, l1, cap in settings:
        print("arena lit72: A %.2f, l0 %.2f, l1 %.2f, gain cap %.0f" % (a, L0, l1, cap))
        for name in NAMES:
            im = np.asarray(Image.open(os.path.join(d, "shot-N30-B63-brawl40-%s.png" % name)).convert("RGB"), dtype=np.float32) / 255.0
            f = 0.299 * im[..., 0] + 0.587 * im[..., 1] + 0.114 * im[..., 2]
            f2 = lit72(inverse(f), a, L0, l1, cap)
            bright = f > 0.45
            print("  %-8s dark<30 whole %.1f %% -> %.1f %% | mean luma %.1f -> %.1f | pixels over 115 move %+.1f" % (
                name, 100 * (f * 255 < 30).mean(), 100 * (f2 * 255 < 30).mean(), f.mean() * 255, f2.mean() * 255, (f2[bright] - f[bright]).mean() * 255))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
