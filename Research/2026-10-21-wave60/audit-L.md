# NIGHT60 audit pass, builder L (NIGHT60.md section 2 step 3, items d / e / f; read-only; 2026-10-22 ~05:00-05:2x)

Sources read: docs/memory/THREADS.md (60 lines, wave 60: "aged two waves or more" = first seen w58 or earlier), docs/memory/FEATURES.md
(112 lines), docs/VERBS.md (154 lines), and tonight's builder L logs. S <= 30 min, M <= 90 min of one worker with its verification.
The three best S items are marked **TOP 3**.

## (d) THREADS.md rows aged two waves or more, with a next approach a worker can run
- [ ] **A-L1 (S) TOP 3: T-91 re-measured with idle60 on**: THREADS.md line 33 (w48): chargerKiln48's seeds 19 and 37 "hit the 240 s cap
  with a drone high in the air"; B10 (Research/2026-10-21-wave60/L-b10.md) found campaign drones never get NIGHT20's climb rules and freeze
  mid-climb under a roof (31764 of 35700 drone idle steps on iceline); `n46flow --level n52-kiln --rows chargerKiln48,idle60 --seeds 40` against
  the same without idle60 says whether that cap is the same freeze (then T-91 joins idle60's row, not the pilot).
- [ ] **A-L2 (S) TOP 3: T-67 closed by tonight's numbers or re-opened with one**: THREADS.md line 17 (w43): "L1's walk bot 0 / 10 through THE
  GANTRY (W3 -> W4 hop)", next "F1's n46flow pilot on the gantry link"; Logs/n60/flow-base.txt prints `n52-foundry SUMMARY: GREEN 10/10`
  (that pilot walks every link): read which link holds the gantry on n52-foundry and close the thread with that line, or name the level it
  still fails on.
- [ ] **A-L3 (S) TOP 3: T-124's spire half done; the foundry dwell left**: THREADS.md line 51 (w52): "re-flow spire" is open, but
  Logs/n60/flow-base.txt already re-flows it (`n52-spire SUMMARY ... stall 17.5 %`, w52's cross was 14 -> 17 %): log that number on the
  thread, and run `n46flow --level n52-foundry --per-seed` to name the rooms behind foundry's dwell (16.8 -> 13.9 s at w53; 14.0 s in flow-base).
- [ ] **A-L4 (S) T-98's pad pass**: THREADS.md line 36 (w48): the crack prompt shows "RT" on the pad by code, "the pad pass is not run":
  a148 with the virtual pad as Gamepad.current (the lead's Player run; one reel spec).
- [ ] **A-L5 (S) T-104's iceline combat frame**: THREADS.md line 40 (w49): "run a349 on iceline's combat frame for the 4/4" (polish53's
  icelineGrade ON, a349's 4/4 not run): one reel spec for the lead.
- [ ] **A-L6 (S) T-138 closed after B9's Player run**: THREADS.md line 60 (w56): "a Player run without -skygrindSmoke that loses focus while
  Playing and prints Paused"; built tonight (n60-b9: 4ac49514, ead21a72; `bash Tools/night/n60-focus-probe.sh <tag> green` / `red
  -skygrindFocusProbe60NoPause`): close it with the two DONE lines, or log the steal that did not take.
- [ ] **A-L7 (M) T-102's missing ledgers**: THREADS.md line 38 (w48) and CORE.md ("no w32 / w47 / w48 / w50 / w51 ledgers"): four
  `wave-memory harvest NN` runs from NIGHT47 / 48 / 50 / 51's LOG and REPORT (the w32 sources are older; not checked).
- [ ] **A-L8 (M) T-75: F1's lava rule in Walk38Pilot**: THREADS.md line 20 (w45): the Game walk pilot does not avoid lava ("bot 1 / 10 on
  n45-foundry without A1's harness rule"); the harness pilot's rule is Harness.N46.Flow.cs's F1Input (LavaAt / the nearest edge + 1.5 m);
  tonight's B14 found its one blind spot (the nearest edge can be a wall: kb3c's run end), so port it with that case.
- [ ] **A-L9 (M) T-82: a void fall as a kill for the pull**: THREADS.md line 26 (w46): a body knocked into a level-4 void gives no pull back
  ("Rooms.cs:164-168 ends it, not a kill"); a sim row (test first) and n46flow on n52-span.
- [ ] **A-L10 (M) T-83: THE WICK's climb top third**: THREADS.md line 27 (w47): 58-78 % first tries vs 80 % ("a 150 ms late release runs
  past a panel's end"); the next approach is written ("longer top panels or a ledge lip at each panel end").
- [ ] **A-L11 (M) T-100: IgnoreFocus in every reel that presses**: THREADS.md line 37 (w48): reels that press through a virtual pad lose it
  when the window loses focus; the n33 recipe (and SmokeGuard55's: InputSystem.settings.backgroundBehavior = IgnoreFocus) is in a748 only.
- [ ] **A-L12 (M) T-52 / T-53: the bend's game-time length**: THREADS.md line 12 (w34): "measure the bend with a per-step pilot (L-071)".
- [ ] **A-L13 (L) T-59 / T-66: the pilots that climb the stack and ride a wall in a Player**: THREADS.md lines 14 and 16 (w37 / w43): both
  need a pilot per sim step (Wick37Pilot, move43's); spec them in NIGHT60-PROPOSALS.md, do not build tonight.
- GG's own calls, not worker items (aged, no worker step written): T-18, T-20, T-57, T-79, T-103, T-107, T-112, T-117, T-132, T-134, T-136.

## (e) FEATURES.md rows OFF with a written next step
- [ ] **A-L14 (M) idle60's second cut: the heavy holders**: tonight's B10 cut 1 (n60-b10, 8f458b87, OFF): drones 32.6 -> 5.5 % idle but the
  MAIN missed (over-20 % 18.4 -> 15.6 %); the wardens (27.9-30.2 %; 80 % of their idle steps hold the heavy token, half with the brain
  wanting them still) and chargers are next: read why the brain parks a heavy holder before any rule (L-b10.md).
- [ ] **A-L15 (M) press57's meter**: FEATURES.md line 112 (w57: press57 OFF) and THREADS.md line 57 (T-135): "press57's MAIN missed because
  the bot's still steps are 31-73 % blocked (the metric, not the tell)": the meter's still steps split by the pilot's stick (Harness.N57.Press.cs
  already counts "still steps with the pilot's stick at rest / pushing"), then the MAIN on the stick-at-rest steps only, fixed before numbers.
- [ ] **A-L16 (M) step55 re-tried**: FEATURES.md line 110 (step55 OFF) and THREADS.md line 55 (T-133): "step55 x0.74 (want <= x0.50), not
  re-tried" on a real step-down.
- Not items: kill55 / yellow56 / rim56 (tonight's B2 / B3 / B7), lamp60 (B8), span58 (GG, DECIDE60 D-8); ssao55 / decor55 OFF with no next
  step written (FEATURES.md line 110: "ssao55 (no defect)").

## (f) docs/VERBS.md rows without a check
- [ ] **A-L17 (S) row 37 interact / buy names its check**: VERBS.md line 62: "not performed (SURVIVAL wall buys; the shop has its own reel)";
  the slice smoke performs a hold-E buy (SliceSmoke.cs: "buys ARMORY at its gate (hold E)"): cite that step as the row's check (a doc line).
- [ ] **A-L18 (S) row 36 ARSENAL / QUICKDRAW names its check**: VERBS.md line 61: "not performed (style names; NIGHT34's rea34 line)": cite
  rea34's line (or n46flow's STYLE column) as the check, or mark it not checked.
- [ ] **A-L19 (M) row 16 glide rail / ice glide performed**: VERBS.md line 31: "not performed by verbs35 (map feature; the walk bot crosses
  it)": a verbs37 line on n52-iceline that rides one rail (an event the reel counts; not checked which event exists).
- [ ] **A-L20 (M) row 35 weapon wheel and the pad column re-performed**: VERBS.md lines 60 and 147: the wheel "not performed by verbs35
  (pad-only UI; PadRbCycle tests)" and the v4 pad column (RB grapple, Y wheel, LB dash, R3 punch) "not re-performed": a verbs37 pad pass with
  the virtual pad (T-98 / T-100's IgnoreFocus first).
- Rows 32 / 33 (NAILGUN, TORCH) and 31 (the THUNDERCLAP's shockwave) SKIP with a written reason (not owned / shockwaveOff25): not findings.
