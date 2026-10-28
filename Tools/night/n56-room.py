# NIGHT56 builder L (L1, NIGHT56.md section 5 L): THE ROOM CHECKER, added to n53-identity.py (imported, its I1-I4 / I5 / I7 kept as they are).
# Per combat room (the 18 rooms of Design/levels/n52/openings53.json), from the plan and the generated level file (no sim):
#   R2 no dead ends   the room's walk graph: the level file's navGrid nodes inside the room's rect (lava surfaces out), coarsened to 2 m x 2 m cells
#                     per 0.5 m height step, joined by the grid's walk / jump / drop / gap links plus each ride wall (start -> landing, both
#                     faces, as I2) and each pad (its disc -> its target), undirected. A DEAD END is a cell with exactly one neighbour cell (a
#                     1-cell spur you must turn back out of). PASS: 0 dead-end cells. The bridges (cells joined by one edge only) are printed, not gated.
#   R5 a foothold     a free-standing box (not flush with the room's walls) of the room's zone whose top stands above the eye (body.eyeHeight 1.6 m, PortExport/config.json) over the room's
#                     floor and whose footprint reaches 4-8 m inside the entry (its depth along the door's inward normal from the door's centre).
#   R8 every job      every solid of the room's zone (a box rising >= 0.5 m, or a ramp: n53-identity's solids) carries a non-empty `job` or
#                     `job36` in the plan (the existing fields; no tags file). PASS: 0 untagged.
#   IN VIEW (the room's OPENING, openings53.json; wave 1 = waveNodes43[0] when set, else nodes + highNodes, as Campaign24.cs:913 / :323):
#     each of wave 1's enemies goes to a node by Campaign24.SpawnWave's rule (a random start, then the first node >= minSpawnDist 12 m from the
#     player's feet, the last try taking any): the feet are the commit point (design.n43.rooms43 commitM 3 m inside the room's spawn); P(node)
#     is exact over the starts. A node is SEEN from an eye when the web collider ray (the level file's collision boxes and wedges) from the eye
#     to the node's body (+1.0 m) or head (+1.6 m) is clear and the node is inside the camera's horizontal half-angle (camera.fovDeg 74 vertical at
#     16:9: 53.3 deg) round the eye's look.
#     held / vista       PASS when the expected enemies (or their spawn tells) seen from the foothold's eye (the better of its two flanks,
#                        eye 1.6 m, looking along the inward normal; the entry eye when there is no foothold) is >= 4 (at any distance).
#     ambushed / ambusher PASS when every wave-1 node in use is (a) within 15 m of midfield (the door's centre + the room's half depth inward),
#                        (b) 8-15 m from the entry door's centre, (c) not behind the foothold (the entry eye's ray to it does not pass through
#                        the foothold box) and (d) has no line into the entry corridor (its ray to an eye 3 m outside the door is blocked).
#   The table's "w1 entry" column: the expected enemies of wave 1 seen from the a452 entry frame (frames52.entry: feet, yaw, pitch; frustum +
#   ray) and the nearest node distance from it: what the a156 meter (FeelReel.N56.Room.cs) prints on the Player, for the agreement check.
#   python -I Tools/night/n56-room.py table [level:room,room ...]   the table over the rooms named (default: the 18), + I5 / I7 per level
#   python -I Tools/night/n56-room.py room <level> <room>           every number for one room
#   python -I Tools/night/n56-room.py planted                       each check on its planted case (exit 0 only when every one FAILs: L-091)
#   python -I Tools/night/n56-room.py wedge-planted                  NIGHT74: a rotated wedge (yaw 0, +-pi / 2, pi) must block a ray across its high end and clear one over its low end (exit 0 only when all 8 hold)
#   ... --legacy-wedge-degrees                                       NIGHT74: the reading before the fix (a wedge's yaw as degrees), for before / after tables: wedge-planted FAILs 5 of its 8 cases with it
#   ... --level-dir <dir>                                            NIGHT74: a gen-slots.cjs --out folder (<dir>/<lv>.plan.json, <dir>/n67-<lv>.json, <dir>/openings67.json) in place of the n52 files: a builder's candidate level
#   python -I Tools/night/n56-room.py agree <Player log> ...         the a156 lines' verdicts (by opening) against IN VIEW: the L2 agreement
#   python -I Tools/night/n56-room.py ... --rev <git rev>           read the plans and level files at a revision (e.g. u3-n52)
import importlib.util, json, math, os, subprocess, sys
sys.dont_write_bytecode = True

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('n53id', os.path.join(HERE, 'n53-identity.py'))
idn = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(idn)

ALL18 = ['kiln:kb2,kb2c,kb3', 'foundry:a3,a5,b1', 'iceline:g2,g3,g4,g5,g7,g8', 'spire:v2,v5,v6', 'span:p1,p3,p5']


def git_json(rev, path):
    txt = subprocess.run(['git', 'show', rev + ':' + path.replace('\\', '/')], capture_output=True, text=True, encoding='utf-8').stdout
    return json.loads(txt)


def cfg_num(cfg, dotted):
    v = cfg
    for k in dotted.split('.'):
        v = v[k]
    return float(v)


def load_cfg():
    port = json.load(open(os.path.join('PortExport', 'config.json'), encoding='utf-8'))
    uc = json.load(open(os.path.join('Design', 'unity-config.json'), encoding='utf-8'))
    eye = cfg_num(port, 'body.eyeHeight')
    fov = cfg_num(port, 'camera.fovDeg')
    msd = cfg_num(uc, 'design.n24.campaign.minSpawnDist')
    commit = cfg_num(uc, 'design.n43.rooms43.commitM')  # Campaign24.cs:609-613 (R43 + "commitM")
    return eye, fov, msd, commit


