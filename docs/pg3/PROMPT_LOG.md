# SKYGRIND Master AI Prompt Log (ASU AME 294, Portfolio Game 3)

Status: DRAFT written by a coding agent (Claude Sonnet 5.5) on 2026-10-28 from files in this repository. GG has not edited it yet.
Rules for this draft: GG's words are copied from the files with his typos kept (he dictates by voice). Anything the files do not
show is marked `TODO(GG)` or "not checked". Nothing here was invented to fill a gap.

How the prompts of this project are stored. GG does not type one long prompt per night. He sends a short message (often by voice).
A "forge" session turns it into a long night prompt, `NIGHTn.md` (the coding agent's whole brief for that night). Those prompts
for nights 7 to 43 are in `nights/` and for nights 44 to 74 in the repo root; both places are untracked by rule (CLAUDE.md rule 1),
so they exist on GG's disk and in the public snapshot's `prompts/` folder only if copied there (see the note in section 8). Each
night's record is in `NIGHTn-LOG.md` and `NIGHTn-REPORT.md` (tracked). GG's own messages are collected in `docs/memory/OWNER.md`.

Contents: 1 Timeline. 2 Coding-agent prompts. 3 Error recovery log. 4 Tripo 3D prompts. 5 Audio prompts. 6 Trailer shot prompts.
7 Reflection (draft). 8 Open items for GG.

---

## 1. Timeline

### 1.1 From the browser game to Unity

| Date | Event | Source |
|---|---|---|
| 2026-09-28 | First commit of the web game (PG2 era): `ef42ed5` "M0: Vite + three scaffold, fixed-step loop, config, debug overlay, movement math tests". Vanilla JS and three.js, built in milestones M0 to M6 from `SPEC.md`, each started by a kickoff prompt. | `C:\Users\gaige\dev\skygrind` (read only); `PROMPTS.md` there |
| 2026-09-28 to 2026-10-01 | 121 commits and 26 tags in the web repo (`p1-endless` to `p17-night6`, plus the `s1` to `s9` survival tags). `p17-night6` (commit `1c73d68`, 2026-10-01) is the frozen reference. | web repo `git tag`, `git log` |
| 2026-09-30 | Unity project created: `3fee5c7c3` "Initial Unity project (URP, 6000.3.7f1)". A feasibility spike ports the movement math, the collider, 16 recorded trajectories and an enemy bench (22 commits that day). Engine research for Steam written the same day. | `SPIKE-RESULTS.md`; web `ENGINE-STEAM-RESEARCH.md` ("Researched 2026-09-30") |
| 2026-10-02 | GG's decision: SKYGRIND is built only in Unity from now on; the web game is frozen and gets nothing new. | `CLAUDE.md` (first paragraph); `nights/NIGHT7.md` |
| 2026-10-07 | Portfolio Game 2, Demo Day 2 peer playtest (the browser game, on itch.io). | `DESIGN-RESEARCH.md` line 135 |
| 2026-10-28 | Portfolio Game 3 due date as recorded on 2026-10-02 (`tasks.json` item U4: "due 2026-10-28"; the co-op plan named there was not built). | `tasks.json` line 15; `DESIGN-RESEARCH.md` line 136 |

Totals measured on 2026-10-28 with `git log`: 4,243 commits by one author (Gaige Geraci) from 2026-09-30 to 2026-10-28; 66 tags.
Commit trailers: 2,622 "Co-Authored-By: Claude Opus 5.5", 1,003 "Claude Opus 5.5 (1M context)", 462 "Claude Sonnet 5.5".
Commits per day peaked at 365 on 2026-10-05 and 329 on 2026-10-23.

### 1.2 Nights and waves

A "night" is one long unattended coding-agent run with a prompt `NIGHTn.md`; a "wave" is the same run seen by the memory system
(w20 = NIGHT20). Dates are the tag's commit date (`git tag --format`), or the date in the source named where there is no tag.
"Outcome" is copied or shortened from the source named; "not summarised" means I did not read enough to state it.

| Night | Date | Theme (the prompt's title) | Tag | Outcome (source) |
|---|---|---|---|---|
| 7 | 2026-10-02 | Stage 1: the vertical slice (port) | u1-slice | Survival run in Unity: Husk, KESTREL, PULSE GAUNTLET, director, HUD, DOWN and RESULTS, Windows Player; slice smoke passed all steps in 20 s (commit `fcf3973b3`) |
| 8 | 2026-10-02 | Stage 2a: the Demo Day build | u2a-demo | Smoke DONE; bench median 3.35 ms, worst 1 % 8.37 ms; 237 / 237 tests, 16 / 16 replays, 55 / 55 golden (tag commit) |
| 9 | 2026-10-03 | The oracle and the arsenal | u3-n9 | Goal: every exported golden vector consumed, V8 math in `JsMath`, arsenal and momentum ported (NIGHT9.md); close not summarised |
| 10 | 2026-10-03 | ARMORY guns and the Mystery Box | u3-n10 | Smoke DONE with a Mystery Box roll and kills by STITCHER, COMET, LONGWAVE; golden 0 mismatches (tag commit) |
| 11 | 2026-10-03 | The run's shape | u3-n11 | Smoke DONE in 86.3 s with the power switch, a HOLD round and THE PROTOCOL (tag commit) |
| 12 | 2026-10-04 | The look, the feel and the meta core | u3-n12 | Bench series on the Silent power plan (tag commit) |
| 13 | 2026-10-04 | Run better, play anywhere, architecture | u3-n13 | Two reviews, build 5 (tag commit) |
| 14 | 2026-10-05 | THE GAME PLAYS ITSELF | u3-n14 | Close after a reboot: suite, builds, smoke findings fixed (tag commit) |
| 15 | 2026-10-05 | A map with a personality; aim assist and the pad | u3-n15 | Close table U3-28.1 to 28.9 DONE (tag commit) |
| 16 | 2026-10-05 | THE FEEL NIGHT | u3-n16 | Release build; smoke re-run "2 step(s) failed" (the two pad steps) (tag commit) |
| 17 | 2026-10-06 | THE OVERHAUL NIGHT (identity, HUD, one stamina bar) | u3-n17 | Suite 969 / 969; reviewers' score 35 / 50 (tag commit) |
| 18 | 2026-10-06 | FEEL IN MOTION, measured | u3-n18 | Metrics PASS 56 to 104 of 125; rubric 34 and 35 of 50 against 25 to 27 (ledger/wave-18.md) |
| 19 | 2026-10-06 | EVERY GUN HAS A JOB | u3-n19 | Cut short by the spend limit (ledger/wave-19.md; error E-05) |
| 20 | 2026-10-07 | GG's playtest, then a build night | u3-n20 | Shockwave on the THUNDERCLAP, zoom on most guns, world speed streaks, enemy routes; 22 additions, few measured on a Player (L-014) |
| 21 | 2026-10-07 | THE MOVES AND THE MOMENTS | u3-n21 | Five features and a moonshot behind switches; 0 of 7 contracts passed (L-028); Editor dialog cost hours (E-03) |
| 22 | 2026-10-07 | FINISH, POLISH, PROVE | none | Cut by a computer crash about 5:40 PM; 0 of 9 contracts proven on a Player (NIGHT22-REPORT.md; E-04) |
| 23 | 2026-10-07 | PROVE WHAT IS BUILT (2 h sprint) | none | 8 of 9 NIGHT22 contracts GREEN on a Player (stated in `Research/2026-10-08-wave24/GG-REQUEST.md`) |
| 24 | 2026-10-08 | THE LINEAR TURN: campaign spine, worm boss | u3-n24 | 12 of 13 rows PASS on a Player; suite 1720 / 1720 (NIGHT25.md header) |
| 25 | 2026-10-08 | THE TEACHING CAMPAIGN | u3-n25 | Closed by GG after his first playtest (tag commit); suite 1760 / 1760 (NIGHT26.md header) |
| 26 | 2026-10-08 | THE FIRST THREE LEVELS: hulk, ice chase, wyrm, level select, ranks | u3-n26 | Tripo worm visuals added the same night (NIGHT26-LOG.md 21:30); Editor stalled, suite 2 had no result (tag commit) |
| 27 | 2026-10-09 | The first three levels, tighter and original | u3-n27 | 8 DONE / 7 SHORT (tag commit) |
| 28 | 2026-10-09 | Levels 4 to 10 to the same standard | none | Tripo worm switched off (NIGHT28-LOG.md 10:20) |
| 29 | 2026-10-09 | THE ARENA OVERHAUL | u3-n29 | Suite 1850 / 1850 (batch mode); level-select fix 4 of 20 to 20 of 20 on a Player (ledger/wave-29.md) |
| 30 | 2026-10-09 | THE READABLE GAME (weapon rules, HUD, lava) | u3-n30 | Rules30, HudWeapons30, Lava30 (ledger/wave-30.md) |
| 31 | 2026-10-10 | BOSSES YOU CANNOT CAMP (grapple pull, charge shot) | u3-n31 | not summarised |
| 32 | 2026-10-10 | Bosses in a minute, a grapple with charges, level 5 | u3-n32 | Boss kills about 30 s for the Player pilot (tag commit) |
| 33 | 2026-10-11 | The polish night (lead and seven builders) | u3-n33 | not summarised (ledger/wave-33.md) |
| 34 | 2026-10-11 | The flow night (the grapple BEND, slow motion) | u3-n34 | not summarised (ledger/wave-34.md) |
| 35 | 2026-10-11 | The fundamentals night (principles, verbs list) | u3-n35 | `docs/VERBS.md` written at the night's start; `docs/PRINCIPLES.md` follows GG's ask of 2026-10-11 (L-052) |
| 36 | 2026-10-12 | Movement is the weapon | u3-n36 | not summarised |
| 37 | 2026-10-12 | The boss night: four bosses restarted from scratch | u3-n37 | K1, S1, O1, C1, L0 DONE; THE WICK's kill not reached in a Player run (tag commit) |
| 38 | 2026-10-13 | The flight night: every level overhauled | u3-n38 | Closed SHORT (ledger/wave-38.md) |
| 39 | 2026-10-13 | The feel night: level 3 rebuilt, pistol that charges | u3-n39 | not summarised (ledger/wave-39.md) |
| 40 | 2026-10-14 | The build night | u3-n40 | 13 of 14 rows ON (ledger/wave-40.md) |
| 41 | 2026-10-14 | The finish night | u3-n41 | 13 of 18 rows ON (ledger/wave-41.md) |
| 42 | 2026-10-15 | The feel night: GG's playtest complaints | u3-n42 | 10 rows ON (ledger/wave-42.md) |
| 43 | 2026-10-16 | The cohesion night: thirty asks | u3-n43 | G1 to G30 fates logged (tag commit) |
| 44 | 2026-10-16 | The finishing night, cut to one hour | u3-n44 | vis44 ON; play build n44-r1 (ledger/wave-44.md) |
| 45 | 2026-10-17 | The arena identity night | u3-n45 | foundry45 ON; verbs37 26 / 20 / 25 / 30 / 30; smoke passed (ledger/wave-45.md) |
| 46 | 2026-10-18 | The flow night (a flow meter as scoreboard) | u3-n46 | Play build n46-r3; GG did not test (ledger/wave-46.md) |
| 47 | 2026-10-19 | GG's playtest night: his 45 asks | u3-n47 | Five bosses cut to about 92 to 97 s median for a human (`docs/BOSSES47.md`); usage limit at 03:28 |
| 48 | 2026-10-19 | Finish GG's 45 asks | u3-n48 | not summarised |
| 49 | 2026-10-19 | One look for the whole game | u3-n49 | not summarised (ledger/wave-49.md) |
| 50 | 2026-10-19 | Ship the look | u3-n50 | Suite 2217 / 2217 (tag commit) |
| 51 | 2026-10-19 | A 3 h session: world look, combat crunch | none | `NIGHT51-REPORT.md` (OWNER.md row "2026-10-19 ~19:40") |
| 52 | 2026-10-20 | THE ARENA NIGHT: rooms to a numbered contract | u3-n52 | 18 rooms pass the contract (L-086) |
| 53 | 2026-10-20 | THE SECOND PASS (plus EXT53) | u3-n53, u3-n53x | Rooms edited in place (ledger/wave-53.md) |
| 54 | 2026-10-20 | THE LOOK PASS | u3-n54 | All five rows ON (ledger/wave-54.md) |
| 55 | 2026-10-20 | The catch-up and push night | u3-n55 | Cut by the usage limit; smoke made focus-proof; every row OFF |
| 56 | 2026-10-21 | LEVELS FIRST | u3-n56 | Usage-limit pause 03:09 to 04:20; clear55 ON |
| 57 | 2026-10-21 | SHIP AND DEEPEN | u3-n57 | 7 rows ON by the baseline-relative shipping rule (L-103) |
| 58 | 2026-10-21 | Finish level 3, start level 4 | u3-n58 | Nothing shipped ON (ledger/wave-58.md) |
| 59 | 2026-10-21 | SHIP WHAT IS READY | u3-n59 | Level 3 exits fix shipped (ledger/wave-59.md) |
| 60 | 2026-10-22 | OVERNIGHT RUN (8 h) | u3-n60 | Rows ON each with a one-row critic PASS (ledger/wave-60.md) |
| 61 | 2026-10-22 | THE BUILD NIGHT that could not build a Player | u3-n61 | Nothing ON; five visible rows built and gated, never seen (ledger/wave-61.md) |
| 62 | 2026-10-22 | THE PROOF SPRINT (2 h) | u3-n62 | gunYellow61 ON (ledger/wave-62.md) |
| 63 | 2026-10-23 | THE BOSS NIGHT | u3-n63 | bossRule61, text61, nearFade63 and more ON, each by a GATE PASS; usage-limit stop 05:23 to 07:41 (L-003) |
| 64 | 2026-10-23 | THE ALIGNED NIGHT | u3-n64 | order64, fightLine64 ON (ledger/wave-64.md) |
| 65 | 2026-10-23 | THE FLIGHT FOUNDRY | u3-n65 | No game change; disk under the guard's limit all night (E-07) |
| 66 | 2026-10-24 | THE FOUNDRY EARNED | u3-n66 | Two usage-limit stops, 4:35 of 8 h lost (L-003) |
| 67 | 2026-10-24 | THE AREAS (levels 2, 3, 5) | u3-n67 | not summarised (ledger/wave-67.md) |
| 68 | 2026-10-25 | THE BETTER LINE | u3-n68 | Minimum close at 9:01 (ledger/wave-68.md) |
| 69 | 2026-10-25 | THE ROOM THAT PAYS (4 h) | u3-n69 | Three rooms measured alone; a build for GG (NIGHT69.md title) |
| 70 | 2026-10-26 | THE VERB THAT HAPPENS | u3-n70 | No game change (CORE.md) |
| 71 | 2026-10-26 | THE BUILD GG PLAYS | u3-n71 | `Builds/n71-play`: eight rebuilt rooms; ten cleanup commits, 359 lines deleted |
| 72 | 2026-10-27 | SEEN | u3-n72 | 8 rows ON in the repo's game; close finished in the next session |
| 73 | 2026-10-27 | MOTION (3.5 h) | u3-n73 | 6 more rows ON; flicker down 42 to 87 % on all five levels |
| 74 | 2026-10-28 | ROOMS (8 h) | u3-n74 | Four level rows ON with 21 rooms (15 rebuilt); `weak74`, `target74`, `kill74`, `spring74`, `reveal74` ON; gg-proxy: rooms still the top fault (NIGHT74-REPORT.md) |
| PG3 | 2026-10-28 | Assignment work on branch `pg3` | none | `818051f66` "PG3 sound74": the game's first sound, ElevenLabs prompts, snapshot script |

Nights 1 to 6 are the web game's (`OVERNIGHT*.md`, `NIGHT4.md` to `NIGHT6.md` in the web repo); I did not read them.

---

## 2. Coding-agent prompts, chronological

Each entry gives the date, the kind of work, GG's words, the key lines of the night prompt, and what the record says came out.
Quoted text is verbatim. "Prompt text" means the line is the forge's or the lead's writing, not GG's.

### P-01. 2026-09-28 to 2026-10-01. PG2 web game: scaffolding by milestone (context for the reflection)
Kind: scaffolding. File: `C:\Users\gaige\dev\skygrind\PROMPTS.md` (read only).
Session 0 prompt: "Read SPEC.md and CLAUDE.md. Interview me with the AskUserQuestion tool about anything ambiguous or contradictory in SPEC.md that would block Milestones M0 to M3. Ask at most 8 questions and skip anything the spec already answers. Then update SPEC.md with my answers. Do not write code."
M1 prompt: "Implement Milestone M1 from SPEC.md. Start from the three.js games_fps Capsule + Octree pattern (reference in §12). Put the accelerate, friction and jump formulas in movementMath.js with vitest tests."
The file's own rule: "When something feels wrong, describe it in plain words ("slide stops dead on slopes"). Paste console errors exactly." Outcome of M0 in its log: "6/6 tests pass, build clean".

### P-02. 2026-10-02. NIGHT7: the port starts (scaffolding)
Kind: scaffolding. File: `nights/NIGHT7.md`.
Prompt text: "GG's decision (2026-10-02): SKYGRIND is built in Unity from now on. The web game (`C:\Users\gaige\dev\skygrind`, tag `p17-night6`) is **frozen**. It is the reference you copy, and it gets nothing new."
Prompt text: "**GG's standing order: build, don't test.** About 15% of the night goes to checks".
Hard rule 3 of the prompt: "Movement stays exact ... all 16 trajectory replays are bit-exact on the C# web collider."
Hard rule 4: "One fixed 120 Hz loop runs in `Update` with an accumulator ... No Rigidbody, no CharacterController, no PhysX for gameplay."
Outcome: nine slice commits `NIGHT7 S1` to `S9`, four "if-time" features and the `Q` commits, all on 2026-10-02; the Windows Player `Builds/Stage1/SKYGRIND.exe`; smoke "all steps pass in 20 s" (commit `fcf3973b3`, `ea5ca4c63`). The first handoff (`CLAUDE.md`, `HANDOFF.md`, `tasks.json`) was written at `9970e8ec2`.

### P-03. 2026-10-02. NIGHT8 and NIGHT9: peers, then the whole web game
Kind: scaffolding and mechanics. Files: `nights/NIGHT8.md`, `nights/NIGHT9.md`.
NIGHT8 prompt text: "Demo Day 2 is **Wednesday Oct 7**: peers play SKYGRIND and give feedback." Goal: "plays a run that a peer could play for 15 minutes".
GG's words (NIGHT9.md): "great we need this next prompt to basically add everything that was built on the browser version (WHICH SHOULD HAVE BEEN UNITY VERSION) and switch over to unity, more weapons, better movement, bosses, other gamemodes, eveerything that was built, style meter, better UI. we need to resrecah theorughly and find best way to tarnsfer all of that and rebuilt ON THE UNITY VERSION. start with resrecah create plan and next prompt, look and see if people have done this before, browser game to unity"
Research result carried into the prompt (prompt text): "keep the old build as the oracle; move in small units in dependency order; check every unit by machine". This is why the golden vectors, the 16 trajectories and `JsMath` exist (`PORT-RESEARCH.md`, `PORT-RULES.md`).

### P-04. 2026-10-04 to 2026-10-05. NIGHT14 to NIGHT16: "creative mode", the map, the feel
Kind: refinement and mechanics. Files: `nights/NIGHT14.md`, `NIGHT15.md`, `NIGHT16.md` (headers).
NIGHT14, GG: "go full creative mode ... dont overhaul anything but lets think outside the box on how to make this game better ... 8+ hour long".
NIGHT15, GG: "a map overhaul ... built to maximize movement and replayability ... feel like CoD zombies and ULTRAKILL with personality, not a bland map"; "auto aim is too strong"; "is there controller support".
NIGHT16, GG: "I want next prompt to be purely focused on quality of life as well as effects ... I want movement to be a little more balanced like you can't you know spam slide over and over and other movement and UI stuff, be critical, think hard and follow a good process ... that cool effects where if you hit a punch or a certain attack or gun hit or head shot its like ultrakill where the game will almost pause or go slomo".
Outcome: aim assist defaults became mouse `off` and pad `assist` (`design.aimAssist` `_why` in `Design/unity-config.json`); the kill and hit "freeze ladder" shipped in NIGHT18 (`docs/memory/FEATURES.md`); the pad layout and Kenney button glyphs came in NIGHT19 and NIGHT20.

### P-05. 2026-10-05 to 2026-10-06. NIGHT17 and NIGHT18: identity, then "measured, not claimed"
Kind: refinement. Files: `nights/NIGHT17.md`, `nights/NIGHT18.md`, `IDENTITY.md`.
NIGHT17, GG (trimmed by the prompt writer): "the UI just all looks very cheap ... the menus don't really follow proper game design ... way too wordy ... way too cluttered and has way too many options ... how points and stuff are shown, the score, it just feels very stale, not energetic ... the effects and the animations are very poor ... I feel like we're going in circles, so we need to find new methods ... I want it to feel like ULTRAKILL where it's just so satisfying to defeat enemies".
`IDENTITY.md` opens with GG: "the text and the font and the colors are just very bland, it needs identity." Result: Archivo Extra Condensed Black Italic with Tourney numerals, a 12 degree slant, one cyan the player owns, gold for points.
NIGHT18, GG: "follow the best available methods ... analyse the results ... what did not get completed or built at all ... create a prompt that will show real tangible improvements".
Result (L-001): NIGHT17 shipped 185 commits with all gates green but reviewers scored 25, 23, 27, 27 of 50; NIGHT18 built a measuring harness first and took the feel metrics from 56 to 104 of 125 PASS.

### P-06. 2026-10-06. NIGHT19 and NIGHT20: GG's playtest becomes mechanics
Kind: mechanics and a reversal. Files: `nights/NIGHT19.md`, `nights/NIGHT20.md`, `docs/memory/OWNER.md`.
GG: "Most weapons should, besides a few that have unique left clicks, zoom: simple zoom."
GG: "There needs to be a long, long cooldown, like maybe every 15 seconds or even longer." (the shockwave)
GG: "The right bumper could be used to switch weapons as well: just scroll through weapons."
GG: "There always really needs to be a route that enemies can climb up, or just find a way to pass by normally."
GG: "Copy an open-source project that has way better dash lines that mimic speed ... Go actually go into the GitHub, I'm serious."
GG, a reversal of NIGHT19's work: "You messed up: the left was supposed to be for the first option and that was supposed to do the shockwave, not the pistol. You should be able to zoom in with the pistol like how it was. And it's not balanced: you can just spam it."
GG, scope: "Go well beyond this prompt. Study game design seriously. This is going on for 8 hours overnight. I need a lot of building; I'm not seeing enough building."
Result: the shockwave moved to the THUNDERCLAP with an 18 s cooldown (and was removed again in NIGHT25 at GG's request, P-08); world-space speed streaks ported from two open-source projects (credited in `ATTRIBUTION.md`); lesson L-008 (map GG's loose words to the web's config names before building).

### P-07. 2026-10-07. NIGHT21: the prompt format settles
Kind: prompt design. File: `nights/NIGHT21.md`.
Header block, verbatim: "id: skygrind-n21 | version: 2 (red-teamed) | date: 2026-10-07 | model / effort: claude-opus-5-5 / high". Body: a `<role_and_goal>` section; NIGHT25 adds `<ggs_words>` and `<facts_already_measured>` sections, so the structure grew over the week.
GG's request that night (`docs/memory/OWNER.md`, `ledger/wave-21.md`): "I WANT BUILDING new fetaures that match and ideas, innovate, resreach".
Result: five features and a moonshot, but no contract passed because nobody owned the Player scenarios (L-028).

### P-08. 2026-10-08. NIGHT24 and NIGHT25: the linear turn, with a bug report inside the request
Kind: scaffolding and bug fix. Files: `Research/2026-10-08-wave24/GG-REQUEST.md`, `nights/NIGHT24.md`, `nights/NIGHT25.md`.
GG (voice-typed): "I would like the arena like think about ultra kill especially and Call of Duty zombies how the game slowly builds up and introduces mechanics to you I want this game to do that with the campaign so I need another map overhaul to really kind of work mechanic to mechanic".
GG, bug report in the same message: "remove that shockwave effect from the shotgun should not be like that also there's no health bar in the it doesn't die also the arena is bugged out".
Prompt text (NIGHT25): "**Bugs GG hit** ...: the worm is on screen over an empty void sky with no floor ... **the boss has no health bar; it does not die**". The prompt orders: "Never cut: the three bug fixes".
Result: a CAMPAIGN mode with rooms, doors, checkpoints and a boss room (`Core/Campaign24.cs`); the first teaching level.

### P-09. 2026-10-08. NIGHT26: new art arrives (the Tripo worm)
Kind: mechanics and art. Files: `nights/NIGHT26.md`, `NIGHT26-LOG.md`, `Research/2026-10-08-wave26/WORM-VISUALS.md`.
GG: "for the second level boss fight ... you're only allowed to use the first [weapon] and you have to get close ... you slowly unlock weapons as you go on and maybe two levels in between the next weapon ... the worm boss maybe the third level or later ... maybe add an easier boss".
GG, 21:30 the same night (quoted in `Design/unity-config.json`, `design.n26.wormVisual._why`): "Update the worm boss to use the new Tripo models, and add glow, aura and animation effects".
Log line: "NEW from GG at 21:30: the worm gets the Tripo models + glow / aura / animation: builder V ... visual only."
Result: the three FBX files (see section 4) drawn with a red-orange emissive shader; reels `wormcoil` and `wormattack` PASS. DECIDE47 D-9 records that on 2026-10-09 GG "asked for the old worm and not the Tripo one" (the DECIDE's wording, not a verbatim GG line), and the row `wormVisual` was turned off (DECIDE28 D-4). The FBX files and the view code stay in the repository.

### P-10. 2026-10-09. NIGHT29 and NIGHT30: rooms from scratch, lava, a gritty look
Kind: rooms and refinement. Files: `docs/memory/OWNER.md`, `docs/memory/ledger/wave-29.md`.
GG: "every single level needs a complete arena overhaul completely from scratch so don't preserve anything OK that is an order".
GG: "I'd like a lot of the rooms to be a lot smaller and more action packed like think about the design of Ultra Kilikow".
GG, after NIGHT29: "A game is looking much better overall, but there are still several areas that need tightening up"; "there are gaps or holes in the floor that look like you can fall into them ... possibly by replacing those gaps with actual lava"; "I want it to lean more into a PlayStation-2 or ULTRAKILL-style gritty aesthetic".
Result: NIGHT29 rebuilt three levels and found that a floor-bot hid a fast camper (L-029); NIGHT30 added lava (`Core/Lava30.cs`: a touch costs 25 % of max health, knocks up 9 m/s, no respawn) and weapon rules.

### P-11. 2026-10-10 to 2026-10-13. NIGHT31 to NIGHT39: bug reports that became rules
Kind: bug fixes. Files: `docs/memory/OWNER.md` rows w30 to w38; `docs/memory/LESSONS.md`.
GG: "the grapple mechanic just doesn't work at all It doesn't pull you in". Fix in NIGHT31: a pull that takes you where you fire, capped at 12 m/s so it cannot cross a level in one tap (L-042).
GG: "why is the char shot [charge shot] gone ... when you hold it down", then "why we're removing abilities ... the pistol lost the ability to charge and fire same with the shotgun". Fix: `docs/PRINCIPLES.md` rule 1, "Never lose what the player had", and `docs/VERBS.md`, a table of every verb with a Player check (L-052).
GG: "we're not following game principles ... that should be a core embedded into this project". `docs/PRINCIPLES.md` opens with that quote.
GG on boss length: first "way too much health" (NIGHT31), then "still way too tanky" (NIGHT32), then reversed at NIGHT38: bosses "one to two minutes, maybe three if you're slow"; the pistol "should intelligently prioritize the charge shot instead of firing the fast shot first".

### P-12. 2026-10-14 to 2026-10-17. NIGHT41 to NIGHT44: typos kept, rules written
Kind: bug fixes and process. Files: `docs/memory/OWNER.md`, `docs/DESIGN-RULES.md`, `docs/VERBS.md`.
GG (n41-r7): "waallride section ... awful and relaly difficult and not fun"; "pressing a near a wall you fly bcak so fast"; "shoutgun jumping is too difficult"; "a skill to help with handoff and ocntext because we are forgetting things".
Fix recorded in `docs/VERBS.md` (row `wallfling42`): "a wall jump only when moving into the wall or riding it (G2: an air jump press near a wall no longer flings back)".
GG (2026-10-16): "treat this as a challenge to make a major leap in quality rather than a collection of tiny patches"; "establish actual design rules and principles ... so that we stop forgetting"; "Fix this at the movement-system level rather than designing around a broken mechanic". Result: `docs/DESIGN-RULES.md` (rules M-1 "Fix the system, never design around it" and more).
GG (2026-10-17): "see where we are, follow best methods, dont repeat same mmistakes, reserach folllow best game deisgn pronicples and if it arena related please dont just randomly plce stuff it needs intension and personality and lets cut fluff and old stuff, dont cut anything meaningful, think before writing this prompt".

### P-13. 2026-10-18. NIGHT47: GG's 45 asks in one message
Kind: refinement and bug fixes. File: `NIGHT47.md` (header: "This text outranks everything below it. Read it twice.").
GG: "you simply can't make it consistently; even with a double jump you barely reach the next ledge, the timing window is too tight".
GG: "these boss fights should last around two to three minutes maximum, so its HP needs to be reduced or its vulnerability windows expanded".
GG: "don't just follow my notes, push past them and refine every part of the design".
Result: five bosses cut to a human median of 92 to 97 s (THE WICK 95.0, THE ICEBREAKER 91.8, THE KILN WYRM 96.7, THE LOOM 92.4, THE CROWN 91.9; `docs/BOSSES47.md`). THE WICK became a three-phase climb fight.

### P-14. 2026-10-19 to 2026-10-20. NIGHT48 to NIGHT54: one look, rooms to a contract, honest critique
Kind: refinement and rooms. Files: `docs/memory/OWNER.md`, `NIGHT52.md`, `NIGHT54.md`.
GG (after NIGHT48): "The charge bar under the AIM reticle needs to come back I don't know why it's gone"; "the colors and stuff are way too bland The effects are inconsistent it all needs to be unified ... apply those changes across the entire game".
GG (NIGHT52 request): "can we plan next detailed goal overnight promptr that will go for 8 hours, please plan and build this pormpt for 30 mins also be honest and critical on where we still need to imporve and polish, follow best methods on how to prompt and get best results, please compare other open source porjects, copy, use skills and roleplay hard cap 8 minimun 7 and it all relate dto building and itterating."
GG (NIGHT54): "a lot of the decorative elements in several arenas are poorly executed and lack strong color direction, and the lighting and flickering bugs that happen constantly break immersion and absolutely need to be fixed; the UI looks good as it is, so don't touch that"; "each boss should have its own distinct color palette and lighting identity ... similar to how JoJo's Bizarre Adventure uses dramatic color changes".
Result (L-086): each room gets a computed contract (size, density with a job per piece, a ride, a pad, spawns); 18 rooms pass.

### P-15. 2026-10-22. FORGE61: a prompt written to stop repeating mistakes
Kind: prompt design and guardrails. File: `FORGE61.md`.
GG: "research best ways to prompt and get better results, skills whatever ... so it can create a powerful goal and loop prompt to build and not run into mistakes, polish, kill dead code, but need to see real results and it needs to be honest and critical where things are not good."
Prompt text (FORGE61 section 4): turn rules that keep breaking into hooks, each tested "with a planted violation". Result: `.claude/hooks/guard.cjs`, a PreToolUse hook that refuses `git push`, `git add -A`, edits to GG's own files, heredoc-written docs and more (see E-15, E-16, E-17).
GG (FORGE63): "i would lead towards bosses, effect colores everything doenst feel alligned, boss animations are too rigid to start, this is one of many many things the night prompt hsould focus on, create it now, 30 mins to build".

### P-16. 2026-10-23 to 2026-10-25. NIGHT64 to NIGHT69: rooms that make you move
Kind: rooms. Files: `NIGHT65.md`, `NIGHT67.md`.
GG (NIGHT65): "ok we need to plan detaile dbuild prompt for next 7 hours 6.5 min 7 hard stop. follow same methods on finding what to focus on, dont run into corners or repeat past mistakes, i wnat real progress and itterations and not just small polishes".
GG (NIGHT67): "need to build a another detailed 8 hour build prompt following same methods, area deisgn need t be better in alot of levels thats the mian focus".
Result: nights 65 to 70 spent much of their time on instruments (a verb pilot, a room contract meter) that could judge a room; L-150: "A room card's MAIN must be measured by an instrument that performs the room's verb".

### P-17. 2026-10-26. NIGHT71: "Bots guard, I judge"
Kind: process change. File: `NIGHT71.md`.
GG: "how is everything looking, use best methods online to anazlize what happened to  this pormpt (OPUS 5.5) follows those methods and what you think is best and the same methods used to build next prompt that doenst repeat same mistakes, i want amjroity building but wnat some time spent to kill dead code or things that we might have moved past (be careful with this) first you are makeing detailed prompt taht will do this".
His four calls the same morning, as the labels he picked (`docs/memory/OWNER.md`): "Clean old worktrees (Recommended)"; **"Bots guard, I judge (Recommended)"**; "Rooms as action blocks, Bosses and colour, Combat feel, Movement and camera feel"; "Dead code + old OFF rows (Recommended)".
Result: `Builds/n71-play`, the build GG plays, with eight rebuilt rooms (PLAY71.md); ten cleanup commits removed 359 lines.
GG's reaction after playing (OWNER.md, 2026-10-26): "make jump height higher, like 50%, also In the first stage that wall run section where there's kind of like a mini roof Can you remove it it just the area looks kind of ugly"; then "keep double jump and do the rest". Applied the same evening: jump x1.5 and an ULTRAKILL-like movement profile (`design.ultra71`, off switch `-skygrindNoUltra71`), roof removed (HANDOFF.md).

### P-18. 2026-10-27 to 2026-10-28. NIGHT72 to NIGHT74: a change goes ON only with a blind pair
Kind: refinement and rooms. Files: `NIGHT73.md`, `NIGHT74.md`, `NIGHT74-REPORT.md`.
GG (NIGHT73): "continue on prompt we were working on and follow best methods to build for next 3.5 hours, take your time and dont rush, I wnat real changes, imporvemnts and itterations, you have full control and dont need my input".
GG (NIGHT74): "what should overnight pormpt focvus on, use same methods weve wbeen using to make detailed prompts that actually imporve and buildt, 45 mins to build 8 hour night goal prompt".
Prompt text (NIGHT74 section 2): "By the close, **Builds/n74-play is the repo's game**". Honest aim stated up front: four level rows ON, at least three rebuilt rooms named by the spotter.
Result (NIGHT74-REPORT.md): four level rows ON with 21 rooms; "THE CROWN's idle cyan outside its weak point 5002 -> 843 px (-83.1 %)"; the report's own section 1 says "The rooms are still corridors and block faces with nothing that makes you use a mechanic".

### P-19. 2026-10-28. PG3: sound
Kind: mechanics (audio). File: `Assets/Scripts/Game/Pg3Audio.cs` header; commit `818051f66`.
GG, 2026-10-28, lifting CLAUDE.md rule 7 (no audio) for the assignment: "Lift it".
Result: one clip per EventBus event for the Reward, Damage and End lines, plus a music bed; 28 ElevenLabs prompts in `Tools/pg3/sounds.json` (section 5). Row `design.n74.sound74`, off switch `-skygrindRows72 sound74:0`. The commit is co-authored by Claude Opus 5.5.

### 2.1 The meta-workflow (how the prompts are produced and checked)

| Piece | What it is | File |
|---|---|---|
| Forge session | A session before a night that builds no game code. It reads the last nights, researches, drafts the night prompt, has a fresh reviewer attack it, and writes the `/goal` line. Folders `Research/<date>-forgeNN/` hold REQUEST, DECISIONS (D-1...), scouts, redteam, `slate-seed.json`, `builder-brief.md`, `GOAL.txt`. | `nights/PROMPT-FORGE.md`; skill `wave-forge`; `Research/2026-10-27-forge74/` |
| `/goal` end condition | A Stop-hook condition judged from the transcript. NIGHT74's: "Met when the latest output of `node Tools/night/n72-close-check.cjs --night n74` in this conversation ends in the line `CLOSE-CHECK n74: PASS` or `CLOSE-CHECK n74: MINIMUM PASS` and the last assistant text after it is exactly `NIGHT74 COMPLETE`". | `Research/2026-10-27-forge74/GOAL.txt` |
| Builders in worktrees | Up to three or four builder agents, each on its own branch in `.claude/worktrees/n74-R`, `-S`, `-B`, `-L`. Settings cap the fleet: `CLAUDE_CODE_MAX_CONCURRENT_SUBAGENTS` 6, spawn depth 1. The lead alone drives the Unity Editor and git and integrates by cherry-pick (builders cannot merge: memory note builder-merge-blocked). | `.claude/settings.json`; `docs/memory/LESSONS.md` L-033, L-117 |
| Blind spotter | `skygrind-spotter`: shown two unlabeled frames A and B, tool `Read` only, replies with one line naming the most obvious difference or "NOTHING OBVIOUS". A change goes ON only if the spotter names it. | `.claude/agents/skygrind-spotter.md` |
| Critic (the GATE) | `skygrind-critic`: judges one finished slice from its slate entry, frames, numbers and the SPOT line; sceptical by default; "a critic judges files, not the lead's sentences" (L-149). | `.claude/agents/skygrind-critic.md` |
| gg-proxy | Plays GG "from his own recorded words only", ranks the ten worst things on the newest frames, "Never praises". Used at the start and the close of a night. | `.claude/agents/gg-proxy.md`; `Logs/n74/gg-proxy-close.md` |
| Reviewer, dead-code auditor | `skygrind-reviewer`: a fresh read-only review of the night's integrated diff ("NIGHT61's run found 2 real bugs no gate could see"). `dead-code-auditor`: proposes deletions with evidence. | `.claude/agents/` |
| Guard hook | A PreToolUse hook (`guard.cjs`) that refuses what CLAUDE.md forbids; a SessionStart hook (`night-resume.cjs`) restores a night after a compaction or resume. | `.claude/settings.json`, `.claude/hooks/` |
| Memory | `docs/memory/`: CORE (loaded into every session), LESSONS (L-001 to L-159), OWNER (GG's words), THREADS, FEATURES, one ledger per wave. Skills `wave-memory` and `night-context`. | `docs/memory/` |
| Player reels | A built Player run with `-skygrindReel <dir>` writes every frame as a JPG on a fixed clock plus per-frame telemetry (1920 x 1080, 60 fps, 120 Hz sim in `reel.json`). The agents read frames and numbers; a blind spotter reads the pair. | `Tools/night/reel.sh`; `Logs/feel/n74-play/bk/reel.json` |

Models named in the record: the night prompts' header says "model / effort: claude-opus-5-5 / high"; the agent definitions set
`model: opus`, `effort: high`; commit trailers add Claude Sonnet 5.5 on 462 commits. This draft was written by Claude Sonnet 5.5.

---

## 3. Error recovery log

Each entry: what broke, how it was re-prompted or what rule was added, how it resolved, the source. L-numbers are in
`docs/memory/LESSONS.md`; "memory" means a file in the auto-memory folder
`C:\Users\gaige\.claude\projects\C--Users-gaige-dev-SKYGRIND-unity\memory\`.

| # | What broke | How it was re-prompted or what rule was added | How it resolved | Source |
|---|---|---|---|---|
| E-01 | The Unity Editor stopped answering when its window lost focus: every command timed out at 30 s, 0 % CPU (seen 2026-10-02, NIGHT14 at 01:13, NIGHT15 11:27 to 12:23) | A real foreground moment plus `set_autotick`; Run In Background on. In NIGHT61 (L-118) the rule became: the wake is one try; if it fails in 30 s, log it and take the no-Editor path | CLAUDE.md now says "Before Play checks: run `editor_focus`"; batch-mode builds and `dn-check` compile gates exist for when the Editor is stuck | memory unity-editor-unfocused-autotick; L-118; CLAUDE.md |
| E-02 | A recompile during Play mode reloaded the domain under the running scene: 27,000 NullReferenceExceptions in a minute; an unfocused Editor in Play froze the CLI for about 40 minutes | Rule: check `editor_status` first; `Tools/unity-loop.sh compile` stops Play mode itself and checks the DLL times | Added to CLAUDE.md "Never: recompile in Play mode" | memory unity-editor-play-mode-recompile |
| E-03 | NIGHT21 began with the Editor down and then blocked on Unity's modal "Recovering Scene Backups": about 45 minutes lost to a poll loop and, per L-025, 3 h of a 5 to 6 h night | Rule L-025: check the Editor is reachable in the first five minutes and ask GG at once if a dialog blocks it. Keyboard automation of the dialog was rejected (GG: "I'll click it myself") | GG clicked at 11:30; Unity wrote the backups to `Assets/_Recovery/` (still untracked) | NIGHT21-LOG.md lines 8 to 12, 100 to 105; L-025 |
| E-04 | NIGHT22 was cut by a computer crash about 5:40 PM: 0 of 9 contracts proven, no suite, no tag. Its Player threw a NullReferenceException at startup (`Ui.BuildCode22` read `results` before the menus built it) that Editor checks never showed | NIGHT23 was re-prompted as a 2 h sprint: "This sprint builds almost nothing new. It turns built code into proven code". Rule: after any UI change, Editor Play with 0 console errors, then one Player reel | The NRE appeared in the first reel minute; 8 of 9 NIGHT22 contracts went GREEN | NIGHT22-REPORT.md; nights/NIGHT23.md; memory player-reel-catches-startup-crash |
| E-05 | The account spend limit (HTTP 429) stopped all five builders at about 17:27 on 2026-10-06, 1:22 into a 5.5 h night; the lead made no progress until 20:55; builders had committed only their first slices | L-003: commit every finished slice, keep a lead-alone plan, budget the agents. Repeats: NIGHT20 (01:05 to 01:52), NIGHT43 (17:15 to 18:50), NIGHT63 (05:23 to 07:41), NIGHT66 (twice, 4:35 of 8 h lost) | L-098: three builders and one batched critic, resumed by SendMessage after a stop. Memory night-usage-window: a fresh window gives about 2:20 of agent work (2:20, 2:25, 2:24 over NIGHT66 to 68), so prompts put agent work before it | memory spend-limit-stops-agents; L-003; L-098 |
| E-06 | The PC ran out of virtual memory on 2026-10-22 17:32:28 (Volsnap event 6 / 15); the FORGE62 session ended that second; black screen at 18:08; restart at 20:17. Commit was 53 of 97.6 GB with three sessions open, about 20 MCP servers each | L-128: read the commit headroom as well as the disk before a night; launch with `--strict-mcp-config`. Later: a "low memory" stop was another session's pytest (NIGHT63), and pagefile growth ate the disk (NIGHT67: C: 11.7 to 4.8 GB free in 2:19) | Culprit never measured (memory pc-out-of-memory-crash says so); rules in L-128 | memory pc-out-of-memory-crash; L-128 |
| E-07 | NIGHT65: free disk fell to 13.2 then 8.7 GB, under the guard's 12 GB limit, so no build, reel or smoke ran all night; the room GATE read 0 of 6 | L-104: free the disk before t0; prune frames after every reel batch (`n57-prune.sh`). NIGHT71's forge deleted old worktrees (C: 7 to 70 GB) | NIGHT71 started with room to build and ran | L-104; NIGHT71.md (his call 1) |
| E-08 | Code that passed Editor compile and tests crashed every Player at startup: `LavaView30` threw 15,186 NREs (`new MaterialPropertyBlock` in a field initializer, L-035); `Kit33Props.S` recursed on float rounding and overflowed the stack at `SurvivalGame.Init` on every NIGHT33 level (L-044) | Rule: the Editor never loads a campaign level, so boot every changed level in a Player (`reel.sh` with `-skygrindReelOnly`). L-031: the level select's real reload path was never tested and 16 of 20 real presses failed | Level select Player check 4 of 20 to 20 of 20; later nights boot each changed level | L-031, L-035, L-044 |
| E-09 | A check was GREEN in the headless harness and wrong in the game: `verbs35` found the grapple never attached in the Player (RopeAttach 0) and the charge fired normal shots | L-052 "Nothing lost": write the verbs list (`docs/VERBS.md`) before any row lands and run one Player per map performing every verb | Verbs check went 13 of 23 to 23 of 23 over four runs; the principle became `docs/PRINCIPLES.md` rule 1 | L-052; NIGHT35 |
| E-10 | A Player key press lasts every sim step of its rendered frame, so edge-triggered rules (a cancel, a charge) saw several presses; held buttons toggled on even steps only | L-053: treat a run of pressed steps as one press and test "pressed on 3 steps"; L-039: hold for pairs of steps in reels | The grapple's fire press stopped cancelling its own latch | L-039; L-053 |
| E-11 | A change that made a verb late passed every count check: NIGHT35's charge made each KESTREL tap fire on release, 50 to 100 ms late, all night; GG then said "the pistol is not satisfying anymore" | L-055: measure when an input acts, not only whether; the `tap36` reel prints frames from press to shot. VERBS row 25: "the tap fires on the PRESS step" | Fixed in NIGHT36 (`tap36`) | L-055; docs/VERBS.md row 25 |
| E-12 | A colour that never showed: `Stone.shader` colours by vertex colour and ignores `_BaseColor`; THE STACK and seven boss views set a colour nobody read and drew white for about 20 waves | L-096: check what the shader reads. Fixed by builder L (`06017ad5`, `3d398a31`) | THE STACK's flicker fell 55 to 2 (NIGHT54) | L-096 |
| E-13 | False reds: a level row ON relabelled a shipped map, so `verbs37` read 25 / 29 RED with every node intact (29 / 29 with the old row); three rooms passed every guard and still failed `verbs37` in a Player | L-154: read the campaign order and the level file before calling a RED a lost verb. L-153: a guard is never the last check; re-run `verbs37` with the rooms in | L-154's RED read 29 / 29 with the old row (`-skygrindOff65`), so no verb was lost. L-153's rooms were reworked and NIGHT74's freeze read `verbs37` 6 / 6 PASS (NIGHT74-REPORT.md) | L-153; L-154 |
| E-14 | A cherry-pick took only the commit's diff, so a builder's earlier unpicked edit was missing (room `a3` regenerated byte-identical to the old level); a range pick re-picked everything; builders were refused `git merge` by the permission classifier | L-158: compare the file's blob id (`git rev-parse n74-X:<f>`) before the pick; L-117: start a range at the builder branch's own sha; integrate by cherry-pick only | `kb2c` and `a5` went out and came back fixed in NIGHT74 | L-117; L-158; memory builder-merge-blocked |
| E-15 | Inline `node -e` and heredoc edits were mangled by Git Bash (backticks ran as commands, `$$` expanded); the same mistake returned on 2026-10-22 | Rule 9 of CLAUDE.md (docs and code only by Write or Edit; multi-line patches in script files); L-114; then a hook | `.claude/hooks/guard.cjs` refuses a heredoc ("rule9-heredoc"). It blocked a heredoc in the session that drafted this file | memory patch-scripts-not-node-e; L-114; FORGE61 section 4 |
| E-16 | The lead ran `git checkout --` and discarded GG's own uncommitted edit to `UnityConnectSettings.asset` (NIGHT33) | L-047: never restore a file without reading its diff. The guard now lists eight of GG's files it will not stage, commit or restore | The line was restored by hand from a night-log record | L-047; `.claude/hooks/guard.cjs` (`GG_FILES`) |
| E-17 | A `git cherry-pick ... && ... &` chain backgrounded the whole chain and its output was lost, three times (NIGHT61 twice, FORGE62) | L-116: start a background job as its own statement; the guard's background-chain rule refuses it | Gates re-run into `Logs/n61/gates-after-V.txt` | L-116 |
| E-18 | A failed close commit let the tag `u3-n62` land on the commit before the close commit (`1b0696b5` instead of `cea57d63`) | L-133: chain a tag to its commit with `&&`, never `;`; the guard refuses to move a tag | Logged in NIGHT62-LOG.md | L-133 |
| E-19 | `TaskStop` killed only the outer shell: orphaned jobs locked `SimHarness.dll` (NIGHT52) and a nested reel script ran a second Player; in NIGHT35 the lead closed a live Player as an "orphan" and lost a verbs line | L-088: list the tree and stop each job by PID; L-054: never close a Player you did not identify by command line and start time | Rules in CLAUDE.md rule 10 ("Close every Player you launch by its PID") | L-054; L-088; memory taskstop-orphans |
| E-20 | NIGHT12's `/goal` demanded things that became unreachable (a bench figure, one tag); the checker never answered "impossible", every turn end was blocked and Claude Code's cap overrode it ("A hook blocked the turn from ending 9 consecutive times"); GG kept the session about 18 h with the goal active | Rule: write a goal that can always be met (done, reported short, or the hard stop) and clear it after a night | Later goals carry a clock clause, for example FORGE61's "Also met at a printed `date` 8:00 after t0 if NIGHT61-REPORT.md is committed, whatever is done" | memory goal-condition-satisfiable; `Research/2026-10-22-forge61/GOAL.txt` |
| E-21 | An AI judge of stills did not predict GG: blind, it preferred a build GG called "awful" to one he called "a little bit better" (NIGHT57). In NIGHT74, 15 rooms passed the guards and the blind pair, and the close look still called g8 "random pillars" and a3 clutter | L-002, L-105: blind reviewers veto defects, they do not approve looks; L-159: the pair proves "different", not "asks you to move", so put gg-proxy on each room before the flip | Open: NIGHT74-REPORT section 7 asks for a room judge that asks "does it ask you to move" | L-002; L-105; L-159 |
| E-22 | A builder tool threw and wrote nothing; only its last line was printed, so the first `verbs37` ran with every row OFF and 15 minutes were lost (NIGHT46) | L-081: a tool that edits a build's copy must refuse loudly and the lead reads its whole output; the tool was rewritten to parse the JSON | Fixed in `Tools/night/n46-build-rows.cjs` | L-081 |
| E-23 | GG reversed work the agents had shipped: on 2026-10-09 he asked for the old worm and not the Tripo one (DECIDE47 D-9's wording); and the NIGHT25 melee-and-fire row ("You cannot shoot a weapon and melee at the same time, you have to wait", OWNER.md) | The worm view row was switched off (DECIDE28 D-4); `meleeFire` set to `enabled: false` (config), so pad layout v5 stays off | Tripo FBX and view code kept; the live pad layout is v4 | DECIDE28; OWNER.md; `Design/unity-config.json` `design.n25.meleeFire` |
| E-24 | The benchmark noise was the poller: a 2 s power-plan poll moved a result from 7.19 to 4.81 ms; the plan also flips from Silent to Balanced silently | Rule: poll the power plan before and after a run only; median of 3; never bench beside the suite (L-005) | CLAUDE.md "Benchmarks" paragraph | MEMORY.md index (bench-power-plan-flips); L-005 |
| E-25 | The Player smoke failed steps whenever a window took focus or a pad was plugged in, so a failure had no meaning alone | L-015: run the smoke alone; L-072: judge a smoke failure only beside a control run on the last good build; L-099: a planted focus steal must take the foreground and print it | The smoke was made focus-proof in NIGHT55 (IgnoreFocus) | L-015; L-072; L-099; ledger/wave-55.md |

Not found: the brief names a Windows case-insensitive file collision. I searched `docs/memory`, the night logs and `HANDOFF.md` and found none.
`TODO(GG)`: if it happened (for example two files differing only in case), add it here with its date.

---

## 4. Tripo 3D prompts

Status: no Tripo prompt, route, credit count or generation date is recorded anywhere in the repository. The only Tripo
output in the project is the Kiln Wyrm (row 1 below), which GG supplied on 2026-10-08. Rows 2 to 4 are the templates for the
PG3 models; every unknown is `TODO(GG)`. Tripo's free plan makes generated models public; say which plan each model was made
on (see `ATTRIBUTION.md`).

| Asset | Prompt (exact text) | Route | Format pipeline | Placement in the game | Credits before | Credits after |
|---|---|---|---|---|---|---|
| THE KILN WYRM, head, body, tail (made 2026-10-08, before PG3) | `TODO(GG)` | `TODO(GG)`. The files are named `tripo_convert_<uuid>.fbx`; the texture sets are named `wormboss_head_*`, `wormboss_body_*`, `wormboss_tail_*` | Three FBX files in `Assets/Art/WormBoss/Head`, `Body`, `Tail` (first committed in `8eff2485c`, 2026-10-08), each with a `.fbm` folder of PBR maps (base colour, metallic, normal, roughness, `rm`). Baked into `Assets/Resources/WormBossParts.asset` by the menu `SKYGRIND > Worm Boss > Build Prefab` (`Assets/Editor/WormBossSetup.cs`) | THE KILN WYRM (level 3 boss), drawn by `Game/WyrmVisual26.cs` only when `design.n26.wormVisual.enabled` is true; it is `false` now, so the game draws the blocky segmented worm | `TODO(GG)` | `TODO(GG)` |
| KESTREL (`kestrel.fbx`) | `TODO(GG)` | `TODO(GG)`: tripo3d.ai direct, or generate-and-import inside Unity | `TODO(GG)`: export `.fbx`, copy to `Assets/Art/Tripo/kestrel.fbx`, set scale and materials on import | The player's pistol, first weapon. Today it is drawn from code (`Game/BlockyKit.cs`, `BlockyKit.Guns52.cs`). `TODO(GG)`: replace the viewmodel, or use only in the trailer | `TODO(GG)` | `TODO(GG)` |
| husk (`husk.fbx`) | `TODO(GG)` | `TODO(GG)` | `TODO(GG)`: `Assets/Art/Tripo/husk.fbx` | The basic enemy. Today it is the pill-language model built on NIGHT43 (DECIDE43 D-4: GG's G21). `TODO(GG)`: say whether the Tripo husk replaces it or is a showcase only; CLAUDE.md rule 8 reserves red-orange for enemies | `TODO(GG)` | `TODO(GG)` |
| THE WICK (`wick.fbx`) | `TODO(GG)` | `TODO(GG)` | `TODO(GG)`: `Assets/Art/Tripo/wick.fbx` | The level 1 boss, a 40 m furnace stack you climb (`docs/BOSSES47.md`). `TODO(GG)`: in the level, or the trailer's hero shot only | `TODO(GG)` | `TODO(GG)` |

What the repo records about the one existing Tripo import (measured by parsing the FBX in `Research/2026-10-08-wave26/WORM-VISUALS.md`):
Z-up raw with a node rotation of -90 about X, metres, pivot at the feet; head 0.54 wide, 0.86 tall, 0.98 long, facing +Z; the
body's tube runs along X, so the setup script turns it a quarter turn; the tail runs along Z. File sizes in the commit: body
65,580 bytes, head 107,852 bytes, tail 52,828 bytes. The Player shader reads the albedo only; the normal, roughness and metallic
maps are used by the prefab's URP Lit materials.

A Coplay MCP tool named `generate_3d_model_from_text` is available in the agent's tool list for this session. Whether GG used
it or tripo3d.ai directly: `TODO(GG)`; not checked.

---

## 5. Audio prompts

Route: the ElevenLabs sound-generation API, called by `Tools/pg3/sfx-gen.cjs` from `Tools/pg3/sounds.json`
(`POST https://api.elevenlabs.io/v1/sound-generation?output_format=mp3_44100_128`, body `text`, `duration_seconds`,
`prompt_influence` 0.5, `loop` true for the music bed). Output: `Assets/Resources/Audio/PG3/<id>.mp3`; the script appends each
request to `Tools/pg3/sfx-gen-log.json` (prompt, seconds, HTTP status, bytes, time). That log did not exist when this draft was
written (no clip had been generated yet), so the status column is `TODO(GG)`: paste it from the log after the run.
GG's words that lifted the no-audio rule (header of `Pg3Audio.cs`): "Lift it". The 28 prompts below are verbatim from
`sounds.json`; they were committed in `818051f66` with Claude Opus 5.5 as co-author. `TODO(GG)`: say which prompts you wrote
or edited yourself.

The game side (`Game/Pg3Audio.cs`, `Game/UnityTuning.Pg3.cs`) plays one clip per EventBus or mover event, with a pool of 10
voices, volume 0.8, a 45 ms minimum repeat gap, and a five-step pitch walk (minus 2 to plus 2 times 0.035) so repeated kills do
not sound copy-pasted; a missing clip is silent.

### 5.1 Reward events

| id | Game event | Seconds | Prompt | Generation status |
|---|---|---|---|---|
| hit | `hit` | 0.3 | Satisfying hitmarker tick, a crisp short high click with a soft fleshy thud underneath, game UI feedback | `TODO(GG)` |
| kill | `kill` | 0.7 | Crunchy enemy kill confirm, a stone body shattering with a bright rising chime on top, very satisfying, game sound effect | `TODO(GG)` |
| points | `points` | 0.4 | Quick coin-like digital points blip, two rising bright notes, arcade game reward | `TODO(GG)` |
| buy | `buy` | 0.8 | Sci-fi vending purchase, a mechanical register clunk followed by a bright confirming chime | `TODO(GG)` |
| powerup | `powerUpGrab` | 1.2 | Power-up pickup, a shimmering rising synth sweep with sparkles, energizing, game reward sound | `TODO(GG)` |
| pickup | `pickup29` | 0.6 | Small item pickup, a soft glassy chime with a short whoosh, game sound effect | `TODO(GG)` |
| weapon-earned | `weaponEarned` | 2.0 | New weapon unlocked fanfare, heavy metallic latch then a triumphant short synth brass stab, game reward | `TODO(GG)` |
| room-clear | `roomClear` | 1.5 | Arena cleared stinger, a deep boom then a rising bright synth chord, victorious, short | `TODO(GG)` |
| parry | `parry` | 0.7 | Perfect parry, a sharp metallic clang with a time-slowing reverse whoosh, crystalline ring, game sound effect | `TODO(GG)` |
| boss-hit | `bossHit` | 0.5 | Bullet hitting a giant armored boss, heavy metallic clank with a deep resonant ring, game sound effect | `TODO(GG)` |

### 5.2 Damage events

| id | Game event | Seconds | Prompt | Generation status |
|---|---|---|---|---|
| hurt | `hurt:player` | 0.5 | Player takes damage, a muffled heavy body impact with a short distorted crunch and a grunt-free low thud, game sound effect | `TODO(GG)` |
| enemy-windup | `huskWindup` | 0.6 | Monster attack wind-up, a raspy growl rising in pitch, warning cue, game sound effect | `TODO(GG)` |
| boss-intro | `bossIntro` | 3.0 | Giant boss monster entrance, a deep earth-shaking roar with metal grinding and a huge low drum hit, cinematic | `TODO(GG)` |

### 5.3 End events

| id | Game event | Seconds | Prompt | Generation status |
|---|---|---|---|---|
| boss-down | `bossDown` | 3.0 | Giant boss defeated, a massive collapsing explosion, metal groaning, then a deep triumphant boom, cinematic | `TODO(GG)` |
| win | `exfilDone` | 3.5 | Victory stinger, a bright rising synth arpeggio into a big warm major chord, triumphant, video game level complete | `TODO(GG)` |
| lose | `death:player` | 3.0 | Game over stinger, a heavy low hit then a slow descending detuned synth tone fading into silence, defeat, video game | `TODO(GG)` |
| retry | `roomRetry` | 1.0 | Respawn whoosh, a reversed swell into a soft digital materialize shimmer, game sound effect | `TODO(GG)` |

### 5.4 Weapons and movement (the "feel" line in `sounds.json`)

| id | Game event | Seconds | Prompt | Generation status |
|---|---|---|---|---|
| fire-kestrel | `fire:revolver` | 0.8 | Punchy sci-fi revolver gunshot, tight low-end thump with a bright metallic crack and a short electric zap tail, close mic, game sound effect | `TODO(GG)` |
| fire-thunderclap | `fire:shotgun` | 1.0 | Heavy pump shotgun blast, huge boomy low end, crunchy distortion, quick reverb tail in a large concrete hall, game sound effect | `TODO(GG)` |
| fire-gauntlet | `punch` | 0.6 | Powered metal gauntlet punch, a whoosh into a hard meaty impact with an electric crackle, game sound effect | `TODO(GG)` |
| fire-torch | `fire:torch` | 1.0 | Short roaring burst from a flamethrower, gas whoosh and crackling fire, game sound effect | `TODO(GG)` |
| pump | `shotgunPump` | 0.5 | Shotgun pump action, two crisp mechanical clacks, metal on metal, close mic | `TODO(GG)` |
| jump | `mover:jump` | 0.3 | Light athletic jump, a quick cloth whoosh with a soft boot push-off, game sound effect | `TODO(GG)` |
| dash | `mover:dash` | 0.5 | Fast air dash, a sharp sci-fi whoosh with a jet burst, game sound effect | `TODO(GG)` |
| land | `mover:land` | 0.3 | Boots landing on concrete, a short solid thud, game sound effect | `TODO(GG)` |
| slam | `mover:slam` | 0.8 | Ground slam, a heavy body hitting the floor from height with a concussive boom and debris, game sound effect | `TODO(GG)` |
| grapple | `mover:ropeFire` | 0.6 | Grappling hook firing, a pneumatic thunk and a cable zipping out fast, game sound effect | `TODO(GG)` |

### 5.5 Music bed

| id | Game event | Seconds | Prompt | Generation status |
|---|---|---|---|---|
| music-loop | `music` (looped; 0.32 volume in a run, half of that on menus, 0.8 s fades) | 22.0 | Dark driving synthwave combat loop at dusk, pulsing bass, punchy drums, tense arpeggios, 128 bpm, seamless loop, instrumental | `TODO(GG)` |

Earlier audio work (PG2, the web repo, not used in the Unity build): `MUSIC-BRIEF.md` there describes a five-note motif
(A3, E4, F4, E4, A4) with title, combat and victory sketches in `music/*.wav` and a "Suno workflow". `TODO(GG)`: say whether
you generated tracks with Suno in PG2 and whether any of them is in this build (none is in the Unity project).

---

## 6. Trailer shot prompts (45 s)

Status: no trailer file, shot prompt or image-to-video tool is recorded in the repository. The shot list below is the plan from
the assignment brief; each source named is a file that exists. Frame rate and size of the game footage available from the reel
system: 1920 x 1080 at 60 fps JPG sequences (`Logs/feel/n74-play/*/reel.json`), or a live capture of `Builds/n74-play/SKYGRIND.exe`.
Assembly tool on this machine: ffmpeg 9.0.2 (WinGet package `Gyan.FFmpeg`). `TODO(GG)`: the exact ffmpeg command you used.

| Shot | Time | What it shows | Source | Prompt or tool settings |
|---|---|---|---|---|
| 1 Hook | 0 to 3 s | A dusk arena, a slow push-in, AI image-to-video from one still | A still picked from the moodboard (`REFERENCES.md` section 5), for example `Logs/n74/pairs/LV-iceline67/on.png` | Tool: `TODO(GG)`. Starting image: `TODO(GG)`. Prompt: `TODO(GG)`. Seed and length: `TODO(GG)` |
| 2 Title | 3 to 6 s | SKYGRIND wordmark: Archivo Extra Condensed Black Italic on near-black ink, 12 degree slant (`IDENTITY.md`) | Made in an editor or from the in-game title | `TODO(GG)` |
| 3 Movement | 6 to 18 s | Wall ride, dash, slide, a launch pad, with the world speed streaks | Player capture or reel frames of levels 1 and 2 (`Logs/feel/n74-play/tf`, `ti`) | Capture settings: `TODO(GG)` |
| 4 Reward and damage | 18 to 28 s | A kill (white marker, points pops), a purchase, a hit taken (red-orange vignette rim), enemy tell | Player capture; sound from the Reward and Damage rows of section 5 | `TODO(GG)` |
| 5 Boss | 28 to 36 s | A boss weak point open (cyan ring, `weak74`), the kill flash (`kill74`: white for two frames, a 0.35 s hold) | `Logs/feel/n74-play/bp/boss63` (THE CROWN), `bk/boss63` (THE KILN WYRM) | `TODO(GG)` |
| 6 Hero | 36 to 41 s | THE WICK from Tripo, a slow turn or push-in under the dusk grade | `Assets/Art/Tripo/wick.fbx` (not yet in the repo) | Tripo prompt in section 4; render settings `TODO(GG)` |
| 7 End card | 41 to 45 s | Title, platform, the Release link, the course and game name | Made in an editor | Text: `TODO(GG)`. Link: `TODO(GG)` |

Audio for the trailer: music bed `music-loop`, then the `win` stinger on the end card (prompts in section 5). Sound is generated,
not recorded.

---

## 7. Reflection

### DRAFT for GG to rewrite in his own voice

(Every fact in it comes from sections 1 to 6 of this file; the first person is a placeholder.)

SKYGRIND began as a browser game, written in four days in plain JavaScript and three.js for PG2. For PG3 I moved it to Unity,
and most of what changed was who does what.

The AI wrote nearly all of the code: a C# port of the browser simulation kept bit for bit against recorded runs, then five
levels, five bosses, a test suite and measuring tools, across 74 nights and more than 4,200 commits. It also researched,
drafted each night's prompt, and checked its own work with blind reviewers.

I did the parts a machine cannot judge. I set the direction and reversed it when it felt wrong: bosses cut to about two
minutes, the Tripo worm swapped back for the old one, the pad layout reverted. I played the builds and named what was bad in
plain words. I owned the art, sound and enemy design, supplied the Tripo models, and for PG3 lifted my rule against audio so
the game could have sound from ElevenLabs.

The engine moved the line. In a browser the AI could run the game itself and I mostly pasted errors. In Unity, code that
passed every test still crashed the Player at startup, the Editor froze when it lost focus, and the AI could not see
Editor-only parts, so every proof needed a Player build and a command line into the Editor. My job moved from debugging to
judging taste. The AI's guards proved a room was different, but not that it made you move; I still had to say so.

(The four paragraphs above are 258 words, counted with `wc -w`. Recount after you edit; the rubric asks for 200 to 300.)

---

## 8. Open items for GG

1. Sections 4, 5 and 6 need your real prompts, routes, credit counts, tool names and links (every `TODO(GG)`).
2. The audio prompts in section 5 were written by the coding agent in `818051f66`; mark which are yours.
3. `Tools/pg3/snapshot.sh` copies `NIGHT*.md`, `DECIDE*.md`, `FORGE*.md` and `EXT*.md` from the repo root only. The earliest
   prompts, NIGHT7 to NIGHT43, live in `nights/` and would be missing from the snapshot's `prompts/` folder. Copy them there if
   the prompt log is graded against the snapshot.
4. The audio loader reads `Assets/Resources/Audio/PG3/`; the brief says `Assets/Audio/PG3/`. Pick one and say which in the README.
5. CLAUDE.md rule 7 still reads "no audio code"; your "Lift it" is recorded only in `Pg3Audio.cs` and commit `818051f66`.
