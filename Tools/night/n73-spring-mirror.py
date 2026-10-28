#!/usr/bin/env python3
# NIGHT73 builder B (spring72 a player can see): the Python mirror of Assets/Scripts/Game/Spring72.cs's Spring72Math.Step (the same semi-implicit Euler in
# sub-steps of w h <= 0.5, max 24) and of the flinch layer in EnemyViews.Spring72.cs (a hit = hp fell: kick the speed; the spring rings back to 0), with
# the hit reaction that is ALREADY in the game under it (HitSpring.cs's exact damped solution, k 1500, c 36: crunch36's squash36 kicked to Crunch36Squash 0.15),
# so the ON half of a pair is told from the OFF half by what the spring layer adds. float32 is not mirrored (the maths agree to 1e-5 at these sizes).
#   python -I Tools/night/n73-spring-mirror.py table <hz> <zeta> <kick> [fps] [cap]    one hit at frame 0: the spring's squash, the existing pop and the ON / OFF heights per frame
#   python -I Tools/night/n73-spring-mirror.py scan                                     the candidates side by side (peak, rebound, settle, the extra px at 5 m and 12 m)
#   python -I Tools/night/n73-spring-mirror.py stream <hz> <zeta> <kick> <cap> <every>  a gun's hit every <every> frames for 2 s: the largest |squash| with and without the cap
import math, sys

W0, MAXSQ = 0.2, 0.2   # Spring72MaxSquash: the sum of breath, wind-up and flinch is held to +-0.2
FOV_DEG, SCREEN_H, BODY_M = 74.0, 1080.0, 1.8   # camera.fovDeg (vertical), 1080p, a husk's height

def step(x, v, target, hz, zeta, dt):
    if dt <= 0 or hz <= 0:
        return x, v
    w = 2 * math.pi * hz
    n = max(1, min(24, math.ceil(w * dt / 0.5)))
    h = dt / n
    for _ in range(n):
        v += (-2 * zeta * w * v - w * w * (x - target)) * h
        x += v * h
    return x, v

def peak_per_velocity(hz, zeta):
    # the continuous peak of an impulse response per unit of speed: exp(-zeta w tp) sin(wd tp) / wd, tp = atan2(wd, zeta w) / wd
    w = 2 * math.pi * hz
    wd = w * math.sqrt(1 - zeta * zeta)
    tp = math.atan2(wd, zeta * w) / wd
    return math.exp(-zeta * w * tp) * math.sin(wd * tp) / wd

def kick(x, v, peak, cap, hz, zeta):
    # TRIED AND NOT USED (stream mode below): HitSpring.Kick's rule on this spring (the spring on its way back stops first; the peak it is heading for never passes cap). It used the continuous
    # peak per speed, which a 60 fps frame reads about 15 % high, so it cut every kick short (a hit every 7 frames: 14.9 % against 17.7 % with a plain add); and a plain add never passed
    # 18 % in any stream tried (every 4, 7, 10, 15 frames), so the game keeps the plain add of NIGHT72: v += kick.
    if peak == 0:
        return x, v
    sgn = 1.0 if peak > 0 else -1.0
    ppv = peak_per_velocity(hz, zeta)
    if v * sgn < 0:
        v = 0.0
    ahead = x * sgn + v * sgn * ppv
    add = min(abs(peak), max(0.0, cap - ahead))
    return x, v + sgn * add / ppv

def hit_spring_advance(x, v, dt, k=1500.0, c=36.0):
    # HitSpring.Advance: the exact damped path
    s = c * 0.5
    wd = math.sqrt(max(k - s * s, 1e-6))
    e, co, si = math.exp(-s * dt), math.cos(wd * dt), math.sin(wd * dt)
    nx = e * (x * co + (v + s * x) / wd * si)
    nv = e * (v * co - ((wd * wd + s * s) * x + s * v) / wd * si)
    return nx, nv

def existing_pop(frames, fps, peak=0.15, k=1500.0, c=36.0):
    # crunch36's squash36 kicked from rest at frame 0 to a peak of `peak` (HitSpring.Kick), advanced a frame at a time
    s = c * 0.5
    wd = math.sqrt(k - s * s)
    tp = math.atan2(wd, s) / wd
    ppv = math.exp(-s * tp) * math.sin(wd * tp) / wd
    x, v = 0.0, peak / ppv
    out = []
    for _ in range(frames):
        x, v = hit_spring_advance(x, v, 1.0 / fps, k, c)
        out.append(x)
    return out

def px_per_unit(d):
    # screen px of one unit of the husk's height at d metres (a vertical field of view of FOV_DEG)
    return SCREEN_H * BODY_M / (2 * d * math.tan(math.radians(FOV_DEG / 2)))

def run(hz, zeta, k, fps, frames=30, cap=None):
    x, v = 0.0, k
    out = []
    for _ in range(frames):
        x, v = step(x, v, 0.0, hz, zeta, 1.0 / fps)
        out.append(max(-MAXSQ, min(MAXSQ, x)))
    return out

