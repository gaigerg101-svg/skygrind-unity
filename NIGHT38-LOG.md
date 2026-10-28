# NIGHT38 log (the lead): the flight night

Prompt: NIGHT38.md (untracked). Brief for the builders: NIGHT38-BRIEF.md (untracked). Design: Research/2026-10-13-wave38/PLAN.md.
Box 5:00, hard cap 5:20. t0 = GG's go (decided in DECIDE38.md, 2026-10-13 ~00:30 local; the night started at 09:15 local).

## U3-51.1 Start (09:15 - ~09:45)
- Editor gate: `unity-loop.sh status` ready, playMode stopped. Free memory 20.6 GB of 63.1 GB at 09:15. Power plan Balanced.
- git: `port/stage3`, base `5e34f483` (wave 38 plan). GG's four files modified and untouched: ProjectSettings/UnityConnectSettings.asset,
  ProjectSettings/EditorBuildSettings.asset, Assets/ProjectSettings/UIEffectProjectSettings.asset, Assets/Scenes/Arena.unity.
- docs/VERBS.md rows 9 and 11 marked **changed by ride38 (NIGHT38)**, each with its ride38 check.
- Shared commit `4bd19270` (the lead): N38Blocks (eight bits: ride38, foundry38, iceline38, kiln38, span38, spire38, finale38, stag38),
  design.n38 (every row off; view rows doors38, hud38, juice38), `--off38` / `-skygrindOff38`, tape format 21 (a VarI after the NIGHT37
  value), Curriculum25.N38Levels (after N37Levels), the sim hook file SurvivalSim.N38.cs, the harness partials for the eight builders,
  the test N38SharedTests (3/3), N37TestBaseline forcing NIGHT38 off.
  Checks before the commit: CoreHarness golden 39 sets / 22 replays, 0 mismatches; SimHarness fingerprint EQUAL with and without --off38;
  unity compile clean (compilationFailed false, consoleErrors 0); EditMode N38SharedTests 3/3, N37SharedTests 3/3, N36SharedTests 3/3,
  Lesson25Tests 10/10, TapeTests 14/14.
- Briefing: NIGHT38-BRIEF.md (untracked): the rules, the shared surface, the eight cards.
- Launched the eight builders in worktrees (isolation worktree, from 4bd19270), each on its card: E (envelopes + gates), M (ride38, then
  doors38), H (hud38, then juice38), F (n38-foundry + n38-finale), S2 (n38-iceline + stag38), K3 (n38-kiln), O2 (n38-span + loom38),
  C2 (n38-spire + crown38).

## Round 1 (launched 09:45; integrate at about 10:30 (1:15) and 11:30 (2:15))
- E's envelope table and gate checker: due by 1:00 (about 10:15). Level builders merge port/stage3 to run the checker.
- Builder reports arrive as notifications; each one is checked against its commit before the lead integrates.

