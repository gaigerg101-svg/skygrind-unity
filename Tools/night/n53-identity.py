# NIGHT53 P0 (the lead): THE IDENTITY CHECKS (NIGHT53.md section 2), computed from a level plan. Each check prints its number; a check is
# trusted only after its planted failure FAILs it (L-034): `--planted` runs each on NIGHT52's named FAIL case and exits 1 if one passes.
#   python -I Tools/night/n53-identity.py room <plan.json> <room>        I1 I2 I3 I4 for one room (I2 / I3: "not computed" until the loop's q1)
#   python -I Tools/night/n53-identity.py level <plan.json>              I5 (one shape) and I7 (openings) for one level
#   python -I Tools/night/n53-identity.py table <level:room,room ...>    the baseline table (Markdown) over the rooms named, + I5 / I7 per level
#   python -I Tools/night/n53-identity.py planted                        every planted case (I1 g4 at u3-n52; I4 kb3 / p1 / a3 at 04732d22;
#                                                                         I5 at u3-n52 (some level > 2); I7 a planted copy of the openings)
# Geometry (all from the plan; no sim): a SOLID is a box rising >= 0.5 m above its room's floor (max y >= 0.5) or a ramp (its footprint:
# the from -> to segment, `width` wide); pits are hazards, not solids. Footprints are rasterised on a 0.25 m grid inside the room's rect.
#   I1 No mirror: the room's solid footprint reflected across the entry door's axis (the line through the door's centre, along its inward
#      direction): overlap(reflected, original) / area(original) < 60 % PASS (a mirrored room reads ~100 %).
#   I4 Pads clear of lines: each pad's 1 m trigger disc (centre `at`) is FAIL when its centre is within 1 m (disc) + 1 m of a walk proof's
#      polyline (the room's `proofs` of kind walk; a line ending within 1.5 m of the pad is its own approach and skipped, D-6) at the line's
#      height (|dy| <= 1.5 m; the trigger is jumpPad.radius 1 m round the player's feet), or within 1 m (disc) of the 3 m forward segment from
#      the room's spawn (the retry spawn: a checkpoint door respawns there; yaw 0 faces -z, forward = (-sin yaw, -cos yaw)).
#   I5 Not one shape: a room is RING-ROUND-A-MASS when its central 40 % x 40 % core is >= 30 % occupied (solids or pits: the mass) and the
#      2.5 m band round the occupied core's bounds (from 0.5 m out) is >= 70 % walkable floor (the ring; DECIDE53 D-4). Per level at most
#      two such rooms PASS.
#   I2 The ride is a shortcut (q1): per ride wall, both faces (0.8 m off) and both directions: the start 0.5 m inside one end, the landing
#      1.5 m past the other, each the nearest nav node within 1.5 m (not in a pit, below the wall's top - 1 m); the walk = the shortest path on
#      the level file's navGrid (walk / jump / drop / gap links, kept inside the room's rect + 0.5 m, never on a lava pit's surface), in 3D m. A ride scores its best
#      (face, direction) walk / length; the room PASSES when one ride scores >= 1.3 or its landing is unreachable on foot.
#   I3 A teach room's verb is required (q1; grapple-pull lessons only): each ring's tier (the highest nav node >= 1.5 m up within 4 m under
#      it) against the nearest floor node: walk / straight pull >= 1.5 (or unreachable) for EVERY ring (D-8: the worst ring decides).
#   I7 Openings: Design/levels/n52/openings53.json gives each of the 18 rooms one of vista / ambushed / ambusher / held (NIGHT53.md section 2,
#      RESEARCH A-8); no two consecutive fight rooms (kind room, both with an opening) in a level share one PASS; the entry frame is J.
import json, math, os, re, subprocess, sys

OPENINGS = os.path.join('Design', 'levels', 'n52', 'openings53.json')
KINDS = ('vista', 'ambushed', 'ambusher', 'held')
G = 0.25


