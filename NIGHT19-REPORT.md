# NIGHT19-REPORT: every gun has a job (for GG; one page)

The night ran from 16:05 on 2026-10-06 (`date`). **It was cut short:** at ~17:27 every builder stopped on the account's
monthly spend limit (HTTP 429), and this session itself made no progress until 20:55. So there were no rounds and no blind
reviewers; the final build holds the builders' first passes only. Every number below is from a tool run tonight; the full
record is `NIGHT19-LOG.md`.

## 1. What's in the final build (`8a33313`)

- **The pad (P):** the v2 layout (L3 sprint as a TOGGLE, R3 punch / parry, LB boost, RB special + grapple, d-pad up
  lock-on) with the v1 migration; every prompt through `PromptText` drawn as Kenney glyph sprites for Xbox, PlayStation and
  Switch; LB / RB switch tabs on SETTINGS, CONTROLS, RESULTS and the run setup. The smoke's pad steps pass.
- **The KESTREL's gust (W):** LT is a wind gust (the zoom is gone from the KESTREL): it pushes you away from your aim and
  pushes and staggers light enemies in a cone.
- **Threats and stamina (E):** the sprint drains 0.3 bar/s (3.3 s on a full bar); a kill within 6 m refills more than a
  far one; the CHARGER is ported (the telegraph, the 22 m/s charge, the wall stun, its view).
- **The HUD (H):** the stamina left the crosshair for chunky pips in the bottom-left vitals plate; a role tag on the weapon
  card; combo callouts; WEAPONS rows as role cards; CONTRACTS cascades; roundend's pops kept off the banner.
- **The hands (V):** the gust's look (cyan streaks, shock rings, FOV kick, wake), alt-fire poses, the R3 jab, textured
  sparks (Kenney), the ZAPPER and BOUNCER viewmodels, horde5's confetti cut.
- Not in: W's niche retune, the ZAPPER / BOUNCER guns, the combos, freshness on points, the sidegrade forge; E's DRONE
  and SHIELDED (uncommitted when the builders stopped).

## 2. The judges (new: THE RANGE, THE CHASE, THE PARITY FARM; headless, deterministic, hand-checked)

| Metric | NIGHT18 (b0) | Final | Target |
| --- | --- | --- | --- |
| S1 dominated pairs | 4 (THUNDERCLAP by 3 guns, LONGWAVE by KESTREL) | 4 | 0 |
| S2 best-cell share | COMET 75 % | COMET 75 % | <= 35 % |
| S5 one-gun farm medians | LONGWAVE 2, STITCHER 6, THUNDERCLAP 7.5, KESTREL 14, COMET 24 | not re-run | +/-20 % |
| S6 free-play kill share | THUNDERCLAP 40 %, STITCHER 29 %, KESTREL 28 % | not re-run | top <= 30 % |
| S7 rotation pays | 1.16 | 1.15 | 1.3-2.0 |
| S8 KITE survives (r10 / r15) | 128.2 / 169.0 s | 133.7 / 67.1 s | <= 120 / 60 s |
| S10 gust-jump / C3 push | n/a | +2.62 m (above the rocket jump's +1.12 m) / 3 of 8 | 2-4 m below the rocket jump / >= 3 |
| S12 keyboard tokens on the pad | 3 prompts ("HOLD E") | 0 in 30 scenarios | 0 |
| S14 persistent HUD in the middle | sprint 5, boost 4 | sprint 1, boost 0 (the speed-lines layer) | 0 |

THE RANGE's table (seconds to clear + 0.05 x HP taken) says the COMET wins almost every situation and the THUNDERCLAP
loses even at 4 m; the farm's bot prefers the THUNDERCLAP anyway (its near-range list). That gap is the next night's job.

## 3. Questions only GG can answer

1. The gust lifts you 2.6 m, higher than the COMET's rocket jump (1.1 m at the same timing). Keep it as strong as that,
   or keep it below the rocket jump as NIGHT19's target said?
2. The KESTREL lost its zoom to the gust (the KESTREL LT switch). Keep, or move the gust to its SPECIAL?
3. NIGHT19's remaining work (W's niches, the ports, the combos, the DRONE) sits uncommitted in the builders' worktrees:
   resume it next night (NIGHT19 again) before map night 2?
