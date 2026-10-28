# NIGHT37 log: the boss night: four bosses restarted from scratch

t0 = 20:38:31 (`date`, GG's "start building"). Box 4:00 (00:38), hard cap 4:20 (00:58). Base: port/stage3 at 50116658 (u3-n36 + the
wave-37 research 040a5b41 + DECIDE37). GG's request: Research/2026-10-12-wave37/GG-REQUEST.md; design: BOSSES.md; decisions:
DECIDE37.md (D-1 THE RIVAL later, D-2 all four tonight: chosen by GG against the lead's one-per-night recommendation).

## U3-50.1 Start (0:00-0:16)
- Editor stopped, heartbeat live; free memory 21.8 GB; 0 SKYGRIND processes.
- Builders launched 20:41 in worktrees before the shared commit (their reading overlaps it; n36's choice): K a6d87b622126ba3e2 (the
  Boss37 brain), W a770fc34aa94e61c4 (THE WICK), S a8bcf2d1ec9adc73a (THE STAG), O a2522085d324a2740 (THE LOOM), C a3187921229068da2
  (THE CROWN). Common brief: the session scratchpad's n37-common.md.
- Shared commit f79cf1c7 (20:53): N37Blocks (5 bits), tape format 20 (VarI), design.n37 rows off (each boss row carries its
  levelFrom -> levelTo, applied by Curriculum25.N37Levels; rejected: one shared levelSwap map, which four builders would conflict on,
  L-049), the four bosses' Begin / System hooks (BossRoomBegin calls them first; one partial per builder, since a partial method can have
  only one body), boss37WalkInHere joining Boss36WalkIn, --off37. Gates: fingerprint EQUAL, --off37 EQUAL, --off36 EQUAL, golden 39 sets
  0 mismatches 22/22, dn-check 0, Editor compile 0 errors, N37SharedTests 3/3, N36 / N35 / N34SharedTests 3/3, TapeTests 14/14,
  Lesson25Tests 10/10.
- K's Boss37 brain interface (ed55ea46, real bodies) integrated at 20:53 and forwarded to W / S / O / C with the shared hooks.
- docs/VERBS.md rows 44-47 (the climb, the slipstream, the thread tear, the ring pass). Tools: n37-rows.cjs, n37-reels.sh.

## Round 1 (0:16-1:10)
- K (21:00-21:14): the kit's numbers + `boss37 kit` GREEN 11/11 (07837658: budget forces a 2.00 s rest after 3 cost-2 attacks; a parry
  refund lets 5 run; burst 2 then 1.50 s; 0 repeats in 2000 picks; habit drifts in 8.00 s; camp due 3.00 s; hide at 3.00 s; window
  2.50 s; leg change 1.50 s; restart keeps leg, meter, HP; 1 tell live), view helpers (b6b960f8: Boss37Glow, Boss37StaggerBar), verbs37
  reel (61164492; two hook lines in FeelReel.N35.Verbs.cs accepted), Boss37Tests (f5fc3a3d): 7/7 in the Editor (21:16). K done.
- O (21:18): n37-span (8880e8b7: n33-span's s1-s7 as they were + o1 THREAD LINE, o2 SWING ANCHOR, o3 THE CANYON: 120 m void, 3 tower
  pairs, 13 thread anchors) + the fall rule (8cdf2b07: back on the last anchor ledge, -20 HP). `loom37`: level PASS (29 nodes), fall
  PASS, cross PASS (9.8 s, 4 pulls, 0 falls); every grapple link <= 31.5 m of the 40 m reach. n36-jobs RED on lava in the copied n33
  rooms only (accepted).
