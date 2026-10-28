# NIGHT56 builder V (slate V1 colour56, V2 rim56, V3 yellow56; NIGHT56.md section 5 V): THE COLOUR METER. Reads Player frames, no role-ID
# render pass (out of scope tonight): the regions come from the reel's own telemetry and sidecars.
#   python -I Tools/night/n56-colour.py <frame dir> [<frame dir> ...] [--config <unity-config.json>] [--kinds entry,mid] [--all-jpg] [--quiet]
#   python -I Tools/night/n56-colour.py planted          (synthetic frames that FAIL each check, and one that PASSES them all)
# A <frame dir> is one reel scenario folder (searched recursively for scenario folders):
#   - a356 (FeelReel.N56.Colour.cs): shot-N56-<level>-<room>-<kind>.png + its sidecar shot-N56-...json (the telemetry frame index "f", the
#     horizon row, the enemy rects at the capture) + telemetry.jsonl (the HUD's ui[] rects, the gun's fx rect, the camera);
#   - a452 (FeelReel.N52.Rooms.cs): shot-N29-K52-<level>-<room>-<kind>.png with no sidecar: each shot is matched to the telemetry by the
#     camera's still segments (the reel holds the camera at each frames52 spot: one segment of >= 20 still frames per shot, in the shots'
#     capture order (file mtime)); the entry / mid shot is the segment's last frame, the high shot 6 frames before it (KillAll + Frames(4));
#   - any reel with f%04d.jpg + telemetry.jsonl (verbs37 x5, for V3): each JPG with its own telemetry line.
# REGIONS per frame (image px, origin top-left, as the telemetry writes them):
#   hud     the union of ui[] rects (alpha > 0.05; a rect over 25 % of the frame is a full-screen overlay, not a HUD element: kept in)
#   enemies the union of enemies[] rects (FeelProbe.Enemy: every drawn husk's body + head bounds) and the sidecar's boss rects
#   gun     fx.gun (the viewmodel's rect)
#   sky     the rows above the horizon row: y_h = h/2 + (h/2) tan(pitch) / tan(fov/2) (the telemetry camera; the sidecar's when present);
#           with an a356 sidecar, only the cells of its ray grid that hit nothing (open sky: upper walls and ceilings stay world)
#   world   the frame minus hud, enemies, gun and sky
#   tells   the pixels inside the enemy / boss rects in the danger band (5-35 deg) or the parry band (38-70: the parry token is 44 deg)
#           at S >= 0.35, V >= 0.25
# CHECKS per frame (each printed with its number; PASS / FAIL against the slate's doneWhen):
#   V1 key    the level's KEY or ACCENT (design.n56.colour56.numbers.levels.<key>.keyHex / accentHex; hue +-20 deg) at S >= 0.35, V >= 0.08
#             outside hud / enemies / gun, as a share of the WHOLE frame: >= 3 %
#   V1 accent the accent class's own share (1-5 % is its budget) and its measured circular-mean hue; across the levels measured in one call,
#             the configured accents pairwise >= 40 deg apart (printed once)
#   V1 sky    sky pixels inside 5-70 deg at S > 0.35 (V >= 0.08), as a share of the frame: 0 % (the dusk-sky risk)
#   V1 sat    the world's saturation in non-role hues (outside 5-70, 170-200, 285-330), V >= 0.3: its p99.5 <= the tells' median S (no tells:
#             the role tokens' least S, 0.77, labelled "token")
#   V1 tint   whole-frame tint (the trap GG named, T-112): FAIL when >= 60 % of the world's pixels (V >= 0.08) sit within 25 deg of the key at
#             S >= 0.2, or the world's luma IQR < 0.06 (walls lost their value structure) (both thresholds are the meter's, not a source's)
#   V2 rim    each enemy in view (rect >= 12 x 12 px inside the frame, >= 15 % of its rect in the warm band 330-70 at S >= 0.25: else listed as
#             hidden). THE SHARPER CONTRACT (V.md: the original is unmeetable by any pill: the world's p90 S is 0.92-0.98 on NIGHT54's a452 mid
#             frames, set by the usable cyan and the hit wash): its BODY (the warm pixels of its rect) mean V and S above the ENVIRONMENT's p90 V and
#             p90 S (env = the world outside every role band); a frame passes when >= 90 % of the enemies in view pass. The original (the rect's
#             mean V and S over the world's p90 V and S, roles included) is printed beside it.
#   edge      the world and the sky leave out the 150 px screen-edge band (at 1080p) where the hit / low-health wash draws (critic-ggproxy #10);
#             its warm share is printed ("edge wash warm")
#   V3 yellow yellow (45-70 deg, S > 0.5, V >= 0.15) outside hud and the enemy / boss rects, as a share of the frame: <= 0.5 %
# Without a role-ID pass a parryable projectile outside an enemy rect counts as yellow (none is in an a452 frame; verbs37 frames list it).
import sys, os, re, json, glob, math
import numpy as np
from PIL import Image

DANGER = (5.0, 35.0)
PARRY_TELL = (38.0, 70.0)
YELLOW = (45.0, 70.0)
CYAN = (170.0, 200.0)
MAGENTA = (285.0, 330.0)
WARM = (330.0, 70.0)  # the enemies' family (wraps through 0)
KEY_TOL = 20.0
S_KEY, V_KEY = 0.35, 0.08
EDGE_PX = 150  # at 1080p: the screen-edge band where the hit / low-health wash draws (critic-ggproxy #10: 150-250 px deep); out of world / sky
TOKEN_S = 0.77  # the parry token #FFC93A's S (the least of the role tokens': danger 0.89, cyan 0.89, RIVAL 0.83)
SIDECAR_RE = re.compile(r"shot-N56-(.+)\.png$")
A452_RE = re.compile(r"shot-N29-K52-(n\d\d-[a-z]+)-([a-z0-9]+)-(entry|mid|high)\.png$")
A356_RE = re.compile(r"shot-N56-(n\d\d-[a-z]+)-([a-z0-9]+)-(entry|mid|high)\.png$")


