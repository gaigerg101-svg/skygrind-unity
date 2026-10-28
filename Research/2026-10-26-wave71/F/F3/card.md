# F3: feel46 kicks bigger and rarer (T-77; design.n71.kicks71, a view row; OFF in the repo, ON in the play build)

- **The change, plainly:** a kill now snaps the view up clearly (body 2.2, head 3.0, punch 3.6 deg; was 0.9 / 1.3 / 1.7 and off), more than the shot's own kick, while hits kick at most about three times a second and a multi-kill is one kick.
- **Off flag:** `-skygrindNoKicks71` (also `-skygrindOff71` or older; `-skygrindOff46` turns feel46 and these off). ON: a play build made with `n71.kicks71` in n65-play-build.sh's row list.
- **Reel (proof, the lead):** on the ON build, `bash Tools/night/n71-reels.sh <tag> k71on,n52-foundry,a546 k71off,n52-foundry,a546,-skygrindNoKicks71` (a546 needs no room: SafeSpot on any map).
  GREEN (on): `V46-on probe: ... hit kicks N, kill kicks M ... kicks71 on (...)` with N, M > 0, the line `design.n71.kicks71 on): the kicks' counts as expected`, `GREEN a546 feel46`.
  RED (off): `hit kicks 0, kill kicks 0 ... kicks71 off`, `design.n71.kicks71 off: -skygrindNoKicks71`.
- **SPOT pair:** `k71spot,n52-foundry,a546,-skygrindSameRun63,n71.kicks71`: every a546 shot gets its held twin with the row off (Shot46 now ends with the same-run hook); the pair for the spotter is `V46-on-kill-050` / `V46-on-pumpkill-050` in `<side>-on` vs `<side>-off`. The "off" pass's twins are a no-op control (feel46 off in both halves).
- **Numbers:** design.n71.kicks71.numbers (15, both copies; re-apply with `node Research/2026-10-26-wave71/F/F3/kicks71-config.cjs`). Rarer: hitGapS 0.35, killGapS 0.12; the THUNDERCLAP shot keeps 0.35 of its shake (feel46: 0.6).
- **Limits:** G4's cut is applied at the shot, so a same-run held off frame keeps it; the kick and FOV stacks are drawn only on frames the row reads on.
- **Gates:** with the row off: gates-off.txt (config identical, golden 0 mismatches, 22 replays exact, fingerprints EQUAL x12). With the row on: EQUAL by construction (Tools/SimHarness compiles Core + SurvivalSim*.cs, no view; nothing it runs reads kicks71); a flipped run: not run.
- **EditMode (the lead):** N71KicksTests (5 tests), FocusAutomation61Tests.
