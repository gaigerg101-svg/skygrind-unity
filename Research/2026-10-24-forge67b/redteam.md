# FORGE67b red team (2026-10-24 ~09:30-10:30, read only; tree 0c4fc632 + the forge's untracked NIGHT67.md / DECIDE67.md). Most damaging first.

Each finding: the risk, the evidence (file:line), the damage, the fix (the text to add or change).

**Read in full:** NIGHT67.md, DECIDE67.md, this folder's history.md, levels-data.md, research.md, code-scout.md, DECISIONS.md,
slate-seed.json, builder-brief.md and GOAL.txt; Tools/night/n67-close-check.cjs, n67-round.sh, n67-red.sh, n67-round-verbs.sh,
n67-gate-pack.cjs / .sh, n67-deco.cjs, n67-table.cjs, n65-verbs.sh, n65-slot-set.cjs, n65-play-build.sh, n67-gates.sh, n67-slate.cjs,
n67-slate-set.cjs, n67-freeze-rows.cjs, gen-n65-foundry.cjs (header, KNOWN, LOCKED); FORGE67's redteam.md; NIGHT66-REPORT.md;
NIGHT66-LOG.md; HANDOFF.md (top); CLAUDE.md.

**Also read, to check the verbs on levels 2-5:**
- Harness.N65.VerbPilot.cs:1-140 and Harness.N65.Verb.cs (the dispatcher :36-56, the ride pilot :392-510, V65Filter :518-578).
- Combat.cs (the fire dispatch :905-930, SwingGauntlet :1282-1336, StepPunch :1338-1350), Rules30.cs:87 and Lockout29.cs:31.
- Launch36.cs:1-60, Unlocks26.cs, SurvivalSim.Weapons31.cs:132, SurvivalSim.N40.cs:109-125 and NightRows.cs:262-340.
- UnityTuning.N53.cs:20-34.
- The config rows design.n30.rules, design.n40.iceline40 / kiln40 (startSet, awards) and design.n31.unlocks.

**Ran (no dotnet, build, Editor or harness):**
- git reads.
- `df -BG /c`: **13 GB free** (14 at the forge).
- Commit free: 19.5 of 98.6 GB. Unity PIDs 49588 / 51936 are still up at 6.8 / 6.9 GB.
- `node Tools/night/n67-slate.cjs <seed>`: 12 todo. `n67-close-check.cjs --selftest`: `SELFTEST PASS (27 cases)`.
- Two scratch scripts outside the repo, in this session's scratchpad (`rt67b/`):
  - `lintprobe.cjs`: rough lint numbers over the 25 shipped fight rooms, from the plan files.
  - `t1slip.cjs`: planted closes in temp git repos (deleted after), run through the repo's n67-close-check.cjs with no `--rows`, as
    GOAL.txt runs it.

Grounding: the repo only. Drive, Gmail, Calendar and Linear were not searched: this game's material lives in the repo, and the forge
found those sessions expired (DECISIONS.md:6-7). No file content asked for any action.

---

