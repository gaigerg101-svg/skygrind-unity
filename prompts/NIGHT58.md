# NIGHT58.md: FINISH LEVEL 3, START LEVEL 4, LAND THE HITS. Iterate on what NIGHT57 shipped and missed (lead + three builders + batched critics; at least 2 h, hard cap 2 h 15 min)

GG, 2026-10-21 ~13:45, verbatim (this outranks everything below it):
> need to build for next 2 hours what else can be built and itterated upon, think plan and create goal prompt 2 hours 2.25 hard cap

**GG will not answer during the run.** Decide every call in `DECIDE58.md` (untracked) with its reason and the alternative rejected. Base:
`port/stage3` HEAD; it must hold tag `u3-n57` with kiln57, wick55, grade55 and colour56 ON in the repo config (`git tag -l u3-n57`;
design.n57.kiln57.enabled in Design/unity-config.json). If either is missing, the NIGHT57 close
was cut short: finish it first (DECIDE57 D-17's four rows via `node Tools/night/n57-freeze-rows.cjs`, gates, commit, tag u3-n57) and log
it as D-5. t0 = the first `date` (the NIGHT57 finish counts inside the box).
Print `date` and the elapsed time at the start of every turn. Rules: CLAUDE.md. The shipping rule: NIGHT57.md section 0 (unchanged;
read it by its absolute path `C:\Users\gaige\dev\SKYGRIND-unity\NIGHT57.md`). Builder brief: Research/2026-10-21-wave57/builder-brief.md
with the n58 names (copy it to Research/2026-10-21-wave58/builder-brief.md in the shared commit).

## 0. Why this slate (NIGHT57-LOG.md close table; THREADS T-135, T-139; GAPS.md 1.1)
- Level 3 (n57-kiln, ON) has two measured weak spots: the harness had to move the bot on 15 / 17 times (n52-kiln 3 / 4: exits are hard
  to find; door dwell 5.7 -> 18.8 s), and at the door only 3 / 6 (kb3c) and 2 / 5 (kb3d) enemies are in view.
- Level 4 THE SPAN still has 3 combat rooms and level 1's roster (husk, swarmer, spitter): the grapple level adds no role (GAPS.md 1.1).
- kill55 misses because the frame after a hit is 97 % white flash, so the marks never show; press57 missed because the bot's "still"
  steps are 31-73 % the bot blocked with the stick pushed, which the MAIN counted as standing still.
- rim56 75 % (foundry 29 %) and yellow56 (one real frame on foundry a5) are each one fix from their numbers.
**NIGHT57's timings (measured, NIGHT57-LOG.md):** a Player build 1-3 min; verbs37 x6 7-8 min; a357 x10 ~10 min; a critic 25-47 min;
the smoke 4 min; the suite Core 12 min + Game A-L 11 min + Game N-Z by class ~35 min; builder L ~35 min per new room once the tools
existed; reel frames ~10 GB per set (prune after every batch: L-104).

## 1. Scope
- **In:** n57-kiln's exits and in-view (level data), level 4 as n58-span (new combat rooms from cards, one new role), kill55, press57's
  measure, rim56, yellow56. **Out:** enemy looks / new kinds (GG's), audio, level 6, the UI's style, the wall ride's speed, the worm's AI,
  new verbs, new modes. Game M's suite hang runs only after the tag (it wedged the Editor once).

## 2. Clock (t0 = your first `date`; close table no earlier than 1:55; hard cap 2:15)
| when | who | what |
|---|---|---|
| 0:00-0:12 | lead | A0: `bash Tools/unity-loop.sh status`, git, `df` (>= 20 GB; prune Logs/feel/n57-* with n57-prune.sh if not); the shared commit (section 3); spawn L, G, V one at a time |
| 0:12-1:25 | L, G, V | their entries, one slice at a time, commit every slice; builders stop at 1:25 |
| ~0:55 | lead | pick, `bash Tools/night/n58-gates.sh --flow`, build n58-r1, rows on in the copy, reels (prune after each batch), ONE batched critic |
| ~1:00-1:25 | lead | while the r1 critic reads: the suite, Core only + the new test classes (`n41-suite.sh n58 Core`; `... n58 Game N class` for N58SharedTests) |
| 1:25-1:45 | lead | pick the last commits, gates, build `n58-freeze` from HEAD, verbs37 x5 (+ n57-kiln and n58-span), the smoke (`n56-smoke-steal.sh`). NO second critic (NIGHT57: 25-47 min): a row without an r1 verdict stays OFF |
| 1:45-2:15 | lead | config rows ON by the rule (`node Tools/night/n58-freeze-rows.cjs`), gates, commit, tag `u3-n58`, the close (section 6; NIGHT57's close took ~30 min) |

## 3. The shared commit (lead, <= 12 min; copy NIGHT57's)
- `Tools/night/n58-shared.cjs` from n57-shared.cjs (its guard "the last design key" is now `n57`): `design.n58.span58` (a LEVEL row:
  levelFrom n52-span, levelTo n58-span; the maps.list row and the placeholder level, a byte copy of n52-span) from `Logs/n58/rows-def.json`;
  reel hooks `FeelReel.N58.cs` (a158 L, a258 G, a358 V); `N58SharedTests`; `Tools/night/n58-gates.sh` (fingerprints plain / --off57 /
  --off58; `--flow` against a new `Logs/n58/flow-base.txt`). No new sim row: kill55 / press57 / rim56 / yellow56 keep their rows.
  n58-shared.cjs needs more than NIGHT57's guard change (the red team): its "exactly one level row, kiln57" assert becomes span58, and
  n58-span needs an `N58LevelFrom` (SurvivalSim.N57.cs maps only "n57-" ids) if any raw-id system keys on span (grep `n52-span` in
  Assets/Scripts/Game first). Copy `n57-freeze-rows.cjs` to `n58-freeze-rows.cjs` with "NIGHT58" in its reason prefix.
- **L0 has no n58 off switch** (it edits n57-kiln's data behind kiln57, already ON): so the `--flow` gate's level list leaves n57-kiln
  OUT (`--level n52-foundry,n52-iceline,n52-span,n52-spire`), and L0 carries its own before / after (L0's MAIN). If L0 has no gate verdict
  by the freeze, `git revert` its commits so the ON level 3 stays the one gate-r3b passed.
- Baselines at HEAD, every n58 row off, BEFORE the builders measure (background, while you spawn; ~5 min each):
  `dotnet run --project Tools/SimHarness -c Release -- n46flow --level n52-foundry,n52-iceline,n52-span,n52-spire --seeds 10 --out
  Logs/n58/flow-base.txt`, the same with `--first 11 --out Logs/n58/flow-base-B.txt`, and n57-kiln alone (`--level n57-kiln`, both seed
  sets: `Logs/n58/kiln-base.txt`, `kiln-base-B.txt`) for L0's before.
- `Logs/n58/slate.json` (force-add): the entries below with MAIN and GUARDS written BEFORE any number is seen.

## 4. The slate (contracts; MAIN beats the baseline beyond noise, GUARDS within band, nothing lost)

### L. LEVELS (builder L; L0 first)
- **L0 n57-kiln's exits and in view (<= 35 min; level data behind kiln57, in gen-n57-kiln.cjs):** MAIN = the harness's "put at the next
  room" count on n57-kiln (15 on seeds 1-10, 17 on 11-20 at NIGHT57: gate-r3b) down to <= 3 per 10-seed set, and in view at the door
  kb3c >= 4 / 6, kb3d >= 4 / 5 (a157). The exits' cause: `n46flow --level n57-kiln --rooms kb3c --trace` (read Harness.N46.Flow.cs's
  options first; `hold52 --dwelltrace` needs `--level` and `--room` and samples the entry door, not the exit).
  GUARDS = clear 150-240 s; stall <= 21.5 % + band; deaths <= 3 per 10 walks + band; every new room still passes n57-l-room.py / identity /
  contract 14/14 / walk 10/10. Cause first: print where the bot stalls by the door (the `hold52` command's `--dwelltrace`, Tools/SimHarness/
  Harness.N52.Hold.cs: read its arguments first; n46flow has no such flag) before cutting anything.
- **L1 level 4 as n58-span: the card, the generator and ONE new combat room (<= 45 min; span58 is expected to stay OFF tonight):** a NEW
  combat room in place of a link (span's combat rooms are p1 / p3 / p5; its links p2, p4, o1, o2: name the slot on the card; p2 or p4
  sits between combat rooms), from an A-9 card (docs/ROOMS.md). gen-n57-kiln.cjs is 556 lines hard-coded to kiln: write `gen-n58-span.cjs`
  from `Tools/night/gen-n52-span.cjs` (span's own generator) with NIGHT57's slots idea, and make `n58-l-room.py` / `n58-l-contract.sh`
  (n57-l-room.py and n57-l-contract.sh hard-code 'kiln'). The span is the grapple's level: one NEW role for level 4 (a flyer over the
  gaps: the drone, or the charger on a bridge), introduced alone (R-2, R-3). MAIN = combat rooms 3 -> 4 with the new one passing its
  checks; the level's kinds 3 -> 4. GUARDS = span stall <= 13.7 % + band, deaths in band (0), verbs37 on n58-span 31 / 31 (grapple rows
  17-23). span58 turns ON only by the rule with a critic verdict; otherwise OFF with the room kept for the next night (the likely case).

### G. LAND THE HITS (builder G)
- **G1 kill55's marks after the flash:** the frame after a kill is 97 % white (the kill flash), so a mark has to show in the frames AFTER
  the flash or the flash must leave the mark's cells. MAIN (measured): MARKS at the first frame after the flash ends >= 5 % of the body
  AND the chain ladder ok (a355). GUARD: a354 jitter <= 0.191 WITH kill55 firing in the a354 run (NIGHT57's a354 never fired kill55: make
  the reel fire it, or say "not evidenced"); no flash budget broken (C-6: <= 3 a second).
- **G2 press57's measure (no code change to the row first):** define "chosen stillness" = steps where the pilot's stick is NOT pushing and
  the speed < 0.5 m/s (written into Logs/n58/slate.json before measuring). MAIN = that share on foundry / iceline / kiln down >= 5 points
  with press57 on vs off; GUARDS as NIGHT57 (deaths in band, clear <= +10 %, hits taken while moving not up). If the MAIN still misses, it
  stays OFF with the number.
- **G3 (leftover):** tokens56's idle on iceline (19.3 % of enemies idle > 20 % of their time).

### V. COLOUR AND READABILITY (builder V)
- **V1 rim56 on foundry:** enemies above the world's p90 V / S: 75 % overall, foundry 29 % (want >= 90 %). Cause first (foundry's lit walls
  vs the rim's gain). MAIN overall >= 90 %; GUARDS world over the tells <= 3 / 36, greyscale >= 25 / 36, no whole-frame tint.
- **V2 yellow56 on foundry a5:** the one real non-parry yellow frame. MAIN non-parry yellow <= 0.5 % on every a357 / verbs37 frame
  (the meter's parry-orb fringe excluded with a planted case, not by hand); GUARDS: parry tells' yellow kept.
- **V3 the exits lit (A-6, for L0's exits):** on n57-kiln, each new room's exit door lit when the room clears (data or a view row inside
  kiln57's level data; not a whole-screen effect). MAIN: the exit's luma vs the room's median at the clear frame (a157 / a452) >= 1.5x;
  GUARD: nothing else in the room brighter than before. Coordinate with L: L owns the plan; V gives L the lamp entries or owns a NEW view file.

### Seeded leftovers (only after every entry above has had its first attempt)
1. A second new span room (L). 2. tokens56 idle on iceline (G). 3. spire f1550's dark door (V). 4. After the tag: Game M per class (lead).

## 5. The gate
One batched critic per build (Research/2026-10-21-wave57/critic-gate.md, unchanged; taste decides nothing: L-105). <= 30 images. The
lead flips `passes` by NIGHT57.md section 0's rule. A builder that changes a checker its row is read by gets a critic review of the diff
(L-106).

## 6. The close (by 1:45; the close table no earlier than 1:55; hard cap 2:15)
NIGHT58-LOG.md close table (every entry: ON / OFF / SHORT, MAIN and GUARDS before / after, evidence, off switch), NIGHT58-REPORT.md for
GG (plain words: what changed in level 3 and level 4, what is ON, the PLAY notes), HANDOFF.md, DECIDE58.md, wave-memory harvest 58
(ledger, THREADS under 60 lines), the forge experiment scored. Commit with explicit paths; tag `u3-n58`. Nothing pushed.

## 7. The forge experiment
Forge experiment: a 2 h run with exactly two builds (r1 + freeze) and builders that stop at 1:25. Hypothesis: a short run ships at least
3 rows ON without a usage-limit stop. Measure: rows ON / rows attempted, and the minutes from the last builder commit to the tag.

## 8. Builder spawn template
```
You are builder <L|G|V> on NIGHT58 of SKYGRIND (Unity 6000.3.7f1, URP; a pad-first movement shooter at dusk), C:\Users\gaige\dev\SKYGRIND-unity.
t0 <time>; you stop at 1:25 after t0. Objective: <one sentence>. Read first: CLAUDE.md, C:\Users\gaige\dev\SKYGRIND-unity\NIGHT58.md (untracked)
sections 0, 1, 4, 5, NIGHT57.md section 0 (the shipping rule), Research/2026-10-21-wave58/builder-brief.md, NIGHT57-LOG.md's close table.
Your entries: <ids with MAIN / GUARDS>. Your files: <paths>. You work in your own git worktree; the lead integrates by cherry-pick.
You may not: run the Unity Editor, a build, a Player, a reel or run_tests, push, merge, or commit outside your branch.
After EVERY commit: `dotnet run --project Tools/SimHarness -c Release -- fingerprint` (and --off57 / --off58) prints EQUAL; after any
Assets/Scripts/Core commit: CoreHarness "0 failing, 0 mismatches". Commit every slice (explicit paths + .meta; trailer
Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>). Report when a slice is ready for r1 (~0:50) and at the end: every sha, MAIN /
GUARDS before / after, the reel specs, the frames the critic must see. Last line: N58-<letter>: built <ids> · commits <shas>.
```

## 9. Hard rules (CLAUDE.md restated) and if something goes wrong
- No push / merge / master; explicit paths; never stage GG's four files or the TMP font assets; restore the TMP fallback asset before
  commits. Only the lead drives the Editor, builds, Players, reels, the suite and git on port/stage3. Never kill the Editor.
- Movement exact: golden 0 mismatches, replays exact, fingerprints EQUAL with tonight's rows off. Numbers from config; Mulberry32 only.
- Disk: prune reel frames after every batch (n57-prune.sh, n58-* tags: copy it to n58-prune.sh); a reel set that prints no verdict line
  is a refusal, not a pass (L-104).
- The Editor does not answer: never kill it; batch-mode builds from SKYGRIND-unity-batch; say so in the report.
- The usage limit: everything is committed; at the resume after 1:45, go straight to the close.

## Kickoff (paste into a fresh Claude Code session in `C:\Users\gaige\dev\SKYGRIND-unity`)
```
Read CLAUDE.md, HANDOFF.md, NIGHT58.md, DECIDE58.md and NIGHT57.md section 0, and run NIGHT58 exactly as written as the lead. At least
2 hours, hard cap 2 h 15 min from your first `date`. I will not answer: decide every call in DECIDE58.md and keep going. Start with `date`.
```

## /goal (paste after the kickoff)
```
/goal Met when this conversation shows NIGHT58-LOG.md's close table printed in full (every slate entry ON, OFF or SHORT with MAIN and GUARD numbers before / after or "n/a" and the evidence path) and `git tag -l u3-n58` printing exactly u3-n58; or when a printed `date` shows 2 h 15 min since the first `date` (start the close by 1:45 whatever is unfinished). Print `date` and elapsed time at the start of every turn. Run NIGHT58 nonstop as the lead; never ask me; decide every call in DECIDE58.md. Order: A0, the shared commit (span58 level row, hooks, baselines), builders L (n57-kiln exits + in view, then level 4's new rooms), G (kill55 marks after the flash, press57's chosen-stillness measure), V (rim56, yellow56, the exits lit) one at a time; build r1 at ~0:55 with one batched critic, the freeze at ~1:25 with no second critic (a row without an r1 verdict stays OFF); prune reel frames after every batch. Ship rows by NIGHT57.md section 0's rule. Commit every slice with explicit paths. Close with the close table, NIGHT58-REPORT.md, HANDOFF, DECIDE58, wave-memory harvest, and tag u3-n58.
```
