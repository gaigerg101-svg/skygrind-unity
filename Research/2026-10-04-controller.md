# Controller, Steam Deck and accessibility for SKYGRIND

Research for the lead, 2026-10-04. Web research only: no repo file was read except an earlier research note (for its
format), only this file was written, and no git, build or Unity command was run. Tags: **[W]** read in a fetched source;
**[S]** search snippet only (or the fetch returned no body); **[U]** my inference for SKYGRIND; **[B]** from the brief
(the web build's pad design), not re-checked in the web repo. The web-search quota ran out partway, so Halo Infinite,
Titanfall 2, Roboquest and Turbo Overkill rest on snippets or are "not verified". Nothing was measured on hardware.

---

## 0. Bottom line (ranked for SKYGRIND)

1. **Accept mouse and stick look in the same frame.** Valve's Deck guidance asks for "mixed input" (mouse and joystick
   camera together, neither locking the other out), and Steam's gyro-to-mouse needs it too **[W]**. Swap prompts by the
   last device **[B]**, but never gate look input on it **[U]**.
2. **Input System for all gameplay; Steamworks only for pad identity and glyphs.** Under Steam Input the game sees an
   emulated Xbox pad **[W]**, so Unity alone shows Xbox prompts for a DualSense **[W]**; `GetInputTypeForHandle` and the
   glyph calls fix that **[W]**. The Input System has no maintained Steam Input integration (2024 forum thread, no staff
   answer) **[W]**.
3. **Latch button edges across the 120 Hz steps.** The default update mode processes events once per frame, before
   `Update` **[W]**: a press read per step fires 3 times at 40 fps and is lost on a frame with 0 steps (>120 fps)
   **[U]**. Latch in `Update`, consume in the first step; don't use Fixed mode (the sim isn't in `FixedUpdate`) **[U]**.
4. **Don't stack Unity's dead zone on the web's.** The default stick dead zone is 0.125 / 0.925 and the gamepad docs say
   the default stick processors use it **[W]**. To keep the web's 0.15 / 0.95 scaled radial exact, read the unprocessed
   value or neutralise the default (needs a check) **[U]**.
5. **Deck Verified is mostly glyphs and defaults:** default config reaches all content; glyphs match the device ("Deck,
   Steam Controller, or Xbox glyphs") with no key / mouse glyphs while inactive; text ≥ 9 px at 1280x800 (12
   recommended); 30 fps at 800p by default **[W]**.
6. **Add a PlayStation glyph set and lay glyphs out by position.** PlayStation is 26 % of Steam controller sessions
   (Xbox 59 %, Deck 10 %) **[W]**. The neutral A/B/X/Y set **[B]** matches Xbox and Deck labels; Nintendo layouts put
   the same letters elsewhere **[U]**.
7. **Keep the snap assist, shaped the 2024+ way:** weaker at point blank, rising with range (Black Ops 6) **[W]**; full
   rotational pull only while the right stick moves toward the target (Black Ops 7) **[S]**; strength / window / ease-in
   sliders (Overwatch 2) **[S]**. SKYGRIND is PvE; Respawn calls aim assist "a critical accessibility feature" **[W]**.
8. **Raise the text floor from 16 to 18 px at 1080p, or ship text scale to 200 %** (XAG PC minimum) **[W]**. 16 px
   scaled to 800p is ~11.9 px: passes Deck's 9 px, just under its 12 px recommendation **[U]**.
9. **Copy DOOM: The Dark Ages' assist and motion set**: game speed 50-150 %, parry-window slider, enhanced targeting,
   separate shake / weapon bob / motion blur, parry recolour, font and HUD scale **[W]**, praised by Can I Play That?
   **[W]**. DOOM Eternal got 1/4 for fine motor over platforming load and hold-only actions **[W]**: SKYGRIND's risk.
10. **Remap actions, not only swap buttons.** XAG 107: remap every control in game, invert X and Y per stick, ideally
    "assign an action to all potential game inputs, as opposed to simply swapping" **[W]**; the web's swap **[B]** is
    short of that.
11. **Deck: aim for 60 fps at 800p with a 40 fps cap option.** 30 fps is the floor **[W]**; the 120 Hz sim with render
    interpolation **[B]** decouples render rate **[U]**; the APU runs 4-15 W **[W]**; 40 Hz / 40 fps saves battery
    **[S]**.
12. **Gyro later, through Steam Input.** The Input System doesn't read DualShock / DualSense gyro outside PlayStation
    consoles and doesn't support the Switch Pro natively on Windows **[W]**; Steam's gyro-to-mouse is free once item 1
    holds **[U]**.

---

## 1. How many PC / Steam players use controllers

- **2021 (Valve, via Game Developer):** ~10 % of daily Steam sessions used a controller; by genre FPS "7-8 %",
  third-person adventure 40-50 %, sports / fighting >70 %, racing / skating >90 %, RTS <1 % **[W]**. Steamworks then
  added per-game controller reports (daily controller share, sessions per pad type, history) **[W]**.
