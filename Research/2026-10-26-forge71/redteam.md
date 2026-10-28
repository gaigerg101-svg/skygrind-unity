# FORGE71 red team: NIGHT71.md and its planning set (2026-10-26)

Read-only review. Inputs: NIGHT71.md; forge71's REQUEST, DECISIONS, slate-seed.json, builder-brief.md, spike.md, scout-rooms.md,
scout-feel.md, DEADCODE-candidates.md, GOAL.txt and postmortem-n70.md; CLAUDE.md; the tools the prompt names (read in full:
n71-close-check, n71-clock, n71-slate*, n71-freeze-rows, n71-runner, n71-reels, n71-prune, n71-gates, n41-build, n56-smoke-steal,
smoke.sh, n65-play-build, n46-build-rows, n70-red, n70-verbs-of; the headers and argument parsing of gen-slots, gen-n65-foundry,
n65-verbs and n65-air); .claude/hooks/guard.cjs; the config; and the Logs red folders. Nothing was run except reads.
Verified as written: the shas bfe1529f and dfa1ed865 and their room paths (both rows carry verb65 launch); n70-red.sh's argument
order and its `SLATE=` / `LEVEL_DIR=` / `ROOMS=` use (Logs/n67 and Logs/n68 slates hold v2, v6, kb2, kb3, kb3c); n65-play-build's
`--add-level` (a flag) and its row list; gen-slots `--room-file`; the 19-slice seed; the close-check's 33-case selftest exists; the
guard lets `n71-freeze` / `n71-play` steps through after 7:15; disk at 78 GB; a build is about 385 MB (Builds/n66-freeze); a pruned
reel tag about 214 MB (Logs/feel/n66-freeze).

Ranked by the run time each would waste. 15 findings.

---

## 1. The lead is the night's bottleneck, and its Player work is not scheduled (sizing, ambiguity). Cost: hours of B / F work left unproven
- **Where:** NIGHT71.md:133-142 (a row needs a RED / GREEN Player proof, a SPOT and a critic) and :74-78 (only the lead runs Players;
  slot 4 holds the critics and spotters after 1:35); slate-seed.json:66-110 (every B / F doneWhen is a Player measure); builder-brief.md:16.
- **What:** No proof build, no proof reel set and no critic or spotter batch has a time. Every visible row needs a build that holds
  its code, an OFF / ON reel pair, a blind SPOT and a critic: two slot-4 agents at 17-23 min each (L-109). B3, B4, F1 and F2 tune toward
  numbers that only a Player prints (stagger > 41,472 px, cyan px, waited steps, p99), so each needs two or more Player rounds. Builders
  "never wait", so each slice ends in `measuring` and the builder moves on. The lead's own load, summed from the prompt: bearings 15, PB0
  about 65, D's picks with an Editor compile and the touched tests (6-10 picks) 40-50, room picks 25, B / F picks 40, proof builds and
  about 40 reels 70-100, F1 bench 15, PB1 30-40, agent orchestration 35, STATE.md every 30 min 25, the close 60. That is about 7:50 of
  8:00. B2's proof alone is 12 reels (boss63 kiln plus verbs37 on five levels, both arms).
- **Likely result:** most rows SHORT at 7:00 with code built and no proof; the aim of 5 rows fails. The slate's 2x depth sits on the
  builders, but the limit is the lead.
- **Fix:** add a "Proof slots" paragraph to section 6. **P1 at about 2:15** (build `n71-p1`); **P2 = PB1's freeze at about 3:45**;
  **P3 at about 5:30** (`n71-p3`). Each slot picks every ready row and runs every OFF / ON pair in ONE `n71-reels.sh` set. Then ONE
  blind spotter and ONE critic each read that slot's files together (one verdict file per row: `Logs/n71/verdict-<id>.md`). State a
  cutoff: a row whose code is not picked by 5:15 is SHORT with its numbers. Tell B and F to put their tuning numbers in config so the
  lead can try two values in one set (`-skygrindSameRun63` pairs). Cut "STATE.md every 30 minutes" to "at every landing".

## 2. No shared commit for `design.n71` and FrameRows, and no pick method for code. Cost: 20-60 lead minutes of conflicts, and a row can be lost
- **Where:** builder-brief.md:12-13, :49-51; NIGHT71.md:117 (the only pick named is `git checkout n71-R -- <file>`); no `design.n71`
  block exists in Design/unity-config.json (checked); SameRun63Math.N63D.cs:28 is one line, and its own comment (:26-27) cites
  FORGE64 redteam.md 11 ("so no two builders edit this line").
