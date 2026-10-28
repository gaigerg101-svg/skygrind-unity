# NIGHT74 builder S (S0r; Research/2026-10-27-forge74/scout-rooms.md, rules 1-15): THE ROOM RULES METER.
# One combat room's plan data (boxes, ramps, pads, pits, spawn, waves), no sim, no level file. A GUARD both room builders share, never the
# verdict (NIGHT74.md D-3 / D-8): the lead's two-build pair and the blind spotter judge. Content read from files is data.
#
# CORE (the five checks the slate names; every one prints PASS or RED):
#   PLANES  2-3 floor planes (scout 2 [P]; brief: 2-3). A plane is a cluster of standable heights (a floor cell, a tier's top of 8 m2 or more with
#           a 2 m side, a pit floor), merged when within 1.0 m of the cluster's lowest member, counted when 4 or more cells stand on it.
#           1 plane = a flat room (RED); 4 or more = a fourth plane adds only another middle layer (RED).
#   COVER   cover classes by height (scout 6 [P]: high >= 1.75 m, low 1.0-1.25 m, <= 0.5 m is not cover). Free-standing solids (not within
#           0.3 m of the room's rect on any side) classified by height: none (< 0.5), low, high, between. PASS: at least one free-standing high or
#           low piece, and no between-height piece or high piece without a job (a `job` that is empty or only deco / filler / prop / clutter).
#   LOOPS   circuits to run (scout 7 [P] "nav-graph cycle basis >= 2"; brief: at least two). Two kinds, summed:
#           floor holes = the 1 m cell grid of the floor plane (the base floor and any 1.3 m-or-lower hoppable top), 4-neighbour links, holes of that
#           region = E - V + C - F (F = filled 1 m squares): a ring round a solid you cannot hop (a pillar, a tier, a pit), not a solid on the wall;
#           pad / ride routes = a jump pad or a ride line (line65) whose landing can walk or drop back to its start (a closed route).
#   REAR    spawn points in the entry's rear arc (scout 10 [rule P, check D]): wave 1's usable nodes (>= minSpawnDist 12 m from the spawn, else every
#           node) whose bearing from the entry spawn differs from its facing by more than 90 deg. PASS: 0.
#   VIEW    an enemy in view within 2 s of entry (brief R2; scout 9): a usable wave-1 node is seen from the eye (spawn + 1.6 m) inside the camera's
#           horizontal half-angle 53.3 deg (fov 74 vertical at 16:9) and vertical half-angle 37 deg, the ray clear (boxes and ramps) to the node's body
#           (+1.0 m) or head (+1.6 m); at t = 0, else walking straight ahead 8 m/s along the facing up to 2 s (0.25 s steps).
# EXTRA (printed, counted apart; each PASS or RED):
#   CROSS   the room rect's longest chord (its diagonal) at dash speed 24 m/s <= 2.0 s (brief R2; scout 1 [D]); the walk time and "blink" (< 0.5 s)
#           are printed.
#   EXPOSE  scout 5 [D]: the fraction of floor-sample pairs (3 m grid, eye to eye, 3 m or more apart) with a clear line <= 60 %; the longest clear
#           line is printed (scout 14: a small room's longest line sits in TF2's medium band, 19.5 m: printed, not gated).
#   CLIMB   scout 3 + GG NIGHT20 ("a route enemies can climb up"): every tier (a connected group of tier cells) is reached from the spawn by
#           walker links (steps and slopes <= 0.95 m, drops). PASS: 0 tiers unreached.
# Numbers: PortExport/config.json (eyeHeight, capsuleRadius, capsuleHeight, walkSpeed, dash.speed, jump.height, camera.fovDeg) and
# Design/unity-config.json design.n24.campaign.minSpawnDist (as Tools/night/n56-room.py load_cfg); the thresholds below are the scout's, tagged.
#
#   python -I Tools/night/n74-room-rules.py room <room.json | plan.json:<room id>> [--rev <git rev>]    every number for one room
#   python -I Tools/night/n74-room-rules.py table [--out <file>]                                          every room of n52 / n57 / n58 / n65 / n67
#   python -I Tools/night/n74-room-rules.py planted [--out <file>]                                        the reference room PASSes; each planted
#                                                                                                         defect turns its check RED; exit 0 only
#                                                                                                         when every one did (L-091)
# A room file is Design/levels/n6x/.../rooms/<id>.json ({row, boxes, ramps, pads, pits, ...}); a plan is Design/levels/<px>/<level>.plan.json
# (rooms[] with the zone-tagged boxes, ramps, pads, pits).
import copy, glob, json, math, os, subprocess, sys, time
import numpy as np
sys.dont_write_bytecode = True

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..'))

# thresholds (scout tags: [P] published, [D] derived by the scout or this meter)
T = {
    'planes_min': 2, 'planes_max': 3,            # scout 2 [P]; brief R2
    'plane_merge_m': 1.0,                         # a 1.0 m step is a hop, not a plane (levelgen's walkers' tier is 0.9 m)
    'plane_min_cells': 4,                         # a plane needs 4 cells (4 m2) standing on it
    'tier_min_area': 8.0, 'tier_min_dim': 2.0,    # a tier's top: 8 m2 and a 2 m side (a hall is >= 2.0 m: scout 14 [P])
    'hop_m': 1.3,                                 # a solid up to 1.3 m is hoppable (jump.height 1.5 m; levelgen: a 1 m top is not a walkers' tier)
    'cover_low': (1.0, 1.25), 'cover_high': 1.75, 'cover_none': 0.5,   # scout 6 [P]
    'free_gap_m': 0.3,                            # free-standing: further than 0.3 m from every side of the rect
    'loops_min': 2,                               # scout 7 [P]; brief R2
    'walker_step_m': 0.95,                        # walkers climb 0.9 m, not 1.0 m (levelgen)
    'player_up_m': 1.4,                           # jump.height 1.5 m less a margin
    'rear_deg': 90.0,                             # scout 10: behind = more than 90 deg off the facing
    'view_s': 2.0, 'view_step_s': 0.25,           # brief R2
    'cross_s': 2.0, 'blink_s': 0.5,               # brief R2; scout 1 [D]
    'expose_max': 0.60, 'expose_grid_m': 3.0, 'expose_min_pair_m': 3.0, 'ray_step_m': 0.4,   # scout 5 [D]
    'expose_max_pts': 80,                         # at most 80 floor samples (3160 pairs) a room: a bigger room is sampled evenly
    'long_line_m': 19.5,                          # scout 14: TF2 medium range, printed only
    'cell_m': 1.0, 'grid_shift_m': 0.013,         # the cell grid, shifted so a cell centre never sits on a metre line
}


