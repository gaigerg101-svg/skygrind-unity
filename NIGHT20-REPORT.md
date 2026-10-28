# NIGHT20-REPORT: your playtest fixes, then a build night (for GG; one page)

The night ran from 22:06 on 2026-10-06 (`date`) to the close. Six builders, then (after the account's spend limit stopped
every agent at ~01:05 for 47 minutes) four fresh ones; ~200 commits on `port/stage3`. Every number below is from a tool
run tonight (judges headless and deterministic, reels on Player builds); the full record is `NIGHT20-LOG.md`. Every
NIGHT20 change sits behind a design block: `-skygrindOff20` plays NIGHT19's game for an A / B.

## 1. Your six asks

| You said | What changed | Measured (A = NIGHT19's game, B = tonight) |
| --- | --- | --- |
| The shockwave is the shotgun's, with a long cooldown | The THUNDERCLAP's LT: the web's double blast + the shock cone + a 12 m/s push away from your aim; 18 s, one charge, ticks while holstered, none in a dash; the FORGE adds a second charge. The KESTREL's gust is gone. | ShockwaveTests 12 / 12 (17.9 s apart: one; 18.0 s: two); lift +2.62 m; 3 of 8 swarmers pushed >= 3 m; KITE with it 73.1 s vs without 95.8 s (it buys no kiting); the reel fires 1 of 3 presses inside the cooldown |
| Zoom like before, on most guns | LT zooms the KESTREL (1.5), STITCHER (1.25; its detonate moved to SPECIAL), LONGWAVE (a 3.5 scope with a real lens), ZAPPER (1.3); zoom ends the sprint; unique LTs only on the THUNDERCLAP and COMET | ZoomTests 9 / 9 (FOV 53.3 / 62.2 / 24.3 deg at base 74); the reel zooms all three on a husk at 20 m |
| RB scrolls weapons | Pad layout v3: RB = next weapon (tap), held 0.25 s = the wheel; Y = SPECIAL + ARCLINE; a saved v2 migrates | PadRbCycle tests 16 / 16; the smoke's pad steps pass on every build; the final pad reel: "slot 5 -> 2 after 3 taps of RightShoulder" |
| Regen only at the round's end | GG's REGEN switch, STRICT (SEGMENTS couldn't get there: measured): no passive HP in a round; a full refill over 2 s at the clear; +1 per close kill, +10 per punch kill, parries as before | THE MARATHON in-round heal / damage 0.81 -> 0.61 (target 0.15-0.4; median round 6.0 -> 10.0); BLOOD PRICE (the web's mod) alone gives back 0.37 of all damage: your call (section 4) |
| Speed lines from open source, not a border | The screen layers and the dash border are retired; AIR world streaks (KvantStream's rule, MIT; the tunnel layout, CC0) with a world-fixed head and a tail along your real velocity; a light travel blur on boosts only (godot-motion-blur + blur-pro-urp, MIT) | M8 v2 on build 4: L1-L5 all PASS (nothing inside the middle ellipse; 9 streaks sprinting, 39-46 within 50 ms of a boost; 41 px sprinting, 110 px boosting; a side boost's streaks 2 deg off horizontal, a slam's fall 90 deg; 0 over an enemy), the same on the final reels; round 4's blind reviewers picked tonight's speed lines in 8 of 8 judgements; the final four: 3 of 4 reviewers (13 of 16 judgements; one preferred NIGHT19's border as "fast at a glance") |
| Enemies must reach every floor | Long drops (to 20 m), a fixed leap (an approach point, a crouch tell, the arc checked), climb links (2.2-5 m; swarmers to 10 m on their own six-legged crawl), authored ladders and leaps, no dead perches (the field always seeds a spawn), perch tokens; the director's camp score notices height | THE CLIMB, Survival: unreachable player nodes 477 -> 0; worst horde time never -> 9.1 s; nodes that can't get down 0 (worst 4.9 s); DRYDOCK 55 -> 4 (the crow's nest: needs a map edit, NIGHT21); the floor rule 187 -> 0; safer perches 3 -> 1 of 10 (ladders now 8 m/s: a 24 m ladder in 3 s) |

## 2. The additions wave (each behind its own `design.adds.<row>`; FINAL: numbers)

LANCER (lancer.js), WARDEN (warden.js), the affixes (VOLATILE, BLINKER, REGENERATING, SWIFT), THE HUNT (rounds 8 / 13 / 18),
THE ELITE, round twists (FOG, SWARMAGEDDON, LETHAL, GOLDEN), BULWARK (block 1), the announcer, the NEXT line, debut cards
(the world slows <= 1.5 s), the reward reveal, the threat bar, the CODEX, SKYGRIND TIME, combos on screen + SENT trap
combos, the FORGE in tiers, the TORCH, THE BEACON, the first fifteen minutes v2, ASSIST, the first-pad hint card,
CONTROLS glyphs + L3 HOLD / TOGGLE, the WEEKLY seed, WHAT GOT YOU's tell, the DRONE (NIGHT19's, merged), the ZAPPER /
BOUNCER (NIGHT19's, merged). 22 rows landed, each with a commit and a number (the table is in `NIGHT20-LOG.md`); the
BULWARK's row is off (the bot never fights it); ASSIST's +3 rounds isn't met (the bot can't shoot drones).

**A defect both round-4 reviewers found:** tonight's style-rank letter / grade badge renders as a magenta square (Unity's
missing-material colour) in the reels. It was found at the freeze; it's NIGHT21's first fix.

