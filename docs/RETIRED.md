# Retired rows (OFF three nights or more, superseded by a later row)

NIGHT45 C1, 2026-10-17 (NIGHT45.md, C1's card; DECIDE45 D-3). **Nothing here is deleted from `Design/unity-config.json` or from the
code.** Old tapes replay through these rows (each sim row is a bit in its night's `N<nn>Blocks`), `--offNN` rebuilds every older night's
game from them, and several later rows read their code or numbers (marked "load-bearing"). "Retired" means: do not turn it on, do not
iterate it, do not plan from it; the row named in "superseded by" is the live one. Re-opening a retired row is GG's call.

How a row was judged: `enabled` read from `Design/unity-config.json` at bc0c8f74 (`n45` is NIGHT45, so "OFF since n42" is three nights:
42, 43, 44); the superseding row's `_why` and its night's log; a code grep for who still reads the row's key.

## Retired: superseded by a later row

| row | OFF since | what it was | superseded by (ON) | evidence | load-bearing? |
|---|---|---|---|---|---|
| `n32.balance32` | n32 (13 nights) | builder G's combat balance pass outside the bosses (weapon DPS parity, room health / counts, damage taken, drops) | `n33.bosses33`, `n39.boss39` (the boss bands) and the kit balance commit 164ca6f2 (NIGHT32: "no weapon over 45 %") | NIGHT32-LOG.md 13: "353b6a7c balance32 table: no outlier weapon, balance32 stays off"; close row 25 the same. The pass found nothing to change, so the row never carried a rule | a bit in `N32Blocks` (Core/N32Blocks.cs:40); read by FeelReel.N32.Grapple / Harness.N32.Grapple |
| `n38.ride38` | n38 (7 nights) | the HELD wall ride keeps its height; the look pitch drifts it | `n40.ride40` (every ride holds; DECIDE40 D-9), then `n43.ride43` (attach / hold / detach forgiving) | NIGHT40-LOG.md 36-37: ride40 ON; "ride38's old a5 2 -> 9 does not reproduce on n38-foundry (deaths over 5 seeds in a5: 249 / 23 / 15 for off / ride38 / ride40)" | **yes**: Core/Ride40.cs:59 falls back to `design.n38.ride38.*` for any number ride40 lacks; Mover.cs:404 and Core/Ride38.cs; a bit in `N38Blocks` |
| `n38.doors38` | n38 (7 nights) | holographic door curtains (cyan open, red-orange locked, pale violet-white boss door) | `n41.doors41On` (turns doors38's view on after NIGHT41's door frames) | NIGHT41-LOG.md 26: "juice38 / doors38 / pump40 on through juice41On / doors41On / pump41On (the n38 / n40 rows" untouched); THREADS w40-42: T-55 closed | **yes**: Game/Doors38View.cs:133 draws it when `doors38 || doors41On`; the row's own `enabled` stays false so `--off41` is NIGHT40's game |
| `n38.juice38` | n38 (7 nights) | the boss entrance hit-stop / shake / flicker and the damage static (ULTRAKILL's CameraShake rules) | `n41.juice41On` | NIGHT41-LOG.md 26 (as above); T-55 closed in w41 | **yes**: Game/Juice38.cs:128 reads `design.n38.juice38.*` (CameraFeel.cs:101, Crt29Pass.cs:70) |
| `n38.stag38` | n38 (7 nights) | THE STAG's canal with tall rideable lane walls and the read pass | `n40.brawl40` + `n40.iceline40`: level 2's boss is THE ICEBREAKER since NIGHT40 | stag38 was turned off on its own night (NIGHT38-LOG.md 37: `Tools/night/n38-stag38-off.cjs`; NIGHT39-LOG.md 219: "stag38 (iceline off)"); NIGHT40-LOG.md 147 / 163: THE ICEBREAKER beaten in the Player at 47.5 s, brawl40 ON. Measured now: Design/levels/n43-iceline.json has one `"boss":"brawl40"` and no stag37 boss; n37-iceline has `"boss":"stag37"` | a bit in `N38Blocks`; Core/Enemies/Stag37Arena.cs:66 reads it when the bit is set (old tapes) |
| `n40.pump40` | n40 (5 nights) | the shotgun's faster firing animation (view) | `n41.pump41On` | NIGHT41-LOG.md 26 and 89 (pump41On in the build copy, then ON) | **yes**: N41View draws pump40 when `pump40 || pump41On` (Game/N41View.cs:6) |
| `n42.boost42` | n42 (3 nights) | the THUNDERCLAP air boost easier (more lift, lift by pitch) | `n43.boost43` (jump + shot inside a window; "boost42's shape with boost43's numbers") | DECIDE42 D-9: OFF (lift 11: 8 m climbs 3.8 -> 22.3 %, but level 3's G1 only +10 %); NIGHT43-LOG.md 37: boost43 "jump + shot within +-0.15 s 1.48-5.08 m -> 6.33-7.98 m"; close row 103 ON; THREADS w43: T-63's boost42 closed as superseded | **yes (code)**: Core/Boost43.cs pushes with `Boost42.Push` (boost42's shape); a bit in `N42Blocks` |
| `n42.parry42` | n42 (3 nights) | the DRONE orb's forgiving parry (reach, mercy, early buffer) | `n43.parry43` (intercepts farther along the orb's path) | DECIDE42 D-10: OFF until framed; NIGHT43-LOG.md 33 / 105: parry43 ON (presses in the last 200 ms 11/24 -> ...; 5.61 m); T-63's parry42 closed in w43 | **yes (code)**: Core/Parry43.cs uses "parry42's volume" and mercy (lines 5, 10). Whether parry43 reads `design.n42.parry42`'s numbers: not checked |
| `n38.iceline38` | n38 (7 nights) | level 2 rebuilt for flight (n38-iceline) | `n40.iceline40` (level 2 as n40-iceline), then `n43.iceline43` | NIGHT38-LOG.md 117: SHORT (G1 the skate gap INFO only); NIGHT40 / NIGHT43: iceline40 / iceline43 ON. T-56 (the skate gap, "GG's call") was asked of n38-iceline, which no longer plays | a flow swap in Curriculum25.N38Levels; a bit in `N38Blocks` |
| `n42.teach42` | n42 (3 nights) | at most two lines in a level's first 60 s | `n43.teach43` (the centre teaching card) | DECIDE42 D-11: OFF, "the reel's walk showed 2 lines in level 1's first 60 s with or without it"; teach43 ON (NIGHT43) | Game/UiTeach42.cs, FeelReel.N42.First.cs |
| `n41.models41` | n41 (4 nights) | the HUSK's own feature + a death break-apart for every kind | `n43.models43` (the pill language, DECIDE43 D-4) | T-60: never seen on a frame; models43 ON in NIGHT43 | Game/EnemyViews.cs / EnemyViews.Death.cs read it; whether models43 kept the break-apart: not checked |
| `n27.chargeShots27` | n27 (18 nights) | RT held on the PISTOL and SHOTGUN charges a shot | `n31.charge` ("design.n27.chargeShots27's rules back on the KESTREL ... and the THUNDERCLAP") | THREADS w31: T-36 closed, "the charge shot ON; the smoke with it ON passed every step, n31-r1" | **yes**: n31.charge runs chargeShots27's rules |

## Not retired: GG decides

These are OFF for three nights or more, but no later row replaced them: each waits on GG, so they stay as they are.

| row | OFF since | the open call | where it is asked |
|---|---|---|---|
| `n38.finale38`, `n41.finale41` | n38, n41 | level 6: the slot stays locked; finale41's greybox waits for GG's reaction to its beat sheet; THE RIVAL is a proposal | DECIDE41 D-3, D-12; T-57 |
| `n42.arena42` | n42 | the arena elements (grinder, breakable cover); its pieces leave b1 with NIGHT45's A1 rebuild, so turning it on later does nothing on n45-foundry | DECIDE42 D-12; DECIDE45 D-6 |
| `n41.aim41` | n41 | weak-point aim assist on the pad's assist mode; never framed | DECIDE41 D-6; T-60 |
| `n41.select41` | n41 | the level-select previews (name, weapon, boss silhouette); never framed | DECIDE41 D-7; T-60 |
| `n42.reticle42` | n42 | every reticle turns faster; "not shown faster" on a frame | T-63; GG's G8 ask |
| `n41.juice41` | n41 | the juice's iteration numbers (juice41On turned on juice38's NIGHT38 numbers, not this iteration) | NIGHT41 J2; not checked whether a frame compared them |

## Not judged tonight

OFF for many nights, neither superseded nor on a DECIDE page that C1 read: `n22.trickChain`, `n22.overdrive`, `n24.prewarm`,
`n25.meleeFire`, `n26.wormVisual`, `n28.rink`. Left as they are; a later cleanup can judge them from their nights' logs.
