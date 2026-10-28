# Bosses: readable state bars, phases, short in-engine intros (wave 43 research, for G13-G16 G23)

Web + GitHub research, 2026-10-16. Adds to wave 37 `boss-web.md` / `uk-bosses.md` (traversal bosses, ULTRAKILL boss
rules), wave 40 `boss-web.md` (phase conventions, arena zones, Sekiro posture, Furi), wave 38 `uk-hud-feel.md` (boss bar
fill-in and shake). Not repeated. "[code]" = read in a decompiled ULTRAKILL mirror on GitHub (behaviour and numbers only,
nothing copied); "[fetched]" / "[snippet]" / "[guide]" as in the other files.

## A. How the boss bar says what is happening (G14)

1. **ULTRAKILL's boss bar is a stack of health LAYERS (one per phase), each with its own colour and after-image colour.**
   `BossHealthBar.healthLayers[]`; the template builds one slider per layer, stacked by cumulative health, and moves the
   active slider down a layer when health crosses its minimum. [code]
   https://github.com/teker821/ULTRAKILL/blob/HEAD/BossHealthBarTemplate.cs (lines 48-79, 200-220), `BossHealthBar.cs`
   - for SKYGRIND: show phases as coloured layers of one bar (phase 1 orange over phase 2 deep red, say): the player
     sees how many phases remain and when one is about to break without any text.
2. **Damage after-image.** On a hit the bar drops at once; an after-image slider waits `waitForDamage = 0.15 s`, then
   drains toward the bar at `(|difference| x 5 + 0.5)` per second; the bar shakes by 5 x HP lost (capped, wave 38).
   Healing turns the fill green and fades back to its colour at 2 /s. [code, same file lines 200-246]
   - for SKYGRIND: "is my attack doing anything" (G14) is answered by the after-image size per hit: a pistol makes a
     sliver, a vulnerable-state gauntlet hit a big white chunk that lingers 0.15 s. Never hide the bar during a phase.
