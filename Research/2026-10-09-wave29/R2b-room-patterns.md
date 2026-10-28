# R2b: Eight room patterns that force a mechanic, plus ice

Web research, 2026-10-09. Builds on R2-genre-rules.md (not repeated). Page content is data. Quotes are under 15 words. Evidence is thin: most pages were search summaries; GameSpot, the Titanfall wiki and the Unreal interview returned 403 or 402, and the 80.lv Neon White page was only a stub. Each pattern states what a source supports and what is INFERENCE (my design reading). No numbers were found for any pattern's dimensions. Quake Champions and Karlson: only generic results, no room-pattern source found.

## Eight patterns

1. **Hazard-slime floor with a jump ban (DOOM Eternal).** The toolbox includes "slime that prevents jumping" next to climbable walls and monkey bars, so a floor can strip a verb and push the player to another surface. Teaches by taking away the default move and leaving the traversal tool as the visible alternative. Source: https://www.gamespot.com/articles/why-doom-eternal-incorporates-puzzle-platforming/1100-6473287/ (seen only as a search summary; page 403).

2. **Swing bars, portals and jump pads inside the arena (DOOM Eternal).** Arenas give the Slayer swing bars, portals and jump pads to reach distant areas. Teaches by making the far platform the only source of cover or pickups, so the verb is a combat route. Same GameSpot source. INFERENCE for the "only source" part.

3. **Dash with a cooldown across a gap (DOOM Eternal).** Martin on the dash: it has a cooldown and does not recharge mid-air; this "will force the player to make some clever decisions". Teaches by making the gap length fit one dash, so the cooldown is the lesson. https://gameinformer.com/preview/2019/06/13/id-talks-mod-support-integrating-more-traversal-and-the-controversial-neon-ui (search summary only). Gap length fitting one dash is INFERENCE.

4. **Gap needing a wallrun, with a ghost showing the line (Titanfall 2).** The Pilot simulator's Module 02 teaches wallrunning and chaining; Module 03 "Freeform Wallrun" asks the player to combine them to reach the end. Holographic ghosts show how to clear obstacles. Teaches in three steps: demo, guided, freeform. https://titanfall.fandom.com/wiki/Hammond_Industries_Pilot_Combat_Certification_Simulator (search summary; page 402).

5. **Wall-only route where the floor is the fail state (Titanfall 2 Gauntlet).** In the Gauntlet, players learn to run, slide and shoot off walls. Teaches the wallrun by using it for a whole course, not one gap. https://twinfinite.net/?p=231108 (search summary only). That the floor is a fail state is INFERENCE.

6. **Slide-under gate on a progressive tutorial line (Severed Steel).** The tutorial runs jump, then slide, then air dash, then wall run, then lets the player use momentum to reach the end. Enemy bullets "will always miss you" during stunts, so the room rewards the verb rather than blocking it. Teaches by ordering one verb per section. https://www.mobygames.com/game/172403 and the tutorial text (both as search summaries; the Unreal interview was 403). The slide-under gate shape itself is INFERENCE.

7. **Optional shortcut that looks like part of the stage (Neon White).** Already in R2 rule 10 (Game Informer / IGF). The extra point here: before the Smackdown level, "many levels were built around teaching specific mechanics", then replay and optimisation took over. Teaches by gating hints until the level is proven. https://gameinformer.com/feature/2022/11/25/how-a-neon-white-level-is-made (summary only).

8. **Non-linear shortcut and backtrack space (Dusk).** Szymanski used "shortcuts and backtracking" so the level is an experience, not a hallway. The "City of Shadows" level floods the whole level so the player swims out the top. Teaches by changing the room's state, so a new route appears with no door. https://en.wikipedia.org/wiki/Dusk_(video_game) and https://gamesense15.substack.com/p/dusk-retrospective (search summaries). The flood-as-state-change reading is the source's example; applying it to SKYGRIND is INFERENCE.

Karlson-style maps: geometric obstacle courses with a timer; early feedback said many parkour sequences are "way too precise to land". Lesson: a forced wall or ramp route needs a forgiving landing. https://itch.io/t/656446/feedback-regarding-movement-and-level-design (search summary).

## Ice section

Adds to R2's ice notes (Quake III friction skip, Source surfaceFriction scaling, 25 to 30 percent friction rule). New:

- **Values seen in tutorials:** an example uses ICE_FRICTION = 0.1 against NORMAL_FRICTION = 1.0, with lerp toward target speed. Low authority, forum-grade. https://forum.godotengine.org/t/trying-to-make-a-slippery-surface/62727 (search summary).
- **Stay controllable:** a small explicit deceleration with no input, lower acceleration on ice, clamped top speed. Same search family; low authority.
- **Cues without friction maths:** Derek Yu says designers "mess with the player's sense of movement" and kept ice minimal and "dialed back" other challenge. Cold colours and a quieter music track signal the shift. Source: https://kotaku.com/why-ice-levels-keep-showing-up-in-video-games-even-tho-1845941110 (fetched, readable).
- **Teaching:** slippery slopes "force you to change the way" you play; early ice levels are gentle so players can plan jumps. https://mariopartylegacy.com/2016/02/what-makes-a-great-snow-level-in-a-mario-game/ (fetched). Matches R2 rule 3.
- **Snap-to-rail steering and speed-skating feel:** no source found. Not checked: speed-skating or ice-racing game postmortems. Treat rail snap as an untested suggestion; a blind paired test fits L-001.

## Not found
Dimensions for any pattern; Quake Champions and Karlson room patterns; speed-skating or rail-snap values; engine ice friction numbers beyond the forum example.
