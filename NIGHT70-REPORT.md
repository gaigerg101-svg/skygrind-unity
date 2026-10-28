# NIGHT70 REPORT (THE VERB THAT HAPPENS; t0 22:43:52, sha 08455efa)

Draft written at 00:52 (2:08 into the night, no usage stop yet). Every claim names its command or file. The close lines in section 8 are still open and are filled at the close.

## 1. What is still not good
- **The GATE is still 0.** Over NIGHT65-69 it was 0 / 22 (NIGHT70.md section 1). Tonight both launch rooms are SHORT: kb3 (R's 22 variants) and v2 (S's variants t1-t22, u1-u4, w1-w2). Numbers in section 4.
- **The verb still happens 3-4 times a run on the plain verb-aware pilot.** On kb3 the plain run's launches are 3-4.5 a run in all 22 variants (R, 01c34248c); on v2 the plain median is 2 (S, dfa1ed865 / 93a27c24e). The fresh bodies arrive (attempts rose from 202 to 238 in R's sight-cut variants) but the rate falls (0.40 to 0.32): a landed punch launches 68-74 % of the time, and 42-52 % of attempts land no punch within 0.8 s (R's reading, its table in R.md, not verified by the lead).
- **The committed pilot (Z3) lifts the chosen run but is slow.** kb3's committed runs make 22-34 launches a run and take 1.5-2x the time of the plain run (49-79 s against 20-40 s, seeds 1-3, Logs/n70/Z3/Z3-skip-kb3-GREEN4.txt), so the faster-of-two rule picks the plain run on kb3 (committed on 2 of 20 seeds, Logs/n70/red-kiln/kb3/skip.txt). On v2 the committed run is chosen on 7 of 20 seeds (Logs/n70/red-spire/v2/skip.txt).
- **Open rule questions** (DECIDE70.md): which run the launch target counts (Q-1); the bar formula's floor under 1.25 (Q-2; the spec's own formula); the fresh test's flat 1.25 against the faster-of-two runs (Q-6, found by the integrated-diff review, Logs/n70/review-night.md:1).
- **Lint and deco on v2:** t16's lint is FAIL 3/4 (a 12.5 m open square in the narthex west; limit 12) (S, 93a27c24e). kb3's d2 fails the deco checker (door kerb touched on 4 of 20 seeds, needs 6) and its stall A is 24.5 % against a ceiling of 22.7 %.
- **GG's reaction** to the last play build (Builds/n69-play, NIGHT69) is not recorded; none since 2026-10-20 (docs/memory/OWNER.md).