- **What:**
  - B (voice71, words71, wyrm71, cyan71, room71) and F (press71, kicks71) each create `design.n71` in both config copies.
  - Both append view rows to the same FrameRows line.
  - D deletes four row keys from the same two files.
  - A whole-file `git checkout <branch> -- Design/unity-config.json` takes one builder's copy and drops the others' edits.
  - A cherry-pick conflicts on the second `"n71": {` insert, twice per pick (both copies). Path checkout also cannot carry D's
    file deletions (Shift46View.cs, UiTeach42.cs and their .meta).
  - The prompt never says how B / F / D commits reach the main tree.
- **Fix:** a shared commit before the worktrees (section 4.3, before `git worktree add`):
  - `design.n71` with `_why`, plus a placeholder per planned row in both copies:
    `{"enabled": false, "enabled_why": "OFF until GG's keep (NIGHT71)", "_why": "..."}`;
  - FrameRows gains voice71, wyrm71, cyan71 and kicks71;
  - branch every worktree from that sha.
  - State the pick: `git cherry-pick <sha>` per builder commit, in order, under the runner hold. On a config conflict, take HEAD's
    copies and re-apply the builder's row block, as L-049 / L-065 say. Rooms keep the path checkout.

## 3. The close holds 30-46 minutes of serial heavy steps after `--start`, plus the papers, in one hour. Cost: a clock stop past 8:00 (MINIMUM at best), or PB2 lost
- **Where:** NIGHT71.md:230-266.
- **What:**
  - (a) up to 10 min, then (b) the picks, then `--start` (7:10-7:20 in practice).
  - (c) a new freeze (2-10 min); five verbs37 sides (63-121 s each, n71-reels.sh:19); the steal smoke (about 4 min); PB2's four
    sides; the play smoke (233 s).
  - (d) the gates with `--flow` (8-10 min, scout-rooms.md:34).
  - Then (f) the report, DECIDE71, HANDOFF, the harvest and a night-context loop "until RESULT: ... landed" (THREADS is at its
    60 / 60 cap, CORE.md); then (g) and (h).
  - After 8:00 the guard refuses every build and reel (guard.cjs:253).
  - Also: (e) runs `--pre` before the papers exist, so it prints FAIL files / play by design. The prompt does not say the FAIL is
    expected; a careful lead may stop and "fix" it.
- **Fix:** a code freeze at 6:00:
  - the last code pick, then `n41-build.sh n71-freeze`;
  - its five verbs37 sides, `n56-smoke-steal.sh n71-freeze` and `n71-gates.sh --no-editor --flow > Logs/n71/gates-flow-close.txt`,
    all by about 6:50 (the close-check reads only the files);
  - after 6:00, only room files and the play build's config change;
  - at 7:00: `--start`, then PB2 from that freeze (seconds), its four sides and the smoke (about 12 min), then the papers. That
    leaves the clock stop at 7:30-7:45.
  - Say in (e): "`--pre` here is only for the ROW line; FAIL files / play is expected until (g)."

## 4. D1 deletes rows the never-delete list protects, and D cannot see the DECIDE pages that name them. Cost: 30-60 of D's 90 minutes, then reverts
- **Where:** NIGHT71.md:145 ("anything a thread, DECIDE page or HANDOFF names") and :150 (`-skygrind*` flags); builder-brief.md:64;
  slate-seed.json:12; DEADCODE-candidates.md:230-240 and :291-292.
