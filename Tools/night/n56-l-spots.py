# NIGHT56 builder L (L2): the spawn-spot finder for one room. Lists the room's walk-graph nodes (n56-room.py's R2 floor: standable, reach and
# return) that pass the room's IN VIEW rule as a wave-1 node: held / vista: seen from the foothold's flanks (or the entry eye), and from the a452
# entry frame; ambushed / ambusher: 8-15 m from the door, <= 15 m from midfield, not behind the foothold, no line into the corridor. Each line:
# the spot, its depth / lateral from the door, its distance from the commit feet (SpawnWave's minSpawnDist 12 m acts on it) and from the door,
# and whether the a452 entry frame sees it. A builder's tool: it picks nothing; the plan edit names the spots it takes.
#   python -I Tools/night/n56-l-spots.py <level> <room> [--every 2] [--all]
import importlib.util, math, os, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_s = importlib.util.spec_from_file_location('n56room', os.path.join(HERE, 'n56-room.py'))
nr = importlib.util.module_from_spec(_s)
_s.loader.exec_module(nr)
idn = nr.idn


def main():
    a = sys.argv[1:]
    lv, rid = a[0], a[1]
    every = float(a[a.index('--every') + 1]) if '--every' in a else 2.0
    show_all = '--all' in a
    L = nr.Level(lv)
    p = L.plan
    room = idn.room_of(p, rid)
    op = nr.openings().get(lv, {}).get(rid, '-')
    near = nr.near_set(L, room)
    cx, cz, nin, did = nr.entry_frame(p, room)
    fh, ft = nr.foothold(p, room)
    feet = nr.commit_feet(p, room)
    ee, elook, epitch = nr.entry_eye(room)
    eyes = nr.flank_eyes(p, room, fh) if fh is not None else []
    if not eyes:
        eyes = [ee]
    X, Y, Z = L.nav[0], L.nav[1], L.nav[2]
    adj = nr.walk_cells(L, p, room)
    # the floor nodes behind the cells (walk_cells keeps the cells; re-derive the nodes: every nav node whose cell is in the graph)
    x0, z0, x1, z1 = room['rect']
    key = lambda x, y, z: (int(math.floor((x - x0) / 2.0)), int(math.floor((z - z0) / 2.0)), int(round(y / 0.5)))
    cells = set(adj.keys())
    picked = {}
    for i in range(len(X)):
        if not (x0 + 0.9 <= X[i] <= x1 - 0.9 and z0 + 0.9 <= Z[i] <= z1 - 0.9):
            continue
        k = key(X[i], Y[i], Z[i])
        if k not in cells:
            continue
        g = (round(X[i] / every), round(Z[i] / every), k[2])
        if g in picked:
            continue
        picked[g] = (X[i], Y[i], Z[i])
    mfx, mfz = cx + nin[0] * nr.room_depth(room, nin) / 2.0, cz + nin[1] * nr.room_depth(room, nin) / 2.0
    corr = (cx - nin[0] * 3.0, nr.floor_y(room) + nr.EYE, cz - nin[1] * 3.0)
    print('== n56-l-spots %s %s (%s): door %s centre (%g, %g) inward (%g, %g); feet (%.1f, %.1f); foothold %s' % (lv, rid, op, did, cx, cz, nin[0], nin[1], feet[0], feet[2], ft))
    print('   wave 1 now: %s  waves %s' % (nr.wave_nodes(room, 0), [len(w) for w in room.get('waves', [])]))
    rows = []
    for nd in sorted(picked.values(), key=lambda n: (nr.depth_of(cx, cz, nin, n[0], n[2]), nr.lat_of(cx, cz, nin, n[0], n[2]))):
        dd = math.hypot(nd[0] - cx, nd[2] - cz)
        dm = math.hypot(nd[0] - mfx, nd[2] - mfz)
        df = math.hypot(nd[0] - feet[0], nd[2] - feet[2])
        ent = nr.seen_frustum(L, near, ee, elook, epitch, nd)
        if op in ('held', 'vista'):
            ok = any(nr.seen(L, near, e, nin, nd) for e in eyes)
            why = 'seen from %s' % ('the foothold' if fh is not None else 'the entry')
        else:
            why = []
            if dm > 15.0: why.append('mid %.0f' % dm)
            if not (8.0 <= dd <= 15.0): why.append('door %.0f' % dd)
            if fh is not None and nr.seg_box(ee, (nd[0], nd[1] + 1.0, nd[2]), fh['min'], fh['max']): why.append('behind')
            if any(nr.ray_clear(L, (nd[0], nd[1] + h, nd[2]), corr, near) for h in (1.0, 1.6)): why.append('corridor')
            ok = not why
            why = ', '.join(why) or 'ok'
        if ok or show_all:
            rows.append('  %s [%g, %g, %g] depth %.1f lat %+.1f | feet %.1f door %.1f | entry frame %s | %s' % (
                'OK ' if ok else 'no ', nd[0], nd[1], nd[2], nr.depth_of(cx, cz, nin, nd[0], nd[2]), nr.lat_of(cx, cz, nin, nd[0], nd[2]), df, dd, 'seen' if ent else '-', why))
    print('\n'.join(rows))
    print('   %d spots listed' % len(rows))


if __name__ == '__main__':
    main()