- S (21:21): THE CANAL (dda2c85b: a 260.1 m loop, three 5 m lanes, 24-facet ride banks, a 25 m tunnel, 18 floes); `stag37 all` GREEN
  (path round-trip exact; ice everywhere; level 6's old Stag still boots). Measured: pumping dashes 21.2 m/s lap average on the canal's
  ice; the Stag is planned at ~23.5 m/s (catching it needs the +25 % slipstream). Lead: Stag37View lines in Boss26 (325b0584).
- C (21:35): the spire + Crown37 first cut + harness + reel (38cde7f2, 98c378c8, 1636bcad, 6b0f68cf): the orbit carries a rider with
  0.0000 m error over 10 s; jump 7.08 m, jump-dash 21 m, every gap PASS; the walk-in moves the feet 0.00 m.
- W (21:42): n37-foundry's THE FLUE + PIERCE rehearsal rooms and THE STACK + Wick37 first slice + reel (780f3124, fb9a8bd2). Measured
  changes to the design: 4 panels per turn (at 3 the jump-off lands short), a 2.6 m rise (at 2.2 a level jump-off already reached the
  next), the bare shaft wall made HOT (any wall can be ridden: only the cyan panels hold); the level jump-off never passes panel 0 (the
  look-up is required: PASS). The pilot falls back at panel 3->4 and the band is 0/3: not fightable yet. Lead: Wick37View lines (febcd7fb).
- After each integration: dn-check 0, fingerprint EQUAL, --off37 EQUAL, golden 0 mismatches 22/22.
- Build point 87e255e1 (boss37, stag37, loom37, crown37 ON); n37-r1 built 21:38. Reels (3; budget 3 / 26): crown37 RED (pass flash
  frame missing; everything else measured: tells 3, window 2, legs, kill; 0 NRE); arena2 / arena4 (L's level36 reel on n37-iceline /
  n37-span) ended at Results before the canal / canyon: the unwired boss rooms clear at once, so those frames wait for S's / O's reels.
  The lead used one inline `node -e` to add the arena sets to n37-reels.sh (CLAUDE.md rule 9 prefers a script file): noted.
- Frames looked at: crown37 arena-bridge, arena-orbit (cyan-edged fragments orbiting the spire core: a platforming space that reads),
  the window (the ring all cyan: reads), tell-gemShard (the gems ORANGE: a parryable tell must be YELLOW): sent to C.

## Round 2 (1:10-)
- S (21:54): THE STAG fightable (80c458bb sim + pilot + harness, 6fcea3ae jointed body + tells' colours, 9201a7b2 reel). `stag37 all`
  GREEN: band 10/10 killed, p50 19.7 s, p90 22.5 s; damage while standing still 0.0 %; tells chunk 1.20 / shard 1.00 / cut 1.40 /
  charge 1.50 s; the walk-in moves the feet 0; level 6 still boots. S found Stag37.cs seeds the Boss37 brain with a constant 37:
  CLAUDE.md rule 6 wants per-run seeding: sent back (Crown37 / Wick37 already pass a seed). Gates EQUAL, golden exact.
- n37-r2 built 21:58 at 9201a7b2; reel stag (1; budget 4 / 26): stag37 GREEN: killed in 23.8 s of fight, 5 tells (kick back 0.80 s:
  under the 1.0 s floor), 3 leg punches, 1 chunk parry, 2 windows, 2 launches, the antler charge parried, slipstream first at 6.5 s,
  floes dropped 0; 0 NRE.
- Frames looked at: the slipstream streak behind the running stag (reads very well), the stag's silhouette (a clear deer, blocky),
  tell-antlerCharge (antlers YELLOW: right), the window (on its side, cyan bands). Sent to S: the KESTREL is in hand in every frame
  (the gauntlet's fight: equip it), leg 2's floes never shown, kick back's wind-up to 1.0 s.
- C (22:06): round 2 (7c266570): the descending pilot; band 10/10, p50 18.18 s (spread tiny: one deterministic pilot); 0.0 % still
  damage; tells 1.00-1.40 s; a pass needs 12 m/s flat (only a jump-dash counts); a parry drains stagger only; the yellow tell, the
  campaign-mode reel, the real pass frame.
- O (22:10): THE LOOM fight (2d7cae56 sim on Boss37, dc78705f view, 575a6cf8 reel, f887d90a band): harness band 10/10, p50 21.6 s,
  0.3 % still damage, every tell 1.2 s. Lead: Loom37View lines (ff03d7a6). Editor compile 0 errors.
- n37-r3 built 22:13 at ff03d7a6. Reels (2; budget 6 / 26):
  - crown37 GREEN (CAMPAIGN run: the banner gone; pass flash frame True; tells lowFire 1.00 / beamCut 1.40 / gemShard 1.00; windows
    2; legs 2-3; kill). The pass frame: the TORCH lit cyan mid-pass over the orbiting pieces.
  - loom37 RED: tears 0, NOT killed (the reel's pilot never tore a knee: harness 10/10), lash wind-up 0.38 s in the Player (harness
    1.2 s), space-start framed in room 4.1, SURVIVAL banner. The reveal reads (the body slung on six legs between the towers, cyan
    knees). Sent to O.
- W (22:18): round 2 (ad461e01, 37288114): `wick37 gaps` measures every gap's release window and margin on the real Mover (look-up +
  double jump 7.2 m window, +2.03 m margin; no double jump +0.80 m; level look 1.6 m, -0.06 m); rise 3.2 m, ledges as 80° arcs at
  8.4 / 23.5 m, cap 38.6 m; the hot wall pulses red-orange. The climbing pilot still never reaches the cap (63 hot-wall burns a run):
  the lead told W to script it per panel from the gaps numbers, and to ship a smaller working fight if the full one can't land by 23:30.

## Round 3 (1:42-)
- S (22:23): round-2 fixes (054f8c34, 30a199fc): the brain + lane weave seeded per run (runSeed ^ salt: 10 seeds now 15.8-29.3 s);
  kick back counts slipstream steps over the last 6 s (habit 1.8 s, measured: 0.7 per fight); floes picked >= speed x 1.3 s ahead
  (the RED pulse leads by >= 1 s), never under the player or in a walled stretch; the GAUNTLET in hand (a landed swing readies the next
  in 0.5 s via ReadyPunch34; no hit carries the bar past the next leg's cut; leg 3 only the parried charge kills); kick back 1.0 s.
  Band 10/10, p50 21.3, p90 25.9 s, 0.0 % still. Sent to S: a "casual" pilot (sprint-glide, no pumping) must still catch the Stag
  (rubber band), p90 to ~24 s.
- n37-r4 built 22:25 at 054f8c34. Reels (2; budget 8 / 26): stag37 GREEN (killed in 35.1 s of fight; tells 1.00-1.50 s incl. kick
  back 1.00; 3 chunk parries, 3 windows; floes dropped 5 with the RED and the down frames; weapon in hand 'gauntlet'); wick37 space
  GREEN (5 frames, 13 / 13 panels trimmed, slag drawn) / fight RED (the pilot never reached ledge 1; the wick37 row is off in config
  and W's reel does not force it on).
- Frames: S37-floes (the gauntlet in hand, the stag ahead on the turn, a RED floe strip on the ice): reads. W37-space-up (looking up the
  shaft: the spiral of cyan-edged panels, the red-orange hot seams in rings): reads as a climb.
- C (22:29): round 3 (f9becf26): each leg opens with forced openers (bladeRing + beamCut; leg 3 bladeRing + plazaDrop), no pass line
  until the last opener's wind-up, rest 0.5 s / leg change 0.8 s (attack rate up, never health); `crown37 attacks` PASS on every line
  (blade ring: stay 1 hit, jump+double 0, drop 0; shard parried drains stagger 4 -> 2; beam cut RED 1.40 s then drop; habit drop
  3.03 s; airborne low fire at 6.00 s; the KESTREL charge finisher kills). Band 10/10, p50 21.93 s (the top of the band), p90 23.02 s;
  every leg met a cut and a blade ring in all 10 seeds.
- O (22:30): the short lash's cause (e8438453: a punched spool called Boss37Brain.Parried, which broke the lash's wind-up) and the reel
  fixes (b31d9993: CAMPAIGN run, room points from the level's rows, the pilot stepped per sim step, a forced cut if none).
- n37-r5 built 22:31 at b31d9993. Reels (2; budget 10 / 26): loom37 GREEN (tears 6: pull 3 + LT 3, reel puppets 0; first window 5.2 s;
  leg 3; killed at 22.0 s; lash 1.20 s; cuts 10; 0 NRE), crown37 GREEN (tells 4 incl. bladeRing 1.20 s; blade 1 hit taken; cuts 3).
- Frames: L37-fight-tear (mid-air over the canyon between cyan ride walls, nails in a glowing knee, legs hanging: the fight reads as a
  fight with no floor), L37-fight-kill (the body falling), L37-space-start ("NAIL THE KNEE. G PULL AWAY." on its plate; the room title
  still reads 4.1: sent to O).
- verbs37 on n37-r5 (5 reels; budget 15 / 26), 0 NRE on every map: n37-iceline 28/30 (RED 24 punch, 40), n37-span 28/32 (RED 13, 18,
  19, 40), n37-spire 29/33 (RED 13, 15, 19, 40), n35-kiln 27/29 (RED 40, 43), n35-finale 28/31 (RED 19, 40, 43). Lead's read: row 40 is
  NIGHT36's open sub-check (the parry works: spitter dead, health 50 -> 66; the dash refill never shown); row 43 on L3 / L6 is a check
  that should SKIP (their bosses are not walk-ins: the Wyrm kept, L6's old Stag); rows 18 / 19 were never proven GREEN in a Player
  (NIGHT35 close); row 15: n37-spire has the same 6 pads as n33-spire (the data is not lost). Row 24 on level 2 is the worry. Sent to K
  (the verbs reel's owner): a verdict per row, real loss vs reel artifact, with the control.
- S (22:48): round 3, the casual player (49128643): before it, a player who only sprint-glides (10.3 m/s) could never catch the Stag
  (23.5 m/s). Now: a look-back (> 25 m behind: half speed until you're back in its slipstream in its lane), held to 0.92 of your speed
  in your slipstream, floes every 2.4 s, leg 3 opens with the charge. `stag37 casual`: slipstream 10/10, catch 7.7-13.9 s, worst
  re-catch 4.6 s, killed 10/10 p50 27.0 s, max 37.0 s. Skilled band p50 20.9, p90 24.8 s, 0.0 % still. S done.
- O (22:55): the title wait (8963f139) and `loom37 camp` (b10645e1): a camper on a tower is severed at 4.00 s (leg 1) / 3.00 s (leg 2),
  and the web wall fires in leg 2 (tell 4.39 s, solid 5.80 s): the rule is reachable; the band's pilot never camps. O done.
- W (23:07): the climb (23840063: a per-panel scripted climber, 10/10 to the cap, median 21.0 s) and the fight end to end (f08a37da,
  672163b4, 8e22f589). Found: no WICK attack had ever fired (ids never matched the config keys); a chained climb hit 20 m/s and fell
  short (the stack's hot air caps horizontal speed at 12.4 m/s); rise 2.8 m with 15 panels; ledges 2.6 m arcs at 7.0 / 22.7 m; a cap
  landing arc at 38.4 m; embers home at 70 deg/s; any KESTREL shot from a ride through a cracked plate is the pierce (a charged one
  drains double); the finish is a dash into the stalled Wick; slag throws you to the leg's checkpoint. Band 10/10 killed, p50 34.3 s
  INCLUDING the ~21 s climb, 0 % still damage.
- DECIDE37 D-7 (the lead, for GG): THE WICK's band is the climb + the fight, ~30-35 s for the bot (rejected: shrinking the 40 m climb
  to fit 15-22 s; the band was set against health-sponge fights, and here the time is the climb, the fight's movement).
- Build point 3e3d9dcf: wick37 ON (every n37 row ON). n37-r6 built 23:10. Reel wick37 (1; budget 16 / 26): space GREEN (15 / 15
  panels trimmed); fight RED (not killed; leg 2; tells ember 1.20 / flare 1.40; parries 10, cracks 10, pierces 1, windows 4; the reel's
  pilot is fed once per frame, not per sim step). 0 NRE.
- Frames: the Wick reads as a plain beige drum, its core hidden; the window reads (plates open, a cyan seam) but weakly; the flare's
  red half-wall shows as a sliver; the SURVIVAL banner. Sent to W: per-step pilot, a campaign run, the core and the flare's look.
- K (23:14): every RED verbs37 row judged with a headless probe (`verbs37probe <level> [--off37]`) and the old levels as a control: NO
  verb lost. Row 24 (level 2): the reset left the gauntlet in hand, where F is the gauntlet's swing by design (same on n35-iceline
  --off37). Rows 13 / 18: the longest wall was over a rehearsal room's drop. Row 15: the pads' room zones were still locked when the reel
  teleported (a locked zone sleeps its pad; same on n33-spire). Row 19: the check counted the fall after the rope lets go (same on
  NIGHT36's levels). Row 40: with the stamina bar on a dash spends the bar, not the pips (Mover.cs TryDash): why M's fix never took.
  Row 43: SKIP on levels without a walk-in boss, decided from the level data. Fixes in f37ebc4a. K done.
- W (23:20): the reel's part B steps the pilot per sim step and runs a CAMPAIGN run (b4d463b5); the look (03f044a1: narrower plates with
  real gaps, a brighter core at 78 % of the cage, plates swing 75 deg open in the window / stall, the flare lights half the wall at once
  plus a red sheet across the shaft). `wick37 band` (D-7): 10/10 killed, total p50 34.3 s, from the cap p50 15.4 s, 0 % still. W done.
- `wick37 all` prints RED (12): the strict per-gap rule "no gap makeable by a look-level leap" fails on all 12 panel gaps (level leap:
  2.4 m of the 8 m panel, +0.26 m; looking up: 7.2 m, +2.45 m). Accepted as W's person-first trade-off (a 2.8 m rise); the full-climb
  control (no look-up) still never reaches ledge 1. The check was not edited.
- Pre-freeze batch suite at f37ebc4a (23:17-23:28, 701 s, 1966 tests): 1964 / 1966. Failures: Lesson25Tests.TheOrder_FollowsTheBlocks
  (NIGHT33's / NIGHT34's pinned orders now see loom37 / crown37's swaps of n33-span / n33-spire) and Lesson25TextTests (S's rehearsal
  prompt 7 words, max 6). Fixed in e6902b00: the two pins run with N37 forced off (as NIGHT35 pinned NIGHT34) + a NIGHT37 assertion
  that each ON boss row swaps its level; the prompt "THE STAG WAITS. LEARN THE LANES.". Lesson25Tests 10/10, Lesson25TextTests 6/6.
  The lead's first patch put a `//` comment mid-line in S's generator and broke it; fixed with a block comment before the regen.

## Freeze (3:00) and verification
- Freeze e6902b00 (23:35). Every design.n37 row ON (boss37, wick37, stag37, loom37, crown37). Gates: dn-check 0, fingerprint EQUAL,
  --off37 EQUAL, --off36 EQUAL, golden 39 sets 0 mismatches 22/22; Editor compile 0 errors; harness `boss37 kit` GREEN, `stag37 all`
  GREEN, `loom37 all` GREEN, `crown37 all` GREEN, `wick37 all` RED only on the accepted per-gap rule (climb 10/10, band PASS),
  n36tap / n36core / boss36 all GREEN (NIGHT36 intact).
- n37-r7 built 23:38 (31 s). Freeze reels (8; 24 / 26 for the night), 0 NRE / 0 exceptions on all eight: wick37 space GREEN + fight
  GREEN (leg 3 reached, NOT killed), stag37 RED (killed 24.2 s; floes 0 this run), loom37 GREEN (killed 21.7 s, 6 tears, 0 puppets),
  crown37 GREEN, verbs37 L1 27/28 (row 43's walk), L2 30/30, L4 32/32, L5 32/33 (row 15 pad, same on n33-spire). NIGHT37-PROOF.md.

- The freeze suite (e6902b00, started 23:50): stopped by Claude Code for critically low memory; the lead closed the orphaned batch Unity
  (pid 159272, command line `-runTests` on the batch copy, started 23:50) at 23:56 (free 4.5 -> 17 GB). Not re-run. The pre-freeze suite
  (f37ebc4a) is the night's suite line: 1964 / 1966, both failures fixed and their tests re-run in the Editor.

## Close table
| card | result | measured reason |
|---|---|---|
| K1 the Boss37 kit | DONE | `boss37 kit` GREEN 11/11 with numbers; Boss37Tests 7/7 in the Editor; the view helpers; used by all four bosses; verbs37 + the probe that judged every RED row (no verb lost). |
| W1 THE WICK | SHORT | The stack, the slag, the hot wall, the climb (10/10 to the cap, gaps measured for a person), the fight on Boss37 (harness 10/10 killed, D-7 band PASS, 0 % still); frames iterated once (r6 beige drum -> r7 flare lighting the shaft, the stall's core). SHORT: the kill not reached in a Player run (leg 3 reached), the window frame faced the wall, and `wick37 all` RED on the strict per-gap rule (accepted). |
| S1 THE STAG | DONE | Killed in the Player at 24.2 s (r7) and 35.1 s (r4) with the gauntlet; band 10/10 p50 20.9 s; a casual player catches it (10/10, max 37 s); per-run seeding (rule 6); frames iterated (gauntlet in hand, floes shown on r4, kick back 1.0 s). r7's RED is the floes not dropping in that run. |
| O1 THE LOOM | DONE | Killed in the Player at 21.7 s / 22.0 s with 6 tears and 0 reel puppets; lash fixed from 0.38 to 1.20 s; camp rule proven; frames iterated (the canyon title, the tear, the kill). |
| C1 THE CROWN | DONE | Killed in every Player run (r1, r3, r5, r7); every attack check PASS; every leg meets a cut and a blade ring; band p50 21.93 s; frames iterated (the yellow tell, the real pass frame, the campaign run). The reel's passes are puppets (the timing is the harness's). |
| L0 lead | DONE | Shared commit f79cf1c7 (gates EQUAL), the four view hooks, VERBS rows 44-47, rule 6 caught (constant seed), the casual-player check, the pre-freeze suite's 2 failures fixed, 24 / 26 reels, frames looked at every round. |

## verbs37 (every docs/VERBS.md row; one Player per boss level, n37-r7)
- n37-foundry (L1): 27/28; RED row 43 (the reel's walk never entered the stack; THE WICK's reel proves the walk-in: feet 0.000 m).
- n37-iceline (L2): 30/30. n37-span (L4): 32/32. n37-spire (L5): 32/33; RED row 15 jump pad (the same on n33-spire --off37: not a NIGHT37
  loss).
- Rows 44-47 (the climb, the slipstream, the thread tear, the ring pass): performed by each boss's own reel (wick37 leg 3 reached by the
  climb; stag37 slipstream 4.2 s + 2 leg punches; loom37 6 tears; crown37 6 passes).
- Levels 3 and 6 (n35-kiln, n35-finale) on n37-r5: boot with 0 NRE; their REDs (40, 43, 19) judged reel artifacts by K and fixed in
  f37ebc4a; not re-run on n37-r7 (reel budget).
