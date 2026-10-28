# NIGHT56 builder L, L3 (NIGHT56.md section 5 L3): the rooms that fail n53-identity.py, fixed by ADDING pieces with jobs (T-131: cuts alone break
# mirrors and give nothing). One function per room; each asserts the room is as L2 left it (its pieces, its pads, its proofs) before it edits,
# and touches only the rows it names. Then levelgen (bash Tools/night/n56-l-regen.sh <level>), n53-identity.py, n56-room.py and the contract.
#   python -I Tools/night/n56-l-l3.py <room> [--dry]
#
# kb2 (I4: THE SPOUT pad [5.5, 0, -89.5] 1.5 m from the circuit's east leg at x 7; R's NIGHT55 note: no pad spot is both ~1.1 m off the
#   crucible's east face, for the launch, and 2 m off the line; the lane is 4.5 m between the crucible (x 4) and THE RUNNER's lava (x 8.5)):
#   ADD THE SPOUT LIP, a 1.2 m kerb (x 6.55..6.85, z -90.6..-88.4; first cut 4 m long: the contract's dwell 1.95 -> 2.35 s, FAIL) between the pad's recess and the east lane: the pad stays where its launch
#   lands (NIGHT53's probe spot), the walk past it goes outside the lip (x 7.75, 1.6 m of floor to the lava's edge), and stepping on the spout
#   is a choice (round the lip's ends), not an accident of the circuit. Job: cover (vault cover, 1.2 m < jump 1.5 m, from the north-east lip's
#   spitter and the runner wall's landing). The circuit's proof (proofs[0]) is re-walked outside the lip: x 7 -> 7.75 on its east leg.
# p5 (I4: THE CAPSTAN SPRING [-5.5, 0, -131] 1.5 m from the north walkway's walk line at z -132.5; the walkway is 5.5 m between the north wall
#   and the winding pit's lip at z -129): the pad moves 1 m toward the pit, to [-5.5, 0, -130] (its disc's edge at the lip; 2.5 m off the line),
#   1 m nearer its gallery target: no piece is added (a pad's move, not a cut); the launch is re-probed by the contract's hold52 --pads.
import json, os, sys

sys.dont_write_bytecode = True
TAG = 'NIGHT56 L (n56-l-l3.py, L3)'


def load(lv):
    path = os.path.join('Design', 'levels', 'n52', lv + '.plan.json')
    text = open(path, encoding='utf-8', newline='').read()
    return path, text, json.loads(text)


def save(path, text, plan):
    eol = '\r\n' if '\r\n' in text else '\n'
    open(path, 'w', encoding='utf-8', newline='').write(json.dumps(plan, indent=1, ensure_ascii=False).replace('\n', eol) + (eol if text.endswith('\n') else ''))


def room(plan, rid):
    return next(r for r in plan['rooms'] if r['id'] == rid)


def zone(plan, key, rid):
    return [q for q in plan.get(key, []) if q.get('zone') == rid]


def insert_box(plan, rid, box):
    idx = max(i for i, b in enumerate(plan['boxes']) if b.get('zone') == rid)
    plan['boxes'].insert(idx + 1, box)


def kb2(plan):
    r = room(plan, 'kb2')
    pads = zone(plan, 'pads', 'kb2')
    spout = next(q for q in pads if q['at'] == [5.5, 0, -89.5])
    assert spout['target'] == [2.5, 4.6, -89.5], 'THE SPOUT moved'
    assert not any('SPOUT LIP' in (b.get('why') or '') for b in zone(plan, 'boxes', 'kb2')), 'the lip is already there'
    p0 = r['proofs'][0]['points']
    assert p0 == [[0, 0, -78], [-6, 0, -84], [-6, 0, -97], [-5, 0, -102], [0, 0, -102], [4.5, 0, -99], [7, 0, -93], [7, 0, -86], [0, 0, -78]], 'the circuit moved'
    why = (TAG + ": THE SPOUT LIP (1.2 m kerb, 0.3 x 2.2 m, the length of the pad's disc, between THE SPOUT's recess against the crucible and the east lane): the circuit walks "
           "past it outside (x 7.75, 1.6 m of floor to the runner's lava), and the spout is stepped on round the lip's ends by choice, not by the "
           "circuit (n53-identity I4: the pad was 1.5 m from the walk line); vault cover (under jump.height 1.5 m) from the north-east lip and "
           "the runner wall's landing")
    insert_box(plan, 'kb2', {'zone': 'kb2', 'kind': 'hull', 'min': [6.55, 0, -90.6], 'max': [6.85, 1.2, -88.4], 'job': 'cover', 'why': why, 'note': why})
    r['proofs'][0]['points'] = [[0, 0, -78], [-6, 0, -84], [-6, 0, -97], [-5, 0, -102], [0, 0, -102], [4.5, 0, -99], [7.75, 0, -93], [7.75, 0, -86], [0, 0, -78]]
    r['proofs56_why'] = TAG + ': proofs[0] (the circuit) re-walked outside THE SPOUT LIP on its east leg (x 7 -> 7.75)'
    return ['kb2: + THE SPOUT LIP hull [6.55, 0, -90.6]..[6.85, 1.2, -88.4] (cover); proofs[0] east leg x 7 -> 7.75']


