#!/usr/bin/env python3
# NIGHT73 builder B: where to cut the spring73 strips (the reel FeelReel.N73.B.cs; the lead's n73-strip.py cuts evenly spaced frames, but a flinch lasts 0.15 s, so the frames must be the
# ones AFTER the hits, not evenly spaced in time). Reads one side's scenario dir: telemetry.jsonl (the frames that carry the `hit` / `kill` events: the telemetry's own truth) and, when it is
# there, spring73.txt (the reel's own rows: the frames of the wind-up, matched to the telemetry by the shots' offset).
#   python -I Tools/night/n73-springcut.py events <spring73 dir>        the hit / kill frames, the wind-up frames, and the sheet commands to run (flinch peak, flinch rebound, wind-up, death)
#   python -I Tools/night/n73-springcut.py same <dir A> <dir B>         whether two sides (ON and OFF) have the same event frames (the sim is deterministic; the row is render only)
#   python -I Tools/night/n73-springcut.py crop <out.png> <dir> <f> ... each frame cropped round the husk (its telemetry rect, a box 1.7 x the median rect height, portrait) and tiled 3 wide at
#                                                                      420 x 560, no labels (the spotter is blind): a husk 5 m away is 86 px high in a 640 x 360 strip tile and 17 % of it is 15 px;
#                                                                      cropped it is 330 px and the squash is 55 px. Give the same frame numbers for the ON and the OFF side.
import json, os, re, sys

def telemetry(d):
    frames = {}
    with open(os.path.join(d, 'telemetry.jsonl'), encoding='utf8') as fh:
        for line in fh:
            o = json.loads(line)
            if o.get('type') == 'frame':
                frames[o['f']] = o
    return frames

def event_frames(frames, name):
    return sorted(f for f, o in frames.items() if name in (o.get('events') or []))

def spring_rows(d):
    p = os.path.join(d, 'spring73.txt')
    if not os.path.exists(p):
        return None, []
    lines = open(p, encoding='utf8').read().splitlines()
    # the reel's line (after the n73-d fix): "... non-lethal (at reel frames n a,b,..; telemetry frames f c,d,..; gaps ...) + the kill (at n X, f Y) ..."; the first version's "shots at reel frames n" is read too
    shots = re.search(r'(?:shots at reel frames n|at reel frames n) ([0-9,]+)', lines[0]) if lines else None
    kill = re.search(r'the kill \(at n ([0-9]+), f ([0-9]+)\)', lines[0]) if lines else None
    ns = [int(x) for x in shots.group(1).split(',') if x] if shots else None
    if ns is not None and kill:
        ns.append(int(kill.group(1)))  # the kill's own frame, last: pairs with the telemetry's last event frame
    rows = []
    for ln in lines[3:]:
        a = ln.split()
        if len(a) >= 9:
            rows.append({'n': int(a[0]), 'windSteps': int(a[2]), 'flinch': float(a[5]), 'wind': float(a[6])})
    return ns, rows

