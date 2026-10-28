# NIGHT59 builder V: V2 yellow56's halo

Builder V, worktree branch `worktree-agent-acfb96294df513625` from e4aad59a. No Editor, build, Player, reel or run_tests was run by V. Every
"after" below is SIMULATED on frames that already exist (said so on its line) or "not measured". After every commit
`dotnet run --project Tools/SimHarness -c Release -- fingerprint` and `... --off58` printed `EQUAL to the baseline`; `bash Tools/night/dn-check-wt.sh`
printed 0 errors before the code commit. No file under `Assets/Scripts/Core` changed.

## Commits
- 501b16f2: the planted case first, `python -I Tools/night/n59-v-yellow.py planted`: **RED (3 of 10 wrong)** on the shader / config at e4aad59a
  (the halo H 48 S 0.99 V 0.84 -> S 0.45; no keep bound in Cap57.shader or Cap57Math).
- bc5550fa: the fix. `dimYellow58.sKeep` 0.85 and `dimYellow58.sKeepVMin` 0.30 (config, written by `node Tools/night/n59-v-rows.cjs`, both copies
  byte-identical, nothing else changed); Cap57.shader skips the dim-yellow cap for S >= `_Cap57YH.w` and V >= `_Cap57.w`; Cap57Math and n57-v-cap.py
  mirror it; `Yellow59.N59V.cs` reads the numbers (off: `-skygrindOff59` or lower, which is NIGHT58's rule exactly). vFull / vNone unchanged.
  Planted **GREEN 12 / 12** (the halo untouched; the lava edge S 0.64 V 0.81, the plate S 0.56 V 0.82 and spire's olive S 1.0 V 0.19 capped; the orb
  core untouched; the -skygrindOff59 side caps the halo as NIGHT58 did). n57-v-cap.py planted 15 / 15, n58-v-yellow.py planted 4 / 4 (unchanged).
- c2f2e0f1: the halo meter `Tools/night/n59-v-halo.py` (planted GREEN 6 / 6). It reproduces gate-x.md on the same frames: ON 4558 -> 918 px
  (20.1 %), OFF 5310 -> 4842 px (91.2 %).

## Why 0.85 and a value floor (not 0.90 alone)
Simulated on the n58-freeze a358 `-nocap` twins (the rule applied to the twin; n58-v-yellow.py's measure on the result; the no-keep simulation
matches the Player's main frames within 0.00-0.07 % on 11 of 12 frames, spire v5-mid excepted: 1.65 % simulated vs 0.04 % shot, and left out
of the "worst" column):
| sKeep / floor | halo kept (sim) | worst non-parry frames (sim) |
|---|---|---|
| none (NIGHT58) | 38 % (Player: 20 %) | a5-entry 0.28 % |
| 0.90 / none | 67 % | spire v6-mid 0.72 %, a5-entry 0.44 %, v5-entry 0.32 % |
| 0.90 / 0.30 | 67 % | a5-entry 0.43 % |
| 0.85 / 0.30 (shipped numbers) | 71 % | a5-entry 0.46 %, spire v2-mid 0.30 % |
| 0.80 / 0.30 | 75 % | a5-entry 0.50 % |
| 0.75 / 0.30 | 84 % | a5-entry 0.58 %, spire v2-mid 0.71 % |
Without the floor, spire's near-black olive (S 1.0 at V 0.18-0.19) passes the S bound. The halo's pixels on the twin: S p25 0.81, p50 0.95,
V p5 0.32; about 28 % of them sit at S < 0.85 and V < 0.89, so no S bound keeps 80 % without letting more of the lava edge (S p75 0.79, p90 0.94)
and spire's plates through, and the MAIN meter counts this halo itself as non-parry (gate-x.md (4)): keeping it raises a5-entry's MAIN.

## MAIN / GUARDS (before -> after)
- MAIN (frames over 0.5 % non-parry yellow, a358 foundry a5 + spire v5, ON): NIGHT58 0 / 12 (Player). After: not measured; simulated 0 / 12 except
  spire v5-mid, whose simulation is not calibrated (1.65 % with NIGHT58's own rule vs 0.04 % shot).
- GUARD halo kept >= 80 %: NIGHT58 ON 20.1 % (Player) -> SIMULATED 70.8 % (`n59-v-halo.py <a5-entry.png> --sim`). Likely SHORT of 80 % even if the
  Player matches the simulation; the lead's b2 reel decides.
- GUARD spire v5-mid orbs within 10 %: not measured (simulated orb share 21.68 % -> 22.57 % on the twin).
- verbs37 x5: not measured (no Player).

## For the lead
Reels: `v2on,n52-foundry,a358` and `v2on,n52-spire,a358` on b2 (yellow56 ON in its copy), the off side `-skygrindOff59` (NIGHT58's yellow56)
or `-skygrindNoYellow56`. Meters: `python -I Tools/night/n58-v-yellow.py <dir>` (MAIN), `python -I Tools/night/n59-v-halo.py <dir>` (the GUARD:
every foundry a5-entry with its -nocap twin). The a358 line names the bound: "a yellow at S 0.85 or over and V 0.30 or over kept (NIGHT59 sKeep)".
Frames for the critic: foundry a5-entry ON / OFF and its -nocap twin; spire v5-mid ON / OFF.
Note: `node Tools/night/n58-v-rows.cjs` rewrites the whole dimYellow58 object: run `n59-v-rows.cjs` after it, or the bound is lost.
