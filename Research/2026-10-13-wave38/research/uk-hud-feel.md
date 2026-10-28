# ULTRAKILL: HUD readiness, crosshair, shake, hurt screen, walls, doors (wave 38)

Source: ULTRAKILL's Assembly-CSharp.dll decompiled with ilspycmd (scratchpad `uk/`). Numbers and rule shapes only, cited
`File.cs:line`. Inspector values (colours set on prefabs, trigger sizes, screen positions) are **not checked**. Earlier
notes cover coins, parry, arena locking and the wall cling in detail (wave-34 `rea-ultrakill.md`, wave-36
`rea-ultrakill-combat.md`). This note confirms them where they overlap and adds the HUD and feel side.

## 1. How "ready" and "charging" look
The same pattern shows up everywhere: **one dull colour while charging, a different colour when full, and on the frame
it fills, a flash to white that fades and a sound.**

- **Dash stamina** (StaminaMeter.cs, NewMovement.cs):
  - The pool is `boostCharge` 0-300: three dashes of 100. It refills at 70/s (x1.5 on difficulty 1, x2 on difficulty 0)
    and not while sliding (NewMovement.cs:754-766).
  - The bar does not jump to the value. It eases toward it at `(gap*25 + 25)` per second, and a slower
    `(gap*5 + 10)` when the level starts (StaminaMeter.cs:70-85).
  - Under max the bar uses `staminaChargingColor`, or `staminaEmptyColor` on the variant that turns red when empty. At
    max it uses `staminaColor` (:110-115, 171-191).
  - When it fills: a flash image goes white at alpha 1 and fades at 1 per second, and a sound plays (:94-107, 151-167).
  - A failed dash (under 100) spawns `staminaFailSound` (NewMovement.cs:1051-1058). Its `StaminaFail` component flashes
    **every** stamina meter **red** (StaminaFail.cs; StaminaMeter.cs:161).
  - The text version shows `x.xx` dashes ("2.35") and is **red under 1.00** (:123-145).
- **Railcannon** (RailcannonMeter.cs, WeaponCharges.cs):
  - `raicharge` fills 0-5 at 0.25/s. Once it reaches 4 it snaps to 5 and plays the full-charge sound, so a recharge
    takes **16 s** (WeaponCharges.cs:190-197).
  - While charging, fill = `raicharge/4` in `railcannonChargingColor` (RailcannonMeter.cs:86-91). When full, fill = 1 in
    `railcannonFullColor`, with a one-time flash lerped from white that decays at 1/s (:51-80). The meter background
    hides once it is full (:93-100).
  - With the weapon icon turned off, a "mini version" shows instead (:138-147). Where it sits: not checked.
- **Revolver piercer charge**: shown **on the gun's own screen**, not the HUD (Revolver.cs `screenProps`/`screenMR`).
  - Below 50 the screen shows a red battery-low texture, below 100 a yellow mid one, and at 100 a full one in the
    variation colour. The full state plays `chargedSound` (volume 0.35, pitch 1-1.1) (Revolver.cs:282-306, 479-482).
  - The charge refills at 40/s (half on the alt revolver), so 2.5 s (:270-279).
  - Holding to charge a shot takes 175/s, about 0.57 s (:351, 385-392). While charging:
    - the gun jitters by up to `charge/250 * 0.05` (:335),
    - the cylinder spins at the charge value (:336),
    - a light's range grows to `charge*0.01` and an effect's scale to `charge*0.02` (:456-466),
    - the sound's pitch rises to `charge*0.005` and its volume to `0.25 + charge*0.005` (:467-469),
    - pad rumble runs at `charge/250` (:472).
- **Coin / spin charges**: panels on a canvas **on the gun** (Revolver.cs:750-786).
  - Marksman: 4 panels (`rev1charge` 400, 25/s). Sharpshooter: `rev2charge` 300, 15/s (35/s alt).
  - Each panel fills by `charge/100 - i`. A panel that is not full is **red** on the marksman and **grey** on the
    others. A full one takes the variation colour.
  - As each panel fills, a sound plays at pitch `1 + i/2`, so it **rises with each coin**.
