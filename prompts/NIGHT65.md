# NIGHT65.md: THE FLIGHT FOUNDRY (7 hours; 6.5 minimum; 7:00 hard stop): level 1's fight rooms rebuilt from scratch, in played rounds

GG, 2026-10-23 ~13:08, verbatim: "ok we need to plan detaile dbuild prompt for next 7 hours 6.5 min 7 hard stop. follow same methods on
finding what to focus on, dont run into corners or repeat past mistakes, i wnat real progress and itterations and not just small polishes".
His room brief (2026-10-13, the most specific on record): "I don't mind if they're wide open arenas with a roof ... there's a time and a place
for the tight rooms and keep them"; "one might encourage the dash jump another 1 encourage wall riding"; "they can't be too exploitable";
"you should be flying through and ... killing the enemies before you enter the next room in a stylish fashionable way". And: "every single
level needs a complete arena overhaul completely from scratch so don't preserve anything" (2026-10-09); "if it arena related please dont just
randomly plce stuff it needs intension and personality ... cut fluff" (2026-10-17); level 1's "waallride section ... awful and relaly
difficult and not fun" (2026-10-14); "decorative elements ... lack strong color direction ... don't change the core mechanics" (2026-10-20).

Written by the forge FORGE65 (Research/2026-10-23-forge65/: DECISIONS D-1..D-11, history.md, research-rooms.md, code-scout.md,
research-run-methods.md, redteam.md (15 findings, all applied), GOAL.txt, slate-seed.json, builder-brief.md). Order of authority: the org
rules, GG's words, CLAUDE.md, this file, HANDOFF.md, the logs. **Read this whole file first and again after any compaction or resume.** The
night ends when section 9's close is done and the last message is `NIGHT65 COMPLETE`, printed at 6:30 or later and before 7:00.

## Where we are, honestly
- **The rooms are the game's worst thing**, by gg-proxy on the shipped frames three nights running (Logs/n64/gg-proxy.md: "big empty boxes",
  8 of 10 level frames). Level 1: 11 rooms with **68 boxes, 8 props, 7 pads, 0 grapple nodes**; NIGHT33's kiln had 332 dressing props and the
  kit is still in the code (code-scout.md sections 1-2).
- **Fifteen nights touched the rooms; every number went green while every human-proxy review said "none would satisfy GG"** (history.md
  section 2). The corners: the walk bot or the contract as the judge; cuts-only re-cuts; stills judges in GG's place; a slot too small for its
  card; a slate of small polishes. **Tonight's rules avoid each one** (section 0 rule 10), and tonight's MAIN is measured play, not counts.
- **GG has not played since 2026-10-20.** The night ends with a play card (level 1 new vs old, five minutes) and eye63's blind X / Y.
- NIGHT64 shipped order64, fightLine64 and bossColours63 (tag u3-n64; play build Builds/n64-freeze).

