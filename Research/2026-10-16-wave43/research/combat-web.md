# Combat feel: parry / reflect reach, impact feedback, haptics, flamethrower (wave 43 research, for G17-G20)

Web + GitHub research, 2026-10-16. Adds to wave 42 `hitfeel-web.md` (hitstop tiers, Nuclear Throne, style meter),
wave 39 `feel-web.md` (Sekiro window, fighting-game hitstop), wave 34 `rea-ultrakill.md` and wave 38 `uk-hud-feel.md`
(ULTRAKILL punch geometry, shake decay); those are cited only where a new rule needs them. "[code]" = read in source this
session; "[wiki]" = fetched page; "[review]" = reviews / guides (weak). SKYGRIND values are the web base values in
`PortExport/config.json` (Unity rows may override: not checked).

## A. Parry and projectile reflect (G18)

1. **ULTRAKILL reaches projectiles far beyond its fist.** The punch parries a projectile on a 4 m ray OR a 0.6 m box
   cast, and catches nearby projectiles in a 3 m-radius sphere centred 3 m ahead (Punch.cs:562-565, 597, 697; wave 34
   `rea-ultrakill.md`). Patch 1.03b changed the projectile parry "from a raytrace into a hitbox ... a bit easier to punch
   at an angle" and added "3 mercy frames between a projectile hitting the player and the damage happening".
   https://hakita.itch.io/ultrakill-prelude/devlog/98473/103b-patch-notes
   - for SKYGRIND: our `orb.reflectRange` = 2 m and `weapons.gauntlet.range` = 2.4 m with a 35 deg cone are a third of
     ULTRAKILL's reach (a 6 m deep catch volume). G18's "wait until extremely close" is this number. Give the reflect
     its OWN volume (a sphere of r ~ 2.5-3 m centred 2.5-3 m ahead, plus a ray to ~5 m along the aim) separate from the
     melee damage cone.
2. **ULTRAKILL also buffers early presses.** A punch that touches an enemy up to 6 physics frames (~0.12 s) before its
   parry window opens still parries when the window opens (wave 36 `rea-ultrakill-combat.md`, Enemy.cs:2698-2842).
   - for SKYGRIND: store a "reflect armed" state for 0.12 s after the gauntlet press; any projectile entering the catch
     volume during it is reflected (the press does not have to coincide with the overlap step).
3. **Windows elsewhere (ms).** Nine Sols perfect parry: 8 frames = 133 ms at 60 fps; a popular mod widens it to 12
   frames (200 ms) "to make Nine Sols parrying feel like Sekiro deflecting". [mod docs]
   https://thunderstore.io/c/nine-sols/p/Supidowagon/PerfectParry/ ; Sekiro: 12 frames (200 ms) per one forum claim,
   ~30 / ~7 frames per another (wave 39). Doom: The Dark Ages ships a parry-window SLIDER and reviewers call the default
   "generous" (no ms published). [review] https://gameinformer.com/review/doom-the-dark-ages/believe-in-the-shield
   Hi-Fi Rush's Rhythm Assist widens timed-input windows by a BPM-dependent number of frames. [review]
   https://caniplaythat.com/2023/04/11/hi-fi-rush-update-addresses-qte-accessibility/
   - for SKYGRIND: 150-200 ms of "time to impact" is the genre band for a deliberate parry; express the window as time
     (distance / closing speed), not metres, so fast and slow orbs feel the same. Expose it as an assist option later.
4. **Projectile hitboxes are larger than their visuals in friendly shooters.** Overwatch's projectiles (Hanzo, Genji,
   Mei) are singled out as the most forgiving, hitting "nearly a head's worth of space above" a head. [article, 2016]
   https://kotaku.com/a-closer-look-at-overwatchs-generous-headshot-hitboxes-1780953233 ; the mirror rule for dodging:
   one indie made "the hitboxes of the projectiles and the player ... slightly smaller than what a player might expect"
   so dodges feel fair. [comment] https://itch.io/post/9400766 (so: big for parry, small for getting hit)
   - for SKYGRIND: give each parryable orb a "parry radius" ~2x its damage radius, and draw the parry-ready glint when
     the orb ENTERS the catch volume, so the player sees "now" at the distance the game accepts.
