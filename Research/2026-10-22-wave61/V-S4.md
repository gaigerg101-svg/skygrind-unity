# NIGHT61 builder V: slate S4, lights hang from real fixtures (design.n61.fixture61, OFF)

Branch `n61-S4`, from port/stage3 at e39148fd. No Editor, no build, no Player and no reel were run in this worktree. Every number below
was read from a file, printed by a command named here, or computed from the levels' data. Anything else is marked "not checked".

## Shas, in order

1. `534edcb7`: the test first. `N61FixtureTests` (Game) and `N61FixtureRowTests` (Core), with pre-placed `Fixture61Math.N61V4.cs`
   (types and signatures; every answer a placeholder) and `UnityTuning.N61V4.cs`. Expected RED: every test except the "row on, no switch"
   line.
2. `1cb74f71`: `Tools/night/n61-v4-rows.cjs` and the row in both config copies (byte-identical). It is appended after `exitFrame61` and
   `bossRule61`, which are left as they are. The name is the slate's, `fixture61`. Expected after it: `N61FixtureRowTests` GREEN.
3. `52d513ab`: the code. `Fixture61Math` filled in and `Fixture61.N61V4.cs` (the component) added. Expected: both test classes GREEN.
4. This notes commit.

## The red (read only)

**gg-proxy's item 5** (`Logs/n61/gg-proxy-start.md`), as written:

> floating: `vi/verbs37/f0300.jpg` top right (a bright ceiling panel under the beams, no mount visible), `vp/verbs37/f0300.jpg` top centre (a
> white lamp block on a thin red-orange line above the door; whether that line is a cable: not checked)

**What I see in the two frames** (`Logs/feel/n60-freeze/vi/verbs37/f0300.jpg` and `.../vp/verbs37/f0300.jpg`). I measured nothing in
them.

- **Iceline.** Top right is a bright white quad near the dark ceiling joists. The ceiling around it is black, and nothing visibly joins the
  quad to anything.
- **Spire.** Top centre is a white block high above the door, with a thin orange vertical line running from the top of the frame down
  through it to the door's "16 m" marker. To me that line reads as the waypoint beam, not a cable; not checked.

**The N55 critic** (`Research/2026-10-20-wave55/critic-ggproxy.md:107-118`) called them "bare white rectangles hang in the ceilings ... a
flat, clipped-white rectangle with no housing, cable or rod, floating under a black ceiling". Its "fixed looks like" was "a dark frame
>= 4 px at 1080p and a hanger to the beam".

I also opened `Logs/feel/n60-b7/v1on-iceline/a357/g2/shot-N56-n52-iceline-g2-entry.png`. No ceiling panel is in view there: a black
ceiling, dark navy joists, and one small wall sconce on the right.

**The red by data.** This is the slice's meter (`Fixture61Math.Count`) run on the plan the views draw (`LightPlan33.GreenFor` with the
config's rows: look30, look33, lights35, lights49 and rods40 all on). It was compiled outside Unity in a scratch harness, the way
`Tools/SimHarness` compiles LightPlan33, and is now deleted. Every ceiling light counts as unmounted: a flush panel with a 0.08 m frame and
no hanger.

| level | ceiling lights, all unmounted |
|---|---|
| n52-foundry | 10 |
| n52-iceline | 10 |
| n57-kiln | 11 |
| n52-span | 8 |
| n52-spire | 10 |
| **total** | **49** |

On n57-kiln the eleventh is kb3d's 2 x 2 m ceiling-socket panel; the others are each room's key panel. This counts every ceiling light in
the plan, not only those in view on the a357 entry frames: the frame count is the lead's (Player).

## The cause (read from the code and data, not measured in a Player)

- **The panel is flush on a real roof.** By data the room's roof box starts exactly at the plan's ceiling height, for example iceline g1:
  plan ceiling 9, roof box y 9..10.
- **The ceiling around it is dark.** look33 makes the key light a spot pointing down ("a dark ceiling round it"), and its key is a hard-
  shadow spot.
- **The frame is too thin to read.** It is 0.08 m, about 2-4 px at 15-25 m at 1080p (camera.fovDeg 74).
- **The beams don't help.** lights35's joists hang 0.45 m round it, and nothing joins the panel to them. A white quad in a black void reads
  as floating.
- **Hanging it lower is ruled out.** rods40 is ON. It hides any piece in the play volume (0.2-12 m over a floor, more than 0.3 m from a
  wall or under a ceiling); that is GG's "white rods that you clip through". A pendant hung lower than 0.3 m would be hidden by
  `LightPlan33.Rods40Filter`, and it would be such a rod. So every mount stays inside the 0.3 m flush band.

## The fix (what the code draws)

All of this lives in `Game/Fixture61.N61V4.cs` and `Game/Fixture61Math.N61V4.cs`, and all of it is view geometry. LightPlan33,
Fixtures33, CampaignLighting24 and LightKit29 are not changed.

The component applies on every campaign level that has the light plan (look33). It reads the same cached plan as the views, because it
passes the same look30 argument. Each ceiling light the plan draws gets:

- **A housing.** Four dark walls, `Fixture61RimW` 0.16 m wide, just outside the 0.08 m frame, hanging `Fixture61Drop` 0.22 m under the
  ceiling. That makes a 0.24 m dark border, about 7 px at 25 m, and the lens sits recessed in a fitting.
