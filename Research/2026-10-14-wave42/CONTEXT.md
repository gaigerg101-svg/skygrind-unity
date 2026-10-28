# CONTEXT: why the nights forget, and the night-context skill (NIGHT42 card X1, G11)

GG, 2026-10-14 ~18:35 (G11, verbatim): "we also need ato create or resrecah skills and think of a skill to help with handoff and
ocntext because we are forgetting things".

The deliverable is `Tools/skills/night-context/` (SKILL.md + `scripts/night-context.cjs`, node only, no npm). It is not installed;
the lead copies the folder to `~/.claude/skills/night-context/` at the close (section 6).

## 1. What we forgot (each read in the files named; nothing here is from memory)

| what | where it was seen | where it should have been carried | how it was lost |
|---|---|---|---|
| THE WICK's fight never killed in a Player since NIGHT37 | NIGHT37-LOG close table: "W1 THE WICK \| SHORT"; NIGHT41-LOG line 268: "wk wick37 n41-foundry: space GREEN, **fight RED**"; committed HANDOFF.md (u3-n41) NIGHT41 block: "Open: ... THE WICK's fight reel never kills it in a Player (since NIGHT37)" | THREADS.md | never in THREADS.md (grep "WICK": 0 hits); then the forge's HANDOFF rewrite (working copy, "after NIGHT41 close + NIGHT42 forge") dropped it: its "Not done" lists aim41 / models41 / select41, A11, the suite, Lesson25TextTests, CODE-FACTS, but not THE WICK. It survives only in NIGHT42.md's `<leftovers>` item 2 (untracked; the lowest-priority list) |
| NIGHT40's 7-word prompt failing Lesson25TextTests unseen | NIGHT40-LOG close row L0: "the batch suite stopped by Claude Code for low memory (no results; targeted Editor tests above instead)"; NIGHT41-LOG line 291: "the 1 failure is NIGHT40's: Lesson25TextTests ... '{grapple} HOOK THE KNEE. {grapple} PULL AWAY.' in n40-span.json is 7 words, the rule is 6" | the NIGHT40 close (a suite SHORT names the tests it did not run) | NIGHT40-LOG line 150's freeze check "Lesson25 9/10 -> 10/10" counts 10 tests, Lesson25Tests' size (NIGHT41-LOG: "Lesson25Tests 10/10"); Lesson25TextTests (6 tests, NIGHT37-LOG line 139) is not in that count. The suite SHORT was recorded without the list of classes left unrun |
| the n40 ids missing from design.maps.list | NIGHT40-LOG lines 55-56: "r1ice / r1loom / r1crown booted SURVIVAL: the n40 ids were not in design.maps.list (the shared commit missed it)"; close row G0 "**missed**" | the shared commit's checklist | a rule learned in w38 (L-062: a campaign level boots only through the list / reel.sh) was not on G0's list; NIGHT41's G0 then did it (its close row: "the four n41 ids in design.maps.list with placeholder files so no reel booted SURVIVAL") |
| aim41 needs a pad frame | NIGHT41-LOG lines 263-264: "no Player proof of a pad lock on a weak point ... aim41 stays OFF"; close row P3 SHORT | THREADS.md + the next prompt | in HANDOFF ("aim41 / models41 / select41 need one Player frame each") and NIGHT42.md's `<leftovers>` item 1, never in THREADS.md and in no NIGHT42 card |
| CODE-FACTS.md not written in the NIGHT42 forge | working HANDOFF.md "Not done": "CODE-FACTS.md was not written (the read-only sweep did not return in the forge box)" | the forge's output | caught: NIGHT42.md told G0 to write it; af1b680a's message: "CODE-FACTS.md (the forge's late sweep, written in G0)" |
| the memory itself: w40 and w41 never harvested | docs/memory/ledger/ ends at wave-39.md; THREADS.md's last paragraph is "w38 + w39"; CORE.md: "State: w39 closed 2026-10-13" | wave-memory harvest at each close | NIGHT40 and NIGHT41 closed without `wave-memory harvest`; so every brief since (the NIGHT42 forge included) read threads two nights stale |

