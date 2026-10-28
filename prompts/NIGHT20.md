# NIGHT20.md: SKYGRIND in Unity: GG'S PLAYTEST, THEN A BUILD NIGHT (the shockwave on the THUNDERCLAP with a long cooldown, zoom on most guns, RB scrolls weapons, regen at the round's end, open-source speed lines, enemies that reach every floor; then a wave of proven additions, measured), at least 8 h, hard cap 8.5 h

> **Written 2026-10-06 ~22:30, after the NIGHT19 close** (tag `u3-n19` on `d4673c5`). Sources:
> - GG's message of 2026-10-06 evening, after playing NIGHT19's build (quoted below);
> - five research notes written tonight: `Research/2026-10-06-speed-lines.md` (open-source speed lines on GitHub, with
>   licences), `Research/2026-10-06-enemy-routes.md` (how hordes reach high ground; today's nav, file:line),
>   `Research/2026-10-06-balance-zoom-regen.md` (zoom, cooldowns, regen, balance methods),
>   `Research/2026-10-06-additions-backlog.md` (a ranked backlog of proven additions), and a code map (facts copied into
>   this file with `path:line`; re-check any you rely on);
> - `Research/2026-10-06-weapon-sandbox.md` and `Research/2026-10-06-assets-and-prompting.md` (NIGHT19's);
> - NIGHT19's record: `NIGHT19-LOG.md`, `NIGHT19-REPORT.md`, `Logs/sandbox/b0` and `Logs/sandbox/final`.
>
> Map night 2 moved again, to `NIGHT21.md` (U3-32, unchanged; it inherits tonight's enemy routes).

You are Claude Code working in the Unity project `C:\Users\gaige\dev\SKYGRIND-unity` (Unity 6000.3.7f1, URP). GG starts
this run and leaves, with the Editor open on this project. Nobody answers questions during the night.

## What GG said (2026-10-06 evening, his words, lightly trimmed)

> You messed up: the left was supposed to be for the first option and that was supposed to do the shockwave, not the
> pistol. You should be able to zoom in with the pistol like how it was. And it's not balanced: you can just spam it.
> There needs to be a long, long cooldown, like maybe every 15 seconds or even longer.
> The sprinting effect and the lines that appear, the dash lines, are just so forced. Copy an open-source project that has
> way better dash lines that mimic speed, not just that weird border that only appears. It needs to be much more dynamic
> and intelligent in how it shows up, in the visual clarity. Go actually go into the GitHub, I'm serious.
> The enemy AI struggles to go above floors and can't really get out of floors most of the time. I'm not saying it has to
> be the most intelligent AI, but you can kind of just camp out. There always really needs to be a route that enemies can
> climb up, or just find a way to pass by normally.
> The regen happens way too quickly and should only really be at the end of the round. The game needs to be balanced a
> lot more.
> The right bumper could be used to switch weapons as well: just scroll through weapons.
> Most weapons should, besides a few that have unique left clicks, zoom: simple zoom.
> Go well beyond this prompt. Study game design seriously. This is going on for 8 hours overnight. I need a lot of
> building; I'm not seeing enough building. I want so many additions, well beyond the things I'm asking: a ton of real
> changes, additions, iterations, and it's up to you to decide them based on the data, the game design, what's been
> proven in making good games, and how to iterate them.

GG's three NIGHT19 questions, answered by this message: (1) the gust's strength: it moves to the THUNDERCLAP with a
cooldown of 15 s or more; (2) the KESTREL keeps its zoom; (3) NIGHT19's unfinished work resumes tonight.

## GG's switches (GG may edit these before starting; the defaults are in capitals)

| Switch | Default | The other choice |
| --- | --- | --- |
| **SHOCKWAVE** | **18 s, ONE CHARGE** on the THUNDERCLAP's LT, ticking while holstered; the FORGE adds a second charge; never below 12 s | **15 s flat**, no upgrade |
| **SHOCKWAVE LIFT** | **KEEP** the measured +2.6 m (rare at 18 s) | **LOWER** it below the COMET's rocket jump (+1.1 m) |
| **ZOOM** | **MOST**: KESTREL 1.5, STITCHER 1.25 (its detonate moves to SPECIAL), LONGWAVE 3.5 scope; unique LTs: THUNDERCLAP (shockwave), COMET (steer) | **STITCHER KEEPS** its detonate on LT (a third unique gun) |
| **REGEN** | **SEGMENTS**: a full refill at the round's clear (2 s); in a round only back to the top of the current 25 HP segment (5 s, 10 HP/s); earned heals: +4 per kill within 6 m, +10 per punch kill, parries as now | **STRICT**: no passive HP in a round at all; the refill and the earned heals only |
| **PAD** | **RB = NEXT WEAPON** (tap; held 0.25 s = the wheel), **Y = SPECIAL + ARCLINE** (v2's two rows swapped), as layout v3 with a v2 migration | RB = next, **Y = PREVIOUS** weapon, SPECIAL on d-pad left |
| **SPEED LINES** | **WORLD STREAKS**: velocity-aligned streaks in the world around the camera (ported, section U3-34.7), plus a light travel blur on boosts (linear velocity only: never on mouse look); the old screen layer retired | **STREAKS ONLY** (no blur) |
| **ROUTES** | **LINKS + AUTHORED**: long drops, a fixed leap, climb links, authored ladders / chains on the tall perches; flyers and snipers (the web's DRONE, LANCER) answer what can't be climbed | **LINKS ONLY** (no authored map edits tonight) |
| **ADDITIONS** | **PORT**: the web's designs ported as they are (BULWARK, LANCER, WARDEN, the affixes) and the backlog's non-enemy rows; new enemy kinds only as proposals | **NO NEW ENEMIES** (the backlog's other rows only) |
| **BUILDERS** | **6** (G, P, E, N, H, V) | **4** (G, E + N merged, H + V merged, P) if the agent budget is tight |
| **AUDIO** | **OFF**: rule 7 holds; every new moment emits a named event for GG's sounds | |
| **ASSETS** | **PULL** (D-72): permissive art and code with licences | **ART ONLY** |

## GG's notes come first

If `GG-NOTES.md` exists in the repo root, or GG's start message carries notes, read them before anything else. Where
they disagree with this plan, they win. Log what changed and why in `NIGHT20-LOG.md`, under RECEIVED.

## Why this night

What GG played (NIGHT19's final build, `8a33313`, tag `u3-n19`), measured tonight:
- **The gust is on the wrong gun and spammable:** the KESTREL's LT, a 0.5 s cooldown plus an air charge (`Gust.cs:69-76`),
  a +2.6 m lift (THE RANGE's probe: higher than the COMET's +1.1 m rocket jump); it turned the KESTREL's zoom off
  (`Combat.cs:973`). GG meant the THUNDERCLAP's shockwave (the web's `weapons.specials.shotgun.shock` and its unported
  double blast).
- **Only one gun ever zoomed, and tonight none does** with the gust on; the web's `ads.js` rule (right mouse zooms the
  zoom guns, the special everywhere else) was half ported.
- **Regen undoes every mistake in ~6 s:** a 4 s delay (2 s while moving over 8 m/s: a sprint always is), then 25 HP/s
  (`PortExport/config.json` `regen`); nothing at a round's end.
- **The pad's RB carries SPECIAL + ARCLINE;** weapons scroll on Y only (`Gamepad.cs:284-286`).
- **The speed lines are a screen layer with a corner mask** (`UiFeel.cs:62-69`, `CameraFeel.cs:625-650`): GG's "weird
  border". NIGHT19's final reel flagged it in the middle of the screen on sprint and vitals (S14).
- **The horde can't reach high floors or leave them:** on the Survival map 0 of 3,227 nodes above 5 m are reachable
  from the ground and none can get back down; on the spire tops no spawn has a route, so the director spawns nothing
  (`Research/2026-10-06-enemy-routes.md` 1.4).
- **NIGHT19's sandbox work is half in:** S1-S3 and S5-S8 FAIL (`Logs/sandbox/final`), W's niches / ports / combos and E's
  DRONE sit on the rescued branches; the suite is 1182 / 1192.
- **And GG wants volume:** "a ton of real changes ... additions, iterations ... based off the data, the game design,
  what's been proven." The additions backlog ranks 31 rows from shipped games; the iteration loop (a playable build every
  60-90 minutes, a judge before the code, keep / iterate / cut) is in its section 4.

**Tonight's single idea:** fix exactly what GG felt, prove each fix with a number, then build as much of the proven
backlog as eight hours allow, every slice behind a flag, judged, and kept or cut on evidence.

## What "done" means tonight (the contract; agreed before any gameplay code)

NIGHT19's S1-S17 stay (NIGHT19.md's table; S14's rule fixed: the lines' own rect). Tonight adds T1-T8. "(inferred)"
marks a target no source publishes; the lead may retune each inferred target once, at round 1, logged with both values.

| # | Metric | How it's measured | Target |
| --- | --- | --- | --- |
| T1 | **The shockwave is a tool, not spam** | `ShockwaveTests`; the farm's shockwaves per round; THE CHASE KITE on / off; THE RANGE's C3 probe | the cooldown holds (17.9 s: one; 18.0 s: two); 1.5-3 per round; KITE +<= 25 %; >= 3 of 8 swarmers pushed >= 3 m |
| T2 | **Most guns zoom** | `ZoomTests`; the reel's `zoom` | KESTREL, STITCHER, LONGWAVE zoom at 1.5 / 1.25 / 3.5 (FOV 53.3 / 62.2 / 24.3 deg at base 74); THUNDERCLAP and COMET keep unique LTs; the zoom's reticle drawn |
| T3 | **RB scrolls** | `PadRbCycleTests`; the reel's `rbcycle` (pad pass); the smoke's pad steps | tap -> next, hold -> wheel, the hotbar answers within 2 frames; the smoke passes |
| T4 | **Regen at the round's end** | `RegenN20Tests`; THE MARATHON; THE CHASE | M-1 0.8-1.3 x NIGHT19's median round; R1 HP at clear 35-70 %; R2 the refill >= 50 % of healing; R3 <= 25 % of deaths in a round's first quarter; FIGHT r10 >= 120 s; KITE r10 <= 120 s, r15 <= 60 s (S8) |
| T5 | **Enemies reach every floor** | THE CLIMB on Survival and DRYDOCK | R1 0 unreachable player nodes; R2 the worst horde time <= 12 s (inferred); R2b every node can get down within 6 s; R3 on the 10 worst perches, STAND's damage / min >= 50 % and survival <= 2 x the ground's |
| T6 | **Speed you can see** | M8 v2 (L1-L6, U3-34.7) on sprint, boost, boostside, slide, strafe; the bench; reviewers | L1-L6 all pass (the clear ellipse, density and length by speed, direction, 0 streaks over an enemy, p99 + 0.3 ms at most); tonight picked in >= 3 of 4 final judgements on "which shows speed better while keeping the middle readable" |
| T7 | **A ton of building** | the log's rows | >= 12 additions-wave rows landed with their done-when numbers; >= 120 commits from builders (NIGHT19: 45 in its 1:20 of building) |
| T8 | **Nothing broke** | the gates | golden 0 mismatches / 22 exact; the fingerprint EQUAL (NIGHT18's, every block off) and `FingerprintBaselineN20` recorded; replay EQUAL; the smoke passes; the suite green; S16 holds |

## How tonight works (and why)

NIGHT19 built the instruments (THE RANGE, THE CHASE, THE PARITY FARM, the reel's pad pass, S12 / S14) and then lost 3.5
hours: at 1:22 every builder died on the account's spend limit (HTTP 429) and the lead stalled until 20:55. Its gun
retune, the ZAPPER / BOUNCER ports, the combos and the DRONE sit uncommitted in two worktrees. Tonight spends the hours on
building, with the instruments already there.

1. **GG's six fixes first, each one measured, in the first round.** They are small and he will try them first. Then
   NIGHT19's unfinished work, then the additions.
2. **Build volume is the goal.** GG: "I'm not seeing enough building." Every builder has a queue of 5-8 items, so a
   builder that lands one takes the next at once, never waits for a round, and never polishes a met item without a new
   number to move. The lead integrates every 45-60 minutes and builds every ~75 minutes.
3. **Every addition earns its place with evidence.** Each row in section 3 cites where the idea is proven (a shipped game
   or a talk, from the research notes) and names a done-when number. "It feels better" is never a done-when.
4. **Commit every finished slice at once.** Never hold more than ~30 minutes of work uncommitted: NIGHT19 lost W's and
   E's work that way. A slice is a test plus the code that turns it green, or one visual change with its probe.
5. **If an agent call fails** (HTTP 429, "spend limit", "rate limit", an API error): don't wait and don't stop. The lead
   builds the cut order alone (section 4), commits, and keeps the rounds going with fewer builders. It retries launching
   or resuming one agent at most once every 30 minutes, and logs each try. A night with no builders still ends with a
   built, gated, documented game.
6. **Measure before and after.** Every gameplay change sits behind a design block (`--off19` is NIGHT18; tonight adds
   `--off20` for NIGHT19's game). The judges and the reels run on the A side and the B side.
7. **Separate, blind judges for the look**; the lead's opinion of a frame is advisory. Reviewers are budgeted (caps) and
   run on rounds 2, 4 and the final.
8. **Port before inventing.** Search Core, the web (`C:\Users\gaige\dev\skygrind`, read only: `src/combat`, `src/enemies`,
   `src/ai`, `src/fx`) and the GitHub repos named in the research before writing a mechanic or an effect. A port keeps
   the source's comments and its licence notice.
9. **State lives in files.** After every integration the log's last line is
   `NEXT: <what the lead does next, which builders are out, what each owes>`.
10. **Your context will be compacted during this run; that is expected.** Don't stop or rush because of it. After a
    compaction or any fresh start, before new work: read `NIGHT20-LOG.md`'s first `date` line and its last `NEXT:`
    line; `tasks.json`'s U3-34 rows; `git log --oneline -8`; run `ListAgents`; run `date` (against the first); run
    `bash Tools/unity-loop.sh status`. Then carry on from `NEXT:`.
11. **Don't game the judges.** The cells, the aim model, the bots, the reel's scenarios and every target are frozen once
    the baseline is logged; a builder never edits `Tools/SimHarness/Harness.Sandbox.cs`, `Tools/sandbox-cells.json`,
    `Game/Bot/Sandbox*.cs`, `Tools/feel-metrics.py` or a target. A change only a judge can see is a failure. Tests verify
    behaviour; if a test is wrong, say so.
12. **No placeholders.** A gun, an enemy, a link type or an effect without its model, view, tells and events is SHORT,
    not DONE.
13. **Every status claim cites a tool result from this session.**

## Read these files

- **First:** `CLAUDE.md`, then this file, then the five research notes named at the top (all of
  `Research/2026-10-06-balance-zoom-regen.md` section 7, `-speed-lines.md` sections 0 and 3, `-enemy-routes.md` sections
  0, 1 and its recommendations, `-additions-backlog.md` sections 0, 2.7, 3 and 4).
- **`HANDOFF.md`:** the NIGHT19 block at the top, then sections 1, 5, 6, 7.
- **`NIGHT19-LOG.md`:** the judges' section (the hand checks, the rules fixed before the baseline), "The stop", the
  close. **`NIGHT19-REPORT.md`:** all of it. **`NIGHT19.md`:** section 1 (the judges), sections 6-7 (the reviewer and
  builder briefs tonight's extend).
- **`Logs/sandbox/b0/SANDBOX.md`** and **`Logs/sandbox/final/SANDBOX.md`** (NIGHT19's numbers), `Tools/sandbox-cells.json`.
- **`PORT-RULES.md`** sections 6, 11, 12; **`IDENTITY.md`**; `DESIGN-PILLARS.md`.
- **The web, read only:** `src/combat/weapons.js` (the THUNDERCLAP's double blast, `:606-637`), `src/input/ads.js`
  (routeSpecial), `src/enemies/lancer.js`, `bulwark.js`, `warden.js`, `affixes.js`, `drone.js`, `src/ai/` (the leap, the
  seeds), `src/nav/navGraph.js` (the build by simulation).
- **The rescued branches:** `git log --oneline -3 worktree-agent-aab6b3cfb8a45d18d` (W, `9687752`) and
  `worktree-agent-a58e9c83cb2f99f09` (E, `ad1e828`).
- **`tasks.json`:** write U3-34 and U3-34.1 .. U3-34.12 before any code. `NIGHT21.md` (map night 2) uses U3-32.
- **The memory notes:** `gg-weapon-controls-taste`, `spend-limit-stops-agents`, `bench-power-plan-flips`,
  `player-smoke-focus-pause`, `play-check-eval-recipe`, `unity-editor-unfocused-autotick`, `night-parallel-agents`,
  `feel-done-means-measured`.

**Run length: at least 8 hours, hard cap 8.5 hours, from your first `date`.**
- The close starts at 7:20 (the code freeze, then the final build), and its table prints at 8:00 or later. Run `date`
  right before printing the table, so the transcript proves the time.
- Running past 8:30 is a failure, and so is closing before 8:00.
- Show the time against the budget in every status line, like `3:10 / 8:30`.

## 0. Tonight's rules (on top of CLAUDE.md, which holds except where this prompt, GG's, says otherwise)

1. **Git:**
   - Branch `port/stage3` from its head (`u3-n19` plus docs). Tag `u3-n20` once, last, on the close commit.
   - No push, no merge, nothing on master. The web repo is read only.
   - Before every commit, restore with `git checkout --` the theme SDF font assets (`Assets/Resources/Fonts/*SDF.asset`),
     the TMP fallback asset and `Mobile_RPAsset.asset`; builds and Play mode rewrite them.
   - A package's `Packages/manifest.json` / `packages-lock.json` change is its own commit (the lead's).
2. **Every NIGHT20 gameplay change sits behind a design block that can be switched off.**
   - The shared commit adds `Core/N20Blocks.cs` (as `N19Blocks`), the blocks in `Design/unity-config.json` with
     `enabled: false` and a `_why` each, THE TAPE format 5 (a NIGHT20 byte; formats 1-4 replay with it off), and
     `--off20` / `-skygrindOff20` (every NIGHT20 block off: NIGHT19's game). `--off19` now turns off NIGHT19's and
     NIGHT20's blocks (NIGHT18's game).
   - The blocks: `design.shockwave` (the THUNDERCLAP's LT and its cooldown; replaces `design.gust` on the KESTREL),
     `design.zoom` (zoom on most guns), `design.regen.n20` (the round-end heal), `design.routes` (the nav's climb / vault /
     drop links and the climbers), `design.director.n20` (special rounds, pacing), `design.arsenal.n20` (tonight's gun
     retune and ports), `design.pad.n20` (RB scrolls), `design.adds` (tonight's additions, one sub-block each).
   - The Player copy stays byte-identical (`N19SharedTests` checks it).
   - **Why:** with every NIGHT20 block off the game is NIGHT19's bit for bit, the A side for every judge, and the
     fingerprint proves it.
3. **The determinism wall:**
   - Golden and replays: 0 mismatches, 22 exact at every commit that touches Core.
   - The composed fingerprint keeps running with every NIGHT19 / NIGHT20 block off (`FingerprintBaseline.N19OnPath`
     false) and must print NIGHT18's baseline at every Core commit; `fingerprint --off19` the same.
   - NIGHT19's close didn't re-record an on-path baseline. Tonight's close records `FingerprintBaselineN20` (every block
     as configured) once, with the reason, and the Player's `-skygrindFingerprint` prints both lines from then on.
   - `Mover.cs` changes only by the lead, only where an item names it (the shockwave's push uses `Mover.Impulse`).
4. **Numbers:** gameplay numbers in design blocks only; visual numbers in `UnityTuning.*.cs` with a comment; colours from
   `UiTheme` tokens. Palette roles: cyan = the player's (the speed lines, the shockwave's air), red-orange = enemies and
   danger (every new enemy tell, the climb tell), magenta = RIVAL, gold = points, pale violet-white = the boundary.
5. **Randomness:** gameplay from the run's `Mulberry32`; THE RANGE's aim from its own stream; VFX from a render-side
   stream.
6. **Accessibility:** flashes through `flashGate`; REDUCED MOTION stills kicks and thins the speed lines; text >= 18 px;
   every glyph keeps a text fallback.
7. **Performance:** no allocation per frame in new code (PORT-RULES 6); pooled particles; S16 holds (bench p99 <=
   baseline + 1.0 ms, never beside another process: see the caps).
8. **Assets (D-72, GG's ASSETS switch, still on):** CC0, MIT, BSD-3, OFL, Unlicense, zlib only; read the LICENSE file
   before an import; art in `Assets/ThirdParty/<Name>/` with the licence beside it; code ported keeps its notice in the
   file header; a `THIRD-PARTY.md` row per item; no audio; re-tint to the palette roles. Not allowed: no-licence repos,
   non-commercial licences (MMFeedbacks), GPL / LGPL code, anything that copies another game's branding or UI one to one.
9. **The smoke and focus:** a window taking focus pauses the Player smoke (memory `player-smoke-focus-pause`). In rounds
   it may run beside builders, one re-run allowed with builders asked to hold for 5 minutes; at the close it runs alone.
   Reels skip the focus pause. The Editor's smoke fails its TITLE pad steps by design; the Player's smoke is the check.
10. **The Editor plays GG's saved map:** set `SurvivalGame.PendingMapId = RunSetup.DefaultMap` and reload in Play mode;
    never `PickMap`.
11. **Caps** (a cap reached makes its item SHORT with the count):
    - Player builds: 9 (build 1, five rounds, the final, two spare).
    - Editor Play checks: 8.
    - Reels: 18 on Players (each build's normal and pad pass), 6 in the Editor.
    - Farm batches: 6, each at most 4 processes, from a snapshot of the harness build (`Temp/sh-<tag>`); never beside a
      bench, a build or the close's smoke; each waited for and closed by PID.
    - Bench runs: 4 (two on build 1, two on the final), **never while the Editor runs tests, a build, a farm or another
      Player** (NIGHT19's final bench ran beside the suite: p99 100 ms, worthless). Start the close's suite after the
      benches.
    - Builders: 6 alive at a time, resumable by SendMessage; agents in total (builders, reviewers, research): 24.
    - Reviewer agents: 8 (two on rounds 2 and 4, four for the final: two pairs, sides swapped).
    - Research agents: 2 (only for a GitHub / licence lookup a builder can't do itself).
    - Lead verification time: 90 minutes, logged per check.
    - Disk: a reel is ~0.7 GB; keep full frames for build 1, the last two rounds and the final.
12. **One driver for the Editor.** Builders work in worktrees with dotnet only (`node Tools/wt-csproj.cjs <root>`), never
    run `unity`, write tests first and see them red, use topic-named members in shared partials, and commit every slice.
13. **Waiting:** wait on a build, a smoke, a farm or a builder with a background command or `Monitor`, never a sleep loop;
    while waiting, integrate, run a judge, write the log, or build an item yourself.
14. **No questions, no waiting.**
    - One status line per item in `NIGHT20-LOG.md`.
    - End every message with `N20-STATUS: <d> done, <s> short, <l> left (of 12) · round <r> · builders <alive>/<6> ·
      judges <passed>/<total> · tests <passed>/<total> · <h:mm> / 8:30`.

## 1. The judges (the lead's; built at 0:25-1:20, then frozen)

NIGHT19's judges stay as they are (`SimHarness range | chase | pfarm | sbreport`, `Game/Bot/Sandbox*.cs`,
`Tools/sandbox-cells.json`, hand-checked, deterministic): read NIGHT19.md section 1 and `NIGHT19-LOG.md`'s judges
section. Tonight adds three, plus reel metrics. Each writes Markdown and JSON to `Logs/sandbox/<tag>/`; each is
deterministic (two runs identical: a test), each is hand-checked once against its own log before it's trusted.

### THE CLIMB (`climb [--map survival|drydock] [--off20] [--tag t]`, new `Game/Bot/SandboxClimb.cs`)

Can the horde reach every place a player can stand? On the map's nav grid with every gate open:
- **The player's set:** every standable node the player can reach from the spawn with the Mover's own reach (walk, jump,
  double jump, dash-jump, wall-jump, the pads), computed on the grid by the jump and step limits in
  `config.movement` (the lead writes the reach model once and checks it on three hand-picked perches by driving the real
  Mover in SimHarness).
- **The horde's time:** for each player node, the husk's travel time from the nearest spawn node along the nav field,
  with tonight's links (climb, vault, drop, leap) at their costs. Unreachable = infinite.
- **The perches:** the 10 player nodes with the worst horde time (the camping spots), named by zone and height.
- **The perch test:** THE CHASE's STAND bot (the autopilot, never moving) placed on each of the 10 worst perches at round
  10: its survival time and damage per minute (300 s limit, median of 5 seeds), against the same bot on the ground.
- **Rows:** R1 unreachable player nodes (target 0); R2 the worst horde time to any player node (target <= 12 s,
  inferred); R2b the worst time for a husk on any node to get back down to the ground (target <= 6 s: "can't get out
  of floors"); R3 on each of the 10 worst perches, the STAND bot's damage per minute >= 50 % of the ground STAND's and its
  survival <= 2 x the ground's (the routes research's camp judge; a perch may buy time, never safety).

### THE MARATHON (`marathon [--seeds n] [--off20] [--tag t]`, in the existing farm)

The farm, read for balance instead of guns: normal, the free-play bot, 40 seeds (one farm batch with its 4 processes).
- **Rows:** M-1 the median round reached (target: >= 0.8 x and <= 1.3 x NIGHT19's 9.0, so regen at the round's end makes
  it harder, not a wall); M-2 deaths by round band (rounds 1-5 / 6-10 / 11+: no band holds more than 50 % of the deaths);
  M-3 HP at each round's start (median >= 90 % of max with the round-end heal); M-4 the in-round heal share (heal from
  in-round sources / damage taken: 0.15-0.4, inferred: enough to recover from a mistake, not enough to ignore damage).

### The reel's additions (the lead)

- **New scenarios:** `shockwave` (the THUNDERCLAP's LT from a standstill: level, then down; then pressed again inside
  its cooldown: nothing fires, the HUD shows the cooldown); `zoom` (the KESTREL, the STITCHER and the LONGWAVE each zoom
  on a husk at 20 m, 1.2 s each); `climb` (a husk climbs a 3 m face toward a perched player: its tell and its climb);
  `special-round` (the special round's announce and its first spawns); `regen` (a round ends with the player at 30 HP:
  the round-end heal on the HUD); `rbcycle` (the pad pass only: RB pressed 3 times, the hotbar answers each).
- **Fix `vitals`:** it found no spot for its kill after the sprint (NIGHT19's final reel): sprint around a 6 m circle
  (yaw turning ~120 deg/s), so the start area stays open, then place the kill.
- **S14 rule fix:** NIGHT19's S14 flagged `UI/HUD frame/Speed lines` on sprint and vitals: a full-screen layer whose
  lines M8 already keeps out of the middle. The rule reads the lines' own probe rect (`FeelFx.SpeedLines`), not the
  layer's: log the change and both values (a rule fix, not a target change).
- **M8 v2 (speed you can see):** the rows L1-L6 of U3-34.7 (the clear ellipse, density by speed, length by speed,
  direction, clarity, cost), from V's probes and a streak-direction channel the lead adds to the telemetry schema
  (`Tools/feel-schema.md`); NIGHT18's M8 ("nothing in the central 60 % box") is replaced by L1, logged.

## 2. Who owns what (six builders, each with a queue in section 3; the lead integrates)

| Builder | Owns (creates or edits only these) | Queue (section 3) |
| --- | --- | --- |
| **G: guns and controls** | Core `Combat.cs`, `CombatMath.cs`, `GunMath.cs`, `Arsenal*.cs`, `ArsenalMath.cs`, `Projectiles.cs`, `Impact.cs`, `Forge.cs`, `MysteryBox.cs`, `Core/Guns/*` (Gust.cs becomes the THUNDERCLAP's shockwave; NIGHT19 W's WIP: Zapper, Bouncer, Freshness19, Sandbox19, AltUse), `Game/SurvivalSim.Arsenal19.cs`, `Game/Bot/AutopilotGuns.cs`, `Game/HudReads19.Arsenal.cs` (new: H's alt-meter and freshness-tier reads), the blocks `design.shockwave`, `design.zoom`, `design.arsenal(.n20)`, `design.forge`, `design.freshness`, the gun tests | U3-34.3 (the rules), U3-34.5 (the gun half of the balance), U3-34.8 (the guns), U3-34.9 rows tagged G |
| **P: the pad, input and menus** | `Core/Gamepad.cs`, `Game/Input/*`, `UiNav.cs`, `GamepadMath.MenuNav`, `UiSettings.cs`, `UiControls.cs`, `UiResults.cs`, `UiMeta.cs`, `UiKiosk.cs`, `UiOnboarding.cs`, `UiRadial.cs`, `UnityTuning.Input.cs`, `UnityTuning.Hotbar.cs`'s pad texts, `design.pad(.n20)`, the pad tests; **the bot tonight**: `Game/Bot/AutopilotInput.cs`, `AutopilotShopper.cs`, `AutopilotTarget.cs`, `Farm.cs`, `Tools/farm-report.cjs` (not `AutopilotGuns.cs`: G's; not `Sandbox*.cs`: the lead's) | U3-34.4, U3-34.9 rows tagged P (P0-2 the bot tells the truth, P0-3 the pacing trace, E1 the first fifteen minutes, E2 ASSIST's setting, CONTROLS glyphs) |
| **E: enemies, the director, survival balance** | Core `Director.cs`, `Husks.cs`, `Swarmer.cs`, `Spitter.cs`, `SwarmDrop.cs`, `Core/Enemies/*` (NIGHT19 E's WIP: the DRONE; the SHIELDED affix; the LANCER port), `Health.cs`'s Regen and `Stamina.cs`, `Game/SurvivalSim.Stamina.cs`, `Game/SurvivalSim.Enemies19.cs`, new `Game/SurvivalSim.Regen20.cs` (the round-end refill on `roundClear`, the earned heals), `Game/EnemyViews.<Kind>.cs` (new kinds' views as partials), the blocks `design.enemies(.n20)`, `design.director.n20`, `design.regen.n20`, `design.stamina.n19`, the enemy / director / regen tests | U3-34.5 (regen and the survival half), U3-34.6 (the height answers: DRONE, LANCER), U3-34.8 (the enemies), U3-34.9 rows tagged E |
| **N: navigation and routes** | Core `NavField.cs`, `Core/Nav/*`, `NavJumper.cs`, `HuskBrain.cs`, `Husk.cs` (the leap and the climb), new `Core/Nav/RouteLinks.cs` (the extra-link layer), the maps' authored links (`Design/maps/*.json` link arrays and `PortExport`-side overlays in a new Unity file, never the web's export), `Game/EnemyViews.Climb.cs` (new: the climb's pose and tell), the block `design.routes`, the nav tests | U3-34.6 (the links, the climb, the perches), U3-34.9 rows tagged N |
| **H: the HUD and the menus' look** | `Ui.cs`, `UiStaminaBar.cs`, `StaminaBarLook.cs`, `StaminaHud.cs`, `UiHotbar.cs`, `UiStyleMeter.cs`, `UiWeapons.cs`, `UiFeelHud.cs`, `UiMenus.cs`, `UiContracts.cs`, `UiPointsPops.cs`, `UiKit.cs` / `UiTheme.cs` additions, `HudReads19.cs`, `UnityTuning.Hud19.cs`, new `UnityTuning.Hud20.cs`, new HUD files for tonight's things (`UiCooldown20.cs`, `UiRound20.cs`, ...), the fonts after the lead bakes them | U3-34.10 (the HUD for every new thing), U3-34.9 rows tagged H |
| **V: the hands and effects** | `Viewmodel*.cs`, `CameraFeel.cs`, `UiFeel.cs` (the speed lines' old screen layer: retired behind `FeelAB` / the new block), `MovementFx*.cs`, new `Game/SpeedLines20.cs` (+ its shader / material under `Assets/Shaders/` or `Assets/ThirdParty/<Repo>/`), `ImpactFx.cs`, `HitSpark.cs`, `Chunks.cs`, `FxSprites.cs`, `GunFx.*.cs`, `UnityTuning.Viewmodel19.cs`, new `UnityTuning.SpeedLines.cs` and `UnityTuning.Fx20.cs`, the travel blur's renderer feature and shader (`Assets/Scripts/Game/Render/`, `Assets/Shaders/`; the lead adds it to `PC_RPAsset`'s renderer in the Editor), vendored open-source effects with their licences | U3-34.7 (speed lines), U3-34.3's look (the shockwave on the THUNDERCLAP, every gun's zoom pose), U3-34.9 rows tagged V |
| **The lead** | the shared commit; `Core/N19Blocks.cs`, new `Core/N20Blocks.cs`, `Core/N19Events.cs` (+ new event payloads), THE TAPE, the fingerprint files; the judges (`Game/Bot/Sandbox*.cs`, new `SandboxClimb.cs`, `Tools/SimHarness/Harness.Sandbox.cs`, `Tools/sandbox-cells.json`); FeelReel and `Tools/feel-metrics.py`; `PlayerChecks.cs`; `SurvivalGame*.cs` and `SurvivalSim*.cs` loop lines (not the builders' partials); `Mover.cs` (only `Impulse` uses); the packages; the renderer asset (`PC_RPAsset` / its renderer data: the travel blur's feature); every doc; the Editor, git, the reviewers, the builds | U3-34.1, .2, .11, .12; and the cut order alone if the builders die |

**The queues** (each builder takes its rows in this order; a row is done at its done-when number, then the next row at
once; the fix lists from each round jump to the front):

| Builder | 1 (GG's fixes, round 1) | 2 | 3 | 4 | 5 | then |
| --- | --- | --- | --- | --- | --- | --- |
| G | the shockwave on the THUNDERCLAP + zoom on most guns (U3-34.3) | merge W's rescued branch: ports, Sandbox19, Freshness19, AltUse wired (U3-34.8) | the balance pass: S1-S7 (U3-34.5 / .8) | B2 combos on screen, D1 trap combos | B3 the FORGE in tiers | B5 TORCH / VORTEX; beyond |
| P | RB scrolls, layout v3 + migration (U3-34.4) | P0-2 the bot tells the truth | P0-3 the pacing trace | E1 the first fifteen minutes v2 | E2 ASSIST's setting; the first-pad hint card | CONTROLS glyphs; the Steam Deck checks |
| E | regen at the round's end (U3-34.5) | merge E's rescued branch: the DRONE; then SHIELDED (U3-34.8) | C2 the LANCER (the perch answer) | A2 THE HUNT, A4 THE ELITE | C3 the affixes | A1 BULWARK (two blocks); A5 twists; C4 WARDEN |
| N | long drops + the leap fixed + no dead perches (U3-34.6 steps 1, 2, 5) | climb links with their tell and view (step 3) | authored links on the tall perches (step 4) | THE CLIMB's R1-R3 on both maps | swarmers scale walls (beyond) | the climber proposal for GG (a page, nothing built) |
| H | the shockwave's cooldown ring, the zoom reticle and scope, the segment ticks and the round-end refill (U3-34.10) | F1 the announcer slabs | A8 the NEXT line; C1 the debut cards | the boss bar, the elite plate, F3 the reward reveal | UIEffect / LitMotion passes (after the packages) | E3 the CODEX; the banned list's next pass |
| V | AIR streaks + retire the border layers (U3-34.7 steps 1-2) | the shockwave's look on the THUNDERCLAP; the zoom poses and the scope (U3-34.3) | the travel blur (step 3) | F2 SKYGRIND TIME's look | A2's dusk shift; the LANCER's beam; the BULWARK's ram line | MovementFx textured; the climb's dust; beyond |

**The rescued NIGHT19 work** (committed by the NIGHT19 lead on the old branches, unverified): W's
`worktree-agent-aab6b3cfb8a45d18d` at `9687752` (ZAPPER / BOUNCER Core ports, Freshness19, Sandbox19: the THUNDERCLAP
falloff / compress / parry, the KESTREL pierce, combos; the forge sidegrade; AltUse half-written; its config turns the
gust off) and E's `worktree-agent-a58e9c83cb2f99f09` at `ad1e828` (the DRONE, its orbs, its view). G and E start by
merging them into their fresh worktrees (`git merge <branch>`), building them, running their tests (ArsenalN19Tests,
DroneTests: 7 red on `u3-n19`), fixing what fails, and committing; the lead cherry-picks only what G / E commit.

**The shared commit** (0:15-0:35, the lead), placed so no builder edits another's file:
- `N20Blocks` + the blocks (every one `enabled: false`), THE TAPE format 5, `--off20` / `-skygrindOff20`, the copy test;
- the three tests NIGHT19's close left red, fixed for the blocks being on: `TapeScript.Fingerprint` sets
  `n19Override = Off` as `RunFingerprint` does (TapeTests.cs:34-40); StaminaTapeTests' format-2 test records with every
  block off (:110-119); its kill test emits the kill at the player's position (:51-64: the range reward);
- the hooks: a `ShockwaveEvent` (or the GustEvent renamed in `N19Events.cs`), a `ClimbEvent` (start, top, the husk id,
  the face), a `RoundHealEvent`, a `SpecialRoundEvent`; `FeelFx.Climb`, `FeelFx.Shockwave`; `SurvivalGame` view hooks for
  N's climb view and E's new kinds; `SurvivalSim` hooks `N20RoutesStartRun` / `N20RoutesStep` (N) and
  `N20DirectorStartRun` / `N20DirectorStep` (E);
- empty fixtures: `ShockwaveTests`, `ZoomTests` (G); `PadRbCycleTests` (P); `RegenN20Tests`, `LancerTests`,
  `SpecialRoundTests` (E); `RouteLinksTests`, `ClimbTests` (N); `SpeedLines20Tests` (V); `Hud20Tests` (H);
  `SandboxClimbTests` (the lead).

## 3. The items (U3-34.1 .. U3-34.12)

**Where an item is decided:** on the final build, by its rows in that build's `SANDBOX.md`, `CLIMB.md`, `MARATHON.md` and
`FEEL-METRICS.md`, and by the final reviewers (`Builds/n19-final` vs tonight, the sides swapped for the second pair).
The rounds show progress; the final decides.

### U3-34.1 Start (lead, 0:00-0:15)

- `date`; GG's notes (`GG-NOTES.md` or the start message: they win, logged under RECEIVED); HANDOFF's NIGHT19 block and
  section 1 row by row; `set_autotick`; the machine (power plan, AC, CPU: other projects' python workers pinned it all of
  NIGHT19; free disk >= 20 GB).
- Copy `Builds/Stage1/` (NIGHT19's final build, `8a33313`) to `Builds/n19-final/`; start its reel and pad pass in the
  background (`-skygrindReel <abs>\Logs\feel\n19-final\on`, then `pad` with `-skygrindReelPad`).
- Check the agent budget once: launch nothing big; if the first builder launch at 0:35 fails on a limit, rule 5 of "How
  tonight works" applies from then on (logged).
- **Done when:** RECEIVED is logged, tasks.json parses with U3-34.*, `Builds/n19-final/` exists and its reels started.

### U3-34.2 The shared commit, the new judges and the baseline (lead, 0:15-1:20)

- **Done when:**
  - the shared commit (section 2) is in with its gates printed: golden 0 / 22 exact, the fingerprint EQUAL and
    `--off19` EQUAL, `impact PASS`, the suite green with the three NIGHT19 reds fixed (7 DroneTests stay red until E's
    merge: logged);
  - `Tools/sandbox-cells.json`'s `damageWeight` 0.05 -> 0.1 (logged with the reason, the one judge change of the night);
  - THE CLIMB and THE MARATHON committed with `SandboxClimbTests` green (two runs identical; one perch hand-checked by
    driving the real Mover onto it and timing a husk's arrival in SimHarness);
  - the baseline on `--off20` (NIGHT19's game) in `Logs/sandbox/b0n20/`: SANDBOX.md S1-S11, CLIMB.md R1-R3, MARATHON.md
    M-1..M-4 / R1-R3, every row valued or n/a with its reason;
  - the reel's additions (section 1), the `vitals` fix, the S14 rule fix and M8 v2 in, and build 1's reels wrote the
    baselines;
  - the bench baseline ran (2 runs, nothing else running, the plan logged);
  - the builders were launched by 0:35, with a `date` output at the launch.

### U3-34.3 The shockwave on the THUNDERCLAP; zoom on most guns (G; V's look; H's HUD)

- **Today (the code map):** NIGHT19's gust lives on the KESTREL (`Core/Guns/Gust.cs:27-44`, `Combat.cs:647`), its cooldown
  is **0.5 s** with one air charge per airtime (`Gust.cs:69-76`), and it turns the KESTREL's zoom off (`Combat.cs:973`:
  `zoomable = ... && !n19.gust`); the THUNDERCLAP has no alt in main (`Combat.cs:13`: "not ported: compress special";
  W's rescued `Sandbox19.cs:193-246` has the compress / double blast). Only the KESTREL ever zooms (`ZoomWeapon`,
  `Combat.cs:500-504`, reads `ads.weapons.<gun>`: revolver 1.5, rail 3.5, rattler 1.4, zapper 1.3, prism 1.4 in
  `PortExport/config.json:2093-2140`).
- **The shockwave** (`design.shockwave`; the switch's numbers from `Research/2026-10-06-balance-zoom-regen.md` 7.1):
  LT on the THUNDERCLAP. One press = the web's double blast (16 x 8 pellets, 10 deg; hits pushed 16 m/s for 0.3 s: the
  web's `weapons.specials.shotgun`, its 1.4 s cooldown overridden, never edited) + the shock cone (30 deg, 8 m, push 12,
  stagger 1 s on light kinds) + the self push 12 m/s opposite the aim (`Mover.Impulse`; none in a dash). **Cooldown
  18 s, one charge, ticking while holstered, no air charge**; LT while not ready fires nothing. A FORGE sidegrade grants a
  second charge (each 18 s); nothing ever lowers the cooldown below 12 s. Events: the web's `doubleBlast` and
  `coneWave`, plus `shockwave` and `shockwaveReady` for GG's sounds.
- **Zoom on most guns** (`design.zoom`, the research's table 7.2): KESTREL 1.5 (the web's; its hip spread back to the web's
  0.8 deg), STITCHER 1.25 (new; its detonate moves to SPECIAL: keyboard B / pad Y), LONGWAVE a 3.5 scope (the web's;
  walk x0.6 zoomed; hold past a full charge = the overcharge), the ports ZAPPER 1.3 / RATTLER 1.4 / PRISM 1.4 (the
  web's) when they land; **unique LTs only on the THUNDERCLAP (the shockwave) and the COMET (steer)**, later the VORTEX
  and the TORCH. Zoom ends the sprint; a dash or a swing suspends it and it resumes while LT is held; only the 3.5 scope
  descopes on a hit. Every `adsActive` reader re-checked (`Combat.cs:707`, `:979`, `:1011`, `:1193`; AimAssist
  `SurvivalSim.cs:166`, the viewmodel `Viewmodel.cs:483`, `:881-882`, `:1027`). The leftovers: `UiHotbar.cs:671-673`'s
  "ZOOM x1.5" and `UnityTuning.Menus17.cs:351`'s "ZOOM" label follow the real binding.
- **V:** the shockwave's look moves to the THUNDERCLAP (V's gust air, rings and wake re-aimed; the kick on the shotgun's
  pose); every zoom gun's ADS pose and its scope overlay for the 3.5 (a real scope ring, not a flat image).
- **H:** the cooldown ring around the LT glyph on the THUNDERCLAP's card (visible while holstered), the seconds left
  while above 3 s, a 0.2 s cyan flash on ready; the zoom's reticle.
- **Done when:** `ShockwaveTests` (two presses 17.9 s apart: one; 18.0 s: two; holstered ticking; none in a dash; the
  FORGE's second charge) and `ZoomTests` (each gun's LT: zoom or its unique alt; the zoom's FOV by the web's optical
  formula: 1.25 -> 62.2 deg, 1.5 -> 53.3, 3.5 -> 24.3 at base 74) green; THE RANGE's gust probes on the THUNDERCLAP (C3:
  >= 3 of 8 pushed back >= 3 m) and THE CHASE's KITE with the shockwave on vs off <= +25 % (S10); the farm's shockwaves
  per round 1.5-3; the reel's `shockwave` and `zoom` scenarios with both reviewers answering yes to "does this read as a
  burst of air that moves the player?" (round 4 or the final).
- **Beyond:** D1 trap combos (push into a live trap: "SENT"); the THUNDERCLAP's point-blank parry (Sandbox19) on a
  wind-up as a heal + stamina refill.

### U3-34.4 The pad: RB scrolls weapons (P)

- **What:** GG: "the right bumper could be used to switch weapons as well, just scroll through weapons". In
  `DefaultLayoutV2` (`Gamepad.cs:277-295`) RB carries SPECIAL + GRAPPLE and Y is NEXT WEAPON. The v3 default
  (`design.pad.n20`): **RB = NEXT WEAPON** (a tap scrolls; held `design.pad.wheelHold` 0.25 s it opens the wheel, as Y
  does today: `PadReader.cs:216-217`, `:357-383`), **Y = SPECIAL + ARCLINE** (v2's two rows swapped: the zoom guns'
  specials, such as the STITCHER's detonate, live on SPECIAL; the grapple must stay bound: `Gamepad.cs:227,293`'s
  required set; the rope is off in Survival anyway, `SurvivalSim.cs:161`). The PAD switch's other choice: Y = PREVIOUS
  WEAPON (CoD's / Halo's swap key) with SPECIAL + ARCLINE on d-pad left. A saved v2 equal to the v2 default becomes v3;
  a customised v2 keeps its buttons unless it never touched RB and Y (the v1 -> v2 rule, `MigrateV1`).
- **Also:** the hotbar's cycle line and the WHEEL prompt read the new binding through `PromptText` (S12 holds); CONTROLS
  shows the change; the Player smoke's pad steps still pass (read `SliceSmoke.Night15Pad.cs` and every pad step first).
- **Done when:** `PadRbCycleTests` (RB tap -> next, hold -> wheel, Y -> special, the v2 -> v3 migration) green; the
  reel's `rbcycle` (pad pass) shows the hotbar's slot change on each of 3 presses within 2 frames; S12 0 on the pad pass;
  the final smoke's pad steps pass.
- **Beyond:** a HOLD / TOGGLE row for L3 in CONTROLS; the first-pad hint card (L3 sprint, R3 melee, RB scroll, LT zoom);
  CONTROLS draws glyphs, not words; the Steam Deck checks (`Research/2026-10-04-controller.md` checklist rows 1-3).

### U3-34.5 Regen at the round's end, and the balance pass (E: regen; G: the guns; P: the bot)

- **Today:** `Regen` (`Core/Health.cs:195-288`, config `regen` at `PortExport/config.json:4787-4796`): **4 s delay (2 s
  while moving above 8 m/s or after dealing damage), then 25 HP/s to full**; nothing heals at `roundClear`
  (`SurvivalRun.cs:322`). Perks: MEND (`Perks.cs:53`); HEAT's NO REGEN (`Heat.cs:73`).
- **The model** (`design.regen.n20`, the REGEN switch; `Research/2026-10-06-balance-zoom-regen.md` 7.3):
  1. **Round end:** at `roundClear`, refill to full over 2 s inside the break (the web's `heal` event; the HP bar fills
     under the round banner: H).
  2. **In the round, segments only:** 25 HP segments (4 at 100 HP); passive regen climbs back only to the top of the
     current segment, 5 s after the last hit, at 10 HP/s, no speed-up. Tick marks on the vitals plate (H).
  3. **Earned heals** (they ignore the segments): +4 HP per kill within 6 m, +10 per punch kill, the parry's +15 and the
     perfect parry's full heal (unchanged), BLOOD PRICE's +3 / kill / stack (unchanged).
  4. MEND scales the segment regen (3 s, 15 HP/s); the style rank's rate bonus stays; NO REGEN = no segments, no refill.
- **The balance pass** (the research's 7.5 procedure, in this order): THE RANGE's `damageWeight` 0.05 -> 0.1 (the lead,
  once, at the start, before the baseline: HP is now a round-long resource); G writes the intent table (each gun's home
  and weak cells) in the log before touching a number; the curve per cell is the median gun's score; one knob per gun per
  iteration in big steps, the outlier first (COMET), then the losers (THUNDERCLAP, LONGWAVE); RANGE every iteration, THE
  CHASE and the parity farm every third, the free farm once RANGE passes. A ledger row per knob in the log: "knob,
  from -> to, cells moved, verdict".
- **Done when:** `RegenN20Tests` green (the refill at `roundClear`, the segment cap, the earned heals, NO REGEN); THE
  MARATHON (after P0-2's bot fixes): M-1 the median round within 0.8-1.3 x NIGHT19's 9.0, M-2 no round band with more than
  50 % of the deaths, M-3 HP at a round's start >= 90 % (median), M-4 the in-round heal share 0.15-0.4, R1 HP at round
  clear 35-70 % median (rounds 5-15), R2 the refill >= 50 % of all HP restored, R3 <= 25 % of deaths in a round's first
  quarter; THE CHASE: S8 KITE <= 120 s at round 10 and <= 60 s at round 15, FIGHT at round 10 >= 120 s, S9 holds; G's
  S1-S3 rows (U3-34.8).
- **Beyond:** E2 ASSIST (Hades' God Mode) for players who'd quit; a hard-damage segment (ULTRAKILL) as GG's A / B.

### U3-34.6 Enemies reach every floor (N, with E's height answers)

- **Today (Research/2026-10-06-enemy-routes.md section 1, measured from the exported grids):** the grid's links are walk
  (0.6 m step), jump (<= 2.2 m), drop (<= 5 m) and gap (`NavGridBuilder.cs:11-15`); on the Survival map **0 of 3,227
  nodes above 5 m are reachable from the ground and none can get back down**; on the four 24 m spire tops and the 29.2 m
  pinnacle (576 "dead" nodes) no spawn node has a route to you, so `Director.PickNode` (`Director.cs:603-605`) spawns
  nothing and husks go `lost`; the husk's free leap (`Husk.cs:242-251`, >= 1.5 m above and <= 10 m across, every 3 s)
  has no line-of-sight or headroom check and launches from the wall's foot, where the field's seeds park it
  (`NavField.cs:555-586`). DRYDOCK: the slipway ramps reach 42 % of the 8-12 m band; the hull deck (560 nodes at 11 m),
  the rib tops and the crow's nest are unreached and can't be left. The director's `high` spawn list is filled and never
  used (`Director.cs:596-626`).
- **The fix, in order (each behind `design.routes`, as a Unity-side extra-link layer, `Core/Nav/RouteLinks.cs`, so the
  web's grid export and the `flowField` golden vectors stay untouched):**
  1. **LONG DROP links** for every walker, up to 12 m, a 0.35 s landing stagger (L4D's DeathDrop is ~10 m): "can't get out
     of floors" (the what-if: ARENA's can-get-down 71 % -> 91 % at 12 m).
  2. **The leap fixed:** an approach point (a reached node 3-6 m back from the wall's foot, line of sight to the landing,
     headroom over the arc), a DANGER crouch tell, and the arc checked against the collider before it launches; no leap
     into a ceiling.
  3. **CLIMB links** (husks and spitters 2.2-5 m: a 0.3 s DANGER tell, 3 m/s, a 0.25 s mantle, knocked off by a punch;
     swarmers 2.2-10 m at 6 m/s; chargers never climb; L4D's ClimbUpHeight ~5.1 m and Vermintide's 5.1 m ledge links are
     the evidence; `ClimbEvent` for GG's sounds and H's off-screen arrow), as new link kinds 4-7 with per-kind masks and a
     cost of `1 + speed x time` (a 5 m climb 14.3, a 10 m drop 8.7), generated per map by casting the
     collider (faces, lips, headroom) and validated by flying a small mover through each link (the web's navGraph build
     by simulation, `Core/Nav/NavGraph.cs`).
  4. **AUTHORED links** in the map files for perches no generic link reaches (Survival's spires and pinnacle, the ARMORY
     roof at 8 m, DRYDOCK's hull deck and ribs): ladders / stair chains of <= 6 m climbs, with their tells. The crow's
     nest and the spire tops may stay perches, answered by E's DRONE and LANCER (web kinds; the LANCER "prioritizes
     campers": `lancer.js`).
  5. **No dead perches, no stuck husks:** the field seeds the nearest reached node under or beside you, so the director
     always spawns (a test on the 24 m spire); a camp score that notices height (3 s to start, not 8, on an unreachable
     perch) and lets the hunter squad spawn there; a husk that can't reach you for 8 s on screen is recycled out of
     sight with its HP kept (CoD BO3 and KF2 recycle stuck enemies; the L4D Tank's 20 s frustration). E owns the camp
     score and the director's side: N sends E the exact lines.
- **Done when:** THE CLIMB on both maps: R1 0 unreachable player nodes (perches answered only by flyers / snipers are
  listed and count as reachable when a ported kind can hit them there, with the kind named); R2 every standable node reached
  within 12 s and within 6 s more than the floor below it; R2b every node can get down within 6 s; R3 on each of the 10
  worst perches, the STAND bot's damage per minute >= 50 % of the ground's and its survival <= 2 x the ground's; `RouteLinksTests`, `ClimbTests` green; the
  reel's `climb` shows the tell >= 0.5 s before the climber's top; golden and replays unchanged (the extra layer is off
  in their config).
- **Beyond:** swarmers scale walls (DRG's Glyphids) as their own verb; a husk pile (World War Z) as a proposal for GG only
  (rule 7); the director spawns from the `high` list when you're up high (E's file: ask E).

### U3-34.7 Speed you can see, ported from open source (V)

- **Today (`Research/2026-10-06-speed-lines.md` section 0):** two screen Images ("Speed lines", "Speed lines boost",
  `UiFeel.cs:62-69`) faded by speed share with a mask out of the HUD's corners (`CameraFeel.cs:625-650`,
  `UnityTuning.CameraMove.cs:66-105`), the dash's edge tint (`MovementFx.RenderTint`) and the sprint's edge-band lines
  (`MovementFx.SpawnSprintLines`): GG's "weird border that only appears".
- **The design ("AIR", the research's section 3; every number a starting value in a new `UnityTuning.SpeedLines.cs`):**
  1. **Retire** (behind the speed-lines block, so a blind A / B compares): the two screen layers, the dash edge tint, the
     sprint's edge-band lines. **Keep:** the wall-ride streaks, the slide lines and sparks, the shockwave's air, the FOV
     numbers, the chromatic aberration's speed share.
  2. **AIR streaks, world space** (the main cue): a pool of 160 thin, additive, tapered quads on `FxLines`' path, at most
     64 drawn. Each keeps a **world-fixed head** and recomputes its tail every frame as `head + v x T` (KvantStream's
     rule: length = travel over an exposure, `T` 0.040-0.065 s, 0.15-2.5 m), so the lines point away from the true
     focus of expansion (a strafe draws sideways, a fall upward) and shrink to dots when you stop. The speed signal
     `v_eff = sqrt(vx^2 + vz^2 + (0.6 vy)^2)`; gate 9 m/s (a walk never shows), full at 24 (the boost); intensity
     `clamp((v - 9) / 15, 0, 1)^0.6` (sprint 0.25, slide 0.35, boost 1.0); spawn `120 x k` per s on a screen ring
     rho 0.40-1.10 around the crosshair at depth 3-14 m, rejected if the camera's ray to it hits the world (the tunnel
     layout of MikeFrom/Godot-Particles-Tunnel-Speed-Lines, CC0, rewritten in C#); life 0.38 s +- 25 %; nothing inside a
     rho 0.33 ellipse, full from 0.55; cyan-pale (the player's), HDR <= 1; a 32-streak burst on a boost's onset, 20 on a
     boost-jump, 18 up on the shockwave; rejected within 40 px of a live enemy, depth-tested, x0.3 while zoomed; 0 with
     REDUCED MOTION; a cosmetic Mulberry32 (never a sim stream).
  3. **The travel blur, screen space, boost-weighted:** Bauxitedev/godot-motion-blur's reprojection (MIT) with **linear
     velocity only** (URP's own Motion Blur also blurs the camera's rotation: every flick would smear the aim), as one
     6-tap full-screen pass after post on the main camera (the pass from daniel-ilett/blur-pro-urp's `RadialBlur.cs`
     render-graph path, MIT, or Cyanilux/URP_BlitRenderFeature, MIT, tested on 6000.3.6f1, or URP's Full Screen Pass
     feature); shutter 0.010 s, mask `smoothstep(0.25, 0.60, rho)`, clamp 1.8 % of the width; strength 0 under 14 m/s
     (no blur on a sprint or a slide), 1 at the boost; a MOTION BLUR setting (on by default, off with REDUCED MOTION). The
     viewmodel's overlay camera and the HUD canvas draw after it, untouched.
  4. **The optional accent** (MirzaBeig/Anime-Speed-Lines, Unlicense: polar noise recentred on the focus of expansion, a
     0.2 s boost-onset overlay, alpha <= 0.35) **stays off** unless the blind A / B says the boost lacks punch: it is the
     kind of overlay GG called forced.
  5. Vendored code keeps its licence beside it and a THIRD-PARTY.md row; KvantStream has its MIT text in the README only:
     re-implement its ~10-line rule, don't vendor its files.
- **Probes:** `FeelFx.SpeedLines` (the drawn streaks' bounding rect, max alpha, count) and `SpeedLinesClear` every frame;
  the lead adds a streak-direction channel to the telemetry (the median screen angle of the drawn streaks) for M8 v2.
- **Done when (M8 v2, measured on the final reel; the research's 3.6):** L1 0 frames with a streak pixel inside rho 0.33,
  alpha < 0.30 out to 0.55; L2 0 streaks alive at a walk, 11 +- 4 while sprinting, >= 35 within 50 ms of a boost's onset,
  back to the sprint's count within 0.3 s of its end; L3 the median screen length 25-45 px sprinting, 80-140 px boosting
  (1080p); L4 a pure strafe's streaks within 20 deg of horizontal, a slam's within 20 deg of vertical; L5 0 frames with a
  streak crossing a live enemy's screen rect; L6 the bench p99 + 0.3 ms at most; T6's blind judgement (>= 3 of 4 final
  judgements pick tonight on "which shows speed better while keeping the middle readable"); `SpeedLines20Tests` (the
  intensity curve, the clear ellipse, the tail by velocity, REDUCED MOTION) green; S14 0 on sprint and boost.
- **Beyond:** the wind-trail wiggle for variety (dip000/my-godotshaders `WindTrailWithLoops`, MIT) on long slides; the
  same streak pool on the shockwave's launch and the BULWARK's ram (V's effects for E's kinds).

### U3-34.8 NIGHT19's unfinished guns and enemies (G, E)

- **G** merges W's rescued branch (`9687752`): the ZAPPER and BOUNCER Core ports (V's viewmodels, arcs and grenades are
  already in), Freshness19 (points only), Sandbox19 (the THUNDERCLAP's falloff, compress / double blast and point-blank
  parry; the KESTREL's pierce; the combos' first pass), the forge sidegrade, AltUse (half-written: finish it and wire it
  into `AutopilotGuns` and `SandboxRange.AltUse`). Its config turned the gust off: tonight the shockwave block replaces
  the gust.
- **E** merges E's rescued branch (`ad1e828`): the DRONE (Drone.cs, its pooled orbs, the parry reflect, its view); then
  the SHIELDED affix; the 7 DroneTests red on `u3-n19` turn green.
- **Done when (G):** S1 0 dominated pairs; S2 each reachable gun near-best in >= 2 cells and none best in > 35 %; S3
  every gun within 1.6 x the best in C7; S4 >= 5 combos each >= 20 % over the best single gun (`design.arsenal.combos`,
  THE RANGE's pair mode); S5 every gun's parity-farm median within +/- 20 %; S6 top kill share <= 30 %, >= 5 guns >= 5 %;
  S7 1.3-2.0; ArsenalN19Tests and FreshnessN19Tests green; the ports fire with the web's numbers.
  **(E):** DroneTests 7 / 7 green; C9 (drones) and C10 (shielded) valued in THE RANGE with a best gun that isn't the
  THUNDERCLAP and every gun clearing them; the reel shows each with its tells.
- **Beyond:** B3 THE FORGE in tiers (5k / 15k / 30k as sidegrades: Research/2026-10-06-additions-backlog.md row 11), B5
  TORCH and VORTEX (row 25), B4 a wonder weapon (row 19; its name and look are GG's: propose, don't ship it on).

### U3-34.9 The additions wave (every builder; `Research/2026-10-06-additions-backlog.md`)

GG: "I want so many additions, well beyond the things I'm asking." The backlog's rows are ranked by value / cost from
shipped games (CoD Zombies, L4D's director, KF2, DRG, RoR2, Hades, ULTRAKILL, DOOM Eternal) with what's proven to fail
(forced solutions, damage sponges, system pile-up, extra currencies). Each row below is a vertical slice: the rule, the
view, the HUD, the event, the test and its judge, behind one sub-block of `design.adds` (or the owner's block), with
the backlog's done-when. Take them in each builder's order after its GG fixes and its NIGHT19 work; keep / iterate / cut
each at the next build (the cut rule: two missed iterations or 1.5 x its time box).

| Row | What (the backlog's row) | Owner (+ helpers) | Cost |
| --- | --- | --- | --- |
| P0-2 | The bot tells the truth: HOLD by the game's own ring test, shopping by route time, exfil called (the farm's round-6 cliff: 89 % -> 50 %, 0 exfils) | P (owns `Game/Bot/AutopilotInput.cs`, `AutopilotShopper.cs`, `AutopilotTarget.cs`, `Farm.cs`, `Tools/farm-report.cjs` tonight) | M |
| P0-3 | The pacing trace: the farm writes seconds per round, the director's BUILD / PEAK / FADE / RELAX seconds, downs by round type | P | S |
| F1 | The announcer: one slab per run beat (BOSS INBOUND, THE HUNT, ELITE INBOUND, LAST ONE, EXFIL OPEN, ... <= 3 words, a priority queue, at most one per 1.5 s) | H | S-M |
| F4 | The tell floor: every enemy hit >= 15 has a DANGER tell >= 0.4 s or a projectile on screen >= 0.25 s; exceptions listed for GG | the lead (a telemetry check) + E (the tells) | S |
| C1 | Kind debut cards (once per profile: the name and one counter, <= 4 words, the world slowed <= 1.5 s, never with an enemy within 8 m) | H (+ E's kinds) | S |
| C2 | LANCER (port `lancer.js`: a lagging aim beam from a perch, 25 damage, blinks between perches; from round 8, at most 6): punishes kiting and perching | E (+ N's perch points, V's beam) | M |
| A2 | THE HUNT: the swarm round as CoD's dog-round ritual (rounds 8, 13, 18: a count, the dusk shifts toward DANGER, spawns in a ring, LAST ONE, the drop) | E (+ V's sky shift, H's slab) | S-M |
| A4 | THE ELITE: a named CHARGER / WARDEN with two affixes mid-round from round 8 every 3-5 rounds, a guaranteed drop | E (+ H's name plate) | S |
| C3 | The affixes (port `affixes.js`: VOLATILE, BLINKER, REGENERATING, SWIFT, + SHIELDED) | E | M |
| A8 | The NEXT line in the break (the next rounds' types, <= 3 words, five pips) | H | S |
| F2 | SKYGRIND TIME (KF2's Zed Time: on a 3+ kill blast or a headshot kill at BOLD+, ~25 % speed for ~1.5 s, 0.3-1.0 per combat minute; `SimHarness impact` PASS; SLOW-MO OFF disables it) | V (+ the lead's loop line in `SurvivalGame.Impact.cs`) | S-M |
| F3 | The reward reveal (boss / elite / hunt rewards: a 0.8 s three-slot reveal, pick 1 of 3) | H | S |
| D1 | Trap combos (push an enemy into a live trap with the shockwave, a COMET launch or a punch: "SENT", credited to you) | G | S |
| B3 | THE FORGE in tiers as sidegrades (5k / 15k / 30k) | G | M |
| E1 | The first fifteen minutes v2 (each verb prompted at its moment of need, gone on use; the first unlock within 5 minutes) | P (+ H) | M |
| E2 | ASSIST (Hades' God Mode: resistance growing per down, runs stamped) | P (setting) + E (the rule) | S |
| A1 | BULWARK on round 5 (port `bulwark.js` + `bossKit.js` + `bossMath.js` as they are; a new `SimHarness boss` judge: a KESTREL-only median kill 60-150 s, win >= 80 %) | E (2 blocks) + H (the boss bar) + V (the ram line, the stomp rings) | L |
| A5 | Round twists from round 9 (FOG, SWARMAGEDDON, LETHAL, GOLDEN, from systems that exist) | E | M |
| C4 | WARDEN (port `warden.js`) | E | M |
| B5 / B4 | TORCH and VORTEX; a wonder weapon (propose its name and look to GG) | G | M / L |

- **Done when:** at least 12 rows landed (each with its backlog done-when number met on a Player build, its event names
  the web's or new port-only names listed for GG's sounds, its flag off = tonight's game), and every row taken has a
  commit, a number and a two-line review in the log; rows cut are logged with the reason.
- **The rule-7 line:** every enemy above is the web's design, ported as it is (CLAUDE.md rule 7). A new kind (a climber,
  a husk pile) is a one-page proposal for GG, never built.

### U3-34.10 Not cheap: the HUD, the menus and the effects for every new thing (H, V)

- Every new thing tonight gets its HUD and its look in the same round it lands: the shockwave's cooldown ring, the zoom
  reticles and the scope, the segment ticks and the round-end refill, the climb's off-screen arrow (a DANGER chevron at
  the screen's edge toward a climbing enemy above or below you), the announcer slabs (F1), the NEXT line (A8), the debut
  cards (C1), the boss bar and the elite's plate, the reward reveal (F3).
- NIGHT19's open leftovers: the speed-lines layer that S14 flagged (V retires it), `UiHotbar.cs:671-673`'s "ZOOM x1.5",
  the alt-meter reads (`HudReads19.cs:45` has no implementation: G writes `HudReads19.Arsenal.cs`), the Share Tech Mono
  numerals (the lead bakes `Assets/Resources/Fonts/MonoNumerals SDF.asset`: OFL reserves "Share Tech"), the TITLE
  pop-ins (P's `UiMeta.cs`).
- The packages (the lead, 0:35-1:20, each its own commit, pinned): UIEffect (MIT) and LitMotion (MIT); H uses them on the
  plates' edge glow and a shine on a rank-up, never on everything (banned 3), and per-character reveals on the slabs.
- **Done when:** S14 0 persistent HUD in the central 40 % on hud-fight, sprint, boost, vitals, zoom; M9 / M10 / M11 at 0
  on every scenario NIGHT19 passed; S15 tonight's side picked in >= 70 % of the final reviewers' judgements over the HUD,
  menu, speed and weapon questions, no "too much", 0 banned patterns ticked on tonight's side by both reviewers; S16 p99
  <= baseline + 1.0 ms (benched alone); every import has its THIRD-PARTY.md row and licence.
- **Beyond:** E3 the CODEX tab; the RESULTS and title passes with UIEffect; the banned list's next pass from the
  reviewers' words.

### U3-34.11 The loop: rounds, beyond, the backlog (lead)

- **Five rounds** on builds 2-6 (~1:50, 3:05, 4:20, 5:35, 6:50). Each round: the build; the smoke; the reels (normal +
  pad); `feel-metrics.py` against `n19-final` and the previous round; the judges (`range`, `chase`, `climb`, `sbreport`)
  on the build's tree; the farm (rounds 2 and 4: THE MARATHON + THE PARITY FARM, within the farm cap); two blind reviewers
  on rounds 2 and 4; the fix lists (a metric, its value, the target, the evidence: never an adjective); keep / iterate /
  cut per slice, logged.
- **Never idle:** while a build, a farm or a reel runs, the lead integrates, runs a judge, builds a row of the cut order
  itself, or writes the log. A builder whose queue is done takes the additions wave's next row it owns, then its beyond
  list.
- **Done when:** five rounds are logged, each with its tables, the reviewers' verdicts (rounds 2 and 4), the fix lists
  and the keep / iterate / cut calls; every beyond or backlog row taken has a commit, a number and a two-line review.

### U3-34.12 Close (lead, from 7:20; table at 8:00 or later; hard stop 8:30)

- **The code freezes at 7:20, and the final build starts at once** (allow 15 minutes).
- **On the final build, in this order** (nothing else running during the smoke and the benches):
  - the smoke, alone; `-skygrindGolden`; `-skygrindFingerprint` (both lines); `-skygrindReplay` on the smoke's tape;
  - the bench (2 runs), **then** the suite on the frozen code, async in the Editor;
  - the final reels (normal and pad); the final judges (`range`, `chase`, `climb`) and the final farm (THE MARATHON and
    THE PARITY FARM: one batch);
  - the final reviewers (two pairs, sides swapped).
- **Record** `FingerprintBaselineN20` (every block as configured) once, with the reason.
- **What the transcript must show, so the /goal checker can see it:** the decoded blind picks (a table per question:
  reviewer, pick, side); the final `SANDBOX.md`, `CLIMB.md`, `MARATHON.md` and `FEEL-METRICS.md` rows; the gate lines; a
  `date` output before the table.
- **The docs** are drafted by 7:20, so the close only fills in numbers: `NIGHT20-LOG.md`; `NIGHT20-REPORT.md` (GG's one
  page: what changed per GG ask, the numbers before / after, the blind verdicts, before / after GIFs for `sprint`,
  `shockwave`, `zoom`, `climb`, the questions); `FEEL-PLAYTEST.md` rewritten as a 10-minute pad-first walk of tonight's
  changes; `THIRD-PARTY.md`; PORT-STATUS (the event list: every new event name for GG's sounds); HANDOFF (the `handoff`
  skill, anchor `u3-n20`); tasks.json.
- **Tag `u3-n20`;** print the close table, then `NIGHT20 CLOSED`.

## 4. The clock (from the first `date`)

| When | Lead | Builders |
| --- | --- | --- |
| 0:00-0:15 | U3-34.1 | none |
| 0:15-0:35 | the shared commit (blocks off, tape 5, the three red tests, hooks, fixtures); launch G, P, E, N, H, V by 0:35 with a `date` | none |
| 0:35-1:20 | THE CLIMB, THE MARATHON, the reel's additions and M8 v2; the baseline (`--off20`: NIGHT19's game) and build 1 at ~1:05 (smoke, reels, bench x2, farm batch 1: the MARATHON and THE PARITY FARM baselines); the packages | GG's six fixes first (G: shockwave + zoom; P: RB; E: regen; N: drops + the leap fix; V: the speed lines' first port; H: the cooldown and the round heal on the HUD) |
| 1:20-7:10 | five rounds on builds 2-6 (~1:50, 3:05, 4:20, 5:35, 6:50): integrate every 45-60 min, the judges after each, reviewers on rounds 2 and 4; build or fix an item yourself while builds and farms run | their queues: the fix lists first, then the next queue row, then beyond, then the additions |
| 7:10-7:20 | the last fixes, checked by the judges and the Editor reel; the docs' drafts | the last fixes, all committed |
| 7:20-8:30 | U3-34.12 (the table at 8:00 or later) | none |

**The cut order if short** (cut from the top): the additions wave's L rows; the beyond lists; THE LANCER; the
speed lines' travel blur (keep the streaks); the fifth round. **Never cut:** GG's six fixes (U3-34.3 .. 34.7's first
rows), the judges and their baseline, the reels' S12 / S14 / M8 v2, the reviewers on round 4 and the final, the gates,
the close.

## 5. The reviewer brief (two per judged round, fresh, read-only), and the banned patterns

> You are judging a game's screens and moves from frames only. You won't see code, metrics or anyone's notes. For each
> scenario you get flipbook sheets for A and B: the same moment rendered by two versions of the game, 8 key frames in
> order, near native resolution. Crop or zoom with PIL if a detail matters.
>
> For each scenario, answer with **A**, **B**, **Same** or **Unknown**, and one line naming the frames that decided it:
> - "Which shows speed better while keeping the middle readable?" (sprint, boost, boostside, slide)
> - "Which weapon moment reads better?" (shockwave, zoom, kill12, swapall)
> - "Which HUD looks more finished, less cheap?" (hud-fight, vitals, rankup)
> - "Which menu looks more finished?" (ui-title, ui-weapons, ui-results)
> - "Which enemy reads better: what it is, what it's about to do?" (climb, charger, drone, special-round)
>
> Then answer "Is either one too much (noisy, hard to read, tiring)?". Then tick each banned pattern below you can see in
> A and in B, naming a frame for each tick. Then score the 10-row rubric for B only, against the anchor images given, one
> line per row. Return a table, under 700 words.

**The banned patterns** (NIGHT19's 14, plus tonight's from GG's words):
1. Flat TMP text on a flat rectangle, with no edge, cut, texture or depth.
2. Default rounded Unity sprites and pill buttons.
3. Soft drop shadows and outer glows on everything; gradient fills as decoration.
4. One ease for every motion, or a rubbery overshoot on everything.
5. Elements that pop in at full size and alpha.
6. Confetti: many small sparks after the main hit.
7. A bloom or blur that hides the thing it celebrates.
8. Effects over text.
9. Persistent HUD in the middle of the screen.
10. Button names written as words where a drawn glyph belongs; keyboard letters on a pad.
11. Colours outside their roles.
12. Centred stacks of same-weight text with no hierarchy.
13. Untextured quads as particles.
14. Grey boxes where a gun or an enemy model belongs.
15. **Speed shown as a frame or border around the screen** (GG: "that weird border that only appears"); lines that don't
    move with the camera's velocity; the same lines at every speed.
16. **Effects that ignore the world:** lines drawn over an enemy's body or the crosshair; streaks with no parallax.
17. **An enemy standing below a player it can't reach** (GG: "you can kind of just camp out").

## 6. The builder brief (common part; each builder also gets its queue)

**A fix-list entry names the metric, the evidence and the target, never an adjective.**

<example type="good">
G, Z1 (THE RANGE r2): the LONGWAVE is best in 0 cells and 8.09 x the COMET in C7. With design.zoom on, its zoom gives
x1.25 headshots and 0.5 x spread, but C5 (the line) still takes 2.78 s against the KESTREL's 1.81. Make the pierce count
from the config (weapons.rail.pierce 3) apply to the zoomed shot with no falloff, then re-run `range --gun rail
--cell C5`. Expect C5 <= 1.10 x the best and S1's "rail by revolver" gone.
</example>

<example type="good">
N, R1 (THE CLIMB r2): 37 standable cells on DRYDOCK's gantry (y 6-9 m) are unreachable to husks; the nearest climbable
face is 2.4 m tall at (12, 0, -8). Add a climb link there (design.routes.climbMax 3.2 m), then re-run `climb --map
drydock`. Expect 0 unreachable cells and the perch's survival <= 45 s.
</example>

<example type="bad">
V: make the speed lines look better.
</example>

The bad one carries no number, no evidence and no way to know when it's done.

- **Objective:** your queue's done-when numbers on the next build's judges or reel. Code that compiles is not done.
- **Before writing a mechanic or an effect:** search Core, the web and the research's GitHub repos; port rather than
  invent; keep the source's licence notice.
- **Rules:** tests first for pure rules (seen red); every new behaviour behind its design block; `FeelProbe` from your
  effects; no allocation per frame; never edit another builder's file (ask the lead with the exact lines); never edit a
  judge or a target; never run `unity`; run the judges yourself in your worktree.
- **Commit every finished slice at once** (a test and its code, or one visual change with its probe). The lead
  cherry-picks what's committed; anything uncommitted when your session ends is lost (NIGHT19 lost two builders' work).
- **When a call fails with a rate or spend limit,** commit what you have (`git add <your files>` + commit) before
  anything else, then stop.
- **Report** (under 400 words): the commits; what a player now sees or does differently and where to aim the reel; which
  metric moved by how much (the judge's line); what you didn't do; the dotnet results.

## 7. The close table (print it, then `NIGHT20 CLOSED`)

| Item | DONE / SHORT | Evidence ("done when", from this session) |
| --- | --- | --- |
| U3-34.1 Start | | |
| U3-34.2 The shared commit, the new judges and the baseline | | |
| U3-34.3 The shockwave on the THUNDERCLAP; zoom on most guns | | |
| U3-34.4 The pad: RB scrolls weapons | | |
| U3-34.5 Regen at the round's end, and the balance pass | | |
| U3-34.6 Enemies reach every floor | | |
| U3-34.7 Speed you can see, ported from open source | | |
| U3-34.8 NIGHT19's unfinished guns and enemies | | |
| U3-34.9 The additions wave | | |
| U3-34.10 Not cheap: HUD, menus, effects for the new things | | |
| U3-34.11 The loop: rounds, beyond, the backlog | | |
| U3-34.12 Close | | |

SHORT needs its measured reason: a cap reached, a decision reserved for GG, a blocker after two approaches, or the time
limit. A SHORT on U3-34.3-34.10 states its metric values and blind-judgement counts.

## 8. The /goal for GG to paste

```text
/goal Run NIGHT20.md in C:\Users\gaige\dev\SKYGRIND-unity. Run `date` first, then read CLAUDE.md, NIGHT20.md and the files it lists, and work U3-34.1 .. U3-34.12 in NIGHT20.md's order with at most six background builders split by file ownership, each with a queue, committing every finished slice, looping rounds (build, judges, reels, reviewers on rounds 2 and 4, fix lists) and taking each item's beyond list and the additions backlog once it is met, until the close. If an agent fails on a rate or spend limit, keep building alone and retry at most every 30 minutes. Never ask me questions and don't stop early; end every message with the N20-STATUS line. Never push, merge or commit to master; the web repo is read only. Run at least 8 hours: don't start the close before 7:20 or print its table before 8:00 from the first `date` (run `date` right before the table); hard cap 8.5 hours. This goal is met when your latest message prints NIGHT20.md's close table with every item U3-34.1 .. U3-34.12 marked DONE (with its "done when" evidence from this session) or SHORT (with its measured reason: a cap reached, a decision reserved for GG, a blocker after two approaches, or the time limit once a `date` output reads 8 hours or more after the first `date` output), followed by the line "NIGHT20 CLOSED". SHORT rows count as met.
```

## GG's standing instruction (verbatim, from NIGHT8; also Anthropic's recommended text for unattended Opus 5.5 runs)

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

Time matters here: do not spend time that can be avoided, and the earlier a correct result is obtained, the better. But
the night's length is fixed: finishing an item early means taking the next one, never closing early.
