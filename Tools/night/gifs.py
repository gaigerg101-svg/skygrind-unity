# NIGHT20 close: before / after GIFs for NIGHT20-REPORT (sprint, shockwave, zoom, climb): A = Logs/feel/A20/on (NIGHT19's
# game), B = Logs/feel/final20/on (the final build), side by side, 960 px wide each half, at most 60 frames (evenly picked),
# about 33 ms x the stride per GIF frame. Writes Reports/n20-gifs/<scenario>.gif.
import os, sys
from PIL import Image, ImageDraw
R = 'C:/Users/gaige/dev/SKYGRIND-unity/'
out = R + 'Reports/n20-gifs'
os.makedirs(out, exist_ok=True)
for s in ('sprint', 'shockwave', 'zoom', 'climb'):
    a = sorted(f for f in os.listdir(R + 'Logs/feel/A20/on/' + s) if f.endswith('.jpg'))
    b = sorted(f for f in os.listdir(R + 'Logs/feel/final20/on/' + s) if f.endswith('.jpg'))
    n = min(len(a), len(b))
    stride = max(1, n // 60)
    frames = []
    for i in range(0, n, stride):
        fa = Image.open(R + 'Logs/feel/A20/on/' + s + '/' + a[i]).convert('RGB').resize((640, 360))
        fb = Image.open(R + 'Logs/feel/final20/on/' + s + '/' + b[i]).convert('RGB').resize((640, 360))
        im = Image.new('RGB', (1280, 380), (12, 10, 20))
        im.paste(fa, (0, 20)); im.paste(fb, (640, 20))
        d = ImageDraw.Draw(im)
        d.text((8, 4), 'A: NIGHT19 (-skygrindOff20)', fill=(220, 220, 230))
        d.text((648, 4), 'B: NIGHT20 final', fill=(120, 230, 255))
        frames.append(im.quantize(colors=128))
    frames[0].save(out + '/' + s + '.gif', save_all=True, append_images=frames[1:], duration=int(1000 / 120 * stride * 4), loop=0)
    print(s, len(frames), 'frames ->', out + '/' + s + '.gif', os.path.getsize(out + '/' + s + '.gif') // 1024, 'KB')