def load_plan(path, rev=None):
    if rev:
        txt = subprocess.run(['git', 'show', rev + ':' + path.replace('\\', '/')], capture_output=True, text=True, encoding='utf-8').stdout
        return json.loads(txt)
    return json.load(open(path, encoding='utf-8'))


def room_of(p, rid):
    return next((r for r in p['rooms'] if r['id'] == rid), None)


def zone_items(p, key, rid):
    return [b for b in p.get(key, []) if b.get('zone') == rid]


def entry_door(p, rid):
    order = [r['id'] for r in p['rooms']]
    idx = order.index(rid)
    for dr in p.get('doors', []):
        bt = dr.get('between', [])
        if rid in bt and idx > 0 and order[idx - 1] in bt:
            return dr
    return None


def door_frame(room, dr):
    x0, z0, x1, z1 = room['rect']
    dx0, dz0, dx1, dz1 = dr['rect']
    cx, cz = (dx0 + dx1) / 2.0, (dz0 + dz1) / 2.0
    if cz >= z1: nin = (0.0, -1.0)
    elif cz <= z0: nin = (0.0, 1.0)
    elif cx <= x0: nin = (1.0, 0.0)
    else: nin = (-1.0, 0.0)
    return cx, cz, nin


def solids(p, rid):
    out = []  # (kind, test(x, z) -> bool, item)
    for b in zone_items(p, 'boxes', rid):
        if b['max'][1] >= 0.5:
            mn, mx = b['min'], b['max']
            out.append(('box', (lambda mn, mx: lambda x, z: mn[0] <= x <= mx[0] and mn[2] <= z <= mx[2])(mn, mx), b))
    for r in zone_items(p, 'ramps', rid):
        fx, fz = r['from']; tx, tz = r['to']; hw = r.get('width', 2) / 2.0
        L = math.hypot(tx - fx, tz - fz) or 1e-9
        ux, uz = (tx - fx) / L, (tz - fz) / L
        out.append(('ramp', (lambda fx, fz, ux, uz, L, hw: lambda x, z: 0 <= (x - fx) * ux + (z - fz) * uz <= L and abs(-(x - fx) * uz + (z - fz) * ux) <= hw)(fx, fz, ux, uz, L, hw), r))
    return out


def grid(room):
    x0, z0, x1, z1 = room['rect']
    nx, nz = int(round((x1 - x0) / G)), int(round((z1 - z0) / G))
    return [(x0 + (i + 0.5) * G, z0 + (j + 0.5) * G) for i in range(nx) for j in range(nz)]


def i1(p, rid):
    room = room_of(p, rid)
    dr = entry_door(p, rid)
    if dr is None:
        return None, 'no entry door (first room?)'
    cx, cz, nin = door_frame(room, dr)
    S = solids(p, rid)
    inside = lambda x, z: any(t(x, z) for _, t, _ in S)
    cells = [c for c in grid(room) if inside(*c)]
    if not cells:
        return None, 'no solids'
    along_z = nin[0] == 0.0  # the door on the north / south wall: the axis is the line x = cx
    refl = (lambda x, z: (2 * cx - x, z)) if along_z else (lambda x, z: (x, 2 * cz - z))
    hit = sum(1 for (x, z) in cells if inside(*refl(x, z)))
    pct = 100.0 * hit / len(cells)
    axis = ('x = %g' % cx) if along_z else ('z = %g' % cz)
    return pct < 60.0, 'mirror overlap %.0f %% across the door axis %s (%s; %d solids, %.0f m2 footprint; < 60 %% PASS)' % (pct, axis, dr['id'], len(S), len(cells) * G * G)


def seg_dist(px, pz, ax, az, bx, bz):
    vx, vz = bx - ax, bz - az
    L2 = vx * vx + vz * vz
    t = 0.0 if L2 == 0 else max(0.0, min(1.0, ((px - ax) * vx + (pz - az) * vz) / L2))
    return math.hypot(px - (ax + t * vx), pz - (az + t * vz)), t


