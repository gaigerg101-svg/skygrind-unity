# w63 memory harvest: DRAFT (builder G 01:07 = 2:31; builder D 02:25 = 3:49; builder G again at port/stage3 1c792500 = 3:58)

For the lead to finish at the close. Nothing here is written into `docs/memory/`. Formats: the wave-memory skill (SKILL.md, formats.md) and
the newest pages (ledger/wave-62.md, LESSONS L-120..L-133).

**Sources, as they stood at 1c792500 (3:58):**
- NIGHT63-LOG.md: 504 lines, committed through 3:53.
- Logs/n63/slate.json, 14 slices:
  - PASS: P2, P3, C2.
  - SHORT: M1, P1, D1, C1, S7, EXIT63.
  - BLOCKED: EYE63.
  - In progress: B1, B2, B3, T63.
- Logs/n63/verdict-{M1-r1,M1-r2,M1,P2,P3,D1,C2-r1,C2,S7,T63,B1,B3}.md.
- DECIDE63.md D-1..D-11 (main tree, untracked).
- NIGHT63-REPORT.md (the 3:58 draft).
- Logs/n63/review-n63.md and Logs/n63/spot/*.txt.
- Builder D's 3:47 update (branch n63-HARV2, 508c8b76, not picked), built on here.
- The B2 / T63 fix rounds picked for b10: T63b da413f3f..f0f0269e; B2d / B2e aad13f07..e5dc105b.

Every number carries its source. `[close: ...]` marks what is still open: the B1 / B2 / B3 / T63 outcomes, the freeze build's verbs37 and
smoke, the tag.

**Caps:** THREADS.md 59 lines (cap 60), FEATURES.md 123 (cap 150), LESSONS.md 137 (cap 150), a ledger page 60.

## (a) ledger/wave-63.md (draft, to be finished at the close)

```markdown
# Wave 63 (NIGHT63, THE BOSS NIGHT; 2026-10-22/23, t0 22:36:01, an 8-hour box, 7 minimum: FREEZE 05:36:01, CAP 06:36:01; tag: at the close)

**Ask (GG 2026-10-22 ~22:11, FORGE63, during NIGHT62's close):** "plan out next long detail night prompt 8 hours 7 minimum, follow detailed
methods we used and pull form online to get best cresuolts, be honets and critcal on where we need to improve, i would lead towards bosses,
effect colores everything doenst feel alligned, boss animations are too rigid to start, this is one of many many things the night prompt
hsould focus on, create it now, 30 mins to build" (NIGHT63.md lines 3-5). Led with the bosses (motion, tells, colours), one colour table,
and NIGHT62's debt (P1 exitFrame61, P2 bossRule61 on time).

**Prompt shape:** NIGHT63.md (189 lines, untracked) + the /goal (Research/2026-10-22-forge63/GOAL.txt); BUILD 0:00-7:00, the close 7:00-8:00;
slate 8 seeded (D1, P1, P2, B1, B2, B3, C1, M1), 14 by 3:58 (P3, C2, S7, T63, EYE63, EXIT63 added from leftovers and DECIDE calls); builders
D, V, G, at most five agents (L-128); 9 builds by 3:40 (b9 02:16). Forge experiment (FORGE63 D-6, "visible by design": a named SPOT pose,
>= 41,472 px on its same-run pair): B1 crown flinch 372,543 (b8) but its SPOTs named the floor and "the big cyan glow"; B2 crownFire tell
766,708 (b7) / 766,631 (b9), named the red column; B3 crown idle 184,181 (b7); C1 131,656 (b4) (Logs/n63/spot/*.txt). [close: per row]

**Shipped (ON):** bossRule61 (P2 PASS, 65bc7cce), text61 (P3 PASS, f2b0c261), nearFade63 (C2 PASS in GATE round 2 after the screen-door
fix C2b, 02c70321; Logs/n63/verdict-C2.md). [close: B1 / B2 / B3 / T63 after their fix rounds, with shas]

**Numbers (source):**
- P2 boss61 in from 0.47 s on all five (n63-b1); P3 two-block entries 0 / 5 ON vs 4 / 5 OFF (n63-b3, verdict-P3.md).
- C2: the husk at 0.5 m 590,004 px same-run (b7), far controls 0 px; THE ICEBREAKER at 1.7 m 936,071 (b6 icem) (verdict-C2.md).
- B1 (GATE FAIL ~02:25, verdict-B1.md): rms 0 -> > 0 on 23 of 23 s on all five; the SPOT named "the big cyan glow" (b7), not a pose; the
  wyrm's flinch 1,084 / wind-up 1,097 / idle 373 px; stagger pairs wick 40,503, loom37 40,733, wyrm 16,942 under the bar. Fix round n63-B1d.
- B2 (b9, Logs/player/n47-reel-n63-b9-b2*.log): the spit fixed (lead 225 -> 492 ms, hue 27 -> 10, verdict short -> ok); crownFire the same
  before and after ("hue 41 sat 0.14 ... obj THE CROWN/crown clapper", off-hue) while the column's pair changed 766,631 px. B2d (the white-hot
  clapper pulled to danger) and B2e (the earliest danger cue printed) ride b10.
- B3 (GATE FAIL ~02:26, verdict-B3.md): THE CROWN's tell ring amber with the row on (35,617 px; 77 px changed by the toggle), the wyrm's weak
  point never cyan, the Icebreaker pair 0 px, the whole-frame meter blind to parts. Fix round n63-B3c.
- T63 (GATE FAIL ~02:16, verdict-T63.md): LATE 4 -> 0, but 2 lessons dropped with no log line; Midair shown (2 of 3 named withholdings).
  T63b (the queue) rides b10.
- eye63 MET (64 of 64 events halved, view only 1.00 mm, n63-b8), BLOCKED for GG's blind A/B (D-10); exit63 own gain 0 / 5 (b8).
- C1 kiln 91.9, span 80.5 % PASS; foundry 13.8, iceline 13.5, spire 30.3 % (slate C1). S7b kb3 468,517 px (b8), named "columns".
- verbs37 x5 27 / 29 / 31 / 31 / 31, RED none, on b1-b6 and b8; NRE 0 in every reel b2-b9; gates GREEN at every pick. The review: must-fix 0,
  should-fix 8, all fixed or probed (review-n63.md; NIGHT63-REPORT.md section 3). [close: freeze verbs37, smoke]

**SHORT (reasons):** D1 (no-op 0-16 px, plant 100-111; GATE FAIL); P1 (own gain 1 / 5, reverted, D-6); M1 (two GATE FAILs); C1 (2 / 5;
iceline / spire lit surfaces, D-9); S7 ("columns" twice); exit63 (own gain 0 / 5); eye63 BLOCKED (no still frame to SPOT). [close: B1 /
B2 / B3 / T63]

**Estimate to actual:** b1 at 0:34 (planned 2:00), b5 2:17, b9 3:40; GATEs 2-5 min. [close: the ratio per slate entry, idle minutes]

**What went wrong:**
- The prune skipped capitalised side names (n63-b4 11 GB).
- B2's meter misjudged three times. The b4 list came from a buggy meter; then b7 and b9 named the clapper (the strongest riser), not the
  column (the cue).
- B1's motion hid under bigger glows: the SPOT named the floor / the glow.
- B3's whole-frame meter could not see parts.
- T63's doneWhen named a fixed list of withheld lines that the run's order decided, and lessons were dropped silently.
- S7's doneWhen kept G's parts list through a redesign. eye63's last-key pin broke on exit63's append.

**Agents:** D, V, G; gg-proxy start; GATEs: M1 x2, P2, P3, D1, C2 x2, S7, T63, B1, B3; the fresh review (01:29-01:44); SPOT rounds b2-b9.
[close: gg-proxy at 6:30, the later GATEs]

**Lessons added or confirmed:** candidates L-134..L-156 (harvest-draft.md (d)); confirmed L-001, L-065, L-068, L-091, L-123, L-129,
L-130, L-132. [close: the ids added]

**Closed threads:** T-146 (all four items built; 7214fdd9, 01c3e6d8, 1b76fb8e). T-148 moved: bossRule61, text61 ON. [close]

**Reaction:** no play recorded (GG's last play 2026-10-20, OWNER.md). [react]
```

(Inside the fence: 58 lines. The close's fills replace the `[close: ...]` marks in place, with two lines spare.)

## (b) FEATURES.md rows

### Changed rows (replace the status and meter cells of the existing w61 rows)
- **THE BOSS RULE LINE** (bossRule61), status: `**ON (w63, 65bc7cce: critic PASS, Logs/n63/verdict-P2.md; the timing fix 33f2cd61 /
  8dac3008: the line BossRule61DelayS 0.45 s after the name card's rise, boss bar or not; P2b eb80ebff / 4f602599: the bar's hint held while
  the line stands)**`; files add `N63BossRuleTimeTests, N63BossRuleHintTests`; meter add `w63: boss61 GREEN on all five, alpha 1.00 at 1.0 s,
  in from 0.47 s, 40.0 px, 4.45-4.75 s (Logs/feel/n63-b1/summary47.txt); SPOT named the line (Logs/n63/spot/P2.txt); open for GG: the
  wyrm's returning bar hint contradicts the line (DECIDE63 D-4, T-150)`.
- **ONE LINE OF TEXT AT A TIME** (text61), status: `**ON (w63, f2b0c261: critic PASS, Logs/n63/verdict-P3.md; a two-run pair with the
  hint pinned by -skygrindPinHint63)**`; meter add `w63 (n63-b3, n63-p3-blocks.cjs): two or more centre blocks 0 / 5 ON vs 4 / 5 OFF; ROOM
  CLEAR after the next name 0 / 5 vs 2 / 5; open: the 3 s fallback stacked text 4 times, kiln taught 10 lines (DECIDE63 D-7; text63 below)`.
- **THE EXIT FRAME** (exitFrame61), status: `OFF (w62 P1 SHORT; w63 P1 SHORT: own gain on n63-b3's same-run pinned pair 1.38 / 1.32 / 1.56
  / 1.18 / 1.37x = 1 / 5 at 1.5x after x2 glow and x3 light (9399b97c, reverted acbabae4), GUARD 4 / 5, the SPOT named the gun first;
  DECIDE63 D-6: a different design, built as exit63 below)`; meter add `w63 same-run own gain 1.21-1.37x (b2) -> 1.18-1.56x (b3)`.

### New rows (w63; owner verdict "not recorded" for each)

| feature | files | switch | since | status | owner verdict | meter |
|---|---|---|---|---|---|---|
| NO ENEMY FILLS THE SCREEN (C2 + C2b: a live enemy part within 0.9 m of the eye drawn through its own 4 x 4 screen-door, the rim solid; ICE: an Icebreaker part within 0.6 m stops drawing) | Game/NearFade63.N63V.cs, Shaders/Enemy.shader (_NearFade), EnemyViews.cs, Brawl40View.N63V.cs; Tools/night/n63-v-near.py, n63-v-c2-rows.cjs, n63-v-c2b-rows.cjs; N63NearFadeTests, N63NearHideTests, N63ScreenDoorTests | design.n63.nearFade63 (view; -skygrindOff63, -skygrindNoNearFade63) | w63 | **ON (w63, 02c70321: GATE round 2 PASS, Logs/n63/verdict-C2.md; round 1 FAIL kept as verdict-C2-r1.md: a shatter-like dissolve)** | not recorded | red: an enemy at the eye in 136 / 98 / 0 / 52 / 62 of 1800 verbs37 frames; b7 C2b near 3.0 / 1.5 / 1.0 / 0.7 / 0.5 / 0.3 m: 0 / 0 / 27,320 / 389,029 / 590,004 / 950,876 px, SPOT "a dotted mesh you can see the floor and walls through"; icem (b6) stagger 936,071 px; verbs37 x5 27/29/31/31/31 on b8 |
| BOSSES THAT MOVE (B1: springs, wind-up poses, flinches, stagger slumps and idle life over the rigid sim copy; view only, each offset capped inside the collider) | Game/BossAttackClock63.cs, BossMotion63.cs, BossMotion63.Layer.cs, Wick37View / Brawl40View / WyrmView / Loom37View / Crown37View .N63V.cs, FeelReel.N63V.cs (boss63), UnityTuning.N63V.cs; Tools/night/n63-v-rows.cjs; N63BossClockTests, N63BossMotionTests, N63ReviewTests | design.n63.bossMotion63 (view; -skygrindOff63, -skygrindNoBossMotion63) | w63 | OFF (GATE FAIL ~02:25, Logs/n63/verdict-B1.md; fix round n63-B1d) [close] | not recorded | red rms 0 in 23 / 23 s on all five (b1) -> rms > 0 on 23 / 23 on all five (b8); crown flinch 372,543 px (b8); the wyrm's flinch 1,084 / wind-up 1,097 / idle 373 px; stagger pairs wick 40,503, loom37 40,733, wyrm 16,942 under 41,472; SPOTs: b2 (two-run) named the wick's slump; b5 "the bright cyan floor ... a different shape"; b7 "the big cyan glow ... larger / smaller, higher / lower" (Logs/n63/spot/B1.txt) |
| BOSS TELLS IN THE DANGER HUE (B2: on the attacks the before measure lists, a red-orange column round the boss through the wind-up; the boss's own warm off-hue glows, and (B2d) a white-hot warm glow, pulled to danger) | Game/BossCue63.cs, BossTell63.cs, BossTell63Late.cs, FeelReel.N63G.cs, UnityTuning.N63G.cs; Tools/night/n63-b2-fixlist.cjs + Assets/Tests/EditMode/Game/Fixtures/n63-b2-before.txt; N63BossCueTests, N63BossTellTests | design.n63.bossTell63 (view; numbers.fix; -skygrindOff63, -skygrindNoBossTell63) | w63 | OFF (B2d 9d3e74f0 + B2e cbffe7b8 ride b10) [close] | not recorded | the list [wyrm:spit, crown37:crownFire] (b6 before, c5edbec9, D-11; the test computes it from a fixture, c79e805b; the colour table's bands, review S3); b9 (same-run): the spit lead 225 -> 492 ms, hue 27 -> 10, verdict ok; crownFire unchanged ("hue 41 sat 0.14 ... crown clapper", off-hue) while its tell pair changed 766,631 px; SPOT (b7) named "a large see-through red cylinder" (spot/B2.txt) |
| BOSS COLOURS BY ROLE (B3: weak points the player's cyan, attack effects red-orange, on every boss; DECIDE63 D-8) | Game/BossColours63.N63D.cs, BossColours63Math.N63D.cs, WyrmView.cs (B3b); Tools/night/n63-d-rows.cjs; N63BossColoursTests | design.n63.bossColours63 (view; -skygrindOff63, -skygrindNoBossColours63) | w63 | OFF (GATE FAIL ~02:26, Logs/n63/verdict-B3.md; fix round n63-B3c) [close] | not recorded | red off-palette 0.107 / 5.148 / 7.779 / 0.635 / 0.777 % (b2; D.md); per boss on -> off (same-run): wick 0.057 -> 0.033, loom37 0.475 -> 0.257, brawl40 0.357 = 0.357, wyrm 1.074 -> 1.063, crown37 1.152 -> 1.166 %; crown idle 184,181 px (b7), SPOT "the big ring ... bright solid cyan"; the GATE: the crown's tell ring amber with the row on (35,617 px, 77 px changed), the wyrm's weak point never cyan, the Icebreaker pair 0 px |
| ONE COLOUR TABLE (C1: the world's off-palette colours routed to the role table; the HUD untouched) | Game/Colours63.N63D.cs, Colours63Math.N63D.cs (ScanDue), UnityTuning.N63D.cs; Tools/night/n63-c1-audit.cjs, n63-c1-meter.py, n63-d-rows.cjs; N63ColoursTests | design.n63.colours63 (view; -skygrindOff63, -skygrindNoColours63) | w63 | OFF, SHORT (MAIN 2 / 5 after C1b d3f91d82, DECIDE63 D-9; review S2: scans on decor54's clock, 2de5520a) | not recorded | 392 hand-written colours not from the table; off-palette drop on verbs37 on vs off: kiln 91.9, span 80.5 % PASS; foundry 10.0 -> 13.8 % after C1b, iceline 13.5, spire 30.3 % FAIL (lit surfaces); SPOT named a muting (spot/C1.txt) |
| KILN'S PRESSES AS PRESSES (S7, redesigned in S7b: a dark bed, a black throat, bright guide columns, a ram head leaving a 2.15 m gap, a crown with a lamp, all unlit, no stripes) | Game/Press63.cs, Press63Math.cs, UnityTuning.N63GS7.cs; Tools/night/n63-g-rows.cjs; N63PressTests | design.n63.press63 (view; -skygrindOff63, -skygrindNoPress63) | w63 | OFF, SHORT (GATE FAIL ~01:30 on b5; S7b on b8 named "columns" again) | not recorded | kb3 entry 292,689 px (b5) -> 468,517 (S7b, b8); "4 dressed"; flow gate EQUAL with it present; verdict-S7.md, spot/S7.txt |
| TEXT KEYED BY TECHNIQUE (text63: "once a run" by the technique a line teaches; the title wait 3 -> 10 s; T63b: a queue, no waiting line ever dropped, each line's fate logged; DECIDE63 D-7) | Game/Text63.cs, Text61.cs (keyed Pass, PassQueued, DrainWaiting), UiText61.cs (Text63Filter), UnityTuning.N63GT.cs (Text63WaitS, Text63HoldS); Tools/night/n63-g-text63-rows.cjs; N63TextTests | design.n63.text63 (view; numbers.verbs; -skygrindOff63, -skygrindNoText63) | w63 | OFF (GATE FAIL ~02:16; T63b da413f3f..f0f0269e rides b10) [close] | not recorded | b6: LATE 4 -> 0, kiln 10 -> 8 lines, 2 of 3 named restatements withheld; 2 lessons dropped with no log line (kiln "Jump SPACE, aim down, fire LEFT CLICK.", spire "Hold LEFT CLICK up close to burn.") (verdict-T63.md); T63b's tests replay b6's flows: both lessons shown, Midair withheld behind Jump (N63TextTests 8 / 8 outside Unity) |
| THE EASED EYE (eye63: the view's eye height follows a critically damped spring; view only; DECIDE63 D-10, M1's proposal 2) | Game/Eye63Math.N63D.cs, CameraFeel.cs (the hook), UnityTuning.N63Eye.cs; Tools/night/n63-d-eye63-row.cjs, n63-m1-chop.cjs --pair; N63EyeTests | design.n63.eye63 (view; -skygrindOff63, -skygrindNoEye63) | w63 | OFF, BLOCKED (slate: measure MET; no still frame to SPOT; waits for GG's blind A/B) | not recorded | n63-b8, verbs37 x5 on vs -skygrindNoEye63: the eye's own residual halved at 64 of 64 events, worse 0, view only 1.00 mm (Logs/n63/eye63-pair-b8.txt); span rms 2.48 -> 2.15 cm; verbs37 27 / 29 / 31 / 31 / 31 both sides |
| THE DOORWAY IS THE WAY OUT (exit63: a cleared room's exit doorway turns cyan: a sheet, linings, a threshold strip, floor dashes; every gain <= 1, no Light; DECIDE63 D-6's "a different design") | Game/Exit63.N63V.cs, Exit63Math.N63V.cs, FeelReel.N63V.Exit.cs, UnityTuning.N63VExit.cs; Tools/night/n63-v-exit-rows.cjs; N63ExitTests | design.n63.exit63 (view; -skygrindOff63, -skygrindNoExit63) | w63 | OFF, SHORT (own gain 0 / 5 on b8) | not recorded | own gain 1.05 / 1.02 / 1.01 / 1.00 / 1.03x = 0 / 5, GUARD 5 / 5, SHOTS 5 / 5 (Logs/n63/exit63-owngain-b8.txt); the 10 m kb3d door shot 14,648 px |
| NIGHT63'S REEL TOOLS: same-run pairs (-skygrindSameRun63), the boss63 reel and its cue meter (B2e: the earliest danger cue printed), the pinned hint (-skygrindPinHint63), one off-switch rule (NightRows.HasSwitch), contact sheets, the chop meter | Core/NightRows.cs; Game/FeelReel.N63Same.cs, SameRun63Math.N63D.cs, SameRun63View.N63D.cs, Ui.N63Same.cs, FeelReel.N63V.cs, FeelReel.N63G.cs, BossCue63.cs, PinHint63.cs, UiPinHint63.cs, UnityTuning.N63GP3.cs; Tools/night/n63-sheet.py, n63-m1-chop.cjs, n63-p3-blocks.cjs, n63-spot-pair.cjs; N63SameRunTests, N63SameRunGameTests, N63PinHintTests, N63SwitchTests | reel options (automation only) | w63 | on (tool); D1 SHORT (verdict-D1.md) | no-op pairs 0-16 px, plant 100-111 (b3, b5); from b8 the pair's ON half is `<side>-on/X` (review S4, a573502d); n63-sheet.py --selftest 9 / 9; m1-chop --pair MET on eye63 (b8) |

## (c) THREADS.md changes (the table stays at 60 lines)

Header line: add `w63 2026-10-23 (drafted at NIGHT63's 2:31, 3:49 and 3:58, finished at its close)` to "updated ...".

**Close (to ledger/wave-63.md "Closed threads"):**
- **T-146**, the review's should-fix items: all four are built.
  - w61 B1, the automation exclusion (proven w62 P6).
  - w63 rim56 one read a frame (7214fdd9).
  - An unlocked Editor run never pauses on a focus loss (01c3e6d8). w63's review S5 probe on b7 shows alt-tab still pauses a Player
    (Logs/n63/focus-probe-b7.txt).
  - n60-v-lamp.py skips an uncleared shot (1b76fb8e); review S6 makes it name every unmeasured shot (54569d37).

**Merge (to keep the cap):**
- **T-74 into T-60**: `| T-60 (+ T-74) | Rows never judged on a frame: aim41 / select41 / models41 (w41); look45, rooms named by their own
  landmark, shows nothing from b1 (w45) | w41 | w46: frames exist (n46-r2 / r3 a546), not judged; look45 1012 px on vs off | judge them
  (DECIDE46: ON only if they read); look45: a room whose pylons are in view, or another job |`
- **T-142 into the new T-153** (the pink pips are a colour-role question).

**Update:**
- **T-148** (tonight's SHORT P1 and exit63, and text63): `| T-148 (+ T-140) | NIGHT61'S ROWS NOT YET ON, THE DOOR, AND TEXT63:
  exitFrame61 (S1) SHORT twice and its redesign exit63 SHORT; fixture61 (S4) waits on GG (DECIDE62); text63 (D-7) GATE FAIL, T63b on b10
  [close] | w61 | w63: bossRule61 ON (65bc7cce), text61 ON (f2b0c261); P1 own gain 1 / 5 at 1.5x (b3), reverted; exit63 own gain 0 / 5 on
  b8, GUARD 5 / 5 (Logs/n63/exit63-owngain-b8.txt); text63: 2 lessons dropped silently on b6 (verdict-T63.md) | the door: an own-gain pose
  that frames the door (the 10 m kb3d shot, 14,648 px) or a measure other than own gain, before a third design; fixture61: GG; text63:
  T63b's queue judged on b10 |`
- **T-123** add to the last number (S7 SHORT folds here, no new line): `w63 S7 press63 SHORT: kiln's presses read as "columns" twice (GATE
  ~01:30 on b5; S7b's SPOT on b8), 468,517 px changed (verdict-S7.md, spot/S7.txt)`; next approach add `a press's shape judged on a crop
  before the build (a stamping head mid-stroke, not a dressed column)`.
- **T-149** last number: add `w63: C: 42.6 GB at t0, 27.7 GB at 00:25 (Logs/feel/n63-b4 11 GB unpruned: the prune pattern skipped
  capitalised side names), 25.9 -> 36 GB after a hand prune, 34.3 GB at 01:07; 41.8 GB commit free at t0 (NIGHT63-LOG.md)`.
  [close: C: at the freeze]
- **T-150** last number: add `w63 DECIDE63 D-4: THE KILN WYRM's bar hint returning after the rule line says "PUNCH THE HEAD TO STAGGER IT",
  against the line's "Swing the gauntlet LEFT CLICK at its charging head."; whether a campaign run reaches the wyrm with the gauntlet:
  not checked`.
- **Optional numbers** (no new line):
  - T-93 (+ T-117): `w63 C2 nearFade63 ON (02c70321): a husk within 0.9 m drawn as a screen-door (590,004 px at 0.5 m, b7)`. C2 passed, so
    T-93's own items (the swarmer's scale, the globs) stay as they are.
  - T-84: `w63 boss63 (b1): the wyrm's sim wind-up leads 533-900 ms; its spit had no drawn cue before B2 (b6)`.
  - T-133: `w63 B3: THE CROWN off-palette 1.152 -> 1.166 % with bossColours63 (same-run); its tell ring stays amber (verdict-B3.md)`.
  - T-147: `w63: not run`.

**Add.** T-151, T-152 and T-153 are certain (D1, M1 and C1 are SHORT; B3 FAILED its GATE). T-154 only if B1 or B2 ends SHORT.
- `| T-151 | D1'S SAME-RUN PAIRS NOT EXACT (D1 SHORT): no-op pairs 0-16 px (b3 2 / 16 / 4 / 6 / 4 / 0; b5 5 / 8 / 8 / 7 / 1 / 0) and the
  plant 100-111 (want 0 / 100), the residual on the viewmodel; the SSAO rescan guess (02785cac) did not remove it; "the sim never sees
  the toggle" not shown; THE CROWN's "35: drawn 34" on same-run runs (b4, b5, b8; plain 49 / 49); b9's aggro cue start 13033 vs 13000 ms
  between two runs with the attack lines identical (G-B2e.md: a ring throat entering the camera's view; drift, not confirmed) | w63 |
  Logs/n63/verdict-D1.md; from b8 the ON half is `<side>-on/` (review S4) | the viewmodel's clock the hold misses; the sim lines on vs off
  from ONE build's runs judged by the fingerprint (L-129); two before-runs of the wyrm on b10 (the lead's call) |`
- `| T-152 | THE CAMERA'S CHOP (M1 SHORT; gg-proxy #2, 7 asks): (a) frame pacing: unsynced bench p99 10.10 / max 32.10 ms, main-thread
  engine spikes (VSYNC on: not measured); (b) the eye's designed events: eye63 built OFF and MET on n63-b8 (64 / 64 halved, view only 1.00
  mm), BLOCKED for a still-frame SPOT | w63 | Logs/n63/m1.md, m1-bench.txt, verdict-M1.md; DECIDE63 D-2, D-10 | GG's blind A/B of eye63
  (L-001); a VSYNC-on bench and a Development Player's profile of the spike |`
- `| T-153 (+ T-142) | ONE COLOUR LANGUAGE: colours63 (C1) SHORT 2 / 5 (foundry 13.8 % after C1b; iceline / spire lit surfaces, D-9);
  bossColours63 (B3) GATE FAIL (the crown's tell ring amber, the wyrm's weak point never cyan, a whole-frame meter blind to parts) [close:
  B3c]; T-142 the PULSE pips at hue 332 (no pink family) | w63 (T-142 w60) | slate C1; verdict-B3.md: 35,617 px amber on the crown's tell
  frame, 77 px changed by the toggle | a per-part role meter (B3c); GG: re-light iceline / spire toward the palette or keep them (D-9); weak
  points cyan (D-8); the pips |`
- `| T-154 | THE BOSSES' READ [close: whichever of B1 / B2 ends SHORT]: B1's GATE FAIL (the SPOT named the floor, "the big cyan glow";
  the wyrm's motion 373-1,097 px; three stagger pairs under 41,472); B2's crownFire read off-hue by the meter's strongest riser (the
  clapper) while the SPOT named the red column (b7, b9) | w63 | verdict-B1.md; Logs/player/n47-reel-n63-b9-b2p.log | [close] |`

**Count:** 59 lines, then:
- minus T-146, T-74 and T-142 (3);
- plus T-151, T-152, T-153 and T-154 (4).

That makes 60. If B1 and B2 both pass, T-154 is not added (59). S7 folds into T-123, exit63 and text63 into T-148, eye63 into T-152: no new
line for any of them.

## (d) LESSONS candidates (from L-134; status "candidate" for the lead; scope as formats.md)

LESSONS.md is 137 lines (cap 150): 13 fit. Twenty-three candidates follow.

**Suggested:**
- **Add 13** (to 150):
  - L-134, L-135, L-137, L-139, L-141, L-143, L-144, L-145, L-147, L-155.
  - L-136 with L-149 folded in.
  - L-138 with L-150 and L-154 folded in.
  - L-151 with L-156 folded in.
- **Fold into existing rows:** L-140 into L-068; L-148 into L-125.
- **Ledger only:** L-142, L-146 (until T63b passes), L-152, L-153 (one evidence each).

The lead's five asks:
- the same-run residual: L-154;
- the glow hiding the pose: L-155;
- the meter's strongest riser: L-149 / L-136;
- lowercase sides: L-141;
- the fixed list of withheld lines: L-156 / L-151.

| id | rule | evidence | scope | status | confirmed |
|---|---|---|---|---|---|
| L-134 | Measure a pixel bar (rule 8's 41,472 px) on a same-run pair only: a two-run pair's count includes the runs' drift | NIGHT63: THE WICK's stagger pair 42,273 px two-run (b2) vs 37,345 same-run (b4), under the bar; d1base vs d1noop sim lines differ from f24, before the first toggle (NIGHT63-LOG); confirms L-129 | lead, reels | candidate | w63 |
| L-135 | Run-time Unity objects have negative instance ids: a meter never uses an id's sign or zero as "none"; keep a found flag | NIGHT63 B2: on n63-b2 all 16 cues read "start none" while the sim's tells ran 1008-1625 ms; the meter's own bug, read as B2's red for ~9 min; fixed 05d30aa6 / 17607e3a | meters, tooling | candidate | w63 |
| L-136 | A cue meter judges an attack by the sim's own kind (parry / dodge), never by the colour it audits; its reference window spans the slowest ambient cycle in view (+ L-149) | NIGHT63 B2b (b4): THE CROWN's beamCut / lowFire called "parry" from a yellow riser, the wyrm spit's riser its 2.1 s seam wave; fixed c8290b27 / ec2c448d; the list from that meter fixed two fine attacks and missed crownFire (b6, D-11) | meters | candidate | w63 |
| L-137 | A probe or overlay runs after the last draw of the frame it reads: the boss views draw in SurvivalGame.Update, THE CROWN in its own LateUpdate, a reel coroutine between them | NIGHT63 B1: THE CROWN read rms 0 in 23 of 23 s on b2 (the probe before its LateUpdate draw); BossMotion63Probe.Take (384d609b) -> 23 / 23 on b4 | reels, view | candidate | w63 |
| L-138 | Compare held frames only with held frames, and let a reel's per-frame meters skip held frames (+ L-150, L-154) | NIGHT63 D1 GATE: a held ON frame differs from the live shot frame by 45,779-253,860 px on 6 of 12 shots; review S4: Shot29's stats described the live frame while the PNG was the held one (fixed a573502d: `<side>-on/`) | reels, meters | candidate | w63 |
| L-139 | Choose a SPOT pose from a measured pair (its px and both halves' luma), never from arithmetic or a guess | NIGHT63: V's ICEBREAKER stagger pose by arithmetic gave a blacked-out pair (b2); B3's named wyrm-stagger pose changed 6 px on b5; the crown idle (184,073 px, measured) was named by its SPOT on b7 | builders, lead | candidate | w63 (twice) |
| L-140 | After a pick, list the test classes the slice added and run each by its own name: a filter that is the prefix of one class misses its sibling | NIGHT63: "N63SameRunTests" never matched N63SameRunGameTests (7 tests), unrun until 00:27 | lead, tests | candidate (fold into L-068) | w63 |
| L-141 | Keep reel side names lowercase and test a prune pattern on the night's real side names | NIGHT63: n63-prune.sh's `[a-z0-9-]+` skipped b63fS / b63kA / ... (b63kA/boss63 1,535 files); Logs/feel/n63-b4 11 GB -> 970 MB by hand; C: 25.9 -> 36 GB; lowercase from b5's sets on (NIGHT63-LOG, "Disk") | lead, tooling, disk | candidate | w63 |
| L-142 | One notes file per slice: two slices appending to one notes file conflict at the pick | NIGHT63 b5's pick: 55610d4a conflicted in D.md; later slices wrote V-ICE / V-C2b / V-EXIT / D-EYE / D-RV / G-RV / G-B2d / G-B2e apart and picked clean | builders | candidate (ledger only) | w63 |
| L-143 | When a fix round must change one of tonight's own pinned assertions, change it in the RED commit with the reason, and the lead logs a DECIDE the GATE reads | NIGHT63: V's B1b (D-6), D's C1b (D-9), V's C2b and G's B2c (D-11), G's S7b and RVG (stated in their RED commits) | builders, lead | candidate | w63 (six times) |
| L-144 | Name a cause from telemetry only where a field shows it: count runs of frames as events, and rule out the reel's own placements before calling a move the game's | NIGHT63 M1: GATE round 1 FAIL (36 of foundry's 43 "snaps" were slide eye-ease runs); round 2 FAIL (the "step-ups" were the reel placing the player 0.2 m up) | lead, meters | candidate | w63 |
| L-145 | Raising a glow, or adding unlit cyan, does not buy own gain on a far clear-shot pose against a lit background: check the meter's reach on the named poses before building | NIGHT63 P1: x2 glow and x3 light, 1 / 5 at 1.5x, reverted (D-6); exit63, a new design (gains <= 1, no Light), 1.00-1.05x = 0 / 5 on b8, as V predicted 0-1 from the poses | look, lead | candidate | w63 (twice) |
| L-146 | Key a "once a run" teaching cut by the technique a line teaches, not by its words or its verb (Principle 1) | NIGHT63 text63: kiln 10 -> 8 lines on b6; its GATE failed on dropped lines (verdict-T63.md); T63b's queue on b10 [close] | design, ui | candidate (ledger only until T63b passes) | w63 |
| L-147 | When a doneWhen says "only the failing ones", compute the list from the before measure by a script, commit its input as a fixture, and test the rest untouched | NIGHT63 B2: b4's column on every non-parry attack broke "attacks already fine are untouched"; a test pinning b4's list failed once the meter was corrected (D-11) -> B2c computes it from a committed fixture (c79e805b) | builders | candidate | w63 |
| L-148 | In a worktree, run git as plain single commands from the worktree; read main with `git show port/stage3:<path>` | NIGHT63 builder G, during both harvest drafts: the guard refused `cd <main> && git show`, a `for` loop over `git show`, a computed `sed` range (each re-run as a plain command at once) | builders, tooling | candidate (fold into L-125) | w63 |
| L-149 | A meter that names "the cue" by the strongest riser can name the wrong object: print the cue the slate asks about on its own (the earliest danger-hue riser: start, lead, hue, object) and read the object against the frame before trusting a verdict | NIGHT63 B2, three times: b4's verdicts from a yellow riser and the wyrm's seam wave; b7 and b9 read crownFire "off-hue" from THE CROWN's white-hot clapper (Crown47WhiteHex x 2.4, hue 41, sat 0.14), identical before and after the fix, while the column's pair changed 766,631 px and the SPOT named the red column (n47-reel-n63-b9-b2p.log; spot/B2.txt); B2e (cbffe7b8) prints the earliest danger cue | meters | candidate (fold into L-136) | w63 (three times) |
| L-150 | Before blaming a reel artefact, run the plain reel once: "the toggle did it" is a hypothesis until a run without the toggle reads clean | NIGHT63 B1: THE CROWN's "35: drawn 34" on same-run runs (b4, b5, b8) though c8f9ec75 skips held frames; b1pp, a plain crown run, GREEN 49 / 49 (b6) | reels, lead | candidate (fold into L-138) | w63 (three builds) |
| L-151 | A doneWhen states the visible outcome and the rule; a list copied from a builder's notes (parts, withheld lines) is re-derived when a fix round redesigns the parts or when the run's order decides the list (+ L-156) | NIGHT63 S7: its doneWhen kept G-S7.md's parts (rails, a chrome strip, striped plinths) after S7b replaced them; T63: its doneWhen named three withholdings from b3's order (L-156) | lead, builders | candidate | w63 (twice) |
| L-152 | A test pins a config row's presence and shape, never its position: "the last key" holds only at the row script's run | NIGHT63: N63EyeTests' last-key assertion failed once V's exit63 was appended after eye63 by its own script (L-065); fixed 479a9c5b | tests, builders | candidate (ledger only) | w63 |
| L-153 | Predict a view change's measure by replaying its algorithm on the last freeze's telemetry before the Player run | NIGHT63 eye63: the replay on n62-freeze read 64 of 64 events halved, the b8 Player pair 64 of 64 | builders, meters | candidate (ledger only) | w63 |
| L-154 | A same-run toggle is only as clean as its held frame: measure the planted no-op pair on every build whose SPOTs use it, and state that residual in each SPOT file | NIGHT63 D1: no-op pairs 19 / 12 / 7 / 45,686 / 195,235 / 253,849 px on b2 (the viewmodel, crosshair, a hit flash and full-screen noise moving), 2-16 px on b3, 0-8 on b5 after the fix rounds, never 0 (the residual on the viewmodel; verdict-D1.md, DECIDE63 D-5 / D-6); every SPOT file from b3 on states it | reels, lead | candidate (fold into L-138) | w63 (four builds) |
| L-155 | A motion row's SPOT pair must change the boss's silhouette more than its glow: look at where the pair's changed pixels are before the spotter does; a pose change hidden under a bigger glow change is named as the glow | NIGHT63 B1: crown flinch 372,543 px (b8) and same-run stagger pairs over the bar, but the blind SPOTs named "the bright cyan floor ... a different shape" (b5) and "the big cyan glow ... larger / smaller, higher / lower" (b7), never a lean or slump; the GATE FAILED on it (Logs/n63/spot/B1.txt, verdict-B1.md) | builders, lead, reels | candidate | w63 (twice) |
| L-156 | A doneWhen written from a builder's pre-measurement names a rule, not a fixed list of outcomes, when the outcome depends on the run's order | NIGHT63 T63: the doneWhen named "Midair ...", "Fire ... down to lift off." and "Boost: ... twice." withheld (b3's order); on b6's verbs37 kiln the jump step waited under the title and was dropped, so Midair showed and only 2 of 3 named withholdings happened: doneWhen 1 FAIL as written (verdict-T63.md); with T63b the withheld wording follows from which the run reaches first (G-T63.md) | lead, builders | candidate (fold into L-151) | w63 |

**Confirmed in w63 (mark "w63" in their confirmed cell):**
- **L-001:** eye63, MET by its measure, waits OFF for GG's blind A/B (D-10; slate EYE63 BLOCKED).
- **L-065:**
  - b5's pick conflicted in both config copies at S7; resolved with HEAD's copies and `n63-g-rows.cjs`.
  - b8's pick conflicted at exit63; resolved with HEAD's copies and `n63-v-exit-rows.cjs` (NIGHT63-LOG).
- **L-068:** see L-140.
- **L-091:** B2's meter needed a planted case per boss; review S6 made the lamp meter name every unmeasured shot (54569d37, PLANTED GREEN).
- **L-123:** four log times first written from memory, corrected 23:09.
- **L-129:** see L-134. Also b9's aggro cue moved one frame between two runs with identical attack lines (G-B2e.md, not confirmed).
- **L-130:** SPOT first on every row (B1-B3, C1, C2, ICE, S7, T63).
- **L-132:** GATEs 2-5 min.
- **L-006** (noted, no measured cost): an Editor compile beside b2's set-2 reels.

**For CORE.md at the close:**
- The strongest candidates, each seen twice or more in w63:
  - L-134 with L-129;
  - L-139;
  - L-143 (six times);
  - L-145 (P1 and exit63);
  - L-136 / L-149 (B2's meter, three times);
  - L-155 (B1's two SPOTs);
  - L-151 / L-156 (S7, T63).
- [close]
