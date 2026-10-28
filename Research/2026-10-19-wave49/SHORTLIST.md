# Wave 49 shortlist (S4: scored against GG's words, the code facts and the research)

Score: fit to GG's ask (0-3) + visible gain (0-3) + risk (3 = low) + cost (3 = fits 4 h). Each pick: a switch, a check, a PLAY note.

| # | pick | GG row | fit | gain | risk | cost | total | why (source) |
|---|---|---|---|---|---|---|---|---|
| 1 | **charge49**: the charge bar under the reticle back (arc40 / meter42 re-gated on a new row, redrawn readable: one thin arc, the weapon's colour, a full-charge flash, never over the centre box) | G1 | 3 | 2 | 3 | 3 | 11 | CODE-FACTS 1: the data is computed every frame; UiHud38.cs:339 is the gate. ULTRAKILL: the Railcannon's charge on the HUD + the gun (ULTRAKILL.md) |
| 2 | **look49 part A, the palette as data**: role tokens (environment dark / mid / light, level accent, danger, usable, pickup, UI) in UnityTuning.Look49.cs, with value / saturation bands | G4, G5 | 3 | 2 | 3 | 3 | 11 | UNIFY.md: tokens do most of the unifying; ULTRAKILL's single colour table (ULTRAKILL.md) |
| 3 | **look49 part B, the cyan budget**: fixture lights named cyan / white / green that are not on a usable surface take the level's accent or a dim neutral (LightKit29.cs:97-107 maps every non-orange name to usable cyan; 1604 cyan fixtures) | G4 | 3 | 3 | 3 | 3 | 12 | CODE-FACTS 3; CLAUDE.md rule 8 (cyan = usable) |
| 4 | **look49 part C, the colour script per level**: one dominant hue per level on a dark base: fog colour + density, trilight ambient, key light, the grade (ShadowsMidtonesHighlights, ColorAdjustments contrast), the stone vertex colours (pillarTop / pillarSide), via PostFx.ApplyTheme33 and UnityTuning.Look33 / Look41 per theme | G2, G3, G4 | 3 | 3 | 2 | 2 | 10 | CODE-FACTS 2-4 (the per-theme grade exists); UNIFY.md (the colour script; Neutral tonemapping keeps role hues) |
| 5 | **look49 part D, foes pop**: an emissive rim / back-light tier on enemies and bosses in the danger family; the emissive tiers tied to one bloom threshold | G5, G6 | 2 | 3 | 2 | 2 | 9 | UNIFY.md emissive tiers; DOOM.md (pending) readability |
| 6 | **fx49, one impact kit**: every hit, kill, parry, blast and flame built from the same three parts (flash, ring, sparks) with sizes and colours by tier (light / heavy / critical) and owner (player cyan-white, gauntlet pink, enemy red-orange, parry yellow), and one timing table in whole sim steps | G5, G6 | 3 | 3 | 2 | 2 | 10 | CODE-FACTS 5 (no shared kit); UNIFY.md (Riot: bigger, brighter, sharper with damage); ULTRAKILL.md (two hitstop tiers, the parry bundle) |
| 7 | moonshot **style49**: a style word on kills (ULTRAKILL's rank feel) in the HUD font, view only | G6 | 1 | 2 | 2 | 1 | 6 | ULTRAKILL.md style meter; not asked by name: behind a switch, leftovers only |
| 8 | dither / colour reduction as the base look | G6 | 1 | 2 | 1 | 1 | 5 | ULTRAKILL.md (the default dither); needs a full-screen shader: cut (DECIDE47 D-13, the G10 lesson) |

**After S7 (FORGE-NOTES.md findings 7, 15):** pick 5 (foes pop) is CUT (the enemy rim exists in Enemy.shader; GG owns enemy design,
rule 7; the drone's look lost three pairs) and pick 7 (style49) is CUT (style42 is already ON). Builder F becomes builder L: the level
lights in their roles (pick 3 plus the landmarks and the white fixtures). Final: H = 1; V = 2 + 4; L = 3 + landmarks; X = 6.

**Picks before S7:** 1, 2, 3, 4, 5, 6 (four builders' worth: H = 1; V = 2, 3, 4; X = 6; F = 5 + the foes); 7 in the leftovers; 8 cut with
its reason. The debt first: NIGHT48's freeze (T-89) and its rows-on decisions (T-96, T-97), because the n48 looks the polish builds on
(the warden plate, the LOOM hull, the lance, the cone) are all OFF.
