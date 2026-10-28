# FORGE68 code scout: fix the pilots before the rooms

Read-only scout, 2026-10-24. Inputs: NIGHT67-REPORT.md section 7 (lines 111-124), the harness and Core sources, Logs/n67. No dotnet, harness,
Player or Editor was run. Every number below comes from a file and line named next to it. Anything I did not run is marked "not checked".
Paths are relative to the repo root.

## 0. The slate in one table

| fix | where (harness only unless said) | minutes | effect on gates |
|---|---|---|---|
| (a) ride pilot rides the line65 face only, once per pass, in the line's direction; the verb-aware run cuts off-line rides | Harness.N65.VerbPilot.cs:115-129, 224-244, 314-325; Harness.N65.Verb.cs:853-856 | 60-75 | none on --flow (VerbPilot only runs with --verbs65: VerbPilot.cs:67; the gate runs without it: n67-gates.sh:27). Every ride room's red moves |
| (b) launch targets only bodies that survive the punch, with a print per punch | VerbPilot.cs:197, 300, 362-369; Harness.N65.Verb.cs:218, 233 | 30-40 | none on --flow; launch reds move |
| (c) walk bot compares heights before the lava hold | Harness.N46.Flow.cs:444-450, 569-572 | 30-40 (+5 for the flow re-base) | the --flow base changes wherever a deck stands over lava; F1Input is also skip v2's walker (Harness.N65.Verb.cs:850), so lava rooms' reds move |
| (d) a parry room rule | Core / data: GG's call | 15 min (data only) to 3.5 h (new rule) | see section 4 |
| (e) gauntlet pilot (level 2 g1-g4) | harness | 120-150 | none on --flow |

Order: (c), then (b), then (a), then re-run the reds. The bars come from the reds (n67-red-bars.cjs:2-3; Logs/n67/red-bars.txt), and L-150 says
to re-run every red after an instrument changes. (e) comes after.

## 1. (a) The ride pilot rides any face to its far end

**Current behaviour.**
- The constructor keeps every box with `ride: true` and a `ride36` block as a face [axis, plane, normal sign, lo, hi, top], plus its zone
  (VerbPilot.cs:115-129). It never reads `line65`.
- Trigger (VerbPilot.cs:224-244): any face whose zone is the room (228). The feet must be inside its length minus 1 m (232), 0.2-2.5 m out
  on the ground or 0.2-1.6 m in the air (234), and more than 1.5 m below the top (234).
- Direction: whichever end is farther (`toHi = (hi - s) >= (s - lo)`, 238). The episode rides to 1.5 m from that end (240) and ends when the
  ride ends or after 3 s without one (319).
- After each episode there is 1 s of rest (`End()`, 168), then the pilot can trigger again. A player walking along the strip therefore rides
  again and again.