def pads_of(p, rid):
    room = room_of(p, rid)
    out = []
    for b in zone_items(p, 'pads', rid) + [q for q in room.get('pads', []) if isinstance(q, dict)]:
        if 'at' in b:
            out.append(b)
    return out


def i4(p, rid):
    room = room_of(p, rid)
    pads = pads_of(p, rid)
    if not pads:
        return True, 'no pads'
    lines = [pr['points'] for pr in room.get('proofs', []) if pr.get('kind') == 'walk' and len(pr.get('points', [])) >= 2]
    sp = room.get('spawn', {})
    bad, near = [], []
    for pd in pads:
        px, py, pz = pd['at']
        best = None
        for li, pts in enumerate(lines):
            # a line that ends within 1.5 m of the pad is that pad's own approach proof (it walks up to the pad), not a through line (D-6)
            if math.hypot(pts[-1][0] - px, pts[-1][2] - pz) <= 1.5 and abs(pts[-1][1] - py) <= 1.5:
                continue
            for k in range(len(pts) - 1):
                a, b = pts[k], pts[k + 1]
                d, t = seg_dist(px, pz, a[0], a[2], b[0], b[2])
                y = a[1] + t * (b[1] - a[1])
                if abs(py - y) <= 1.5 and (best is None or d < best[0]):
                    best = (d, li)
        if best and best[0] < 2.0:
            bad.append('pad [%g,%g,%g] %.2f m from walk line %d (< 2.0: the 1 m disc within 1 m)' % (px, py, pz, best[0], best[1]))
        elif best:
            near.append('%.1f' % best[0])
        if 'at' in sp:
            sx, sy, sz = sp['at']
            yaw = math.radians(sp.get('yawDeg', 0))
            fx, fz = -math.sin(yaw), -math.cos(yaw)
            d, t = seg_dist(px, pz, sx, sz, sx + 3 * fx, sz + 3 * fz)
            if abs(py - sy) <= 1.5 and d < 1.0:
                bad.append('pad [%g,%g,%g] %.2f m from the spawn\'s 3 m forward line (< 1.0)' % (px, py, pz, d))
    txt = '%d pads, %d walk lines; ' % (len(pads), len(lines)) + ('; '.join(bad) if bad else 'all clear (nearest line distances %s m)' % (', '.join(near) or '-'))
    return not bad, txt


def ring_room(p, rid):
    room = room_of(p, rid)
    x0, z0, x1, z1 = room['rect']
    w, d, cx, cz = x1 - x0, z1 - z0, (x0 + x1) / 2.0, (z0 + z1) / 2.0
    S = solids(p, rid)
    pits = zone_items(p, 'pits', rid)
    occ = lambda x, z: any(t(x, z) for _, t, _ in S) or any(q['rect'][0] <= x <= q['rect'][2] and q['rect'][1] <= z <= q['rect'][3] for q in pits)
    cells = grid(room)
    core = [(x, z) for x, z in cells if abs(x - cx) <= 0.2 * w and abs(z - cz) <= 0.2 * d]
    oc = [(x, z) for x, z in core if occ(x, z)]
    f = 100.0 * len(oc) / max(1, len(core))
    if not oc:
        return False, 'core occupied 0 %'
    bx0, bx1 = min(x for x, _ in oc), max(x for x, _ in oc)
    bz0, bz1 = min(z for _, z in oc), max(z for _, z in oc)
    band = [not occ(x, z) for x, z in cells if 0.5 <= math.hypot(max(bx0 - x, 0, x - bx1), max(bz0 - z, 0, z - bz1)) <= 3.0]
    fr = 100.0 * sum(band) / max(1, len(band))
    return f >= 30.0 and fr >= 70.0, 'core (central 40 %% x 40 %%) occupied %.0f %% (>= 30 = a mass), the band round it %.0f %% walkable (>= 70 = a ring)' % (f, fr)


