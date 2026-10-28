# NIGHT36 log: the overhaul, slice 1: movement is the weapon

t0 = 16:44:25 (`date`, GG's "now begin prompt"). Box 3:00 (19:44), hard cap 3:20 (20:04). Base: port/stage3 at u3-n35 (29c80815).
GG's request: Research/2026-10-12-wave36/GG-REQUEST.md; review: REVIEW.md; plan: PLAN.md; decisions: DECIDE36.md (D-1 A, D-2 depth
first, D-3 new boss models, decided by GG ~16:30).

## U3-49.1 Start (0:00-0:17)
- Editor: stopped, heartbeat live. Free memory 22.4 GB.
- The stray Player pid 115072 (Builds/n33-r4, started 2026-10-11 13:33 by a NIGHT33-era session reel; ours) was closed by PID.
- Builders launched 16:58 in worktrees: F a521d251fc7e6b188, P a375265dc4dbd3a52, M aad0f2db4334d59d3, L a8f50ea098d147aaa,
  B a68c1350d9f01114d (Tools/night/integrate.sh). Choice: launch before the shared commit so their reading overlaps it (rejected:
  launch after it, the n35 way; costs ~15 builder minutes each).
- Shared commit 46655b0f (17:01): N36Blocks (nine bits), tape format 19 with the NIGHT36 value as a VarI (nine bits do not fit the byte the
  older nights used; rejected: two bytes, a second format change later), design.n36 rows all off, --off36, the walkin36 gate
  (ICampaignWorld.BossWalkIn, default false), B's N36BossBegin first in BossRoomBegin, Curriculum25.N36Levels, the harness and reel
  partials. Gates: fingerprint EQUAL, --off36 EQUAL, --off35 EQUAL, golden 39 sets 0 mismatches, 22/22, dn-check 0, Editor compile 0
  errors, N36SharedTests 3/3, N35SharedTests 3/3, N34SharedTests 3/3, TapeTests 14/14, Lesson25Tests 10/10.
- 0a4a06b0: B's N36BossSystem hook first in BossRoomSystem (B asked: BossRoomDefeated must see its boss). dn-check 0, fingerprint EQUAL.
- Integrated early: P's tells36 shape (81eb2f23, for B), B's hall spec (08d1c40a, for L), L's beat sheet (2aa5179a). Forwarded by message.
- docs/VERBS.md: rows 9, 25, 27 marked changed by wallgrip36 / tap36; rows 40-43 (parry, launch, heal by aggression, the boss walk-in).

## Round 1 (0:17-1:00)
- F (17:25): tap36 (c8ed9c6e) + its reel (9bf0c1ad) + muzzle36 (6c105ab9). `n36tap` GREEN both guns: a 60 ms click fires at +0 steps
  (--off36: +7, on release); a 0.8 s hold fires at +0 and a charged shot at +96; CHARGING from +22 (0.18 s). Grenades not in muzzle36
  (the BOUNCER's view): listed.
- P (17:29): the core loop's sim (9f02a1b3). `n36core` GREEN: tells 1.00 s yellow; a parried glob returns and kills the spitter (freeze
  0.25 s); a punch 0.125 s early parries by the buffer; a dash-punch throws a husk 10.5 m into a wall burst; a standing punch no launch;
  husk into husk = 2 kills; heal +10 at 2.5 m, none at 12 m. Views next.
- M (17:33): wallgrip36 (167b763b), text36 (68915940), the ride trim (48e7e460). `n36move grip` PASS: a jump into a wall at 8.33 m/s
  rises +5.68 m before, +0.05 m after (tap ride); hold ride +0.99 -> +0.05; ride on 30 m 29.21 -> 28.95 m; chain speed 20.7 unchanged.
  M patched CameraFeel once with a python heredoc (CLAUDE.md rule 9 says scripts via Write): result checked by dn-check, noted.
- After each integration: dn-check 0, fingerprint EQUAL, --off36 EQUAL, golden 0 mismatches 22/22. Editor compile 0 errors (17:36);
  Text36LayoutTests 8/8, N36SharedTests 3/3.
- Build point b041bd42 (17:38): rows ON tap36, muzzle36, wallgrip36, text36, tells36, parry36, launch36, heal36. Build n36-r1.

