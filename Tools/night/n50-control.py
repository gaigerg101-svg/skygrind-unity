# NIGHT50 P0 control pairs (the lead's). Stages three calibration pairs from NIGHT49's arc40 frame with n50-pair.py, so a judge
# must call them: c0 same frame twice -> TIE; c1 arc40 moved 6 px -> TIE; c2 the sharp arc40 against a blurred copy -> the sharp one wins.
#   python -I Tools/night/n50-control.py
# Source frame: NIGHT49 r2 h1080 arc40 (Logs/feel/n49-r2/h1080/a149). Shifted and blurred copies are written to Logs/n50/control/.
import os
import subprocess
import sys

from PIL import Image, ImageFilter

SRC = os.path.join("Logs", "feel", "n49-r2", "h1080", "a149", "shot-N30-H49-pair-arc40-50-1920x1080.png")
CTL = os.path.join("Logs", "n50", "control")
PAIR = os.path.join("Tools", "night", "n50-pair.py")


def main():
    if not os.path.isfile(SRC):
        sys.exit("missing source frame: " + SRC)
    os.makedirs(CTL, exist_ok=True)
    src = Image.open(SRC).convert("RGB")
    w, h = src.size
    # c1: the same frame moved 6 px right (the bar's nearest pixel moves; the rest of the frame is the same)
    shifted = Image.new("RGB", (w, h))
    shifted.paste(src, (6, 0))
    shifted_path = os.path.join(CTL, "arc40-shift6.png")
    shifted.save(shifted_path)
    # c2: the sharp frame against a copy blurred to hide its detail (a plainly worse readability frame)
    blurred_path = os.path.join(CTL, "arc40-blur.png")
    src.filter(ImageFilter.GaussianBlur(radius=4)).save(blurred_path)
    for label, new in (("cal-1", SRC), ("cal-2", shifted_path), ("cal-3", blurred_path)):
        subprocess.run([sys.executable, "-I", PAIR, "stage", label, SRC, new], check=True)
        subprocess.run([sys.executable, "-I", os.path.join("Tools", "night", "n50-pair-check.py"), label], check=False)


if __name__ == "__main__":
    main()
