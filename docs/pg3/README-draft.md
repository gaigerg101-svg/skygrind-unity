# SKYGRIND (README draft for ASU AME 294, Portfolio Game 3)

Status: DRAFT by a coding agent (Claude Sonnet 5.5), 2026-10-28, from the code and docs in this repository. Controls come from
`PortExport/config.json` and `Assets/Scripts/Core/Gamepad.cs`; numbers come from the config files named. "Not checked" means I
did not run the game or open the Editor. Nothing here was play-tested by the agent. Updated 2026-10-28 13:15 for the Release build.

## 1. What SKYGRIND is

SKYGRIND is a first-person movement shooter at dusk, made in Unity 6000.3.7f1 (URP). GG's one-line brief (2026-10-03, quoted in
`DESIGN-PILLARS.md`): "cod zombies meets ultrakill as the main, but other games should be inspired". You slide, dash, wall ride
and double jump through small arenas, shoot with a pistol and other guns, punch and parry with a gauntlet, and fight a boss at the
end of each level. A style meter rates how well you move and kill, and the rating multiplies your points.

Two ways to play, both in the same build (the title screen's MODE row; CAMPAIGN is the default mode per `design.n26.levelSelect`,
`Game/SurvivalGame.Init.cs`):
- **Campaign.** A level select, then levels of small rooms with locked doors. Clear a room's waves and its door opens. Each level
  ends in a boss: level 1 THE WICK (a climb up a furnace stack), 2 THE ICEBREAKER, 3 THE KILN WYRM, 4 THE LOOM, 5 THE CROWN
  (`docs/BOSSES47.md`). A level is ranked S, A, B or C on time, kills and style. Level 6 is parked (`docs/memory/THREADS.md` T-57).
- **Survival.** Rounds of enemies on a zoned map; points open gates and buy guns and perks, in the style of CoD Zombies. The
  fixed-step simulation, the movement and the Survival rules were ported from GG's earlier browser game (PG2).

The movement is the browser game's, exact to the bit: at every commit that touches `Skygrind.Core`, the golden vectors show 0
mismatches and all 16 recorded trajectories replay bit-exact (CLAUDE.md rule 3). The simulation runs at a fixed 120 Hz.

## 2. The Reward, Damage and End triad as it works in the game

### Reward: points, buys, power-ups, kills
- **Points.** From `PortExport/config.json` (`points`): you start a Survival run with 500; 10 per hit; 60 per kill; 100 per
  headshot kill; 130 per punch kill; 50 per parry; 100 when a round clears. Your style rank multiplies points: x1, x1.1, x1.25,
  x1.5, x1.75, x2 up the six ranks (`rankMult`; the HUD shows a letter and a word such as "D DULL"). Standing still gets you
  EXPOSED, which halves points (`campMult` 0.5) and pauses regen.
- **Buys.** Hold E (0.35 s, `buy.holdTime`) at a gate, a wall buy, a perk machine or the Mystery Box. Gates open new zones; the
  ARMORY wall sells the STITCHER (1250), COMET (2000) and LONGWAVE (3000); the Mystery Box costs 950; THE FORGE upgrades the gun in
  hand for 5000 (`PORT-STATUS.md`, NIGHT10 and NIGHT7 rows). In the campaign, points carry across levels as a wallet and are spent in
  a SHOP between levels (`design.n33.shop33`).
- **Power-ups** (`config powerUps.types`): DOUBLE POINTS (30 s, x2), OVERCLOCK (20 s), SHOCKWAVE (a burst with damage 99999), STASIS
  (6 s), REFUND. They drop from kills; six kills inside 4 s, or three in the air inside 2 s, drop one on purpose (`styleDrop`).
- **Kills and unlocks.** A kill shows a white kill marker and a points pop. In the campaign each level also earns you a new weapon
  (`Design/levels/unlocks.json`; the current order is not re-checked here). In Survival a MODS draft offers run upgrades every few
  rounds.

### Damage: what hurts you
- You have 100 health (`health.max`). At 0 you go DOWN for 1.5 s (`health.downTime`).
- Enemies: husks and swarmers bite, spitters throw globs, drones fire orbs, chargers stampede, lancers use a beam, wardens carry
  shields; bosses use telegraphed attacks. Yellow marks an attack you can parry (`docs/BOSSES47.md`, common rules).
