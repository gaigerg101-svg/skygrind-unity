# Scout: bosses, colour, camera and combat feel (FORGE71, read-only Explore agent, 2026-10-26 ~09:45; greps, ls, git log only)

No file below changed since u3-n64 (`git log u3-n64..HEAD` on them is empty), so the n64 numbers are the latest. Every row is mirrored in
Assets/StreamingAssets/Design/unity-config.json. **A new view row must be added to `SameRun63Math.N63D.cs:28` (FrameRows) before
`-skygrindSameRun63 nNN.row` pairs work on it.**

## T-155 the boss text: no single arbiter
- **Centre:** hud47's `fight47` (Game/UiHud47.Boss.cs). One queue, `Fight47Post` :56-75: 2 boss hint, 1 slot line, 0 teach line.
  bossRule61 takes the slot at the boss's rise (:136; UiBossRule61.cs:46-79). Its how-to lines are in config :9806-9822
  (n61.bossRule61, ON).
- **Bar hint:** `b43Hint` (UiBossBar43.cs:207-215) also posts to the fight line. The text comes from the sim
  (SurvivalSim.N43.Bosses.cs:64/81/99) out of design.n43.wick43.bosses (config :6790-6840); e.g. wyrmIdle "PUNCH THE HEAD TO STAGGER IT"
  has no {punch} glyph.
- **THE CROWN's world label:** `call40` ("OPEN: SHOOT IT" / "BLOCKED"), Crown37View.Crown40.cs:127-135, on its own overlay canvas
  (:32-43). **No arbiter sees it**: this is the two-orders frame (bp/crown37-stagger).
- **Partial arbiters:** order64 (Order64.N64G.cs:60-71, UiOrder64.N64G.cs:24-55, ON) hides the bar hint only when its text equals the
  fight line's. fightLine64 (ON) keeps room titles out.
- **Weapon awareness:** none of the text code knows the held weapon. `ResolveLesson25` (UiLesson25.cs:87) resolves both the bar hint
  and the lessons, so it is the place for a weapon filter. The wyrm is staggered by a fist punch with any weapon
  (SurvivalSim.N48.Brawl.cs:7,124-146); that also settles DECIDE63 D-4 (wording).
- **Meters:**
  - `bash Tools/night/n64-reels.sh n64-b1 uk,n57-kiln,boss63,-skygrindSameRun63,n64.order64`, red twin
    `ukoff,n57-kiln,boss63,-skygrindNoOrder64`.
  - `node Tools/night/n64-u1-dup.cjs`: both copies drawn uk 0 / ukoff 391 frames (Logs/n64/u1-dup-b1.txt).
  - `node Tools/night/n64-l2-titles.cjs`.
  - **No meter covers call40.**

## T-153 one colour language
- **Boss colours:** BossColours63.N63D.cs (wrappers :69-136) and BossColours63Math.N63D.cs, whose rule is **weak point = the player's
  cyan on purpose** (:4, :49). The parts table is config :10022 (ON). Older cyan: the crown's idle ring (Crown37View.Crown40.cs:3-6), the
  window (config :6804), THE LOOM's knee. So "bosses wear cyan" is GG's open call (DECIDE64.md:19-20), not a bug.
- **The magenta dashes:** the PULSE pips (UiHud47.cs:439-500, colour UnityTuning.F47.cs:76 0xff6fb0, hue ~333; unlit at 0.3 alpha). GG:
  "the UI looks good as it is, so don't touch that" (DECIDE64.md:21). **Not tonight.**
- **Meters:**
  - `python -I Tools/night/n63-c1-meter.py --pair ...`: masks the HUD corners.
  - `python -I Tools/night/n63-b3-roles.py --boss <s>-on/boss63 <s>-off/boss63 <s> --hue ...`: its check (d) REQUIRES >= 300 cyan px on
    a weak point.

## T-154 the wyrm's motion, T-152 the camera's chop
- **The wyrm:** BossMotion63.Layer.cs, hooked at WyrmView.N63V.cs:1-8. It has the tightest guard, because the drawn boxes are the
  hitboxes. Config :9980-9992: gain 0.6, follow 0, limitShare 0.25, limitDeg 14.
  - The boss63 reel frames it at 29.4 m (FeelReel.N63V.cs:110,172,254).
  - Command: `bash Tools/night/n63-reels.sh n63-b10 b1k,n57-kiln,boss63,-skygrindSameRun63,n63.bossMotion63`.
  - Last pair: stagger 16,649 px against the 41,472 bar; idle 389 (Logs/n63/verdict-B1.md:69,73).
