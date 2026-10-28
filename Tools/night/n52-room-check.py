# NIGHT52 (the lead, P0; from Research/2026-10-19-wave52/density-baseline.py): THE ROOM CONTRACT's computed items (NIGHT52.md section 2)
# for one room of a level plan. Prints one line per item: PASS / FAIL for the (C) items it can compute, J for the lead's judgement items
# (listed so the lead logs them with the SVG path), and a last line "ROOM CONTRACT <room>: C <n>/<m> PASS" (exit 0 only when every C item
# passes). Teach and boss rooms are exempt (the contract says so): pass --exempt to print the numbers without the verdict.
#   python -I Tools/night/n52-room-check.py <plan.json> <room> [--report Reports/LEVEL-<id>-validate.md] [--grapple]
#       [--hold <hold-probe output>] [--flow-before <n46flow output> --flow <n46flow output>] [--dwell <before> <after>]
#       [--campaign <campaign --seeds output>]
# The bot is n46flow's walk pilot (--flow: its per-room row: clears k/n, deaths, dwell); `campaign --seeds` is read only when no --flow.
# What counts as a PIECE (DECIDE52 D-2): every box, ramp, pad and grapple row with zone == room, the room row's grappleNodes and pads, and
# every prop with "collider": true in the room (a pillar prop is geometry; a pickup without a collider is not).
# A TIER is a standing plane: the floor (0) plus each cluster of walkable tops (deck / slab boxes, ramp tops) >= 1.0 m high, a cluster being
# tops within 1.5 m of its lowest (a 0.5 m step is not a plane; RESEARCH A4: at most 3). Pit floors are hazards, not tiers.
import json, math, os, re, sys

args = sys.argv[1:]
if len(args) < 2:
    print(__doc__ if __doc__ else "usage: n52-room-check.py <plan> <room> [...]")
    sys.exit(2)
plan_path, room_id = args[0], args[1]
def opt(k, n=1):
    if k not in args:
        return None
    i = args.index(k)
    return args[i + 1:i + 1 + n] if n > 1 else args[i + 1]
exempt = '--exempt' in args
grapple_owned = '--grapple' in args

p = json.load(open(plan_path, encoding='utf-8'))
room = next((r for r in p['rooms'] if r['id'] == room_id), None)
if room is None:
    print('n52-room-check: no room ' + room_id + ' in ' + plan_path)
    sys.exit(2)
x0, z0, x1, z1 = room['rect']
w, d = x1 - x0, z1 - z0
area = w * d
height = room.get('height')
zone = lambda b: b.get('zone') == room_id

boxes = [b for b in p.get('boxes', []) if zone(b)]
ramps = [b for b in p.get('ramps', []) if zone(b)]
pads = [b for b in p.get('pads', []) if zone(b)] + list(room.get('pads', []))
anchors = [b for b in p.get('grapple', []) if zone(b)] + list(room.get('grappleNodes', []))
props = [q for q in p.get('props', []) if q.get('room') == room_id]
solid_props = [q for q in props if q.get('collider')]
pieces = [('box', b) for b in boxes] + [('ramp', b) for b in ramps] + [('pad', b) for b in pads] + [('anchor', b) for b in anchors] + [('prop', q) for q in solid_props]

results = []  # (kind 'C' | 'J', name, ok, text)
def C(name, ok, text):
    results.append(('C', name, bool(ok), text))
def J(name, text):
    results.append(('J', name, None, text))

# 1. size
long_side = max(w, d)
C('1 size', 20 <= long_side <= 32 and height is not None and 10 <= height <= 16,
  'rect %g x %g m (long side %g, band 20-32), height %s (band 10-16), area %g m2' % (w, d, long_side, height, area))

# 2. density + every piece's job and why
dens = len(pieces) * 100.0 / area
C('2 density', dens >= 2.5, '%d pieces (boxes %d, ramps %d, pads %d, anchors %d, solid props %d) / %g m2 = %.2f per 100 m2 (>= 2.5)' % (
    len(pieces), len(boxes), len(ramps), len(pads), len(anchors), len(solid_props), area, dens))
