import io, sys
p = 'Tools/feel-metrics.py'
s = io.open(p, encoding='utf-8', newline='').read()
crlf = chr(13) + chr(10) in s
if crlf:
    s = s.replace(chr(13) + chr(10), chr(10))

meters = '''
# ---------------------------------------------------------------------------------------------------------------
# NIGHT22 meters (the lead; each has a synthetic self-test in `selftest`, as M-MAGENTA had)
#   T11     the RESULTS grade card ("Grade" group) must not intersect the POINTS group on any frame (T-11, open since w18)
#   LUM     the mean frame luminance (0-255): per scenario the median of the frame means; the `dusk` scenario is read in three equal
#           time thirds (rounds 1, 8, 15 in order) and the medians must not rise by more than LUM_SLACK (the dusk clock sinks)
#   STAMP-T the entrance time of any element whose path contains "stamp": first frame to settled (alpha >= 95 % of rest, scale
#           within 3 %), target 200-300 ms (NIGHT22.md U3-36.4: our tuning; M9's 90-300 ms is the floor)
LUM_SLACK = 1.0
STAMP_MS = (200.0, 300.0)


def leaf_of(e):
    return (e.get('path') or '').rstrip('/').split('/')[-1].strip().lower()


def t11_frames(frames):
    """(frames with both groups listed, the list of frame numbers where their rects intersect)."""
    both, bad = 0, []
    for fr in frames:
        pts = grade = None
        for e in ui_list(fr):
            if draws_nothing(e):
                continue
            r = rect_of(e.get('rect'))
            if r is None:
                continue
            leaf = leaf_of(e)
            if leaf == 'points':
                pts = r
            elif leaf == 'grade':
                grade = r
        if pts is not None and grade is not None:
            both += 1
            if rects_intersect(pts, grade):
                bad.append(fr['f'])
    return both, bad


def t11(sc, ctx):
    both, bad = t11_frames(sc.frames)
    if not both:
        return {'T11': NA('no frame lists both the POINTS and the Grade groups')}
    return {'T11': PF(not bad, '%d of %d frames intersect' % (len(bad), both),
                      detail=['f%d' % f for f in bad[:12]], num_=len(bad))}


def lum_of(rgb):
    a = np.asarray(rgb, dtype=np.float64)
    return float((0.2126 * a[..., 0] + 0.7152 * a[..., 1] + 0.0722 * a[..., 2]).mean())


def lum_thirds(vals):
    """The medians of the frame means in three equal time thirds (needs >= 3 values)."""
    n = len(vals)
    if n < 3:
        return None
    cuts = [0, n // 3, 2 * (n // 3), n]
    return [statistics.median(vals[cuts[i]:cuts[i + 1]]) for i in range(3)]


def lum_monotonic(thirds, slack=LUM_SLACK):
    return all(thirds[i + 1] <= thirds[i] + slack for i in range(len(thirds) - 1))


def lum(sc, ctx):
    vals = []
    for fr in sc.frames:
        im = sc.load_rgb(fr['f'])
        if im is not None:
            vals.append(lum_of(im))
    if not vals:
        return {'LUM': NA('no frame images in the scenario')}
    med = statistics.median(vals)
    if sc.name != 'dusk':
        return {'LUM': R('info', '%.1f median of %d frames' % (med, len(vals)), 'info only outside the dusk scenario')}
    th = lum_thirds(vals)
    if th is None:
        return {'LUM': NA('fewer than 3 frames')}
    ok = lum_monotonic(th)
    return {'LUM': PF(ok, 'thirds %s' % ' > '.join('%.1f' % t for t in th), 'must fall (slack %.1f)' % LUM_SLACK, num_=th[0] - th[2])}


def stamp_settle_ms(series, fps):
    """series = [(alpha, scale)] per frame from the element's first listed frame; rest = the median of the last 10; the ms to
    the first frame from which every later frame (to the run's end or 6 frames) is settled."""
    if len(series) < 3:
        return None
    tail = series[-10:]
    ra = statistics.median([a for a, _ in tail])
    rs = statistics.median([sv for _, sv in tail])
    oks = [a >= 0.95 * ra - 1e-9 and abs(sv - rs) <= 0.03 * max(rs, 1e-6) for a, sv in series]
    for j in range(len(series)):
        if all(oks[j:j + 6]):
            return j * 1000.0 / fps
    return None


def stamp_t(sc, ctx):
    runs = _ui_runs(sc)
    times, bad = [], []
    for eid, rl in runs.items():
        for run in rl:
            if 'stamp' not in (run[0][2].get('path') or '').lower() or run[0][0] == 0:
                continue
            ser = [(num(x[2].get('alpha'), 1.0), num(x[2].get('scale'), 1.0)) for x in run]
            d = stamp_settle_ms(ser, sc.fps)
            times.append(d)
            if d is None or not (STAMP_MS[0] <= d <= STAMP_MS[1]):
                bad.append('%s f%d: %s' % (run[0][2].get('path'), run[0][1], 'never settles' if d is None else '%d ms' % round(d)))
    if not times:
        return {'STAMP-T': NA('no stamp element enters in the scenario')}
    ds = [d for d in times if d is not None]
    rng = (' (%d-%d ms)' % (round(min(ds)), round(max(ds)))) if ds else ''
    return {'STAMP-T': PF(not bad, '%d of %d in %d-%d ms%s' % (len(times) - len(bad), len(times), STAMP_MS[0], STAMP_MS[1], rng),
                          detail=bad, num_=len(bad))}

'''
marker = '''# ---------------------------------------------------------------------------------------------------------------
# The table
'''
assert s.count(marker) == 1
s = s.replace(marker, meters + marker)