## 3. The judges (A = NIGHT19's game, `--off20`, at the night's start -> B = the final code `560a4e0`)

| Judge | A (baseline) | B (final) | Target |
| --- | --- | --- | --- |
| S1 dominated guns | 4 pairs: FAIL | 0: PASS | 0 |
| S2 every gun a niche | rocket best 75 %: FAIL | shotgun best in 0 cells: FAIL | each near-best >= 2, none best > 35 % |
| S3 falloff (C7) | rail 10.13 x: FAIL | rail 1.51 x: PASS | <= 1.6 x |
| S4 combos | n/a | 4 of 5 >= 20 % (SWAP-CANCEL 19 %): FAIL | 5 at >= 20 % |
| S5 parity farm | rocket 22 vs median 6: FAIL | round 4 (pre-pick code): PASS, median of medians 9.0; not re-run at the close (time) | each within +/- 20 % |
| S6 free play's guns | n/a | round 4: shotgun 60 %: FAIL; not re-run at the close (time) | top <= 30 % |
| S7 rotation | 1.15: FAIL | 1.92: PASS | 1.3-2.0 |
| S8 kiting | 133.7 / 67.1 s: FAIL | 81.2 / 44.2 s: PASS | <= 120 / <= 60 s |
| S9 movement | 15 %: PASS | 18 %: PASS | <= 50 % |
| CLIMB R1 (Survival / DRYDOCK) | 477 / 55 unreachable | **0** / 4 (the crow's nest) | 0 |
| CLIMB R2 worst / floor rule | never / 187 | 8.7 s / 0 (DRYDOCK: never / 4) | <= 12 s / 0 |
| CLIMB R2b can't get down | 0 / 4 | 0 / 0 | 0 |
| CLIMB R3 safer perches | 3 / 8 of 10 | 1 / 1 of 10 | 0 |
| MARATHON M-1 median round | 6.0 (honest bot) | 10.0 (1.11 x): PASS | 7.2-11.7 |
| M-2 / M-3 | 48.0 % / 100 % | 44.1 % / 100 %: PASS / PASS | <= 50 % / >= 90 % |
| M-4 in-round heal / damage | 0.81 | 0.61: FAIL (BLOOD PRICE, MEND: section 4) | 0.15-0.4 |
| R1 / R2 / R3 | 100 % / 6.9 % / 12.5 % | 100 % / 26.7 % / 17.6 %: FAIL / FAIL / PASS | 35-70 % / >= 50 % / <= 25 % |
| M8 v2 speed lines (final reels) | (the old border) | L1-L5 all PASS on both passes | the research's 3.6 |
| FEEL-METRICS (final, on / pad) | | 193 / 197 PASS, 25 / 26 FAIL (M9 pop-ins and entrance times on new screens) | |
| S12 / S14 | | 0 FAIL / 0 FAIL on both passes | 0 |
| Bench p99 | 14.26 / 14.54 ms | 17.36 / 114.51 ms: **not comparable** (another session's jobs held the CPU at 100 %; the cap of 4 runs is spent) | <= baseline + 1.0 ms |

The blind picks (round 4 and the final) are in `NIGHT20-LOG.md`, decoded per scenario. The final four, in short: the
shockwave reads as a burst of air on tonight's side for all four; speed, zoom, the CODEX tabs and THE HUNT went to
tonight; the HUD question (hud-fight, vitals, rank-up) went to NIGHT19 in 11 of 12 because of the magenta rank square;
over the speed / weapon / HUD / menu scenarios tonight won 26 of 56 judgements (46 %; NIGHT19 16, Same 14).

**Before / after GIFs** (A = NIGHT19's game, B = the final build, side by side, made from the reels' frames; local only,
`Logs/` isn't in git): `Logs/n20/gifs/sprint.gif`, `shockwave.gif`, `zoom.gif`, `climb.gif`.

## 4. Questions only you can answer

1. BLOOD PRICE heals 3 HP per kill per stack mid-round (the web's mod): with it, in-round healing stays at ~0.5 of damage
   taken. Keep it as the web has it, cut it to 1, or bank its heal into the round-end refill?
2. REGEN: tonight runs your switch's STRICT choice (SEGMENTS measured 0.78 of damage healed in rounds). Keep STRICT?
3. The DRYDOCK crow's nest: no route can reach its middle (a 6 x 6 m platform on a mast); the LANCER can't see its
   middle either. Add a plate under its rim for a ladder (builder N's exact box), or leave it a reward perch?
4. Long drops reach 20 m (L4D's is ~10 m) so every node can get down within 6 s: keep?
5. The wonder weapon (B4) and the climber (C5) are proposals in `Reports/`: name them, or not.
