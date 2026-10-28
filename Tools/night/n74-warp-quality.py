"""NIGHT74 builder L (L1 / L4): how well pan74's motion compensation worked, by screen region, on a Player run's .p74 files.
The Player logs one calibration line per sweep (`compensation: ... mean |residual| X levels against the plain frame difference Y (compensated | WEAK)`); that figure is taken over the WHOLE
picture, which includes the screen-fixed HUD and gun (the warp moves them, so they read as residual: a "WEAK" can be those, not the world). This prints, per sweep and per region, the mean |plain
block difference| and the mean |residual| (levels) and their ratio: a world region well compensated reads a ratio well under 1.
  python -I -B Tools/night/n74-warp-quality.py <dir | file.p74>...         (-B: no .pyc beside the tool)
Regions (1920 x 1080 px): world core (y 160-640, x 240-1520: no HUD counter, no gun), ceiling band (y 0-250, x 600-1700), gun + HUD lower right (y 700-1080, x 1100-1920), the whole frame.
"""
import importlib.util
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n74flick", os.path.join(HERE, "n74-flick.py"))
flick = importlib.util.module_from_spec(spec)
spec.loader.exec_module(flick)

REGIONS = (("world core", (160, 640, 240, 1520)), ("ceiling band", (0, 250, 600, 1700)), ("gun + HUD lower right", (700, 1080, 1100, 1920)), ("whole", (0, 1080, 0, 1920)))


def main(argv):
    files = flick.files_of(argv)
    if not files:
        print("n74-warp-quality: no .p74 files in " + " ".join(argv))
        return 2
    for f in files:
        bw, bh, block, means, resid, _partial = flick.read_stack(f)
        plain = np.abs(np.diff(means, axis=0))
        r = np.abs(resid[1:])
        sx = bw * block / 1920.0
        name = os.path.basename(f)[:-4]
        for label, (y0, y1, x0, x1) in REGIONS:
            sl = (slice(None), slice(int(y0 * sx) // block, int(y1 * sx) // block), slice(int(x0 * sx) // block, int(x1 * sx) // block))
            p, q = plain[sl], r[sl]
            print("%s %-22s mean |dY| %.3f  mean |r| %.3f  ratio r/dY %.2f | p95 |dY| %.2f p95 |r| %.2f" % (name, label, p.mean(), q.mean(), q.mean() / max(p.mean(), 1e-9), np.percentile(p, 95), np.percentile(q, 95)))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
