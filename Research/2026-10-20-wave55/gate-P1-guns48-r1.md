# Gate P1-guns48, round 1: blind A/B pairs ("which hits harder?")

Fresh gate critic, NIGHT55. Rubric: `Research/2026-10-20-wave55/critic-gate.md`. Only the pair frames were opened:
`Logs/feel/n55-g48/x48/a748/shot-N30-X48-pair-<action>-<P|Q>-f2.png` and `-f6.png`, 36 frames, all 1920x1080, none missing.
No log, code, config, report, `sheet-*` or `torch-*-lick` file was read. The critic does not know which side is new.

Method: order 1 viewed P f2, P f6, Q f2, Q f6 for each action; order 2 viewed Q f2, Q f6, P f2, P f6.
**Process caveat:** for kestrel-charge, thunder-boost and swap, the order-1 pick was not written down during the order-1 pass, so it
was recorded on a re-view after order 2 (marked `*`). Those three order-1 picks are not independent of order 2.

## Picks

| action | order-1 pick | order-2 pick | why (as seen on the frame) | look-rule notes (rubric line 4), per side |
|---|---|---|---|---|
| kestrel-tap | Q | Q | Marginal. The same small camera kick and cyan impact glow at the crosshair on both sides; Q's f6 adds a faint cyan tracer cone from the barrel to the hit. | P: cyan impact (player) OK; a red-orange enemy at the left (enemy) OK. Q: cyan glow and tracer OK. Neither covers the screen. |
| kestrel-charge | Q* | Q | Q's f2 floods almost the whole frame cyan (the room disappears), with a glowing cyan gun and a white muzzle disc; f6 is still a frame-wide cyan bloom with a white shard. P is readable: the gun kicks up hard, a cyan beam, a white wedge and a cyan star impact, plus a red-orange hit chevron. Q has the bigger flash; P has the clearer kick and impact. | **Q: FAIL, the cyan wash covers close to the whole screen at f2 and most of it at f6.** P: local beam / star (cyan) OK; hit chevron red-orange OK. |
| thunder-shot | Q | Q | Slight. f2 is idle on both. At f6 both show a white starburst at the muzzle and debris at the crosshair; Q adds a cyan flare streak, so it reads bigger and brighter. | P: white starburst, local, OK. Q: white plus cyan streak, local, OK. (These frames are not re-checked in this file; described from notes taken while viewing.) |
| thunder-slug | SAME | SAME | Both sides: f2 and f6 are a frame-wide pale white blow-out (the room is gone) with the gun lit solid yellow at the bottom right. The only differences are a lens-flare hexagon's place and colour, and the barrel angle. Nothing separates them. | **P: FAIL, whole-screen white at f2 and f6. Q: FAIL, the same.** Both: the gun lit solid yellow, and yellow means parryable in the rubric, so a yellow gun could read as a parry cue (flag). |
| thunder-boost | P* | P | P: a small cyan flare and faint debris at f2, then a large cyan shockwave ring around the muzzle at f6, so the weight lasts. Q: a crisp white muzzle starburst, pellet tracers and cube debris at f2, almost gone by f6. P also shows a wide white floor stripe; it may be level lighting and not the shot (white floor strips appear in other actions' frames too). Without the stripe the margin is small. | P: cyan ring OK; the white floor stripe is not the whole screen. Q: white starburst OK; a red-orange enemy at the top left (enemy) OK. |
| pulse-punch | P | P | P: a pink-white starburst under the gauntlet that grows from f2 to f6. Q: the gauntlet barely moves and there is no flash on either frame. | **P: flag, the starburst is pink / magenta-tinted, and magenta is RIVAL only; measure its hue against the RIVAL magenta.** Q: no effect; a red-orange enemy at the left OK. |
| fist | Q | Q | Q: the view is thrown up off the room into the dark (f2 almost black-purple; f6 navy with the floor edge at the bottom) and the pistol is knocked low, a strong camera throw. P: the fist only extends forward, with no camera move. No flash on either side. | Q: the whole frame is dark at f2. Most likely the camera is pointed at the ceiling void, but a frame-wide dim cannot be ruled out from two frames (not evidenced either way). Q only shows a style meter ("D DULL"). P: OK. |
| torch | P | P | P: a dense cyan spark cloud already at f2 that holds and thickens at f6, plus a red-orange hit chevron. Q: nothing at f2 (pilot light only), then a white muzzle ring and a cyan spark cloud at f6. P arrives earlier and stays heavier. | P: cyan sparks (player) OK; chevron red-orange OK. Q: white ring and cyan sparks, local, OK. |
| swap | SAME* | SAME | The same weapon motion on both sides (the pistol comes up from low at f2 to ready at f6), and neither has a flash or impact. The only difference is the scene (Q has an enemy). | P: nothing to judge, OK. Q: the enemy's red-orange glow and a red floor strip at f2 (danger) OK. |

Summary: the picks agree across both orders on all 9 actions (P: thunder-boost, pulse-punch, torch; Q: kestrel-tap, kestrel-charge,
thunder-shot, fist; SAME: thunder-slug, swap). Three order-1 picks are marked `*` (not independent; see the process caveat).

## What weakens these pairs (for the lead)
- **The scenes do not match within several pairs:**
  - an enemy appears on one side only: kestrel-tap P, pulse-punch Q, swap Q, thunder-boost Q
  - the camera position or score differs: torch (P 980 / Q 1,168)
  - a hit chevron or style meter shows on one side only: torch P, kestrel-charge P, fist Q

  A side's extra red-orange or HUD element can sway a "hits harder" read. Re-capture from the same spawn and the same target for
  a cleaner pair.
- **Two frames per side cannot evidence "at most 3 flashes a second".** That clause of line 4 is not evidenced for every action.
- **Whole-screen effects (rubric line 4 FAIL):**
  - thunder-slug, both sides (white)
  - kestrel-charge Q (cyan)

  These fail no matter which side is new.
- **Flags to measure:**
  - pulse-punch P's pink / magenta-tinted flash (magenta is RIVAL only)
  - the thunder-slug gun lit solid yellow (yellow means parryable)

GATE P1-guns48 pairs: written.