EYE, FOV, MIN_SPAWN, COMMIT = load_cfg()
BESIDE_M = 10.0  # m: an ambushed room's wave-1 node is BESIDE the foothold when within this of its box (gate-r2 L1; SKYGRIND number, a proposal:
#                  the slate and levels.md name no distance; the foothold stands 4-8 m in, the spawns 8-15 m from the door: 0-11 m deeper)
HALF_H = math.degrees(math.atan(math.tan(math.radians(FOV / 2.0)) * 16.0 / 9.0))  # the horizontal half-angle at 16:9


LEVEL_DIR = None  # --level-dir <dir>: read <dir>/<lv>.plan.json, <dir>/n67-<lv>.json and <dir>/openings67.json (a gen-slots.cjs --out folder: a builder's candidate level) instead of the n52 files (NIGHT74)


class Level:
    def __init__(self, lv, rev=None):
        self.lv = lv
        if LEVEL_DIR:
            self.plan = json.load(open(os.path.join(LEVEL_DIR, lv + '.plan.json'), encoding='utf-8'))
            lf = json.load(open(os.path.join(LEVEL_DIR, 'n67-' + lv + '.json'), encoding='utf-8'))
        else:
            pp = idn.plan_path(lv)
            lp = os.path.join('Design', 'levels', 'n52-' + lv + '.json')
            self.plan = git_json(rev, pp) if rev else json.load(open(pp, encoding='utf-8'))
            lf = git_json(rev, lp) if rev else json.load(open(lp, encoding='utf-8'))
        col = lf['collision']
        self.boxes = [(b['min'], b['max']) for b in col['boxes']]
        self.wedges = col.get('wedges', [])
        g = lf['navGrid']
        self.nav = (g['nodes']['x'], g['nodes']['y'], g['nodes']['z'], g['links']['linkStart'], g['links']['linkTo'])


# ---- the ray (the web collider's boxes + wedges, as the level file exports them)
def seg_box(a, b, mn, mx):
    t0, t1 = 0.0, 1.0
    for k in range(3):
        d = b[k] - a[k]
        if abs(d) < 1e-12:
            if a[k] < mn[k] or a[k] > mx[k]:
                return False
            continue
        u0, u1 = (mn[k] - a[k]) / d, (mx[k] - a[k]) / d
        if u0 > u1:
            u0, u1 = u1, u0
        t0, t1 = max(t0, u0), min(t1, u1)
        if t0 > t1:
            return False
    return t1 > 1e-4 and t0 < 1 - 1e-4


LEGACY_WEDGE_DEGREES = False  # --legacy-wedge-degrees: the reading before NIGHT74 (S's finding), for the before / after tables only: a wedge's yaw as DEGREES


def wedge_yaw(w):
    """A wedge's yaw in RADIANS. The level file's collision wedges carry the yaw in radians (0, +-1.5708, 3.1416 in every shipped level; PortData.Wedge -> WedgeSpec.yaw -> ArenaMap.WedgeCorners and the sim's
    collider call JsMath.Sin(w.yaw) directly). This tool ran it through math.radians until NIGHT74: a ramp turned a quarter or half turn was read as turned 1.5 / 3.1 degrees, so every rotated ramp stood where a
    yaw-0 one would, and IN VIEW, the beside rule and every ray_clear were wrong wherever a rotated ramp stands between an eye and a node."""
    y = w.get('yaw', 0)
    return math.radians(y) if LEGACY_WEDGE_DEGREES else y


def in_wedge(w, x, y, z):
    yaw = wedge_yaw(w)
    fx, fz = -math.sin(yaw), -math.cos(yaw)
    dx, dz = x - w['x'], z - w['z']
    al = dx * fx + dz * fz
    lat = -dx * fz + dz * fx
    if al < 0 or al > w['length'] or abs(lat) > w['width'] / 2.0:
        return False
    return 0 <= y <= w['rise'] * al / w['length']


def ray_clear(L, a, b, near=None):
    boxes = L.boxes if near is None else near[0]
    wedges = L.wedges if near is None else near[1]
    for mn, mx in boxes:
        if mx[1] <= mn[1]:
            continue
        if seg_box(a, b, mn, mx):
            return False
    if wedges:
        n = max(2, int(math.dist(a, b) / 0.2))
        for i in range(1, n):
            t = i / n
            p = (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t, a[2] + (b[2] - a[2]) * t)
            for w in wedges:
                if in_wedge(w, *p):
                    return False
    return True


def near_set(L, room, m=6.0):
    x0, z0, x1, z1 = room['rect']
    bx = [(mn, mx) for mn, mx in L.boxes if mx[0] >= x0 - m and mn[0] <= x1 + m and mx[2] >= z0 - m and mn[2] <= z1 + m]
    wd = [w for w in L.wedges if x0 - m <= w['x'] <= x1 + m and z0 - m <= w['z'] <= z1 + m]
    return bx, wd


# ---- the room's frame: the entry door, its inward normal, depth
def entry_frame(p, room):
    dr = idn.entry_door(p, room['id'])
    if dr is not None:
        cx, cz, nin = idn.door_frame(room, dr)
        return cx, cz, nin, dr['id']
    sp = room['spawn']['at']
    yaw = math.radians(room['spawn'].get('yawDeg', 0))
    return sp[0], sp[2], (-math.sin(yaw), -math.cos(yaw)), 'no door: the spawn'