The pattern: the SHORT reaches the night's own log, HANDOFF and at best the next prompt's leftovers (an untracked file, the
list cut first), but not THREADS.md, the only tracked file that ages it.
HANDOFF is rewritten every close, so an item that lives only there survives exactly as long as each rewriter copies it.
THREADS.md also has 29 threads open 10+ nights that were never closed or parked (T-02..T-27 from w18-w21, T-35, T-41, T-42),
against wave-memory's own rule ("A thread aged two waves or more must appear in the next wave's P0 or be parked by the owner").

## 2. What already exists (read on 2026-10-14)

| skill | what it does | why it did not catch these |
|---|---|---|
| wave-memory (`~/.claude/skills/wave-memory/SKILL.md`) | writes docs/memory/ (ledger per wave, LESSONS, OWNER, FEATURES, THREADS, CORE); modes harvest / react / brief / recall / consolidate | prose only, no script: "brief" is a reading list the model performs by hand; nothing runs it at a close, so two harvests were skipped and nothing noticed |
| handoff (`~/.claude/skills/handoff/SKILL.md`, 15 lines) | compacts a conversation into a doc in the OS temp folder | not the repo's HANDOFF.md; no check that open items survive a rewrite |
| context-save / context-restore (gstack, ~1,000 lines each) | git state + decisions into `~/.gstack/.../checkpoints`, restored per branch | per-session checkpoints outside the repo; knows nothing about close tables, THREADS or GG's words |
| wave-forge, game-prompt-playbook | write the next prompt from wave-memory's brief and the playbook | consume the memory; if it is stale they plan from stale threads (the NIGHT42 forge planned with w40 / w41 unharvested) |
| CLAUDE.md `@docs/memory/CORE.md` import | loads CORE (40 lines) into every session | CORE is only as fresh as the last harvest ("State: w39 closed") |

## 3. Anthropic's guidance used (fetched 2026-10-14)

