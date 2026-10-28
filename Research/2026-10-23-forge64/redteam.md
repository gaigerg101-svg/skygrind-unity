# FORGE64 red team: NIGHT64.md, GOAL.txt, slate-seed.json and the n64 tools (2026-10-23 ~09:10-09:35)

Read-only: the draft, its forge files, HANDOFF, the n64 tools, guard.cjs and each slice's code. Ran the HANDOFF lines needing no Editor or dotnet,
`node --check` / `bash -n` on every n64 tool (all OK), git status, commit free and disk. No build, Player, reel, Editor, gate or test. Worst first.

## 1. CRITICAL: the PASS-to-sha check will block the night's own flips (n64-freeze-rows.cjs)
- **Evidence:** n64-freeze-rows.cjs:28 refuses a flip when `git diff --name-only passSha HEAD -- passFiles` prints anything. Every flip
  rewrites both config copies (:62). NIGHT64 section 0 rule 9 sets passFiles from `git diff --name-only <first>^ <last>`, run on port/stage3.
  Section 4.2 picks "RED commits first, then the rest", so C4's range also holds U1's RED commits, and B3's range holds U2's.
- **What breaks, case 1:** B3's own fix writes the config ("a weak-rim colour in the table", slate B3 doneWhen 5; code-scout section 4 option
  1: the bossColours63 table at Design/unity-config.json:10018-10064). So B3's passFiles hold both config copies. If U2 is flipped first
  (one critic slot, run in turn), B3's flip is refused. If B3 goes first, U2 is refused, because U2's range includes B3's config commit.
- **What breaks, case 2:** a fix round on one slice voids its neighbour's PASS when their picks are interleaved.
- **What breaks, case 3:** a slate or log commit that lands inside a pick range (`git add -f Logs/n64/slate.json`) voids every later flip.
  These refusals land at 2:05-2:10, with the close at 2:15.
- **Fix:**
  - Build passFiles from the slice's own picked commits: the union of `git show --name-only --format= <sha>` over the shas its builder
    reported, after cherry-pick.
  - Leave out the two config copies. Have freeze-rows compare that row's own block instead (`design.<night>.<row>` with its numbers, at passSha
    against HEAD, `enabled` / `enabled_why` aside).
  - Or flip every PASS of a build in one `n64-freeze-rows.cjs a=.. b=..` call, since all checks run before the first write.
  - Plant one case in the forge: two rows, one shared config edit, a second flip. It must pass.

## 2. HIGH: the n64 tools are untracked, so the builders' worktrees will not have them
- **Evidence:** `git status --short Tools/` prints `?? Tools/night/n64-{clock,freeze-rows,gates,prune,reels,slate,slate-set,spot-pair}` and
  `?? Tools/night/forge64-make-n64.cjs`. Builders branch from the A1 commit (NIGHT64 section 1.6), and builder-brief.md rule 10 has them run
  `bash Tools/night/n64-gates.sh --no-editor` in their own worktree: "No such file". HANDOFF line 10 says the later commits include
  "FORGE64's tools", but none does yet.
- **Fix:** commit the nine files (and HANDOFF.md, see 15) in the forge's last commit. Add a verify line: `git ls-files Tools/night/n64-gates.sh`
  prints the path.

## 3. HIGH: the clock cannot hold four slices; B3 and U2 cannot reach a GATE before FREEZE
- **Measured costs (NIGHT63-LOG):** a build takes about 1 minute (b1 23:09:14-23:09:52). A reel side takes about 1.2-2 minutes (b5's six
  sides 00:55-01:02). A critic takes 3-5 minutes (P2 23:24 to 23:29, C2 01:11 to 01:15). The smoke takes 252 s. The flow gate is about 7-12
  minutes. Only one Player runs at a time, and spotter, critics and gg-proxy share one slot.
- **b1 (C4, U1):**
  - picks, the Editor RED, gates: about 15 minutes
  - C4: a157 n52-spire same-run plus verbs37 spire x1: about 5 minutes
  - U1: the per-boss duplicate count, row on and off, on boss63 x5: about 10 sides, 15 minutes
  - 2 SPOTs and 2 critics in the one slot: about 12 minutes
  - So flips land near 1:50, and rc1 (build, verbs37 x5, smoke: about 14 minutes) lands near 2:05, not 1:45.
- **b2 (B3, U2):** picks by 1:50 (while rc1 holds the Player), build about 2:05. Then B3's boss63 x5 same-run (about 8 minutes) and U2's
  verbs37 x5 row on and x5 row off (doneWhen 2 compares per level: 10 sides, about 20 minutes). After 2:15 the guard refuses every reel set
  not tagged n64-freeze (guard.cjs:252). So both are SHORT by construction, and the report would call them "attempted".
