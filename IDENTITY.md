# IDENTITY: how SKYGRIND looks (NIGHT17 U3-30.2, 2026-10-05)

GG after NIGHT16: "the text and the font and the colors are just very bland, it needs identity." This page is the style
tile in words. The tokens live in one file, `Assets/Scripts/Game/UnityTuning.Theme.cs`, read through `UiTheme`; a token
change restyles the whole game. The capture it was picked from: `Logs/shots/n17-b1/n17-tile.png` (build 1, 21:35;
gitignored).

## The idea in one line

**Black-italic speed on near-black ink, one cyan you own, gold you earn.** Everything leans forward on one 12 degree
slant, like the wordmark and like the movement. Nothing is translucent grey, nothing is outlined, nothing appears
without moving.

## The pick (from the tile)

Two directions were drawn with the same components (the round's hero number, the rank letter and word with its decay
bar and callouts, HP in big numerals, the stamina bar, the points with a merged pop, PAUSED's four rows, a settings
slider with its footer, an onboarding prompt):

- **A (picked): Archivo Extra Condensed Black Italic + Archivo SemiBold / ExtraBold + Tourney Black Italic.** The display
  face is already slanted at about the theme's 12 degrees, so the menus, the numbers and the slant read as one gesture;
  "PAUSED", "12450" and "+150 ×3" read in one glance. Tourney's "07" and "S" have a character nothing else in the game
  has.
- **B: Big Shoulders Display Black + Saira.** Upright and condensed: it reads like a sports scoreboard (close to Bebas
  Neue, which the research lists as a cliché) and fights the slant.

Review in two lines: A's italic display and Tourney's numerals carry the identity on their own; B was clean but generic.
A's one risk is legibility under 24 px for the display face, so the display starts at 32 px and body text below that is
the upright Archivo.

## The tokens

| Token | Value | Use |
| --- | --- | --- |
| DISPLAY | Archivo Extra Condensed Black Italic (wdth 62, wght 900) | heads, hero numbers, buttons, points, callouts at 32 px and up |
| BODY / BODY-BOLD | Archivo SemiBold / ExtraBold (wdth 100) | everything under 32 px |
| RANK | Tourney Black Italic | the style rank's letter, the round's hero number |
| INK | `ui.theme.panel` (#0C0B16) at 96 % | every panel: opaque, or no panel at all |
| INK-SOFT | INK toward the text by 8 % | an unfocused row or plate on INK |
| WHITE | `ui.theme.text` (#F4F1EA) | text |
| MUTED | WHITE toward INK by 42 % | secondary text (a token, never a low alpha) |
| CYAN | `colors.usable` (#19D3E6) | the player's: focus, fills, the bar, everything you act with |
| CYAN-DEEP / CYAN-PALE / CYAN-HOT | CYAN toward INK 55 % / toward white 45 % / 78 % | the value ladder: the rank (pale -> cyan -> hot + glow), offsets behind hero numbers |
| GOLD | `ui.theme.gold` (#FFC23D) | points only |
| DANGER | `colors.enemy` (#FF4A1C) | enemies and danger only (unchanged: CLAUDE.md rule 8); magenta stays RIVAL's |
| Type scale | 18 / 24 / 32 / 42 / 56 / 75 / 100 / 133 px at 1080p | at most 4 sizes a screen; the floor is 18 px (13.3 px on the Steam Deck) |
| Slant | 12 deg | panels, bars, rows, dividers (`UiShape`) |
| Corner cut | 45 deg, 10 px, top-right and bottom-left | plates and buttons (`UiShape`, or `UiTheme.Cut` sliced) |
| Accent bar | 4 px CYAN | a focused or primary row's left edge |
| Shadows | a hard 2 px INK underlay; hero numbers get a 5 px CYAN-DEEP offset copy (the wordmark's idea) | text over the world |
| Motion | in 150 ms ease-out-back (10 % overshoot), out 100 ms ease-in; a punch 1 -> 1.15 -> 1 over 150 ms at most every 100 ms; a slam 2x -> 1x in 100 ms; count-ups 250-600 ms by the log of the change | `UiTween`, `UiEnter`, `UiPunch`, `UiCount`, `UiSlam` |
| Safe inset | 56 px | nothing closer to an edge |

## Do

- Big numerals with no labels for what you glance at (HP, points, the bar). The number is the label.
- One focal element per screen; the focused row lit (CYAN plate, INK text, the accent bar), the rest INK-SOFT.
- Enter along the slant; every number change rolls, punches or slams.
- Glyph + verb for teaching ("[SHIFT] BOOST"), shown when it's useful, gone on use.
- Use the value ladder for intensity (the rank climbing goes pale -> cyan -> hot), never a new hue.

## Don't

- No translucent grey plates, no 1 px outlines, no low-alpha text standing in for a colour.
- No synthetic bold or italic on the display face (it is a black italic already).
- No `new Color(` literal in UI code: a token or the config.
- No key hints on the HUD at rest; no sentence toasts; no line over 8 words (3 on the HUD).
- No red-orange or magenta for the player's things (the rank's top included).
