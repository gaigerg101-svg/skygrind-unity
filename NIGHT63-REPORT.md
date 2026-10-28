# NIGHT63-REPORT: THE BOSS NIGHT (2026-10-22 22:36 to 2026-10-23 07:4x; 8 hours planned, branch A)

GG asked (2026-10-22 ~22:11): lead with the bosses ("boss animations are too rigid"), make the effect colours line up, be honest and
critical. Plan: NIGHT63.md. Log: NIGHT63-LOG.md. Slate: Logs/n63/slate.json. Your calls: DECIDE63.md (untracked).

**The short version:** six rows turned ON by the rule (a blind SPOT naming the change, then a critic PASS): **the bosses move**
(bossMotion63: wind-ups, flinches, a stagger slump, idle breathing on all five), **two boss attacks that gave no readable warning now do**
(bossTell63: a red column through the wyrm's spit and the crown's crownFire wind-ups), the boss rule line on time (bossRule61), one line of
text at a time (text61), one lesson per technique (text63) and an enemy in your face turning see-through (nearFade63). Seven slices are
SHORT with their numbers (the boss colours, the colour table, the exit door twice, the presses, the same-run tool, the camera measure) and
the eased eye waits for your A / B. The movement and the rooms were not tonight's lead and are still the game's two worst things.
**The close ran late:** the usage limit (a monthly spend cap) stopped the session from ~05:23 to 07:41, so the close window passed and the
tag went on at CAP (section 8; DECIDE63 D-16). Afterwards (07:55-08:17) the three lost steps ran on the tagged code, all GREEN: the
n63-freeze build with verbs37 x5 27 / 29 / 31 / 31 / 31 and 0 errors, the smoke "DONE: all steps passed", the flow gate "gates: all
GREEN 08:17:31". **Play Builds/n63-freeze.** gg-proxy's ranking of the shipped frames ran after too (section 1): nothing on the level
frames got better, and 11 of 18 boss shots still show no readable boss.

## 1. What is still not good
**gg-proxy's ranking of the shipped frames** (Logs/n63/gg-proxy.md; run after the close, the first attempt killed by the usage limit;
n63-freeze's verbs37 frames and n63-b12's boss shots, whose boss code is the shipped one). It saw **36 frames, short of its brief**
(16 of 30 level frames, 18 of 22 boss shots, 2 kiln entries: many image reads came back "request limit"), so every count is out of what
it saw. Its top 8, in GG's words:
1. **The rooms are still big empty boxes** (7 asks; "many of them feel very boring", 2026-10-09): 12 of 16 level frames, e.g.
   Logs/feel/n63-freeze/vk/verbs37/f1650.jpg, vf/verbs37/f0100.jpg. The same as at the start of the night.
2. **The movement is choppy and the wall ride clunky** (7 asks; "the movement needs to feel cleaner, more fluid, and less choppy",
   2026-10-20): a frame cannot show it (eye63 waits for your A / B).
3. **In the boss fights you can't see the boss or tell what it wants** (7 asks; "hard to understand what to do", "boss animations are too
   rigid"): 11 of 18 boss shots show no readable boss: THE CROWN's ring fills the screen with the gun over it (all 4 crown shots), the
   wyrm is a flat green square or out of view (all 4), THE LOOM a wire shape inside a cyan sphere (all 3). In 4 shots the same order is
   printed twice at once, small under the health bar and large in the centre ("STAGGERED: HOLD LEFT CLICK, SLUG IT"), with text61 ON.
   Whether the motion reads as less rigid: a frame cannot show it.
4. **Effect colours still don't line up, and the bosses wear the player's cyan:** the danger-red line down spire's exit on 5 of 5 spire
   frames; THE LOOM, THE CROWN's ring and THE WICK's open core drawn in cyan (6 boss shots: their own colours, bossColours63 is OFF); the
   wyrm's head lime.
5. **Lamps float in mid-air:** 10 of 16 level frames.
6. **The fight is hidden:** 9 of 16 level frames show no enemy while the counter reads 8-16 LEFT.
7. **Kills don't satisfy:** a frame cannot show it.
8. **The style meter sits at D / DULL and the score at 500:** 500 in 16 of 16 level frames, D in 10 of 34.
Below the top 8 it lists text stacked on the room banner, a noisy red-brown border on 5 boss shots, and the near enemy's screen-door
dither (vf/f0400). Against its start-of-night table: **nothing on the level frames got better.**