- **Fist**: the fill is `punchStamina/2` (2 charges, refilling at 1.25/s, WeaponCharges.cs:257-260). Below 1 charge it is
  60 % as bright, and its background is the icon colour x0.2 (HudController.cs:201-207).
- **Weapon icon**: the sprite plus a glow sprite, both tinted with the variation colour. Nothing shows ready or charging
  (WeaponHUD.cs:20-34).
- **Weapon wheel**: time slows to **0.25** while it is open (WeaponWheel.cs:34-35). It has N equal arcs with a 0.005 gap
  and icons at radius 112 (:149-182).
- **Crosshair HUD arcs** (the optional ring around the crosshair):
  - The arcs copy the health and stamina sliders' fill and colour (SliderToFillAmount.cs).
  - With "crossHairHudFade" on, they stay solid for 1 s after the last change, then fade out over 1 s (fadeOutTime 2 ->
    0 at 1/s unscaled; alpha = min(t, 1)) (FadeOutBars.cs:43-65).
- **The whole HUD sways with velocity:** the target offset is `-localVelocity/1000`, eased at `15*distance` per second
  (NewMovement.cs:776-780).

## 2. The crosshair
- It is **static**: no spin, scale, bloom or change on fire, reload or charge (Crosshair.cs is only settings: 3 types,
  10 colours, and colour 0 = invert material, :49-176).
- The only motion: with auto-aim on, it **moves to the locked target's screen position** and goes back to the centre
  when there is no target (CameraFrustumTargeter.cs:371-383).
- Charge feedback lives on the gun model (section 1), not on the crosshair.

## 3. Screen shake (CameraController.cs:247-276, 477-489)
- **Not additive:** `CameraShake(a)` sets `shake = max(shake, a * pref)`, and a pref of 0 disables it.
- **Decay:** linear, 3 per second of **unscaled** time, so a 1.0 shake lasts 0.33 s and a 3.0 lasts 1 s.
- **The size rule:** under 1, the offset is `shake * random(-1..1)` on the camera's right and up axes. **Above 1, it
  jumps by whole units (-1, 0 or +1 m on each axis).** That is the cap on how far the camera moves, so 2 and 3 only last
  longer.
- The camera never shakes into a wall: a raycast pulls it back 0.5 m from the hit (:266-273).
- `StopShake()` zeroes it when a revolver fires or throws a coin (Revolver.cs:415, 553).
- **Who calls it, with what:**

| Event | Amount | Source |
|---|---|---|
| Player hurt | `damage/20`, integer division: under 20 dmg = 0, 20-39 dmg = 1 | NewMovement.cs:1938 |
| Fake hurt | 0.1 | NewMovement.cs:1898-1899 |
| Wall or slide scrape | 0.1 per frame | NewMovement.cs:884-888 |
| Ground-slam shockwave | 0.75 | GroundCheck.cs:143 |
| Parry | 0.5, plus a 0.25 s TrueStop and a 0.1 s flash | TimeController.cs:140-149 |
| Explosion within 3 x size | 1.5 (x0.25 if it does no damage) | Explosion.cs:108-112 |
| Explosion from there to 85 m | falls off linearly x size/6 | Explosion.cs:113-116 |
| Shotgun blast / hammer | 1.0 | Shotgun.cs:692; ShotgunHammer.cs:1158 |
| Railcannon | 2.0 | Railcannon.cs:266 |
| Revolver beam | 2.0 / 0.25 (by beam type) | RevolverBeam.cs:314-318 |
| Rocket fire | 0.75 | RocketLauncher.cs:433, 497 |
| Punch | 0.5 x a multiplier | Punch.cs:607 |
| Enemy death (some) | 1.0 | EnemyIdentifier.cs:1506 |
| Enrage | 1.0 | EnrageEffect.cs:21 |
| Minos (corpse) arm slam | 2.0 | MinosArm.cs:249 |
| Minos swing end / death impact | 2.0 / 3.0 | MinosBoss.cs:326, 337 |
| Minotaur slams | 1.5-3.0 | Minotaur.cs:640, 684, 702 |
| Geryon | 3.0 | Geryon.cs:471 |
| Maurice beam | 2.0 | MaliciousFace.cs:1082 |
| Gabriel intro | rises toward 0.25 (2 x 0.125) while it shakes | GabrielIntro.cs:44-50 |
| Doors | 0.05 x how open, every frame while moving | Door.cs:359; BigDoor.cs:84 |
| Placed shakers | `amount`, fading to 0 between minDistance and maxDistance | Screenshaker.cs:44-69 |

