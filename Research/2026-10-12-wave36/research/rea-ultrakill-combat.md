# ULTRAKILL combat, read from its shipped code (2026-10-12, GG's own installed copy)

**What was read, and how.** This is the same DLL as the wave-34 note (`Research/2026-10-11-wave34/research/rea-ultrakill.md`):
`ULTRAKILL_Data/Managed/Assembly-CSharp.dll`, SHA-256 `816a334fa4bbd78181c1fdf20b2edc5ffc83a61b3ddf0bf71fb7ec925471d21d`.
It was decompiled with ILSpy (`ilspycmd` 11.1.0, MIT), which was installed only in the session scratch folder and wrote its
output only there (1,538 files). The REA MCP tools were not used again, because their 64 MB managed budget blocked them last
time. **No code is in the repo.** This note holds only numbers and the shape of each rule, cited as `File.cs:line` in the
decompiled output.

Some values are set in the Unity inspector or in animation clips: swing distances, spread, `fireRate`, beam width and the
timing of animation events such as `ReadyGun`, `DamageStart` and `StopTracking`. They live in asset files, so each is
marked **inspector value, not checked**. All times assume the physics step is 0.02 s. Nothing here was measured in play.

## 1. Weapons: where the hit comes from, and where the visual comes from

### Revolver (Revolver.cs, RevolverBeam.cs)
- **The hit ray starts at the camera.** The beam object is created at `cc.transform.position/rotation`, which is the
  camera, and is turned toward the auto-aim target if there is one (Revolver.cs:565-569). It is given
  `alternateStartPoint = gunBarrel.transform.position` (Revolver.cs:572).
- **The hit test** runs in the beam's `Start()` (RevolverBeam.cs:158-161 calls `Shoot()`). It is a 1000 m ray
  (RevolverBeam.cs:229, 243) plus a sphere cast toward enemies along the same camera line. The sphere's radius is 0.4 for a
  normal player beam, 0.1 for an enemy beam, and 0.6 for a piercing beam (1.2 for the railcannon) (RevolverBeam.cs:212-224,
  321). This means the revolver's hitbox is about 0.4 m wider than its visible line.
- **The visible line starts at the muzzle.** It is a `LineRenderer` with point 0 at `alternateStartPoint` (the barrel) and
  point 1 at `shotHitPoint`, the camera ray's hit (RevolverBeam.cs:347-383). The muzzle-flash child is moved to the barrel
  too (RevolverBeam.cs:389-391). So the line is drawn from the gun to wherever the camera ray hit, not along the camera
  ray.
- **The line fades out:** `widthMultiplier` drops by 1.5 per second and the muzzle light by 100 per second, and the object
  is destroyed at width 0 (RevolverBeam.cs:173-186). The starting width is an inspector value, not checked; if it is 1, the
  line lasts about 0.67 s.
- **Feedback comes in the same call as the shot.** `Shoot()` spawns the beam, plays the sound, kicks the FOV by
  `defaultFov/40` (`/20` for a charged shot), sets rumble, turns the cylinder and fires `anim.SetTrigger("Shoot")` or
  `"ChargeShoot"` (Revolver.cs:551-654). The FOV kick recovers at 300 degrees per second (CameraController.cs:357), so it is
  a pop of about one frame.
- **Fire rate.** Fire is `Fire1.IsPressed`, so holding the button keeps firing (Revolver.cs:365). After each shot,
  `shootCharge` refills at 200 per second up to 100, which is **0.5 s between shots** (Revolver.cs:251-261). There is also
  a `gunReady` gate that `ReadyGun` reopens; it is an animation event, not checked.
- **The piercer's alt-fire is a separate charge.** Holding Fire2 charges `pierceShotCharge` at 175 per second, so it is full
  in **0.57 s** (Revolver.cs:351, 378-392). The shot fires when Fire2 is released, or when Fire1 is pressed, once the charge
  is full (Revolver.cs:352). While the charge is building, the tap shot is blocked (`!chargingPierce`, :365); otherwise the
  tap shot stays instant on press. If you let go early, the charge drains at the same rate (:396-409). The piercer meter
  then refills at 40 per second, which is **2.5 s** (5 s on the alternate revolver) (Revolver.cs:276-283).