- **Hangers.** A rail 0.12 x 0.10 m, 0.10-0.20 m under the ceiling, from the housing to the nearest joist on each side within 4 m. If no
  joist is there, one rail goes to the nearest wall face within 6 m.

Not every ceiling light is mounted: the component skips any panel Fixtures33 itself skips (a ceiling socket's panel over a grapple node).

The parts are built into one stone mesh, coloured `colors.pillarSide` x 0.8 (lighter than the frames' 0.55, so the metal reads against
the dark ceiling). The mesh has no collider and casts no shadow.

On the five levels this gives 294 parts: every ceiling light is housed and held by two beam hangers of 0.59-0.99 m, none by a wall.

**Unchanged:**
- The plan's lights (position, colour, range, intensity, type) and fixtures, which are only read. So no light moves, dims or flickers.
- The key spot's hard shadows, because the mounts cast none.
- Rods40's play volume. Every part stays within `Rods40WallClear` under its ceiling, and on the five levels rods40 hides none of the 294
  parts (checked with `Rods40.BoxHidden`).

**Off:** the row off; `-skygrindOff61` (or older); or `-skygrindNoFixture61` (this row alone).

**The proof (L-089):** one Player log line per map built, for example:

```
[fixture61] mounts on n52-iceline: ceiling lights 10, housed 10, held by a beam 10, by a wall 0, unmounted 0
```

Nothing is logged when the row is off. `Fixture61.Line` and `Fixture61.Now` are kept for a reel.

## Checks run here

- `dn-check-wt.sh`: 0 errors in Skygrind.Game, Skygrind.Game.Tests, Skygrind.Core.Tests and Assembly-CSharp-Editor, at the test-first
  state (13:40) and the code state (13:50). The scratch folder was deleted after each check.
- `bash Tools/night/n61-gates.sh --no-editor`: "gates: all GREEN 13:55:28". Config copies identical; golden 39 vector sets, 22 replays
  exact, 0 mismatches; fingerprint plain and `--off57`..`--off61` EQUAL.
- A scratch copy of the test checks (the synthetic rooms, the switch, the row in the file, and the five-level meter with flush and rods40
  checks), compiled outside Unity on `52d513ab`'s code: 85 / 85 passed. Five levels: red unmounted 49; with fixture61, 0. It is deleted.
  It is not the EditMode run.
- `node Tools/night/n61-v4-rows.cjs`: the dry run, the write, and a refused second run.

## Not checked

- The EditMode tests in the Editor: RED on `534edcb7`, GREEN on `52d513ab`. That run is the lead's.
- **Whether the mounts read at all.** They are dark stone under a ceiling the key spot leaves dark, lit only by the fill light and the
  ambient. They may not show against the black ceiling at 1080p. If they don't, the lever is `Fixture61Shade`: an unlit or emissive trim
  would be a new design, not a number. Not checked.
- The N55 critic's other ask: "clipped-white area per light <= 400 px". The lens is unchanged; not addressed.
- Everything in a Player:
  - the a357 frame count;
  - the SPOT;
  - the a154 flicker count on vs off;
  - the frame-to-frame luma of a still pose;
  - verbs37;
  - NREs.

## The lead's next steps (Player)

1. **Pick and run the tests.** Pick `534edcb7` and run `N61FixtureTests` and `N61FixtureRowTests` (RED). Pick `1cb74f71` (RowTests
   GREEN). Pick `52d513ab` (both GREEN).
   - If the config conflicts with another builder's n61 row, take the merged copies and re-run `node Tools/night/n61-v4-rows.cjs`.
   - Then run `bash Tools/night/n61-gates.sh`. The Editor run of `N61FixtureTests.TheMeter_OnTheFiveLevels_RedEveryCeilingLight_GreenNone`
     prints the meter per level (TestContext).
2. **Build:**
   ```
   bash Tools/night/n41-build.sh <tag>
   git diff --stat ProjectSettings
   node Tools/night/n46-build-rows.cjs <tag> on n61.fixture61
   ```
3. **Reels**, the a357 entry frames on the five levels, on vs off. One pair per level, shown for foundry; repeat for n52-iceline, n57-kiln,
   n52-span and n52-spire:
   ```
   bash Tools/night/n61-reels.sh <tag> s4on-f,n52-foundry,a357 s4off-f,n52-foundry,a357,-skygrindNoFixture61
   ```
4. **The flicker guard (light54's count),** on vs off:
   ```
   bash Tools/night/n61-reels.sh <tag> s4on-l,n52-foundry,a154 s4off-l,n52-foundry,a154,-skygrindNoFixture61
   ```
   Also iceline and kiln. Then compare the `[reel] a154` lines: flicker events, slot moves and holds, mean luminance.
5. Verbs37 x5.
6. **Proof:**
   ```
   grep "\[fixture61\] mounts on" Logs/player/n47-reel-<tag>-s4on-*.log
   ```
   Expect one line per level: ceiling lights 10 / 10 / 11 / 8 / 10, unmounted 0. The off logs should have 0 `[fixture61]` lines.
7. **SPOT:** two a357 entry pairs, on and off, chosen from rooms whose key panel is in view on the entry frame. Which entries show one: not
   checked.
