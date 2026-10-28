# NIGHT56 builder L, L2 (spawns in view; NIGHT56.md section 5 L2): each combat room's FIRST wave gets its own spawn spots (waveNodes43[0];
# `r.nodes` is not moved: highNodes merge into it, Campaign24.cs:323-324; Campaign24.cs:913 reads waveNodes43 first), picked so the room
# passes n56-room.py's IN VIEW rule for its opening, and seen in the a452 entry frame (the a156 meter's camera):
#   held / vista       spots seen from the foothold's flanks (the entry eye when the room has no foothold) and in the entry frame, >= 12 m from
#                      the commit feet and from the entry frame's feet (SpawnWave's minSpawnDist acts on all of them alike: no pile-up);
#   ambushed           spots 8-15 m from the door, <= 15 m from midfield, not behind the foothold, no line into the entry corridor, seen in the
#                      entry frame, < 12 m from both feet (the same band: SpawnWave's fallback spreads them evenly).
# Each original wave-1 spot that already passes (and sits in the band) is kept; each one that fails is replaced by the nearest passing spot
# (height kept where one exists: a perch's spitter stays on a perch), no two picks within 3 m. Floor spots only (n56-room.py's walk graph:
# standable, reach and return), flat (not a ramp), off pads (1.5 m) and out of pits, 0.9 m off the room's walls.
# Four rooms open on a wave too small to show 4 (held / vista: ">= 4 enemies visible"); their first two waves (or one body) are re-dealt, the
# same enemies in the same rooms, nothing added or removed (the beat's other waves keep their own spots):
#   foundry a5 [3, 2, 3] -> [5, 3]     waves 1 + 2 merged (the spitter's floor and its husks come at once)
#   iceline g2 [1, 3, 3] -> [4, 3]     waves 1 + 2 merged (both wardens' shields across the pillars at once)
#   span p1 [3, 6] -> [4, 5]           one swarmer of wave 2 comes with the husks
# iceline g4 (one drone, then two: three in all) cannot show 4 without a new enemy: left as it is (one of the <= 3 rooms the doneWhen allows).
# iceline g7 [3, 7, 6] keeps its waves: its wave 1 [tank, spitter, spitter] is at the budget (LevelValidate29: 10 of 10 points; a fourth body
# costs 10.5, ERROR), and a 4-body wave 1 would let wave 2's pack overlap it (overlap46's growthMax 2: 6 <= 8), the stack NIGHT46 measured as
# g7's deaths 0 -> 8: its three get spots in view (3 of 3 seen) and the room stays a FAIL of the >= 4 count (the second allowed room).
#   python -I Tools/night/n56-l-wave1.py [--dry] [level ...]    prints each room's picks; writes the plans unless --dry
import importlib.util, json, math, os, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_s = importlib.util.spec_from_file_location('n56room', os.path.join(HERE, 'n56-room.py'))
nr = importlib.util.module_from_spec(_s)
_s.loader.exec_module(nr)
idn = nr.idn

SKIP = {('iceline', 'g4'): 'one drone then two: no placement shows 4 (left as it is)'}
REDEAL = {
    ('foundry', 'a5'): ('merge', 'waves 1 + 2 merged [3, 2, 3] -> [5, 3]: the spitter, its four husks on the floor it holds, at once'),
    ('iceline', 'g2'): ('merge', 'waves 1 + 2 merged [1, 3, 3] -> [4, 3]: both wardens and their husks hold the pillars at once'),
    ('span', 'p1'): ('move', 'one swarmer of wave 2 comes with wave 1 [3, 6] -> [4, 5]'),
}
TAG = 'NIGHT56 L (n56-l-wave1.py, L2 spawns in view)'


def redeal(room, how):
    waves = [list(w) for w in room['waves']]
    wn = room.get('waveNodes43')
    wn = [None if w is None else [list(n) for n in w] for w in wn] if wn else None
    if how == 'merge':
        waves = [waves[0] + waves[1]] + waves[2:]
        if wn is not None:
            wn = [None] + (wn[2:] if len(wn) > 2 else [])
    else:
        k = next(i for i, x in enumerate(waves[1]) if x == 'swarmer')
        waves[0].append(waves[1].pop(k))
    room['waves'] = waves
    if wn is not None:
        room['waveNodes43'] = wn
    elif 'waveNodes43' in room:
        del room['waveNodes43']


