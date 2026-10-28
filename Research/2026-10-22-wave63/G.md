# NIGHT63 builder G: notes

Branch `n63-P2` (from port/stage3 1ee56384, NIGHT63 A1). Builder G never ran the Editor, a build, a Player or a reel.

## P2: the boss rule line on time (design.n61.bossRule61, OFF; slate P2)

Shas on n63-P2, in order: `688ced75` (test-first, RED), `36861917` (the fix, GREEN), then this notes commit. Pick: `git cherry-pick
688ced75^..<notes sha>`. dn-check 0 errors on both code commits (scratch deleted after each). `bash Tools/night/n63-gates.sh --no-editor`: all
GREEN 22:58:10 (config identical; golden 0 failing, 0 mismatches, 22 replays exact; fingerprints plain and --off57..--off63 EQUAL).

### The red (measured before any change, read only)
- `Logs/feel/n62-p0/summary47.txt` line 372 (b61k, n57-kiln, boss61): `RED boss61 wyrm | ... at 1.000 s, 1920x1080: shown "", alpha 0.00 ...
  | shows in 8 s 1, visible 4.75 s (from 2.45 s), planned 4.77 s ... BAD: no line at 1.0 s (alpha 0.00)`.
- The same run's frames, read from `Logs/feel/n62-p0/b61k/boss61/telemetry.jsonl` with a scratch script (b61tel.cjs: per frame, the
  "Boss card 38", "Boss bar 43" and "Fight line 47" texts and alphas):
  - t 0.317 s: the name card "KILN WYRM" flickers in (the rise);
  - t 0.767 s to 1.800 s: the card's own subtitle "SHOOT ITS HEAD. THE OPEN CORE HITS X3" (entrance41's line) under the name;
  - t 2.317 s: bossbar43 first draws (2.00 s after the rise);
  - t 2.750 s: the rule line "Swing the gauntlet LEFT CLICK at its charging head." comes in (2.43 s after the rise), full alpha at t 2.867 s,
    gone at t 7.52 s; the bar's hint "PUNCH THE HEAD TO STAGGER IT" follows it in the fight line.
- The frame `Logs/feel/n62-p0/b61k/boss61/shot-N30-B61-wyrm-1000.png`: the card (KILN WYRM + the entrance41 subtitle), bars41's thin bar at the
  top (not bossbar43), no rule line.

### The cause (two, both in the view)
1. UiBossRule61.cs began the line only with the boss bar up (`if (!barUp) ... return false`), and THE KILN WYRM has no bossbar43 through its
   2.0 s emerge: `Wyrm.cs` SetPhase("intro") at its rise, and `SurvivalSim.N43.Bosses.cs` fills no bar while the phase is "intro".
2. On a session's first boss the fight line itself (`fight47`) was built by that bar's lazy build (`UiBossBar43.cs` Bar43Frame ->
   `UiHud47.Boss.cs` Bar47Build), and Fight47Tick returns while `fight47 == null`, so the wait did not even age until the bar came up; then
   BossRule61DelayS (0.45 s) more: 2.0 + 0.45 = 2.45 s.

### The fix (view only; no sim, config or Core change)
- `BossRule61.cs`: the line's clock is pure now (`BossRule61.Clock`, `Arm`, `Tick`, `Beat`): in BossRule61DelayS (0.45 s) after the rise
  **whether or not the boss bar is up**; the bar only ends it, when it goes down after it was up since the rise (the fight over). An armed line
  is never dropped any more (BossRule61ArmS removed; BossRule61Probe.Dropped stays, always 0, because the boss61 reel prints it).
- `UiBossRule61.cs`: drives that clock; the arm also builds the fight line if no bar has (`Fight47Build()`); while the line holds the place it
  hides the name card's own subtitle (`BossRule61Sub`, after RenderCard38 drew it, as hud47's Fight47Late already does once the bar is up), so
  one rule stands under the name, not two (NIGHT62's 1.0 s frame had the card's "SHOOT ITS HEAD ..." where the rule line now stands). The
  proof line adds `| in <s> s after the rise, boss bar up / not up yet, name card up / down`.
- `UiHud47.Boss.cs`: Bar47Build's fight-line half moved, unchanged, into `Fight47Build()` (idempotent); Bar47Build calls it.
- `UnityTuning.N61G.cs`: BossRule61DelayS's comment (counted from the rise, bar or not); BossRule61ArmS removed (no reader left).
- Off (the row off, -skygrindOff61 or older, -skygrindNoBossRule61): BossRule61Arm returns before arming or building, so nothing changes.
- The boss61 reel (`FeelReel.N61G.cs`) is untouched.