**g8 r3 data.**
- One face: box x -10..-9.4, z -246..-223, face E, plane -9.4 (Logs/n67/r3-iceline/n67-iceline.json, g8's only `ride` box).
- line65: (-6,0,-226) -> (-8.8,1.5,-228) -> (-8.8,2,-235.5) -> (-8.8,2,-242) -> (-6,0,-245). That is 0.6 m off the face, riding -z from
  -228 to -242 (same file, room g8).
- By the code's own arithmetic, the old trigger fires anywhere in a 2.3 m x 21 m strip (x -9.2..-6.9, z -245..-224). In its south half
  (z -245..-234.5) it rides north, against line65, for up to 20 m.
- Skip v2 totals: wall rides 92 / 55 / 55, median clear 47.4 / 33.2 / 33.2 s (Logs/n67/r3-iceline/verbs-skip.txt:121). The trigger held
  5054 steps in seed 5 (verbs-skip.txt:89; 87.2 s against 30.0 s).
- kb3c has two faces. One is the line's (x 10.4, face W). The other (x -7.4, face E) is on no line, and the pilot rides it too. kb3c totals:
  wall rides 49 / 84 / 84 (Logs/n67/r3-kiln/verbs-skip.txt:154; faces from Logs/n67/r3-kiln/n67-kiln.json).

**Second cause: the runs are not the same pilot.**
- The verb-aware run is the bare verb pilot. Only the refused-in and walker runs get the V65Filter wrapper (Harness.N65.Verb.cs:853-856).
- The filter's ride refusal is a backstop: on any wall ride it turns the stick away from the wall, so the Mover ends the ride
  (Harness.N65.Verb.cs:670-686, the away rule at Mover.cs:1581).
- The Mover rides any wall that ride40's rules allow, not only `ride36` faces (Mover.cs:2338-2378). So the inner walker's own accidental
  rides survive only in the verb-aware run.
- Evidence: seeds 2, 4 and 9 held the trigger 0 steps, so the pilot started no episode, yet the verb-aware run rode 6 / 10 / 5 times against
  5 / 2 / 5. Its clear times were 35.6 / 49.4 / 39.6 s against 31.7 / 32.8 / 41.6 s (verbs-skip.txt:74-76, 84-86, 109-111).
- g8's first rides start at x 10.7, the east wall, which has no ride36 face (verbs-skip.txt:79, 84).

**How line65 is read today.**
- `Tour65.Line(row)` (Assets/Scripts/Game/Tour65.cs:55-64) returns `[[x,y,z],...]` or `{points|waypoints}`.
- The verb probes use it only for the start pose (Harness.N65.Verb.cs:106-119) and pierce's first aim (313-322). The verb pilot never reads it.
- Rooms carry `verb65` and `line65` in the row (gen-slots.cjs:125 asks the builders for both; verb65 is read at VerbPilot.cs:113).

**Fix (a1): the line face.**
- In the constructor, keep each room's row and call `Tour65.Line(row)`.
- For each segment (a, b) and each face of the room, the segment is "on the face" when both ends are 0-1.6 m out on the face's side, the
  segment spans at least 3 m along the face, and it lies within the face's lo..hi. The longest such segment gives the face, the direction
  (sign of sb - sa), the entry s = sa and the exit s = sb clamped to lo + 1.5 .. hi - 1.5.
- Trigger only on that face, with the feet within 2.5 m (flat) of the entry, and only when armed. The pilot disarms at an episode's start and
  re-arms once the feet are more than 4 m from the entry. End the ride at the exit (replacing lines 238-241).
- When no segment matches, print once: `REFUSED: line65 matches no ride36 face of <room>` and count it in `stat.refused`. It then shows in
  V65Report's REFUSED column (VerbPilot.cs:78) and in skip v2's line (Harness.N65.Verb.cs:1141).

**Fix (a2): one pilot in all three runs.**
- In the verb-aware run, apply the filter's backstop (Harness.N65.Verb.cs:670-686) to any WallRide that is not the line episode's. Count it as
  `off-line rides cut N`.
- Then the three runs differ only by the line ride. Keep a switch `--keep-offline-rides` for the planted RED. This is a definition change:
  the critic should check it.

**Planted pair.**
1. A pure-case run with no sim. `n65verb ride-cases --level Logs/n67/r3-iceline/n67-iceline.json --room g8` walks a 1 m grid of g8's floor
   through the old and the new trigger and prints the cells that trigger and the cells that ride against line65.
   - RED (old): the 2.3 x 21 m strip triggers, and about half of it rides against the line.
   - GREEN (new): triggers only within 2.5 m of (-8.8, -228), with 0 against the line.
   - Planted refusal: `--plant-line65 g8:0,0,-226/0,0,-240` (a line on no face) must print REFUSED.
2. Sim pair: skip v2 on the r3 g8 file, seeds 1-10, old against new.
   - The instrument line: verb-aware wall rides 92 -> episodes plus 0 off-line, and seed 5's trigger steps 5054 -> under 600.
   - The M1 ratio is printed whatever it is; no outcome is predicted.
   - The same pair on kb3c: rides on the x -7.4 face must read 0.

**Time.** About 35 min of code, 10 min for the cases, 9 s for the harness build (Logs/n67/gate-g7-build.txt: 00:00:08.98), and skip v2 at
123 s for g8 and 155 s for kb3c (verbs-skip.txt totals). Then a critic.

## 2. (b) The launch punch kills its target

**Current behaviour.**
- The probe's target rule is `e.hp > punch.damage` (Harness.N65.Verb.cs:218, 233). punch.damage is 10 (PortExport/config.json `punch`).
- The game multiplies the punch:
  - momentum: x lerp(1, meleeMax, M), meleeMax 3 (Combat.cs:1483-1488; MomentumMath.cs:107-111; config `momentum.meleeMax` 3)
  - the arsenal tags, the bonus and vuln26 (Combat.cs:1507, 1511)
- A body dead after `Damage` is never launched (Combat.cs:1406, then 1410 `if (launch && enemy.alive)`). The shockwave skips the punched body
  (Impact.cs:116), so the kill is the punch's own `dealt`.
- kb3 r3: target husk hp 40, dash punch at 1.6 m in Dash, then `launch36 0 ... target dead (punch)` (Logs/n67/r3-kiln/verbs-launch.txt:7-8).
  So dealt was at least 40, which is more than 10 x 3 = 30 before the tags. The exact dealt is not printed: not checked.
- The verb-aware pilot has no hp rule at all. Any husk-shaped body within 5.5 m and 50 degrees qualifies (VerbPilot.cs:197).
- The pilot ends the episode on the punch press (VerbPilot.cs:368), so it never sees the outcome.

**Fix.**
- A bound `B = max(punch.damage x momentum.meleeMax, the largest punch dealt seen in this process)`, read from the config, never typed.
- Target only `e.hp > B`, in both places (VerbPilot.cs:197; Harness.N65.Verb.cs:233).
- Listen for `HitEvent` "hit" with `weapon == "punch"` (fields damage / killed: Economy.cs:17-18; emitted at Combat.cs:1517-1523).
  - Probe line: `punch: dealt X vs hp Y: KILLED | survived`.
  - V65Report (VerbPilot.cs:77) and skip v2's seed line gain `punch kills K, skipped S (hp <= bound B)`.
- If plain husks never clear the bound, the pilot says `no target the punch leaves alive (bound B)`. That is a design finding for GG: a full
  dash punch kills a fresh husk, so launch36 cannot throw one. It is not a pilot failure.

**Planted pair.** `n65verb launch --plant-pair` on the r3 kb3 file holds two husks in front of the pose, spawned the way V66Ride plants its
target (Harness.N65.Verb.cs:557-565). Husk A is at 3 m with hp set to 11. Husk B is at 6 m with hp set to 400. Both are probe values.
- RED (`--launch-v67`, the old rule): A qualifies (11 > 10), the punch kills it, launch36 is 0.
- GREEN: A is skipped (11 <= 30), B is launched, launch36 is at least 1. Both runs print the dealt line.

**Time.** 20 min of code, 10 min for the pair, then the kb3 / v2 / v6 launch probes and skip v2 (171 / 120 / 138 s, verbs-skip.txt totals).

## 3. (c) The walk bot freezes over lava in plan

**Current behaviour.**
- On the ground, F1Input stops all movement, jump and dash when the point 1.5 m ahead is `Burning` (Harness.N46.Flow.cs:569-578).
  After 240 such steps it plans a chase (579-593).
- `Burning(x, z)` means inside a top-level `lava` rect and not `Bridged` (450). `Bridged` means a collision box over (x, z) with its top at
  -0.2..1.0 (444-448). No height of the player enters the rule.
- So a deck or column top over lava higher than 1 m reads as lava from anywhere, the deck itself included.
- The lava exit rule already uses the feet: `p.y < -0.2` (511-512).
- v2's lava (-15..-5 x 61..69) has two boxes over it: a slab at -1.8..-0.8, and a block at x -15..-9, z 64..67, y -0.8..5. The block's 5 m
  top reads as lava (Logs/n67/r3-spire/n67-spire.json).
- v2's stall is 24.7 % against a 19.0 ceiling (NIGHT67-REPORT.md:83).

**Fix.** `Burning(x, z, feetY)` uses `Bridged(x, z, feetY)` with a top at `-0.2 .. Math.Max(1.0, feetY + 1.0)`. Line 572 passes `p.y`.
- With the feet at or under 0 this is exactly the old rule, so floor walks are the same step for step.
- Plan / Blocked (454-497) stay as floor plans.
- Count `deckHolds` (a hold the old rule makes with the feet above 1 m) and print it in the level summary beside `edgeHolds`
  (Harness.N46.Flow.cs:351).

**Planted pair.** `n46flow --lava-cases`, a pure rule over a planted level, needs `LavaAt`, `Bridged` and `Burning` made static over the two
lists. Planted level: lava [0,0,10,10], deck box (2,3,2)..(8,3.5,8), bridge top 0.8. Cases (feet -> point ahead: old, new):
(1) (5,3.5,5) on the deck -> (5,6.5): true, false, the RED -> GREEN; (2) (5,3.5,7.5) -> (5,9) off the deck's edge: true, true;
(3) (-1,0,5) floor -> (0.5,5): true, true; (4) floor -> under the deck: true, true; (5) the 0.8 bridge: false, false.
Only case 1 may flip (cases 2-5 are the guards). Room evidence: n46flow on the r3 spire file for v2, seeds 1-10 and 11-20, with deckHolds above 0 before and 0 after.
The stall number is printed, not predicted.

**Gate cost.**
- `--flow-base` must be re-made (about 5 min, n67-gates.sh:6, 17-21), with a diff that names every changed room. The --flow gate runs
  n52-foundry / n52-iceline / n57-kiln / n52-span / n52-spire (n67-gates.sh:10). Which of them have decks over lava: not checked.
- Every lava room's red must be re-run.

## 4. (d) The parry room: what each rule would cost (GG's call)

**Why geometry fails** (R's finding):
- A spitter spits only with `ai.attack && ai.los` (Spitter.cs:176). los is a ray from the spitter's AimPoint to the player's eye at
  0.8 x eyeHeight (HuskBrain.cs:816-822).
- The bot aims at the target's AimPoint (AutopilotTarget.cs:89, 123), and a returned glob homes on its shooter's AimPoint (Spitter.cs:454-459).
- So one line serves the spit, the gun and the return. With the KESTREL in hand, the refusing player shoots the spitter (g7's refused run:
  shots 47 vs 36, NIGHT67-REPORT r3 row; Logs/n67/r3-iceline/verbs-skip.txt totals line for g7).

| option | what changes | code / config | cost | risks |
|---|---|---|---|---|
| D1 MELEE ONLY room (exists) | g7's row `"limit": ["meleeOnly"]` | none. Parsed at Campaign24.cs:35, 299 and applied on entry (656). SurvivalSim.Campaign24.cs:214-233 leaves the gauntlet alone and restores the owned list at the door. Banner "MELEE ONLY" (Campaign24.cs:45). Needs design.n24.campaign.limitFlags on (absent = on: Campaign24.cs:505; its value is not checked) | 15 min data, plus (e)'s gauntlet parry for the instrument (the punch is dropped with the gauntlet in hand: Combat.cs:1344; the pilot's parry punches: VerbPilot.cs:333) | The spitters must sit beyond weapons.gauntlet.range (Combat.cs:479) or the player swings them. PRINCIPLES rule 1: the KESTREL leaves for one room. The tank in g7's first wave dies to the gauntlet |
| D2 WARD spitter (a new enemy rule) | a ward spitter takes damage only from the returned glob ("orb": Spitter.cs:470) and parry36's burst | Core flag on Enemy; an early return in Combat.Damage beside the stand-in rule (Combat.cs:1464) or in EnemyHits.Filter, like SHIELDED's block (Affixes.cs:123-140, affixBlock event); a row key parsed beside `limit` (Campaign24.cs:299) and set at spawn (as SpawnStandIn29: SurvivalSim.Systems29.cs:40-47); a Unity-side config block design.n68.ward68 (CLAUDE.md rule 5); the bot skips ward bodies for guns (AutopilotTarget.cs:123); a shell on the pill model (rule 7, GG's) | 2.5-3.5 h builder: Core 45, EditMode tests 45, row and spawn 30, view 45, bot 20, Player proof 30. Golden and trajectories unchanged with the row off | Enemy design is GG's (rule 7). The refused run cannot kill the ward spitters, so it hits the 120 s cap: M1 passes by construction. The critic should judge time-to-parry instead |
| D3 per-wave limit (parry-only wave) | the gun away for one wave | Campaign24 applies limits on room entry only (Campaign24.cs:656). A wave hook plus Campaign24State.limits (Campaign24.cs:528, 619) | 1.5-2 h (Core: golden vectors at commit) | The same as D1 |
| D4 global vulnerability | config `design.n26.weaponIdentity.vulnerability.kind.spitter` with guns at 0 | none (Weapon26.cs:63-103, applied at Combat.cs:1511) | 5 min | Every spitter in every level becomes gun-proof: a global enemy redesign. Listed for cost only |

## 5. (e) A gauntlet pilot for level 2's g1-g4

**Facts.**
- g1-g4 hold the gauntlet alone; the KESTREL is g6's award (the put-in line, Logs/n67/r3-iceline/verbs-skip.txt:67).
- Room verbs: g1 one swing kills a tank, g2 punch the WARDEN's shield, g3 the LT blast, g4 return the drone's orb and ride the lock walls
  (`verb36`, n67-iceline.json rows).
- With the gauntlet in hand the punch is dropped (Combat.cs:1342-1348), so the verb pilot's launch and parry never press (VerbPilot.cs:300, 333, 166), and V65Launch says RED "holds no gun" (Harness.N65.Verb.cs:222).
- The gauntlet's own swing launches a body when it lands during a speed verb (Combat.cs:1321, 1332). It reflects a glob (Foes40.cs:31-48),
  and LT is a 0.25 s parry window or the BLAST when the glove is full (LtAlways31.cs:6-9; PulseCharge27.cs:114-117; event pulse47Blast at
  Blast47.cs:112).
- The walker never presses altFire (AutopilotInput.cs:442-465). Its swing is the gun's `fire`.
- Drone orbs are DroneOrbs (Drone.cs:555 TryReflect). The pilot's parry trigger and the filter see spitter globs only (VerbPilot.cs:248, 330;
  Harness.N65.Verb.cs:694).
- Skip v2 counts `shots` from "fire" events (Harness.N65.Verb.cs:869). A swing emits "meleeSwing" (Combat.cs:1335; Foes40.cs:42).
- The ride probe credits a wall kill only after a "fire" in WallRide (Harness.N65.Verb.cs:571-583), and with no gun it cannot shoot
  (Harness.N65.Verb.cs:457). So the gauntlet's kills never count.

**What the pilot needs** (all in the harness):
1. Hand-aware episodes when `Weapon == "gauntlet"`: `launch` = dash, then `fire` (the swing) inside gRange x 0.9, replacing the punch;
   `parry` = `fire` or `altFire` at a glob or a drone orb within reach; `blast` (new verb65) = altFire when PulseCharge27.Ready and at
   least N bodies are in the blast zone; `stun` (g2) = the swing at the WARDEN's front.
2. Refusals in V65Filter: blast drops altFire while Ready; orb parry extends GlobInReach (Harness.N65.Verb.cs:694) to drone orbs. g1's
   swing has no refusal (the gauntlet is the only weapon), so g1 gets an M3-style probe (a tank dead to one swing), not skip v2.
3. Counters: swings, swing reflects (`swingReflects40`), LT returns (ltParry31Return), blasts (pulse47Blast). `shots` stays "fire"-only.
4. `n65verb ride --swing`: steer to a body within gRange on the wall and press fire. Count meleeSwing as the shot for the wall kill.
5. Verb names the slate can carry: `blast`, `swing`, `stun`. An unknown verb is REFUSED today (VerbPilot.cs:189-190).

**Planted pairs.**
- g4 orb parry with the gauntlet. RED: today's punch, parried no (the punch is dropped). GREEN: swing or LT, reflected at least 1.
- g3 blast. Verb-aware pulse47Blast at least 1 against 0 when refused.
- ride --swing on a planted husk (V66Ride's `--plant-target auto`). Wall kills: 0 old, at least 1 new.

**Time.** 120-150 min. g1-g4 reds then run first: about 4-8 min a level round (section 7).

## 6. The n67 tools: generic or copied

**Level-generic, use as they are (no n68 copy):**
- gen-slots.cjs: any level in levels67.json via `--level <L>` or `L67` (gen-slots.cjs:19-20, 47, 61)
- levels67.json: the table (foundry / iceline / kiln / spire)
- slots67.json files and the design.n67 rows (iceline67 / kiln67 / spire67) carry over while NIGHT68 works the same rooms
- levels67-kits.py is not an n67 tool. It is NIGHT25's kit file for levels 6 and 7 (levels67-kits.py:1).

**Night-bound, need an n68 copy** (they hard-code Logs/n67, NIGHT67, --off67 or the n67- tool names):
- the round set: n67-round.sh (16 Logs/n67 references), n67-red.sh, n67-round-verbs.sh, n67-table.cjs, n67-red-bars.cjs; the GATE:
  n67-gate-pack.cjs (20 references) and n67-gate-pack.sh; the close and its guards: n67-gates.sh, n67-close-check.cjs, n67-clock.cjs,
  n67-prune.sh, n67-reels.sh, n67-freeze-rows.cjs; the slate tools: n67-slate*.cjs, n67-spot-pair.cjs
- n67-area.cjs, n67-deco.cjs and n67-verbs-of.cjs are room-generic; their Logs/n67 references are comments only (n67-area.cjs:252,
  n67-deco.cjs:10, 198, 244, n67-verbs-of.cjs:2). Copy them anyway, because the renamed round tools call them by the n68 name.
- Do not copy n67-a1.cjs, the shared commit that wrote design.n67 and slots67 (n67-a1.cjs:1-3). NIGHT68 needs an a1 only if it adds a row
  (for example D2's ward68).

**How forge67-make-n67.cjs works** (64 lines): it copies every `n66-*.cjs|sh` (forge67-make-n67.cjs:16); `common()` renames `\bn66-`,
`Logs/n66`, `'Logs', 'n66'`, NIGHT66, flow-off66 and --off66 (18-24); `SPECIAL` edits (clock `night`, the gates' --off loop keeping the old
flag, close-check's rows) must each match exactly once or the file is not written (26-42, 55-59). It refuses to overwrite an existing file
(48-49), sets the .sh files executable (62), and prints the lines still naming n66 (63).

**forge68-make-n68.cjs needs these changes** (the pitfalls are mine; none was run):
- The FILES filter becomes `/^n67-.*\.(cjs|sh)$/`, minus `n67-a1.cjs`.
- **Pitfall 1:** `\bn67-` also matches the level ids n67-iceline, n67-kiln and n67-spire: 12 hits in n67-a1.cjs, n67-close-check.cjs:235,
  and gen-slots.cjs. Use `\bn67-(?!iceline|kiln|spire)`.
- **Pitfall 2:** keep `L67`. gen-slots.cjs:61 reads `process.env.L67`, and the round tools pass it on (n67-round.sh:25, 30, 48; deco and
  table have 4-5 references).
- Keep `slots67`, `levels67`, `design.n67` / `n67.iceline67`, `Design/levels/n67/` and `openings67.json`. These are data, not the night.
- SPECIAL edits: n67-clock.cjs:13 `night: 'n67'` -> `'n68'` (closeTags `n67-freeze` is handled by the regex); n67-gates.sh:34 append
  `"--off68"` and keep `"--off67"`; n67-close-check.cjs has 37 lines with `'n67'`, n67cc-, u3-n67 and CLOSE-CHECK / PRE-CHECK n67: rename
  those, but keep its default rows `n67.iceline67 / kiln67 / spire67` (n67-close-check.cjs:19-20, 235, 244, 292).

## 7. Run times (main tree, sim only)

- **Round r3, the final rooms** (assemble / red / verbs / total): iceline 12 / 73 (9 jobs) / 264 / 372 s; kiln 21 / 104 / 347 / 485 s;
  spire 12 / 79 / 277 / 384 s. The three levels ran serially from 16:04:21 to 16:25:02, 20 min 41 s (Logs/n67/r3-<L>-run.txt:2, 5, 7, 12,
  13, 22).
- **Earlier rounds:** r1-iceline 239 s and r2-iceline 233 s, with verbs at 147 / 155 s (Logs/n67/r1-iceline-run.txt:12, r2-iceline-run.txt:12).
  Why r3's verbs took longer is not checked; the KESTREL loadout's extra shots are a guess.
- **Skip v2 per room** (10 seeds, 3 runs each): g7 119, g8 123, kb3 171, kb3c 155, v2 120, v6 138 s (Logs/n67/r3-*/verbs-skip.txt totals).
- **GATE pack g7** (`L67=iceline bash Tools/night/n67-gate-pack.sh g7 r2-iceline`): 12:57:12 to the last file at 12:59:02, about 110 s.
  - Its skip v2 on seeds 11-20 took 69 s of wall time (Logs/n67/gate-g7-run.txt:1-2; mtimes of Logs/n67/gate/g7/*;
    gate/g7/verbs65-skip.txt).
  - The harness build before it took 9 s (gate-g7-build.txt).
- **Gates:** the --flow base takes about 5 min (n67-gates.sh:6); one fingerprint about 1 s (Logs/n67/gates-flow-r2.txt:4-15).

## 8. Not checked

All fixes are proposals; none was run and no number they would give is known. Also not checked: the punch's exact dealt in kb3 seed 1; the
value of design.n24.campaign.limitFlags; which --flow levels have decks over lava; whether a gauntlet swing at speed kills before it launches
(the same issue as (b)); whether g7's spitters are beyond gauntlet reach.
