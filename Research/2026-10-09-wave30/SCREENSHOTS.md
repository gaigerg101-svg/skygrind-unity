# The nine screenshots, read critically (the forge, 2026-10-09 ~19:15; GG's Editor Game view at 2560x1440)

Every line is what the frame shows; a cause is marked "suspect" until the run reads the code.

| Shot | Where | What is wrong (or right) |
| --- | --- | --- |
| 165911 | foundry f2 "JUMP THE GAP" | The gap is a black, bottomless-looking trench with a yellow strip in it: it reads as a FALL (death), yet a fall respawns the player and enemies fall in and vanish. GG: replace gaps with lava. The cyan edge trims and the ceiling panel work; the floor is one flat dark plane with nothing on it. |
| 170047 | foundry, a fight room | Bright yellow-white floor panels (a pool light and a strip) are the brightest thing in the frame and mean nothing to the player; the room is a dark box with one raised frame: no cover, no landmark, no reason to wall ride. HUD: "PULSE READY" text is drawn ON TOP of the weapon icons and overlaps "TRIGGER STALE". |
| 170216 | the Lantern arena (rest phase) | A square beige box with a grid; the boss is a small white cube with red diamonds; sentry blocks on the walls; the orange ring on the floor is the only arena feature. Nothing tells the player the rule (ride the wall, shoot from it). |
| 170227 | the Lantern arena (floor live) | The WHOLE frame is washed red-orange (the floor-live tint covers walls, sky, HUD backing): it is loud but unreadable: the player cannot tell the safe surfaces from the lava. The "x1.2 AIR" chip floats above the rank plate. Health 21: the floor is punishing. |
| 170919 | iceline i4 "HOLD FORWARD. DASH SIDE TO SWITCH." | A narrow tunnel with an orange block; cyan floor slabs that glare white; it reads as a corridor, not a skate lane. Not readable as ice. |
| 170932 | iceline i5 "DASH SIDE TO SWITCH LANES" | Four orange capsule husks in a row across a dark corridor: it is a shooting gallery, not a skating room. A green "1/2" badge sits beside the score with no meaning to a new player. |
| 190231 | THE GLACIER fight | The dark rink and cyan lines are better than NIGHT28; but the lesson text "TANKS FIRST. PUNCH THEM." from room i6 is STILL on screen in the boss fight (stale lesson text: a bug). The gap bar sits in the middle of the sky; the boss is a white block on a cyan pole. 32 console errors in the status bar. |
| 190310 | Leaving the Glacier (pause / title?) | A dark diagonal panel with "PLAY" covers half the frame while the gap bar, "DOORS OPEN" and the stale lesson text stay drawn over the menu: HUD rows that belong to the run leak into the menu (a bug). The sky behind is the best-looking frame of the set. |
| 190414 | kiln k4 flue "RIDE UP. SHOTGUN CLOSE." | The KILN WYRM boss bar is drawn in a normal room (DOORFREE took him there early? suspect: the boss bar shows before its room). 64 console errors: all `NullReferenceException` in LitMotion's `TextMeshProMotionAnimator.UpdateCore` called from `HudFx20Install.Tick` (Assets/Scripts/HudFx20/HudFx20Install.cs:52): a text reveal keeps animating a TMP text that no longer exists (suspect: the static `reveals` dictionary survives a scene reload). |

## The pattern across the set
1. **The rooms are boxes with lights, not places.** No room has a hero object, a height tier players want to reach, cover, or a reason to wall ride; props are few and flat. The light is the strongest element and it is spent on meaningless floor panels.
2. **The rules are invisible.** The Lantern's rule, the gauntlet's state, the skate lane, the "1/2" badge, "TRIGGER STALE": the HUD names states without explaining them, and the one explanatory line (the lesson text) goes stale.
3. **The HUD has layout bugs.** Overlapping text (PULSE READY over the icons), chips floating over plates, run rows leaking into menus, a boss bar out of its room, 64 NREs from a text effect.
4. **Danger is shown as a full-screen wash** (the Lantern) instead of on the dangerous surface, and pits read as death while acting as a respawn.
5. **What is right:** the dusk sky, the cyan trim language, the dark Glacier rink with lane lines, the closed ceilings, the CRT grain at 35 %.