- Skills in Claude Code (https://code.claude.com/docs/en/skills): personal skills live at `~/.claude/skills/<skill-name>/SKILL.md`;
  bundle scripts beside it ("scripts/helper.py (utility script - executed, not loaded)"); "Keep `SKILL.md` under 500 lines"; once
  loaded, "every line is a recurring token cost. State what to do rather than narrating how or why". Frontmatter used: `name`,
  `description`, `argument-hint`, `allowed-tools`. The `!`command`` injection was not used: "A failed command aborts the entire
  skill invocation".
- Skill authoring best practices (https://platform.claude.com/docs/en/agents-and-tools/agent-skills/best-practices): descriptions
  "Always write in third person" and say what + when; utility scripts are "More reliable than generated code" and "Only the
  script's output consumes tokens"; "Solve, don't defer" (the script handles missing files itself); feedback loops "Run validator
  -> fix errors -> repeat" (the close mode's loop); "Avoid Windows-style paths" (the SKILL uses forward slashes).
- Memory (https://code.claude.com/docs/en/memory): CLAUDE.md "target under 200 lines per CLAUDE.md file. Longer files consume more
  context and reduce adherence"; `@path` imports load at launch; auto memory loads "first 200 lines or 25KB". So the brief is not
  imported into CLAUDE.md (it changes every night); it is printed on demand or by a hook.
- Hooks (https://code.claude.com/docs/en/hooks): SessionStart "Runs when Claude Code starts a new session or resumes an existing
  session. Useful for loading development context"; "Claude Code adds a SessionStart hook's plain-text stdout to Claude's
  context". This is the optional wiring in section 6 (the owner's call: it changes settings.json).

## 4. The design

One script, two modes, read-only.
- **brief** (session start; a builder passes `--card <id>`): the stale-memory alarm; GG's newest GG-REQUEST (verbatim, cut) + its
  G-rows + the last three OWNER rows; THREADS.md parsed (the table rows and the per-wave paragraphs: "New: T-nn ...", "closed
  T-nn (...)", "met in wNN", "superseded", "parked", "also open: ...") with the age computed as tonight's night minus the wave first
  seen (the table's age column is stale since w21), 10+ night threads folded into one line, and how many tonight's NIGHTnn.md /
  DECIDEnn.md never name; the newest close table's SHORT rows and SHORT fates (fates that repeat a row are dropped); HANDOFF's open
  items (both HANDOFF shapes); lessons ranked by keyword weight (idf, the scope column counts double) against the card's text.
- **close** (exit 1 on any miss): each SHORT row / fate in THREADS.md and HANDOFF.md, matched by keys in tiers (row names like
  `aim41`; test classes; file names; boss names; then weak keys, a night-local id `A11`, a `D-n`, the SHORT's own word `suite`,
  level ids, which need a second shared word); each HANDOFF open item in THREADS.md; each open item of the committed HANDOFF
  (`--handoff-ref`, the last freeze tag) still in the working HANDOFF or THREADS; each G-row named in the log; the ledger page and
  CORE's state line for the night. Every match prints the key and the line it found, so a false "ok" is visible.

## 5. Tested on tonight's files (real output, pasted unedited)

### 5.1 brief, card X1 (`node Tools/skills/night-context/scripts/night-context.cjs brief --root C:/Users/gaige/dev/SKYGRIND-unity --card X1`, exit 0, 52 lines, 0.13 s)

```
# night-context brief: NIGHT42 (root C:/Users/gaige/dev/SKYGRIND-unity)
!! memory not harvested for w40, w41 (newest ledger wave-39; THREADS has no paragraph for them): run wave-memory harvest before trusting the threads below

## GG, latest words
Research/2026-10-14-wave42/GG-REQUEST.md: GG's request for wave 42 (2026-10-14 ~18:35, after playing n41-r7), verbatim
> the waallride section on level one is awful and relaly difficult and not fun, eneeds to be more balanced, also the wall ride or whe you are pressing a near a wall you fly bcak so fast and it create very awkward movement, make the pulse gauhtlent attack faster .7 seconds faster, also the parry attack timing from the flying enimies is relaly tight and you miss depsite felling like you hit it. shoutgun jumping is too difficult and needs to be eaiser to boost, alo way too much instruction at the tsrat of the level. also the meter or how the pulse or charge attack works field award and the it needs to be polished and more accurate. animation for redical for all weapons needs to spin faster and...
asks: G1 level 1 · G2 movement · G3 gauntlet · G4 parry · G5 shotgun · G6 teaching · G7 HUD · G8 reticle · G9 feel · G10 charge · G11 process · G12 arenas · G13 UI · G14 boss · G15 gauntlet feel
- OWNER 2026-10-13 ~00:10 | "Gameplay feels a little bit better"; THE STAG's "middle walls are a little too short and clunky"; doors "not visible now and you...
- OWNER 2026-10-13 ~14:05 | level 3 "just isn't good ... encourage far more mechanics, have a new arena, and keep the worm AI the same"; the pistol "should i...
- OWNER 2026-10-14 ~18:35 | (n41-r7) level 1 "waallride section ... awful and relaly difficult and not fun"; "pressing a near a wall you fly bcak so fast"; g...

## Open threads aged 2+ nights (THREADS.md, age = 42 - first seen)
- T-46 (w33, 9 nights, parked) the lava rooms' orange share 57-63 % by a hue-only metric (fix the metric or the braziers)
- T-47 (w33, 9 nights, parked) spawns33's 0.6 s hold-still tell off (`tellStagger: false`, bot death loops) and s3b's nearest spawn 5.3 m
- T-48 (w33, 9 nights, parked) n33 v6 walkers pressing the lava edge (~0.87 per step, 6245 guard pushbacks)
- T-49 (w33, 9 nights, parked) ZeroAllocTests HuskBrain.Step 1 allocation in 600 steps (suite 2 batch only; passes alone)
- T-50 (w34, 8 nights) the bend shortens boss fights (THE STAG 10.1 s Player with the bend vs 17.6 s; band 15-22 s; DECIDE34 N-11)
- T-52 (w34, 8 nights) six short rooms (f5, f5b, f9 lava-lesson lanes 13-15 m; f7 corridor; i2 / i5 no nodes)
- T-53 (w34, 8 nights) the bend's length in game seconds never measured in the Player (reels log wall time; the r4 reel ended inside an extended bend)
- T-54 (w34, 8 nights) nobody has PLAYED n34-r4
- T-55 (w39, 3 nights) doors38 and juice38 built but never seen on screen (the reels open doors; no boss-entrance frame looked at): OFF
- T-56 (w39, 3 nights) level 2's skate gap (no skate-only gap on flat ice, E's ice judge): GG's call, iceline38 OFF
- T-57 (w39, 3 nights) level 6's gates (the exam owns every verb): GG's call, finale38 OFF
- T-58 (w39, 3 nights) ride38's a5 deaths (2 -> 9) never investigated
- 29 more open 10+ nights, never closed or parked: T-02 T-03 T-04 T-05 T-06 T-07 T-08 T-09 T-10 T-11 T-12 T-13 T-14 T-15 T-16 T-17 T-18 T-19 T-20 T-21 T-22 T-23 T-24 T-25 T-26 T-27 T-35 T-41 T-42
- (no T-id, w39, 3 nights) p39's heavier / cannon look not seen
- (no T-id, w39, 3 nights) the no-ride texture not seen
- (no T-id, w39, 3 nights) level 5's grapple verbs unproven (no node for the verbs reel)
- (no T-id, w39, 3 nights) the Editor's 13:01 NullReferences not reproduced
- not named in NIGHT42.md / DECIDE42.md (wave-memory: aged 2+ goes in P0 or is parked): 12 of 12 (T-46 T-47 T-48 T-49 T-50 T-52 T-53 T-54 T-55 T-56 T-57 T-58)

## The last close table: SHORT
NIGHT41-LOG.md (10 rows)
- T2 proof tooling: A11 SHORT :: A11: the reel's slide ran over n40-kiln's lava (found), fixed on L1 (26/26), still RED on n41-kiln (the only floored start l...
- J2 boss moment / doors / bars / select: select41 SHORT :: select41 built (boss names, slot 6 REBUILDING; no silhouette art), LevelSelect29 PASS with it on,...
- E6 enemies: models41 SHORT :: models41 built, OFF (the husk's arm unreadable at the lineup's 12 m; no blind re-test)
- P3 pad: SHORT :: aim41 OFF: harness gains (THE LOOM 0.63 -> 3.17, THE ICEBREAKER 0.43 -> 1.89 hits / s, weak pilot) but no Player frame of a pad lock on a ...
- L0 lead: the suite SHORT :: the suite: Core 1095 / 1095, Game A-L 527 / 528 (NIGHT40's 7-word prompt), M / N past the 600 s timeout, P-Z not run;
HANDOFF "HANDOFF: SKYGRIND-unity (2026-10-14 ~19:00, after NIGHT41 close + NIGHT42 forge)", open:
- `Research/2026-10-14-wave42/CODE-FACTS.md` was not written (the read-only sweep did not return in the forge box). NIGHT42.md tells the lead to write it in ...
- NIGHT41 SHORT rows: aim41, models41, select41 built but OFF (never seen on a frame)
- A11 row 13 RED on n41-kiln (a reel limit)
- the suite stopped at Game letter L (batches M and N pass the 600 s tool timeout; P-Z gave no result)
- Pre-existing failure: Lesson25TextTests (n40-span's prompt "{grapple} HOOK THE KNEE. {grapple} PULL AWAY." is 7 words, limit 6)

## Lessons for card X1
- L-054 [lead] Never close a Player you did not identify by its command line and start time: the memory guard can stop a reel script's wrapper while its Play...  {script, memory, player, start, close}
- L-062 [lead, Player checks] `-skygrindMap <id>` alone boots SURVIVAL (an automation flag); a campaign level boots only through `Tools/night/reel.sh` (adds ...  {tool, player, check, lead}
- L-060 [lead, planning] One builder per boss, end to end (arena, AI, pilot, reel, frames), plus a kit builder landing the shared brain first, made four from...  {kill, player, land, lead}
- L-026 [run, tests] When the Editor is down, build Core + SimHarness slices headless (builders in worktrees), but run the full suite the minute it returns: ...  {suite, build, test}
- L-017 [tooling] Prove the reel's input path before judged rounds; Player builds are a cap  {tool, player, build}
- L-031 [tests, ui] A real scene reload is a different code path from a skipped one: the level-select reels never reloaded and passed for 5 nights while 16 o...  {player, check, test}
```

The lesson match is weakest on a process card like X1 (LESSONS.md has few process lessons). On game cards it reads better, e.g.
`--card L1`: L-045 (walk every level), L-061 (re-run gates when the checker gains a move), L-044 (the Editor never loads a
campaign level), L-064, L-041, L-043; `--card M1`: L-064 (a speed cap at one step is not a chain cap), L-042, L-037, L-053.

### 5.2 close, NIGHT41 against the forge's HANDOFF (`close --root C:/Users/gaige/dev/SKYGRIND-unity --night 41 --handoff-ref u3-n41`, exit 1)

```
# night-context close check: NIGHT41 (root C:/Users/gaige/dev/SKYGRIND-unity)
close table: NIGHT41-LOG.md (10 rows, 28 fates)
HANDOFF: "HANDOFF: SKYGRIND-unity (2026-10-14 ~19:00, after NIGHT41 close + NIGHT42 forge)"

## SHORT items -> THREADS.md and HANDOFF.md
!! T2 proof tooling: A11 SHORT [keys: A11, n40-kiln, n41-kiln]
      THREADS: MISSING
      HANDOFF: A11 :: built but OFF (never seen on a frame); A11 row 13 RED on n41-kiln (a reel limit); the suite stopped at Game...
!! J2 boss moment / doors / bars / select: select41 SHORT [keys: select41]
      THREADS: MISSING
      HANDOFF: select41 :: - NIGHT41 SHORT rows: aim41, models41, select41 built but OFF (never seen on a frame); A11 row 13 RED on n4...
!! E6 enemies: models41 SHORT [keys: models41]
      THREADS: MISSING
      HANDOFF: models41 :: - NIGHT41 SHORT rows: aim41, models41, select41 built but OFF (never seen on a frame); A11 row 13 RED on n4...
!! P3 pad: SHORT [keys: aim41]
      THREADS: MISSING
      HANDOFF: aim41 :: - NIGHT41 SHORT rows: aim41, models41, select41 built but OFF (never seen on a frame); A11 row 13 RED on n4...
!! L0 lead: the suite SHORT [keys: suite]
      THREADS: MISSING
      HANDOFF: suite :: 3 RED on n41-kiln (a reel limit); the suite stopped at Game letter L (batches M and N pass the 600 s tool t...

## HANDOFF open items ("- Open:" / "## Not done") -> THREADS.md
!! `Research/2026-10-14-wave42/CODE-FACTS.md` was not written (the read-only sweep did not return in the forge box). NIGHT42.md tells the lead to write it in...
!! NIGHT41 SHORT rows: aim41, models41, select41 built but OFF (never seen on a frame)
!! A11 row 13 RED on n41-kiln (a reel limit)
!! the suite stopped at Game letter L (batches M and N pass the 600 s tool timeout; P-Z gave no result)
!! Pre-existing failure: Lesson25TextTests (n40-span's prompt "{grapple} HOOK THE KNEE. {grapple} PULL AWAY." is 7 words, limit 6)

## Dropped by a HANDOFF rewrite (git u3-n41:HANDOFF.md open items vs the working HANDOFF + THREADS)
!! THE WICK's fight reel never kills it in a Player (since NIGHT37)

## GG asks -> a fate in the log
(Research/2026-10-14-wave41/GG-REQUEST.md has no "| Gn |" ask table: nothing to check)

## Memory
!! docs/memory/ledger/wave-41.md MISSING (run wave-memory harvest 41)
!! closed nights with no ledger and no THREADS paragraph: w40, w41
!! CORE.md state line names w41: State: w39 closed 2026-10-13 (the feel night): the pistol's hold charges (p39), the ride capped at 16 m/s (ride39), r...

RESULT: 14 missing (fix THREADS.md / HANDOFF.md / the harvest, then re-run)
```

It finds the examples of section 1 by itself: THE WICK (dropped by the rewrite), aim41 (in HANDOFF, never in THREADS), the
Lesson25TextTests failure (in HANDOFF, never in THREADS), and the two skipped harvests. Run with no `--night` tonight, it starts
`!! MISSING: NIGHT42-LOG.md has no close table yet (checked NIGHT41's instead)` (exit 1), which is right before the close.

### 5.3 The checker tested both ways (L-034: a checker that always passes is worse than none)

A scratch copy of the files (docs/memory/, NIGHT41-LOG.md, the working HANDOFF.md, the wave41 GG-REQUEST; scratchpad `fixture/`):
before the plant `RESULT: 13 missing`, exit 1 (14 on the real tree counts the dropped WICK, which needs git). Then
`fixture-plant.cjs` appended one "w40 + w41" THREADS paragraph naming each item (A11 on n41-kiln, aim41 / models41 / select41, the
suite, Lesson25TextTests, CODE-FACTS.md, THE WICK, D-3), wrote ledger/wave-41.md and set CORE's state to w41. After: every row
`ok`, `RESULT: every SHORT and open item landed`, exit 0. The plant's first run still showed 2 misses (CODE-FACTS.md and
Lesson25TextTests: the key regexes missed a path prefix and a digit in a class name); both fixed in 15504a26, then 0.

## 6. For the lead (the close)

1. Install: copy `Tools/skills/night-context/` to `~/.claude/skills/night-context/` (SKILL.md + scripts/night-context.cjs).
2. At tonight's close, before declaring it: `wave-memory harvest 40` and `41` (and 42), then
   `node ~/.claude/skills/night-context/scripts/night-context.cjs close --handoff-ref u3-n41` until RESULT is clean or each `!!`
   is a SHORT row with its reason. Put THE WICK back into THREADS.md (it is in neither HANDOFF nor THREADS now).
3. Optional, the owner's call (it edits settings.json): a SessionStart hook running `... night-context.cjs brief` puts the brief
   into every session.
4. Builders: add one line to the next brief: "run `night-context.cjs brief --root <main checkout> --card <id>` first".

## 7. Limits (measured or read, not guessed)

- Lesson matching is keyword overlap (idf): it ranks the right lessons for level / movement cards (5.1) and weakly for process
  cards; it never reads the evidence column.
- "Landed" is a key match: an item counted `ok` because its key appears in an unrelated line is possible; each `ok` prints the
  key and the line so the reader can see it.
- THREADS.md's prose is parsed by its current phrasing ("New:", "closed", "met in", "superseded", "parked", "also open:"); a new
  phrasing for closing a thread is read as open (it shows as a stale thread, the safe side).
- The GG-ask check needs a `| Gn |` table (wave42 has one; wave41 has none).
