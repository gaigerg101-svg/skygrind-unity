# NIGHT63 builder V: B1 BOSSES THAT MOVE LIKE THEY MEAN IT (design.n63.bossMotion63)

Branch `n63-B1` from port/stage3's A1 (1ee56384). Shas as they are on my branch (L-117). Nothing here was run in the Editor, a build, a Player or
a reel: every Player number below is "not checked" until the lead runs it.

## Pick 1: BossAttackClock63 and the boss63 reel (B1's first deliverable; builder G's B2 builds on it)

Shas, in order: `70500356` (test first, RED), `cdb636e9` (the clock, the reel, the switch and the probe). Cherry-pick: `git cherry-pick 70500356^..cdb636e9`.

### The red (read-only, measured before any change)
- The layer does not exist, so every boss part's offset from its rigid pose is 0 by construction. A grep of the boss view files for
  spring / damp / anticipat / flinch / overshoot / follow-through / squash (command:
  `for f in Assets/Scripts/Game/{Wick37View*,Brawl40View*,WyrmView,Loom37View*,Crown37View*,Boss37Kit.View}.cs; do grep -ciE 'spring|damp|anticipat|flinch|overshoot|follow-through|squash' $f; done`):
  Wick37View.N43.cs 10 (wick43's flinch on OPEN hits only), Brawl40View.N47.cs 5, Loom37View.Hull48.cs 21, Loom37View.Look47.cs 16,
  Crown37View.Look47.cs 10 (crownLook47 already rocks, rears through a tell and kicks on a hit), Crown37View.N47.cs 2, every other file 0
  (Wick37View.cs, Brawl40View.cs, WyrmView.cs, Loom37View.cs, Crown37View.cs, Boss37Kit.View.cs 0 or 1).
- What the views do today (read): THE WICK's body is the sim's centre plus a sine bob; THE ICEBREAKER snaps its lean / head / drop per state
  (Brawl40View.cs:270-318, one number per state, no easing); THE KILN WYRM's boxes ARE its hitboxes, copied each frame; THE LOOM's body, legs and
  knees are spans between the sim's points; THE CROWN rocks (crownLook47). No boss has a slump, and only THE CROWN has a wind-up pose.
- In a Player the red is the boss63 reel's own line: with the row on and no layer, `[boss63] rms <boss> s<n> 0.0000 m parts 0` every second
  and the verdict RED "rms 0 in N of N seconds (the rigid copy)". Not checked (no Player).

### What pick 1 adds
- `Game/BossAttackClock63.cs`: `BossAttackClock63.Read(sim, boss)` returns a `BossAttack63`: boss (bossRise id), attack id, phase (Reveal, Idle,
  Windup, Strike, Recover, Stagger, Change, Dying, None), stepsToHit (the whole sim steps until the strike begins; 0 in the strike; -1 else),
  windupSteps, phaseSteps, why (window / stuck / dazed / staggered), centre, weak point, hp, maxHp. READ ONLY: no Step, Force or setter is
  called. The hit = the step the strike begins: Boss37Brain (THE WICK, THE LOOM, THE CROWN) and Brawl40 go TELL -> ACTIVE when the tell's
  steps left run out, so stepsToHit = stateLeft (pinned on a bare brain: the strike begins exactly stepsToHit steps after the read); the wyrm
  has no active state, so a tell logged at step s of `seconds` hits at s + StepsFor(seconds), read as a Strike for 0.25 s
  (UnityTuning.Boss63WyrmStrikeS). Weak points: THE WICK the core, THE ICEBREAKER the torso, THE KILN WYRM the head (the core while open),
  THE LOOM a live knee (the spinneret in the window), THE CROWN the ring's top blade (its centre is empty).
- `Game/FeelReel.N63V.cs`: the `boss63` scenario (registered in FeelReel.N41.cs's PhaseN41T beside boss61). boss61's walk-in, then 25 s after
  the rise on the fixed clock (FeelClock), the camera on the weak point, taps every 2 frames after the reveal. Stagger FORCED at 14 s if none
  came by itself (the sims' own public calls: Boss37Brain.Stagger, Brawl40.Drain, Wyrm31.OnStagger; the line says FORCED). Lines (Player.log
  and `<side>/boss63-<boss>.txt`):
  - `[boss63] attack <id> windup <ms> hit <ms> lead <ms> pose <ms | off>` (ms from the rise; pose = how long before the hit the drawn wind-up
    showed, BossMotion63Probe.Windup >= 0.25)
  - `[boss63] flinch <ms> hp <a> -> <b> drawn +<n> frames | off`
  - `[boss63] stagger <ms> <why>[ FORCED]` and `[boss63] stagger end <ms> after <ms>`
  - `[boss63] rms <boss> s<n> <m> m parts <n> max <m> m limit <share>`
  - the `[reel] boss63:` summary (GREEN / RED / OFF with BAD reasons).
  Frames (`shot-N30-<name>.png`): `B63-<boss>-idle`, `B63-<boss>-flinch` (2 frames after the first damage event past 3 s), `B63-<boss>-stagger`
  (0.6 s into the first stagger), `B63-<boss>-windup` (a wind-up 70 % through: B2's cue frame, never B1's SPOT).
- `Game/BossMotion63.cs`: the row's switch (`BossMotion63.On(cfg)`: NightRows.ViewOn, -skygrindOff63 or older, -skygrindNoBossMotion63; read every
  frame, never cached, for D1's same-run toggle) and `BossMotion63Probe` (what the layer drew on a frame; all zero until the layer lands).
- `Game/UnityTuning.N63V.cs`: the reel's Unity-only timing, one comment each.
- For builder G (B2): two partial hooks in FeelReel.N63V.cs, `Boss63EachG(g, dir, BossAttack63 a, double ms, List<string> lines)` every frame
  of the hold and `Boss63EndG(g, dir, boss, lines)` after it; implement them in your own file, nothing in mine changes. The clock's
  `stepsToHit`, `windupSteps` and `Windup01` give each attack's hit step.
- Tests: `Assets/Tests/EditMode/Game/N63BossClockTests.cs` (6 tests: the brain's count to the strike and read-only twin check, the brain's leg
  change, every Icebreaker state, the wyrm's log, the proof lines' text, boss63's registration). RED on 70500356, GREEN expected on cdb636e9.
  Not run (the Editor is the lead's).

### Checks (pick 1)
- `bash Tools/night/dn-check-wt.sh <scratch>`: 0 errors in all four projects (Skygrind.Game, Skygrind.Game.Tests, Skygrind.Core.Tests, Editor);
  the scratch deleted after.
- `bash Tools/night/n63-gates.sh --no-editor` in the worktree at cdb636e9: config copies identical; golden 0 failing, 0 mismatches, 22 replays
  exact; fingerprints plain and --off57 .. --off63 EQUAL; "gates: all GREEN 23:00:47".
- Not checked: the EditMode tests (the lead's Editor run), any Player run.

### Player steps for pick 1 (the lead)
- Any build with pick 1 (the row stays OFF in it: the off reel is the red's other half). One Player per boss:
  `bash Tools/night/n63-reels.sh <tag> b63f,n52-foundry,boss63 b63i,n52-iceline,boss63 b63k,n57-kiln,boss63 b63s,n52-span,boss63 b63p,n52-spire,boss63`
- Grep: `grep -h "^\[boss63\]" Logs/player/n47-reel-<tag>-b63*.log` and `Logs/feel/<tag>/summary47.txt` for the `boss63:` lines.
- Expected with the row off: verdict OFF, rms 0.0000 every second, `pose off`, `drawn off`; each attack's `lead` is the sim's own wind-up
  (config: 0.55-1.5 s). The flinch and stagger frames exist per boss (a stagger FORCED where none came).
- The lead's run on n63-b1 (row OFF; Logs/feel/n63-b1/summary47.txt, the lead's message): all five OFF, rms 0 in 23 / 23 seconds, damage
  events drawn 0 of 42 / 14 / 21 / 17 / 35, wind-ups posed 0; the sim's leads wick 1100-1300, brawl40 1100, wyrm 533-900, loom37
  1325-1633, crown37 1000-1508 ms; staggers 1 / 4 / 2 / 1 / 1 (FORCED on wick, wyrm, loom37, crown37); 0 NREs. That is B1's red in a Player.

## Pick 2: the layer BossMotion63 (design.n63.bossMotion63, OFF)

Shas, in order: `d76cc4d6` (test first, RED: N63BossMotionTests, the layer a placeholder, the numbers {}), `a1810813` (the numbers by
Tools/night/n63-v-rows.cjs), `3ae25b65` (the layer, the numbers retuned), `437bcbf8` (the five hooks, the side lean and keel, the wyrm's piece
numbers), `84097a5e` (boss63 prints the distance at its SPOT-candidate frames), then this notes commit.
Cherry-pick: `git cherry-pick d76cc4d6^..<this notes sha>`.

### What it does (view only; the sim, the hitboxes, the damage frames and every fingerprint untouched)
- `Game/BossMotion63.Layer.cs`: `BossMotionRig63` per boss steps a `Motion63` each frame from the boss's RIGID pose (what the view drew until
  tonight) and BossAttackClock63's read: the follow spring (t3ssel8r: k1 = z / (pi f), k2 = 1 / (2 pi f)^2, k3 = r z / (2 pi f); f 2.2, z 0.5,
  r 1.6; sub-stepped under its safe step; held in hitstop; reset by a jump past 4 x size); the wind-up (eased in over 0.3 of the wind-up, then
  held to the hit: lean back 13 deg, cock to one side 10 deg, pull back 0.14 x size, crouch 10 %; the breath muted); the strike (the lean
  target jumps to 0.75 forward for 0.25 s; the lean spring f 2.8, z 0.45, r 1.2 snaps through rest to 1.18 and swings back to -0.22,
  simulated); the flinch on each damage event (Quilez's impulse h exp(1 - h), peak 40 ms, on the FRAME clock so it is drawn on its own frame
  even in hitstop; a volume-kept squash of 20 %, a recoil of 0.12 x size, a tip back of 7 deg; 0.55-1 by the hit's share of the pool); the
  slump while staggered (its spring f 1.6, z 0.55, r 1.2: sink 0.45 x size, droop 22 deg toward the player, keel 14 deg to one side, squash
  12 %, sway 4 deg at 0.6 Hz); the breath (0.4 Hz, height 4 %, lift 0.04 x size).
- THE GUARD: the offset at most limitShare x the boss's collider radius (the sim's own number), the tilt at most limitDeg, the squash at
  most 0.28 from 1: THE WICK 0.85 m / 26 deg (cageR 1.7), THE ICEBREAKER 0.6 m / 26 deg (radius 1.2), THE KILN WYRM 0.375 m / 14 deg on the
  head and 0.25 x each piece's half width (no follow lag: its boxes are its hitboxes), THE LOOM 0.8 m / 20 deg (bodyHalfH 1.6), THE CROWN
  0.75 m / 18 deg (inside its 2.4 m pass radius).
- The hooks (one line in each view, its body in `<View>.N63V.cs`): THE WICK moves the cage (bodyRoot); THE ICEBREAKER springs its own
  per-state pose (lean / head / knee drop, which NIGHT40 snaps), the tilt into the torso's lean and the sink into the knees (the feet planted),
  the arms re-spanned to the sim's fists (the fists stay on the hit spheres); THE KILN WYRM moves the head (the core and the trim plate ride with
  it) and ripples, sags and breathes the body pieces (never a tilt); THE LOOM offsets the shuttle's centre in loomLook47 and turns / squashes
  the hull in loomHull48 (the knees, the grapple nodes, never move); THE CROWN moves the ring after crownLook47's own springs.
- Off (the row off, -skygrindOff63, -skygrindNoBossMotion63, reduced motion): every hook draws the rigid pose exactly (offset zero, identity,
  scale one; the Loom's lines are behind `if (m63.on)`), and the springs hold their state. `BossMotion63.On(cfg)` is read every frame.
- Why the side lean and keel (the lead's pick-1 note): a lean toward or away from the player turns about the screen's horizontal axis, mostly
  in depth from where the player stands; only the sink, the squash and a sideways lean move in the screen's plane.

### The SPOT pose (NIGHT63.md rule 8: >= 41,472 px changed on the same-run pair)
- **THE ICEBREAKER, side b63i (n52-iceline), frame `shot-N30-B63-brawl40-stagger.png`**: 0.6 s into its first stagger, which on n63-b1 came
  by itself (4 staggers, none forced): after a haymaker the Icebreaker is STUCK with its fist in the ice. It started the swing within 5 m of
  the player (design.n40.brawl40.haymaker.startDist) and lunged 2.5 m, so its centre is about 2.5-4 m from the camera. At 74 deg FOV
  (camera.fovDeg) a 3.6 m brute at 4 m is about 650 px of the 1080 px frame. On it the torso droops 22 deg further, the knees drop 0.54 m
  (about 100 px), the whole body keels 14 deg to one side (its head moves about 0.85 m, about 150 px), and it squashes 12 %. My estimate is
  well over 100,000 px changed. That is an estimate, not a measurement; the summary line now prints the camera's distance at that frame
  ("stagger yes (stuck) at <m> m").
- Second choice: THE WICK's `B63-wick-stagger` (side b63f). You measured the cage at about 200 x 300 px. Its slump sinks 0.77 m (about 58 px
  at that size) and keels 14 deg (the top about 36 px sideways), plus the squash. My estimate is 40,000-60,000 px: borderline. Use it only if
  b63i's frame misses.
- Never B63-<boss>-windup (B2's cue frame). bossRule61's line stands over the first ~5 s of a fight in b1's copy. It is the same in both
  captures, and the stagger frames come later anyway (b63i: the first haymaker's stuck, or the 14 s forced stagger).

### MAIN / GUARD and how to read them (the boss63 reel, the row ON in the build)
- MAIN rms: `[boss63] rms <boss> s<n> <m> m parts <n> ...` > 0 in every second after the reveal on every boss (the verdict says "rms 0 in N of
  N seconds" when not); off = 0 (the OFF verdict).
- MAIN wind-up: `[boss63] attack <id> windup <ms> hit <ms> lead <ms> pose <ms>`: pose >= 250 for every strike (the verdict counts "wind-up
  poses under 250 ms before the hit" and "strikes without a drawn wind-up pose"). Simulated on the shortest config wind-up (0.55 s): the pose
  passes 0.25 of its peak 100 ms after the tell starts, 450 ms before the hit.
- MAIN flinch: `[boss63] flinch <ms> hp <a> -> <b> drawn +<n> frames`: n <= 2 for every damage event (the verdict counts late and undrawn ones).
  The envelope is 0.75 of its peak on the first frame. The view hooks run in Update (the Crown in LateUpdate), so expect +0 (the Crown +1).
- GUARD: `limit <share>` <= 1.00 in every rms line (the verdict's "a part at X of its limit"); the hit frames are the sim's own (`hit <ms>`
  on and off agree on the same seed, and the fingerprints are EQUAL: no Core or sim file changed).
- Stagger slump and idle breathing: the `B63-<boss>-stagger` and `-idle` frames; the probe's slump and breath are not printed per frame.

### Player steps (the lead)
1. Build with the picks, then `node Tools/night/n46-build-rows.cjs <tag> on n63.bossMotion63`.
2. The SPOT pair first: with D1 picked, the same-run toggle on b63i's `B63-brawl40-stagger` shot (the off capture in `b63i-off/`). Without
   D1, two runs: `bash Tools/night/n63-reels.sh <tag> b63i,n52-iceline,boss63 b63ioff,n52-iceline,boss63,-skygrindNoBossMotion63`, and say
   it was a two-run pair. Then `python -I Tools/night/n62-gun-yellow.py --pair <on>/shot-N30-B63-brawl40-stagger.png <off>/shot-N30-B63-brawl40-stagger.png --rect 0,0,1,1`.
3. The rest: `bash Tools/night/n63-reels.sh <tag> b63f,n52-foundry,boss63 b63i,n52-iceline,boss63 b63k,n57-kiln,boss63 b63s,n52-span,boss63 b63p,n52-spire,boss63`
   (row on), and the same five with `,-skygrindNoBossMotion63` for off = 0.
4. Grep: `grep -h "^\[boss63\]" Logs/player/n47-reel-<tag>-b63*.log`; the `boss63:` verdicts in `Logs/feel/<tag>/summary47.txt` (GREEN on, OFF off).
5. verbs37 x5 on that build (the row only draws in a boss fight; verbs37 has none, so it should be unchanged).

### Checks (pick 2)
- dn-check 0 errors after every step (all four projects); the scratch deleted at the end.
- `bash Tools/night/n63-gates.sh --no-editor`: started at 437bcbf8, "gates: all GREEN 23:20:42". The config copies were identical, golden
  0 failing / 0 mismatches with 22 replays exact, and every fingerprint (plain, --off57 .. --off63) was EQUAL. The FeelReel-only commit
  84097a5e landed during that run. The re-run at this notes commit is in the report message.
- N63BossMotionTests (10 tests): RED expected on d76cc4d6 (the placeholder answers the rigid pose, the numbers {}), GREEN expected from
  437bcbf8 (the hooks test needs the five hook lines). Not run (the Editor is the lead's). The spring thresholds the tests use were checked in a
  scratch simulation of the same step (the step settles at 1.000 and peaks at 1.255 at 60 fps, 1.274 at 30 fps; the slump is at 0.98 0.6 s
  into a stagger and at 0 2.5 s after).

### Not checked
- Any Player frame: whether the motion reads as intended, its size on screen, the changed pixels on the SPOT pair. The estimates above are
  arithmetic, not measurements.
- How D1's same-run toggle holds the clocks. The layer's springs step on game.WorldDt and the flinch on game.FrameDt. The layer draws the
  rigid pose in any frame where BossMotion63.On reads false, and its state is untouched while off.
- THE LOOM with loomLook47 off (not in any build since NIGHT47): nothing moves there.
- The Icebreaker's slump on top of its own window pose (lean 35, drop 0.6): with mine the drop reaches about 1.1 m (capped at 0.6 m over
  the view's own number). It may read as kneeling deep into the legs; judge it on the frame.

### D1's contract, read after pick 2 (no change needed)
D1 (aac1abbc / 13953aed / 34f1bb41): while a same-run shot is held, FeelClock.FrameDelta is 0. So the loop's FrameDt and WorldDt are 0 in a
held frame, and the layer's springs and flinch hold. The layer reads the row every frame (BossMotion63.On -> NightRows.ViewOn, which D1
makes consult the reel's override). The held frame with the row off draws the rigid pose from the same state, and the state is untouched
while off.

## Leftovers (branch n63-L from port/stage3 at e87536d8: D1 and pick 2 in; the lead's list, ranks 3 and 4)

Shas, in order (each item test first, RED on its own commit):
| item | RED | GREEN |
|---|---|---|
| T-146 rim56 read twice a frame (the w60 review's S2) | 37486d89 | d7d1c669 |
| T-146 an unlocked Editor run paused by a focus loss (the w60 review's S4) | ed313f54 | 74086080 |
| R1 N3 the public mutable mode list | 2864ed53 | a166b223 |
| R1 N11 the KESTREL and the gauntlet outside gunYellow61's rule | 58b3285a | e96a9a3c |
| R1 N12 the style-red tool's substring match | 355c49d0 | 3a895fc4 |
Then this notes commit. Cherry-pick: `git cherry-pick 37486d89^..<this notes sha>`. Skipped as the lead said: R1 N1 and N5 (D's and G's
files tonight).

- **S2 (T-146): rim56 read once a frame.** Rim56.LateUpdate's lit gain called Rim60.On(g.cfg), which read the row again through Rim56.On.
  The review estimated about 0.5 KB of garbage a frame. It now takes `Rim60.LitOn(on)`, the frame's own read: rim56 on and neither
  -skygrindOff60 nor -skygrindOff58. `Rim60.On(cfg)` is `LitOn(Rim56.On(cfg))`, so the band's caller (Decor55, once a map) is unchanged.
  At that line `on` is true, so the truth table is the same. The row is still read every frame, not cached, as D1 needs.
  Test: N63RimReadTests (LitOn on the process switches; Rim56.cs one `On(g.cfg)` and no `Rim60.On(`).
- **S4 (T-146): an unlocked Editor run is never paused by a focus loss.** `SurvivalGame.PausesOnFocusLoss63` is NIGHT60's rule plus the
  cursor locked this frame, and SurvivalGame.cs's focus line passes `input.Locked`.
  - A Player still pauses: NIGHT60 B9's probe read the lock as Locked after a Player's focus loss.
  - A locked Editor run is still paused by the lock rule.
  - NIGHT60's PausesOnFocusLoss and N60FocusPauseTests are untouched.
  Test: N63FocusLockTests (unlocked never; locked = NIGHT60's rule over every flow and flag; the call site).
- **N3:** `PlayerChecks.FocusAutomation61Modes` is a readonly `IReadOnlyList<string>` (Array.AsReadOnly of the same eleven flags, same
  order). Its one reader iterates with foreach; FocusAutomation61Tests' literal scan is unaffected. Test: N63ModeListTests.
- **N11:** Viewmodel.cs routes the KESTREL's (the revolver family) and the gauntlet's colours through GunYellow61.Family, as the THUNDERCLAP
  and FamilyOf do. Both are kinetic cyan, so they come back exactly as they came: no pixel moves today. Only Viewmodel.cs calls the rule
  (N61GunYellowTests holds). Test: N63GunFamilyTests.
- **N12:** n61-g-style-red.cjs matches ranks as whole words (`\b<rank>\b`).
  - `--selftest` (11 planted texts) measured 7 / 11 on the RED commit (COOLDOWN, BOLDER, SKYGRINDING, DULLNESS miscounted) and 11 / 11
    after the fix.
  - Read-only on Logs/feel/n62-freeze/vf/verbs37/telemetry.jsonl: 1178 frames name a rank, all DULL.

Checks:
- dn-check 0 errors at every commit: each test file was parked until its own item, so each RED commit compiles.
- Gates --no-editor: in the report message.
- Not checked: the five C# tests in the Editor (the lead's run). A Player run of the focus probe with S4's change (the probe's run must
  still read "cursor Locked" at the loss: `bash Tools/night/n60-focus-probe.sh` on a build with focus60 on).

## B1 fix round (branch n63-B1b from port/stage3 at 9399b97c; n63-b2's two-run pairs, DECIDE63 D-5)

n63-b2's B1 reels (the lead): wick GREEN, brawl40 GREEN, loom37 GREEN; wyrm RED (one wind-up posed 192 ms before its hit); crown37 RED (every
measure 0). The SPOT named the change on THE WICK's stagger pair: 42,273 px, just over the 41,472 bar. THE ICEBREAKER's stagger frame was
not usable: at the reel's 1.7 m its OFF frame is nearly all black, so its 990,686 px were the screen being covered, not the pose. My first
choice was wrong.

| item | RED | GREEN |
|---|---|---|
| 1 THE KILN WYRM's wind-up hidden by its stagger | 2e99d5c0 | 6d6502eb |
| 2 the row-off path keeps the state (D1's contract) | 42f8361d | 97fe2b42 |
| 3 THE CROWN's draws unseen by the reel | fb331fda | 4dae11fe |

- **1, the wyrm (the cause, from b63k's own lines):** "stagger 18000 staggered ... stagger end 24267 after 6267" and "attack spit windup
  23592 hit 24492 lead 900 pose 192". The sim told the spit inside the stagger, and BossAttackClock63.FromWyrm read the stagger before any
  pending tell. The layer's lean only started at 24267, 225 ms before the hit.
  - Now FromWyrm reads: the phase strings, then a hit's strike (only Boss63WyrmStrikeOverS, two reel frames, when another tell is pending),
    then the pending tell's wind-up, then the stagger, else idle. A tell not given yet is skipped.
  - The slump releases while the wind-up shows and comes back after the strike if the worm is still staggered.
  - The other four bosses never tell in a window or stuck, so their reads are unchanged.
  - One existing assertion changed, stated in its commit: Wyrm_TheLogsTellsCountToTheirHits pinned "staggered with two tells pending reads
    Stagger", the rule that hid the spit. That stagger case now reads where no tell is pending; Wyrm_ATellPendingInAStaggerShowsItsWindup
    pins the pending case.
- **2, D1's contract (the lead's message):** BossMotionRig63.Step advances the whole state every frame of a live fight, on or off, and the
  row only decides the drawing (Rest when off). The ICEBREAKER's hook steps its pose springs every frame and starts them fresh only at a
  fight's first frame; its off path no longer resets them. The other four hooks hold no state of their own.
  - Consequence: a hit taken while the row is off now ages its flinch as it would have with the row on.
  - Tests: Rig_OffKeepsItsState_TheFrameAfterAToggleEqualsARunWithoutIt (a toggled rig equals the never-toggled one exactly after the row
    returns) and Hooks_TheOffPathKeepsTheirState.
- **3, THE CROWN (the cause, from the code):** Crown37View draws in LateUpdate, after the boss63 reel's coroutine has read the probe. The
  reel only counted a draw stamped this frame, so every Crown measure read 0: rms, pose, flinches, limit share. b63p's Player log has no
  exception.
  - BossMotion63Probe.Take hands the reel each draw once: this frame's, or the frame before's for THE CROWN. The reel dates each taken
    draw to its own frame, so the pose's time is the draw's and a flinch counts only from a draw at or after its damage event.
  - Not checked: that the ring visibly moved on b2. The hook ran, but the reel could not see it; b3's b63p reel is the check.
- The GUARD (limit share <= 1.00) and the off state's drawing are unchanged by all three: the off state still draws the rigid pose
  exactly, and the probe is written only when drawn.

**SPOT pose, corrected:** THE WICK's `B63-wick-stagger` (side b63f) is B1's SPOT pose (42,273 px on b2's pair). For a bigger margin on a
later pair, the boss must fill more of a frame where it does not cover the camera: THE LOOM's hull or THE CROWN's ring at the stagger. Not
measured.

## Leftover 7: C2 the enemy glow up close (branch n63-C2 from port/stage3 at 5141affb; view row design.n63.nearFade63, OFF)

Shas, in order: `76c19058` (the red, read only: Tools/night/n63-v-near.py), `03def204` (test first, RED: N63NearFadeTests, NearFade63 a
placeholder), `dd68dcb8` (the row, the rule, the hook, the near63 reel), then this notes commit. Cherry-pick: `git cherry-pick 76c19058^..<notes sha>`.

### The red (read only, measured)
- `python -I Tools/night/n63-v-near.py --selftest`: 11 / 11 PASS (planted telemetry lines, planted images, a 10 x 10 change = 100 px).
- `--telemetry` on n62-freeze's verbs37, an enemy AT THE EYE (its screen rect at least the frame's height or width): vf 136, vi 98, vk 0,
  vs 52, vp 62 of 1800 frames each (348 of 9000, 3.9 %), in runs up to 43 frames (0.7 s).
- `--pixels` (the enemy-hue share: colors.enemy's hue +- 30 deg, S >= 0.65, V >= 0.12):
  - vf/f0400 (gg-proxy's "screen-filling red-orange dome holding a flat red bean"): 0.773.
  - vf/f0100: 0.020; vf/f0300: 0.025; vs/f0100: 0.002; vs/f0400: 0.007; vi/f0200: 0.002; vp/f0800: 0.030.
  - The S floor was raised from 0.45 after measuring: the foundry floor's S median is 0.38, the enemy bodies' p10 0.72, the dome's p10 0.93.
- What f0400 is (read from its telemetry and Enemy.shader, not from a probe):
  - At frames 396-398 the eye reaches husk 2: its rect grows to 3224 x 7298 px.
  - The dome is husk 2's own capsule at the eye, dark red with an orange rim. I checked and ruled out the tell halo, the launch shell
    (fx49 makes it cyan, fx47 caps it) and the spawn flash.
  - The bean is husk 3 behind it, drawn by the x-ray pass (ZTest Greater: it draws wherever a nearer body hides it). It is flat red, with
    the visor arc, and husk 3's rect (299 x 678 px) matches.
  - f0100's "stacked discs" are the same x-ray through a nearer husk.
- How often in a game: not measured. The 3.9 % is the verbs37 reel's, which walks and dashes through lined-up husks; a player sliding
  through enemies meets it too.

### The change (view only; no sim, ray or hitbox change)
- `Game/NearFade63.N63V.cs`: each live enemy part dissolves through the enemy shader's own cell-noise dissolve as the eye comes within
  `farM` (1.0 m) of its world bounds. `_Dissolve` (1 whole .. 0 gone) eases to `floor` (0.1) at `nearM` (0.3 m) or inside.
  - The dissolve is the death's, 2.5 cm cells, and every pass clips by it, the depth passes included.
  - So the room shows through the near body, and an enemy behind it draws as itself: the dissolved cells write no depth, so its x-ray has
    nothing to draw behind.
  - A fading part has no edge band (the death's burning edge belongs to a kill).
- The hook (`EnemyViews.cs`, EnemyLook):
  - `Frame()` reads the row, its numbers and the eye once a frame (D1's contract; no per-part allocation).
  - `Tint` sets every live part's `_Dissolve` and `_DissolveBand` on its shared block every time and compares the dissolve in its change cache.
- Off (the row, -skygrindOff63, -skygrindNoNearFade63): `_Dissolve` 1 and the material's own band, the material's live values exactly. The
  shader returns before its clip at 1.
- The row: `Tools/night/n63-v-c2-rows.cjs` adds `design.n63.nearFade63` as design.n63's last key after colours63; both copies
  byte-identical, nothing else changed.
- Every enemy kind's body goes through EnemyLook.Tint: the husk, swarmer, spitter, charger, lancer, warden, drone, bulwark, burrower,
  climber and Enemy40. Not checked one by one in a Player.

### The SPOT pose and the Player steps
- The SPOT pose: `near63` reel, frame `shot-N30-N63-near-0.5.png`, a frozen husk with its body 0.5 m from the eye at the map's open spot.
  - Its off half is the f0400 kind: the body fills the frame with the red-orange dome.
  - With the row on, the rule dissolves about 72 % of its cells there (the reel's line prints the value for the capsule's bounds).
  - Expected far above 41,472 px. That is arithmetic on a body filling most of the frame, not a measurement.
  - The far control: `N63-near-3.0` (whole on and off).
- Steps:
  1. Build, then `node Tools/night/n46-build-rows.cjs <tag> on n63.nearFade63`.
  2. With D1's toggle usable: `-skygrindSameRun63 n63.nearFade63` on a near63 side. Else two runs (the placements are fixed, so they
     should not drift): `bash Tools/night/n63-reels.sh <tag> near63on,n52-foundry,near63 near63off,n52-foundry,near63,-skygrindNoNearFade63`.
  3. `python -I Tools/night/n63-v-near.py --pair <on>/shot-N30-N63-near-0.5.png <off>/shot-N30-N63-near-0.5.png`: the changed px and the
     enemy-hue share on vs off. Run the same on N63-near-3.0: expect about 0 changed.
  4. Grep `[near63]` and the `near63:` verdict (GREEN on, OFF off).
  5. verbs37 x5 unchanged in its rows. Its frames change wherever the eye meets a husk: that is the point.
- Not checked: any Player frame. Two open questions for the frames:
  - Whether the 2.5 cm cells read as an intentional screen-door at 0.3-1 m (the cells are about 30-40 px there) or as noise.
  - Whether a husk being punched at about 1 m should keep more of its body: farM 1.0 starts the fade at the edge of melee range.
