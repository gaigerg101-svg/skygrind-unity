# NIGHT59 builder L (L1, NIGHT59.md section 3 L1: "n57's hard-code 'kiln'"): Tools/night/n57-l-room.py with the LEVEL as an argument.
# THE ROOM CHECKER (Tools/night/n56-room.py) and THE IDENTITY CHECKS (Tools/night/n53-identity.py) on any generated level <px>-<level>
# (n57-kiln, n58-span, ...), without editing either tool: both are imported and their three hard-coded n52 paths are pointed at the
# level's files: the plan Design/levels/<px>/<level>.plan.json, the level file Design/levels/<px>-<level>.json, and the openings: n52's
# Design/levels/n52/openings53.json for that level, overlaid by Design/levels/<px>/openings<NN>.json where it exists (<NN> = the prefix's
# digits: openings57.json for n57 holds every n57-kiln room, so n57-kiln reads exactly what n57-l-room.py reads; openings58.json for n58
# names the new rooms of n58-span; a slot that still holds n52's room keeps n52's opening). Every check, number and threshold is the old
# tools'. A level id n52-<level> reads the n52 files unchanged (the before side).
#   python -I Tools/night/n59-l-room.py <px>-<level> table [level:room,room ...]     n56-room's table (R2 / R5 / R8 / IN VIEW + I1-I4,
#                                                                                       I5 / I7 per level); default: every kind-room row of
#                                                                                       the level's plan
#   python -I Tools/night/n59-l-room.py <px>-<level> room <room>                       every number for one room (n56-room's `room` mode)
#   ... [--dir <dir>]   NIGHT66 T1c: the level's plan (<level>.plan.json), level file (<px>-<level>.json) and openings (openings<NN>.json)
#                       read from <dir> (a round's Logs/n66/<tag>/, written by gen-n65-foundry.cjs --out), not from Design/levels
#   python -I Tools/night/n59-l-room.py <px>-<level> identity [level:room,room ...]    n53-identity's table (I1-I4 per room, I5 / I7)
#   python -I Tools/night/n59-l-room.py <px>-<level> planted                           (1) the old tools' own planted cases (n52,
#                                                                                       unchanged: they must still FAIL) and (2) one planted
#                                                                                       case per check on the level's plan (each must FAIL)
import copy, importlib.util, json, os, re, sys
sys.dont_write_bytecode = True

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('n56room', os.path.join(HERE, 'n56-room.py'))
rm = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(rm)
idn = rm.idn
ORIG = {'plan_path': idn.plan_path, 'nav_of': idn.nav_of, 'OPENINGS': idn.OPENINGS, 'Level': rm.Level,
        'rm_openings': rm.openings, 'idn_openings': idn.openings}


DIR = None  # NIGHT66 T1c: --dir <dir>, a round's folder (Logs/n66/<tag>/, gen-n65-foundry.cjs --out) holding <level>.plan.json,
             # <px>-<level>.json and openings<NN>.json: the level's three files read from there instead of Design/levels


def openings_file(px):
    if DIR:
        return os.path.join(DIR, 'openings' + re.sub(r'\D', '', px) + '.json')
    return os.path.join('Design', 'levels', px, 'openings' + re.sub(r'\D', '', px) + '.json')


def level_file(px, level):
    return os.path.join(DIR, px + '-' + level + '.json') if DIR else os.path.join('Design', 'levels', px + '-' + level + '.json')


