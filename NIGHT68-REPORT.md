# NIGHT68 REPORT: THE BETTER LINE (8 hours in two work blocks; t0 2026-10-24 22:50:23; branch C, sim only; the minimum close)

Every claim names its command or file; anything not checked says "not checked"; every rebuilt level is **not checked in a Player**
(branch C: C: 11.4 GB free at t0). The log is NIGHT68-LOG.md. **Two usage stops:** about 1:13-4:00 (2:47) and about 6:24-9:01 (2:37);
the block-1 chain computed through the first; the second ate the play build and the full close, so this is NIGHT68.md section 7's minimum
close, started at 9:01 (Logs/n68/close-start.txt). No game code changed: only the harness (Tools/SimHarness), Tools/night and room files
behind OFF rows.

## 1. What is still not good
- **No room passed its GATE: 0 of 7.** M2 VERB (skip v2 pooled over seeds 1-20 vs mainBar; the latest round each room ran): kb3c ride
  0.99 vs 1.25 (r3), g8 ride 0.96 vs 1.25 (r3), g7 parry (melee only) 1.35 vs 1.32 PASS on its line (r3) but fresh 21-30 1.10 FAIL and its
  instrument not sound (below), kb3 launch 1.12 vs 1.27 (r2; its bar on the final reds 1.33), v2 launch 1.09 vs 1.25 (r2), kb2 parry 0.94
  vs 1.38 (r2), kb3d launch 0.91 vs 1.25 (r2). Every fresh set (seeds 21-30, never tuned on) FAILs: best g8 1.06 and g7 1.10 (r3).
- **The verb is still not the faster line, now measured with instruments that see the room.** Half of NIGHT67's failure was the
  instruments (fixed tonight: section 3); the other half is the rooms. The ride in g8 is a detour (r2's level review: the ride run 44.0 s
  / 242 m vs 34.7 s / 171 m on foot; every body comes to the near bank). kb2's shipped room (1.18) beat every rebuild. kb3d's launch
  reaches about a third of its hp (740 of 1060 hp is a rooted burrower or a charger thrown at x0.3: enemy design, DECIDE68 Q6).
