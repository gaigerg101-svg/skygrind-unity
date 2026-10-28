# EXT53 (the lead; row W): for each pad that fails I4 (Tools/night/n53-identity.py), search the room for the nearest spot that passes I4
# (2.0 m off every through walk line at its height, 1.0 m off the spawn's 3 m forward line), keeps 0.6 m from every solid's footprint and off
# every ramp, stays inside the room's rect by 1.5 m, stands on the same floor height (a nav node within 0.3 m of the pad's y) and keeps its
# throw to the target no longer than 1.15 x today's (the pad's arc is aimed at its target: a much longer throw can miss, as g8's did).
# Prints candidates nearest first; writes nothing.   python -I Tools/night/e53-padspot.py <level> <room> [<room> ...]
import importlib.util, json, math, os, sys

spec = importlib.util.spec_from_file_location('ident', os.path.join(os.path.dirname(__file__), 'n53-identity.py'))
m = importlib.util.module_from_spec(spec); spec.loader.exec_module(m)


def lines_of(room):
    return [pr['points'] for pr in room.get('proofs', []) if pr.get('kind') == 'walk' and len(pr.get('points', [])) >= 2]


def line_ok(p, room, x, y, z):
    for pts in lines_of(room):
        if math.hypot(pts[-1][0] - x, pts[-1][2] - z) <= 1.5 and abs(pts[-1][1] - y) <= 1.5:
            continue
        for a, b in zip(pts, pts[1:]):
            d, t = m.seg_dist(x, z, a[0], a[2], b[0], b[2])
            if abs(y - (a[1] + t * (b[1] - a[1]))) <= 1.5 and d < 2.0:
                return False
    sp = room.get('spawn', {})
    if 'at' in sp:
        sx, sy, sz = sp['at']; yaw = math.radians(sp.get('yawDeg', 0))
        d, _ = m.seg_dist(x, z, sx, sz, sx - 3 * math.sin(yaw), sz - 3 * math.cos(yaw))
        if abs(y - sy) <= 1.5 and d < 1.0:
            return False
    return True


def main():
    lv, rooms = sys.argv[1], sys.argv[2:]
    p = m.load_plan(m.plan_path(lv))
    nav = m.nav_of(lv)
    X, Y, Z = nav[0], nav[1], nav[2]
    for rid in rooms:
        room = m.room_of(p, rid)
        x0, z0, x1, z1 = room['rect']
        boxes = [b for b in m.zone_items(p, 'boxes', rid)]
        ramps = m.zone_items(p, 'ramps', rid)
        for pd in m.pads_of(p, rid):
            px, py, pz = pd['at']
            if line_ok(p, room, px, py, pz):
                continue
            tx, ty, tz = pd['target']
            throw0 = math.hypot(tx - px, tz - pz)
            cands = []
            for i in range(int((x1 - x0) * 4) + 1):
                for j in range(int((z1 - z0) * 4) + 1):
                    x, z = x0 + i * 0.25, z0 + j * 0.25
                    if not (x0 + 1.5 <= x <= x1 - 1.5 and z0 + 1.5 <= z <= z1 - 1.5):
                        continue
                    if math.hypot(x - px, z - pz) > 6:
                        continue
                    if math.hypot(tx - x, tz - z) > 1.15 * throw0:
                        continue
                    if any(b['max'][1] > py + 0.3 and b['min'][1] < py + 2 and math.hypot(max(b['min'][0] - x, 0, x - b['max'][0]), max(b['min'][2] - z, 0, z - b['max'][2])) < 0.6 for b in boxes):
                        continue
                    if any(b['min'][0] <= x <= b['max'][0] and b['min'][2] <= z <= b['max'][2] and abs(b['max'][1] - py) > 0.3 and b['max'][1] > py for b in boxes):
                        continue
                    bad = False
                    for r in ramps:
                        fx, fz = r['from']; gx, gz = r['to']; L = math.hypot(gx - fx, gz - fz) or 1e-9; ux, uz = (gx - fx) / L, (gz - fz) / L
                        a = (x - fx) * ux + (z - fz) * uz; s = abs(-(x - fx) * uz + (z - fz) * ux)
                        if -0.6 < a < L + 0.6 and s < r.get('width', 2) / 2 + 0.6:
                            bad = True; break
                    if bad or not line_ok(p, room, x, py, z):
                        continue
                    # a nav node at the pad's height within 0.4 m (the spot is floor at that height)
                    if not any(abs(X[k] - x) <= 0.6 and abs(Z[k] - z) <= 0.6 and abs(Y[k] - py) <= 0.3 for k in range(len(X))):
                        continue
                    cands.append((math.hypot(x - px, z - pz), x, z, math.hypot(tx - x, tz - z)))
            cands.sort()
            print('%s %s pad %s -> %s (throw %.1f m): %d spots; nearest: %s' % (lv, rid, pd['at'], pd['target'], throw0, len(cands),
                  '; '.join('[%g, %g, %g] %.1f m away, throw %.1f' % (x, py, z, d, t) for d, x, z, t in cands[:4]) or 'none'))


main()
