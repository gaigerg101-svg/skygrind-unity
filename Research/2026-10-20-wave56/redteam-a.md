# NIGHT56 red team (2026-10-20 ~23:45, read-only review of NIGHT56.md before the run)

Read: NIGHT56.md; Research/2026-10-20-wave56/ (IDEAS-POOL, levels, enemies, colors, gameplay); HANDOFF.md's NIGHT55 block;
NIGHT55-LOG.md; CLAUDE.md; docs/PRINCIPLES.md; docs/DESIGN-RULES.md; docs/memory/OWNER.md and CORE.md. Checked against the tree,
the builder branches and the harness sources with Glob / Grep / Read / git (nothing edited but this file). "Cost" is my estimate
of run time lost if the finding is not fixed. It is not measured. Ranked by that cost, largest first.

---

## 1. Builders branch before NIGHT55's code is on port/stage3 (sequencing; cannot be done as written)
- **Lines:** NIGHT56.md 56-58 (spawn L, G, V at 0:00-0:20; P0 picks at 0:20-1:30), 80 (a RED NIGHT55 row gets ONE fix from tonight's
  builder), 100 (L5 builds on R's slice 1), 111 (G2 builds on kill55's ladder), 117 (G4 uses wick55's method), 123 (V1 starts
  from decor55), 131 (V4 is grade55 r2).
- **Problem:** each worktree is cut from HEAD at spawn time, which is the shared commit. NIGHT55's later commits are not on it:
  B 26121bd5 / fd71daf1 / 169e162c, C 91f9c48e / 47c64d8b / 5fe9b6cc / 283f3c41, K 3c8fd885 / 1b996462, R edfee264 / 876dc97e /
  e25d89bb. Five of tonight's entries and every P0 RED fix build on that code. Builders may not merge (template line 166; memory
  builder-merge-blocked: the classifier refuses a builder's `git merge port/stage3`), and the prompt never tells them to
  cherry-pick. So each builder either re-implements code that already exists or edits a stale copy. All four NIGHT55 branches
  also edit the same region of `Design/unity-config.json` (hunks at lines 9034-9220 in fd71daf1, 283f3c41, 1b996462, edfee264,
  26121bd5, 169e162c).
- **Cost:** 2-3 builder-hours, plus conflict work at every pick.
- **Fix:** do P0-pick and its gates FIRST (0:00-0:45). Then make the shared commit on top, then spawn the builders from it. If
  the builders must start earlier, add to the template: "first `git cherry-pick <the HANDOFF list, in order>` onto your branch
  (5fe9b6cc together with 283f3c41; skip 8288aec2)".

## 2. The usage budget is not counted (hidden cost; could end the run)
- **Lines:** 21-24 and 156-158 (the forge counts builders and critics only), 95 and 99 (a fresh GG-persona review per room,
  L-087), 141-145 (one critic per build), 151 (wave-memory harvest of w55 + w56).
- **Problem:** the plan's estimate is "3 builders, so no limit". NIGHT55 hit the limit after about 15 agent-hours: 5 builders x
  ~1.8 h, 6 read-only agents x ~0.6 h, and the lead 2.5 h (17:05-19:33). Tonight's plan is about 27 agent-hours:
  - builders 3 x 5.2 h
  - the lead 7.5 h
  - 4 gate critics
  - 8 persona reviewers (L3 x7, L4 x1), each reading frames
  - the harvest
  That is nearly twice NIGHT55's spend before the cut (an estimate, not measured). Section 11 also assumes "the lead continues
  alone", but the limit stops the lead too (L-003; NIGHT55 did nothing from 19:33 to 22:01).