1. **The close can never pass, and never tag, if any n67 slots file is missing. That happens if T1 slips, and also in any level where no
   room PASSes. The /goal then has no reachable end.**
   - **Risk:** with no arguments, as GOAL.txt runs it, the close-check requires four rows and four slot files (n67-close-check.cjs:57-58):
     level 1's, plus n67.iceline67, kiln67 and spire67, each with `Design/levels/n67/<level>/slots67.json`.
     - `frozen` lists every slot file, and a file with no commit counts as "committed in the close" (:105-108).
     - `row` prints `unreadable` for a missing row or a missing slots file (:140-153).
     - Both checks are in PRE (:157). `--tag --minimum` forgives only `player` and `gates` (:166).
   - **Evidence:**
     - Nothing in the plan creates the slots files:
       - The shared commit's list (NIGHT67.md:85-87; slate-seed.json:20) has the rows, the placeholder levels, the map list, look49 and
         Polish53. It has no slots67.json.
       - n65-slot-set.cjs builds the file in memory and writes it only at the first `set` (n65-slot-set.cjs:26-37).
     - So a level with no PASS never gets its file. Neither does any level when T1 slips.
     - My planted closes (t1slip.cjs), with every other line correct:
       - The rows and slots files present: `CLOSE-CHECK n67: PASS`.
       - The rows present, no slots file ever written: `FAIL frozen ... Design/levels/n67/iceline/slots67.json, .../kiln/...,
         .../spire/...` and `FAIL row ... unreadable: no Design/levels/n67/iceline/slots67.json`, then `CLOSE-CHECK n67: FAIL frozen, row`.
       - No shared commit at all: the same two FAILs, with `unreadable: n67.iceline67: Cannot read properties of undefined`.
       - The minimum close with `--tag --minimum` at 7:35: `PRE-CHECK n67: FAIL frozen, row`, and no tag.
     - The selftest never runs the four-row default: its `run()` passes `--rows n65.foundry65=...` unless a case gives its own (:260).
   - **Damage:**
     - With no tag, GOAL.txt's FAIL exit (it needs `u3-n67 -> <sha>`) is closed too, and the judge loops. This is FORGE67 red-team finding
       7's trap, reached by a likely outcome: one of three levels with no PASS.
     - The slots files cannot be committed in the close either. `frozen` needs them "last committed before the close's start".
   - **Fix:**
     - NIGHT67.md section 1 step 7 and slate T1 line 2, add: "n67-a1.cjs also writes `Design/levels/n67/<level>/slots67.json` for each
       level, every slot `old` (n65-slot-set.cjs's readSlots shape), committed in the shared commit".
     - n67-close-check.cjs: the default rows become level 1's row plus every `design.n67.*` row in the config that has a levelTo. A row
       missing from the config prints `ROW: <row> absent (no shared commit)` and is not required. A row present without its slots file
       still FAILs.
     - Selftest cases:
       - no n67 rows -> PASS;
       - rows without slots files -> `FAIL row, frozen`;
       - the four-row default with no `--rows`, as GOAL.txt runs it -> PASS.
     - Section 9: "at 6:30, `node Tools/night/n67-close-check.cjs --pre` once (its row and frozen lines must PASS; the rest may not yet)".

2. **Three of level 2's four rooms (g1, g3, g4) cannot pass M3 by any verb, and their M2 can only be the ride. The brief's verb list is
   wrong for every level tonight.**
   - **Risk:** level 2 starts with the gauntlet alone (`design.n40.iceline40.startSet ["gauntlet"]`), and the KESTREL is awarded in g6
     (`iceline40.awards [{room g6, weapon revolver}]`). In g1-g5 the gauntlet is in hand. By reading (not run):
     - **launch and parry:** the pilot's episodes press `punch` (VerbPilot.cs:6-14). StepPunch drops a punch with the gauntlet in hand
       (Combat.cs:1343-1348; Rules30.cs:87 `MeleeBlocked(gauntletInHand) => gauntletInHand || ...`), and design.n30.rules is enabled.
     - **The refused launch leaks:** with the gauntlet, `fire` swings it (Combat.cs:924-927). The swing launches at speed
       (Combat.cs:1321). V65Filter refuses only `c.punch` (Verb.cs:573-578).
     - **ride --shoot (M3):** it counts a kill from the wall only after a `fire` event (Verb.cs:453). The gauntlet's swing emits
       `meleeSwing` (Combat.cs:1335); only the guns emit `fire`. So with the gauntlet in hand it reads 0 kills from the wall: RED.
     - **pierce:** needs the KESTREL in hand (VerbPilot.cs:8-9).
     - **slide:** n65verb has no slide pilot (`launch|pierce|parry|ride|skip`, Verb.cs:46). M3's "slide: the slide line's kill"
       (slate-seed.json:8) has no tool. None of the eight rooms has a tunnel (plan read).
     - **Parry needs a spitter's glob** (VerbPilot.cs:14). g1, g3, g4 and v6 have no spitter in any wave (plan read).
   - **The brief's verb list is wrong:**
     - builder-brief.md:26-27 says "ride, slide, launch, parry; pierce only on level 5, which owns the revolver".
     - Pierce works in g8 (after g6) and in kb3 (kiln40 awards the revolver in kb2b).
     - Slide cannot get an M3.
     - kb2 is the THUNDERCLAP's alone (`kiln40.startSet ["shotgun"]`), so it has no pierce.
     - kb3 is handed the gauntlet on entry (`kiln40.awards kb3 gauntlet`). Whether that puts it in hand, and so blocks launch, parry and
       ride --shoot there too, was not checked.
   - **Damage:**
     - R's g1, g3 and g4 can never meet the ROOM list (M3 has no GREEN path), so at most one of R's four rooms (g8) can GATE.
     - "Four or more PASS on two levels" then needs S's rooms.
     - R's cards, written by the brief, will pick launch or parry for gauntlet rooms. Their reds and rounds will read "exercised 0/10" or
       leak, which wastes R's night.
   - **Fix:**
     - Bearings, before the cards, a 10-minute probe with the day-one tools on the shipped level. Run
       `bash Tools/night/n65-verbs.sh ride Design/levels/n52-iceline.json g3 --shoot` and
       `bash Tools/night/n65-verbs.sh skip Design/levels/n52-iceline.json g3 --seeds 3 --plant-verb65 g3:launch`, then the same on kb3.
       Save them to Logs/n67/verb-probe.txt.
     - If they read as above, re-scope R to the KESTREL half of level 2: g8 and **g7 THE MIX**. g7 is BIG, with 143 falls in 10 walks
       (levels-data.md:82), spitters and the KESTREL. g1, g3 and g4 move to "not tonight: their gauntlet verbs have no pilot".
     - Never award the KESTREL early to make a room measurable. That is GG's level-2 rule (iceline40's _why).
     - The brief's verb line becomes: "the verb the pilot performs in THIS room's loadout. Ride (a gun in hand for M3), launch and parry
       (not with the gauntlet in hand; parry needs a spitter), pierce (the KESTREL in hand: level 2 from g6, level 3 from kb2b, level 5).
       Never slide (no M3 tool)".
     - A gauntlet branch for the pilot and the refusal (swing for launch and parry, `meleeSwing` hits from the wall, a refused swing at
       speed) is harness code that needs a critic. T1b took three critic rounds. It is a slice for another night.

