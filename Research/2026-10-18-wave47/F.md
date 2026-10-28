# NIGHT47 builder F: research notes and baselines (round 0)

Builder F owns G10-G24 and the TORCH part of G43 (NIGHT47-BRIEF table; absorbs NIGHT47.md's X, U, E). Each research line is marked
**Verified** (I read the page tonight) or **Reported** (a search summary or a secondary source; not read in full). Each baseline line says
how it was measured; "not checked" where it was not. The cards are in `F-cards.md`.

## 1. Research (tonight)

| # | source | what it says (the part we use) | mark |
|---|---|---|---|
| R1 | bottosson.github.io/posts/oklab | OKLab: linear sRGB -> LMS (M1) -> cube root -> M2; the inverse cubes. "Blending two colors should result in even transitions"; CIELAB / CIELUV / HSV "show hue shifts towards purple" when blending toward blue, OKLab predicts hue well (hue RMS 0.49 vs JzAzBz 0.43). The code is public domain / MIT. Use: the health bar's 6-stop gradient and the overheal blue -> purple ramp interpolate in OKLab, so green -> yellow never goes muddy and blue -> purple does not drift to magenta early. | Verified |
| R2 | docs.unity3d.com/ScriptReference/Shader.Find.html | "a shader might be not included into the player build if nothing references it ... Shader.Find will work only in the Editor, and will result in the pink error shader in a build"; three fixes: a material in a scene, Always Included, a Resources folder; "Returns null if the shader is not found"; prefer references to names. | Verified |
| R3 | docs.unity3d.com/Manual/urp/lighting/light-limits-in-urp.html (6000.3) | Forward: 1 main + 8 additional lights per object (9), 256 additional lights per camera on desktop / console, 32 mobile. Forward+ / Deferred lift the per-object cap. The page does not say how lights over a limit are chosen. | Verified |
| R4 | docs.unity3d.com/Manual/urp/rendering/forward-rendering-paths.html | Forward+: "no limit to the number of lights that can affect each GameObject", "still a limit on the number of lights visible per-camera"; lights are binned per screen tile. | Verified |
| R5 | Unity Forward+ limitations page (search summary) | The Forward+ per-camera visible light cap is 256 on desktop and includes the main light. | Reported |
| R6 | learn.microsoft.com XAG 101 | Minimum default body height (descender to ascender): console 26 px at 1080p, PC 18 px; scaling to 200 % without loss; an outline pixel counts toward height only above 2:1 contrast to the background; lines of text in sentence case on request (a 1-2 word label is exempt); letter spacing >= 0.12x the size, word spacing >= 0.16x; a black outline on gameplay text "to increase visibility, regardless of the color of elements behind the text" (Fenyx example); glyphs inside icons meet the same minimum. | Verified |
| R7 | NIGHT47.md section 10 (the lead's digest): uhiyama-lab hit feedback (flash 80 ms; hitstop 0-30 / 50-80 / 100-150 ms; layer order flash, particles, shake, hitstop), Doom Eternal flame belch, RocketBrush shape language, bugnet pink materials | used as the lead read them | Verified (by the lead) |
| R8 | infovore.org/?p=5275 (summary of Jan Willem Nijman / Vlambeer on Nuclear Throne's pistol): the camera kicked back 6 px against the aim, +4 screenshake that fades fast, the gun sprite moves back then snaps | the kick is opposite the aim and the gun returns with a snap (a spring, not a lerp) | Reported |
| R9 | doomwiki.org/wiki/Blood_punch (search summary) | Doom Eternal's Blood Punch: a shockwave that hurts the target and the bodies around it; two rings, an inner high-damage one and an outer shockwave that makes larger enemies falter; breaks shields; one charge stored, earned by kills | Reported |
| R10 | discussions.unity.com/t/how-to-shockwave-effect/780517 (search summary) | a shockwave distortion = a transparent ring that samples the camera's opaque texture (`_CameraOpaqueTexture`) with an offset; the URP asset needs Opaque Texture on | Reported |
| R11 | fab.com "Customizable Delayed Health Bar" listing (search summary) | the delayed (ghost) bar: a second bar trailing the main one so the player sees the damage just taken, catch-up speed tunable, updated on change, not per frame | Reported |
| R12 | Unity LineRenderer properties page | not read (the URL 404'd); the LineRenderer's width curve, colour gradient, texture mode and alignment are from memory of the API, not checked tonight | not checked |
| R13 | swearsoft.itch.io/jackmclantern/devlog/76709/direction-based-hit-reaction | R4 hitzone47: the hit's side from dot products against the enemy's forward (front / back, then left / right), a tolerance band for centred hits: 8 directions + 1 centred; each a 10-frame single-pose hit animation on its own layer above the rest, upper-body masked so the body keeps walking | Verified |
| R14 | wayline.io/blog/making-combat-feel-impactful | R4 hitzone47: the enemy should react to a hit ("staggering, flinching, or being knocked back"), the feedback "almost simultaneously" with it ("even slight delays can diminish the impact"); no numbers for hitstop, knockback or kill slow motion | Verified |
| R15 | itch.io/post/2862542 (search summary) | R4 finale47: a player found a final-hit pause long; the developer's reply: the longer pause belongs to the boss-defeated moment, ordinary hit stops are barely there | Reported |
| R16 | thegameswiki.com Onimusha: Way of the Sword, Break Issen (search summary; a fan wiki) | R4 finale47: a boss finisher freezes the boss mid-motion while the camera moves in, short, no extra input | Reported |

## 2. Baselines (measured tonight unless marked)

### B1. The LT blast's knockback on a husk (G20; harness, the config's ON game, pulse47 off)
`dotnet run --project Tools/SimHarness -c Release -- n47feel blast --seeds 5` (new: `Tools/SimHarness/Harness.N47.Feel.cs`), 22:58:
- 5 / 5 blasted; **husk knockback 1.40 m** (min 1.40, max 1.40; at the knockback's end 1.40 m: the husk slides 7 m/s for 0.2 s = 24 steps,
  exactly `weapons.gauntlet.knockback` x `knockbackTime`, then walks back); **damage 64** (`weapons.gauntlet.damage` 32 x
  `design.n27.pulse27.blastDamageMult` 2; a round-1 husk has 40 hp, so it dies); the player's peak speed in the 2 s **5.9 m/s** although
  `boostSpeed` is 16 m/s (why: not checked; likely the boost into the husk's body or the spawn's walls: the harness printed "no open line of
  4 m from (0,40)" on the survival map's spawn).
- The blast's geometry today (PulseCharge27.cs:228-246): bodies within 1.5 m (+ their radius) of the aim line, 0-6 m ahead, once each:
  `e.Knockback(dir, 7 m/s, 0.2 s)`, `Damage(64)`. No lift, no launch, no burst; a straight horizontal shove of 1.4 m. This is GG's "weak gust
  of wind with no weight" in numbers: launch36's punch throws a body ~12 m (16 m/s x 0.8 s, lift 5 m/s) and bursts it on a wall; the
  gauntlet's ultimate pushes 1.4 m.
- G20's target: >= 2.8 m (2x). The card aims at ~10 m (7x).

### B2-B6 (code inventories, tonight)
In the cards file's sections (measured by read-only code inventories run tonight, each with file:line): the render diagnosis (B2), the lava
lights (B3), the HUD's text sizes and the reticle bar's information (B4), the enemy models (B5), the weapon feedback layers (B6). See
`F-cards.md` sections 1-6 for the numbers and the file:line evidence. The headline numbers:
- **B2 render (G10):** 13 runtime `Shader.Find` names; every one is Always Included (4) or in `Assets/Shaders/Resources/` (10) or a scene
  material's: hypothesis (a) refuted in code. **Cause found (d):** `Doors38View.cs:147-148` re-makes its material on each re-attach (each scene
  load) while the DontDestroyOnLoad shimmer veil (`:433-451`) keeps the destroyed one: a full-view magenta flash for 0.28 s on the next pass
  through an open door after any level change. Fixed behind door47 (commit 2ed201bd). Not yet seen in a Player (the lead's reel).
- **B3 lava (G11):** the lava surface lights nothing; two roaming real lights do it: LightKit29's lava slot re-picked every 0.25 s with no fade
  (`LightKit29.cs:373-411`) and Look43View's light teleported per frame to the pool under the feet (`Look43View.cs:478-486`). Lava fields:
  foundry 9 (1972 m2, 21 planned lava lights), iceline 3 (168 m2), kiln 3 (336 m2), span 0, spire 0. Real lights in a lava room ~13-19 on
  Forward+ (256 cap): the budget is not the cause. No light is created per frame (all made once; LightPools reuses 3 slots + a flash).
- **B4 HUD (G5, G16-G19):** 1 fontSize unit = 1.000 px at 1080p, 0.667 px at 720p (ScaleWithScreenSize 1920 x 1080, match height). Fight /
  teach line 1: 40 px shrinking to 28; Message slot banners 24; the counter 36 + "LEFT" 16; boss bar name 32, labels 16, hint 18; corner labels
  16-24. No TMP outline (a 2 px shadow copy). HUD SCALE does nothing to the live HUD; no text-size option. The health bar: 10 cells; no
  overheal anywhere. The reticle arc shows the KESTREL / THUNDERCLAP charge + FULL + cooldown, the THUNDERCLAP pump, the PULSE's 5 kill pips,
  the BLAST ready + cooldown, the swing cooldown, the STITCHER heat, any cooldown >= 0.6 s, plus the grapple charges row.
- **B5 enemies (G12-G14):** the warden's shield floats 0.13 m off the body, 1.9 m wide on a 1.5 m body, covers +-48 deg vs the sim's +-60,
  and cuts through the body when down; visors 0.036-0.29 m tall; the small robot is the swarmer (0.42 of a husk; iceline 29, span 14, spire 13).
- **B6 weapons (G20-G24, G43):** the LT blast has no visual of its own; the gauntlet's hits are cyan; the TORCH flame is warm orange
  (rule 8: enemy colours); the THUNDERCLAP zoom is x1.25 (74 -> ~62 deg) with the hip pose lowered 0.05 m; the "dotted line" is the grapple's
  5 pull beads; the live pad layout is v4: RB = the grapple (GG is right), keyboard G.