- **What:**
  - Three of D1's four rows are named on DECIDE pages: drone49 (DECIDE49 D-2), shift46 (DECIDE46 N-2) and teach42 (DECIDE42 D-11).
  - Deleting shift46 also removes the flag `-skygrindShift46` (Shift46View.ReelFlag).
  - REQUEST call 4 says "no open thread or DECIDE **question**"; NIGHT71 says "DECIDE **page**". The two differ.
  - DECIDE46.md and DECIDE49.md (and DECIDE64.md, which B4 cites) are **untracked** (checked). They are absent from D's worktree, and
    `git show port/stage3:<path>` (the brief's only read method, builder-brief.md:14) cannot read them.
  - So D's re-grep at its head finds nothing and D deletes. The cleanup critic, working in the main tree, finds the names and FAILs D.
- **Fix:** write the reading into section 5.3: "a DECIDE item that recorded a decided OFF verdict is not an open question; it may go".
  Also allow removing the reel-only flag `-skygrindShift46`, or move shift46 to CANDIDATE. Commit the untracked DECIDE42-64 pages
  before t0, or tell builders to read them by absolute path.

## 5. R's new rooms trip n70-red's verb check, and no shipped red exists for them. Cost: 10-15 min per room the first time, on R4-R6 (400 of R's 550 slate minutes)
- **Where:** NIGHT71.md:108; builder-brief.md:38-39; n70-verbs-of.cjs:31-32; gen-slots.cjs:125 (the export tells R to give the row
  verb65); Logs/n68/red-iceline holds only g7 and g8; Logs/n68/red-kiln and Logs/n70/red-kiln hold no kb2c; red-spire holds no v5.
- **What:**
  - kb2c, v5 and g1-g6 are in no slate (Logs/n67, n68 and n70 checked).
  - A rebuilt row with verb65 and no slate entry is MISMATCH, so n70-red prints "REFUSED" and exits 1.
  - Logs/n71/slate.json cannot serve as `SLATE=`: its ids are R1-R7, not room ids.
  - G3's "the shipped red" does not exist for these rooms, and the prompt does not say how to make one.
- **Fix:** add to the brief's R section:
  - "write `Research/2026-10-26-wave71/R/slate-r.json` = `{"slices":[{"id":"kb2c","level":"kiln","verb":"<verb>"}]}` and pass
    `SLATE=` that file";
  - "make the shipped red first: `gen-slots --level <L> --slots=old --out=<ev>/base`, then
    `LEVEL_DIR=<ev>/base ROOMS=<id> bash Tools/night/n70-red.sh 6 n67-<L> <ev>/red` (no verb65 on a base row reads `none`)".

## 6. The slate and the flip tool refuse the shapes the prompt produces. Cost: 10-30 min at the close (slate MALFORMED blocks `--pre` / `--tag`)
- **Where:** slate-seed.json:24 (D2 writes `Logs/n71/verdict-D.md`, first line `D: PASS`) against n71-slate.cjs:27 (the first line
  must be `<id>: PASS`, i.e. `D1: PASS` / `D2: PASS`); slate-seed.json:2 (rooms are PASS by "the batched critic's reading of the GUARD
  files") against NIGHT71.md:105-117 (no room critic named); n71-freeze-rows.cjs:66 (needs `passSha` and `passFiles`, never mentioned)
  and :100 / :103 (needs an existing `enabled_why` key, while builder-brief.md:12 / :49 say `_why`).
- **What:**
  - A lead that marks D1 / D2 PASS on one verdict file, or a room in GG's build PASS without a verdict, gets slate MALFORMED (exit 1).
    That fails `slate`, and `--tag` refuses.
  - The voice71 / words71 flip throws "the text edit and the parsed edit disagree".
  - The alternative, marking every delivered room SHORT, adds about 10 SHORT items to night-context's THREADS / HANDOFF loop.
- **Fix:**
  - The D critic writes verdict-D1.md and verdict-D2.md.
  - Section 5.1 gains "one batched room critic per play build (slot 4) writes verdict-R<n>.md from the guard files".
  - The brief's row shape includes `enabled_why` (finding 2's placeholder).
  - Section 5.3 says "set `passSha` / `passFiles` with n71-slate-set.cjs before n71-freeze-rows".

## 7. PB1 builds over the freeze that GG's PB0 play build hard-links; `--update` after new code ships a row with no code; "a new tag" contradicts the check. Cost: a broken PB0 / PB1 for GG, or 15-20 min rebuilding
- **Where:** NIGHT71.md:191-193 (PB1: "A new n71-freeze from HEAD", with no move of the old freeze; "`--update` (or a fresh
  n71-play ...)"); :239 ("a new tag if n71-freeze exists: move the old one"); n65-play-build.sh:3-6, :10 (hard links; `--update`
  keeps <dst>'s other files); n41-build.sh:13 (no check for an existing folder).
- **What:**
  - Builds/n71-play is `cp -al` of Builds/n71-freeze. A Unity build into the existing Builds/n71-freeze can write through the shared
    inodes into GG's play build, or fail on a file a running Player holds.
  - `--update` after new code keeps PB0's binaries and writes HEAD's config, so a new n71 row reads ON with its code absent.
  - "A new tag" would move the reels away from `Logs/feel/n71-freeze/` (what the `player` check reads) and outside the guard's
    closeTags after 7:15.
- **Fix:** PB1 step 1: `mv Builds/n71-freeze Builds/n71-freeze-pb0; mv Builds/n71-play Builds/n71-play-pb0`, then the build. Say
  "`--update` only when no code was picked since the freeze the play build came from". In (c): "the same tag n71-freeze (move the old
  folder to n71-freeze-pb1)".