old = "    ('M-MAGENTA', 'M-MAGENTA', '0 px within 40 of (255, 0, 255) on every frame', m_magenta),\n)"
new = ("    ('M-MAGENTA', 'M-MAGENTA', '0 px within 40 of (255, 0, 255) on every frame', m_magenta),\n"
       "    ('T11', 'T11', 'the Grade card never intersects POINTS', t11),\n"
       "    ('LUM', 'LUM', 'dusk: the thirds do not rise', lum),\n"
       "    ('STAMP-T', 'STAMP-T', 'stamp entrance 200-300 ms', stamp_t),\n)")
assert s.count(old) == 1
s = s.replace(old, new)

# selftests: pure helpers, after the M-MAGENTA row checks
anchor = "    ctx = {'roles': roles, 'roles_err': None}\n    b_rows, (bh, _) = compute(Reel(base), dict(ctx))"
assert s.count(anchor) == 1
st = '''    # NIGHT22 T11 / LUM / STAMP-T: synthetic checks of the pure helpers (each seen both ways)
    def _t(tag, got, want):
        print('  %-4s N22 %-28s %s (want %s)' % ('ok' if got == want else 'BAD', tag, got, want))
        if got != want:
            fails.append(('N22', tag, want, got))
    def _fr22(grade_rect):
        return {'f': 0, 'ui': [{'path': 'Results/Points', 'rect': [100, 100, 300, 200]}, {'path': 'Results/Grade', 'rect': grade_rect}]}
    _t('T11 overlapping rects', t11_frames([_fr22([350, 150, 200, 100])])[1], [0])
    _t('T11 apart rects', t11_frames([_fr22([500, 100, 200, 100])])[1], [])
    _t('LUM falling thirds', lum_monotonic([120.0, 100.0, 80.0]), True)
    _t('LUM rising thirds', lum_monotonic([80.0, 100.0, 120.0]), False)
    _t('LUM thirds of 9', lum_thirds([9, 8, 7, 6, 5, 4, 3, 2, 1]), [8, 5, 2])
    ser_ok = [(i / 12.0 if i < 12 else 1.0, 1.0) for i in range(30)]   # 12 frames ramp at 60 fps = 200 ms
    ser_slow = [(i / 40.0 if i < 40 else 1.0, 1.0) for i in range(60)]  # 40 frames = 667 ms
    _t('STAMP-T 200 ms ramp in range', STAMP_MS[0] - 20 <= (stamp_settle_ms(ser_ok, 60) or 0) <= STAMP_MS[1], True)
    _t('STAMP-T 667 ms ramp out of range', STAMP_MS[0] <= (stamp_settle_ms(ser_slow, 60) or 0) <= STAMP_MS[1], False)
'''
s = s.replace(anchor, st + anchor)
if crlf:
    s = s.replace(chr(10), chr(13) + chr(10))
io.open(p, 'w', encoding='utf-8', newline='').write(s)
print('ok')
