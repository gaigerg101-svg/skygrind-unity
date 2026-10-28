# NIGHT61 builder V: slate C3, no yellow on the guns (design.n61.gunYellow61, OFF)

Branch `n61-C3`, from port/stage3 at 3441f356. No Editor, no build, no Player and no reel were run in this worktree. Every number below
was read from a file or printed by a command named here. Anything else is marked "not checked".

## Shas, in order

1. `33419f71`: the test first. `N61GunYellowTests` (Game) and `N61GunYellowRowTests` (Core), with pre-placed `GunYellow61.N61V3.cs`
   (signatures; every answer a placeholder) and `UnityTuning.N61V3.cs` (the arc's numbers). Expected RED: the switch, the arc's "in" lines,
   the move to cyan, the caller scan and the proof line.
2. `a25126a2`: `Tools/night/n61-v3-rows.cjs` and the row in both config copies (byte-identical). It is appended after `fixture61`; the
   other n61 rows are left as they are. The name is the slate's, `gunYellow61`. Expected after it: `N61GunYellowRowTests` GREEN.
3. `edf66c47`: the code. `GunYellow61` filled in, plus two hook lines in `Viewmodel.cs`. It also narrows the test's caller scan to calls
   of the rule (`GunYellow61.Family`): the tuning file's header comment names `GunYellow61.N61V3.cs` and would have been flagged. The scan's
   intent is unchanged. Expected: both test classes GREEN.
4. This notes commit.

## The red (read only)

gg-proxy's C3 (`Logs/n61/gg-proxy-start.md`): "yellow slabs and pips on THUNDERCLAP (kb2c-entry.png, vs/verbs37/f0600.jpg) and TORCH
(vp/verbs37/f0300.jpg, f0900.jpg); the yellow pixel count on the viewmodel in those poses is this slice's first measurement".

I opened all four frames and measured nothing in them. What I see:

- **`Logs/feel/n60-b8/b8on/a157/shot-N29-L57-n57-kiln-kb2c-entry.png`** (THUNDERCLAP, bottom right): a flat gold-yellow slab on the top of
  the body, a bright yellow triangle on top (the gem), and a lime-green line along the body's side.
- **`Logs/feel/n60-freeze/vs/verbs37/f0600.jpg`** (THUNDERCLAP): the same gold slab and bright yellow triangle. A lime-green stripe runs
  along the floor's edge on the left; that is level decor, not the gun.
- **`Logs/feel/n60-freeze/vp/verbs37/f0300.jpg`** and **`f0900.jpg`** (TORCH): an orange-gold slab on top, a small yellow square on a post
  at the muzzle (the pilot light), a yellow triangle in a dark ring (the gem), and thin gold bands round the tank.

## Where the yellow comes from (read from the code and config)

**The yellow families.** In `weapons.families` (the web's config):

| family | colour | hue | weapons |
|---|---|---|---|
| edge | #ffb000 | 41° | THUNDERCLAP (shotgun), DISCUS, SUNDER |
| fire | #ff8c1a | 30° | TORCH |
| blast | #ffe14a | 50° | BOUNCER |

Volt (#b8ff2c) is lime at 80°, outside the tells' band (45-70°).

**What the family colour drives.** Each viewmodel gun's accent plate is built from it (through `HandsLook.Accent`), and so are its gem
(`Gem(..., family)`) and the TORCH's pilot and nozzle glow (`family * 3`).

**Where it is read.** `Viewmodel.FamilyOf` (STITCHER, COMET, LONGWAVE, ZAPPER, BOUNCER, TORCH, VORTEX), plus the THUNDERCLAP's own build
line. `Refinish` rebuilds from `rig.gemColor`, which is the same value.

**Why yellow56 didn't fix it.** yellow56 moved these guns to a pale brass at S 0.32, which is not a dusk palette colour, and its row is OFF:
it needs a tells mask (RETRO.md:54).

## The fix

All of this lives in `Game/GunYellow61.N61V3.cs`, plus two hook lines in `Viewmodel.cs`. It is view only.

- **The rule.** A viewmodel gun's family colour in the yellow arc takes `colors.usable` (#19d3e6), the dusk palette's player-usable cyan,
  with its alpha kept. The arc is yellow56's own: hue 28-75°, S over 0.5 (`UnityTuning.N61V3.cs`).
- **What moves together.** The plate, the gem and the TORCH's pilot glow. On the config this moves edge (THUNDERCLAP), fire (TORCH) and
  blast (BOUNCER), and leaves kinetic, volt, gravity, impact and pierce.
- **Unchanged.** The guns' shapes (colour only), wall buys, pickups, the mystery box, muzzle flashes, and every enemy and tell colour. A
  test scans `Assets/Scripts` and finds the rule called from `Viewmodel.cs` only.
- **With yellow56 also on,** this rule runs first (inside yellow56's call). Yellow56 then leaves the cyan, which is outside its arc.
- **The row is read when the viewmodel is built,** before PlayerChecks, so the command line is read as well (as yellow56 and guns52 do).
- **Off:** the row off; `-skygrindOff61` (or older); or `-skygrindNoGunYellow61` (this row alone).
- **The proof (L-089):** one Player log line per gun moved, for example `[gunyellow61] shotgun #FFB000 -> #19D3E6 (colors.usable)`.
  Nothing is logged when the row is off. `GunYellow61.GunsMoved` and `Moved` are kept for a reel.

## Checks run here

- `dn-check-wt.sh`: 0 errors in Skygrind.Game, Skygrind.Game.Tests, Skygrind.Core.Tests and Assembly-CSharp-Editor, at 14:14, 14:18 and
  14:20 (after the scan fix). The scratch folder was deleted after each check.
- `bash Tools/night/n61-gates.sh --no-editor`: "gates: all GREEN 14:22:20". Config copies identical; golden 39 sets, 22 replays exact,
  0 mismatches; fingerprint plain and `--off57`..`--off61` EQUAL.
- `node Tools/night/n61-v3-rows.cjs`: the dry run, the write, and a refused second run.
- The arc on the config's colours, by hand from their hex values: edge 41° / S 1.00, fire 30° / 0.90, blast 50° / 0.71 are in; kinetic
  186°, volt 80°, gravity 259°, impact 221°, pierce 200° / S 0.08 and colors.enemy (12°) are out. The EditMode test pins the same.

## Not checked

- The EditMode tests in the Editor. That run is the lead's.
- **The tank's gold bands.** Whether they are drawn from the family accent (and so turn cyan) or from a fixed colour is not checked;
  if fixed, they stay.
- Everything in a Player:
  - the yellow pixel count in the four poses;
  - the SPOT;
  - verbs37;
  - NREs;
  - how a cyan THUNDERCLAP or TORCH reads next to the KESTREL, which is already this cyan (kinetic = colors.usable).
- Finishes (mastery skins with their own body tones) are not covered. The reels use the factory finish.
- A TORCH heated or firing: the heat glow (`viewmodel.heatColor` #ffd890) and its flames are not this rule's.

## The lead's next steps (Player)

1. **Pick and run the tests.** Pick `33419f71` and run `N61GunYellowTests` and `N61GunYellowRowTests` (RED). Pick `a25126a2` (RowTests
   GREEN). Pick `edf66c47` (both GREEN).
   - If the config conflicts with another builder's n61 row, take the merged copies and re-run `node Tools/night/n61-v3-rows.cjs`.
   - Then run `bash Tools/night/n61-gates.sh`.
2. **Build:**
   ```
   bash Tools/night/n41-build.sh <tag>
   git diff --stat ProjectSettings
   node Tools/night/n46-build-rows.cjs <tag> on n61.gunYellow61
   ```
3. **Reels, on vs off in one build.** The four poses: the THUNDERCLAP in kb2c's a157 entry and span's verbs37 f0600; the TORCH in spire's
   verbs37 f0300 and f0900.
   ```
   bash Tools/night/n61-reels.sh <tag> c3on-k,n57-kiln,a157 c3off-k,n57-kiln,a157,-skygrindNoGunYellow61
   bash Tools/night/n61-reels.sh <tag> c3on-s,n52-span,verbs37 c3off-s,n52-span,verbs37,-skygrindNoGunYellow61 c3on-p,n52-spire,verbs37 c3off-p,n52-spire,verbs37,-skygrindNoGunYellow61
   ```
   Then verbs37 x5.
4. **Meter.** Count yellow-hue pixels (yellow56's band: 45-70°, S > 0.5, V >= 0.15) inside the viewmodel's region of those frames, on vs
   off. Which tool: not checked. `Tools/night/n56-colour.py` measures yellow outside the enemy rects and the HUD.
5. **Proof:**
   ```
   grep "\[gunyellow61\]" Logs/player/n47-reel-<tag>-c3on-*.log
   ```
   Expect a line for each moved gun the run builds (shotgun, torch, bouncer). The off logs should have 0 such lines.
6. **SPOT** on the THUNDERCLAP pair (the kb2c entry, on and off).