- n36-r1 built 17:40 (43 s). Reels on n35-foundry (L1 not landed), 4 invocations (budget 4 / 22):
  - tap36 GREEN (the shot 1 frame after the press; hold: 1 press shot + 1 charged on release; CHARGING only from 0.35 s; tracer start
    to the muzzle star 0.00 cm). RED (-skygrindOff36): TAP RED, WORD RED ('CHARGING' at 0.1 s).
  - wallgrip36 GREEN (tap: rose +0.02 m, +1 s -0.30 m; hold: +0.05 m); RED rose only +0.13 / +0.46 m: the reel enters slowly and does
    not show GG's climb. Sent to M: a rising-entry case.
  - text36 GREEN (centre 0.620 down, 32 px; the top slot keeps the room name); RED: 24 px in the top slot, truncated "Let go ...".
  - crunch36, fist36, parry36: NOT WRITTEN yet (F, P working).
  - 0 NRE, 0 exceptions in every Player log.
- Frames looked at by the lead: tap36 press (flash star at the barrel on the shot's frame: PASS); release (a large barrel flash, the
  beam from the barrel to the target, CHARGED: PASS); text36 (below the crosshair, but no visible plate: the line's right end runs onto
  the pale KESTREL viewmodel and loses contrast: sent to M, a dark plate and a 44 % max width); stitcher volley (nails too thin to read
  in the frame: not checked further).

## Round 2 (1:00-)
- L (17:47): n36-foundry (bb50529d level + generator + n36-jobs GREEN with its selftest catching 3 planted errors; d387cb0c config +
  maps / unlocks; 638b2bbf ride panels `ride: true`). B's 8 m hall became a link room a7 + a 4 m door (levelgen's doorFit refuses 8 m).
  Top-down render looked at: one line of rooms, each with one job; the intake's trench is a 1.5 m void (safe).
- F (17:50): crunch36 (6be6851a: Core ImpactRules BodyHit tier 18 ms with its own 0.1 s/s share; punch 0.1 s; stagger + knockback;
  the KESTREL hammer), 4d702dc8 BOUNCER grenades from the muzzle. `n36crunch` GREEN (--off36 RED: body 0 ms, punch 60 ms). Gates EQUAL.
- Build point cbed2cbe: crunch36, level36 ON. n36-r2 built 17:54. Reels (3; budget 7 / 22):
  - level36 NOT WRITTEN (L asked to write it first). n36-foundry boots: 0 NRE, 0 exceptions.
  - tap36 GREEN again on n36-foundry; MUZZLE: nails off the eye line in 6 / 6 frames, offset 25 cm.
  - crunch36 GREEN: body 18 ms, stagger 11.9°, hammer 38°; punch 100 ms, 21.9°. Frame: a head-on back-tilt barely reads (sent to F:
    flash + squash + twist).
  - heal36 GREEN: +10 (hp 50 -> 61); the frame's cyan +10 reads.
  - parry36 RED: the spitter showed a tell but never fired (glob 0); launch36 RED: launched 1, bursts 0 (the flight frame shows the
    husk airborne). Both sent to P with the lines.

## Round 3 (1:16-)
- B (18:00): boss36 + walkin36 sim (2694e805), bossmodel36 + hall view + reel (243fea71), harness `boss36 level` (c393b4a2). `boss36 all`
  GREEN: walk-in moves the feet 0.000 m, slab 0.40 s, fight at 3.50 s; tells 1.20 s; camp slam lands; from L's hall the fight starts at
  0.81 s with no step over 0.41 m; band p50 20.8 s (19.8 s with every n36 row). The lead added the two view lines (SurvivalGame.Boss26.cs)
  and L's last-kill slow in the time layer (SurvivalGame.Arena36.cs) in 1 commit; gates EQUAL.
- L (18:01): the arena flow (a8555540: arena36 rooms only; switch-on 1.000-1.008 s, stagger 0.100 s, next wave 1.000-1.008 s over 3
  seeds; n35-foundry control 0 events) and Arena36View + the level36 reel (3e7d10c7).
- M (18:05): verbs36 (58f8dd42), the text plate + rising case + `ride: true` keying (a55d81d8), the walk bot on n36-foundry (3a773371:
  10/10, median 236 s; deaths a3 7, a4 1, a5 64 over 10 seeds: a5 not diagnosed, sent back).
- F (18:08): fist36 (0d4f1355), the head-on hit read + thicker nails (cdb039bb). P (18:11): tell poses (3ecb3ade), the reel fixes +
  drone orbs + charger / lancer tells + the husk early buffer (7c483ccc).
