#!/usr/bin/env python3
# NIGHT73 K2 (calm72 reworked; the lead's bar: with `-skygrindRows72,calm72:1` the hurt frames show the red border at most 60 px deep).
# The depth of the red screen-edge border, in px from each screen edge.
#   python -I Tools/night/n73-edge.py <image> [<image> ...]      per image: the depth of each edge and the largest; then the mean of the largest
#   python -I Tools/night/n73-edge.py --selftest                 the planted cases (a thin frame, a thick one, none, and a HUD-like red bar the strips must not read)
# A pixel is RED when red dominates it: R exceeds both G and B by at least 8 (of 255) and R >= 51 (V >= 0.20). (First draft: n73-hue.py's danger band, hue 0-40 /
# 345-360, S >= 0.35. The planted case showed it blind to most of a frame's ramp: over a cool plum wall the overlay only enters that band where its alpha is already
# about 0.3, so a 135 px frame read 40 px. Red-dominant is reached at an alpha of about 0.14, so the depth follows the ramp: planted thin 0.94 reads about 38 px of its
# 58, planted thick 0.86 about 89 px of its 135.) Per edge, a strip is the line of pixels at distance d from the
# edge over a window that keeps clear of the HUD (the sides: the middle half of the height; the top: x 20-32 % and 68-80 %, beside the boss bar and clear of the
# corners' panels; the bottom: x 27-45 %, left of the muzzle and right of the health plate). The share of red pixels in the strip, moving-averaged over 5 px, is the
# profile; the DEPTH is the first d where it falls under 20 % (0 when the edge itself is under 20 %: no border). The meter prints the share at the edge too, and the
# window's share 300 px in (a scene that is red inside reads a border that never ends: the depth then says >= the scan limit and the line says so).
# It measures what the frame shows (L-093): the overlay's alpha ramp shows as red only where it pushes the scene's pixel into the band, so the depth is a floor on the
# ramp's own depth, the same on both sides of a pair (read the OFF side too: the thick frame reads about 90-135 px).
import sys
import numpy as np
from PIL import Image

LIMIT_SIDES, LIMIT_TOP = 400, 300
UNDER = 0.20


def red_mask(im):
    a = np.asarray(im.convert('RGB')).astype(np.int32)
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    return (r - np.maximum(g, b) >= 8) & (r >= 51)


