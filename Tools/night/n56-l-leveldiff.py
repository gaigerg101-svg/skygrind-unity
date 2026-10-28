# NIGHT56 builder L: what changed in a generated level file against a git revision (default HEAD): the top-level keys, the campaign's keys
# and, per campaign room, the fields that differ. A regenerated level must change only the rows its plan edit names.
#   python -I Tools/night/n56-l-leveldiff.py <level> [rev]
import json, subprocess, sys

lv = sys.argv[1]
rev = sys.argv[2] if len(sys.argv) > 2 else 'HEAD'
path = 'Design/levels/n52-%s.json' % lv
a = json.loads(subprocess.run(['git', 'show', rev + ':' + path], capture_output=True, text=True, encoding='utf-8').stdout)
b = json.load(open(path, encoding='utf-8'))
out = []
for k in sorted(set(a) | set(b)):
    if k != 'campaign' and a.get(k) != b.get(k):
        out.append('top: ' + k)
ca, cb = a.get('campaign', {}), b.get('campaign', {})
for k in sorted(set(ca) | set(cb)):
    if k != 'rooms' and ca.get(k) != cb.get(k):
        out.append('campaign: ' + k)
ra = {r['id']: r for r in ca.get('rooms', [])}
rb = {r['id']: r for r in cb.get('rooms', [])}
for i in sorted(set(ra) | set(rb)):
    x, y = ra.get(i, {}), rb.get(i, {})
    d = [kk for kk in sorted(set(x) | set(y)) if x.get(kk) != y.get(kk)]
    if d:
        out.append('room %s: %s' % (i, ', '.join(d)))
print('%s vs %s: %s' % (path, rev, '; '.join(out) if out else 'no change'))
if a.get('generatedBy') != b.get('generatedBy'):
    print('  generatedBy was: %s' % str(a.get('generatedBy'))[:160])
    print('  generatedBy now: %s' % str(b.get('generatedBy'))[:160])
da, db = a.get('data'), b.get('data')
if isinstance(da, dict) and da != db:
    print('  data keys that differ: %s' % [k for k in sorted(set(da) | set(db or {})) if da.get(k) != (db or {}).get(k)])