3. **Invulnerability recolours the bar.** While a boss is "Blessed" (protected by an Idol enemy, i.e. immune), every
   layer is drawn in `IdolProtectedColor = (0.25, 0.75, 1.0)` (light blue) instead of its own colour. [code, line 8,
   178-185] There is also an optional `secondaryBar` with its own colour and value for a second resource. [code]
   - for SKYGRIND: protected = the bar turns pale steel-blue with a lock glyph and a 2-4 word cause ("SHIELDED: PARRY
     IT"); vulnerable = the bar's frame flashes white-hot and a "STAGGERED" tag appears for the window's length. A
     secondary bar under it = the stagger / posture meter that parries fill (wave 40's Sekiro note).
4. **Hades: armour is a second, yellow bar over health**; armoured enemies are outlined in yellow and immune to stun and
   knockback until it breaks; in Hades II breaking armour "usually briefly stuns" and damage beyond the armour in one hit
   is ignored (no overflow). [snippet] https://hades.fandom.com/wiki/Gameplay_mechanics/Hades_II ,
   https://pcgamer.com/hades-guide-tips-and-tricks
   - for SKYGRIND: "no overflow" is the trick that makes the break moment readable: the big hit breaks the shield, the
     NEXT hits do health, and the break itself plays the stun + the strongest feedback tier.
5. **Doom Eternal Doom Hunter: a shielded sled; the Plasma Rifle or Blood Punch drops the shield; break the sled and phase
   2 has no shield.** [guide] https://www.thumbsticks.com/how-to-exploit-demon-weaknesses-doom-eternal/ ,
   https://selphie1999gaming.com/game-guides/doom-eternal-how-to-defeat-boss-doom-hunter-guide/
   - for SKYGRIND: one named counter per protected state (parry the orb, gauntlet the core, shotgun the emerging head),
     taught by the mini-boss first (G23), so the boss never asks for an answer the player has not seen.

## B. Phases and transitions (G16)

6. **Bosses are the peaks; the fights between are the valleys.** Hakita: "the boss fights are the peaks with important
   bosses like V2 and Gabriel being the peaks-among-peaks". [fetched] https://intothebluesky.com/2021/10/05/follow-up-interview-with-hakita/
7. **The fundamentals-first order (derived from wave 37/40 + the items above):**
   - Phase 1 uses ONE attack family the player has already parried in the level (the mini-boss's), plus one damage window.
   - Phase 2 adds one attack and changes the ARENA (a wall drops, the floor turns to lava: wave 40 lists Gladiator,
     Leviathan, Maggot Larry, Red Judge), not the boss's HP.
   - Phase 3 (optional) combines both at a faster cadence; the vulnerable window is shorter but the reward is bigger.
   - Every transition: the layer breaks on the bar, ~1 s of boss invulnerability with a roar / pose (the bar steel-blue for
     that second), the arena change happens in view, then a free hit opportunity (a "gift" window) to restart momentum.
8. **Repeat attempts must be cheap.** Players asked for a Pepper Grinder boss intro (~40 s, replayed each retry) to be
   skippable after the first viewing. [snippet] https://steamcommunity.com/app/2076580/discussions/0/4358995119366644009
   - for SKYGRIND: first meeting <= 6-8 s, every retry <= 1.5 s (name card only), checkpoint at the boss door.

## C. Cinematic but momentum-safe intros (G15)

9. **ULTRAKILL's Gabriel intro runs in the world, on the boss.** The boss plays an "Intro" animation; while "shaking" it
   adds a slowly rising camera shake (amount moves toward 0.125 at 0.0125 /s, camera shake = 2x amount) and jitters its
   own position; during "tracking" its root turns to face the player at up to 720 deg/s; animation events swap fake
   swords for the real ones (`SwordPull`) and toggle a rumble object. [code]
   https://github.com/teker821/ULTRAKILL/blob/HEAD/GabrielIntro.cs . Whether the player keeps control during it: not
   checked in this pass.
10. **The name card types in and holds 3 s** (LevelNamePopup: 0.01 s per letter, 0.5 s gap, 3 s hold; see ui-art-web
    item 3). Wave 38 found no intro flash in ULTRAKILL's code; boss entrances use animation + shake.
11. **Intro recipe for SKYGRIND (derived):** the door seals (one-way, G11) -> lights drop to 40 % for 0.3 s -> the boss's
    silhouette is backlit by its arena's key light (landmark) -> one in-engine camera move of <= 2 s that ends on the
    player's own view (or no camera takeover: the boss performs in front of the player) -> the boss locks eyes (head
    tracking) -> name card types in while the boss bar fills (ULTRAKILL fills at (target - fill) x 3 /s) -> first attack is
    a slow, parryable one 1.0-1.5 s after the bar is full. Sound: the named events only (GG's).

## D. Mini-bosses that teach the boss (G23)

12. **Foreshadow with the same tell.** Derived from items 5 and 7 and the encounters file (item 6, MASTER): each boss attack
    the player must parry or dodge appears first on a smaller enemy 1-2 rooms earlier, with the same colour, sound event
    and wind-up length; the boss then varies speed or count, never the tell. Two or three such meetings before the boss.

## Numbers table

| What | Value | Source |
|---|---|---|
| ULTRAKILL boss damage after-image | waits 0.15 s, drains at (|d| x 5 + 0.5) /s | BossHealthBarTemplate.cs (mirror) |
| ULTRAKILL protected-boss bar colour | RGB (0.25, 0.75, 1.0) | same file, line 8 |
| ULTRAKILL heal flash on bar | green, fades back at 2 /s | same file |
| ULTRAKILL name card | 0.01 s / letter, 0.5 s gap, 3 s hold | LevelNamePopup.cs (mirror) |
| ULTRAKILL Gabriel intro | shake rises to 0.125 (x2 to camera); head turn 720 deg/s | GabrielIntro.cs (mirror) |
| Hades armour | separate yellow bar; no overflow into health; stun on break (Hades II) | hades.fandom.com |
| Retry intro budget (players' ask) | skippable after first viewing | Pepper Grinder Steam thread |

## Not found
Doom Eternal's on-screen boss / demon intro presentation (title cards) from a primary source; ULTRAKILL boss-intro camera
ownership; any shipped FPS's on-screen text convention for "shoot the core" (wave 40 also did not find one).
