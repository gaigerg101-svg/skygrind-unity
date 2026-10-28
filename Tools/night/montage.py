# NIGHT30 (the lead): tile Player frames into one labelled image so a batch of frames can be looked at at once.
#   python Tools/night/montage.py <out.png> <cols> <img> [<img> ...]
import sys, os
from PIL import Image, ImageDraw

out, cols, paths = sys.argv[1], int(sys.argv[2]), sys.argv[3:]
W, H = 800, 450
rows = (len(paths) + cols - 1) // cols
sheet = Image.new("RGB", (cols * W, rows * (H + 24)), (16, 16, 20))
d = ImageDraw.Draw(sheet)
for i, p in enumerate(paths):
    x, y = (i % cols) * W, (i // cols) * (H + 24)
    try:
        im = Image.open(p).convert("RGB").resize((W, H))
        sheet.paste(im, (x, y + 24))
    except Exception as e:
        d.text((x + 8, y + 40), "missing: " + str(e)[:80], fill=(255, 80, 80))
    d.text((x + 8, y + 6), os.path.basename(p), fill=(230, 230, 230))
sheet.save(out)
print("montage", out, len(paths))