def jget(o, dotted):
    for k in dotted.split('.'):
        o = o[k]
    return float(o)


def load_cfg():
    port = json.load(open(os.path.join(ROOT, 'PortExport', 'config.json'), encoding='utf-8'))
    uc = json.load(open(os.path.join(ROOT, 'Design', 'unity-config.json'), encoding='utf-8'))
    fov = jget(port, 'camera.fovDeg')
    half_v = fov / 2.0
    half_h = math.degrees(math.atan(math.tan(math.radians(half_v)) * 16.0 / 9.0))
    return {'eye': jget(port, 'body.eyeHeight'), 'r': jget(port, 'body.capsuleRadius'), 'hgt': jget(port, 'body.capsuleHeight'),
            'walk': jget(port, 'ground.walkSpeed'), 'dash': jget(port, 'dash.speed'), 'jump': jget(port, 'jump.height'),
            'half_h': half_h, 'half_v': half_v, 'min_spawn': jget(uc, 'design.n24.campaign.minSpawnDist')}


CFG = None


def cfg():
    global CFG
    if CFG is None:
        CFG = load_cfg()
    return CFG


class Box:
    def __init__(self, b, rect):
        self.src = b
        self.min = np.array(b['min'], float)
        self.max = np.array(b['max'], float)
        self.kind = b.get('kind', '')
        self.job = (b.get('job') or b.get('job36') or '').strip()
        self.dx = self.max[0] - self.min[0]
        self.dz = self.max[2] - self.min[2]
        self.h = self.max[1] - self.min[1]
        self.area = self.dx * self.dz
        self.mindim = min(self.dx, self.dz)
        x0, z0, x1, z1 = rect
        g = T['free_gap_m']
        self.perim = (self.min[0] - x0 < g or x1 - self.max[0] < g or self.min[2] - z0 < g or z1 - self.max[2] < g)

    def covers(self, x, z, grow=0.0):
        return self.min[0] - grow < x < self.max[0] + grow and self.min[2] - grow < z < self.max[2] + grow


class Ramp:
    def __init__(self, r, floor_y):
        self.a = np.array(r['from'], float)     # the low end's centre [x, z]
        self.b = np.array(r['to'], float)       # the high end's centre [x, z]
        self.w = float(r.get('width', 2.0))
        self.rise = float(r['rise'])
        self.base = float(r.get('y', floor_y))
        d = self.b - self.a
        self.len = float(np.hypot(d[0], d[1]))
        self.u = d / self.len if self.len > 0 else np.array([1.0, 0.0])
        self.job = (r.get('job') or '').strip()

    def height_at(self, x, z):
        """the ramp surface's height above its base at (x, z), None outside its footprint"""
        p = np.array([x, z]) - self.a
        t = float(p @ self.u)
        v = float(p[0] * -self.u[1] + p[1] * self.u[0])
        if t < 0 or t > self.len or abs(v) > self.w / 2.0:
            return None
        return self.rise * t / self.len


