#!/usr/bin/env python3
# NIGHT73 builder B: a check of the wyrm framing gate's geometry (Assets/Scripts/Game/FeelReel.N73.Frame.cs; UnityTuning.N72B.cs Fin73*), no Unity: the head on the coil (a ring of radius R_COIL round the arena's
# centre at height Y_COIL, Design/bosses/wyrm.json), the player at the vantage (Fin73VantageM from the centre toward the room's entry, +z in the arena's own coordinates: playerSpawn z 27.5), the gate's terms:
# eye-to-aim distance in the band, a sight line the arena's boxes (Design/maps/wyrm-arena.json: pillars, walls) do not cut, the line missing the centre by Fin73ClearM. Prints, round the ring in 2 degree steps,
# the arcs where the gate's terms hold, and how long the head takes to cross each at the coil's speeds.
#   python -I Tools/night/n73-frame-geometry.py [vantage_m] [pieces_rho_m]
import json, math, sys

R_COIL, Y_COIL = 19.0, 3.5          # wyrm.json coil: radius 19, y 2.5-5
EYE_Y = 1.7                          # the player's eye over the floor (the sim's Eye; a stand)
FOV, SHARE = 74.0, 1.0 / 3.0         # camera.fovDeg, Fin73FillShare
LO, HI, CLEAR = 0.8, 1.25, 6.0       # Fin73BandLo, Fin73BandHi, Fin73ClearM
BASE, PHASE = 22.0, 1.25             # design.n24.wyrm.speed base, phaseMult (phase 2: x1.25, phase 3: x1.25^2 if it compounds)

vant = float(sys.argv[1]) if len(sys.argv) > 1 else 12.0
rho = float(sys.argv[2]) if len(sys.argv) > 2 else 4.0   # the head + 2 pieces' bounding sphere (head half 2.5 m; 2.4 m spacing; 1.25 half size: 2.4 + 1.25 = 3.65 about the first piece, the head's 2.5 + 2.4 more)
dt_target = rho / math.sin(math.radians(SHARE * FOV / 2))
boxes = json.load(open('Design/maps/wyrm-arena.json'))['collision']['boxes']
obst = [b for b in boxes if b['kind'] in ('pillar', 'wall')]

def seg_hits_box(p, q, b):
    # slab test of the segment p->q against an AABB
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

def miss_from_centre(p, q):
    sx, sz = q[0] - p[0], q[2] - p[2]
    sl = sx * sx + sz * sz
    t = max(0.0, min(1.0, (-p[0] * sx - p[2] * sz) / sl)) if sl > 1e-9 else 0.0
    return math.hypot(p[0] + sx * t, p[2] + sz * t)

eye = (0.0, EYE_Y, vant)
print('vantage %.1f m toward the entry (+z), eye %s, target distance %.1f m (rho %.1f m, %.0f deg x %.2f), band %.1f..%.1f m, miss >= %.0f m' % (vant, eye, dt_target, rho, FOV, SHARE, LO * dt_target, HI * dt_target, CLEAR))
ok = []
for deg in range(0, 360, 2):
    a = math.radians(deg)
    head = (R_COIL * math.sin(a), Y_COIL, R_COIL * math.cos(a))   # 0 deg = +z (the player's side), turning toward +x
    d = math.dist(eye, head)
    inband = LO * dt_target <= d <= HI * dt_target
    sight = not any(seg_hits_box(eye, head, b) for b in obst)
    miss = miss_from_centre(eye, head)
    ok.append(inband and sight and miss >= CLEAR)
    if deg % 10 == 0:
        print('%3d deg  d %5.1f  %s%s miss %4.1f %s' % (deg, d, 'band ' if inband else '     ', 'sight' if sight else 'BLOCK', miss, '<- gate ok' if ok[-1] else ''))
arcs, start = [], None
for i in range(len(ok) + 1):
    v = ok[i % len(ok)]
    if v and start is None:
        start = i
    if not v and start is not None:
        arcs.append((start * 2, (i - 1) * 2))
        start = None
print('gate-ok arcs (deg, 2 deg steps):', arcs)
for ph, mult in (('phase 1', 1.0), ('phase 2', PHASE), ('phase 3 (compounding)', PHASE * PHASE)):
    v = BASE * mult
    for lo, hi in arcs:
        arc = math.radians(max(2, hi - lo + 2)) * R_COIL
        print('%s %.1f m/s: arc %d..%d deg = %.1f m in %.2f s = %d frames at 60 fps' % (ph, v, lo, hi, arc, arc / v, round(arc / v * 60)))
