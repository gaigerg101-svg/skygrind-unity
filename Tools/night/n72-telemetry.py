#!/usr/bin/env python
# NIGHT72 builder B (leftover 8 and the H5 check): a reel's telemetry.jsonl as MOTION numbers. A still frame cannot show a kick, a spin or a spring; the reel's per-frame records can.
#   python -I Tools/night/n72-telemetry.py cam    <label> <on/telemetry.jsonl> <off/telemetry.jsonl>     the camera's pitch / fov / roll against the OFF run, frame by frame
#   python -I Tools/night/n72-telemetry.py reticle <label> <on/telemetry.jsonl> <off/telemetry.jsonl>    the reticle's `spin` (and `spinKind`) over the run, ON against OFF
#   python -I Tools/night/n72-telemetry.py pump   <label> <on/telemetry.jsonl> <off/telemetry.jsonl>     the shotgun's `pump` phase: frames pumping, longest run, ON against OFF
#   python -I Tools/night/n72-telemetry.py enemy  <label> <on/telemetry.jsonl> <off/telemetry.jsonl>    the first enemy's screen rect (size, centre) over the run, ON against OFF
#   python -I Tools/night/n72-telemetry.py spinlen <label> <on/telemetry.jsonl> <off/telemetry.jsonl>   each reticle turn's episode (start s, duration s, spinKind), ON against OFF (weapons42: reticle42's
#                                                                                                       own check is "the tap's turn over by 0.27 s, before 0.34 s; the cannon's by 0.46 s, before 1.82 s")
#   python -I Tools/night/n72-telemetry.py pumplen <label> <on/telemetry.jsonl> <off/telemetry.jsonl>   each THUNDERCLAP pump episode (start s, duration s), ON against OFF (weapons41; pump40: 0.22 s)
#   python -I Tools/night/n72-telemetry.py keys   <telemetry.jsonl>                                     the keys of the frame records (what a reel can measure)
#   python -I Tools/night/n72-telemetry.py --planted                                                    the tool's own planted case (exit 0 when it reads right)
# Both runs are deterministic (the reel's fixed clock), so frame i of ON and frame i of OFF are the same sim step; the numbers are differences at the same frame. No bar is set here:
# the tool prints the numbers a verdict cites; a bar belongs to the slate's doneWhen.
import json
import math
import sys


def load(path):
    rows = []
    with open(path, encoding='utf-8') as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            try:
                r = json.loads(line)
            except ValueError:
                continue
            if r.get('type') == 'frame':
                rows.append(r)
    return rows


def pair(a, b):
    n = min(len(a), len(b))
    return a[:n], b[:n]


def cam(on, off, label):
    a, b = pair(on, off)
    d_pitch = [abs(x['cam']['pitch'] - y['cam']['pitch']) for x, y in zip(a, b)]
    d_fov = [abs(x['cam']['fov'] - y['cam']['fov']) for x, y in zip(a, b)]
    d_roll = [abs(x['cam']['roll'] - y['cam']['roll']) for x, y in zip(a, b)]
    frames = [i for i, v in enumerate(d_pitch) if v > 1e-6]
    fv = [i for i, v in enumerate(d_fov) if v > 1e-6]
    print(f"{label}\tframes {len(a)}\tcamera pitch differs on {len(frames)} frames, max {max(d_pitch, default=0):.4f} (units of the log's pitch), first at frame {frames[0] if frames else '-'}"
          f"\tfov differs on {len(fv)} frames, max {max(d_fov, default=0):.4f} deg, first at {fv[0] if fv else '-'}\troll max {max(d_roll, default=0):.4f}")
    if frames:
        i0 = frames[0]
        print(f"{label}\tON pitch from frame {i0}: " + ' '.join(f"{x['cam']['pitch']:.3f}" for x in a[i0:i0 + 12]))
        print(f"{label}\tOFF pitch from frame {i0}: " + ' '.join(f"{x['cam']['pitch']:.3f}" for x in b[i0:i0 + 12]))
    return len(frames)


