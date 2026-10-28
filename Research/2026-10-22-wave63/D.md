# NIGHT63 builder D: D1 same-run SPOT pairs (branch n63-D1, from port/stage3 799a0970)

## What it does
One reel run gives both halves of every SPOT pair. With `-skygrindSameRun63 <night.row>` (for example `n61.exitFrame61`), each reel
shot works like this:
1. The frame with the row ON is written as always (Shot29 / Shot30).
2. FeelClock holds one more frame. In that frame the sim takes 0 steps and keeps its alpha. There is no input read, no draft and no
   impact frame. `Time.timeScale` is 0. Every view's own wall clock goes through FeelClock's shims. UnityEngine.Random and the SSAO
   noise are replayed.
3. `NightRows.ReelOverride` turns the row OFF for that one frame. That frame is written to `<side>-off/` under the same file name.
4. The row comes back on and the hold ends. FeelProbe never records the held frame, so telemetry.jsonl and f####.jpg are the same
   as in a run without the option.

Without the option nothing runs and no frame is added. Every shim returns Unity's own value, bit for bit, until a hold begins.

## Commits on n63-D1, in order (cherry-pick `39177d68^..<notes sha>`)
1. `39177d68` Test first (RED). The tests are N63SameRunTests (Core) and N63SameRunGameTests (Game), run against placeholders.
2. `ef89a55a` Core: `NightRows.ReelOverride`, `NightRows.Live(decided, row)` and an allocation-free `ViewOn`
   (`GameConfig.DesignEnabledAt`). This applies the lead's hint.
3. `0f80d89c` The reel:
   - SameRun63Math
   - FeelClock's hold and shims
   - FeelProbe skips held frames
   - FeelReel.N63Same.cs: the option, `SameRun63`, and the planted scenario `same63`, registered in PhaseN57
   - SameRun63View: Random replay, the plant63 square and the UI root hide
   - Ui.N63Same.cs
   - One hook line each in Shot29 and Shot30
   - ExitFrame61 re-reads its row every frame
4. `aac1abbc` Every visible unscaled-time reader now goes through FeelClock's shims, in 30 files. Ssao55 snapshots the shot
   frame's SSAO noise and replays it in the held frame.
5. `655b8942` SurvivalGame.Update: three one-line guards so a held frame reads no input, opens no draft, and runs no impact frame or
   step.
6. This notes commit.

## The red (measured, read only, before any change)
`python -I Tools/night/n62-gun-yellow.py --pair Logs/feel/n62-p0/s1on/a157/shot-N29-L57-n57-kiln-kb3-entry.png
Logs/feel/n62-p0/s1spot/a157/shot-N29-L57-n57-kiln-kb3-entry.png --rect 0,0,1,1` gives **changed px 139791**. This is the kb3 ENTRY
frame of NIGHT62's two P1 runs, taken before any lamp lights. The difference is pure drift between two runs.
- The kb3 CLEAR pair (the row plus the drift) gives changed px 57608.
- The nearest enemy, from `grep -o 'nearest [0-9.]*' Logs/player/n47-reel-n62-p0-s1{on,spot}.log`, was 15.3 vs 15.2 m at kb3 and 16.7
  vs 16.8 m at kb3d.

## Tests (measured outside Unity; the Editor run is the lead's)
I ran a scratch net10.0 project with NUnit 3.14 from the NuGet cache. It compiled Core, the two test files and the pure Game files
(SameRun63Math, UnityTuning.N63D). The project is in my scratchpad, not in the repo.
- **RED on 39177d68:** 14 failed, 3 passed. The 3 that pass pin behaviour that must not change. The old ViewOn allocated 620,800
  bytes over 200 frames of the six rows.
- **GREEN on ef89a55a / 0f80d89c:** 17 / 17 passed (N63SameRunTests 10, N63SameRunGameTests 7).
  - With no override, ViewOn equals NIGHT62's formula for every design row of the config. This holds with no off switch and under
    ForceOffFrom 46, 55, 61, 62, 63 and 64.
  - The override wins over the off switch, and the off switch wins over the design.
  - ViewOn plus Live allocate 0 bytes, with and without an override.