5. **The reward makes the window worth learning.** ULTRAKILL: 0.25 s TrueStop, shake 0.5, a 0.1 s white flash, full
   stamina, heal (wave 34/42). SKYGRIND: `punch.parryHitstop` 0.06 s, flash 0.08 s at 0.45, heal 15, pad
   `[0.45, 0.2, 70]` (low, high, ms).
   - for SKYGRIND: a projectile reflect is our "major" event; 0.06 s is a "minor" tier. Try 0.15-0.25 s on a reflect,
     with the reflected orb leaving in the hot-pink "combined" colour (G19) so a reflect reads as the same family.

## B. Impact feedback stack (G19, G20)

6. **Trauma model (Eiserloh, GDC 2016 "Juicing Your Cameras With Math").** shake = trauma^2 (or ^3); trauma in 0..1, added
   per event, decays linearly; offsets = smooth noise x shake x max. Bevy's reference: decay 0.5 /s, exponent 2, max
   roll 10 deg, max translation 20 px, noise speed 20, +0.4 per event. [code]
   https://bevy.org/examples/camera/2d-screen-shake/ ; talk video: https://www.gamedeveloper.com/programming/video-sprucing-up-cameras-with-math
   - for SKYGRIND: ULTRAKILL takes max() of shakes (not additive, decays 3/s, wave 38). Use trauma for the empowered
     impact: +0.6 on a high-speed dash impact, +0.3 on a charged shotgun, +0.1 per flame tick capped at 0.35, so a
     normal hit and an empowered hit differ by 4x in shake (0.6^2 / 0.3^2) for free. In first person, rotate (roll /
     pitch) more than translate.
7. **Layer timing.** Sound + vibration at t0, flash +20 ms, particles +50 ms, text +100 ms; flash 50-100 ms. [blog,
   wave 42] https://eastondev.com/blog/en/posts/dev/20260521-game-feedback-feel/ ; Smash vibrates the hit actor during
   hitstop (wave 39 critpoints).
   - for SKYGRIND: during the empowered-impact hitstop (0.1-0.15 s), shake the ENEMY mesh, not the camera, then release
     the camera trauma on the first running frame; the HUD (health frame, enemy counter) pulses hot-pink on the same frame.
8. **Gamepad rumble in Unity.** `Gamepad.SetMotorSpeeds(low, high)`: "the left motor emits a low-frequency rumble, and the
   right motor emits a high-frequency rumble"; `PauseHaptics` / `ResumeHaptics` / `ResetHaptics` (pause in menus).
   Rumble works for Xbox pads on Windows, PS4 pads on Windows / Mac, and console pads on their consoles (with console
   packages); the desktop Switch Pro HID has no rumble listed. [docs]
   https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/manual/Gamepad.html
   `DualSenseGamepadHID` adds `SetLightBarColor(Color)` and `SetMotorSpeedsAndLightBarColor(low, high, color)`; no
   adaptive-trigger API. [docs] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.11/api/UnityEngine.InputSystem.DualShock.DualSenseGamepadHID.html
   Xbox GameInput has 4 motors (low, high, left trigger, right trigger, each 0..1); trigger motors are not reachable
   through XInput. [docs] https://devdocs.xbox.com/reference/input/gameinput/structs/gameinputrumbleparams
   - for SKYGRIND: three haptic "words": a THUD (low 0.8, high 0.2, 60-90 ms) for impacts and the shotgun, a BUZZ
     (low 0.15, high 0.5, held) for sustained fire (flamethrower), a CRACK (low 1.0, high 1.0, 40 ms then low 0.6
     decaying over 200 ms) only for the empowered impact and boss-vulnerable hits. On a DualSense, flash the light bar
     hot-pink on the empowered impact (cheap, unique to it). Rumble must keep running in real time through hitstop.
