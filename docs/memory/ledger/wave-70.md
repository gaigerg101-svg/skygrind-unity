# Wave 70 (NIGHT70, THE VERB THAT HAPPENS; 2026-10-25 22:43:52 MST to 2026-10-26 ~06:20, 8 hours planned: BUILD 0:00-7:15, FREEZE, CAP 8:00; tag u3-n70)

**Ask (GG 2026-10-25, verbatim, the FORGE70 request):** "can we use the same 2 methods to prompt another 8 hour overnight prompt" (OWNER.md, the forge asks table). Standing: GG owns the enemy design; the night was one run, no usage stop.

**Prompt shape:** FORGE70 (Research/2026-10-25-forge70/: scout.md, design-research.md, prompt-methods.md, spike.md, DECISIONS.md D-1..D-10, redteam.md, slate-seed.json, builder-brief.md, room-chain.sh, GOAL.txt); NIGHT70.md 120 lines (acts keyed to the usage resets, the queue runner through the stops, a 7:00 close, a clock gate: the close-check needs the clock stopped between 7:30 and 8:00 since t0); the n70 tools (Tools/night/n70-*); slate 7 (Z1 Z2 Z3 tools; kb3 v2 rooms; LV; PC); builders R (kb3), S (v2), T (tools); one review slot (critics, the integrated review). The usage window never stopped the night. The runner (Tools/night/n70-runner.sh) computed four jobs (flow base 4 min, reds 8 min, gates 5 min, gates 5 min) and idled from 00:13.

**What shipped (game):** nothing. Every room row OFF; no file under Assets/, Design/, PortExport/, Packages/ or ProjectSettings/ changed (review-night.md, the one question). Harness and tools: Z1 the M3 from the 20-seed throw chart (909b9d45); Z3 the committed pilot (a7dcad55: the slate's literal rule as the default, the burst cap an option off; P3 print d9336634); Z2 the throw chart and the falls via fallReturn43 (68671700d, print only). Each: planted pair, critic PASS (Logs/n70/verdict-Z1/Z2/Z3.md).

**Numbers (the room screen; M2 VERB pooled seeds 1-20 vs bar; the chosen run is the faster of plain and committed per seed):**
- kb3 (launch, bar 1.33, red 1.13): 22 variants by R; best d2 (bfe1529f) 1.32 plain-era; re-screened on Z3 (7509d3237): 1.39 chosen, plain 1.32, plain launches median 4 vs target 6. SHORT.
- v2 (launch, bar 1.25 -> 1.37 on Z3, red 0.99 chosen / 0.87 plain): t16 (dfa1ed865) 1.32 chosen, plain 1.30, plain launches median 2 vs target 4; M1 lint FAIL 3/4. SHORT.
- LV: not started (starts at a room PASS). PC: refused by the guard (12 GB needed; C: 5-6 GB at the close).

**Estimate to actual** (the NIGHT70.md targets, measured from commit times): cards 0:30 -> about 0:20 (R, S); Z1 0:40 -> 0:22; Z3 1:20 -> 0:48 (round 4 at 0:54 after the critic's literal-rule finding); Z2 1:50 -> 1:24; the first room candidates 0:50 -> 0:25 (none cleared); the room work ended 1:24-1:31; the gate runs 5-8 min each; critics 3-6 min each (Z1 198 s, Z3 294 s, Z2 300 s, integrated review 360 s). Idle: the lead waited from 1:31 to 7:00 with nothing queued (no usage stop; the close-check clock gate sets 7:30 as the earliest stop, so ~5 h of waiting followed the room result). The forge's act plan (a1 computed in the stop) had nothing to compute: no room cleared the screen, so no a1 was queued.

**SHORT, with reasons (the slate's shortNote):** kb3 (launch count on the plain run, the card's target 6; the committed side passes M2 but the rule names the plain run); v2 (launch count 2 vs 4; lint 3/4); LV (no room PASS: its rule starts it, so no row flip); PC (the play build needs 12 GB free; refused twice at the close, quoted under NIGHT70-LOG.md ## Blocked).

**Reviews:** Z1 critic PASS (caveat: a launch --slide room's M3 reads the plain chart; the flier clause unchecked). Z3 critic PASS on lines 1-3 (P1 the round-4 header sha; P3 the plain STYLE, fixed by print d9336634; P4 the bar floor under 1.25 (the spec's own formula); P7 kb3's committed run never wins). Z2 critic PASS (print-only re-counted: 52 / 51 / 29 identical; end `dead` outside the contract's list). The integrated-diff review (8 findings; major: the fresh test (M2 FRESH, seeds 21-30) reads the chosen run against a flat 1.25). Gate runs GREEN on main at a7dcad55 / 468a9856 and at 68671700d; the close's gates at 8ef612ab9 (gates-flow-close.txt, all GREEN).

**Lessons:** L-152 (new): a room's screen counts only the run its card names, and an instrument change to the chosen run moves every bar and test that reads it (re-make the reds; the fresh test's bar). L-146 merged into L-128 (the same headroom rule). Forge note for the next prompt: when the room screen fails by 1:30, the night's remaining hours are the clock's gate (7:30 minimum); put a measured spike there or shorten the night's plan, do not leave it idle.

**Bookkeeping miss (the lead's):** Logs/n70/stops.txt was never written (the per-turn clock line rule was not followed), so E70's first number cannot be read from a file. The clock arithmetic was also misread once (STATE.md wrote the 7:00 mark as 06:43; the clock tool gives 05:43:52). Both logged in NIGHT70-REPORT.md section 8.

**Closed threads:** T-102 (the wave-memory harvest for w47, and the missing w32 / w48 / w50 / w51 ledgers) moved here to hold THREADS at its 60-line cap. No ledger will be written for those waves: the record is CORE.md's "no w32 / w47 / w48 / w50 / w51 ledgers" note and this paragraph. No action is left.

**Threads opened:** T-156 (the verb's conversion: plain launches 3-4.5 on kb3, 2 on v2; the committed pilot's time cost; v2's TANK and its lint); T-157 (GG's calls: the launch target's run, the bar floor, the fresh test's flat bar; LV and PC waiting on a PASS and on disk).

**Reaction:** none yet (no build of this night reached GG: Builds/n70-play was not built; Builds/n69-play is still GG's latest; last recorded reaction 2026-10-20, OWNER.md).

**FORGE71's post-mortem** (2026-10-26, Research/2026-10-26-forge71/postmortem-n70.md):
- Work share 1:37 of 8:00 (20 %), and one idle block of 5:23 (00:20-05:43, no agent, no commit).
- Causes: one plan with no backlog; acts keyed to a usage stop that never came; the GATE instrument as the night's purpose; disk; a
  prompt too dense to follow.
- The close left 165 Logs/n70 files staged and uncommitted (committed by the forge as 9d0476e3c).
- GG's answer (2026-10-26): "Bots guard, I judge".
- Lessons for the w71 harvest: a next item when the main bet fails and a line for an event that never comes; after N failed nights on a
  metric gate, ask whether it should decide.

**Seed for the next prompt:** decide T-157's Q-1 and Q-6 first (both change what a room PASS means); then one spike on the plain pilot's attempt-to-launch step (an instrument change written in DECIDE before any measurement, the reds re-made); v2's lint (the narthex square) and the TANK's walk (GG's design call). Free disk before the night (a build needs 12 GB; C: was 5-7 GB; the other sessions' worktrees hold most of it).