3. **The area lint as specified would pass every room on some rules, push every room into one enemy template on others, is not static,
   and can be gamed by deletion and by a hand-drawn line.**
   - **Evidence:** my probe (lintprobe.cjs; rough: planes from box tops, ranged = spitter + drone) over the 25 shipped rooms. The rules are
     from slate-seed.json:6.
     - **P1 size (long axis <= 32 m):** 25 / 25 pass. It never discriminates, so GG's "a lot smaller" is driven by nothing.
     - **P1 (the last wave's ranged on a plane >= 3.5 m):** 15 / 25 rooms have no ranged enemy in the last wave, 5 of the 8 targets among
       them (g1, g3, kb3, v2, v6). The rule is undefined there: a vacuous PASS, or a FAIL that forces ranged enemies into the last wave.
     - **P11 (ranged 0.2-0.6 of the room):** 7 / 25 pass (a5, g7, g8, kb2, kb3c, p1, p5). Of the targets, only g8 (0.20) and kb2 (0.22)
       pass. g1 TANK ALONE and g3 THE SWARM are 0.00, g4 is 1.00 and v6 0.12. v2's high nodes are all at 2.4 m, against the 2.5 m bar.
       The rule rewrites each room's enemy mix toward one template (ranged up high, everywhere). That runs against research P3 ("one
       spatial law per room, different from its neighbours", research.md:99-106). It also turns M1 into a waves edit, which the brief's
       rule 4 says is not the work.
     - **P17 (ride walls >= 4 x 8 m):** fails only a8, g5 and v5. All 8 targets already pass.
     - **P4 is not static.** "Does a job by the decoration checker" means test (a), the skip run's touch table. M1's command has no
       `--touches` (slate-seed.json:6; NIGHT67.md:113).
       - Test (b) counts spitters as the only ranged kind (n67-deco.cjs:113). In g1, g3, g4 and v6, no tall solid can ever pass by sight.
       - The checker judges only pieces added or moved since `--base`, and REFUSES a base where the file did not exist
         (n67-deco.cjs:167-170, :192-194). "Every solid" (P4) and "every piece" (M4) are not what it can do.
       - The 25-room baseline has no touch tables (25 skip runs is about 75 min of dotnet).
     - **P5 uses a verb line the builder draws** (`line65`; no shipped room has one, levels-data.md:47). So P5 cannot be computed for the
       baseline. A builder passes it by routing line65 behind a wall, or within 6 m of a spawn node.
     - **P9 (two climbs per plane, tops >= 8 m apart):**
       - It cannot hold on a pillar-top ledge 1-4 m across. That is the very piece P4 asks pillars to become ("a ledge on a route").
       - A flat room (g1) passes it vacuously.
       - The nav graph it needs is "not checked" (research.md:349).
     - **M1's command reads no round folder** (`n67-area.cjs --level <L> <id>`, NIGHT67.md:113). Any rule that reads the level JSON would
       read the tracked placeholder, which is the shipped level. NIGHT66's iso pictures had this bug (NIGHT66-REPORT.md section 4).
   - **Damage:**
     - M1 can be met by deleting pillars, flattening planes, editing waves and drawing line65, with no new piece that has a job. That is
       history.md corner 3 ("cuts-only re-cuts"), now as the MAIN.
     - Or a literal T2 critic FAILs T2 on P4 and P5, and no room's M1 counts.
   - **Fix:**
     - slate T2 line 1 gains: "the lint reads the round's folder (`--dir Logs/n67/r<n>-<L>`) and its touch table (`--touches`), and prints
       both in its header".
     - Ranged = spitter + drone in the lint and in n67-deco.cjs alike.
     - A rule with nothing to judge (no ranged in the last wave, no plane) prints N/A. It never counts as a PASS toward 6 / 6.
     - P5's verb line is the verb-aware pilot's measured path (a new print in the touch table), not line65. Until that print exists, P5 is
       a GUARD.
     - P9 counts only planes 6 m or more across.
     - P11's share applies only to archetypes that put the ranged up high (TOWER, GALLERY, SPLIT LEVEL: research.md section 2), and
       research P3 (the archetype's signature differs from both neighbours') joins M1.
     - P1(c) is replaced by a size rule GG's words need: "the fight floor (nav floor at y <= 1 m, outside pits and fills) <= 450 m2, or
       25 % under the shipped room's". Or the target says plainly "no room is smaller tonight" (finding 7).
     - M4 gains: "and at least one piece ADDED with a job by (a) or (c)". Deletion alone never passes a round.
     - A rule every shipped room passes is dropped from M1's count, not just named (slate-seed.json:30).

4. **The schedule runs straight through the stop it predicts. Under NIGHT66's load the night has two work blocks of about 2:20, and
   nothing is set to run in the gap.**
   - **Evidence:**
     - NIGHT66-LOG.md:50 ("resets 12:40am"), :54 (resumed 00:41) and :260 (stopped 03:01, "resets 5:40am"). The windows are 5 hours, and
       a full window gave 2:20 of work under three builders, one or two critics and the lead.
     - NIGHT67 keeps that load until 2:15: R, S, T, the review slot and the lead (rule 5, NIGHT67.md:47-48).
     - With t0 right after a reset (DECIDE67's ask), the expected shape is: work 0:00-~2:20; stopped ~2:20-5:00; work 5:00-~7:20; stopped
       from ~7:20. This is inferred, not checked.
     - Rule 11 says "2:30-3:30" (NIGHT67.md:61-62), but section 5's table still puts T1's critic at 1:45, the reds at 1:30-2:30, T2 at
       2:15, r1 at 2:30, r2 at 3:30, r3 at 4:30 and r4 at 5:30 (NIGHT67.md:129-136). Background shells outlive a stop (FORGE67 red team 4),
       yet no round is scheduled to start before it.
   - **Damage:**
     - r1 starts at the resume (~5:05-5:15), after the bars and T2's critic. r2 ends ~6:40.
     - The GATE packs, spotters and level critics land 6:45-7:00, inside the builders' stop and the close. Likely 0-2 GATEs.
     - The close (7:00-8:00) meets the second window's end (~7:20), which pushes the night to the after-8:00 minimum close.
   - **Fix:** section 5, replaced by two blocks:
     - **Block 1 (0:00-2:10):**
       - The bearings, then **the reds at 0:10 on the shipped level ids**, with every feasible verb planted per room (finding 2's probe
         table). A 6-job pool in the main tree, about 15 min. gen-slots --check later proves the assembled level is the shipped one,
         and the log says so.
       - The cards and the card critic.
       - T1a's export by ~0:50 (finding 5). Builders on room files from ~1:00.
       - The bars at ~1:15, when the critic has fixed each verb.
     - **At ~2:05, before the likely stop:** one background chain for r1 on all three levels, then the lint and deco files, so the stop's
       2:40 computes. Commit the report skeleton first.
     - **Block 2 (5:00-7:00):** the resume (rule 11), r1's review, r2 at ~5:30, the GATEs straight after r2, builders stop at 6:45.
     - The close starts at 7:00 sharp, with (b)-(e) as one background chain right after `--start`.
     - Say in "The target": "about 4:40 of agent time is expected; plan for at most two judged rounds".

5. **T1 is code-scout's #1-#10 plus three dry rounds in 85 minutes. It has no FAIL branch, and it bundles GATE-only items with what
   M1 and M2 need. Nothing says how its tools and the shared commit reach the builders' trees.**
   - **Evidence:**
     - T1's doneWhen (slate-seed.json:19-21) covers levels67.json, gen-slots.cjs for three levels with byte-equal --check and a
       byte-for-byte cross-check on level 1, n67-a1.cjs, and the round tools (n67-round, red, round-verbs with the slate verb, table,
       gate-pack, deco, slot-set) plus `dry-<L> --shipped` "end to end on all three levels".
     - code-scout.md:151-153 spread the same work to 4:00.
     - A dry round with every slot is iceline 8 + kiln 5 + spire 3 = 16 rooms of serial skip at ~180 s each (NIGHT66 r1: 1082 s for six,
       NIGHT66-LOG.md:137). That is about 50 min of T's tree on its own.
     - NIGHT65's T wrote gen-n65-foundry in 32 min (NIGHT65-LOG.md:63), so the assembler is not the problem. The bundle is.
     - "Before T1 passes, no room's M2 counts" (NIGHT67.md:97), and T2 bundles the multi-level play build with the lint
       (slate-seed.json:31). A FAIL on n65-slot-set's self-test, or on the play build, voids every room's M2 or M1.
   - **Builders' trees:**
     - Builders may not merge, rebase or cherry-pick (builder-brief.md:24). Their branch is cut at t0, yet their room files are "written
       by T1's gen-slots.cjs --export into your tree" (:31-32).
     - Without the shared commit's config rows, `NightRows.BaseId("n67-iceline")` returns itself (NightRows.cs:283-296). The level-2
       startSet and the unlocks table then miss (SurvivalSim.Weapons31.cs:132; Unlocks26.cs:78-88: "null: the loadout is left alone"). So
       a builder's sim on an n67-* id runs with the wrong loadout. By reading; not run.
     - Builders idle from the card critic (1:10) to T1's landing. The brief gives them nothing for that gap (code-scout.md:182-184 names
       the scratch-plan probe).
   - **Fix:**
     - **Split T1:**
       - **T1a by 0:50:** levels67.json, gen-slots `--export / --check / --run --out` on iceline (R's level) and n67-a1.cjs (with
         finding 1's slots files). The lead's shared commit follows at ~0:55.
       - **T1b by 1:45:** kiln and spire, then the round tools taking LEVEL. The dry round is ONE room per level (`ROOMS=g8`, `ROOMS=kb3`,
         `ROOMS=v6`).
       - **T1c by the first GATE:** gate-pack, deco `--level`, slot-set per level.
     - **Split T2:** T2a is the lint. T2b is the multi-level play build, moved to PC.
     - A T1 FAIL branch: "a fix that changes only plumbing (paths, tables) keeps the reds and r1's files; a fix that changes gen-slots'
       assembled level voids them: re-run the reds and say so".
     - Brief step 3, added: "when the lead says T1a landed: `git -C <wt> checkout port/stage3 -- <the shared commit's paths>
       Tools/night/gen-slots.cjs Tools/night/levels67.json` (a path checkout; never commit those paths). Before that, measure only the
       shipped ids (n52-iceline, n57-kiln, n52-spire). From 1:10 until then, block round 1's change in a scratch plan copy
       (code-scout.md section 4) and port it when the export lands".

6. **Rebuilt rooms need `verb65` and `line65` in the room file, and nothing tells the builders. Research tells them to add an `area67`
   block, which the assembler refuses.**
   - **Evidence:**
     - The GATE pack runs its fresh skip on seeds 11-20 (M2's second set) only when the round plan's row has `verb65`
       (n67-gate-pack.cjs:122-124). It never plants.
     - T1 plants the slate's verb "on base rooms" only (slate-seed.json:21).
     - The verb pilots take their pose from line65 or the spawn (levels-data.md:205).
     - The brief never names verb65 or line65 (builder-brief.md:23-45).
     - research.md:71 says the new fields are "kept under one `area67` block in the room row": `area67.high`, `.lanes`, `.loop`,
       `.landmark` and others. The brief sends builders to research.md section 1 (builder-brief.md:19-20).
     - gen-n65-foundry.cjs's KNOWN list (:73-85) has no `area67`, and "refuses an unlisted key" (:17-18). The round then stops at
       "the latest assembly failed" (n67-round.sh:34) for that level's rooms.
   - **Damage:**
     - With no verb65, no room can GATE (no seeds 11-20).
     - One builder's area67 block kills its whole level's round.
     - A room's verb65 and the slate's verb can drift apart, and nothing checks them.
   - **Fix:**
     - Brief step 3, added: "your room row carries `verb65` (exactly the slate's `verb`) and `line65` (3 or more waypoints of the verb's
       line) from round 1. Never add `area67`: the lint infers from the geometry".
     - T1b: the round and the pack print `verb65 <x> (slate <y>)` per room and refuse a mismatch.
     - research.md:71's line is marked "a proposal: not in tonight's KNOWN keys".

7. **The level-3 picks rest on quotes about rooms that no longer exist, and leave out the one level-3 room with a measured fault.
   "Smaller" is promised and not built.**
   - **Evidence:**
     - kb2's title is "GG's 'The first room is confusing'" (slate-seed.json:47; NIGHT67.md:7). That quote is from 2026-10-16
       (history.md:116), about the n43 kiln.
     - NIGHT57 rebuilt the kiln from scratch on 10-21 (history.md:118, :174), and GG has not played n57-kiln (levels-data.md:174). "Just
       isn't good" (10-13) predates the rebuild too. This is corner 15, a quote read onto the wrong room (history.md:218).
     - kb2 trips no flag (levels-data.md:165). Its DULL of 63.6 % is the first-room effect the scout warns about (:191).
     - The one measured level-3 fault is kb3c's skippable ride (0.81, levels-data.md:206), which NIGHT67.md:25 cites. kb3c is not in the
       slate.
     - Size: P1's size rule passes 25 / 25 (finding 3). The slot locks `id, kind, rect` (gen-n65-foundry.cjs:86-87). The brief's "smaller
       inside the rect when the long axis is over 32 m" (builder-brief.md:36) never applies. All 8 targets are 560-768 m2. Yet NIGHT67.md:5
       opens with "a lot smaller".
   - **Damage:** S spends half its rooms on a level-3 room with no measured problem and an unfixed one beside it. The report will claim GG's
     words for rooms he never saw, and GG's size ask stays untouched with no line saying so.
   - **Fix:**
     - Slate: kb2 -> **kb3c THE TAP RUN** (the KESTREL in hand from kb2b, ride carded, a red of 0.81 already measured).
     - kb3 keeps gg-proxy's evidence, written as what it is: "bot-tour frames of n57-kiln; GG has not played n57-kiln".
     - Every title and card quotes GG only on the room he played, with its date.
     - "The target" gains: "Not smaller: the slot keeps each rect (GG's later words: 'wide open arenas with a roof', 2026-10-13). A room
       gets smaller only by a fill or a void that does a job, measured by finding 3's size rule".

8. **The review slot and the main tree's single dotnet slot are each booked past an hour per hour. The slate is read live by a running
   round.**
   - **Evidence:**
     - The one review slot holds: the card critic, T1's and T2's critics, four level reviews, a spotter per room (8) and a critic per level
       (3). That is about 18 runs at 15-23 min each (L-109), against about 4:40 of agent time (finding 4).
     - "Spotters ... may run in a free slot" (NIGHT67.md:47-48), but after T ends three agents run, which is the cap. There is no free
       slot.
     - Per round, the main tree holds three level rounds in series, about 35-40 min for eight rooms (the serial skip at ~180 s a room;
       code-scout.md:228-235). The pool is "cuttable" (code-scout.md:142) and not in T1.
     - Each tool pick needs n67-gates.sh, and each GATE pack needs its own levelgen plus a 10-seed skip. Neither may overlap a round
       (NIGHT67.md:49-50, :117).
     - n67-round-verbs.sh passes `--bars "$MAIN/Logs/n67/slate.json"` read at each skip call (:28). Rule 6 forbids picks, commits and slot
       writes during a round, but not slate writes.
   - **Damage:** rounds outrun their reviews, or GATEs queue behind rounds. A mainBar edited mid-round changes the bars of the round's later
     rooms (NIGHT66's line-3 corner, by another file).
   - **Fix:**
     - One batched critic for T1a and T1b.
     - The GATE critic writes its level's review for the GATE round (FORGE67 red team 2).
     - One spotter run per level, reading every pack of that level.
     - No level review for a round in which nobody is a GATE candidate. The builders get the table and the lint lines instead.
     - n67-round.sh copies the slate to `Logs/n67/<tag>/slate-at-start.json` and passes that as `--bars`.
     - Rule 6 gains "or slate write".
     - Rounds run about every 70 min, not 60.

9. **Leftover (1), "fodder in one link per rebuilt level", has no pipeline. Doing it means editing a shipped level in place, with no
   GATE.**
   - **Evidence:**
     - The slot model keeps every non-slot room bit for bit ("KEPT", gen-n65-foundry.cjs:5; code-scout.md:136). The links (gr; kb1,
       kb1b, kb2b, kb4; the spire's v1, v3, v4, v7-v10) are not slots.
     - Levels 2 and 5 have no generator, and "an in-place edit ships at once" (levels-data.md:245). Their rows iceline52 and spire52 are
       ON.
   - **Damage:** a lead who reaches the leftover at 5:30 either edits Design/levels/n52/iceline.plan.json, which changes the shipped game
     with no review, GATE or off switch (rule 8; PRINCIPLES rule 1's risk), or finds gen-slots refusing it.
   - **Fix:** NIGHT67.md:105-106 and DECISIONS D-9: "(1) is out tonight: links are KEPT by the slot model. It needs a link slot in
     gen-slots (its own GATE) first. Never edit a shipped plan in place". The ranking then starts at level 1's a4.

10. **Pierce rooms would be judged on the charge rule nobody has judged. NIGHT67 dropped the T0 that was to judge it.**
    - **Evidence:**
      - HANDOFF.md:34-35: "T's last commits are on main (d25ea7d8 the charge rule's reach + n65-verbs.sh's timeout 900 ...):
        unjudged, NIGHT67's T0".
      - THE AREAS has no T0. A grep of NIGHT67.md, DECISIONS.md and the slate finds no d25ea7d8 and no charge rule, except D-9's
        leftover.
      - Pierce is allowed (slate-seed.json:4), and finding 2's fix makes it the natural verb for g8, g7 and kb3.
    - **Damage:** a pierce room's M2 on an unjudged trigger is exactly what FORGE67's T0 existed to prevent. A literal GATE critic rules
      UNSURE.
    - **Fix:** T1's critic also reads d25ea7d8. T re-runs its eight after files (T tree, Research/2026-10-23-wave66/T/t1b-amend-after-*)
      on main's DLL and diffs the seed, SUMMARY and M1 lines (FORGE67 red team 1's step). Until that critic PASSes, no room cards pierce.

11. **GG's branch-C play build would show the rebuilt ice line without its grade, because the old build's code does not know the new
    id.**
    - **Evidence:**
      - `Polish53IcelineLevels = { "n47-iceline", "n52-iceline" }` is code (UnityTuning.N53.cs:29), and the shared commit adds n67-iceline
        there (slate-seed.json:20).
      - The play build is a data swap on Builds/n66-freeze (NIGHT67.md:124-126; slate-seed.json:72), which carries the old code.
      - Section 9 (b) builds n67-freeze first when 12 GB are free (NIGHT67.md:155).
    - **Damage:** GG's "better, worse or the same" mixes the rebuilt rooms with a missing colour grade. A look the player had is lost in
      his build.
    - **Fix:** NIGHT67.md section 4 step 6 and PC: "the play build's source is Builds/n67-freeze when (b) built it; else n66-freeze, with
      'iceline's polish53 grade is not applied (old code)' on the play card, or kiln and spire only".

12. **Branch A / A-lean breaks twice: the flip tool cannot find two of the three rows, and the close's reels would load the old levels
    with no campaign place.**
    - **Evidence:**
      - n67-freeze-rows.cjs:58 matches `String(s.row).includes("design." + key)`. LV's row reads "design.n67.iceline67 / kiln67 / spire67"
        (slate-seed.json:60), so `n67.kiln67` and `n67.spire67` are refused.
      - With a row ON, `n52-iceline` leaves the campaign order (NightRows.cs:269-278), and `-skygrindMap n52-iceline` loads it with no
        place. That is NIGHT66's vn artefact: the full loadout and grapple rows RED (NIGHT66-LOG.md:274-279). Yet (c) names
        `vi,n52-iceline,...` "(a rebuilt level plays through its row when the row is ON)" (NIGHT67.md:157-158).
      - LV's doneWhen needs the flip (its line 4, slate-seed.json:65) before its critic PASS, and the flip needs LV's PASS
        (n67-freeze-rows.cjs:57-61). The order is circular.
    - **Damage:** if GG frees the disk, LV cannot flip kiln or spire, and the close's player line reads RED.
    - **Fix:**
      - LV's row becomes "design.n67.iceline67 / design.n67.kiln67 / design.n67.spire67".
      - Section 9 (c) names `vi,n67-iceline,verbs37` (and vk, vp) for every ON row.
      - LV's doneWhen: "the critic PASSes on rc1 built with the rows on through n46-build-rows; then the flip; then the flow-base remake".

13. **Rule 8's slot command writes the wrong file.**
    - **Evidence:**
      - NIGHT67.md:55 writes `--slots Design/levels/n67/<level>/slots67.json` with a space.
      - n65-slot-set.cjs reads only `--slots=` (:21), so the file argument becomes a positional. The slots file falls back to
        `Design/levels/n65/slots65.json` (:61).
      - SLOTS and roomPath are level 1's (:19, :23), so today it refuses g1. A T1 that only widens SLOTS would write g1 into level 1's
        file.
    - **Fix:**
      - Rule 8: `node Tools/night/n65-slot-set.cjs <room> new --level=<L> --slots=Design/levels/n67/<L>/slots67.json
        --slate=Logs/n67/slate.json`.
      - T1c: refuse an unknown option and a room outside that file's level. With `--level`, never fall back to slots65.json.
      - A self-test case for each.

14. **Smaller items.**
    - **M3 launch** asks for "a launched body into a hazard or off a 3 m drop" (slate-seed.json:8). `n65verb launch` prints the flight's
      end, thrown metres and bursts (Verb.cs:262), and no hazard or drop. Re-word it before any measurement to what it prints (`flight end
      wall|body`, kills), or a literal critic finds it NOT MET (FORGE67 red team 5(d)).
    - **Two dotnet jobs at the bearings:**
      - HANDOFF's arrival table runs `gen-n65-foundry.cjs --check`, a levelgen through `dotnet run` that must be "run alone".
      - Step 3 starts the flow base in the background, also `dotnet run`, in the same turn (NIGHT67.md:73-75).
      - Run the table's --check first, then the flow base. This is the corner NIGHT66 hit with MSB3021 (NIGHT66-LOG.md:197-199).
    - **The commit-free hold** ("under 15 GB: hold", NIGHT67.md:48) has no end and no fallback, and T1's landing needs a gate run plus an
      Editor compile. The two workers still hold 13.7 GB (19.5 GB free now). Add: "pause the builders' pools for the gates; still under
      15 GB after 10 min: gates `--no-editor`, then the Editor compile alone".
    - **`LEVEL=<L>` collides with n67-red.sh's own `LEVEL` (an id, set from $2: n67-red.sh:12).** Name the short level `LV67` in T1's
      tools.
    - **The brief's guard command** (builder-brief.md:41) reads `Design/levels/<level>.json` with no LEVEL_DIR (n67-red.sh:20-21). Name
      it: "`n67-red.sh 2 n67-<L> ...` after `gen-slots --level <L> --run` in your tree (its default writes your tree's copy), or with
      `LEVEL_DIR=<your --out>`".
    - **A verb changed after the reds** (a card CHANGE in round 1) leaves the room's red, bar and ceilings stale. Add: "a verb change
      re-runs that room's red first".
    - **The disk:** 13 GB now, 1 GB above the guard's build floor. NIGHT65 lost 4.5 GB in a night. The close's (b)-(d) may well be
      `## Blocked`. That path is handled, but the play build then comes from n66-freeze (finding 11).

## Checked and fine (so the night does not spend time on them)
- **The close-check's selftest** passes 27 cases, and a correct four-row close passes with no arguments (t1slip.cjs's first case).
  GOAL.txt is FORGE67's corrected text.
- **BaseId** follows a level row whether it is ON or OFF (NightRows.cs:280-296). Once the shared commit is in a tree, n67-* ids get
  their base level's loadout and awards in the sim (SurvivalSim.Weapons31.cs:132). Before it they do not (finding 5).
- **n46flow and skip v2 read any level file** (`--file`, `--level <file>`), and the plant sets verb65 and bot43 in memory
  (VerbPilot.cs:51-62). The ride pilot needs only `ride36` faces, and every target room has them (plan read).
- **The worktrees:** n66-R a64c5c3d, n66-S 29322705 and n66-T 2fa9c5a6 hold no tracked change. Their untracked NIGHT66 evidence is as
  the brief's first commit lists it. No `n67-*` branch exists yet.
- **n67-slate-set.cjs** sets any field but `doneWhen` and `id`, so `verb=<x>` works today.
- **Every target room's long axis is 32 m or less, and its ride walls are 4 x 8 m or more:** nothing tonight is blocked by the slot
  rect's size (finding 7 is the reverse problem).