def series(rows, key):
    return [float(r.get(key, 0) or 0) for r in rows]


def reticle(on, off, label):
    a, b = pair(on, off)
    sa, sb = series(a, 'spin'), series(b, 'spin')

    def stats(s):
        speeds = [abs(s[i] - s[i - 1]) for i in range(1, len(s))]
        return (max(s, default=0), sum(1 for v in s if abs(v) > 1e-9), max(speeds, default=0), sum(speeds) / max(1, len(speeds)))

    ma, na, va, ea = stats(sa)
    mb, nb, vb, eb = stats(sb)
    print(f"{label}\tframes {len(a)}\treticle spin: ON max {ma:.3f} on {na} frames, fastest step {va:.4f} per frame, mean step {ea:.4f}\tOFF max {mb:.3f} on {nb} frames, fastest step {vb:.4f}, mean step {eb:.4f}")
    kinds = sorted(set(int(r.get('spinKind', 0) or 0) for r in a)), sorted(set(int(r.get('spinKind', 0) or 0) for r in b))
    print(f"{label}\tspinKind values ON {kinds[0]} OFF {kinds[1]}")
    return (va, vb)


def pump(on, off, label):
    a, b = pair(on, off)

    def stats(rows):
        p = [float(r.get('pump', -1) if r.get('pump', -1) is not None else -1) for r in rows]
        live = [i for i, v in enumerate(p) if v >= 0]
        run = best = 0
        for i, v in enumerate(p):
            run = run + 1 if v >= 0 else 0
            best = max(best, run)
        return len(live), best, (live[0] if live else -1), max(p, default=-1)

    la, ba, fa, ma = stats(a)
    lb, bb, fb, mb = stats(b)
    print(f"{label}\tframes {len(a)}\tpump: ON live on {la} frames, longest run {ba}, first {fa}, max {ma:.3f}\tOFF live on {lb} frames, longest run {bb}, first {fb}, max {mb:.3f}")
    return (ba, bb)


def enemy(on, off, label):
    a, b = pair(on, off)
    n = 0
    sizes_a, sizes_b = [], []
    for x, y in zip(a, b):
        ea, eb = x.get('enemies') or [], y.get('enemies') or []
        if ea and eb:
            ra, rb = ea[0].get('rect'), eb[0].get('rect')
            if ra and rb:
                n += 1
                sizes_a.append((ra[2], ra[3]))
                sizes_b.append((rb[2], rb[3]))
    if not n:
        print(f"{label}\tno enemy rect in both runs (keys: " + ', '.join(sorted((a[0].get('enemies') or [{}])[0].keys()) if a and a[0].get('enemies') else []) + ')')
        return 0

    def rms(s):
        h = [v[1] for v in s]
        m = sum(h) / len(h)
        return math.sqrt(sum((v - m) ** 2 for v in h) / len(h))

    print(f"{label}\tframes with the enemy in both {n}\theight rms about its mean: ON {rms(sizes_a):.2f} px, OFF {rms(sizes_b):.2f} px\tmax height ON {max(v[1] for v in sizes_a):.1f} OFF {max(v[1] for v in sizes_b):.1f}")
    return n


def episodes(rows, live):
    """The runs of consecutive frames where live(row) is true: [(start t s, duration s, first row)]. A run's duration is its last frame's t minus its first frame's t plus one frame."""
    out = []
    start = None
    prev_t = 0.0
    for r in rows:
        t = float(r.get('t', 0) or 0)
        if live(r):
            if start is None:
                start = (t, r)
        elif start is not None:
            out.append((start[0], prev_t - start[0] + 1.0 / 60.0, start[1]))
            start = None
        prev_t = t
    if start is not None:
        out.append((start[0], prev_t - start[0] + 1.0 / 60.0, start[1]))
    return out


