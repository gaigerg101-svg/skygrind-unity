# NIGHT33 PROOF (the lead, 2026-10-11)

Freeze commit e5a53d14 (port/stage3), Player build n33-r7. Every number below was measured by the named check. "LOOKED AT" means the lead opened the
frame. RED = the same build with `-skygrindOff33` (NIGHT32's game), or the NIGHT32 path in a harness.

## Exactness and gates (freeze)
- fingerprint EQUAL, `--off33` EQUAL, `--off32` EQUAL (shared commit); golden 37/37; trajectories 22/22; Editor compile 0 errors; dn-check 0
  (proven red by every builder once).
- Suite run 1 (batch, c628b9bd): 1914 / 1915. The red is AutopilotTests.PerStep_AllocatesNothing ("Expected 0, was 1"), the known flake from NIGHT17,
  18 and 21; it passed twice alone in the Editor. Suite run 2 (e5a53d14): 1914 / 1915. The red is ZeroAllocTests.SimSteps_With30Enemies_AllocateNothing
  (HuskBrain.Step allocated once in 600 steps). It passed 6 / 6 twice alone in the Editor. SHORT: either a flake under batch load, or E's
  edge-stall code allocating once. NIGHT34 checks it.
- Player n33-r7: smoke `[smoke] 216.1s DONE: all steps passed`; LevelSelect29 `DONE 20 presses, 20 PASS, 0 FAIL` (all five n33 levels), 0 exceptions;
  DoorFree29 PASS (n33-iceline), 0 exceptions.
- `Mover.cs` untouched. Every sim change sits behind a design.n33 bit (tape format 16).

## Per card (Player numbers unless marked harness)
| Card | Row(s) | Proof |
| --- | --- | --- |
| U HUD + front UI | hud33 | hud33 reel n33-r6: 0 rule problems at 1080p and 720p, against RED 31 (n33-r1). Every reaction seen in both passes: charge, hit, kill, switch list, one slot line at a time held >= 2.2 s, boss bar. Campaign door marker, results with POINTS BANKED, title and level select: clean. LOOKED AT rest / charge / switch (r1), title / camp-results / camp-door (r5). |
| L light + VFX | look33 | light33 harness: RED foundry 53 lights with no fixture and 422 of 465 bars unmounted; GREEN 0 / 0 on all five levels. look33 n33-r6 on n33-foundry: mean 0.235-0.249, dark 20-26 %, 0.98+ <= 0.1 %. LOOKED AT f10-a (r5) and f2-a (r6). SHORT: lava rooms' orange share is 57-63 % against 35 %. The metric counts the lava's hue at any brightness, and the frame reads dark. |
| K kit + dressing | kit33 | kit33 harness 14/14 on n33-foundry and PASS on all five levels. Player: 617 decor rows in 45 meshes (36840 vertices), 0 NRE. LOOKED AT f10-a: RED bare walls; GREEN pilasters, pipes, recesses, landmark, lintels. |
| R overhauls + devices | arenas33, devices33 | 10 rooms rebuilt (f8 f9 i2 i5 k1 k4 s1 s6 v2 v4), all pass arenas33. Device tests 19/19. Walk 10/10 on all five levels (spawns33 on, E's spawns33walk). Deaths: f5 291 -> 17, k4 146 -> 34. Player: platform moved 5.6 m, fan lift 5.0 m, vent tell drawn. Vent look fixed after the r6 frame (4044e074); not seen in a Player since. |
| E enemies, spawns, bosses | ai33, spawns33, bosses33 | ai33 Player v6: GREEN 0 of 5 enemies burned, RED 39 of 41 (LOOKED AT v6-20s). Harness: 72 -> 0 AI lava deaths over 26 lava rooms. spawns33: 20 / 21 rooms pass, nearest spawn 8.1 m; s3b is 5.3 m both ways. Bosses, Player n33-r6: Lantern 18.3 s, Stag 17.6 s (1 death), Wyrm 19.0 s. Harness p50: Loom 19.3 s, Crown 18.1 s. camp 40 % GREEN on all five (max 35 %). |
| W viewmodel, whip, piercer | view33, whip33, pierce33 | ZOOM (r3): the gun moves 1 px and drops 5 cm; RED -338 px, 4.8° roll. PIERCE: 1 beam, 5 / 5 hit; RED 0 / 3. WHIP (r6): stop 1.98 m, 0 frames inside, 1 hit (LOOKED AT whip-stop). whipboss33: 0 steps inside on all five bosses. Recoil settle 111-137 ms. |
| P shop | shop33 | Typical earn p50 2683-5845 per level; prices 2650 / 5300 / 10600. shop33 n33-r5: PASS (pad buy, next run 120 / 120). After the bleed fix, n33-r6 frames: RESULTS hidden, 0 text overlaps, smallest text 16 px (LOOKED AT shop-open). The pad steps in r6 failed when run after hud33 in one Player (focus / order interference); the reel was hardened (b8655b3c) but not re-run (reel cap). Tape round trip with upgrades: PASS. |

## Found and fixed by the checks tonight
- Player n33-r4 hung at boot on every n33 level: StackOverflowException in Kit33Props.S (a float-rounding recursion). The Editor never loads
  an n33 level, so only the Player showed it. Fixed with de73295b, then a bounded cut (b5e8aa10, Kit33CutTests 4/4).
- spawns33 soft-locked levels (walk bot: iceline 1/10, spire 1/10). Fixed with deac6c46: walk 10/10 on all five.
- ai33's lava mask broke LevelValidate24's pit-recovery rows (false errors on every lava pit). Fixed with 25ec5481.
- v6's enemy catwalk made a 5-cell pit soft-lock. Fixed with 4b3e6378.
- The RESULTS screen bled through the SHOP. Fixed with 96a2d3bd.
- The results head showed the level id. Fixed with b5b6a2cc.
- The look grade washed out (r3, mean 0.35, dark 2 %). Fixed with b41142c2.

## Not verified
- No person has played it.
- The shop's pad path after the bleed fix in a fresh Player.
- The vent's new look in a Player.
- THE LOOM and THE CROWN Player kill times (harness only).
- The 0.6 s hold-still spawn tell is off (`tellStagger: false`), because it put the walk bot into death loops.
