# L2 (the frame spike F1 names): waiting for F1's bench; what the saved benches already say

- **Not started as a fix:** the source is not named yet. F1's bench prints `bench loop top source:` (the player-loop slot that grows in the worst 1 %); L2 starts from that line.
- **No regression window (bench-history.txt, every saved bench log, oldest first):** every 1920 x 1080 bench since NIGHT18 has the worst-1 % spikes (p99 8.2-17 ms, engine part 8.2-20 ms). The only clean runs, NIGHT21's n21-bench-n30-base / -r7 (p99 3.72 / 3.74 ms, 0.17 / 0.04 % over 6.94 ms, engine worst 4.45 / 3.78 ms), ran at **1280 x 720**.
- **So:** the main-thread spike scales with the resolution. That points at a pixel-bound cost reaching the main thread (a GPU sync or readback, a full-screen pass's CPU side) rather than scripts, the sim or GC. A lead, not a source: the 720p and 1080p runs are different nights' builds.
- **A cheap check for the lead's quiet window, if there is room:** the same build, `-screen-width 1280 -screen-height 720` against 1920 x 1080 (`-skygrindVsync 0` both), compared on `bench loop` slots. Not run by F.
- **Noise seen:** NIGHT29's crt / off pairs (n29-5-bench-*) swing 2.7 % -> 26.9 % over 6.94 ms between two OFF runs: the power plan (MEMORY: bench power plan flips). Judge only pairs whose `.power` file shows one plan.