def hsv(rgb):
    a = rgb.astype(np.float32) / 255.0
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    mx = a.max(axis=2)
    mn = a.min(axis=2)
    d = mx - mn
    s = np.where(mx > 1e-6, d / np.maximum(mx, 1e-6), 0.0)
    dd = np.maximum(d, 1e-6)
    h = np.where(r == mx, (g - b) / dd, np.where(g == mx, 2.0 + (b - r) / dd, 4.0 + (r - g) / dd))
    h = np.where(d > 1e-6, (h / 6.0) % 1.0 * 360.0, 0.0)
    lum = 0.2126 * r + 0.7152 * g + 0.0722 * b
    return h, s, mx, lum


def band(h, lo, hi):
    return (h >= lo) & (h <= hi) if lo <= hi else (h >= lo) | (h <= hi)


def near(h, hue, tol):
    d = np.abs(h - hue) % 360.0
    return np.minimum(d, 360.0 - d) <= tol


def gap(a, b):
    d = abs(a - b) % 360.0
    return 360.0 - d if d > 180.0 else d


def hex_hue(x):
    r, g, b = ((x >> 16) & 255) / 255.0, ((x >> 8) & 255) / 255.0, (x & 255) / 255.0
    mx, mn = max(r, g, b), min(r, g, b)
    d = mx - mn
    if d < 1e-9:
        return 0.0, 0.0, mx
    if mx == r:
        hh = ((g - b) / d) % 6
    elif mx == g:
        hh = (b - r) / d + 2
    else:
        hh = (r - g) / d + 4
    return hh * 60.0 % 360.0, d / mx, mx


def circ_mean(hs):
    if hs.size == 0:
        return float("nan")
    a = np.radians(hs)
    return float(np.degrees(math.atan2(np.sin(a).mean(), np.cos(a).mean())) % 360.0)


def level_key(level_id):
    m = re.match(r"n\d+-(.+)$", level_id or "")
    return m.group(1) if m else (level_id or "")


def load_palette(cfg_path):
    pal = {}
    try:
        with open(cfg_path, encoding="utf-8") as fh:
            cfg = json.load(fh)
        lv = cfg["design"]["n56"]["colour56"].get("numbers", {}).get("levels", {})
        for k, v in lv.items():
            if isinstance(v, dict) and "keyHex" in v and "accentHex" in v:
                pal[k] = (int(v["keyHex"]), int(v["accentHex"]))
    except (OSError, KeyError, ValueError):
        pass
    return pal


def rect_mask(mask, r, val=True):
    h, w = mask.shape
    x, y, rw, rh = r
    x0, y0 = max(0, int(math.floor(x))), max(0, int(math.floor(y)))
    x1, y1 = min(w, int(math.ceil(x + rw))), min(h, int(math.ceil(y + rh)))
    if x1 > x0 and y1 > y0:
        mask[y0:y1, x0:x1] = val
    return (x0, y0, x1, y1)


def horizon_row(h, cam):
    if not cam:
        return None
    try:
        p, fov = float(cam.get("pitch", 0)), float(cam.get("fov", 74))
    except (TypeError, ValueError):
        return None
    y = h / 2.0 + (h / 2.0) * math.tan(math.radians(p)) / math.tan(math.radians(fov / 2.0))
    return int(round(min(max(y, 0), h)))