def candidates(L, p, room, op):
    # held / vista: n56-room.py scores the better of the foothold's two flanks, so the picks come from ONE flank: the one that sees more spots
    fh, _ = nr.foothold(p, room)
    eyes = nr.flank_eyes(p, room, fh) if fh is not None else []
    if op in ('held', 'vista') and len(eyes) > 1:
        best = max(((candidates_from(L, p, room, op, [e]), e) for e in eyes), key=lambda t: len(t[0][0]))
        return best[0]
    return candidates_from(L, p, room, op, eyes or None)


def candidates_from(L, p, room, op, eyes):
    nr.walk_cells(L, p, room)
    core = nr.walk_cells.core
    X, Y, Z = L.nav[0], L.nav[1], L.nav[2]
    x0, z0, x1, z1 = room['rect']
    fy = nr.floor_y(room)
    pits = [q['rect'] for q in idn.zone_items(p, 'pits', room['id'])]
    pads = [q['at'] for q in idn.pads_of(p, room['id'])]
    near = nr.near_set(L, room)
    cx, cz, nin, did = nr.entry_frame(p, room)
    fh, _ = nr.foothold(p, room)
    feet = nr.commit_feet(p, room)
    ee, elook, epitch = nr.entry_eye(room)
    efeet = (ee[0], ee[1] - nr.EYE, ee[2])
    if not eyes:
        eyes = [ee]
    mfx, mfz = cx + nin[0] * nr.room_depth(room, nin) / 2.0, cz + nin[1] * nr.room_depth(room, nin) / 2.0
    corr = (cx - nin[0] * 3.0, fy + nr.EYE, cz - nin[1] * 3.0)
    def passes(nd):
        x, y, z = nd
        if not nr.seen_frustum(L, near, ee, elook, epitch, nd):
            return False, False
        df = math.hypot(x - feet[0], z - feet[2])
        de = math.hypot(x - efeet[0], z - efeet[2])
        if op in ('held', 'vista'):
            if not any(nr.seen(L, near, e, nin, nd) for e in eyes):
                return False, False
            return True, df >= nr.MIN_SPAWN and de >= nr.MIN_SPAWN
        if math.hypot(x - mfx, z - mfz) > 15.0 or not (8.0 <= math.hypot(x - cx, z - cz) <= 15.0):
            return False, False
        if fh is not None and nr.seg_box(ee, (x, y + 1.0, z), fh['min'], fh['max']):
            return False, False
        if any(nr.ray_clear(L, (x, y + h, z), corr, near) for h in (1.0, 1.6)):
            return False, False
        return True, df < nr.MIN_SPAWN and de < nr.MIN_SPAWN
    out = []
    for i in core:
        x, y, z = X[i], Y[i], Z[i]
        if not (x0 + 0.9 <= x <= x1 - 0.9 and z0 + 0.9 <= z <= z1 - 0.9):
            continue
        if abs(y * 10 - round(y * 10)) > 0.02 or y < fy - 0.1:
            continue
        if any(q[0] <= x <= q[2] and q[1] <= z <= q[3] for q in pits) or any(math.hypot(x - a[0], z - a[2]) < 1.5 for a in pads):
            continue
        nd = (round(x, 2), round(y, 2), round(z, 2))
        ok, band = passes(nd)
        if ok:
            out.append((nd, band))
    return out, passes


def pick(L, p, room, op):
    orig = nr.wave_nodes(room, 0)
    cands, passes = candidates(L, p, room, op)
    inband = [c for c, b in cands if b]
    pool = inband if len(inband) >= 3 else [c for c, _ in cands]
    want = max(3, min(6, len(orig)))
    picks = []
    def ok_gap(c):
        return all(math.hypot(c[0] - q[0], c[2] - q[2]) >= 3.0 for q in picks)
    pool_set = set(pool)
    for o in orig[:want]:
        # an original spot is kept when it passes itself (in the band the pool uses) and stands on the floor (a pool spot within 0.75 m)
        same = [c for c in pool if math.hypot(c[0] - o[0], c[2] - o[2]) <= 0.75 and abs(c[1] - o[1]) <= 0.3]
        po, pb = passes(tuple(o))
        if same and po and (pb or pool is not inband) and ok_gap(o):
            picks.append(tuple(o)); continue
        best = None
        for c in pool:
            if not ok_gap(c):
                continue
            score = math.hypot(c[0] - o[0], c[2] - o[2]) + 3.0 * abs(c[1] - o[1])
            if best is None or score < best[0]:
                best = (score, c)
        if best is not None:
            picks.append(best[1])
    while len(picks) < want:
        rest = [c for c in pool if ok_gap(c)]
        if not rest:
            break
        # the spot farthest from the picks so far (spread)
        picks.append(max(rest, key=lambda c: min(math.hypot(c[0] - q[0], c[2] - q[2]) for q in picks) if picks else 0))
    return orig, picks, len(cands), len(inband)


