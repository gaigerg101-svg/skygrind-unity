# NIGHT19 U3-33.4 (builder P): packs the vendored Kenney Input Prompts (1.5A, CC0: ../License.txt) into the one atlas the
# game loads (../Resources/KenneyInputPromptsAtlas.png; Game/Input/PromptGlyphs.cs builds a TMP sprite asset on it at run
# time). This folder ends in "~", so Unity never imports it: the sources ship in the repo, not in the Player.
# Run: python pack_atlas.py   (Pillow; from this folder)
#
# The layout MUST match UnityTuning.Input.cs (PromptAtlas*): 3 sets (Xbox, PlayStation, Switch by position) x 18 glyphs
# (PadButtonName order 0-15, then the left and right sticks), each drawn plain then with the HOLD ring; cell
# PromptAtlasCell px with the glyph (PromptAtlasGlyph px) centred, PromptAtlasColumns per row from the top left; index
# i = hold * 54 + set * 18 + button.
# The palette (CLAUDE.md rule 8, NIGHT19 rule 0.9 "re-tint every imported texture"): Kenney's red d-pad highlight becomes
# the player's cyan (config colors.usable 0x19D3E6); the cut-out letters (holes) are filled with the theme's ink
# (config ui.theme.panel 0x0C0E16) so a glyph reads on any background; the HOLD ring (flair_circle_8) is cyan. Fully
# transparent pixels get the white of the glyph (no dark fringe when the texture is filtered).
import os
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, '..', 'Resources', 'KenneyInputPromptsAtlas.png')
SIZE = 1024      # UnityTuning.PromptAtlasSize
CELL = 80        # UnityTuning.PromptAtlasCell
GLYPH = 64       # UnityTuning.PromptAtlasGlyph
COLUMNS = 12     # UnityTuning.PromptAtlasColumns
HOLD_INNER = 0.66  # the glyph's share of the ring's size in a HOLD glyph
CYAN = (0x19, 0xD3, 0xE6)
INK = (0x0C, 0x0E, 0x16)

SETS = [
    ('Xbox Series', ['xbox_button_a', 'xbox_button_b', 'xbox_button_x', 'xbox_button_y', 'xbox_lb', 'xbox_rb', 'xbox_lt', 'xbox_rt',
                     'xbox_button_view', 'xbox_button_menu', 'xbox_stick_l_press', 'xbox_stick_r_press', 'xbox_dpad_up', 'xbox_dpad_down',
                     'xbox_dpad_left', 'xbox_dpad_right', 'xbox_stick_l', 'xbox_stick_r']),
    ('PlayStation Series', ['playstation_button_cross', 'playstation_button_circle', 'playstation_button_square', 'playstation_button_triangle',
                            'playstation_trigger_l1', 'playstation_trigger_r1', 'playstation_trigger_l2', 'playstation_trigger_r2',
                            'playstation5_button_create', 'playstation5_button_options', 'playstation_button_l3', 'playstation_button_r3',
                            'playstation_dpad_up', 'playstation_dpad_down', 'playstation_dpad_left', 'playstation_dpad_right',
                            'playstation_stick_l', 'playstation_stick_r']),
    # Nintendo by position: south prints B, east A, west Y, north X (UnityTuning.NintendoGlyphs)
    ('Nintendo Switch', ['switch_button_b', 'switch_button_a', 'switch_button_y', 'switch_button_x', 'switch_button_l', 'switch_button_r',
                         'switch_button_zl', 'switch_button_zr', 'switch_button_minus', 'switch_button_plus', 'switch_stick_l_press',
                         'switch_stick_r_press', 'switch_dpad_up', 'switch_dpad_down', 'switch_dpad_left', 'switch_dpad_right',
                         'switch_stick_l', 'switch_stick_r']),
]


def recolour(im):
    """Kenney's red highlight -> cyan; interior holes -> ink; transparent -> white RGB."""
    im = im.convert('RGBA')
    w, h = im.size
    px = im.load()
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a > 0 and r > 180 and g < 110 and b < 130:
                px[x, y] = CYAN + (a,)
    # Flood the outside (transparent pixels reached from the border); every other transparent-ish pixel is a hole.
    outside = [[False] * w for _ in range(h)]
    stack = [(x, y) for x in range(w) for y in (0, h - 1)] + [(x, y) for y in range(h) for x in (0, w - 1)]
    while stack:
        x, y = stack.pop()
        if x < 0 or y < 0 or x >= w or y >= h or outside[y][x] or px[x, y][3] >= 128:
            continue
        outside[y][x] = True
        stack.extend(((x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1)))
    # A hole: a transparent pixel the outside never reached; its rim: a pixel within 2 px of one (the letter's anti-aliasing).
    hole = [[px[x, y][3] < 128 and not outside[y][x] for x in range(w)] for y in range(h)]
    near = [[any(hole[yy][xx] for yy in range(max(0, y - 2), min(h, y + 3)) for xx in range(max(0, x - 2), min(w, x + 3)))
             for x in range(w)] for y in range(h)]
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a < 255 and near[y][x] and not outside[y][x]:
                # ink under the glyph's white, by the glyph's coverage
                t = a / 255.0
                px[x, y] = (round(r * t + INK[0] * (1 - t)), round(g * t + INK[1] * (1 - t)), round(b * t + INK[2] * (1 - t)), 255)
            elif a == 0:
                px[x, y] = (255, 255, 255, 0)
    return im


def glyph(path, size):
    return recolour(Image.open(path)).resize((size, size), Image.LANCZOS)


def hold(path, ring_path, size):
    ring = Image.open(ring_path).convert('RGBA')
    rp = ring.load()
    for y in range(ring.size[1]):
        for x in range(ring.size[0]):
            r, g, b, a = rp[x, y]
            rp[x, y] = CYAN + (a,) if a > 0 else (255, 255, 255, 0)
    ring = ring.resize((size, size), Image.LANCZOS)
    inner = int(round(size * HOLD_INNER))
    g = glyph(path, inner)
    out = Image.new('RGBA', (size, size), (255, 255, 255, 0))
    out.alpha_composite(ring)
    out.alpha_composite(g, ((size - inner) // 2, (size - inner) // 2))
    return out


def main():
    atlas = Image.new('RGBA', (SIZE, SIZE), (255, 255, 255, 0))
    ring = os.path.join(HERE, 'Flairs', 'flair_circle_8.png')
    pad = (CELL - GLYPH) // 2
    i = 0
    for h in (0, 1):
        for folder, names in SETS:
            for n in names:
                src = os.path.join(HERE, folder, n + '.png')
                im = hold(src, ring, GLYPH) if h else glyph(src, GLYPH)
                col, row = i % COLUMNS, i // COLUMNS
                atlas.alpha_composite(im, (col * CELL + pad, row * CELL + pad))
                i += 1
    assert (i + COLUMNS - 1) // COLUMNS * CELL <= SIZE, 'the atlas is too small'
    atlas.save(OUT, optimize=True)
    print('wrote', OUT, i, 'glyphs')


if __name__ == '__main__':
    main()