## 8. Stops: foreground timeouts orphan Players and prune loops; the Editor stalls unfocused; a few early-end edges. Cost: idle stretches, and a `running` FAIL at the final check
- **Where:** NIGHT71.md:159, :177-180, :240-245 (builds, reel sets and smokes with no word on how to run them); the Bash tool's
  10-minute cap against n71-reels.sh's 420 s per side (5 sides can run 35 min); n71-close-check.cjs:216-218 (a SKYGRIND.exe under
  Builds\n71-* or a `bash Tools/night/...` started since t0 fails `running`); NIGHT71.md:90 (bearings check only `status`).
- **What:**
  - A foreground reel set past the cap is killed. Its Player and its `n71-prune.sh --loop` survive (the loop exits only on a new
    "== all done"; TaskStop orphans, MEMORY).
  - The play smoke run directly takes 233 s, past the default 2-minute Bash timeout.
  - The Editor stalls when unfocused (CLAUDE.md, Workflow; MEMORY autotick), so `n41-build.sh` / `wait-build.sh` can hang PB0.
  - Smaller:
    - gen-n65-foundry uses `dotnet run --project Tools/SimHarness` (gen-n65-foundry.cjs:372). It rebuilds the DLL the runner's
      jobs execute (n70-red.sh:5-6: "never beside another dotnet build in this tree").
    - "LIST EXHAUSTED: go to the close at once" (:211) meets `--start`'s 7:00 refusal.
- **Fix:**
  - Section 3 rule 2: "run every build, reel set and smoke with run_in_background and wait for its notification; after each, list
    SKYGRIND.exe and `n71-prune` processes and close leftovers by PID".
  - Bearings: `editor_focus` plus set_autotick once (MEMORY).
  - PB0 step 2: "after 001-flowbase.done exists".
  - Exhausted list before 7:00: "start the code-freeze steps (finding 3); `--start` waits for 7:00".

## 9. PB0 compares against the wrong or stale reds. Cost: wrong in / out calls on up to 11 rooms, and a re-run
- **Where:** NIGHT71.md:172-173.
- **What:**
  - "level 1: Logs/n66/r2" is NIGHT66's round 2 of the **rebuilt** rooms. The shipped red is Logs/n66/red (r2/table.md shows it
    only in parentheses).
  - That red was made 2026-10-24 01:03 (sha 109d9dcd). The walk bot changed after it: NIGHT68 I3 c2d5fdf2a ("reads lava by height")
    and 0b5124b6e. L-150 / L-152 say to re-make reds after an instrument change.
  - v6's red is not in Logs/n70/red-spire (v2 only). It is Logs/n67/red-spire/v6, from the n67 instrument.
- **Fix:** add the reds to PB0's runner job, with the same tool on the same day:
  - `gen-n65-foundry.cjs --run --slots=old --out=Logs/n71/play/foundry-red`, then
    `LEVEL_DIR=... bash Tools/night/n70-red.sh 6 n65-foundry Logs/n71/play/foundry/red-base`;
  - the same with `gen-slots --slots=old` for kiln and spire (with the same SLATE / ROOMS);
  - about 6-8 runner minutes.