- **Cost:** everything after the cut (NIGHT55: 2 h 28 min in, 25 of 26 entries never gated).
- **Fix:**
  - Fold the per-room persona reviews into each build's critic as one GG-persona section.
  - Count every spawned agent in DECIDE56.
  - Pre-write the freeze / close steps so a resumed lead can run them cold.
  - In section 11, say what to do when the cut lands (see #15).

## 3. L's slate does not fit in 5 h 10 min; L1 duplicates data that exists; L3 and L4 overlap (sizing; contradiction)
- **Lines:** 85-101.
- **Problem:**
  - **L1 in 30 min** must build a walk graph (ride / pad / drop edges), dead-end and line-of-sight checks, R5 footholds, a new
    `jobs56.json` and a planted case. But every solid already has a job: 199 solids in the 18 rooms' plan `boxes`, 0 untagged,
    in the vocabulary line / cover / perch / lane / ride / circuit / climb. jobs56.json would re-tag all of them in levels.md R8's
    other vocabulary (foothold / los-break / loop / ...).
  - **"Wave nodes in view" is half built:** n46flow already prints `w1 in view 4/4` per room (cand-A-n52-foundry.txt).
  - **L3 is too big:** 7 rooms, each needing contract 14/14 (n52-contract-all.sh: 2.5-10 min a run at 10-12 dotnet processes),
    walk 10/10 and a review. History: NIGHT55's builder R did 2 rooms in ~1.5 h; EXT53's two builders did 6 rooms in ~2 h.
  - **L4 throws L3's work away:** it rebuilds a3 or g4 from scratch, and both are in L3's list.
  - **The pool says per level:** IDEAS-POOL L-3 says "one showcase room per level"; L4 says one room.
- **Cost:** ~2 h of L's work that never reaches a gate.
- **Fix:**
  - L1 reuses the plan's `job` field (map the R8 words onto it) and n46flow's in-view column; cap L1 at "dead ends + in view +
    the planted case".
  - Take L4's room out of L3 (6 rooms, FAIL target <= 2 including it), in a priority order.
  - L4 starts only after L2 passes at r2.

## 4. The clock: A0, P0 and r3 are mis-sized, and the freeze has no build (sizing)
- **Lines:** 56, 57, 59, 60, 61.
- **Problem:**
  - **A0 in 20 min:** it holds bearings, the shared commit (rows-def, the n56-shared / addrows scripts, FeelReel.N56,
    N56SharedTests, n56-gates.sh, the slate), its gates, the builder brief and 3 spawns with `df`. NIGHT55's equivalent steps ran
    17:33-17:50 after a 28-min A0.
  - **P0 in 70 min:** it holds 12 picks (possible JSON conflicts, see #1) and new .cs files that leave the Editor stale (memory
    pick-new-file-stale-editor). Then gates, a build, and ~16 Player runs, because decor55's proof is a452 + n55-decor-share.py on
    each level, not a255 as line 78 says. Then the smoke (~4 min) and a critic (~21 min in NIGHT55). That is 70-80 min with zero
    conflicts and zero REDs.
  - **r3 builds at ~5:00, but builders commit until 5:30.** Those 30 min (about 1.5 builder-hours) can never be gated.
  - **r3 runs into the freeze:** its reels and critic end ~5:45, when the freeze starts. The freeze also changes the repo config
    (the ON calls) but lists no freeze build.
  - **Seeded leftovers:** they have no owner (bossText55 was B's, fov55 was C's) and are gated behind "every entry's first
    attempt", so they will not be reached.
- **Cost:** ~2 h (the late commits, plus a P0 slip pushing r2 and L2's only retry).
- **Fix:**
  - A0 0:00-0:40, P0 0:40-1:50.
  - The builders' last commit at 4:40 for r3 at 4:45.
  - Add "build n56-freeze" to the 5:45 row.
  - Give each leftover an owner, or drop the list.

## 5. G's entries rebuild systems the game already has (contradiction with the code; ambiguity)
- **Lines:** 16 ("the style rank ... pays nothing"), 105-116, 138; gameplay.md 12-14 ("not checked in code"), 122-126, 130-134.
- **Problem:**
  - **The rank already pays.** Style.cs:4-5 says "the rank multiplies points (economy), speeds regen (regen.rankPerRank) and
    stamina (style.rankRewardPerRank)". In config: points.rankMult = [1, 1.1, 1.25, 1.5, 1.75, 2]. A hit already costs style
    (style.damagePenalty 30), and a movement-state multiplier exists (style.stateMult 1.2).
  - **The rank-up slam exists:** UiStyleMeter.cs:263-342, white-hot then the cyan ladder.
  - **tokens41 already does G3's first clause:** "at most two enemies winding up at once in the campaign", cap 2, ON
    (Design/unity-config.json design.n41.tokens41; HuskBrain.cs:660).
  - **fresh56 duplicates freshness:** Freshness19 and style.freshness already exist.
  - **G2's ladder is P0's row:** the hold ladder is kill55's, which P0 gates the same night, so two rows would own one ladder.
  - **The "DULL in 2772 / 2772 frames" premise is wrong:** those frames come from reels with no kills (critic-ggproxy.md:79-86,
    "Not one kill on any frame").
  - **The pool disagrees:** IDEAS-POOL:40 sends E-2 (tokens / idle advance) to IDEAS.md as a runner-up, while G3 builds it.
  - **gameplay.md breaks the hold cap:** P2's 30-step (0.25 s) parry exceeds line 110's 0.12 s cap.
- **Cost:** ~1-1.5 h of G building what exists, or stacking a second rank payoff on the first.
- **Fix:**
  - G1 = the existing rank's payoff extended to heal36 / RIDE KILL, measured against a baseline taken first.
  - G2 merges into kill55's P0 fix (one row).
  - G3 = on-screen priority + accuracy vs speed + idle advance only.
  - Strike fresh56 and gameplay.md's 0.25 s parry in the builder brief.

## 6. Weakest new feature: V1 colour56 is the most likely to be reversed by GG (owner fit)
- **Lines:** 121-126, 146 (a GG-named slice stays ON on a tie); colors.md 228-256.
- **Why:**
  - **What V1 does:** it caps 95 % of world pixels at S <= 0.55 (Valve's L4D grey-down) and puts the key on only 1-5 % of the
    frame. Spire becomes achromatic (S <= 0.08) and kiln a grey umber (S <= 0.18). It also removes the saturated dusk orange from
    the sky (5-70 deg at S > 0.35 = 0 %).
  - **What GG said, latest first** (OWNER.md:37, 118, 119):
    - 2026-10-19: "the colors and stuff are way too bland ... very inspired by Ultra Kill and Doom".
    - 2026-10-20 (NIGHT54): "lack strong color direction".
    - w16: colours "bland".
  - **The rule it follows predates him:** DESIGN-RULES V-1's "desaturated" world was written 2026-10-16, before his 10-19 words.
  - **It touches the game's identity:** CLAUDE.md rule 8 makes "a dusk palette" part of it.
  - **It changes every frame of every level,** so GG sees it in the first minute. Line 146 can ship it ON on a tie, because GG
    said "colors".
- **Runners-up:**
  - G3's "accuracy falls with speed" (GG w19, OWNER.md:36: "you move too fast ... nothing can catch you. We need a little bit of
    balance").
  - G1's S x2.5 RIDE KILL refund, which feeds the ride's speed that line 50 rules Out (GG 2026-10-13: "you gain speed too
    quickly").
- **Cost:** most of V's night (~2-3 h) if GG reverses it.
- **Fix:**
  - Gate on reserved bands only (colors.md method 2: world pixels inside the four role bands <= 2 %), not a global S cap.
  - Keep spire and kiln chromatic, and keep the dusk sky warm but below danger saturation (or rose-violet).
  - Ask the blind pair "which is less bland / has the stronger colour" in both orders; a tie goes OFF.
  - Quote GG's 10-19 words in the DECIDE line.

## 7. `n46flow --level all` measures the old levels; no NIGHT55 flow baseline exists (wrong data; cannot be done as written)
- **Lines:** 42-43, 91-92, 115.
- **Problem:**
  - **Wrong levels:** Harness.N46.Flow.cs:62 sets `F1Levels = { "n45-foundry", "n45-iceline", "n45-kiln", "n40-span",
    "n41-spire" }`. L2's stall / clear gate and G3's deaths gate would run levels nobody plays.
  - **Stale baseline:** the flow table (Logs/n52/flow-freeze/table.md) is NIGHT52's, and NIGHT55 re-cut iceline g5 / g7 since.
  - **Wrong number:** line 43's "iceline clear 221 s" is n47's base (n52: 246.4 s).
  - **The pool disagrees:** IDEAS-POOL L-1 says stall <= 15 %; line 91 says <= 25 %.
- **Cost:** ~45 min of re-runs (each level is ~131 s wall per 10 seeds), plus ON / OFF calls made on wrong numbers.
- **Fix:** run `n46flow --level n52-foundry,n52-iceline,n52-kiln,n52-span,n52-spire --seeds 20` as in the n52 table, at u3-n55,
  as the baseline before L2 / G3 change anything. State one stall bar.

## 8. The image cap is above the limit NIGHT55 measured; one critic per build is a single point of failure (hidden cost)
- **Lines:** 77-79, 111, 125, 142-145.
- **Problem:**
  - **The cap is too high:** NIGHT55's critics lost image reads at 26 (critic-ggproxy.md:6) and 36 (critic-firsttime.md:8, 20).
    The cap here is 40.
  - **r1 alone is over it:** P0's ~12 NIGHT55 entries x <= 4 frames is ~48 images.
  - **Later rounds add blind pairs:** V1's (5 levels x 2 orders x A / B = 20 images) and G2's (4). When the critic runs dry,
    the whole build's gate is lost.
  - **The blind-pair tool only takes one naming:** n46-blind-pairs.cjs:13 hard-codes `shot-N46-V46-off|on-<moment>.png`, so it
    finds no a452 / a256 frame. It has no "both orders" mode.
- **Cost:** one gate round (~30-45 min) per overflow.
- **Fix:**
  - Cap at 20 images per critic.
  - Montage 4 frames into one contact sheet, and show a pair side by side as one image.
  - Split P0's critic from the builders' critic.
  - Add a renaming step (or a `--pattern` argument) to the blind-pair tool, run it twice with swapped sides, and keep the key
    out of the critic's folder.

## 9. G4 would tune THE KILN WYRM in the wrong arena (cannot be done as written)
- **Lines:** 44, 117-118.
- **Problem:**
  - **Wrong default:** `n47human --boss wyrm` defaults to `--level n46-kiln` (Harness.N47.Human.cs:4, 11, 32). n46-kiln's k5 uses
    `Design/maps/wyrm46-arena.json`, while the shipped n52-kiln's k5 uses `wyrm47-arena.json` (THE SLAG ISLANDS).
  - **Wrong baseline:** NIGHT49's 122.6 s was also n46-kiln (Logs/n49/meters/wyrm-default.txt:5).
  - **Wrong switch:** G4 tunes design.n55.wyrm55, a NIGHT55 row, so `-skygrindOff56` does not switch it off.
  - **GG's limit:** "keep the worm AI the same" (2026-10-13).
- **Cost:** ~1 h of G4 (40 seeds is ~491 s wall per run).
- **Fix:** `n47human --boss wyrm --level n52-kiln --seeds 40`, re-baseline first. Encounter numbers only; name `-skygrindOff55`
  as its comparison side.

## 10. L2's "spawns in view" contradicts the openings, R5 / R6 and I7 (contradiction; ambiguity)
- **Lines:** 87, 89-92; levels.md R5 (65-77), R6 (79-87); Design/levels/n52/openings53.json.
- **Problem:**
  - **Two checks pull against each other:** L1 adds R5's full-height foothold 4-8 m inside the entry ("it hides the room's far
    half"), while L2 wants >= 2 spawn nodes within 15 m and in line of sight of the entry.
  - **Source R5 forbids it:** no spawn within 8 m, and none with line of sight into the entry corridor. R6 triggers ambush rooms
    past midfield.
  - **The openings say far by design:** openings53.json marks 8 rooms "held" and 3 "vista" (enemies far). I7 needs no two
    consecutive rooms to share an opening.
- **Two readings:**
  - One engineer moves spawns forward everywhere, which makes the openings the same and can fail I7.
  - Another keeps the openings and misses 15 / 18.
- **The reel does not measure it:** a452's "in view" is a +-45 deg yaw cone with no distance and no occlusion
  (FeelReel.N52.Rooms.cs:56-67), so ">= 2 within 15 m / spawn tells visible" needs new reel code. The prompt does not say
  whether a452 or a156 gets it.
- **Cost:** ~45 min of moves and re-moves.
- **Fix:**
  - Per opening: held / vista need >= 4 enemies or tells visible at any distance; ambushed need the first group within 15 m by
    midfield.
  - Keep spawns 8-15 m and beside the foothold, not behind it.
  - Make "I7 PASS" part of L2's doneWhen.
  - Name the reel phase that prints distance + line of sight.

## 11. G1's doneWhen has no pilot that can meet it (cannot be done as written; ambiguity)
- **Lines:** 105-108; gameplay.md 23-24; IDEAS-POOL:18.
- **Problem:**
  - **No skilled pilot for combat rooms exists:** n46flow is Walk38Pilot + the "normal" autopilot; n47human is boss-only;
    "skilled" pilots exist only for brawl / crown. Neither harness prints the rank.
  - **The numbers do not reach A:**
    - A is at 470 (style.thresholds).
    - Drain is 17 + 4.8 per rank per s.
    - A husk kill is worth 30.
    - The walk pilot's kill gap median is 2.4 s on n52-foundry, so it earns about 12-15 style/s against 22-27 drain at C / B.
    Reaching A in 20 s means tuning gains to the bot (rank inflation).
  - **x2.5 of an integer:** RIDE KILL refunds `int charges` (RideKill53.cs:61), so "S x2.5" of 1 boost is ambiguous (round to
    2? to 3? make it fractional?).
  - **The pool's bar differs:** ">= 30 % of combat at B+" (IDEAS-POOL) vs "< 40 % at D" (line 107).
- **Cost:** ~45 min of tuning to an unmeetable bar.
- **Fix:**
  - Take the rank baseline first (add a rank column to n46flow).
  - Gate on a shift of the rank distribution vs off with the same pilot, and prove "A within 20 s" with a scripted chain in a256.
  - State the rounding rule.

## 12. Colour entries contradict each other and the rules (contradiction)
- **Lines:** 123-126, 129-133, 49.
- **Problem:**
  - **V3 vs V4:** V3 reserves yellow 45-70 deg for parry and meters it on a254 frames. V4 turns THE WYRM inside 15-55 deg (45-55
    is parry yellow) on those same boss frames.
  - **V1's two starting points disagree:**
    - decor55 (283f3c41) moved foundry's accent to 72 deg, 2 deg past parry yellow, and iceline's to 150 deg, inside colors.md's
      cyan caution band.
    - colors.md's keys are about 132 / 220 deg.
  - **Kiln and iceline share a hue:** kiln's cobalt accent (222-230) sits on iceline's key (215-225).
  - **Key or accent?** V1 puts the "key" on 1-5 % of the frame. In colors.md the key is 50-70 % of world pixels (method 7); 1-5 %
    is the landmark accent (method 8).
  - **"Or one achromatic":** two keys are near-achromatic (kiln S <= 0.18, spire S <= 0.08).
  - **V3 touches an enemy look:** it recolours THE CROWN's gems, which line 49 and CLAUDE.md rule 7 put Out.
- **Cost:** ~30-45 min of V's iterations plus critic disputes.
- **Fix:**
  - THE WYRM 15-44 deg.
  - Pick colors.md's keys or decor55's, and log the choice.
  - Name key vs accent with their shares, and say which level may be the achromatic one.
  - THE CROWN's gems become a proposal page only.

## 13. Split ownership of NIGHT55 builder B's files and the shared config (contention)
- **Lines:** 80 ("G kills / boss text / WICK, V grade / decor / SSAO"), 131-133, 165 ("Your files: <paths>" never filled).
- **Problem:** B's three commits share FeelReel.N55.Boss.cs (a155 holds both the grade and FIGHT MODE), Tools/night/n55-b-rows.cjs
  (a replayed config writer) and the design.n55 block. Tonight THE WICK goes to G and the grade to V. Both would edit a155 and
  re-run n55-b-rows.cjs, which rewrites the other's numbers. V4's "hot wall" and G's WICK fix both touch THE WICK's burn state.
  G and V also fill adjacent design.n56 rows (tokens56 sits next to rim56 in the row order).
- **Cost:** ~30 min per round in conflicts and clobbered numbers.
- **Fix:**
  - One owner for a155 / n55-b-rows.cjs (V), with G's FIGHT MODE in a new partial file.
  - Write the file list into each spawn.
  - Order rows-def by builder, with one writer script per builder.

## 14. tokens56 makes the sim read the camera; G3's deaths gate sits inside the noise (ambiguity; determinism)
- **Lines:** 112-116.
- **Problem:**
  - **The sim would read a player setting:** "tokens go to enemies on screen first" needs a frustum. FOV is a player video
    setting (UiSettings.cs:30 `fovDeg`), plus the speed FOV boost and zoom. A sim rule that reads it ties replays and tapes to a
    viewer's settings, against rule 4's split of sim and view. Two engineers would build it two ways.
  - **The deaths gate is noise:** "deaths within +-20 % of NIGHT55's" sits below the flow table's own band for deaths (8). Span
    has 1 death and spire 0 per 20 walks.
- **Cost:** ~30 min, or a fingerprint / tape problem found late.
- **Fix:**
  - Define "on screen" in the sim as a cone from config `camera.fovDeg` (74) around the sim's look yaw / pitch, never the
    setting.
  - Gate deaths as "inside the measured band".

## 15. Stops and resumes the prompt does not handle (stops; /goal)
- **Lines:** 1 ("at least 7 h"), 53, 187-189, 197.
- **Problem:**
  - **The fallback cannot run:** the usage limit stops the lead too (#2). NIGHT55 resumed 2.5 h later, past its cap.
  - **The window can become impossible:** a resume after 7:30 meets "close table no earlier than 6:50" and "never build past the
    cap" with no rule for a cold close. A /goal of "at least 7 h" or "close table at >= 6:50" can then not be met as written.
  - **"At least 7 h" reads two ways:** the close table at 6:50, or the end of the run at 7:00.
  - **Reels skip silently between 6 and 12 GB:** n55-reels.sh (copied as n56-reels.sh) refuses every reel under 12 GB free and
    only prints SKIPPED, while line 189 allows reels down to 6 GB. A gate can run on missing evidence.
- **Cost:** a lost close, or a close written past the cap without the freeze.
- **Fix:**
  - Section 11: "on resume after 6:30, skip to the freeze; after 7:30, write the close table at once from what is gated".
  - A /goal of "the close table printed at >= 6:50 by `date`, or at once on a resume past 7:30".
  - Make n56-reels.sh's floor 6 GB, or a SKIPPED line marks the entry RED.

---

Tools named in the brief, checked: `Tools/night/n46-blind-pairs.cjs` exists (naming limit, #8). `n55k-teach.cjs` is not on
port/stage3 (only on K's branch, 1b996462; it arrives with P0-pick and takes `<telemetry.jsonl> <level>` pairs).
`n55-smoke-steal.sh` exists (it writes to Logs/n55/). n46flow's `--level all` is wrong (#7). `n47human --boss` exists, but its
default arena is wrong (#9). `Tools/night/n53-identity.py table level:rooms` works as written. `Design/levels/n52/` exists
(plans, openings53.json; jobs56.json would duplicate the plans' `job` field, #3). Tag u3-n55 = HEAD = c1a1a273.
