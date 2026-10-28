#!/usr/bin/env python3
# NIGHT73 (NIGHT72-REPORT section 7: "a motion instrument the spotter can judge (a short clip strip), so spring72, cam72 and finish55 can pass
# on what a player sees"). A still cannot show motion; a strip of evenly spaced frames from one reel window can. A reel side keeps every rendered
# frame (60 fps) until the prune, so the strip is cut from an unpruned side (n73-reels.sh --keep).
#   python -I Tools/night/n73-strip.py peek <scenario dir> [every]      per-frame camera lines from telemetry.jsonl (fov, roll, height, speed, fired),
#                                                                      to choose a window; `every` (default 5) frames apart
#   python -I Tools/night/n73-strip.py strip <out.png> <scenario dir> <f0> <f1> [n]
#                                                                      n (default 6) frames evenly spaced over f0..f1 (inclusive), each scaled to
#                                                                      640 x 360, tiled 3 wide in reading order, no labels (the spotter is blind);
#                                                                      prints the frames used. Refuses a missing frame (a pruned side).
import json, os, sys

def peek(d, every):
    tel = os.path.join(d, 'telemetry.jsonl')
    with open(tel, encoding='utf8') as fh:
        for line in fh:
            o = json.loads(line)
            if o.get('type') != 'frame' or o['f'] % every:
                continue
            c, v = o.get('cam') or {}, o.get('vel') or {}
            have = os.path.exists(os.path.join(d, 'f%04d.jpg' % o['f']))
            print('f%04d t %.3f fov %.2f roll %.2f h %.3f speed %.2f fired %s enemies %d %s' % (
                o['f'], o.get('t', 0), c.get('fov', 0), c.get('roll', 0), c.get('height', 0), v.get('speed', 0),
                o.get('fired'), len(o.get('enemies') or []), '' if have else '(no jpg)'))

def strip(out, d, f0, f1, n):
    from PIL import Image
    if n < 2 or f1 <= f0:
        sys.exit('n73-strip: need n >= 2 and f1 > f0')
    frames = sorted(set(round(f0 + (f1 - f0) * i / (n - 1)) for i in range(n)))
    paths = [os.path.join(d, 'f%04d.jpg' % f) for f in frames]
    missing = [p for p in paths if not os.path.exists(p)]
    if missing:
        sys.exit('n73-strip: missing ' + ', '.join(missing) + ' (a pruned side? re-run it with n73-reels.sh --keep)')
    cols, w, h = 3, 640, 360
    rows = (len(paths) + cols - 1) // cols
    sheet = Image.new('RGB', (cols * w, rows * h), (0, 0, 0))
    for i, p in enumerate(paths):
        im = Image.open(p).convert('RGB').resize((w, h), Image.LANCZOS)
        sheet.paste(im, ((i % cols) * w, (i // cols) * h))
    os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
    sheet.save(out)
    print('n73-strip: %s <- %s frames %s' % (out, d, ' '.join('f%04d' % f for f in frames)))

if __name__ == '__main__':
    a = sys.argv[1:]
    if a[:1] == ['peek'] and len(a) >= 2:
        peek(a[1], int(a[2]) if len(a) > 2 else 5)
    elif a[:1] == ['strip'] and len(a) >= 5:
        strip(a[1], a[2], int(a[3]), int(a[4]), int(a[5]) if len(a) > 5 else 6)
    elif a[:1] == ['text'] and len(a) >= 3:
        # text <scenario dir> <substring>: the frames whose HUD telemetry (ui[].text) holds the substring: first, last and count
        hits = []
        with open(os.path.join(a[1], 'telemetry.jsonl'), encoding='utf8') as fh:
            for line in fh:
                o = json.loads(line)
                if o.get('type') == 'frame' and any(a[2] in (u.get('text') or '') for u in (o.get('ui') or [])):
                    hits.append(o['f'])
        print('n73-strip text %r: %d frames%s' % (a[2], len(hits), (' f%04d..f%04d' % (hits[0], hits[-1])) if hits else ''))
    elif a[:1] == ['sheet'] and len(a) >= 3:
        # sheet <out.png> <image> [<image> ...]: any frames tiled 3 wide at 640 x 360 (the lead's look at a set of shots)
        from PIL import Image
        ims = a[2:]
        sheet = Image.new('RGB', (3 * 640, ((len(ims) + 2) // 3) * 360))
        for i, p in enumerate(ims):
            sheet.paste(Image.open(p).convert('RGB').resize((640, 360), Image.LANCZOS), ((i % 3) * 640, (i // 3) * 360))
        sheet.save(a[1])
        print('n73-strip sheet: %s <- %d images' % (a[1], len(ims)))
    else:
        sys.exit(__doc__ or 'usage: n73-strip.py peek <dir> [every] | strip <out.png> <dir> <f0> <f1> [n]')
