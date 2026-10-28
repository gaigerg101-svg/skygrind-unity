#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""feel-metrics.py: NIGHT18's reader for the feel reels (format: Tools/feel-schema.md).

  metrics  <reel> [--base <b1 reel>] [--prev <previous round's reel>] [--out <md>] [--roles <json>]
  flip     <reel> [--out <dir>]
  ab       <reel A> <reel B> --out <neutral dir> --key <key file> [--seed N] [--invert-key <old key>] [--force]
  gif      <reel A> <reel B> <scenario> [--out ab-<scenario>.gif] [--labels N17 N18]
  roles    <reel> [--roles Tools/feel-roles.json] [--suggest]
  selftest [--dir <scratch dir>] [--keep]

A <reel> is one side's folder (Logs/feel/b1/on): one sub-folder per scenario, each with telemetry.jsonl and
f%04d.jpg. A tag folder (Logs/feel/b1) that holds on/ is read as its on/.

Pixel targets are at 1080p: a reel at another size (the Editor's 2560 x 1440) is read at its header's w / h and its
px values are scaled by 1080 / h (M1's window by h / 1080 and its count by (1080 / h)^2).

Decisions (the lead's schema notes of 2026-10-06 09:0x, plus what the schema leaves open):
- "Frame 1" of an event is the frame whose line carries it. M1 = the shot's frame vs the frame before it, the window
  centred on the target's rect on that pre-frame, the ui[] rects of both frames masked. M2's "frames 1-4" are the
  kill's frame and the 3 after it; the kill marker's frame number is (its frame - the kill's frame + 1).
- M5 reads the body's own motion: (rect centre - enemies[].anchor) and (rect top - anchor y) against the pre-frame,
  so the camera's recoil kick (which moves the anchor too) doesn't count. No anchor: move / back are n/a and the raw
  rect motion is shown as the value.
- A freeze can start mid-frame (held false, heldSteps > 0): M4 sums heldSteps over the first run of frames that
  are held or swallow steps, starting within 30 frames of the shot (the input when there is no shot event). M3's
  "last held frame" and the flipbook's HELD tiles use the fully held frames of that run.
- M9's PASS / FAIL counts first appearances only (the schema); an element that leaves and pops back, or is reparented
  (the nav focus frame moving to another row: a new path), is listed as a re-entry in the value and the details. M9 and
  M10 skip a TMP text with no characters (listed by the scanner, but nothing is drawn). A run that starts within 6 frames of the reel's end is skipped (no rest).
  A pop-in's first frame is within 95-105 % of rest scale (an entrance from above rest is motion, not a pop).
  An element's entrance is judged on its run up to where, still for 10 frames, it starts moving again (a focus move:
  the scanner keeps a reparented element's first path); "rest" is that segment's last 10 frames.
  An entrance's time runs from its first frame to the first frame from which it stays (6 frames, or to its end)
  within 3 % of rest scale, at >= 95 % of rest alpha and within 3 px of rest: an ease-out-back reads at about its
  nominal length, not at its first crossing of rest.
- M8 "central 60 %" is the box from 20 % to 80 % of each side. A speedLines rect that contains that whole box is an
  outer bound (lines on every edge), which can't be told from lines in the centre: those frames are n/a.
- M11 matches plain string prefixes ("HUD/Pops/Pop" allows Pop0..Pop9); keys starting with "_" are comments.

Python 3.12 with PIL + numpy only. Never crashes on a missing scenario, fx group, frame line or JPG: the row says
n/a and why. Exit codes: 0 done, 1 roles violations (roles) or selftest failures, 2 bad input.
"""

import argparse
import bisect
import datetime
import json
import re
import math
import os
import random
import shutil
import statistics
import sys
import tempfile

import numpy as np
from PIL import Image, ImageDraw, ImageFont

# ---------------------------------------------------------------------------------------------------------------
# Numbers (from NIGHT18.md's metrics table and Tools/feel-schema.md)

LUMA_DELTA = 40            # M1: a pixel counts as changed when its luma moves more than this
M1_WINDOW = 400            # M1: the window (px) centred on the target
M2_FRAMES = 4              # M2: frames 1-4 of the kill
M3_AFTER_MS = 250          # M3: measured this long after the freeze's last held frame
HELD_SEARCH = 30           # M4 / flip: a freeze must start within this many frames of the shot
M4_TARGETS = {             # M4: held steps (120 Hz) per scenario; 5-kill blast: >= 18, capped at 160 ms = 19 steps
    'hit12': (0, 2), 'kill12': (6, 8), 'headshot12': (10, 13), 'punchkill': (11, 14), 'horde5': (18, 19),
}
POP_SCALE = 0.95           # M9: a pop-in is a first frame at >= 95 % rest scale ...
POP_SCALE_MAX = 1.05       # ... and <= 105 % (an entrance from above rest, a slam-in, is motion; inf = the literal rule)
POP_ALPHA = 0.95           # ... and >= 95 % rest alpha ...
POP_DIST = 40              # ... and < 40 px from rest
ENTRANCE_MS = (90, 300)    # M9: an animated entrance takes 90-300 ms
SETTLE_HOLD = 6            # M9: an entrance has settled once it stays at rest this many frames (100 ms at 60 fps)
TEXT_FLOOR = 18            # M10: px at 1080p
KILL_TILE = 480            # flip: kill / hit crops
WIDE_TILE = (960, 540)     # flip: camera / ui tiles
MARGIN = 28                # flip: the number strip above each tile row
UI_OFFSETS_MS = (0, 33, 67, 100, 167, 250, 400, 700)
KILL_AFTER_MS = (120, 250, 400)
GIF_SIDE = (640, 360)

KINDS_CROP = ('kill', 'hit')
ROLE_NAMES = ('GOLD', 'DANGER', 'RIVAL')

QUESTIONS = {
    'kill': 'Which is the more satisfying kill (does it read as a kill at a glance)?',
    'hit': 'Which shows the hit landing on the body?',
    'boost': 'Which boost feels faster?',
    'boostside': 'Which side boost feels faster?',
    'sprint': 'Which sprint feels faster?',
    'slide': 'Which slide feels lower and faster?',
    'strafe': 'Which strafe has more weight (the camera leaning into it)?',
    'land': 'Which landing has more weight?',
    'ui': 'Which menu enters with motion rather than appearing?',
    'hud': 'Which HUD answers with motion rather than popping in?',
}
TOO_MUCH = 'Is either one too much (noisy, hard to read, tiring)?'


# ---------------------------------------------------------------------------------------------------------------
# Small helpers

def num(v, default=None):
    if v is None or isinstance(v, bool):
        return default
    try:
        x = float(v)
    except (TypeError, ValueError):
        return default
    if math.isnan(x) or math.isinf(x):
        return default
    return x


def rect_of(v):
    if not isinstance(v, (list, tuple)) or len(v) < 4:
        return None
    vals = [num(x) for x in v[:4]]
    if any(x is None for x in vals):
        return None
    return tuple(vals)


def rect_union(a, b):
    if a is None:
        return b
    if b is None:
        return a
    x0, y0 = min(a[0], b[0]), min(a[1], b[1])
    x1, y1 = max(a[0] + a[2], b[0] + b[2]), max(a[1] + a[3], b[1] + b[3])
    return (x0, y0, x1 - x0, y1 - y0)


def rect_centre(r):
    return (r[0] + r[2] / 2.0, r[1] + r[3] / 2.0)


def pt_in_rect(p, r):
    return r[0] <= p[0] <= r[0] + r[2] and r[1] <= p[1] <= r[1] + r[3]


def rects_intersect(a, b):
    return a[0] < b[0] + b[2] and b[0] < a[0] + a[2] and a[1] < b[1] + b[3] and b[1] < a[1] + a[3]


def rect_contains(outer, inner):
    return (outer[0] <= inner[0] and outer[1] <= inner[1] and outer[0] + outer[2] >= inner[0] + inner[2]
            and outer[1] + outer[3] >= inner[1] + inner[3])


def mean(vals):
    vals = [v for v in vals if v is not None]
    return sum(vals) / len(vals) if vals else None


def fmt(x, nd=1):
    return '-' if x is None else ('%.' + str(nd) + 'f') % x


def cell(s):
    return str(s).replace('|', '/').replace('\n', ' ')


def font(size):
    try:
        return ImageFont.load_default(size=size)
    except Exception:  # an old PIL without FreeType: the bitmap font
        return ImageFont.load_default()


def guess_kind(name):
    n = name.lower()
    if n.startswith('hit'):
        return 'hit'
    if n.startswith(('kill', 'headshot', 'punchkill', 'horde')):
        return 'kill'
    if n.startswith('ui-'):
        return 'ui'
    return 'camera'


# NIGHT20 (NIGHT20.md section 5, the reviewer brief): one question per judged scenario
QUESTIONS_N20 = {
    'speed': 'Which shows speed better while keeping the middle readable?',
    'weapon': 'Which weapon moment reads better?',
    'hud': 'Which HUD looks more finished, less cheap?',
    'menu': 'Which menu looks more finished?',
    'enemy': 'Which enemy reads better: what it is, what it\'s about to do?',
}
N20_GROUP = {
    'sprint': 'speed', 'boost': 'speed', 'boostside': 'speed', 'slide': 'speed',
    'shockwave': 'weapon', 'zoom': 'weapon', 'kill12': 'weapon', 'swapall': 'weapon',
    'hud-fight': 'hud', 'vitals': 'hud', 'rankup': 'hud',
    'ui-title': 'menu', 'ui-weapons': 'menu', 'ui-results': 'menu',
    'climb': 'enemy', 'special-round': 'enemy', 'charger': 'enemy', 'drone': 'enemy',
}


def question_for(name, kind):
    n = name.lower()
    if n in N20_GROUP:
        return QUESTIONS_N20[N20_GROUP[n]]
    if kind in QUESTIONS and kind in ('kill', 'hit', 'ui'):
        return QUESTIONS[kind]
    for key in ('boostside', 'boost', 'sprint', 'slide', 'strafe', 'land'):
        if n.startswith(key):
            return QUESTIONS[key]
    return QUESTIONS['hud']


def cam(fr, k):
    c = fr.get('cam')
    return num(c.get(k)) if isinstance(c, dict) else None


def vel(fr, k):
    v = fr.get('vel')
    return num(v.get(k)) if isinstance(v, dict) else None


def fx_group(fr, g):
    """The fx group drawn this frame (a > 0.01), else None."""
    fx = fr.get('fx')
    if not isinstance(fx, dict):
        return None
    v = fx.get(g)
    if not isinstance(v, dict):
        return None
    if num(v.get('a'), 1.0) <= 0.01:
        return None
    return v


def events(fr):
    ev = fr.get('events')
    return ev if isinstance(ev, list) else []


def ui_list(fr):
    u = fr.get('ui')
    return [e for e in u if isinstance(e, dict)] if isinstance(u, list) else []


def chunk_pts(fr):
    out = []
    pts = fr.get('chunkPts')
    if isinstance(pts, list):
        for p in pts:
            if isinstance(p, (list, tuple)) and len(p) >= 2:
                x, y = num(p[0]), num(p[1])
                if x is not None and y is not None:
                    out.append((x, y))
    return out


def has_live_effect(fr):
    fx = fr.get('fx')
    if isinstance(fx, dict):
        for g in fx:
            if fx_group(fr, g) is not None:
                return True
    return bool(chunk_pts(fr)) or (num(fr.get('chunkTotal'), 0) or 0) > 0


# ---------------------------------------------------------------------------------------------------------------
# The reel

class Scenario:
    def __init__(self, name, folder):
        self.name = name
        self.dir = folder
        self.header = {}
        self.by_f = {}
        self.notes = []
        self.bad_lines = 0
        path = os.path.join(folder, 'telemetry.jsonl')
        try:
            with open(path, 'r', encoding='utf-8-sig') as fh:
                for ln in fh:
                    ln = ln.strip()
                    if not ln:
                        continue
                    try:
                        o = json.loads(ln)
                    except ValueError:
                        self.bad_lines += 1
                        continue
                    if not isinstance(o, dict):
                        self.bad_lines += 1
                        continue
                    t = o.get('type')
                    if t == 'scenario':
                        if not self.header:
                            self.header = o
                        continue
                    if t not in (None, 'frame'):
                        continue
                    f = o.get('f')
                    if isinstance(f, bool) or num(f) is None:
                        self.bad_lines += 1
                        continue
                    o['f'] = int(num(f))
                    self.by_f[o['f']] = o
        except OSError as e:
            self.notes.append('telemetry unreadable: %s' % e)
        if self.bad_lines:
            self.notes.append('%d unreadable telemetry lines skipped' % self.bad_lines)
        if not self.header:
            self.notes.append('no scenario header: kind guessed from the folder name')
        self.fs = sorted(self.by_f)
        self.frames = [self.by_f[f] for f in self.fs]
        h = self.header
        self.kind = h.get('kind') if isinstance(h.get('kind'), str) else guess_kind(name)
        self.fps = num(h.get('fps')) or 60.0
        self.sim_hz = num(h.get('simHz')) or 120.0
        self.w = int(num(h.get('w')) or 1920)
        self.h = int(num(h.get('h')) or 1080)
        tg = h.get('target')
        self.target = None if (tg is None or isinstance(tg, bool)) else tg
        inp = h.get('inputFrame')
        self.input_frame = int(num(inp)) if num(inp) is not None else self.event_frame('input')
        self.max_speed = num(h.get('maxSpeed'))
        self.feel_ab = h.get('feelAB')
        self.k = 1080.0 / self.h if self.h > 0 else 1.0  # header px -> 1080p px (pixel targets are at 1080p)

    # frames
    def frame(self, f):
        return self.by_f.get(f)

    def prev_f(self, f):
        i = bisect.bisect_left(self.fs, f)
        return self.fs[i - 1] if i > 0 else None

    def next_f(self, f):
        i = bisect.bisect_right(self.fs, f)
        return self.fs[i] if i < len(self.fs) else None

    def first_f(self):
        return self.fs[0] if self.fs else None

    def last_f(self):
        return self.fs[-1] if self.fs else None

    def ms(self, frames):
        return frames * 1000.0 / self.fps

    def frames_for(self, ms):
        return int(round(ms * self.fps / 1000.0))

    def event_frame(self, ev, start=None):
        for f in self.fs:
            if start is not None and f < start:
                continue
            if ev in events(self.by_f[f]):
                return f
        return None

    def shot_frame(self):
        """(frame, note): the shot event, else the input frame."""
        s = self.event_frame('shot')
        if s is not None:
            return s, ''
        if self.input_frame is not None:
            return self.input_frame, 'no shot event: the input frame used'
        return None, 'no shot event and no input frame'

    def target_rect(self, f):
        fr = self.by_f.get(f)
        if fr is None or self.target is None:
            return None
        ens = fr.get('enemies')
        if not isinstance(ens, list):
            return None
        for e in ens:
            if isinstance(e, dict) and e.get('id') == self.target:
                return rect_of(e.get('rect'))
        return None

    def target_anchor(self, f):
        """The target's anchor [x, y] (sim position + 0.9 m projected; moves only with the camera), or None."""
        fr = self.by_f.get(f)
        if fr is None or self.target is None:
            return None
        for e in fr.get('enemies') or []:
            if isinstance(e, dict) and e.get('id') == self.target:
                a = e.get('anchor')
                if isinstance(a, (list, tuple)) and len(a) >= 2 and num(a[0]) is not None and num(a[1]) is not None:
                    return (num(a[0]), num(a[1]))
                return None
        return None

    def target_flash(self, f):
        fr = self.by_f.get(f)
        if fr is None or self.target is None:
            return None
        for e in fr.get('enemies') or []:
            if isinstance(e, dict) and e.get('id') == self.target:
                return num(e.get('flash'), 0.0)
        return None

    def target_rect_at_or_before(self, f):
        """(frame, rect) of the target on f or the nearest earlier frame that draws it."""
        i = bisect.bisect_right(self.fs, f)
        for g in reversed(self.fs[:i]):
            r = self.target_rect(g)
            if r is not None:
                return g, r
        return None, None

    def enemies_union(self, f):
        fr = self.by_f.get(f)
        u = None
        if fr is not None:
            for e in fr.get('enemies') or []:
                if isinstance(e, dict):
                    u = rect_union(u, rect_of(e.get('rect')))
        return u

    @staticmethod
    def _frozen(fr):
        # a freeze can start mid-frame: that frame runs some steps (held false) and swallows the rest (heldSteps > 0)
        return fr.get('held') is True or (num(fr.get('heldSteps'), 0) or 0) > 0

    def held_run(self, start, within=HELD_SEARCH):
        """The first freeze starting in [start, start + within]: contiguous frames that are held or swallow steps.
        Returns (first, last, heldSteps sum, first held frame, last held frame) or None. The held frames fall back
        to first / last when no frame of the run is fully held."""
        if start is None:
            return None
        i = bisect.bisect_left(self.fs, start)
        first_i = None
        while i < len(self.fs) and self.fs[i] <= start + within:
            if self._frozen(self.by_f[self.fs[i]]):
                first_i = i
                break
            i += 1
        if first_i is None:
            return None
        total, last, hf, hl, i = 0, self.fs[first_i], None, None, first_i
        while i < len(self.fs):
            f = self.fs[i]
            fr = self.by_f[f]
            if not self._frozen(fr) or (i > first_i and f != last + 1):
                break
            total += int(num(fr.get('heldSteps'), 0) or 0)
            if fr.get('held') is True:
                hf = f if hf is None else hf
                hl = f
            last = f
            i += 1
        first = self.fs[first_i]
        return first, last, total, (first if hf is None else hf), (last if hl is None else hl)

    def pre_mean(self, getter):
        if self.input_frame is None:
            return None
        return mean([getter(fr) for fr in self.frames if fr['f'] < self.input_frame])

    # images
    def img_path(self, f):
        return os.path.join(self.dir, 'f%04d.jpg' % f)

    def load_rgb(self, f, draft=None):
        """The frame as RGB, or None. draft=(w, h): let the JPEG decoder scale down (fast; at least that size)."""
        try:
            with Image.open(self.img_path(f)) as im:
                if draft:
                    im.draft('RGB', draft)
                im.load()
                return im.convert('RGB')
        except Exception:
            return None

    def load_luma(self, f):
        p = self.img_path(f)
        if not os.path.isfile(p):
            return None, '%s missing' % os.path.basename(p)
        try:
            with Image.open(p) as im:
                im.load()
                return np.asarray(im.convert('L'), dtype=np.int16), None
        except Exception as e:
            return None, '%s unreadable (%s)' % (os.path.basename(p), e)


class Reel:
    def __init__(self, path):
        self.path = os.path.abspath(path) if path else ''
        self.meta = {}
        self.scenarios = {}
        self.order = []
        self.errors = []
        self.notes = []
        if not path or not os.path.isdir(self.path):
            self.errors.append('not a folder: %s' % path)
            return
        names = self._scan(self.path)
        if not names and os.path.isdir(os.path.join(self.path, 'on')):
            self.notes.append('%s holds no scenario: read its on/ folder' % self.path)
            self.path = os.path.join(self.path, 'on')
            names = self._scan(self.path)
        rj = os.path.join(self.path, 'reel.json')
        if os.path.isfile(rj):
            try:
                with open(rj, 'r', encoding='utf-8-sig') as fh:
                    m = json.load(fh)
                if isinstance(m, dict):
                    self.meta = m
            except Exception as e:
                self.notes.append('reel.json unreadable: %s' % e)
        else:
            self.notes.append('no reel.json')
        listed = self.meta.get('scenarios') if isinstance(self.meta.get('scenarios'), list) else []
        self.order = [n for n in listed if n in names] + [n for n in names if n not in listed]
        for n in listed:
            if isinstance(n, str) and n not in names:
                self.notes.append('reel.json lists %s but its folder has no telemetry.jsonl' % n)
        for n in self.order:
            self.scenarios[n] = Scenario(n, os.path.join(self.path, n))
        if not self.order:
            self.errors.append('no scenario folder (with telemetry.jsonl) in %s' % self.path)

    @staticmethod
    def _scan(path):
        try:
            return sorted(d for d in os.listdir(path)
                          if os.path.isfile(os.path.join(path, d, 'telemetry.jsonl')))
        except OSError:
            return []

    def feel_ab(self):
        v = self.meta.get('feelAB')
        if isinstance(v, bool):
            return v
        for sc in self.scenarios.values():
            if isinstance(sc.feel_ab, bool):
                return sc.feel_ab
        return None

    def describe(self):
        m = self.meta
        bits = [self.path]
        if m:
            bits.append('build %s, feelAB %s, map %s, %sx%s, %s fps' % (
                m.get('build', '?'), m.get('feelAB', '?'), m.get('map', '?'), m.get('w', '?'), m.get('h', '?'),
                m.get('fps', '?')))
        bits.append('%d scenarios' % len(self.order))
        return '; '.join(bits)


# ---------------------------------------------------------------------------------------------------------------
# Metric results

class R:
    """One metric's result on one scenario."""
    __slots__ = ('status', 'value', 'reason', 'detail', 'num')

    def __init__(self, status, value='', reason='', detail=None, num_=None):
        self.status, self.value, self.reason, self.detail, self.num = status, value, reason, detail or [], num_


def NA(reason, value='-'):
    return R('n/a', value, reason)


def PF(ok, value, reason='', detail=None, num_=None):
    return R('PASS' if ok else 'FAIL', value, reason, detail, num_)


# --- M1 ---------------------------------------------------------------------------------------------------------

def m1_count(sc):
    """(count, info, None) or (None, None, reason): luma-changed px in the target window on the shot's frame 1
    (the frame whose line carries `shot`) vs the frame before it, with both frames' ui[] rects masked. The window
    is 400 x 400 at 1080p (scaled by the image's h / 1080); the count is in 1080p px (scaled by (1080 / h)^2)."""
    s, note = sc.shot_frame()
    if s is None:
        return None, None, note
    pre, post = sc.prev_f(s), s
    if pre is None:
        return None, None, 'no frame line before the shot (f%d)' % s
    g, r = sc.target_rect_at_or_before(pre)
    if r is None:
        return None, None, 'the target (id %s) is not drawn before the shot' % sc.target
    a, err = sc.load_luma(pre)
    if a is None:
        return None, None, err
    b, err = sc.load_luma(post)
    if b is None:
        return None, None, err
    if a.shape != b.shape:
        return None, None, 'f%d and f%d differ in size' % (pre, post)
    ih, iw = a.shape
    sx, sy = iw / float(sc.w), ih / float(sc.h)
    cx, cy = rect_centre(r)
    cx, cy = cx * sx, cy * sy
    half = M1_WINDOW * (ih / 1080.0) / 2.0
    x0, y0 = max(0, int(round(cx - half))), max(0, int(round(cy - half)))
    x1, y1 = min(iw, int(round(cx + half))), min(ih, int(round(cy + half)))
    if x1 <= x0 or y1 <= y0:
        return None, None, 'the target window is off screen'
    changed = np.abs(b[y0:y1, x0:x1] - a[y0:y1, x0:x1]) > LUMA_DELTA
    keep = np.ones(changed.shape, dtype=bool)
    for fr in (sc.frame(pre), sc.frame(post)):
        for e in ui_list(fr):
            ur = rect_of(e.get('rect'))
            if ur is None:
                continue
            ux0 = int(math.floor(ur[0] * sx)) - x0
            uy0 = int(math.floor(ur[1] * sy)) - y0
            ux1 = int(math.ceil((ur[0] + ur[2]) * sx)) - x0
            uy1 = int(math.ceil((ur[1] + ur[3]) * sy)) - y0
            ux0, uy0 = max(0, ux0), max(0, uy0)
            ux1, uy1 = min(keep.shape[1], ux1), min(keep.shape[0], uy1)
            if ux1 > ux0 and uy1 > uy0:
                keep[uy0:uy1, ux0:ux1] = False
    raw = int(np.count_nonzero(changed & keep))
    count = int(round(raw * (1080.0 / ih) ** 2))
    masked = int(keep.size - np.count_nonzero(keep))
    info = 'f%d vs f%d, window %dx%d image px at (%d,%d), %d image px changed, %d masked (image h %d)' % (
        post, pre, x1 - x0, y1 - y0, x0, y0, raw, masked, ih)
    if note:
        info += '; ' + note
    return count, info, None


# --- M2 ---------------------------------------------------------------------------------------------------------

def m2(sc, ctx):
    out = {}
    k = sc.event_frame('kill', sc.input_frame)
    if k is None:
        k = sc.event_frame('kill')
    if k is None:
        return {'M2 fx': NA('no kill event'), 'M2 marker': NA('no kill event')}
    groups = ('burst', 'chunks', 'ring')
    ever = any(fx_group(fr, g) is not None for fr in sc.frames for g in groups)
    u, seen = None, []
    i = bisect.bisect_left(sc.fs, k)
    for f in sc.fs[i:]:
        if f > k + M2_FRAMES - 1:
            break
        fr = sc.by_f[f]
        for g in groups:
            v = fx_group(fr, g)
            r = rect_of(v.get('rect')) if v else None
            if r is not None:
                u = rect_union(u, r)
                if g not in seen:
                    seen.append(g)
    if not ever:
        out['M2 fx'] = NA('fx.burst / chunks / ring never reported in this scenario')
    elif u is None:
        out['M2 fx'] = PF(False, '0 % of h', 'no burst / chunks / ring on frames 1-4 (f%d-f%d)' % (k, k + 3), num_=0.0)
    else:
        share = 100.0 * u[3] / sc.h
        out['M2 fx'] = PF(share >= 8.0 - 1e-9, '%.1f %% of h (w %.1f %%)' % (share, 100.0 * u[2] / sc.w),
                          detail=['union f%d-f%d of %s: rect [%d, %d, %d, %d]' % (
                              k, k + 3, '/'.join(seen), u[0], u[1], u[2], u[3])], num_=share)
    # the kill marker
    first = None
    for f in sc.fs[i:]:
        if fx_group(sc.by_f[f], 'killMarker') is not None:
            first = f
            break
    if first is None:
        out['M2 marker'] = NA('no fx.killMarker after the kill (KILL MARKER off, or not reported)')
        return out
    v = fx_group(sc.by_f[first], 'killMarker')
    r = rect_of(v.get('rect'))
    size = max(r[2], r[3]) * sc.k if r else 0.0  # 1080p px
    n, last, j = 0, first, sc.fs.index(first)
    while j < len(sc.fs) and sc.fs[j] == last + (1 if n else 0) and fx_group(sc.by_f[sc.fs[j]], 'killMarker'):
        last = sc.fs[j]
        n += 1
        j += 1
    dur = sc.ms(n)
    rel = first - k + 1
    at_end = last == sc.last_f()
    ok = rel in (1, 2) and size >= 48 and 300 - 1e-6 <= dur <= 400 + 1e-6
    reasons = []
    if rel not in (1, 2):
        reasons.append('frame %d' % rel)
    if size < 48:
        reasons.append('%d px' % size)
    if not (300 - 1e-6 <= dur <= 400 + 1e-6):
        reasons.append('%d ms' % round(dur))
    out['M2 marker'] = PF(ok, 'frame %d, %d px, %d ms%s' % (rel, size, round(dur), ' (still drawn at the end)' if at_end else ''),
                          ', '.join(reasons))
    return out


# --- M3 ---------------------------------------------------------------------------------------------------------

def m3(sc, ctx):
    k = sc.event_frame('kill', sc.input_frame)
    if k is None:
        k = sc.event_frame('kill')
    if k is None:
        return {'M3 centroid': NA('no kill event'), 'M3 outside': NA('no kill event')}
    if not any(chunk_pts(fr) for fr in sc.frames):
        r = NA('chunkPts never reported in this scenario')
        return {'M3 centroid': r, 'M3 outside': r}
    s, _ = sc.shot_frame()
    run = sc.held_run(s if s is not None and s <= k else k)
    note = ''
    if run is None:
        last_held = k
        note = 'no freeze: from the kill frame'
    else:
        last_held = run[4]
    t = last_held + sc.frames_for(M3_AFTER_MS)
    pre = sc.prev_f(k)
    body = None
    if pre is not None:
        if sc.target is not None:
            _, body = sc.target_rect_at_or_before(pre)
        else:
            body = sc.enemies_union(pre)
            note = (note + '; ' if note else '') + 'no target id: the union of the enemies before the kill'
    if body is None or body[3] <= 0:
        r = NA('no body rect on the frame before the kill')
        return {'M3 centroid': r, 'M3 outside': r}
    fr = sc.frame(t)
    if fr is None:
        r = NA('no frame line f%d (%d ms after the freeze; the reel ends at f%s)' % (t, M3_AFTER_MS, sc.last_f()))
        return {'M3 centroid': r, 'M3 outside': r}
    pts = chunk_pts(fr)
    if not pts:
        why = 'no chunk on screen at f%d (chunkTotal %s)' % (t, fr.get('chunkTotal'))
        return {'M3 centroid': PF(False, '-', why), 'M3 outside': PF(False, '-', why)}
    cx = sum(p[0] for p in pts) / len(pts)
    cy = sum(p[1] for p in pts) / len(pts)
    bx, by = rect_centre(body)
    d = math.hypot(cx - bx, cy - by)
    dh = d / body[3]
    outside = sum(1 for p in pts if not pt_in_rect(p, body)) / float(len(pts))
    det = ['f%d (last held f%d + %d ms): centroid (%d, %d), body [%d, %d, %d, %d] centre (%d, %d), %d pts%s' % (
        t, last_held, M3_AFTER_MS, cx, cy, body[0], body[1], body[2], body[3], bx, by, len(pts),
        '; ' + note if note else '')]
    return {
        'M3 centroid': PF(dh >= 1.5 - 1e-9, '%.2f body h (%d px at 1080p)' % (dh, d * sc.k), detail=det, num_=dh),
        'M3 outside': PF(outside >= 0.6 - 1e-9, '%d %% of %d' % (round(outside * 100), len(pts)), num_=outside * 100),
    }


# --- M4 ---------------------------------------------------------------------------------------------------------

def m4(sc, ctx):
    lo, hi = M4_TARGETS.get(sc.name, (None, None))
    s, note = sc.shot_frame()
    if s is None:
        return {'M4 freeze': NA(note)}
    run = sc.held_run(s)
    total_all = sum(int(num(fr.get('heldSteps'), 0) or 0) for fr in sc.frames)
    if run is None:
        steps, where = 0, 'no frozen frame within %d frames of f%d' % (HELD_SEARCH, s)
    else:
        steps, where = run[2], 'frozen f%d-f%d (held f%d-f%d)' % (run[0], run[1], run[3], run[4])
    val = '%d steps (%d ms)' % (steps, round(steps * 1000.0 / sc.sim_hz))
    det = ['%s; scenario total %d held steps%s' % (where, total_all, '; ' + note if note else '')]
    if lo is None:
        return {'M4 freeze': R('info', val, detail=det, num_=steps)}
    return {'M4 freeze': PF(lo <= steps <= hi, val, detail=det, num_=steps)}


# --- M5 ---------------------------------------------------------------------------------------------------------

def m5(sc, ctx):
    h = sc.event_frame('hit', sc.input_frame)
    if h is None:
        h = sc.event_frame('hit')
    if h is None:
        r = NA('no hit event')
        return {'M5 move': r, 'M5 back': r, 'M5 flash': r}
    pre = sc.prev_f(h)
    g0, r0 = sc.target_rect_at_or_before(pre) if pre is not None else (None, None)
    if r0 is None:
        r = NA('the target (id %s) is not drawn before the hit' % sc.target)
        return {'M5 move': r, 'M5 back': r, 'M5 flash': r}
    # the body's own motion: (rect centre - anchor) and (rect top - anchor y) against the pre-frame, so the
    # camera's recoil kick (which moves the anchor too) doesn't count; in 1080p px
    win = sc.frames_for(200)
    frames = [f for f in sc.fs[bisect.bisect_left(sc.fs, h):] if f <= h + win and sc.target_rect(f) is not None]
    a0 = sc.target_anchor(g0)
    anchored = a0 is not None and all(sc.target_anchor(f) is not None for f in frames)

    def off(r, a):
        c = rect_centre(r)
        return (c[0] - a[0], c[1] - a[1], r[1] - a[1])

    o0 = off(r0, a0 if anchored else (0.0, 0.0))
    rows = []  # (f, ms, centre d, top d)
    for f in frames:
        o = off(sc.target_rect(f), sc.target_anchor(f) if anchored else (0.0, 0.0))
        rows.append((f, sc.ms(f - h), math.hypot(o[0] - o0[0], o[1] - o0[1]) * sc.k, abs(o[2] - o0[2]) * sc.k))
    out = {}
    early = [x for x in rows if x[1] <= 50 + 1e-6]
    peak = None
    if not early:
        out['M5 move'] = NA('the target is not drawn within 50 ms of the hit')
    else:
        mc = max(x[2] for x in early)
        mt = max(x[3] for x in early)
        peak = max(mc, mt)
        if anchored:
            out['M5 move'] = PF(peak >= 8 - 1e-9, 'centre %.1f px, top %.1f px' % (mc, mt), num_=peak)
        else:
            why = 'no enemies[].anchor: the raw rect motion includes the camera kick'
            out['M5 move'] = NA(why, value='raw centre %.1f px, top %.1f px' % (mc, mt))
            out['M5 back'] = NA(why)
    if 'M5 back' in out:
        pass
    elif peak is None or not rows:
        out['M5 back'] = NA('no move measured')
    elif max(max(x[2], x[3]) for x in rows) <= 2:
        out['M5 back'] = NA('never moved more than 2 px')
    else:
        pk = max(range(len(rows)), key=lambda i: max(rows[i][2], rows[i][3]))
        back = None
        for x in rows[pk + 1:]:
            if x[2] <= 2 and x[3] <= 2:
                back = x
                break
        span = sc.ms((sc.last_f() or h) - h)
        if back is not None:
            out['M5 back'] = PF(back[1] <= 200 + 1e-6, 'back at %d ms (f%d)' % (round(back[1]), back[0]), num_=back[1])
        elif span < 200 - 1e-6:
            out['M5 back'] = NA('the reel ends %d ms after the hit, before 200 ms' % round(span))
        else:
            lastx = rows[-1]
            out['M5 back'] = PF(False, 'not back by 200 ms (%.1f px off at f%d)' % (max(lastx[2], lastx[3]), lastx[0]))
    n = 0
    for f in sc.fs[bisect.bisect_left(sc.fs, h):]:
        if f > h + sc.frames_for(500):
            break
        fl = sc.target_flash(f)
        if fl is not None and fl > 0.5:
            n += 1
    out['M5 flash'] = PF(n >= 2, '%d frames > 0.5' % n, num_=n)
    return out


# --- M6 / M7 ----------------------------------------------------------------------------------------------------

def _series(sc, f0, f1, getter):
    out = []
    for f in sc.fs[bisect.bisect_left(sc.fs, f0):]:
        if f > f1:
            break
        v = getter(sc.by_f[f])
        if v is not None:
            out.append((f, v))
    return out


def m6(sc, ctx):
    name = sc.name
    i = sc.input_frame
    if i is None:
        return {lab: NA('no input frame') for lab in M6_LABELS.get(name, [])}
    if not any(fr['f'] < i for fr in sc.frames):
        return {lab: NA('no pre-roll before the input (f%d)' % i) for lab in M6_LABELS.get(name, [])}
    end = sc.last_f()
    bf = sc.pre_mean(lambda fr: cam(fr, 'fov'))
    bh = sc.pre_mean(lambda fr: cam(fr, 'height'))
    br = sc.pre_mean(lambda fr: cam(fr, 'roll'))
    dfov = lambda fr: (cam(fr, 'fov') - bf) if (bf is not None and cam(fr, 'fov') is not None) else None
    out = {}
    if name.startswith('boost'):
        s2 = _series(sc, i, i + 2, dfov)
        sall = _series(sc, i, end, dfov)
        if not s2:
            return {'M6 boost kick': NA('no cam.fov'), 'M6 boost back': NA('no cam.fov')}
        kick = max(v for _, v in s2)
        fpk, peak = max(sall, key=lambda x: x[1])
        out['M6 boost kick'] = PF(8 - 1e-9 <= kick and peak <= 12 + 1e-9,
                                  '+%.1f deg by frame 2 (peak +%.1f at f%d)' % (kick, peak, fpk), num_=kick)
        thr = max(0.5, 0.1 * peak)
        back = None
        for f, v in sall:
            if f > fpk and abs(v) <= thr:
                back = f
                break
        if peak <= 0.5:
            out['M6 boost back'] = NA('no kick to return from')
        elif back is None:
            out['M6 boost back'] = PF(False, 'not back by the reel end (+%.1f deg at f%d)' % (sall[-1][1], sall[-1][0]))
        else:
            d = sc.ms(back - fpk)
            out['M6 boost back'] = PF(300 - 1e-6 <= d <= 400 + 1e-6, '%d ms from the peak (within %.1f deg)' % (round(d), thr),
                                      num_=d)
    elif name.startswith('sprint'):
        # NIGHT18 round 1 (the lead): the sprint starts with the boost (STAMINA KEY: one key, the press boosts, the hold
        # sprints), so its first 300-400 ms are the boost's kick (M6 boost). The sprint's own widen is the held value:
        # the mean over 500-1000 ms after the press, +5-6 deg.
        s = _series(sc, i + sc.frames_for(500), i + sc.frames_for(1000), dfov)
        if not s:
            return {'M6 sprint fov': NA('no cam.fov 500-1000 ms after the press')}
        v = sum(x for _, x in s) / len(s)
        out['M6 sprint fov'] = PF(5 - 0.05 <= v <= 6 + 0.05,
                                  '+%.1f deg held (mean 500-1000 ms; min +%.1f, max +%.1f)' % (
                                      v, min(x for _, x in s), max(x for _, x in s)), num_=v)
    elif name.startswith('slide'):
        w = sc.frames_for(100)
        s = _series(sc, i, i + w, dfov)
        hs = _series(sc, i, i + w, lambda fr: cam(fr, 'height'))
        rs = _series(sc, i, i + sc.frames_for(500), lambda fr: cam(fr, 'roll'))
        out['M6 slide fov'] = (PF(6 - 0.05 <= max(x for _, x in s) <= 8 + 0.05, '+%.1f deg' % max(x for _, x in s),
                                  num_=max(x for _, x in s)) if s else NA('no cam.fov'))
        if hs and bh is not None:
            drop = bh - min(x for _, x in hs)
            out['M6 slide drop'] = PF(drop >= 0.4 - 1e-9, '%.2f m' % drop, num_=drop)
        else:
            out['M6 slide drop'] = NA('no cam.height')
        if rs and br is not None:
            rl = max(abs(x - br) for _, x in rs)
            out['M6 slide roll'] = PF(3 - 0.05 <= rl <= 5 + 0.05, '%.1f deg' % rl, num_=rl)
        else:
            out['M6 slide roll'] = NA('no cam.roll')
    return out


M6_LABELS = {
    'boost': ['M6 boost kick', 'M6 boost back'],
    'sprint': ['M6 sprint fov'],
    'slide': ['M6 slide fov', 'M6 slide drop', 'M6 slide roll'],
}


def m7(sc, ctx):
    i = sc.input_frame
    out = {}
    if sc.name.startswith('strafe'):
        if i is None:
            return {'M7 strafe roll': NA('no input frame')}
        br = sc.pre_mean(lambda fr: cam(fr, 'roll'))
        if br is None:
            br = 0.0
        pts = [(fr['f'], vel(fr, 'right'), cam(fr, 'roll')) for fr in sc.frames if fr['f'] >= i]
        pts = [p for p in pts if p[1] is not None and p[2] is not None]
        if not pts:
            return {'M7 strafe roll': NA('no vel.right / cam.roll')}
        vmax = max(abs(p[1]) for p in pts)
        if vmax < 0.5:
            return {'M7 strafe roll': NA('no lateral speed (max |vel.right| %.2f m/s)' % vmax)}
        full = [p for p in pts if abs(p[1]) >= 0.9 * vmax]
        rl = mean([abs(p[2] - br) for p in full])
        rmax = max(abs(p[2] - br) for p in full)
        out['M7 strafe roll'] = PF(1.5 - 0.05 <= rl <= 3 + 0.05,
                                   '%.1f deg (max %.1f) at |right| >= %.1f m/s, %d frames' % (rl, rmax, 0.9 * vmax, len(full)),
                                   num_=rl)
        return out
    # land
    ld = sc.event_frame('land', i)
    if ld is None:
        ld = sc.event_frame('land')
    if ld is None:
        r = NA('no land event')
        return {'M7 land dip': r, 'M7 land back': r}
    bh = sc.pre_mean(lambda fr: cam(fr, 'height'))
    if bh is None:
        pre = [cam(sc.by_f[f], 'height') for f in sc.fs if f < ld][-6:]
        bh = mean(pre)
    hs = _series(sc, ld, ld + sc.frames_for(500), lambda fr: cam(fr, 'height'))
    if bh is None or not hs:
        r = NA('no cam.height')
        return {'M7 land dip': r, 'M7 land back': r}
    fmin, hmin = min(hs, key=lambda x: x[1])
    dip = bh - hmin
    pf = sc.prev_f(ld)
    fall = -vel(sc.by_f[pf], 'up') if pf is not None and vel(sc.by_f[pf], 'up') is not None else None
    out['M7 land dip'] = PF(0.05 - 1e-9 <= dip <= 0.15 + 1e-9, '%.3f m (fall %s m/s)' % (dip, fmt(fall)), num_=dip)
    if dip <= 0.005:
        out['M7 land back'] = NA('no dip')
        return out
    thr = max(0.005, 0.1 * dip)
    back = None
    for f, v in hs:
        if f > fmin and bh - v <= thr:
            back = f
            break
    if back is None:
        out['M7 land back'] = PF(False, 'not back within 500 ms')
    else:
        d = sc.ms(back - ld)
        out['M7 land back'] = PF(150 - 1e-6 <= d <= 200 + 1e-6, '%d ms from the landing' % round(d), num_=d)
    return out


# --- NIGHT20: M8 v2 (speed you can see: L1-L5 from the speed lines' own report, telemetry "streaks") --------------
# The lines report each frame (FeelProbe.Streaks): count, lenPx (median screen length), angle (median screen angle, deg:
# 0 right, 90 up), minRho (the nearest streak pixel in ellipse units dx / (w/2), dy / (h/2)), innerAlpha (the strongest
# alpha inside rho 0.55), cross (streaks over a live enemy's rect). NIGHT18's M8 ("nothing in the central 60 % box") is
# replaced by L1 (logged in NIGHT20-LOG.md). L6 (the bench's p99) is the bench's, not the reel's.
L_LABELS = ('L1 clear', 'L2 count', 'L3 length', 'L4 direction', 'L5 enemies')


def streaks_of(fr):
    s = fr.get('streaks')
    return s if isinstance(s, dict) else None


def _med(vals):
    v = sorted(x for x in vals if x is not None)
    if not v:
        return None
    n = len(v)
    return v[n // 2] if n % 2 else 0.5 * (v[n // 2 - 1] + v[n // 2])


def m8v2(sc, ctx):
    rep = [(fr, streaks_of(fr)) for fr in sc.frames]
    rep = [(fr, s) for fr, s in rep if s is not None]
    if not rep:
        r = NA('no streaks channel (FeelProbe.Streaks never reported: the lines are not instrumented on this build)')
        return {k: r for k in L_LABELS}
    out = {}
    i = sc.input_frame
    # L1: no streak pixel inside rho 0.33; alpha < 0.30 out to 0.55
    bad = []
    for fr, s in rep:
        if (num(s.get('count'), 0) or 0) <= 0:
            continue
        mr, ia = num(s.get('minRho'), -1), num(s.get('innerAlpha'), -1)
        if mr is not None and 0 <= mr < 0.33:
            bad.append('f%d minRho %.2f' % (fr['f'], mr))
        elif ia is not None and ia >= 0.30:
            bad.append('f%d innerAlpha %.2f' % (fr['f'], ia))
    out['L1 clear'] = PF(not bad, '%d frames with a streak inside rho 0.33 (or alpha >= 0.30 inside 0.55)' % len(bad),
                         detail=bad[:10], num_=len(bad))
    # L2: the count by speed: 0 below 9 m/s; sprint 11 +/- 4 (held: from 500 ms after the input); a boost >= 35 within 50 ms
    # NIGHT20 round 2 (a rule fix, logged): "0 at a walk" counts frames that have been under 9 m/s for 0.45 s or more (a
    # streak's life 0.38 s + 25 %): the streaks a boost leaves die out on their own as it slows (the research's "back to
    # the sprint's count within 0.3 s of its end"), they are not a walk's
    slow_since, walk = None, []
    for fr, s in rep:
        sp = vel(fr, 'speed') or 0
        if sp < 9:
            slow_since = fr['f'] if slow_since is None else slow_since
        else:
            slow_since = None
        if slow_since is not None and fr['f'] - slow_since >= 27 and (num(s.get('count'), 0) or 0) > 0:
            walk.append((fr, s))
    l2 = []
    ok = not walk
    if walk:
        l2.append('%d frames below 9 m/s with streaks (f%d: %d)' % (len(walk), walk[0][0]['f'], num(walk[0][1].get('count'), 0)))
    if sc.name in ('sprint',) and i is not None:
        held = [num(s.get('count'), 0) for fr, s in rep if fr['f'] >= i + 30]
        mc = _med(held)
        ok = ok and mc is not None and 7 <= mc <= 15
        l2.append('sprint median %s (target 11 +/- 4)' % ('-' if mc is None else '%.0f' % mc))
    if sc.name in ('boost', 'boostside') and i is not None:
        early = [num(s.get('count'), 0) for fr, s in rep if i <= fr['f'] <= i + 3]
        mx = max(early) if early else None
        ok = ok and mx is not None and mx >= 35
        l2.append('boost max %s within 50 ms (target >= 35)' % ('-' if mx is None else '%d' % mx))
    out['L2 count'] = PF(ok, '; '.join(l2) if l2 else 'no frame under 9 m/s with streaks', num_=len(walk))
    # L3: the median screen length: 25-45 px sprinting (held), 80-140 px boosting (the first 300 ms)
    if sc.name in ('sprint', 'boost', 'boostside') and i is not None:
        if sc.name == 'sprint':
            lens = [num(s.get('lenPx'), None) for fr, s in rep if fr['f'] >= i + 30 and (num(s.get('count'), 0) or 0) > 0]
            lo, hi = 25, 45
        else:
            lens = [num(s.get('lenPx'), None) for fr, s in rep if i <= fr['f'] <= i + 18 and (num(s.get('count'), 0) or 0) > 0]
            lo, hi = 80, 140
        ml = _med(lens)
        out['L3 length'] = PF(ml is not None and lo <= ml <= hi, 'median %s px (target %d-%d)' % ('-' if ml is None else '%.0f' % ml, lo, hi),
                              num_=ml) if ml is not None else NA('no streak lengths in the window')
    else:
        out['L3 length'] = NA('L3 reads sprint and boost')
    # L4: a pure strafe's streaks within 20 deg of horizontal; a fall's (land) within 20 deg of vertical
    if sc.name in ('strafe', 'boostside', 'land', 'slam'):  # NIGHT20 round 2: boostside / slam added (a walk strafe and a 3 m drop stay under the 9 m/s gate)
        scope = rep
        if sc.name == 'slam':  # NIGHT20 round 3 (a rule fix, logged): the slam's fall only (to the first 'land' event)
            land_f = next((fr['f'] for fr in sc.frames if 'land' in events(fr)), None)
            if land_f is not None:
                scope = [(fr, s) for fr, s in rep if fr['f'] < land_f and (sc.input_frame is None or fr['f'] >= sc.input_frame)]
        angs = [num(s.get('angle'), None) for fr, s in scope if (num(s.get('count'), 0) or 0) > 0]
        ma = _med([a % 180 for a in angs if a is not None])
        if ma is None:
            out['L4 direction'] = NA('no streak angles (no streaks drawn)')
        else:
            off = min(ma, 180 - ma) if sc.name in ('strafe', 'boostside') else abs(ma - 90)
            out['L4 direction'] = PF(off <= 20, 'median %.0f deg (%.0f deg off %s)' % (ma, off, 'horizontal' if sc.name in ('strafe', 'boostside') else 'vertical'), num_=off)
    else:
        out['L4 direction'] = NA('L4 reads strafe, boostside, land and slam')
    # L5: no streak over a live enemy
    cross = [(fr, num(s.get('cross'), 0)) for fr, s in rep if (num(s.get('cross'), 0) or 0) > 0]
    out['L5 enemies'] = PF(not cross, '%d frames with a streak over a live enemy' % len(cross),
                           detail=['f%d: %d' % (fr['f'], c) for fr, c in cross[:10]], num_=len(cross))
    return out


# --- M8 ---------------------------------------------------------------------------------------------------------

def m8(sc, ctx):
    drawn = [(fr, fx_group(fr, 'speedLines')) for fr in sc.frames]
    drawn = [(fr, v) for fr, v in drawn if v is not None]
    if not drawn:
        r = NA('fx.speedLines never reported (not drawn, or not instrumented)')
        return {'M8 alpha': r, 'M8 region': r}
    out = {}
    if not sc.max_speed:
        out['M8 alpha'] = NA('no maxSpeed in the header')
    else:
        bad, on, amax, amax_ratio, nmeas = [], 0, 0.0, None, 0
        for fr, v in drawn:
            a = num(v.get('a'), 1.0)
            sp = vel(fr, 'speed')
            if sp is None:
                continue
            nmeas += 1
            ratio = sp / sc.max_speed
            if a > 0.3:
                on += 1
                if ratio <= 0.7 + 1e-9:
                    bad.append('f%d a %.2f at %d %%' % (fr['f'], a, round(ratio * 100)))
            if a > amax:
                amax, amax_ratio = a, ratio
        if not nmeas:
            out['M8 alpha'] = NA('no vel.speed on the frames that draw the lines')
        else:
            out['M8 alpha'] = PF(not bad, '%d frames a > 0.3 at <= 70 %% (max a %.2f at %s %%; a > 0.3 on %d)' % (
                len(bad), amax, '-' if amax_ratio is None else round(amax_ratio * 100), on), detail=bad[:10],
                num_=len(bad))
    centre = (0.2 * sc.w, 0.2 * sc.h, 0.6 * sc.w, 0.6 * sc.h)
    # NIGHT18 round 1 (the lead): the lines report the clear box they leave in the middle (fx.speedLinesClear, builder
    # M): the region test is that box holding the central 60 % on every frame the lines draw
    boxes = [(fr, rect_of(fx_group(fr, 'speedLinesClear').get('rect'))) for fr, v in drawn
             if fx_group(fr, 'speedLinesClear') is not None and num(v.get('a'), 1.0) > 0.01]
    boxes = [(fr, b) for fr, b in boxes if b is not None]
    if boxes:
        bad = ['f%d clear box [%d, %d, %d, %d]' % (fr['f'], b[0], b[1], b[2], b[3]) for fr, b in boxes
               if not rect_contains(b, centre)]
        out['M8 region'] = PF(not bad, '%d of %d frames with lines in the central 60 %% (clear box)' % (len(bad), len(boxes)),
                              detail=bad[:10], num_=len(bad))
        return out
    inside, outer, clear = [], 0, 0
    for fr, v in drawn:
        r = rect_of(v.get('rect'))
        if r is None:
            continue
        if rect_contains(r, centre):
            outer += 1
        elif rects_intersect(r, centre):
            inside.append('f%d rect [%d, %d, %d, %d]' % (fr['f'], r[0], r[1], r[2], r[3]))
        else:
            clear += 1
    if inside:
        out['M8 region'] = PF(False, '%d frames inside' % len(inside), detail=inside[:10], num_=len(inside))
    elif clear == 0:
        out['M8 region'] = NA('the rect spans the whole centre on all %d frames: an outer bound (edge lines on every '
                              'side) can not be told from lines in the centre' % outer)
    else:
        out['M8 region'] = PF(True, '0 frames inside (%d clear%s)' % (clear, ', %d outer-bound n/a' % outer if outer else ''),
                              num_=0)
    return out


# --- M9 / M10 / M11 ---------------------------------------------------------------------------------------------

def draws_nothing(e):
    """A TMP text with no characters: listed by the scanner (its Graphic is active) but nothing is on screen."""
    return e.get('px') is not None and isinstance(e.get('text'), str) and not e.get('text').strip()


def _ui_runs(sc):
    """id -> list of runs; a run = consecutive frame lines where the element is listed under one path:
    [(idx, f, elem, rect)]. A reparented element (the nav focus frame moving to another row) starts a new run."""
    runs, last_idx, last_path = {}, {}, {}
    for idx, fr in enumerate(sc.frames):
        for e in ui_list(fr):
            if draws_nothing(e):
                continue
            eid = e.get('id')
            if eid is None:
                eid = e.get('path')
            r = rect_of(e.get('rect'))
            if r is None or eid is None:
                continue
            lst = runs.setdefault(eid, [])
            li = last_idx.get(eid)
            if li == idx:
                continue
            if lst and li == idx - 1 and last_path.get(eid) == e.get('path'):
                lst[-1].append((idx, fr['f'], e, r))
            else:
                lst.append([(idx, fr['f'], e, r)])
            last_idx[eid] = idx
            last_path[eid] = e.get('path')
    return runs


def _still(a, b, k):
    """Two consecutive frames of one element: no visible change (scale, alpha, position at 1080p)."""
    ea, eb = a[2], b[2]
    ca, cb = rect_centre(a[3]), rect_centre(b[3])
    return (abs(num(ea.get('scale'), 1.0) - num(eb.get('scale'), 1.0)) <= 0.002
            and abs(num(ea.get('alpha'), 1.0) - num(eb.get('alpha'), 1.0)) <= 0.005
            and math.hypot(ca[0] - cb[0], ca[1] - cb[1]) * k <= 0.5)


def _entrance_segment(run, k):
    """The run up to where the element, still for 10 frames, starts moving again (a focus move, a later effect): the
    entrance is judged on that first segment, its rest the segment's last 10 frames."""
    still = 0
    for i in range(1, len(run)):
        moved = not _still(run[i - 1], run[i], k)
        if moved and still >= 9:
            return run[:i]
        still = 0 if moved else still + 1
    return run


def m9(sc, ctx):
    if not sc.frames:
        r = NA('no frame lines')
        return {'M9 pop-ins': r, 'M9 entrance time': r}
    runs = _ui_runs(sc)
    n_last = len(sc.frames) - 1
    pops, repops, slow, timed, cut = [], [], [], [], 0
    entrances = 0
    for eid, rl in runs.items():
        for k, run in enumerate(rl):
            if run[0][0] == 0:
                continue
            first_app = (k == 0)
            if run[-1][0] == n_last and len(run) < 6:
                cut += 1
                continue
            run = _entrance_segment(run, sc.k)
            # NIGHT18 round 1 (the lead): rest is the element's brightest 10-frame window (ties: the latest), not its last
            # 10 frames: a transient (a points pop, a tag) fades out at its end, and its exit read as its rest
            n = min(10, len(run))
            best_i, best_a = len(run) - n, -1.0
            for i in range(0, len(run) - n + 1):
                ma = sum(num(x[2].get('alpha'), 1.0) for x in run[i:i + n]) / n
                if ma >= best_a - 1e-9:
                    best_i, best_a = i, ma
            tail = run[best_i:best_i + n]
            rs = statistics.median([num(x[2].get('scale'), 1.0) for x in tail])
            ra = statistics.median([num(x[2].get('alpha'), 1.0) for x in tail])
            rcx = statistics.median([rect_centre(x[3])[0] for x in tail])
            rcy = statistics.median([rect_centre(x[3])[1] for x in tail])
            e0, r0 = run[0][2], run[0][3]
            s0, a0 = num(e0.get('scale'), 1.0), num(e0.get('alpha'), 1.0)
            c0 = rect_centre(r0)
            dist = math.hypot(c0[0] - rcx, c0[1] - rcy) * sc.k  # 1080p px
            scale_ok = POP_SCALE * rs <= s0 <= POP_SCALE_MAX * rs if rs > 1e-6 else True
            alpha_ok = a0 >= POP_ALPHA * ra if ra > 1e-6 else True
            pop = scale_ok and alpha_ok and dist < POP_DIST
            path = e0.get('path') or ('id %s' % eid)
            desc = '%s f%d: scale %.2f (rest %.2f), alpha %.2f (rest %.2f), %d px from rest' % (
                path, run[0][1], s0, rs, a0, ra, dist)
            if first_app:
                entrances += 1
            if pop:
                (pops if first_app else repops).append(desc)
                continue
            if not first_app:
                continue
            # settled: within 3 % of rest scale, at >= 95 % of rest alpha (the pop-in test's own alpha rule, so a
            # non-pop can't settle on its first frame), within 3 px (1080p) of rest, and staying so for SETTLE_HOLD
            # frames (or to the run's end): an ease-out-back's way up crosses rest before its overshoot comes back
            oks = []
            for idx, f, e, r in run:
                s, a = num(e.get('scale'), 1.0), num(e.get('alpha'), 1.0)
                c = rect_centre(r)
                oks.append(abs(s - rs) <= 0.03 * max(rs, 1e-6) and a >= POP_ALPHA * ra - 1e-9
                           and math.hypot(c[0] - rcx, c[1] - rcy) * sc.k <= 3)
            settle = None
            for j in range(len(run)):
                if all(oks[j:j + SETTLE_HOLD]):
                    settle = run[j][1]
                    break
            d = None if settle is None else sc.ms(settle - run[0][1])
            timed.append(d)
            if d is None or not (ENTRANCE_MS[0] - 1e-6 <= d <= ENTRANCE_MS[1] + 1e-6):
                slow.append('%s f%d: %s' % (path, run[0][1], 'never settles' if d is None else '%d ms' % round(d)))
    out = {}
    extra = []
    if repops:
        extra.append('%d re-entry pop%s' % (len(repops), '' if len(repops) == 1 else 's'))
    if cut:
        extra.append('%d cut by the reel end' % cut)
    tail_txt = (' (+' + ', '.join(extra) + ')') if extra else ''
    det = pops + ['re-entry: ' + x for x in repops]
    if entrances == 0:
        if repops:
            out['M9 pop-ins'] = R('info', '0 first entrances%s' % tail_txt, 'no element first appears after frame 0',
                                  detail=det)
        else:
            out['M9 pop-ins'] = NA('no element first appears after frame 0' + (tail_txt and ' ' + tail_txt))
    else:
        out['M9 pop-ins'] = PF(not pops, '%d of %d entrances%s' % (len(pops), entrances, tail_txt), detail=det,
                               num_=len(pops))
    if not timed:
        out['M9 entrance time'] = NA('no animated entrance')
    else:
        ok = len(timed) - len(slow)
        ds = [d for d in timed if d is not None]
        rng = (' (%d-%d ms)' % (round(min(ds)), round(max(ds)))) if ds else ''
        out['M9 entrance time'] = PF(not slow, '%d of %d in 90-300 ms%s' % (ok, len(timed), rng), detail=slow,
                                     num_=len(slow))
    return out


def m10(sc, ctx):
    small, any_px = {}, False
    for fr in sc.frames:
        for e in ui_list(fr):
            px = num(e.get('px'))
            if px is None or draws_nothing(e):
                continue
            any_px = True
            if px < TEXT_FLOOR - 0.01:
                key = e.get('path') or e.get('id')
                if key not in small or px < small[key][0]:
                    small[key] = (px, fr['f'], e.get('text'))
    if not any_px:
        return {'M10 text floor': NA('no TMP text on screen')}
    det = ['%s: %.1f px at f%d, "%s"' % (k, v[0], v[1], v[2] if v[2] is not None else '') for k, v in sorted(
        small.items(), key=lambda kv: kv[1][0])]
    mn = min((v[0] for v in small.values()), default=None)
    return {'M10 text floor': PF(not small, '%d texts < 18 px%s' % (len(small), '' if mn is None else ' (min %.1f)' % mn),
                                 detail=det, num_=len(small))}


def load_roles(path):
    if not path:
        return None, 'no roles file given'
    if not os.path.isfile(path):
        return None, 'roles file missing: %s' % path
    try:
        with open(path, 'r', encoding='utf-8-sig') as fh:
            d = json.load(fh)
    except Exception as e:
        return None, 'roles file unreadable: %s' % e
    if not isinstance(d, dict):
        return None, 'roles file is not an object'
    out = {}
    for k, v in d.items():
        if k.startswith('_'):
            continue
        out[k] = [p for p in v if isinstance(p, str)] if isinstance(v, list) else []
    return out, None


def role_users(sc):
    """(role, path) -> (first f, colour) for every ui[] with a role."""
    users = {}
    for fr in sc.frames:
        for e in ui_list(fr):
            role = e.get('role')
            if not isinstance(role, str) or not role:
                continue
            key = (role, e.get('path') or ('id %s' % e.get('id')))
            if key not in users:
                users[key] = (fr['f'], e.get('color'))
    return users


def role_violations(sc, roles):
    viol = []
    users = role_users(sc)
    for (role, path), (f, col) in sorted(users.items()):
        allowed = roles.get(role, [])
        if not any(path.startswith(p) for p in allowed):
            viol.append((role, path, f, col))
    return viol, users


def m11(sc, ctx):
    roles, err = ctx.get('roles'), ctx.get('roles_err')
    if roles is None:
        return {'M11 roles': NA(err or 'no roles file')}
    viol, users = role_violations(sc, roles)
    det = ['%s %s (%s) from f%d' % (r, p, c, f) for r, p, f, c in viol]
    return {'M11 roles': PF(not viol, '%d unlisted of %d role users' % (len(viol), len(users)), detail=det,
                            num_=len(viol))}


# --- NIGHT19: S12 (the pad shows the pad) and S14 (the centre is clear) -------------------------------------------

# Keyboard and mouse tokens a pad player must never read (NIGHT19.md section 1's list, whole words), and the pad words a
# keyboard player must never read. The style meter's rank letters (D C B A S) are never in either list.
S12_KEYS = re.compile(r'(?<![A-Z0-9-])(E|F|Q|R|TAB|SHIFT|CTRL|SPACE|ESC|LMB|RMB|MOUSE)(?![A-Z0-9-])')
S12_PAD = re.compile(r'(?<![A-Z0-9-])(LT|RT|LB|RB|L1|R1|L2|R2|ZL|ZR|L3|R3|D-PAD|STICK)(?![A-Z0-9-])')
S12_SKIP = ('Style meter/Letter',)


def _reel_pad(sc):
    """The pad pass? (reel.json's "pad", written by FeelReel from NIGHT19 on; absent: the keyboard pass)."""
    try:
        with open(os.path.join(os.path.dirname(sc.dir.rstrip('/\\')), 'reel.json'), 'r', encoding='utf-8-sig') as fh:
            return bool(json.load(fh).get('pad'))
    except Exception:
        return False


def s12(sc, ctx):
    pad = _reel_pad(sc)
    rx = S12_KEYS if pad else S12_PAD
    found = {}
    for fr in sc.frames:
        for e in ui_list(fr):
            txt = e.get('text')
            if not isinstance(txt, str) or not txt.strip() or draws_nothing(e):
                continue
            path = e.get('path') or ''
            if any(s in path for s in S12_SKIP):
                continue
            for m in rx.finditer(txt.upper()):
                key = (path, m.group(1))
                if key not in found:
                    found[key] = (fr['f'], txt)
    det = ['%s: "%s" in "%s" (f%d)' % (k[0], k[1], v[1], v[0]) for k, v in sorted(found.items())]
    what = 'keyboard / mouse tokens on the pad pass' if pad else 'pad words on the keyboard pass'
    return {'S12 tokens': PF(not found, '%d %s' % (len(found), what), detail=det, num_=len(found))}


# The central box (40 % x 40 % of the screen) and what may stand in it: the crosshair and its accessories (the lock
# brackets, the momentum ring), the hit and kill markers, transient pops.
S14_BOX = (0.30, 0.30, 0.70, 0.70)
# NIGHT20 (the lead, a rule fix, not a target change; logged): 'Zoom reticle' and 'Scope' are the zoom's crosshair (the
# zoom scenario is read tonight); the speed lines' full-screen layer ('Speed lines') is judged by its own probe (M8 v2 L1),
# not as a HUD element (NIGHT19's final reel flagged the layer on sprint and vitals: its lines never drew in the middle).
S14_ALLOWED = ('Crosshair', 'Lock brackets', 'Momentum ring', 'Kill marker', 'Hit marker', 'Points pop', 'Pop', 'Callout',
               'Zoom reticle', 'Scope', 'Speed lines')
S14_PERSIST = 0.5  # an element listed in at least half the frames is persistent


def s14(sc, ctx):
    if sc.name not in ('hud-fight', 'sprint', 'boost', 'vitals', 'zoom'):
        return {'S14 centre': NA('S14 reads hud-fight, sprint, boost, vitals and zoom')}
    w = num(sc.header.get('w'), 1920)
    h = num(sc.header.get('h'), 1080)
    box = [S14_BOX[0] * w, S14_BOX[1] * h, (S14_BOX[2] - S14_BOX[0]) * w, (S14_BOX[3] - S14_BOX[1]) * h]
    seen, total = {}, 0
    for fr in sc.frames:
        total += 1
        for e in ui_list(fr):
            path = e.get('path') or ''
            if not path.startswith('UI/HUD') or draws_nothing(e):
                continue
            if any(a in path for a in S14_ALLOWED):
                continue
            a = num(e.get('a'), 1)
            if a is not None and a <= 0.01:
                continue
            r = rect_of(e.get('rect'))
            if r is None or not rects_intersect(r, box):
                continue
            seen[path] = seen.get(path, 0) + 1
    persistent = {k: v for k, v in seen.items() if total and v / total >= S14_PERSIST}
    det = ['%s: %d of %d frames' % (k, v, total) for k, v in sorted(persistent.items(), key=lambda kv: -kv[1])]
    return {'S14 centre': PF(not persistent, '%d persistent HUD elements in the central 40 %%' % len(persistent), detail=det,
                             num_=len(persistent))}


# NIGHT21 M-MAGENTA (the lead; NIGHT20's rank letter / grade badge drew a magenta square in the Player, the Editor hid it:
# a missing material shows (255, 0, 255)): the pixels within 40 (Euclidean, RGB) of (255, 0, 255) on every frame of every
# scenario, target 0. Magenta is also RIVAL's role colour, so a scenario that shows a RIVAL on purpose can list magenta
# pixels: read the frame (the detail names the worst one) before calling it a missing material.
MAGENTA_RGB = (255, 0, 255)
MAGENTA_DIST = 40


def magenta_pixels(rgb):
    """The number of pixels of an RGB image (PIL) within MAGENTA_DIST of MAGENTA_RGB."""
    a = np.asarray(rgb, dtype=np.int32)
    d = (a[..., 0] - MAGENTA_RGB[0]) ** 2 + (a[..., 1] - MAGENTA_RGB[1]) ** 2 + (a[..., 2] - MAGENTA_RGB[2]) ** 2
    return int((d < MAGENTA_DIST * MAGENTA_DIST).sum())


def m_magenta(sc, ctx):
    bad, worst, read = [], (0, None), 0
    for fr in sc.frames:
        im = sc.load_rgb(fr['f'])
        if im is None:
            continue
        read += 1
        n = magenta_pixels(im)
        if n > 0:
            bad.append((fr['f'], n))
            if n > worst[0]:
                worst = (n, fr['f'])
    if not read:
        return {'M-MAGENTA': NA('no frame images in the scenario')}
    det = ['f%d: %d px' % (f, n) for f, n in bad[:12]]
    if len(bad) > 12:
        det.append('... %d more frames' % (len(bad) - 12))
    return {'M-MAGENTA': PF(not bad, '%d px on %d of %d frames%s' % (sum(n for _, n in bad), len(bad), read,
                                                                        '' if not bad else ' (worst f%d: %d px)' % (worst[1], worst[0])),
                            detail=det, num_=sum(n for _, n in bad))}



# ---------------------------------------------------------------------------------------------------------------
# NIGHT22 meters (the lead; each has a synthetic self-test in `selftest`, as M-MAGENTA had)
#   T11     the RESULTS grade card ("Grade" group) must not intersect the POINTS group on any frame (T-11, open since w18)
#   LUM     the mean frame luminance (0-255): per scenario the median of the frame means; the `dusk` scenario is read in three equal
#           time thirds (rounds 1, 8, 15 in order) and the medians must not rise by more than LUM_SLACK (the dusk clock sinks)
#   STAMP-T the entrance time of any element whose path contains "stamp": first frame to settled (alpha >= 95 % of rest, scale
#           within 3 %), target 200-300 ms (NIGHT22.md U3-36.4: our tuning; M9's 90-300 ms is the floor)
LUM_SLACK = 1.0
LUM_FALL = 3.0  # the dusk scenario's first third must be at least this much brighter (0-255) than its last: a flat sky is RED
STAMP_MS = (200.0, 300.0)


def leaf_of(e):
    return (e.get('path') or '').rstrip('/').split('/')[-1].strip().lower()


def t11_frames(frames):
    """(frames with both groups listed, the list of frame numbers where any Grade element's rect intersects any POINTS element's).
    ui[] lists leaf elements (UI/Menus/Results/Tally/Points/Number, .../Tally/Grade/Plate ...): an element belongs to the group
    whose name is a path segment of a RESULTS path (NIGHT23: the first version looked for the group's own entry, which is never listed)."""
    both, bad = 0, []
    for fr in frames:
        pts, grade = [], []
        for e in ui_list(fr):
            if draws_nothing(e):
                continue
            r = rect_of(e.get('rect'))
            if r is None:
                continue
            segs = [x.strip().lower() for x in (e.get('path') or '').split('/')]
            if 'results' not in segs:
                continue
            if 'points' in segs:
                pts.append(r)
            elif 'grade' in segs:
                grade.append(r)
        if pts and grade:
            both += 1
            if any(rects_intersect(a, b) for a in pts for b in grade):
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


def lum_monotonic(thirds, slack=LUM_SLACK, fall=LUM_FALL):
    return all(thirds[i + 1] <= thirds[i] + slack for i in range(len(thirds) - 1)) and thirds[0] - thirds[-1] >= fall


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
    return {'LUM': PF(ok, 'thirds %s' % ' > '.join('%.1f' % t for t in th), 'must fall by >= %.1f (slack %.1f)' % (LUM_FALL, LUM_SLACK), num_=th[0] - th[2])}


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
            if 'round stamp' not in (run[0][2].get('path') or '').lower() or run[0][0] == 0:  # NIGHT23: only THE STAMP (UiStamp22's 'Round stamp'); RESULTS' older NEW BEST / card-foot 'Stamp' elements are not it
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

# ---------------------------------------------------------------------------------------------------------------
# The table

M1_TARGET = '>= 3.0 x hit12'
SPEC = (
    # (metric, label, scenario, target, function)
    [('M1', 'M1 hit count', 'hit12', 'the M1 denominator', None),
     ('M1', 'M1 kill/hit', 'kill12', M1_TARGET, None)]
    + [('M2', 'M2 fx', s, '>= 8 % of h', m2) for s in ('kill6', 'kill12', 'kill20')]
    + [('M2', 'M2 marker', s, 'frame 1-2, >= 48 px, 300-400 ms', m2) for s in ('kill6', 'kill12', 'kill20')]
    + [('M3', lab, s, tg, m3) for s in ('kill12', 'horde5')
       for lab, tg in (('M3 centroid', '>= 1.5 body h'), ('M3 outside', '>= 60 % outside'))]
    + [('M4', 'M4 freeze', s, '%d-%d steps' % M4_TARGETS[s], m4) for s in M4_TARGETS]
    + [('M5', 'M5 move', 'hit12', '>= 8 px within 50 ms', m5),
       ('M5', 'M5 back', 'hit12', 'back within 200 ms', m5),
       ('M5', 'M5 flash', 'hit12', '>= 2 frames flash > 0.5', m5)]
    + [('M6', 'M6 boost kick', 'boost', '+10 +/- 2 deg by frame 2', m6),
       ('M6', 'M6 boost back', 'boost', 'back over 300-400 ms', m6),
       ('M6', 'M6 sprint fov', 'sprint', '+5-6 deg within 250 ms', m6),
       ('M6', 'M6 slide fov', 'slide', '+6-8 deg within 100 ms', m6),
       ('M6', 'M6 slide drop', 'slide', '>= 0.4 m within 100 ms', m6),
       ('M6', 'M6 slide roll', 'slide', '3-5 deg', m6)]
    + [('M7', 'M7 strafe roll', 'strafe', '1.5-3 deg at full strafe', m7),
       ('M7', 'M7 land dip', 'land', '0.05-0.15 m', m7),
       ('M7', 'M7 land back', 'land', 'back in 150-200 ms', m7)]
    # NIGHT20: M8 v2 (L1-L5) replaces NIGHT18's M8 (logged; m8 kept for reading old reels by hand)
    + [('L', 'L1 clear', s, '0 frames inside rho 0.33; alpha < 0.30 to 0.55', m8v2) for s in ('sprint', 'boost', 'boostside', 'slide', 'strafe')]
    + [('L', 'L2 count', s, '0 under 9 m/s; sprint 11 +/- 4; boost >= 35 in 50 ms', m8v2) for s in ('sprint', 'boost', 'boostside', 'strafe')]
    + [('L', 'L3 length', s, 'sprint 25-45 px; boost 80-140 px', m8v2) for s in ('sprint', 'boost')]
    + [('L', 'L4 direction', s, 'strafe within 20 deg of horizontal; a fall of vertical', m8v2) for s in ('strafe', 'boostside', 'land', 'slam')]
    + [('L', 'L5 enemies', s, '0 frames over a live enemy', m8v2) for s in ('sprint', 'boost', 'boostside', 'slide', 'strafe')]
)
EVERY = (
    ('M9', 'M9 pop-ins', '0 pop-ins', m9),
    ('M9', 'M9 entrance time', '90-300 ms', m9),
    ('M10', 'M10 text floor', '0 texts < 18 px', m10),
    ('M11', 'M11 roles', '0 unlisted role users', m11),
    ('S12', 'S12 tokens', '0 (pad pass: no keys; keyboard pass: no pad words)', s12),
    ('S14', 'S14 centre', '0 persistent HUD in the central 40 %', s14),
    ('M-MAGENTA', 'M-MAGENTA', '0 px within 40 of (255, 0, 255) on every frame', m_magenta),
    ('T11', 'T11', 'the Grade card never intersects POINTS', t11),
    ('LUM', 'LUM', 'dusk: the thirds do not rise', lum),
    ('STAMP-T', 'STAMP-T', 'stamp entrance 200-300 ms', stamp_t),
)


class Row:
    def __init__(self, metric, label, scenario, target, r):
        self.metric, self.label, self.scenario, self.target, self.r = metric, label, scenario, target, r

    def key(self):
        return (self.label, self.scenario)

    def result(self):
        r = self.r
        if r.status == 'n/a':
            return 'n/a: ' + r.reason
        return r.status + (' (' + r.reason + ')' if r.reason else '')

    def short(self):
        return self.r.value if self.r.status in ('PASS', 'FAIL', 'info') else 'n/a'


def _guard(fn, sc, ctx):
    try:
        return fn(sc, ctx)
    except Exception as e:  # never crash: the row says why
        return {'__error__': NA('error: %s: %s' % (type(e).__name__, e))}


def compute(reel, ctx):
    """Rows for one reel. ctx: roles, roles_err, denom (count, source) or None. Returns (rows, hit12 count)."""
    rows, cache = [], {}
    # M1
    h12 = reel.scenarios.get('hit12')
    hcount = hinfo = herr = None
    if h12 is None:
        herr = 'scenario not in reel'
    else:
        try:
            hcount, hinfo, herr = m1_count(h12)
        except Exception as e:
            herr = 'error: %s' % e
    if hcount is None:
        rows.append(Row('M1', 'M1 hit count', 'hit12', 'the M1 denominator', NA(herr)))
    else:
        rows.append(Row('M1', 'M1 hit count', 'hit12', 'the M1 denominator',
                        R('info', '%d px' % hcount, detail=[hinfo], num_=hcount)))
    denom = ctx.get('denom')
    if denom is None:
        denom = (hcount, 'this reel', herr)
    k12 = reel.scenarios.get('kill12')
    tgt = M1_TARGET + (' (base)' if denom[1] == 'base' else '')
    if k12 is None:
        rows.append(Row('M1', 'M1 kill/hit', 'kill12', tgt, NA('scenario not in reel')))
    else:
        try:
            kcount, kinfo, kerr = m1_count(k12)
        except Exception as e:
            kcount, kinfo, kerr = None, None, 'error: %s' % e
        if kcount is None:
            r = NA(kerr)
        elif denom[0] is None:
            r = NA('no hit12 count for the denominator (%s: %s)' % (denom[1], denom[2]), value='%d px' % kcount)
        elif denom[0] == 0:
            r = NA('the hit12 denominator is 0 px (%s)' % denom[1], value='%d px' % kcount)
        else:
            ratio = kcount / float(denom[0])
            r = PF(ratio >= 3.0 - 1e-9, '%.2f (%d / %d px)' % (ratio, kcount, denom[0]),
                   detail=[kinfo + '; denominator: hit12 of %s' % denom[1]], num_=ratio)
        rows.append(Row('M1', 'M1 kill/hit', 'kill12', tgt, r))
    for metric, label, scen, target, fn in SPEC:
        if fn is None:
            continue
        sc = reel.scenarios.get(scen)
        if sc is None:
            rows.append(Row(metric, label, scen, target, NA('scenario not in reel')))
            continue
        key = (fn.__name__, scen)
        if key not in cache:
            cache[key] = _guard(fn, sc, ctx)
        res = cache[key]
        r = res.get(label) or res.get('__error__') or NA('not measured')
        rows.append(Row(metric, label, scen, target, r))
    for metric, label, target, fn in EVERY:
        for scen in reel.order:
            sc = reel.scenarios[scen]
            key = (fn.__name__, scen)
            if key not in cache:
                cache[key] = _guard(fn, sc, ctx)
            res = cache[key]
            r = res.get(label) or res.get('__error__') or NA('not measured')
            rows.append(Row(metric, label, scen, target, r))
    return rows, (hcount, herr)


def render_table(rows, base_idx, prev_idx):
    lines = ['| Metric | Scenario | Value | Target | Result | Base | Prev |',
             '| --- | --- | --- | --- | --- | --- | --- |']
    for row in rows:
        b = base_idx.get(row.key()) if base_idx is not None else None
        p = prev_idx.get(row.key()) if prev_idx is not None else None
        lines.append('| %s | %s | %s | %s | %s | %s | %s |' % tuple(cell(x) for x in (
            row.label, row.scenario, row.r.value, row.target, row.result(),
            '-' if b is None else b.short(), '-' if p is None else p.short())))
    return lines


def cmd_metrics(a):
    reel = Reel(a.reel)
    if reel.errors:
        print('feel-metrics: ' + '; '.join(reel.errors), file=sys.stderr)
        return 2
    roles_path = a.roles or default_roles_path()
    roles, rerr = load_roles(roles_path)
    ctx = {'roles': roles, 'roles_err': rerr}
    base = Reel(a.base) if a.base else None
    prev = Reel(a.prev) if a.prev else None
    base_rows = prev_rows = None
    denom = None
    notes = []
    if base is not None:
        if base.errors:
            notes.append('base unreadable: ' + '; '.join(base.errors))
            base = None
        else:
            base_rows, (bh, berr) = compute(base, dict(ctx))
            if bh is not None:
                denom = (bh, 'base', None)
            else:
                notes.append("the base's hit12 count is n/a (%s): this reel's own hit12 is M1's denominator" % berr)
    rows, (own_h, own_err) = compute(reel, dict(ctx, denom=denom))
    if prev is not None:
        if prev.errors:
            notes.append('prev unreadable: ' + '; '.join(prev.errors))
            prev = None
        else:
            prev_rows, _ = compute(prev, dict(ctx, denom=denom))
    base_idx = {r.key(): r for r in base_rows} if base_rows else None
    prev_idx = {r.key(): r for r in prev_rows} if prev_rows else None
    table = render_table(rows, base_idx, prev_idx)
    counts = {}
    for r in rows:
        counts[r.r.status] = counts.get(r.r.status, 0) + 1
    summary = '%d PASS, %d FAIL, %d n/a, %d info (of %d rows)' % (
        counts.get('PASS', 0), counts.get('FAIL', 0), counts.get('n/a', 0), counts.get('info', 0), len(rows))
    out = a.out or default_metrics_out(reel.path)
    den_txt = ('hit12 = %d px (%s)' % (denom[0], denom[1])) if denom else (
        'hit12 = %s px (this reel)' % own_h if own_h is not None else 'n/a (%s)' % own_err)
    md = ['# FEEL-METRICS', '',
          '- reel: %s' % reel.describe(),
          '- base: %s' % (base.describe() if base else 'none'),
          '- prev: %s' % (prev.describe() if prev else 'none'),
          '- roles: %s%s' % (roles_path, '' if roles is not None else ' (%s)' % rerr),
          "- M1's denominator: %s" % den_txt,
          '- written: %s by Tools/feel-metrics.py' % datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S'),
          '', '**%s**' % summary, '']
    md += table
    det = []
    for row in rows:
        if row.r.detail and row.r.status in ('FAIL', 'info', 'PASS'):
            if row.r.status == 'PASS' and row.metric in ('M9', 'M10', 'M11'):
                continue
            det.append('- **%s / %s** (%s): %s' % (row.label, row.scenario, row.r.status,
                                                    '; '.join(str(x) for x in row.r.detail[:12])
                                                    + (' ... (%d more)' % (len(row.r.detail) - 12)
                                                       if len(row.r.detail) > 12 else '')))
    md += ['', '## Details', ''] + (det or ['- none'])
    rnotes = []
    for rl, tag in ((reel, 'reel'), (base, 'base'), (prev, 'prev')):
        if rl is None:
            continue
        for n in rl.notes:
            rnotes.append('- %s: %s' % (tag, n))
        for s in rl.order:
            for n in rl.scenarios[s].notes:
                rnotes.append('- %s %s: %s' % (tag, s, n))
    md += ['', '## Notes', ''] + ['- ' + n for n in notes] + rnotes + [
        '- px values are at 1080p (a reel of another size is scaled by 1080 / h; M1 counts by (1080 / h)^2).',
        '- frame 1 of an event = the frame whose line carries it (M1 = the shot frame vs the frame before; M2 frames '
        '1-4 = the kill frame + 3; marker frame = its frame - the kill frame + 1).',
        '- M1 masks the ui[] rects of both compared frames. M5 = the rect against enemies[].anchor (camera kick '
        'removed); n/a without an anchor. M4 sums heldSteps over the first frozen run (held, or heldSteps > 0) '
        'starting within %d frames of the shot.' % HELD_SEARCH,
        '- M9 PASS / FAIL counts first appearances (re-entries listed); a pop-in starts at 95-105 %% of rest scale, '
        '>= 95 %% rest alpha, < 40 px from rest. M8 central 60 %% = 20-80 %% of each side.']
    try:
        os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
        with open(out, 'w', encoding='utf-8', newline='\n') as fh:
            fh.write('\n'.join(md) + '\n')
    except OSError as e:
        print('feel-metrics: could not write %s: %s' % (out, e), file=sys.stderr)
        return 2
    for ln in table:
        print(ln)
    print()
    print(summary)
    for n in notes:
        print('note: ' + n)
    print('wrote ' + out)
    return 0


def default_roles_path():
    return os.path.join(os.path.dirname(os.path.abspath(__file__)), 'feel-roles.json')


def _side_suffix(reel_path):
    b = os.path.basename(os.path.normpath(reel_path))
    return '' if b == 'on' else '-' + b


def default_metrics_out(reel_path):
    parent = os.path.dirname(os.path.normpath(reel_path))
    return os.path.join(parent, 'FEEL-METRICS%s.md' % _side_suffix(reel_path))


# ---------------------------------------------------------------------------------------------------------------
# Flipbooks

def pick_frames(sc):
    """8 (frame, role) picks, chosen from telemetry."""
    if not sc.fs:
        return []
    lo, hi = sc.fs[0], sc.fs[-1]
    clamp = lambda f: max(lo, min(hi, f))
    i = sc.input_frame if sc.input_frame is not None else lo
    if sc.kind in KINDS_CROP:
        s, _ = sc.shot_frame()
        run = sc.held_run(s if s is not None else i)
        if run is not None:
            fh, lh = run[3], run[4]
            end = sc.next_f(run[1])
            end = end if end is not None else run[1] + 1
            roles = ['input', 'first HELD', 'last HELD', 'freeze end']
        else:
            ev = None
            for name in ('kill', 'hit'):
                ev = sc.event_frame(name, i)
                if ev is not None:
                    break
            if ev is None:
                ev = i
            # no freeze: tiles 2-4 are the frames after the event (the event frame is tile 1 when it is the input)
            o = 1 if ev == i else 0
            fh, lh, end = ev + o, ev + o + 1, ev + o + 2
            roles = ['input', 'event + %d (no freeze)' % o, 'event + %d' % (o + 1), 'event + %d' % (o + 2)]
        live = None
        for f in reversed(sc.fs):
            if f <= i:
                break
            if has_live_effect(sc.by_f[f]):
                live = f
                break
        if live is None:
            live = end + sc.frames_for(700)
            lrole = 'freeze end + 700 ms (no live effect)'
        else:
            lrole = 'last live effect'
        picks = [(i, roles[0]), (fh, roles[1]), (lh, roles[2]), (end, roles[3])]
        picks += [(end + sc.frames_for(ms), '%s + %d ms' % (roles[3], ms)) for ms in KILL_AFTER_MS]
        picks.append((live, lrole))
    else:
        picks = [(i + sc.frames_for(ms), 'input' if ms == 0 else 'input + %d ms' % ms) for ms in UI_OFFSETS_MS]
    return [(clamp(f), role) for f, role in picks]


def crop_centre(sc):
    i = sc.input_frame if sc.input_frame is not None else (sc.fs[0] if sc.fs else 0)
    g, r = sc.target_rect_at_or_before(i)
    if r is None:
        for f in sc.fs:
            r = sc.target_rect(f)
            if r is not None:
                break
    if r is None and sc.target is None:
        r = sc.enemies_union(sc.prev_f(i) if sc.prev_f(i) is not None else i)
    if r is not None:
        return rect_centre(r)
    for f in sc.fs:
        if f < i:
            continue
        u = None
        for g in ('burst', 'chunks', 'ring', 'hitFlash', 'hitMarker'):
            v = fx_group(sc.by_f[f], g)
            if v:
                u = rect_union(u, rect_of(v.get('rect')))
        if u is not None:
            return rect_centre(u)
    return (sc.w / 2.0, sc.h / 2.0)


def _blank(size):
    return Image.new('RGB', size, (58, 58, 64))


def render_sheets(sc, picks):
    """kill / hit: one 4 x 2 sheet of native 480 crops; else two 2 x 2 sheets of 960 x 540 frames."""
    fnt = font(20)
    if sc.kind in KINDS_CROP:
        cx, cy = crop_centre(sc)
        t = KILL_TILE
        sheet = Image.new('RGB', (4 * t, 2 * (t + MARGIN)), (14, 14, 18))
        d = ImageDraw.Draw(sheet)
        for n, (f, _) in enumerate(picks[:8]):
            col, row = n % 4, n // 4
            x, y = col * t, row * (t + MARGIN)
            im = sc.load_rgb(f)
            label = str(n + 1)
            if im is None:
                tile = _blank((t, t))
                label += '  (frame missing)'
            else:
                # 480 px at 1080p: a 1440p frame crops 640 px and scales it to 480, so the field stays the same
                sx, sy = im.width / float(sc.w), im.height / float(sc.h)
                ts = int(round(t * im.height / 1080.0))
                x0 = int(round(cx * sx - ts / 2.0))
                y0 = int(round(cy * sy - ts / 2.0))
                x0 = max(0, min(x0, im.width - ts)) if im.width >= ts else 0
                y0 = max(0, min(y0, im.height - ts)) if im.height >= ts else 0
                tile = im.crop((x0, y0, x0 + ts, y0 + ts))
                if ts != t:
                    tile = tile.resize((t, t), Image.Resampling.LANCZOS)
            sheet.paste(tile, (x, y + MARGIN))
            d.text((x + 8, y + 4), label, fill=(210, 210, 210), font=fnt)
        return [sheet]
    tw, th = WIDE_TILE
    sheets = []
    for half in (0, 1):
        sheet = Image.new('RGB', (2 * tw, 2 * (th + MARGIN)), (14, 14, 18))
        d = ImageDraw.Draw(sheet)
        for n in range(4):
            idx = half * 4 + n
            if idx >= len(picks):
                break
            f = picks[idx][0]
            col, row = n % 2, n // 2
            x, y = col * tw, row * (th + MARGIN)
            im = sc.load_rgb(f)
            label = str(idx + 1)
            if im is None:
                tile = _blank((tw, th))
                label += '  (frame missing)'
            else:
                tile = im.resize((tw, th), Image.Resampling.LANCZOS)
            sheet.paste(tile, (x, y + MARGIN))
            d.text((x + 8, y + 4), label, fill=(210, 210, 210), font=fnt)
        sheets.append(sheet)
    return sheets


def default_flip_out(reel_path):
    parent = os.path.dirname(os.path.normpath(reel_path))
    return os.path.join(parent, 'flip' + _side_suffix(reel_path))


def cmd_flip(a):
    reel = Reel(a.reel)
    if reel.errors:
        print('feel-metrics: ' + '; '.join(reel.errors), file=sys.stderr)
        return 2
    out = a.out or default_flip_out(reel.path)
    os.makedirs(out, exist_ok=True)
    index = ['# flipbooks of %s (tile: frame, why)' % reel.path]
    n = 0
    for name in reel.order:
        sc = reel.scenarios[name]
        picks = pick_frames(sc)
        if not picks:
            index.append('%s: no frame lines, skipped' % name)
            continue
        sheets = render_sheets(sc, picks)
        if len(sheets) == 1:
            files = [name + '.png']
        else:
            files = [name + '-a.png', name + '-b.png']
        for im, fn in zip(sheets, files):
            im.save(os.path.join(out, fn))
            n += 1
        missing = sum(1 for f, _ in picks if not os.path.isfile(sc.img_path(f)))
        index.append('%s (%s): %s%s' % (name, sc.kind, ', '.join('%d=f%d %s' % (k + 1, f, r) for k, (f, r) in
                                                               enumerate(picks)),
                                         '; %d frames missing on disk' % missing if missing else ''))
    with open(os.path.join(out, 'index.txt'), 'w', encoding='utf-8', newline='\n') as fh:
        fh.write('\n'.join(index) + '\n')
    print('wrote %d sheets to %s (index.txt lists the frames)' % (n, out))
    return 0


# ---------------------------------------------------------------------------------------------------------------
# Blind A/B sheets

def _same_path(a, b):
    return os.path.normcase(os.path.abspath(a)) == os.path.normcase(os.path.abspath(b))


def cmd_ab(a):
    ra, rb = Reel(a.reel_a), Reel(a.reel_b)
    for r in (ra, rb):
        if r.errors:
            print('feel-metrics: ' + '; '.join(r.errors), file=sys.stderr)
            return 2
    out, key_path = os.path.abspath(a.out), os.path.abspath(a.key)
    if os.path.normcase(key_path).startswith(os.path.normcase(out + os.sep)):
        print('feel-metrics: the key must not be inside the neutral folder', file=sys.stderr)
        return 2
    os.makedirs(out, exist_ok=True)
    old = [f for f in os.listdir(out) if len(f) > 3 and f[0] == 's' and f[1:3].isdigit()]
    if old and not a.force:
        print('feel-metrics: %s already holds %d sNN files; pass --force to overwrite' % (out, len(old)),
              file=sys.stderr)
        return 2
    for f in old:
        try:
            os.remove(os.path.join(out, f))
        except OSError:
            pass
    inverted = None
    if a.invert_key:
        try:
            with open(a.invert_key, 'r', encoding='utf-8') as fh:
                inverted = json.load(fh)
        except Exception as e:
            print('feel-metrics: --invert-key unreadable: %s' % e, file=sys.stderr)
            return 2
    seed = a.seed if a.seed is not None else random.SystemRandom().randrange(1, 2 ** 31)
    rng = random.Random(seed)
    names = [n for n in ra.order if n in rb.scenarios]
    if getattr(a, 'only', None):  # NIGHT20: the judged set only, in the given order
        want = [s.strip() for s in a.only.split(',') if s.strip()]
        names = [n for n in want if n in ra.scenarios and n in rb.scenarios]
    key = {'seed': seed, 'first': ra.path, 'second': rb.path, 'out': out,
           'inverted_from': os.path.abspath(a.invert_key) if a.invert_key else None, 'scenarios': {}}
    old_by_name = {}
    if inverted:
        for tag, v in (inverted.get('scenarios') or {}).items():
            if isinstance(v, dict) and 'name' in v:
                old_by_name[v['name']] = v
    n_img = 0
    for num_, name in enumerate(names, 1):
        tag = 's%02d' % num_
        coin = rng.random() < 0.5
        if name in old_by_name:
            coin = not _same_path(old_by_name[name].get('A', ''), ra.path)
        side = {'A': ra if coin else rb, 'B': rb if coin else ra}
        frames = {}
        kind = ra.scenarios[name].kind
        for lab in ('A', 'B'):
            sc = side[lab].scenarios[name]
            picks = pick_frames(sc)
            frames[lab] = [f for f, _ in picks]
            sheets = render_sheets(sc, picks) if picks else []
            if len(sheets) == 1:
                sheets[0].save(os.path.join(out, '%s-%s.png' % (tag, lab)))
                n_img += 1
            else:
                for k, im in enumerate(sheets, 1):
                    im.save(os.path.join(out, '%s-%s%d.png' % (tag, lab, k)))
                    n_img += 1
        if kind in KINDS_CROP:
            tiles = ('tiles: 1 the input; 2 the first frozen frame; 3 the last frozen frame; 4 the first frame moving '
                     'again (with no freeze, 2-4 are the three frames after the hit); 5-7 then +120, +250, +400 ms; '
                     '8 the last frame with a live effect. Crops of 480 x 480 at 1080p, centred on the target.')
        else:
            tiles = ('tiles: 1 the input; 2-8 the input + 33, 67, 100, 167, 250, 400, 700 ms (tiles 1-4 on sheet 1, '
                     '5-8 on sheet 2). Whole frames at half size.')
        with open(os.path.join(out, tag + '.txt'), 'w', encoding='utf-8', newline='\n') as fh:
            fh.write('%s: scenario %s\nQuestion: %s\nThen: %s\n%s\n' % (
                tag, name, question_for(name, kind), TOO_MUCH, tiles))
        key['scenarios'][tag] = {'name': name, 'A': side['A'].path, 'B': side['B'].path,
                                 'A_is': 'first' if coin else 'second', 'frames': frames}
    os.makedirs(os.path.dirname(key_path), exist_ok=True)
    with open(key_path, 'w', encoding='utf-8', newline='\n') as fh:
        json.dump(key, fh, indent=1)
    print('wrote %d images for %d scenarios to %s; the key is in the key file' % (n_img, len(names), out))
    only = [n for n in ra.order if n not in rb.scenarios] + [n for n in rb.order if n not in ra.scenarios]
    if only:
        print('skipped (in one reel only): ' + ', '.join(only))
    return 0


# ---------------------------------------------------------------------------------------------------------------
# Side-by-side GIF

def cmd_gif(a):
    ra, rb = Reel(a.reel_a), Reel(a.reel_b)
    for r in (ra, rb):
        if r.errors:
            print('feel-metrics: ' + '; '.join(r.errors), file=sys.stderr)
            return 2
    name = a.scenario
    if name not in ra.scenarios or name not in rb.scenarios:
        print('feel-metrics: %s is not in both reels' % name, file=sys.stderr)
        return 2
    sa, sb = ra.scenarios[name], rb.scenarios[name]
    if not sa.fs or not sb.fs:
        print('feel-metrics: %s has no frame lines on one side' % name, file=sys.stderr)
        return 2
    labels = a.labels or ['N17', 'N18']
    ia = sa.input_frame if sa.input_frame is not None else sa.fs[0]
    ib = sb.input_frame if sb.input_frame is not None else sb.fs[0]
    pre = min(sa.frames_for(300), ia - sa.fs[0], ib - sb.fs[0])
    pre = max(0, pre)
    a0, b0 = ia - pre, ib - pre
    length = max(sa.fs[-1] - a0, sb.fs[-1] - b0) + 1
    step = max(1, int(a.step))
    fps = sa.fps
    sw, sh = GIF_SIDE
    gap = 8
    fnt = font(20)
    frames, last_a, last_b = [], None, None
    for k in range(0, length, step):
        canvas = Image.new('RGB', (2 * sw + gap, sh + MARGIN), (14, 14, 18))
        d = ImageDraw.Draw(canvas)
        for col, (sc, f0, lab) in enumerate(((sa, a0, labels[0]), (sb, b0, labels[1]))):
            f = min(f0 + k, sc.fs[-1])
            im = sc.load_rgb(f, draft=(sw, sh))
            if im is None:
                im = last_a if col == 0 else last_b
            else:
                im = im.resize((sw, sh), Image.Resampling.LANCZOS)
                if col == 0:
                    last_a = im
                else:
                    last_b = im
            if im is None:
                im = _blank((sw, sh))
            x = col * (sw + gap)
            canvas.paste(im, (x, MARGIN))
            d.text((x + 8, 4), lab, fill=(210, 210, 210), font=fnt)
        frames.append(canvas.quantize(colors=255, method=Image.Quantize.FASTOCTREE))
    # real speed: GIF delays are whole centiseconds, so round the cumulative time, not each delay
    per = step * 1000.0 / fps
    durs = []
    for k in range(len(frames)):
        t0 = int(round(k * per / 10.0)) * 10
        t1 = int(round((k + 1) * per / 10.0)) * 10
        durs.append(max(20, t1 - t0))
    out = a.out or 'ab-%s.gif' % name
    od = os.path.dirname(os.path.abspath(out))
    os.makedirs(od, exist_ok=True)
    frames[0].save(out, save_all=True, append_images=frames[1:], duration=durs, loop=0, disposal=1, optimize=False)
    print('wrote %s: %d frames, %.2f s (every %d frame%s), %s left / %s right' % (
        out, len(frames), sum(durs) / 1000.0, step, '' if step == 1 else 's', labels[0], labels[1]))
    return 0


# ---------------------------------------------------------------------------------------------------------------
# Roles (M11 alone)

def _collapse(path):
    """HUD/Pops/Pop3 -> HUD/Pops/Pop (pooled siblings share one prefix)."""
    head, _, last = path.rpartition('/')
    stripped = last.rstrip('0123456789')
    if stripped != last and stripped:
        return (head + '/' if head else '') + stripped
    return path


def cmd_roles(a):
    reel = Reel(a.reel)
    if reel.errors:
        print('feel-metrics: ' + '; '.join(reel.errors), file=sys.stderr)
        return 2
    if a.suggest:
        seen = {r: set() for r in ROLE_NAMES}
        for name in reel.order:
            for (role, path) in role_users(reel.scenarios[name]):
                seen.setdefault(role, set()).add(_collapse(path))
        d = {'_doc': 'feel-roles.json: the ui paths (plain prefixes) allowed to use each colour token (M11); '
                     'suggested from %s' % reel.path}
        for r in sorted(seen):
            d[r] = sorted(seen[r])
        print(json.dumps(d, indent=1))
        return 0
    path = a.roles or default_roles_path()
    roles, err = load_roles(path)
    if roles is None:
        print('feel-metrics: ' + err, file=sys.stderr)
        return 2
    total = 0
    for name in reel.order:
        viol, users = role_violations(reel.scenarios[name], roles)
        total += len(viol)
        print('%s: %d unlisted of %d role users' % (name, len(viol), len(users)))
        for role, p, f, col in viol:
            print('  %s %s (%s) from f%d' % (role, p, col, f))
    print('M11: %d unlisted role users in %d scenarios (roles: %s)' % (total, len(reel.order), path))
    return 1 if total else 0


# ---------------------------------------------------------------------------------------------------------------
# Self test: synthetic reels that run every metric's PASS and FAIL paths, plus the hand-written sample

def _fr(f, fps=60, **kw):
    fr = {'type': 'frame', 'f': f, 't': round(f / float(fps), 6), 'step': 100 + 2 * f, 'steps': 2, 'held': False,
          'heldSteps': 0, 'slowmo': 1.0, 'flow': 'Playing',
          'cam': {'fov': 70.0, 'roll': 0.0, 'pitch': 0.0, 'height': 1.6, 'x': 0.0, 'y': 1.6, 'z': 0.0},
          'vel': {'fwd': 0.0, 'right': 0.0, 'up': 0.0, 'speed': 0.0}, 'enemies': [], 'fx': {}, 'chunkPts': [],
          'chunkTotal': 0, 'ui': [], 'events': []}
    fr.update(kw)
    return fr


def _ui(eid, path, rect, scale=1.0, alpha=1.0, color='#F4F1EA', role=None, px=None, text=None):
    return {'id': eid, 'path': path, 'rect': [round(x, 1) for x in rect], 'scale': round(scale, 4),
            'alpha': round(alpha, 4), 'color': color, 'role': role, 'px': px, 'text': text}


def _write_scenario(root, name, kind, frames, header_extra=None, draw=None, images=True):
    d = os.path.join(root, name)
    os.makedirs(d, exist_ok=True)
    h = {'type': 'scenario', 'name': name, 'kind': kind, 'feelAB': True, 'seed': 1, 'distance': None,
         'target': None, 'inputFrame': 18, 'w': 1920, 'h': 1080, 'fps': 60, 'simHz': 120, 'maxSpeed': 10.5}
    h.update(header_extra or {})
    with open(os.path.join(d, 'telemetry.jsonl'), 'w', encoding='utf-8', newline='\n') as fh:
        fh.write(json.dumps(h) + '\n')
        for fr in frames:
            fh.write(json.dumps(fr) + '\n')
    if images:
        for fr in frames:
            im = Image.new('RGB', (1920, 1080), (100, 100, 104))
            dr = ImageDraw.Draw(im)
            if draw:
                draw(fr, dr)
            else:
                _draw_default(fr, dr)
            im.save(os.path.join(d, 'f%04d.jpg' % fr['f']), quality=80)


def _draw_default(fr, dr):
    fov = fr['cam']['fov']
    y = 540 + (fov - 70) * 12 + fr['cam']['roll'] * 6
    dr.rectangle([0, y, 1919, 1079], fill=(70, 66, 80))
    for e in fr['enemies']:
        x, yy, w, h = e['rect']
        dr.rectangle([x, yy, x + w, yy + h], fill=(255, 255, 255) if e.get('flash', 0) > 0.5 else (40, 40, 44))
    for g, v in fr['fx'].items():
        x, yy, w, h = v['rect']
        dr.rectangle([x, yy, x + w, yy + h], outline=(255, 120, 60), width=4)
    for p in fr['chunkPts']:
        dr.rectangle([p[0] - 5, p[1] - 5, p[0] + 5, p[1] + 5], fill=(255, 90, 40))
    for e in fr['ui']:
        x, yy, w, h = e['rect']
        c = int(255 * e['alpha'])
        dr.rectangle([x, yy, x + w, yy + h], fill=(c, c, c))


def _gen_hit12(root, good):
    # schema: the shot's frame (f18) carries input / shot / hit and is frame 1; a freeze may start mid-frame
    # (held false, heldSteps > 0). The camera kick moves rect and anchor alike; only the body offset is the hit.
    T = 7
    body = [930, 440, 60, 200]
    frames = []
    kick = {18: 20, 19: 12, 20: 6, 21: 2}
    for f in range(40):
        ev, held, hs, steps = [], False, 0, 2
        if f == 18:
            ev = ['input', 'shot', 'hit']
            hs, steps = 1, 1
        if not good and f in (19, 20):
            held, hs, steps = True, 2, 0
        dx = {18: 10, 19: 8, 20: 5, 21: 1}.get(f, 0) if good else (3 if f >= 18 else 0)
        flash = ({18: 1.0, 19: 0.9, 20: 0.6, 21: 0.2}.get(f, 0.0) if good else {18: 1.0}.get(f, 0.0))
        cx = kick.get(f, 0)
        ui = [_ui(0, 'HUD/Points/Value', [1650, 60, 200, 60], color='#FFC23D', role='GOLD', px=42.0, text='0')]
        fx = {}
        if f >= 18:
            k = f - 18
            if good:
                s = min(1.0, 0.5 + 0.5 * k / 8.0)
                ui.append(_ui(2, 'HUD/Markers/Hit', [1000, 520, 40 * s, 40 * s], scale=s, alpha=min(1.0, 0.4 + k * 0.1)))
            else:
                ui.append(_ui(2, 'HUD/Markers/Hit', [1000, 520, 40, 40]))
            fx['hitMarker'] = {'rect': [1000, 520, 40, 40], 'n': 1, 'a': 1.0}
        frames.append(_fr(f, held=held, heldSteps=hs, steps=steps, events=ev,
                          enemies=[{'id': T, 'rect': [body[0] + dx + cx, body[1], body[2], body[3]], 'flash': flash,
                                    'anchor': [960 + cx, 540]}],
                          ui=ui, fx=fx))

    def draw(fr, dr):
        _draw_default(fr, dr)
        if fr['f'] == 18:
            dr.rectangle([940, 520, 980, 560], fill=(255, 255, 255))

    _write_scenario(root, 'hit12', 'hit', frames, {'target': T, 'distance': 12.0}, draw)
    if not good:
        os.remove(os.path.join(root, 'hit12', 'f0018.jpg'))


def _gen_kill(root, name, good, held_steps, fx_h, marker, chunks, images=True, target=3, pixels=None):
    frames = []
    body = [930, 440, 60, 200]
    # like the Editor reel: f18 runs 1 step and swallows 1 (held false), then fully held frames swallow 2 each
    held_map = {18: 1}
    rem, f = held_steps - 1, 19
    while rem > 0:
        held_map[f] = min(2, rem)
        rem -= 2
        f += 1
    last_held = max(held_map)
    for f in range(90):
        ev = ['input', 'shot', 'hit', 'kill'] if f == 18 else []
        hs = held_map.get(f, 0)
        held = f in held_map and f > 18
        ens = []
        if target is None:
            if f < 18:
                ens = [{'id': i, 'rect': [700 + 120 * i, 440, 60, 200], 'flash': 0.0} for i in range(5)]
        elif f < 18 or not good:
            ens = [{'id': target, 'rect': body, 'flash': 0.0}]
        fx, pts = {}, []
        if 18 <= f <= 23 and fx_h:
            fx['burst'] = {'rect': [960 - fx_h / 2.0, 540 - fx_h / 2.0, fx_h, fx_h], 'n': 1, 'a': 1.0}
        if marker is not None:
            m0, msize, mdur = marker
            if m0 <= f < m0 + mdur:
                fx['killMarker'] = {'rect': [960 - msize / 2.0, 540 - msize / 2.0, msize, msize], 'n': 1, 'a': 1.0}
        if chunks and f >= 18:
            k = max(0, f - last_held)
            if chunks == 'thrown':
                cx, cy = 960 - 15 * k, 540 - 20 * k
            else:
                cx, cy = 960, 540
            spread = 20 if chunks == 'thrown' else 10
            pts = [[cx + spread * math.cos(j), cy + spread * math.sin(j)] for j in range(12)]
            fx['chunks'] = {'rect': [cx - spread, cy - spread, 2 * spread, 2 * spread], 'n': 12, 'a': 1.0}
        frames.append(_fr(f, held=held, heldSteps=hs, steps=0 if held else 2 - hs, events=ev, enemies=ens, fx=fx,
                          chunkPts=[[round(p[0], 1), round(p[1], 1)] for p in pts], chunkTotal=len(pts),
                          ui=[_ui(0, 'HUD/Points/Value', [1650, 60, 200, 60], color='#FFC23D', role='GOLD', px=42.0)]))

    def draw(fr, dr):
        _draw_default(fr, dr)
        if fr['f'] == 18 and pixels:
            dr.rectangle([960 - pixels / 2, 540 - pixels / 2, 960 + pixels / 2, 540 + pixels / 2], fill=(255, 255, 255))

    _write_scenario(root, name, 'kill', frames, {'target': target, 'distance': 12.0}, draw, images)


def _gen_camera(root, name, good):
    frames = []
    for f in range(60):
        k = f - 18
        fov, height, roll, right, speed, up = 70.0, 1.6, 0.0, 0.0, 0.0, 0.0
        ev = ['input'] if f == 18 else []
        fx = {}
        if name == 'boost':
            if k >= 0:
                if good:
                    d = {0: 5, 1: 9, 2: 10}.get(k)
                    if d is None:
                        d = max(0.0, 10 - 10 * (k - 2) / 21.0)
                else:
                    d = min(2 * (k + 1), 14) if k < 7 else max(0.0, 14 - (k - 6))
                fov = 70 + d
                speed = max(0.0, 10.5 - 0.2 * k) if good else 5.0
            if k >= 0:
                fx['speedLines'] = ({'rect': [0, 0, 1920, 1080], 'n': 40, 'a': 0.6 if speed >= 0.8 * 10.5 else 0.2}
                                    if good else {'rect': [700, 400, 500, 300], 'n': 40, 'a': 0.5})
        elif name == 'sprint':
            if k >= 0:
                fov = 70 + (min(5.5, 0.5 * (k + 1)) if good else min(3.0, 0.3 * (k + 1)))
                speed = min(10.5, 0.6 * (k + 1))
                a = (0.5 if speed >= 0.75 * 10.5 else 0.0) if good else 0.5
                if a > 0:
                    fx['speedLines'] = {'rect': [0, 0, 300, 1080] if good else [600, 300, 700, 500], 'n': 20, 'a': a}
        elif name == 'slide':
            if k >= 0:
                fov = 70 + (min(7.0, 2.5 * (k + 1)) if good else 3.0)
                height = 1.6 - (min(0.5, 0.125 * (k + 1)) if good else 0.2)
                roll = 4.0 if good else 1.0
        elif name == 'strafe':
            if k >= 0:
                right = min(7.0, 0.7 * (k + 1))
                roll = (2.2 if good else 0.5) * right / 7.0
                speed = abs(right)
        elif name == 'land':
            ld = 38
            if f == ld:
                ev = ev + ['land']
            if 18 <= f < ld:
                up = -0.4 * (f - 18)
            if f >= ld:
                j = f - ld
                if good:
                    prof = [0.03, 0.07, 0.1, 0.09, 0.08, 0.07, 0.055, 0.04, 0.03, 0.02, 0.008, 0.0]
                else:
                    prof = [0.1, 0.2, 0.3, 0.28, 0.26, 0.24, 0.22, 0.2, 0.18, 0.16, 0.14, 0.12, 0.1, 0.08, 0.06,
                            0.05, 0.04, 0.03, 0.02, 0.01, 0.0]
                height = 1.6 - (prof[j] if j < len(prof) else 0.0)
        frames.append(_fr(f, events=ev, fx=fx,
                          cam={'fov': fov, 'roll': roll, 'pitch': 0.0, 'height': height, 'x': 0, 'y': height, 'z': 0},
                          vel={'fwd': speed, 'right': right, 'up': up, 'speed': max(speed, abs(right))},
                          ui=[_ui(0, 'HUD/Points/Value', [1650, 60, 200, 60], color='#FFC23D', role='GOLD',
                                  px=42.0)]))
    _write_scenario(root, name, 'camera', frames, images=(name != 'land'))


def _gen_ui(root, name, good):
    frames = []
    for f in range(60):
        k = f - 19
        # ui-settings: an empty ui list until the menu opens; ui-title: the logo is up from frame 0
        ui = [] if name == 'ui-settings' else [_ui(0, 'Title/Logo', [660, 120, 600, 160], px=96.0, text='SKYGRIND')]
        if k >= 0:
            if good or name == 'ui-settings':
                t = min(1.0, (k + 1) / 9.0)
                e = 1 + 2.70158 * (t - 1) ** 3 + 1.70158 * (t - 1) ** 2  # ease-out-back
                s = 0.6 + 0.4 * e
                ui.append(_ui(1, 'Title/Menu/Play', [860 + 60 * (1 - e), 600, 200 * s, 60 * s], scale=s,
                              alpha=min(1.0, t * 1.5), px=24.0, text='PLAY'))
            else:
                ui.append(_ui(1, 'Title/Menu/Play', [860, 600, 200, 60], color='#FFC23D', role='GOLD', px=24.0,
                              text='PLAY'))
                ui.append(_ui(3, 'Title/Menu/Hint', [860, 900, 200, 30], px=14.0, text='press E'))
        if k >= 2:
            if good or name == 'ui-settings':
                t = min(1.0, (k - 1) / 9.0)
                e = 1 + 2.70158 * (t - 1) ** 3 + 1.70158 * (t - 1) ** 2
                ui.append(_ui(2, 'Title/Menu/Quit', [860, 700, 200 * (0.6 + 0.4 * e), 60], scale=0.6 + 0.4 * e,
                              alpha=min(1.0, t * 1.5), px=24.0, text='QUIT'))
            else:
                s = 0.5 if k == 2 else 1.0
                ui.append(_ui(2, 'Title/Menu/Quit', [860, 700, 200 * s, 60 * s], scale=s, alpha=1.0, px=24.0,
                              text='QUIT'))
        if k >= 0 and name == 'ui-title' and good:
            # a slam-in: full alpha from 150 % scale down to rest over 150 ms is motion, not a pop-in
            s = 1.5 - 0.5 * min(1.0, (k + 1) / 9.0)
            ui.append(_ui(4, 'Title/Menu/Badge', [1300 - 50 * s, 600 - 20 * s, 100 * s, 40 * s], scale=s, px=20.0,
                          text='NEW'))
        frames.append(_fr(f, flow='Title', events=['input'] if f == 18 else [], ui=ui))
    _write_scenario(root, name, 'ui', frames, images=(good or name != 'ui-settings'))


def _gen_reel(root, good, with_jpgs=True):
    os.makedirs(root, exist_ok=True)
    _gen_hit12(root, good)
    if good:
        _gen_kill(root, 'kill12', True, 7, 400, (18, 64, 21), 'thrown', pixels=300)
        _gen_kill(root, 'kill20', True, 7, 100, (18, 64, 21), 'thrown', images=False)
        _gen_kill(root, 'headshot12', True, 12, 300, None, 'thrown', images=False)
        _gen_kill(root, 'punchkill', True, 12, 300, None, None, images=False)
        _gen_kill(root, 'horde5', True, 18, 500, None, 'thrown', images=False, target=None)
    else:
        _gen_kill(root, 'kill12', False, 2, 40, (21, 30, 6), 'swell', pixels=50)
        _gen_kill(root, 'kill20', False, 7, 50, None, 'swell', images=False)
        _gen_kill(root, 'headshot12', False, 8, 300, None, 'swell', images=False)
        _gen_kill(root, 'horde5', False, 22, 500, None, None, images=False, target=None)
    for n in ('boost', 'sprint', 'slide', 'strafe', 'land'):
        _gen_camera(root, n, good)
    _gen_ui(root, 'ui-title', good)
    _gen_ui(root, 'ui-settings', good)
    with open(os.path.join(root, 'reel.json'), 'w', encoding='utf-8') as fh:
        json.dump({'build': 'selftest', 'feelAB': good, 'map': 'ARENA', 'w': 1920, 'h': 1080, 'fps': 60,
                   'simHz': 120, 'scenarios': ['hit12', 'kill12', 'kill20', 'ui-title', 'missing-one']}, fh)


EXPECT_GOOD = {
    ('M1 kill/hit', 'kill12'): 'PASS', ('M1 hit count', 'hit12'): 'info',
    ('M2 fx', 'kill12'): 'PASS', ('M2 fx', 'kill20'): 'PASS', ('M2 fx', 'kill6'): 'n/a',
    ('M2 marker', 'kill12'): 'PASS',
    ('M3 centroid', 'kill12'): 'PASS', ('M3 outside', 'kill12'): 'PASS',
    ('M3 centroid', 'horde5'): 'PASS', ('M3 outside', 'horde5'): 'PASS',
    ('M4 freeze', 'hit12'): 'PASS', ('M4 freeze', 'kill12'): 'PASS', ('M4 freeze', 'headshot12'): 'PASS',
    ('M4 freeze', 'punchkill'): 'PASS', ('M4 freeze', 'horde5'): 'PASS',
    ('M5 move', 'hit12'): 'PASS', ('M5 back', 'hit12'): 'PASS', ('M5 flash', 'hit12'): 'PASS',
    ('M6 boost kick', 'boost'): 'PASS', ('M6 boost back', 'boost'): 'PASS', ('M6 sprint fov', 'sprint'): 'PASS',
    ('M6 slide fov', 'slide'): 'PASS', ('M6 slide drop', 'slide'): 'PASS', ('M6 slide roll', 'slide'): 'PASS',
    ('M7 strafe roll', 'strafe'): 'PASS', ('M7 land dip', 'land'): 'PASS', ('M7 land back', 'land'): 'PASS',
    ('L1 clear', 'sprint'): 'n/a', ('L1 clear', 'boost'): 'n/a',  # NIGHT20: M8 v2 (the synthetic reel has no streaks channel)
    ('M9 pop-ins', 'ui-title'): 'PASS', ('M9 entrance time', 'ui-title'): 'PASS', ('M9 pop-ins', 'hit12'): 'PASS',
    ('M9 pop-ins', 'boost'): 'n/a', ('M9 pop-ins', 'ui-settings'): 'PASS',
    ('M10 text floor', 'ui-title'): 'PASS', ('M10 text floor', 'hit12'): 'PASS',
    ('M11 roles', 'ui-title'): 'PASS', ('M11 roles', 'kill12'): 'PASS',
}
EXPECT_BAD = {
    ('M1 kill/hit', 'kill12'): 'FAIL', ('M1 hit count', 'hit12'): 'n/a',
    ('M2 fx', 'kill12'): 'FAIL', ('M2 fx', 'kill20'): 'FAIL', ('M2 marker', 'kill12'): 'FAIL',
    ('M2 marker', 'kill20'): 'n/a',
    ('M3 centroid', 'kill12'): 'FAIL', ('M3 outside', 'kill12'): 'FAIL', ('M3 centroid', 'horde5'): 'n/a',
    ('M4 freeze', 'hit12'): 'FAIL', ('M4 freeze', 'kill12'): 'FAIL', ('M4 freeze', 'headshot12'): 'FAIL',
    ('M4 freeze', 'punchkill'): 'n/a', ('M4 freeze', 'horde5'): 'FAIL',
    ('M5 move', 'hit12'): 'FAIL', ('M5 back', 'hit12'): 'FAIL', ('M5 flash', 'hit12'): 'FAIL',
    ('M6 boost kick', 'boost'): 'FAIL', ('M6 boost back', 'boost'): 'FAIL', ('M6 sprint fov', 'sprint'): 'FAIL',
    ('M6 slide fov', 'slide'): 'FAIL', ('M6 slide drop', 'slide'): 'FAIL', ('M6 slide roll', 'slide'): 'FAIL',
    ('M7 strafe roll', 'strafe'): 'FAIL', ('M7 land dip', 'land'): 'FAIL', ('M7 land back', 'land'): 'FAIL',
    ('L1 clear', 'sprint'): 'n/a', ('L1 clear', 'boost'): 'n/a',  # NIGHT20: M8 v2 (the synthetic reel has no streaks channel)
    ('M9 pop-ins', 'ui-title'): 'FAIL', ('M9 entrance time', 'ui-title'): 'FAIL', ('M9 pop-ins', 'hit12'): 'FAIL',
    ('M10 text floor', 'ui-title'): 'FAIL', ('M11 roles', 'ui-title'): 'FAIL', ('M11 roles', 'kill12'): 'PASS',
}
EXPECT_SAMPLE = {
    ('M1 hit count', 'hit12'): 'info', ('M4 freeze', 'hit12'): 'PASS', ('M5 move', 'hit12'): 'n/a',
    ('M5 back', 'hit12'): 'n/a', ('M5 flash', 'hit12'): 'PASS', ('M10 text floor', 'hit12'): 'PASS',
    ('M11 roles', 'hit12'): 'PASS', ('M1 kill/hit', 'kill12'): 'n/a', ('M2 fx', 'kill12'): 'n/a',
}


def cmd_selftest(a):
    here = os.path.dirname(os.path.abspath(__file__))
    root = os.path.abspath(a.dir) if a.dir else tempfile.mkdtemp(prefix='feel-selftest-')
    made_root = not a.dir
    os.makedirs(root, exist_ok=True)
    fails = []

    def check(tag, rows, expect):
        idx = {r.key(): r for r in rows}
        for k, want in expect.items():
            got = idx.get(k)
            st = got.r.status if got else 'missing'
            ok = st == want
            print('  %-4s %-8s %-18s %-12s want %-4s got %s%s' % (
                'ok' if ok else 'BAD', tag, k[0], k[1], want, st,
                '' if ok else '  [%s | %s]' % (got.r.value if got else '', got.result() if got else '')))
            if not ok:
                fails.append((tag, k, want, st))

    roles_path = os.path.join(root, 'roles.json')
    with open(roles_path, 'w', encoding='utf-8') as fh:
        json.dump({'_doc': 'selftest', 'GOLD': ['HUD/Points', 'HUD/Pops/Pop'], 'DANGER': [], 'RIVAL': []}, fh)
    roles, _ = load_roles(roles_path)
    print('selftest in %s' % root)
    good, bad, base = (os.path.join(root, n) for n in ('good', 'bad', 'base'))
    for p, g in ((good, True), (bad, False), (base, True)):
        if not os.path.isdir(p):
            _gen_reel(p, g)
    # the hand-written sample, with flat grey frames
    sample = os.path.join(root, 'sample')
    sdir = os.path.join(sample, 'hit12')
    os.makedirs(sdir, exist_ok=True)
    shutil.copyfile(os.path.join(here, 'feel-sample', 'hit12', 'telemetry.jsonl'), os.path.join(sdir, 'telemetry.jsonl'))
    for f in range(5):
        Image.new('RGB', (1920, 1080), (128, 128, 128)).save(os.path.join(sdir, 'f%04d.jpg' % f), quality=90)
    # NIGHT21 M-MAGENTA: a flat grey frame has none; a 10 x 10 patch of (255, 0, 255) or of (240, 10, 240) is 100 px; a patch of
    # (200, 0, 200) (distance 78) and one of the dusk cyan is none; the row FAILs on the magenta frame and PASSes on the grey.
    def _mag(c, patch=True):
        im = Image.new('RGB', (1920, 1080), (128, 128, 128))
        if patch:
            im.paste(c, (100, 100, 110, 110))
        return im
    for tag, im, want in (('grey', _mag(None, False), 0), ('magenta', _mag((255, 0, 255)), 100),
                          ('near magenta', _mag((240, 10, 240)), 100), ('far (200, 0, 200)', _mag((200, 0, 200)), 0),
                          ('cyan', _mag((0, 255, 255)), 0)):
        got = magenta_pixels(im)
        print('  %-4s M-MAGENTA %-18s %d px (want %d)' % ('ok' if got == want else 'BAD', tag, got, want))
        if got != want:
            fails.append(('M-MAGENTA', tag, want, got))
    mroot = os.path.join(root, 'magenta-test')
    for sname, c in (('flat', None), ('square', (255, 0, 255))):
        sd = os.path.join(mroot, sname)
        os.makedirs(sd, exist_ok=True)
        with open(os.path.join(sd, 'telemetry.jsonl'), 'w', encoding='utf-8') as fh:
            fh.write(json.dumps({'scenario': sname, 'w': 1920, 'h': 1080}) + '\n')
            for f in range(3):
                fh.write(json.dumps({'f': f}) + '\n')
        for f in range(3):
            _mag(c, c is not None).save(os.path.join(sd, 'f%04d.jpg' % f), quality=95)
    mrows, _ = compute(Reel(mroot), {'roles': None, 'roles_err': 'selftest'})
    mi = {(r.label, r.scenario): r.r.status for r in mrows if r.label == 'M-MAGENTA'}
    for sname, want in (('flat', 'PASS'), ('square', 'FAIL')):
        got = mi.get(('M-MAGENTA', sname), 'missing')
        print('  %-4s M-MAGENTA row %-8s want %s got %s' % ('ok' if got == want else 'BAD', sname, want, got))
        if got != want:
            fails.append(('M-MAGENTA row', sname, want, got))
    # NIGHT22 T11 / LUM / STAMP-T: synthetic checks of the pure helpers (each seen both ways)
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
    _t('LUM flat thirds (switch off)', lum_monotonic([100.0, 100.0, 100.0]), False)
    _t('LUM thirds of 9', lum_thirds([9, 8, 7, 6, 5, 4, 3, 2, 1]), [8, 5, 2])
    ser_ok = [(i / 12.0 if i < 12 else 1.0, 1.0) for i in range(30)]   # 12 frames ramp at 60 fps = 200 ms
    ser_slow = [(i / 40.0 if i < 40 else 1.0, 1.0) for i in range(60)]  # 40 frames = 667 ms
    _t('STAMP-T 200 ms ramp in range', STAMP_MS[0] - 20 <= (stamp_settle_ms(ser_ok, 60) or 0) <= STAMP_MS[1], True)
    _t('STAMP-T 667 ms ramp out of range', STAMP_MS[0] <= (stamp_settle_ms(ser_slow, 60) or 0) <= STAMP_MS[1], False)
    ctx = {'roles': roles, 'roles_err': None}
    b_rows, (bh, _) = compute(Reel(base), dict(ctx))
    print('base hit12 count: %s' % bh)
    print('good reel:')
    g_rows, _ = compute(Reel(good), dict(ctx, denom=(bh, 'base', None)))
    check('good', g_rows, EXPECT_GOOD)
    print('bad reel:')
    x_rows, _ = compute(Reel(bad), dict(ctx, denom=(bh, 'base', None)))
    check('bad', x_rows, EXPECT_BAD)
    print('sample (Tools/feel-sample/hit12):')
    s_rows, _ = compute(Reel(sample), dict(ctx))
    check('sample', s_rows, EXPECT_SAMPLE)
    # the commands end to end
    ns = argparse.Namespace
    rc = cmd_metrics(ns(reel=good, base=base, prev=bad, out=os.path.join(root, 'FEEL-METRICS.md'), roles=roles_path))
    if rc != 0 or not os.path.isfile(os.path.join(root, 'FEEL-METRICS.md')):
        fails.append(('metrics', 'rc', 0, rc))
    rc = cmd_metrics(ns(reel=os.path.join(root, 'nope'), base=None, prev=None, out=None, roles=None))
    if rc != 2:
        fails.append(('metrics missing reel', 'rc', 2, rc))
    flip = os.path.join(root, 'flip')
    rc = cmd_flip(ns(reel=bad, out=flip))
    for fn, size in (('kill12.png', (1920, 960 + 2 * MARGIN)), ('ui-title-a.png', (1920, 1080 + 2 * MARGIN)),
                     ('ui-title-b.png', (1920, 1080 + 2 * MARGIN)), ('kill20.png', (1920, 960 + 2 * MARGIN))):
        p = os.path.join(flip, fn)
        got = Image.open(p).size if os.path.isfile(p) else None
        print('  %-4s flip %s %s' % ('ok' if got == size else 'BAD', fn, got))
        if got != size:
            fails.append(('flip', fn, size, got))
    sc = Reel(good).scenarios['kill12']
    picks = [f for f, _ in pick_frames(sc)]
    want = [18, 19, 21, 22, 29, 37, 46, 89]  # input, held 19-21 (f18 swallows 1 step unheld), end 22, +7/+15/+24
    print('  %-4s flip kill12 frames %s (want %s)' % ('ok' if picks == want else 'BAD', picks, want))
    if picks != want:
        fails.append(('flip picks', 'kill12', want, picks))
    ab = os.path.join(root, 'ab')
    key = os.path.join(root, 'ab-key.json')
    rc = cmd_ab(ns(reel_a=good, reel_b=bad, out=ab, key=key, seed=7, invert_key=None, force=True))
    imgs = sorted(f for f in os.listdir(ab) if f.endswith('.png'))
    n_want = 2 * 5 + 4 * 7  # 12 shared scenarios: 5 kill / hit (one sheet a side), 7 camera / ui (two a side)
    print('  %-4s ab: %d images (want %d), key %s, key outside: %s' % (
        'ok' if rc == 0 and len(imgs) == n_want else 'BAD', len(imgs), n_want, os.path.isfile(key),
        not os.path.exists(os.path.join(ab, os.path.basename(key)))))
    if rc != 0 or len(imgs) != n_want:
        fails.append(('ab', 'images', n_want, len(imgs)))
    leak = [f for f in os.listdir(ab) if f.endswith('.txt') and
            any(w in open(os.path.join(ab, f), encoding='utf-8').read() for w in ('good', 'bad', 'N17', 'N18'))]
    print('  %-4s ab: no side named in the neutral txt files %s' % ('ok' if not leak else 'BAD', leak))
    if leak:
        fails.append(('ab leak', leak, [], leak))
    ab2 = os.path.join(root, 'ab2')
    key2 = os.path.join(root, 'ab2-key.json')
    cmd_ab(ns(reel_a=good, reel_b=bad, out=ab2, key=key2, seed=None, invert_key=key, force=True))
    k1 = json.load(open(key, encoding='utf-8'))['scenarios']
    k2 = json.load(open(key2, encoding='utf-8'))['scenarios']
    inv_ok = all(k1[t]['A_is'] != k2[t]['A_is'] for t in k1)
    print('  %-4s ab --invert-key swaps every side' % ('ok' if inv_ok else 'BAD'))
    if not inv_ok:
        fails.append(('ab invert', '', True, False))
    gif = os.path.join(root, 'ab-kill12.gif')
    cmd_gif(ns(reel_a=bad, reel_b=good, scenario='kill12', out=gif, labels=None, step=2))
    try:
        with Image.open(gif) as g:
            nfr, dur = g.n_frames, 0
            for i in range(nfr):
                g.seek(i)
                dur += g.info.get('duration', 0)
            gsize = g.size
    except Exception as e:
        nfr, dur, gsize = 0, 0, str(e)
    real = (90 - 0) / 60.0
    print('  %-4s gif %s frames, %.2f s (real %.2f s), %s' % (
        'ok' if nfr > 0 and abs(dur / 1000.0 - real) < 0.05 else 'BAD', nfr, dur / 1000.0, real, gsize))
    if not (nfr > 0 and abs(dur / 1000.0 - real) < 0.05):
        fails.append(('gif', 'duration', real, dur / 1000.0))
    rc = cmd_roles(ns(reel=bad, roles=roles_path, suggest=False))
    if rc != 1:
        fails.append(('roles bad', 'rc', 1, rc))
    rc = cmd_roles(ns(reel=good, roles=roles_path, suggest=False))
    if rc != 0:
        fails.append(('roles good', 'rc', 0, rc))
    print('selftest: %s' % ('PASS' if not fails else 'FAIL: %d checks' % len(fails)))
    for f in fails:
        print('  ' + repr(f))
    if made_root and not a.keep and not fails:
        shutil.rmtree(root, ignore_errors=True)
    return 1 if fails else 0


# ---------------------------------------------------------------------------------------------------------------

def main(argv=None):
    try:
        sys.stdout.reconfigure(errors='replace')
        sys.stderr.reconfigure(errors='replace')
    except Exception:
        pass
    p = argparse.ArgumentParser(prog='feel-metrics.py', description='NIGHT18 feel reels: metrics, flipbooks, '
                                'blind A/B sheets, GIFs, colour roles (Tools/feel-schema.md).')
    sub = p.add_subparsers(dest='cmd', required=True)
    m = sub.add_parser('metrics', help='M1-M11 for one reel -> FEEL-METRICS.md')
    m.add_argument('reel')
    m.add_argument('--base', help="build 1's reel (M1's denominator and the Base column)")
    m.add_argument('--prev', help="the previous round's reel (the Prev column)")
    m.add_argument('--out', help='default: <reel>/../FEEL-METRICS.md (FEEL-METRICS-<side>.md unless the side is on)')
    m.add_argument('--roles', help='default: Tools/feel-roles.json')
    f = sub.add_parser('flip', help='8-frame flipbooks per scenario')
    f.add_argument('reel')
    f.add_argument('--out', help='default: <reel>/../flip (flip-<side> unless the side is on)')
    b = sub.add_parser('ab', help='blind A/B sheets for two reels')
    b.add_argument('reel_a')
    b.add_argument('reel_b')
    b.add_argument('--out', required=True, help='the neutral folder the reviewers read')
    b.add_argument('--key', required=True, help='the key file (only the lead reads it); not inside --out')
    b.add_argument('--seed', type=int)
    b.add_argument('--invert-key', help="a previous key: every side swapped (the second reviewer's order)")
    b.add_argument('--force', action='store_true', help='replace sNN files already in --out')
    b.add_argument('--only', help='NIGHT20: a comma list of scenarios (the judged set), in this order')
    g = sub.add_parser('gif', help='side-by-side animated GIF at real speed')
    g.add_argument('reel_a')
    g.add_argument('reel_b')
    g.add_argument('scenario')
    g.add_argument('--out')
    g.add_argument('--labels', nargs=2, metavar=('LEFT', 'RIGHT'), help='default: N17 N18 (reel A is the left)')
    g.add_argument('--step', type=int, default=2, help='keep every Nth 60 fps frame (default 2: 33 ms)')
    r = sub.add_parser('roles', help="M11's check alone")
    r.add_argument('reel')
    r.add_argument('--roles')
    r.add_argument('--suggest', action='store_true', help='print a feel-roles.json from the role users in the reel')
    s = sub.add_parser('selftest', help='synthetic reels + the sample through every command')
    s.add_argument('--dir', help='work here (kept); default: a temp folder, removed on PASS')
    s.add_argument('--keep', action='store_true')
    a = p.parse_args(argv)
    return {'metrics': cmd_metrics, 'flip': cmd_flip, 'ab': cmd_ab, 'gif': cmd_gif, 'roles': cmd_roles,
            'selftest': cmd_selftest}[a.cmd](a)


if __name__ == '__main__':
    sys.exit(main())