def has_reason(b):
    if not isinstance(b, dict):
        # a bare [x, y, z] grapple node in the room row: its reason is the room's `grapple` sentence (the plan format's place for it)
        return bool(str(room.get('grapple', '')).strip())
    return bool(str(b.get('job', '')).strip()) and bool(str(b.get('why', '')).strip())
missing = [k + ':' + str(b.get('id', b.get('min', b.get('from', b.get('at', '?')))) if isinstance(b, dict) else b) for k, b in pieces if not has_reason(b)]
C('2 job+why', not missing, 'every piece has a job and a why' if not missing else 'missing job/why: ' + '; '.join(missing[:8]))
VERBS = ['ride', 'wall jump', 'walljump', 'boost', 'slide', 'dash', 'double', 'grapple', 'climb', 'parry', 'punch', 'slam', 'pad', 'drop',
         'sight', 'line of', 'cover', 'circle', 'loop', 'perch', 'vault', 'launch', 'jump', 'gauntlet', 'shotgun', 'hop', 'lane', 'circuit']
noverb = [k + ':' + str(b.get('id', b.get('min', b.get('from', b.get('at', '?'))))) for k, b in pieces if isinstance(b, dict) and has_reason(b)
          and not any(v in (str(b.get('why', '')) + ' ' + str(b.get('job', ''))).lower() for v in VERBS)]
J('2 verb named', 'pieces whose why names no verb word (cut or reword): ' + ('; '.join(noverb[:10]) if noverb else 'none'))

# 3. tiers
tops = []
for b in boxes:
    if b.get('kind') in ('deck', 'slab') and b['max'][1] >= 1.0:
        tops.append((b['max'][1], 'box ' + str(b.get('id', '[%g,%g]' % (b['min'][0], b['min'][2])))))
for r in ramps:
    if r.get('rise', 0) >= 1.0:
        tops.append((float(r.get('base', 0)) + r['rise'], 'ramp top'))
tops.sort()
clusters = []
for h, label in tops:
    if clusters and h - clusters[-1][0] <= 1.5:
        clusters[-1][2].append(label)
        clusters[-1][1] = max(clusters[-1][1], h)
    else:
        clusters.append([h, h, [label]])
planes = 1 + len(clusters)
C('3 tiers', planes == 3, '%d standing planes: floor 0 + %s (exactly 3)' % (planes, ', '.join('%.1f-%.1f m (%d tops)' % (c[0], c[1], len(c[2])) for c in clusters) or 'none'))
rep = opt('--report')
if rep and os.path.exists(rep):
    row = next((l for l in open(rep, encoding='utf-8') if l.startswith('| enemies reach every tier: ' + room_id + ' |')), None)
    C('3 enemies reach', row is not None and '| PASS |' in row, (row.strip() if row else 'no tier row for ' + room_id + ' in ' + rep))
else:
    J('3 enemies reach', 'no --report given (levelgen report row "enemies reach every tier: ' + room_id + '")')
J('3 two verbs per tier', 'each tier reached by >= 2 different player verbs (judge on the SVG)')

# 4. circuit
J('4 circuit + lanes', 'a loop round a central mass, no dead end > 6 m, 1-3 lanes; the entrance gated (door locks or one-way drop)')

# 5. verbs asked
rides = []
for b in boxes:
    if b.get('ride'):
        ln = max(b['max'][0] - b['min'][0], b['max'][2] - b['min'][2])
        tall = b['max'][1] - max(b['min'][1], 0.0)
        rides.append((ln, tall))
best = max(rides) if rides else (0, 0)
C('5 ride wall', any(ln >= 17 and tall >= 3 for ln, tall in rides),
  '%d ride walls; longest %.1f m (>= 17 m at >= 3 m tall)' % (len(rides), best[0]) if rides else 'no ride:true wall in the room')
up_pieces = [b for k, b in pieces if isinstance(b, dict) and ('boost' in str(b.get('why', '')).lower() or 'boost' in str(b.get('job', '')).lower())]
C('5 upward line', len(pads) >= 1 or len(up_pieces) >= 1, '%d pad(s), %d piece(s) naming a boost line' % (len(pads), len(up_pieces)))
if grapple_owned:
    C('5 anchors', len(anchors) >= 2, '%d grapple anchors (>= 2 where the grapple is owned)' % len(anchors))
