import json, glob, os
def zone(b):
    return b.get('zone')
for f in sorted(glob.glob('Design/levels/n47/*.plan.json')) + ['Design/levels/n46/foundry.plan.json']:  # foundry's geometry: n46 (the forge's review)
    p = json.load(open(f))
    print('==', os.path.basename(f), 'boxes', len(p.get('boxes', [])), 'ramps', len(p.get('ramps', [])), 'pads', len(p.get('pads', [])),
          'grapple', len(p.get('grapple', [])), 'pits', len(p.get('pits', [])))
    for r in p['rooms']:
        if r.get('kind') not in ('room', 'boss'):
            continue
        rc = r['rect']
        w, d = rc[2] - rc[0], rc[3] - rc[1]
        n = sum(1 for b in p.get('boxes', []) if zone(b) == r['id'])
        m = sum(1 for b in p.get('ramps', []) + p.get('pads', []) + p.get('grapple', []) if zone(b) == r['id'])
        m += len(r.get('grappleNodes', [])) + len(r.get('pads', []))  # the forge's dry run: anchors live in the room rows
        print('  ', r['id'], r.get('kind'), '%dx%d m' % (w, d), 'boxes', n, 'ramps/pads/grapple', m,
              'pieces per 100 m2 %.2f' % ((n + m) * 100.0 / (w * d)), 'height', r.get('height'))