def fmt(n):
    return [float('%g' % v) if not float(v).is_integer() else int(v) for v in n]


def main():
    a = sys.argv[1:]
    dry = '--dry' in a
    levels = [x for x in a if not x.startswith('--')] or ['kiln', 'foundry', 'iceline', 'spire', 'span']
    ops = nr.openings()
    for lv in levels:
        path = idn.plan_path(lv)
        text = open(path, encoding='utf-8', newline='').read()
        plan = json.loads(text)
        before = json.loads(text)
        L = nr.Level(lv)
        L.plan = plan
        touched = []
        for rid, op in ops[lv].items():
            room = idn.room_of(plan, rid)
            if (lv, rid) in SKIP:
                print('%s %s (%s): SKIPPED: %s' % (lv, rid, op, SKIP[(lv, rid)]))
                continue
            ok0, t0, _, _, _ = nr.in_view(L, plan, room, op)
            note = ''
            if (lv, rid) in REDEAL:
                how, why = REDEAL[(lv, rid)]
                redeal(room, how)
                note = why
            elif ok0:
                print('%s %s (%s): already PASS (%s)' % (lv, rid, op, t0[:120]))
                continue
            orig, picks, nc, nb = pick(L, plan, room, op)
            wn = room.get('waveNodes43')
            wn = list(wn) if wn else [None] * 1
            wn[0] = [fmt(n) for n in picks]
            room['waveNodes43'] = wn
            ok1, t1, et1, _, _ = nr.in_view(L, plan, room, op)
            why = ('%s: wave 1 (%s) spawns where the entry sees it (%s, %d passing floor spots, %d in the minSpawnDist band): %s%s' % (
                TAG, ', '.join(room['waves'][0]), op, nc, nb, '; '.join('[%s]' % ', '.join('%g' % v for v in n) for n in picks),
                ('; ' + note) if note else ''))
            prev = room.get('waveNodes43_why')
            room['waveNodes43_why'] = (prev + ' ' + why) if prev else why
            if note:
                room['waves56_why'] = TAG + ': ' + note + ' (the same enemies, re-dealt: no wave-1 placement can show 4 of a smaller wave)'
            touched.append(rid)
            print('%s %s (%s): %s -> %s | %s | entry %s%s' % (lv, rid, op, 'PASS' if ok0 else 'FAIL', 'PASS' if ok1 else 'FAIL', t1[:200], et1, (' | ' + note) if note else ''))
            print('    was %s' % [fmt(n) for n in orig])
            print('    now %s' % wn[0])
        for k in before:
            if k == 'rooms':
                for rb, ra in zip(before['rooms'], plan['rooms']):
                    if rb['id'] not in touched and rb != ra:
                        raise SystemExit('room %s changed' % rb['id'])
                    if rb['id'] in touched:
                        for kk in set(rb) | set(ra):
                            if kk not in ('waveNodes43', 'waveNodes43_why', 'waves', 'waves56_why') and rb.get(kk) != ra.get(kk):
                                raise SystemExit('%s.%s changed' % (rb['id'], kk))
            elif before[k] != plan[k]:
                raise SystemExit(k + ' changed')
        if dry or not touched:
            continue
        eol = '\r\n' if '\r\n' in text else '\n'
        open(path, 'w', encoding='utf-8', newline='').write(json.dumps(plan, indent=1, ensure_ascii=False).replace('\n', eol) + (eol if text.endswith(('\n',)) else ''))
        print('wrote %s (%s)' % (path, ', '.join(touched)))


if __name__ == '__main__':
    main()
