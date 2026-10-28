# FORGE71 spikes (2026-10-26; every command NIGHT71.md names was run here or by a scout today, with its date)

## S1 Disk (GG's call 1), 09:25-09:48
- The scan (`wt-scan.sh`, read-only) found 234 worktrees: 203 with no tracked change, 31 dirty, and 1 missing directory.
- `wt-clean.sh` removed **183** of them with `git worktree remove` (no --force); `git worktree prune -v` cleared the missing one.
  - It kept 50: dirty, untracked files, dated 2026-10-25 or later, or n70-*.
  - Every detached HEAD it removed was contained in a branch or tag (0 kept for unreachability).
  - Branches kept: 389. 51 worktrees remain.
- **C: free went from 6-7 GB to 70 GB** (`df -BG /c`). The manifest of removed worktrees is `worktrees-removed.tsv`: date, branch, HEAD,
  path, result. Any of them can be restored with `git worktree add <path> <branch or HEAD>`.

## S2 The n71 tools, 09:50-10:05
- `node Tools/night/forge71-make-n71.cjs` wrote n71-clock, n71-close-check, n71-slate, n71-slate-set, n71-slate-add, n71-runner.sh,
  n71-gates.sh, n71-reels.sh, n71-prune.sh and n71-freeze-rows. One special edit missed on the first run (a wrong pattern); after the fix
  it applied exactly once.
- `node Tools/night/n71-close-check.cjs --selftest`: **SELFTEST PASS (33 cases)**. That is NIGHT70's 30 plus three `play` cases: no play
  build fails; a play build without a committed PLAY71.md fails; the guard's refusal quoted with the card committed passes.
- `node Tools/night/n71-clock.cjs` prints `clock n70: ... stopped` (NIGHT70's stopped clock, so the guard ignores it) and "C: 62.5 GB
  free" (mid-cleanup). `n71-clock.cjs start` at t0 writes the n71 clock (closeTags n71-freeze, n71-play).
- `node Tools/night/n71-slate.cjs Research/2026-10-26-forge71/slate-seed.json`: 19 slices, 19 todo, exit 0 (well formed).

## S3 The room pipeline on the real candidates (scratch only; no tracked file touched), 09:55-09:59
- **kiln:** `git show bfe1529f:Design/levels/n67/kiln/rooms/kb3.json > <scratch>/kb3-d2.json`, then
  `node Tools/night/gen-slots.cjs --level kiln --slots=kb2,kb3c --room-file=kb3:<scratch>/kb3-d2.json --run --out=<scratch>/kiln`.
  - 23 s; `levelgen --walk: 0 ERROR`; "kept rooms bit-equal to n57-kiln's"; the shipped level files unchanged (sha printed).
  - The quick walk's "DID NOT reach the last exit" appears, as the scout said: n46flow is the walk guard.
- **spire:** the same with `--level spire --slots=v6 --room-file=v2:<scratch>/v2-t16.json` (t16 from dfa1ed865).
  - `levelgen --walk: 1 ERROR (pit recovery: v11 ...: n52-spire's own)`. That one ERROR is the base room v11's and is expected.
- **The area lint without `--touches`:** kb3 FAIL 3/4 and v2 FAIL 2/4. With the touch table, NIGHT70 read kb3 4/4. So the lint needs
  G2's skip output for P4 (a) and (c). The brief says `--touches`.
- **The guards, `SLATE=Logs/n68/slate.json LEVEL_DIR=<scratch>/kiln ROOMS="kb2 kb3 kb3c" bash Tools/night/n70-red.sh 6 n67-kiln <scratch>/kiln/red`:**
  rc 0, **13 jobs in 120 s** (09:57:08-09:59:16). The walk, flow-A, seeds 1-10:

| room | result | stall | deaths | falls | clear |
|---|---|---|---|---|---|
| kb2 | GREEN 10/10 | 21.1 % | 0 | 0 | 29.2 s |
| kb3 (d2) | GREEN 10/10 | 25.3 % | 0 | 0 | 50.8 s |
| kb3c | GREEN 10/10 | 32.7 % | 0 | 8 | 24.7 s |

  kb3c's 8 falls: the scout's table read 0/1 deaths on Logs/n68 (falls not listed); a fall returns the player for 10 health. The room
  check's own lines are in room-check.txt per room.
- **The Editor:** `bash Tools/unity-loop.sh status` at 10:00 reads ready, not compiling, playMode stopped. The Player build needs it
  open and stopped.

## S4 Found while spiking (the night's bearings must know)
- **NIGHT70's evidence was never committed.** 165 files under Logs/n70/ sat staged (A) after the close commit 20f07e72e, and HEAD tracks
  0 Logs/n70 files. The close commit's message and HANDOFF claim the opposite. The forge commits them as their own commit (the tag
  u3-n70 stays where it is).
- **Where the rest of the disk is** (`du -sh`, measured during the cleanup, 09:20-09:50): Logs/ **207 GB** (mostly reel frames under
  Logs/feel), Builds/ **79 GB** (one freeze build is about 440 MB, e.g. Builds/n47-r1d .. n49-r3), Library 5.8 GB, .claude/worktrees 14 GB
  after the removal.
  - One more freeze build costs about 0.5 GB plus its reel frames. So NIGHT71's three freezes fit easily in 70 GB.
  - Freeing old reel frames and old builds is GG's call. It goes on DECIDE71, and nothing is deleted tonight.
- **DECIDE45 D-3 is an open GG question** on six old rows (balance32, ride38, doors38 / juice38, stag38, pump40, boost42). The forge's
  grep covered DECIDE6x / 70 only; the auditor found it. None of the six is on the night's delete list; they go to DECIDE71.