- After each: dn-check 0, fingerprint EQUAL, --off36 EQUAL, golden 0 mismatches 22/22; Editor compile 0 errors (18:06).
- Build point 8b81a5fa (walkin36, boss36, bossmodel36, fist36 ON), then n36-r3 rebuilt at 7c483ccc with P in. Reels (7; budget 14 / 22):
  - level36 RED (8/8 rooms, 24/24 frames, 4/4 arena rooms locked, bubbles in 3 of 4), boss36 GREEN (feet 0.000 m, tells 3/3 at 1.20 s,
    parry 1 crack 1, stun 1, punishes 2), tap36 / crunch36 / fist36 GREEN, parry36 RED (pose 0.31, spitter alive, freeze read 0),
    launch36 RED (the reel faced a wall 0.9 m away), heal36 GREEN, wallgrip36 GREEN, text36 GREEN, verbs36 26/27 (row 40 RED only
    because the dash pips were already full: spitter dead, health 50 -> 65). 0 NRE, 0 exceptions in all seven Player logs.
  - P reported "the Player's spitter tell never counted down" in r2 and made its reel drive the clock: the lead asked for the root cause
    (a real game must not have stalled spitters) instead of a reel that masks it.
- Frames looked at: the boss reveal (jointed body risen on its beam, name bar), tell-fan (yellow arm tips, plates open), tell-sweep (one
  red arm out), stun (slumped). Reads as a creature, not a block; but small in the 30 m ring, the core does not read as the weak point,
  and the stun reads only as a slump: sent to B (x1.4, a lit core in the tell colour, cyan stun flash). A survival "ROUND 1 IN 86" shows
  in the reel's boss frames: B to find whether it is the reel's run mode only. Level: a1 calm and the text plate reads; a4 the lone
  spitter on its perch under the prompt (the teaching shot); a3-b and a5-b look at blank walls and the room name lags a room: sent to L.
  Fist lunge reads as a cyan smear: sent to F.

## Freeze (2:00-) and verification
- After the last integrations (F 17a8b0b9 fist pass, M b432e08c bot / row 40 / rising, P 369f4faf + 660198ff reel root cause + lanes, B
  4b926daf + 3f6697b6 name card / lead / 1.4x / lit core / stun, L f53ad261 viewpoints / titles): dn-check 0, fingerprint EQUAL, --off36
  EQUAL, --off35 EQUAL, golden 0 mismatches 22/22, n36tap / n36core / n36crunch GREEN, n36move grip PASS, boss36 all GREEN (band p50
  20.1 s), Editor compile 0 errors. Freeze f53ad261; all 14 rows ON; n36-r4 built 18:46.
- P's root cause (369f4faf): not a sim bug. FeelReel.One freezes every enemy before a phase-J body (husks.frozen); P's reels never
  unfroze them. The reel-owned tell clock was removed; the sim counts again.
