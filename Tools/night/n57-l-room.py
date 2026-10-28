# NIGHT57 builder L (L0, NIGHT57.md section 5 L: "the checkers read n57"): THE ROOM CHECKER (Tools/night/n56-room.py) and THE IDENTITY CHECKS
# (Tools/night/n53-identity.py) on an n57 level, without editing either tool: both are imported and their three hard-coded n52 paths are
# pointed at the prefix's files (the plan Design/levels/<px>/<level>.plan.json, the level file Design/levels/<px>-<level>.json, the openings
# Design/levels/n57/openings57.json for n57, Design/levels/n52/openings53.json for n52). Every check, number and threshold is the old tools'.
#   python -I Tools/night/n57-l-room.py table [level:room,room ...]      n56-room's table (R2 / R5 / R8 / IN VIEW + I1-I4, I5 / I7 per level);
#                                                                         default: every kind-room row of Design/levels/n57/kiln.plan.json
#   python -I Tools/night/n57-l-room.py room <level> <room>              every number for one room (n56-room's `room` mode)
#   python -I Tools/night/n57-l-room.py identity [level:room,room ...]   n53-identity's table (I1-I4 per room, I5 / I7 per level)
#   python -I Tools/night/n57-l-room.py planted                          (1) the old tools' own planted cases (n52, unchanged: they must still
#                                                                         FAIL) and (2) one planted case per check on the n57 plan (each must FAIL)
#   --prefix n52   read the n52 files instead (the same command on NIGHT56's level: the before side)
import copy, importlib.util, json, math, os, sys
sys.dont_write_bytecode = True

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('n56room', os.path.join(HERE, 'n56-room.py'))
rm = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(rm)
idn = rm.idn
ORIG = {'plan_path': idn.plan_path, 'nav_of': idn.nav_of, 'OPENINGS': idn.OPENINGS, 'Level': rm.Level}
OPEN57 = os.path.join('Design', 'levels', 'n57', 'openings57.json')


def use_prefix(px):
    idn._NAV.clear()
    if px == 'n52':
        idn.plan_path, idn.nav_of, idn.OPENINGS, rm.Level = ORIG['plan_path'], ORIG['nav_of'], ORIG['OPENINGS'], ORIG['Level']
        return
    idn.OPENINGS = OPEN57
    idn.plan_path = lambda level: os.path.join('Design', 'levels', px, level + '.plan.json')

    def nav_of(level):
        if level not in idn._NAV:
            g = json.load(open(os.path.join('Design', 'levels', px + '-' + level + '.json'), encoding='utf-8'))['navGrid']
            n, l = g['nodes'], g['links']
            idn._NAV[level] = (n['x'], n['y'], n['z'], l['linkStart'], l['linkTo'])
        return idn._NAV[level]
    idn.nav_of = nav_of

    class Level(ORIG['Level']):
        def __init__(self, lv, rev=None):
            self.lv = lv
            pp = idn.plan_path(lv)
            lp = os.path.join('Design', 'levels', px + '-' + lv + '.json')
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