- **Coins (marksman).** Fire2's `WasPerformedThisFrame` throws a coin (Revolver.cs:413). There are 4 charges
  (`rev1charge` 400, 100 per coin), refilled at 25 per second, so **4 s per coin** (WeaponCharges.cs:14, 103, 135-137).
  The coin spawns 0.5 m below the camera and is thrown with `forward*20 + up*15 + the player's velocity`
  (Revolver.cs:668-678).

### Shotgun (Shotgun.cs)
- **12 pellets** (Shotgun.cs:527). They are `Projectile` objects spawned at the **camera position** with the camera's
  rotation, each rotated by a random amount within plus or minus `spread` (an inspector value, not checked)
  (Shotgun.cs:588-617). They are real moving projectiles, not rays.
- **A point-blank zone:** a 4 m `RaycastAll` from the camera damages every enemy body it passes through (damage 4, tagged
  `"shotgunzone"`) (Shotgun.cs:557-581).
- **The visuals come from the muzzles:** a muzzle flash is spawned at each `shootPoints[i]` (Shotgun.cs:701-714).
- **Feedback in the same call:** camera shake 1.0, `anim.SetTrigger("Fire")` (Shotgun.cs:692-699), and a +1 m/s push
  along the look direction, flattened to the ground plane (Shotgun.cs:520-523).
- **Fire rate** comes back through the `ReadyGun` animation event (Shotgun.cs:937-939). That is an animation clip value,
  not checked.
- **The core-eject alt** holds to charge `grenadeForce` at 60 per second up to 60 (1 s), with at least 0.5 s between cores
  (Shotgun.cs:309-314).

### Nailgun (Nailgun.cs)
- **Nails spawn 1 m in front of the camera,** or at the camera itself when a wall is within 1 m (Nailgun.cs:725, 814-819).
  Each nail is rotated by a random amount within plus or minus `spread/3` (Nailgun.cs:843). The muzzle flash alternates
  between the barrels, using `shootPoints[barrelNum]` (Nailgun.cs:742-759).
- **Fire rate.** `fireCooldown = currentFireRate`, and it drains at 100 per second in `FixedUpdate`, so the interval in
  seconds is `fireRate/100` (Nailgun.cs:634-667, 729). Holding fire raises `heatUp` by 0.55 per second, about 1.8 s to full
  (:337). The rate spins up from `fireRate + 3.5` to `fireRate` as it heats (:385). `fireRate` is an inspector value, not
  checked. The barrel spin goes from 250 to 2500 (:285).

## 2. Coins: the ricochet rule (Coin.cs)
- **The timeline after a throw** (Coin.cs:150-154):
  - 0.1 s: the colliders turn on, so the coin can be hit.
  - 0.35 s to 0.417 s: `hitTimes = 2`, a 67 ms window with a flash effect (:286-302). This is the "split shot".
  - 0.417 s: back to 1 (:304-311).
  - From 1.0 s: `hitTimes = 2` again, with a lasting charge effect (:313-328).
  - 5 s: the coin is deleted. A coin slower than 1 m/s is also removed after about 0.1 s (:266-283).
- **Each hop waits 0.1 s** (`Invoke("ReflectRevolver", 0.1f)`, Coin.cs:360). A split coin re-fires its second hit 0.05 s
  later (:795-800).
- **How the next target is picked,** in this priority order:
  1. Another coin, if more than one is out.
  2. An explosive (grenade or cannonball) within 100 m of the player.
  3. The nearest visible living enemy, aimed at its weak point (head) when that is visible. Enemies already hit in this
     chain are skipped.
  4. Glass.
  5. If none of these, a random direction.

  Sources: Coin.cs:381-660. "Nearest" is real: `Vision.TrySee` returns the first match in a distance-sorted list
  (ULTRAKILL.Enemy/Vision.cs:69-91, SortJob.cs:23, DistanceComparer.cs:9-11).
- **Coins stack power.** Each coin-to-coin hop gives `power + 1` (Coin.cs:392), and the beam gains `power/4` damage per
  coin (:777-778). The style award is 50 points, plus 15 per ricochet when there is more than one (:1336-1357).