The critics' points still open tonight, each with its file:
- **The boss motion is uneven.** THE WICK slumps and keels where you see it (52,428 px; the blind spotter: "tipped over sideways and
  slumped low"), THE ICEBREAKER and THE CROWN move big, but THE KILN WYRM barely moves on screen (its drawn boxes are its hitboxes, so its
  guard is 0.375 m: flinch 1,076 px, stagger 16,649 px) and THE LOOM's stagger is 40,650 px (Logs/n63/verdict-B1.md). Three earlier SPOTs
  on other poses named "the big cyan glow", "the floor", "a cyan shape", not the boss (Logs/n63/spot/B1.txt).
- **A shot at the drawn Wick may miss:** during its OPEN window the drawn orb can sit up to 1.53 m from the centre of its 1.7 m hit
  sphere (D-12); not checked in a Player.
- **The spit's new warning is never on screen in a reel frame** (the wyrm's tell shot faces a wall) and clears the 300 ms bar by
  125-192 ms only; the wyrm frame shows "STAGGERED: HOLD LEFT CLICK, SLUG IT" during the spit's wind-up (D-15).
- **The wyrm's rule line contradicts its own bar hint** ("Swing the gauntlet ... at its charging head" vs "PUNCH THE HEAD TO STAGGER IT";
  D-4), and on the reel's frame the player holds the THUNDERCLAP, not the gauntlet.
- **Boss colours are not one language yet:** the crown's tell is red now, but its weak point mixes cyan, magenta and rose; the wyrm's weak
  point is half cyan, half green under its sulphur grade (B3 SHORT, Logs/n63/b3-roles-b11.txt).
- **Kiln still teaches seven lines in one run** (text63 withholds three restatements; Logs/n63/verdict-T63.md), and a queued lesson can
  still show in the next room (D-14).
- **The exit doors are still not bright enough to read as the way out:** exitFrame61 own gain 1 / 5 rooms after its brighter round (and it
  lit the gun); exit63 (the doorway turning cyan) 0 / 5.
- **Colour roles hold on two levels of five:** colours63 drops the off-palette share by half on kiln and span only; iceline's blue and
  spire's teal are the levels' own light (D-9).
- **Kiln's presses still read as columns** to a blind eye after two designs (S7).