def room_depth(room, nin):
    x0, z0, x1, z1 = room['rect']
    return (z1 - z0) if nin[0] == 0.0 else (x1 - x0)


def depth_of(cx, cz, nin, x, z):
    return (x - cx) * nin[0] + (z - cz) * nin[1]


def lat_of(cx, cz, nin, x, z):
    return -(x - cx) * nin[1] + (z - cz) * nin[0]


def floor_y(room):
    return room['spawn']['at'][1]


# ---- R5
def foothold(p, room):
    cx, cz, nin, _ = entry_frame(p, room)
    fy = floor_y(room)
    best = None
    for b in idn.zone_items(p, 'boxes', room['id']):
        mn, mx = b['min'], b['max']
        if mx[1] - fy <= EYE:
            continue
        x0, z0, x1, z1 = room['rect']
        if mn[0] <= x0 + 0.6 or mx[0] >= x1 - 0.6 or mn[2] <= z0 + 0.6 or mx[2] >= z1 - 0.6:
            continue  # a liner against the room's wall is the wall, not a foothold: the foothold stands free (you can go round it)
        ds = [depth_of(cx, cz, nin, x, z) for x in (mn[0], mx[0]) for z in (mn[2], mx[2])]
        d0, d1 = min(ds), max(ds)
        if d1 < 4.0 or d0 > 8.0:
            continue
        mid = (max(d0, 4.0) + min(d1, 8.0)) / 2.0
        score = abs(mid - 6.0)
        if best is None or score < best[0]:
            best = (score, b, d0, d1)
    if best is None:
        return None, 'no box above the eye (%.1f m) reaching 4-8 m inside the entry' % EYE
    b = best[1]
    return b, 'box %s..%s (%s, job %s): top %.1f m over the floor, %.1f-%.1f m inside' % (
        b['min'], b['max'], b.get('kind', '?'), b.get('job') or b.get('job36') or '-', b['max'][1] - fy, best[2], best[3])


def flank_eyes(p, room, fh):
    cx, cz, nin, _ = entry_frame(p, room)
    mn, mx = fh['min'], fh['max']
    fy = floor_y(room)
    lats = [lat_of(cx, cz, nin, x, z) for x in (mn[0], mx[0]) for z in (mn[2], mx[2])]
    dps = [depth_of(cx, cz, nin, x, z) for x in (mn[0], mx[0]) for z in (mn[2], mx[2])]
    dm = (max(min(dps), 4.0) + min(max(dps), 8.0)) / 2.0  # the flank at the foothold's 4-8 m band (a long wall: its near part)
    out = []
    for lat in (min(lats) - 0.8, max(lats) + 0.8):
        # back from lateral / depth to x, z: p = c + nin * depth + perp * lat, perp = (-nin.z, nin.x)
        x = cx + nin[0] * dm - nin[1] * lat
        z = cz + nin[1] * dm + nin[0] * lat
        x0, z0, x1, z1 = room['rect']
        if x0 + 0.3 <= x <= x1 - 0.3 and z0 + 0.3 <= z <= z1 - 0.3:
            out.append((x, fy + EYE, z))
    return out


# ---- wave 1's nodes and SpawnWave's placement odds
def wave_nodes(room, w):
    wn = room.get('waveNodes43')
    if wn and w < len(wn) and wn[w]:
        return [tuple(n) for n in wn[w]]
    return [tuple(n) for n in room.get('nodes', [])] + [tuple(n) for n in room.get('highNodes', [])]


def commit_feet(p, room):
    # Campaign24.Committed43: the first wave waits until the feet are commitM (3 m) from the room's spawn; the player comes in through the door,
    # so the feet at the spawn are the first point on the door's inward line, from the room's edge, that far from the spawn (the edge itself
    # when the spawn stands deeper than 3 m)
    sp = room['spawn']['at']
    cx, cz, nin, did = entry_frame(p, room)
    x0, z0, x1, z1 = room['rect']
    if did.startswith('no door'):
        return (sp[0] + nin[0] * COMMIT, sp[1], sp[2] + nin[1] * COMMIT)
    if nin[0] == 0.0:
        ex, ez = cx, (z1 if nin[1] < 0 else z0)
    else:
        ex, ez = (x1 if nin[0] < 0 else x0), cz
    for k in range(0, 200):
        x, z = ex + nin[0] * 0.1 * k, ez + nin[1] * 0.1 * k
        if math.hypot(x - sp[0], z - sp[2]) >= COMMIT:
            return (x, sp[1], z)
    return (ex, sp[1], ez)


def place_odds(nodes, feet):
    n = len(nodes)
    if n == 0:
        return []
    cnt = [0] * n
    for s in range(n):
        for t in range(n):
            k = (s + t) % n
            d = math.hypot(nodes[k][0] - feet[0], nodes[k][2] - feet[2])
            if t < n - 1 and d < MIN_SPAWN:
                continue
            cnt[k] += 1
            break
    return [c / float(n) for c in cnt]


def seen(L, near, eye, look, node, half=HALF_H):
    dx, dz = node[0] - eye[0], node[2] - eye[2]
    d = math.hypot(dx, dz)
    if d > 0.5:
        c = (dx * look[0] + dz * look[1]) / d
        if c < math.cos(math.radians(half)):
            return False
    for h in (1.0, 1.6):
        if ray_clear(L, eye, (node[0], node[1] + h, node[2]), near):
            return True
    return False