## 3. Telegraphs (how an attack says it is coming)
- **The parry window follows the animation, and there is an early buffer.** A punch lasts 6 active physics frames
  (`activeFrames = 6`, Punch.cs:524). A punch that lands on an enemy before it is parryable stores `parryFramesLeft = 6`
  (Enemy.cs:2698, 2740), which counts down per `FixedUpdate` (Enemy.cs:653-666). If the enemy's `ParryableCheck()` opens the
  window within those frames, the parry fires on its own (Enemy.cs:2816-2842). So **a punch up to about 0.12 s early still
  parries**. A parry takes 5 health (4 when it came from the buffer) (Enemy.cs:2690).
- **The flashes** are `parryableFlash` and `unparryableFlash` (DefaultReferenceManager.cs:31-33). Each enemy spawns its
  own flash object at the moment the attack commits (listed below).
- **Filth (ZombieMelee)**
  - It swings when the target is closer than `swingDistance` (an inspector value, not checked) (:177-181).
  - It turns to track the player during the wind-up. The `StopTracking` animation event then locks its facing and opens
    the parry window (:458-467). After that come `DamageStart` and `DamageEnd` (:424-456).
  - Cooldown between attacks: 1 / 0.75 / 0.5 / 0.25 by difficulty, draining at 0.4 per second. At Standard that is about
    1.25 s (:104-119, 158).
  - Wind-up length: animation clip, not checked.
- **Stray / Schism (ZombieProjectiles)**
  - During the wind-up it holds a visible projectile in its hand. That projectile becomes its weak point and can be
    broken to interrupt the attack (`SpawnProjectile`, :954-963).
  - `DamageStart` opens the parry window (:965-984).
  - Cooldown: 1 to 2.5 s at random after each attack, and 1 s after it first spots the player (:655, 929).
- **Swordsmachine**
  - `HeatSword` makes the blade glow (a heat material and a trail), spawns a flash at the head, and opens the parry window
    in the same moment (:1026-1043). A sword throw spawns `gunFlash` instead (:1045-1072).
  - How it picks an attack by range: shotgun beyond 5 m (phase 1, then 5 to 10 s between bursts); sword throw beyond 20 m
    (beyond 10 m if uncharged); combo under 5 m; a running swing at 8 m or less with a 3 s cooldown (:661, 668, 698,
    736-740, 776).
- **Cerberus (StatueBoss)**
  - Each attack has its own charge sound at its start (stomp, tackle, throw) (:695-735).
  - It checks for an attack every 0.2 s (:465). A melee attack needs `meleeRecharge >= 2`.
  - The tackle chance moves by plus or minus 20 after each pick, which pushes it to alternate stomp and tackle (:471-492).
  - It throws at 9 m or more.
  - Brutal adds an extra tackle: the animation runs at 0.1 speed and a 5x `unparryableFlash` shows, with 0.5 s before it
    lunges (:968-980).
- **Malicious Face (SpiderBody)**
  - The beam charges at 0.5 per second, so 2 s (1.33 s on Brutal). The orb in its mouth grows to 2.5x, its light rises to
    30, and its pitch climbs (:681-697).
  - It then locks aim at `position + velocity*0.5 s`, shows a spark pointing at that aim point, and **fires 0.5 s later**
    (0.75 s on Lenient, 1 s on Harmless). It is parryable during that wait (:699-725).
  - Projectile bursts: one shot every 0.1 s, then a 1 s cooldown (:287, 659).
- **Mindflayer**
  - A wind-up sound object plays for each attack (:805-830).
  - The melee attack shows an 8x `warningFlash` and opens the parry window at `SwingStart` (:832-836). It melees when the
    player is closer than 5 m (:414).
  - The beam aims at the player's position plus `velocity*2.5`, shows an 8x unparryable flash, and **draws a preview line
    of the beam** before firing (:851-892).
  - After a homing volley it waits 4 to 5 s (:987).
  - **Camping is punished:** after 10 s without a melee (15 s on Standard) it teleports close, and it melees if the player
    is within 8 m (:418-426). After 3 s out of sight it teleports (:451-456).