def i5(p):
    rows = []
    for r in p['rooms']:
        if r.get('kind') != 'room':
            continue
        ok, txt = ring_room(p, r['id'])
        rows.append((r['id'], ok, txt))
    rings = [rid for rid, ok, _ in rows if ok]
    return len(rings) <= 2, rings, rows


def openings(level):
    if not os.path.exists(OPENINGS):
        return {}
    return json.load(open(OPENINGS, encoding='utf-8')).get(level, {})


def i7(p, level, ops=None):
    ops = openings(level) if ops is None else ops
    fights = [r['id'] for r in p['rooms'] if r.get('kind') == 'room']
    bad_kind = [rid + '=' + str(ops[rid]) for rid in fights if rid in ops and ops[rid] not in KINDS]
    clash = []
    for a, b in zip(fights, fights[1:]):
        if a in ops and b in ops and ops[a] == ops[b]:
            clash.append('%s and %s both %s' % (a, b, ops[a]))
    txt = 'openings ' + ', '.join('%s %s' % (rid, ops.get(rid, '-')) for rid in fights)
    ok = not clash and not bad_kind
    return ok, txt + ('; ' + '; '.join(clash + bad_kind) if not ok else '; no consecutive pair shares one')


# ---- I2 / I3 (NIGHT53 q1, the loop's first cycle): path lengths on the bot's nav grid (the level file's navGrid: NavGridBuilder's export,
# 1 m cells, walk / jump (<= 2.2 m up) / drop (<= 5 m) / gap (<= 3 m) links: a walker's graph, no pads, rides or grapple). A path is kept
# inside the room's rect (+0.5 m: the doors lock in a fight); its length is the sum of the 3D node-to-node distances.
_NAV = {}


def nav_of(level):
    if level not in _NAV:
        f = os.path.join('Design', 'levels', 'n52-' + level + '.json')
        g = json.load(open(f, encoding='utf-8'))['navGrid']
        n, l = g['nodes'], g['links']
        _NAV[level] = (n['x'], n['y'], n['z'], l['linkStart'], l['linkTo'])
    return _NAV[level]


def nav_near(nav, room, x, z, r=1.5, ymin=-0.3, ymax=99.0):
    X, Y, Z = nav[0], nav[1], nav[2]
    best = None
    for i in range(len(X)):
        dx, dz = X[i] - x, Z[i] - z
        if abs(dx) > r or abs(dz) > r or not (ymin <= Y[i] <= ymax):
            continue
        d = math.hypot(dx, dz)
        if d <= r and (best is None or d < best[0]):
            best = (d, i)
    return None if best is None else best[1]


def lava_of(p, rid):
    return [q['rect'] + [q.get('floorY', -0.8)] for q in zone_items(p, 'pits', rid) if q.get('lava')]


def nav_path(nav, room, a, b, lava=()):
    # lava: [x0, z0, x1, z1, floorY] per lava pit: its surface nodes are no walk (builder K, 08:46: the grid links every lava surface; a dry
    # pit such as g4's canal stays walkable: a fall costs a jump out)
    import heapq
    X, Y, Z, LS, LT = nav
    x0, z0, x1, z1 = room['rect']
    inlava = lambda i: any(q[0] <= X[i] <= q[2] and q[1] <= Z[i] <= q[3] and Y[i] <= q[4] + 0.3 for q in lava)
    inside = lambda i: x0 - 0.5 <= X[i] <= x1 + 0.5 and z0 - 0.5 <= Z[i] <= z1 + 0.5 and not inlava(i)
    dist = {a: 0.0}
    h = [(0.0, a)]
    while h:
        d, i = heapq.heappop(h)
        if i == b:
            return d
        if d > dist.get(i, 1e18):
            continue
        for k in range(LS[i], LS[i + 1]):
            j = LT[k]
            if not inside(j):
                continue
            nd = d + math.sqrt((X[j] - X[i]) ** 2 + (Y[j] - Y[i]) ** 2 + (Z[j] - Z[i]) ** 2)
            if nd < dist.get(j, 1e18):
                dist[j] = nd
                heapq.heappush(h, (nd, j))
    return None