9. **Insomniac on DualSense:** haptics scale "from explosions all the way down to footsteps"; firing is the obvious use,
   subtler cues layered on top; Miles Morales hints attack direction by haptics. [dev blog]
   https://blog.playstation.com/2021/05/13/devs-reveal-their-upcoming-dualsense-wireless-controller-implementations/
   - for SKYGRIND: a per-event haptic table (name, low, high, ms) in a Unity-side config row, next to the event names
     GG's audio hooks use, keeps sound and haptics on the same beats.
10. **Hitstop reference values.** ULTRAKILL: punch hit 0.1 s, revolver beam / grenade 0.05 s, parry 0.25 s (TrueStop),
    enemy projectile 0.005 s; last enemy of a wave = SlowDown(0.15) recovering by 0.02 per physics step (wave 34).
    - for SKYGRIND: tiers: bullet 0 / 1 step, shotgun 2-3 steps, charged shotgun 6 steps (0.05 s), gauntlet 12 steps
      (0.1 s), empowered dash impact / boss-vulnerable hit 18-30 steps (0.15-0.25 s). All in whole 120 Hz steps.

## C. Sustained weapons: the flamethrower (G20)

11. **Doom Eternal's Flame Belch is a resource verb, not a DPS tool.** Burning demons "take additional damage and drop
    armor shards", a kill while burning drops more; cooldown 25 s (12.5 s upgraded). [wiki]
    https://doomwiki.org/wiki/Flame_Belch
    - for SKYGRIND: a flame weapon feels strong when the TARGET changes state: burning enemies get an orange-white rim,
      emit embers, stagger on ignite and pay a reward (points / heal) on a burning kill. Damage numbers alone don't sell it.
12. **"Stats fine, feels weak" is a known diagnosis.** Playtesters on an indie flamethrower: "more than okay in terms of
    stats, it just lacks satisfying VFX"; "not powerful enough when firing". [comment] https://itch.io/post/13338396 ;
    VFX timing advice: damage effects "happen very quickly ... then fade out slower", add "flash or dust puff". [forum, search summary only; page not fetched]
    https://realtimevfx.com/t/vfx-blue-fire-burst-with-spear/24180
    - for SKYGRIND: the flame needs (a) a muzzle that lights the viewmodel and the near walls (a point light flicker
      0.8-1.2x), (b) a cone of large soft particles that collide with and spread over surfaces, (c) a heat shimmer, (d) a
      ramp-up: the first 0.2 s of fire grows reach and haptic from 50 % to 100 %, so holding it feels like building power.
13. **Charged shot = anticipation then release.** Wave 40 `feel-web.md` covered charge timing; the release frame needs the
    biggest single spike: hitstop 0.05 s, trauma +0.3, a ring decal on the floor, and enemies launched (ULTRAKILL's
    Knuckleblaster "launches light/medium enemies", wave 42).

## D. The empowered impact (G19), derived
- Condition: player speed at contact >= the dash's speed band (or a gauntlet hit within 0.15 s of a dash). Response, all
  on one step: hitstop 0.15-0.2 s; enemy mesh shake during it; a two-colour burst (cyan player energy + orange enemy energy
  meeting in a hot-pink core, then expanding rings); CRACK haptic; trauma +0.6 on release; a 1-frame chromatic split;
  HUD frame pulse; the named event for GG's audio. RIVAL's magenta stays distinct by SHAPE (rings and sparks, never
  RIVAL's beam / silhouette) and by DURATION (under 0.4 s). GG's ruling on rule 8 is DECIDE43's.

## Not found
ULTRAKILL's parry window in ms; Doom: The Dark Ages default parry window in ms; Sony / Microsoft public haptic guidelines
for sustained fire (registered-developer docs only); a developer article on flamethrower feel.
