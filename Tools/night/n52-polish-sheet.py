# NIGHT52 builder U, round 10: GG's before / after sheet for the polish lane (hud52, skin52, guns52, look52) from reel frames already on disk
# (no Player). One row per item; in each row its pairs left to right, OFF | ON, each tile labelled with the tag / run it came from.
#   python Tools/night/n52-polish-sheet.py [newest-tag] [--out <png>]
#   e.g. python Tools/night/n52-polish-sheet.py n52-r15
# The newest tag defaults to the highest n52-rN under Logs/feel. Each pair comes from the NEWEST tag from n52-r8 up to that one that holds both
# of its frames (OFF and ON from the same tag and the same map). OFF is NIGHT51's look: the -skygrindOff52 run's frame (the row off), read
# from the tag's summary47.txt run headers ("== <run> <time> (map <map>, scenario <sc> [-skygrindOff52]; ..."). A pair found nowhere is a
# grey tile marked MISSING, and the console lists every pair with its source. Frames are read by absolute path in the main checkout.
# Writes C:/Users/gaige/dev/SKYGRIND-unity/Logs/n52/sheet/polish.png (Logs is ignored: the PNG is not committed).
import os
import re
import sys
from PIL import Image, ImageDraw, ImageFont

ROOT = 'C:/Users/gaige/dev/SKYGRIND-unity/'
FEEL = ROOT + 'Logs/feel/'
OUT = ROOT + 'Logs/n52/sheet/polish.png'
FIRST_TAG = 8  # n52-r8: the first tag with the U rows' frames

TILE_H = 300  # px: every tile's height (a frame fits inside TILE_W_MAX x TILE_H, letterboxed)
TILE_W_MAX = 534  # px: a 16:9 frame's tile width at TILE_H
MAX_PAIRS = 3  # pairs per line in a row (a longer row wraps under itself)
LABEL_W = 300  # px: the row's label column
PAD = 12  # px between tiles and round the sheet
PAIR_GAP = 28  # px between two pairs in a row
CAP_H = 30  # px: a tile's caption band (above the frame)
BG = (12, 10, 20)
PANEL = (24, 21, 36)
TEXT = (232, 230, 240)
DIM = (150, 146, 168)
OFF_C = (190, 186, 200)  # the OFF caption: bone grey
ON_C = (25, 211, 230)  # the ON caption: the game's usable cyan
MISS_C = (255, 74, 28)  # a missing pair's mark: the danger token

# HUD crops (1920 x 1080 frames; image rows top-down): the health panel (bottom left) and the lesson card (top middle)
CROP_HP = (20, 820, 580, 1080)  # hud52's health panel sits at x 105-453, y 966-1038 (a152's line)
CROP_CARD = (600, 80, 1320, 300)  # the teach43 card (n52-r12's frames: x ~720-1180, y ~130-200 at full size)
CROP_TOP = (0, 0, 1920, 240)  # round 11: the top band (the counter top-left, the room title top-centre, the score top-right)

# (label, OFF spec, ON spec); a spec = (scenario, file name, crop or None, map preference or None). OFF specs are read from the
# -skygrindOff52 run, ON specs from the run without it, on the same map.
ITEMS = [
    ('hud52', 'the health and charge block; the lesson card', [
        ('health 60 %', ('a152', 'shot-N30-U52-hp60-off52.png', CROP_HP, None), ('a152', 'shot-N30-U52-hp60-on.png', CROP_HP, None)),
        ('health 29 % (low)', ('a152', 'shot-N30-U52-hp29-off52.png', CROP_HP, None), ('a152', 'shot-N30-U52-hp29-on.png', CROP_HP, None)),
        ('lesson card at 8 s (the chip)', ('a152', 'shot-N30-U52-teach-8s-off52.png', CROP_CARD, None), ('a152', 'shot-N30-U52-teach-8s-on.png', CROP_CARD, None)),
        # round 11: the counter, the room title and the score (a152's top-<side> frames, from n52-r16 on)
        ('the top: counter, title, score', ('a152', 'shot-N30-U52-top-off52.png', CROP_TOP, None), ('a152', 'shot-N30-U52-top-on.png', CROP_TOP, None)),
    ]),
    ('skin52', 'the enemy surface (two tones, the near rim cut)', [
        ('husk at 2.5 m', ('a252', 'shot-N30-S52-husk-near-off52-crop.png', None, ['n47-kiln', 'n47-spire']), ('a252', 'shot-N30-S52-husk-near-on-crop.png', None, ['n47-kiln', 'n47-spire'])),
        # the scorch core does not ship (design.n52.skin52.numbers.scorchGlow 0 after n52-r15: NIGHT51's mark), so no scorch pair
    ]),
    ('guns52', 'the viewmodels, idle', [
        ('KESTREL', ('a352', 'shot-N30-G52-kestrel-idle-off52-crop.png', None, None), ('a352', 'shot-N30-G52-kestrel-idle-on-crop.png', None, None)),
        ('THUNDERCLAP', ('a352', 'shot-N30-G52-thunderclap-idle-off52-crop.png', None, None), ('a352', 'shot-N30-G52-thunderclap-idle-on-crop.png', None, None)),
    ]),
    ('look52', 'the pickup, the grapple, the levels', [
        ('health pickup at 15 m', ('a252', 'shot-N30-S52-pickup15-off52-crop.png', None, ['n47-kiln', 'n47-spire']), ('a252', 'shot-N30-S52-pickup15-on-crop.png', None, ['n47-kiln', 'n47-spire'])),
        ('grapple ring and rope', ('a252', 'shot-N30-S52-ring-off52-crop.png', None, ['n47-spire', 'n47-span']), ('a252', 'shot-N30-S52-ring-on-crop.png', None, ['n47-spire', 'n47-span'])),
        # what ships only (the lead after n52-r15): the tiers on foundry (spire's left the list), no iceline / span grade (gradeLevels [])
        ('foundry tiers (room)', ('a349', 'shot-N49-V-n52-foundry-room-on.png', None, None), ('a349', 'shot-N49-V-n52-foundry-room-on.png', None, None)),
    ]),
]