def smooth(v, k=5):
    if len(v) < k:
        return v
    return np.convolve(np.pad(v, (k // 2, k // 2), mode='edge'), np.ones(k) / k, mode='valid')


def first_under(profile):
    for d, s in enumerate(profile):
        if s < UNDER:
            return d
    return None  # never fell under: red all the way to the scan limit


def depths(path_or_im):
    im = Image.open(path_or_im) if isinstance(path_or_im, str) else path_or_im
    m = red_mask(im)
    h, w = m.shape
    out = {}
    ys = slice(int(h * 0.25), int(h * 0.75))
    left = m[ys, :LIMIT_SIDES].mean(axis=0)
    right = m[ys, ::-1][:, :LIMIT_SIDES].mean(axis=0)
    xr = [slice(int(w * 0.20), int(w * 0.32)), slice(int(w * 0.68), int(w * 0.80))]
    top = np.mean([m[:LIMIT_TOP, s].mean(axis=1) for s in xr], axis=0)
    bot = m[::-1][:LIMIT_TOP, slice(int(w * 0.27), int(w * 0.45))].mean(axis=1)
    for name, prof in (('left', left), ('right', right), ('top', top), ('bottom', bot)):
        p = smooth(np.asarray(prof, dtype=float))
        # the literal reading (the share falls under 20 %: None when the scene is red inside, so the strip never falls) and the baseline-relative one (the red the
        # scene itself has 40 px before the scan limit is the floor: the depth is where the EXCESS over it falls under 20 % of the edge's excess; no border under 0.20 excess)
        base = float(p[-40:].mean())
        edge = float(p[:3].mean())
        excess = edge - base
        rel = 0
        if excess >= UNDER:
            thr = base + UNDER * excess
            rel = next((d for d, s in enumerate(p) if s < thr), len(p))
        out[name] = {'abs': first_under(p), 'rel': rel, 'edge': edge, 'base': base}
    return out


def line(name, res):
    parts = []
    big = 0
    skipped = []
    for k in ('left', 'right', 'top', 'bottom'):
        r = res[k]
        if r['base'] > 0.30:  # a strip that is red over 30 % of its length inside (an orange floor, the lava / pillar bars on the left) cannot show a border cleanly: not read
            parts.append('%s n/a (the scene is %.0f %% red there)' % (k, r['base'] * 100))
            skipped.append(k)
            continue
        txt = '%s %d px' % (k, r['rel'])
        if r['abs'] is None or r['abs'] != r['rel']:
            txt += ' (edge %.0f %% red, inside %.0f %%%s)' % (r['edge'] * 100, r['base'] * 100, '' if r['abs'] is None else '; literal under-20 %% reading %d px' % r['abs'])
        else:
            txt += ' (edge %.0f %% red)' % (r['edge'] * 100)
        parts.append(txt)
        big = max(big, r['rel'])
    print('%-92s %s  | deepest %d px' % (name, ', '.join(parts), big))
    return big, bool(skipped)


def thin_overlay(base, clear, edge_alpha, widget_alpha, colour=(255, 40, 10)):
    """The calm72 / UiEdge31 frame as Ui draws it: alpha = smoothstep((max(|x|, |y|) - clear) / (1 - clear)) x edge_alpha x widget_alpha over the scene."""
    h, w = base.shape[:2]
    x = np.abs((np.arange(w) + 0.5) / w * 2 - 1)[None, :]
    y = np.abs((np.arange(h) + 0.5) / h * 2 - 1)[:, None]
    t = np.clip((np.maximum(x, y) - clear) / (1 - clear), 0, 1)
    a = (t * t * (3 - 2 * t) * edge_alpha * widget_alpha)[..., None]
    return np.clip(base * (1 - a) + np.array(colour, dtype=float) * a, 0, 255).astype(np.uint8)


def selftest():
    rng = np.random.default_rng(7)
    w, h = 1920, 1080
    scene = np.zeros((h, w, 3), dtype=float)
    scene[...] = (82, 62, 120)  # the arena's cool plum walls (kiln73 ON)
    scene += rng.normal(0, 4, scene.shape)  # grain
    bar = scene.copy()
    bar[95:108, 640:1280] = (255, 70, 40)  # the boss bar: danger red-orange, top centre, 13 px tall, y 95-108: inside the top scan depth
    bar[960:1040, 100:460] = (240, 90, 30)  # the health plate's orange rim, bottom left
    cases = [
        ('no overlay', np.clip(scene, 0, 255).astype(np.uint8), lambda d: d == 0),
        ('thin (0.94, hurt 0.8 x widget 0.6 = the first frame of a hit)', thin_overlay(scene, 0.94, 0.8, 0.6), lambda d: 0 < d <= 60),
        ('thin + a HUD bar and plate planted (must not be read)', thin_overlay(bar, 0.94, 0.8, 0.6), lambda d: 0 < d <= 60),
        ('thick (UiEdge31 0.86, the frame calm72 replaces)', thin_overlay(scene, 0.86, 0.8, 0.6), lambda d: 60 < d <= 140),
    ]
    ok = True
    for name, img, want in cases:
        res = depths(Image.fromarray(img))
        big, open_end = line('selftest: ' + name, res)
        good = (not open_end) and want(big)
        print('   -> %s' % ('PASS' if good else 'FAIL'))
        ok = ok and good
    # the planted thin frame is measured against its own ramp: the red band is where the overlay moves the pixel into it, so the depth is under the ramp's 58 px
    print('selftest: %s' % ('all PASS' if ok else 'FAILED'))
    return 0 if ok else 1


if __name__ == '__main__':
    args = sys.argv[1:]
    if not args:
        print(__doc__ if __doc__ else 'usage: n73-edge.py <image> ... | --selftest')
        sys.exit(2)
    if args[0] == '--selftest':
        sys.exit(selftest())
    tot = []
    for p in args:
        big, _ = line(p, depths(p))
        tot.append(big)
    print('mean of the deepest edge %.1f px over %d frames; deepest of all %d px (the bar: at most 60)' % (sum(tot) / len(tot), len(tot), max(tot)))