def expected_seen(L, near, eye, look, nodes, odds, n_enemies):
    return n_enemies * sum(pr for nd, pr in zip(nodes, odds) if pr > 0 and seen(L, near, eye, look, nd))


def entry_eye(room):
    f = (room.get('frames52') or {}).get('entry')
    if f:
        yaw = math.radians(f[3])
        return (f[0], f[1] + EYE, f[2]), (-math.sin(yaw), -math.cos(yaw)), f[4] if len(f) > 4 else 0.0
    sp = room['spawn']['at']
    yaw = math.radians(room['spawn'].get('yawDeg', 0))
    return (sp[0], sp[1] + EYE, sp[2]), (-math.sin(yaw), -math.cos(yaw)), 0.0


def seen_frustum(L, near, eye, look, pitch, node):
    # the a452 entry frame's camera: the horizontal half-angle round the yaw, the vertical 37 deg round the pitch (up +)
    if not seen(L, near, eye, look, node):
        return False
    for h in (1.0, 1.6):
        dy = node[1] + h - eye[1]
        dh = math.hypot(node[0] - eye[0], node[2] - eye[2])
        el = math.atan2(dy, max(dh, 1e-6))
        if abs(el - pitch) <= math.radians(FOV / 2.0):
            return True
    return False


def in_view(L, p, room, opening):
    near = near_set(L, room)
    nodes = wave_nodes(room, 0)
    waves = room.get('waves', [])
    n1 = len(waves[0]) if waves else 0
    feet = commit_feet(p, room)
    odds = place_odds(nodes, feet)
    use = [nd for nd, pr in zip(nodes, odds) if pr > 0]
    cx, cz, nin, did = entry_frame(p, room)
    fh, _ = foothold(p, room)
    ee, elook, epitch = entry_eye(room)
    # the entry frame (a156's number): the expected enemies seen in the frustum, the nearest node in use
    ent = n1 * sum(pr for nd, pr in zip(nodes, odds) if pr > 0 and seen_frustum(L, near, ee, elook, epitch, nd))
    dists = sorted(math.dist((ee[0], ee[1] - EYE, ee[2]), nd) for nd in use)
    entry_txt = '%.1f of %d seen, nearest %s m' % (ent, n1, '%.1f' % dists[0] if dists else '-')
    if not nodes or n1 == 0:
        return False, 'no wave-1 nodes or enemies', entry_txt, ent, dists
    in_view.fallback = None
    if opening in ('held', 'vista'):
        eyes = flank_eyes(p, room, fh) if fh is not None else []
        src = 'the foothold\'s flanks'
        if not eyes:
            eyes, src = [ee], 'the entry eye (no foothold flank)'
        best = max(expected_seen(L, near, e, nin, nodes, odds, n1) for e in eyes)
        ok = best >= 4.0 - 1e-9
        txt = '%s: %.1f of wave 1\'s %d expected seen from %s (>= 4 PASS); %d nodes, %d in use' % (opening, best, n1, src, len(nodes), len(use))
        if fh is None:
            # gate-r2 L1 (FAIL 1): the doneWhen's eye is the foothold's: with no foothold the clause is not measured (not a PASS); the entry-eye
            # reading is kept beside it for the report (in_view.fallback)
            in_view.fallback = ok
            return None, 'not measured: no foothold (R5 FAIL) | the entry-eye fallback: %s %s' % ('PASS' if ok else 'FAIL', txt), entry_txt, ent, dists
        return ok, txt, entry_txt, ent, dists
    mfx, mfz = cx + nin[0] * room_depth(room, nin) / 2.0, cz + nin[1] * room_depth(room, nin) / 2.0
    corr = (cx - nin[0] * 3.0, floor_y(room) + EYE, cz - nin[1] * 3.0)
    bad = []
    for nd in use:
        dm = math.hypot(nd[0] - mfx, nd[2] - mfz)
        dd = math.hypot(nd[0] - cx, nd[2] - cz)
        why = []
        if dm > 15.0:
            why.append('%.1f m from midfield' % dm)
        if not (8.0 <= dd <= 15.0):
            why.append('%.1f m from the door' % dd)
        if fh is not None:
            tgt = (nd[0], nd[1] + 1.0, nd[2])
            if seg_box(ee, tgt, fh['min'], fh['max']):
                why.append('behind the foothold')
            db = math.hypot(max(fh['min'][0] - nd[0], 0, nd[0] - fh['max'][0]), max(fh['min'][2] - nd[2], 0, nd[2] - fh['max'][2]))
            if db > BESIDE_M:
                why.append('%.1f m from the foothold (> %g: not beside it)' % (db, BESIDE_M))
        if any(ray_clear(L, (nd[0], nd[1] + h, nd[2]), corr, near) for h in (1.0, 1.6)):
            why.append('a line into the entry corridor')
        if why:
            bad.append('[%g,%g,%g] %s' % (nd[0], nd[1], nd[2], ', '.join(why)))
    ok = not bad
    if fh is None:
        in_view.fallback = ok
        return None, 'not measured: no foothold (R5 FAIL) | without the foothold clauses: %s, %d of %d nodes in use fail%s' % (
            'PASS' if ok else 'FAIL', len(bad), len(use), (': ' + '; '.join(bad)) if bad else ' (8-15 m from the door, <= 15 m from midfield, no line into the corridor)'), entry_txt, ent, dists
    return ok, '%s: %d of %d nodes in use fail%s' % (opening, len(bad), len(use), (': ' + '; '.join(bad)) if bad else ' (every one 8-15 m from the door, <= 15 m from midfield, within %g m of the foothold and not behind it, no line into the corridor)' % BESIDE_M), entry_txt, ent, dists


