# NIGHT21.md: SKYGRIND in Unity: MAP NIGHT 2 (the whole map: every zone, every buyable, the quest, the variation; a farm on it), at most 12 h

> **Deferred a fourth time (2026-10-06 ~21:40, at the NIGHT19 close):** GG's playtest notes on NIGHT19 (the shockwave on
> the THUNDERCLAP, zoom on most guns, RB scrolls weapons, regen only at a round's end, open-source speed lines, enemies
> that reach every floor) and his ask for an 8-hour building night became `NIGHT20.md` (U3-34). This file keeps U3-32 and
> moves to NIGHT21: read every "NIGHT20" below as "NIGHT21" (the log `NIGHT21-LOG.md`, the tags `u3-n20` (the base) /
> `u3-n21`, `N21-STATUS`). NIGHT20 builds enemy routes (climb / vault / drop links in the nav field and THE CLIMB judge):
> this map night inherits them; re-run THE CLIMB on every new zone.

> **Deferred three times.** 2026-10-05: GG asked for the overhaul night first, so this moved from NIGHT17.md to
> NIGHT18.md. 2026-10-06 morning: GG asked for a NIGHT17 follow-up first (NIGHT18.md: feel in motion), so it moved to
> NIGHT19.md. 2026-10-06 afternoon: GG asked for the controller and weapon-sandbox night first (NIGHT19.md: every gun
> has a job), so it moved to NIGHT20.md.
> **Renumbered 2026-10-06 (afternoon):** U3-32 (unchanged), the log `NIGHT20-LOG.md`, the tags `u3-n19` (the base) /
> `u3-n20` (this night's close), `N20-STATUS`. Re-read HANDOFF (written at NIGHT19's close) first: NIGHT17 changed the
> HUD, the menus, the identity, the kills, the hands and the movement (one stamina bar: `design.stamina`; the fingerprint
> re-recorded at NIGHT17, THE TAPE format 3); NIGHT18 adds the feel harness (`-skygrindReel` v2, telemetry,
> `Tools/feel-metrics.py`); NIGHT19 changes the pad layout (v2), the guns and their alt-fires, the enemy roster (web
> ports), the stamina numbers, THE TAPE (format 4: the sprint bit) and the fingerprint, and adds THE RANGE / THE CHASE
> (`SimHarness range`, `chase`). DRYDOCK's measured moves and the AUTOPILOT's rounds on it must be re-run on NIGHT19's
> rules before they are trusted. Where this file says "NIGHT16's baseline" read "NIGHT19's".

> **Written 2026-10-05 at the NIGHT16 close** (anchor `u3-n16`) from `QUEUE-map-night-2.md` (written at the NIGHT15 close,
> queued when GG asked for the feel night first), renumbered (U3-32) and re-checked against NIGHT16's changes:
> - **The slide changed** (NIGHT16 U3-29.4, `design.move`: a charge, a flat-ground decay, a hop falloff; on by default,
>   LAB rows). DRYDOCK's measured moves were re-run with the rules on and are unchanged (`SimHarness verbs` and `spam`'s
>   skilled section, NIGHT16-LOG: the lane A slide 28.81 m/s, a rib wall-ride 1.20 s to 7.68 m, LAUNCH RAIL 3 of 6, the
>   deck-lip dash-jump 3 of 4, pads within 0.07 m); the AUTOPILOT on DRYDOCK: `newmap PASS ... round 8 / 8, cleared 7`.
> - **The fingerprint baseline was re-recorded** (NIGHT16 `0ac694e`): FingerprintBaseline.cs is tonight's "EQUAL" target.
>   THE TAPE is format 2 (format 1 replays with the move flags off).
> - **The impact layer** (`design.impact`, HIT FREEZE / SLOW-MO) and the hands (`Viewmodel*.cs`) are new; the Player's
>   strip tool (`-skygrindStrip <dir> [scenario]`, `Game/FeelStrip.cs`) and the tour's UI pass (`-skygrindShotsUi`) exist.
> - **If GG's message answers NIGHT16's questions** (FEEL-PLAYTEST.md's six, NIGHT16-REPORT.md section 6: the slide's
>   recharge and the held-slide crawl, the freeze lengths and the 0.5 s gap, slow-mo's default, the fist, the GAUNTLET
>   under the HUD plate), apply those first, each in its own commit with its measurement, before the map work.
>
> NIGHT15 decided nothing GG owns: it built the pipeline, designed three concepts, recommended DRYDOCK (D-59, GG's) and
> greyboxed its heart (THE GANGWAY, THE CRADLE, THE RIBS, THE SLIPWAY). **If GG picked another concept (D-59), this night
> builds that concept's heart instead** from the same pipeline (a `Design/maps/<id>.plan.json`, `SimHarness mapgen`,
> `Build_<id>.cs`), and the rest of this prompt applies to it. The night after this one: the look (lighting, landmarks,
> set dressing) and the first-fifteen-minutes pass (`QUEUE-first-fifteen.md`); then the POWER guns
> (`QUEUE-power-guns.md`).

