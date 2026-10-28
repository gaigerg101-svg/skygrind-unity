# F1: the camera's chop measured honestly (T-152a; a bench switch, not a game row; nothing the player sees changes)

- **The change, plainly:** the bench can now run with VSYNC forced on or off (`-skygrindVsync 1` / `0`) whatever the profile saved, and every bench names which part of Unity's frame grows in the worst 1 % of frames.
- **Off flag:** none needed (bench only). `-skygrindNoLoopSplit71` leaves the player loop unsplit.
- **The lead runs (quiet window, the same build, median of 3 each, power plan polled by the wrapper):**
  `bash Tools/survival-bench.sh 60 Builds/<tag>/SKYGRIND.exe Logs/player/n71-bench-vs0-<k>.log "play -skygrindVsync 0"` and the same with `"play -skygrindVsync 1"` (`vs1-<k>`), k = 1..3.
- **Fields each arm prints:** `bench vsync:` (the arm, vsync, cap, the display's Hz); `bench result:` (median, p95, worst 1 %, `vsync N`); `bench tail:` p99, p99.9, max, frames over 6.94 ms (share); `bench refresh:` frames over 1.5 display refreshes (share; the chop under VSYNC); `bench cpu:` / `bench breakdown` (FrameTiming main, present wait, render thread, GPU); `bench loop:` the loop's own p99 / p99.9 / max, the 8 player-loop slots by excess, `bench loop top source:`, and kill / hit / shot shares; `<log>.loop.csv`.
- **The spike source, from the n64-rc2 and n63 CSVs (spike-read.txt, spike-align.txt):** main-thread time outside SurvivalGame.Update in 154 of 156 worst frames (n63: 144 of 147), each interval paired with the frame it measured; not GC (0.6 %), not the sim (0.40 ms), not SurvivalGame.Update (1.55 ms vs 1.18). The render thread is also busier (3.57 vs 1.05 ms). **Which engine system: not found from code** (a release Player has no Profiler markers); `bench loop top source:` names it on the lead's first bench run.
- **Note:** the old `engine = interval - Update` row pairs frame n-1's interval with frame n's Update; aligned, it reads the same (9.03 vs 8.76 ms).
- **Proof RED / GREEN:** RED = n64-rc2 `vsync 0` only (no arm); GREEN = `bench result: ... vsync 1` on the `-skygrindVsync 1` arm. Fingerprints: not touched (the bench is its own mode).
- **Numbers:** UnityTuning.F71.cs (1.5 refreshes, 8 slots, 600 rows/s, 192 slots).
