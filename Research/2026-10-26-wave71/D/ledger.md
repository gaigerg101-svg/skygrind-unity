# NIGHT71 builder D ledger (cleanup; t0 10:46:36 MST, base t0b 106dd56e7; one line per commit or block)

Format: time since t0 | item | PASS / blocked | sha | evidence (all under Research/2026-10-26-wave71/D/)

- 0:21 | baseline at 106dd56e7 | gates all GREEN (golden 0 mismatches, 22 replays exact, fingerprints EQUAL x12); dn-check 0 errors | - | gates-base.txt, dn-base.txt
- 0:29 | D1 flow46 (design.n46.flow46 removed; N46SharedTests.ViewRows list) | PASS | 4da846d8e | D1-flow46.md, flow46-checks.txt
- 0:37 | D1 drone49 (design.n49.drone49 removed; the ON term in EnemyViews.Drone.cs:286; N49SharedTests.ViewRows list) | PASS | 78cf3f743 | D1-drone49.md, drone49-checks.txt
- 0:49 | D1 teach42 (design.n42.teach42 removed; UiTeach42.cs + .meta deleted; UiHud33 hooks; first42 reel terms) | PASS | 28ef8cbf9 | D1-teach42.md, teach42-checks.txt. NOTE for the lead / critic: DECIDE43 line 3 lists "D-11 teach42" among DECIDE42's open rows carried (no later page carries it); one revert if read as open
- 0:54 | D2 N1 (6 stamina-bar constants, UnityTuning.CameraMove / Hud17) | PASS (compile) | 3bdbd2e7a | N1-proofs.txt, N1-checks.txt
- 0:58 | D2 N2 (5 points-pop literals + GlacierSpikeWidth) | PASS (compile) | 8893ca10e | N2-proofs.txt, N2-checks.txt
- 1:02 | D2 N3 (Hud17SwapNameH, Hud17CardSwapFrom, Crown37BeamGain; Crown37ShardSize kept) | PASS (compile) | 0d40b0c9f | N3-proofs.txt, N3-checks.txt
- 1:06 | D2 N4 (N37Fixes.cs + .meta) | PASS (compile) | 7c9546104 | N4-proofs.txt, N4-checks.txt
- 1:10 | D2 batch 8 (ShortName, HotbarLabel, HotbarShortNames, HotbarLabelPad; test FeelHudTests.AHotbarLabelIsNeverACutWord removed) | PASS (compile) | 66dd2ee68 | B8-proofs.txt, B8-checks.txt
- 1:15 | D2 batch 9 (WheelHintRender, HotbarWheelHint, 3 fields, 2 constants; test FeelHudTests.TheWheelHintTeachesPadPlayersWithThreeGuns removed) | PASS (compile) | 37a3e77f2 | B9-proofs.txt, B9-checks.txt
- 1:17 | D2 batch 10 (RenderSpecialHint, SpecialHintText, 4 fields + the per-run reset, Hud20ShockWord, HudSpecialDivider, HudPadSpecial) | PASS (compile + gates) | (this commit) | B10-proofs.txt, B10-checks.txt (dn-check 0 errors; the D2 gates on the last batch: config identical, golden 0 failing / 0 mismatches, 22 replays exact, fingerprints EQUAL 12 / 12, "gates: all GREEN 12:08:22")
- 1:22 | D ends: list exhausted to the 1:20 cut. Removed vs t0b (Assets + Design): about 359 lines deleted, 20 changed; 2 files + 2 .meta deleted (UiTeach42, N37Fixes); 2 tests removed (FeelHudTests). Commit free was 7.1 GB at 1:19 (a Git Bash fork failed with Win32 error 1455 once; three Unity processes at ~7.6 GB each)

## Not done (the 1:20 cut)
- D2 batches 37-51 (NIGHT61's 49 + 2 read-outs, ShotTour.RunReel): not started (the cut). Their proofs are not re-run at this head.
- Core batches 52-59: not tonight (the brief).

## For the lead (DELETE items not in D1, and follow-ups)
- shift46 (audit DELETE, score 8): not D's; moved to DECIDE71 (its -skygrindShift46 flag feeds two reel scenarios).
- After batch 10, `SpecialHint20.ZoomWord` is test-only (Hud20Tests); after batches 9 / 10, `wheelHintText` and `specialText` are still
  built inactive (removing them changes the UI hierarchy). Follow-ups, not deleted.
- Test classes changed (the lead runs them in the Editor): N46SharedTests, N49SharedTests (Skygrind.Core.Tests), FeelHudTests
  (Skygrind.Game.Tests). The first42 reel's line lost its teach42 terms (D1 teach42).