# ---- R2
def walk_cells(L, p, room, extra=None):
    # the combat floor: the nav nodes inside the room's rect (lava surfaces out) that are STANDABLE (a same-height grid neighbour along x AND
    # along z: a 1 m wall top or a ledge strip is not floor) and in the strongly connected part that holds the commit feet (you can get there
    # and back: a pit you cannot leave or a roof you cannot reach is not the room's floor); the rides (both faces, both ways, I2's ends) and
    # the pads (disc -> target) are directed edges of that graph. The cells join where a link joins two of their nodes (undirected).
    X, Y, Z, LS, LT = L.nav
    x0, z0, x1, z1 = room['rect']
    lava = idn.lava_of(p, room['id'])
    inl = lambda i: any(q[0] <= X[i] <= q[2] and q[1] <= Z[i] <= q[3] and Y[i] <= q[4] + 0.3 for q in lava)
    ids = [i for i in range(len(X)) if x0 <= X[i] <= x1 and z0 <= Z[i] <= z1 and not inl(i)]
    S = set(ids)
    def standable(i):
        ax = az = False
        for k in range(LS[i], LS[i + 1]):
            j = LT[k]
            if j not in S or abs(Y[j] - Y[i]) > 0.5:
                continue
            dx, dz = abs(X[j] - X[i]), abs(Z[j] - Z[i])
            if dx > 0.5 and dz < 0.5: ax = True
            if dz > 0.5 and dx < 0.5: az = True
        return ax and az
    S = set(i for i in ids if standable(i))
    out = {i: [LT[k] for k in range(LS[i], LS[i + 1]) if LT[k] in S] for i in S}
    nav = L.nav
    def add(a, c):
        if a in S and c in S and a != c:
            out[a].append(c)
    for b in idn.zone_items(p, 'boxes', room['id']):
        if not b.get('ride'):
            continue
        mn, mx = b['min'], b['max']
        along_z = (mx[2] - mn[2]) >= (mx[0] - mn[0])
        for off in ((mn[0] - 0.8, mx[0] + 0.8) if along_z else (mn[2] - 0.8, mx[2] + 0.8)):
            ends = [(off, mn[2] - 1.5), (off, mx[2] + 1.5)] if along_z else [(mn[0] - 1.5, off), (mx[0] + 1.5, off)]
            a = idn.nav_near(nav, room, ends[0][0], ends[0][1], ymax=mx[1] - 1.0)
            c = idn.nav_near(nav, room, ends[1][0], ends[1][1], ymax=mx[1] - 1.0)
            add(a, c); add(c, a)
    for pd in idn.pads_of(p, room['id']):
        a = idn.nav_near(nav, room, pd['at'][0], pd['at'][2], r=1.5, ymin=pd['at'][1] - 0.5, ymax=pd['at'][1] + 0.5)
        t = pd.get('target')
        if t is not None:
            add(a, idn.nav_near(nav, room, t[0], t[2], r=2.0, ymin=t[1] - 1.0, ymax=t[1] + 1.0))
    feet = commit_feet(p, room)
    f0 = min(S, key=lambda i: (X[i] - feet[0]) ** 2 + (Z[i] - feet[2]) ** 2 + 4 * (Y[i] - feet[1]) ** 2) if S else None
    def reach(adjf, src):
        seen_, st = {src}, [src]
        while st:
            v = st.pop()
            for w in adjf.get(v, ()):
                if w not in seen_:
                    seen_.add(w); st.append(w)
        return seen_
    rev = {}
    for v, ws in out.items():
        for w in ws:
            rev.setdefault(w, []).append(v)
    core = (reach(out, f0) & reach(rev, f0)) if f0 is not None else set()
    walk_cells.core = core  # the floor's nav nodes (n56-l-wave1.py picks spawn spots from them)
    key = lambda x, y, z: (int(math.floor((x - x0) / 2.0)), int(math.floor((z - z0) / 2.0)), int(round(y / 0.5)))
    cell = {i: key(X[i], Y[i], Z[i]) for i in core}
    adj = {}
    def link(a, b):
        if a == b:
            return
        adj.setdefault(a, set()).add(b)
        adj.setdefault(b, set()).add(a)
    for i in core:
        adj.setdefault(cell[i], set())
        for j in out[i]:
            if j in core:
                link(cell[i], cell[j])
    if extra:
        for a, b in extra:
            link(a, b)
    return adj


def bridges(adj):
    # Tarjan's bridges on the undirected cell graph (iterative)
    disc, low, out, t = {}, {}, 0, [0]
    for root in adj:
        if root in disc:
            continue
        stack = [(root, None, iter(adj[root]))]
        disc[root] = low[root] = t[0]; t[0] += 1
        while stack:
            v, par, it = stack[-1]
            nxt = next(it, None)
            if nxt is None:
                stack.pop()
                if par is not None:
                    low[par] = min(low[par], low[v])
                    if low[v] > disc[par]:
                        out += 1
                continue
            if nxt == par:
                continue
            if nxt in disc:
                low[v] = min(low[v], disc[nxt])
            else:
                disc[nxt] = low[nxt] = t[0]; t[0] += 1
                stack.append((nxt, v, iter(adj[nxt])))
    return out