- M's a5 diagnosis: the autopilot's map has no lava; it walked into the pool. Pilot fixed: deaths 64 -> 2 over 10 seeds; walk 10/10.
- Freeze reels (8; 22 / 22 for the night): NIGHT36-PROOF.md. level36 PASS, boss36 GREEN, tap36 / crunch36 / fist36 / heal36 /
  wallgrip36 GREEN, parry36 + launch36 RED (the new lane setup placed no enemies), verbs36 26/27 (row 40's dash-refill sub-check),
  RED side as expected except wallgrip36 (the reel's entries never climb) and boss36 (its reel ignores -skygrindOff36). 0 NRE.
- No reel budget was left to re-run parry36 / launch36 after the freeze: they stay RED with their reason; verbs36 rows 40-41 are the
  Player proof of the mechanics.

- Batch suite at f53ad261: 1955 / 1956 (894 s). The failure: DeterminismTests.Core_CallsNoSystemMathTranscendental (Lantern36.cs:381
  Math.Asin). Fixed in 026eb43b (JsMath.Asin); DeterminismTests 1/1, gates EQUAL, golden exact, boss36 GREEN. Not re-built (n36-r4 is
  the freeze; the difference is the last bits of the fan's pitch).

- Players at the close (19:17-19:20): closing the stale pid 115072 at 16:45 had released NIGHT33's own reel loop (`n33-reels.sh n33-r4
  lookf hudc kit arenas shop bosses`, pid 121912, started 2026-10-11 11:34, hung on 115072 for ~29 h): it went on to its "shop" set
  (pid 139140, 16:45) and its "bosses" set (pid 124852, 19:17). Each was identified by its command line and parent (L-054), then the
  loop, its reel.sh wrappers and both Players were closed by PID. 0 SKYGRIND processes and 0 reel scripts remain.

## Close table
| card | result | measured reason |
|---|---|---|
| F1 the guns feel like guns | DONE | tap36 GREEN in the Player (the shot 1 frame after the press; off36: on release); muzzle36 tracer-to-flash 0.00 cm, nails off the eye line 6/6 frames; crunch36 GREEN (18 ms body, 100 ms punch, twist 8°, squash 0.15); fist36 GREEN (lunge 1.93x). Iterated once each on frames (head-on read; the cyan smear). Open: the fist reads as a box from behind. |
| P1 the core loop | SHORT | Harness n36core GREEN for every rule. Player: verbs36 row 41 launch PASS (2 kills from one dash-punch), row 42 heal PASS (+11 at 2.5 m, 0 at 12 m), row 40 parry: the glob returned and killed the spitter, health 50 -> 66, but RED on its dash-refill sub-check (pips were full). P's own parry36 / launch36 reels RED at the freeze (no enemies placed by the new lane setup); the tell pose (0.31 in r3, raised after) not seen in a Player since. |
| M1 walls grip, text you can read | DONE | wallgrip36 GREEN (rise 0.02-0.05 m; harness 5.68 m -> 0.05 m on GG's entry); text36 GREEN (0.620 down, 32 px, dark plate); ride trim on 4 / 4 panels; walk bot 10/10 on n36-foundry; verbs36 26/27. Open: the reel cannot reproduce GG's fast wall entry, so the Player RED/GREEN pair does not show the old climb. |
| L1 level 1 from scratch | DONE | n36-foundry from a beat sheet (one job per room), every solid job-tagged (n36-jobs GREEN, selftest 3/3 planted errors), arena flow (switch-on 1.00 s, stagger 0.10 s); level36 reel PASS (8/8 rooms, 4/4 arenas locked, titles 8/8, trim 4); frames looked at and iterated once (viewpoints, title lag). |
| B1 bosses: Boss36 + THE LANTERN | DONE | boss36 GREEN in the Player: walk-in moved the feet 0.000 m (no Respawn), tells 3/3 at 1.20 s, parry -> crack, stun, punishes; band p50 20.1 s (bot); frames iterated once (1.4x, lit core, stun burst). Open: the stun frame does not show the cyan core; no person's time; orbs mostly miss a rider; the reel's RED side is invalid. |
| L0 lead | DONE | shared commit 46655b0f + hooks 0a4a06b0 (gates EQUAL), VERBS rows 40-43, every round's frames looked at and turned into builder fixes, 22 / 22 reels, close docs. |

## verbs36 (every docs/VERBS.md row; n36-foundry, n36-r4)
1 PASS · 2 PASS · 3 PASS · 4 PASS · 5 PASS · 6 PASS · 7 PASS · 8 PASS · 9 PASS (rise 0.05 m) · 10 PASS · 11 PASS · 12 PASS · 13 PASS · 14 SKIP ·
15 SKIP · 16 SKIP · 17-20 PASS (not owned on L1, D-2) · 21 SKIP · 22-23 PASS (not owned) · 24 PASS · 25 PASS (on the press) · 26 PASS ·
27 PASS (1 normal + 1 charged) · 28-37 SKIP (not owned on L1 / not performed) · 38 PASS (text36) · 39 not checked tonight (pickup35
unchanged) · 40 RED (parry works; the dash refill not shown) · 41 PASS · 42 PASS · 43 PASS. Total 26/27 performed.

## L's beat sheet, read by the lead (Design/levels/n36/foundry.beats.md)
1.1 INTAKE (calm: step, slide slot, trench) · 1.2 KESTREL (three stand-ins in a line for the pierce; a husk pair down a ramp) · 1.3 RIDE
(a panel over floor, then over lava; husks on the only dry route where a launch sends them in) · 1.4 PARRY (one spitter alone on a perch,
no cover; then a pair) · 1.5 CRUCIBLE (lava centre, two panels, spitters on a deck, husks on the rims; 3 waves) · 1.6 REST · 1.7 HALL ·
1.8 THE LANTERN. Approved: each room has one job and the props serve it.
