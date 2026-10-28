# ULTRAKILL, read from its shipped code (2026-10-11, GG's own installed copy)

**What was read, and how.** GG's Steam install of ULTRAKILL (Unity, Mono). Target:
`ULTRAKILL_Data/Managed/Assembly-CSharp.dll`, 2,797,568 bytes, SHA-256
`816a334fa4bbd78181c1fdf20b2edc5ffc83a61b3ddf0bf71fb7ec925471d21d`.
- REA (the Reverse Engineer Anything MCP server, rea 6.4.0) classified it as managed PE/CLI (`inspect_managed_artifact`, Evidence
  `ev_3abbe465a22d9981b3e7ed7391f29c657b47c236825b049fc590bfa671bb5e22`), but its metadata exceeds REA's fixed 64 MB representation
  budget (`ManagedDecodeBudget.js`, not configurable), so `inspect_managed_members` returned no members (Evidence
  `ev_808ad7b240b0a0c4060171686c1acd854776c30b433117d7200a7bb082d391fd`). REA's managed tools also do not decompile method bodies.
- The code was then read with ILSpy (`ilspycmd` 11.1.0, MIT), installed only in the session's scratch folder, decompiling to scratch.
  Nothing from it is in the repo. **No code is copied into SKYGRIND**: only numbers and the shape of rules, each cited by file:line in
  the decompiled output. Fields set in the Unity inspector (walk speed, jump power, wall-jump power) live in asset files and were
  not read: "not checked".

## Time: hitstop and slow motion (TimeController.cs)
- `SlowDown(amount)` sets the GLOBAL time scale to `amount` at once, then returns it toward 1 by 0.02 per FixedUpdate
  (`Mathf.MoveTowards(slowDown, 1, 0.02f)`). There is no hold. With a 0.02 s physics step, a 0.25 slow-down recovers over about
  ln(1 / 0.25) = 1.4 s of real time (derived, not measured). Audio pitch follows the time scale.
- `HitStop(t)` freezes the time scale to 0 for t real seconds; `TrueStop(t)` also mutes pitch. The longest pending stop wins.
- Who calls them (every call site): the last enemy of a wave `SlowDown(0.15f)` (ActivateNextWave.cs:90, ActivateNextWaveHP.cs:56);
  bosses' scripted moments 0.25 / 0.15 / 0.01-0.0001; a parry `TrueStop(0.25f)` + camera shake 0.5 + a 0.1 s flash (ParryFlash);
  the shotgun hammer TrueStop 0.25; every punch hit `HitStop(0.1f)` (Punch.cs, six sites); the revolver beam and grenades
  `HitStop(0.05f)`; an enemy projectile `HitStop(0.005f)`.
- **The weapon wheel** (WeaponWheel.cs:34-35): while it is open `Time.timeScale = 0.25f` and `timeScaleModifier = 0.25f`. It opens when
  NextWeapon / PrevWeapon / LastWeapon is HELD for 0.25 s (GunControl.cs:215, `WeaponWheelTime = 0.25f`).
- **No grapple slow motion exists in ULTRAKILL**; its slow-downs are global (the player slows too) and short.

## Weapon switching (GunControl.cs)
- `SwitchWeapon` swaps the active gun at once: no global swap delay. A tap of a weapon key (held under 0.25 s) switches; scrolling
  has `scrollCooldown = 0.5f` drained at 5 per second (about 0.1 s between scroll switches).
- Weapons keep their own cooldowns while holstered (the reward for switching is freshness, below).
- **QUICKDRAW** (Shotgun.cs:572-574, RevolverBeam.cs:950, Coin.cs): +50 style for a hit while the weapon's Equip animation is
  still playing: switching and firing at once is paid.
- **ARSENAL** (EnemyIdentifier.cs:1563-1565): +50 style when an enemy dies having been hit by more than one weapon
  (`hitterWeapons.Count > 1`), per enemy, no time window.

## Style (StyleCalculator.cs, StyleHUD.cs)
- Air multiplier (StyleCalculator.cs:47-55): rises 2 per second while airborne up to x3, falls 10 per second on the ground (about
  0.2 s back to x1). This is exactly NIGHT34 B's air34.