def planted57():
    # one planted case per check on the n57 plan (in memory; the files are never written): each must FAIL (L-091)
    res = []
    L = rm.Level('kiln')
    rid = fights('kiln')[0]
    # R2: a 3-cell spur off one floor cell (as n56-room's own g2 plant)
    room = idn.room_of(L.plan, rid)
    adj = rm.walk_cells(L, L.plan, room)
    c0 = next(c for c, n in adj.items() if c[2] == 0 and len(n) >= 3)
    spur = [(c0[0], c0[1], 40 + k) for k in range(3)]
    ok, t = rm.r2(L, L.plan, room, [(c0, spur[0]), (spur[0], spur[1]), (spur[1], spur[2])])
    res.append(('R2 n57 %s + a 3-cell spur' % rid, ok, t, False))
    # R5: every foothold of the room taken out
    p = copy.deepcopy(L.plan)
    room = idn.room_of(p, rid)
    fh, ft = rm.foothold(p, room)
    taken = 0
    while fh is not None and taken < 12:
        p['boxes'] = [b for b in p['boxes'] if b is not fh]
        taken += 1
        fh, ft = rm.foothold(p, room)
    res.append(('R5 n57 %s, %d footholds out' % (rid, taken), fh is not None, ft, False))
    # R8: one solid's job taken out
    p = copy.deepcopy(L.plan)
    room = idn.room_of(p, rid)
    it = idn.solids(p, rid)[0][2]
    it.pop('job', None); it.pop('job36', None)
    ok, t = rm.r8(p, room)
    res.append(('R8 n57 %s, one job out' % rid, ok, t, False))
    # IN VIEW: wave 1's nodes moved behind the entry (the door's centre - 2 m outward, the room's floor): none can be seen looking in
    p = copy.deepcopy(L.plan)
    room = idn.room_of(p, rid)
    cx, cz, nin, _ = rm.entry_frame(p, room)
    fy = rm.floor_y(room)
    behind = [[cx + nin[0] * 1.0 - nin[1] * k, fy, cz + nin[1] * 1.0 + nin[0] * k] for k in (-1.5, 1.5)]
    room['waveNodes43'] = [behind] + list(room.get('waveNodes43', [])[1:])
    op = rm.openings().get('kiln', {}).get(rid, 'held')
    ok, t, et, _, _ = rm.in_view(L, p, room, op)
    res.append(('IN VIEW n57 %s (%s), wave 1 at the door' % (rid, op), ok, t, False))
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
    res.append(('I1 n57 %s + its mirror' % rid, ok, t, False))
    # I4: a pad planted on the room's first walk proof's first leg
    p = copy.deepcopy(L.plan)
    room = idn.room_of(p, rid)
    walk = next(pr['points'] for pr in room.get('proofs', []) if pr.get('kind') == 'walk')
    a, b = walk[0], walk[1]
    p['pads'] = p.get('pads', []) + [{'zone': rid, 'at': [(a[0] + b[0]) / 2.0, a[1], (a[2] + b[2]) / 2.0], 'target': [0, 4, 0]}]
    ok, t = idn.i4(p, rid)
    res.append(('I4 n57 %s, a pad on walk line 0' % rid, ok, t, False))
    # I7: two consecutive fight rooms given one opening
    p = json.load(open(idn.plan_path('kiln'), encoding='utf-8'))
    fl = fights('kiln')
    ops = dict(idn.openings('kiln'))
    ops[fl[1]] = ops.get(fl[0], 'held')
    ok, t = idn.i7(p, 'kiln', ops)
    res.append(('I7 n57 planted (%s = %s)' % (fl[0], fl[1]), ok, t, False))
    good = True
    for name, ok, t, _ in res:
        want = ok is False or (name.startswith('R5') and ok is False)
        print('  %s %-44s %s%s' % (idn.tag(ok), name, t[:300], '' if want else '   <- SHOULD FAIL'))
        good = good and want
    print('PLANTED n57: %s' % ('every check FAILs its planted case (trusted)' if good else 'a planted case did not FAIL (that check is not trusted)'))
    return good


def main():
    a = sys.argv[1:]
    px = 'n57'
    if '--prefix' in a:
        i = a.index('--prefix'); px = a[i + 1]; a = a[:i] + a[i + 2:]
    if not a:
        print(open(__file__, encoding='utf-8').read().split('import copy')[0]); sys.exit(2)
    if a[0] == 'planted':
        print('== the old tools\' own planted cases (n52 paths, unchanged)')
        use_prefix('n52')
        g1 = rm.planted()
        g2 = True
        if px != 'n52':
            print('== the n57 planted cases (Design/levels/n57/kiln.plan.json, in memory)')
            use_prefix(px)
            g2 = planted57()
        sys.exit(0 if g1 and g2 else 1)
    use_prefix(px)
    if a[0] in ('table', 'identity') and len(a) == 1:
        a = a + ['kiln:' + ','.join(fights('kiln'))]
    if a[0] == 'identity':
        sys.argv = [sys.argv[0], 'table'] + a[1:]
        idn.main()
    sys.argv = [sys.argv[0]] + a
    rm.main()


if __name__ == '__main__':
    main()