HEAD = re.compile(r'^== (\S+) \S+ \(map (\S+), scenario (\S+)([^;]*);')


def font(size, bold=True):
    names = ['Assets/Fonts/ChakraPetch/ChakraPetch-SemiBold.ttf', 'Assets/Fonts/BarlowSemiCondensed/BarlowSemiCondensed-SemiBold.ttf'] if bold \
        else ['Assets/Fonts/BarlowSemiCondensed/BarlowSemiCondensed-SemiBold.ttf']
    for n in names:
        try:
            return ImageFont.truetype(ROOT + n, size)
        except OSError:
            continue
    return ImageFont.load_default()


def tags_upto(newest):
    have = []
    for d in os.listdir(FEEL):
        m = re.fullmatch(r'n52-r(\d+)', d)
        if m and os.path.isdir(FEEL + d):
            have.append(int(m.group(1)))
    top = max(have) if newest is None else int(re.fullmatch(r'n52-r(\d+)', newest).group(1))
    return ['n52-r%d' % n for n in sorted(have, reverse=True) if FIRST_TAG <= n <= top]


def runs_of(tag):
    """The tag's runs from its summary47.txt headers: (run, map, scenario, off52)."""
    out = []
    p = FEEL + tag + '/summary47.txt'
    if not os.path.isfile(p):
        return out
    with open(p, encoding='utf-8', errors='replace') as f:
        for line in f:
            m = HEAD.match(line)
            if m:
                out.append((m.group(1), m.group(2), m.group(3), '-skygrindOff52' in m.group(4)))
    return out


def frame_path(tag, run, scenario, name):
    p = FEEL + '%s/%s/%s/%s' % (tag, run, scenario, name)
    return p if os.path.isfile(p) else None


def find_pair(tags, off, on):
    """The newest tag whose -skygrindOff52 run and plain run (same scenario and map) hold the OFF and ON frames."""
    for tag in tags:
        runs = runs_of(tag)
        maps = []
        for r in runs:
            if r[2] == on[0] and r[1] not in maps:
                maps.append(r[1])
        pref = on[3]
        if pref:
            maps.sort(key=lambda m: pref.index(m) if m in pref else len(pref))
        for mp in maps:
            pon = poff = None
            ron = roff = None
            for run, rmap, sc, o52 in runs:
                if rmap != mp:
                    continue
                if sc == on[0] and not o52 and pon is None:
                    pon = frame_path(tag, run, sc, on[1])
                    ron = run if pon else None
                if sc == off[0] and o52 and poff is None:
                    poff = frame_path(tag, run, sc, off[1])
                    roff = run if poff else None
            if pon and poff:
                return tag, mp, (poff, roff), (pon, ron)
    return None