- Hazards: lava costs 25 % of your maximum health per touch and knocks you up 9 m/s, with 0.5 s grace (`design.n30.lava`); in the
  campaign a fall below a room's floor line returns you to the room's spawn (`respawnY`, event `fallReturn43`); in Survival a fall
  is a death (NIGHT7's "fall = death" rule). Not re-checked on every level.
- Healing is earned, not passive: health refills when a round clears (`design.regen.n20`, "STRICT") and from parries and close kills
  (`docs/VERBS.md` rows 40 and 42). Camping is punished by EXPOSED.

### End: how a run ends, and how you restart
- **Loss.** Survival: health 0, the DOWN screen, then RESULTS. Campaign: a death retries the room after 1.2 s with the same seed
  (`design.n24.campaign.retryDelay`, `Core/Campaign24.cs` header).
- **Win.** Campaign: kill the level's boss; the level clears (event `layerClear`) and the campaign RESULTS screen shows your ranks with
  CONTINUE, RETRY LEVEL and TITLE. Survival: an exfil (`design.exfil`, ON): from round 11 and every 5 rounds, call it at the radio
  (hold E), reach the site within 90 s, then get 35 kills within 85 s; success ends the run as a win ("RESULTS: EXFIL"), failure as a
  death.
- **Restart without the Editor.** Press **R** on the DOWN screen, the RESULTS screen or the PAUSED screen
  (`Game/SurvivalGame.Flow.cs`, `Restart()` and `RestartRule`): from DOWN it skips the wait and starts a new run on a new seed. The
  RESULTS screen also has RETRY SAME SEED. PAUSED has a RESTART button. The title's QUIT closes the game.

## 3. Controls

Open CONTROLS from the title or the pause menu to see the live table and to rebind keys. Ctrl, Esc, F5, F11, F12 and the Meta keys
cannot be bound (`Core/Bindings.cs`, `Unbindable`).

### Keyboard and mouse (defaults from `PortExport/config.json`, `input.bindings`)
| Action | Input |
|---|---|
| Move | W A S D |
| Look | Mouse |
| Jump, double jump in the air | Space |
| Dash (a hold after the dash sprints; exact keyboard timing not re-checked) | Left Shift |
| Slide; slam when pressed in the air | C |
| Fire (hold to charge on the KESTREL and THUNDERCLAP) | Left mouse button |
| Zoom (most guns) or the gun's alt-fire | Right mouse button |
| Punch and parry | F |
| Buy, hold | E |
| ARCLINE grapple, hold (owned from level 4 in the campaign: `docs/VERBS.md` row 17) | G |
| Reel in | Z |
| Last weapon | Q |
| Weapon wheel, hold | Tab |
| Weapon slots | Number keys; the mouse wheel also changes weapon (`Game/CombatReader.cs` reads it; which way it scrolls: not checked) |
| Lock-on | V |
| X-ray | X |
| Perk list, hold | I |
| Special | B |
| Restart (DOWN, RESULTS, PAUSED) | R |
| Pause | Esc |

### Gamepad (layout v4, the live default)
Layout v5 (punch on RB) is switched off (`design.n25.meleeFire.enabled` is false), so v4 applies: `Gamepad.cs`
`DefaultLayoutV4`, with `design.n19`, `design.n20` and `design.n24.padY` all enabled. Xbox names; the game shows glyphs for Xbox,
PlayStation or Switch pads from the Kenney set.

| Action | Input |
|---|---|
| Move, look | Left stick, right stick |
| Jump | A |
| Slide; slam in the air | B |
| Buy (hold) and reel in | X |
| Next weapon (tap); weapon wheel (hold 0.25 s) | Y |
| Previous weapon | D-pad left |
| Last weapon | D-pad right |
| Fire | Right trigger |
| Zoom or alt-fire | Left trigger |
| Dash | Left bumper |
| Special and ARCLINE grapple | Right bumper |
| Sprint (click toggles; SETTINGS can make it HOLD) | Left stick click |
| Punch and parry | Right stick click |
| Lock-on | D-pad up |
| X-ray | D-pad down |
| Perk list | Select |
| Pause | Start |

Aim assist defaults: mouse off, pad "assist" (`design.aimAssist`); change it under SETTINGS.

## 4. Download and run

- Download: the latest GitHub Release (https://github.com/gaigerg101-svg/skygrind-unity/releases/latest), or itch.io (https://gaiger101.itch.io/skygrind).
- Platform: Windows x64 only. The Player is built for `StandaloneWindows64`; no other platform was built or tested (not checked).
- Run: unzip the folder and start `SKYGRIND.exe`. The folder holds `SKYGRIND.exe`, `SKYGRIND_Data`, `UnityPlayer.dll`, `D3D12`,
  `MonoBleedingEdge`, `DirectML.dll` and `UnityCrashHandler64.exe` (as in `Builds/n74-play`, 394 MB unpacked).
- The latest Release build is `Builds/pg3-t11` (commit 1d3ba26e7, 2026-10-28): the PG3 sound and music (41 tracks), the Tripo models for every
  gun, enemy and four bosses, the lower jump. Checked with the sound muted: the built-in smoke test passed, the movement check passed on all
  four levels, the WICK, ICEBREAKER, CROWN and LOOM boss checks passed. The earlier `pg3-final2` Release (sound, no models) stays up.
- Speed reference, old: at tag `u2a-demo` (2026-10-02) the Player's frame time was median 3.35 ms and worst 1 % 8.37 ms on GG's
  laptop. It has not been re-measured for PG3 (not checked).
- Optional start-up flags (CLAUDE.md, HANDOFF.md): `-skygrindSmoke -logFile <file>` runs the built-in smoke test and quits;
  `-skygrindBench 60` measures frames with 24 enemies; `-skygrindRows72 <row>:0` turns one night's change off, for example
  `sound74:0` turns the sound off.

## 5. Build steps

1. Install Unity **6000.3.7f1** (`ProjectSettings/ProjectVersion.txt`) with the Windows build support. Two packages come from
   GitHub (LitMotion v2.0.2 and UIEffect 5.9.7, `Packages/manifest.json`), so Git must be installed for the first import.
2. In Unity Hub choose Add, select this folder, open it with 6000.3.7f1. The first import takes a while; URP 17.3.0.
3. Scenes: `Assets/Scenes/Survival.unity` (the game, first in the build list) and `Assets/Scenes/Arena.unity` (the movement test
   arena). In the build window (File, Build Profiles in Unity 6) tick Survival, then Arena. The working copy of
   `ProjectSettings/EditorBuildSettings.asset` also lists SampleScene and Enemies; CLAUDE.md names only the two.
4. Run the menu **SKYGRIND, Sync Port Data** once, so `Assets/StreamingAssets/PortExport/` matches `PortExport/` (the Player reads
   that copy).
5. Choose target Windows, x86-64, then Build. The project's own command, with the Unity CLI plugin, is
   `unity command build --target StandaloneWindows64 --outputPath Builds/Stage1/SKYGRIND.exe` (CLAUDE.md).
6. Check the build: `SKYGRIND.exe -skygrindSmoke -logFile smoke.log` should end with "DONE: all steps passed" (the line the logs
   quote). Do it with no other window taking focus: a focus change pauses the smoke (L-015).
7. Tests: Window, General, Test Runner, EditMode. The tests are in `Assets/Tests/EditMode` (assembly `Skygrind.Core.Tests`). From a
   shell with the Editor open: `bash Tools/unity-loop.sh tests`.

## 6. Repository layout

| Path | What it holds |
|---|---|
| `Assets/Scripts/Core` | `Skygrind.Core`: the pure C# simulation and every rule, no Unity references; ports of the web modules; `JsMath` |
| `Assets/Scripts/Game` | `Skygrind.Game`: MonoBehaviours, input, views, UI (uGUI and TextMesh Pro), `SurvivalGame.cs`, `Pg3Audio.cs` |
| `Assets/Scripts/Bench`, `HudFx20` | The enemy bench; HUD effects |
| `Assets/Editor` | Scene and prefab set-up scripts, `SKYGRIND/Sync Port Data`, `WormBossSetup.cs` |
| `Assets/Tests/EditMode` | The EditMode test suite |
| `Assets/Scenes`, `Shaders`, `Resources`, `StreamingAssets` | Scenes, shaders, runtime-loaded assets (fonts, the worm parts), the Player's data copy |
| `Assets/Art/WormBoss` | The three Tripo FBX files for THE KILN WYRM (section 7) |
| `Assets/ThirdParty` | Kenney Input Prompts and Kenney Particle Pack (both CC0) |
| `Assets/Fonts` | Google Fonts families with their OFL licences |
| `PortExport/` | Config, golden vectors, 16 trajectories, maps exported from the web game; what the tests read |
| `Design/` | `unity-config.json` (every Unity-side number and switch), `levels/`, `bosses/`, `maps/`, `weapons/` |
| `Tools/` | `unity-loop.sh`, `night/` (night tooling), `SimHarness`, `CoreHarness`, `pg3/` (sound generation, snapshot) |
| `docs/` | `PRINCIPLES.md`, `VERBS.md`, `DESIGN-RULES.md`, `ROOMS.md`, `THIRD-PARTY.md`, `memory/` (lessons, GG's words, ledgers), `pg3/` |
| `Research/` | The research and forge folders behind each night's prompt |
| `NIGHTn-LOG.md`, `NIGHTn-REPORT.md` | Each night's record. The prompts `NIGHTn.md` are untracked by rule (CLAUDE.md rule 1) |
| `CLAUDE.md`, `HANDOFF.md`, `tasks.json` | The standing rules and the state a new session starts from |

## 7. AI-made assets

- **3D models (Tripo).** The only Tripo files now are the three KILN WYRM parts in `Assets/Art/WormBoss/` (made 2026-10-08, drawn
  only when `design.n26.wormVisual.enabled` is true; it is false now). PG3 models go in `Assets/Art/Tripo/` (the folder does not
  exist yet). Prompts, routes and credits: `docs/pg3/PROMPT_LOG.md` section 4.
- **Sound (ElevenLabs).** 138 sound effects (`Assets/Resources/Audio/PG3/`) and 31 music tracks (`.../PG3/Music/`, ElevenLabs
  Music, music_v2_5). `node Tools/pg3/sfx-gen.cjs` and `music-gen.cjs` call the ElevenLabs APIs (key in the `ELEVENLABS_API_KEY`
  environment variable); `layer-mix.cjs` mixes the 15 layered weapon / hit sounds in ffmpeg; `normalize.cjs` levels every clip.
  `Game/Pg3Audio.cs` plays each effect on a game event; `Game/MusicDirector.cs` runs the music. Turn them off with
  `-skygrindRows72 sound74:0+music74:0`. Every prompt: `docs/pg3/AUDIO_PROMPTS.md`.
- **Code.** Written by Claude Code (Opus 5.5 and Sonnet 5.5 are named in the commit trailers), directed by GG's night prompts.
  `docs/pg3/ATTRIBUTION.md` lists every tool, font, package and ported source.

## 8. Known problems (from the NIGHT74 report, `NIGHT74-REPORT.md` section 1)

- The rooms are the top fault. gg-proxy (an agent that plays GG from his recorded words) reads g8 as "random pillars" and a3 as
  clutter, and says the rooms overall are still corridors and block faces with nothing that makes you use a mechanic (better in
  g1, g4 and kb3).
- Colours across rooms and bosses still do not belong together; cyan light columns can sit on the crosshair; some bosses read stiff.
- Movement and gameplay feel cannot be judged from a frame and were not judged by a person in the last night's build.
- Moving-camera flicker was measured on all five levels and not reduced (1.9 to 3.2 % of the frame, pooled).

## 9. More

- Credits and licences: `docs/pg3/ATTRIBUTION.md`. References and moodboard: `docs/pg3/REFERENCES.md`.
- Prompt log, error recovery, reflection: `docs/pg3/PROMPT_LOG.md`.
- What GG has asked for, in his words: `docs/memory/OWNER.md`.