def measure(rgb, meta, pal_entry, quiet=False):
    """One frame. meta: {enemies:[rect], bosses:[rect], hud:[rect], gun:rect|None, horizon:int|None}. pal_entry: (keyHue, accentHue) or None."""
    H, W = rgb.shape[0], rgb.shape[1]
    h, s, v, lum = hsv(rgb)
    hud = np.zeros((H, W), bool)
    for r in meta.get("hud", []):
        if r[2] * r[3] <= 0.25 * W * H:
            rect_mask(hud, r)
    en = np.zeros((H, W), bool)
    rects = list(meta.get("enemies", [])) + list(meta.get("bosses", []))
    for r in rects:
        rect_mask(en, r)
    gun = np.zeros((H, W), bool)
    if meta.get("gun"):
        rect_mask(gun, meta["gun"])
    sky = np.zeros((H, W), bool)
    hy = meta.get("horizon")
    if hy is not None and hy > 0:
        sky[:hy, :] = True
    grid = meta.get("skyGrid")
    if grid and grid.get("bits"):
        # the a356 sidecar's ray grid (FeelReel.N56.Colour.cs: one ray per cell through the world collider; '1' = no hit = open sky):
        # sky = above the horizon AND an open cell (an upper wall or a ceiling above the horizon is world, not sky)
        cols, rows, bits = int(grid["cols"]), int(grid["rows"]), grid["bits"]
        g = np.array([c == "1" for c in bits[:cols * rows]], bool).reshape(rows, cols)
        gy = np.minimum((np.arange(H) * rows) // H, rows - 1)
        gx = np.minimum((np.arange(W) * cols) // W, cols - 1)
        sky &= g[gy][:, gx]
    edge = np.zeros((H, W), bool)
    eb = int(round(EDGE_PX * H / 1080.0))
    if eb > 0:
        edge[:eb, :] = True
        edge[-eb:, :] = True
        edge[:, :eb] = True
        edge[:, -eb:] = True
    free = ~hud & ~en & ~gun
    world = free & ~sky & ~edge
    out = {}
    out["edgeWarmPct"] = 100.0 * (edge & free & band(h, 0.0, 40.0) & (s > 0.35) & (v >= 0.15)).sum() / max(1, (edge & free).sum())
    # tells
    tellm = en & (band(h, *DANGER) | band(h, *PARRY_TELL)) & (s >= 0.35) & (v >= 0.25)
    tell_s = float(np.median(s[tellm])) if tellm.sum() >= 30 else None
    out["tellS"] = tell_s if tell_s is not None else TOKEN_S
    out["tellSrc"] = "tells n %d" % int(tellm.sum()) if tell_s is not None else "token"
    # world saturation in non-role hues
    # round 2 (n56-r2): the danger family runs on to pure red: lava and the tells render at hue 350-5 after the grade (v1foff's S 1.00 pixels at
    # hue 0-10); with 5-35 only they counted as world. The cyan family runs on to 150 (colors.md's caution band: the usable strips' bloom and
    # chromatic-aberration fringes render at hue 150-165, S 1.0, on the kiln's ON and OFF frames alike). The role mask (here and for the
    # environment) takes 350-35 and 150-200; the sky check stays 5-70.
    role = band(h, 350.0, 360.0) | band(h, 0.0, 35.0) | band(h, *YELLOW) | band(h, 150.0, 200.0) | band(h, *MAGENTA) | band(h, 35.0, 45.0)
    wn = world & ~role & (v >= 0.30)  # S is noise on near-black pixels (a 5 / 10 / 30 RGB dark blue reads S 0.83)
    out["worldS995Hue"] = circ_mean(h[wn & (s >= np.percentile(s[wn], 99.5))]) if wn.sum() > 100 else float("nan")
    out["worldS995"] = float(np.percentile(s[wn], 99.5)) if wn.sum() > 100 else 0.0
    wv = world & (v >= 0.0)
    out["worldN"] = int(world.sum())
    out["worldV90"] = float(np.percentile(v[world], 90)) if world.sum() > 100 else 1.0
    out["worldS90"] = float(np.percentile(s[world], 90)) if world.sum() > 100 else 1.0
    env = world & ~role
    out["envV90"] = float(np.percentile(v[env], 90)) if env.sum() > 100 else 1.0
    out["envS90"] = float(np.percentile(s[env], 90)) if env.sum() > 100 else 1.0
    q75, q25 = (np.percentile(lum[world], [75, 25]) if world.sum() > 100 else (0, 0))
    out["worldLumIQR"] = float(q75 - q25)
    out["satOk"] = bool(out["worldS995"] <= out["tellS"])
    # late NIGHT56: the p99.5 of a small world (most of a dark kiln frame is under V 0.3) is set by 0.04-0.09 % of the frame (the cyan strips'
    # green fringes at hue 128-152, S 1.0, V 0.8 on n56-r4 kiln): the share of the FRAME the world puts over the tells' S is printed beside it
    out["worldOverTellsPct"] = 100.0 * (wn & (s > out["tellS"])).sum() / (W * H)
    # sky warm
    skyw = sky & free & ~edge & band(h, 5.0, 70.0) & (s > 0.35) & (v >= 0.08)
    out["skyWarmPct"] = 100.0 * skyw.sum() / (W * H)
    out["skyOk"] = bool(out["skyWarmPct"] < 0.005)  # "= 0 %" at the printed precision (2 decimals: under ~100 px of a 1080p frame; dither strays)
    # yellow outside parry tells and enemy / boss rects (hud out)
    yel = ~hud & ~en & band(h, *YELLOW) & (s > 0.5) & (v >= 0.15)
    out["yellowPct"] = 100.0 * yel.sum() / (W * H)
    out["yellowGunPct"] = 100.0 * (yel & gun).sum() / (W * H)
    out["yellowBrightPct"] = 100.0 * (yel & (v >= 0.5)).sum() / (W * H)  # printed beside: a parry tell is bright; dark ochre floors / olive walls are V 0.2-0.4
    # round 3 (n56-r3 spire v5-mid: 18.8 % "yellow" = two parryable orbs, emissive V 0.93, outside every enemy rect): with no role pass, a
    # bright (V >= 0.85) yellow blob of >= 1500 px that fills >= 55 % of its box (a round emissive orb, not a strip or a plate) is counted as
    # a PARRY TELL BY SHAPE and printed apart; "yellow outside tells" is the contract's share minus those blobs (both printed)
    orbs = 0
    orb_px = 0
    try:
        from scipy import ndimage
        lab, nl = ndimage.label(yel & (v >= 0.85))
        if nl:
            idx = np.arange(1, nl + 1)
            sizes = ndimage.sum(np.ones_like(lab), lab, idx)
            boxes = ndimage.find_objects(lab)
            for k, (sz, bx) in enumerate(zip(sizes, boxes)):
                if bx is None or sz < 1500:
                    continue
                bh, bw = bx[0].stop - bx[0].start, bx[1].stop - bx[1].start
                if sz >= 0.55 * bh * bw and 0.4 <= bw / max(1, bh) <= 2.5:
                    orbs += 1
                    orb_px += int(((lab[bx] == k + 1)).sum())
    except ImportError:
        pass
    out["yellowOrbs"] = orbs
    out["yellowNoOrbPct"] = 100.0 * (yel.sum() - orb_px) / (W * H)
    out["yellowOk"] = bool(out["yellowPct"] <= 0.5)
    # key / accent
    if pal_entry is not None:
        kh, ah = pal_entry
        sat = free & (s >= S_KEY) & (v >= V_KEY)
        km = sat & near(h, kh, KEY_TOL)
        am = sat & near(h, ah, KEY_TOL)
        out["keyPct"] = 100.0 * km.sum() / (W * H)
        out["accentPct"] = 100.0 * am.sum() / (W * H)
        out["keyOrAccentPct"] = 100.0 * (km | am).sum() / (W * H)
        # round 2, the sharper V1 sat contract (V.md): the KEY / ACCENT pixels themselves (V >= 0.3) under the tells' median S; the whole world's
        # p99.5 S (above) is printed too: on n56-r2 it reads 1.00 on the OFF sides of foundry / kiln / spire (decor55's green-brass, the kiln's
        # S 1.0 greens at ~143, the spire's emerald-lit walls at ~106), which colour56 does not draw
        ka = (km | am) & (v >= 0.30)
        out["keyAccS995"] = float(np.percentile(s[ka], 99.5)) if ka.sum() > 100 else 0.0
        out["ownSatOk"] = bool(out["keyAccS995"] <= out["tellS"])
        out["accentHue"] = circ_mean(h[am])
        out["keyHue"] = circ_mean(h[km])
        out["coverOk"] = bool(out["keyOrAccentPct"] >= 3.0)
        wv8 = world & (v >= 0.08)
        tint = wv8 & near(h, kh, 25.0) & (s >= 0.2)
        out["tintPct"] = 100.0 * tint.sum() / max(1, wv8.sum())
        out["tintOk"] = bool(out["tintPct"] < 60.0 and out["worldLumIQR"] >= 0.06)
    else:
        out["coverOk"] = None
        out["tintOk"] = bool(out["worldLumIQR"] >= 0.06)
    # the frame's saturated non-role hue peaks (what colour direction the frame has, palette or not)
    pk = free & ~role & (s >= S_KEY) & (v >= V_KEY)
    hist, edges = np.histogram(h[pk], bins=36, range=(0, 360))
    top = np.argsort(hist)[::-1][:3]
    out["peaks"] = ["%d-%d:%.1f%%" % (edges[i], edges[i + 1], 100.0 * hist[i] / (W * H)) for i in top if hist[i] > 0]
    # enemies
    ens = []
    for r in meta.get("enemies", []):
        m = np.zeros((H, W), bool)
        x0, y0, x1, y1 = rect_mask(m, r)
        if x1 - x0 < 12 or y1 - y0 < 12:
            continue
        rs, rv, rh = s[y0:y1, x0:x1], v[y0:y1, x0:x1], h[y0:y1, x0:x1]
        warm = band(rh, *WARM) & (rs >= 0.25) & (rv >= 0.12)
        share = float(warm.mean())
        e = {"rect": [x0, y0, x1 - x0, y1 - y0], "warm": share, "meanV": float(rv.mean()), "meanS": float(rs.mean()),
             "bodyV": float(rv[warm].mean()) if warm.any() else 0.0, "bodyS": float(rs[warm].mean()) if warm.any() else 0.0}
        e["inView"] = bool(share >= 0.15)
        e["ok"] = bool(e["meanV"] > out["worldV90"] and e["meanS"] > out["worldS90"])
        e["bodyOk"] = bool(e["bodyV"] > out["envV90"] and e["bodyS"] > out["envS90"])  # the sharper contract (V.md): the body vs the environment
        ens.append(e)
    vis = [e for e in ens if e["inView"]]
    out["enemies"] = ens
    out["enemiesInView"] = len(vis)
    out["enemiesOk"] = sum(1 for e in vis if e["ok"])
    out["enemiesBodyOk"] = sum(1 for e in vis if e["bodyOk"])
    out["rimOk"] = bool(out["enemiesBodyOk"] >= 0.9 * len(vis)) if vis else None  # the sharper contract; the original (rect vs world p90 with the roles) is printed beside it
    out["rimOrigOk"] = bool(out["enemiesOk"] >= 0.9 * len(vis)) if vis else None
    return out


def frieze_ab(on, off, meta, key_hue, tell_s):
    """a356's A/B: the frieze's exact pixels = the pixels that change by > 0.12 (largest channel) between the shot and its -nofrieze twin one frame
    later, sit within 25 deg of the key's hue on the shot at S >= 0.25, outside the HUD and the enemy rects (they move in a frame)."""
    H, W = on.shape[0], on.shape[1]
    if off.shape != on.shape:
        return {}
    d = np.abs(on.astype(np.int16) - off.astype(np.int16)).max(axis=2) / 255.0
    h, s, v, _ = hsv(on)
    m = (d > 0.12) & near(h, key_hue, 25.0) & (s >= 0.25)
    for r in meta.get("hud", []):
        if r[2] * r[3] <= 0.25 * W * H:
            rect_mask(m, r, False)
    for r in list(meta.get("enemies", [])) + list(meta.get("bosses", [])):
        rect_mask(m, r, False)
    n = int(m.sum())
    if n < 50:
        return {"friezePct": 100.0 * n / (W * H), "friezeHue": float("nan"), "friezeS50": 0.0, "friezeS995": 0.0, "friezeV50": 0.0, "friezeSatOk": None}
    s995 = float(np.percentile(s[m], 99.5))
    return {"friezePct": 100.0 * n / (W * H), "friezeHue": circ_mean(h[m]), "friezeS50": float(np.median(s[m])), "friezeS995": s995,
            "friezeV50": float(np.median(v[m])), "friezeSatOk": bool(s995 <= tell_s)}


# ------------------------------------------------------------------ reading the reel folders ------------------------------------------------
def read_telemetry(d):
    p = os.path.join(d, "telemetry.jsonl")
    if not os.path.exists(p):
        return None, []
    head, frames = None, []
    with open(p, encoding="utf-8") as fh:
        for line in fh:
            line = line.strip()
            if not line:
                continue
            try:
                o = json.loads(line)
            except ValueError:
                continue
            if o.get("type") == "scenario":
                head = o
            elif o.get("type") == "frame":
                frames.append(o)
    return head, frames


def meta_of(fr, h, extra=None):
    m = {"enemies": [], "bosses": [], "hud": [], "gun": None, "horizon": None}
    if fr is not None:
        m["enemies"] = [e["rect"] for e in fr.get("enemies", []) if isinstance(e, dict) and "rect" in e]
        m["hud"] = [u["rect"] for u in fr.get("ui", []) if isinstance(u, dict) and u.get("alpha", 1) > 0.05 and "rect" in u]
        g = (fr.get("fx") or {}).get("gun")
        if g:
            m["gun"] = g["rect"]
        m["horizon"] = horizon_row(h, fr.get("cam"))
    if extra:
        if extra.get("enemies") and not m["enemies"]:
            m["enemies"] = extra["enemies"]
        if extra.get("bosses"):
            m["bosses"] = extra["bosses"]
        if extra.get("horizonY") is not None:
            m["horizon"] = int(round(extra["horizonY"]))
        if extra.get("skyGrid"):
            m["skyGrid"] = extra["skyGrid"]
        if extra.get("hud") and not m["hud"]:
            m["hud"] = extra["hud"]
        if extra.get("gun") and not m["gun"]:
            m["gun"] = extra["gun"]
    return m


def still_segments(frames):
    segs, cur, start = [], None, 0
    for i, f in enumerate(frames):
        c = f.get("cam") or {}
        k = (c.get("x"), c.get("y"), c.get("z"))
        if k != cur:
            if cur is not None:
                segs.append((start, i - 1))
            cur, start = k, i
    if frames:
        segs.append((start, len(frames) - 1))
    return [sg for sg in segs if sg[1] - sg[0] + 1 >= 20 and sg[0] > 0]


def frame_list(d, kinds, all_jpg):
    """Yields (name, path, level, room, kind, meta-builder) for every frame of one scenario folder."""
    head, frames = read_telemetry(d)
    byf = {f["f"]: f for f in frames}
    pngs = [p for p in glob.glob(os.path.join(d, "shot-*.png"))]
    out = []
    a356 = [p for p in pngs if A356_RE.search(os.path.basename(p))]
    a452 = [p for p in pngs if A452_RE.search(os.path.basename(p))]
    for p in a356:
        lv, room, kind = A356_RE.search(os.path.basename(p)).groups()
        side = p[:-4] + ".json"
        extra = None
        if os.path.exists(side):
            with open(side, encoding="utf-8") as fh:
                extra = json.load(fh)
        fr = byf.get(extra.get("f")) if extra else None
        out.append((os.path.basename(p), p, lv, room, kind, fr, extra, "sidecar" if extra else "no sidecar"))
    if a452:
        a452.sort(key=os.path.getmtime)
        segs = still_segments(frames)
        for i, p in enumerate(a452):
            lv, room, kind = A452_RE.search(os.path.basename(p)).groups()
            fr = None
            how = "no telemetry"
            if i < len(segs) and len(segs) <= len(a452):
                s0, s1 = segs[i]
                fi = s1 if kind != "high" else max(s0, s1 - 6)
                fr = frames[fi]
                how = "segment %d-%d -> f%d" % (frames[s0]["f"], frames[s1]["f"], fr["f"])
                if s1 == len(frames) - 1:
                    how += ", the telemetry's last line (ProbeMaxFrames cut it: enemies approximate)"
            elif frames:
                how = "no segment (%d segments, %d shots: the telemetry ended; HUD / enemies unknown)" % (len(segs), len(a452))
            out.append((os.path.basename(p), p, lv, room, kind, fr, None, how))
    if not pngs or all_jpg:
        lv = (head or {}).get("map", "")
        for p in sorted(glob.glob(os.path.join(d, "f[0-9][0-9][0-9][0-9].jpg"))):
            fi = int(os.path.basename(p)[1:5])
            out.append((os.path.basename(p), p, "", "", "jpg", byf.get(fi), None, "telemetry f%d" % fi if fi in byf else "no line"))
    if kinds:
        out = [o for o in out if o[4] in kinds or o[4] == "jpg"]
    return out, head


def scenario_dirs(roots):
    ds = []
    for r in roots:
        for dp, dn, fn in os.walk(r):
            if any(f == "telemetry.jsonl" or (f.startswith("shot-") and f.endswith(".png")) for f in fn):
                ds.append(dp)
    return sorted(set(ds))


def reel_map(d):
    for p in (os.path.join(d, "..", "reel.json"), os.path.join(d, "reel.json")):
        if os.path.exists(p):
            try:
                with open(p, encoding="utf-8") as fh:
                    return json.load(fh).get("map", "")
            except ValueError:
                pass
    return ""


def fmt_ok(b):
    return "n/a" if b is None else ("PASS" if b else "FAIL")


def run(roots, cfg_path, kinds, all_jpg, quiet):
    pal = load_palette(cfg_path)
    pal_h = {k: (hex_hue(a)[0], hex_hue(b)[0]) for k, (a, b) in pal.items()}
    if pal_h:
        names = sorted(pal_h)
        acc = [(n, pal_h[n][1]) for n in names]
        mins = []
        for i in range(len(acc)):
            for j in range(i + 1, len(acc)):
                mins.append((gap(acc[i][1], acc[j][1]), acc[i][0], acc[j][0]))
        mins.sort()
        print("palette (%s): %s" % (cfg_path, "; ".join("%s key #%06x (%.0f) accent #%06x (%.0f)" % (n, pal[n][0], pal_h[n][0], pal[n][1], pal_h[n][1]) for n in names)))
        if mins:
            print("palette accents pairwise: least gap %.0f deg (%s / %s) -> %s (>= 40)" % (mins[0][0], mins[0][1], mins[0][2], "PASS" if mins[0][0] >= 40 else "FAIL"))
        for n in names:
            kh, ah = pal_h[n]
            rg = min(gap(kh, 12), gap(kh, 44), gap(kh, 186), gap(kh, 312))
            ra = min(gap(ah, 12), gap(ah, 44), gap(ah, 186), gap(ah, 312))
            def band_gap(x):
                best = 360.0
                for lo, hi in ((5, 35), (45, 70), (170, 200), (285, 330)):
                    if lo <= x <= hi:
                        return 0.0
                    best = min(best, gap(x, lo), gap(x, hi))
                return best
            print("  %s: key's least gap to a role token %.0f deg / to a role BAND %.0f deg; accent's %.0f / %.0f deg (tokens 12 / 44 / 186 / 312; bands danger 5-35, yellow 45-70, cyan 170-200, magenta 285-330: 0 = inside one)" % (n, rg, band_gap(kh), ra, band_gap(ah)))
    else:
        print("palette: none in %s (design.n56.colour56.numbers.levels.<key>.keyHex / accentHex): V1 key / accent not measured" % cfg_path)
    total = {"frames": 0, "cover": [0, 0], "sky": [0, 0], "sat": [0, 0], "tint": [0, 0], "yellow": [0, 0], "rim": [0, 0], "rimOrig": [0, 0], "ownSat": [0, 0], "enIn": 0, "enOk": 0, "enBodyOk": 0}
    per_level = {}
    worst_y = (0, "")
    for d in scenario_dirs(roots):
        items, head = frame_list(d, kinds, all_jpg)
        if not items:
            continue
        mp = reel_map(d)
        print("== %s (%d frames%s)" % (d, len(items), ", map " + mp if mp else ""))
        ylist = []
        for name, path, lv, room, kind, fr, extra, how in sorted(items, key=lambda o: o[0]):
            rgb = np.asarray(Image.open(path).convert("RGB"))
            meta = meta_of(fr, rgb.shape[0], extra)
            key = level_key(lv or mp)
            pe = pal_h.get(key)
            o = measure(rgb, meta, pe, quiet)
            nf = path[:-4] + "-nofrieze.png"
            if os.path.exists(nf) and pe is not None:
                o.update(frieze_ab(rgb, np.asarray(Image.open(nf).convert("RGB")), meta, pe[0], o["tellS"]))
            # NIGHT57 V (a357): colour56's world saturation cap's A/B: the same view one frame later with the cap hidden (shot-...-nocap.png):
            # the cap's exact pixels by the difference (outside the HUD and the gun, which the overlay draws after the pass), and the sat check
            # on the twin (the frame as NIGHT56's colour56 drew it) beside the shot's
            nc = path[:-4] + "-nocap.png"
            if os.path.exists(nc):
                twin = np.asarray(Image.open(nc).convert("RGB"))
                if twin.shape == rgb.shape:
                    o2 = measure(twin, meta, pe, quiet)
                    dd = np.abs(rgb.astype(np.int16) - twin.astype(np.int16)).max(axis=2) > 3
                    hh, ss, vv, _ = hsv(rgb)
                    h2, s2, v2, _ = hsv(twin)
                    o["capMovedPct"] = 100.0 * dd.sum() / dd.size
                    o["capSatS0"] = float(np.median(s2[dd])) if dd.sum() > 50 else 0.0
                    o["capSatS1"] = float(np.median(ss[dd])) if dd.sum() > 50 else 0.0
                    o["capHueShift"] = float(np.median(np.minimum(np.abs(hh[dd] - h2[dd]) % 360.0, 360.0 - np.abs(hh[dd] - h2[dd]) % 360.0))) if dd.sum() > 50 else 0.0
                    o["capVShift"] = float(np.median(vv[dd] - v2[dd])) if dd.sum() > 50 else 0.0
                    o["noCapSatOk"] = o2["satOk"]
                    o["noCapOver"] = o2["worldOverTellsPct"]
                    total.setdefault("noCapSat", [0, 0])
                    total["noCapSat"][0] += 1 if o2["satOk"] else 0
                    total["noCapSat"][1] += 1
            total["frames"] += 1
            for k, ok in (("cover", o["coverOk"]), ("sky", o["skyOk"]), ("sat", o["satOk"]), ("tint", o["tintOk"]), ("yellow", o["yellowOk"]), ("rim", o["rimOk"]), ("rimOrig", o["rimOrigOk"]), ("ownSat", o.get("ownSatOk"))):
                if ok is None:
                    continue
                total[k][0] += 1 if ok else 0
                total[k][1] += 1
            total["enIn"] += o["enemiesInView"]
            total["enOk"] += o["enemiesOk"]
            total["enBodyOk"] += o["enemiesBodyOk"]
            ylist.append(o["yellowPct"])
            if o["yellowPct"] > worst_y[0]:
                worst_y = (o["yellowPct"], path)
            if kind != "jpg":
                L = per_level.setdefault(key, {"n": 0, "cover": [], "acc": [], "accHue": [], "sky": 0, "sat": 0, "tint": 0})
                L["n"] += 1
                if "keyOrAccentPct" in o:
                    L["cover"].append(o["keyOrAccentPct"])
                    L["acc"].append(o["accentPct"])
                    if not math.isnan(o["accentHue"]):
                        L["accHue"].append(o["accentHue"])
                L["sky"] += 0 if o["skyOk"] else 1
                L["sat"] += 0 if o["satOk"] else 1
                L["tint"] += 0 if o["tintOk"] else 1
            if quiet and kind == "jpg":
                continue
            line = "  %s [%s]" % (name, how)
            if "keyPct" in o:
                line += " | V1 key %.1f%% accent %.1f%% (hue %s) key|accent %.1f%% %s" % (o["keyPct"], o["accentPct"], "-" if math.isnan(o["accentHue"]) else "%.0f" % o["accentHue"], o["keyOrAccentPct"], fmt_ok(o["coverOk"]))
                line += " | key|accent S p99.5 %.2f vs tells %.2f %s" % (o["keyAccS995"], o["tellS"], fmt_ok(o["ownSatOk"]))
                if "friezePct" in o:
                    line += " | FRIEZE (exact, A/B) %.1f%% of the frame, hue %s, S median %.2f p99.5 %.2f (vs tells %.2f: %s), V median %.2f" % (
                        o["friezePct"], "-" if math.isnan(o["friezeHue"]) else "%.0f" % o["friezeHue"], o["friezeS50"], o["friezeS995"], o["tellS"],
                        fmt_ok(o["friezeSatOk"]), o["friezeV50"])
                line += " | tint %.0f%% of world, luma IQR %.3f %s" % (o["tintPct"], o["worldLumIQR"], fmt_ok(o["tintOk"]))
            else:
                line += " | luma IQR %.3f" % o["worldLumIQR"]
            if "capMovedPct" in o:
                line += " | CAP A/B (the -nocap twin): moved %.1f%% of the frame (S median %.2f -> %.2f, hue shift median %.1f deg, V shift %.3f); sat without the cap %s (world over the tells %.2f%%) -> with it %s" % (
                    o["capMovedPct"], o["capSatS0"], o["capSatS1"], o["capHueShift"], o["capVShift"], fmt_ok(o["noCapSatOk"]), o["noCapOver"], fmt_ok(o["satOk"]))
            line += " | sky warm %.2f%% %s | world S p99.5 %.2f (its hue %s) vs tells %.2f (%s) %s, world over the tells %.2f%% of the frame" % (o["skyWarmPct"], fmt_ok(o["skyOk"]), o["worldS995"], "-" if math.isnan(o["worldS995Hue"]) else "%.0f" % o["worldS995Hue"], o["tellS"], o["tellSrc"], fmt_ok(o["satOk"]), o["worldOverTellsPct"])
            line += " | V3 yellow %.2f%% (gun %.2f%%, bright V >= 0.5 %.2f%%) %s; parry orbs by shape %d, yellow outside them %.2f%% %s" % (
                o["yellowPct"], o["yellowGunPct"], o["yellowBrightPct"], fmt_ok(o["yellowOk"]), o["yellowOrbs"], o["yellowNoOrbPct"], fmt_ok(o["yellowNoOrbPct"] <= 0.5))
            line += " | V2 enemies in view %d: body over env p90 (V %.2f S %.2f) %d %s; original rect over world p90 (V %.2f S %.2f) %d %s" % (o["enemiesInView"], o["envV90"], o["envS90"], o["enemiesBodyOk"], fmt_ok(o["rimOk"]), o["worldV90"], o["worldS90"], o["enemiesOk"], fmt_ok(o["rimOrigOk"]))
            line += " | edge wash warm %.0f%%" % o["edgeWarmPct"]
            line += " | peaks " + ",".join(o["peaks"])
            print(line)
            if not quiet:
                for e in o["enemies"]:
                    print("      enemy rect %s warm %.0f%% %s: body V %.2f S %.2f -> %s; rect mean V %.2f S %.2f -> %s" % (e["rect"], 100 * e["warm"], "in view" if e["inView"] else "hidden?", e["bodyV"], e["bodyS"], "over" if e["bodyOk"] else "under", e["meanV"], e["meanS"], "over" if e["ok"] else "under"))
        if ylist:
            ya = np.array(ylist)
            print("  yellow per frame: median %.2f%%, max %.2f%%, frames over 0.5%%: %d / %d" % (float(np.median(ya)), float(ya.max()), int((ya > 0.5).sum()), len(ya)))
    print("== per level (shots)")
    for k in sorted(per_level):
        L = per_level[k]
        cov = ("cover median %.1f%% (min %.1f%%), accent median %.1f%%, accent hue %s" % (float(np.median(L["cover"])), min(L["cover"]), float(np.median(L["acc"])), "%.0f" % circ_mean(np.array(L["accHue"])) if L["accHue"] else "-")) if L["cover"] else "no palette"
        print("  %s: %d shots; %s; sky FAIL %d, sat FAIL %d, tint FAIL %d" % (k or "?", L["n"], cov, L["sky"], L["sat"], L["tint"]))
    def r(k):
        return "%d/%d" % tuple(total[k])
    print("TOTAL frames %d | V1 cover %s, sky %s, sat (whole world) %s, key|accent sat %s, tint %s | V3 yellow %s (worst %.2f%% %s) | V2 frames %s, enemies in view %d: body over env p90 %d (%.0f%%); original: frames %s, rect over world p90 %d (%.0f%%)"
          % (total["frames"], r("cover"), r("sky"), r("sat"), r("ownSat"), r("tint"), r("yellow"), worst_y[0], os.path.basename(worst_y[1]) if worst_y[1] else "-", r("rim"), total["enIn"], total["enBodyOk"],
             100.0 * total["enBodyOk"] / max(1, total["enIn"]), r("rimOrig"), total["enOk"], 100.0 * total["enOk"] / max(1, total["enIn"])))
    if "noCapSat" in total:
        print("CAP A/B (NIGHT57 V1 GUARD, a357's -nocap twins): world out-saturates the tells on %d / %d frames without the cap -> %d / %d with it (sat FAIL counts)"
              % (total["noCapSat"][1] - total["noCapSat"][0], total["noCapSat"][1], total["sat"][1] - total["sat"][0], total["sat"][1]))
    print("V1 GUARD count: world out-saturates the tells (sat FAIL) on %d / %d frames" % (total["sat"][1] - total["sat"][0], total["sat"][1]))
    return total


# ------------------------------------------------------------------ planted cases ------------------------------------------------------------
def hsv_px(hue, s, v):
    import colorsys
    r, g, b = colorsys.hsv_to_rgb((hue % 360) / 360.0, s, v)
    return np.array([r * 255, g * 255, b * 255], np.uint8)


def base_frame(W=480, H=270):
    """A world with value structure: dark neutral walls in bands of different value (S 0.1), a floor, a sky row above y 100."""
    img = np.zeros((H, W, 3), np.uint8)
    for x in range(0, W, 40):
        img[:, x:x + 40] = hsv_px(220, 0.10, 0.12 + 0.55 * ((x // 40) % 3) / 2)
    img[200:, :] = hsv_px(30, 0.08, 0.22)
    img[:100, :] = hsv_px(250, 0.15, 0.25)  # a cool dusk sky above the horizon (y 100)
    return img


def planted():
    pal = (132.0, 265.0)
    W, H = 480, 270
    en_rect = [200, 120, 40, 60]
    ok_meta = {"enemies": [en_rect], "bosses": [], "hud": [[10, 10, 80, 20]], "gun": [360, 180, 120, 90], "horizon": 100}
    cases = []

    def good():
        img = base_frame(W, H)
        img[105:118, :] = hsv_px(132, 0.5, 0.55)  # the key: a band on the walls (13 rows = 4.8 % of the frame)
        img[150:156, 0:300] = hsv_px(265, 0.55, 0.5)  # the accent: a string course
        img[120:180, 200:240] = hsv_px(12, 0.9, 0.95)  # the enemy: bright, saturated red-orange
        img[60:80, 220:230] = hsv_px(250, 0.10, 0.9)
        return img

    cases.append(("PASS all", good(), ok_meta, {"coverOk": True, "skyOk": True, "satOk": True, "tintOk": True, "yellowOk": True, "rimOk": True}))
    img = good()
    img[105:118, :] = hsv_px(220, 0.10, 0.4)
    img[150:156, 0:300] = hsv_px(220, 0.10, 0.4)
    cases.append(("FAIL cover (no key, no accent: grey)", img, ok_meta, {"coverOk": False}))
    img = good()
    img[0:60, :] = hsv_px(25, 0.6, 0.7)
    cases.append(("FAIL sky (an orange dusk band above the horizon)", img, ok_meta, {"skyOk": False}))
    img = good()
    img[150:156, 0:300] = hsv_px(265, 1.0, 0.95)
    img[120:180, 200:240] = hsv_px(12, 0.6, 0.95)
    cases.append(("FAIL sat (the accent S 1.0 over the tells' S 0.6)", img, ok_meta, {"satOk": False}))
    img = good()
    img[100:200, :] = hsv_px(132, 0.45, 0.4)
    img[120:180, 200:240] = hsv_px(12, 0.9, 0.95)
    cases.append(("FAIL tint (the key on every wall, flat value)", img, ok_meta, {"tintOk": False}))
    img = good()
    img[210:240, 40:160] = hsv_px(55, 0.8, 0.85)
    cases.append(("FAIL yellow (a yellow floor trim 2.8 % of the frame)", img, ok_meta, {"yellowOk": False}))
    img = good()
    img[120:180, 200:240] = hsv_px(12, 0.5, 0.2)
    cases.append(("FAIL rim (a dim enemy under the world's p90)", img, ok_meta, {"rimOk": False}))
    bad = 0
    for name, img, meta, want in cases:
        o = measure(img, meta, pal, True)
        got = {k: o[k] for k in want}
        hit = all(got[k] == want[k] for k in want)
        bad += 0 if hit else 1
        print("planted %-56s -> %s | cover %s sky %s sat %s tint %s yellow %s rim %s | key|accent %.1f%%, sky warm %.2f%%, world S %.2f vs tells %.2f, tint %.0f%% IQR %.3f, yellow %.2f%%, enemies %d/%d"
              % (name, "as planted" if hit else "WRONG (" + json.dumps(got) + ")", fmt_ok(o["coverOk"]), fmt_ok(o["skyOk"]), fmt_ok(o["satOk"]), fmt_ok(o["tintOk"]), fmt_ok(o["yellowOk"]), fmt_ok(o["rimOk"]),
                 o.get("keyOrAccentPct", -1), o["skyWarmPct"], o["worldS995"], o["tellS"], o.get("tintPct", -1), o["worldLumIQR"], o["yellowPct"], o["enemiesOk"], o["enemiesInView"]))
    # the palette's pairwise accent check, planted: two accents 20 deg apart
    a, b = 120.0, 140.0
    print("planted accents %.0f / %.0f: gap %.0f -> %s (as planted: FAIL)" % (a, b, gap(a, b), "PASS" if gap(a, b) >= 40 else "FAIL"))
    print("planted: %d / %d cases as planted" % (len(cases) - bad, len(cases)))
    return bad


def main():
    args = sys.argv[1:]
    if args and args[0] == "planted":
        sys.exit(1 if planted() else 0)
    cfg = "Design/unity-config.json"
    kinds = None
    all_jpg = quiet = False
    if "--config" in args:
        i = args.index("--config"); cfg = args[i + 1]; del args[i:i + 2]
    if "--kinds" in args:
        i = args.index("--kinds"); kinds = set(args[i + 1].split(",")); del args[i:i + 2]
    if "--all-jpg" in args:
        args.remove("--all-jpg"); all_jpg = True
    if "--quiet" in args:
        args.remove("--quiet"); quiet = True
    if not args:
        print(__doc__ or "usage: n56-colour.py <frame dir> ... | planted")
        sys.exit(2)
    run(args, cfg, kinds, all_jpg, quiet)


if __name__ == "__main__":
    main()