def tile(path, crop):
    im = Image.open(path).convert('RGB')
    if crop:
        im = im.crop(crop)
    w, h = im.size
    k = min(TILE_W_MAX / w, TILE_H / h)
    im = im.resize((max(1, round(w * k)), max(1, round(h * k))), Image.LANCZOS)
    tw = max(TILE_H, im.size[0])  # a square crop keeps a square tile; a wide one up to TILE_W_MAX
    t = Image.new('RGB', (tw, TILE_H), PANEL)
    t.paste(im, ((tw - im.size[0]) // 2, (TILE_H - im.size[1]) // 2))
    return t


def missing_tile(name):
    t = Image.new('RGB', (TILE_H, TILE_H), (40, 38, 48))
    d = ImageDraw.Draw(t)
    d.text((16, TILE_H // 2 - 40), 'MISSING', font=font(28), fill=MISS_C)
    words, line, y = name.split('-'), '', TILE_H // 2
    for w in words:  # the file name, wrapped at its dashes
        if len(line) + len(w) > 26:
            d.text((16, y), line, font=font(18, False), fill=DIM)
            y += 22
            line = ''
        line += ('-' if line else '') + w
    d.text((16, y), line, font=font(18, False), fill=DIM)
    return t


def main():
    args = sys.argv[1:]
    out = OUT
    if '--out' in args:
        i = args.index('--out')
        out = args[i + 1]
        del args[i:i + 2]
    tags = tags_upto(args[0] if args else None)
    if not tags:
        print('no n52-rN tag from n52-r%d under %s' % (FIRST_TAG, FEEL))
        return 1
    print('tags searched (newest first): %s' % ', '.join(tags))
    f_item, f_sub, f_cap = font(30), font(20, False), font(18, False)
    rows, filled, missing = [], 0, 0
    for item, sub, pairs in ITEMS:
        cells = []
        for label, off, on in pairs:
            hit = find_pair(tags, off, on)
            if hit:
                tag, mp, (poff, roff), (pon, ron) = hit
                cells.append((label, tile(poff, off[2]), tile(pon, on[2]), '%s %s/%s' % (mp, tag, roff), '%s/%s' % (tag, ron)))
                print('%-7s %-30s OFF %s  |  ON %s' % (item, label, poff.replace(FEEL, ''), pon.replace(FEEL, '')))
                filled += 1
            else:
                cells.append((label, missing_tile(off[1]), missing_tile(on[1]), 'not found', 'not found'))
                print('%-7s %-30s MISSING (%s | %s in n52-r%d..%s)' % (item, label, off[1], on[1], FIRST_TAG, tags[0]))
                missing += 1
        lines = [cells[i:i + MAX_PAIRS] for i in range(0, len(cells), MAX_PAIRS)]
        line_h = CAP_H * 2 + TILE_H + PAD
        w = max(LABEL_W + sum(a.size[0] + b.size[0] + PAD for _, a, b, _, _ in ln) + PAIR_GAP * (len(ln) - 1) + PAD for ln in lines)
        row = Image.new('RGB', (w, line_h * len(lines)), BG)
        d = ImageDraw.Draw(row)
        d.text((PAD, PAD), item, font=f_item, fill=TEXT)
        y = PAD + 44
        for part in sub.split('; '):
            d.text((PAD, y), part, font=f_sub, fill=DIM)
            y += 26
        for li, ln in enumerate(lines):
            x, y0 = LABEL_W, li * line_h
            for label, a, b, ca, cb in ln:
                d.text((x, y0 + 4), label, font=f_sub, fill=TEXT)
                d.text((x, y0 + CAP_H), 'OFF  ' + ca, font=f_cap, fill=OFF_C)
                d.text((x + a.size[0] + PAD, y0 + CAP_H), 'ON  ' + cb, font=f_cap, fill=ON_C)
                row.paste(a, (x, y0 + CAP_H * 2))
                row.paste(b, (x + a.size[0] + PAD, y0 + CAP_H * 2))
                x += a.size[0] + b.size[0] + PAD + PAIR_GAP
        rows.append(row)
    W = max(r.size[0] for r in rows)
    title_h = 64
    sheet = Image.new('RGB', (W, title_h + sum(r.size[1] + PAD for r in rows)), BG)
    d = ImageDraw.Draw(sheet)
    d.text((PAD, 14), 'NIGHT52 polish lane: OFF (NIGHT51, -skygrindOff52) | ON, newest tag up to %s per pair' % tags[0], font=font(28), fill=TEXT)
    y = title_h
    for r in rows:
        sheet.paste(r, (0, y))
        y += r.size[1] + PAD
    os.makedirs(os.path.dirname(out), exist_ok=True)
    sheet.save(out)
    print('wrote %s (%d x %d): pairs %d filled, %d missing' % (out, sheet.size[0], sheet.size[1], filled, missing))
    return 0


if __name__ == '__main__':
    sys.exit(main())