- **V2**
  - Before a revolver volley, a 20x `gunFlash` shows, then the shots come at **0.75, 0.95 and 1.15 s** (V2.cs:921-933,
    1041-1044).
  - The alt-fire charges for 1 s (1.25 s Lenient, 1.5 s Harmless) (:877-896). Other weapons fire after 0.75 s (:942-951).

## V2 (V2.cs): its whole kit
- **Weapon by range:** shotgun at 15 m or less, nailgun under 25 m, revolver beyond that (:815-828).
- **Movement patterns:** Straight, Circle, Chase or Coward (V2Pattern.cs). It switches pattern every 2 to 5 s at random
  (:1387-1402), and **its wing trail colour changes with the pattern** (:1546-1552).
  - **Circle** moves at 90 degrees to the player: 80 degrees beyond 10 m, 100 degrees under 5 m. That spirals it back
    toward about 5 to 10 m (:485-499).
  - **Chase** slides when it faces the player and the player is more than 20 m away (:500-509). Brutal adds random slides.
  - It jumps when a wall is within 4 m and wall-jumps off walls (:532-550).
- **Keeping its distance:** under 10 m, `closeRangePatience` drains. When it runs out, V2 force-dodges away and switches
  to the **Coward** pattern (running away) until patience is back above 2 (:553-575).
- **Dodging the player's shots:** only when it is more than 15 m away. The dodge cooldown is `6 - difficulty` s, and the
  dodge speed is `movementSpeed*5`, decaying over about 0.33 s (:1417-1452, 1194). Otherwise it may slide or jump (:1436-1452).
- **Enrage against distance and camping:** while the player is more than 15 m away or out of sight, `distancePatience`
  rises 2 per second up to 12. At 4 it switches to Chase and moves faster (x1.5 to x3). At 12 (6 s) **it enrages**. It calms
  down below 10 (:708-731). The patience is shown on its health bar's second bar (:785-805).
- **Coins (second encounter only):**
  - It **shoots the player's coins**. It picks the nearest unshot coin in line of sight within 60 m. After a delay that
    depends on difficulty (0 to 0.8 s) it flashes, then fires 0.4 s later (:962-1039, 680-705).
  - A shot through the player's coin chain comes back at the player as a "COUNTER" ricochet (Coin.cs:1179-1232, 1347).
  - It also throws its own coins: 3 (1 on Lenient), 0.2 s apart, 50 % of the time when it is 8 m or more away
    (:871-876, 1046-1067).
- **The phase transition** is mostly driven by the scene. In code: at "death", with `dontDie` set, it is knocked out
  ("KnockedDown" in the first encounter, "Flailing" in the second) (:1560-1600). The first encounter then runs to an
  `escapeTarget` from an animation event (`BeginEscape`, :1671-1684; V2AnimationController.cs:22-24). Health thresholds
  live in the level, not checked.
- **What the player is meant to do (inference):** stay mid-range and in sight. Long range or hiding makes it chase and
  enrage. Hugging it makes it dodge and run. Hitting the coin flash window is the counter-play.

## 4. Arena flow
- **Locking:** a trigger, or the arena being enabled, starts the arena music, sets every door active and calls `Lock()`
  (ActivateArena.cs:79-118).
  - A locked door closes by force, turns its lights **red**, shows a `noPass` blocker, and plays a sound at pitch 0.2
    (Door.cs:705-758).
  - **Enemies start appearing 1 s after the doors lock, then one more every 0.1 s.** They are placed in the scene ahead of
    time and switched on (`SetActive`) (ActivateArena.cs:107-110, 126-151). There is no check for whether they are in
    view.
- **The spawn effect** (`spawnIn`, EnemyIdentifier.cs:773-777; SpawnEffect.cs): a bubble that shrinks at 2 per second, a
  light whose range shrinks at 50 per second, and a sound at a random pitch within plus or minus 0.1.