You are Claude Code working in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP). GG starts
this run and leaves; the Editor is open on this project. Nobody answers questions during the night.

## Why this night

DRYDOCK's heart plays: the AUTOPILOT clears 5-8 rounds in it, the validator is clean, and the zones' moves were measured
on the real Mover (`SimHarness verbs`: a 28.8 m/s slide down lane A, a 1.2 s wall-ride on a rib to 7.68 m, 3 of 6 kicker
take-offs onto LAUNCH RAIL, the pads within 0.07 m; unchanged under NIGHT16's slide rules). But half the map is hatched on
the plan: no power room, no armory, no ARCLINE zone, no exfil pier; the quest runs on two placeholder relays; nothing
varies per run; the hub is mirror-symmetric and the north row is three equal boxes (MAP-DESIGN.md section 4's review, "As
built"). The farm still measures the bot's limits (NIGHT14: its HOLD ring test, its shopping, its guns, no exfil), so a
farm on the new map says nothing until the bot is fixed.

## Read these files

- **`CLAUDE.md`** first, then this file, then **`MAP-DESIGN.md`** (all of it, "As built" last), `Design/maps/drydock.plan.json`,
  `Reports/MAP-drydock-validate.md`.
- **`HANDOFF.md`** sections 0, 1, 4, 5 and 6; **`PORT-RULES.md`** sections 6, 11 and 12; **`tasks.json`** (write U3-32 and
  U3-32.1 .. U3-32.9 below before any code); `FEEL-DESIGN.md` section 3 (what NIGHT16 measured) and section 5's last
  rows (the HUD cues NIGHT16 ported unseen; HUD STYLE, unported; the web gaps still open, section 6).
- The memory notes: `unity-editor-unfocused-autotick` (the Editor stalls unfocused: send `set_autotick` at the start),
  `play-check-eval-recipe`, `night-parallel-agents`.

**Run length: up to 12 hours from your first `date`; the close starts by hour 10:30.**

## 0. Tonight's rules (on top of CLAUDE.md)

1. **Branch `port/stage3`** from `u3-n19`. Tag `u3-n20` once, last, on the close commit. The web repo stays read only.
2. **The sim stays as it is.** The map is data (boxes, wedges, pads, grapple nodes, the core grid, the spots) in the
   schema `SurvivalMap.FromExport` reads; Mover.cs untouched (its NIGHT16 `design.move` fields included); golden 0
   mismatches and 22 / 22 replays at every Core commit.
3. **One plan file is the source** (`Design/maps/drydock.plan.json`): the plan picture, `SimHarness mapgen` and
   `Build_drydock.cs` read it; `Design/maps/drydock.json` is generated, with its byte-identical Player copy.
4. **The old arena keeps its game:** `survival.json`, its fingerprint (NIGHT16's baseline) and every old smoke step
   unchanged.
5. **GG owns** the map's identity (D-59), enemy design, config.json's numbers, round order and run rules. NIGHT15's
   Proposals (MAP-DESIGN.md section 6) stay proposals unless GG answered; a run rule that changes play (per-run variation,
   the FORGE's access, the launch) ships behind a `design.*` flag, **off by default**, with a LAB row if a player should
   try it.
6. **Verify by looking:** every change on screen DONE only with a capture after it and a two-line review. The Player's
   capture tour (`-skygrindShots <dir> -skygrindMap drydock`, `Game/ShotTour.cs`) is the tool: add poses there.
7. **Builds:** build 1 within 2.5 h of the first `date`, every build runs the smoke; at most 5. A smoke run with someone
   at the machine can fail the pad steps and a wall-clock step (NIGHT16 build 3): check the idle time, re-run once before
   calling a failure the build's.
8. **One driver for the Editor;** builders (at most 4) in worktrees, dotnet only (`Tools/wt-csproj.cjs`; a refresh
   regenerates the main tree's csproj: re-glob before a dotnet build there). Long suites async (`run_tests --async_tests
   true`, poll `test_status`).
9. **No questions, no waiting;** one status line per item in `NIGHT20-LOG.md`; end every message with `N20-STATUS: <d>
   done, <s> short, <l> left (of 9) · tests <passed>/<total> · <time from date>`.

## 1. The items (U3-32.1 .. U3-32.9)

### U3-32.1 Start (lead, 15 min)
- `date`; HANDOFF section 1 row by row; `set_autotick` while GG is at the terminal; RECEIVED in `NIGHT20-LOG.md`; the
  machine; U3-32.* in tasks.json. **Done when:** the rows are logged and tasks.json parses.

### U3-32.2 The rest of the map (builder G2; lead 40 min)
- **Do:** the four hatched zones from the plan, each with its verb in geometry and a shape of its own (the north row must
  not read as three equal boxes: the review's finding):
  - THE BOILERWORKS (1250): a 4.5 m roof over pillared aisles, a raised floor with 2 m pits, the open FURNACE YARD behind
    a 1.5 m grate you slide under (THE FIREBOX relay out there, not under the roof: the overload pitfall), the power switch.
    NIGHT16: an uncharged slide still enters at walking speed or more, so a slide under the grate always passes: measure it.
  - THE DRAFTING OFFICE (1500): tight rooms, the web's ARMORY wall of every gun (`data.armoryWall`).
  - THE GANTRY (2000): crane platforms at 10-14 m over the low floor, crane hooks as grapple nodes, aimed pads for players
    without the ARCLINE, the ARCLINE's gadget spot here, THE HOOK relay on a crane's tip.
  - THE PIER (1500): the exfil pier at the slipway's foot.
  - The doorways the plan's north edge waits for; break the hub's symmetry (the slipway deck reaching into the ring's
    north-east corner; a crane over the ring).
- **Done when:** `SimHarness mapvalidate`: findings 0 (VERB rows named); `SimHarness verbs` extended to each new zone's
  signature move (a slide under the grate, a swing between hooks, a pad chain), each with its measured line (with
  design.move on: the game's default); the Editor's `BuildDrydock.Build()` writes the same bytes ("the file before: SAME").

### U3-32.3 Every buyable (lead + G2; 40 min)
- Every ported gun on a wall somewhere (one per zone where the plan says; the armory wall for the rest), every ported
  perk (`Perks.Ported`: 10 of them) at a spot, the 6 box spots, the traps (`data.trapSpots`: plate, crusher, fan,
  pylon), THE FORGE on the hull deck, the ARCLINE in the gantry.
- **Done when:** the headless composition line reads every wall gun hung and every perk machine standing (mapgen's line);
  the validator reaches every one (or VERB).

### U3-32.4 The quest and the objectives (lead, 40 min)
- THE FIREBOX and THE HOOK replace the two placeholder relays on the hull deck; KEEL TOP and LAUNCH RAIL stay; the crow's
  nest stays the secret. HOLD spots: 6 (never under the boilerworks' roof). Exfil: the radio in the gangway, sites on the
  pier, the slipway deck, the ribs' cross alley.
- **Done when:** the smoke's new-map step (24) also lights the four relays on DRYDOCK (a scripted run like the arena's
  step 10, through the map's spots) and holds one HOLD ring; captures of the four relays reviewed.

### U3-32.5 The bot fixes (builder A2; lead 40 min): `QUEUE-first-fifteen.md` U3-28.3 "The farm tells the truth"
- Its five fixes (HOLD by the game's own test, shopping by route time, guns by damage at range, exfil called, per-round
  records), each test-first. **Done when:** as written there, plus `SimHarness newmap Design/maps/drydock.json 300` still
  PASS.

### U3-32.6 A farm on both maps (lead, 40 min; after U3-32.5)
- 3 x 100 runs on the arena (the new baseline, `Reports/farm-n17-arena/`) and 3 x 100 on DRYDOCK
  (`Reports/farm-n17-drydock/`); `node Tools/farm-report.cjs diff` between them. **Done when:** both reports and the diff
  are written and quoted: rounds reached, what downs you, kills by gun, exfils, unspent points; observations only (GG's
  numbers). NIGHT16's arena farm (100 runs, normal, seeds 1..100, the slide rules on: mean round 10.06, median 7.5) is in
  `Logs/farm-n16-b1/` for comparison.

### U3-32.7 Per-run variation behind a flag (lead, 40 min)
- `design.variation` (off by default, a LAB row): the box's first spot, the HOLD spots used, the GOLD RUSH route and the
  PROTOCOL's first zone drawn from the run seed (Mulberry32, reset at run start). The fingerprint stays EQUAL with the
  flag off. **Done when:** a test shows two seeds differ and one seed repeats; the fingerprint EQUAL.

### U3-32.8 The captures (lead, 30 min)
- The tour: every zone's signature place, the landmark from three far corners (the review found the mast hidden inside
  the rib alleys: say whether night 2's geometry fixes that), TITLE's MAP row. **Done when:** at most 12 captures, each
  reviewed in two lines; MAP-DESIGN.md "As built (night 2)".
- **NIGHT16's unseen HUD:** NIGHT16 ported nine of the web's HUD cues that never got on screen (FEEL-DESIGN.md section
  5, entries 30-38: the points pops, the damage vignette, EXPOSED's edge, the buy prompt, REGEN, the move row, the ADS
  ring / scope, the crosshair tag, the charge edge). Spend two of the 12 captures on them: a fight with a kill and the
  HP below full, and the LONGWAVE scoped. Log what crowds or overlaps (the scoped range label vs the movement ticks is a
  known risk); fix only a clear overlap, the rest goes to GG.

### U3-32.9 Close (lead, 60 min)
- The final Release build: the smoke (all steps; NIGHT14's residual, steps 18 / 21, was fixed in NIGHT16 (`51bc000`:
  the aim assist's scan clock and the director's tallies carried across runs): a failure there is a new carry, so start
  from `SimHarness residual check` / `residual diff <recipe>`),
  `-skygrindGolden` 0 mismatches, `-skygrindFingerprint` EQUAL, `-skygrindReplay` EQUAL, the UI-hurt Silent series if on
  AC and idle. tasks.json; `NIGHT20-LOG.md`; `NIGHT20-REPORT.md` for GG (one page: the map's state with three captures,
  the farm on both maps, the questions); HANDOFF rewritten (`handoff` skill, anchor `u3-n20`); `NIGHT21.md` (the look and
  the first fifteen minutes); PORT-STATUS's Maps row; tag `u3-n20`; the close table, then `NIGHT20 CLOSED`.

## 2. Caps

Play checks 3 · captures 25 · Player builds 5 · bench runs 6 (the close's series only) · builders 4 · reviewers 1 ·
research 30 min. A cap reached makes its item SHORT with the count. Cut order if short: U3-32.7, then U3-32.6's DRYDOCK
half, then U3-32.8's corners. Never cut U3-32.1, .2, .9.

## 3. The close table (print it, then `NIGHT20 CLOSED`)

| Item | DONE / SHORT | Evidence ("done when", from this session) |
| --- | --- | --- |
| U3-32.1 Start | | |
| U3-32.2 The rest of the map | | |
| U3-32.3 Every buyable | | |
| U3-32.4 The quest and the objectives | | |
| U3-32.5 The bot fixes | | |
| U3-32.6 A farm on both maps | | |
| U3-32.7 Per-run variation | | |
| U3-32.8 The captures | | |
| U3-32.9 Close | | |

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

## Suggested `/goal` for GG

> Run NIGHT20.md in C:\Users\gaige\dev\SKYGRIND-unity. Run `date` first, then read CLAUDE.md, NIGHT20.md and the files it
> lists, and work U3-32.1 .. U3-32.9 in NIGHT20.md's order with at most four background builders split by file ownership.
> Never ask me questions and don't stop early; end every message with the N20-STATUS line. Never push, merge or commit to
> master; the web repo is read only. This goal is met when your latest message prints NIGHT20.md's close table with every
> item U3-32.1 .. U3-32.9 marked DONE (with its "done when" evidence from this session) or SHORT (with its measured reason:
> a cap reached, a decision reserved for GG, a blocker after two approaches, or the time limit once a `date` output reads
> 10.5 hours or more after the first `date` output), followed by the line "NIGHT20 CLOSED". SHORT rows count as met.
