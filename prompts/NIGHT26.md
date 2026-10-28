# NIGHT26.md: SKYGRIND in Unity: THE FIRST THREE LEVELS, DONE RIGHT (a 2.5 h build, hard cap 3 h: platforming, a chase boss, weapon identity, a level select with ranks)

<library_header>
id: skygrind-n26 | version: 1 | date: 2026-10-08 | model / effort: claude-opus-5-5 / high
raw request: GG, 2026-10-08 ~20:20, after his first playtest of NIGHT25 (verbatim in Research/2026-10-08-wave26/GG-REQUEST.md: copy the quote block below at 0:00)
research: Research/2026-10-08-wave26/RESEARCH.md (wall riding, ULTRAKILL weapons and ranks, chase bosses, Terraria worms): read it before designing
base: branch port/stage3 at the commit tagged `u3-n25` (suite 1760 / 1760; NIGHT25's open rows listed in NIGHT25-LOG.md's close table)
outcome: (after the run)
</library_header>

<role_and_goal>
You are the lead engineer on SKYGRIND, a first-person movement shooter at dusk (Unity 6000.3.7f1, URP; a deterministic 120 Hz C# sim with its own collider and an input tape), in `C:\Users\gaige\dev\SKYGRIND-unity`, branch `port/stage3`. GG owns the game; you build for GG's taste, for Steam.

GG played NIGHT25's campaign and said: the level reads better; the tutorial's beginning is fine but still bare; the first map's arena is **far too wide**; he wants **platforming, wall-jump and dash sections**; the worm fight is good (attacks, phase 2, the coil, the projectiles) but should be **harder and chase you when you are near it, like Terraria**, with **body hits doing less than head hits**, and its arena should **invite wall riding**; the worm is **too hard for a first boss**: put it at level 3 or later and add an **easier boss first**; level 2's boss is a **chase on speed-skating ice, like JoJo part 5's Ghiaccio highway**, pistol only, get close, dodge telegraphs; **unlock one weapon every level or two**; rocket and grenade clear too fast; **every weapon needs an LT that zooms or does something else**; weapons must have strengths and weaknesses so he wants to switch (ULTRAKILL); **no mystery box and no end-of-level BIG PICK in the campaign** (the survival modes keep them); the **campaign is the default mode** with an **ULTRAKILL-style level select showing every level, its best rank and time**; and the **PULSE GAUNTLET's blast is the LEFT TRIGGER**: it charges with use and launches you (NIGHT25 put it on the click: wrong). Two fixes were already made at the close of NIGHT25: PUNCH is back on **R3** (layout v4; the row `design.n25.meleeFire` is OFF) and the campaign's end-of-level BIG PICK is off (`design.n24.bigPick.campaign` false).

**GG's rules for this night: only the FIRST THREE levels; build everything in about 2 hours; STOP OVER-TESTING: build, then check each thing once.** Level count in the end: 8 to 10; levels 4+ are NIGHT27.

**Run length: a 2.5 h box from your first `date`: code freeze at 1:50, one suite and one smoke 1:50-2:20, docs and the close table 2:20-2:30, hard stop 3:00.** The close table prints at 2:20 or later, never after 3:00.
**Lessons that rule this night (NIGHT24 and NIGHT25 logs):** (a) never wait on a stalled Editor: `Tools/night/dn-check.sh` compiles Unity-side code with dotnet and a Player builds from a batch-mode project copy (`C:\Users\gaige\dev\SKYGRIND-unity-batch`, `Assets/Editor/BatchBuild24.cs`; recipe in memory `batch-build-and-editor-stall`); (b) a row passes only on a Player line with its RED, and the lead reads the first Player line of every builder scenario the hour it lands; (c) a real gamepad plugged into the PC breaks virtual-pad steps; (d) frame-count loops cut long scenarios short: bound by game time; (e) a prompt that says "R STICK PUNCH" must match the live layout (placeholders); (f) a usage limit can cut the lead: commit every slice and keep the close table's skeleton written early.
**Testing budget (GG: stop over-testing):** per row ONE scenario seen RED once and GREEN once; one batch Player build per integration round (at most 6 builds); ONE suite at the freeze (a second only if it fails); NO persona triage, NO bench, NO farm, NO reviewers unless time remains; headless bots only for level clear rates (10 seeds, not 30).
</role_and_goal>

<ggs_words>
GG, 2026-10-08 (voice-typed; spelling is his transcription's); decides ties:
> "the right shoulder button ... shouldn't be the melee button it should be the right stick when you press it" (done: R3)
> "the gauntlet the second weapon it doesn't do the blast attack at all with the left trigger that's the attack that I was asking where it's supposed to charge and build up"
> "for that first map the arena is just way too wide ... it's still very bare bones and basic but I want it to really encourage better uses of mechanics like the wall jump and the dashing like platforming sections"
> "also don't add a mystery box in the campaign I want like natural progression" ... "remove ... those little upgrades at the end of the level ... that shouldn't be for the campaign at all" (done)
> "The boss fight is good I think there should be it should be a little more difficult ... almost feel a little like Terraria where if the boss kind of is close it might just kind of go after you ... I like the second phase ... a final harder attack ... when it coils in the middle ... I would like the arena to encourage ... wall riding"
> "I like the endless one ... Ghiaccio from JoJo part 5 where you're constantly either wall riding or moving or ... chasing after this you're kind of like skating ... different surfaces ... the second one to feel like you're speed skating to get this boss and use movement dodge these telegraph attacks and get close to the boss and do damage"
> "like Terraria ... if you hit the body of it it does damage but lesser damage and it does more if you hit the head"
> "for the second level boss fight ... you're only allowed to use the first [weapon] and you have to get close ... you slowly unlock weapons as you go on and maybe two levels in between the next weapon ... the worm boss maybe the third level or later ... maybe add an easier boss"
> "the grenade and rocket launcher are a little too broken they just clear enemies too fast ... get creative with weapons ... almost all the weapons should really have a left trigger that either zooms in or does something else ... look at ULTRAKILL ... every single weapon has a positive a weakness and they work in certain scenarios ... you really don't feel incentivized at all to switch weapons"
> "when you click into the campaign which should be the first game mode by the way that should be the default ... it should be like ULTRAKILL where it kind of shows all the levels what rank you got what time ... copy that and go beyond this"
> "all this should really be built in 2 hours max ... stop with the over testing just build ... we're only going to focus on the first 3 right now"
</ggs_words>

<facts_already_measured>
Read at NIGHT25's close (`NIGHT25-LOG.md`, `NIGHT25-PROOF.md`); re-check each in the first 15 minutes.
- **Proven on a Player in NIGHT25:** tutorial (6 stages, bot clears in 42 s, prompts follow device and layout), campaign RESULTS screen with ranks and CONTINUE (focus fixed), campaign boss rooms (wyrm 40 s, bulwark 87 s, monolith 33 s, ELDER WYRM 38 s, RIVAL PRIME 58 s with the real gun after HP cuts), the worm's bar, death, phase-start retry, aimed spit (9.3 hits / min standing in phase 2), round counter hidden in campaign.
- **Where things are:** levels `Design/levels/{tutorial,layer1,level3,level4,level5,level06..level10}.plan.json` (+ generated `.json`; ids are mixed: layer1 = level 2 THE KILN, level3.., level06..), `Design/levels/CURRICULUM.md` (the table), `Core/Campaign24.cs` (rooms, doors, LIMIT flags, `boss` plan key, ranks), `Core/Lesson25.cs`, `Core/Curriculum25.cs` (order), bosses `Core/Enemies/{Wyrm,Bulwark,Monolith,Rival*}.cs`, `Design/bosses/*.json`, `Design/maps/*-arena.json`, `Game/UiCampaignResults25.cs`, `Game/UiMeta.cs` (MODE row; CAMPAIGN is first in the cycle but the default mode is SURVIVAL), `Core/Guns/AltUse.cs` + `Shockwave.cs`, `Core/Fist24*.cs` + the NIGHT25 pulse code (`SurvivalSim.Combat25.cs`, `StepPulse25`), weapons config `PortExport/config.json` `weapons.*` (18 weapons: revolver, gauntlet, shotgun, nailgun, rocket, rail, rattler, torch, bouncer (the grenade), zapper, discus, vortex, sunder, flicker, thumper, tether, choir, prism), `Core/Gamepad.cs` (v4 default, v5 behind `meleeFire`).
- **Mover:** `Core/Mover.cs` has ground friction in one place (line about 964: `MovementMath.ApplyFriction(velocity, c.ground.friction, ...)`) and wall ride / wall jump / wall kick; **rule 3: golden vectors and 16 / 22 trajectory replays must stay exact. This prompt NAMES one Mover change: a public `groundFrictionMult = 1` multiplied into that friction call (default 1: every vector unchanged), set by a sim-side surface zone. Nothing else in the Mover.**
- **Open from NIGHT25 (SHORT rows):** first-play hitch (cold PLAY worst 74 ms quiet: SimStep 24 ms, R.World 20 ms), N25OnPath fingerprint record, M9 entrance times, `Design/levels/FORMAT-REQUESTS.md` stale items.
- **Bosses' numbers:** the worm: shared pool, body x1, head x2, core x3 (config `design.n24.wyrm.hp`), pool 7800 with `design.n25.wyrmAim` (hp.mult 1.1), 47 attacks all told >= 0.5 s ahead; composed human-lag bot kills in about 3:57 with 2 deaths. Weak, easy boss to add: none yet (BULWARK is the shield lesson, 87 s kill after cuts).
</facts_already_measured>

<scope>
- **In (the first three levels):** (1) the PULSE on the left trigger; (2) every weapon's LT (zoom or a unique alt) and weapon identity: enemy vulnerabilities, costs, the rocket and BOUNCER nerfs, one weapon unlock per level or two; (3) SURFACES (ice, boost strips) through the one named Mover multiplier; (4) LEVEL 1 rebuilt (a TUTORIAL kept short and a first stage that is narrower, with platforming, wall-jump and dash sections) ending in an EASY boss; (5) LEVEL 2 = the speed-skating CHASE boss on ice, pistol only; (6) LEVEL 3 = THE KILN WYRM with proximity aggro, body x0.5 / head x2, a wall-riding arena, a harder final attack; (7) the LEVEL SELECT screen, ranks (time, kills, style, aggregate, P rank), CAMPAIGN as the default mode; (8) no mystery box / BIG PICK in the campaign (survival keeps both).
- **Out:** levels 4-10 (they exist as NIGHT25 plan files and stay, but are unreachable until their unlocks: the select shows them LOCKED), audio (rule 7), art and polish (GG: later), new Mover verbs, the highway STUB of NIGHT25's plan (the chase boss here replaces it), the web repo. New ideas go to `docs/memory/IDEAS.md`.
</scope>

<design_brief>
**Level 1: THE FOUNDRY (the tutorial, shortened; GG: the first map is too wide).** Keep stages 1.1-1.6 but cut every room's width to at most 28 m and add a **PLATFORMING RUN** after 1.3 (a 40 s stretch of wall-jump shafts, dash gaps, pad hops over a pit: one verb per section, recovery floors, no soft-lock, a par for the wall route versus the floor route) and a **WALL-RIDE HALL** (a long continuous wall with gaps a wall jump or dash bridges; guide light strip along rideable walls). Level 1's last room is a short fight (husks only) and its boss is the EASY boss: **THE SLAG HULK** (new, small): a slow walking bruiser with one telegraphed slam (a jumpable shockwave ring, tell 0.8 s) and one charge (tell 0.8 s, it stops at a wall, stunned and hittable for 2 s), 1500 HP, three bars of phases at 66 / 33 %, weak point: back and head x2; built from `BossKit` and `Wyrm`-style pieces (boxes, shared HP pool); kill target for a human with the pistol: 60-90 s; a 3 s checkpoint at each phase. Reward at the end: the **THUNDERCLAP** (shotgun) is earned.
**Level 2: THE ICE LINE (the chase).** One long straight course on ICE (low friction, high speed: `groundFrictionMult` about 0.25 on ice zones, boost strips x1.0 push) in recycled 40 m tiles, 2-3 lanes separated by low rails, wall lines to ride, gaps to dash, grapple nodes on gantries (existing verbs only). The boss, THE GLACIER (a fast skating hull) is AHEAD and pulls away; the **gap meter** (DESIGN24-bosses.md section 2) shows the distance; it throws telegraphed LANE attacks (a sweeping bar: jump; a low bar: slide; a wall of spikes: dash; a net: grapple or wall-ride), tells >= 0.6 s, never two lethal attacks inside 0.5 s; it can be hurt only when the gap is 25 m or less; the **PISTOL ONLY** (LIMIT flag; GG: only the first weapon and you must get close: the pistol's damage rises close in via a close-range bonus: config); a soft fail at gap 80 m (back to the checkpoint). Pad-first, the camera FOV lifts with speed. Reward: the **NAILGUN** is earned.
**Level 3: THE KILN (the worm).** Today's layer1 rooms trimmed (narrower, one platforming link), then THE WORM: arena redesigned for wall riding (a cylinder 60 m across with a continuous wall ring, pillars to chain between, three platforms), proximity aggro (see RESEARCH.md S5: within a radius of the player the head breaks its cycle and dives, a 0.5 s telegraphed re-aim, cooldown 6 s), body hits x0.5 / head x2 / core x3, a phase-3 FINAL ATTACK (a full coil squeeze with one marked gap; tell 1 s), the coil in the middle kept, harder than NIGHT25's (a second spit lane in phase 2); human-lag bot kill 4:00-5:00. Reward: the **RATTLER** is earned (or a weapon chosen from the table below).
**Weapon identity (GG: you should want to switch).** One table in `Design/weapons/WEAPON-IDENTITY.md` and config `design.n26.weapons.*` (each row `_why`): per weapon the STRENGTH, the WEAKNESS (a cost: ammo, heat, charge, slow, self-damage), its LT, and the enemy it beats. Starting points (the builders decide, the lead reviews): KESTREL (pistol): accurate, close-range bonus, LT = zoom 1.25x with a charged shot; PULSE GAUNTLET: **LT = the BLAST: hold to charge by use (hits), release to launch + small damage, 3 s lockout (NIGHT25's code on the click moves to LT; the click is a plain swing)**; THUNDERCLAP: close and fat, LT = zoom (no shockwave), weak at range; NAILGUN: sustained mid-range, pins; LT = detonate the nails; RATTLER: spray; LT = zoom; ROCKET: slow, big splash, **self-damage and a 2.2 s reload, blast radius -35 %, damage -30 %**, LT = detonate in flight; BOUNCER (grenade): **fuse 2.4 s, damage -35 %, 3 in the pool**, LT = hold to cook; RAIL: breaks shields and armour, slow, LT = zoom; ZAPPER: stuns chains; LT = overload; each weapon at least ONE enemy kind it clearly beats and ONE it is bad against: encode as per-enemy damage multipliers in config (`design.n26.vulnerability`: warden shield x0.2 except rail / punch, swarmer x0.6 for single-target, charger windup x1.5, drone x0.5 for slow shots, husk x1). Prove with the harness: a weapon-by-enemy table (time to kill 8 of each kind) printed once; no weapon the best on every row; rocket and BOUNCER no longer the fastest room clear (their room-clear time within 15 % of the best other weapon).
**Unlocks:** level 1 tutorial: KESTREL + PULSE; end of level 1: THUNDERCLAP; level 2 allows the PISTOL ONLY; end of level 2: NAILGUN; end of level 3: RATTLER (or the table's choice); later levels: ZAPPER (4), RAIL (5), BOUNCER (6), ROCKET (8): the unlock order is data (`Design/levels/unlocks.json`), one row per level; the loadout wheel shows only owned weapons; a level's LIMIT flags override.
**Level select + ranks (ULTRAKILL; RESEARCH.md S3).** The CAMPAIGN is the default mode and the first MODE row. TITLE > PLAY on CAMPAIGN opens the **LEVEL SELECT**: a column / grid of every level (ten slots; levels 4+ LOCKED), each with its name, the best aggregate rank letter, best time, kills %, style, and the weapon it unlocks; the cursor on the next unlocked level; A starts it; pad and keyboard; 18 px text; no pop-ins. Ranks per level: TIME, KILLS, STYLE each D-S by per-level thresholds in the plan file, aggregate = S 5 / A 4 / B 3 / C 2 / D 1 summed minus 1 per checkpoint restart, **P rank** = S in all three without restarts; the best per level persists in the profile (Core `Profile` + a test: an old profile loads). RESULTS shows the three ranks, the aggregate, and the weapon unlocked.
**Surfaces (the one Mover change).** `Mover.groundFrictionMult = 1` (public), multiplied into the ground friction call; a sim system `Surface26` sets it from a level plan's `surfaces` list (rect + kind: ice 0.25, mud 2.0, boost strip: a forward impulse through `Mover.Impulse`); golden / trajectory exact (default 1), tape records the surface state only through the level data (no new input); a harness line proves: on ice a standing-start sprint reaches speed S in T s and a stop takes D m; on normal ground unchanged to the bit.
**Wall riding that feels good (RESEARCH.md S1).** No Mover change: long continuous wall runs in the level geometry, light strips along rideable walls (cyan), a short FOV lift while riding and the existing roll tuned (`feel.wallRide*` config), style points and a par that assumes the wall route; the validate report prints floor-route vs wall-route times for every platforming section (as NIGHT24's flue: 3.2 s vs 8.6 s).
</design_brief>

<lean_rules>
- **Building share:** at least 75 % of agent-minutes go to committed code, plans and fixes (GG: build).
- **Proof, small:** per row one scenario (`reel`) or harness line, RED once and GREEN once, printed with its preconditions; the lead reads the first Player line the hour it lands.
- **Golden at every Core commit (CLAUDE.md rule 3):** fingerprint + golden + trajectory before each Core commit; the Mover change is exactly the named multiplier with default 1 and its own commit.
- **Config:** web numbers from `PortExport/config.json`; Unity-only numbers in `design.n26.<row>` (both copies byte-identical via `Tools/night/n24-sync.sh`; each `_why`); nothing typed in code; Mulberry32 only; no `System.Math` Sin / Cos / Atan2 in Core (JsMath).
- **Switches:** `Core/N26Blocks.cs` (bits: pulseLT, weaponIdentity, surfaces, levelSelect, level1, level2, level3), `--off26`, tape format 10, `FingerprintBaseline.N26OnPath` (record it at the freeze: NIGHT25 never did); with every block off the game is NIGHT25's on the sim path. Copy `N25Blocks.cs`, `SurvivalSim.N25.cs`, `N25SharedTests.cs`, `Harness.N25.cs`, `FeelReel.N25.cs`.
- **Caps (a cap reached = SHORT with the count):** Player builds 6; reel invocations 20; suite runs 2; builders 4 alive, 5 in total; no reviewers, no bench, no farm.
- **Heavy jobs one at a time:** a build, a reel, a suite, the smoke. An edit under `Assets/` while a suite runs recompiles the Editor and kills the run: the lead edits only outside `Assets/` during the suite.
- **Commits:** every finished slice, explicit paths plus `.meta`; never `ProjectSettings/UnityConnectSettings.asset`; restore the TMP fallback font assets and `Mobile_RPAsset.asset` after Play or a build; never master, never push, never merge. Night prompts stay untracked; logs are tracked.
- **Time boxes:** `date` at every phase boundary into `NIGHT26-LOG.md`. A phase 15 minutes over its box is cut.
</lean_rules>

<context>
**Read first:** `CLAUDE.md` (imports `docs/memory/CORE.md`), this file, `Research/2026-10-08-wave26/RESEARCH.md`, `NIGHT25-LOG.md` (close table), `NIGHT25-PROOF.md`, `Design/levels/CURRICULUM.md`, `docs/LEVEL-DESIGN-NOTES.md`, `Research/2026-10-08-wave24/DESIGN24-bosses.md` (the chase section 2), `HANDOFF.md`.
**Key files:** see `facts_already_measured`; also `Core/Enemies/BossKit.cs`, `Game/WyrmRoomView.cs`, `Game/CampaignLighting24.cs`, `Assets/Editor/Maps/LevelPlan24.cs`, `Core/LevelValidate24`, `Tools/night/{dn-check.sh,integrate.sh,n24-sync.sh,n25-rows.py,n25-proof-run.sh,reel.sh}`.
**GG's answers come first:** act on any filled Answer in `DECIDE26.md` or any new word from GG, and log how. A blank Answer means the default stands.
**The rules that bite:** CLAUDE.md 1 (git), 3 (golden exact; the one named Mover change), 4 (the C# collider), 5 (numbers), 6 (Mulberry32), 7 (no audio; boss events keep the web's names), 8 (cyan player, red-orange danger, magenta RIVAL only, 18 px text), 9 (docs by Write / Edit), 10 (close every Player by PID; never kill the Editor).
</context>

<autonomy>
- Never ask GG. Decide, log the choice with the rejected alternative, move on. What only GG decides is in `DECIDE26.md` with its default.
- If one approach fails twice, switch to the next-best and log both. A row that still fails after two fix rounds is SHORT with its numbers.
- **The Editor gate (first 10 minutes):** `date`; `bash Tools/night/editor-ready.sh`; a "Recovering Scene Backups" dialog: `powershell -File Tools/night/dismiss-dialog.ps1` after 5 minutes; no Unity process: start it once with `Start-Process` on `C:\Program Files\Unity\Hub\Editor\6000.3.7f1\Editor\Unity.exe -projectPath C:\Users\gaige\dev\SKYGRIND-unity`. Unreachable for 15 minutes: the batch path and `dn-check.sh`; Editor-only rows SHORT with the date.
- **After a crash or usage limit:** `git status`, `git log`, the last lines of `NIGHT26-LOG.md`; redo only what is half-done; commit before any risky step.
- The stops you do not want: a summary announcing the next step with no tool call; an offer to continue; a list of decisions that block nothing. (The kickoff appends `PROMPT-STOPS.md`.)
</autonomy>

<builders>
Four builders in git worktrees by file ownership (`isolation: "worktree"`); only the lead touches the Editor, builds, reels, the integration and the shared files. Each builder runs a dn-check pointed at its own worktree before each Unity-side commit (copy `Tools/night/dn-check.sh`, change the repo root and scratch dir) and says what is unrun.

| Builder | Owns | Queue (in order) |
| --- | --- | --- |
| **M** (movement, surfaces, camera) | `Core/Mover.cs` (ONLY the named multiplier, its own commit), `Core/Surface26.cs`, `Game/SurvivalSim.Surface26.cs`, plan `surfaces` in `LevelPlan24.cs` / `Campaign24` data, wall-ride camera tuning (`feel.wallRide*`), guide light strips (`CampaignLighting24.cs` additions), harness `surfaces`, scenario `ice` | (1) the Mover multiplier + Surface26 + harness line (golden / trajectory exact); (2) ice and boost strips in the plan format and the generator; (3) wall-ride camera / FOV lift / light strips; (4) the validate report's floor-route vs wall-route for platforming sections |
| **W** (weapons) | `Core/Guns/*`, `Core/Combat.cs` LT paths, the pulse code on LT, `Design/weapons/WEAPON-IDENTITY.md`, `design.n26.weapons.*` + `vulnerability`, `Design/levels/unlocks.json` + the loadout filter, harness `weapons`, scenario `pulselt` | (1) PULSE on LT (blast: charge by use, release to launch + damage; the click is a plain swing; `--off26` restores NIGHT25's); (2) every weapon's LT and cost; rocket and BOUNCER nerfs; the weapon-by-enemy table printed once; (3) per-enemy vulnerabilities; (4) the unlock order and the wheel filter; (5) PULSE / weapon prompts in `Lesson25Text` placeholders |
| **B** (bosses) | `Core/Enemies/Hulk26*.cs` (THE SLAG HULK), `Core/Enemies/Glacier26*.cs` (THE GLACIER + the gap meter), `Wyrm*.cs` changes (proximity aggro, body x0.5, final attack, second spit lane), `Design/bosses/*.json`, arenas `Design/maps/hulk-arena.json`, `glacier-course.json`, `wyrm-arena.json` (wall-ride redesign), views, bars, scenarios `hulk`, `glacier`, `wyrm26` | (1) THE SLAG HULK (easy, level 1); (2) the worm changes (level 3); (3) THE GLACIER chase on ice (needs M's surfaces: stub the ice as normal ground until they land) |
| **L** (levels and UI) | `Design/levels/*` (level 1 rebuilt: narrower, platforming run, wall-ride hall; level 2 course plan; level 3 = layer1 trimmed), `Core/Campaign24*.cs` (ranks: time / kills / style / aggregate / P), `Core/Profile` best-per-level, `Game/UiLevelSelect26.cs`, `UiMeta.cs` (default mode CAMPAIGN), `UiCampaignResults25.cs` (three ranks + unlock), `Core/Curriculum25.cs`, `FeelReel.N26.Level.cs` | (1) the level select + ranks + profile + default mode (the first row GG will see); (2) level 1; (3) level 3; (4) level 2's course plan (with M and B) |

**The lead alone** touches: `SurvivalSim.cs`, `SurvivalSim.Run.cs`, `SurvivalGame*.cs`, `Ui.N26.cs`, `FeelReel.N26.cs` (the phase list), `Tools/SimHarness/Program.cs`, `Harness.N26.cs`, `RunTape.cs`, `FingerprintBaseline*.cs`, the config merge, and every build, reel, integration. A builder who needs a line there names the one call in the report; the lead applies it the same hour.

## Contracts at a glance (`NIGHT26-FEATURES.json`, written by the lead at U3-40.2)
pulse-lt (W) | weapon-lt (W: every weapon has an LT) | weapon-identity (W: table, nerfs, unlocks) | surfaces (M) | wall-ride-feel (M) | hulk (B) | glacier (B) | wyrm-26 (B) | level-1 (L) | level-3 (L) | level-select (L) | ranks (L) | campaign-default (L) | no-campaign-box (lead: grep + a Player line) | fingerprint (lead)
</builders>

<tasks>
## U3-40.1 Start (0:00-0:10)
`date`; `export NIGHT=n26`; the Editor gate; `git status`; tasks.json items U3-40 and U3-40.1 .. U3-40.8 (template `Tools/night/n25-tasks.py`); `Research/2026-10-08-wave26/GG-REQUEST.md` from the quote block; `DECIDE26.md` from the table below; `NIGHT26-LOG.md`.
## U3-40.2 The shared commit and the builders (0:10-0:35)
N26Blocks (7 bits), tape format 10, `--off26`, config rows, scenario ids (`pulselt`, `weapons`, `ice`, `hulk`, `glacier`, `wyrm26`, `level1`, `level3`, `levelselect`, `ranks`), `Harness.N26.cs` dispatch, `NIGHT26-FEATURES.json`; gates: fingerprint EQUAL as configured and under every older `--off`, n26shared PASS, golden exact, dn-check 0. Commit, launch M, W, B, L with their briefs and a `date`; **L's first deliverable is the level select (GG sees it first), M's is the Mover multiplier.**
## U3-40.3 Build rounds (0:35-1:50): integrate at 1:00, 1:25, 1:50
Each round: `integrate.sh`, `n24-sync.sh`, `dn-check.sh` 0, one batch Player build, ONE reel invocation for the new rows (ON), the RED line (`-skygrindOff26`) once, then back to the builders with exact lines. The lead reads the first Player line of every row and three frames per new room.
## U3-40.4 Freeze (1:50) and verification (1:50-2:20)
Record `FingerprintBaselineN26` once in its own commit if a sim-changing default is on (pulseLT, weaponIdentity change the composed run: expect true); `--off26` EQUAL to the old baseline; ONE suite on the frozen code (alone); the smoke once (real pad disabled around its virtual-pad steps); the final proof lines into `NIGHT26-PROOF.md`.
## U3-40.5 Docs and the close (2:20-2:30)
`NIGHT26-LOG.md` close table, `NIGHT26-REPORT.md` (one page for GG: outcome first: can he play levels 1-3 in order, what each boss is, the unlocks, the level select; per row one line; what is not verified), `FEEL-PLAYTEST.md` (10 minutes, pad first), `HANDOFF.md`, `PORT-STATUS.md`, `NIGHT26-PROOF.md`, `docs/memory` (`wave-memory harvest 26`), and the **NIGHT27 prompt** (start from `NIGHT27.draft.md`: levels 4-10 rebuilt to the same standard, the highway / more chase, art and lighting, GG's balance answers). Tag `u3-n26`; `date`; print the table; "NIGHT26 CLOSED".
</tasks>

<definition_of_done>
| Item | DONE when (from this session's output) |
| --- | --- |
| U3-40.1 | the Editor gate logged, tasks.json parses |
| U3-40.2 | the shared commit with its gates printed; builders launched with a `date` |
| each contract row | its scenario or harness line seen RED once then GREEN on a Player or harness line from tonight, the off-test green, golden exact; or SHORT with the measured reason |
| U3-40.3 | three integration rounds logged |
| U3-40.4 | the freeze checks, N26OnPath decision, one suite, the smoke |
| U3-40.5 | the docs, `NIGHT27.md`, `u3-n26` tagged, the close table, "NIGHT26 CLOSED" |
A row is SHORT with a measured reason: a cap, a GG decision, a blocker after two approaches, the spend limit with its `date`, or the time limit.
</definition_of_done>

<final_report>
`NIGHT26-REPORT.md`, one page for GG, outcome first: can he start the game, land on the CAMPAIGN level select, play level 1 (platforming, the easy boss), level 2 (the ice chase) and level 3 (the worm) in order; which weapon he owns at each; whether the PULSE blast is on the left trigger; what each weapon is good and bad at; the ranks he sees. Then per row one line (what GG can do, its switch, the contract's number, what felt worse), what is not verified, the DECIDE26 questions that matter. Every number comes from a run this session.
</final_report>

<closing_reminder>
- **Build, then check each thing once.** GG: stop over-testing; 2 hours.
- **The first three levels only;** levels 4+ are LOCKED on the select.
- **The Mover change is one multiplier with default 1 and its own commit;** golden exact.
- **No mystery box and no BIG PICK in the campaign; the survival modes keep them.**
- Commit every slice. Never push, merge or commit to master; never stage `UnityConnectSettings.asset`.
- Every message ends with `N26-STATUS: <d> done, <s> short, <l> left (of 5 phases) · round <r> · builders <alive>/4 · rows <n>/15 · <h:mm> / 3:00`.
</closing_reminder>

## DECIDE26 defaults (the lead copies this table to `DECIDE26.md`; a blank Answer means the default stands)
| id | question | default |
| --- | --- | --- |
| D-1 | The easy first boss | THE SLAG HULK (new, 1500 HP, a slam ring and a charge, back / head weak points) |
| D-2 | Level order | 1 FOUNDRY (tutorial + platforming + HULK), 2 ICE LINE (chase, pistol only), 3 THE KILN (worm) |
| D-3 | Weapon unlock order | THUNDERCLAP after level 1, NAILGUN after 2, RATTLER after 3, then ZAPPER, RAIL, BOUNCER, ROCKET (data in unlocks.json) |
| D-4 | PULSE blast | LT hold charges by use (hits), release launches + small damage, 3 s lockout; the click is a plain swing; R3 punch |
| D-5 | Rank formula | TIME / KILLS / STYLE D-S per level; aggregate S5 A4 B3 C2 D1 minus 1 per checkpoint restart; P = S in all three, no restarts |
| D-6 | Ice friction | `groundFrictionMult` 0.25 on ice; boost strips push 6 m/s through `Mover.Impulse` |
| D-7 | Worm damage split | body x0.5, head x2, core x3 (config); proximity aggro radius 14 m, cooldown 6 s |
| D-8 | Rocket and BOUNCER nerfs | rocket damage -30 %, splash radius -35 %, self-damage on, reload 2.2 s; BOUNCER fuse 2.4 s, damage -35 %, pool 3 |
| D-9 | Survival modes | keep BIG PICK and the mystery box; the campaign has neither |