### Tests
- `Assets/Tests/EditMode/Game/N63BossRuleTimeTests.cs` (6 tests): the wyrm's case (bar up 2.0 s after the rise) at 30 / 60 / 144 fps, the
  bar from the rise, a bar never up, a bar that goes down, a new rise, the constants (DelayS + InS <= ShotS).
- Run by me OUTSIDE Unity on the dn-check scratch build (a scratch net10 runner that loads the built DLLs and invokes each [Test]; not the
  Unity test runner): RED commit 4 passed, 2 failed (`TheWyrm_BarAfterItsEmerge...: began at 2 s`, `BarNeverUp...: begins once, but was 0`);
  GREEN commit 6 / 6. N61BossRuleTests 4 / 4 on the GREEN build (with Unity's managed DLLs resolvable). N61BossRuleRowTests could not run
  outside Unity (Port.Root is an engine call): not run; its config is unchanged.

### Player steps for the lead
1. Build with the row on: `node Tools/night/n46-build-rows.cjs <tag> on n61.bossRule61`, then the build.
2. Reels (one Player at a time): `bash Tools/night/n63-reels.sh <tag> b61f,n52-foundry,boss61 b61i,n52-iceline,boss61 b61k,n57-kiln,boss61
   b61s,n52-span,boss61 b61p,n52-spire,boss61 b61koff,n57-kiln,boss61,-skygrindNoBossRule61`.
3. Grep `Logs/feel/<tag>/summary47.txt` for `[reel] boss61:`. Want: `GREEN boss61 wick`, `brawl40`, `wyrm`, `loom37`, `crown37`, each with
   `at 1.0xx s, 1920x1080: shown "<the line>", alpha 1.00, em 40.0 px` (>= 36), `at <~1.2-2> s, 1280x720: ... alpha 1.00, em 26.7 px`
   (>= 24), `shows in 8 s 1`, `visible 4.7x s (from 0.4x s)`; the off side `OFF boss61 wyrm ... shows in 8 s 0`.
4. Grep each side's Player log (`Logs/player/n47-reel-<tag>-<side>.log`) for `[boss61] line:`; want one per side, ending `| in 0.4x s after the
   rise, boss bar not up yet, name card up` on n57-kiln (the wyrm) and `boss bar up, name card up` on the other four (if a boss says "not up
   yet", read that side's 1.0 s frame for a second line under the reticle: see "not checked"). `[boss61] cut after` should not appear within
   the line's first 5 s on any side.
5. SPOT pose (P2 is exempt from the 2 % rule): boss61 n57-kiln `shot-N30-B61-wyrm-1000`, 1.0 s after the rise: the name card "KILN WYRM"
   with the rule line "Swing the gauntlet <LEFT CLICK glyph> at its charging head." directly under it, in the fight line's slot (250-302 px
   from the top at 1080p), where NIGHT62's frame showed the card's own subtitle. D1's same-run OFF (the line's UI root `UI/HUD 33/Fight line
   47` hidden) leaves the name card with no line under it (the row stays on in both captures, so the card's subtitle stays hidden).
6. verbs37 x5 on the same build (the slate's guard).

### Not checked
- Nothing in a Player or the Editor: the line's real timing on any boss, the frames, the proof line, the em at 720p after the change.
- The other four bosses' bar timing: by the code (Wick43Fill from Reveal; Brawl40, THE LOOM and THE CROWN when their state is not Off) their
  bossbar43 is up from the rise, so hud47 already takes their BossLine40 rule lines into the fight queue behind this line; not measured. If one
  is not up at the rise, its BossLine40 line could show under the reticle at the same time as this line.
- Building the fight line inside the bossRise event handler (in the sim step, not the HUD's render pass): plain uGUI / TMP creation on the main
  thread; not run.
- TEXT SIZE above 1: Fight47Build registers the label as before (sized47 without an immediate rescale: NIGHT47's behaviour, unchanged).

## B2, step 1: the cue measurement (slate B2 doneWhen 1, "before"; design.n63.bossTell63 stays OFF; no view, row or sim change)

Branch `n63-B2` from port/stage3 fdd3acc7 (V's BossAttackClock63 and boss63 picked). Shas in order: `58e7edc2` (test-first, RED),
`bf858d0a` (the measure, GREEN), `1f30bb1e` (the scan on its cadence only), then this notes commit. Pick: `git cherry-pick 58e7edc2^..<notes
sha>`. V's files are not edited: the measure fills V's two partials from my own file. dn-check 0 errors on each code commit (scratch deleted
after each); `bash Tools/night/n63-gates.sh --no-editor` all GREEN 23:20:04 on bf858d0a (config identical; golden 0 failing, 0 mismatches,
22 replays exact; fingerprints plain and --off57..--off63 EQUAL); 1f30bb1e touches only FeelReel.N63G.cs, which no gate compiles.

### The red
The slate's B2 red is "measured first on V's boss63 reel", so it does not exist until this measure runs in a Player: before this commit
boss63 printed no cue line (`grep -c "\[boss63\] cue"` = 0 by construction). The numbers come from b1.

### What it measures (Game/BossCue63.cs pure, Game/FeelReel.N63G.cs the reel's half, UnityTuning.N63G.cs the thresholds)
- **What counts as drawn:** on the hold's first live frame (the reveal, before the reel's taps), the transform roots of the drawn renderers
  within 3 m of the boss's centre or weak point are taken as the boss's own; from then on, every 30 frames, the renderers (meshes, skinned
  meshes, lines; no particles or trails) and lights under those roots within 30 m of the boss and more than 3 m from the eye (inactive ones
  too, so a pooled telegraph is known before it turns on). Every frame each one's drawn state (enabled, active, `isVisible`; a light enabled
  with intensity > 0) and its colour as the view set it: a renderer's MaterialPropertyBlock `_BaseColor` / `_EmissionColor` / `_Color` (the
  brightest), else its material's; a line's start colour; a light's colour x intensity. **The method is the drawn object's colour, not
  screen pixels** (said in the end line).
- **Per attack** (BossAttackClock63's wind-up first step W and strike step H, ms from the rise on the fixed clock, V's reckoning): each
  object's reference = its highest level (the colour's max channel) per hue class (danger 345-30 deg, parry 38-62 deg, pale under 0.5
  saturation, other) over [W - 250 ms, W - 17 ms); a riser = an object drawn in a class above its reference there by max(0.15, 0.3 x the
  reference). **start** = the first frame with any riser; **hue / sat** = the strongest riser's at the summed rise's peak; **parry** = a riser
  in the parry band; **tell** = the first danger- or parry-class riser (B2's target, "a danger-hue cue >= 300 ms before its damage frame");
  **kind** = the sim's tell colour (a Boss37Brain attack's kind; THE ICEBREAKER's block yellow, its others red; the wyrm "?").
- **Lines** (Player.log and `<side>/boss63-<boss>.txt`, beside V's):
  - `[boss63] cue roots <boss>: <root names> | at <ms> ms, phase <phase>` (once: what was measured)
  - `[boss63] cue <boss> <attack> start <ms|none> hit <ms> lead <ms> hue <deg|-> sat <s|-> parry <yes|no> danger <yes|no> tell <ms|none> kind
    <yellow|red|?> objects <n> obj <the strongest riser's parent/name>` (one per strike whose wind-up began in the hold)
  - `[boss63] cues <boss> attacks <n> found <f> | under 300 ms <k> (ids) | off the danger hue <m> (ids) | parry drawn <p> of <y> yellow |
    method ...` (the fix list: short = no tell or a tell lead under 300 ms; off the danger hue = a cue whose strongest riser is not in the
    danger band, a parryable attack's yellow not counted)

### Tests
N63BossCueTests (8): the hue classes on the palette's own hexes (0xFF3A14 9.7 deg and 0xFF5A1F danger, 0xFFD23C parry, 0x36E0FF other,
0xD8D0FF pale), a cue from 600 ms (lead, danger), a lamp pulsing before the wind-up is no cue, a parry tell, a 200 ms cue and no cue are
short, a brightening object found at its threshold, an early cyan riser does not hide a late red tell (added with the code), the lines.
Run outside Unity on the dn-check scratch build (a scratch runner, not the Unity test runner): RED 0 / 7 on 58e7edc2, GREEN 8 / 8 on
bf858d0a. V's N63BossClockTests on the same build: 5 / 6 (the sixth calls the engine and cannot run outside Unity: not a result).

### Player steps for the lead (b1)
1. The build with V's and my picks; bossTell63 OFF (nothing reads it yet). bossMotion63 on or off does not change these lines' method.
2. V's sides: `bash Tools/night/n63-reels.sh <tag> b63f,n52-foundry,boss63 b63i,n52-iceline,boss63 b63k,n57-kiln,boss63 b63s,n52-span,boss63
   b63p,n52-spire,boss63`.
3. `grep -h "^\[boss63\] cue" Logs/player/n47-reel-<tag>-b63*.log` (or `Logs/feel/<tag>/b63*/boss63-*.txt`). Read first the `cue roots` line
   per boss (it must name the boss's view root, not "none" and not a player FX root such as "Weapon FX" / "Impact rings"), then each `cue` line,
   then the `cues` line: its two lists are B2's fix list.

### Not checked / limits (said so B2's fixes are judged on what the measure can see)
- Everything in a Player: no number yet.
- Screen pixels: a colour set on a renderer whose shader does not read that property (L-096: vertex colours), the post stack and the PS2
  pass are not seen; particles and trails are not read; `isVisible` counts any camera (shadows included); values may be one frame late (the
  views draw in the game's frame, the reel's coroutine reads after it).
- The roots: if a boss's parts were parented at the scene root one by one, a telegraph away from its core would not be measured (the cue
  reads late or none: an over-flag, never an under-flag); the `cue roots` line shows what was used.
- Attacks whose wind-up began before the hold are said ("not measured") and counted in the end line.

## P2b: one instruction at the fight's start (the P2 critic's problem 5; row design.n61.bossRule61, ON since 65bc7cce)

Branch `n63-P2b` from port/stage3 799ec397. Shas in order: `bce1cc1a` (test-first, RED), `5ed17345` (the fix, GREEN), then this notes
commit. Pick: `git cherry-pick bce1cc1a^..<notes sha>`.

- **The red** (Logs/n63/verdict-P2.md 6c; the frame Logs/feel/n63-b1/b61i/boss61/shot-N30-B61-brawl40-1000.png, read): THE ICEBREAKER's
  1.0 s frame shows the bar's "PARRY THE BLOCK. PUNISH STUCK FISTS" above the rule line "Get close and swing the gauntlet LEFT CLICK.": two
  instructions at once. The b1 telemetry names the element: `UI/HUD 33/Boss bar 43/Hint` (config design.n43.wick43.bosses.brawlIdle).
- **The cause:** hud47's Bar47Build hides that hint when the bar is built ("the hint is the fight line now"), but `Text33.Set` shows its
  holder again on every new text (UiStyle33.cs:78) and Bar43Draw sets it on each hint change, so the bar's hint has been drawn since NIGHT47
  (NIGHT62's b61k telemetry shows "PUNCH THE HEAD TO STAGGER IT" on the wyrm's bar too). The P2 fix hid the name card's subtitle, not this.
- **The fix:** `BossRule61.HintShown(lineHolds, hint)`: not drawn while the rule line holds the place; else when it has text (Text33.Set's own
  rule, so the bar is unchanged outside the hold). `UiBossRule61.cs`: BossRule61Sub hides it every frame the line holds (after Bar43Draw set
  it that frame); BossRule61HintRelease shows it again at the line's end, a cut or a new run (a held hint is never left hidden: Text33.Set
  skips an unchanged text). Proof lines: `[boss61] bar hint held: "<hint>" (the rule line holds the place)` and `[boss61] bar hint back:
  "<hint>"`. Off: nothing is armed, the bar is NIGHT62's. The boss61 reel is untouched.
- **Tests:** N63BossRuleHintTests (2): held while the line holds whatever its text; outside the hold drawn exactly when it has text. Outside
  Unity on the scratch build: RED 1 / 2 on bce1cc1a (WhileTheRuleLineHolds: True), GREEN 2 / 2 on 5ed17345; N63BossRuleTimeTests 6 / 6 and
  N61BossRuleTests 4 / 4 on the same build. dn-check 0 errors on both code commits.
- **Player steps:** the next build with the pick; `bash Tools/night/n63-reels.sh <tag> b61i,n52-iceline,boss61 b61k,n57-kiln,boss61` (the
  icebreaker's bar is up at the rise; the wyrm's comes up at 2.0 s during the line). Want: boss61 GREEN as before; the 1.0 s frame
  `shot-N30-B61-brawl40-1000.png` with the bar's name, LEG and STAGGER but no "PARRY THE BLOCK..." under the bar; in the Player log one `bar
  hint held` and one `bar hint back` per side (the wyrm's held at ~2.0 s when its bar first draws the hint).
- **Not checked:** a Player; whether hud52's top plate or another layer draws the same hint elsewhere (the b1 telemetry shows only the bar's).
- **For the report (not fixed, logged for GG in DECIDE63 D-4 by the lead):** the critic's problems 3 / 4: the wyrm's rule line names the
  gauntlet while boss61's player holds the THUNDERCLAP (verbs37 on n57-kiln: owned [shotgun]); its old subtitle said "SHOOT ITS HEAD", the
  rule line says to swing the gauntlet at its charging head (both true of the sim: the line's _why cites SurvivalSim.Boss30.cs:282-310).

## B2, step 2: the meter fixed, then the fix (design.n63.bossTell63, OFF; branch n63-B2fix from port/stage3 f398cea3)

Shas in order: `c984ddc9` (meter fix test-first, RED), `b12acbb0` (meter fix, GREEN; gates GREEN 23:54:59), `811bce3b` (the fix test-first,
RED), `6a490fa1` (the fix, GREEN; dn-check 0; `bash Tools/night/n63-gates.sh --no-editor` all GREEN 00:00:05: config identical, golden 0
failing, 0 mismatches, 22 replays exact, fingerprints plain and --off57..--off63 EQUAL), then this notes commit. Pick: `git cherry-pick
c984ddc9^..<notes sha>`.

### The red (n63-b2's boss63 off sides, bossMotion63 off: `grep -h "^\[boss63\] cue" Logs/player/n47-reel-n63-b2-b63*off.log`, read)
- 16 attacks, every one `start none`. **That was my meter's bug, not the bosses:** Unity's instance ids of run-time objects are negative and
  BossCue63 took a negative strongest id for "no riser", so every riser was dropped as the strongest while its `tell` column saw it: wick
  wickSlam x2 tell 1100 ms (kind red), wickEmber 1300 (yellow, parry yes); loom37 spool x2 1325 (yellow, parry yes), lash 1325, cage 1625 (red);
  brawl40 brawlLeap 1008 (red). Each tell = the whole sim wind-up: a danger- or parry-class riser from the wind-up's first frame. Their "off the
  danger hue" lists (wick 2, loom 2, brawl 1) are artifacts (danger is computed only when found): their cue hue is NOT KNOWN yet.
- The wyrm (aggro, spit) and the crown (6 attacks): `cue roots ... none` at the reveal (the wyrm emerging, the crown a ring with an empty
  centre: nothing drawn within 3 m), so `objects 0`, `tell none`: not measured.

### The meter fix (c984ddc9, b12acbb0)
- BossCue63: `haveTop` (a riser is a riser whatever its id); the reel names the strongest when found. Test: ARiserWithANegativeInstanceId_IsFound.
- Roots: the boss views' own root names first (UnityTuning.Boss63CueRootNames: NIGHT37 THE WICK, NIGHT40 THE ICEBREAKER, KILN WYRM + NIGHT43
  THE RING + WYRM47 BEATS, NIGHT37 THE LOOM, NIGHT37 CROWN), then the 3 m sphere; retried at each 30-frame scan until found; the end line says
  `NO ROOTS ... not a measurement` if none ever was.
- N63BossCueTests 9 / 9 (outside Unity, a scratch runner). Not run in a Player.

### The fix (811bce3b, 6a490fa1): THE DANGER COLUMN
- **What the player sees:** through every non-parry attack's wind-up, an open tube of red-orange light (Tell36RedHex) around the boss
  (radius / height per boss: wick 3.5 / 8 m, icebreaker 3.5 / 7, wyrm 3 / 6 around the head, loom 5 / 10, crown 5 / 10), brightening from
  0.45 to 1 x BossTell63Gain (0.6) as the hit nears, gone on the strike. Unlit and additive: it does not depend on the room's lights (the
  lead's note on b3's darker held frames), and adds the same light on a dark frame.
- **Which attacks:** `BossTell63.Wants`: the sim's tell kind not yellow, not in UnityTuning.BossTell63Skip. Parryable attacks keep only their
  own yellow tell (THE WICK's ember, THE LOOM's spool, THE CROWN's gem shard, THE ICEBREAKER's block); the wyrm's aggro / aggroLean (its punch
  window: the head goes yellow, WyrmView.cs:125; the rule line says to swing the gauntlet at its charging head) and its roar (the core opening)
  are left alone. "Attacks already fine are untouched" (the slate) is NOT applied attack by attack: the corrected before-numbers were not
  available, and the lead's order was "for every non-parry attack"; the off sides of the same build give the before for each attack.
- **Timing:** driven by BossAttackClock63 (read only): on from the wind-up's first step, so the cue's lead is the sim's wind-up (config's tells
  0.55-1.5 s; n63-b2's tells 1008-1625 ms): over 300 ms for every attack whose wind-up is. No damage frame moves; the sim is not written.
- **Drawn as the meter reads it** (L-096): the views' own glow path (FxDraw.Additive on the neon material = URP Unlit, a MaterialPropertyBlock
  `_BaseColor`: Brawl40View's fists), under the view's own root (so the meter's roots include it). N63BossTellTests pins that its colour is in
  the danger class at every level and over the meter's rise threshold at the wind-up's first step. **With the row on, the boss63 cue lines are the
  meter's planted case in a Player:** each non-parry attack should read `tell` = the wind-up (V's attack line's lead) and `danger yes`.
- **Switch:** NightRows.ViewOn read every frame (D1's same-run toggle), -skygrindOff63 or older, -skygrindNoBossTell63; with the row off
  nothing is built (NIGHT62's boss exactly). Hooks: one line first in Wick37View.Render, Brawl40View.Render, WyrmView.Render, Loom37View.Render,
  Crown37View.LateUpdate. Proof: `[tell63] cue <boss> <attack> | level .. at wind-up ..` and `[tell63] skip <boss> <attack> (<why>)`.
- **Tests:** N63BossTellTests (4): RED 1 / 4 on 811bce3b, GREEN 4 / 4 on 6a490fa1 (outside Unity, a scratch runner); N63BossCueTests 9 / 9.

### Player steps
1. Build: `node Tools/night/n46-build-rows.cjs <tag> on n63.bossTell63` (with the meter fix in the same build).
2. Sides, bossMotion63 off on both (the slate: "with bossMotion63 off in that build"):
   `bash Tools/night/n63-reels.sh <tag> t63f,n52-foundry,boss63,-skygrindNoBossMotion63 t63i,n52-iceline,boss63,-skygrindNoBossMotion63
   t63k,n57-kiln,boss63,-skygrindNoBossMotion63 t63s,n52-span,boss63,-skygrindNoBossMotion63 t63p,n52-spire,boss63,-skygrindNoBossMotion63`
   and the same five with `,-skygrindNoBossTell63` (sides t63foff ... t63poff): the corrected before.
3. Greps: `grep -h "^\[boss63\] cue\|^\[tell63\]" Logs/player/n47-reel-<tag>-t63*.log`. Want on: each `cue roots` line names the boss's view
   root (the wyrm and the crown too); every non-parry attack `danger yes`, `tell` within a frame of V's `[boss63] attack ... lead`; the `cues`
   line `under 300 ms 0` and `off the danger hue 0`; one `[tell63] cue` per non-parry attack, one `[tell63] skip ... (parryable ...)` per
   parryable one, whose cue line keeps `parry yes` as on the off side. Off: no `[tell63]` line.
4. **SPOT pose:** boss63 n52-foundry `shot-N30-B63-wick-windup.png` (V's frame at 70 % of the hold's first wind-up: THE WICK's wickSlam, red,
   column level ~0.88), on vs off, bossMotion63 off on both; a same-run pair if D1 covers bossTell63 (it is read every frame), else the two
   runs above. Visible change: the red-orange column around THE WICK. Changed px: `python -I Tools/night/n62-gun-yellow.py --pair <on> <off>
   --rect 0,0,1,1`, want >= 41,472 (not measured: the column's size on screen depends on the reel's distance). Second choice if the wick's first
   wind-up is parryable in that run: `shot-N30-B63-brawl40-windup.png` (brawlLeap) or `shot-N30-B63-crown37-windup.png` (bladeRing).
5. verbs37 x5 on that build (nothing lost).

### Not checked
- Everything in a Player: the column's look and size on screen, the changed px, the meter's after-numbers, whether D1's same-run toggle covers it.
- Whether a view's own cue already met the slate for some attacks (the corrected before comes with the same build).
- The wyrm's spit / shed being parryable (the sim gives no kind): treated as attacks, so they get the column.
- An attack whose sim wind-up is under 300 ms would still be short: the column cannot start before the sim's tell (none seen in n63-b2).
