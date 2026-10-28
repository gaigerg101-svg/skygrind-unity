# NIGHT56 builder L (L3): where a room can GAIN solid footprint without a mirror (n53-identity I1). Prints the room's map with each free 1 m
# cell marked: '*' an empty floor cell whose reflection across the entry door's axis is empty too (a solid there adds footprint and no
# overlap), '-' an empty cell whose reflection is solid (a solid there raises the overlap), kept off walk proofs (0.9 m), pads (1.6 m),
# wave-1 spots (1.2 m), ramps and pits; and the I1 number a piece of A m2 on '*' cells would give.   python -I Tools/night/n56-l-i1free.py <level> <room>
import importlib.util, math, os, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
_s = importlib.util.spec_from_file_location('n53id', os.path.join(HERE, 'n53-identity.py'))
idn = importlib.util.module_from_spec(_s)
_s.loader.exec_module(idn)

lv, rid = sys.argv[1], sys.argv[2]
p = idn.load_plan(idn.plan_path(lv))
room = idn.room_of(p, rid)
dr = idn.entry_door(p, rid)
cx, cz, nin = idn.door_frame(room, dr)
along_z = nin[0] == 0.0
refl = (lambda x, z: (2 * cx - x, z)) if along_z else (lambda x, z: (x, 2 * cz - z))
S = idn.solids(p, rid)
solid = lambda x, z: any(t(x, z) for k, t, _ in S if k == 'box')
ramp = lambda x, z: any(t(x, z) for k, t, _ in S if k == 'ramp')
pits = [q['rect'] for q in idn.zone_items(p, 'pits', rid)]
pads = [q['at'] for q in idn.pads_of(p, rid)]
lines = [pr['points'] for pr in room.get('proofs', []) if pr.get('kind') == 'walk']
wn = room.get('waveNodes43') or []
spots = (wn[0] if wn and wn[0] else [])


def near_line(x, z, d):
    for pts in lines:
        for a, b in zip(pts, pts[1:]):
            if idn.seg_dist(x, z, a[0], a[2], b[0], b[2])[0] < d:
                return True
    return False


x0, z0, x1, z1 = room['rect']
anys = lambda x, z: solid(x, z) or ramp(x, z)  # n53-identity's footprint: boxes and ramps
cells = [c for c in idn.grid(room) if anys(*c)]
hit = sum(1 for (x, z) in cells if anys(*refl(x, z)))
area = len(cells) * idn.G * idn.G
print('%s %s: I1 now %.0f %% (overlap %.0f of %.0f m2); door axis %s' % (lv, rid, 100.0 * hit / len(cells), hit * idn.G * idn.G, area, ('x = %g' % cx) if along_z else ('z = %g' % cz)))
need = hit * idn.G * idn.G / 0.6 - area
print('  free area needed for < 60 %%: > %.1f m2 on "*" cells' % need)
W, H = int(x1 - x0), int(z1 - z0)
n_star = 0
for j in range(H):
    row = ''
    for i in range(W):
        x, z = x0 + i + 0.5, z0 + j + 0.5
        rx, rz = refl(x, z)
        if solid(x, z):
            row += '#'
        elif ramp(x, z) or any(q[0] <= x <= q[2] and q[1] <= z <= q[3] for q in pits):
            row += '~'
        elif any(math.hypot(x - a[0], z - a[2]) < 1.6 for a in pads) or any(math.hypot(x - a[0], z - a[2]) < 1.2 for a in spots) or near_line(x, z, 0.9) \
                or x < x0 + 0.5 or x > x1 - 0.5 or z < z0 + 0.5 or z > z1 - 0.5:
            row += '.'
        elif solid(rx, rz) or ramp(rx, rz):
            row += '-'
        else:
            row += '*'
            n_star += 1
    print('%6.1f %s' % (z0 + j + 0.5, row))
print('  "*" cells: %d m2' % n_star)