def lens(on, off, label, what, live, tag):
    a, b = pair(on, off)
    ea, eb = episodes(a, live), episodes(b, live)

    def fmt(es):
        return ', '.join(f"t {t:.2f} s {d:.2f} s{tag(r)}" for t, d, r in es) or 'none'

    print(f"{label}\t{what}: ON {len(ea)} episodes: {fmt(ea)}")
    print(f"{label}\t{what}: OFF {len(eb)} episodes: {fmt(eb)}")
    if ea and eb and len(ea) == len(eb):
        print(f"{label}\t{what}: duration ON / OFF per episode: " + ', '.join(f"{x[1]:.2f}/{y[1]:.2f}" for x, y in zip(ea, eb)))
    return (ea, eb)


def spinlen(on, off, label):
    return lens(on, off, label, 'reticle turn (spin > 0)', lambda r: float(r.get('spin', 0) or 0) > 0.001, lambda r: f" (spinKind {int(r.get('spinKind', 0) or 0)})")


def pumplen(on, off, label):
    return lens(on, off, label, 'THUNDERCLAP pump (phase >= 0)', lambda r: float(r.get('pump', -1) if r.get('pump', -1) is not None else -1) >= 0, lambda r: '')


def keys(rows):
    r = rows[len(rows) // 2]
    for k, v in r.items():
        print(f"{k}: {str(v)[:150]}")


def planted():
    # two runs of 20 frames: ON kicks the pitch for 6 frames at frame 5; the spin ON is faster than OFF; the pump runs 4 frames ON and none OFF
    def run(on):
        out = []
        for i in range(20):
            kick = (0.02 * math.sin(math.pi * (i - 5) / 6.0)) if on and 5 <= i < 11 else 0.0
            spin = ((i - 2) / 8.0 if 3 <= i <= 10 else 0.0) if on else ((i - 2) / 14.0 if 3 <= i <= 16 else 0.0)  # the turn ON takes 8 frames, OFF 14
            out.append({'type': 'frame', 'f': i, 't': i / 60.0, 'cam': {'pitch': 0.1 + kick, 'fov': 74 - (1.2 if on and 5 <= i < 8 else 0), 'roll': 0},
                        'spin': spin, 'spinKind': 1, 'pump': (0.1 * (i - 8) if on and 8 <= i < 12 else -1),
                        'enemies': [{'rect': [100, 100, 40, 80 + (4 if on and i % 2 else 0)]}]})
        return out

    on, off = run(True), run(False)
    ok = cam(on, off, 'planted') == 5  # the planted kick's sine is 0 at its first frame: it differs on 5 of its 6 frames
    reticle(on, off, 'planted')
    ea, eb = spinlen(on, off, 'planted')
    ok = ok and len(ea) == 1 and len(eb) == 1 and abs(ea[0][1] - 8 / 60.0) < 1e-6 and abs(eb[0][1] - 14 / 60.0) < 1e-6  # 8 frames ON, 14 OFF
    pa, pb = pumplen(on, off, 'planted')
    ok = ok and len(pa) == 1 and abs(pa[0][1] - 4 / 60.0) < 1e-6 and len(pb) == 0
    ba, bb = pump(on, off, 'planted')
    ok = ok and ba == 4 and bb == 0
    ok = ok and enemy(on, off, 'planted') == 20
    print('planted case', 'PASS' if ok else 'FAIL')
    return 0 if ok else 1


if __name__ == '__main__':
    if len(sys.argv) >= 2 and sys.argv[1] == '--planted':
        sys.exit(planted())
    if len(sys.argv) == 3 and sys.argv[1] == 'keys':
        keys(load(sys.argv[2]))
        sys.exit(0)
    if len(sys.argv) != 5 or sys.argv[1] not in ('cam', 'reticle', 'pump', 'enemy', 'spinlen', 'pumplen'):
        print(__doc__)
        sys.exit(2)
    mode, label = sys.argv[1], sys.argv[2]
    on, off = load(sys.argv[3]), load(sys.argv[4])
    {'cam': cam, 'reticle': reticle, 'pump': pump, 'enemy': enemy, 'spinlen': spinlen, 'pumplen': pumplen}[mode](on, off, label)