## 2. GG's play card
- **Not built.** PC is SHORT (slate). The build needs 12 GB free and C: had 5 GB at 00:12 (the guard's rule; `Logs/n70/STATE.md`). Builds/n69-play is still GG's latest build; no n70-play exists.
- The labels the card would carry: kb3 NOT PASSED (SHORT), v2 NOT PASSED (SHORT), g8 NOT PASSED (not run tonight; NIGHT69's r1f).

## 3. The tools proven (planted pairs; critic verdicts)
- **Z1, the M3 from the 20-seed throw chart** (picked 909b9d45). Planted pair: RED on the t0 readers 9 / 27 (`Logs/n70/Z1/Z1-m3-cases-RED.txt`), GREEN 27 / 27 (`Logs/n70/Z1/Z1-m3-cases-GREEN.txt`; main's run `Logs/n70/z1-m3-cases-main.txt`). Critic: `Z1: PASS` (`Logs/n70/verdict-Z1.md`). Caveats: the GREEN ran on 08455efa plus the working copy; a launch --slide room's M3 now reads the plain-launch chart (DECIDE70 Q-4); "incl. the flier" is not checked.
- **Z3, the committed pilot** (picked a7dcad55; the round-4 literal rule as the default; the burst cap as an option, off). Planted pair RED (`Logs/n70/Z3/Z3-skip-*-RED.txt`) and GREEN4 (`Logs/n70/Z3/Z3-skip-*-GREEN4.txt`, compares `Z3-compare-*-GREEN4.txt`: every plain line IDENTICAL on both rooms; `--no-commit` identity 50 / 50 and 49 / 49: `Logs/n70/Z3/` FINAL4 files). Held-step shots 0. The reds re-made on it (`Logs/n70/queue/002-reds-z3.done`, rc 0): kb3 1.13 (mainBar 1.33), v2 0.99 (plain 0.87; mainBar 1.37). Critic: `Z3: PASS` on all three lines (`Logs/n70/verdict-Z3.md`). Its findings P1 (round-4 headers name an older sha), P3 (the plain STYLE now printed; the fix picked as d9336634), P4 (the bar floor), P7 (kb3's committed run never wins).
- **Z2, the throw chart and the falls** (picked 68671700d). Print-only: the `launchend` lines (14 / 6 / 9 on kb3 / v2 / r1d seeds 1-3) and the falls via fallReturn43 (12 of 12 seed lines on r1d). Identity: kb3 52 identical, v2 51 identical, r1d 29 identical + 12 falls-only (`Logs/n70/Z2/`). Critic: `Z2: PASS` (`Logs/n70/verdict-Z2.md`).
- **The main tree's gates** (job 003 at HEAD a7dcad55 / 468a9856, `Logs/n70/gates-flow-z3.txt`; job 004 at HEAD 68671700d, `Logs/n70/gates-flow-z2.txt`): `gates: all GREEN`; flow EQUAL to `Logs/n70/flow-base.txt`; golden 39 vector sets, 22 replays exact, 0 mismatches.
- **The integrated-diff review** (`Logs/n70/review-night.md`): the game is unchanged with the rows OFF (nothing under Assets/, Design/, PortExport/, Packages/, ProjectSettings/ changed; the four rows `enabled: false`; slots all old). 8 findings, 1 major (Q-6), 7 minor (Q-2, Q-7).

## 4. What failed, with numbers
- **kb3 (level kiln, SHORT).** Target: launches per plain run >= 6 (the card: the red's median 4 + 2; shipped median 4, NIGHT69's r1d 2.5). Best candidates: d2 `bfe1529f` (M2 1.32, launches 4, deco FAIL; `cand` on the old harness); d2 on the Z3 harness `7509d3237` (M2 PASS 1.39 on the chosen runs, plain alone 1.32, plain launches median 4, committed 22; `kb3/d2z/skip.txt`); k2 `01c34248c` (1.34 chosen, plain 1.27, launches 3). Twenty-two variants: plain launches 3-4.5 in all; attempts 202 (red) to 238 (k1); the rate 0.40 to 0.32 (R's table in R.md). Verdict: SHORT; the room file stays d2 (`bfe1529f`; R's last commit `01c34248c`).
- **v2 (level spire, SHORT).** Target: launches >= 4 (the red's median 2 + 2). Best: t16 `dfa1ed865` / `93a27c24e` (M2 1.32 against the new bar 1.37 on the Z3 harness; plain alone 1.30; plain launches median 2, committed 5); t6 `86176a45` (1.27 on the old harness; 2 launches). Lint FAIL 3/4. Verdict: SHORT (slate `shortNote`).
- **LV (the level rows): SHORT, not started.** It starts at the first room PASS (no later than 6:00); no room has one.
- **PC (the play build): SHORT.** The guard refuses the build under 12 GB free (C: 5 GB at 00:12). Re-checked at the close (section 8).
- **The reds' bars moved.** v2's bar rose from 1.25 to 1.37 because the committed run lifted the red (0.87 plain to 0.99 chosen; `n70-red-bars.cjs --write`). Any v2 candidate from before the Z3 harness reads against 1.37 now.
- **The committed pilot did not help kb3.** On seeds 1-3 the committed runs were slower on every seed (kb3 chosen on 0 of 3 with the literal rule; `Logs/n70/Z3/Z3-compare-kb3-GREEN4.txt`); on 20 seeds kb3's committed side is chosen on 2 (`Logs/n70/red-kiln/kb3/skip.txt`).
- **The first rule for a candidate (D-4) held all night.** No candidate reached a main round; no a1 was queued. The flow base and the reds were the only room-side runs on main (`Logs/n70/queue/`).

## 5. Not checked
- **No Player.** No freeze build, no reels, no smoke, no Editor run: the guard refused the builds (disk under 12 GB); the close records each refusal under `## Blocked` (section 8).
- **Fresh seeds 21-30** for kb3 and v2: not run (no candidate cleared the screen, so no GATE round).
- **40-seed screens** (red team M6): not run.
- **Rebuilt levels in a Player:** none rebuilt; nothing flipped; every row OFF.
- **gg-proxy on the final rooms:** not run (no new build; nothing to rank beyond the reds).
- **The critics' unchecked items:** Z1's "incl. the flier", the GREEN run on the working copy (main's run agrees); Z2's `dead` end is outside the contract's list; Z3's walk on a7dcad55 (the gates job 003 covers the main tree).
- **Chart and lint details** of the h / j / k variants (R): flow guards and throw charts only for a1, a2, a3, c1, c2 and d2.

## 6. Your calls
- DECIDE70.md, Q-1 to Q-7: the launch target's run (plain or chosen), the bar's floor, the chargers in the committed pilot (the literal rule kept as the default; the cap as an option), the slide rooms' M3, the room result, the fresh bar with the gain, and the minor tool findings.

## 7. The next prompt's seed
- Decide Q-1 and Q-6 first: both change what a room PASS means. A night that repeats the NIGHT70 screen without them will read SHORT again.
- The bottleneck is the verb's conversion, not the room: the plain pilot's attempt-to-launch step (R's table: 42-52 % of attempts land no punch within 0.8 s). A spike on the plain pilot (an instrument change, written in DECIDE before any measurement, re-made reds) is the next question; the committed pilot shows the launch count can rise (22-34 on kb3) at the cost of time.
- v2's clear time: S's pooled 120-seed read: the TANK alone at the end of a run sets the seed's ratio (0-3 s alone: median 1.21; 10 s or more: 0.76). A design question for GG (the TANK's walk and its death).
- v2's lint: the narthex square (12.5 m against 12).
- Disk: C: at 4-5 GB (other sessions' worktrees are most of it); a build needs 12 GB. Freeing disk is GG's call (the worktrees under the scratchpad are other sessions').

## 8. The close lines and every ROW line
- ROW: n65.foundry65 enabled=false; SLOTS: a2=old a3=old a4=old a5=old b1=old a8=old b2=old
- ROW: n67.iceline67 enabled=false; SLOTS: g1=old g2=old g3=old g4=old g5=old g6=old g7=old g8=old
- ROW: n67.kiln67 enabled=false; SLOTS: kb2=old kb2c=old kb3=old kb3c=old kb3d=old
- ROW: n67.spire67 enabled=false; SLOTS: v2=old v5=old v6=old
- The slate: `slate: 7 slices: 3 PASS (passes true), 4 SHORT, 0 BLOCKED, 0 in progress, 0 todo` (`node Tools/night/n70-slate.cjs`, after the close's writes). PASS: Z1, Z2, Z3. SHORT: kb3, v2 (the screen), LV (not started: no room PASS), PC (the play build refused under 12 GB free).
- The close started at 7:00 (05:43:52 wall; `Logs/n70/close-start.txt`: "the close starts at 7:00 since t0"). The runner exited at its own 7:00 check (runner.log: "the clock reads 420 min (7:00): the close; exit", 05:43:52). The gates ran in the foreground at 05:44-05:52 (`Logs/n70/gates-flow-close.txt`, HEAD 8ef612ab9, "gates: all GREEN 05:52:16"). The Player steps were refused by the guard (NIGHT70-LOG.md, `## Blocked`).
- The tag line: pending (`u3-n70 -> <sha>` is printed by `node Tools/night/n70-close-check.cjs --tag` after the close's commit).
- MISS list: none logged. `node Tools/night/n70-next.cjs` ran at 0:00 (bearings), at 1:03 (its table: Z1 critic, Z3 critic, both rooms missing a round); no miss was found when the printer's pieces were checked against the picks (checked by hand, not by a log entry: open for the close to confirm).
- **MISS (the lead's, logged):** Logs/n70/stops.txt was never written: the rule "every turn, append the clock line to stops.txt" was not followed all night. So the first of E70's numbers cannot be measured from that file.
- **E70's three numbers:** (1) uncommitted minutes at each stop: **no usage stop happened** (none in 7:00 hours), so no stop-time figure exists; (2) runner minutes computed during the stops: **0** (no stop; the runner computed four jobs while the lead was awake: 001 flow base 4 min, 002 reds 8 min, 003 gates 5 min, 004 gates 5 min, `Logs/n70/runner.log`; idle from 00:13 to 05:43 with nothing queued); (3) close minutes after CAP: **0** (the close ran 7:00 to 7:08 and every close step ran before 7:15, before the CAP at 8:00).
- Other items the close needs: `Logs/n70/close-start.txt` (7:00); the clock stop at 7:30 or later (the clock's own check); the tag (`n70-close-check.cjs --tag`) after the close's commit.
