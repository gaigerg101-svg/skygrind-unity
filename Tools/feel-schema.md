# feel-schema: the reel's telemetry (NIGHT18, FeelProbe -> Tools/feel-metrics.py)

A reel run (`SKYGRIND.exe -skygrindReel <abs dir> [-skygrindReelOnly a,b] [-skygrindFeelAB off]`) writes one folder per
side and scenario. Every frame of a scenario is a full-resolution JPG plus one JSONL line.

## Folders

```
<reel dir>/                      e.g. Logs/feel/b1/on/
  reel.json                      the run: build stamp, feelAB, map, screen size, fps, the scenarios run (below)
  <scenario>/                    e.g. kill12/
    telemetry.jsonl              line 1 = the scenario header; then one line per rendered frame, in order
    f0000.jpg f0001.jpg ...      every rendered frame, JPG q90, 1920 x 1080 (frame f = the line with "f": f)
```

`Logs/feel/<tag>/` holds a build's reels: `on/` (FeelAB on, tonight's look) and `off/` (`-skygrindFeelAB off`,
NIGHT17's look). Build 1 (the baseline) has `on/` only; it is the reference column of every metric.

## reel.json

```json
{"build":"SKYGRIND 0.1.0 (abc1234)","feelAB":true,"map":"ARENA","w":1920,"h":1080,"fps":60,"simHz":120,
 "scenarios":["hit12","kill12"],"started":"2026-10-06T09:40:00"}
```

## The scenario header (line 1, `"type":"scenario"`)

| Field | Type | Meaning |
| --- | --- | --- |
| `name` | string | the scenario (`kill12`, `boost`, `ui-settings` ...) |
| `kind` | string | `kill`, `hit`, `camera` or `ui` (picks the flipbook layout and the metrics) |
| `feelAB` | bool | the side |
| `seed` | int | the run's seed (the same on both sides) |
| `distance` | number or null | the target's distance (m) for kill / hit scenarios |
| `target` | int or null | the target enemy's id (the `enemies[].id` to follow) |
| `inputFrame` | int | the frame of the scenario's input (the shot, the key press, the menu open); the frames before it are pre-roll |
| `w`, `h` | int | the frame size in px (1920, 1080) |
| `fps` | int | the capture rate (60): every frame is exactly 1/60 s of game time |
| `simHz` | int | the sim's rate (120) |
| `maxSpeed` | number | the player's top ground speed (m/s), for M8's 70 % line |

## A frame line (`"type":"frame"`)

| Field | Type | Meaning |
| --- | --- | --- |
| `f` | int | frame index from 0 (the JPG `f%04d.jpg`) |
| `t` | number | game time since frame 0 (s): `f / fps` |
| `step` | int | the sim step after this frame's steps ran |
| `steps` | int | sim steps run this frame (2 at 60 fps; 0 while HELD) |
| `held` | bool | the frame is inside a freeze (hitstop): no sim time passed |
| `heldSteps` | int | the steps the freeze swallowed this frame (`round(frameDt x simHz) - steps` while held, else 0) |
| `slowmo` | number | the impact layer's time scale this frame (1 = none) |
| `flow` | string | `Title`, `Playing`, `Paused`, `Down`, `Results` |
| `cam` | object | `fov` (vertical deg), `roll` (deg, + = clockwise on screen), `pitch` (deg, + = up), `height` (m above the player's feet), `x`,`y`,`z` (Unity world) |
| `vel` | object | the player's velocity in the camera's level frame (m/s): `fwd`, `right`, `up`; and `speed` (horizontal) |
| `enemies` | array | each drawn husk: `{"id":3,"rect":[x,y,w,h],"flash":0.0,"anchor":[x,y]}` (anchor: its sim position + 0.9 m on screen; moves only with the camera) |
| `fx` | object | each effect group drawn this frame (absent when not drawn): `{"rect":[x,y,w,h],"n":12,"a":1.0}`; groups: `chunks`, `burst`, `ring`, `killMarker`, `hitMarker`, `speedLines`, `hitFlash`, `sparks`, `shed` |
| `chunkPts` | array | the chunks' screen centres this frame (at most 64): `[[x,y],...]`; `chunkTotal` is the true count |
| `ui` | array | every visible UI Graphic (below) |
| `events` | array of string | moments on this frame: `input`, `shot`, `hit`, `kill`, `headshot`, `freezeStart`, `freezeEnd`, `flourish`, `land`, `slide`, `boost` ... |

**Rects are image pixels, origin top-left: `[x, y, w, h]`** at the frame's size, so `rect` crops the JPG directly.

### A UI element (`ui[]`)

| Field | Type | Meaning |
| --- | --- | --- |
| `id` | int | stable per element for the whole scenario (the scanner's index) |
| `path` | string | the GameObject path under its canvas root (`HUD/Points/Value`) |
| `rect` | [x,y,w,h] | screen rect, image px, top-left origin |
| `scale` | number | `lossyScale.x` divided by its root canvas's `lossyScale.x` (1 = authored size) |
| `alpha` | number | the effective alpha: the colour's alpha x every CanvasGroup above it |
| `color` | string | `#RRGGBB` of the Graphic's colour (TMP: the vertex colour) |
| `role` | string or null | `GOLD`, `DANGER` or `RIVAL` when the colour is that token (M11); else null |
| `px` | number or null | TMP only: the rendered size at 1080p (`fontSize x canvas.scaleFactor x 1080 / Screen.height`) |
| `text` | string or null | TMP only: the first 24 characters (to tell a digit change from a new element) |

An element is listed only on frames where it's visible (active, alpha > 0.01, rect on screen, w and h > 0). The harness's
own canvases (the FeelAB chip, the strip stamp) are never listed.

## How the metrics read it (Tools/feel-metrics.py)

- **Frame 1** of an event is the frame whose line carries the event: its sim steps ran the shot / hit / kill, and its JPG
  is the first picture rendered after it. "Frame 2" is the next one, and so on. The frame before it is the event's
  pre-frame.
- **M1** (kill12 vs hit12): frame 1 of the `shot` vs the shot's pre-frame; luma change > 40 in a 400 x 400 window
  (scaled by h / 1080 when the frame isn't 1080 p) centred on the target's rect on the pre-frame; the `ui[]` rects of both
  frames masked out.
- **M2**: the union of `fx.burst`, `fx.chunks`, `fx.ring` rects over frames 1-4 of `kill`, as a share of `h`; the
  `fx.killMarker` frame (first after `kill`), its rect's longer side, and how long it stays.
- **M3**: 250 ms (15 frames) after the last `held` frame of the kill: the chunks' centroid (mean of `chunkPts`) against
  the target's rect on the frame before `kill` (the body height = that rect's h); the share of `chunkPts` outside that rect.
- **M4**: the sum of `heldSteps` from `shot` to the first frame not held after it.
- **M5**: the body's own motion: the target's `rect` centre minus its `anchor` (and the top edge minus the anchor's y),
  frame by frame from `hit`, against the same on the hit's pre-frame. The anchor is the husk's sim position + 0.9 m
  projected: it moves only with the camera, so the gun's recoil kick (which moves the whole view) cancels out. Pixels
  are scaled to 1080 p (x 1080 / h). The flash: frames with `flash` > 0.5.
- **M6 / M7**: `cam.fov`, `cam.height`, `cam.roll` from `input`, against the mean of the pre-roll; `vel.right` for the
  strafe.
- **M8**: `fx.speedLines.a` against `vel.speed / maxSpeed`; its rect against the central 60 % of the frame.
- **M9**: elements whose `id` first appears after frame 0 (not visible on frame 0); rest = the median of their last 10
  visible frames' `scale`, `alpha`, `rect` position; a pop-in is a first frame at >= 0.95 rest scale and >= 0.95 rest
  alpha and < 40 px from rest. A `text` change on an element already visible is not an entrance.
- **M10**: every `ui[]` with `px` < 18.
- **M11**: every `ui[]` with a `role`, against `Tools/feel-roles.json` (the allowed path prefixes per role).

A hand-written sample of a hit scenario (5 frames) is `Tools/feel-sample/hit12/telemetry.jsonl` (no JPGs: the metrics
tool's tests make flat grey frames of the right size).