class Room:
    def __init__(self, label, row, boxes, ramps, pads, pits, props=()):
        self.label = label
        self.row = row
        self.id = row['id']
        self.rect = [float(v) for v in row['rect']]
        self.height = float(row.get('height', 8))
        sp = row['spawn']
        self.spawn = np.array(sp['at'], float)
        self.yaw = float(sp.get('yawDeg', 0.0))
        fr = (row.get('frames52') or {}).get('entry')
        self.look_pitch = float(fr[4]) if fr and len(fr) >= 5 else 0.0     # radians (frames52.entry: x, y, z, yawDeg, pitch)
        self.floor_y = float(self.spawn[1])
        self.boxes = [Box(b, self.rect) for b in boxes]
        self.ramps = [Ramp(r, self.floor_y) for r in ramps]
        self.pads = pads
        self.pits = pits
        self.props = list(props)
        self.waves = row.get('waves') or []
        wn = row.get('waveNodes43')
        allp = [np.array(n, float) for n in (row.get('nodes') or []) + (row.get('highNodes') or [])]
        if wn:
            # a wave whose entry is null / empty takes every node (Campaign24.cs: wave 1 = waveNodes43[0] when set, else nodes + highNodes)
            self.wave_nodes = [[np.array(n, float) for n in w] if w else list(allp) for w in wn]
        else:
            self.wave_nodes = [list(allp) for _ in (self.waves or [0])]
        self.line65 = row.get('line65')
        self.verb = row.get('verb65', '')
        self._build_los()
        self._build_grid()

    # ---------- facing ----------
    def facing(self):
        a = math.radians(self.yaw)
        return np.array([-math.sin(a), -math.cos(a)])   # yaw 0 faces north (-z), +yaw turns left (west): LEVEL-FORMAT.md

    # ---------- line of sight ----------
    def _build_los(self):
        if self.boxes:
            self.bmin = np.array([b.min for b in self.boxes])
            self.bmax = np.array([b.max for b in self.boxes])
        else:
            self.bmin = np.zeros((0, 3))
            self.bmax = np.zeros((0, 3))

    def blocked(self, a, b):
        a = np.asarray(a, float)
        b = np.asarray(b, float)
        d = b - a
        L = float(np.linalg.norm(d))
        n = max(2, int(L / T['ray_step_m']))
        t = (np.arange(1, n) / n)[:, None]
        P = a[None, :] + d[None, :] * t
        if len(self.bmin):
            inside = ((P[None, :, :] > self.bmin[:, None, :] + 1e-6) & (P[None, :, :] < self.bmax[:, None, :] - 1e-6)).all(axis=2)
            if inside.any():
                return True
        for r in self.ramps:
            p = P[:, [0, 2]] - r.a[None, :]
            tt = p @ r.u
            vv = p[:, 0] * -r.u[1] + p[:, 1] * r.u[0]
            ok = (tt >= 0) & (tt <= r.len) & (np.abs(vv) <= r.w / 2.0)
            if ok.any():
                h = r.base + r.rise * tt / r.len
                if ((P[:, 1] < h) & ok).any():
                    return True
        return False

    # ---------- the grid ----------
    def _pit_at(self, x, z):
        for p in self.pits:
            r = p.get('rect')
            if r and r[0] < x < r[2] and r[1] < z < r[3]:
                return float(p.get('floorY', self.floor_y - 2.0))
        return None

    def _surfaces(self, x, z):
        """the standable surfaces at a cell centre: [(height, src)], src in floor, pit, step, tier, ramp"""
        c = cfg()
        base = self._pit_at(x, z)
        src_base = 'pit' if base is not None else 'floor'
        if base is None:
            base = self.floor_y
        ints = []
        for b in self.boxes:
            if b.covers(x, z):
                ints.append((float(b.min[1]), float(b.max[1]), b))
        for r in self.ramps:
            h = r.height_at(x, z)
            if h is not None and h > 0.05:
                ints.append((r.base, r.base + h, r))
        out = []

        def clear_above(y, skip=None):
            ups = [lo for lo, hi, o in ints if o is not skip and lo > y + 0.05]
            top = min(ups) if ups else self.floor_y + self.height
            return top - y >= c['hgt'] - 0.05

        def inside_other(y, skip):
            return any(lo + 0.05 < y < hi - 0.05 for lo, hi, o in ints if o is not skip)

        occupied = any(lo <= base + 0.1 < hi - 0.05 for lo, hi, o in ints)
        if not occupied and clear_above(base):
            out.append((base, src_base))
        for lo, hi, o in ints:
            if inside_other(hi, o) or not clear_above(hi, o):
                continue
            rel = hi - base
            if isinstance(o, Ramp):
                out.append((hi, 'ramp'))
            elif rel <= T['hop_m'] and o.mindim >= 1.0 and o.kind != 'wall':
                out.append((hi, 'step'))
            elif o.kind in ('deck', 'slab') and o.area >= T['tier_min_area'] and o.mindim >= T['tier_min_dim']:
                out.append((hi, 'tier'))
        out.sort()
        ded = []
        for h, s in out:
            if ded and abs(ded[-1][0] - h) < 0.1:
                if s == 'tier' or s == 'step':
                    ded[-1] = (ded[-1][0], s)
                continue
            ded.append((h, s))
        return ded

    def _build_grid(self):
        cs = T['cell_m']
        x0, z0, x1, z1 = self.rect
        sh = T['grid_shift_m']
        self.nx = int((x1 - x0) // cs)
        self.nz = int((z1 - z0) // cs)
        self.cx = [x0 + cs / 2 + sh + cs * i for i in range(self.nx)]
        self.cz = [z0 + cs / 2 + sh + cs * j for j in range(self.nz)]
        self.nodes = {}      # (i, j) -> [(h, src)]
        for i in range(self.nx):
            for j in range(self.nz):
                s = self._surfaces(self.cx[i], self.cz[j])
                if s:
                    self.nodes[(i, j)] = s
        # edges: out[a] = {b: type}; types W (walk, both ways), D (drop), U (jump up), P (pad), R (ride)
        self.out = {}

        def add(a, b, ty):
            self.out.setdefault(a, {})
            self.out.setdefault(b, {})
            if b not in self.out[a]:
                self.out[a][b] = ty

        wstep, pup = T['walker_step_m'], T['player_up_m']
        for (i, j), sa in self.nodes.items():
            for di, dj in ((1, 0), (0, 1), (-1, 0), (0, -1)):
                nb = self.nodes.get((i + di, j + dj))
                if not nb:
                    continue
                for ka, (ha, _) in enumerate(sa):
                    a = (i, j, ka)
                    below = [(hb, kb) for kb, (hb, _) in enumerate(nb) if hb - ha < -wstep]
                    above = [(hb, kb) for kb, (hb, _) in enumerate(nb) if wstep < hb - ha <= pup]
                    for kb, (hb, _) in enumerate(nb):
                        b = (i + di, j + dj, kb)
                        if abs(hb - ha) <= wstep:
                            add(a, b, 'W')
                    if below:
                        hb, kb = max(below)
                        add(a, (i + di, j + dj, kb), 'D')
                    if above:
                        hb, kb = min(above)
                        add(a, (i + di, j + dj, kb), 'U')
        # pads and ride lines: non-adjacent connectors
        self.connectors = []
        for p in self.pads:
            a = self._node_near(p['at'], 1.2)
            b = self._node_near(p['target'], 1.6)
            if a is not None and b is not None:
                add(a, b, 'P')
                self.connectors.append(('pad', a, b))
        if self.line65 and len(self.line65) >= 2 and self.verb in ('ride',):
            a = self._node_near(self.line65[0], 1.2)
            b = self._node_near(self.line65[-1], 1.6)
            if a is not None and b is not None and a != b:
                add(a, b, 'R')
                self.connectors.append(('ride', a, b))

    def _cell_of(self, x, z):
        i = int((x - self.rect[0]) // T['cell_m'])
        j = int((z - self.rect[1]) // T['cell_m'])
        return i, j

    def _node_near(self, pos, dy):
        i, j = self._cell_of(pos[0], pos[2])
        best = None
        for di in (0, -1, 1):
            for dj in (0, -1, 1):
                nb = self.nodes.get((i + di, j + dj))
                if not nb:
                    continue
                for k, (h, _) in enumerate(nb):
                    d = abs(h - pos[1]) + 0.3 * (abs(di) + abs(dj))
                    if abs(h - pos[1]) <= dy and (best is None or d < best[0]):
                        best = (d, (i + di, j + dj, k))
        return best[1] if best else None

    def surf(self, n):
        return self.nodes[(n[0], n[1])][n[2]]

    def reach(self, start, types, targets=None):
        seen = {start}
        stack = [start]
        while stack:
            a = stack.pop()
            for b, ty in self.out.get(a, {}).items():
                if ty in types and b not in seen:
                    seen.add(b)
                    stack.append(b)
        return seen


# ---------------------------------------------------------------- the checks
def check_planes(R):
    cnt = {}
    for n, s in R.nodes.items():
        for h, src in s:
            if src in ('floor', 'tier', 'pit'):
                cnt.setdefault(round(h, 2), 0)
                cnt[round(h, 2)] += 1
    hs = sorted(cnt)
    planes = []
    i = 0
    while i < len(hs):
        lo = hs[i]
        grp = [h for h in hs if lo <= h <= lo + T['plane_merge_m']]
        cells = sum(cnt[h] for h in grp)
        planes.append((lo, cells))
        i += len(grp)
    planes = [(h, c) for h, c in planes if c >= T['plane_min_cells']]
    n = len(planes)
    ok = T['planes_min'] <= n <= T['planes_max']
    return ok, 'planes %d (%s)' % (n, ', '.join('%.1f m: %d cells' % (h, c) for h, c in planes)), {'n': n, 'planes': planes}


def cover_class(b):
    h = b.h
    if h < T['cover_none']:
        return 'none'
    if T['cover_low'][0] <= h <= T['cover_low'][1]:
        return 'low'
    if h >= T['cover_high']:
        return 'high'
    return 'between'


GENERIC_JOBS = ('', 'deco', 'decor', 'filler', 'prop', 'props', 'clutter')      # no job at all
BETWEEN_JOBS = GENERIC_JOBS + ('cover',)     # a between-height piece that is only "cover" is neither high nor low cover: it needs a stated job


def check_cover(R):
    free = []
    for b in R.boxes:
        if b.perim or b.min[1] > R.floor_y + 0.5:      # on the wall, or overhead (a slab on posts): not floor cover
            continue
        if b.kind == 'wall':
            continue
        free.append(b)
    cls = {'none': [], 'low': [], 'high': [], 'between': []}
    for b in free:
        c = cover_class(b)
        if b.kind in ('deck', 'slab') and c == 'between':
            c = 'between' if b.area < T['tier_min_area'] else 'none'      # a tier is a tier, not cover
        cls[c].append(b)
    jobless_between = [b for b in cls['between'] if b.job.lower() in BETWEEN_JOBS]
    jobless_high = [b for b in cls['high'] if b.job.lower() in GENERIC_JOBS]
    pieces = len(cls['high']) + len(cls['low'])
    ok = pieces >= 1 and not jobless_between and not jobless_high
    msg = 'cover: free-standing high %d, low %d, between %d (%d without a job), none %d%s' % (
        len(cls['high']), len(cls['low']), len(cls['between']), len(jobless_between), len(cls['none']),
        '' if pieces else ' [no free-standing cover]')
    return ok, msg, {'high': len(cls['high']), 'low': len(cls['low']), 'between': len(cls['between']), 'none': len(cls['none']),
                     'free': len(free), 'between_boxes': cls['between'], 'jobless_between': jobless_between, 'jobless_high': jobless_high,
                     'high_boxes': cls['high'], 'low_boxes': cls['low']}


def floor_holes(R):
    """holes of the floor-plane region (the base floor and hoppable tops): E - V + C - F"""
    cells = {}
    for (i, j), s in R.nodes.items():
        for k, (h, src) in enumerate(s):
            if src in ('floor', 'step') and abs(h - R.floor_y) <= T['hop_m'] + 1e-6:
                cells[(i, j)] = h
                break
    V = len(cells)
    E = sum(1 for (i, j) in cells if (i + 1, j) in cells) + sum(1 for (i, j) in cells if (i, j + 1) in cells)
    F = sum(1 for (i, j) in cells if (i + 1, j) in cells and (i, j + 1) in cells and (i + 1, j + 1) in cells)
    seen = set()
    C = 0
    for c in cells:
        if c in seen:
            continue
        C += 1
        stack = [c]
        seen.add(c)
        while stack:
            i, j = stack.pop()
            for d in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                n = (i + d[0], j + d[1])
                if n in cells and n not in seen:
                    seen.add(n)
                    stack.append(n)
    return E - V + C - F, V


def check_loops(R):
    holes, v = floor_holes(R)
    closed = 0
    notes = []
    for kind, a, b in R.connectors:
        back = R.reach(b, ('W', 'D', 'U', 'P', 'R'))
        if a in back:
            closed += 1
        else:
            notes.append('%s %s -> %s does not return' % (kind, a, b))
    total = holes + closed
    ok = total >= T['loops_min']
    return ok, 'loops %d (floor holes %d + closed pad / ride routes %d of %d)' % (total, holes, closed, len(R.connectors)), {
        'loops': total, 'holes': holes, 'closed': closed, 'connectors': len(R.connectors), 'notes': notes}


def bearing_off(R, p):
    """the angle (deg) between the entry facing and the bearing from the entry spawn to p"""
    d = np.array([p[0] - R.spawn[0], p[2] - R.spawn[2]])
    n = float(np.hypot(d[0], d[1]))
    if n < 1e-6:
        return 0.0
    f = R.facing()
    cosv = float(np.clip((d @ f) / n, -1, 1))
    return math.degrees(math.acos(cosv))


def usable_nodes(R, wave=0):
    nodes = R.wave_nodes[wave] if wave < len(R.wave_nodes) else []
    far = [p for p in nodes if float(np.hypot(p[0] - R.spawn[0], p[2] - R.spawn[2])) >= cfg()['min_spawn']]
    return far if far else list(nodes), bool(far)


def check_rear(R):
    nodes, far = usable_nodes(R, 0)
    rear = [p for p in nodes if bearing_off(R, p) > T['rear_deg']]
    allrear = 0
    for w in R.wave_nodes:
        allrear += sum(1 for p in w if bearing_off(R, p) > T['rear_deg'])
    ok = len(rear) == 0
    return ok, 'rear arc: %d of %d wave-1 usable nodes behind the entry facing (all waves: %d node uses behind)%s' % (
        len(rear), len(nodes), allrear, '' if far else ' [no node 12 m out: every node taken]'), {'rear': len(rear), 'n': len(nodes), 'allrear': allrear}


def seen_from(R, eye, look_deg_yaw, pitch, p):
    c = cfg()
    d = np.array([p[0] - eye[0], p[2] - eye[2]])
    n = float(np.hypot(d[0], d[1]))
    if n < 1e-6:
        return False
    a = math.radians(look_deg_yaw)
    f = np.array([-math.sin(a), -math.cos(a)])
    off = math.degrees(math.acos(float(np.clip((d @ f) / n, -1, 1))))
    if off > c['half_h']:
        return False
    for dy in (1.0, 1.6):
        tgt = np.array([p[0], p[1] + dy, p[2]])
        elev = math.degrees(math.atan2(tgt[1] - eye[1], n))
        if abs(elev - math.degrees(pitch)) > c['half_v']:
            continue
        if not R.blocked(eye, tgt):
            return True
    return False


def check_view(R):
    c = cfg()
    nodes, _ = usable_nodes(R, 0)
    f = R.facing()
    t = 0.0
    first = None
    seen0 = 0
    while t <= T['view_s'] + 1e-9:
        pos = np.array([R.spawn[0] + f[0] * c['walk'] * t, R.spawn[1], R.spawn[2] + f[1] * c['walk'] * t])
        # stop where a solid or the rect's edge stops the walk
        i, j = R._cell_of(pos[0], pos[2])
        if not (0 <= i < R.nx and 0 <= j < R.nz) or not any(abs(h - R.spawn[1]) < 0.7 for h, _ in R.nodes.get((i, j), [])):
            break
        eye = np.array([pos[0], pos[1] + c['eye'], pos[2]])
        n = sum(1 for p in nodes if seen_from(R, eye, R.yaw, R.look_pitch, p))
        if t == 0.0:
            seen0 = n
        if n and first is None:
            first = t
            break
        t += T['view_step_s']
    ok = first is not None
    return ok, 'view: %s (%d of %d wave-1 nodes seen at entry)' % ('enemy in view at %.2f s' % first if ok else 'no enemy in view within %.1f s' % T['view_s'], seen0, len(nodes)), {
        't': first, 'seen0': seen0, 'n': len(nodes)}


def check_cross(R):
    c = cfg()
    x0, z0, x1, z1 = R.rect
    chord = math.hypot(x1 - x0, z1 - z0)
    td, tw = chord / c['dash'], chord / c['walk']
    ok = td <= T['cross_s']
    note = ' [blink: a dash crosses it in under %.1f s]' % T['blink_s'] if td < T['blink_s'] else ''
    return ok, 'cross: chord %.1f m, dash %.2f s, walk %.1f s (bar %.1f s at dash)%s' % (chord, td, tw, T['cross_s'], note), {'chord': chord, 'dash_s': td, 'walk_s': tw}


def check_expose(R):
    c = cfg()
    g = max(1, int(round(T['expose_grid_m'] / T['cell_m'])))
    pts = []
    for (i, j), s in sorted(R.nodes.items()):
        if i % g == 0 and j % g == 0:
            for h, src in s:
                if src in ('floor', 'step') and abs(h - R.floor_y) <= T['hop_m'] + 1e-6:
                    pts.append(np.array([R.cx[i], h + c['eye'], R.cz[j]]))
                    break
    if len(pts) > T['expose_max_pts']:        # a big room: every k-th sample, so the pair count stays about 3000
        k = int(math.ceil(len(pts) / float(T['expose_max_pts'])))
        pts = pts[::k]
    vis = tot = 0
    longest = 0.0
    for a in range(len(pts)):
        for b in range(a + 1, len(pts)):
            d = float(np.linalg.norm(pts[a] - pts[b]))
            if d < T['expose_min_pair_m']:
                continue
            tot += 1
            if not R.blocked(pts[a], pts[b]):
                vis += 1
                longest = max(longest, d)
    frac = vis / tot if tot else 0.0
    ok = frac <= T['expose_max']
    return ok, 'exposure: %.0f %% of %d floor-sample pairs see each other (bar %.0f %%); longest clear line %.1f m (medium band %.1f m)' % (
        100 * frac, tot, 100 * T['expose_max'], longest, T['long_line_m']), {'frac': frac, 'pairs': tot, 'longest': longest}


def check_climb(R):
    start = R._node_near(R.spawn, 0.6)
    if start is None:
        return False, 'climb: the spawn stands on no cell', {'unreached': -1}
    got = R.reach(start, ('W', 'D'))
    tiers = {}
    for n in R.out:
        h, src = R.surf(n)
        if src == 'tier':
            tiers[n] = None
    # groups of tier cells joined by walker links
    groups = []
    seen = set()
    for n in tiers:
        if n in seen:
            continue
        grp = {n}
        stack = [n]
        seen.add(n)
        while stack:
            a = stack.pop()
            for b, ty in R.out[a].items():
                if b in tiers and ty == 'W' and b not in seen:
                    seen.add(b)
                    grp.add(b)
                    stack.append(b)
        groups.append(grp)
    groups = [g for g in groups if len(g) >= T['plane_min_cells']]
    miss = [g for g in groups if not (g & got)]
    ok = not miss
    msg = 'climb: %d tier group(s), %d not reached by walker links' % (len(groups), len(miss))
    if miss:
        msg += ' (at %s)' % '; '.join('%.1f m near (%.0f, %.0f)' % (R.surf(next(iter(g)))[0], R.cx[next(iter(g))[0]], R.cz[next(iter(g))[1]]) for g in miss)
    return ok, msg, {'groups': len(groups), 'unreached': len(miss)}


CORE = (('PLANES', check_planes), ('COVER', check_cover), ('LOOPS', check_loops), ('REAR', check_rear), ('VIEW', check_view))
EXTRA = (('CROSS', check_cross), ('EXPOSE', check_expose), ('CLIMB', check_climb))


def run(R):
    res = {}
    for name, fn in CORE + EXTRA:
        ok, msg, data = fn(R)
        res[name] = (ok, msg, data)
    return res


def tag(ok):
    return 'PASS' if ok else 'RED'


def summary(res):
    core = sum(1 for n, _ in CORE if res[n][0])
    extra = sum(1 for n, _ in EXTRA if res[n][0])
    return core, extra


# ---------------------------------------------------------------- loading
def git_json(rev, path):
    txt = subprocess.run(['git', 'show', rev + ':' + path.replace('\\', '/')], capture_output=True, text=True, encoding='utf-8', cwd=ROOT).stdout
    return json.loads(txt)


def read_json(path, rev=None):
    if rev:
        rel = os.path.relpath(path, ROOT) if os.path.isabs(path) else path
        return git_json(rev, rel)
    p = path if os.path.isabs(path) else os.path.join(ROOT, path)
    return json.load(open(p, encoding='utf-8'))


def room_from_plan(plan, rid, label):
    row = next(r for r in plan['rooms'] if r['id'] == rid)
    z = lambda key: [b for b in plan.get(key, []) if b.get('zone') == rid]
    return Room(label, row, z('boxes'), z('ramps'), z('pads'), z('pits'), z('props'))


def room_from_file(d, label):
    row = d['row']
    return Room(label, row, d.get('boxes', []), d.get('ramps', []), d.get('pads', []), d.get('pits', []), d.get('props', []))


def load_spec(spec, rev=None):
    path, rid = (spec.rsplit(':', 1) if ':' in spec and not (len(spec) > 1 and spec[1] == ':' and spec.count(':') == 1) else (spec, None))
    d = read_json(path, rev)
    if 'rooms' in d and isinstance(d['rooms'], list):
        if not rid:
            raise SystemExit('a plan needs :<room id>')
        return room_from_plan(d, rid, os.path.basename(path).replace('.plan.json', '') + '/' + rid)
    return room_from_file(d, os.path.basename(os.path.dirname(os.path.dirname(path))) + '/' + d['row']['id'] if d.get('row') else path)


def level_rooms():
    out = []
    plans = [('n52-' + lv, 'Design/levels/n52/%s.plan.json' % lv) for lv in ('foundry', 'iceline', 'kiln', 'span', 'spire')]
    plans += [('n57-kiln', 'Design/levels/n57/kiln.plan.json')]
    for p in sorted(glob.glob(os.path.join(ROOT, 'Design', 'levels', 'n58', '*.plan.json'))):
        plans.append(('n58-' + os.path.basename(p).split('.')[0], os.path.relpath(p, ROOT).replace('\\', '/')))
    plans += [('n65-foundry', 'Design/levels/n65/foundry.plan.json')]
    for lv in ('iceline', 'kiln', 'spire'):
        plans.append(('n67-' + lv, 'Design/levels/n67/%s.plan.json' % lv))
    for label, path in plans:
        full = os.path.join(ROOT, path)
        if not os.path.exists(full):
            continue
        plan = json.load(open(full, encoding='utf-8'))
        for r in plan['rooms']:
            if r.get('kind') == 'room':
                out.append((label + '/' + r['id'], room_from_plan(plan, r['id'], label + '/' + r['id'])))
    cands = []
    for d in ('Design/levels/n65/rooms', 'Design/levels/n65/rooms-n52', 'Design/levels/n67/iceline/rooms', 'Design/levels/n67/kiln/rooms',
              'Design/levels/n67/spire/rooms', 'Design/levels/n67/iceline/rooms-base', 'Design/levels/n67/kiln/rooms-base', 'Design/levels/n67/spire/rooms-base'):
        for f in sorted(glob.glob(os.path.join(ROOT, d, '*.json'))):
            j = json.load(open(f, encoding='utf-8'))
            if 'row' in j and j['row'].get('kind') == 'room':
                cands.append(('file:' + d.replace('Design/levels/', '') + '/' + j['row']['id'], room_from_file(j, 'file')))
    return out, cands


# ---------------------------------------------------------------- reporting
def line(R, res):
    core, extra = summary(res)
    x0, z0, x1, z1 = R.rect
    pl = res['PLANES'][2]
    cv = res['COVER'][2]
    lp = res['LOOPS'][2]
    rr = res['REAR'][2]
    vw = res['VIEW'][2]
    ex = res['EXPOSE'][2]
    cr = res['CROSS'][2]
    cl = res['CLIMB'][2]
    fails = ','.join(n for n, _ in CORE + EXTRA if not res[n][0])
    return '%-34s %4.0fx%-4.0f %4d | %d %s | %d/%d/%d %s | %d %s | %d %s | %s %s | %.2f %s | %3.0f%% %4.1fm %s | %s | core %d/5 extra %d/3 RED: %s' % (
        R.label[:34], x1 - x0, z1 - z0, (x1 - x0) * (z1 - z0), pl['n'], tag(res['PLANES'][0]),
        cv['high'], cv['low'], cv['between'], tag(res['COVER'][0]), lp['loops'], tag(res['LOOPS'][0]),
        rr['rear'], tag(res['REAR'][0]), ('%.2fs' % vw['t']) if vw['t'] is not None else 'none', tag(res['VIEW'][0]),
        cr['dash_s'], tag(res['CROSS'][0]), 100 * ex['frac'], ex['longest'], tag(res['EXPOSE'][0]), tag(res['CLIMB'][0]),
        core, extra, fails or '-')


HEAD = ('%-34s %9s %4s | planes | cover hi/lo/btw | loops | rear | view | cross | exposure | climb | verdicts' % ('room', 'WxD m', 'm2'))


def header(argv):
    sha = subprocess.run(['git', 'rev-parse', '--short', 'HEAD'], capture_output=True, text=True, cwd=ROOT).stdout.strip()
    return ['# command: python -I Tools/night/n74-room-rules.py ' + ' '.join(argv), '# sha: %s; time: %s' % (sha, time.strftime('%Y-%m-%d %H:%M:%S'))]


def detail(R, res):
    out = ['%s  rect %s  spawn %s yaw %.0f  floor %.1f' % (R.label, R.rect, [float(v) for v in R.spawn], R.yaw, R.floor_y)]
    for name, _ in CORE + EXTRA:
        ok, msg, data = res[name]
        out.append('  %-7s %s  %s' % (name, tag(ok), msg))
    cv = res['COVER'][2]
    for b in cv['between_boxes']:
        out.append('    between-height piece: %s %s-%s h %.2f job "%s"%s' % (b.kind, [float(v) for v in b.min], [float(v) for v in b.max], b.h, b.job[:60],
                                                                          '  <- no job' if b in cv['jobless_between'] else ''))
    for b in cv['jobless_high']:
        out.append('    high piece with no job: %s %s-%s h %.2f job "%s"' % (b.kind, [float(v) for v in b.min], [float(v) for v in b.max], b.h, b.job[:60]))
    for tag_, key in (('high', 'high_boxes'), ('low', 'low_boxes')):
        for b in cv[key]:
            out.append('    free-standing %s piece: %s %s-%s h %.2f job "%s"' % (tag_, b.kind, [float(v) for v in b.min], [float(v) for v in b.max], b.h, b.job[:50]))
    pl = res['PLANES'][2]
    out.append('    walkers reach every tier: %s' % res['CLIMB'][1])
    for n in res['LOOPS'][2]['notes']:
        out.append('    ' + n)
    core, extra = summary(res)
    out.append('  core %d/5 PASS, extra %d/3 PASS' % (core, extra))
    return out


def emit(lines, out_path, argv):
    txt = '\n'.join(header(argv) + lines) + '\n'
    if out_path:
        p = out_path if os.path.isabs(out_path) else os.path.join(ROOT, out_path)
        os.makedirs(os.path.dirname(p), exist_ok=True)
        with open(p, 'w', encoding='utf-8', newline='\n') as f:
            f.write(txt)
    sys.stdout.write(txt)


# ---------------------------------------------------------------- the reference room and the planted cases
def reference_room():
    """a small vertical room that should PASS every check (in memory); x -10..10, z -26..0 (north is -z), the door on the south edge"""
    z = 'ref'
    boxes = [
        {'zone': z, 'kind': 'hull', 'min': [-2, 0, -14], 'max': [1, 2.5, -11], 'job': 'cover: the centre pillar (a circuit)'},
        {'zone': z, 'kind': 'hull', 'min': [3, 0, -9], 'max': [5, 2.0, -7], 'job': 'cover: the east post (a second circuit)'},
        {'zone': z, 'kind': 'hull', 'min': [-7, 0, -7], 'max': [-5, 1.1, -5], 'job': 'cover: the low wall'},
        {'zone': z, 'kind': 'hull', 'min': [-4, 0, -21], 'max': [0.5, 2.0, -19.5], 'job': 'cover: the back wall piece (breaks the long line)'},
        {'zone': z, 'kind': 'hull', 'min': [1.5, 0, -5], 'max': [4, 1.8, -3.5], 'job': 'cover: the door-side piece (breaks the entry line)'},
        {'zone': z, 'kind': 'deck', 'min': [-10, 0, -22], 'max': [-6, 2.5, -13], 'job': 'tier: the catwalk'},
        {'zone': z, 'kind': 'deck', 'min': [5, 0, -26], 'max': [10, 4, -19], 'job': 'tier: the hearth'},
    ]
    ramps = [
        {'zone': z, 'from': [-8, -8], 'to': [-8, -13], 'width': 3, 'rise': 2.5, 'job': 'climb: the catwalk stair'},
        {'zone': z, 'from': [8.5, -10], 'to': [8.5, -19], 'width': 3, 'rise': 4, 'job': 'climb: the hearth stair'},
    ]
    pads = [{'zone': z, 'at': [-8, 2.5, -18], 'target': [7.5, 4, -22], 'apexAbove': 2, 'job': 'line: the hoist'}]
    row = {'id': z, 'name': 'reference', 'kind': 'room', 'rect': [-10, -26, 10, 0], 'height': 9, 'spawn': {'at': [0, 0, -2], 'yawDeg': 0},
           'waves': [['husk', 'husk', 'swarmer']],
           'waveNodes43': [[[3, 0, -16], [-1, 0, -19], [0, 0, -24]]],
           'nodes': [[3, 0, -16], [-1, 0, -19], [0, 0, -24]], 'highNodes': [[7.5, 4, -22]]}
    return row, boxes, ramps, pads


def build_ref(mod=None):
    row, boxes, ramps, pads = reference_room()
    row, boxes, ramps, pads = copy.deepcopy(row), copy.deepcopy(boxes), copy.deepcopy(ramps), copy.deepcopy(pads)
    if mod:
        row, boxes, ramps, pads = mod(row, boxes, ramps, pads)
    return Room('ref', row, boxes, ramps, pads, [])


def planted_cases():
    def flat(row, boxes, ramps, pads):
        row['rect'] = [-30, -62, 30, 20]
        row['spawn'] = {'at': [0, 0, -20], 'yawDeg': 0}
        row['waveNodes43'] = [[[-10, 0, 8], [10, 0, 8], [0, 0, -55]]]
        return row, [], [], []

    def no_tiers(row, boxes, ramps, pads):
        return row, [b for b in boxes if b['kind'] != 'deck'], [], []

    def no_cover(row, boxes, ramps, pads):
        return row, [b for b in boxes if b['kind'] != 'hull'], ramps, pads

    def between(row, boxes, ramps, pads):
        for b in boxes:
            if b['kind'] == 'hull':
                b['max'][1] = 1.5
                b['job'] = 'deco'
        return row, boxes, ramps, pads

    def behind(row, boxes, ramps, pads):
        row['waveNodes43'] = [[[3, 0, -1], [-1, 0, -0.5], [-8, 0, -0.5]]]
        row['spawn'] = {'at': [0, 0, -12], 'yawDeg': 0}
        row['waveNodes43'] = [[[-8, 0, -0.6], [8, 0, -0.6], [0, 0, -0.4]]]
        return row, boxes, ramps, pads

    def blind(row, boxes, ramps, pads):
        # the spawn faces a wall of cover at 3 m, every wave-1 node behind it: nothing in view within 2 s
        boxes.append({'zone': 'ref', 'kind': 'hull', 'min': [-10, 0, -6], 'max': [10, 6, -5], 'job': 'cover: planted blind wall'})
        return row, boxes, ramps, pads

    def big(row, boxes, ramps, pads):
        row['rect'] = [-40, -70, 40, 0]
        return row, boxes, ramps, pads

    def unreachable(row, boxes, ramps, pads):
        return row, boxes, [r for r in ramps if r['rise'] < 3], pads

    def one_loop(row, boxes, ramps, pads):
        # only the centre pillar stands free: one ring, no pad, so one circuit
        return row, [b for b in boxes if b['kind'] != 'hull' or 'centre' in b['job']], ramps, []
    return [
        ('PLANES: no tier, no ramp (one floor plane)', no_tiers, 'PLANES'),
        ('COVER: no hull on the floor', no_cover, 'COVER'),
        ('COVER: every hull 1.5 m with a deco job', between, 'COVER'),
        ('LOOPS: one free pillar left, the pad taken out', one_loop, 'LOOPS'),
        ('REAR: wave 1 behind the entry facing', behind, 'REAR'),
        ('VIEW: a 6 m wall of cover 3 m ahead', blind, 'VIEW'),
        ('CROSS: the rect 80 x 70 m', big, 'CROSS'),
        ('CLIMB: the hearth stair taken out (a pad and no walkers\' route)', unreachable, 'CLIMB'),
        ('EXPOSE: the flat box 60 x 82 m (no solid)', flat, 'EXPOSE'),
    ]


def cmd_planted(out_path, argv):
    lines = []
    ref = build_ref()
    rres = run(ref)
    lines += detail(ref, rres)
    refok = all(rres[n][0] for n, _ in CORE + EXTRA)
    lines.append('reference room: %s' % ('ALL PASS (as it must)' if refok else 'NOT ALL PASS (the reference is broken)'))
    bad = 0
    for title, mod, check in planted_cases():
        R = build_ref(mod)
        res = run(R)
        red = [n for n, _ in CORE + EXTRA if not res[n][0]]
        hit = check in red
        bad += 0 if hit else 1
        lines.append('planted %-62s -> RED on %s %s' % (title, ','.join(red) or '-', '(ok: %s is RED)' % check if hit else '(MISSED: %s stayed PASS)' % check))
    flat_room = build_ref(planted_cases()[-1][1])
    fres = run(flat_room)
    nred = sum(1 for n, _ in CORE + EXTRA if not fres[n][0])
    ncore = sum(1 for n, _ in CORE if not fres[n][0])
    lines.append('planted flat box (a 60 x 82 m rect, nothing in it, the wave behind): RED on %d checks (%d of the five core): %s' % (
        nred, ncore, ','.join(n for n, _ in CORE + EXTRA if not fres[n][0])))
    flat_ok = ncore >= 3
    lines.append('the flat box reads RED on at least 3 core checks: %s' % ('YES' if flat_ok else 'NO'))
    for n in fres:
        pass
    lines += detail(flat_room, fres)
    good = refok and bad == 0 and flat_ok
    lines.append('PLANTED: %s' % ('every planted case flips its check RED and the reference PASSes' if good else 'FAIL (a case missed)'))
    emit(lines, out_path, argv)
    return 0 if good else 1


def cmd_room(spec, rev, argv):
    R = load_spec(spec, rev)
    res = run(R)
    emit(detail(R, res), None, argv)
    return 0


def cmd_table(out_path, argv):
    rooms, cands = level_rooms()
    lines = ['# thresholds: planes %d-%d, loops >= %d, rear 0, view <= %.1f s, cross <= %.1f s at dash, exposure <= %.0f %%, cover: >= 1 free high or low piece, no between-height piece without a job'
             % (T['planes_min'], T['planes_max'], T['loops_min'], T['view_s'], T['cross_s'], 100 * T['expose_max']),
             '# cover hi/lo/btw = free-standing pieces; loops = floor holes + closed pad / ride routes; view = the first second an enemy is seen; cross = dash seconds across the diagonal',
             '# the table is a base, never a verdict (NIGHT74 D-3, D-8)', '',
             '## every fight room of the level plans (n52 / n57 / n58 / n65 / n67), ' + str(len(rooms)) + ' rooms', HEAD]
    tot = {n: 0 for n, _ in CORE + EXTRA}
    for label, R in rooms:
        res = run(R)
        lines.append(line(R, res))
        for n in tot:
            tot[n] += 1 if res[n][0] else 0
    lines.append('')
    lines.append('PASS counts over %d plan rooms: %s' % (len(rooms), ', '.join('%s %d' % (n, tot[n]) for n in tot)))
    lines.append('')
    lines.append('## the room files (rebuilds and bases under Design/levels/n65/rooms*, n67/<level>/rooms*), ' + str(len(cands)) + ' files')
    lines.append(HEAD)
    for label, R in cands:
        R.label = label
        res = run(R)
        lines.append(line(R, res))
    emit(lines, out_path, argv)
    return 0


def main():
    argv = sys.argv[1:]
    out_path = None
    rev = None
    if '--out' in argv:
        k = argv.index('--out')
        out_path = argv[k + 1]
        del argv[k:k + 2]
    if '--rev' in argv:
        k = argv.index('--rev')
        rev = argv[k + 1]
        del argv[k:k + 2]
    if not argv:
        print(__doc__)
        return 2
    cmd = argv[0]
    if cmd == 'planted':
        return cmd_planted(out_path, argv)
    if cmd == 'table':
        return cmd_table(out_path, argv)
    if cmd == 'room' and len(argv) >= 2:
        return cmd_room(argv[1], rev, argv)
    print('usage: room <spec> | table | planted')
    return 2


if __name__ == '__main__':
    sys.exit(main())
