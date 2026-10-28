# Wave 21: THE MOVES AND THE MOMENTS (2026-10-07 08:20 to ~13:30, branch port/stage3, box 5 to 6 h, tag u3-n21)

**Ask:** GG, 2026-10-07: "I WANT BUILDING new fetaures that match and ideas, innovate, resreach" and "notable changes ...
additions innovations role played iterate". Task U3-35. Planning set `Research/2026-10-07-wave21/` (forge, red-teamed v2).

**What happened first:** the terminal crashed before the run; the Unity Editor reopened on "Recovering Scene Backups" (09:09)
and stayed there until GG clicked it (~11:30): **about 3 h of 5 without compile, tests, builds or Play**. Builders M, S, W
(and W2) built Core + SimHarness only, in worktrees (NIGHT21-LOG.md).

**Shipped** (each behind `design.n21.<row>`; `-skygrindOff21` = NIGHT20's game; NIGHT21-REPORT):
- THE ARCLINE in Survival (yank 15 s, zip, swing), SLIDE BITES, THE STAMP (round grades; the magenta fix), DEADLINE, THE GRIND:
  on; INSTANT REPLAY (Core ring only): off; challenge codes (Core only): on, no UI.
- Tape format 6, N21Blocks, `FingerprintBaselineN21` (recorded once, 1d2500a), `FingerprintBaselineN20` kept for `--off21`.
- Judges: SimHarness arcline / slidebite / stamp / deadline / grind / challenge / `chase --twist deadline`; feel-metrics M-MAGENTA.

**Numbers (all from runs that night):**
- Suite first run in Unity 1620 / 1626 (4 stale tests fixed, 2 Editor timeouts); frozen code 1625 / 1626, replays 22 / 22, golden 97 / 97;
  the red, AutopilotTests.PerStep_AllocatesNothing, 8 / 8 twice alone.
- M-MAGENTA 0 px on 9,571 Player reel frames (NIGHT20's frame 9487): T-01 closed.
- Headless: arcline attaches 10/10, peak 30.7 m/s, yank 8.6 m; grind median wave 8 over 10 seeds; KITE r10 under DEADLINE 48 % of normal
  (25 / 10 s fuse gave 60 %, retuned to 18 / 5); stamp grades A12 B11 C3 D3 S3 over 32 rounds.
- Final Player: three fingerprint lines EQUAL, golden 0 mismatches, replay EQUAL; smoke 199.5 s DONE on build 1; the final build's
  first smoke failed 3 pad steps, the re-run alone was 200.1 s DONE. Bench not run (CPU 100 %).

**Close: 3 DONE / 8 SHORT of 11; 0 of 7 contract `passes`.** SHORT reasons (NIGHT21-LOG close table):
- .2 the FeelReel phase-E partials never placed; .3-.8 each lacks its reel scenario (no Editor for 3 h, then the time went to the suite)
- .5 stamp entrance + T-11 not built; .6 no fuse bar; .7 no smoke GRIND step; .8 no replay view
- .10 one persona triage, no veto reviewers or blind rounds (no N21 scenario to show them)

**Prompt shape:** NIGHT21.md 385 lines, 11 items, builders 3 (M / S / W by file ownership) + W2 (challenge codes, added by the lead), four planned
rounds (none run), freeze 4:30 (done 12:47), caps builds 7 (used 2), reels 14 (2), farms 4 (0), bench 4 (0). Build share: not checked.
**Forge experiment verdict:** hypothesis "at least 4 of the 6 headlines DONE" FAILED (0 of 6 passed their contracts); sprint contracts did
stop false "done" claims (nothing flipped without a Player or reel line), but no contract could pass because each needed a reel
scenario that nobody was assigned to build. GG's switch answers: none yet (DECIDE21).
**Went wrong:** (1) the lead polled `build_status` for 10 minutes on a build that was never queued (`build` needs `--confirm true`);
(2) an unanswered Editor dialog; (3) M, S and W each enabled or left rows without the lead's reel scenario plan.
**Late (13:50):** reel phase E written after the tag: arcline ran on the Player (pull 1, attach 1, M-MAGENTA 0, S12 0 pad), slidebite read 0 bites (scenario placement); still 0 of 7 passes.
**Late (13:58):** slidebite reel fixed (the spawn spot was beside the jump pad at (0, -26.5)): `slide bites 3`, contract met: 1 of 7 passes, 4 DONE / 7 SHORT.
**Lessons added or confirmed:** L-025 .. L-028 (new), L-014, L-015, L-022 (confirmed), L-021 (confirmed).
**Reaction:** GG, after the Editor was opened: "ok its opened now try again and finish full prompt"; then "finish" (2026-10-07).