def r2(L, p, room, extra=None):
    adj = walk_cells(L, p, room, extra)
    x0, z0 = room['rect'][0], room['rect'][1]
    dead = [c for c, n in adj.items() if len(n) == 1]
    live = [c for c, n in adj.items() if n]
    nb = bridges({c: n for c, n in adj.items() if n})
    where = ', '.join('(%g, %g, y %g)' % (x0 + 2 * c[0] + 1, z0 + 2 * c[1] + 1, c[2] * 0.5) for c in dead[:4])
    return not dead, '%d dead-end cells of %d (2 m x 0.5 m cells; %d bridges)%s' % (len(dead), len(live), nb, (': ' + where) if dead else '')


# ---- R8
def r8(p, room):
    S = idn.solids(p, room['id'])
    un = [it for _, _, it in S if not (str(it.get('job') or '').strip() or str(it.get('job36') or '').strip())]
    what = ', '.join(('box %s..%s' % (it['min'], it['max'])) if 'min' in it else ('ramp %s->%s' % (it['from'], it['to'])) for it in un[:3])
    return not un, '%d untagged of %d solids%s' % (len(un), len(S), (': ' + what) if un else '')


def room_report(L, rid, opening):
    p = L.plan
    room = idn.room_of(p, rid)
    a, at = r2(L, p, room)
    fh, ft = foothold(p, room)
    b = fh is not None
    c, ct = r8(p, room)
    d, dt, et, ent, dists = in_view(L, p, room, opening)
    return [('R2 no dead ends', a, at), ('R5 foothold', b, ft), ('R8 every job', c, ct), ('IN VIEW ' + opening, d, dt)], et, ent, dists


def tag(ok):
    return idn.tag(ok)


def openings():
    if LEVEL_DIR and os.path.exists(os.path.join(LEVEL_DIR, 'openings67.json')):
        return json.load(open(os.path.join(LEVEL_DIR, 'openings67.json'), encoding='utf-8'))
    return json.load(open(idn.OPENINGS, encoding='utf-8'))


def table(items, rev=None):
    ops = openings()
    print('| room | opening | R2 dead ends | R5 foothold | R8 untagged | in view | w1 entry (a156) | identity I1-I4 | verdict |')
    print('|---|---|---|---|---|---|---|---|---|')
    allok, n_view, n_rooms, n_fall = True, 0, 0, 0
    levels = []
    for it in items:
        lv, rooms = it.split(':')
        levels.append(lv)
        L = Level(lv, rev)
        for rid in rooms.split(','):
            op = ops.get(lv, {}).get(rid, '-')
            ls, et, ent, dists = room_report(L, rid, op)
            ident = idn.room_lines(L.plan, rid, lv) if rev is None else []
            ifails = [n.split()[0] for n, ok, _ in ident if ok is False]
            fails = [n.split()[0] for n, ok, _ in ls if ok is False]
            n_rooms += 1
            n_view += 1 if ls[3][1] else 0
            n_fall += 1 if (ls[3][1] or (ls[3][1] is None and in_view.fallback)) else 0
            allok = allok and not fails
            short = lambda t: t.split(' (')[0] if t else '-'
            print('| %s %s | %s | %s %s | %s %s | %s %s | %s %s | %s | %s | %s |' % (
                lv, rid, op, tag(ls[0][1]), ls[0][2].split(' (')[0], tag(ls[1][1]), ('%.0f m top' % (0,) if False else ls[1][2].split(':')[0] if ls[1][1] is False else ls[1][2].split(' (')[0]),
                tag(ls[2][1]), short(ls[2][2]), tag(ls[3][1]), ls[3][2][:220], et, ('FAIL ' + ','.join(ifails)) if ifails else ('PASS' if ident else 'n/c'),
                'FAIL ' + ','.join(fails) if fails else 'PASS'))
    print('')
    print('IN VIEW PASS %d / %d rooms (L2 doneWhen: >= 15 of 18; a room with no foothold is not measured, not a PASS)' % (n_view, n_rooms))
    print('IN VIEW with the entry-eye fallback where there is no foothold (the r2 reading): %d / %d' % (n_fall, n_rooms))
    if rev is None:
        print('')
        print('| level | I5 ring rooms (<= 2) | I7 openings |')
        print('|---|---|---|')
        for lv in levels:
            p = idn.load_plan(idn.plan_path(lv))
            ok5, rings, _ = idn.i5(p)
            ok7, t7 = idn.i7(p, lv)
            print('| %s | %s %d (%s) | %s %s |' % (lv, tag(ok5), len(rings), ', '.join(rings) or 'none', tag(ok7), t7))
    return allok


