# Wave 49 spike: can "bland" be measured? (S5, 2026-10-19 ~11:35)

Premise to prove before the run: a look pass can be scored by numbers on Player frames, not only by eye, so every polish step has a
before / after and a blind pair.

## The meter (a scratch prototype; the run copies it into Tools/night/n49-lookmeter.py)
Per frame, with the HUD bands cut (top 12 %, bottom 18 %): mean value V, the share of dark (V < 0.25) / mid / bright (V > 0.75)
pixels, mean saturation, the share of strongly saturated pixels (S > 0.5 and V > 0.3), the share of cyan pixels (hue 170-200 deg,
S > 0.35, V > 0.35), and the hue spread (30-degree bins holding >= 3 % of the saturated pixels).

## The baseline (n48-r5 Player frames, measured)
| frame | V | dark | mid | bright | S | saturated | cyan | hue bins |
|---|---|---|---|---|---|---|---|---|
| L1 foundry, WICK arena (a148 g5-phase2) | 0.64 | 2 % | 69 % | 29 % | 0.52 | 48 % | 0.1 % | 2 |
| L1 foundry, the climb (a148 g1-top-p6) | 0.50 | 14 % | 69 % | 18 % | 0.35 | 8 % | 0.5 % | 3 |
| L2 iceline, ICEBREAKER (a248 haymaker) | 0.41 | 32 % | 61 % | 7 % | 0.39 | 6 % | 0.9 % | 7 |
| L3 kiln (a348 glimpse-top) | 0.26 | 63 % | 33 % | 3 % | 0.58 | 27 % | 4.9 % | 5 |
| L4 span, LOOM (a448 shuttle-20) | 0.24 | 61 % | 35 % | 4 % | 0.39 | 5 % | 3.1 % | 8 |
| L5 spire, CROWN (a548 fire-pair-A) | 0.46 | 30 % | 46 % | 25 % | 0.42 | 20 % | 2.2 % | 5 |
| a plain room (a648 play) | 0.43 | 9 % | 86 % | 5 % | 0.28 | 3 % | 4.4 % | 5 |

## What it says
- The premise holds: the meter separates the frames GG calls bland (a plain room: 86 % mid, 3 % saturated; the climb: 69 % mid, 8 %
  saturated) from the strongest frames (L3: 63 % dark with 27 % saturated accents).
- "Bland" here is mostly the MID band: most rooms sit in mid values with few saturated accents. L1 is the opposite fault: one hot hue
  everywhere (48 % saturated, 2 hue bins, almost no dark), so nothing stands out either.
- Cyan is 0.1-4.9 % of a frame, but it is spread over every edge (the light names: 1604 "cyan" fixtures map to colors.usable,
  CODE-FACTS 3). The meter cannot tell usable from decorative cyan by itself: the run pairs it with the fixture count by colour.

## Proposed targets for the run (hypotheses, set per level by the colour script; the run logs any change)
- A combat frame: dark >= 35 %, mid <= 55 %, saturated 10-30 %, hue bins 3-5 (one dominant level hue + danger + usable).
- Decorative cyan fixtures -> the level's accent: the count of "cyan" fixtures that are not usable drops to 0; cyan stays on ride
  walls, grapple nodes, pads, pickups.
- Each pass judged by a blind pair (3 judges: "which looks more like a polished, high-contrast action game?"), not by the meter alone.