## 10. PB0 runs no rule-1 check, and the walk's falls are not guarded. Cost: rooms pulled at PB1 after GG may already have them
- **Where:** NIGHT71.md:110 (pads, ride faces, footholds and air65 "at least the shipped count") against :168-173 (only n70-red
  runs); scout-rooms.md:15-24 (G1-G7 name no air65 tool); spike.md:38-40 (kb3c falls 8 against the red's 0); NIGHT71.md:127 ("never
  lava or a pit on the walk").
- **What:**
  - air65 is `Tools/night/n65-air.cjs <plan> <room>`, named nowhere in the prompt; R2's whole slice depends on it.
  - PB0 ships kb3 d2, v2 t16, kb2, kb3c, v6 and level 1's six without the rule-1 count.
  - kb3c's 8 walk falls pass G3 (deaths only).
- **Fix:** add to section 5.1 and the PB0 job: `node Tools/night/n65-air.cjs <LEVEL_DIR>/<L>.plan.json <id>` against the same on the
  red's plan, plus the pads.txt count against the red's. Add "falls at most the red's + 1" as a guard, or print it as a label and say
  so on the card.

## 11. F1's bench and the mid-night smokes have no quiet window. Cost: polluted numbers or a SHORT F1; smoke reruns
- **Where:** NIGHT71.md:58 (every slot busy all night) against slate-seed.json:95 (median of 3, power plan polled) and MEMORY ("never
  beside the Editor suite"; "Player smoke focus pause: run it with no agents active"); n70-red.sh runs 6 parallel harness jobs per
  call, in R's tree and in the runner at once.
- **Fix:** one quiet slot of about 20 minutes right after PB1. Tell builders by SendMessage "code and docs only, no dotnet, 3:50-4:10",
  and run F1's two bench arms (3 each) and PB1's smoke there. Log the power plan before and after.

## 12. The brief's compile line does not exist. Cost: about 5 min per builder, four builders
- **Where:** builder-brief.md:19: "`bash Tools/dn-check-wt.sh` if it exists, else the lead's compile line". The script is
  `Tools/night/dn-check-wt.sh <scratchDir>`, and the argument is required. No "lead's compile line" is written anywhere.
- **Fix:** "`bash Tools/night/dn-check-wt.sh <your scratchpad>/dn` (0 errors on the four projects)".

## 13. The voice71 / words71 repo flip contradicts the prompt's own rule. Cost: a contested ON in main, and a config commit that must precede `--start`
- **Where:** NIGHT71.md:141-142 ("rows that restore a CLAUDE.md rule (voice71, words71)") against :36 ("keeps every new row OFF
  until GG's keep") and DECISIONS.md:21-22 ("colour rule 8, one instruction at a time").
- **What:**
  - CLAUDE.md rule 8 covers colours, 16 px text and fonts. "One instruction at a time" is GG's taste (T-155), not a CLAUDE.md rule.
  - The one colour row tonight (cyan71) is GG's open DECIDE64 call. The shipped bossColours63 reads weak-point cyan as rule 8 itself
    ("the thing the player acts on", BossColours63Math.N63D.cs:4), so not even cyan71 is a clear restoration.
  - A flip after a late critic also lands after `--start` and fails `frozen`.
- **Fix:** drop the repo flips ("Bots guard, I judge"), or name the rule each row restores and require the flip before 6:00.

## 14. GG's card names the wrong side-by-side switch. Cost: a misleading A / B for GG
- **Where:** NIGHT71.md:185 and scout-rooms.md:50 ("`-skygrindOff67` boots the shipped levels").
- **What:** an off switch turns OFF "every row of night NN and later" (NightRows.cs:12, :158, :274). `-skygrindOff67` therefore leaves
  n65.foundry65 ON, so level 1 stays rebuilt, and it turns off every n71 row at the same time. blast48 / guns48 (night 48) stay ON.
- **Fix:** "`-skygrindOff65` boots every shipped level (it also turns off the night's n71 rows); `-skygrindNo<Row>71` turns off one row".

## 15. The weakest item: F2 press71 is the slice the owner is most likely to reverse
- **Where:** slate-seed.json:96-101; DECISIONS.md:49-51; CLAUDE.md rule 4.
- **Why:**
  - It ends the hit freeze on any fresh press. Because a latched press is consumed at the first step after the freeze either way
    (scout-feel.md:63), the only change is a shorter hitstop. A tapping player (KESTREL) rarely sees the full freeze again.
  - GG's standing complaint is "kills not crunchy" (NIGHT16), and F3 in the same track makes kicks bigger for crunch.
  - CLAUDE.md rule 4 lists hitstop as part of the web's loop.
  - DECISIONS D-7 calls it a Core sim row; the slate calls it Game-side with Core untouched. The two disagree.
  - It is F's largest slice (90 min), and its proof needs a reel "that presses inside freezes" that does not exist yet.
- **Fix:** demote F2 below F3 and L2, and cap it at 45 min. Ship it only as a play-build switch, labelled "shorter freeze on a tap",
  with F4's check first (is a tap dropped at all?). The runner-up for reversal is cyan71: it inverts bossColours63's deliberate "weak
  point = the player's cyan" (BossColours63Math.N63D.cs:4, :44; shipped ON at w64), so its card should show both looks.