def p5(plan):
    pads = zone(plan, 'pads', 'p5')
    cap = next(q for q in pads if q['at'] == [-5.5, 0, -131])
    assert cap['target'] == [1.5, 4.2, -121], 'THE CAPSTAN SPRING moved'
    cap['at'] = [-5.5, 0, -130]
    add = TAG + ': moved 1 m south, [-5.5, 0, -131] -> [-5.5, 0, -130] (its disc to the pit\'s lip, 2.5 m off the north walkway\'s walk line at z -132.5; n53-identity I4: it was 1.5 m)'
    cap['why'] = cap['why'] + ' ' + add
    if 'note' in cap:
        cap['note'] = cap['note'] + ' ' + add
    return ['p5: THE CAPSTAN SPRING [-5.5, 0, -131] -> [-5.5, 0, -130]']


def g8(plan):
    # g8 (I2 1.08 / 1.10: two 23 m lane walls beside open lanes, walked flat). ADD THE ASH PIT, the roundhouse's firebox dump (hot ash: lava
    # rules, 0.8 m down, as the iceline boiler room g7's GRATE), across the west lane from the west lane wall's face (x -9.4) to THE PIT's west
    # edge (x -1.5), 4 m long (z -230.5 .. -226.5: past the 3 m gap jump), between the door floor and the west sand tower: the west lane wall
    # rides over it (the shortcut); on foot the west lane is reached through the inspection pit's corner or round its east side. Job: hazard
    # (both ways: it burns you and knocks you up; a charger that misses you at its rim goes in).
    # First cut (reverted): z -237.5 .. -233.5, north of the sand tower (wave 2's [-8.3, 0, -236] moved out of it): I2 1.56, the level report
    # clean once the proof stopped cutting THE PIT's north corner (STOPPED at (-1.15, -1.5, -239.65)), but the contract's hold probe found a
    # camping spot: the west gallery, tier 1 [-5.5, 3, -248], 0 hits in one seed of 3 (the gallery ramp's foot shut off from the door side).
    # South of the tower the ramp stays open to the floor's north half, where the waves stand; wave 2's spots are untouched.
    # The west walk proof (proofs[0]) crossed it: it is re-walked round THE PIT's south end, up its east side and back west 1.3 m past its north end.
    r = room(plan, 'g8')
    assert len(zone(plan, 'pits', 'g8')) == 1, 'g8 already has its ash pit'
    p0 = r['proofs'][0]['points']
    assert p0 == [[0, 0, -222], [-7, 0, -226], [-7, 0, -237.5], [-2.3, 0, -238.3], [-2.3, 0, -244.5], [0, 0, -248]], 'proofs[0] moved'
    why = (TAG + ": THE ASH PIT (the roundhouse's firebox dump: hot ash, lava rules 0.8 m down as g7's GRATE; 7.9 x 4 m across the west lane "
           "from the west lane wall's face to THE PIT, between the door floor and the west sand tower): the west lane wall is the way over it "
           "(n53-identity I2 was 1.08 / 1.10: both lanes walked flat beside their walls); on foot the lane is through the inspection pit's corner "
           "or round its east side; a charger that misses you at its rim goes in")
    idx = max(i for i, q in enumerate(plan['pits']) if q.get('zone') == 'g8')
    plan['pits'].insert(idx + 1, {'zone': 'g8', 'rect': [-9.4, -230.5, -1.5, -226.5], 'floorY': -0.8, 'lava': True, 'job': 'hazard', 'why': why, 'note': why})
    r['proofs'][0]['points'] = [[0, 0, -222], [2.8, 0, -228.3], [3, 0, -241.3], [-2.3, 0, -241.3], [-2.3, 0, -244.5], [0, 0, -248]]
    r['proofs56_why'] = TAG + ": proofs[0] re-walked round THE PIT's south end, up its east side and west past its north end (THE ASH PIT closes the west lane at z -230.5 .. -226.5)"
    return ['g8: + THE ASH PIT [-9.4, -230.5, -1.5, -226.5] lava (hazard); proofs[0] re-walked']