## The target, said plainly
**n65-foundry**: level 1 rebuilt for flight behind `design.n65.foundry65` (levelFrom n52-foundry -> levelTo n65-foundry; off: `-skygrindOff65`),
**six fight-room slots** rebuilt from cards, each through three played rounds: **a2 KESTREL, a3 RIDE, a5 CRUCIBLE** (builder R), **b1 THE YARD,
a8 PIERCE, a4 PARRY** (builder S). A slot keeps the n52 room until its new room PASSes (Design/levels/n65/slots65.json, the lead's). Kept bit for
bit: a1 INTAKE, b2 THE GANTRY (verbs37's 25-30 m wall and 14 m lane, the fall return), a6, a7, a9 (THE WICK). **Done well = four or more rooms
PASS, nothing lost (verbs37 n65-foundry 27 / 27), two play builds for GG (the verified freeze, and a copy with every latest room).** Not tonight:
other levels, movement, bosses, the links (leftover).

## 0. Rules at a glance (all of CLAUDE.md's hard rules apply; the guard hook enforces 1-4)
1. **The clock** (`node Tools/night/n65-clock.cjs`, every turn): BUILD 0:00-6:15; FREEZE 6:15-7:00 = the close (only builds, reel sets and the
   smoke tagged `n65-freeze`; no test run; no new agent); CAP 7:00 (hard stop). **The close starts between 6:00 and 6:15**; the final message
   at 6:30 or later. Never restart t0; never edit Logs/clock.json (only `stop` at the end).
2. **Disk:** the night needs **20 GB free at t0** (it writes about 5 GB; the guard refuses builds, reels and smokes under 12 GB). The red set
   runs through a junction (`cmd //c mklink /J Builds\n65-red Builds\n64-freeze`; removed with `cmd //c rmdir Builds\n65-red`, never `rm -rf`);
   keep only the newest round build (delete this night's older ones); prune reel frames after every set. Older nights' builds only with GG's
   yes in DECIDE65.
3. **Docs and code only with Write / Edit** (rule 9): no heredocs, `node -e` / `python -c` writes, `sed -i`, redirects into docs or code.
4. **Git and the machine:** explicit paths plus `.meta`; never GG's four files or the four TMP fonts; no push, merge, rebase, master, `reset
   --hard`, `stash`, `commit -a` / `--amend`, `branch -D`, `unity test`, killing Unity.exe, a background `&` ending an `&&` chain.
5. **Memory and agents:** one Player at a time; **at most four subagents alive: builders T, R, S and one review slot** (in turn: the card
   critic, the reviewers, the spotter, the GATE critics, gg-proxy). **Read commit free in the same command before every build, reel set, smoke
   or gate run** (`powershell -NoProfile -Command '$o=Get-CimInstance Win32_OperatingSystem; [int]($o.FreeVirtualMemory/1MB)'`): under 15 GB,
   hold and log the top processes (another session's jobs are not ours to stop, L-146).
6. **Background, not foreground:** every reel set, gate run, build, smoke, test batch, critic, reviewer and gg-proxy in the background.
   Never two dotnet runs in one tree at once.
7. **Every output a verdict cites is a file under Logs/n65/** the moment it prints, with the command, the sha and the time at its top (L-149).
8. **A PASS binds to its sha, and the slots are the lead's:** at a room's PASS the lead sets passSha and passFiles (its room file, card and
   evidence; notes left out); `node Tools/night/n65-slot-set.cjs <room> new` writes Design/levels/n65/slots65.json only if the room file is
   unchanged since passSha. Round builds use `--slots=latest` (every room's newest file); the shipped level uses slots65.json. The level row
   flips only by its own slice LV (its own critic and verdict-LV.md), never by a room's PASS. A change after a PASS is GATEd again or not shipped.
9. **Reviewers see evidence, not the story:** frames, plan pictures, the measured files, the card and the diff; no builder notes; at most 20
   images each (contact sheets: `python -I Tools/night/n63-sheet.py`).
10. **Never repeat the corners** (history.md section 3): the walk bot and the checkers are guards, never the verdict; no cuts-only round (each
    adds or moves a piece with a job); no stills judge in GG's place (the play builds are his); no lava pit without a way out and a reason; no
    card whose design does not fit its slot's rect (the card gate rejects it, L-112); no fix after a PASS unreviewed; no shared scratch folder;
    no doneWhen the old room already fails (relative to its red, L-103). **A reviewer's BLOCKING finding must name a piece, a rule or a GG
    quote, and a fix one round can make;** after round 3 open findings go to the GATE critic as evidence, not a veto.
11. **The usage limit** (L-003): commit every step; resume agents by SendMessage (their ids in the log); the report committed by 3:00 and by
    5:30; after any stop past 6:00, the minimum close (section 7) first.

## 1. Bearings (steps 1-3 every session; 4-9 the first session only)
1. First session: `node Tools/night/n65-clock.cjs start`. Every session: the clock, `git status --short`, `git log --oneline -5`; **HANDOFF.md's
   "Verify on arrival" table**, each line logged; `claude --version`. t0 at the top of NIGHT65-LOG.md (tracked). The slate (`node
   Tools/night/n65-slate.cjs`), DECIDE65.md (untracked; one D-id per call made for GG).
2. **The Editor:** `timeout 60 bash Tools/unity-loop.sh status` ("playMode": "stopped"), then `timeout 45 unity command set_autotick --enable
   true --interval_ms 16 --caller plugin --skill unity-cli`. Commit free (rule 5) and `df -BG /c | tail -1`.
3. **The branch:** **A** (20 GB disk or more, the Editor answers, 20 GB commit free): the plan. **A-lean** (16-20 GB disk, or 15-20 GB commit):
   builders T and R only; S's rooms become leftovers; one round build kept. **C** (under 16 GB disk, or the Editor deaf twice 10 minutes apart):
   no Player; rooms built and judged sim-side, the report opens with it.
4. `mkdir -p Logs/n65 && cp Research/2026-10-23-forge65/slate-seed.json Logs/n65/slate.json`; Logs/n65/rc.json by the Write tool:
   `{"rc": [{"name": "rc0", "build": "Builds/n64-freeze", "sha": "<git rev-parse u3-n64>", "verified": "2026-10-23 13:07:12: verbs37 x5
   27/29/31/31/31, the smoke DONE, flow GREEN"}]}`. Then `bash Tools/night/n65-gates.sh --flow-base` (background, ~10 min).
5. **At 0:05 spawn the three builders** in the background, each in its own worktree with `Research/2026-10-23-forge65/builder-brief.md`, told
   t0, their deadlines and the sha to branch from (HEAD now: T1 touches no A1 file; R and S write cards first). Log each agent's id.
6. **The sim red, by the lead at 0:05-0:25 (no Player; files under Logs/n65/red/<id>/):** for each slot: `python -I Tools/night/n59-l-room.py
   n52-foundry room <id>`; `n46flow --level n52-foundry --rooms <id>` with seeds 1-10 and 11-20 (the band, as n59-l-contract.sh runs them);
   `hold52 --level n52-foundry --room <id> --pads`; `n53ridekill ride --level Design/levels/n52-foundry.json --room a3`; the plan PNG
   (`bash Tools/night/n52-svg-png.sh Design/levels/n52/foundry.plan.json <id> <png>`). Send each builder its rooms' red paths at once.
7. **A1 (the lead, 0:25-1:00; NIGHT57's shared commit as the model, code-scout.md section 5):** `design.n65` with a `_why` and the row
   `foundry65 {levelFrom: "n52-foundry", levelTo: "n65-foundry", enabled: false}` in both config copies by a row script modelled on
   Tools/night/n64-a1-rows.cjs; **the maps.list entry for n65-foundry** (an unlisted id boots the arena, SurvivalGame.Maps.cs:66-70); **every
   literal "n52-foundry" in the config and the code gets "n65-foundry" beside it** (`grep -rn "n52-foundry" Design/unity-config.json
   Assets/Scripts`: at least maps.list, look49's levels list (Design/unity-config.json:8168, PostFx.cs:491-520) and Look52TierLevels
   (UnityTuning.U52.cs:150), each with a test); the placeholder Design/levels/n65-foundry.json + its StreamingAssets copy (+ .meta): n52-foundry
   with its names changed (T1's `--check` output once it lands); `N65SharedTests` (the row off, the order unchanged, maps.list has the id);
   after the flow base, `bash Tools/night/n65-gates.sh` GREEN; `node Tools/night/n65-freeze-rows.cjs --selftest`; commit; message the
   builders A1's sha.
8. **The red set on rc0** (after A1, background): `bash Tools/night/n65-reels.sh n65-red fr,n52-foundry,a157,-skygrindSpawnShots60
   fv,n52-foundry,verbs37` through the junction (rule 2). a157 frames only a3, a5 and b1 (frames52.entry); every slot's red frame for its SPOT
   comes from tour65 on rc0 once T2 lands (its fallback pose is frames36.mid, which every room has). Until then the cards use
   Logs/feel/n64-freeze/vf (37 kept frames) and the plan PNGs.
9. **At 0:45:** rewrite HANDOFF.md's NIGHT65 section (the anchor sha, rc.json's last line, the slate line, the next step); refresh at every RC.

## 2. The slate and the evidence
Logs/n65/slate.json (NIGHT64's fields plus passSha / passFiles). Only the lead edits it (`n65-slate-set.cjs`, `n65-slate-add.cjs`) and commits
it with `git add -f`. A doneWhen is written before any measurement and never loosened. A room slice's row is "slot <id> of n65.foundry65" (so
no room's PASS can flip the level row). **A room's evidence per round** (Research/2026-10-23-wave65/<id>/r<n>/ by the builder; Logs/n65/ by
the lead): the sim files (room-check, flow with both seed sets, hold52 pads, the ride pilot on ride cards, the plan PNG), the Player frames
(tour65's poses and the verb line's sequence; a157 where it frames; verbs37 n65-foundry), the two reviews, the builder's change list (what
each added piece is for). **The SPOT** at the GATE: the room's red frame (tour65 on rc0, the same pose) against the GATE build's; a fresh
skygrind-spotter with the two paths only; the SPOT must name **the piece that serves the card's verb**, not just "a different room".

## 3. Order of work and owners
- **T (tools, from 0:05, branched from HEAD):** **T1 by 1:00:** the assembler `Tools/night/gen-n65-foundry.cjs`: the room file
  `Design/levels/n65/rooms/<id>.json` holds the whole room row (every key), its zone-keyed boxes / ramps / pads / pits, its room-keyed lights /
  props, its grapple anchors and its teach35 entries; the assembler prints a per-key keep / replace / drop-with-reason table and refuses an
  unlisted key; `--slots=latest | slots65.json`; levelgen `--walk`; then the finish step of gen-n52-foundry.cjs (a9's n47 / n48 keys: wick47,
  frames47, climbTop48; data.name "n46-foundry": gen-n52-foundry.cjs:7-10, 429-473) and `--finish` to both copies; `--check`: all slots old =
  n52-foundry apart from names (reuse gen-n57-kiln.cjs:12, 60, 80-93); `Tools/night/n65-slot-set.cjs` (writes slots65.json after the passSha
  check; refuses a changed room file: a planted case); **the bot opt-out**: a rebuilt row carries `bot43: false` and Walk36 / Walk38Foundry /
  Walk43Foundry skip their id-keyed scripts for it (the generic autopilot runs), with a test. **T2 by 2:00:** tour65 (each room file's
  `frames65` poses framed with wave 1 live, one Player run per level, a proof line per pose; a `line65` (the card's verb line: waypoints) framed
  as a sequence of 8 frames; fallback pose frames36.mid; a planted pose). **Then T's leftovers** (section 3's list).
- **R (a2 KESTREL, a3 RIDE, a5 CRUCIBLE) and S (b1 THE YARD, a8 PIERCE, a4 PARRY):** 0:05-0:50 a card per room
  (Research/2026-10-23-wave65/cards/<id>.md: the one verb it forces and why it cannot be skipped or exploited; the design measured against the
  slot's rect first (a2 14 x 20 m, a8 14 x 22, a4 8 x 19 are tight rooms: GG "keep them"; a3 20 x 26, a5 and b1 20 x 32 open arenas with a
  roof); at most three planes 1 m apart; every piece's job; enemy climb routes (ramps or stairs); spawns in front; the entry's view and
  drop; the lit exit; dressing (Kit33Props / PropsView29) with a job and a role colour each; the teach beat kept (verb36, lessons, teach35);
  frames65 poses and line65; **the card's own MAIN numbers against its red**: the n46flow air % and STYLE it aims at, the pad touches, the ride
  pilot on a ride card). **The card gate at 0:50-1:00** (one fresh critic, all six cards, research-rooms.md's rules and GG's words; it rejects
  a card whose design does not fit its rect). Then rounds.
- **The lead:** bearings, the sim red, A1, the red set, picks, assembly, builds, reels, reviews, GATEs, slots, RCs, the play builds, the report.
- **Leftovers, ranked (builds, not polishes; whenever a list waits; never past 5:15):** (1) the links between the rebuilt rooms (gg-proxy's
  "one straight green corridor"), as slots with cards (T); (2) the frame-pacing bench switch (`-skygrindVsync`, a synced and an unsynced bench:
  M1's open half, T-152) (T); (3) T-155 one instruction on screen at a time across the centre line, the bar and the world labels. **Not
  tonight:** other levels, the movement's feel, the bosses' shapes, MapChoiceTests.

## 4. One round (three per room) and the GATE
1. **The builder** edits only its room files (and its card when a review asks): each change adds or moves a piece with a job. The sim loop in
   its own tree (about 5 minutes, no gates unless it changed C#): `node Tools/night/gen-n65-foundry.cjs --slots=latest` + levelgen + `--finish`;
   `python -I Tools/night/n59-l-room.py n65-foundry room <id>`; n46flow `--rooms <id>` (seeds 1-10 and 11-20); `hold52 --room <id> --pads`;
   `n53ridekill ride --level Design/levels/n65-foundry.json --room <id>` on a ride card; the plan PNG. It saves every output, commits the room
   file and the outputs (never the generated level), reports, and **starts the next round from its own outputs while the lead runs the
   Player round** (no waiting on the reviews; their findings fold in when they arrive).
2. **The lead** picks, regenerates (`--slots=latest`), runs `n65-gates.sh` (GREEN), commits, builds `n65-b<n>` with the row on in its copy
   (`n46-build-rows.cjs n65-b<n> on n65.foundry65`), deletes the previous round build, runs one reel set (`tour,n65-foundry,tour65`,
   `a,n65-foundry,a157,-skygrindSpawnShots60`, `v,n65-foundry,verbs37`), then the two reviewers in turn, batched over every room: gg-proxy (GG's
   words only) and a fresh level-design reviewer (research-rooms.md's 15 rules and the card), each per room `BLOCKING: <piece> / <rule or
   quote> / <one-round fix>` or `NONE`. Findings to the builders by SendMessage.
3. **Rounds at about 1:45 (R1), 2:55 (R2), 4:05 (R3).** No round 4: the fix round below takes its place.
4. **The GATE, per builder, as soon as its R3 reviews land (about 4:10 for R, 4:30 for S):** the SPOT per room first (rule 2 of section 2), then
   one fresh skygrind-critic per builder's three rooms: the slate entries, the red and the R3 files, the frames, verbs37 n65-foundry 27 / 27,
   rc0's numbers, the open findings, the diff, the sha; a verdict file per room (`Logs/n65/verdict-<id>.md`, first line `<id>: PASS` / `FAIL
   <reason>` / `UNSURE <reason>`). PASS: passSha, passFiles, `n65-slot-set.cjs <id> new`. FAIL with a named cause: one fix round picked by 5:00,
   a round-2 critic; else the slot stays old.
5. **The level row (slice LV):** once slots65.json holds a new room, the lead assembles with slots65.json, builds, runs `tour` + `verbs37
   n65-foundry` + the off check (`-skygrindOff65` boots n52-foundry: a reel line), and LV's own critic judges (verdict-LV.md); PASS: the flip by
   `n65-freeze-rows.cjs`, then **rc1** = a fresh build of HEAD: verbs37 x5 (with n65-foundry) + the smoke (no agent alive); a failure rolls the
   row back in one commit. Late PASSes go into **rc2** by 5:40.
6. **Always two play builds** (section 9): the verified freeze, and `Builds/n65-play-latest`, a copy of the freeze whose StreamingAssets level
   is the assembly with every room's newest file and whose copy has the row on, labelled "unreviewed rooms" in the play card, so GG's hands
   judge the night's work even if no room passes.

## 5. Delegation
Three builders (T, R, S) and one review slot; at most four subagents alive. Resume a stopped agent by SendMessage, never a fresh copy. A
builder never runs the Editor, a build, a Player, a reel or the suite.

## 6. The clock (t0 = the clock's start)
| from | to | what |
|---|---|---|
| 0:00 | 1:00 | bearings, the verify table, the flow base; builders at 0:05 (T1; R / S cards); the sim red; A1 (0:25-1:00); the card gate (0:50-1:00) |
| 1:00 | 1:45 | round 1 (R / S from their cards); T2; the red set and tour65 on rc0 once T2 lands; b1 at ~1:45; the reviews |
| 1:45 | 2:55 | round 2; b2; the reviews; the report skeleton committed by 3:00 |
| 2:55 | 4:05 | round 3; b3; the reviews |
| 4:05 | 5:00 | GATEs per builder (R ~4:10, S ~4:30); slots; LV; rc1; fix rounds picked by 5:00 |
| 5:00 | 6:00 | round-2 critics; rc2 by 5:40; n65-play-latest; the report committed by 5:30 and finished |
| 6:00 | 7:00 | **the close** (section 9), started by 6:15; the final message at 6:30 or later |

## 7. Usage limit, resume and the minimum close
State on disk: Logs/n65/slate.json, rc.json, NIGHT65-LOG.md (every agent's id), slots65.json, the builders' branches and notes. After a stop:
bearings 1-3, the slate, resume each agent by SendMessage, log the gap. **The minimum close** (10 minutes, no agent, no Player; first after any
stop past 6:00): (1) the clock line; (2) every slice in progress set SHORT with what was measured; `node Tools/night/n65-slate.cjs` with 0 in
progress; (3) NIGHT65-REPORT.md committed with what is known, "not checked" for the rest, its sections 2 and 8; (4) HANDOFF.md's NIGHT65
section with rc.json's **last verified RC** as the play build; (5) commit with explicit paths, `git tag u3-n65` on that commit; (6) `git
status --short NIGHT65-REPORT.md NIGHT65-LOG.md HANDOFF.md docs/memory/CORE.md` printing nothing; (7) `node Tools/night/n65-clock.cjs stop`.

## 8. Honesty
Every claim names its command or frame; anything not checked says "not checked" (in particular "the verb cannot be skipped: not checked"
unless a pilot shows it); a failed room is reported failed with its numbers; a room short of three rounds is reported SHORT; tests are never
edited to pass; reviewers and critics report every problem and the lead filters with a logged reason. The rooms are a tested guess until GG
plays them, and the report says so. Mistakes become LESSONS.md rows (at its 150 cap: merge or retire).

## 9. The close (started between 6:00 and 6:15, done by 7:00) and the report
- **In series:** (a) no GATE or flip after 6:00. (b) `bash Tools/night/n41-build.sh n65-freeze` from HEAD and `git diff --stat ProjectSettings`.
  (c) `bash Tools/night/n65-reels.sh n65-freeze vf,n52-foundry,verbs37 vn,n65-foundry,verbs37 vi,n52-iceline,verbs37 vk,n57-kiln,verbs37
  vs,n52-span,verbs37 vp,n52-spire,verbs37` (six levels; background). (d) With no agent alive, `bash Tools/night/n56-smoke-steal.sh n65-freeze`
  ("DONE: all steps passed"). (e) `bash Tools/night/n65-gates.sh --flow` ("gates: all GREEN"). (f) Builds/n65-play-latest (section 4 step 6),
  its level file's sha printed. (g) `node Tools/night/n65-slate.cjs`, 0 in progress. (h) Finish NIGHT65-REPORT.md, HANDOFF.md, LESSONS rows
  and the harvest (ledger wave-65.md, THREADS <= 60, FEATURES, CORE <= 40). (i) The row printed from both config copies and slots65.json
  printed, matched to the report; the minimum close's steps (5)-(7); the final message `NIGHT65 COMPLETE` at 6:30 or later, with no background
  job or agent running. At 7:00 whatever is left is logged under Blocked, then (i).
- **NIGHT65-REPORT.md, in this order:** (1) what is still not good (the last reviews and gg-proxy; the rooms that fell short and why); (2) **GG's
  play card:** Builds/n65-freeze (the passed rooms; `-skygrindOff65` for the old level 1) and Builds/n65-play-latest (every room's newest,
  unreviewed), five minutes, what to try in each room (its verb), the one question ("better, worse or the same as the old level 1, and which
  room"); eye63's X / Y beside it; then each room ON with its frames; (3) tools proven (the assembler, the slot tool, tour65, the bot opt-out);
  (4) what failed, with numbers; (5) not checked; (6) your calls (DECIDE65.md); (7) the next prompt's seed (plan vs actual per room and round,
  D-9's measure, leftovers ranked, the next level by the same recipe); (8) the close lines.

**Closing reminder:** cards before geometry; three rounds per room; every round adds pieces with jobs; measured play over counts; the bot is a
guard; outputs as files; a PASS binds to its sha; GATE each builder's set as soon as its round 3 lands; two play builds for GG.