## Integrations (port/stage3, fast-forward, each checked on the merged tree)
- E1 `1532c588`: envelope table + gate checker (harness only). E's checker has two gaps: no dash-then-jump try (C2's spire 0/4)
  and no chain release (S2's G3 RED). E asked for both.
- M ride38 + doors38 `3467e012`..`5951d3dd`: golden 0 mismatches, 22/22; fingerprint EQUAL plain and --off38; Editor compile clean;
  Ride38Tests 5/5. ride38 OFF in config. Open: the walk bot on n36-foundry with ride38 on: room a5 deaths 2 -> 9 (not explained yet).
- H hud38 + juice38 `aea9b725`..`eda9a526`: fingerprint EQUAL plain and --off38; Editor compile clean; Hud38Tests 6/6, Juice38Tests 5/5.
  Both rows OFF. Frames owed (H's list in Research/2026-10-13-wave38/H.md).
- O2 n38-span + loom38 `2e9ea05b`..`7965acb3`: fingerprint EQUAL; span38 ON in its branch (gates 3/3, walk 10/10 in the harness). Its
  walk dispatch line was already in port/stage3 (Walk29Pilot SampleMove, before the Span30Rooms line). The regen script's rule-9
  fix is in `3129cbcc` (a .cjs, byte-identical output). Not yet booted in a Player.
- S2 stag38 + n38-iceline `05ade31d`..`66a6ac1f`: fingerprint EQUAL; walk 10/10 (harness). stag38 turned OFF in config
  (Tools/night/n38-stag38-off.cjs); iceline38 OFF until E's checker passes G3. Walk dispatch line (Ice38, after the Room36 line)
  applied by the lead in Walk29Pilot.cs.
- C2 spire38 meter + n38-spire: merged `1b9925ea` (conflict-free); the meter on the N38 hooks (`a3bb1266`); walk dispatch `2aa90619`.
  Row OFF: waits on E's dash-then-jump try (spire38 gates 0/4 on E's checker, 4/4 on C2's harness).
- F level 1 n38-foundry + level 6 n38-finale: merged `0901ef44`. Both rows OFF; fingerprint EQUAL plain and --off38.
  - n38-foundry: built and walked (5/5 harness); gates wait on E's classifier fix (rides judged by planned verbs; F's fix
    `(g.atom == "ride" && tr.rides > 0)` sent to E).
  - n38-finale: SHORT on gates (0/4 on E's checker). The exam level owns every verb, so D-6 cannot hold without a rule that takes
    the boost and the grapple away at a gate. **Open for GG (not a lead call): keep D-6 strict and rebuild the exam gates, or
    relax D-6 for the exam level.**

- K3 level 3 n38-kiln: merged `827b8d66` (one conflict in Walk29Pilot's hook lines, kept both sides; all dispatch lines confirmed).
  Row **held OFF** by the lead (`Tools/night/n38-row-off.cjs kiln38`): K3's branch had it on. Its checks: n38 gates 4/4 on E's checker,
  walk 10/10 on the harness, fingerprint EQUAL. Two gaps keep the proof incomplete: (1) the PULSE gauntlet's LT blast is not in owns38
  (a charged gauntlet can skip one gate per charge); (2) the checker drops wall-jump tries on noride walls, though the Mover allows
  them (K3 measured 3.50 m at most under the ledges). Sent to E. Also flagged, not checked: n35-kiln's `award: "thunderclap"` may hand
  over a gun that never fires if the pickup is missed (n38 uses the id `shotgun`).
  Compile clean (11:59).

## Gate checker round 2 (E): merged `198ae14d`; the stricter checker
- E's slices 2-3: a dash jump in the Mover's order (dash, then jump), the double after it, ride chains (let go at each wall's end),
  the verb the Mover actually used (a ride that started counts as a ride), shifted lines (+/-2, +/-4 m), the PULSE gauntlet's blast,
  noride walls judged by the real flag. Fingerprint EQUAL plain and --off38 after the merge.
- Results on port/stage3 (198ae14d): span38 3/3 PASS (grapple only). spire RED 0/4 (blast crosses all four: v3 +39 %, v4/v5/v7 +37 %).
  iceline RED 0/2 (G2 dash jump + double +66 %; G3 blast +14 %; G1 skate not checked, no ice in the box world). kiln RED 0/4 (blast on
  all four; dash jump on G3/G4). foundry RED 0/2 (dash jump + double crosses 30.5 m on the +2 m line).
- Not covered by the checker: C2's meter; spire's boost; the finale's two-pull grapple (x6).
- Round 3 (sent 12:10, report by 13:00): C2 spire, K3 kiln, S2 iceline, F foundry: GEOMETRY fixes only, each gate must stop every owned
  verb with the 10 % margin kept. No rule or verb change. Rows stay OFF in their commits. Level 6 is left for GG's decision.
- span38 stays ON: its gates are GREEN on the stricter checker.

## Round 3 results (merged: C2 `2a605bdb`, K3 `3fe1e36c`, S2 `d2ff10e9`; F pending)
- **C2 spire, SHORT:** the PULSE blast 0.3 s before the double reaches 30.36 m; no ceiling stops it; the gap window is empty
  (gap must be over 30.36 m and at most 30.36 m). spire38 stays off. Its gates stay at 17 / 18 m.
- **K3 kiln, SHORT:** the blast and the dash jump beat every void the boost crosses; the checker never flew the boost sequence that
  climbs (jump, shoot down, double at the apex, shoot down: 12.68 m). Its plan proposal: noride ledges of about 9.5-10 m, which needs
  E's boost sequence. kiln38 stays off.
- **S2 iceline, G2 and G3 PASS** on the stricter checker (ride walls 64 m voids; G2 +20 %, G3 +15 %; with --ride38 both +15 %). G1 is
  a skate gate the box world cannot judge: it is INFO now, not a D-6 gate. **Lead's call: iceline38 stays off until G1 is either
  judged on ice or accepted as a flow beat. This is a GG-level question if the checker cannot judge ice.**
- **F foundry, PASS** (merged `9899eacc`): both GANTRY gates GREEN 2/2 on E's checker (+32 % and +26 %; the +32 % try rode the wall, which
  the checker counts as a ride). The gate changed geometry only (a 3.6 m ceiling over each gap; the gap grown to 37 m). Walk 3/3 GREEN,
  no deaths. The gauntlet blast is excluded from this plan by design (level 1 owns no gauntlet). **foundry38 turned ON** by the lead
  (`node Tools/night/n38f-config.cjs --on foundry38`): level 1 is the campaign's first level, so the Player check is owed before the close.
  Level 6 stays off.
- **E round 4 (report by 13:15):** (1) the boost sequence as the intended boost's tries (more accurate, no rule change); (2) measure in
  the real sim whether a room's `pistolOnly` limit removes the PULSE blast; the checker honours room limits only if it does.

## Round 4 (13:00): spire38 ON
- E (`703c5260`, b77dcb30): the checker honours each room's limits (measured in the sim: a `pistolOnly` room removes the PULSE blast
  and the THUNDERCLAP, `n38 limits`). spire's gates 4/4 PASS (v3 +39 %, v4/v5/v7 +37 %). kiln still RED 0/4 (blast beats G1/G2;
  dash jump and blast beat G3/G4). E's boost sequence (jump, shoot down, double, shoot down) reaches 13.36 m at -89 deg.
- C2 (`f684ef22`): the walk bot clears v1-v10 on 5 seeds, 0 deaths (NIGHT37's v9 falling stones and v10 ring scripted, the only
  changed file Walk38Spire.cs). Level median 112.9 s.
- **spire38 turned ON** by the lead (`node Tools/night/n38-row-on.cjs spire38`): gates GREEN and the walk clears every room.
- Compile at 13:00: clean (`compilationFailed` false) but the console shows NullReferenceExceptions from `SurvivalGame.Update`
  (SurvivalGame.cs:271, `cfg` null) and the views' `LateUpdate` (Look.C, cfg null), timestamped 13:01, play mode stopped. Not
  explained yet. Not in the sim (fingerprint EQUAL). The Player boot is the real test; open.

## Freeze (13:05-13:25): build `n38-r1`
- Player build `Builds/n38-r1/SKYGRIND.exe` from 01c5c032 (foundry38, span38, spire38 on): Succeeded, 0 errors, 517 warnings.
  `git diff` of ProjectSettings and the font / URP assets: no new rewrites (GG's three files were already modified).
- Smoke (`-skygrindSmoke`, 400 s): DONE, all steps passed; 0 NullReferenceException in the Player log. The Editor's NullReference
  entries (13:01) did not reproduce in the Player.
- **Level boots NOT PROVEN:** `-skygrindMap n38-foundry | n38-span | n38-spire` (90 s each) booted the attract on SURVIVAL, not the
  campaign level (no fallback warning; the flag's path into a campaign level is not the one I used). 0 NullReference, 0 errors, but
  that proves the Player runs, not the three levels. Open: find the campaign boot (the level select / a saved campaign pick) and boot
  each level.
- Batch EditMode suite at 01c5c032 (`n27-batch-tests.sh`, tag n38-freeze): running at 13:25; result in NIGHT38-LOG when it completes.
- Verbs (VERBS.md) on the levels: not run (no time; the verbs reel needs the level boots above).
- Frames owed: not taken (no Editor frame capture on the Player this round).

## Close table (13:50; box 5:00, cap 5:20)
| card | status | measured reason |
|---|---|---|
| E1 envelopes + gates | DONE (harness) | envelopes38.json; checker with dash-then-jump, chain release, blast, room limits (`n38 limits`); selftests GREEN. Gaps open: the finale's two-pull grapple at x6; a skate-gap judge on ice (none in the box world). |
| M1 ride38 + doors38 | DONE (row OFF) | ride38: golden 0 mismatches, Ride38Tests 5/5, n38ride hold PASS. The walk on n36-foundry with ride38 on: room a5 deaths 2 -> 9, unexplained. doors38 frames not taken. |
| H1 hud38 + juice38 | DONE (rows OFF) | Hud38Tests 6/6, Juice38Tests 5/5, n38hud GREEN. Frames not taken. |
| F1 level 1 + level 6 | level 1 DONE (ON); level 6 SHORT | foundry38 ON: gates GREEN 2/2, walk 3/3; the Player level boot NOT proven. Level 6 (n38-finale): gates 0/4 on the checker (the exam level owns every verb); needs GG's rule decision. |
| S2 level 2 + stag38 | SHORT (iceline38 OFF) | G2 and G3 PASS on the checker; G1 (the skate gap) is INFO only (no ice in the box world): GG or the lead must accept it as a flow beat. stag38 OFF. Walk 10/10. |
| K3 level 3 | SHORT (kiln38 OFF) | gates RED 0/4: the blast beats G1/G2; dash jump and blast beat G3/G4. No geometry found; a noride 9.5-10 m ledge plan needs the boost sequence in the checker (E added it: 13.36 m). Not re-run on the plan. |
| O2 level 4 + loom38 | DONE (span38 ON) | gates 3/3 GREEN (grapple); walk 10/10. Player boot NOT proven. loom38 frames not taken. |
| C2 level 5 + crown38 | DONE (spire38 ON) | gates 4/4 GREEN with room limits (`pistolOnly` removes the blast, measured in the sim); walk 10/10 on v1-v10 (5 seeds). crown38 frames not taken. Player boot NOT proven. |
| L0 lead: the shared commit, the integrations, the freeze | DONE with the open items | fingerprint EQUAL plain and --off38 at each merge; Editor compile clean at each merge (one run with NullReference console entries, not reproduced in the Player); Player build n38-r1 Succeeded; smoke DONE (0 NullReference). |
| Verbs on the six levels | NOT DONE | the verbs reel needs the campaign boot, which is not proven. |
| Batch suite (freeze) | 1982 / 1985 | three failures: Lesson25Tests.TheOrder_FollowsTheBlocks (FIXED: NIGHT38 forced off in the order pins; Lesson25Tests 10/10 after the fix, Play mode stopped first); AutopilotTests.PerStep_AllocatesNothing (1 allocation over 17,748 fight steps, not diagnosed); MapChoiceTests.EveryListedMap_LoadsAndPlays600Steps (180 s timeout; flaky since NIGHT35, not diagnosed). |

## Frames owed (the lead, at the freeze; not yet taken)
- H: hud38 / juice38, listed in Research/2026-10-13-wave38/H.md (KESTREL hold, THUNDERCLAP spin, grapple throw, boss entrance, hit).
- M: doors38 (open, locked, boss door, a noride wall).
- S2: beacon over a lane wall in leg 2; the Stag's chevron behind the island; lane wall / island trims; THE RUN from its spawn; the
  RINK from its spawn.
- O2: each p-room of n38-span and a nailed knee (pips).
- C2: a gate room, a pad, a crown38 gem break.