def use_prefix(px):
    idn._NAV.clear()
    idn.plan_path, idn.nav_of, idn.OPENINGS, rm.Level = ORIG['plan_path'], ORIG['nav_of'], ORIG['OPENINGS'], ORIG['Level']
    rm.openings, idn.openings = ORIG['rm_openings'], ORIG['idn_openings']
    if px == 'n52':
        return
    # the openings: n52's per level, overlaid by the prefix's own file (its rooms win)
    merged = json.load(open(ORIG['OPENINGS'], encoding='utf-8'))
    of = openings_file(px)
    if os.path.exists(of):
        mine = json.load(open(of, encoding='utf-8'))
        for lv, rooms in mine.items():
            if lv.startswith('_') or not isinstance(rooms, dict):
                continue
            merged[lv] = dict(merged.get(lv, {}), **rooms)
    rm.openings = lambda: merged
    idn.openings = lambda level: merged.get(level, {})
    idn.plan_path = lambda level: os.path.join(DIR, level + '.plan.json') if DIR else os.path.join('Design', 'levels', px, level + '.plan.json')

    def nav_of(level):
        if level not in idn._NAV:
            g = json.load(open(level_file(px, level), encoding='utf-8'))['navGrid']
            n, l = g['nodes'], g['links']
            idn._NAV[level] = (n['x'], n['y'], n['z'], l['linkStart'], l['linkTo'])
        return idn._NAV[level]
    idn.nav_of = nav_of

    class Level(ORIG['Level']):
        def __init__(self, lv, rev=None):
            self.lv = lv
            pp = idn.plan_path(lv)
            lp = level_file(px, lv)
            self.plan = rm.git_json(rev, pp) if rev else json.load(open(pp, encoding='utf-8'))
            lf = rm.git_json(rev, lp) if rev else json.load(open(lp, encoding='utf-8'))
            col = lf['collision']
            self.boxes = [(b['min'], b['max']) for b in col['boxes']]
            self.wedges = col.get('wedges', [])
            g = lf['navGrid']
            self.nav = (g['nodes']['x'], g['nodes']['y'], g['nodes']['z'], g['links']['linkStart'], g['links']['linkTo'])
    rm.Level = Level


def fights(lv):
    p = json.load(open(idn.plan_path(lv), encoding='utf-8'))
    return [r['id'] for r in p['rooms'] if r.get('kind') == 'room']


def planted_level(px, lv):
    # one planted case per check on the level's plan (in memory; the files are never written): each must FAIL (L-091)
    res = []
    L = rm.Level(lv)
    # the first fight room with an entry door (n57-kiln: kb2, as n57-l-room.py; n58-span: p1 is entered from the start, no door: p2)
    rid = next(r for r in fights(lv) if idn.entry_door(L.plan, r) is not None)
    # R2: a 3-cell spur off one floor cell (as n56-room's own g2 plant)
    room = idn.room_of(L.plan, rid)
    adj = rm.walk_cells(L, L.plan, room)
    c0 = next(c for c, n in adj.items() if c[2] == 0 and len(n) >= 3)
    spur = [(c0[0], c0[1], 40 + k) for k in range(3)]
    ok, t = rm.r2(L, L.plan, room, [(c0, spur[0]), (spur[0], spur[1]), (spur[1], spur[2])])
    res.append(('R2 %s %s + a 3-cell spur' % (px, rid), ok, t, False))
    # R5: every foothold of the room taken out
    p = copy.deepcopy(L.plan)
    room = idn.room_of(p, rid)
    fh, ft = rm.foothold(p, room)
    taken = 0
    while fh is not None and taken < 12:
        p['boxes'] = [b for b in p['boxes'] if b is not fh]
        taken += 1
        fh, ft = rm.foothold(p, room)
    res.append(('R5 %s %s, %d footholds out' % (px, rid, taken), fh is not None, ft, False))
    # R8: one solid's job taken out
    p = copy.deepcopy(L.plan)
    room = idn.room_of(p, rid)
    it = idn.solids(p, rid)[0][2]
    it.pop('job', None); it.pop('job36', None)
    ok, t = rm.r8(p, room)
    res.append(('R8 %s %s, one job out' % (px, rid), ok, t, False))
    # IN VIEW: wave 1's nodes moved behind the entry (the door's centre - 2 m outward, the room's floor): none can be seen looking in
    p = copy.deepcopy(L.plan)
    room = idn.room_of(p, rid)
    cx, cz, nin, _ = rm.entry_frame(p, room)
    fy = rm.floor_y(room)
    behind = [[cx + nin[0] * 1.0 - nin[1] * k, fy, cz + nin[1] * 1.0 + nin[0] * k] for k in (-1.5, 1.5)]
    room['waveNodes43'] = [behind] + list(room.get('waveNodes43', [])[1:])
    op = rm.openings().get(lv, {}).get(rid, 'held')
    ok, t, et, _, _ = rm.in_view(L, p, room, op)
    res.append(('IN VIEW %s %s (%s), wave 1 at the door' % (px, rid, op), ok, t, False))
    # I1: the room's solids plus their mirror across the door axis (a mirrored room reads ~100 %)
    p = copy.deepcopy(L.plan)
    room = idn.room_of(p, rid)
    dr = idn.entry_door(p, rid)
    cx, cz, nin = idn.door_frame(room, dr)
    add = []
    for b in [b for b in p['boxes'] if b.get('zone') == rid]:
        m = copy.deepcopy(b)
        if nin[0] == 0.0:
            m['min'][0], m['max'][0] = 2 * cx - b['max'][0], 2 * cx - b['min'][0]
        else:
            m['min'][2], m['max'][2] = 2 * cz - b['max'][2], 2 * cz - b['min'][2]
        add.append(m)
    for r in [r for r in p.get('ramps', []) if r.get('zone') == rid]:
        m = copy.deepcopy(r)
        if nin[0] == 0.0:
            m['from'][0], m['to'][0] = 2 * cx - r['from'][0], 2 * cx - r['to'][0]
        else:
            m['from'][1], m['to'][1] = 2 * cz - r['from'][1], 2 * cz - r['to'][1]
        add.append(('ramp', m))
    p['boxes'] += [a for a in add if not isinstance(a, tuple)]
    p['ramps'] = p.get('ramps', []) + [a[1] for a in add if isinstance(a, tuple)]
    ok, t = idn.i1(p, rid)
    res.append(('I1 %s %s + its mirror' % (px, rid), ok, t, False))
    # I4: a pad planted on the room's first walk proof's first leg
    p = copy.deepcopy(L.plan)
    room = idn.room_of(p, rid)
    walk = next(pr['points'] for pr in room.get('proofs', []) if pr.get('kind') == 'walk')
    a, b = walk[0], walk[1]
    p['pads'] = p.get('pads', []) + [{'zone': rid, 'at': [(a[0] + b[0]) / 2.0, a[1], (a[2] + b[2]) / 2.0], 'target': [0, 4, 0]}]
    ok, t = idn.i4(p, rid)
    res.append(('I4 %s %s, a pad on walk line 0' % (px, rid), ok, t, False))
    # I7: two consecutive fight rooms given one opening
    p = json.load(open(idn.plan_path(lv), encoding='utf-8'))
    fl = fights(lv)
    ops = dict(idn.openings(lv))
    ops[fl[1]] = ops.get(fl[0], 'held')
    ok, t = idn.i7(p, lv, ops)
    res.append(('I7 %s planted (%s = %s)' % (px, fl[0], fl[1]), ok, t, False))
    good = True
    for name, ok, t, _ in res:
        want = ok is False
        print('  %s %-44s %s%s' % (idn.tag(ok), name, t[:300], '' if want else '   <- SHOULD FAIL'))
        good = good and want
    print('PLANTED %s-%s: %s' % (px, lv, 'every check FAILs its planted case (trusted)' if good else 'a planted case did not FAIL (that check is not trusted)'))
    return good