def planted():
    res = []
    # R2: a copy of iceline g2 (u3-n52, NIGHT52's cut) with a 1-cell spur planted on its walk graph: three cells in a line off one floor cell
    L = Level('iceline', 'u3-n52')
    room = idn.room_of(L.plan, 'g2')
    adj = walk_cells(L, L.plan, room)
    c0 = next(c for c, n in adj.items() if c[2] == 0 and len(n) >= 3)
    spur = [(c0[0], c0[1], 40 + k) for k in range(3)]
    extra = [(c0, spur[0]), (spur[0], spur[1]), (spur[1], spur[2])]
    ok, t = r2(L, L.plan, room, extra)
    res.append(('R2 iceline g2 @u3-n52 + a 3-cell spur', ok, t))
    # R5: a copy of a room that has a foothold, its foothold box taken out
    for lv, rid in (('foundry', 'a3'),):
        L = Level(lv, 'u3-n52')
        p = L.plan
        room = idn.room_of(p, rid)
        fh, _ = foothold(p, room)
        taken = 0
        while fh is not None and taken < 12:
            p['boxes'] = [b for b in p['boxes'] if b is not fh]
            taken += 1
            fh, ft = foothold(p, room)
        res.append(('R5 %s %s @u3-n52, %d footholds out' % (lv, rid, taken), fh is not None, ft))
    # R8: a copy of kiln kb3 @u3-n52 with one solid's job fields taken out
    L = Level('kiln', 'u3-n52')
    p = L.plan
    room = idn.room_of(p, 'kb3')
    S = idn.solids(p, 'kb3')
    it = S[0][2]
    it.pop('job', None); it.pop('job36', None)
    ok, t = r8(p, room)
    res.append(('R8 kiln kb3 @u3-n52, one job out', ok, t))
    # IN VIEW: rooms with a foothold, their wave-1 spots as NIGHT52 / L1 left them (veteran #4: 1-4 of 7-14 seen): kb3 at 0e3e6024 (L1's
    # RED table: a foothold, 2.2 of 5 seen; at u3-n52 it had none), a3 at u3-n52 (g4, the first cut's held plant, has no foothold: not measured)
    ops = openings()
    for lv, rid, rev in (('kiln', 'kb3', '0e3e6024'), ('foundry', 'a3', 'u3-n52')):
        L = Level(lv, rev)
        room = idn.room_of(L.plan, rid)
        ok, t, et, _, _ = in_view(L, L.plan, room, ops[lv][rid])
        res.append(('IN VIEW %s %s @%s (%s)' % (lv, rid, rev, ops[lv][rid]), ok, t + ' | entry ' + et))
    # gate-r2 L1: a room with no foothold is NOT MEASURED (None), never a PASS: a3 @u3-n52 with its footholds taken out (planted), and the
    # beside rule (wave-1 nodes > BESIDE_M from the foothold box FAIL): a3 @u3-n52 as it stands (its nodes 20-27 m from the door)
    L = Level('foundry', 'u3-n52')
    p = L.plan
    room = idn.room_of(p, 'a3')
    fh, _ = foothold(p, room)
    while fh is not None:
        p['boxes'] = [b for b in p['boxes'] if b is not fh]
        fh, _ = foothold(p, room)
    ok, t, et, _, _ = in_view(L, p, room, ops['foundry']['a3'])
    res.append(('IN VIEW foundry a3 @u3-n52, no foothold', ok, t))
    # the beside rule alone: span p3 at 626dc329 (L2's spots; its only failing clause is one node 11.9 m from the foothold box)
    L = Level('span', '626dc329')
    room = idn.room_of(L.plan, 'p3')
    ok, t, et, _, _ = in_view(L, L.plan, room, ops['span']['p3'])
    res.append(('IN VIEW span p3 @626dc329 (beside, %g m)' % BESIDE_M, ok, t))
    good = True
    for name, ok, t in res:
        nm = name.endswith('no foothold')
        want = ok is None if nm else ok is False  # the no-foothold plant must read "not measured"; every other plant FAIL
        print('  %s %-44s %s%s' % (tag(ok), name, t[:400], '' if want else ('   <- SHOULD BE NOT MEASURED' if nm else '   <- SHOULD FAIL')))
        good = good and want
    print('PLANTED: %s' % ('every check FAILs its planted case (trusted)' if good else 'a planted case did not FAIL (that check is not trusted)'))
    return good


def wedge_planted():
    """NIGHT74 (S's finding): a rotated wedge must block a ray across its HIGH end and must not block one over its LOW end, for every yaw the level files carry (0, +-pi / 2, pi; radians).
    A ramp 6 m long, 4 m wide, rising 3 m from (0, 0) along its yaw (0 = toward -z, ArenaMap.WedgeCorners); the rays run at the eye's 1.6 m: the high end is 2.5 m tall where the
    "high" ray crosses (5 m up the ramp), the low end 0.5 m where the "low" ray crosses (1 m up). Run it with --legacy-wedge-degrees to see the old reading fail the same cases."""
    class Stub:  # a Level with a wedge and no box
        boxes = []
        wedges = []

    good = True
    s2 = math.pi / 2
    cases = []
    # (yaw, a name, the ray's start and end, expected clear)
    for yaw, name, hi, lo in (
        (0.0, 'yaw 0 (rises toward -z)', ((-8, 1.6, -5), (8, 1.6, -5)), ((-8, 1.6, -1), (8, 1.6, -1))),
        (s2, 'yaw +pi/2 (rises toward -x)', ((-5, 1.6, -8), (-5, 1.6, 8)), ((-1, 1.6, -8), (-1, 1.6, 8))),
        (-s2, 'yaw -pi/2 (rises toward +x)', ((5, 1.6, -8), (5, 1.6, 8)), ((1, 1.6, -8), (1, 1.6, 8))),
        (math.pi, 'yaw pi (rises toward +z)', ((-8, 1.6, 5), (8, 1.6, 5)), ((-8, 1.6, 1), (8, 1.6, 1))),
    ):
        cases.append((name + ': a ray across the HIGH end is blocked', yaw, hi, False))
        cases.append((name + ': a ray over the LOW end is clear', yaw, lo, True))
    print('wedge planted (%s reading):' % ('LEGACY degrees' if LEGACY_WEDGE_DEGREES else 'radians, as the level files carry'))
    for label, yaw, (a, b), want_clear in cases:
        L = Stub()
        L.wedges = [{'x': 0.0, 'z': 0.0, 'yaw': yaw, 'length': 6.0, 'width': 4.0, 'rise': 3.0}]
        got = ray_clear(L, a, b)
        ok = got == want_clear
        good = good and ok
        print('  %s %-66s ray_clear %s (want %s)' % ('ok  ' if ok else 'FAIL', label, got, want_clear))
    print('WEDGE PLANTED: %s' % ('every rotated wedge blocks and clears as the sim collider would' if good else 'a rotated wedge is misplaced (the check cannot be trusted on a ramp that does not rise toward -z)'))
    return good


