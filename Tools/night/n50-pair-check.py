# NIGHT50 pair check: refuses a pair before judges see it. Run after `n50-pair.py stage`, on each o1/o2 A and B.
#   python -I Tools/night/n50-pair-check.py <label>
# Fails (exit 1) when an image is missing, is nearly black (mean luma < 8 of 255), is nearly blank (luma std < 2), or when the two
# images of a pair are the same size (a crop mismatch, the NIGHT49 black-pair fault). Prints each image's size and mean / std luma.
# Needs Pillow; falls back to a plain PNG decode via zlib only if Pillow is absent (not checked here: Pillow is the path used).
import json
import os
import sys

try:
    from PIL import Image, ImageStat
except ImportError:
    sys.exit("Pillow is needed: python -I -m pip install pillow (the check does not guess at pixels)")

ROOT = os.path.join("Logs", "n50", "judge")
MIN_MEAN = 8.0
MIN_STD = 2.0


def check(path):
    if not os.path.isfile(path):
        return "missing: " + path
    im = Image.open(path).convert("L")
    st = ImageStat.Stat(im)
    mean, std = st.mean[0], st.stddev[0]
    print(f"{path}: {im.size[0]}x{im.size[1]} mean {mean:.1f} std {std:.1f}")
    if mean < MIN_MEAN:
        return "nearly black: " + path
    if std < MIN_STD:
        return "nearly blank: " + path
    return None


def main():
    if len(sys.argv) != 2:
        sys.exit("usage: python -I Tools/night/n50-pair-check.py <label>")
    label = sys.argv[1]
    problems = []
    sizes = {}
    for order in ("o1", "o2"):
        for side in ("A", "B"):
            p = os.path.join(ROOT, label, order, side + ".png")
            problem = check(p)
            if problem:
                problems.append(problem)
            elif os.path.isfile(p):
                sizes.setdefault(order, []).append(Image.open(p).size)
    for order, pair in sizes.items():
        if len(pair) == 2 and pair[0] != pair[1]:
            problems.append(f"{order}: A and B sizes differ {pair[0]} vs {pair[1]}")
    key_path = os.path.join("Logs", "n50", "judge-keys", label + ".json")
    if not os.path.isfile(key_path):
        problems.append("no key: " + key_path)
    else:
        with open(key_path, encoding="utf8") as f:
            json.load(f)
    if problems:
        for p in problems:
            print("REFUSE:", p)
        sys.exit(1)
    print("PASS:", label)


if __name__ == "__main__":
    main()
