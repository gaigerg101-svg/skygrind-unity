# Tools/night: the lead's helper scripts for a night (moved from NIGHT20's session scratchpad by the wave-21 forge)

Every script runs from Git Bash, `cd`s into the repo, closes each Player it starts by its PID (CLAUDE.md rule 10), and
writes logs as `Logs/player/${NIGHT:-n21}-...`. Set `NIGHT=n22` for a later night.

| script | use |
| --- | --- |
| `smoke.sh <tag>` | the Player smoke (`BDIR=Builds/<dir>`), idle time first; prints the `[smoke]` FAIL / DONE lines |
| `bench.sh <tag>` | `-skygrindBench 60`; the plan and CPU before and after; **skips (exit 3) when the CPU is over 30 %** unless `FORCE=1` |
| `reel.sh <buildDir> <tag> <side> [timeout] [args]` | the measured reel into `Logs/feel/<tag>/<side>` (`-skygrindReelPad` for the pad pass) |
| `wait-build.sh` | polls `build_status` and prints the result |
| `wait-tests.sh <out.json>` | polls `test_status` (up to 50 min) and prints totals, replays, golden, failures |
| `integrate.sh M S W` | cherry-picks each builder's new commits (fill the ID map with tonight's agent ids first) |
| `close.sh <buildDir>` | the close's Player checks in order: smoke, the tape copied aside, golden, fingerprint, replay, bench x2 |
| `rehearse.sh <buildDir> <tag>` | the same without the bench (a round's rehearsal) |
| `final-judges.sh <tag>` | one farm batch of 4 processes from a harness snapshot; slow beside other jobs (narrow it, NIGHT20) |
| `decode.py <key.json> "<picks>" [who]` | decodes a blind reviewer's picks with its key |
| `gifs.py` | before / after GIFs from two reel folders (edit the folders at the top) |
| `resolve-adds.cjs` | merges `design.adds` conflicts in both config copies |
