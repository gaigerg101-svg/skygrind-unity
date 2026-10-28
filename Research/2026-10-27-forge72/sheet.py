# FORGE72: tile reel shots into one labelled contact sheet (3 columns), and print per-shot luma numbers.
#   python -I Research/2026-10-27-forge72/sheet.py <out.jpg> <shot.png> ...
# Per shot: mean luma (0-255, Rec.709 on the sRGB values), the share of pixels under 30 (dark), and over 235 (blown).
import sys
from PIL import Image, ImageDraw

out, paths = sys.argv[1], sys.argv[2:]
W, H, C = 640, 360, 3
rows = (len(paths) + C - 1) // C
sheet = Image.new("RGB", (W * C, H * rows), (0, 0, 0))
d = ImageDraw.Draw(sheet)
for i, p in enumerate(paths):
    im = Image.open(p).convert("RGB")
    g = im.convert("L")
    hist = g.histogram()
    n = sum(hist)
    mean = sum(v * c for v, c in enumerate(hist)) / n
    dark = sum(hist[:30]) / n
    blown = sum(hist[236:]) / n
    name = p.replace("\\", "/").split("/")[-1].replace("shot-N29-T65-", "").replace(".png", "")
    print(f"{name}\tmean {mean:5.1f}\tdark<30 {dark*100:5.1f}%\tblown>235 {blown*100:4.1f}%")
    t = im.resize((W, H))
    x, y = (i % C) * W, (i // C) * H
    sheet.paste(t, (x, y))
    d.rectangle([x, y, x + 300, y + 18], fill=(0, 0, 0))
    d.text((x + 4, y + 3), f"{name} m{mean:.0f} d{dark*100:.0f}%", fill=(255, 255, 255))
sheet.save(out, quality=88)