J('5 the one verb', 'ROOMS.md row: the ONE verb and the 30 s problem (no other room shares it)')

# 6. pickups + spawns
health = [q for q in props if q.get('kind') == 'healthPickup']
C('6 health pickup', len(health) >= 1, '%d healthPickup prop(s)%s' % (len(health), (' at ' + ', '.join(str(q['at']) for q in health)) if health else ''))
nodes = [('ground', n) for n in room.get('nodes', [])] + [('high', n) for n in room.get('highNodes', [])]
ys = sorted(set(round(n[1]) for _, n in nodes))
C('6 spawn tiers', len(ys) >= 2, 'spawn nodes on %d height(s): %s' % (len(ys), ys))
order = [r['id'] for r in p['rooms']]
idx = order.index(room_id)
entry = None
for dr in p.get('doors', []):
    bt = dr.get('between', [])
    if room_id in bt and idx > 0 and order[idx - 1] in bt:
        entry = dr
if entry is None:
    J('6 spawn vs door', 'no entry door found (first room?)')
else:
    dx0, dz0, dx1, dz1 = entry['rect']
    cx, cz = (dx0 + dx1) / 2.0, (dz0 + dz1) / 2.0
    if cz >= z1: nin = (0.0, -1.0)
    elif cz <= z0: nin = (0.0, 1.0)
    elif cx <= x0: nin = (1.0, 0.0)
    else: nin = (-1.0, 0.0)
    near, behind = [], []
    for kind, n in nodes:
        vx, vz = n[0] - cx, n[2] - cz
        dist = math.hypot(vx, vz)
        if dist < 6: near.append('%s%s %.1f m' % (kind, n, dist))
        ang = math.degrees(math.acos(max(-1.0, min(1.0, (vx * nin[0] + vz * nin[1]) / dist)))) if dist > 0 else 0.0
        if ang > 100: behind.append('%s%s %.0f deg' % (kind, n, ang))
    C('6 spawn vs door', not near and not behind, 'entry door %s at (%g, %g), inward (%g, %g): %d nodes; within 6 m: %s; beyond 100 deg: %s' % (
        entry['id'], cx, cz, nin[0], nin[1], len(nodes), '; '.join(near) or 'none', '; '.join(behind) or 'none'))
J('6 pickup placement', 'the health on the exposed part of the circuit (the best at the riskiest spot)')

# 7. no camping spot
kinds = sorted(set(k for wv in room.get('waves', []) for k in wv))
per_wave = [len(set(wv)) for wv in room.get('waves', [])]
C('7 enemy types', 2 <= len(kinds) <= 4, 'types in the room: %s (2-3 a fight, A15); per wave %s' % (', '.join(kinds) or 'none', per_wave))
def flow_row(path):
    # n46flow's per-room row: "  kb2   room | stall% maxStall air% | kgap | near | dwell dead | deaths falls | clear (k/n) | geometry"
    if not path or not os.path.exists(path):
        return None
    for l in open(path, encoding='utf-8'):
        if re.match(r'^\s+' + re.escape(room_id) + r'\s+(room|boss)\s+\|', l):
            f = [x.strip() for x in l.split('|')]
            dwell, dead = (float(x) for x in f[4].split()[:2])
            deaths, falls = (int(x) for x in f[5].split()[:2])
            m = re.match(r'([\d.]+|-)\s+\((\d+)/(\d+)\)', f[6])
            return {'dwell': dwell, 'dead': dead, 'deaths': deaths, 'falls': falls, 'clear': m.group(1) if m else '?',
                    'k': int(m.group(2)) if m else 0, 'n': int(m.group(3)) if m else 0, 'stall': float(f[1].split()[0]), 'geo': f[7] if len(f) > 7 else ''}
    return None
def flow_rows(arg):
    # one file, or two comma-separated files (seeds 1-10 and 11-20): DECIDE52 D-13
    return [r for r in (flow_row(p) for p in (arg or '').split(',') if p) if r]
