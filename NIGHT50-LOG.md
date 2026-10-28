# NIGHT50-LOG

## CLOSE (2026-10-19 17:45 MST; generated from Tools/night/n50-rows.json by `python -I Tools/night/n50-close.py`)

| row | title | state | passes | evidence | note |
| --- | --- | --- | --- | --- | --- |
| P0 | Judge calibration (control pairs, content checks, both orders, NIGHT49's pairs re-judged) | DONE: the protocol passes its controls (same frame TIE, 6 px shift TIE, sharp beats blur 3-0); NIGHT49's pairs re-judged, 5 of 8 verdicts flipped (DECIDE50 D-3); the raw votes are in Tools/night/n50-votes.md | true | Logs/n50/judge-keys/cal-1.json; Logs/n50/judge-keys/cal-2.json; Logs/n50/judge-keys/cal-3.json; Logs/n50/judge-keys/rej-1.json; Logs/n50/judge-keys/rej-8.json; DECIDE50.md D-2, D-3 | judges were fresh sonnet subagents, both orders per judge, a pick only when both orders agree (Tools/night/n50-rubric.md) |
| G0 | Shared commit: design.n50 rows (charge50, gunglow50, off), look49.numbers.levels = [], a150 reel routing, N50SharedTests, n50-rows.json | DONE: commit 484fef18; cmp identical; N50Shared 3/3, N49Shared 2/2, Golden 97/97, compile clean | true | commit 484fef18; Tools/night/n50-shared.cjs | - |
| G1 | The charge bar back under the reticle (arc40 re-placed outside the 18 px reticle box) | GREEN on the pair and the frames (the re-placed bar beat NIGHT46's placement 3-0; nearest pixel 22.5 px at 1080p, 10 px tall); the Player RED run (-skygrindOff50) was not run, so the check is not complete | false | Logs/feel/n50-r2/h1080/a150/shot-N30-H50-on-50-1920x1080.png; Logs/feel/n50-r2/h1080/a150/shot-N30-H50-old46-50-1920x1080.png; Logs/n50/judge-keys/g1-charge.json; Logs/feel/n50-r2/summary50.txt | STITCHER heat on the arc was not checked; the off state is the reel's forced off in the same run, not a separate Player run |
| G2 | The charge on the gun, four steps (25 / 50 / 75 / 100 %) on KESTREL, THUNDERCLAP, PULSE | SHORT, fallback shipped: KESTREL and THUNDERCLAP read in the reel at 1080p and 720p (50 % and 100 % frames seen by the lead); PULSE OFF in code (commit 9bd04af8) | false | Logs/feel/n50-r2/h1080/a150/shot-N30-H50-glow-revolver-50-1920x1080.png; Logs/feel/n50-r2/h1080/a150/shot-N30-H50-glow-revolver-100-1920x1080.png; Logs/feel/n50-r1/h1080/a150/shot-N30-H50-glow-gauntlet-75-1920x1080.png; Logs/feel/n50-r2/summary50.txt | PULSE: its steps sat inside the fist's block (gauntlet-0 and -75 read alike). The Player RED run was not run. Frames at 0 / 25 / 75 not viewed by the lead (the 50 % and 100 % frames were) |
| G3 | Arenas and colours: look49 per level (look49.numbers.levels) | SHORT, fallback shipped: look49 ON on n48-foundry and n47-spire (meter 4 of 4 each; pairs TIE); kiln OFF (meter 2 of 4, the pair had won); iceline and span OFF (the saturated band fails; the one attempt did not pass) | false | Logs/feel/n50-r1/v-n48-foundry/a349; Logs/feel/n50-r1/v-n47-spire/a349; Logs/feel/n50-r1/v-n47-kiln/a349; Logs/feel/n50-r1/v-n47-iceline/a349; Logs/feel/n50-r1/v-n47-span/a349; Logs/n50/judge-keys/rej-5.json; Logs/n50/judge-keys/rej-6.json | the grayscale audit is logged, not gated. On foundry and spire the grade reads as a strong teal and yellow-green shift (the lead's look, not judged against the palette). The default (kiln ON) was amended: its meter fails |
| G4 | One impact kit ON (fx49), the enemy kill's wash <= +0.25 (the lead's edit behind fx49) | SHORT: fx49 ON; the kill's wash peaks at +0.27 on one frame (f8) and sits at the bar on another (f6, +0.25), against a +0.28 peak with the kit off. Every other tier passes (light +0.17, heavy +0.27 with the kill, critical +0.20; off +0.34 / +0.48 / +0.50) | false | Logs/player/n50-reel-n50-r2-x1080.log; Logs/player/n50-reel-n50-r1-x1080.log; Logs/feel/n50-r2/summary50.txt; Logs/n50/judge-keys/rej-4.json; Tools/night/n50-votes.md | tried: the heavy tier's size 0.40 and share 0.24 (n50-r2, n50-r3; both reverted, d05bf5f1; the kill stayed at +0.27). Kept in the freeze: the enemy death burst's scale 0.5 (Fx49KillBurstScale), which moved the kill from +0.28 (fx49 off) to +0.27 (fx49 on) in r1, not enough. The per-effect fallback (the kill on its old path) was not run. The kit's pair (rej-4) went 3-0 for the kit |
| G5 | Lights stay ON (lights49), verbs unchanged | GREEN: lights49 ON at the freeze; verbs37 26 / 20 / 25 / 30 / 30 on the five levels (RED rows [] on each, NRE 0). The smoke FAILED (4 pad steps) and is listed separately under Done when | true | Logs/feel/n50-freeze/summary47.txt; Logs/n50-smoke-freeze.txt; Logs/n50-smoke-freeze-2.txt; Logs/n49-smoke-now.txt | the NIGHT49 control build fails the same four pad steps in this session; the reel frames show a 'GAMEPAD CONNECTED' toast, but the reel's device list (Keyboard, Mouse, a Xencelabs HID device) does not name a gamepad, and the smoke log has no device line. So the cause is probable, not shown (THREADS T-73 has the pattern) |
| G6 | The ULTRAKILL / DOOM effect as one: before / after contact sheet per level (n49-freeze vs n50-freeze) | SHORT: the grade-only sheet is built (the grade changes on 2 of 5 levels, so at most 2 of 5 can win on that sheet); the full before / after sheet the row asks for (n49-freeze vs n50-freeze per level, with the charge and the kit) was NOT built, its Player runs were not run. Not judged | false | Logs/n50/sheet/sheet.png; Tools/night/n50-sheet.py | the grade-only sheet compares the saved a349 combat frames (look off vs the freeze state). The full sheet would change all five levels (fx49, charge50 and gunglow50 are ON at the freeze and OFF at the NIGHT49 base); the row's GREEN test is 3 of 5 on the full sheet, not run |

**Done when (NIGHT50.md section 6):** G1-G6 as above (no row GREEN on all its checks: G1's Player RED run, G2's PULSE and Player RED run, G3's
iceline and span attempts and the grayscale gate, G4's kill, G6's full sheet). `n50-freeze` verbs37 passes (26 / 20 / 25 / 30 / 30, nothing lost):
YES. Smoke DONE: NO (four gamepad steps fail; NIGHT49's control build fails the same four; rerun unplugged). Full suite green: YES on
8ec0a1c0 (2217 run, 2217 passed; the first run on 22adc1d9 had 2 failures, both fixed in 8ec0a1c0). Golden / fingerprints EQUAL: YES on
22adc1d9. Flow table: NOT RUN. Tag `u3-n50`: set at this close (message lists the caveats).

**Reviewer (fresh subagent, evidence only; its report on the table):** four gaps, all fixed or answered:
- P0: "4 verdicts flipped" was wrong (five of eight): fixed in the JSON; the raw judge votes were not saved: now in Tools/night/n50-votes.md (rej-8's o1 votes corrected to B, A, B; its result is unchanged, new 2-0).
- G4: "+0.27 on two frames" was one frame at +0.27 and one at the bar: fixed; "reverted" was wrong for the death-burst scale (0.5, still in the freeze, moved the kill +0.28 to +0.27): fixed.
- G5: the connected-gamepad cause was stated as shown: now stated as probable (a toast in the frames, no device line).
- G6: the sheet was grade-only but the row asks for the full sheet: fixed in the state and the note.
- G0, G1 (the RED caveat), G2, G3 were OK as written; G1's reel verdict prints RED at both sizes (PULSE), noted in the reviewer's report only.

**Not verified this night (said plainly):** the smoke on a pad-free session; the flow table; G1's Player RED run; G2's PULSE and its frames at 0 / 25 / 75 (the lead viewed 50 and 100 only); STITCHER heat on the arc; the grade's colour read against the palette (the lead saw a strong teal shift on foundry and spire).

**Memory:** the system stopped the first fixed-commit suite run (8ec0a1c0) at 17:41 for low memory; its Unity batch child finished on its own and wrote Logs/n50-freeze-tests2-tests.xml (2217 / 2217). Nothing was restarted.

## NIGHT50-LOG (earlier entries follow)

## STOP POINT (15:58 MST; base 9165ee0a; P0 and G0 done; cap ~19:43; close starts by 19:15)

Close table so far (from `Tools/night/n50-rows.json`; every row still `passes: false` until its evidence is printed):

| row | state | evidence / reason |
|---|---|---|
| P0 judge calibration | DONE (protocol PASS) | controls: same frame TIE x3, 6 px shift TIE x3, sharp beats blur 3-0. NIGHT49 re-judged (DECIDE50 D-3): H c3 vs arc40 flipped 3-0 -> TIE; V kiln and iceline new 2-0; spire, foundry, span TIE; L kiln TIE; X kit on 3-0. Logs/n50/judge-keys/ |
| G0 shared commit | DONE (config copies cmp-identical; N50Shared 3/3, N49Shared 2/2, Golden 97/97, compile clean) | commit 484fef18 |
| G4 kill-wash scale (lead, fx49-gated) | EDITED, NOT MEASURED | commit 48a93558: Fx49KillBurstScale 0.5 behind fx49; N49 prefix 5/5; the wash number is not checked yet |
| G1 charge bar back under the reticle (H, charge50) | NOT STARTED | default: arc40 re-placed outside the 18 px reticle box (DECIDE50 D-5) |
| G2 charge on the gun, four steps (H, gunglow50) | NOT STARTED | fallback: KESTREL + THUNDERCLAP only |
| G3 grade per level (V, look49.numbers.levels) | NOT STARTED | default: [n48-foundry, n47-spire, n47-kiln] (DECIDE50 D-4); iceline and span one attempt |
| G4 impact kit ON (fx49) + kill wash (lead) | NOT STARTED | fallback: kill effect on its old path |
| G5 lights stay ON (lights49) | ON (from NIGHT49) | verbs37 to be re-checked at freeze |
| round 1 integrated (16:25) | BUILT, NOT JUDGED | H: 14395e9b 5397ee9e 8d334d4d 25742a24 (charge50, gunglow50, a150). V: c2fbee4e b4bc7218 (per-level gate, iceline/span attempt, a349 audit). Compile clean; V50 6/6, N50 3/3, N49 5/5. Player Builds/n50-r1 (16:24): drift only GG's two ProjectSettings files. Build copy rows on: charge50, gunglow50, fx49 (repo config untouched). Reels running: `n50-reels.sh n50-r1 h x v` -> Logs/feel/n50-r1/summary50.txt |
| G6 before / after sheet per level | NOT STARTED | |

## 15:58 P0 and G0 (the lead)
- P0 protocol: `Tools/night/n50-control.py` (controls), `Tools/night/n50-rubric.md` (the question and three criteria), judges are fresh subagents (sonnet), each order a separate call, every pair in one order per call.
- Controls: cal-1 same frame TIE 3/3 (both orders); cal-2 6 px shift TIE 3/3 (one J1 o2 vote B, overridden by its o1 TIE); cal-3 blur: the sharp frame wins 3/3. PASS.
- NIGHT49 re-judge (the four deciding pairs plus the level grades): see DECIDE50 D-3 for each verdict and its flip against NIGHT49.
- G0: `Tools/night/n50-shared.cjs` (dry run, then real); design.n50 charge50 + gunglow50 (off), design.n49.look49.numbers.levels = []; UnityTuning.H50.cs, FeelReel.N50.cs (a150), N50SharedTests (3/3); n50-rows.json.
- Not done: X builder (no spawn, DECIDE50 D-6); the rest of the builder rounds.

## STOP POINT (15:43 MST, kickoff t0; base 9165ee0a; cap ~19:43; close starts by 19:15)

Close table so far (from `Tools/night/n50-rows.json`, written at G0; every row passes: false until its evidence is printed):

| row | state | evidence / reason |
|---|---|---|
| P0 judge calibration | IN PROGRESS | protocol being written; control pairs not run |
| G0 shared commit (rows, n50-rows.json, FeelReel a150) | NOT STARTED | |
| G1 charge bar under the reticle (H, charge50) | NOT STARTED | default: ship arc40 re-placed outside the 18 px reticle box |
| G2 charge on the gun, four steps (H, gunglow50) | NOT STARTED | fallback: KESTREL + THUNDERCLAP only |
| G3 grade per level (V, look49.levels) | NOT STARTED | default: ship foundry, spire, kiln |
| G4 impact kit ON (X, fx49) + kill wash (lead) | NOT STARTED | fallback: kill effect on its old path |
| G5 lights stay ON (lights49) | ON (from NIGHT49) | verbs37 unchanged to be re-checked at freeze |
| G6 before / after sheet per level | NOT STARTED | |

## 15:43 P0 progress
- `Tools/night/n50-pair.py` written: `stage` makes both orders per judge (o1, o2) and a seeded key; `tally` takes each judge's vote in
  each order (A / B / TIE) and counts a pick only when both orders name the same frame. Judges can vote TIE.
- Smoke test (same frame twice): stage writes four files; tally of six TIE votes -> TIE; tally of A A A / B B B -> old (consistent pick).
  Both measured here. The control's identical-frame case is what must return TIE on real judges; not yet run with judges.
- Not done yet: `n50-pair-check.py` (content / size / key-sha check before judges see a pair); the NIGHT49 re-judging; G0.

## 15:46 P0 progress (continued)
- `Tools/night/n50-pair-check.py` written. Pillow is installed. Control pair (same frame, both orders): PASS, 480x480, mean 62.0.
  Negative test: a black frame staged against a good one: REFUSE "nearly black" on the black image's order (exit 1). Measured here.
- Still open in P0: real judges on the control pairs (3 judges x 2 orders; identical frames must come back TIE); NIGHT49's four deciding
  pairs re-judged; then G0.

## 15:43 Lead
- Read NIGHT50.md, PROMPT-RESEARCH.md, NIGHT49 pair keys (`Logs/n49/judge-keys/`), `Tools/night/n49-pair.py`.
- The goal is set. The run cannot finish within this session in full; the STOP POINT above is the honest state. Next: P0 control pairs,
  then the shared commit.
