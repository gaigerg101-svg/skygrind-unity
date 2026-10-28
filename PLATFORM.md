# PLATFORM.md: what Steam (and the Deck) and Switch 2 need (NIGHT15 U3-28.8, 2026-10-05)

A checklist for GG. Each row says where the thing lives (file:line on `port/stage3` after NIGHT15's U3-28.8 commits),
its state today, and the decision it needs. No SDK work was done. The platform facts come from `MAP-RESEARCH.md`
sections 6-8 (their UNVERIFIED marks kept); the codebase facts were read with grep. "Not checked" means nobody tried or
measured it.

State: **DONE** in the code now · **PART** partly · **NO** missing · **?** not checked.

## 1. Both platforms: the pad

| # | Item | Where | State | Decision for GG |
| --- | --- | --- | --- | --- |
| P1 | Every menu and a run by pad only (MAP-RESEARCH 8: "every menu by pad"; Deck Verified: the default config reaches everything, section 7) | `UiNav.cs:1-24` (menus), `SliceSmoke.Night15Pad.cs` (the "pad only" smoke step: TITLE, a run, the wheel, PAUSED, SETTINGS, back) | DONE in code; the step runs in the lead's smoke | none |
| P2 | The weapon wheel on the pad: hold NEXT WEAPON (Y) `design.pad.wheelHold` s, or a button bound to WEAPON WHEEL; the right stick picks; release takes | `PadReader.cs:13-19` (rule), `WeaponSelect.cs:130` (PointRadial), `ReaderInput.cs:153` (ApplyPad), `Design/unity-config.json:288-294` | DONE (PadWheelTests) | `wheelHold` 0.25 s and `wheelStickDeadzone` 0.5 are design numbers: tune by feel |
| P3 | Glyphs follow the last device used, every frame (MAP-RESEARCH 6) | `ReaderInput.cs` Poll writes `PlayerDevices.Glyphs`; `Glyphs.cs` | DONE: Xbox, PlayStation (in words), Nintendo, the web's neutral set | none |
| P4 | Nintendo glyphs (A / B and X / Y at swapped positions) by the device's names | `Glyphs.cs:35-50`, `UnityTuning.PadWheel.cs:17-26`, called once per device at `PadReader.cs:242` | DONE for the PC's Switch Pro controller layout (`SwitchProControllerHID`, read in the Input System 1.18 package); the console's NPad name is UNVERIFIED (NDA package) | none |
| P5 | The hotbar's pad line on a Nintendo pad | `UiHotbar.cs:232-237` (the cycle line), `:336` (the special hint's mode), `:361` (the alt key) | DONE (`c72f014`): X NEXT and ZL from `UnityTuning.PadWheel.cs:31-32`; the hotbar's line doesn't yet say the wheel is a hold of Y | a hint "HOLD Y: WHEEL" on the hotbar? (its width is not checked) |
| P6 | Confirm / back on a Nintendo pad | `UiNav.cs:139` Submit = `<Gamepad>/buttonSouth`, `:142` Cancel = `buttonEast` | positional: on a Nintendo pad Submit is the button printed **B** | keep positional (one muscle memory on every pad, as the web) or by label on Nintendo pads (A confirms, Nintendo's own convention; whether Lotcheck requires it is UNVERIFIED) |
| P7 | Rumble with an intensity slider (MAP-RESEARCH 6) | `Rumble.cs:32` (strength x RUMBLE), `:41` (on from `design.pad.vibrationDefault`), the RUMBLE row `GameSettings.cs:68-69` | DONE; the settings' VIBRATION default follows `design.pad` with the lead's U3-28.7 migration | none |
| P8 | Full pad rebinding (MAP-RESEARCH 6: ULTRAKILL was criticised without it) | CONTROLS' GAMEPAD table (`UiControls.cs`), its row list `Core/Gamepad.cs:285-290` (LabelOrder) | PART: the web's 17 actions rebind; WEAPON WHEEL isn't listed, so only a saved layout can bind it | list WEAPON WHEEL in CONTROLS? (a Core table change and a rebind rule: RebindPad needs the action bound somewhere first) |
| P9 | Radial dead zones with an outer zone, a response curve, X / Y sensitivity, invert Y (MAP-RESEARCH 6) | the GAMEPAD tab (config.settings' pad keys), `PadReader.cs` PadSettings | DONE (the web's) | none |
| P10 | Gyro aim (MAP-RESEARCH 5 and 8: standard on Switch; gyro as a mouse) | none (no gyro code in `Assets/Scripts`) | NO | whether and when |
| P11 | Pause when the pad in use disconnects (a commonly cited Lotcheck failure, UNVERIFIED) | `SurvivalGame.cs:230-235`: emits `input:padConnected` and shows a hint | NO pause | recommended: pause a run when the pad in use goes away |
| P12 | Pause on focus loss / suspend (MAP-RESEARCH 8) | `SurvivalGame.cs:267` pauses when the cursor unlocks; `Rumble.cs:43` stops the motors on focus loss | PART: a console, or a Deck played by pad, never holds a cursor lock, so nothing pauses | recommended: pause on `Application.focusChanged(false)` while playing |

## 2. Both platforms: saves

| # | Item | Where | State | Decision for GG |
| --- | --- | --- | --- | --- |
| S1 | What lives under `persistentDataPath` | settings `GameSettings.cs:111`; profile `SurvivalGame.Init.cs:65`; the saved run `SurvivalGame.Save.cs:26`; run logs and tapes `RunLog.cs:53` (`runs/`); ghosts `UnityTuning.Route.cs:14` (`ghosts/`); the bot's nav cache `SurvivalGame.Night14.cs:32` (`botnav/`) | read | which of them Auto-Cloud syncs (D8) |
| S2 | What lives in PlayerPrefs (the Windows registry: Auto-Cloud doesn't carry it, MAP-RESEARCH 7) | key bindings `InputPrefs.cs:17`, `:24-25`; the pad layout `InputPrefs.cs:30`, `:35-36`; one-time legacy reads `GameSettings.cs:112`, `:116`, `SurvivalGame.Init.cs:67` | bindings and the pad layout wouldn't follow a player to another PC | move them into the settings JSON (a migration) before Steam? |
| S3 | Atomic writes (save corruption is a commonly cited Lotcheck failure, UNVERIFIED) | temp file then move / replace: `Profile.cs:225-227`, `SettingsView.cs:139-168`, `SurvivalGame.Save.cs:65-67`, `GhostStore.cs:73`, `BotNav.cs:110`; appends: `RunLog.cs:275`; a direct write: `RouteCard.cs:64` (the PNG) | PART (the player's data is atomic) | none on PC |
| S4 | The save folder's name | `ProjectSettings/ProjectSettings.asset:15-16`: companyName `DefaultCompany`, productName `SKYGRIND-unity` (persistentDataPath on Windows is `LocalLow/<company>/<product>`) | ? | set the company and product names before the first public build: a later change moves every player's saves and the Auto-Cloud root |
| S5 | One save layer behind a platform interface (MAP-RESEARCH 8) | System.IO is called directly in the files of S1 and S3 | NO | needed for Switch, whose save data is not a plain folder (UNVERIFIED, NDA); its Switch 1 player settings are at their defaults: `ProjectSettings.asset:663` switchStartupUserAccount 0, `:667` switchUserAccountSaveDataSize 0 |
| S6 | SAVE CARD writes a PNG to Pictures | `RouteCard.cs:34-36` (MyPictures/SKYGRIND, else persistentDataPath) | PC only | on Switch, hide it or use the system's capture (UNVERIFIED) |

## 3. Steam and the Steam Deck

| # | Item | Where | State | Decision for GG |
| --- | --- | --- | --- | --- |
| D1 | The Steamworks SDK | none: no Steamworks package or plugin (`Packages/manifest.json`, `Assets/Scripts`) | NO | which wrapper and when (Steamworks.NET or Facepunch.Steamworks); needed for the Steam Input API, achievements and Auto-Cloud checks |
| D2 | Steam Input or not (MAP-RESEARCH 7: Steam Input and Unity's Input System can see duplicate pads (Xbox) or lose non-Xbox pads) | `PadReader.cs` reads the Input System's `Gamepad.current`; `Packages/manifest.json:11` inputsystem 1.18.0 (its Steam controller plugin is unused) | ? not tried with Steam running. A pad Steam presents as an Xbox pad gets the Xbox set (`Glyphs.cs:37`, the type rule) | (a) Steam Input on, with its API for glyphs and action sets (SDK work, D1), or (b) PlayStation and Nintendo pads opted out in the partner settings, so the Input System sees them and `Glyphs.ForDevice` picks their set. Recommended: (b) first, no SDK; revisit with D1. The Deck's own controls reach a game through Steam Input either way (UNVERIFIED) |
| D3 | Deck: the default config reaches everything | P1 | DONE in code | none |
| D4 | Deck: matching glyphs | the Deck's face buttons sit as Xbox's (A at the bottom), so the Xbox set fits | ? not checked on a Deck | none |
| D5 | Deck: text at least 9 px at 800p, 12 px preferred (MAP-RESEARCH 7) | the canvas scales with the height: `Ui.cs:191-194` (1920 x 1080 reference); the smallest type is 16 px at 1080p (`PortExport/config.json:6540` small, `:6542` label), so 11.9 px at 800p | passes 9, under 12 | UI SCALE 110 % on a Deck (13.0 px; settings.uiScale 80-140 %, `config.json:6916-6921`, default 100 at `:6496`)? It needs a Deck test (the SDK, or a system check: not checked) |
| D6 | Deck: 16:10 at 1280 x 800 | at 16:10 the 1080-high canvas is 1728 px wide, not 1920: a row wider than that clips, e.g. TITLE's controls line, 1800 px (`Ui.cs:371`) | ? not checked | a capture of a 1280 x 800 window before a Deck review |
| D7 | Deck: fullscreen, no launcher, 30 fps at the default settings (MAP-RESEARCH 7) | WINDOW MODE default windowed (`UnityTuning.Controls.cs:20`), RESOLUTION AUTO (`:31`); `ProjectSettings.asset:45-46` 1024 x 768, `:111` fullscreenMode 1, `:99` resizableWindow 0, `:86` runInBackground 1; FPS CAP / VSYNC `SurvivalGame.cs:205-209`; no launcher | ? not run on a Deck. The only frame-time series is an x86 laptop's: median 4.28 ms on battery (`NIGHT14-LOG.md:438-441`) | default FULLSCREEN WINDOW (on a Deck, or everywhere)? |
| D8 | Auto-Cloud on files under persistentDataPath, not PlayerPrefs (MAP-RESEARCH 7) | S1, S2, S4 | NO (configured on the partner site) | sync settings, profile, the saved run and ghosts; leave out `runs/` and `botnav/` |
| D9 | The on-screen keyboard through the API (MAP-RESEARCH 7) | no text input anywhere (no InputField in `Assets/Scripts`) | not needed | none |

## 4. Nintendo Switch 2

| # | Item | Where | State | Decision for GG |
| --- | --- | --- | --- | --- |
| N1 | Access (MAP-RESEARCH 8): developer.nintendo.com, then a Switch Access Request; Unity's Switch 2 support is an add-on for approved developers; dev kits are limited (UNVERIFIED) | none | NO | when to register |
| N2 | IL2CPP (consoles compile AOT) | `ProjectSettings.asset:836-837`: scriptingBackend lists Android only, so the Windows Player is Mono; the IL2CPP trial was blocked (`NIGHT13-LOG.md:248-252`: no IL2CPP player variation, no C++ toolchain; D-40 stays Mono) | NO | install the Windows IL2CPP module and the C++ build tools (a machine change, GG's call), then run the golden vectors, the replays and the fingerprint on an IL2CPP Windows Player: bit-exactness is proven only on Mono and CoreCLR on x86 (MAP-RESEARCH 8), ARM is further still |
| N3 | AOT-safe code (MAP-RESEARCH 8: reflection, Emit) | no `Reflection.Emit`, `DynamicMethod` or expression trees in `Assets/Scripts`. Reflection that reads fields: `PortData.cs:143`, `:152` (MovementConfig, at every boot), `RunSnapshot.cs:343-351` (Dictionary's private `_entries` / `_count` / `_freeList` / `next`: the save keeps its slot layout, and falls back when they're absent), `RunSnapshot.cs:553` (Activator.CreateInstance), `SimFingerprint.cs:73` (the Mulberry32 fields by name), `GoldenRunShape.cs:151` | ? not built under IL2CPP | IL2CPP's managed stripping (`ProjectSettings.asset:841` managedStrippingLevel: the platform defaults) can strip fields only reflection reads: add a link.xml or [Preserve] for these types, and check RunSnapshot's Dictionary names on IL2CPP's class library |
| N4 | The CPU budget (MAP-RESEARCH 8: 6 game cores, A78C at ~1.0-1.1 GHz; the risk is the CPU, not the GPU) | the 120 Hz sim (CLAUDE.md rule 4), the C# collider, the husk counts; worker tasks `SurvivalGame.Night14.cs:42` (the nav cache), the run log and tape writers | ? not measured | the CPU knob: enemy and particle caps per quality preset (the web's presets' particle cap isn't ported: `Game/Quality.cs:21-22`). A first ARM reading is possible on a Windows-on-ARM PC: this Editor has a win_arm64 Mono player (`NIGHT13-LOG.md:248-249`), though it isn't the Switch's CPU |
| N5 | Mobile or PC quality for the Switch platform | `QualitySettings.asset:119-131` (m_PerPlatformDefaultQuality): `Nintendo Switch: 1` (`:124`) is the **PC** level (`:64`, PC_RPAsset `:104`); the Mobile level (`:10`, Mobile_RPAsset `:50`) is excluded on Standalone only (`:61-62`). Switch 2's own platform key isn't in the file (its module isn't installed: not checked whether it reads "Nintendo Switch") | ? | Switch on the Mobile asset (cheaper URP), on PC, or on a third level made for it. The runtime presets, AUTO and the dynamic resolution apply on top (`Game/Quality.cs:1-27`; AUTO runs only in a Player, `:18-20`) |
| N6 | Joy-Con 2 mouse mode, gyro (MAP-RESEARCH 8) | none | NO | UNVERIFIED how the Switch 2 input package exposes them (NDA); see P10 |
| N7 | NPad styles and count | `ProjectSettings.asset:701` switchSupportedNpadStyles 22, `:704` switchSupportedNpadCount 8 (Switch 1 player settings at their defaults) | ? | set them with the Switch 2 module (one player) |
| N8 | Lotcheck's commonly cited failures (UNVERIFIED, MAP-RESEARCH 8): suspend / resume, save corruption, controller disconnect, button terminology | P12, S3 / S5, P11, P4 / P6 | PART | as in those rows |
| N9 | QUIT on TITLE | `Ui.cs:366` (the button), `Ui.cs:671` (Application.Quit); the kiosk hides it (`UiKiosk.cs:60`) | PC only | hide it on consoles (a console game doesn't quit itself: UNVERIFIED as a rule) |
| N10 | Text on the 1080p handheld screen | CLAUDE.md rule 8 (at least 16 px at 1080p) | DONE by the rule; legibility on the small screen not checked | none |

## 5. The decisions, in one list

1. **Steam Input (D2):** opt PlayStation and Nintendo pads out first (no SDK), or Steam Input on with its API (SDK).
2. **The company and product names (S4)** before the first public build.
3. **Bindings and the pad layout (S2):** move them from PlayerPrefs into the settings JSON?
4. **Auto-Cloud (D8):** settings, profile, the saved run and ghosts; not `runs/` or `botnav/`.
5. **Deck (D5, D7):** FULLSCREEN WINDOW by default; UI SCALE 110 % on a Deck.
6. **Confirm on a Nintendo pad (P6):** by position (today) or by label.
7. **WEAPON WHEEL in CONTROLS (P8).**
8. **Pause on focus loss and on a pad disconnect (P11, P12):** recommended yes.
9. **IL2CPP on this machine (N2, D-40):** the module and the C++ tools.
10. **Switch quality level and the CPU knob (N4, N5).**
11. **Gyro (P10, N6).**