- Freshness (StyleHUD.cs:58-65): the weapon in use loses 0.5 per scoring move and 0.25 per second; holstered weapons gain 1 per
  move and 0.5 per second; per-event multipliers (shotgun hit 0.15, nail hit 0.1, explosion hit 0.75, kill 0.3, quickdraw 0,
  arsenal 0). Bosses: style gain x1.5, freshness decay x1.5.

## Movement (NewMovement.cs; constants in code only)
- Coyote: a ground jump is allowed up to 0.06 s after leaving the ground (`sinceLastGrounded < 0.06f`, :1027). SKYGRIND: 0.1 s.
- **Wall jumps: up to 3 per airtime** (`currentWallJumps < 3`, :1038), reset on landing (:1287). Each wall jump plays its sound
  higher (+0.25 pitch per jump) and the third has its own sound (:1726-1734). SKYGRIND already allows 3 too
  (`wallJump.maxPerAirtime` 3 in PortExport/config.json; builder M's harness: 3 chained wall jumps). Correction: the lead first
  quoted `wallJumps` 1, which is the "grip" perk's row, not the movement rule.
- **Slide into a wall** (WallJump, :1754-1763): while sliding (or within 4 frames of a slide start) the wall jump REFLECTS the slide
  direction off the wall, keeps the full speed, adds a 0.35 push away, and keeps at least 15 upward. A normal wall jump zeroes the
  velocity and pushes away + up.
- Dash: three charges (`boostCharge` 300, a dash costs 100, refill 70 per second: about 1.4 s per charge); a dash lasts while
  `boostLeft` 100 drains 4 per physics step (25 steps).
- Slide: a 0.2 s pre-slide window keeps the speed you entered with (`preSlideDelay = 0.2f`, `preSlideSpeed`); a slide-jump bonus
  within 3 frames of the slide's start (`framesSinceSlide <= 3`).

## The Whiplash (HookArm.cs)
- Throw speed 250 per second (:817); cooldown 0.5 s (:513); it pulls the player to the hooked point at a constant 60 (:1545) and
  yanks a light enemy to the player at 60 (:1501-1505); the hook returns at 100 + distance / 2.
- **Throwing it readies the punch**: `if (fistCooldown > 0.1f) fistCooldown = 0.1f` (:524-526): whip -> punch is a built-in link.
- Being pulled to a target adds "hard damage" (anti-HP) up to 50 (:1326-1332): the pull costs something.

## The punch (Punch.cs): a hitbox bigger than the model
- The punch hits enemies by a 4 m ray, then, if that misses, a **1 m radius sphere cast 4 m forward** (:649-652).
- Parrying projectiles: a 4 m ray or a 0.6 m box cast (:562-565, 597); a 3 m-radius sphere 3 m ahead for nearby projectiles (:697).
- Every punch hit is a 0.1 s hitstop.

## What this means for SKYGRIND (inferences; each a design choice for GG)
1. **The bend is new ground**: ULTRAKILL never slows time on the grapple, and its slow-downs slow the player too. The SKYGRIND split
   bend is a deliberate departure (GG's request). ULTRAKILL's own heavy moments are short: 0.1-0.25 s freezes and a 0.15 slow-down
   on the last kill with a ~1.4 s recovery.
2. **Bosses**: ULTRAKILL never hands the player free slow motion in a boss fight (only scripted boss moments). Supports DECIDE34
   N-11's "only the short bend in boss rooms" option.
3. **Wall jumps:** both games allow 3 per airtime (no change needed). ULTRAKILL's rising pitch per wall jump is a sound cue: GG's.
4. **Slide-into-wall reflection** keeps speed through walls: a momentum-preserving route the movement-line rooms can use.
5. **ARSENAL and QUICKDRAW** are ULTRAKILL's direct rewards for switching weapons, outside any slow motion.
6. **Whip readies the gauntlet**: a combo link (grapple -> punch) that costs nothing to add.
7. The gauntlet's 0.6 m boss margin is in line with ULTRAKILL's 1 m sphere punch.
