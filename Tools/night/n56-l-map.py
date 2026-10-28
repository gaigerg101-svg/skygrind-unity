# NIGHT56 builder L: a top-down text map of one room from its plan (1 m cells, north = -z up the page, x to the right), for placing pieces.
#   R ride wall   # a box >= 0.5 m (its top's metres as a digit 1-9, '+' for 10 m and over)   / a ramp   ~ lava pit   _ dry pit
#   P a pad's disc   o a wave-1 spot (waveNodes43[0])   w a walk proof's point   D a door   . floor
#   python -I Tools/night/n56-l-map.py <level> <room>
import json, math, os, sys

lv, rid = sys.argv[1], sys.argv[2]
p = json.load(open(os.path.join('Design', 'levels', 'n52', lv + '.plan.json'), encoding='utf-8'))
room = next(r for r in p['rooms'] if r['id'] == rid)
x0, z0, x1, z1 = room['rect']
W, H = int(x1 - x0), int(z1 - z0)
g = [['.'] * W for _ in range(H)]


def put(x, z, ch, over=True):
    i, j = int(math.floor(x - x0)), int(math.floor(z - z0))
    if 0 <= i < W and 0 <= j < H and (over or g[j][i] == '.'):
        g[j][i] = ch


for q in p.get('pits', []):
    if q.get('zone') != rid:
        continue
    a, b, c, d = q['rect']
    for j in range(H):
        for i in range(W):
            x, z = x0 + i + 0.5, z0 + j + 0.5
            if a <= x <= c and b <= z <= d:
                g[j][i] = '~' if q.get('lava') else '_'
for b in p.get('boxes', []):
    if b.get('zone') != rid or b['max'][1] < 0.5:
        continue
    mn, mx = b['min'], b['max']
    top = mx[1]
    ch = 'R' if b.get('ride') else ('+' if top >= 10 else str(max(1, int(round(top)))))
    for j in range(H):
        for i in range(W):
            x, z = x0 + i + 0.5, z0 + j + 0.5
            if mn[0] <= x <= mx[0] and mn[2] <= z <= mx[2]:
                g[j][i] = ch
for r in p.get('ramps', []):
    if r.get('zone') != rid:
        continue
    fx, fz = r['from']; tx, tz = r['to']; hw = r.get('width', 2) / 2.0
    L = math.hypot(tx - fx, tz - fz) or 1e-9
    ux, uz = (tx - fx) / L, (tz - fz) / L
    for j in range(H):
        for i in range(W):
            x, z = x0 + i + 0.5, z0 + j + 0.5
            if 0 <= (x - fx) * ux + (z - fz) * uz <= L and abs(-(x - fx) * uz + (z - fz) * ux) <= hw:
                g[j][i] = '/'
for pr in room.get('proofs', []):
    if pr.get('kind') == 'walk':
        pts = pr['points']
        for a, b in zip(pts, pts[1:]):
            n = int(max(1, math.hypot(b[0] - a[0], b[2] - a[2]) * 2))
            for k in range(n + 1):
                t = k / float(n)
                put(a[0] + (b[0] - a[0]) * t, a[2] + (b[2] - a[2]) * t, 'w', over=False)
for q in p.get('pads', []):
    if q.get('zone') == rid:
        put(q['at'][0], q['at'][2], 'P')
wn = room.get('waveNodes43')
for n in (wn[0] if wn and wn[0] else []):
    put(n[0], n[2], 'o')
print('%s %s rect %s (x %g..%g across, z %g at the top .. %g at the bottom); spawn %s' % (lv, rid, room['rect'], x0, x1, z0, z1, room['spawn']))
print('      ' + ''.join(str(int(abs(x0 + i)) % 10) for i in range(W)))
for j in range(H):
    print('%6.0f %s' % (z0 + j, ''.join(g[j])))