def kb2off(plan):
    # kb2's lip OUT again (d619427d undone in the data): its contract passed (14/14, dwell 2.20 s) but the whole-level walk bot (n46flow n52-kiln,
    # 10 seeds, 02:21) paid for it: kb2's clear 31.0 -> 46.2 s, deaths 4 -> 6, the level's clear 92.2 -> 110.1 s (+19 % over the baseline 92.6:
    # L2's gate is <= +10 %) and stall 25.5 -> 32.5 %: the 1.6 m lane between the lip and THE RUNNER's lava holds the pilot at the lava's edge.
    # kb2 goes back to its I4 FAIL (the pad 1.5 m off the circuit): a lane re-cut, as R said on NIGHT55.
    r = room(plan, 'kb2')
    lips = [b for b in zone(plan, 'boxes', 'kb2') if 'THE SPOUT LIP' in (b.get('why') or '')]
    assert len(lips) == 1, 'no lip to take out'
    plan['boxes'].remove(lips[0])
    assert r['proofs'][0]['points'][6] == [7.75, 0, -93]
    r['proofs'][0]['points'] = [[0, 0, -78], [-6, 0, -84], [-6, 0, -97], [-5, 0, -102], [0, 0, -102], [4.5, 0, -99], [7, 0, -93], [7, 0, -86], [0, 0, -78]]
    del r['proofs56_why']
    return ['kb2: THE SPOUT LIP out, proofs[0] back to x 7 (the bot cost: kiln clear 92.2 -> 110.1 s)']


def g5off(plan):
    # L2's wave-1 spots in iceline g5 OUT (g5's room row back to L1's, 0e3e6024: it had no waveNodes43): the ambush rule put g5's first pack
    # 7-10 m from the entry, and the walk pilot fights it where it stands: n46flow g5 stall 37.1 -> 59.0 %, iceline 32.0 -> 37.3 % (the
    # proposed stall gate is the baseline + 3 points: 35.0). With g5 back, L2's IN VIEW is 15 / 18 (g4, g7, g5), still the doneWhen's >= 15;
    # g5's entry frame still shows its pack (3.3 of 5 seen, nearest 15.3 m: past the ambush rule's 15 m).
    import subprocess
    old = json.loads(subprocess.run(['git', 'show', '0e3e6024:Design/levels/n52/iceline.plan.json'], capture_output=True, text=True, encoding='utf-8').stdout)
    was = room(old, 'g5')
    assert 'waveNodes43' not in was
    r = room(plan, 'g5')
    assert 'NIGHT56 L (n56-l-wave1.py' in r.get('waveNodes43_why', ''), 'g5 is not as L2 left it'
    i = plan['rooms'].index(r)
    plan['rooms'][i] = was
    return ["g5: the room row back to 0e3e6024's (L2's wave-1 spots out)"]


def g3walk(plan):
    # L2 attempt 2 (gate-r2: "move g3's / g5's wave-1 nodes onto the walk line"): g3's three husks onto the circuit's west leg (proofs[0]:
    # (0, -64) -> (-8.5, -71)), each still passing the ambush rule (n56-l-spots.py: 8-15 m from the door, no line into the corridor).
    # g5 has no ambush-passing spot within 3 m of its walk lines (they run down the door's axis, which the corridor rule excludes): not moved.
    r = room(plan, 'g3')
    wn = r['waveNodes43']
    assert wn[0] == [[-7.5, 0, -72.5], [-4.5, 0, -67.5], [7.5, 0, -72.5]], 'g3 wave 1 is not L2s'
    wn[0] = [[-4.5, 0, -67.5], [-6.5, 0, -70.5], [-8.5, 0, -68.5]]
    r['waveNodes43_why'] = r['waveNodes43_why'] + ' ' + TAG + ' L2 attempt 2: wave 1 onto the circuit\'s west leg [-4.5, 0, -67.5], [-6.5, 0, -70.5], [-8.5, 0, -68.5]'
    return ['g3: wave 1 -> [-4.5, 0, -67.5], [-6.5, 0, -70.5], [-8.5, 0, -68.5]']


ROOMS = {'kb2': ('kiln', kb2), 'p5': ('span', p5), 'g8': ('iceline', g8), 'kb2off': ('kiln', kb2off), 'g5off': ('iceline', g5off), 'g3walk': ('iceline', g3walk)}


def main():
    a = sys.argv[1:]
    lv, fn = ROOMS[a[0]]
    rid = a[0][:-3] if a[0].endswith('off') else ('g3' if a[0] == 'g3walk' else a[0])  # kb2off edits kb2, g3walk g3
    path, text, plan = load(lv)
    before = json.loads(text)
    done = fn(plan)
    # nothing outside the room's own rows changed
    for k in before:
        if k in ('rooms', 'boxes', 'pads', 'ramps', 'pits'):
            continue
        assert before[k] == plan[k], k + ' changed'
    for rb, ra in zip(before['rooms'], plan['rooms']):
        assert rb['id'] == ra['id']
        if rb['id'] != rid:
            assert rb == ra, 'room ' + rb['id'] + ' changed'
    for key in ('boxes', 'pads', 'ramps', 'pits'):
        assert [q for q in before.get(key, []) if q.get('zone') != rid] == [q for q in plan.get(key, []) if q.get('zone') != rid], key + ' outside ' + rid + ' changed'
    print('\n'.join(done))
    if '--dry' in a:
        print('DRY RUN'); return
    save(path, text, plan)
    print('wrote ' + path)


if __name__ == '__main__':
    main()
