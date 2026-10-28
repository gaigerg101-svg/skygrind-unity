#!/usr/bin/env python3
# NIGHT73 builder B: where to stand for the wyrm framing (a companion of n73-frame-geometry.py, same terms). Every spot on a 1 m grid within MAXR of the arena's centre that is not inside a pillar, scored by the
# longest arc of the coil (a ring of radius 19 m at 3.5 m, 2 degree steps) on which the gate's terms hold: the eye-to-aim distance in the band, a clear sight line (pillars and walls), the line missing the centre
# by at least CLEAR. Prints the best spots by arc and the share of the ring that is gate-ok in all.
#   python -I Tools/night/n73-frame-search.py [clear_m] [maxr_m]
import json, math, sys

R_COIL, Y_COIL, EYE_Y = 19.0, 3.5, 1.7
FOV, SHARE, RHO = 74.0, 1.0 / 3.0, 4.0
LO, HI = 0.8, 1.25
CLEAR = float(sys.argv[1]) if len(sys.argv) > 1 else 6.0
MAXR = float(sys.argv[2]) if len(sys.argv) > 2 else 15.0
DT = RHO / math.sin(math.radians(SHARE * FOV / 2))
boxes = json.load(open('Design/maps/wyrm-arena.json'))['collision']['boxes']
obst = [b for b in boxes if b['kind'] in ('pillar', 'wall')]
pill = [b for b in boxes if b['kind'] == 'pillar']

def seg_hits_box(p, q, b):
    t0, t1 = 0.0, 1.0
    for a in range(3):
        d = q[a] - p[a]
        lo, hi = b['min'][a], b['max'][a]
        if abs(d) < 1e-9:
            if p[a] < lo or p[a] > hi:
                return False
        else:
            ta, tb = (lo - p[a]) / d, (hi - p[a]) / d
            if ta > tb:
                ta, tb = tb, ta
            t0, t1 = max(t0, ta), min(t1, tb)
            if t0 > t1:
                return False
    return True

def miss(p, q):
    sx, sz = q[0] - p[0], q[2] - p[2]
    sl = sx * sx + sz * sz
    t = max(0.0, min(1.0, (-p[0] * sx - p[2] * sz) / sl)) if sl > 1e-9 else 0.0
    return math.hypot(p[0] + sx * t, p[2] + sz * t)

res = []
for vx in range(-int(MAXR), int(MAXR) + 1):
    for vz in range(-int(MAXR), int(MAXR) + 1):
        if math.hypot(vx, vz) > MAXR:
            continue
        if any(b['min'][0] - 0.6 <= vx <= b['max'][0] + 0.6 and b['min'][2] - 0.6 <= vz <= b['max'][2] + 0.6 for b in pill):
            continue
        eye = (vx, EYE_Y, vz)
        ok = []
        for deg in range(0, 360, 2):
            a = math.radians(deg)
            head = (R_COIL * math.sin(a), Y_COIL, R_COIL * math.cos(a))
            d = math.dist(eye, head)
            ok.append(LO * DT <= d <= HI * DT and not any(seg_hits_box(eye, head, b) for b in obst) and miss(eye, head) >= CLEAR)
        # the longest circular run
        best, run = 0, 0
        for v in ok + ok:
            run = run + 1 if v else 0
            best = max(best, min(run, len(ok)))
        res.append((best * 2, sum(ok) * 2, vx, vz))
res.sort(reverse=True)
print('target distance %.1f m, band %.1f..%.1f m, miss >= %.0f m, spots within %.0f m of the centre: %d' % (DT, LO * DT, HI * DT, CLEAR, MAXR, len(res)))
print('best spots (longest arc deg, total gate-ok deg of the ring, x, z in the arena frame; arc 10 deg = 3.3 m of coil = 0.1 s at 34 m/s):')
for r in res[:14]:
    print('  arc %3d deg, total %3d deg at (%3d, %3d), %.1f m from the centre, entry side z %+d' % (r[0], r[1], r[2], r[3], math.hypot(r[2], r[3]), r[3]))
