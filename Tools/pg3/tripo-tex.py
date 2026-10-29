# PG3 (GG 2026-10-28: "guns and weapons look very desaturated"): grades the Tripo base-colour textures and makes an emission map per model.
# Reads the untouched originals in Inbox/tripo/<id>/*.fbm/<id>_basecolor.JPEG (never stacks), writes Assets/Resources/Tripo/Textures/<id>_basecolor.JPEG
# (saturation x SAT, contrast x CON, brightness x BRI) and <id>_emission.PNG (the accent pixels: saturated and bright cyan for player things, amber / orange
# for enemy eyes; black elsewhere), which TripoModels.Glow puts on the model's material.
# Usage: python -I Tools/pg3/tripo-tex.py
import colorsys, glob, os
from PIL import Image, ImageEnhance

SAT, CON, BRI = 1.7, 1.15, 1.08
# cyan glows only on the player's things (CLAUDE.md rule 8: cyan is player-usable; rule W: cyan on a boss means "shoot here"): an enemy's or a boss's teal / ice never glows
PLAYER = {'kestrel', 'kestrel-cylinder', 'thunderclap', 'thunderclap-pump', 'gauntlet', 'torch', 'stitcher', 'comet', 'longwave'}
root = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
out = os.path.join(root, 'Assets', 'Resources', 'Tripo', 'Textures')
for src in sorted(glob.glob(os.path.join(root, 'Inbox', 'tripo', '*', '*.fbm', '*_basecolor.JPEG'))):
    mid = os.path.basename(src)[:-len('_basecolor.JPEG')]
    im = Image.open(src).convert('RGB')
    g = ImageEnhance.Brightness(ImageEnhance.Contrast(ImageEnhance.Color(im).enhance(SAT)).enhance(CON)).enhance(BRI)
    g.save(os.path.join(out, mid + '_basecolor.JPEG'), quality=92)
    small = im.resize((512, 512))
    em = Image.new('RGB', small.size)
    sp, ep = small.load(), em.load()
    lit = 0
    for y in range(small.size[1]):
        for x in range(small.size[0]):
            r, gg, b = sp[x, y]
            h, s, v = colorsys.rgb_to_hsv(r / 255, gg / 255, b / 255)
            hue = h * 360
            cyan = mid in PLAYER and 165 <= hue <= 205 and s > 0.45 and v > 0.55
            amber = (15 <= hue <= 50) and s > 0.6 and v > 0.75
            if cyan or amber:
                ep[x, y] = (r, gg, b)
                lit += 1
    em = em.resize(im.size, Image.BILINEAR)
    em.save(os.path.join(out, mid + '_emission.PNG'))
    print(mid, 'emission pixels %.1f%%' % (100.0 * lit / (512 * 512)))