fbs, fas = flow_rows(opt('--flow-before')), flow_rows(opt('--flow'))
fb, fa = (fbs[0] if fbs else None), (fas[0] if fas else None)
dw = opt('--dwell', 2)
if fas and fbs:
    mb, ma = sum(r['dwell'] for r in fbs) / len(fbs), sum(r['dwell'] for r in fas) / len(fas)
    band = abs(fbs[0]['dwell'] - fbs[1]['dwell']) if len(fbs) > 1 else 0.0
    grace = max(0.5, band)
    C('7 dwell', ma <= mb + grace, 'flow dwell %.2f -> %.2f s (mean of %d / %d seed sets; no worse past max(0.5 s, the room\'s own P0 seed-set band %.1f s) = %.1f s: D-10, D-13)' % (
        mb, ma, len(fbs), len(fas), band, grace))
elif dw:
    b, a = float(dw[0]), float(dw[1])
    C('7 dwell', a <= b, 'flow dwell %.1f -> %.1f s (no worse)' % (b, a))
else:
    J('7 dwell', 'no --flow-before / --flow n46flow outputs (Tools/night/n52-flow-table.sh on the level)')
hold = opt('--hold')
if hold and os.path.exists(hold):
    rows = [l.strip() for l in open(hold, encoding='utf-8') if l.startswith('hold52 ' + room_id + ' ')]
    zero = [l for l in rows if re.search(r' hits 0\b', l)]
    C('7 hold probe', rows and not zero, '%d spots probed; 0-hit spots: %s' % (len(rows), '; '.join(zero) or 'none') if rows else 'no "hold52 ' + room_id + '" lines in ' + hold)
else:
    J('7 hold probe', 'no --hold file (builder K\'s hold probe: lines "hold52 <room> tier <t> spot [x,y,z] hits <n>")')

# 8. proof
camp = opt('--campaign')
if fa:
    # the bot = n46flow's walk pilot (DECIDE52 D-3: NIGHT24's campaign bot reaches no room of the n47 / n48 levels: 0/10, NaN at P0)
    C('8 bot clears', fa['n'] > 0 and fa['k'] == fa['n'], 'walk pilot clears %d/%d seeds (all), clear median %s s, deaths %d over %d seeds (first-try hypothesis 50-95 %%, logged), stall %.1f %%, %s' % (
        fa['k'], fa['n'], fa['clear'], fa['deaths'], fa['n'], fa['stall'], fa['geo']))
elif camp and os.path.exists(camp):
    t = open(camp, encoding='utf-8').read()
    m = re.search(r'clear rate room \d+ ' + re.escape(room.get('name', '')) + r' \([^)]*\): first-try (\d+)/(\d+) = (\d+) %, cleared eventually (\d+)/(\d+)', t)
    if m:
        ft, n, rate, ev, n2 = int(m.group(1)), int(m.group(2)), int(m.group(3)), int(m.group(4)), int(m.group(5))
        C('8 bot clears', ev == n2, 'cleared eventually %d/%d (must be all); first-try %d %% (hypothesis 50-95, logged)' % (ev, n2, rate))
    else:
        C('8 bot clears', False, 'no clear-rate line for "' + room.get('name', '') + '" in ' + camp)
else:
    J('8 bot clears', 'no --campaign file (campaign <plan> --seeds 10)')
J('8 frames + svg', 'SVG (node Tools/night/level-svg.cjs <plan> <svg> --rooms ' + room_id + '); 3 Player frames (entry, mid-fight, highest tier); ROOMS.md row')

print('== n52-room-check %s %s "%s" (kind %s)%s' % (os.path.basename(plan_path), room_id, room.get('name', ''), room.get('kind'), ' EXEMPT' if exempt else ''))
for kind, name, ok, text in results:
    tag = 'J   ' if kind == 'J' else ('PASS' if ok else 'FAIL')
    print('  %s %-22s %s' % (tag, name, text))
cs = [r for r in results if r[0] == 'C']
npass = sum(1 for r in cs if r[2])
verdict = 'EXEMPT (numbers only)' if exempt else ('PASS' if npass == len(cs) else 'FAIL')
print('ROOM CONTRACT %s: C %d/%d PASS -> %s; J items: %d (the lead logs each)' % (room_id, npass, len(cs), verdict, sum(1 for r in results if r[0] == 'J')))
sys.exit(0 if exempt or npass == len(cs) else 1)
