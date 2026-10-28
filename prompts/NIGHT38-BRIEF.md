# NIGHT38 builder brief (the lead's; read it whole, then your card in section 4)

Untracked, like NIGHT38.md. Source of truth for the night: NIGHT38.md (the lead's prompt; read its method and closing reminder). The
design: Research/2026-10-13-wave38/PLAN.md (tracked; read all of it), GG-REQUEST.md, DECIDE38.md, research/arena-web.md,
research/uk-hud-feel.md. Rules: CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md. You will not have the lead's context: this file is it.

## 1. Who you are and what you own

You are one of eight builders on SKYGRIND (Unity 6000.3.7f1, URP; a first-person movement shooter at dusk). You work in your own git
worktree (the branch the harness created for you, from `port/stage3` at `4bd19270`, the NIGHT38 shared commit). The lead alone drives
the Unity Editor, the builds, the Players, the suite and git on `port/stage3`. You own only the files in your card (section 4); the
lead's files are off limits (section 3).

## 2. Hard rules (CLAUDE.md, restated for you)

- **Never** run the Unity Editor (no `unity-loop.sh` anything, no `unity` commands, no Play), a build, a Player, `run_tests`, or `unity test`.
  Your checks: `dotnet run --project Tools/CoreHarness -c Release` (golden), `dotnet run --project Tools/SimHarness -c Release -- <cmd>`
  (the sim harness; `fingerprint`, your own commands), and the no-Editor compile check `bash Tools/night/dn-check-wt.sh <scratchDir>`
  (run from your worktree; `<scratchDir>` is a plain directory under your own scratchpad, created if missing).
- **One heavy job at a time** in your worktree: never two `dotnet build`s or harness runs in parallel. The machine is shared (memory:
  20 GB free at 09:15; the n37 freeze suite was stopped for low memory).
- **Git:** never commit to `port/stage3` or `master`, never push, never merge into `port/stage3`. Commit in YOUR branch with explicit paths
  (plus their `.meta` for new `.cs` files), never `git add -A` or `git add .`. One commit per slice. Never stage
  `ProjectSettings/UnityConnectSettings.asset`, `Assets/ProjectSettings/UIEffectProjectSettings.asset`, `Assets/Scenes/Arena.unity` or
  `ProjectSettings/EditorBuildSettings.asset` (GG's four modified files; never touch them). Commit trailer:
  `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **Before each report:** `git merge port/stage3` into your branch (the lead integrates by your branch, so you pick up each shared fix).
  Fix conflicts by keeping your own rows and HEAD's shared lines (never hand-edit another row).
- **Movement exact:** at every commit that touches `Skygrind.Core`, CoreHarness shows 0 mismatches and all 22 replays exact. Change
  `Core/Mover.cs` ONLY inside the `wallride35` branch, and only if your card names it (ride38, builder M).
- **Sim parity:** before each sim commit, `dotnet run --project Tools/SimHarness -c Release -- fingerprint --off38` must print EQUAL to the
  baseline, and so must the plain `fingerprint`. Every new sim row is behind `N38Blocks` (`Core/N38Blocks.cs`, bits 0-7, names in order:
  ride38, foundry38, iceline38, kiln38, span38, spire38, finale38, stag38) and reads its own `design.n38.<row>.enabled`.
- **Numbers:** every gameplay number lives in `Assets/StreamingAssets/Design/unity-config.json` (source `Design/unity-config.json`; keep
  the two byte-identical) under `design.n38.<row>` with a `_why` that says where it came from. Never type a gameplay number in code.
  Render / UI numbers go in a `Assets/Scripts/Game/UnityTuning.<Name>38.cs` file, each with a comment. Patch ONLY your own row's text
  (write a `.cjs` script, never `node -e`, never a heredoc; keep the others' rows as they are).
- **Randomness:** gameplay randomness only from `Mulberry32` seeded per run. Never `UnityEngine.Random` or `System.Random` in sim code.
- **Math:** no `System.Math` transcendentals in Core; use `JsMath` where a web number must match.
- **No audio:** no AudioSource, no sound files. Gameplay events get names (e.g. `hud38Ready`, `boss38Entrance`) for GG's sounds.
- **Look:** cyan = player-usable; red-orange = enemies and danger; magenta = RIVAL only; pale violet-white = the boundary. UI text at least
  16 px at 1080p. Enemy models are the web's blocky models (no new enemy design; GG owns enemy design).
- **Nothing lost (PRINCIPLES 1):** no verb is removed; a row that changes an old verb says so in your commit and in your report (VERBS.md).
- **Reports:** under 300 words, with what is done / SHORT, every commit sha, every check with its measured result, and what the lead must
  do. Your final line: `N38-BUILDER <name>: round <r> · done <k>/<n> · commits <sha list>`.
- **Docs and logs:** write them with the Write / Edit tools only (no heredocs into docs). You may leave notes in
  `Research/2026-10-13-wave38/<your-name>.md` (tracked later by the lead).
- **Deleting:** delete only your own scratch files. Never delete anything else.

## 3. The shared surface (the lead's, already committed in 4bd19270)

- `Core/N38Blocks.cs`: the eight bits (above), `From(cfg)`, `FromBits`, `ForceOff`.
- `Game/SurvivalSim.N38.cs`: `n38Override`, `n38` (the run's blocks), `N38StartRun`, `N38Step`. Declaration-only partials you may fill:
  `N38RideStartRun` / `N38RideStep` (builder M) and `N38StagStartRun` / `N38StagStep` (builder S2). Fill a partial by writing the
  body in YOUR own file (for example `Game/SurvivalSim.N38.Ride.cs`) with the same signature.
- `Core/Curriculum25.cs`: `N38Levels` swaps each level row's `levelFrom` for its `levelTo` (rows: foundry38, iceline38, kiln38, span38,
  spire38, finale38) after `N37Levels`. A level row is only live when its `enabled` is true; the level file must exist before you turn it on.
- Harness partials in `Tools/SimHarness/Program.cs` (declaration-only, each a builder's entry point): `N38Gates` (E), `N38Ride` (M),
  `N38Hud` (H), `N38Foundry` (F), `N38Iceline` (S2), `N38Kiln` (K3), `N38Span` (O2), `N38Spire` (C2). Implement yours in a new file
  `Tools/SimHarness/Harness.N38.<Owner>.cs` as `static partial void N38<Owner>(string cmd, string[] args, ref int code)` (check the
  existing `Harness.N37.*.cs` files for the exact shape).
- `--off38` (harness) and `-skygrindOff38` (Player) turn all NIGHT38 blocks off. Tape format 21 records the bits (a VarI).
- `design.n38` (every row `enabled: false`): `ride38`, `foundry38` / `iceline38` / `kiln38` / `span38` / `spire38` / `finale38` (level rows,
  with `levelFrom` / `levelTo`), `stag38`, and the view rows `doors38`, `hud38`, `juice38` (view rows have no bit; they read their own
  `enabled`, and are off with `--off38` too: check `!N38Blocks.ForceOff && cfg.Design("n38.<row>.enabled")`).
- Campaign order today (NIGHT37's rows on): n37-foundry, n37-iceline, n35-kiln, n37-span, n37-spire, n35-finale. The old levels stay
  playable until a row is on. The `n38-*` level files do not exist yet: the level builder creates them.
- VERBS.md rows 9 and 11 are marked "changed by ride38" (builder M's check is in the row).

## 4. The eight cards

### E: envelopes + the gate checker (builder E; owns Harness.N38.Gates.cs, Design/levels/envelopes38.json, Tools/night/n38-gates*, the level-format notes)
Card (PLAN.md section 2; NIGHT38.md E1): measure in the REAL sim (SimHarness, the real Mover and Combat; no hand-typed distances) the max
distance and height of every verb and combo, from flat ground and from a wall ride: jump, double jump, dash, jump-dash, slide-hop, the
wall-ride jump-off (pitch -45 / 0 / +45), wall-jump chains, the THUNDERCLAP boost (mid-air shot: -aim x 10 m/s, `weapons.shotgun.jumpSpeed`;
chained shots), the shockwave (12 m/s), the grapple (40 m, 2 charges). Write `Design/levels/envelopes38.json` and the harness command that
regenerates it. Then `n38 gates <plan.json>` (a harness command): for every room solid tagged `gate: {verb, from, to}`, drop the Mover at
`from` and try the intended verb and EVERY other verb / combo the level owns; PASS only if the intended verb crosses with >= 10 % margin
and no other single verb or combo crosses (D-6); print each gate's margins. A planted cheesable gate must FAIL (a selftest). Also write the
level-format notes for `gate`, `ceiling` (a solid that caps the apex), `noride` (a wall the Mover must not ride: |normal.y| rule or a tagged
non-ride surface; coordinate with M for any Mover flag through the lead), and `flight` (a room type). Land the envelope table and the checker
first, as a commit the lead can merge by 1:00 (about 90 minutes). Then support gate runs for the level builders.

### M: ride38 + doors38 (builder M; owns Core/Mover.cs (wallride35 branch ONLY), Core/Ride35.cs, Core/Grip36.cs, the `noride` flag, DoorFrames29 /
Zones' view, Arena36View, a new Game/Doors38View.cs, Game/SurvivalSim.N38.Ride.cs, Tools/SimHarness/Harness.N38.Ride.cs)
Card (PLAN.md section 1, decision D-3; NIGHT38.md M1):
- ride38 (Core/Mover.cs, inside the wallride35 branch only): a HOLD ride (jump held) keeps its height: no sink (wallride35.sinkSpeed 0.1
  becomes 0 under the row) and no 8 s safety cap (a 60 s stuck guard). The look pitch drifts the ride: pitch above +15 deg rises, below
  -15 deg sinks, up to ~2.5 m/s at +-45 deg; never above the wall's top minus the capsule's upper sample (Mover's contact band feet+0.35 ..
  feet+1.45). A TIMED (tap) ride keeps today's 1.2 s rule. Ends stay: release (the jump-off), crouch, "away", a corner beyond 100 deg, the
  wall ending. Gameplay numbers into design.n38.ride38 with `_why`. The row off = today's Mover bit for bit (golden 0 mismatches).
- noride: a wall flag (dull, ridged material; no cyan trim) that the Mover cannot ride. Coordinate the flag's name with E (E's gate checker
  reads it). Mover needs the flag only inside the wallride35 branch.
- doors38 (view, Game/Doors38View.cs; the row `design.n38.doors38`): every doorway is a holographic curtain across its opening, visible from
  40 m, a scrolling scanline shimmer. CYAN while open (no collider; pass through with a short shimmer on the camera); RED-ORANGE while locked
  (solid, today's gate field made visible from afar); the boss door PALE VIOLET-WHITE with its sign; the open frame lit, the next room lit
  behind it. Fix the slab orientation bug in Arena36View (x vs z doors).
- Checks (harness, no Editor): `--off38` EQUAL; golden 0 mismatches; a 30 s held ride on a 60 m wall holds the feet within 0.05 m of the
  attach height (ride38 on); pitch +45 deg rises at ~2.5 m/s and stops under the wall top; pitch -45 deg sinks. The lead takes the frames
  of a doorway open / locked / boss and the Player reel. Land ride38 (the Core change) as its own commit with its harness rows.

### H: hud38 + juice38 (builder H; owns UiHud33's reticle / rail (H alone in UiHud33 tonight), UiRail34*, HudWeapons30, a new Game/UiHud38.cs,
CameraFeel's boss / hurt hooks, Crt29Pass's strength hook (Render/Crt29Pass.cs), UiTv35, Game/UnityTuning.Hud38.cs / Juice38.cs)
Card (PLAN.md section 4; NIGHT38.md H1; GG: "I want it to be similar to ultra kill UI ... three bars or ... a bar under the crosshair"):
- hud38 (row `design.n38.hud38`): under the crosshair, a three-segment bar per gun's charge / cooldown (KESTREL's charge, THUNDERCLAP's
  slug, the gauntlet's BLAST pips (hidden today by hud33's values layer: UiHotbar.cs:143 / UiHud33.cs:119-124), the grapple's 2 charges).
  One colour while charging, CYAN when full, a white flash and the event `hud38Ready` on the frame it fills (ULTRAKILL's RailcannonMeter /
  Revolver rule). The crosshair's ticks ROTATE through each reload / cooldown (a full turn per cycle) and SNAP square when ready (today they
  only turn on a kill: UiHud33.cs:672-683). The grapple gets its own hook icon with two charge pips.
- juice38 (row `design.n38.juice38`): wire `feel.trauma.boss` (in config, read by nothing); a boss entrance = hit-stop + shake (ULTRAKILL
  boss stomps 2-3, decays 3/s, the bigger replaces) + a full-screen flicker (Crt29Pass's strength spiked ~0.3 s, Render/Crt29Pass.cs:130-146)
  + the card, for ALL bosses incl. THE KILN WYRM; big boss moves (the worm's dig / erupt, the Stag's crash, the Loom's fall, the Crown's
  shatter, the Wick's dive) shake + flicker; damage = a full-screen static / RGB-split burst for the i-frame time scaled by the hit + a small
  shake (UiTv35's hurt glitch is HUD-only today). Events for GG's sounds (names only).
- Numbers: UI / render numbers in UnityTuning.Hud38.cs / UnityTuning.Juice38.cs (each with a comment); gameplay trauma numbers in
  design.n38 rows. Frames come from the lead (no Editor for you): write the code so each cue is one visible change, and say in your report
  what frame to look at.

### F: level 1 additions + level 6 (builder F; owns gen-n36-foundry.cjs's NEW rooms (NOT the stack: W wrote the stack last night; keep it),
Tools/night/gen-n38-finale.cjs, their level files (the n38-foundry and n38-finale levels), the rows `foundry38` and `finale38`)
Card (PLAN.md section 2 L1 and L6; DECIDE38 D-4, D-5): level 1 = `n37-foundry` copied into `n38-foundry`: KEEP a1-a6 (GG praised the
tutorial), ADD one OPEN ROOFED ARENA (KESTREL + ride walls, enemies spawned ON the route) and one FLIGHT ROOM of wall-ride gates (walls
farther apart than any jump, so only a ride chain crosses), placed before THE WICK's rehearsal rooms. Level 6 = `n35-finale` rebuilt whole as
`n38-finale`: mixed gates (one per verb, in sequence: ride, boost, grapple, dash jump), two open arenas; its boss room stays as the old Stag
until THE RIVAL (D-5). Every room: a beat sheet first (`Design/levels/n38/<level>.beats.md`; one job per room; job-tagged solids as n36-jobs
expects), the n36-jobs check GREEN on the new rooms, gate tags for E's checker (only the gates the checker passes ship), the walk bot through
every room (a `Bot/Walk38Foundry.cs` / `Walk38Finale.cs` script of your own rooms, the way Walk36 does it), and a level reel set later (the
lead takes frames). Method: NIGHT36's (Design/levels/n36/foundry.beats.md; Tools/night/gen-n36-foundry.cjs; n36-lib.cjs; n36-jobs.cjs).
Deliver the level to the Player: the JSON, the StreamingAssets copy, `.meta`, the maps list and the unlock row, as the n37 levels did (read
the n37-foundry commit with `git show` for the exact list). Turn your row on only when its gates are GREEN.

### S2: level 2 + stag38 (builder S2; owns gen-n37-iceline-boss.cjs, Design/levels/n37/iceline* (and the n38-iceline level), Stag37* (THE STAG),
the rows `iceline38` and `stag38`, Game/SurvivalSim.N38.Stag.cs, Tools/SimHarness/Harness.N38.Iceline.cs, Bot/Walk38Iceline.cs)
Card (PLAN.md section 2 L2, section 5; NIGHT38.md L2): level 2's copied rooms i1a..i7 are replaced by new rooms (skating, the GAUNTLET,
ride chains; open roofed arenas; one flight room of skate-lane + ride gates). KEEP s1-s4 (the rehearsal rooms and the canal). stag38 (bit 7):
lane walls 5 m tall in runs of 30 m+ (a 2.6 m wall holds only while the feet are under ~2.25 m: Mover's contact band feet+0.35..+1.45), the
island's walls rideable (not `noride`), and a read pass (an off-screen arrow pointing to THE STAG, brighter lane warnings). THE STAG's fight
logic stays (Stag37*). Keep the Stag's budget / pick behaviour; you change the arena geometry and the read pass only.

### K3: level 3 (builder K3; owns gen-n38-kiln.cjs, Design/levels/n38/kiln*, the level `n38-kiln`, the row `kiln38`, Bot/Walk38Kiln.cs)
Card (PLAN.md section 2 L3; NIGHT38.md L3): n35-kiln rebuilt whole around the THUNDERCLAP and the SHOTGUN BOOST (the mid-air shot's
x10 m/s): boost gates (`noride` walls the ride cannot use, ceilings under the double-jump apex, ledges only a mid-air boost reaches; chained
boosts in the last gates), open roofed arenas. THE KILN WYRM's room k5 and the Wyrm are untouched (GG). Same deliverable and gate rules as F
(beat sheet, job tags, n36-jobs GREEN, E's checker GREEN, the walk bot, the Player delivery list, row on only when GREEN).

### O2: level 4 + loom38 (builder O2; owns gen-n37-span-boss.cjs, Design/levels/n37/span* (and the level `n38-span`), Loom37* (THE LOOM), the row `span38`,
Bot/Walk38Span.cs)
Card (PLAN.md section 2 L4; NIGHT38.md L4): n37-span's copied rooms s1-s7 replaced: grapple gates over voids too long for any jump or ride
chain, open roofed arenas with grapple nodes, the STITCHER. Keep o1-o3. loom38: one polish pass from the n37-r7 frames the lead will send
(the lead's frames are the only Editor frames; until then, read the code and write the pass as config-driven numbers) plus its entrance juice
hook (`boss38Entrance`, the event H owns: call the name only). The grapple rows (VERBS 17-23) keep their behaviour: grapple nodes change only
the rooms.

### C2: level 5 + crown38 (builder C2; owns gen-n37-spire-boss.cjs, Design/levels/n37/spire* (and the level `n38-spire`), Crown37* (THE CROWN), the row
`spire38`, Bot/Walk38Spire.cs)
Card (PLAN.md section 2 L5; NIGHT38.md L5): n37-spire's copied rooms v1-v8 replaced by far dash-jump gates under ceilings with a limited dash
meter at each gate (so the dash jump cannot be spammed gap to gap); open roofed arenas; the TORCH (the level's weapon, VERBS 33). Keep v9-v11.
crown38: a look pass (the ring's gems, the shatter, the spire) written as config-driven numbers the lead can tune from the n37-r7 frames.

## 5. Your first hour, in order

1. Read this brief, PLAN.md, GG-REQUEST.md, and your card's source files named in the card.
2. Read the commit `4bd19270` (`git show --stat 4bd19270`) and the n37 files it mirrors (Core/N37Blocks.cs, SurvivalSim.N37.cs).
3. Build your first slice (your card's first checkable item). Check it (golden / fingerprint / dn-check / your harness command).
4. Commit it on your branch. Merge port/stage3. Report (under 300 words) with the final line. The lead integrates at about 1:15, 2:15,
   3:15 and 4:00 into `port/stage3`, and resumes you by message for fixes.

The night's clock: the lead's box is 5:00 (cap 5:20). A report that says SHORT with a measured reason is better than a late DONE.