- **gg-proxy's ranking** (Logs/n68/gg-proxy-close.md, from GG's recorded words, worst first): (1) four of seven rooms (kb3c, kb2, kb3d,
  kb3) keep 5-9 of the old room's blocks and all seven keep the old size, doors and spawn: "the old room with a patch", against GG's
  2026-10-09 order; (2) no room makes its mechanic the better way through; (3) tall posts with no job (kb3d's 12 m posts; g7's coal bin
  blocks 0 of 110 sight lines); (4) **things the player had are taken away**: g7 jams the gun (the melee-only rule) and loses its ride
  walls and pads, v2 loses its ride wall and pads, kb3 its four presses (GG 2026-10-11: "why we're removing abilities"); (5) pits on the
  walk (kb3d's moat catches the walk bot 34 / 17 times while the thrown bodies land on the bridge).
- **The noise is wider than the design changes.** kb3c's per-seed ratios run 0.67-1.80 (S); the same room file reads 0.10 differently with
  different rooms assembled beside it (R: kb3 r1d 1.22 alone vs 1.12 on r2's level, tools identical).
- **Rules that cannot be met as written:** the STYLE rule asks 91-94 % on levels 3 and 5 where the verb runs reach 77-84 % (kept: never
  loosened after a measurement; DECIDE68 Q8); a melee-only room cannot meet stall / air ceilings set from the gun room (g7 stall A 54-60 %
  vs 29.3: the walk bot stands next to a body to swing).
- **Hazards, again:** lava where the fight reaches it killed the walk bot (kb3d 4 / 3, 5 / 1, S's tree); dry pits on the walk catch the
  walk bot, not the thrown bodies; the room check walks through a 1.5 m dry pit (g8).
- **The night lost 5:24 to two usage stops,** and PC (GG's play build) did not happen: GG still has nothing new to play.

## 2. GG's play card
- **No play build tonight.** No room PASSed, and the second usage stop (about 6:24-9:01) ate PC's window (6:20); at 9:01 the clock was at
  CAP, where the guard allows no build. PC is SHORT. Every slot in Design/levels/n67/<level>/slots67.json stays `old`; the rows stay OFF;
  the shipped game is unchanged (n66-freeze is still the verified build).
- **What there is to look at (files, not a build; not checked in a Player):** the rebuilt rooms' files (Design/levels/n67/<level>/rooms/
  <id>.json), their cards (Research/2026-10-24-wave67/cards/<id>.md) and the sim's pictures (Logs/n68/r2-<level>/<id>/iso.png, plan.png;
  r3 for kb3c, g8, g7).
- **Still yours from before (about five minutes each):** Builds/n65-play-latest (level 1's rebuilt rooms); eye63's blind X / Y.
- **Questions (DECIDE68.md):** Q6 may kb3d's wave 3 change (enemy design)? Q7 keep the melee-only parry room (g7) idea at all, given it
  takes the gun away in that room? Q8 lower the STYLE rule for the next night? Q0 the disk (about 25 GB) so rooms can be seen in a Player.

## 3. The instruments proven (I1-I4, with their pairs)
- **I3 PASS** (Logs/n68/verdict-I3.md): the walk bot reads lava by height. Planted cases 6 / 9 -> 9 / 9 on main
  (Logs/n68/I3/main-lava-cases.txt); a deck 2 m over lava crossed (holds 479 -> 0), lava at foot height still refused; the whole-level walk
  changes only n52-iceline's g7 / g8; the flow base re-made (Logs/n68/flow-base-i3.txt).
- **I1 PASS** (Logs/n68/verdict-I1.md): the refused ride runs ride 0 (the game's own no-ride rule, Mover.SetNoRide38; no game code
  changed) and print `refused ride N`; the verb-aware pilot rides only line65's face, once per pass: kb3c rides 49 / 84 / 84 -> 10 / 0 / 0,
  g8 92 / 55 / 55 -> 7 / 0 / 0 (Logs/n68/I1/).
- **I4 PASS on round 3** (Logs/n68/verdict-I4-r3.md; rounds 1-2 UNSURE / FAIL): skip v2 pools seeds 1-20 (the M2 VERB line), prints the
  refusal and the leak per seed, the STYLE rule, M2 FRESH on seeds 21-30, `--room-file` / `--fresh` tied to the room and level files; the
  verdict and exit code follow M2; red-bars reads the pooled ratio; **every red re-made on the final harness and committed before r3**
  (122f83ad), the critic's own re-pool matching every line.
- **I2 SHORT** (round 1 FAIL, round 2 FAIL: Logs/n68/verdict-I2.md, verdict-I2-r2.md): the launch bound (hp above max(punch x 3, the
  largest non-parry punch)) and the parry hold are sound (shipped kb3 "a launched body that survived the punch" 1 / 3 -> 3 / 3; the
  swarmer-only room on main: 0 episodes, Logs/n68/I2/main-swarmers3.txt), but the M3 reader prints a pit PASS for a body that walked into
  the pit unlaunched (n65-verbs.sh:20, n68-gate-pack.cjs:190), and the fix was never run on the rebuilt kb3.
- **The g7 instruments:** the parry refusal narrowed and the glove swing-parry (PASS, Logs/n68/verdict-g7tool.md); the leak fix and the
  probe's swing (FAIL, Logs/n68/verdict-g7leak.md: the leak closes, 0 / 20, but the spit-tell rule refuses swings that cannot return a
  glob: 182 of 327 refused cases uncatchable in the critic's model).
- **The instruments' open points:** Research/2026-10-24-wave68/T/open.md (16, each with file:line and an estimate).

## 4. What failed, with numbers
The bar is max(1.25, the red's pooled ratio + 0.20) on the final reds (Logs/n68/red-bars.txt, committed 122f83ad). Rounds:
Logs/n68/r1-<L>/ (NIGHT67's r3 files: a baseline), r2-<L>/ (the builders' first files), r3-<L>/ (kb3c, g8, g7 only).

| room (verb) | red | bar | r2 M2 VERB | r3 M2 VERB | fresh 21-30 (latest) | why it is not a PASS |
|---|---|---|---|---|---|---|
| kb3c (ride) | 0.97 (12/20) | 1.25 (floor) | 1.09 | 0.99 (sets 1.21 / 0.86) | 1.03 UNSURE (6/10) | M2; the noise wider than the change |
| g8 (ride) | 0.63 | 1.25 | 0.80 | 0.96 (0.84 / 1.00) | 1.06 | M2; the ride a detour |
| g7 (parry, melee only) | 1.12 (gun room) | 1.32 | 1.35 FAIL on the leak | 1.35 PASS on its line (1.26 / 1.55) | 1.10 | fresh FAIL; the leak fix over-refuses (inflated); stall A 54.0 vs 29.3 |
| kb3 (launch) | 1.13 | 1.33 | 1.12 (vs 1.27) | - | 0.89 (r2) | M2; STYLE 81.9 vs 94.4; M3 RED |
| v2 (launch) | 0.87 | 1.25 | 1.09 | - | 1.04 (r2) | M2; STYLE 79.1 vs 90.8; stall 21.5 vs 19.0 |
| kb2 (parry) | 1.18 | 1.38 | 0.94 | - | 1.04 (r2) | M2: the shipped room beat every rebuild |
| kb3d (launch) | 0.89 | 1.25 | 0.91 | - | 0.94 (r2) | M2; M3 in a pit no; 740 / 1060 hp unlaunchable |

- r2's launch and parry numbers ran before the I2 parry fix and the g7 leak fix (verdict-I4-r3.md Q1): they cannot be compared with
  the final bars; all fail either way.
- r1 (NIGHT67's r3 files on the night's instruments): kb3c 0.96, kb2 1.18, kb3d 1.01, kb3 1.03, g8 0.94 UNSURE, g7 1.32, v2 1.02.
- The lead's own errors: the I2 pick missed one line (`--plant-only`), so main's first swarmer run was the real kb3 (verdict-I2.md);
  launch reds made before the I2 fix had to be re-made (verdict-I4-r2.md).

## 5. Not checked
- Every rebuilt level in a Player (branch C); verbs37, the freeze build and the smoke (CAP at the close: section 8).
- The final gates (`n68-gates.sh --flow` at the close: the guard's CAP; the last gates run: "gates: all GREEN 23:37:49", Logs/n68/I2/
  main-gates-flow.txt, before I1).
- The launch rooms (kb3, v2, kb2, kb3d) on the final harness in a round (only their reds ran on it).
- `--verbs65` walks on ride rooms before / after I1; the no-ride strip's margins on planted geometry.

## 6. Your calls
- DECIDE68.md: the forge's calls at GG's "you make the calls" (g7 melee only; no ride boost; no pit rule; no deletion; the resume hook),
  the run's calls D-1..D-6 (g8 stays ride; the resume after the stop; kb2's label; the STYLE rule kept; the reds re-made; g7 cannot
  pass), and the new questions Q6-Q8.

## 7. The next prompt's seed
- **The instruments are now mostly sound** (I1, I3, I4 PASS): start the next night from them, with T's open points first
  (Research/2026-10-24-wave68/T/open.md): the M3 reader that passes an unlaunched body; launch kills in a pit not counted; the parry leak
  without over-refusing (predict the volley with the game's lob); the ride fallback.
- **Fix the noise before the rooms:** 20 seeds cannot see a 0.10 change in kb3c (spread 0.67-1.80), and a room reads differently with its
  neighbours: measure each room alone on its level (a fixed neighbour set) or with more seeds.
- **Rules to settle before measuring** (GG's DECIDE68 Q7, Q8): the STYLE rule on levels 3 and 5; a melee-only room's own walk baseline.
- **The rooms:** a ride room needs a target the gun cannot clear from the floor (R); kb2's shipped room (1.18) is the better parry start;
  kb3d needs its wave 3 changed (GG); rebuilds, not patches (gg-proxy's #1); nothing the player had taken away.
- **The usage limit:** two stops ate 5:24; plan two blocks again, and put PC (the play build) before the second likely stop.
- **The disk:** still about 10-12 GB (DECIDE67 D-1): free 25 GB or the night is sim-only again.

## 8. The close lines and every ROW line
- ROW: n65.foundry65 enabled=false; SLOTS: a2=old a3=old a4=old a5=old b1=old a8=old b2=old
- ROW: n67.iceline67 enabled=false; SLOTS: g1=old g2=old g3=old g4=old g5=old g6=old g7=old g8=old
- ROW: n67.kiln67 enabled=false; SLOTS: kb2=old kb2c=old kb3=old kb3c=old kb3d=old
- ROW: n67.spire67 enabled=false; SLOTS: v2=old v5=old v6=old
- The slate: `slate: 13 slices: 3 PASS (passes true), 10 SHORT, 0 BLOCKED, 0 in progress, 0 todo` (`node Tools/night/n68-slate.cjs`).
- The close started at 9:01 (Logs/n68/close-start.txt): the minimum close (NIGHT68.md section 7). The freeze build, verbs37, the smoke and
  the closing gates were not run: the clock was at CAP, where the guard allows no build, reel set, smoke or test run (NIGHT68-LOG.md
  "## Blocked").
- The tag u3-n68 is made by `node Tools/night/n68-close-check.cjs --tag --minimum`; the clock stops after it; the final check prints the
  verdict.
- **The forge experiment (STATE.md + the resume hook):** Logs/n68/STATE.md rewritten 5 times (0:02, 0:30, 0:57, 4:02, 4:27). Minutes from
  each resume to the next useful tool call: after the first stop (4:00) the clock / git / chain read came at once, resumed agents by id at
  4:01-4:02; after the second (9:01) the close started at 9:01. No compaction happened in the lead's session tonight (the hook's print was
  not exercised); no agent id was lost.