- **June 26, 2024 (Valve's "An update on Steam Input and controller support", via Game Developer):** daily controller
  use tripled from ~5 % (2018) to up to 15 %; ~42 % of controller sessions use Steam Input; Xbox 59 %, PlayStation
  26 %, Steam Deck 10 %; PlayStation's share grew from 11 % (2018) to 27 %; Valve suggests listing PlayStation support on
  the store page **[W]**. "48 million players have used a controller" is a PC Gamer headline **[S]**.
- **Deck share:** Linux was 3.9 % of the August 2026 Steam survey and SteamOS 21 % of Linux **[W]**, so ~0.8 % of
  surveyed users run SteamOS (Deck plus Steam Machine) **[U]**; Linux peaked at 5.33 % in March 2026 **[S]**.
- No Valve figure for controller use in Next Fest demos was found (not verified).
- **[U] For SKYGRIND:** expect at least the genre's 7-8 % on a pad (a 2021 figure; the total has grown since), and every
  Deck player is a pad player. The Deck badge shows on the store page whatever the share, so the pad pass has to be
  good enough for reviewers, not merely present.

## 2. Steam Deck Verified, and the Deck as a target

| Area | Requirement (Steamworks `steamdeck/compat`, all **[W]**) | How to meet it in Unity **[U]** |
|---|---|---|
| Input | "must support Steam Deck's physical controls"; "The default controller configuration must provide users with the ability to access all content." | Every menu works on the pad from boot; set the Steamworks Steam Input default to a Gamepad template, or bundle one. |
| Glyphs | "On-screen glyphs must match the inputs being used, whether it's Deck, Steam Controller, or Xbox glyphs." "Mouse and keyboard glyphs should not be shown if they are not the active input." | Hot-swap by last device; the Deck appears as an Xbox pad, so Xbox or neutral letters match. |
| Text input | A Steamworks on-screen keyboard API, or built-in controller text entry. | Any name / seed field calls `ShowFloatingGamepadTextInput` **[W]**. |
| Display | Deck resolutions; native 1280x800 or 1280x720 recommended. | Check the 16:10 CanvasScaler anchoring. |
| Legibility | Readable at 12 in; ≥ 9 px at 1280x800, 12 px recommended; adjustable size recommended. | See section 6. |
| Seamless | No unsupported-hardware warnings; launchers fully pad-navigable. | No launcher; no GPU warning under Proton. |
| Performance | "a default configuration that results in a playable framerate. On Steam Deck, this is 30fps at 800p". | A Deck preset (below). |

**Review:** Valve tests input, legibility, performance and system support and aims for "results within a week of
submission"; partners request it via "Steam Hardware Compatibility Review" under Technical Tools **[W]**. Cost, and
whether a Next Fest demo gets its own review: not verified. **Playable** means "functions … but may require manual work
from the user" (community config, manual keyboard, touchscreen launcher) **[W]**. No data on either badge's sales effect
(not verified); community sites call the badges uneven and rarely re-reviewed **[S]**. **[U]** The gap between the two
badges is the glyph and default-config work, which the pad pass already contains.

**Hardware and performance:**
- APU: Zen 2 4c/8t, 8 RDNA 2 CUs (1.6 TFLOPS FP32), TDP 4-15 W, 16 GB LPDDR5. OLED: 1280x800 up to 90 Hz, four grip
  buttons, capacitive sticks, 6-axis IMU, two trackpads **[W]**. LCD: 60 Hz, 40 Hz mode since SteamOS 3.2 **[S]**.
- 40 fps at 40 Hz paces evenly (25 ms) and roughly halves power against uncapped play **[S]**. Valve wants "an FPS
  limit, whether implemented in the game itself or using the global FPS limiter", and recommends Vulkan **[W]**. Proton
  on the OLED reports 90 Hz, so some vsync paths halve to 45 **[S]**.
- **[U]** A Deck preset at 1280x800 targeting 60 fps; a frame-cap setting (30 / 40 / 60 / uncapped); Vulkan in the
  Windows build's API list. Not checked whether URP 17 on Vulkan beats DX11 under Proton.

**Detecting the Deck (2026 change):** Steamworks SDK 1.65 (July 24, 2026) deprecated `IsSteamRunningOnSteamDeck`; its
replacement `IsRunningOnSteamHardware()` (`k_ESteamHardwareTypeSteamDeck` / `…SteamMachine` / `…SteamFrame`) is for
analytics, and settings should come from `GetSteamHardwareDefaultConfig()`, "tuned for specific Steam hardware"
**[W]**. Also new: `IsRunningUnderProton()`, and `SetGamePerformanceSetting` / `SetGameRenderResolution`, which feed
Valve's opt-in framerate data **[W]**.

**Unity and Steam (2026):** at GDC on March 11, 2026, Unity announced official Steam, Steam Deck and Steam Machine build
targets plus Linux-runtime work for the Deck, some "available today"; no Unity version named **[W]**. The Platform
Toolkit for Steamworks (`com.unity.platformtoolkit.steam`, tested with SDK 1.62) documents achievements, not input or
glyphs **[W]**. **[U]** One benchmark of a native Linux Player against the Proton run is worth doing before the demo.

## 3. Steam Input vs the Unity Input System

**What Steam Input does to a game without its API:** on Windows, Steam hooks XInput, DirectInput, RawInput and
Windows.Gaming.Input and "inject[s] an emulated Xbox controller device"; extra inputs fold together (PlayStation trackpad
click and Options both become Start) **[W]**. Which pads get this is set per game (Application → Steam Input); for a
standard game with Xbox support Valve recommends "checking all checkboxes other than Xbox" **[W]**. The player's own
"PlayStation Controller Support" setting interacts with that choice **[S]**.

**Failure modes seen with Unity:** the Input System sees the real pad and Steam's virtual one, doubling input; one
developer told players to disable Steam Input and "many just refund" **[W]**. In a Steam-launched build every pad shows
as XInput; the fix (disable Steam Input for those pads in Steamworks) "depends on the user's Steam Controller setting",
and the virtual pad "intentionally doesn't identify itself" **[W]**.

| Option | What you get | Cost |
|---|---|---|
| A. Input System only | One path; in-game remap and hold / toggle as designed **[B]**; with Steam Input off for PlayStation, the real DualSense HID **[U]**. | Pads under Steam Input (Deck, Switch, generic, player overrides) show as Xbox, so glyphs can be wrong **[W]**; no gyro **[W]**. |
| B. A + Steamworks for identity and glyphs | `GetControllerForGamepadIndex` → `GetInputTypeForHandle` gives the real type; `GetActionOriginFromXboxOrigin` + `GetGlyphPNGForActionOrigin` / `…SVG…` give Steam's art **[W]**; `UnitySteamInputAdapter` maps an Input System control to an `EInputActionOrigin` for this **[W]**. | Steamworks.NET (or Unity's toolkit) as a dependency; Steam's art unless you map types to your own set **[U]**. |
| C. Full Steam Input API | Action sets and layers ("Menu", "In-game"), `ShowBindingPanel`, `GetMotionData` gyro, `TriggerVibration`, `SetLEDColor`; re-read origins every frame rather than cache **[W]**. | A second path for non-Steam runs; an IGA / VDF manifest; `Init` / `RunFrame` per frame **[W]**; remapping moves to Steam's overlay while XAG 107 wants it in game **[W][U]**. |