## 2. What you can see tonight (play Builds/n63-freeze)
Printed at the close from both config copies (byte-identical): n61.bossRule61 true, n61.text61 true, n61.gunYellow61 true (NIGHT62's),
n63.bossMotion63 true, n63.bossTell63 true, n63.nearFade63 true, n63.text63 true; n61.exitFrame61, n61.fixture61, n63.bossColours63,
n63.colours63, n63.press63, n63.eye63, n63.exit63 false.
- **The bosses move** (design.n63.bossMotion63; off: `-skygrindNoBossMotion63`). Frame: Logs/n63/spot/bgfuxe (A = ON, THE WICK's
  stagger, n63-b10; contact sheet Logs/n63/sheets/bossMotion63.png is from b7). PLAY: level 1's THE WICK. Before it strikes it leans back
  (every wind-up starts 250 ms or more before the hit); each hit makes it flinch; when you stagger it, it keels over sideways and slumps
  low, alternating sides; standing, it breathes. All five bosses carry it (view only: hit frames and fingerprints unchanged).
- **Two boss attacks now warn you in red** (design.n63.bossTell63; off: `-skygrindNoBossTell63`). Frame: Logs/n63/spot/dbngue (A = ON,
  THE CROWN's crownFire wind-up). PLAY: level 5's THE CROWN: before crownFire a see-through red column rises around it 1.3 s before the
  hit; level 3's THE KILN WYRM: the same column through its spit's wind-up (0.4-0.5 s). Only those two attacks: the before-measure found
  every other attack already warned in time and in the danger hue, and they are untouched.
- **The boss rule line on time** (design.n61.bossRule61; off: `-skygrindNoBossRule61`). Frame: Logs/n63/sheets/bossRule61.png; SPOT pair
  Logs/n63/spot/lfzivb (A = ON). PLAY: start any boss fight (level 1's THE WICK is the first). Within 0.47 s of the name card, one line
  under it says what the boss wants ("Swing the gauntlet LEFT CLICK at its charging head." on THE KILN WYRM); it holds about 4.5 s, and the
  bar's own hint waits while it stands. Measured 1.000-1.017 s alpha 1.00, 40 px em at 1080p, on all five bosses (n63-b1).
- **One line of text at a time** (design.n61.text61; off: `-skygrindNoText61`). Frame: Logs/n63/sheets/text61.png; SPOT pair
  Logs/n63/spot/ctjtca (A = ON). PLAY: level 3 (kiln). Entering a room, the room's title shows alone; the instruction card waits until the
  title clears (0 of 5 entry frames stacked vs 4 of 5 off, n63-b3).
- **An enemy in your face turns see-through** (design.n63.nearFade63; off: `-skygrindNoNearFade63`). Frame:
  Logs/n63/sheets/nearFade63.png; SPOT pair Logs/n63/spot/sbwoqj (A = ON). PLAY: let a husk walk into you on level 1, or fight THE
  ICEBREAKER (level 2) up close. Within 0.9 m the enemy becomes an even dotted screen with its rim kept, so you see the room through it;
  THE ICEBREAKER's body no longer blacks out the camera at 1.7 m (590,004 px on the husk pair; 936,071 px on the Icebreaker's).
- **One lesson per technique** (design.n63.text63; off: `-skygrindNoText63`). Frame: Logs/n63/spot/emhvvl (B = ON, kiln kb2 clear).
  PLAY: level 3 (kiln). The boost is taught once in the first words you reach ("Jump SPACE, aim down, fire LEFT CLICK."); its
  restatements ("Midair ...", "Fire ... down to lift off.", "Boost: ... twice.") no longer appear, the step-by-step lines still do, and a
  lesson waiting for a room title is never lost.

## 3. Bugs fixed, tests and tools proven
- **Tests:** 28 N63 classes, 170 tests (N63Shared 3, BossRuleTime 6, BossClock 7, SameRun 10, SameRunGame 10, BossMotion 15, B1d 5, Reach
  4, BossCue 14, BossTell 9, Colours 10, BossColours 11, PinHint 4, BossRuleHint 2, RimRead 2, FocusLock 3, ModeList 2, GunFamily 2,
  NearFade 5, NearHide 5, ScreenDoor 4, Press 8, Switch 2, R1 2, Text 8, Eye 7, Exit 7, Review 3), each seen RED at its test-first pick
  (all but N63Shared, written with A1's block and GREEN from its first run) and GREEN on its last Editor run (`bash Tools/unity-loop.sh tests <class>`, Logs/n63/pick-*.txt). They were run class by class at
  their picks, not together on the final code: after the last code change only N63TextTests 8 / 8, N61TextTests 6 / 6 and N63ReachTests
  4 / 4 were re-run, with the gates all GREEN 03:50:14 (golden 0 mismatches, 22 replays exact, fingerprints plain and --off57..--off63
  EQUAL; Logs/n63/revert-t63c.txt). The full Editor suite was not run (MapChoiceTests can wedge the Editor, T-147).
- **The fresh review** (skygrind-reviewer, Logs/n63/review-n63.md): with every NIGHT63 row OFF nothing changes what the game does or draws
  except one deliberate focus-pause change; 0 must-fix, 8 should-fix, all 8 fixed or checked: S1 the Icebreaker's view hook overwrote its
  leap height and recoil (drawn on the floor while its hit spheres were in the air), S2 colours63's scans on the wall clock, S3 three hue
  band sets disagreeing (now one: the colour table's), S4 the held ON frame overwriting the shot the sidecars describe, S5 the focus pause
  still fires in a Player (b7: "paused on the focus loss True"), S6 the lamp meter's shrinking denominator, S7 the late pull turning the
  wyrm's yellow punch window red, S8 boss63's wall-clock waits.
- **D1, the same-run pair:** one reel run captures each shot with a row on and off (`-skygrindSameRun63 <row>`). Not exact (planted no-op
  0-16 px, plant 100-111 px; D1 SHORT) but 4-5 orders cleaner than NIGHT62's two-run pairs (139,791 px of drift at kb3); every SPOT from b3
  on used it with the residual stated.
- **Tools proven tonight:** boss63 (each boss's attack clock, wind-up, flinch, stagger and each part's offset from its rigid pose, on the
  reel's fixed clock); the boss cue meter (each attack's drawn cue lead and hue, by the sim's tell kind); n63-b2-fixlist.cjs (the fix list
  computed from before logs); n63-c1-meter.py (off-palette share, planted GREEN); n60-v-lamp.py's SHOTS line; n63-m1-chop.cjs and its
  `--pair` (the eye's own residual per event, planted RED); n63-sheet.py (contact sheets, self-test 9 / 9); n63-spot-pair.cjs;
  n63-b3-roles.py (per-boss role classes of a same-run pair, planted 11 / 11); boss63's reach line (b12: no boss's drawn weak-point
  centre outside its hit sphere; the wick 1.57 m of 1.70).
- **The crown's "drawn off" flinch** (B1's round 1) was the reel counting two damage events on consecutive frames before the LateUpdate
  draw, not the game (FlinchBook63; 49 / 49 drawn on a plain run).
- **A memory near-crash avoided:** at 03:08 another session's runaway pytest took commit free to 1.0 GB (the PC crashed this way on
  2026-10-22); stopped, 22.4 GB after (NIGHT63-LOG.md).

## 4. What failed, with its numbers
- **M1 the camera's chop (SHORT):** measured, not a change. Fixed-clock path rms 1.90-2.61 cm, 13-16 eye-snap events per 30 s reel (mostly
  the slide's 0.70 m eye ease and dip); the bench p50 3.70 ms, p99 10.10 ms, max 32.10 ms, spikes in main-thread engine time. Two GATE
  rounds FAIL (counting frames as events; the reel's 0.2 m placement read as step-ups).
- **P1 the exit frame (SHORT):** own gain 0 / 5 rooms at 1.5x before, 1 / 5 after a brighter round that broke the GUARD and lit the gun;
  reverted.
- **D1 same-run pairs (SHORT):** see section 3.
- **C1 the colour table (SHORT):** off-palette share drop foundry 13.8 %, iceline 13.5 %, kiln 91.9 %, span 80.5 %, spire 30.3 % (wants 50 %
  on every level); 392 hand-written colours found, the top offenders routed.
- **S7 kiln's presses (SHORT):** two designs, 292,689 and 468,517 px changed, the spotter both times: "columns".
- **EXIT63 the doorway turns cyan (SHORT):** own gain 1.00-1.05x, 0 / 5 rooms; its 10 m SPOT pose changed 14,648 px.
- **EYE63 the eased eye (BLOCKED, for you):** measure MET on a Player pair (64 of 64 slide and landing events halved, view only 1.00 mm,
  Logs/n63/eye63-pair-b8.txt), but a camera ease has no still frame to SPOT: it waits OFF for your blind A / B (D-10).
- **B3 boss colours (SHORT):** round 1 GATE FAIL (the crown's tell amber, the wyrm's weak point never cyan); after its fix round, on
  n63-b11 by its own role meter: the wick and the loom PASS, the Icebreaker unchanged, the crown FAILs (weak point in 2-3 colours incl.
  magenta) and the wyrm FAILs (weak point 36,491 px cyan + 27,240 px green); the crown's tell is red now (amber 34,712 -> 223 px).
- B1 and T63 each failed their first GATE and passed the second after one fix round; B2 passed its first.
- **T63c (a fix to text63 after its PASS, never GATEd):** on n63-b12 it dropped iceline's only gauntlet lesson ("One LEFT CLICK swing
  drops a tank.") when the player left the room early; reverted (f93c2de0, 829b8394), text63 ships as judged; b13 shows the lesson again.
- **The close itself:** not run in its window (the usage limit, section 8); its three lost steps ran after it, all GREEN.

## 5. Not checked
- Real frame pacing with VSYNC on (the bench forces it off).
- Any ON row in a full campaign run by a human; the boss rows only in the boss63 reel's 30 s after the rise.
- Whether a campaign run reaches level 3's boss holding the gauntlet (the wyrm's rule line).
- 14 of the 30 level frames and 4 of the 22 boss shots gg-proxy was asked to read (image reads refused at a request limit; section 1).
  (The smoke, the freeze build and the final flow gate, missing at the close, ran after it, all GREEN: section 8.)
- Whether a shot at the edge of THE WICK's drawn orb misses while it slumps (the reach line measures centres only).
- THE KILN WYRM's red spit column in any reel frame (its tell shot faces a wall); whether the red columns read as clutter in a real fight.
- All 170 N63 tests together on the final code (section 3).

## 6. Your calls (DECIDE63.md)
D-1 yellow = the parry tell only; D-2 / D-10 which "choppy" you feel, and eye63's blind A / B; D-4 the wyrm's line vs its bar hint; D-6
the exit door needs a different design, not more glow; D-7 text61's open points; D-8 weak points cyan on every boss; D-9 iceline's blue and
spire's teal (keep or re-light); D-11 B2's fix list; D-12 THE WICK's raised guard (its drawn orb can sit 1.53 m from its hit sphere's
centre); D-13 B3's restated pins and the crown's gems sharing the pass ring's cyan; D-14 text63's open points and the trade T63c showed (a
late lesson shown in the next room, or dropped); D-15 bossTell63's open points and the staggered wyrm that still spits (enemy design);
D-16 the close at CAP (play b13); plus the forge's: what "rigid" means to you, the colour roles, rooms vs movement next.

## 7. The next prompt's seed
**Plan vs actual per slate entry** (NIGHT63.md section 6's windows; the clock from t0 22:36:01; NIGHT63-LOG.md):
| id | planned | ended | outcome |
|---|---|---|---|
| M1 | 0:40-2:00 (lead) | 0:24 | SHORT (two GATE rounds FAIL; measured only) |
| P2 | 0:40-2:00 (G) | 0:55 | PASS, ON (bossRule61) |
| P1 | 2:00-4:00 (lead) | 1:24 | SHORT (own gain 1 / 5 after its fix round) |
| P3 | leftover 2 | 1:34 | PASS, ON (text61) |
| D1 | 0:40-2:00 (D) | 1:44 | SHORT (residue 0-16 px; used for every pair after b3) |
| C1 | 2:00-4:00 (D) | 2:34 | SHORT (2 / 5 levels) |
| EXIT63 | not planned (V) | 3:33 | SHORT (own gain 0 / 5) |
| S7 | leftover 6 (G) | 3:34 | SHORT (two designs read as "columns") |
| EYE63 | not planned (D, D-10) | 3:43 | BLOCKED (MET; needs GG's A / B) |
| C2 | leftover 7 (V) | 3:49 | PASS round 2, ON (nearFade63) |
| T63 | not planned (G) | 4:39 | PASS round 2, ON (text63) |
| B1 | 0:40-4:00 (V) | 4:41 | PASS round 2, ON (bossMotion63) |
| B2 | 2:00-6:00 (G) | 4:44 | PASS, ON (bossTell63) |
| B3 | 4:00-6:00 (D) | 4:54 | SHORT (GATE round 2 FAIL) |
Every slate entry ended by 4:54; the slate and the leftovers ran dry at 4:43 and the rest went to fixes of ON rows (T63c, tried and
reverted), measurement (B1e) and the close's preparation.

**The forge experiment (visible by design, rule 8):** 21 blind SPOT runs; **15 named the slice's own visible change (71 %)**: P2, P3, B1
x2 of 4, B2 4 / 4, B3 2 / 2, C1, C2 2 / 2, T63 2 / 2; the six that did not: P1 (the gun first), B1 x2 ("the floor", "the big cyan glow"),
the Icebreaker half of C2 ("a cyan shape"), S7 x2 ("columns"). 6 GATE PASSes out of 14 slate entries. Changed pixels at each slice's
decisive SPOT pose (of 2,073,600): **B1 52,428 (2.5 %), B2 787,061 (38.0 %), B3 126,327 (6.1 %), C1 131,656 (6.3 %)** (the four rule 8
binds); P2 20,382 (1.0 %), P3 37,767 (1.8 %), C2 590,004 (28.5 %), T63 11,949 (0.6 %), S7 468,517 (22.6 %), P1 60,715 (2.9 %), EXIT63
14,648 (0.7 %). Pixels did not decide a PASS: S7 changed 22.6 % and failed (the wrong thing read), T63 changed 0.6 % and passed (the
right thing read). Same-run pairs made the SPOTs fair (D1's residue 0-16 px against two-run drift of 139,791 px).

**Leftovers, ranked:** (1) B3's wyrm (aim the compensated weak point 25-30 deg bluer, D-B3c.md) and the Icebreaker's route on screen,
then re-judge B3; (2) the rooms (gg-proxy #1); (3) the movement feel, with eye63's A / B first (D-10); (4) the exit door, a new design
(D-6); (5) C1's lit surfaces (D-9, GG's call first); (6) text63's stale queue, with GG's answer to D-14's trade; (7) MapChoiceTests in
batch mode (T-147).

**Aged threads:** docs/memory/THREADS.md (T-151 the same-run toggle's residue and one-sample shift, T-152 the chop and eye63, T-153 one
colour language, T-154 the shipped rows' open points; T-148 / T-149 / T-150 carried).

**Three research questions:** (1) how action games keep a boss's hit volume and its animated body in step (a shot at the drawn part:
recoil inside the collider, or a moving hitbox in the sim); (2) how a colour grade keeps one role hue (cyan) constant across scenes with
different grades (grade exemptions, a post-grade overlay, or per-scene role tables); (3) how tutorials pace lessons that wait for a
title (drop, defer to the next room, or re-trigger on the next use of the verb).

**Commands that worked:** `bash Tools/night/n63-reels.sh <tag> <side>,<level>,boss63,-skygrindSameRun63,<night.row>` then
`python -I Tools/night/n62-gun-yellow.py --pair <side>-on/... <side>-off/... --rect 0,0,1,1`; `node Tools/night/n63-spot-pair.cjs <id>
<on> <off> <tag>` then a fresh skygrind-spotter; `node Tools/night/n63-freeze-rows.cjs "<night.row>=<reason>"`; test-first picks
(`git cherry-pick <RED>`, the Editor RED, then the GREENs); a plain run beside a same-run run to separate the toggle from the row.

## 8. The close (NIGHT63-LOG.md "Blocked" and "The close")
- The usage limit stopped the session from ~05:23 to 07:41 local; on resuming, `node Tools/night/n63-clock.cjs` printed "clock n63:
  9:05 / 8:00 ... phase CAP ... commit what is done, log the rest under Blocked, end". By NIGHT63.md section 9, whatever is left at 8:00
  is logged under Blocked, then (h).
- (a) no GATE was due (the last reported at 03:31). (b) the final `bash Tools/night/n63-gates.sh --flow`: **not run** (CAP); the last
  `--flow` "gates: all GREEN 03:32:11", the last plain gates "all GREEN 03:50:14" after the night's last code change. (c)-(e) the
  n63-freeze build, its verbs37 x5 and the smoke: **not run** (CAP); Builds/n63-b13 (0848b181, code-identical to the tag) has verbs37 x5
  27/27, 29/29, 31/31, 31/31, 31/31, RED rows none, NRE 0; the smoke: not checked. `git diff --stat ProjectSettings` after b13: GG's two
  files only, never staged.
- (f) `node Tools/night/n63-slate.cjs`: "slate: 14 slices: 6 PASS (passes true), 7 SHORT, 1 BLOCKED, 0 in progress, 0 todo".
- (g) this report, HANDOFF.md, the w63 harvest (docs/memory: ledger/wave-63.md, FEATURES, THREADS 60 lines, LESSONS 150, CORE 40; the
  `[close]` markers filled at the close), DECIDE63 D-12..D-16.
- (h) each row's `enabled` printed in both copies (section 2); committed with explicit paths (0ec78408); `git tag u3-n63` on it (`git
  tag -l u3-n63` printed `u3-n63`); the status check printed nothing; `n63-clock.cjs stop`: "clock stopped at 9:09 since t0". No agent
  and no Player alive.
- **After the close** (07:55-08:17 local, the clock stopped, on GG's "continue"; NIGHT63-LOG.md "After the close"): `bash
  Tools/night/n41-build.sh n63-freeze` at 0ec78408 (drift: GG's two files only); verbs37 x5 on it 27/27, 29/29, 31/31, 31/31, 31/31, RED
  rows none, NRE 0; `bash Tools/night/n56-smoke-steal.sh n63-freeze` (both planted focus steals "took True"): **"[smoke] 252.5s DONE: all
  steps passed"**; `bash Tools/night/n63-gates.sh --flow`: flow EQUAL to flow-base, golden 0 mismatches, 22 replays exact, fingerprints
  EQUAL, the Editor 0 errors: **"gates: all GREEN 08:17:31"**. The tag stays on 0ec78408 (every later commit is a log or a doc).
