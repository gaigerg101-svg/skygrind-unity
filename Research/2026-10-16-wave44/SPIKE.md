# NIGHT44 forge spike (2026-10-16 ~20:15): the five premises, read from code, logs and frames (nothing run)

1. **T-59 THE WICK's Player fight reel (wick37 fight).** B1's pilot fix is in the Player build (Game/SurvivalSim.Wick37Pilot.cs,
   wallLean 1.0; FeelReel.N37.Wick.cs:116). What differs is the loop: the harness (Tools/SimHarness/Harness.N43.Wick.cs:194-198) runs
   pilot.Step -> si.Set -> sim.Step every sim step; the Player reel (FeelReel.N37.Wick.cs:134-151) runs pilot.Step `ran` times after the
   frame against one post-frame position, ORs the presses into g.forcedMove, which ReaderInput.cs:230 applies to every sim step of the next
   frame (L-053), with no Edge35Input: a held press on the second step is a double jump, the held attach lost: "panels ridden []".
   Fix: an IPlayerInput adapter calling pilot.Step once per sim step, passed to sim.UseInput (as FeelReel.N43.Foundry.cs:37 does with
   Walk38Pilot); at least wrap in Edge35Input.
2. **T-66 move43.** The input path is right (Edge35Input, FeelReel.N43.Move.cs:94); the setup differs from the harness: the real b2 chimney
   gap 4.50 m from 1.5 m up (:125) vs the harness's box 4.0 m from 2 m (Harness.N43.Move.cs:736-776, :182); a pressed wall jump flings
   ~5 m/s out and falls across 4.5 m (M1.md:26-29). Fix: in Move43Chimney (:61-64) prefer a 3.5-4.0 m gap and start at floor + 3 m (THE
   FLUE itself is 3.5 m now), or decide the flip per sim step through an adapter.
3. **T-69 the stale card.** teach43's card reads text36's MessageSlot33 (UiHud33.cs:769 -> UiHud36Text.cs:85-90); MessageSlot33.cs:90
   keeps a line for MinHold 1.2 s (UnityTuning.Hud33.cs:96); nothing clears it on roomEnter (only OnNewRun, UiHud36Text.cs:75). The reel
   teleported to b2 inside a1's 1.2 s hold. A real player almost never sees it (a room change within 1.2 s of a line; fallReturn43 respawns
   in the same room). Fix: a drop-priority call on MessageSlot33 on roomEnter beside DropLesson25Now; close the teach43 card with it.
4. **T-68 the burrower counter on a frame.** The kiln43 reel's ash-bed part (FeelReel.N43.Kiln.cs:152-199) never fires; the rule counts a
   hit in Emerge with windowSteps > 0 (Core/Enemies/Burrower43.cs:189-200); A1's burrow43 reel countered 1. Fix: on BurrowEmerge, aim at the
   body and press fire inside windowSeconds (0.5 s).
5. **T-65 THE RING.** The worm's paths are data: Design/bosses/wyrm26.json (coil, dive, lunge, loop, finale; 10 variants, 372 waypoints, +
   exits / holes / emerge; attack marks are waypoint indices); Wyrm.cs:542-551 builds the Catmull-Rom path; WyrmArena.cs (48 lines) only
   loads boxes. The arena is chosen per level (SurvivalSim.Kiln39.cs:21-29) but the paths come from one key design.n26.bosses.wyrm26.paths
   (SurvivalSim.Wyrm24.cs:80, :122). To move them: ~30-40 lines in 2 files (a per-level paths override), one new Design/bosses/wyrm44.json
   with the waypoints shifted, the arena generator's 2.9 m clearance check against the new routes, re-run `n43kiln wyrm`: about half a card.
6. **Other rough edges in the reel lines:** the foundry43 line says "rides 0" though L43-gantry-mid (which needs a ride) saved: the pilot's
   ride count undercounts; L43-gantry-land never saved ("dashes 10": the flight38 DashFail loop, T-71, fixed by the respawn clear after r4?
   not re-run); move43 7 / 14 frames; wick37 only 2 fight frames; 39 / 29 / 20 failed frame writes in wick / brawl / v4.