def agree(paths):
    # the a156 lines (FeelReel.N56.Room.cs) against this checker's IN VIEW: the a156 verdict by the room's opening (held / vista: seen >= 4;
    # ambushed / ambusher: seen >= 1 and the nearest seen <= 15 m), one row per room; agreement over the foundry / iceline / kiln rooms (L2)
    import re
    ops = openings()
    lvl_of_room = {rid: lv for lv in ops if not lv.startswith('_') for rid in ops[lv]}
    got = {}
    for pth in paths:
        for line in open(pth, encoding='utf-8', errors='replace'):
            mm = re.search(r'\[reel\] a156: (\S+) entry .*?\| wave1 (\d+) \| alive (\d+) \| seen (\d+) \| nearest (\S+) \| second (\S+) \| nearestAlive (\S+) \| drawn (\d+)', line)
            if mm:
                got[mm.group(1)] = mm.groups()
    print('| room | opening | a156 seen / nearest / drawn | a156 verdict | n56-room IN VIEW | agree |')
    print('|---|---|---|---|---|---|')
    n = k = n3 = k3 = 0
    cache = {}
    for rid, g in got.items():
        lv = lvl_of_room.get(rid)
        if lv is None:
            continue
        op = ops[lv][rid]
        seen, near = int(g[3]), (float(g[4]) if g[4] != '-' else float('inf'))
        a = seen >= 4 if op in ('held', 'vista') else (seen >= 1 and near <= 15.0)
        if lv not in cache:
            cache[lv] = Level(lv)
        L = cache[lv]
        ok, t, et, _, _ = in_view(L, L.plan, idn.room_of(L.plan, rid), op)
        same = (a == ok)
        n += 1; k += same
        if lv in ('foundry', 'iceline', 'kiln'):
            n3 += 1; k3 += same
        print('| %s %s | %s | %s / %s m / %s | %s | %s | %s |' % (lv, rid, op, seen, g[4], g[7], tag(a), tag(ok), 'yes' if same else 'NO'))
    print('')
    print('AGREE foundry / iceline / kiln: %d / %d rooms (%s; L2: >= 80 %%); all levels %d / %d' % (k3, n3, ('%.0f %%' % (100.0 * k3 / n3)) if n3 else '-', k, n))
    return n3 > 0 and k3 >= 0.8 * n3


def main():
    global LEGACY_WEDGE_DEGREES, LEVEL_DIR
    a = sys.argv[1:]
    if '--legacy-wedge-degrees' in a:
        LEGACY_WEDGE_DEGREES = True
        a = [x for x in a if x != '--legacy-wedge-degrees']
    if '--level-dir' in a:
        i = a.index('--level-dir'); LEVEL_DIR = a[i + 1]; a = a[:i] + a[i + 2:]
    rev = None
    if '--rev' in a:
        i = a.index('--rev'); rev = a[i + 1]; a = a[:i] + a[i + 2:]
    if not a:
        print(open(__file__, encoding='utf-8').read().split('import importlib')[0]); sys.exit(2)
    if a[0] == 'table':
        print('== n56-room table%s (eye %.1f m, fov %.0f deg -> horizontal half %.1f deg, minSpawnDist %.0f m, commit %.0f m)' % (
            (' @' + rev) if rev else '', EYE, FOV, HALF_H, MIN_SPAWN, COMMIT))
        ok = table(a[1:] or ALL18, rev)
        sys.exit(0 if ok else 1)
    if a[0] == 'room':
        lv, rid = a[1], a[2]
        L = Level(lv, rev)
        op = openings().get(lv, {}).get(rid, '-')
        ls, et, ent, dists = room_report(L, rid, op)
        print('== n56-room room %s %s (%s)%s' % (lv, rid, op, (' @' + rev) if rev else ''))
        for name, ok, t in ls:
            print('  %s %-20s %s' % (tag(ok), name, t))
        print('  w1 entry frame (a156): %s; node distances %s' % (et, ', '.join('%.1f' % d for d in dists)))
        if rev is None:
            for name, ok, t in idn.room_lines(L.plan, rid, lv):
                print('  %s %-20s %s' % (tag(ok), name, t[:300]))
        fails = [n for n, ok, _ in ls if ok is False]
        print('ROOM %s: %s' % (rid, 'PASS' if not fails else 'FAIL ' + ', '.join(fails)))
        sys.exit(1 if fails else 0)
    if a[0] == 'agree':
        sys.exit(0 if agree(a[1:]) else 1)
    if a[0] == 'planted':
        sys.exit(0 if planted() else 1)
    if a[0] == 'wedge-planted':
        sys.exit(0 if wedge_planted() else 1)
    print('unknown mode ' + a[0]); sys.exit(2)


if __name__ == '__main__':
    main()
