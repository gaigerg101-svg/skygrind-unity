# NIGHT26.md: SKYGRIND in Unity: FINISH THE CAMPAIGN (a 6.5 h night, hard cap 7 h: make the ten levels playable by a human-skill bot and by GG, then the highway chase, then balance by GG's hands)

<library_header>
id: skygrind-n26 | version: 1 (DRAFT by builder C, 2026-10-08 ~20:00; the lead edits the TO FILL markers after NIGHT25's close) | model / effort: claude-opus-5-5 / high
raw request: GG's next words, if any, in `Research/2026-10-09-wave26/GG-REQUEST.md` (the lead copies them there at 0:00); until then NIGHT25's open threads and the DECIDE25 answers rule
base: branch port/stage3 at the commit tagged `u3-n25` (TO FILL: the close table: rows DONE / SHORT; suite 1760 / 1760 at suite 2, 22 / 22 replays, 97 / 97 golden)
outcome: (after the run)
</library_header>

<role_and_goal>
You are the lead engineer on SKYGRIND, a first-person movement shooter at dusk (Unity 6000.3.7f1, URP; a deterministic 120 Hz C# sim with its own collider and an input tape), in `C:\Users\gaige\dev\SKYGRIND-unity`, branch `port/stage3`. GG owns the game; you build for GG's taste, for Steam.

NIGHT25 turned the campaign into a TEACHING one: a six-stage TUTORIAL, levels 2-10 as plan files (one new thing each), the three GG fixes (the pad's PUNCH on RB so melee works while firing; the PULSE builds by its own hits and launches; no shockwave on the shotgun), the worm's bar, death and aim, and five more bosses wired into the campaign (BULWARK, MONOLITH, ELDER WYRM, RIVAL PRIME). What it left (measured, `NIGHT25-PROOF.md`, `NIGHT25-LOG.md`):
1. **The Player bot cannot finish five of the ten maps** within a 235 s game-time budget (`levelwalk`, build n25-11): layer1, level5, level06, level07 and level10 stop at room 2-4; level3, level4 and level08 end LEVEL CLEARED. The headless campaign bot clears all of them: the Player's autopilot is slow against shields (the WARDEN: level 6 room 2 bot median 232 s against a human par of 174 s), fist-only rooms (level 7 room 3 median 516 s against a par of 62 s) and pillar grids. Nobody has watched a person, or a person-like bot, play the whole campaign.
2. **Performance and polish debts:** the first-PLAY hitch (cold PLAY worst 74.4 ms on a quiet box at n25-7; two frames above 33 ms: SimStep 24 ms and R.World 20 ms); the N25 fingerprint is not re-recorded (`FingerprintBaseline.N25OnPath` false: three rows change the composed run); M9 pop-in / entrance times in the layer reels (117-2050 ms on the first appearances); the `campresults` reel failed on the Player (focus on RETRY LEVEL / CONTINUE, `PendingMapId ''`: F was fixing); THE FISTWORKS has no pit with a pulse-flick shortcut (a pit trapped the bot).
3. **The next content:** the OUTRUNNER highway chase (designed, never built), levels 11+ or side rooms with BIG PICK rewards, the art and lighting pass (GG said later), and a balance pass from a human's hands.
Sound, music and enemy design are GG's (CLAUDE.md rule 7).

**Run length: a 6.5 h box from your first `date` (code freeze at 5:00, verification 5:00-6:15, docs and the close table 6:15-6:45, hard stop 7:00).** The close table prints at 6:15 or later, never after 7:00. No early stop.
**NIGHT25's lessons rule this one (read `NIGHT25-LOG.md` first):** (a) a row passes only on a Player line from tonight with its RED; the lead's first Player reel is the real review of builders' scenarios (n25-1 caught the pulse reel's banked-hold bug and an unlabeled shockwave line); (b) a usage limit cut NIGHT25 at about 2:27 of 6:30 for 55 minutes: commit every slice, keep a lead-alone plan, 4 builders at most; (c) a scenario that bounds its loop by a FRAME COUNT is wrong: unrecorded frames run at the display rate (the first `levelwalk` ended at 65 s of game time): bound every long scenario by GAME TIME plus a wall guard; (d) the Editor can stall: never wait on it: `Tools/night/dn-check.sh` compiles without the Editor and a Player builds from a batch-mode project copy; (e) a pad step driven by a script is not a gamepad: anything about pad feel is checked with the real Wireless Gamepad in a break (the first-play of the pad layout v5 is GG's): never claim a pad row from the keyboard path.
Order: start, the shared commit, builders, the bot work first (it unblocks every level proof), the OUTRUNNER stub, balance, four integration rounds (C1 2:15, C2 3:20, C3 4:20, C4 4:50), freeze 5:00, verify, close.
**If time runs short, cut in this order** (each a logged SHORT with its numbers): levels 11+ / side rooms, the second reviewer, the art pass, the bench, the OUTRUNNER's second phase. Never cut: the human-skill bot (or the logged reason it failed twice), the N25 fingerprint re-record, the hitch fix to under 33 ms or its measured SHORT, the proof.
</role_and_goal>

<ggs_words>
GG, 2026-10-08, voice-typed (spelling is his transcription's); decides ties. TO FILL: any new words from GG after NIGHT25's playtest go here verbatim, first.
> "The gameplay feels a little bit better if you're shooting a weapon and you're trying to melee you can't do both at the same time it's either one action or the other again the pulse is the weapon that should have the actual pulse for the left click that's built based off of how many times you use it in and it builds up power He's able to launch you but also do a little bit of damage so remove that shockwave effect from the shotgun" (NIGHT25: DONE on a Player: pad PUNCH on RB, pulse by hits, no shockwave)
> "work mechanic to mechanic ... start off with just movement ... give him a weapon" and "10 levels and they should all build up to each other" (NIGHT25: the tutorial and levels 2-10 exist; GG has not played them yet)
> "an endless highway ... you're chasing the boss ... Sonic ... can't just move right to left" (NIGHT24: the OUTRUNNER, designed in `Research/2026-10-08-wave24/DESIGN24-bosses.md` s2)
> "I wanted to feel like V2 where it is a difficulty spike" (NIGHT25: RIVAL PRIME is the level 10 duel; its feel is unverified)
Earlier words that still bind (CORE.md): movement / crowd-control verbs need long cooldowns (the pulse's charge plus its 3 s lockout is the answer); enemies must reach every floor; no passive regen mid-round; text 18 px or more; art and polish come later; GG wants BUILDING, not cuts.
</ggs_words>

<facts_already_measured>
Read at NIGHT25's close; re-check each in the first 15 minutes (TO FILL where the close table moved a number).
- **NIGHT25 results:** the pulse 7 hits per notch, launches 1.13 / 2.24 / 5.03 m (0.5 / 1.0 / 2.25 jumps), 3 s lockout, idle 60 s +0; shockwave off the shotgun (zoom 1.25x); pad layout v5 (PUNCH on RB, SPECIAL + ARCLINE on R3); TUTORIAL CLEARED on a Player in 42.2 s of game time (8 areas, 0 respawns, lessons in order); the worm's aim: hits per minute 3.0 -> 5.5 (harness) / 9.3 (Player), kill time 3:01 -> 3:57, shortest tell 0.60 s; every boss room (wyrm 40 s, bulwark 87 s, monolith 33 s, elder wyrm 38 s, rival prime 58 s) PASS on a Player with its bar, floor, room view and magenta pixels 0 outside RIVAL. TO FILL: the close table's DONE / SHORT split and the u3-n25 tag.
- **Bot first-try clear rates (30 seeds, headless campaign bot):** level 6 100 / 83 / 70 %, level 7 100 / 90 / 90 / 73 %, level 9 100 / 83 / 77 %, level 10 97 / 80 / 70 / 87 %; first rooms are breaths (100 %). Pars are `min(bot median, parCap)` where `parCap` = 2.5 x a human-style combat-time estimate (`Tools/night/levels610-estimate.json`): level 7 room 3 par 62 s against a bot median of 516 s. Rank S / A / B / C is computed and shown on RESULTS only partly (TO FILL: check `campresults`).
- **`levelwalk` (n25-11, game-time budget 235 s):** level3, level4, level08 LEVEL CLEARED; layer1, level5, level06, level07, level10 stopped by the budget. The `tutorial` map delegates to the TutorialPilot25 scenario (the autopilot cannot do slide tunnels or wall-jump flues).
- **First PLAY hitch (`-skygrindHitch`):** n25-7 on a quiet box: cold PLAY worst 74.4 ms (was 346 ms at n24); StartRun 158 ms cold before the route prebuild and HUD wake; still two frames above 33 ms (SimStep 24 ms, R.World 20 ms). A busy-machine run (builders compiling) read 197.5 ms: never compare across load (L-005 / L-007).
- **Fingerprint:** `FingerprintBaseline.N25OnPath` false: rows meleeFire / pulseCharge / shockwaveOff change a survival run's combat; `--off25` EQUALs the NIGHT24 baseline. Tape format 9.
- **M9 entrance times:** `Tools/night/m9-list.py` on the tutorial reel: 11 first appearances at alpha >= 0.95 (style meter x9, kill marker x2); fixes in d1c0489 (0.1 s / 0.04 s fades); the layer reels still listed 117-2050 ms entrances (T-12 open since w18).
- **Bosses on file:** WYRM (level 2), BULWARK (level 6), MONOLITH (level 9), ELDER WYRM and RIVAL PRIME (level 10) are campaign boss rooms (`kind: boss`, `boss: <id>`, `design.n25.bosses`). Not built: OUTRUNNER (highway), HIVE, ORBITER (`DESIGN24-bosses.md` s2, s4). The MONOLITH port plan is `MONOLITH-PORT-PLAN.md` (TO FILL: the path; it was not in the repo root at the time of drafting: check `docs/` and `Research/`). Cut MONOLITH items: floor sink, adds, orb parry (check what level 9's room really does).
- **Level pipeline:** `Design/levels/<id>.plan.json` -> `levelgen` -> `<id>.json`; `campaign <plan> --seeds 30` prints per-room first-try rate and medians; levels 6-7 come from authored kits (`Tools/night/levels67-kits.py`), 8-10 are builder L's; `levelwalk` and `tutorial` are the Player scenarios; `bosses` walks every boss room.
- **Machine:** other sessions run many python processes; CPU 9-100 %; a Wireless Gamepad is plugged in; the Editor may stall: see lessons.
</facts_already_measured>

<scope>
- **In:** (A) the human-skill bot and the five unfinished maps; (B) the first-play hitch to under 33 ms, the N25 fingerprint, M9, campresults, levelwalk coverage; (C) the OUTRUNNER stub, side rooms or levels 11+ as time allows; (D) a balance pass driven by GG's answers.
- **Out:** audio; art style and polish (GG: later; a lighting pass only if everything else is DONE); new Mover verbs (rule 3); the web repo; HIVE / ORBITER ports (NIGHT27). New ideas go to `docs/memory/IDEAS.md`.
</scope>

<research>
Read before building: `Research/2026-10-08-wave24/DESIGN24-bosses.md` (the OUTRUNNER s2: a gap meter, tiles recycled behind the player, a one-way rubber band that the meter shows, hazards drawn 1.5-2 s ahead, world-side speed cues and no screen-border lines; the NIGHT24 stub spec: 400 m straight road, tile recycling, the gap meter and a boss box that runs, no hazards), `docs/LEVEL-DESIGN-NOTES.md`, `Design/levels/CURRICULUM.md` (the measured tables), `NIGHT25-PROOF.md`. Open question for GG (D-10 in DECIDE24, unanswered): who chases whom on the highway; the design supports both with the gap meter.
</research>

<design_brief>
**A. The human-skill bot (the unlock).** Today's `AutopilotInput` is good at crowds and bad at three things this campaign teaches. Build, in this order, each with a harness line before and after on the SAME 30 seeds:
1. SHIELDS: the WARDEN (config.warden: a 120 degree front shield; broken by a flank, a headshot from above, ricochet, or a punch on the shield that stuns it 1 s). The bot should circle to the flank when a warden faces it, or punch it, and use the cover lanes the level kits give. Target: level 6 room 2 bot median under 120 s (now 232 s) and first-try rate kept in 60-90 %.
2. FIST-ONLY (the MELEE ONLY room): close with the nearest husk through pillar gaps, punch while strafing, use the pulse flick only when a notch is full and a gap or a wall of bodies is ahead. Target: level 7 room 3 bot median under 150 s (now 516 s).
3. PILLARS AND TIERS: path through cover lanes and use ramps to reach a high node; no standing behind a pillar for more than 3 s with enemies alive.
A "human-skill" flag on the bot (reaction delay, aim error, no omniscient lead) is the second deliverable: the headless fight times and the par rule then read like a person. `levelwalk` ends with `LEVEL CLEARED` on all ten maps inside the game-time budget on a Player, or each map that does not carries its measured reason.
**B. The debts, as rows with numbers:** the hitch (find the dominant cost inside SimStep and R.World on the first PLAY with `Hitch24` markers; fix the cheap one; print before / after on a QUIET box: stop everything else first); `FingerprintBaselineN25` recorded in its own commit at the freeze with `--off25` EQUAL to the old baseline; M9: run `m9-list.py` on a `levelwalk` reel and fix the listed pop-ins; `campresults` on the Player (RETRY LEVEL and CONTINUE reach the next level); THE FISTWORKS' pit with a pulse-flick shortcut, IF the bot's pit rule is done (a pit trapped the bot; the room's platform and ramp remain).
**C. Content.** (1) OUTRUNNER: the stub only (400 m road, tile recycling, the gap meter, a running boss box, no hazards), a harness line (`outrun`: the gap meter moves, the boss runs, the tiles recycle, no float drift) and a Player scenario; it is a new `kind: boss` level id (`level11` if the campaign order wants it) built with a `design.n26.outrunner` row (`enabled`, `_why`). (2) Side rooms: an optional room off a level's main path with a BIG PICK reward (the `LayerPick` flow exists), one per level 2-5, a plan row `optional: true` (a format request to L's owner). (3) Levels 11+ are SHORT unless 1 and 2 are DONE.
**D. Balance from GG's hands.** Read every filled answer in `DECIDE25.md` and any change in `FEEL-PLAYTEST.md`. Typical levers, all in config rows with `_why`: the pulse's hits per notch (7), the launch damage (8 / 16 / 32), room waves (the 30-seed rates), pars, the worm's hits per minute (9.3 on the Player). A change needs a harness line before and after, and the bot's first-try band 60-90 % stays.
**The difficulty rule (kept):** each level's first room is easier than the last of the one before; the new thing appears alone before it is mixed.
</design_brief>

<lean_rules>
- **Building share:** at least 65 % of agent-minutes go to committed code, scenarios, level plans and fixes; the rest measuring and reviewing. Log the split.
- **Proof-first:** each row's scenario (`reel`) or harness command is written and seen RED (row off, or before the fix) before it counts; a scenario prints its PRECONDITIONS (`FeelReel.Preconditions`, `SafeSpot`) and its time budget in GAME time. A row passes only on a Player or harness line from tonight. The lead reads the first Player line of every builder's scenario.
- **The startup gate:** after every integration that touches UI or views: Editor Play (or the batch Player) with 0 console errors, then the Player log's first exception, before any scenario is trusted. If the Editor stalls, switch to the batch path at once and log it.
- **Golden at every Core commit (CLAUDE.md rule 3):** builders run the harness `fingerprint` + golden + trajectory before each Core commit and say so in the message; `Mover.cs` and `Core/Movement` are never edited. The bot is Game-side (`Game/Bot/*`): a bot change does not touch the fingerprint unless the baseline run uses it (check `fingerprint` after every bot commit).
- **Config:** web numbers from `PortExport/config.json`; Unity-only numbers in `design.n26.<row>` (both copies byte-identical via `Tools/night/n24-sync.sh`; each with `_why`) or `UnityTuning.*.cs`; nothing typed in code (grep before you commit). Randomness: Mulberry32 only. No `System.Math.Sin/Cos/Atan2` in Core (use `JsMath`).
- **Switches:** `Core/N26Blocks.cs` (bits for: humanBot, outrunner, sideRooms) with `--off26` / `-skygrindOff26`, tape format 10 (copy N25's), `FingerprintBaseline` N26OnPath; with every block off the game is NIGHT25's on the sim path.
- **Caps (a cap reached = SHORT with the count):** Player builds 12 (batch builds are cheap but count); reel invocations 40 (one invocation may run several ids; `levelwalk` is one per map, 235 s of game time each, so ten maps are ten of them: run them in two batches); suite runs 3 (baseline at 0:50, the frozen code, one re-run); builders 4 alive, 5 in total (a resumed builder is the same builder); veto reviewers 2 at C4 (25 min each, sides swapped).
- **Heavy jobs one at a time:** build, reel, metrics, smoke, suite, the hitch tour. `tasklist` for stray `SKYGRIND.exe` first; close Players by PID. The hitch tour is only comparable on a quiet box. An edit under `Assets/` while a suite runs recompiles the Editor and kills the run: the lead edits only outside `Assets/` during a suite.
- **Commits:** every finished slice, explicit paths plus `.meta` files; builders at least every 30 minutes; never `ProjectSettings/UnityConnectSettings.asset`; restore the TMP fallback font assets and `Mobile_RPAsset.asset` after Play or a build; never master, never push, never merge. Night prompts stay untracked; logs are tracked.
- **Time boxes:** `date` at every phase boundary into `NIGHT26-LOG.md` (Write / Edit only). A phase 25 minutes over its box is cut.
</lean_rules>

<context>
**Read first:** `CLAUDE.md` (imports `docs/memory/CORE.md`), this file, `NIGHT25-LOG.md`, `NIGHT25-PROOF.md`, `NIGHT25-REPORT.md`, `HANDOFF.md` (NIGHT25 block), `docs/LEVEL-DESIGN-NOTES.md`, `Design/levels/LEVEL-FORMAT.md`, `Design/levels/CURRICULUM.md`, `Design/levels/FORMAT-REQUESTS.md`, `Research/2026-10-08-wave24/DESIGN24-bosses.md`, `DECIDE25.md`.
**Key files:** `Game/Bot/AutopilotInput.cs` + `AutopilotGuns.cs` (the bot), `Game/FeelReel.N25.Levels.cs` (`levelwalk`), `Game/FeelReel.N25.Level.cs` (the tutorial pilot), `Tools/SimHarness/Harness.N24.Campaign.cs` (`campaign --seeds`), `Tools/night/gen-levels610.py` + `levels67-kits.py` + `levels610-pars.py`, `Core/Enemies/Warden.cs` (the shield), `Core/Fist24.cs` + `Core/Guns/Pulse25.cs`, `Game/Hitch24.cs`, `Core/Campaign24.cs`, `Core/Enemies/BossKit.cs`, `Design/bosses/*.json`.
**GG's answers come first:** act on any filled Answer in `DECIDE25.md` or any new word from GG, and log how. A blank Answer means the default stands.
**The rules that bite:** CLAUDE.md 1 (git), 3 (golden exact), 4 (the C# collider), 5 (numbers), 6 (Mulberry32), 7 (no audio; boss events keep the web's names), 8 (cyan player, red-orange danger, magenta RIVAL only, 18 px text), 9 (docs by Write / Edit), 10 (close every Player by PID; never kill the Editor).
</context>

<autonomy>
- Never ask GG. Decide, log the choice with the rejected alternative, move on. What only GG decides is in `DECIDE26.md` with its default.
- If one approach fails twice, switch to the next-best and log both. A row that still fails after two fix rounds is SHORT with its numbers.
- **The Editor gate (first 10 minutes):** `date`; `bash Tools/night/editor-ready.sh`; a "Recovering Scene Backups" dialog: `powershell -File Tools/night/dismiss-dialog.ps1` after 5 minutes; no Unity process: start it once with `Start-Process` on `C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe -projectPath C:\Users\gaige\dev\SKYGRIND-unity`; if it stalls (5 s eval timeouts, Pipeline timeouts) use the batch path and dn-check and say so.
- **After a crash or a usage limit:** `git status`, `git log`, the last lines of `NIGHT26-LOG.md`; redo only what is half-done; commit before any risky step; retry agents at most every 30 minutes.
- The stops you do not want: a summary announcing the next step with no tool call; an offer to continue; a list of decisions that block nothing. (The kickoff appends `PROMPT-STOPS.md`.)
</autonomy>

<builders>
Four builders in git worktrees by file ownership (`isolation: "worktree"`); only the lead touches the Editor, builds, reels, integration and the shared files. A builder compiles Unity-side code with `dn-check.sh` copied and pointed at its own worktree (a `git merge port/stage3` first; a worktree isolated agent cannot run compound shell, so scripts are written with the Write tool and run as one plain command).

| Builder | Owns | Queue (in order) |
| --- | --- | --- |
| **B** (the bot) | `Game/Bot/AutopilotInput.cs`, `AutopilotGuns.cs`, `Game/Bot/HumanSkill26.cs` (new), `Harness.N26.Bot.cs`, tests | (1) the harness `botskill`: the SAME 30 seeds before / after on level 6 room 2 (shields, target median under 120 s), level 7 room 3 (fist-only, under 150 s), levels 6-7 room 4 / 3 (pillars, tiers); (2) the warden rule (flank / punch the shield), the fist rule (close, punch while strafing, flick only with a full notch and a reason), the pillar rule; (3) the `humanSkill` flag (reaction delay, aim error, no omniscient lead) and the fight times it gives; (4) `levelwalk` on the maps the bot could not finish: report each map's verdict. First-try rates stay in 60-90 %: a bot that gets too good is a re-tune of the plans, not a win |
| **O** (the outrunner) | `Core/Enemies/Outrunner*.cs`, `Game/OutrunnerView.cs`, `Design/levels/level11.plan.json` + `Design/bosses/outrunner.json`, `FeelReel.N26.Outrun.cs`, `Harness.N26.Outrun.cs`, tests | (1) the harness `outrun` seen RED; (2) the stub: a 400 m straight road, 40 m tiles recycled behind the player, the gap meter (starts at 60 m, hurt only at 25 m or less, escape at 80 m = checkpoint, never instant death), a boss box that runs, a one-way rubber band that the meter shows, no hazards; (3) a `kind: boss` level id wired through `BeginBoss` as the other bosses are; (4) the Player scenario `outrun`; (5) hazards one at a time only if (1)-(4) are PASS |
| **F** (debts) | `Game/Hitch24.cs`, `Game/Ui*.cs` entrances, `FeelReel.N26.Debt.cs`, `Harness.N26.Debt.cs`, tests | (1) the hitch: markers inside SimStep and R.World on the first PLAY, the dominant cost, the cheap fix, before / after on a quiet box; (2) `m9-list.py` on a `levelwalk` reel and the listed pop-ins; (3) `campresults` on the Player (focus on RETRY LEVEL / CONTINUE); (4) the RESULTS room-rank display (S / A / B / C is computed: show it) |
| **L** (levels) | `Design/levels/*` (not `level11`), `Core/Campaign24*.cs`, `Assets/Editor/Maps/LevelPlan24.cs`, `LevelValidate24`, `FeelReel.N26.Level.cs`, tests | (1) a plan row `optional: true` and the side room kit: an optional room off levels 2-5 with a BIG PICK; (2) THE FISTWORKS' pit with the pulse-flick shortcut once the bot has a pit rule; (3) levels 11+ only if (1) is DONE; (4) `levelwalk` and `tutorial` kept GREEN after every change |

**The lead alone** touches: `SurvivalSim.cs`, `SurvivalSim.Run.cs`, `SurvivalGame*.cs`, `Ui.N26.cs`, `FeelReel.N26.cs` (the phase list), `Tools/SimHarness/Program.cs`, `Harness.N26.cs`, `RunTape.cs`, `FingerprintBaseline*.cs`, the config merge (`n24-sync.sh`), `Tools/feel-metrics.py`, and every build, reel, probe, gate and the integration. A builder who needs a line there leaves a `partial void` hook (or a plain method) and names the one call in the report; the lead applies it the same hour.

## Contracts at a glance (`NIGHT26-FEATURES.json`, the lead writes it from this table; each row has its scenario id and a check with a number)
bot-shield (B: level 6 room 2 bot median under 120 s, first-try 60-90 %, same 30 seeds before / after) | bot-fist (B: level 7 room 3 median under 150 s) | bot-human (B: the humanSkill flag prints reaction delay, aim error and the fight time) | levelwalk-all (B + lead: LEVEL CLEARED on all ten maps within the budget, or each failure with its measured reason) | hitch (F: no frame above 33 ms on a quiet box, or the dominant cost named with before / after) | fingerprint (lead: N25OnPath recorded once at the freeze, its own commit) | m9 (F: the list before / after) | campresults (F: PASS on a Player) | outrun (O: the gap meter moves, the boss runs, tiles recycle, no float drift, a Player line) | side-room (L: an optional room with a BIG PICK on a Player) | balance (lead: each lever changed has a before / after line)
</builders>

<tasks>
## U3-40.1 Start (0:00-0:15)
`date`; `export NIGHT=n26`; the Editor gate; `git status`; `tasklist | grep -i skygrind`; CPU and python-process count; `editor_focus`, `set_autotick`. Copy GG's words to `Research/2026-10-09-wave26/GG-REQUEST.md` and write `DECIDE26.md` (defaults below). Add U3-40 and U3-40.1 .. U3-40.9 to `tasks.json`.
## U3-40.2 The shared commit (0:15-0:50)
`Core/N26Blocks.cs` (3 bits: humanBot, outrunner, sideRooms), `SurvivalSim.N26.cs` hooks, `design.n26.<row>` in both config copies (`enabled: false`, `_why`), `--off26`, tape format 10, `FingerprintBaseline.N26OnPath` false, the scenario ids, `Harness.N26.cs` dispatch, `NIGHT26-FEATURES.json`. Copy N25's shared commit. Gates: fingerprint EQUAL as configured and under every older `--off`, `n26shared PASS`, golden exact, `dn-check.sh` 0, Editor (or batch) compile. Commit; launch B, O, F, L with `date`, briefs and contracts; B's first message is the before-numbers on the 30 seeds.
## U3-40.3 The baseline (0:50-1:40, builders working)
Baseline suite alone (expect 8-17 min), a Development Player from the batch path, the smoke alone, `levelwalk` on layer1 and level06 for the baseline verdict (235 s of game time each), the hitch tour on a quiet box. Log what moved since NIGHT25's close.
## U3-40.4 Integration rounds (C1 2:15, C2 3:20, C3 4:20, C4 4:50)
Each: `integrate.sh`, `n24-sync.sh`, `dn-check.sh` 0, compile, focused tests, a batch build, ONE reel invocation per side (ON; RED with `-skygrindOff26`), `feel-metrics.py` one process at a time, record every RED and GREEN line in `NIGHT26-PROOF.md`.
## U3-40.5 The contract rows (the builders' queues)
Rules the reviewers will check: a bot change does not move the fingerprint; every long scenario is bounded by game time; the OUTRUNNER's gap meter is visible and its rubber band one-way; a side room never blocks the main path; the first room of every level stays a breath.
## U3-40.6 Level reviews (2:15-4:50, the lead)
At each round the lead plays the newest level on a Player by the human-skill bot AND reads three frames per room; a level whose bot first-try rate leaves 60-90 % per room goes back with the numbers.
## U3-40.7 The freeze (5:00) and the verification (5:00-6:15, one heavy job at a time)
(1) record `FingerprintBaselineN25` AND N26 if a sim-changing default is on, each in its own commit; (2) the final build; (3) the suite on the frozen code; (4) the smoke alone; (5) the rows' final scenarios (ON, RED, PAD) and `levelwalk` on all ten maps in two batches; (6) two reviewers (`Tools/night/reviewer-veto.md`, sides swapped, 25 minutes each) read frames and code while the suite runs. The lead writes the report skeleton outside `Assets/` meanwhile.
## U3-40.8 Carry-overs (a free builder takes the next; none after 5:00)
1. The PAD pass with the real gamepad (a break for GG: pad layout v5, the pulse, zoom on the shotgun). 2. MONOLITH's cut items (floor sink, adds, orb parry) if level 9's room does not use them. 3. HIVE and ORBITER ports only if everything is DONE. 4. A lighting pass on levels 3+. 5. `docs/memory` harvest (`wave-memory harvest 26`). 6. The stale `FORMAT-REQUESTS.md` R1 (boss rooms now work).
## U3-40.9 The docs and the close (6:15-6:45)
`NIGHT26-LOG.md` (close table, building share), `NIGHT26-REPORT.md` (one page for GG), `FEEL-PLAYTEST.md` (10 minutes, pad first: the tutorial, melee on RB, the pulse, levels 6 and 7, a boss, the OUTRUNNER stub), `PORT-STATUS.md`, `DECIDE26.md`, `HANDOFF.md`, `tasks.json`, `NIGHT26-FEATURES.json`, `NIGHT26-PROOF.md`, `docs/LEVEL-DESIGN-NOTES.md`, `docs/memory` (harvest), and the NIGHT27 draft (`NIGHT27.md`, untracked). Tag `u3-n26`; `date`; print the table; "NIGHT26 CLOSED".
</tasks>

<definition_of_done>
| Item | DONE when (from this session's output) |
| --- | --- |
| U3-40.1 | the Editor gate logged, contracts copied, tasks.json parses |
| U3-40.2 | the shared commit with its gates printed; builders launched with a `date` |
| U3-40.3 | the baseline suite totals (or SHORT with minutes), the smoke, the baseline `levelwalk` verdicts |
| each contract row | its scenario or harness line seen RED, then GREEN on a Player or harness line from tonight, `passes: true` with the evidence, the off-test green, golden exact; or SHORT with the measured reason |
| U3-40.4 | four integration rounds logged, each with its gates |
| U3-40.7 | the freeze checks, N25OnPath (and N26) decision, suite on frozen code, smoke, reviewers or SHORT |
| U3-40.9 | the docs, `NIGHT27.md`, `u3-n26` tagged, the close table, "NIGHT26 CLOSED" |
A row is SHORT with a measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit with its `date`, or the time limit.
</definition_of_done>

<final_report>
`NIGHT26-REPORT.md`, one page for GG, outcome first: can a person-like bot finish the ten levels (which maps, how long), is the first PLAY smooth (the worst frame on a quiet box), what the OUTRUNNER stub is and is not, what changed in balance and why. Then per row: what GG can do (one line), its switch, the contract's number, what felt worse. Then what is not verified and the DECIDE26 questions that still matter. Every number comes from a run this session.
</final_report>

<closing_reminder>
- **Bot first**: it unblocks every level proof; a long scenario is bounded by game time, never by frames.
- **One heavy job at a time**; never wait on a stalled Editor: batch path + `dn-check.sh`; the hitch tour only on a quiet box.
- **The pad is GG's hands**: a scripted pad step is not a gamepad: pad feel is a break for the real gamepad.
- Commit every slice. Never push, merge or commit to master; never stage `UnityConnectSettings.asset`.
- Every message ends with `N26-STATUS: <d> done, <s> short, <l> left (of 9 phases) · integration round <r> · builders <alive>/4 · passes <n>/11 · <h:mm> / 7:00`.
</closing_reminder>

## DECIDE26 defaults (the lead copies this table to `DECIDE26.md` at 0:15; a blank Answer means the default stands)
| id | question | default |
| --- | --- | --- |
| D-1 | Who chases whom on the highway (the OUTRUNNER)? | the player chases (GG: "you're chasing the boss"); the gap meter supports both; the boss is hurt only at 25 m or less |
| D-2 | The pad's PUNCH button | RB (layout v5); R3 carries SPECIAL + ARCLINE; if GG dislikes R3 for ARCLINE, the default moves it to a toggle (SETTINGS) |
| D-3 | Levels 11+ | none this night unless the side rooms and the OUTRUNNER stub are DONE; a second layer is GG's call |
| D-4 | Side rooms | optional, one per level 2-5, a BIG PICK each, never on the main path |
| D-5 | Pars | `min(bot median, parCap)`; with the human-skill bot the pars are re-measured and the cap rule stays |
| D-6 | The pulse's numbers (7 hits per notch, 8 / 16 / 32 launch damage) | stay until GG's hands say otherwise |
| D-7 | SURVIVAL / THE GRIND | stay as they are; the pad layout and the pulse apply to them too |