**[U] Recommendation: B.** Keep Steam Input on for Switch and Generic pads. Off or on for PlayStation is a playtest call:
off gives native DualSense (rumble over USB only, below); on gives Steam remapping, with glyphs from the Steamworks type.
Ship a Gamepad-template default config so the Deck's grip buttons are bound (templates: Gamepad, High Precision Camera,
Camera Controls variants **[W]**). No shipped Unity FPS's input stack was verified for this note.

**Input System 1.18 specifics:**
- **PlayerInput vs generated class:** PlayerInput auto-switches control schemes, keeps its own action copy ("use
  `PlayerInput.actions`, not `InputSystem.actions`") and notifies by SendMessage, BroadcastMessage, UnityEvents or C#
  events **[W]**. **[U]** The sim polls inside its own loop, so the generated class (or an asset reference) polled by
  `SurvivalGame` fits better and keeps message dispatch off the frame path.
- **Last device without PlayerInput:** `InputUser.listenForUnpairedDeviceActivity` + `onUnpairedDeviceUsed` (reports
  "non-leaf, non-synthetic, non-noisy" controls; "a non-zero cost"); `InputUserChange` includes `ControlsChanged`,
  `DeviceLost`, `DeviceRegained`, `ControlSchemeChanged` **[W]**. **[U]** Cheaper: watch `action.activeControl.device` on
  look / move / fire with a magnitude threshold so stick noise doesn't flicker prompts.
- **Rebinding:** `PerformInteractiveRebinding()` with `WithControlsExcluding`, `WithCancelingThrough`,
  `WithBindingGroup`, `WithTargetBinding`; must `Dispose()`. Persist with `SaveBindingOverridesAsJson` /
  `LoadBindingOverridesFromJson`; reset with `RemoveAllBindingOverrides`; show with `GetBindingDisplayString(…, out
  deviceLayout, out controlPath)` for icons **[W]**. No built-in swap on the fetched page: the web's swap is hand-written
  **[U]**.
- **uGUI:** `InputSystemUIInputModule` takes Move (Vector2), Submit, Cancel, plus `moveRepeatDelay` / `moveRepeatRate`
  (defaults not on the API page) **[W]**. `deselectOnBackgroundClick` defaults to true **[W]**; set it false so a stray
  click doesn't drop pad focus **[U]**. `VirtualMouseInput` exists for screens without navigation **[W]**.
- **Rumble:** `Gamepad.SetMotorSpeeds(low, high)`; `InputSystem.PauseHaptics` / `ResumeHaptics` / `ResetHaptics`;
  `SetLightBarColor` / `SetMotorSpeedsAndLightBarColor` on PlayStation pads; DualSense rumble and light bar don't work
  over Bluetooth on Windows, USB HID works **[W]**. Not verified that Steam forwards emulated-XInput rumble to every pad.
- **Gyro:** PS4 / PS5 gyro is "not supported on platforms other than the PlayStation consoles"; no native Switch Pro on
  Windows **[W]**. Routes: Steam Input (gyro-to-mouse, single player only; or `GetMotionData`) **[W]**, or JoyShockLibrary
  (MIT; DualSense, DualShock 4, Joy-Con, Pro; its Unity C# file "isn't up to date") **[W]**.
- **Update mode for a 120 Hz loop inside `Update`:** Dynamic (default) processes events "at the start of each dynamic
  update before MonoBehaviour.Update" **[W]**. Fixed groups events by fixed-step timestamps for `FixedUpdate` **[S]**: it
  doesn't fit, since the sim's steps aren't Unity fixed steps **[U]**. Manual runs only on `InputSystem.Update()`, "most
  useful for placing input updates in the frame explicitly", and events pile up if the call is skipped **[S]**. Unity's
  fixed-step advice: store values in dynamic update and consume them in the step, since per-frame edge checks
  "introduce the problem of missed or duplicate discrete events" **[W]**.
  **[U]** Stay on Dynamic (or Manual at the top of `SurvivalGame.Update` if script order ever matters). Snapshot held
  buttons and sticks once per frame and feed that snapshot to every step; latch press / release edges and clear them
  after the first step. Stick look is a rate (deg/s × 1/120 per step), so repeating it is correct. Whether mouse delta
  is split across steps must follow the web (not checked). The Editor's "Play Mode Input Behavior" routes pads
  regardless of Game view focus by default **[W]**: handy for Play checks.

## 4. Aim assist on PC FPS

**Vocabulary:** Nick Weihs (Insomniac, GDC 2013, Resistance 3) covers camera acceleration and dead zones, then
magnetism, centering and friction **[W]**. Common names: slowdown / friction (lower turn rate over a target), rotational
/ magnetism (the camera follows a moving target), bullet magnetism (shots bend inside a cone), snap (aim jumps to a target
on aim-down-sights).

| Game | What it does / what changed | Settings | Tag |
|---|---|---|---|
| Apex Legends | Season 22 (Aug 6, 2024): PC controller assist "about 25 %" lower (0.4 → 0.3); console players in PC lobbies −22 % (performance mode) / −18 %. "Aim assist … will never be removed as it's a critical accessibility feature." | not checked | **[W]**; 0.4 → 0.3 **[S]** |
| CoD: Black Ops 6 | Oct 23, 2024: the beta's close-range cut-off was "jarring", so strength interpolates linearly, "much weaker at point blank ranges" rising "out to a short range". | AA types Default / Precision / Focusing / Black Ops | **[W]**; types **[S]** |
| CoD: Black Ops 7 / Warzone | Full rotational strength needs the right stick moving toward the target; minimum −35 % vs beta, later −25 %. | as above | **[S]** |
| Halo Infinite | Bullet magnetism (mouse and pad); rotational / friction and "sticky aim" (pad only) in a "red reticle" range; Infinite cut magnetism and rotational, raised sticky aim. | not checked | **[S]** |
| Destiny 2 | Per-weapon Aim Assistance stat widens the magnetism cone, Range deepens it; reticle magnetism and friction controller-only; mouse gets bullet magnetism. | weapon stat | **[W]** |
| Overwatch 2 | Aim Assist Strength / Window Size / Ease-In / Legacy; Aim Smoothing, Aim Ease-In, Aim Technique (Linear Ramp / Dual Zone); gyro. | as listed | **[W]**; "no aim assist on PC even with a controller" **[S]** |
| Titanfall 2 | "Target compensation" on by default; off in advanced look controls. | toggle | **[S]** |
| DOOM Eternal | Aim assist slider (max 1); "Target Snapping" for precision attachments; look smoothing. | slider, toggle | **[S]** |
| DOOM: The Dark Ages | Aim Assist toggle; "Enhanced Targeting" ("significant aiming forgiveness"); Look Acceleration; H / V sensitivity; vibration intensity. | as listed | **[W]** |
| ULTRAKILL | "Auto Aim" 0-100 (screen radius; 100 = anywhere), mouse and pad, also targets coins and core grenades; a "minor assist" that doesn't flag ranks; offered at the intro when a pad is found. | slider | **[W]** |
| VALORANT (console) | "Focus Mode" on LT: lower sensitivity and slight zoom instead of aim-down-sights. | not checked | **[S]** |
| Roboquest / Turbo Overkill | Turbo Overkill: "hyper-aggressive" assist that can be turned off **[S]**; Roboquest's official settings not found. | not verified | not verified |

**Reception:** complaints centre on close-range rotational assist in crossplay PvP, and Respawn and Treyarch answered
with range / input conditions and percentage cuts **[W][S]**. PvE and single-player games (DOOM, ULTRAKILL) frame
assists as accessibility options with sliders **[W]**.

**[U] For SKYGRIND's snap (12° cone, 240°/s pull, breakaway capped at 80 % of turn rate [B]):** scale the pull with
range (close enemies span a wide angle and a hard snap fights movement tech); apply the rotational part only with stick
input toward the target (the 80 % cap already allows breakaway); expose Off / Slowdown only / Snap, strength 0-100 %,
window size and ease-in; separately offer ULTRAKILL-style auto-aim for mouse and pad as an accessibility assist; apply
pad assist only to stick look, never mouse or gyro-as-mouse. New numbers (range curve, ease-in) go in a Unity-side
config file (rule 5).

## 5. Stick feel and the settings players expect

- **Dead zones:** Josh Sutphin: axial snaps to cardinals, plain radial "kicks" at the edge; scaled radial
  `v.normalized * ((|v| - dz) / (1 - dz))` is smooth (example dz = 0.25) **[W]**. Minimuino's survey adds sloped and
  scaled-axial variants; a "hybrid" (scaled radial + sloped scaled axial) passes 5 of 6 tests; outer dead zones and power
  curves for fine aim **[W]**. **[U]** The web's scaled radial 0.15 / 0.95 **[B]** is right; expose inner and outer per
  stick.
- **Curves and acceleration:** the web has linear and "precise" (squared) **[B]**; Overwatch 2 has Linear Ramp and Dual
  Zone; DOOM: The Dark Ages has "Look Acceleration" **[W]**; Insomniac treats camera acceleration as part of aim design
  **[W]**. **[U]** An optional ramp (extra yaw after full tilt for a set time, at a set rate): two new numbers for a
  Unity-side config.
- **Sensitivity and invert:** separate horizontal and vertical (Overwatch 2, DOOM: The Dark Ages, Halo Infinite) **[W]**;
  XAG 107 wants invert X and Y "individually for each individual control stick" and a range of at least ±50 % of default
  **[W]**; an ADS / zoom multiplier if SKYGRIND has zoom (not checked).
- **Vibration:** an intensity slider (ULTRAKILL 0-1.5, DOOM: The Dark Ages "Vibration Intensity") **[W]**; GAG Basic:
  "Include toggle/slider for any haptics" **[W]**.
- **Gyro (GyroWiki, Jibb Smart) [W]:** "natural sensitivity scale" (1.0 = a 37.5° real turn gives 37.5° in game; allow up
  to ~10); a gyro-off button (the "ratchet") or off while the right stick is active; optional acceleration 1 → 2 over
  0-75 °/s; tiered smoothing and tightening at ~5 °/s; "Make all the bells and whistles … optional"; Player Space for
  held pads, Local Space for handhelds such as the Deck; flick stick fires at 90 % of radius with "around 0.1s" flick
  time.

## 6. Accessibility that reviewers and platforms check

- **GAG Basic items that apply [W]:** adjust game speed; haptics toggle / slider; control sensitivity; UI reachable with
  the gameplay input; remap; simple controls or a simpler alternative; avoid flicker and repeating patterns; readable
  default font size; high text / background contrast; sensible default FOV; nothing essential by colour alone; all
  settings saved; accessibility features described in game and on the store page; a wide choice of difficulty.
- **GAG Intermediate items that apply [W]:** alternatives to holding buttons; every key action doable with digital
  input; no simultaneous presses required; more than one input device; crosshair colour / design; turn off background
  movement; adjustable FOV; remove the controller-vs-camera difference (bob, smoothing); assist modes such as auto-aim;
  expose gameplay variables; a no-fail practice mode.
- **XAG 117 (motion) [W]:** "Avoid the use of camera shake, camera bobbing effects, motion blur … or provide an option to
  turn off these behaviors"; adjustable FOV, H / V sensitivity, a way to turn off automatic camera movement. Halo
  Infinite gives 0-100 % sliders for radial blur, screen shake, full-screen effects and speed lines. **[U]** Each
  SKYGRIND camera or screen effect (shake, hitstop punch, chromatic / vignette pulses) wants its own 0-100 % slider,
  plus a parry-flash toggle (ULTRAKILL has one **[W]**).
- **XAG 101 (text) [W]:** minimum body height (ascender to descender) 18 px at 1080p on PC, 26 px on console; scaling to
  200 %; text inside glyphs meets the same minimum. **[U]** CLAUDE.md's 16 px is below the PC figure, and it isn't clear
  whether it's a TMP size or a measured body height: measure one capture with XAG's method.
- **XAG 103 (colour) [W]:** "Color alone should never be used to represent information"; if colour is primary, let
  players change it (Black Ops Cold War lets them pick ally / enemy marker colours). **[U]** SKYGRIND's palette rule ties
  meaning to colour (cyan player, red-orange danger, magenta RIVAL, violet-white boundary): pair each with a shape or
  icon and offer recolours like DOOM: The Dark Ages' "Enable Parry Recolor" and colour-highlight sliders **[W]**.
- **XAG 107 (input) [W]:** remap everything in game, including Esc; prompts follow the remap; UI works with single
  digital presses, sliders too; no mashing, long holds or chords, or an alternative; analog functions work from digital
  input; single-stick options.
- **Hold vs toggle:** Can I Play That?'s motor guide calls toggles for held actions (aim, run, crouch, weapon wheel)
  essential **[S]**; DOOM: The Dark Ages makes shield block, lock-on and UI actions press-or-hold and adds "Weapon Wheel
  Open Delay" **[W]**.
- **Can I Play That? on the genre [W]:** DOOM Eternal scored 4/4 hearing, 4/4 vision, 1/4 fine motor: no stick
  customisation (no one-handed legacy setup), platforming "like a demonic Super Mario Bros.", holds for weapon mods; its
  colour-blind suite is "industry leading". DOOM: The Dark Ages was praised for game speed, enemy aggression and
  projectile speed sliders, "Enhanced Targeting … quite literally a life-saver", a full remap that allows conflicts,
  independent motion settings, HUD size and opacity, high-contrast colours and subtitle options; criticised for no
  screen reader at launch. Its sliders include Game Speed 50-150 %, Parry Window and Daze Duration **[W]**; Damage to
  Player 50-500 % **[S]**.
- **[U] For SKYGRIND (DOOM Eternal's movement risk):** a game speed slider (a gameplay number for config; check replay
  exactness); a parry-window multiplier; hold / toggle for slide, grapple and fire; southpaw and stick-swap presets;
  assists flagged on RESULTS the way ULTRAKILL's major assists flag ranks **[W]**, so leaderboards and the DAILY stay
  honest.

## 7. Menus on a pad

- **Focus [U]:** every screen opens with a selected element (`EventSystem.SetSelectedGameObject`) when the last device
  is a pad; B / Cancel always goes back; sliders move with single presses (XAG 107's Minecraft example **[W]**); explicit
  navigation on grids; `deselectOnBackgroundClick = false`; keep the selection when the mouse moves, and hand focus back
  on the first pad input.
- **Weapon selection:** DOOM Eternal holds a wheel that slows time ("otherwise you'd be dead") **[S]**. DOOM: The Dark
  Ages adds "Weapon Wheel Open Delay" and pairs weapons by class (shotguns, launchers) with a dedicated sibling-swap
  input, so four guns swap without the wheel **[W]**. ULTRAKILL: bumpers cycle, holding opens a wheel **[S]**; options
  choose the scroll target (Weapons / Variations / Both) and what "swap to already drawn weapon" does **[W]**.
  **[U]** Tap Y to cycle or switch to the last weapon; hold for a wheel with an open-delay setting; d-pad direct slots if
  there are four guns or fewer. A wheel slow-mo would be a new gameplay number inside the deterministic sim: GG's call;
  the default is "hold to open, time keeps running".
- **Prompts:** hot-swap by last device **[B]** with hysteresis **[U]**; A/B/X/Y on Deck and Xbox; shapes on PlayStation,
  with the type from `GetInputTypeForHandle` under Steam Input **[W]**; Nintendo layouts drawn by position (south / east /
  west / north) and labelled per device **[U]**. Glyph text meets the text minimum and scales with it **[W]**; with the
  pad active, no key / mouse glyph anywhere, tutorials and RESULTS included **[W]**. SDK 1.65 adds Switch 2, Steam
  Controller and Steam Frame Controller support **[W]**; fall back to Steam's art for unknown types, as Valve recommends
  **[W]**.

---

## 8. Requirements checklist

| # | Requirement | Source | Check a developer can run |
|---|---|---|---|
| 1 | All content reachable on a pad from boot (settings, rebinding, profile, RESULTS) | Deck compat **[W]** | Unplug mouse and keyboard; title → run → RESULTS → settings → quit with the pad only. |
| 2 | Glyphs match the active device; no key / mouse glyph during pad use | Deck compat **[W]** | Alternate key and pad every 2 s on each screen; screenshot; any key glyph during pad use fails. |
| 3 | Real pad type known under Steam Input | ISteamInput **[W]** | DualSense, Steam Input on: `GetInputTypeForHandle` logs PS5 and prompts use the PlayStation set. |
| 4 | No doubled devices or input under Steam Input | Unity forum **[W]** | Log `InputSystem.devices` with Steam Input on and off; one press gives one action. |
| 5 | Mouse and stick look summed in one frame | Deck recommendations **[W]** | Move mouse and right stick together; both turn the camera; look never locks. |
| 6 | Edges neither lost nor repeated at 30 / 40 / 60 / 144+ fps | Input System timing **[W]**, **[U]** | Test build counts jump presses vs step consumptions at each cap: 1:1. |
| 7 | One dead-zone stage, exactly the web's 0.15 / 0.95 | Input System defaults **[W]**, **[B]** | Feed raw (0.2, 0) into the look path; output equals the web's formula. |
| 8 | Inner / outer dead zone per stick; linear / precise curve | Sutphin **[W]**, **[B]** | Settings round-trip; inner 0 shows drift (proves the setting is live). |
| 9 | Per-axis sensitivity, ≥ ±50 % of default | XAG 107 **[W]** | Slider spans ≥ 0.5×-1.5×; measure °/s at both ends. |
| 10 | Invert X and Y separately per stick | XAG 107 **[W]** | Toggle each of the four; only that axis flips. |
| 11 | Every action remappable per device, labels follow, assign not only swap | XAG 107, GAG **[W]** | Rebind fire to a face button; HUD and tutorial show it; restart; JSON override persists. |
| 12 | Hold / toggle for every held action | GAG **[W]**, CIPT **[S]** | List held actions in settings; test both modes each. |
| 13 | No required chords or holds > 1 s, or an alternative | XAG 107 **[W]** | Audit the binding table. |
| 14 | Assist modes Off / Slowdown / Snap, strength, window | Apex, BO6, OW2 **[W][S]** | Strength 0 gives the same yaw trace as Off; at 100 the pull stays within the cap. |
| 15 | Pad assist affects stick look only | **[U]** | Mouse-only run with assist 100: yaw trace equals Off. |
| 16 | Vibration toggle and intensity; haptics paused in menus | GAG, Input System **[W]** | Intensity 0 = no motor; pause mid-rumble stops it (`PauseHaptics`). |
| 17 | Text ≥ 9 px at 1280x800 (12 rec.), ≥ 18 px body at 1080p, or scale to 200 % | Deck compat, XAG 101 **[W]** | Capture at both resolutions; measure the smallest string ascender to descender. |
| 18 | 1280x800 (16:10) layout correct | Deck compat **[W]** | Player windowed at 1280x800; no clipped HUD or menu. |
| 19 | ≥ 30 fps at 800p by default; target 60 with a 40 cap | Deck compat **[W]**, **[U]** | Deck or Proton bench, 60 s, median and worst 1 % per cap. |
| 20 | In-game cap 30 / 40 / 60 / uncapped | Deck FAQ **[W]** | Each cap holds within ±1 fps. |
| 21 | On-screen keyboard for any text field | Deck compat, ISteamUtils **[W]** | Focus each field with the pad; `ShowFloatingGamepadTextInput` opens. |
| 22 | No compatibility warnings, no launcher | Deck compat **[W]** | Cold boot under Proton: no dialog before the title. |
| 23 | Deck defaults via `GetSteamHardwareDefaultConfig` | ISteamUtils SDK 1.65 **[W]** | Code search finds no `IsSteamRunningOnSteamDeck`. |
| 24 | Steam Input default config set (Gamepad template or bundled) | Steam Input getting started **[W]** | Fresh Deck config: grip buttons do something sensible; every action bound. |
| 25 | Shake, bob, blur, FX pulses each 0-100 % or off | XAG 117 **[W]** | All at 0; record dash, slam, hit: no camera offset or screen effect. |
| 26 | Adjustable FOV, sensible default | GAG, XAG 117 **[W]** | Applies live; persists. |
| 27 | Nothing by colour alone; enemy / danger / parry colours changeable | XAG 103, DOOM: The Dark Ages **[W]** | Greyscale capture: enemies, danger, RIVAL, parry cue still told apart by shape. |
| 28 | Flash / flicker can be turned off | GAG, ULTRAKILL **[W]** | Toggle off; capture a parry: no full-screen flash. |
| 29 | Game speed and parry-window assists, flagged on RESULTS | DOOM: The Dark Ages, ULTRAKILL **[W]** | 70 % speed plays slower; RESULTS shows the flag; DAILY scores marked. |
| 30 | Weapon switching without the wheel; wheel open delay | DOOM: The Dark Ages **[W]** | Reach every gun with taps only; time the configured delay. |
| 31 | Pad focus never lost; single-press sliders | XAG 107 **[W]**, **[U]** | Mouse-click the background, press the d-pad: focus returns to a control. |
| 32 | All settings saved | GAG **[W]** | Change everything, restart, compare. |
| 33 | PlayStation support on the store page only once its glyphs ship | Valve 2024 **[W]** | Store page review. |

---

## 9. Sources

[W] = fetched and read; [S] = search snippet only, or the fetch returned no article body.

**Valve / Steam**
- [W] https://partner.steamgames.com/doc/steamdeck/compat
- [W] https://partner.steamgames.com/doc/steamdeck/recommendations
- [W] https://partner.steamgames.com/doc/steamdeck/faq
- [W] https://www.steamdeck.com/en/tech
- [W] https://partner.steamgames.com/doc/features/steam_controller
- [W] https://partner.steamgames.com/doc/features/steam_controller/steam_input_gamepad_emulation_bestpractices
- [W] https://partner.steamgames.com/doc/features/steam_controller/getting_started_for_devs
- [W] https://partner.steamgames.com/doc/api/isteaminput
- [W] https://partner.steamgames.com/doc/api/ISteamUtils
- [W] https://steamdeckhq.com/news/steamworks-sdk-165-update/
- [W] https://www.gamedeveloper.com/business/steam-s-new-controller-reporting-lets-devs-know-how-their-games-are-played
- [W] https://www.gamedeveloper.com/business/steam-s-average-controller-use-triples-to-15-percent-with-majority-usage-from-xbox-controllers
- [W] https://www.phoronix.com/news/Steam-Survey-August-2026
- [S] https://steamcommunity.com/games/593110/announcements/detail/4142827237888316812 (Valve's 2024 post; no body)
- [S] https://www.pcgamer.com/48-million-players-use-controllers-on-steam/
- [S] https://www.tomshardware.com/video-games/pc-gaming/gamers-using-game-controllers-for-pc-gaming-has-tripled-since-2018
- [S] https://www.techradar.com/gaming/consoles-pc/steam-players-use-of-controllers-has-tripled-since-2018
- [S] https://www.slashgear.com/1683749/make-ps4-ps5-controller-work-pc-games/
- [S] https://www.gamingonlinux.com/2026/02/steam-survey-for-january-2026-shows-a-small-drop-for-linux/
- [S] https://m.gsmarena.com/steamos_32_update_adds_refresh_rate_settings_quieter_fan_curve_to_the_steam_deck-amp-54457.php
- [S] https://overkill.wtf/how-to-graphic-settings-steam-deck/
- [S] https://steamcommunity.com/groups/steamworks/announcements/detail/676258795703239817 (Deck framerate data; no body)
- [S] https://steamdeckhq.com/news/the-steam-deck-verified-system-needs-to-change/
- [S] https://steamdeckhq.com/news/steam-next-fest-june-2026-demos-great-steam-deck/

**Unity**
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/SupportedDevices.html
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/Gamepad.html
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/Settings.html
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/ActionBindings.html
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/PlayerInput.html
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/UISupport.html
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/UserManagement.html
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/api/UnityEngine.InputSystem.InputSettings.html
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/api/UnityEngine.InputSystem.UI.InputSystemUIInputModule.html
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/api/UnityEngine.InputSystem.Users.InputUser.html
- [W] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.12/manual/timing-optimize-fixed-update.html
- [S] https://docs.unity3d.com/Packages/com.unity.inputsystem@1.18/manual/timing-select-mode.html
- [S] https://docs.unity3d.com/Packages/com.unity.inputsystem@0.2/manual/Steam.html
- [W] https://docs.unity3d.com/Packages/com.unity.platformtoolkit.steam@1.0/manual/get-started-with-pt-and-steamworks.html
- [W] https://www.gamingonlinux.com/2026/03/unity-announce-expanded-supported-for-steam-linux-steam-deck-and-steam-machine/
- [S] https://videocardz.com/newz/unity-expands-official-support-for-steam-linux-steam-deck-and-steam-machine
- [W] https://discussions.unity.com/t/steam-input-support/936239
- [W] https://discussions.unity.com/t/why-are-all-my-gamepads-shown-as-xinput-devices/1530195
- [W] https://github.com/eviltwo/UnitySteamInputAdapter
- [S] https://www.gamineai.com/help/steam-deck-controller-glyphs-wrong-unity-input-system-layout-detection-fix

**Aim assist**
- [W] https://www.gdcvault.com/play/1017942/Techniques-for-Building-Aim-Assist
- [S] https://archive.org/details/GDC2013Weihs
- [W] https://www.pcgamesn.com/apex-legends/aim-assist-nerf-season-22
- [S] https://x.com/alphaINTEL/status/1819026881410134177
- [S] https://www.thegamer.com/apex-legends-season-20-nerfed-aim-assist/
- [W] https://www.dexerto.com/call-of-duty/black-ops-6-oct-23-update-patch-notes-multiplayer-map-aim-assist-changes-2958148/
- [S] https://www.pcgamer.com/games/call-of-duty/call-of-duty-black-ops-6s-new-set-of-patch-notes-reveals-that-aim-assist-will-be-much-weaker-at-point-blank-range/
- [S] https://thenerdstash.com/black-ops-6-and-warzone-rotational-aim-assist-is-off-the-menu/
- [S] https://www.dexerto.com/call-of-duty/black-ops-7-warzone-aim-assist-changes-3272835/
- [S] https://www.gamespot.com/articles/cod-black-ops-7-patch-notes-detail-aim-assist-change-drone-pod-nerf-and-smg-buffs/1100-6536440/
- [S] https://dotesports.com/call-of-duty/news/black-ops-7-post-launch-update-changes
- [S] https://www.windowscentral.com/gaming/call-of-duty-black-ops-7-and-warzone-just-made-some-big-aim-assist-nerfs-at-launch-here-are-the-changes-and-why-im-a-fan
- [S] https://www.callofduty.com/patchnotes/2025/11/call-of-duty-black-ops-7-preseason-patch-notes (fetch failed)
- [S] https://x.com/LeviathanGamer2/status/1522362607780474881
- [W] https://d2.destinygamewiki.com/wiki/Aim_Assistance
- [W] https://www.charlieintel.com/best-overwatch-2-controller-settings/202173/
- [S] https://dotesports.com/overwatch/news/how-to-turn-off-aim-assist-in-overwatch-2
- [S] https://steamcommunity.com/app/1237970/discussions/0/2515771867779365271/
- [S] https://steamcommunity.com/sharedfiles/filedetails/?id=2464347025 (fetch got HTTP 429)
- [S] https://steamcommunity.com/app/782330/discussions/0/3118172724627697276/
- [W] https://ultrakill.wiki.gg/wiki/Assists
- [W] https://ultrakill.wiki.gg/wiki/Options
- [S] https://steamcommunity.com/app/1229490/discussions/0/3002172678260263555/
- [S] https://strategywiki.org/wiki/Turbo_Overkill/Controls
- [S] https://sometipsygnostalgic.tumblr.com/post/748024995571220480/turbo-overkill-on-steamdeck-review
- [S] https://kotaku.com/valorant-console-focus-mode-ps5-xbox-riot-fps-1851545155

**Stick feel and gyro**
- [W] https://joshsutphin.com/gamedev/doing-thumbstick-dead-zones-right.html
- [S] https://www.gamedeveloper.com/business/doing-thumbstick-dead-zones-right
- [W] https://minimuino.github.io/thumbstick-deadzones/
- [W] http://gyrowiki.jibbsmart.com/blog:player-space-gyro-and-alternatives-explained
- [W] http://gyrowiki.jibbsmart.com/blog:good-gyro-controls-part-1:the-gyro-is-a-mouse
- [W] http://gyrowiki.jibbsmart.com/blog:good-gyro-controls-part-2:the-flick-stick
- [W] https://github.com/JibbSmart/JoyShockLibrary

**Accessibility and menus**
- [W] https://gameaccessibilityguidelines.com/basic/
- [W] https://gameaccessibilityguidelines.com/intermediate/
- [W] https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/101
- [W] https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/103
- [W] https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/107
- [W] https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/117
- [W] https://caniplaythat.com/2020/06/10/doom-eternal-accessibility-review/
- [W] https://caniplaythat.com/2025/06/04/doom-the-dark-ages-accessibility-review/
- [S] https://caniplaythat.com/basic-accessibility-options-for-mobility
- [W] https://slayersclub.bethesda.net/en-US/article/doom-the-dark-ages-accessibility-guide
- [S] https://game8.co/games/Doom-The-Dark-Ages/archives/520407
- [S] https://www.thegamer.com/doom-the-dark-ages-accessibility-settings-sliders-discussion/
- [W] https://gamerant.com/doom-dark-ages-weapon-wheel-class-pair-swap-combat-good-why/
- [S] https://www.doomworld.com/forum/topic/113230-weapon-wheel-problem/