def stats(sq, fps, tol=0.015):
    lo = min(sq)
    i_lo = sq.index(lo)
    hi = max(sq[i_lo:])
    i_hi = sq.index(hi, i_lo)
    settle = 0
    for i, s in enumerate(sq):
        if abs(s) > tol:
            settle = i + 1
    return lo, i_lo + 1, hi, i_hi + 1, settle

def table(hz, zeta, k, fps, cap):
    sq = run(hz, zeta, k, fps)
    pop = existing_pop(len(sq), fps)
    p5, p12 = px_per_unit(5), px_per_unit(12)
    print('hz %.2f zeta %.2f kick %.2f fps %d | frame, spring squash (-) / stretch (+), existing pop (+ = squash), OFF height, ON height, extra px at 5 m, at 12 m' % (hz, zeta, k, fps))
    for i, (s, q) in enumerate(zip(sq, pop)):
        off, on = 1 - q, (1 - q) * (1 + s)
        print('f%02d %+0.3f %+0.3f %.3f %.3f %+6.1f %+6.1f' % (i + 1, s, q, off, on, (on - off) * p5, (on - off) * p12))
    lo, f_lo, hi, f_hi, settle = stats(sq, fps)
    print('peak squash %.3f at frame %d (%.0f ms), rebound stretch %.3f at frame %d (%.0f ms), under 1.5 %% from frame %d (%.0f ms)' % (
        -lo, f_lo, f_lo * 1000.0 / fps, hi, f_hi, f_hi * 1000.0 / fps, settle + 1, (settle + 1) * 1000.0 / fps))

def solve_kick(hz, zeta, peak, fps=60):
    # the kick (share of the height per s, negative: squash first) whose first squash peak, read at the frame ends of `fps`, is `peak` (bisection; the frame's coarse
    # integration reads about 15 % under the continuous peak, so the continuous formula is not the number to use)
    lo, hi = 0.0, 200.0
    for _ in range(60):
        mid = (lo + hi) / 2
        sq = run(hz, zeta, -mid, fps, frames=int(fps * 0.5))
        if -min(sq) < peak:
            lo = mid
        else:
            hi = mid
    return -(lo + hi) / 2

def scan():
    cands = [('NIGHT72 (-4.5, 5 Hz, z 0.3)', 5.0, 0.30, -4.5)]
    for peak in (0.15, 0.165, 0.18):
        for hz, z in ((6.0, 0.30), (7.0, 0.35), (7.0, 0.30), (8.0, 0.35)):
            cands.append(('%.1f %%, %.0f Hz z %.2f (kick %.2f)' % (peak * 100, hz, z, solve_kick(hz, z, peak)), hz, z, solve_kick(hz, z, peak)))
    p5, p12 = px_per_unit(5), px_per_unit(12)
    print('px of a 100 %% height: %.0f at 5 m, %.0f at 12 m' % (p5, p12))
    for name, hz, z, k in cands:
        for fps in (60, 120, 30):
            sq = run(hz, z, k, fps, frames=int(fps * 0.8))
            lo, f_lo, hi, f_hi, settle = stats(sq, fps)
            print('%-30s %3d fps | squash %.3f at %3.0f ms | stretch %.3f at %3.0f ms | under 1.5 %% at %3.0f ms | px at 5 m: squash %.0f, stretch %.0f' % (
                name, fps, -lo, f_lo * 1000.0 / fps, hi, f_hi * 1000.0 / fps, (settle + 1) * 1000.0 / fps, -lo * p5, hi * p5))

def stream(hz, zeta, k, cap, every):
    fps = 60
    for use_cap in (False, True):
        x, v, worst, worst_up = 0.0, 0.0, 0.0, 0.0
        for f in range(120):
            if f % every == 0:
                if use_cap:
                    x, v = kick(x, v, k * peak_per_velocity(hz, zeta), cap, hz, zeta)
                else:
                    v += k
            x, v = step(x, v, 0.0, hz, zeta, 1.0 / fps)
            worst = min(worst, x)
            worst_up = max(worst_up, x)
        print('hit every %d frames, %s: largest squash %.3f, largest stretch %.3f (before the +-%.2f clamp)' % (every, 'with the cap %.3f' % cap if use_cap else 'no cap', -worst, worst_up, MAXSQ))

if __name__ == '__main__':
    a = sys.argv[1:]
    if a[:1] == ['table'] and len(a) >= 4:
        table(float(a[1]), float(a[2]), float(a[3]), int(a[4]) if len(a) > 4 else 60, float(a[5]) if len(a) > 5 else None)
    elif a[:1] == ['scan']:
        scan()
    elif a[:1] == ['stream'] and len(a) >= 6:
        stream(float(a[1]), float(a[2]), float(a[3]), float(a[4]), int(a[5]))
    else:
        sys.exit(__doc__)