- The Editor runs the alloc check on the `Recorder("GC.Alloc")` branch (TrapsTests' helper). That branch is not checked; my scratch
  run used `GC.GetAllocatedBytesForCurrentThread`.

Other checks:
- dn-check: 0 errors after every commit.
- Gates: `bash Tools/night/n63-gates.sh --no-editor` all GREEN at 23:05:01 (on ef89a55a) and **23:16:25 (on 655b8942)**. Config copies
  identical; golden 0 failing, 0 mismatches, 22 / 22 replays exact; fingerprint plain and --off57..--off63 EQUAL.

## The contract for every row D1 claims (pass it to V, G, and D's own C1 / B3)
Claimed rows:
- **Frame kind:** exitFrame61, bossMotion63, bossTell63, bossColours63, colours63
- **UI root kind:** bossRule61
- **Planted:** noop63, plant63

Any other row is "unclaimed". Its proof line says a 0 px pair proves nothing about it.

1. Read the row **every frame**: `NightRows.Live(decided, "<row>")`, where `decided` is the row's own switch read once at build. You
   can also call `NightRows.ViewOn(cfg, "<row>")`. Both are allocation-free now.
2. While it reads false, show the row's off look. **Keep its state** (springs, fades, timers, lit flags) as it was, so the next frame
   is the one it would have drawn. See ExitFrame61's `reelHidden`.
3. Animate on the loop's `frameDt` / `WorldDt` (0 in a held frame). Where a wall clock is needed, use `FeelClock.UnscaledDelta`,
   `UnscaledTime`, `UnscaledNow` or `FrameCount`. Never use `Time.unscaledDeltaTime`, `Time.unscaledTime` or `Time.frameCount` for
   anything drawn. Once-a-frame guards keep `Time.frameCount`.
4. A new shot helper ends with `yield return SameRun63(dir, "<the png's file name>");` after it writes its PNG. Shot29 and the
   2-argument Shot30 already do. V's boss63: use Shot30, or add that line.
5. A row decided at an event is judged OFF with its UI root hidden. Add a hide/restore pair like `Ui.SameRun63HideRule` and name the
   row in `SameRun63Math.UiRootRows`.

One build can now carry several rows ON. Each reel side toggles only its own row, so a pair differs by that row alone (NIGHT63.md
section 2).

## The exact Player steps (the lead's: I ran no Editor, build, Player or reel)
Build:
- `bash Tools/night/n41-build.sh <tag>`, then `node Tools/night/n46-build-rows.cjs <tag> on n61.exitFrame61 n61.bossRule61`.
- The rows are only needed for the claimed-row cases. The planted cases need none.

Reel sides, one invocation in the background:
```
bash Tools/night/n63-reels.sh <tag> d1base,survival,same63 d1noop,survival,same63,-skygrindSameRun63,n63.noop63 \
  d1plant,survival,same63,-skygrindSameRun63,n63.plant63 d1noopk,n57-kiln,a157,-skygrindClearShots60,-skygrindSameRun63,n63.noop63 \
  d1noopw,n57-kiln,boss61,-skygrindSameRun63,n63.noop63 p1k,n57-kiln,a157,-skygrindClearShots60,-skygrindSameRun63,n61.exitFrame61 \
  p2w,n57-kiln,boss61,-skygrindSameRun63,n61.bossRule61
```
`survival` is RunSetup.DefaultMap, the map the survival-style reels run on. same63 on a campaign map is not checked.

The meters and what each doneWhen line wants:
- **Planted no-op, want 0 on every shot.**
  `for f in Logs/feel/<tag>/d1noop/same63/shot-N30-S63-*.png; do python -I Tools/night/n62-gun-yellow.py --pair "$f" "${f/d1noop/d1noop-off}" --rect 0,0,1,1; done`
  prints "changed px 0" for each of the 6 shots. Do the same on the real poses:
  - d1noopk: every `a157/shot-N29-*.png`
  - d1noopw: `boss61/shot-N30-B61-wyrm-1000.png` and `-720.png`
- **Planted 10 x 10, want exactly 100 on every shot.** The same loop on d1plant/same63 prints "changed px 100".
- **One planted case per claimed row.**
  - exitFrame61 (p1k): the kb3 clear pair changes inside the door's box. P1's MAIN is
    `python -I Tools/night/n60-v-lamp.py Logs/feel/<tag>/p1k/a157 Logs/feel/<tag>/p1k-off/a157` (OWN GAIN line).
  - bossRule61 (p2w): `--pair .../p2w/boss61/shot-N30-B61-wyrm-1000.png .../p2w-off/boss61/shot-N30-B61-wyrm-1000.png` gives
    changed px > 0, and its proof line says `ui root hidden yes`.
  - The four n63 rows are not built yet. For now, the Core test `EachClaimedRow_FollowsTheOverrideBothWays` pins the override on
    their names. Their Player case is each row's own SPOT reel with `-skygrindSameRun63 n63.<row>` once the row is picked and on.
- **The sim never sees the toggle.**
  - `diff <(grep '"type":"frame"' Logs/feel/<tag>/d1base/same63/telemetry.jsonl) <(grep '"type":"frame"' Logs/feel/<tag>/d1noop/same63/telemetry.jsonl)`
    should print nothing. same63 uses frame-counted waits only. Whether two plain runs of same63 are themselves identical is not
    checked; if the diff is not empty, run d1base twice to see the baseline noise.
  - The two `[reel] same63:` lines should match on "frames recorded" and "sim steps".
  - The fingerprints are the gates.
- **Proof lines:**
  - `grep "\[same63\] option" Logs/player/n47-reel-<tag>-d1noop.log` gives one line.
  - `grep -c "\[same63\] toggle fired" Logs/player/n47-reel-<tag>-d1noop.log` gives 6 (one per shot).
  - `grep "\[same63\] toggle fired" <log> | grep -vc "sim held yes"` gives 0. The line carries step, feet, alive, alpha, FeelClock.Now,
    Time.time, the frame numbers and the off path.
- **P1's SPOT pair:**
  `node Tools/night/n63-spot-pair.cjs P1 Logs/feel/<tag>/p1k/a157/shot-N29-L57-n57-kiln-kb3-clear.png Logs/feel/<tag>/p1k-off/a157/shot-N29-L57-n57-kiln-kb3-clear.png <tag>`

SPOT pose: n/a (tooling). Pruning: n63-prune.sh deletes only f####.jpg in sides named in summary47; `<side>-off/` holds PNG shots
only and is never pruned.

## Not checked (no Editor, no Player here)
Nothing was run in Unity. The Player is the measure: the planted no-op pair must be 0 changed px. These assumptions are untested:
- **Unity:** a coroutine resumed at end of frame that yields `null` resumes after the next frame's Update. `captureDeltaTime` with
  `timeScale` 0 gives `Time.deltaTime` 0.
- **URP:** `_Time` comes from scaled `Time.time` in a Player build. FilmGrain's per-frame `Random.value` is replayed by the Random
  state restore. The SSAO noise snapshot taken at the shot frame's end is the noise that frame drew with.
- **The plant63 square:** an overlay canvas, pixel perfect, at scale 1, gives exactly 100 px, and no pixel under it is within 24 of
  magenta.
- **Views not routed:** any view that animates per frame on a clock I did not route stays a risk. The routed and left lists are in
  `aac1abbc`'s message. Views on scaled time (`Time.time`, `Time.deltaTime`) are covered by `timeScale` 0.
- **Not covered:**
  - The f####.jpg recording gets no pairs (shots only).
  - Wall-clock waits inside a157 and boss61 still make two runs differ. That is why the pair comes from one run.
  - A shot taken in the very frame a freeze ends, or on a frame whose input carries a press: the held frame keeps the shot frame's
    state, so the pair should be clean, but not checked.

## D1 fix round (n63-b2: the planted cases RED; branch n63-D1fix, one commit `f1d2acd8`, cut from port/stage3 799ec397)
- **The lead's measurement:** the no-op pairs changed 19 / 12 / 7 / 45,686 / 195,235 / 253,849 px. The plant pairs changed 122 / 117 /
  117 / 45,595 / 195,276 / 253,369. b63i1's off frames had no world.
- **The cause (my amplified diff maps of d1noop shots 03 and 05):** edges over the whole frame plus speckle on the dithered surfaces.
  The camera and the viewmodel were drawn one update further on. Some views read another's output before it updates within a frame
  (CameraFeel.Apply reads the viewmodel's punch, which Viewmodel.Render updates after it), so the first held frame is not the shot
  frame's draw.
- **The fix:**
  - Both halves are held frames. Held frame 1 (row as it is) replaces the shot's PNG; held frame 2 (row off) is the off half.
  - A queued crush waits for the next live frame.
  - Every toggle line now also prints `pair changed px` (the planted numbers: noop 0, plant 100), `held on vs the shot's frame`, the
    three frames' mean luma, and `DARK HELD FRAME` when a held frame loses half its luma.
- **b63i1's black off frame:** the cause was not found by reading. I cleared the boss grades, the light clocks, Move54, CameraFeel /
  the FOV kicks, V's springs, G's cue scan, the shaders and the crush. The new diagnostics will say which frame goes dark.
- **For V:** Brawl40View.N63V.cs's off path sets `springsLive63 = false`, so the springs reset after every toggle. That breaks the
  keep-state contract.
- **Checks:** dn-check 0 errors; gates GREEN 23:46:51.

## C1 THE COLOUR TABLE (branch n63-C1, from port/stage3 1fc24f85)
Commits, in order (cherry-pick `619f852d^..<notes sha>`):
1. `619f852d` the red's tools: the audit and the meter
2. `b7807abe` the tests, committed RED
3. `c65a0be7` the table: config numbers and Colours63Math
4. `c4a662e9` the routing behind the row, plus the Route47View hook
5. this notes commit

**The red, measured (read only):**
- `node Tools/night/n63-c1-audit.cjs` gives **392** hue-bearing literal colours in game code not read from the role table. Of those,
  **191** sit outside every role band: world 171, boss 9, ui 9, enemy 1, effect 1. The HUD's are listed, never changed. Planted 19 / 19.
- `python -I Tools/night/n63-c1-meter.py --levels Logs/feel/n62-freeze` gives the off-palette share of the frame (vivid S >= 0.55,
  V >= 0.50, outside the HUD corners, hue in no role band). Planted 14 / 14.

  | Level | Off-palette share | Main source |
  |---|---|---|
  | foundry | **0.498 %** | amber 0.370 (the lava's gold, by its mask) |
  | iceline | **0.458 %** | blue 0.407 |
  | kiln | **0.590 %** | blue 0.555 (the accent floor trims, by its mask) |
  | span | **0.538 %** | lime 0.407 (the accent floor trims) |
  | spire | **0.187 %** | green 0.087, amber 0.077 |

- gg-proxy's targets, found:
  - The danger-red line down spire's exit is **THE CROWN's goal column** (Route47View, crownLook47: red-gold #FF8A3A, drawn through
    the walls).
  - The lime stripes are **colour56's span accent** (#8DD957) on the decor's floor trims.
  - The iceline green is its accent #86D9A2 (S 0.38).
  - The yellow edge line is most likely the kit lamp's #E8C48A (edgeLight props). That is a code reading, not checked on a frame.

**The table** (design.n63.colours63.numbers, written by `node Tools/night/n63-d-rows.cjs`, both copies byte-identical, the row still OFF):
- **Roles:** player / danger / boundary are read from colors.usable / colors.enemy / barrier.boundaryColor. The parry yellow #FFD000
  (D-1) and RIVAL's #FF2BD6 are typed in the table, because config has neither.
- **Bands:** danger 345-35, parry 45-70, player 160-205, rival 285-335; the boundary is pale (S under 0.40) in 230-300.
- **World tier:** an off-band colour at S >= 0.30 and V >= 0.35 keeps its hue and luminance at S 0.25.
- **Routes:** goal47 goes to the boundary; lavaGold goes to danger.

**The routing** (Colours63.N63D.cs; read every frame through `NightRows.Live`, so D1's held off frame restores every own colour):
- It covers the materials' `_BaseColor` and the Lights under the world's roots, LavaView30's `_Gold`, and Route47View's column.
- It never touches enemies, bosses (that is B3's job), the viewmodel, the HUD (the magenta dashes stay) or effects.
- Tests: N63ColoursTests 7 / 7 GREEN outside Unity (RED 0 / 7 on b7807abe).
- Checks: dn-check 0 errors; gates GREEN 00:01:07.

**SPOT pose (rule 8):** `a157` on **n52-span** with `-skygrindSameRun63 n63.colours63`, the room entry shot with the largest same-run
change. A rough upper bound comes from simulating the world tier on the frame's pixels (not a render; it also counts lit wall
pixels that no routed material colours):
- span's verbs37 frames: median 144,106 px, top 826,263
- kiln's a157 shots: about 33k px, under 41,472
- foundry's verbs37 frames: median 47k px
- iceline's verbs37 frames: median 56k px

**The exact Player steps:**
- **Build** (needs f1d2acd8's per-shot `pair changed px` for the quick read): `bash Tools/night/n41-build.sh <tag>`, then
  `node Tools/night/n46-build-rows.cjs <tag> on n63.colours63`.
- **Reel sides, in the background:**
  ```
  bash Tools/night/n63-reels.sh <tag> c1s,n52-span,a157,-skygrindSameRun63,n63.colours63 c1k,n57-kiln,a157,-skygrindSameRun63,n63.colours63 \
    c1vf,n52-foundry,verbs37 c1vi,n52-iceline,verbs37 c1vk,n57-kiln,verbs37 c1vs,n52-span,verbs37 c1vp,n52-spire,verbs37 \
    c1xf,n52-foundry,verbs37,-skygrindNoColours63 c1xi,n52-iceline,verbs37,-skygrindNoColours63 c1xk,n57-kiln,verbs37,-skygrindNoColours63 \
    c1xs,n52-span,verbs37,-skygrindNoColours63 c1xp,n52-spire,verbs37,-skygrindNoColours63
  ```
  The c1v* sides double as verbs37 x5 on this build.
- **SPOT pair:** take the c1s shot with the largest `pair changed px` in `grep "\[same63\] toggle fired" Logs/player/n47-reel-<tag>-c1s.log`,
  or with `n62-gun-yellow.py --pair <c1s/a157/shot> <c1s-off/a157/shot> --rect 0,0,1,1` on each shot. Rule 8 wants >= 41,472. Then
  `node Tools/night/n63-spot-pair.cjs C1 <on> <off> <tag>`.
- **MAIN, a drop of >= 50 % per level, two runs, same build:**
  `python -I Tools/night/n63-c1-meter.py --pair Logs/feel/<tag>/c1vX/verbs37 Logs/feel/<tag>/c1xX/verbs37` for X in f, i, k, s, p.
  The meter's planted cases: `python -I Tools/night/n63-c1-meter.py planted` (GREEN 14 / 14).
- **Proof lines:**
  - `grep "\[colours63\]" Logs/player/n47-reel-<tag>-c1vs.log` gives `colours63 ON` and two `scan` lines, each with the routed count
    per root, the lights, the lava gold, examples, and the off-palette left in other roots.
  - The -skygrindNoColours63 sides print nothing.
- **Audit:** `node Tools/night/n63-c1-audit.cjs --out Logs/n63/c1-audit.txt` (planted: `node Tools/night/n63-c1-audit.cjs planted`).

**Not checked (no Editor, no Player):**
- Whether the world's roots really hold the trims, fixtures and lights. The scan line's per-root counts and "left in other roots" will
  say.
- Whether another system re-sets a routed colour every frame. Lights re-picked by LightKit29's clock would lose their route ("lost").
- Whether the change reaches 41,472 px at the SPOT pose, and whether the drop reaches 50 % per level. Iceline's blue and spire's amber
  may come from lit surfaces rather than material or light colours, and the routing does not move those.
- The muted world against GG's "bland" (2026-10-19): that is the critic's and SPOT's call. The hue is kept per level; only the
  saturation falls to the effects' tier.
- The teal-green grapple rings: GrappleNodes30 reads colors.usable (in band). Their teal is the light or grade on a cyan material, not
  a source colour: not routed.

## D1's residual on n63-b3 (looked at, not fixed)
- What remains: 0-16 px per no-op pair, plant 100-111.
- Where it sits: on the viewmodel gun only (n63-b3 d1noop shot 01: faint shading noise on the gun's top and one edge, max channel delta
  ~40, x 1183-1335, y 676-787). So the gun is drawn a sub-pixel or a lighting step apart between the two held frames.
- Not found cheaply: the viewmodel's own Render at frameDt 0 and its lights are the candidates.

## B3 BOSS COLOURS (branch n63-B3, from port/stage3 5141affb)
Commits, in order (cherry-pick `c5fe14bd^..<notes sha>`):
1. `c5fe14bd` the tests, committed RED
2. `83c1e3da` the parts in config and BossColours63Math
3. `7b9476ce` the wrappers at the boss views' colour sites
4. this notes commit

Tests: N63BossColoursTests RED 0 / 5, then GREEN 5 / 5 outside Unity (29 / 29 with C1's and D1's). dn-check 0 errors. Gates GREEN
00:14:21.

**The red, measured (read only):** `python -I Tools/night/n63-c1-meter.py Logs/feel/n63-b2/b63<boss>/boss63`, 35 frames each, % of the frame.

| Boss | Off-palette | Parry yellow | Cyan | Notes |
|---|---|---|---|---|
| THE WICK | 0.107 | 0.54 | 0.36 | — |
| THE ICEBREAKER | **5.148** (blue 5.06) | — | — | — |
| THE KILN WYRM | **7.779** (green 7.37) | 0.61 | — | — |
| THE LOOM | 0.635 | — | 1.23 | blue, violet, amber |
| THE CROWN | 0.777 (amber 0.62) | 1.09 | 3.02 | — |

From the frames (n63-b2 boss63 sheet) and the code, a weak point is cyan when OPEN on four bosses, but a different colour on each
when closed:

| Boss | Weak point when closed |
|---|---|
| THE WICK | its core, amber #FFA040 |
| THE CROWN | its gems, red-orange #FF5A1F (bone #F2EAD8 with N40) |
| THE LOOM | its knees, pale violet-white #D8D0FF, with amber nails and yellow seams |
| THE KILN WYRM | its punchable head, yellow #FFD23A |
| THE ICEBREAKER | its stuck fists, yellow since N47 |

The yellow ones are not parry tells.

**The call (for DECIDE63, the builder's):** the weak point is the player's cyan on every boss (CLAUDE.md rule 8, DESIGN-RULES V-2: the
thing the player acts on), at the site's own brightness: dim while closed, bright when open. Rejected:
- yellow (D-1 keeps it for the parry tell);
- the boundary's pale violet-white (it means the edge of the arena);
- danger (the weak point would read as the attack).

**The parts** (design.n63.bossColours63.numbers.parts, written by `node Tools/night/n63-d-rows.cjs --row bossColours63`):
- **weak:** wick.core, crown.gems, crown.halo, loom.knees, loom.seams, loom.nails, wyrm.punch, brawl.weak.
- **danger:** wick.edges, crown.clap, crown.edge, crown.flameYellow.
- **Left as they are:** the parry tells keep their yellow (THE WICK's yellow core flash and embers, THE ICEBREAKER's thrown block, THE
  LOOM's yellow spool tell). G's danger column and V's motion hooks are untouched.
- **Cyan that stays:** THE CROWN's blades and rim (the ring the player dashes through) and THE LOOM's live knees (grapple points)
  are player-usable. No boss attack effect is cyan or magenta after the change. That is a code reading, not checked on a frame.

**The routing:** BossColours63.Part(cfg, part, colour) at each site (one wrapper per site) gives the part's role colour from C1's table
at the original's luminance. It reads NightRows.Live every call, so D1's held off frame shows the own colours. The views choose their
colours every frame, so no state is lost.

**SPOT pose (rule 8):** boss63 on **n57-kiln**, `shot-N30-B63-wyrm-stagger.png` (THE KILN WYRM's punchable head, the big pale square).
- Estimate (not a render): about 97,363 px in its pale-green / yellow-green window on n63-b2's frame, so its change to cyan should
  clear 41,472.
- THE WICK's core alone is about 9.7k px, too small for the SPOT.
- The other bosses' shots are the critic's evidence.

**The exact Player steps:**
- **Build:** `bash Tools/night/n41-build.sh <tag>`, then `node Tools/night/n46-build-rows.cjs <tag> on n63.bossColours63`. C1's
  table must be in the build: it is on port/stage3. colours63's own row may stay off; B3 reads only its numbers.
- **Reel sides:**
  ```
  bash Tools/night/n63-reels.sh <tag> b3k,n57-kiln,boss63,-skygrindSameRun63,n63.bossColours63 b3f,n52-foundry,boss63,-skygrindSameRun63,n63.bossColours63 \
    b3i,n52-iceline,boss63,-skygrindSameRun63,n63.bossColours63 b3s,n52-span,boss63,-skygrindSameRun63,n63.bossColours63 \
    b3p,n52-spire,boss63,-skygrindSameRun63,n63.bossColours63
  ```
- **SPOT pair:** `node Tools/night/n63-spot-pair.cjs B3 Logs/feel/<tag>/b3k/boss63/shot-N30-B63-wyrm-stagger.png Logs/feel/<tag>/b3k-off/boss63/shot-N30-B63-wyrm-stagger.png <tag>`.
  Its changed px are in the toggle line (`pair changed px`).
- **The meter per boss, on vs off from the same run:**
  `python -I Tools/night/n63-c1-meter.py --pair Logs/feel/<tag>/b3X/boss63 Logs/feel/<tag>/b3X-off/boss63` for X in k, f, i, s, p.
  The parry-yellow and off-palette shares should drop on THE WICK (idle), THE LOOM, THE WYRM and THE CROWN. THE ICEBREAKER's blue
  armour, frost and arena are not routed.
  Planted cases: `python -I Tools/night/n63-c1-meter.py planted` and N63BossColoursTests.
- **Proof lines:** `grep "\[bossColours63\]" Logs/player/n47-reel-<tag>-b3k.log` gives "bossColours63 ON: 12 parts" and one
  "<part>: <role> #from -> #to" line per part the fight shows (on kiln: wyrm.punch).
- **verbs37 x5** on the build: unchanged (no boss on verbs37's frames).

**Not checked:**
- Whether the wyrm's pale square on the stagger frame is the punchable head this routes (WyrmView's head colour when
  sim.Wyrm30Punchable). If it is another part, the SPOT change is smaller.
- THE ICEBREAKER's blue: its armour plates (Brawl41PlateHex #5F95FF), frost and ice arena are its identity and are not routed, so its
  5 % off-palette stays. Routing them would be a later call.
- THE WICK's core light (a point light built amber at the fight start) is not routed; only its glow material is.
- How the boss grade (BossGrade55) shifts the routed cyan on each boss's frame.

## R1 N1 + N5 and D1's residual (branch n63-R1, from port/stage3 91f998f8)
B3's notes are on branch n63-B3 (04085278). Commits, in order (cherry-pick `a3bc6366^..<notes sha>`):
1. `a3bc6366` the tests, committed RED.
   - N63SwitchTests (Core) is 1 / 2 RED in my scratch runner outside Unity; the passing one pins what must not change.
   - N63R1Tests (Game) needs the Editor. It is RED by construction against the placeholders, not run here.
2. `f720da4e` **N1:** the exit frame's one-time cube and material are made in Build, after OffReason and the level check
   (ExitFrame61Math.MakesAssets), never in Attach. The row's own test, N61ExitFrameTests, is to be re-run in the Editor.
3. `cbcc6ea9` **N5:** `NightRows.HasSwitch`, Rim58.No's rule (that exact switch, case ignored), read by every row's own off switch.
   - Case-sensitive until now: bossRule61, text61, kill55, focus60.
   - Unchanged in meaning (they already ignored case, each with its own loop): exitFrame61, fixture61, gunYellow61, bossMotion63,
     bossTell63, colours63, Rim58.No.
   - Left as they were: `-skygrindBench` and `-skygrindNoHold29` (not rows).
   - When B3 is picked, BossColours63Math.Off still has its own case-ignoring loop. It means the same; one line to switch it.
4. `1a619893` **D1's residual, a hypothesis:**
   - The facts: the 0-16 px no-op and the plant's extra 0-11 px sit on the viewmodel gun, as speckle on its surface plus one edge.
   - The guess: an SSAO material the snapshot never saw, because Find stops looking once it has one; for example the overlay camera's
     renderer's.
   - The change: the snapshot rescans every SSAO material, and the toggle line prints `ssao materials <n>, noise replays <m>`.
   - The check: a d1noop / d1plant on a build with this should read `pair changed px` 0 / 100. If not, the cause is elsewhere in the
     overlay camera.
5. this notes commit.

Checks: dn-check 0 errors after each; gates GREEN 00:30:48 (N5) and 00:34:13 (the residual). Not run: the Editor (N61ExitFrameTests,
N61BossRuleTests, N61TextTests, N61FixtureTests, N61GunYellowTests, N63R1Tests) and a Player.

## C1 fix round (branch n63-C1b, from port/stage3 2a5a1345)
b4's MAIN was 2 of 5 (the lead's numbers):

| Level | Off | On | Drop |
|---|---|---|---|
| foundry | 0.496 % | 0.447 % | 10.0 % |
| iceline | 0.460 % | 0.398 % | 13.5 % |
| kiln | 0.589 % | 0.048 % | 91.9 % |
| span | 0.538 % | 0.105 % | 80.5 % |
| spire | 0.188 % | 0.131 % | 30.3 % |

**The read (read only, n63-b4 c1v? on-frames, the meter's mask):**
- **Foundry:** 83 % of what is left is amber (0.371 of 0.447 %; mean RGB 166 126 38, hue 43, S 0.86): **the lava**.
  - The lava's gold was routed to danger at the gold's luminance: #FF8231 at HDR ×1.8, i.e. (1.8, 0.92, 0.34). The screen clips that
    to (1, 0.92, 0.34), amber-yellow again.
  - **Fixed at the source:** a route keeps the luminance unless that passes the original's brightest channel; then it keeps the
    brightest channel. The gold becomes (1, 0.29, 0.11).
  - Estimate: foundry falls near 0.08 %, a drop of about 85 %. Not measured.
- **Iceline:** blue (66 102 141, hue 211, S 0.63), the same on and off, in the middle band of the frame: **the azure-lit walls**.
- **Spire:** teal (66 150 122, hue 158), under the cyan exit door: **the door's light on the olive floor**.
- Neither iceline's nor spire's remainder is a material or light colour in the world's roots. The scans list nothing off-palette left
  there; what is left in other roots is the viewmodel (Main Camera 7), Spitters 54, NIGHT32 CROWN 1 and Impact rings 8. They are lit
  surfaces. Moving them means re-lighting the levels:
  - desaturating the key light is the "bland" risk;
  - giving it a role's hue gives the level a false meaning.
  - Not done. **C1's MAIN can reach 3 of 5 at best:** kiln, span, and foundry if the estimate holds.

**Commits:**
- `4134e403` the tests, committed RED. This is a **spec change, said plainly**: a new test, plus the two luminance assertions it
  changes (C1's gold, B3's routed parts). They pinned the overflow and are replaced by the rule, not loosened. 3 RED.
- `8c7ad1b7` the fix, 32 / 32 GREEN. The same rule fixes B3's danger routes (THE WICK's amber edges, THE CROWN's gold clap and edge
  overflowed the same way). The weak routes to cyan and the world tier are unchanged.
- this notes commit.

**Checks:** dn-check 0 errors; gates GREEN 00:44:02.

**Player steps:** rebuild with colours63 on, then c1vf against c1xf on n52-foundry verbs37, then
`python -I Tools/night/n63-c1-meter.py --pair Logs/feel/<tag>/c1vf/verbs37 Logs/feel/<tag>/c1xf/verbs37`. On a boss63 frame of THE
WICK or THE CROWN with bossColours63 on, the danger routes should read red-orange, not amber.

## B3b: B3 made visible on the wyrm (branch n63-B3b, from port/stage3 de6bcd38)
The lead: on n63-b5's b3k the wyrm's same-run pairs changed 0 / 0 / 0 / 6 px. B3's notes named the wrong thing as the SPOT pose.

**The read (read only): n63-b5's same-run pairs per boss.** Pixels changed per pair (`pair changed px` in each toggle line), the
proof lines, and what the frames show.

| Boss (reel) | idle | flinch | windup | stagger | Routed on the run |
|---|---|---|---|---|---|
| THE CROWN (b3p) | **184,073** | 5,048 | 5,468 | 5,837 | crown.gems, halo, clap, edge, flameYellow |
| THE LOOM (b3s) | 16 | 10,095 | 12,447 | 40,419 | loom.knees |
| THE WICK (b3f) | 17,751 | 16,499 | 2,890 | 0 | wick.core, wick.edges |
| THE ICEBREAKER (b3i) | 0 | 0 | 0 | 0 | brawl.weak |
| THE KILN WYRM (b3k) | 0 | 0 | 0 | 6 | wyrm.punch (after the four shots) |

- **THE CROWN, idle:** its gem ring goes from bone white (232 234 220) to cyan (17 229 223) and its flame's yellow to red-orange.
  That pair already clears 41,472. Its reel ended RED for "1 damage events without a drawn flinch", which is bossMotion63's line, not
  B3's.
- **THE LOOM:** its knees, dim violet to dim teal. The stagger pair is 1,053 px short.
- **THE WICK:** its core, amber (164 151 27) to cyan (32 154 158).
- **THE ICEBREAKER:** nothing to route on its shots.
  - Its weak point on the stagger shot ("FISTS STUCK: HIT IT NOW") is already cyan, on and off. The Stuck case sets fistRC to NIGHT40's
    cyan (Brawl40View.cs:306) before Render47 replaces cyanC.
  - brawl.weak recolours only the Window state's rims.
  - So 0 px is the right answer there.
- **THE KILN WYRM:** the pale-green square on its shots is the **head seen end-on** (3 x 3 m: about 290 px at 13.2 m on the stagger
  shot).
  - It is drawn in dark ash with jaw heat under BossMarks32's faint cyan-white shell, which reads pale green after the grade (181 222
    123).
  - The one routed part, the punch window's head, is on no shot: its first proof line comes after shot 4.
  - B3's notes called the square "the punchable head". That was wrong; its "not checked" line doubted it.

**The change** (`8183ce93` tests RED, `a871e8fd` GREEN):
- **wyrm.open (weak):** while the wyrm is **OPEN** (Wyrm31 staggered, or its core exposed: the HUD's "OPEN", "STAGGERED: ... SLUG IT";
  the head takes x2 and a staggered wyrm full damage), the head is lit as the punch window's head (Wyrm47PunchHex x Wyrm47PunchGain)
  through the weak role: cyan x2.2.
  - A new wrapper, BossColours63.Lit(cfg, part, off, lit): with the row off (or the part not in the table) it gives today's head,
    unchanged.
  - This is a new drawn state (the open head lit), not only a recolour. Today nothing on the open wyrm says "shoot here" but the
    faint shell.
- **wyrm.core (weak):** the exposed core (x3), red-orange today on the thing you shoot, at its own brightness. The roar tell is not
  routed.
- **wyrm.seams, wyrm.spit (danger):**
  - The amber seams (#FF9A28, hue 36, just past danger's 35). G's BossTell63Late pulls them to danger only in a listed wind-up.
  - The spit and the shards.
- **Numbers:** `node Tools/night/n63-d-rows.cjs --row bossColours63 --replace` writes only that object. Both copies are byte-identical;
  the row is still OFF.

**Checks:** N63BossColoursTests was RED (4 of 7 failed), then GREEN 7 of 7 outside Unity (17 of 17 with C1's and the switch tests). The
hook test reads WyrmView.cs's text. dn-check 0 errors at each commit; gates GREEN 01:12:50.

**SPOT pose (rule 8):**
- **Measured on b5:** `shot-N30-B63-crown37-idle.png` on b3p (n52-spire boss63), 184,073 px on the same-run pair.
- **Expected for the wyrm with B3b, not checked:** `shot-N30-B63-wyrm-stagger.png` on b3k. The head's square (about 290 x 290 px) goes
  from pale green to lit cyan. That is about 84k px if the shell lets the fill show through.

**The exact Player steps:**
1. Build with bossColours63 on, as for b5.
2. Run `bash Tools/night/n63-reels.sh <tag> b3k,n57-kiln,boss63,-skygrindSameRun63,n63.bossColours63`. Add the other four sides to
   re-measure them.
3. Read the toggle lines: `grep "toggle fired" Logs/player/n47-reel-<tag>-b3k.log`. The stagger shot's `pair changed px` should be
   41,472 or more.
4. Read the proof lines: `grep "\[bossColours63\]" ...b3k.log` should show
   `wyrm.open: weak #FFD23A x2.2 -> #1CEAFF x2.2` by the stagger shot.

**Not checked:**
- Whether the reel's stagger shot falls inside Wyrm31's staggered window ("stagger yes (staggered)" says so on b5) with the head
  above the floor.
- How much of the head's fill shows through BossMarks32's shell.
- The core, seams and spit on any shot: the windup shot is the aggro charge, and the spit came later in the run.
- The Editor run of N63BossColoursTests.