def i2(p, rid, level):
    room = room_of(p, rid)
    rides = [b for b in zone_items(p, 'boxes', rid) if b.get('ride')]
    if not rides:
        return None, 'no ride wall'
    nav = nav_of(level)
    parts, ok_any = [], False
    for b in rides:
        mn, mx = b['min'], b['max']
        along_z = (mx[2] - mn[2]) >= (mx[0] - mn[0])
        L = (mx[2] - mn[2]) if along_z else (mx[0] - mn[0])
        best = None
        for side in (-1, 1):
            off = (mn[0] - 0.8) if (along_z and side < 0) else (mx[0] + 0.8) if along_z else (mn[2] - 0.8) if side < 0 else (mx[2] + 0.8)
            for fwd in (-1, 1):
                if along_z:
                    s = (off, mx[2] - 0.5) if fwd < 0 else (off, mn[2] + 0.5)
                    t = (off, mn[2] - 1.5) if fwd < 0 else (off, mx[2] + 1.5)
                else:
                    s = (mx[0] - 0.5, off) if fwd < 0 else (mn[0] + 0.5, off)
                    t = (mn[0] - 1.5, off) if fwd < 0 else (mx[0] + 1.5, off)
                a = nav_near(nav, room, s[0], s[1], ymax=mx[1] - 1.0)
                c = nav_near(nav, room, t[0], t[1], ymax=mx[1] - 1.0)
                if a is None or c is None:
                    continue
                w = nav_path(nav, room, a, c, lava_of(p, rid))
                ratio = float('inf') if w is None else w / max(L, 1e-6)
                cand = (ratio, 'side %+d dir %+d: walk %s m / ride %.1f m = %s' % (side, fwd, 'unreachable' if w is None else '%.1f' % w, L, 'inf' if w is None else '%.2f' % ratio))
                if best is None or cand[0] > best[0]:
                    best = cand
        if best is None:
            parts.append('ride [%g..%g, %g..%g]: no start / landing on the grid' % (mn[0], mx[0], mn[2], mx[2]))
            continue
        ok_any = ok_any or best[0] >= 1.3
        parts.append('ride %.0f m at x %g..%g z %g..%g: best %s' % (L, mn[0], mx[0], mn[2], mx[2], best[1]))
    return ok_any, '; '.join(parts) + ' (>= 1.3 or unreachable PASS: at least one ride a shortcut)'


def i3(p, rid, level):
    room = room_of(p, rid)
    prompts = ' '.join(str(l.get('prompt', '')) for l in room.get('lessons', []))
    if '{grapple}' not in prompts or 'AT AN ENEMY' in prompts:
        return None, 'not a grapple-pull teach room (ride lessons: I2; whip, pad and slide lessons: not computed)'
    nav = nav_of(level)
    rings = [g['at'] if isinstance(g, dict) else g for g in (list(room.get('grappleNodes', [])) + zone_items(p, 'grapple', rid))]
    X, Y, Z = nav[0], nav[1], nav[2]
    best = None
    for ring in rings:
        # the ring's target: the highest walkable node within 4 m of the ring, at least 1.5 m up and below it (the tier it serves)
        tgt = None
        for i in range(len(X)):
            if abs(X[i] - ring[0]) <= 4 and abs(Z[i] - ring[2]) <= 4 and 1.5 <= Y[i] < ring[1] and math.hypot(X[i] - ring[0], Z[i] - ring[2]) <= 4:
                if tgt is None or Y[i] > Y[tgt] or (Y[i] == Y[tgt] and math.hypot(X[i] - ring[0], Z[i] - ring[2]) < math.hypot(X[tgt] - ring[0], Z[tgt] - ring[2])):
                    tgt = i
        if tgt is None:
            continue
        # the verb's route: from the nearest floor node (y < 0.5) to the target, straight (the pull)
        fl = None
        for i in range(len(X)):
            x0, z0, x1, z1 = room['rect']
            if Y[i] < 0.5 and Y[i] > -0.3 and x0 <= X[i] <= x1 and z0 <= Z[i] <= z1:
                d = math.hypot(X[i] - X[tgt], Z[i] - Z[tgt])
                if fl is None or d < fl[0]:
                    fl = (d, i)
        if fl is None:
            continue
        f = fl[1]
        route = math.sqrt((X[tgt] - X[f]) ** 2 + (Y[tgt] - Y[f]) ** 2 + (Z[tgt] - Z[f]) ** 2)
        w = nav_path(nav, room, f, tgt, lava_of(p, rid))
        ratio = float('inf') if w is None else w / max(route, 1e-6)
        cand = (ratio, 'ring %s -> tier %.1f m at (%g, %g): walk %s m / pull %.1f m = %s' % (ring, Y[tgt], X[tgt], Z[tgt], 'unreachable' if w is None else '%.1f' % w, route, 'inf' if w is None else '%.2f' % ratio))
        if best is None or cand[0] < best[0]:
            best = cand
    if best is None:
        return None, 'no ring with a tier under it'
    return best[0] >= 1.5, 'worst ring ' + best[1] + ' (every ring needs the pull: >= 1.5 or unreachable PASS, D-8)'


