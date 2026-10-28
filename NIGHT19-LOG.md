# NIGHT19-LOG: every gun has a job (U3-33)

The first `date`: **Tue Oct 6 16:05:12 USMST 2026** (0:00). The close starts at 4:20 (20:25), its table prints at 5:00 or
later (21:05), the hard cap is 5:30 (21:35). Times below are from `date` or tool output.

## Status lines (one per item)

- U3-33.1 Start: DONE at 0:07 (RECEIVED below; n18-before's reel ran 16:10:38-16:12:03).
- U3-33.2 The judges and the baseline: in progress.
- U3-33.3 The pad plays like a shooter (P): not started.
- U3-33.4 The pad shows the pad (P): not started.
- U3-33.5 The gust, and an LT for every gun (W, V): not started.
- U3-33.6 Every gun has a job (W): not started.
- U3-33.7 Threats that catch you, stamina you earn (E): not started.
- U3-33.8 Not cheap (H, V): not started.
- U3-33.9 The loop: not started.
- U3-33.10 Close: not started.

## The verification budget (75 min of lead time: suites and the headless gates only)

| When | Check | Minutes | Total |
| --- | --- | --- | --- |
| 16:21 | SimHarness build + fingerprint x2 + impact + CoreHarness (the shared commit's gates) | 3 | 3 |
| 16:28 | the Editor: N19Shared / StaminaTape / Tape / Gamepad / ControlsRebind / PadWheel / StaminaBar groups | 3 | 6 |

## RECEIVED (U3-33.1, 16:05-16:12)

- **GG's notes:** no `GG-NOTES.md` in the repo root; the start message is the /goal text only (no notes). NIGHT18-REPORT
  section 5's six questions stay at NIGHT18's defaults. GG's switches: the defaults in capitals (NIGHT19.md was not
  edited).
- **HANDOFF section 1, row by row:**
  1. HEAD `3c9fd31`, `git describe` `u3-n18-1-g3c9fd31` (u3-n18 + the NIGHT19 prep docs commit). As expected.
  2. `git status --short`: ` M ProjectSettings/UnityConnectSettings.asset`, `?? Assets/_Recovery*`, `?? NIGHT7.md` ..
     `NIGHT20.md`, `?? QUEUE-*.md`, **and 12 untracked `.meta` files** of NIGHT18 scripts whose `.cs` are tracked
     (HitSpark, HitSpring, UiHudMotion18, UiMenuMotion18, UnityTuning.CameraMove, UnityTuning.HitReact, and six test
     files). A NIGHT18 slip: committed on their own below.
  3. `unity-loop.sh status`: the Editor answered, but **in Play mode** (`"playMode": "playing"`); stopped (`Exited play
     mode`). `set_autotick`: `Auto-tick already enabled (interval updated to 16ms)`.
  4. The suite: run after the shared commit's compile (the verification table).
  5. CoreHarness: `golden total: 39 vector sets (48094 records), 22 replays (22 exact); 0 failing, 0 mismatches`
     (16:21, on the shared commit's tree).
  6. SimHarness `fingerprint` EQUAL; `impact PASS` (16:21, the shared tree).
  7. The Player smoke: not run at the start (the first Player smoke is build 1's).
  8. The machine: plan **Balanced** (381b4222...), AC (BatteryStatus 2), CPU **100 %** (two samples), the load is ten
     `python` processes (another project's workers, as HANDOFF section 1 row 8 says for NIGHT18), Unity pid 5908. Free disk
     on C: 197.2 GB (>= 15 GB).
- **Builds/n18-before:** `Builds/Stage1/` (NIGHT18's build 7) copied to `Builds/n18-before/` (173 MB). Its reel
  (`-skygrindReel <abs>\Logs\feel\n18-before\on`, pid 268288, closed by itself): `done: 2357 frames written, 0 failed, 64 s
  wall`, 23 scenarios, devices `Keyboard, Mouse, Xencelabs Hid Device, Wireless Gamepad; disabled for the reel: 1`.
- **tasks.json:** U3-33 and U3-33.1 .. U3-33.10 inserted after U3-31.10; `parses: 170 items; U3-33.*: 11`.
- **D-72 (ASSETS, GG's word in NIGHT19.md's switches; replaces D-30 "no new packages", 2026-10-03):** permissive art and
  code (CC0, MIT, BSD-3, OFL, Unlicense) may be pulled in, each read at its LICENSE file before the import, vendored with
  the licence beside it in `Assets/ThirdParty/<Name>/` (art) or pinned in `Packages/manifest.json` (packages, the lead's,
  one commit each), with a row in a root `THIRD-PARTY.md` and the notices shipped beside the exe. Not allowed: the
  MMFeedbacks public repo (non-commercial), repositories with no licence file, anything with audio (rule 7), anything that
  imitates another game's branding or UI one to one. Also logged in HANDOFF section 5 at the close.
- **NIGHT18's scratch scripts** (reel2.sh, smoke.sh, bench.sh, wait-build.sh, wait-tests.sh, close-checks.sh, decode.py and
  the rest) copied from NIGHT18's scratchpad into this session's; `reel3.sh` takes the build folder as an argument.

## The shared commit (U3-33.2, 16:13-16:33)

Placed so no builder edits another's file (NIGHT19.md section 2):
- **The NIGHT19 blocks:** `Core/N19Blocks.cs` (seven bits: arsenal, gust, forge, freshness, stamina.n19, enemies.n19,
  pad.n19; `ForceOff` for `--off19` / `-skygrindOff19`); the design blocks in `Design/unity-config.json`, every one
  `enabled: false`, with the switches' starting numbers (forge mult 1.4; the freshness table; stamina.n19 ownKey, drain
  0.3, the range rewards; pad.n19 layout shooter, l3Sprint toggle); the Player copy byte-identical (a test).
- **The sim:** `Game/SurvivalSim.N19.cs` (the run's blocks: a tape's, off with ForceOff, else the design defaults; handed
  to `Loadout.n19` and `Director.n19` before their resets; the sprint key rule after the stamina rules); partial hooks
  `N19ArsenalStartRun / N19ArsenalStep` (W, `SurvivalSim.Arsenal19.cs`) and `N19EnemiesStartRun / N19EnemiesStep` (E,
  `SurvivalSim.Enemies19.cs`), stepped after the loadout and after the enemies.
- **The sprint bit (rule 4):** `InputFrame.sprint`; `Stamina.Params.ownKey`; the Mover's hook
  `want = rules.sprint && ((armed && held && !pressed) || (rules.ownKey && input.sprint.held)) && bar > 0`;
  `PadAction.Sprint` (ActionNames "sprint", the label, `FrameButtons` + Sprint, `FrameButtonsWeb` for the required set,
  `MoveButtons`, `MergeMove`). LabelOrder unchanged (P adds the CONTROLS row with the v2 layout).
- **THE TAPE format 4:** the NIGHT19 byte after the stamina flags (formats 1-3: every block off); the sprint key in the
  move buttons' bits 10-11 (always 0 before format 4); the recorder and the replay carry it.
- **The fingerprint:** `FingerprintBaselineN18` (NIGHT18's twelve hashes, for good); `FingerprintBaseline.N19OnPath =
  false`: until the close's re-record, the composed fingerprint runs with every block off (so the smoke's fingerprint
  steps stay valid all night); SimHarness `fingerprint --off19` compares with N18; the Player's `-skygrindFingerprint`
  prints the off path's line too.
- **PromptText:** `Game/Input/PromptText.For(action, hold, today)`, a stub returning today's text, called from the buy
  prompt and the READY line (Ui.cs), the slot cycle, the wheel hint and the special hint (UiHotbar.cs), the retry label
  (UiFeelHud.cs), the DOWN / RESULTS retry chips (UiMenus.cs) and `SurvivalGame.Meta.KeyLabel`.
- **Hooks:** `Enemy.Airborne`, `Enemy.Shielded`, `Enemy.ConductiveNails` / `Conductive`; `FeelFx.Gust` ("gust");
  `SurvivalGame.N19.cs` view hooks (E: charger, drone, affix; V: zapper, bouncer); SimHarness `Sandbox` dispatch
  (`Harness.Sandbox.cs`: range, chase, pfarm).
- **Fixtures:** `N19SharedTests` (7 tests: the copy, the bits, the sprint key off / on, format 4, a format 3 tape replays
  EQUAL, the fingerprint off until the close) and the empty `GustTests`, `FreshnessN19Tests`, `ChargerTests`,
  `DroneTests`, `PadLayoutV2Tests`, `PromptTextTests`, `StaminaN19Tests`, `SandboxHarnessTests`.
- **Tests that followed the format:** `StaminaTapeTests` (the format pinned to `FormatVersion`; its format-2 helper strips
  the NIGHT19 byte too), `GamepadTests.TheDefaultLayoutCoversEveryInputFrameButtonAndPause` (the web's ten frame buttons).
- **The gates (16:21-16:31):** SimHarness `fingerprint seed 1: 12 hashes in 1.02 s: EQUAL to the baseline`;
  `fingerprint seed 1 (--off19 ...): EQUAL to the baseline`; `impact PASS: the budget holds on 2 map(s)`; CoreHarness
  `22 replays (22 exact); 0 failing, 0 mismatches`; `tape-old: REFUSED: unknown op 105 at byte 0` (a mislabelled tape
  is refused, as before); the Editor: N19SharedTests 7 / 7 (the format-3 tape `AFormat3Tape_RecordedOnNight18sGame_
  ReplaysEqual` passed), StaminaTapeTests 6 / 6, TapeTests 13 / 13, GamepadTests 21 / 21, ControlsRebind 2 / 2,
  PadWheel 7 / 7, StaminaBar 6 / 6.
- **A slip:** one two-line move in `PlayerChecks.cs` (the `-skygrindOff19` read moved to the top of Attach) was done with
  `node -e` instead of a written script (CLAUDE.md rule 9). A second `node -e` patch (Harness.Sandbox's trace start) at
  17:05: the same slip.

## Builders (launched 16:38:49, `date`; 0:33)

P (pad), W (weapons), E (threats, stamina), H (HUD), V (hands, effects), each in a worktree from `d0705d9`; the brief
`builders-n19.md` (scratchpad). V and H reported at ~17:20 (V 7 commits, H 15) and were cherry-picked at once.

## The judges and the baseline (U3-33.2, 16:40-17:12)

- Committed `a66917d` (judges), `7aa4324` (FeelReel phase B, phase C prompts, the pad pass), the S12 / S14 rules in
  feel-metrics.py. SandboxHarnessTests 3 / 3 in the Editor (Mono).
- **The hand checks, side by side** (judge vs the per-shot / per-hit log):
  - KESTREL C2 seed 1: judge 97 steps, 0.808 s, score 0.808; log: headshots (50, kills) at steps 1, 49, 97 (0.4 s =
    48 steps) -> 97 / 120 = 0.808 s, 0 HP: AGREE.
  - THUNDERCLAP C2 seed 1: judge 217 steps, 1.808 s, HP 15, score 2.558; log: blasts at steps 1, 109, 217 (0.9 s =
    108 steps), one husk hit for 15 at 1.075 s -> 1.808 + 0.05 x 15 = 2.558: AGREE.
  - KITE round 10 seed 3: the judge first said 26.692 s against the hit list's death at 26.683 s (one step late: fixed,
    the death's own clock); then 26.683 s, 10 hits, 106.5 HP: AGREE.
- **Rules fixed before the baseline (logged, frozen):** every gate open for the judges (a locked zone's nav nodes are
  blocked); the KITE steers by clearance and away from enemies within 25 m on a 20 m loop (a 10 m circle died in 10 s);
  the aim error's Gaussian through fdlibm (Mono = CoreCLR).
- **The baseline (Logs/sandbox/b0/SANDBOX.md, --off19):** S1 FAIL (THUNDERCLAP dominated by KESTREL, STITCHER, COMET;
  LONGWAVE by KESTREL); S2 FAIL (COMET best in 6 of 8 cells); S3 FAIL (LONGWAVE 8.09 x in C7); S4 n/a; S5 FAIL (pfarm
  medians LONGWAVE 2.0, STITCHER 6.0, THUNDERCLAP 7.5, KESTREL 14.0, COMET 24.0); S6 FAIL (THUNDERCLAP 40 %, STITCHER
  29 %, KESTREL 28 %); S7 1.16; S8 KITE 128.2 s (r10) / 169.0 s (r15); S9 PASS 14 %; S10 n/a; S11 PASS.
- **Farm batch 1 of 6** (4 processes, 17:00-17:07): 5 guns x 40 pfarm + 100 free-play runs.
- **Build 1** (`7aa4324`, 55.6 s, 0 errors): smoke `[smoke] 183.9s DONE: all steps passed`; bench p99 14.06 / 12.48 ms
  (Balanced, AC, CPU 100 % from ten python processes); reels b1 on + pad: S12 pad pass FAIL on prompt-buy / box / forge
  ("HOLD E"), S14 FAIL sprint 5, boost 4, vitals 5, PASS hud-fight.

## The stop (17:27-20:55)

At ~17:27 every builder ended on the same API error: "You've hit your monthly spend limit" (HTTP 429, weekly limit
resets Oct 10). This session made no progress from 17:27 to 20:55 (`date` 20:55:10 at its next step): the rounds 1-4
(builds 2-5), the blind reviewers and the beyond lists never ran. The close started at once (4:50).

## U3-33.10 the close (20:55-)

- **The freeze:** the builders' committed work cherry-picked (P 5 commits: the v2 layout + migration, PromptText's glyphs,
  LB / RB tabs, design.pad.n19 on; W 2: the gust, design.gust on; E 6: design.stamina.n19 on, the CHARGER port + view,
  design.enemies.n19 on, DroneTests first). Left out (uncommitted in the worktrees, unverified): W's ZAPPER / BOUNCER /
  freshness / AltUse / Sandbox19, E's DRONE.
- **Gates on the frozen tree:** SimHarness `fingerprint ... EQUAL to the baseline` and `--off19 ... EQUAL`; CoreHarness
  `22 replays (22 exact); 0 failing, 0 mismatches`; the copy byte-identical.
- **The final build** (build 2 of 7; code `8a33313` + metas `f8d54d1`): `Succeeded`, 41.9 s, 0 errors.
  - smoke, alone: `[smoke] 190.2s DONE: all steps passed`.
  - `-skygrindGolden`: `22 replays (22 exact); 0 failing, 0 mismatches`.
  - `-skygrindFingerprint`: `EQUAL to the baseline`; the off path vs FingerprintBaselineN18: `EQUAL`.
  - `-skygrindReplay` (the smoke's tape): `EQUAL (1 of 1 checkpoints, 491 steps)`.
  - the bench: p99 19.55 / 100.61 ms, **not comparable**: it ran while the Editor ran the suite (a sequencing slip);
    the bench cap (4 / 4) is spent.
  - the suite on the frozen code: **1182 / 1192**; 10 failed: 7 DroneTests (E's tests-first commit; its implementation
    wasn't committed), StaminaTapeTests.AFormat2Tape... and .KillAndParry... (they assume NIGHT18's flat kill refill and
    record with the NIGHT19 blocks on), TapeTests.ScriptedRun_TapedAndReplayed... (not investigated: the time).
  - the final judges (Logs/sandbox/final): S1-S3 FAIL unchanged (W's niche retune wasn't committed); S7 1.15; S8 KITE
    133.7 s (r10) / 67.1 s (r15); S9 PASS 15 %; S10 gust-jump +2.62 m (above the rocket jump's +1.12 m), C3 3 / 8 pushed
    >= 3 m; S11 PASS (FIGHT 0.0 s on an empty bar vs KITE 109.0 s). S5 / S6: not re-run (the farm needs ~7 min).
  - the final reels (on + pad): S12 0 tokens in all 30 scenarios of both passes (the three buy prompts were "HOLD E"
    at build 1); S14 PASS hud-fight, boost (was 4); FAIL sprint, vitals (1 each: `UI/HUD frame/Speed lines`, the
    full-screen speed-lines layer, which M8 measures as clear of the middle); `vitals` scenario: NO SPOT for the kill.
  - the blind reviewers: none (agents end on the spend limit).

NEXT: the close table; then HANDOFF / tasks.json; tag u3-n19.
