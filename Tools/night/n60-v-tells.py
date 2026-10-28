# NIGHT60 builder V (B7 attempt 3): what n56-colour.py's "tells" are on one iceline shot, off vs on: its tell pixels (inside the enemy rects, the
# danger 5-35 or parry 38-70 hue, S >= 0.35, V >= 0.25), their median S (the sat guard's side), and by V and hue bins.
#   python -I Tools/night/n60-v-tells.py <shot substring, e.g. g2-entry> <root with v1off-iceline / v1on-iceline a357 folders>
import sys, os, importlib.util
sys.dont_write_bytecode = True
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location("n56colour", os.path.join(HERE, "n56-colour.py"))
M = importlib.util.module_from_spec(spec)
spec.loader.exec_module(M)
shot, root = sys.argv[1], sys.argv[2]
for side in ('v1off', 'v1on'):
    for d in M.scenario_dirs([os.path.join(root, side + '-iceline', 'a357')]):
        items, _ = M.frame_list(d, ['entry', 'mid'], False)
        for name, path, lv, room, kind, fr, extra, how in items:
            if shot not in os.path.basename(path):
                continue
            rgb = np.asarray(Image.open(path).convert('RGB'))
            meta = M.meta_of(fr, rgb.shape[0], extra)
            h, s, v, _ = M.hsv(rgb)
            en = np.zeros(s.shape, bool)
            for r in meta.get('enemies', []):
                M.rect_mask(en, r)
            tell = en & (M.band(h, *M.DANGER) | M.band(h, *M.PARRY_TELL)) & (s >= 0.35) & (v >= 0.25)
            print('%s %s: rects %d, tells n %d, median S %.3f, median V %.3f, median H %.1f' % (side, os.path.basename(path), len(meta.get('enemies', [])), tell.sum(), np.median(s[tell]), np.median(v[tell]), np.median(h[tell])))
            for lo, hi in ((0.25, 0.5), (0.5, 0.75), (0.75, 1.01)):
                m = tell & (v >= lo) & (v < hi)
                print('   V %.2f-%.2f: n %6d (%.0f %%), median S %.3f, median H %.1f' % (lo, hi, m.sum(), 100.0 * m.sum() / max(1, tell.sum()), np.median(s[m]) if m.any() else -1, np.median(h[m]) if m.any() else -1))
            for lo, hi in ((5, 15), (15, 25), (25, 35), (38, 70)):
                m = tell & M.band(h, lo, hi)
                print('   H %d-%d: n %6d (%.0f %%), median S %.3f, median V %.3f' % (lo, hi, m.sum(), 100.0 * m.sum() / max(1, tell.sum()), np.median(s[m]) if m.any() else -1, np.median(v[m]) if m.any() else -1))