def lvl_of(plan_path):
    return os.path.basename(plan_path).split('.')[0]


def plan_path(level):
    return os.path.join('Design', 'levels', 'n52', level + '.plan.json')


def tag(ok):
    return 'n/c ' if ok is None else ('PASS' if ok else 'FAIL')


def room_lines(p, rid, level=None):
    a, at = i1(p, rid)
    b, bt = i4(p, rid)
    if level is None:
        c, ct, e, et = None, 'not computed (no level)', None, 'not computed (no level)'
    else:
        c, ct = i2(p, rid, level)
        e, et = i3(p, rid, level)
    return [('I1 no mirror', a, at), ('I2 ride shortcut', c, ct), ('I3 teach verb needed', e, et), ('I4 pads clear', b, bt)]


def main():
    a = sys.argv[1:]
    if not a:
        print(open(__file__, encoding='utf-8').read().split('import json')[0]); sys.exit(2)
    if a[0] == 'room':
        p = load_plan(a[1]); rid = a[2]
        print('== n53-identity room %s %s' % (lvl_of(a[1]), rid))
        lines = room_lines(p, rid, lvl_of(a[1]))
        for name, ok, txt in lines:
            print('  %s %-22s %s' % (tag(ok), name, txt))
        fails = [n for n, ok, _ in lines if ok is False]
        print('IDENTITY %s: %s' % (rid, 'PASS (computed items)' if not fails else 'FAIL ' + ', '.join(fails)))
        sys.exit(1 if fails else 0)
    if a[0] == 'level':
        p = load_plan(a[1]); lv = lvl_of(a[1])
        ok5, rings, rows = i5(p)
        for rid, ok, txt in rows:
            print('  I5 %-4s %s %s' % (rid, 'RING' if ok else '-   ', txt))
        print('  %s I5 one shape        %s: %d ring-round-a-mass rooms (%s; <= 2 PASS)' % (tag(ok5), lv, len(rings), ', '.join(rings) or 'none'))
        ok7, t7 = i7(p, lv)
        print('  %s I7 openings         %s' % (tag(ok7), t7))
        sys.exit(0 if ok5 and ok7 else 1)
    if a[0] == 'table':
        items = a[1:]
        print('| room | I1 mirror overlap | I2 walk / ride (per ride) | I3 walk / pull (worst ring) | I4 pads | verdict |')
        print('|---|---|---|---|---|---|')
        levels = []
        allok = True
        for it in items:
            lv, rooms = it.split(':')
            levels.append(lv)
            p = load_plan(plan_path(lv))
            for rid in rooms.split(','):
                ls = room_lines(p, rid, lv)
                fails = [n.split()[0] for n, ok, _ in ls if ok is False]
                allok = allok and not fails
                i1t = ls[0][2].split(' (')[0].replace('mirror overlap ', '')
                num = lambda t: re.findall(r' m = ([0-9.]+|inf)', t) or ['-']
                i2t = '/'.join(num(ls[1][2])) if ls[1][1] is not None else '-'
                i3t = num(ls[2][2])[0] if ls[2][1] is not None else '-'
                print('| %s %s | %s %s | %s %s | %s %s | %s %s | %s |' % (lv, rid, tag(ls[0][1]), i1t, tag(ls[1][1]), i2t, tag(ls[2][1]), i3t, tag(ls[3][1]), ls[3][2][:160], 'FAIL ' + ','.join(fails) if fails else 'PASS'))
        print('')
        print('| level | I5 ring rooms (<= 2) | I7 openings |')
        print('|---|---|---|')
        for lv in levels:
            p = load_plan(plan_path(lv))
            ok5, rings, _ = i5(p)
            ok7, t7 = i7(p, lv)
            allok = allok and ok5 and ok7
            print('| %s | %s %d (%s) | %s %s |' % (lv, tag(ok5), len(rings), ', '.join(rings) or 'none', tag(ok7), t7))
        sys.exit(0 if allok else 1)
    if a[0] == 'planted':
        res = []
        p = load_plan(plan_path('iceline'), 'u3-n52')
        ok, t = i1(p, 'g4'); res.append(('I1 iceline g4 @u3-n52', ok, t))
        for lv, rid in (('kiln', 'kb3'), ('span', 'p1'), ('foundry', 'a3')):
            p = load_plan(plan_path(lv), '04732d22')
            ok, t = i4(p, rid); res.append(('I4 %s %s @04732d22' % (lv, rid), ok, t))
        # I2's planted case is g4 @u3-n52 (D-11: a3's, the review's '~1 m longer', measured 26.1 m walk vs 18 m ride once lava is no walk)
        for lv, rid, fn, nm in (('iceline', 'g4', 'i2', 'I2'), ('span', 'p1', 'i3', 'I3')):
            g = json.loads(subprocess.run(['git', 'show', 'u3-n52:Design/levels/n52-' + lv + '.json'], capture_output=True, text=True, encoding='utf-8').stdout)['navGrid']
            _NAV[lv] = (g['nodes']['x'], g['nodes']['y'], g['nodes']['z'], g['links']['linkStart'], g['links']['linkTo'])
            ok, t = globals()[fn](load_plan(plan_path(lv), 'u3-n52'), rid, lv)
            res.append(('%s %s %s @u3-n52' % (nm, lv, rid), ok, t))
            _NAV.pop(lv, None)
        worst = None
        for lv in ('foundry', 'iceline', 'kiln', 'span', 'spire'):
            ok5, rings, _ = i5(load_plan(plan_path(lv), 'u3-n52'))
            if not ok5:
                worst = (lv, rings)
        res.append(('I5 some level @u3-n52', worst is None, ('%s: %d rings (%s)' % (worst[0], len(worst[1]), ', '.join(worst[1]))) if worst else 'every level <= 2'))
        p = load_plan(plan_path('iceline'))
        fights = [r['id'] for r in p['rooms'] if r.get('kind') == 'room']
        planted = dict(openings('iceline')); planted[fights[1]] = planted.get(fights[0], 'held'); planted[fights[0]] = planted[fights[1]]
        ok, t = i7(p, 'iceline', planted); res.append(('I7 planted copy (%s = %s)' % (fights[0], fights[1]), ok, t))
        good = True
        for name, ok, t in res:
            print('  %s %-30s %s%s' % (tag(ok), name, t, '' if ok is False else '   <- SHOULD FAIL'))
            good = good and ok is False
        print('PLANTED: %s' % ('every check FAILs its planted case (trusted)' if good else 'a planted case did not FAIL (that check is not trusted)'))
        sys.exit(0 if good else 1)
    print('unknown mode ' + a[0]); sys.exit(2)


if __name__ == '__main__':
    main()
