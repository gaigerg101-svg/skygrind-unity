# SKYGRIND references (ASU AME 294, Portfolio Game 3)

Status: DRAFT by a coding agent (Claude Sonnet 5.5), 2026-10-28. Every quote is GG's, copied from the file named, typos kept. A
reference here means "GG named it" or "a file in the repo cites it". `TODO(GG)` marks what only you can add.

## 1. Concept inspiration (GG's words, dated)

SKYGRIND in one line, GG, 2026-10-03 (`DESIGN-PILLARS.md`): "cod zombies meets ultrakill as the main, but other games should be
inspired". The brief behind it (`ZOMBIES-ULTRAKILL-RESEARCH.md` header): "want this game to be like cod zombies meets ultrakill as
the main, but other games should be inspired... pull from examples with real users and proven data".
`docs/PRINCIPLES.md` states the mix: "ULTRAKILL's movement and style, CoD Zombies' rounds and wall buys, a boss rush's bosses, and
Terraria's progression and readable items, at dusk."

| Date | GG's words | Reference | Source |
|---|---|---|---|
| 2026-10-05 | "feel like CoD zombies and ULTRAKILL with personality, not a bland map" | CoD Zombies, ULTRAKILL | `nights/NIGHT15.md` header |
| 2026-10-06 | "don't copy ULTRAKILL's UI but it should feel similar" | ULTRAKILL's HUD | OWNER.md (NIGHT19.md) |
| 2026-10-06 | "games like ULTRAKILL ... when you know how to use the movement it becomes easy, so I guess that's not too much of a problem" | ULTRAKILL's movement skill curve | OWNER.md (NIGHT19.md) |
| 2026-10-06 | "Copy an open-source project that has way better dash lines that mimic speed ... Go actually go into the GitHub, I'm serious." | Open-source speed lines (section 5) | OWNER.md (NIGHT20.md) |
| 2026-10-08 | "think about ultra kill especially and Call of Duty zombies how the game slowly builds up and introduces mechanics to you" | The teaching campaign | `Research/2026-10-08-wave24/GG-REQUEST.md` |
| 2026-10-08 | a chase boss where "you're chasing the boss"; "V2" | JoJo part 5 (the forge's reading of his "Giacho / giaccio from Jojo part 5"), ULTRAKILL's V2 fight as the difficulty spike | same file, the forge's reading table |
| 2026-10-08 | a worm boss, asked twice | Terraria's worms (the forge's reading) | same file |
| 2026-10-09 | "think about the design of Ultra Kilikow" | ULTRAKILL's small, action-packed rooms | OWNER.md (w29) |
| 2026-10-09 | "I want it to lean more into a PlayStation-2 or ULTRAKILL-style gritty aesthetic ... UI elements that buzz, glow briefly, or flicker like an old TV" | PS2 era, old TV | OWNER.md (w29) |
| 2026-10-09 | "it needs to feel like speed skating" | speed skating on ice (level 2) | OWNER.md (w29) |
| 2026-10-11 | "almost like that VR game where you're dodging and striking in slow motion as the world bends around you" | SUPERHOT VR (the forge's reading) | OWNER.md (w33) |
| 2026-10-11 | "overhaul the wall riding to feel a lot more like Lucio from Overwatch" | Lucio's wall ride | OWNER.md (w34) |
| 2026-10-16 | keep "the pill-shaped enemy concept" | The enemy language | OWNER.md (n42-r4 row) |
| 2026-10-19 | "very inspired by Ultra Kill and Doom" | ULTRAKILL, DOOM | OWNER.md (w48 row) |
| 2026-10-20 | "each boss should have its own distinct color palette and lighting identity ... similar to how JoJo's Bizarre Adventure uses dramatic color changes" | JoJo's colour turns | OWNER.md (NIGHT54.md) |
| 2026-10-26 | "I really want the jump height gravity and movement to feel a lot more like that game almost a one to one copy" | ULTRAKILL's jump and gravity | OWNER.md (w71 row) |

Platform target, GG 2026-10-05 (auto-memory `skygrind-targets-and-taste`): "will go on Steam and hopefully Nintendo Switch", which is
why the pad comes first.

## 2. Gameplay references (what the repo cites for how a thing should work)

| Reference | Used for | Where it is cited |
|---|---|---|
| ULTRAKILL | Movement numbers, style rank, parry, the dash pips, wall jumps (its 3 wall jumps: `docs/DESIGN-RULES.md` M-2), HUD messages that name the bound key | `Research/2026-10-26-ultrakill/movement.md` (sources: https://ultrakill.wiki.gg/wiki/Movement, `/wiki/Stamina`, `/wiki/SSJ`, `/wiki/Options`; a third party's reading of the game's code at https://github.com/Rand0mNme/V1-Ultrakill-v01-/blob/main/docs/ULTRAKILL_values.md, with confidence codes P / D / C / N); `docs/PRINCIPLES.md` rule 2 |
| CoD Zombies | Points per hit and kill (10; 60, 100, 130), rounds, gates, wall buys, perks, the Mystery Box, the power switch, the exfil | `ZOMBIES-ULTRAKILL-RESEARCH.md`; `design.exfil` `_why` in `Design/unity-config.json` (cites the Treyarch Cold War guide https://www.treyarch.com/game-intel/2020/10/Black_Ops_Cold_War_Zombies_Gameplay_Guide and Black Ops 6's exfil) |
| Terraria | Worm bosses, readable items, progression | `docs/PRINCIPLES.md`; `Research/2026-10-08-wave26/RESEARCH.md` (not read) |
| Neon White | A one-key restart | comment in `Game/SurvivalGame.Flow.cs` ("Neon White's one-key restart") |
| Celeste, Titanfall 2, a parkour mod | Forgiving timing windows (coyote, buffer) | `docs/DESIGN-RULES.md` M-3 |
| TF2 and Quake | A self-push resolved on the ground is weak; combined inputs combine | `docs/DESIGN-RULES.md` M-6 |
| Hakita (ULTRAKILL's developer) | Enemy roles by behaviour, not hit points | `docs/DESIGN-RULES.md` line 77 |
| Gunfire Reborn, Sker Ritual, Killing Floor 3 | The closest indie and AA comparables, with Steam review numbers read 2026-10-04 | `Research/2026-10-04-comparables.md` |
| Vampire Survivors | A browser game that moved to Unity: the port precedent | `nights/NIGHT9.md` |
| Balatro, Brotato, Deep Rock Galactic | The opt-in difficulty ladder (HEAT) | `design.heat` `_why` in `Design/unity-config.json` |

Not copied: the web game's notices state "Nothing comes from ULTRAKILL: the HUD and viewmodel are 'ULTRAKILL-inspired' in layout and
feel only, with no ULTRAKILL code, models, textures, fonts or logos" (`C:\Users\gaige\dev\skygrind\THIRD_PARTY_NOTICES.md`). The
Unity rule (`docs/THIRD-PARTY.md`): copy only MIT, Apache, CC0 or Unlicense; "A reading of a repo for its pattern or parameters is
not a copy."

## 3. Target look

From CLAUDE.md rule 8, copied: "Look: a dusk palette. Cyan is player-usable; red-orange is enemies and danger only; magenta is RIVAL
only; pale violet-white is the boundary. UI text is at least 16 px at 1080p. Fonts: the converted theme fonts in
`Assets/Resources/Fonts/` (TMP SDF assets: Archivo, Barlow, Big Shoulders, Chakra Petch, Saira, Tourney)."

From `IDENTITY.md` (GG after NIGHT16: "the text and the font and the colors are just very bland, it needs identity."): "**Black-italic
speed on near-black ink, one cyan you own, gold you earn.** Everything leans forward on one 12 degree slant". Display face Archivo
Extra Condensed Black Italic, body Archivo SemiBold and ExtraBold, numerals Tourney Black Italic.

From `docs/PRINCIPLES.md` rule 7: "Low detail, strong look: the PS2-era treatment (low internal resolution, dither, colour steps,
vertex snap) must be visible in every frame; lights hang from real fixtures and light the room; the UI moves a little like an old TV.
No clutter for its own sake." Rule 3: "One colour per meaning".

Never again (CORE.md, GG's rulings): screen-border speed lines; a stamina bar mid-screen; filled HUD boxes; the gun sliding to the
centre on zoom; a grapple target diamond.

What the current frames do (viewed by the agent): scanlines and a colour fringe from the TV pass; a teal, violet and tan palette
per level; cyan for ramps, pads, light columns and weak points; red-orange pill-shaped enemies; a slanted dark HUD.

## 4. Target sound

GG's recorded words about the sound he wants: none found. I searched `docs/memory/OWNER.md` and every `GG-REQUEST.md`; CLAUDE.md rule
7 kept sound and music GG's own until 2026-10-28, when he said "Lift it" for the assignment (`Game/Pg3Audio.cs` header).
`TODO(GG)`: name two or three games or tracks whose sound you want, and one line on the mood.

What the repo holds for sound:
- The event list a sound can hang on: the web's `docs/AUDIO-HOOKS.md` and `PORT-STATUS.md` (events `hit`, `kill`, `points`,
  `pointsSpent`, `buy`, `powerUpGrab`, `hurt`, `death`, `bossDown`, `runEnd`, `roomClear`, `roomRetry`, `exfilDone` and more).
- The music brief from PG2 (`C:\Users\gaige\dev\skygrind\MUSIC-BRIEF.md`): a five-note motif A3, E4, F4, E4, A4, title at 172 bpm,
  combat at 188 bpm an octave down, victory with F raised to F sharp; plus code-made sketches in `music/`.
- The PG3 prompts in `Tools/pg3/sounds.json`, whose music line reads: "Dark driving synthwave combat loop at dusk, pulsing bass, punchy
  drums, tense arpeggios, 128 bpm, seamless loop, instrumental". These are the agent's wording until you confirm
  (`PROMPT_LOG.md` section 5).

## 5. Open-source references that were read or ported

Licences were read by the agents before use. "Ported" means rewritten in C# with credit; "read" means looked at for a pattern only.
Full licence texts and credits: `docs/pg3/ATTRIBUTION.md` and `docs/THIRD-PARTY.md`.

| Source | How it was used |
|---|---|
| keijiro/KvantStream (MIT) | Ported rule: a streak's tail is head plus velocity times an exposure (`Game/SpeedLines20.cs`) |
| MikeFrom/Godot-Particles-Tunnel-Speed-Lines (CC0) | Ported rule: births on a ring, quads along the tunnel wall (`SpeedLines20.cs`) |
| MirzaBeig/Anime-Speed-Lines (Unlicense) | Read only; GG banned the border effect |
| chrisnolet/QuickOutline (MIT) | The smoothed-normal technique, rewritten (`Skin52.cs`, `Enemy.shader`) |
| ColinLeung-NiloCat/UnityURPToonLitShaderExample (MIT) | The cel ramp's form, rewritten (`Enemy.shader`) |
| Minoteaur / UnityHFSM, https://github.com/Inspiaaa/UnityHFSM (MIT; the name and URL are as `Core/Enemies/BossFsm27.cs` writes them) | The state-machine pattern for the boss base, rewritten; no file copied |
| orangeduck/Spring-It-On (MIT, Copyright (c) 2021 Daniel Holden) | Ported line for line: critically damped half-life springs for the gun's recoil and sway (`Game/Viewmodel.Springs.cs`, licence text kept in the file) |
| Bauxitedev/godot-motion-blur (MIT, Copyright (c) 2018 Bauxitedev) and daniel-ilett/blur-pro-urp (MIT, Copyright (c) 2026 Daniel Ilett) | Ported: the boost's travel blur, linear velocity only (`Game/Render/TravelBlurFeature.cs`, `TravelBlur.shader`) |
| dip000/my-godotshaders, `WindTrailWithLoops` (MIT, Copyright (c) 2023 Adrián) | Ported: the long slide's wind wisps (`Game/MovementFx.Wind20.cs`) |
| CoffweDev/physics-based-wallrunning-unity (MIT) | Parameters read as a cross-check of the wall-ride feel |
| Titanfall 2 and Karlson wall-run repositories | Not copied |
| three.js r186 Octree (MIT) | The web game's collision world, reproduced exactly in `Core/ThreeOctree.cs` |
| V8 and fdlibm | Maths reproduced bit for bit in `Core/JsMath*.cs` |

## 6. Moodboard frame candidates (real files)

All paths are relative to the repository root, all are 1920 x 1080 and come from real Player reels of the NIGHT74 build. I opened
the first six and describe them; the rest exist and I did not open them. In every reel frame the health number at bottom left
reads 9999, not the 100 maximum in the config; I did not check why (probably a reel setting), so recapture or crop for public use.

Viewed:
1. `Logs/n74/pairs/LV-foundry65/on.png`: level 1, a teal-green corridor, a cyan launch pad with a light column, three red-orange pill
   enemies on a wall, "11 LEFT", the KESTREL panel "RMB ZOOM".
2. `Logs/n74/pairs/LV-iceline67/on.png`: level 2, THE ORB YARD, violet-blue walls on a tan floor, cyan ramps and columns, a drone.
3. `Logs/n74/pairs/LV-kiln67/on.png`: level 3, a salmon wall against violet, cyan trim, three amber enemies, THUNDERCLAP panel.
4. `Logs/n74/pairs/LV-spire67/on.png`: level 5, THE BELL WALLS, a purple floor under teal walls, chevron ramps, red enemies.
5. `Logs/n74/pairs/B1/on.png`: THE CROWN, a ring of ivory and amber spokes with the cyan open weak point, "GEMS 20 / 20", the TORCH.
6. `Logs/n74/pairs/B3/on.png`: THE KILN WYRM staggered, the white kill-flash band, "STAGGERED: HOLD LEFT CLICK, SLUG IT", an orange grid floor.

Not opened:
- Rooms: `Logs/n74/pairs/{g1,g2,g3,g4,g5,g6,g7,g8,a2,a3,a5,kb2c,kb3,v2}/on.png` (each folder has `on.png`, `off.png` and `sheet.jpg`;
  `on` is the NIGHT74 room, `off` the room it replaced).
- Effects: `Logs/n74/pairs/spring74/on.png` (enemy squash on a hit), `Logs/n74/pairs/B2/on.png` (the off-screen weak-point arrow),
  `Logs/n74/pairs/L2/on.png` (the edge arrow to unseen enemies).
- Tours: `Logs/feel/n74-play/tf/tour65/f0000.jpg` to `f0550.jpg` (level 1), `ti` (level 2), `tk` (level 3), `tp` (level 5), every 50th frame.
- Boss reels: `Logs/feel/n74-play/bk/boss63/f0000.jpg` to `f0350.jpg` (THE KILN WYRM) and `bp/boss63` (THE CROWN).
- Before and after sheets: `Logs/n74/look/a8-on-off.jpg`, `Logs/n74/look/kb3-on-off.jpg`.
- THE WICK, newest frames found: `Logs/feel/e53-freeze2/base54wick/a147/f0000.jpg` onward (441 frames, 2026-10-20, an older build).

`TODO(GG)`: pick 6 to 9 frames; add two or three reference frames from games you named (ULTRAKILL, CoD Zombies) with their source
and a one-line note on what you take from each.