def main():
    global DIR
    a = sys.argv[1:]
    if '--dir' in a:  # NIGHT66 T1c
        i = a.index('--dir')
        DIR = a[i + 1]
        del a[i:i + 2]
        if not os.path.isdir(DIR):
            print('n59-l-room: --dir %s: no such folder' % DIR); sys.exit(2)
    lid = next((x for x in a if re.match(r'^n\d+-[a-z]+$', x)), None)
    if lid is None or len(a) < 2:
        print(open(__file__, encoding='utf-8').read().split('import copy')[0]); sys.exit(2)
    a.remove(lid)
    px, lv = lid.split('-', 1)
    if a[0] == 'planted':
        print('== the old tools\' own planted cases (n52 paths, unchanged)')
        use_prefix('n52')
        g1 = rm.planted()
        g2 = True
        if px != 'n52':
            print('== the %s planted cases (Design/levels/%s/%s.plan.json, in memory)' % (lid, px, lv))
            use_prefix(px)
            g2 = planted_level(px, lv)
        sys.exit(0 if g1 and g2 else 1)
    use_prefix(px)
    if a[0] in ('table', 'identity') and len(a) == 1:
        a = a + [lv + ':' + ','.join(fights(lv))]
    if a[0] == 'room' and len(a) == 2:
        a = ['room', lv, a[1]]
    if a[0] == 'identity':
        sys.argv = [sys.argv[0], 'table'] + a[1:]
        idn.main()
    sys.argv = [sys.argv[0]] + a
    rm.main()


if __name__ == '__main__':
    main()
