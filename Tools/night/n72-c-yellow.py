# NIGHT72 builder C (slate C4, yellow56): the share of a frame's pixels (outside the HUD corners) in the parry yellow's arc: hue 45-70, S > 0.5, V >= 0.15 (n56-colour.py's rule),
# per tour65 frame. Yellow means parry only (CLAUDE.md rule 8, DECIDE63 D-1): what remains of it on a world frame is yellow56's job (the floors' tan, the trims' brass, the TORCH's plate);
# enemy and boss parts in yellow are GG's design and are in the count (a frame with a husk's parry halo reads higher: look at the frame before reading the number).
#   python -I Tools/night/n72-c-yellow.py <folder or image> [...]        e.g. Logs/feel/n71-forge72/tf/tour65
import glob
import importlib.util
import os
import sys

sys.dont_write_bytecode = True
spec = importlib.util.spec_from_file_location("n72v", os.path.join(os.path.dirname(os.path.abspath(__file__)), "n72-c-values.py"))
v = importlib.util.module_from_spec(spec)
spec.loader.exec_module(v)


def share(path):
    a = v.load(path)
    ok = v.hud_mask(a.shape)
    h, s, val = v.hsv(a)
    y = ok & (h >= 45) & (h <= 70) & (s > 0.5) & (val >= 0.15)
    return 100.0 * y.sum() / ok.sum()


def main(args):
    for arg in args:
        files = v.frames_of(arg)
        out = [(os.path.basename(f)[:-4].replace("shot-N29-T65-", ""), share(f)) for f in files]
        print(arg, " | ".join("%s %.2f%%" % o for o in out), "| over 0.5 %%: %d of %d" % (sum(1 for _, p in out if p > 0.5), len(out)))
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