- **Waves:** in `FixedUpdate`, once `deadEnemies >= enemyCount` (the designer's child count), the next wave starts after
  1 s, again one enemy every 0.1 s (ActivateNextWave.cs:31-91).
  - After the last wave there is a **0.15 slow-down**, then the doors unlock one every 0.1 s, the door forward opens, and
    the arena music ends (:68-142).
  - Unlocking plays a sound at pitch 0.5 and restores the light colour (Door.cs:762-784).
- **Doors:** an unlocked door opens when the player or an enemy is inside its `DoorController` trigger, and closes when
  they leave (DoorController.cs:48-122).
- **Checkpoints:** taking one saves style points and zeroes the kill counter (CheckPoint.cs:280-312). A restart:
  - re-creates the rooms past the checkpoint from saved copies,
  - removes the kills made since the checkpoint,
  - restores the style points (CheckPoint.cs:370-423, 522-548),
  - and counts restarts at the same checkpoint (`sameCheckpointRestarts`; on Lenient, more than 2 raise the health cap to
    200, NewMovement.cs:2100-2103).

## 5. Walls (NewMovement.cs `Cling`, :820-858; called from `Update` while in the air, :697)
- **Clinging is a slide, not a hold.** It applies only when all of these are true:
  - a `WallCheck` touches a wall,
  - the move input points into a wall within 1 m,
  - the player is not sliding and not in a heavy fall,
  - the player is **already falling faster than 1 m/s**.
- **The slide speed:** fall speed is set to `2 * clingFade`. `clingFade` grows 4 per second up to 50, so the slide starts
  near 0 and gets about 8 m/s faster every second, capped at 100 (:844-847).
- **`clingFade` resets only on the ground or on a stomp off an enemy** (:637, 2230). The grip therefore gets weaker over
  one airtime, across every wall touched.
- **There is no upward motion from touching a wall.** The only lift is the wall jump (3 per airtime, from the wave-34
  note).
- A wall-scrape particle sits at the contact point (:845).
- **Possible bug in the horizontal clamp:** the code reads `Mathf.Clamp(-1f, 1f, dot)` (:842-843). Unity's argument order
  is `(value, min, max)`, so as written this returns 1, not a value clamped to plus or minus 1. The intent looks like
  capping horizontal speed to 1 m/s; not checked in play.

## 6. Healing and hard damage
- **Blood heals by touch, not by a radius test.** Each gore splatter has a trigger collider that stays live for
  **0.25 s** after it spawns (2.5 s underwater) (Bloodsplatter.cs:92-146).
  - A player inside it gets `hpAmount` once (Bloodsplatter.cs:163-176). The amount is set per gore type and the collider
    size is an inspector value; neither was checked.
  - Splatters heal only if the enemy-damage code marked them ready (`GetReady`, EnemyIdentifier.cs:1475, 1617, 1991).
  - Blood particles that collide with the player give 3 HP each (Bloodsplatter.cs:220-222).
- **Hard damage** applies from Standard difficulty up:
  - **35 % of each hit** becomes `antiHp`, capped at 99 (NewMovement.cs:1907, 1949-1968).
  - Its cooldown gets +1 (if it was 0), +1 more on Violent and up, plus `damage/20`.
  - Once the cooldown ends, hard damage drains at 15 per second, or `rank*10` per second at style rank 4 or higher. The
    cooldown itself drains faster at rank 4 or higher (:785-812).
  - At **ULTRAKILL rank (7)** hard damage is cleared at once (:786-790).
  - Healing is capped at `100 - antiHp` (:2093-2117).

## 7. Enemy caps per wave
- **The campaign has no cap in code.** A wave is whatever children the designer placed (`enemyCount = childCount`,
  ActivateNextWave.cs:23-26).
- **Cyber Grind (EndlessGrid.cs):**
  - The point budget starts at 10, and each wave adds `3 + wave/3` (:537-554).
  - Uncommon enemies appear from wave 12 (:961).
  - Per-type caps: Stalker and Deathcatcher `min(wave/8, 3)`, Idol `min(wave/8, 5)`, Turret `min(wave/5, 6)`, Gutterman
    `min(wave/15, 5)`, Guttertank `min(wave/20, 3)`, Virtue `min(wave/5, 8)` (:1107-1150).
  - Enemy spawn costs are inspector values, not checked.
