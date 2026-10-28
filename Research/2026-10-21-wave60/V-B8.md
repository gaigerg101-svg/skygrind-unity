# NIGHT60 builder V: B8 the exit lamp (V3)

No Editor, Player, build or reel was run. This note is a file of its own so that B7 attempt 2's append to V.md picks without a conflict.

## 1. The switch: the view row design.n60.lamp60

**Commit 1, the test.** `Assets/Tests/EditMode/N60SharedTests.cs` checks:
- design.n60 exists with a `_why`;
- lamp60 is a view row (no tape bit) with a `_why`, a bool `enabled`, an `enabled_why` and a `numbers` object;
- it is OFF unless its `enabled_why` starts with ON;
- the checker is clean;
- `NightRows.ViewOn` reads it, and `--off60` / `--off59` turn it off.

It is RED on that commit because design.n60 does not exist yet.

**Commit 2, the row.** `Tools/night/n60-v-rows.cjs` adds design.n60 = { `_why`, lamp60 } after design.n58 in both config copies.
- The two copies stay byte-identical.
- The script asserts nothing else changed and refuses to run twice.
- The block has `numbers: {}` as every NIGHT56+ row has (N56SharedTests' rule).

The code:
- `Exit58.Off(cfg)` is now: `!NightRows.ViewOn(cfg, "lamp60")`, or `-skygrindOff60` (`Rim60.Off60`), or `-skygrindOff58`, or
  `-skygrindNoExit58`. The OFF line names which one applied.
- `UnityTuning.Exit58On` is removed.
- A build copy turns the lamp on with `node Tools/night/n46-build-rows.cjs <tag> on n60.lamp60`.
- N60SharedTests goes GREEN on this commit.

## 2. The frames: a157's clear shot (`-skygrindClearShots60`)

No reel framed a room's clear. a157 frames each room at entry with its wave alive; a358 is a357's colour run.

**What the flag adds** (`Assets/Scripts/Game/FeelReel.N60.Lamp.cs`, one hook line in `FeelReel.N57.Level.cs`; a157 is unchanged
without it). For each room, after its entry shot:
- **The exit.** It picks the room's exit as Exit58 lamps it: a gated campaign door leaving the room, not into a boss room.
- **The clear.** It kills the waves until Zones opens that door, or until `UnityTuning.Lamp60ClearWaitS` (30 s) runs out.
- **The wait.** It waits out the lamp's rise (`Exit58FadeS`) and `Lamp60SettleS` (0.5 s).
- **The pose.** It stands at the entry spot and looks level at the door's centre.
- **The output.** It saves `shot-N29-L57-<level>-<room>-clear.png` with a sidecar `.json`:
  - box: the opening grown by the lamp frame, image px `[x, y, w, h]` from the top-left;
  - the opening;
  - opened;
  - `Exit58.Line`.

Its reel-only numbers are in a NEW `UnityTuning.N60V8.cs`, with a comment each.

## 3. The meter (`Tools/night/n60-v-lamp.py`, planted output `Logs/n60/V/lamp-planted.txt`)

- **MAIN, per room on the ON side:** the mean luma inside the box / the median luma outside it >= 1.5.
- **GUARD, ON vs OFF:** the p95 luma outside the box within TOL 0.02 of the off side.
- Luma is Rec. 709 on the stored sRGB values.

Planted cases (synthetic, all as planted, GREEN):

| case | ratio | p95 \|d\| | MAIN | GUARD |
|---|---|---|---|---|
| the lamp in the box | 1.54x (off 1.01x) | 0.000 | PASS | PASS |
| no lamp | 1.01x | | FAIL | |
| a room-wide x1.10 spill | | 0.025 | | FAIL |
| the lamp plus its point light's pool on the floor | | 0.007 | | PASS |
| a lamp at 1.2x the wall | 1.04x | | FAIL | |

**Planted on real n57-kiln a157 entry frames** (NIGHT59 l0on). The lamp is painted in a door-sized box at the frame's centre. The
first figure is with the lamp, the one in brackets without:

| room | ratio with the lamp (without) |
|---|---|
| kb2 | 2.51x (1.91x) |
| kb2c | 2.43x (1.84x) |
| kb3 | 2.16x (1.57x) |
| kb3c | 1.62x (1.03x) |
| kb3d | 1.43x (0.95x) |

The p95 outside the box moves by 0.000 on every frame.

**The MAIN can pass without any lamp.** Some of those boxes read 1.57-1.91x unpainted. An open door onto a bright room can do the
same. So the meter prints the OFF ratio and the lamp's own gain (door luma on / off) beside every room. The MAIN stays as B8 writes
it.

## 4. Not checked, and what the lead runs

- **Not checked:** everything in the Player.
- **Build:** a Player from port/stage3 with these commits, then `node Tools/night/n46-build-rows.cjs <tag> on n60.lamp60`. kiln57 is
  already ON in the repo.
- **Reels:** `bash Tools/night/n60-reels.sh <tag> b8on,n57-kiln,a157,-skygrindClearShots60 b8off,n57-kiln,a157,-skygrindClearShots60,-skygrindNoExit58`
- **Each on-side clear line must print:**
  - `door <id> opened yes`
  - `lamp exit lamps ON on n57-kiln: gated doors N, lit >= 1`
- **The off side must print:** `exit lamps OFF (-skygrindNoExit58)`.
- **MAIN and GUARD:** `python -I Tools/night/n60-v-lamp.py Logs/feel/<tag>/b8on/a157 Logs/feel/<tag>/b8off/a157`. Its LAMP line is
  the MAIN count over the new rooms (kb2, kb2c, kb3, kb3c, kb3d) and the GUARD count.
- **Risks:**
  - A room whose waves do not end inside 30 s prints `opened NO`.
  - A door that is off the screen at the entry spot prints `box null` and is not measured.

## 5. Attempt 2: the cause (n60-b6: the lamp's own gain 1.00-1.06x, the clear shot not at a clear)

Read against `Logs/feel/n60-b6/b8on/a157/shot-N29-L57-n57-kiln-kb2c-clear.png` (ON) and its OFF twin, with the boxes drawn (scratch,
not committed).

**Cause 1: the reel never cleared the rooms.**
- a157 boots DOORFREE: `Campaign41(g, offs, true, ok)` calls `DoorFree41`, so every door opens at the start.
- Exit58's lamps all lit at frame 272. Every clear line reads `lit 8; last lit d7 at frame 272`.
- So the clear shot's "until the door opens" loop ended at once. The frame caught the room's wave alive: "15 LEFT", swarmers
  mid-room, ON and OFF the same.
- **Fixed (this commit).** The condition is the room's own clear: the campaign in this room reads phase Cleared, with every wave killed
  until then.
- The line and the sidecar now print `room cleared yes|NO (phase, alive)`.

**Cause 2: the lamp is buried in the wall.** n57-kiln's doors are 4 m deep passages. d5's rect is z -197.5..-193.5, between two 2 m
end walls (kb2c's z -195.5..-193.5, kb3's -197.5..-195.5), with a lintel wall above (y 4.5..14).
- **Where Exit58 puts the frame.** It centres the frame on the door rect's centre. The posts sit at x ±2.26 (0.22 thick: 2.15..2.37,
  inside the side walls that start at x ±2) and at z -195.5, 2 m deep from either room's face. The lintel sits at y 4.65..4.87, inside
  the lintel wall.
- **So the frame is invisible from both rooms.** The point light sits 1.5 m from the centre, still inside the passage. That fits the
  lamp's own gain of 1.00-1.06x.
- **The smallest fix** (the next commit, behind lamp60 like the whole lamp):
  - stand the frame just in front of room A's face of the passage (the face seen from the room being left), at the rect's centre -
    ahead x (its depth / 2 + FrameW / 2 + a small gap);
  - put the light Exit58LightBack in front of that face.

  A thin door's centre is its face, so a thin door is barely moved.