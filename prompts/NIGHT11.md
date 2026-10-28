# NIGHT11.md: SKYGRIND in Unity, stage 3 night 3 (the run's shape), no human in the loop, at most 12 h

You are Claude Code working alone in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP).
GG starts this run and leaves; the Editor is open on this project. Nobody answers questions and nobody playtests.

**Read these files:**
- **this file:** tonight's job
- **`CLAUDE.md`:** the rules that always hold (read it first)
- **`NIGHT11-RESEARCH.md`:** why tonight looks like this (sections 0, C, D), with the evidence and URLs
- **`PORT-RULES.md`:** how to translate a web module
- **`HANDOFF.md`:** where things stand (anchor `u3-n10`; read its update note first)
- **`tasks.json`:** tonight's items are U3-24.1 .. U3-24.10; U3-24 is the umbrella and its acceptance holds
- **`NIGHT10-LOG.md`:** how last night ran, its slips

**Run length: up to 12 hours from your first `date`, or what GG says when he starts it** (NIGHT10 ran 6 h on his word).
Stop earlier only when the definition of done (section 2) is met.

GG, verbatim: "want this game to be like cod zombies meets ultrakill as the main, but other games should be inspired"
(2026-10-03); "great you decide on next wave but lets resrecah and come up with next detailed prompt" (2026-10-03,
after NIGHT10). He delegated the choice: D-19 is decided (the research's order, Survival first, to a Next Fest demo).

**Why tonight looks like this (NIGHT11-RESEARCH.md):**
- A run has no shape yet: rounds go on until you die. CoD gives a run its arc with a first goal (the power), special
  rounds with a reward (dog rounds end in a Max Ammo), traps, and an ending you choose (exfil).
- **The frozen web game already built most of that, and none of it is ported:** THE PROTOCOL (a guided quest: four
  relays on the map's movement showpieces, a SURGE round, every gun forged free), objective rounds (HOLD, GOLD RUSH),
  four traps, the run's grade on RESULTS. Port, don't invent: they come first.
- **Then three small additions behind design flags:** a power switch that wakes the relays and the traps, the swarm
  round's reward, and exfil (BO6: from round 11, every 5) as the ending.
- **ULTRAKILL's health (hard damage + a blood heal, D-18) goes in the LAB, off:** GG kept D-18 off, and the LAB is where
  proposals that change an earlier call go (D-03). He A/Bs it in one click.
- **Not tonight:** the round budget (with the roster, N13), save-and-quit (N12 or the Steam night), the POWER guns
  (N12), bosses (N14), new enemies (N13).

**Goal (the morning):** GG plays a run with a shape: he throws the power switch and the traps light; a HOLD round makes
him stand in a ring; a swarm round ends with a drop; at round 6 the PROTOCOL's relays wake and he climbs to light them,
the SURGE comes and every gun is forged; at round 11 the exfil radio rings, he runs to the site, clears the horde and
leaves; RESULTS shows "EXFIL" and his grade. In the LAB, ULTRA HEALTH turns on ULTRAKILL's hard damage and blood heal.
With every new flag off, one player plays exactly the web's game.

## Facts already measured (2026-10-03; re-check them, don't trust them blindly)

- **The baseline (`u3-n10`):** `tests: 368 / 368 passed, 0 failed`, `replays: 22 / 22`, `golden tests: 86 / 86`
  (12:01); the Player's smoke `[smoke] 32.1s DONE: all steps passed`; `-skygrindGolden` `30 vector sets (30529
  records), 22 replays (22 exact); 0 failing, 0 mismatches`; bench median 2.91 ms, worst 1% 8.69 ms on Balanced, GC Used
  Memory +103,916 KB in 60 s (u3-n9: +75,672 KB on Silent). Harness: `dotnet run --project Tools/CoreHarness -c Release`.
- **The web modules** (`C:\Users\gaige\dev\skygrind\src`, read only):
  - `survival/objectives.js` (136 lines): HOLD (a 4 m ring, `objectives.holdTime` 30 s in all, points x1.5 inside,
    bonus 50 x round; the round can't clear until held: survival.js's `holdRound`) and GOLD RUSH (kills x2; 3+ kills
    within 3 s pay 25 x chain); `config.objectives` :592; the kind from `ai/directorMath.js roundType` :194-203
    (`director.rounds.objectiveFrom 6, objectiveEvery 4, holdOffset 2, goldOffset 3`: HOLD 6, 14, 22, 26..., GOLD 7,
    11, 19, 27...); never a boss or swarm round.
  - `survival/protocol.js` (191 lines): 4 relays (`map.relaySpots`), awake at `protocol.wakeRound` 6, hold BUY (zone
    open), events `relayLit`, `protocolSurge`, `protocolComplete`, `secretRelay`, `banner`; the SURGE (`protocol.surge`,
    4 squads of charger / lancer / warden / spitter) joins the round; its clear forges every owned gun free (`forge`),
    +2500 (`protocol.bonus`), a cosmetic finish (finishes are not ported: log it); a hidden 5th relay (`secretRelay`).
    Config :617-621.
  - `survival/traps.js` (250 lines): SHOCK PLATE 750, CRUSHER 1000, GRAVITY FAN 900, ARC PYLON 1250 (`config.traps`
    :1090-1100: time 4.8-8 s, cooldown 30-40 s, `kill` 99999 to regulars, elite / boss damage), the switch beside each
    (`map.trapSpots` sx / sz), TRAPPED style bonus; imports `assets/machineModels.js` and `fx/arcs.js` (not ported:
    draw the pylon's arcs simply and say so).
  - `modes/runMoments.js` (90 lines): `gradeOf` (S / A / B / C / D from round x0.5 full at 25, best rank x0.3, accuracy
    x0.2; bands S 0.85, A 0.7, B 0.55, C 0.4: `config.results.grade` :1176), `scoreLine`, the best moments (biggest
    style award, longest swing, top speed, biggest multi-kill within `results.multiWindow` 1.2 s).
  - **Web tests:** `tests/night6.test.js` (THE PROTOCOL :93-180: relays on their showpieces, the finish flag; objective
    rounds :208-235: the schedule, hold spots clear of every box), `tests/runModes.test.js` :6-20 (the grade, the
    score line). No traps test.
- **The map export** (`PortExport/maps/survival.json` `data`) has `relaySpots` (4), `secretRelay`, `holdSpots` (5),
  `trapSpots` (4, with switch positions), `forgeSpot`, `perkSpots`, `zones`, `startZone` [V].
- **What Unity already has** [V]: swarm rounds (`Director.IsSwarmRound`: 8, 13, 18...; no reward, no banner); boss
  rounds (5, 10, ...) "play with what exists" (Director.cs header) and `Director.Available(kind)` (husk, swarmer,
  spitter); `Economy.SetBoost` / `Grant`; `Forge.Give(id, 0)` (a free forge); `PowerUps.Spawn(at, type)`;
  `Buyables` items (the Mystery Box pattern); `Zones.IsOpen` / `ZoneAt`; `Records.Submit(key, run)`; `Lab` (design.lab
  toggles; a LAB run never stores a best). **Missing:** `roundType` (not in the directorMath vectors), a hold hook in
  `SurvivalRun`, a `banner` event the HUD shows (Ui.cs has the banner text, no listener), `Director.AddSquads`.
- **Design flags:** `Design/unity-config.json` (`design.box`, `design.ultra` are the newest); Core objects keep the
  web's rules by default and SurvivalGame applies a flag at boot (`Core/Ultra.cs`), so the golden sets, the replays and
  the existing tests stay the web's. The Player's copy is `Assets/StreamingAssets/Design/unity-config.json` (tracked).
- **Research numbers** (NIGHT11-RESEARCH.md A-B): dog rounds at 5-7 then every 4-6, the last kill drops a Max Ammo;
  Kino's power 4 doors deep (about 4,250 points), thrown at round 3-5; exfil BO6 at 11, 16, 21: about 90 s to reach the
  site, then 35 kills in about 85 s at round 11, failure ends the run; ULTRAKILL hard damage 35% of a hit, a 1 s +
  damage / 20 s timer, drain 15 / s (40 / 50 / 60 at the top ranks, wiped at the top), heals fill only to max - hard
  damage; blood heal per hit: punch 10 / 20 / 30, revolver 10 / 20 / 30, nail 1, pellet 3-30; no passive regen there.

## 0. Tonight's rules (on top of CLAUDE.md and PORT-RULES.md, which are in force)

1. **Branch:** keep working on `port/stage3` (from `u3-n10` plus the research commit). Tag `u3-n11` at the end.
2. **The web repo is read only** (CLAUDE.md rule 2), except a new untracked `skygrind/port-export-p20/` (a copy of p19
   plus your recorders, run with `node` from the web repo root).
3. **Port, don't improve, for every web module:** line for line, comments kept; its web tests ported and seen red; its
   pure functions' vectors (`roundType`, `gradeOf`, `scoreLine`) recorded in p20 and checked bit for bit; sessions for
   objectives and traps on NIGHT10's p19 pattern (fake enemies facade, a box arena) if they cost under 30 minutes, else
   log why not. A difference from the web is a bug until a vector proves otherwise.
4. **New design tonight only:** the power switch, the swarm round's drop and banner, exfil, the LAB's ULTRA HEALTH.
   Their numbers go in `Design/unity-config.json` with a `_why` naming the reference (NIGHT11-RESEARCH.md's sections and
   URLs). Each is a flag; defaults: power, swarm drop, exfil ON (the research recommends them; GG delegated the wave);
   ULTRA HEALTH a LAB toggle, OFF (D-18 stays GG's). With the flags off, one player plays exactly the web's game.
5. **Numbers:** every web number from `PortExport/config.json` (CLAUDE.md rule 5); a missing key is re-exported in p20.
6. **Movement stays bit-exact.** Mover.cs changes only if an item names it; none does. After every Core commit: every
   golden test, 22 / 22 replays.
7. **No questions, no waiting.** Pick the safe default, log it with the rejected alternative, continue. A status line in
   `NIGHT11-LOG.md` at the end of every item, with times from `date`.
8. **GG owns sound and looks** (CLAUDE.md rule 7): relays, the switch, the radio, the exfil site and traps are the web's
   models or plain blocky shapes in the dusk palette (cyan = usable; red-orange = danger: the crusher and the plate
   when live). No audio code; new events get names and a PORT-STATUS line marked port-only.
9. **What only GG decides goes in HANDOFF's pending decisions, never into code:** D-18 (ULTRA HEALTH as the default),
   D-23, D-24, D-02, D-16, D-21, D-22, D-09, D-11..D-15.
10. **Agents:** background agents split by file ownership (memory `night-parallel-agents`): a p20 recorder in the web
    repo; Core work in a git worktree compiled with the harness; Game agents on named files. You alone drive the Editor
    and git.

## 1. Start (U3-24.1)

1. Read the files listed at the top, CLAUDE.md first.
2. Run HANDOFF section 1, every row; `RECEIVED` in `NIGHT11-LOG.md`. HEAD is one research commit above `u3-n10`
   (HANDOFF's update note). If tests, the Player smoke or the Player's golden fail, fix that first and log it.
3. Check HANDOFF section 5 for GG's answers (D-18, D-23, D-24) and log them. If GG turned D-18 on, ULTRA HEALTH ships
   as a design flag ON instead of a LAB toggle.
4. Load `unity:unity-cli` before driving the Editor.

## 2. Definition of done (all measurable)

1. **U3-24.2, the allocation profile (NIGHT10's debt):** a Development Player build (the night's first build) runs the
   bench; the top allocators per SimStep ProfilerMarker and per frame are listed in the log (GC Allocated In Frame);
   any from NIGHT10's code fixed with a ZeroAllocTests line; the rest listed in HANDOFF risks with numbers.
2. **U3-24.3, special rounds:** `roundType` ported (p20 vectors, bit for bit); objectives.js ported (HOLD with
   SurvivalRun's hold hook, GOLD RUSH, the ring, the HUD line, the `banner` event shown by the HUD); night6.test.js's
   objective-round tests ported; **new:** a swarm round opens with a banner and its last kill drops a power-up from the
   run's power-up stream (`design.zombies.swarmDrop`); tests (the drop on and off; off = the web); a smoke step holds a
   HOLD ring.
3. **U3-24.4, THE PROTOCOL:** protocol.js ported (relays and their buyables, the beacon / guide, awake at round 6, the
   SURGE through a new `Director.AddSquads` that keeps the director's rule for unported kinds, the clear's free forge
   and +2500, the secret relay, the HUD line, the results' summary); night6.test.js's PROTOCOL tests ported; a smoke
   step lights the four relays (debug-skip to round 6; god mode) and completes the PROTOCOL.
4. **U3-24.5, traps:** traps.js ported (the four, their switches, timing, kill / elite / boss rules, TRAPPED); tests
   (each trap's kill and its cooldown); a smoke step kills a husk with a trap.
5. **U3-24.6, the power switch** (`design.power`): a switch (hold E, free) at a spot chosen by collider casts in a zone
   2-3 gates from the spawn; on: the traps can be bought only with the power on, and the PROTOCOL's relays wake at the
   first round begin with the power on and the round at least `protocol.wakeRound` (the web's pacing kept; the power
   is the extra key); off: the web's rules; a `powerOn` event (port-only), a banner; tests (on and off); a smoke step
   throws it before buying a trap.
6. **U3-24.7, exfil** (`design.exfil`): from round 11, every 5 (BO6), a radio at the spawn (hold E in the break);
   the next round becomes the exfil: a site (3 spots by collider casts, one picked by round), reach it within 90 s, then
   kill 35 (scaled per later exfil round: log the rule) within 85 s, the director at full pace; success ends the run as
   a win (RESULTS: "EXFIL", records keep the flag, the recap names it); failure ends it as a death ("EXFIL FAILED");
   events `exfilCall`, `exfilStart`, `exfilDone`, `exfilFail` (port-only); tests (the schedule, success, failure, off =
   no radio); a smoke step exfils at round 11 (debug skip, god mode).
7. **U3-24.8, the run's grade:** runMoments.js ported (`gradeOf`, `scoreLine` with p20 vectors; the moments tracker);
   RESULTS shows the grade, the moments and the exfil line; runModes.test.js's grade tests ported.
8. **U3-24.9, ULTRA HEALTH (LAB, off):** `design.lab` toggle `lab.ultraHealth` with `design.lab.ultraHealth` numbers
   and a `_why` (ULTRAKILL wiki): hard damage 35% of each hit, a 1 s + damage / 20 s timer, drain 15 / s, faster at the
   top ranks (AIRBORNE 40, SAVAGE 60, SKYGRIND wipes it: log the mapping), every heal and regen fill only to max - hard
   damage, a grey cap on the HP bar; a blood heal per hit within 4 m of the hit point (punch / GAUNTLET 20, KESTREL 10 /
   30 head, THUNDERCLAP 10 per enemy, STITCHER 1 per nail, LONGWAVE 30, blasts 0); the parry heals to the cap; tests
   (on and off; the LAB stamp on the results).
9. **U3-24.10, the close:** section 7, every line.
10. **Invariants at the end:** `tests: N / N passed, 0 failed` with N above 368; every golden test passes (above 86);
    replays 22 / 22; the Editor and Player smokes DONE with the new steps; the Player's `-skygrindGolden` 0 mismatches;
    `git status --short` shows only `UnityConnectSettings.asset` (if modified) and untracked NIGHT7..NIGHT12.md.

## 3. Notes (decided; don't re-open them)

- **One currency (points).** No new currencies or crafting (research pitfall 2).
- **Add to the round loop, don't replace it** (pitfall 1): HOLD, the SURGE and exfil live inside SurvivalRun's rounds.
- **THE FORGE stays open from round 1** (the web's rule; BO6's Liberty Falls). The power gates the relays and traps.
- **The SURGE plays with what exists** until the roster (N13): the director's rule, logged; it never blocks the
  PROTOCOL from completing.
- **Exfil ends the run either way.** Records keep the round and an exfil flag; a LAB run still never stores a best.
- **Play checks place actors by casting the collider** and keep god mode on through any buy (memory
  `play-check-eval-recipe`). Smoke steps that need a late round use `run.DebugSkipTo` on a seeded run.

## 4. Testing budget

- Per item: one compile, the item's tests (seen red once), at most one Play check (one eval file). The harness first
  for new vectors. The full suite after each Core commit and once at the close.
- **Caps:** Play checks at most 5; captures at most 4 (the power switch on, a HOLD ring, a relay lit with its beacon,
  RESULTS after an exfil); Player builds at most 2 (U3-24.2's Development Player, the close's). **Caps are caps:** if a
  retake would cross one, report it short instead.
- One heavy job at a time (a build, a bench, the suite).

## 5. Mistakes not to repeat (NIGHT7-10)

1. Patch scripts made through the shell: create `.cjs` / `.cs` patches with the Write tool, then run them.
2. Unqualified names in an eval file: no `using` lines; fully qualify (`Skygrind.Game.SliceSmoke.Begin()`).
3. A smoke step that outlives god mode (NIGHT10: VIGOR's apply reset health.max and the run ended mid-step).
4. A search direction not checked (NIGHT10: a target search ran through the ARMORY wall).
5. Reading the wrong property in a check (NIGHT10: `activeSelf` on objects WeaponFx toggles by renderer).
6. A string built per step or per frame (NIGHT10: `$"ads.weapons.{Weapon}"`; PORT-RULES section 6).
7. A night prompt's fact taken on trust (NIGHT10's "a parry doesn't touch the dash pips" was wrong): read the code.
8. Builds rewrite files: `git diff --stat` after each build; restore `Mobile_RPAsset.asset` (D-09).
9. The tag after the close: every docs fix in the close commit, then tag once.

## 6. Order of work and budgets

| Order | Item | Budget |
|---|---|---|
| 1 | U3-24.1 verify on arrival (and GG's answers) | 15 min |
| 2 | U3-24.2 the allocation profile (Development Player) | 45 min |
| 3 | U3-24.3 special rounds (roundType, objectives.js, the swarm drop) | 75 min |
| 4 | U3-24.4 THE PROTOCOL | 90 min |
| 5 | U3-24.5 traps | 90 min |
| 6 | U3-24.6 the power switch | 45 min |
| 7 | U3-24.7 exfil | 90 min |
| 8 | U3-24.8 the run's grade | 45 min |
| 9 | U3-24.9 ULTRA HEALTH (LAB) | 60 min |
| 10 | U3-24.10 close | 60 min |

- **An item 25% over its budget:** ship what works behind its flag, mark it `in_progress` with what remains, move on,
  come back after the others.
- **The close starts by hour 10:30** (scale it with the run's length) whatever is left.
- **Cut order** (only if the run is cut short): ULTRA HEALTH, then traps (and the power switch's trap half), then the
  grade's moments (keep the grade).
- **Never cut:** U3-24.1, U3-24.3, U3-24.4, U3-24.7, U3-24.10, and the rule that the flags off give the web's game.

## 7. Close (U3-24.10): the handoff loop

1. **The invariants, results in the log.** Rebuild the Player (Release; scenes Survival then Arena), then `git diff
   --stat` and the restores; INV-smoke (with the new steps); `-skygrindGolden`; one bench (`bash
   Tools/survival-bench.sh 60`, the plan polled).
2. **`tasks.json`:** every item touched has a status and evidence (a commit hash, an output line, a time). Never delete
   an item. U3-24 passes when its acceptance is met.
3. **Docs:** PORT-STATUS.md (Survival, the director, UI, events: `relayLit`, `protocolSurge`, `protocolComplete`,
   `secretRelay`, `objectiveStart`, `objectiveDone`, `trap`, `trapKill`, `banner` are the web's; the power's and
   exfil's are port-only), PORT-PLAN.md section 2 ticks, ZOMBIES-ULTRAKILL-RESEARCH.md section 3 (power, special round,
   exfil, secrets), PORT-RULES.md (a rule learned twice).
4. **Rewrite `HANDOFF.md`** with the `handoff` skill, in its own structure (a new anchor, the verify block with
   tonight's values, next actions = N12 in the research order, decisions, risks, dead ends, invisible state, a
   changelog line).
5. **`NIGHT11-REPORT.md` for GG (one page):** what shipped with commits, tonight's numbers, what is partial and why, a
   10-minute play checklist, one question each: the power switch's spot, a HOLD round, a GOLD RUSH, the swarm drop, the
   PROTOCOL's climb (is the guide enough?), a trap, exfil at round 11 (too hard solo? too easy?), the grade, ULTRA
   HEALTH in the LAB (better or worse than regen alone?).
6. **Write `NIGHT12.md`** (untracked) from the research's section 6 N12 row (the rest of guns.js, the hotbar and the
   wheel, THE FORGE tiers) plus save-and-quit, re-checked against the code; split its umbrella in tasks.json.
7. **Promote any lesson seen twice to memory**; propose CLAUDE.md changes in HANDOFF's pending decisions.
8. **Finish:** restore the TMP fallback asset if a Play run changed it; tag `u3-n11` once on the close commit;
   `git status --short` as in section 2.10; nothing pushed, nothing merged.

## GG's standing instruction (verbatim, from NIGHT8)

A standing instruction from the user, the person you are working for. It is about how your turns end. A message with no
tool call in it ends your turn, and the work stops there until you are asked to continue. The user has seen you end turns
in four ways while work they asked for was still owed, and does not want any of them. One: a long summary of what was
done that closes by announcing the next step and has no tool call, so the next thing never starts. Two: an offer to carry
on with something unless the user would prefer otherwise, which stops to wait for an answer the user was not going to
give. Three: a list of decisions for the user when, by your own account, none of them blocks the rest of the work. Four:
deciding that this is a good place to report, because the turn has been long or a milestone is done. Status notes are
welcome, and so are your recommendations on open decisions, but put them in the same message as your next tool call and
carry on with whatever does not depend on the user's answer. If you notice yourself inviting the user to redirect you or
offering to wait, delete it and do the next thing. The stops the user does want are the ones where nothing can move
without them, or where the thing blocking you is deliberately protected from you. This does not override the need for
confirmation on risky or destructive actions.

## Closing reminder (the rules this kind of night has dropped before)

- **Port first, then design:** the web's PROTOCOL, objective rounds, traps and grade are ported line for line against
  their tests and vectors; only the power, the swarm drop, exfil and ULTRA HEALTH are new, each behind a flag.
- **The flags off give the web's game,** and the golden sets and replays run with them off.
- **Write only what was measured.** Every number in the log, report and tasks.json comes from an output line tonight.
- **The web repo is read only** except the new untracked `port-export-p20/`. Caps are caps.