- **Boss intro flash:** none found in code. Intros are animations plus the shakes above. The boss bar fills in at
  `(target - fill)*3` per second (BossHealthBarTemplate.cs:193-199).
- **The boss bar shakes when the boss is hit:** `shakeTime = 5 x HP lost`, capped at 10, decaying 10/s, with a jitter of
  plus or minus shakeTime in UI units (:209, 251-265).

## 4. The hurt screen (NewMovement.cs:471-480, 746-750, 1925-1939)
- One overlay colour: the `hurtScreen` Image's own colour, which is set in the inspector (not checked). It is pushed
  every frame as the global shader colour `_HurtScreenColor`, so a post or UI shader draws it. How that shader looks
  (scanlines, static) is not checked; nothing in C# animates it beyond the alpha.
- **Alpha on a hit:** 0.5, or 0.8 for 50+ damage (0.25 for FakeHurt). It decays at **1 per second**.
- **The invincibility after a hit equals that alpha in seconds** (0.5 s or 0.8 s, :1933-1936). The overlay is the
  i-frame timer made visible.
- It comes with the shake of `damage/20` above and a sound at pitch 0.8-1.0.
- No "WhiteHurt" exists. `ScreenDistortionController` is unrelated: it drives `_RandomNoiseStrength` and the
  `WickedEffect` post effect from distortion fields in the level (ScreenDistortionController.cs:17-44).

## 5. Walls (confirms the wave-36 note: NewMovement.cs:820-858)
- **You can stay on a wall indefinitely, but always sliding down**, and faster over time:
  - the fall speed is set to `2*clingFade`,
  - `clingFade` grows 4/s up to 50, so the slide gains about **8 m/s per second**, capped at 100 m/s,
  - it resets only on the ground or on a stomp off an enemy (:637, 2230).
- The cling needs input into a wall within 1 m and a fall already faster than 1 m/s.
- **No vertical control on a wall** apart from the wall jump (3 per airtime, :1038).
- **New detail:** while *sliding* in `FixedUpdate`, touching a wall while falling adds `+0.4 x gravity` upward, so a slide
  into a wall falls at 60 % gravity (:1204-1230).

## 6. Doors (confirms the wave-36 note; Door.cs, DoorController.cs, DoorBlocker.cs)
- **Locked:**
  - the emissive light colour goes to **pure red**, or to black on doors flagged `turnEmissiveOffWhenLocked`,
  - a `noPass` blocker is switched on,
  - the door force-closes, with a sound at pitch 0.2 (Door.cs:705-758).
- **Unlocked:** the colour goes back to the material's own default (whatever it was authored with), with a sound at
  pitch 0.5. It opens itself if `openOnUnlock` is set (:762-784).
- **Open on approach:** there is no distance number. The door opens while the player, or a Machine-class enemy (not a
  Drone), is inside the `DoorController` trigger, and closes when they leave (DoorController.cs:54-124, 128-170). The
  trigger's size is set in the inspector: not checked.
- **Movement:** `MoveTowards` at `speed`. With `easeIn`, the speed is `min(speed, d*2*speed + speed/50)`, so it slows
  near the end. Sound volume and pitch scale with how open it is (Door.cs:346-364).
- **`DoorBlocker`:** a door that hits a blocker closes with a sound, and reopens when the blocker is destroyed
  (DoorBlocker.cs).