def events(d):
    frames = telemetry(d)
    hits = event_frames(frames, 'hit')
    kills = event_frames(frames, 'kill')
    print('n73-springcut %s: %d frames, hit events at %s, kill events at %s (a killing shot carries `kill`, not `hit`)' % (d, len(frames), hits, kills))
    if len(hits) < 2:
        print('  too few hit frames to cut')
        return
    nl = [h for h in hits if h not in kills]
    gaps = [b - a for a, b in zip(nl, nl[1:])]
    print('  non-lethal hit frames %s, gaps %s (the reel aims at 30)' % (nl, gaps))
    shots, rows = spring_rows(d)
    wind = None
    allshots = sorted(set(hits) | set(kills))
    if shots and rows and len(shots) <= len(allshots):
        offs = [h - n for h, n in zip(allshots, shots)]
        off = sorted(offs)[len(offs) // 2]
        ws = [r['n'] + off for r in rows if r['windSteps'] > 0]
        if ws:
            wind = (min(ws), max(ws))
        print('  the reel frame n -> the telemetry frame f: f = n + %d (median over the shots; per shot %s)' % (off, offs))
    peak = [f + 1 for f in nl]
    reb = [f + 5 for f in nl]
    rest = [max(0, nl[0] - 3)]
    print('  flinch peak frames (hit + 1: the squash at 17 %% in the mirror) %s' % peak)
    print('  flinch rebound frames (hit + 5: the 5 %% stretch) %s' % reb)
    print('  python -I Tools/night/n73-strip.py sheet <out>-peak.png %s' % ' '.join(os.path.join(d, 'f%04d.jpg' % f) for f in rest + peak[:5]))
    print('  python -I Tools/night/n73-strip.py sheet <out>-rebound.png %s' % ' '.join(os.path.join(d, 'f%04d.jpg' % f) for f in rest + reb[:5]))
    print('  python -I Tools/night/n73-springcut.py crop <out>-peak-crop.png %s %s   (the same numbers for the other side)' % (d, ' '.join(str(f) for f in rest + peak[:5])))
    if wind:
        k = kills[0] if kills else wind[1]
        pts = sorted(set(round(wind[0] + (k - wind[0]) * i / 5) for i in range(6)))
        print('  wind-up frames f%04d..f%04d, the kill at f%04d: python -I Tools/night/n73-strip.py strip <out>-wind.png %s %d %d 6' % (wind[0], wind[1], k, d, wind[0], k))
        print('  the same frames cropped: python -I Tools/night/n73-springcut.py crop <out>-wind-crop.png %s %s' % (d, ' '.join(str(f) for f in pts)))
    if kills:
        print('  death: python -I Tools/night/n73-strip.py strip <out>-death.png %s %d %d 6' % (d, kills[0], kills[0] + 30))

def same(a, b):
    ea, eb = telemetry(a), telemetry(b)
    ok = True
    for name in ('hit', 'kill', 'shot'):
        fa, fb = event_frames(ea, name), event_frames(eb, name)
        print('  %s frames: %s | %s | %s' % (name, fa, fb, 'EQUAL' if fa == fb else 'DIFFERENT'))
        ok = ok and fa == fb
    print('n73-springcut same: %s' % ('EQUAL (cut the same frame numbers from both)' if ok else 'DIFFERENT (the sides ran different frames: cut each side by its own events)'))

def crop(out, d, fs):
    from PIL import Image
    frames = telemetry(d)
    rects = []
    for f in fs:
        es = (frames.get(f) or {}).get('enemies') or []
        if es:
            rects.append(es[0]['rect'])
    if not rects:
        sys.exit('n73-springcut crop: no enemy rect in frames %s' % fs)
    hs = sorted(r[3] for r in rects)
    boxh = max(200.0, 1.7 * hs[len(hs) // 2])
    boxw = boxh * 0.75
    tw, th, cols = 420, 560, 3
    rows = (len(fs) + cols - 1) // cols
    sheet = Image.new('RGB', (cols * tw, rows * th), (0, 0, 0))
    used = []
    for i, f in enumerate(fs):
        p = os.path.join(d, 'f%04d.jpg' % f)
        if not os.path.exists(p):
            sys.exit('n73-springcut crop: missing %s (a pruned side?)' % p)
        im = Image.open(p).convert('RGB')
        es = (frames.get(f) or {}).get('enemies') or []
        r = es[0]['rect'] if es else rects[-1]
        cx, cy = r[0] + r[2] / 2.0, r[1] + r[3] / 2.0
        x0 = max(0, min(im.width - boxw, cx - boxw / 2.0))
        y0 = max(0, min(im.height - boxh, cy - boxh / 2.0))
        tile = im.crop((int(x0), int(y0), int(x0 + boxw), int(y0 + boxh))).resize((tw, th), Image.LANCZOS)
        sheet.paste(tile, ((i % cols) * tw, (i // cols) * th))
        used.append('f%04d' % f)
    os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
    sheet.save(out)
    print('n73-springcut crop: %s <- %s frames %s, box %d x %d px of the frame' % (out, d, ' '.join(used), boxw, boxh))

if __name__ == '__main__':
    a = sys.argv[1:]
    if a[:1] == ['events'] and len(a) >= 2:
        events(a[1])
    elif a[:1] == ['same'] and len(a) >= 3:
        same(a[1], a[2])
    elif a[:1] == ['crop'] and len(a) >= 4:
        crop(a[1], a[2], [int(x) for x in a[3:]])
    else:
        sys.exit(__doc__)