- **Frame pacing:**
  - Command: `bash Tools/survival-bench.sh 60 Builds/n64-rc2/SKYGRIND.exe Logs/player/bench-n64-rc2.log play`.
  - Result: p99 9.03, p99.9 12.60, max 22.48 ms, 4.48 % of frames over 6.94 ms, vsync 0. In the worst 1 %, engine time is 8.76 of 10.57
    ms (bench-n64-rc2.log:204-215).
  - VSYNC is a player setting, off by default (GameSettings.cs:41,109,251). **No -skygrindVsync switch.** `-skygrindProfile <frames> f.raw`
    needs a Development Player (PlayerChecks.cs:23,193-195).
- **eye63** (OFF; the A/B kit Builds/n64-abx / n64-aby) waits for GG's X / Y. **Keep Logs/n64/eye63-ab-chop.txt (its line 1 names
  on / off) away from the play card.**

## Combat feel
| item | where | sim or render | measure / last |
|---|---|---|---|
| T-77 feel46 kicks | Feel46View.cs:8-9,56-61; config :7091 (kicks false :7095) | render | a546 reel + n46-blind-pairs.cjs; last "same" (THREADS:18) |
| T-86 guns48 / blast48 | Fx48View.Guns.cs:1-28, Fx48View.Blast.cs:49,83; config :8100, :8109 (OFF) | render | a748 (`-skygrindMap n47-iceline -skygrindReelOnly a748`); blast48's lance won 2-1, never ON; guns48: frames with the row on never taken |
| T-105 kill wash | polish53.numbers.killWash.on 0 (config :8353-8365) | render | n53-reels.sh <tag> u (a153); +0.275-0.315 vs +0.25 |
| T-78 swap raise | Core/Combat.cs:1130-1145; weapons.swapTime 0.25 (PortExport) | **sim, golden-pinned: not tonight** | V1.md:44,150 |
| T-78 press inside a freeze | the loop runs no step while frozen (SurvivalGame.cs:316-339, SurvivalGame.Impact.cs:35-60, ImpactFrames.cs:99,146; rules in Core ImpactRules.cs) | Game-side pacing; the tape is unaffected; medium risk (high if ImpactRules changes) | kill 10, gauntlet 19-29, FUSION 26, parry 30-36 steps (V1.md:50-51) |

Presses are **delayed, not dropped**: latched and consumed by the next step (CombatReader.cs:130-133,175-182). Release edges are not
latched, so whether a KESTREL tap pressed and released inside a freeze still fires is **not checked**.

## How GG reaches a level in a Player
TITLE -> PLAY (mode CAMPAIGN, the default) opens the level select (UiLevelSelect26.cs:189-193). Up / down to a slot, then Enter / A /
click. All five slots start, and `sequential` is false (config :1649). Reels boot a level with `-skygrindMap <id>`.
`-skygrindLevelSelect29` is a self-test that quits, not a shortcut.

## The scout's ranked items (minutes are the scout's estimates; the slate re-sizes them)
1. One voice on THE CROWN (hide call40 while the fight line or bar draws the window order): Voice71.cs, `n71.voice71`; proof
   `[voice71] crown both drawn N frames` 0 on vs > 0 off in a boss63 n52-spire same-run pair. 45 min.
2. Words that match the hand (ResolveLesson25: the wyrm's lines get {punch}; "swing" lessons held while the gauntlet is not held):
   `n71.words71`; proof: lines naming a weapon not held, 0 on vs > 0 off over boss63 kiln + verbs37 x5 (27/29/31/31/31 kept). 75 min.
3. feel46 kicks bigger and rarer (T-77): a546 + 3-judge blind pairs. 45 min.
4. guns48 frames, then decide (T-86). 45 min.
5. A wyrm motion that reads (gain / limitShare / limitDeg up; the boss63 stand at 12-15 m): b1k stagger > 41,472 px, idle > 3,000. 45 min.
6. blast48 ON (the lance; a748 pairs; verbs37 x5). 30 min.
7. A VSYNC bench arm (-skygrindVsync beside PlayerChecks.cs:325): bench `vsync 1`, p99 / max vs 9.03 / 22.48 ms. 30 min.
8. A press ends the freeze (ImpactBeginFrame: a fresh latch ends the freeze after a minimum hold; Core/ImpactRules.cs untouched):
   `n71.press71`; `[press71] waited p50/max steps` <= 6 (from 10-36), fingerprints EQUAL. 90 min, medium.
9. Boss cyan only while open: **needs GG's DECIDE64 answer**; B3's meter check (d) would need re-stating first. 60 min.
10. Kill wash (T-105): low odds (never GREEN in three tries). 60+ min.
11. The pip hue: **blocked by GG's "don't touch the UI".**