- **A1 is also not "~10 min":** a row script modelled on the 9.5 KB n63-a1-rows.cjs, the N64SharedTests, the Editor gates and the commit take
  20-25 minutes. So the builders start about 0:30, and 1:05 leaves about 20 minutes to code.
- **Fix, what to cut first:**
  - (1) U2: see finding 5; it is the costliest to evidence.
  - (2) Leftovers 2 and 4.
  - (3) B3, unless it moves into b1 (D builds C4 and B3 both by 1:05; C4 is about 20 lines), so one b1 reel set carries all three rows.
  - U1's counter prints both numbers in one same-run run per boss: "would-duplicate frames" (fight line up and hint text equal, whatever the
    row) and "duplicate frames shown". That makes 5 sides, not 10.
  - Re-time the table so rc1 is about 1:55, and state plainly in "Where we are" that the target is C4, U1 and L1.

## 4. HIGH: "red first" cannot be measured as written for C4, U1 and U2 (builders never run a Player)
- **C4:** its red is "a157 n52-spire's clear shots". a157 has never run on n52-spire: every `done: ... a157 | map` line in Logs/feel/*/summary47.txt
  is n57-kiln (31), n52-span (1) or n52-kiln (1), and no file `*L57-n52-spire*` exists. Spire does have frames52.entry on v2, v5 and v6, so it
  can run, but nobody has tried, and an untried scenario may come back RED or stall to the 420 s side cap.
- **U1:** its red is "boss63 x5 on n63-freeze (plain) ... counted by the builder's proof counter". Logs/feel/n63-freeze holds only vf, vi, vk,
  vs and vp, and the counter does not exist in rc0.
- **U2:** its red is a per-frame count over verbs37, but n63-freeze's verbs37 frames are pruned to every 50th (37 jpgs in vp/verbs37). No
  per-frame text log exists.
- **Builders:** builder-brief.md rules 2 and 10 say "measure your slice's red (read-only) before changing anything" and "You never run ... a Player".
- **Fix:**
  - The lead runs a red set on rc0 at 0:20-0:40, beside L1: `n64-reels.sh n64-red cr,n52-spire,a157,-skygrindClearShots60` and boss63 on the
    wyrm and the loom. The red frames' paths go to the builders.
  - Restate U1's and U2's red as "the row-off side of the first build that carries the counter".

## 5. HIGH: U2's premise does not match the code; its doneWhen lines contradict each other and text63
- **"DOOR" in vf/f1000 is a room title, not a door banner:** the door zones are named "DOOR" ("name":"DOOR" occurs 10 / 9 / 9 / 7 / 10 times in
  n52-foundry / iceline / n57-kiln / span / spire). Ui.Banner posts them to hud33's slot as MessageSlot33.Title (Ui.cs:610, UiHud33.cs:101).
  text61 already waits for that title (UiText61.cs:70). The stack happens because a title arriving over a card that is already up does not
  hide it (Text61.cs:107: "A line on the card stands while the slot hands it"). Extending the wait cannot clear vf/f1000.
- **"DOOR OPEN" in vp/f0400 is the same widget (hud33's slot), on channel Door = 3** (MessageSlot33.cs:19). It is a WANT, held for as long as
  the cleared room's door is open and the player is more than 6 m from it (UiHud33.cs:424-438; `hideWithin` 6, unity-config.json:1161). So
  waiting for it holds a lesson up to Text63WaitS = 10 s (UnityTuning.N63GT.cs:10), and then shows it LATE.
  - That breaks U2's own doneWhen 3 (kiln: "no '(LATE' line"), and in practice "no lesson lost": the wall-ride line would come after the wall.
- **There is also a separate DOOR OPEN cue widget** (UiCampaignCue24.cs:118), but it is not what the frames show.
- **Fix:** cut U2 tonight. Re-scope it for a later night as "a room title that arrives over a standing card takes its place, and the card
  resumes" (text63's queue, T63 re-run), with a doneWhen that allows a door WANT to outlast the cap.

## 6. HIGH: B3's doneWhen cannot be reached in about 40 minutes, and two of its gates are already met by NIGHT63's failed code
- **"Every boss's weak point reads hue 175-200" covers bosses no fix line touches:**
  - THE ICEBREAKER: 0 / 6 / 0 / 6 px changed on b11, and its weak point is never on screen (verdict-B3.md).
  - THE LOOM: median hue 170.9 on b11, below 175, and code-scout section 3 could not tell which renderer is its sphere.
- **No meter exists for two of the lines:** n63-b3-roles.py has no median-hue and no contrast output (grep finds no "median"). NIGHT63's critic
  used its own scratch scripts.
- **41,472 px and the SPOT are already met:** b11's wyrm stagger pair changed 94,790 vivid px with the toggle, so a spotter would name the
  head's colour change (green against lime) with no new work. Only the hue line tests tonight's fix.
- **Two of the three listed fixes probably do not move the SPOT pose:** rimHdr already undoes phase 1's filter (BossGrade54.cs:215-217), the
  stagger shot is forced at 14 s (phase 1), and the drift is the sulphur split toning. That leaves the table's weak-rim colour, a one-shot guess
  without a Player loop.
- **Fix:**
  - Restate before the night: "THE KILN WYRM's weak point median hue 175-200 on its stagger pair (row on), measured by
    `n63-b3-roles.py --hue <rect>`, a mode the builder writes with a planted case".
  - The Icebreaker and the Loom go to "not checked / next night".
  - Or cut B3 after U2.

## 7. HIGH: "verbs37 x5 unchanged" sits in every doneWhen, but GATE builds run verbs37 x1
- **The conflict:** section 4.3 runs the fast check (verbs37 x1) on GATE builds and x5 only on the RC builds after the flips. The critic gets
  "the latest verbs37 x5 with its build", which is rc0's: a different sha. So the critic must FAIL or UNSURE, or PASS on evidence from another
  build, which is what D-6 exists to stop.
- **No rule for a failed rc1:** nothing says what happens to the flips when rc1's verbs37 x5 or smoke fails.
- **Fix:** restate the line as "verbs37 x1 on the touched level on the GATE build; x5 on rc1". Add: "if rc1's x5 or smoke fails, the rows
  flipped since the last RC go back OFF in one commit, and the earlier RC stays the play build".

## 8. MEDIUM: the "HEAD's code equals rc1's" escape almost never holds, so D-8's hypothesis cannot be tested
- **Why it fails:** b2's picks (B3 and U2 with their rows OFF) land on port/stage3 after rc1 even when they go SHORT. So HEAD's Assets / Design
  differ from rc1's, and the freeze always re-runs build, x5 and smoke.
- **rc1 is also undefined:** nothing says rc1 must be a fresh build from HEAD after the flips with no `n46-build-rows` copy edit. b1's copy
  carries rows ON through its build copy, so it is not HEAD.
- **Fix:** define rc1 that way. Say plainly that the close re-runs the heavy steps unless nothing was picked after rc1. Change D-8's
  hypothesis to "a verified RC newer than rc0 exists from rc1 on".

## 9. MEDIUM: the GOAL's CAP escape is too loose, and a long stop makes it unreachable
- **Too loose:** "Also met once ... phase CAP and NIGHT64-REPORT.md is committed" is already true from the 1:30 draft. After a usage stop past
  3:00 (NIGHT63's lasted 2 h 18 min), the goal is met before section 7's minimum close: no u3-n64 tag, no HANDOFF section, the clock never
  stopped.
- **Unreachable after 5 hours:** past staleMin 300 (n64-clock.cjs DEF) the clock prints phase NONE, never CAP.
- **Fix:** make the clause "phase CAP (or NONE (stale)) and section 7's minimum close done: the report and HANDOFF committed, `git tag -l
  u3-n64` printing u3-n64, 'clock stopped' printed".

## 10. MEDIUM: the GOAL's "in order" clashes with section 9's parallel steps, and the parallel load is a known risk
- **Order:** the GOAL lists the flow gate before the freeze's verbs37 and smoke. Section 9(e) starts the flow gate "as soon as (b) is built",
  beside the reels, and the smoke follows the reels. A transcript-judged order can fail on that alone.
- **Load:** n46flow (5 levels x 10 seeds, dotnet) plus the gate's Editor compile beside a Player reel set and the focus-steal smoke is the
  close-batch-load case (memory: 3-5x slow beside other jobs). NIGHT63 ran them in series and was green (build 07:55, gates 08:17:31).
- **Fix:** run in series: build, reels, smoke, then `--flow` (about 25 minutes). Or write "in any order" for those three clauses.

## 11. MEDIUM: the same-run toggle treats the n64 rows as "unclaimed"
- **Evidence:** `-skygrindSameRun63 n64.goal64` parses (SameRun63Math.Row: NightKey 64 = NightOfName 64) and the override works through
  ViewOn (NightRows.cs:216). But KindOf returns Unclaimed, because FrameRows lists only exitFrame61, bossMotion63, bossTell63, bossColours63
  and colours63 (SameRun63Math.N63D.cs:27). Every proof line then prints "unclaimed (its view may read the row once: a 0 px pair proves
  nothing about it)", which a critic will cite.
- **Conflict risk:** both builders would edit that one array line, which gives a cherry-pick conflict (L-049).
- **Fix:** A1 adds "goal64" and "order64" to FrameRows (no test pins the array: grep finds no FrameRows in Assets/Tests). Each builder's RED
  test then pins that its view re-reads the row every frame.

## 12. MEDIUM: two dotnet runs at once during the bearings
- **Evidence:** the HANDOFF verify table runs `n64-gates.sh --no-editor` in the background (line 17). Bearings step 4 then starts `--flow-base`
  in the background. Both `dotnet run --project Tools/SimHarness` on one obj tree, and the DLL lock gives a false GATE FAIL. Rule 5's "never two
  gate runs at once" reaches only A1's run.
- **Fix:** start `--flow-base` only after the verify table's gates line prints, or drop that line, since A1's gates cover it.

## 13. MEDIUM: C4's "visible" overclaims, and one wrong call is easy to make
- **The red stays:** C4 leaves the red-gold window shafts (Route47View.cs:101-106) untouched. That is the orange bar over the door in
  vp/f0400, inside or touching the door's box. So "so red means danger again" is not what the frame will show.
- **Route47View.cs:58 uses one colour for both** the column and the shafts, so C4 must split them.
- **The table:** C4 must not call `Colours63.Route`, which returns its input while colours63 is off (Colours63.N63D.cs:91; the table is null
  at :137). `Colours63Math.From(cfg)` reads design.n63.colours63's table whatever its `enabled` (:50-75). Point the builder there.
- **Fix:** visible should read "the line down every spire exit turns pale; the red-gold bar over each door stays (GG's call, DECIDE64)". The
  report should say the same.

## 14. MEDIUM: rc1's smoke runs with agents alive
- **Evidence:** section 9(d) runs the smoke "with no agent alive" (memory player-smoke-focus-pause). rc1's smoke at about 1:45-2:05 runs with
  D, G and the review slot busy, and the prompt is silent on it.
- **Fix:** run rc1's smoke at a moment no agent is alive, and log it. Otherwise mark rc1 "smoke beside agents" and do not use it for the escape.

## 15. LOW: arrival checks and small contradictions
- HANDOFF's "tree" line expects only GG's files, but `git status` shows ` M HANDOFF.md`: commit it with the tools.
- builder-brief.md:44-45 says "**V** C4 ... **D** B3"; NIGHT64 section 3 says D does C4 then B3. DECISIONS D-3 allows four agents, rule 5 three.
  The brief stops G at 2:00; G's harvest is due 2:10. Slate C4 cites "DECIDE64 D-2", but the forge's items have no D-ids (DECIDE64.md:26).
  `--flow-base` is "~10 min" in NIGHT64, "~5 min" in the script. A-lean drops G's slices whole: keep U1 on D after C4 instead.

## 16. LOW: slate-set turns some short shas into numbers
- n64-slate-set.cjs:19 parses every value as JSON: an all-digit 8-hex sha (about 2 %) becomes a number, "0e123456" becomes 0; freeze-rows then
  throws in execFileSync or prints "no passSha". **Fix:** store the full 40-character sha, or keep `*Sha` fields as strings in slate-set.

## 17. LOW: L1's blind is only blind to GG
- The lead writes the coin flip's mapping, and `n63-m1-chop.cjs --pair n64-abx n64-aby` prints the halving per side (which build has eye63).
  **Fix:** keep the chop's direction out of the report and the play card; say in section 2 that the lead knows the mapping and GG does not.

## Checked and fine
- n46-build-rows.cjs takes any `design.<night>.<row>` with a boolean `enabled`. `--off64` is generic in SimHarness (Program.cs) and the Player
  (PlayerChecks.cs:76). In FREEZE the guard passes n64-freeze builds, reels and the smoke; n64-gates.sh is no "test run"; agents stop at 2:15.
- Route47View re-reads its row every frame (:50, :58); the clear shot goes through Shot29, so it has a same-run pair (FeelReel.N60.Lamp.cs:97).
  UiBossRule61.cs:120-150 writes b43Hint every frame: U1's write must come after it, or the rule line's release shows a 1-frame duplicate.
- HANDOFF lines OK: tag 0ec78408, no code since it, config copies identical, last clock "NONE (stopped)", slate "5 todo", 22.3 / 25.5 GB.
- Critic time (3-5 min) matches NIGHT63; the close's heavy steps fit 45 minutes in series.
