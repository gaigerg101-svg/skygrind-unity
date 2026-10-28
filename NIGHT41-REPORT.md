# NIGHT41 report for GG: the finish night

**Play:** `Builds/n41-r7/SKYGRIND.exe` (the freeze; port/stage3, tag u3-n41). `-skygrindOff41` gives NIGHT40's game for a side-by-side.
Your request (2026-10-14 ~08:40): "ok lets make a prompt related to things that were not done or still partial, expand upon that much more
and think of other things to imporve and make better". The list it became: Research/2026-10-14-wave41/INVENTORY.md (28 rows). Each switch
is `design.n41.<row>` in Design/unity-config.json. **13 of 18 rows are ON**; every ON row was seen on Player frames first and changed once
from what the frames showed (NIGHT41-LOG.md has every frame and number). Walk it with FEEL-PLAYTEST.md (NIGHT41's part, 15 minutes).

| row | what was missing | tonight | how it was checked |
|---|---|---|---|
| A1 | the shotgun's faster pump, never on a frame | **ON (pump41On).** The pump is mid-stroke 0.30 s after the shot and done by 0.45 s (it ran to 0.75 s). | a weapons reel with per-frame telemetry: frames at +0 / 0.15 / 0.30 / 0.45 s |
| A2 | the pistol's rhythm and the slug's look never judged on screen | **Judged, no change.** The pistol kicks and is back at rest by +0.15 s; the crosshair's turn is done by +0.30 s; the charged shot and the slug flood the screen cyan for one frame. | the same reel's frames |
| A3 | level 2 did not read as an ice canal | **ON (look41).** Every level's sun and room light now follow its theme: level 2 cold blue-white, level 3 molten amber, level 4 dusk violet, level 5 pale gold, level 1 as before. Level 2 also gets a frost crust on its wall bases and ice sheen. Boss rooms keep the warm sun so red-orange tells stay red-orange. | level 2's floor colour, the same frames before / after: blue / red 0.80 -> 1.39 (blue now above red), brightness within 1.2 %; level 3 0.58 -> 0.47 |
| A4 | THE LOOM's floor sweep | **ON (sweep41).** A new attack: a red-orange sheet lights over a band of the canyon for 2 s, then a line crosses it; answer it with height (ride a wall, hook, stand above it). In phase 2 it sits over the towers, so the hook is the answer. | both bot pilots 10/10 kills (51.6 s and 58.6 s); frames of the sheet in legs 1 and 2 |
| A5 | too many enemies winding up at once | **ON (tokens41).** At most two enemies are in an attack wind-up at a time in the campaign; the rest circle (survival unchanged). | time with more than two winding up: 3.1 % -> 0 % (level 2); every level's walk 10/10; your moves check unchanged on all five levels |
| A6 | the test suite never finished (low memory twice) | **SHORT.** Run in the open Editor (no second Unity, no memory stop): Core 1095 / 1095; Game A-L 527 / 528 (one failure from NIGHT40: a level-4 prompt is 7 words, the rule is 6); two big batches ran past the tool's 10-minute limit and P-Z were not reached. | NIGHT41-PROOF.md |
| A7 / A8 | the husk's silhouette; enemy parts vanishing at death | **Built, OFF (models41).** The husk drags one long arm with a red-orange claw (it lifts in the wind-up); every kind's parts break into flying blocks at death. | the lineup frame at 12 m is too small to judge the arm and no kill of a husk was framed: turn on `design.n41.models41.enabled` after a look |
| A9 | room banners over boss reels ("4.2 FIRST GAP OPEN") | **Fixed in the reels** (the real game never showed them). | no stray banner on tonight's boss frames |
| A10 | level 5's grapple moves failing (no node) | **ON (spire41).** Level 5 = n41-spire: six grapple rings in its two arena rooms (low enough that a pull ends with the room in view), and a DRONE pair in 5.6 THE CHOIR. | your moves check on level 5: 26/30 -> 30/30; bot walk 10/10 |
| A11 | the slide-bounce check failing on level 3 | **SHORT (a test limit, not the game).** The check's slide ran over a lava pit; fixed on level 1; on level 3 the only clean slide spot still ends in a ground jump in the Player while the harness passes. | moves check level 1 26/26, level 3 24/25 |
| A12 | THE GANTRY crossed by one endless ride | **ON (foundry41, your D-2 default).** Level 1 = n41-foundry: the long wall's last 4 m refuse a ride, so the chain of walls is needed again. Note: the band looks like the rest of the wall. | the crossing checker: "climb" +156 % -> "ride chain" +32 %; walk 10/10; moves 26/26 |
| A13 | level 6 out, to be rebuilt | **A plan for you (finale41 OFF, D-3).** Design/levels/n41/finale.beats.md (THE MIRROR: nine small rooms, each mixing two or three old moves, a climb route for enemies in every room), a walkable greybox (`-skygrindMap n41-finale`), THE RIVAL's one-page proposal (Research/2026-10-14-wave41/RIVAL.md, four questions for you). | levelgen 0 errors; the bot walks it 10/10 |
| A14 | the pistol's crosshair spin with a pad | **Checked:** the same turn as on the keyboard. | the weapons reel with the pad flag |
| B1 | the boss entrance shake / flicker and the damage static (OFF since NIGHT38) | **ON (juice41On).** | the shake and flicker on all five bosses' entrance frames; the "TV" static on the frames after a hit (grain, colour split, gone in ~0.3 s) |
| B2 | doors you can see (OFF since NIGHT38) | **ON (doors41On).** A locked doorway is a red-orange holographic sheet, an open one cyan, the boss door framed pale violet-white. | frames at 12 m and 3.5 m with the room's wave alive; the colour reads the game's real lock |
| B3 / B4 | stag38, iceline38, finale38 | **Retired** (THE STAG left the campaign; level 2 and 6 rebuilt). | |
| C1 | pad buttons in every new line | **Done.** Every NIGHT40 line shows pad buttons on a pad (22 lines on levels 2, 3, 5). Level 3's boost chain says "PRESS A" in words, not an icon (that slot has no icons). | a pad reel: device pad on every frame, 0 keyboard names |
| C2 | aim assist on a boss's weak point | **Built, OFF (aim41).** On a pad in ASSIST mode it pulls to THE LOOM's lit knee, THE ICEBREAKER's stuck fist, THE CROWN's ring, the CYAN core; the lock box frames the weak point. | the harness: hits per second THE LOOM 0.63 -> 3.17, THE ICEBREAKER 0.43 -> 1.89 (a weak pad player); no Player frame showed the lock on a weak point yet |
| C3 | boss bars that show phases | **ON (bars41).** One bar for all five: a tick at each phase threshold (THE LOOM and THE CROWN show three leg pips instead), the stagger meter under it with its label (STAGGER, or OPEN in cyan while a window counts down). | frames on all five fights |
| C4 | the boss entrance moment | **ON (entrance41).** The name card, the shake + flicker and the boss's rule in one 1.5 s beat, the same order for all five; the rule shows once. | entrance frames at 0 / 0.15 / 0.3 / 0.6 / 0.9 / 1.2 / 1.6 s for all five |
| C5 | the new enemies on levels 3 and 5 | **ON (kiln41, spire41).** A CHARGER wave in level 3's 3.2 POUR HALL; two DRONEs in level 5's 5.6 THE CHOIR. | level 3's walk 10/10, gates 4/4; level 5's walk 10/10 |
| C6 | THE ICEBREAKER read tan like the walls | **ON (brawlLook41).** Slate-blue ice plates on a dark core, red-orange fist shells through the tells, the stuck fist cyan, a shatter into blocks at death. | frames of the slam, the stuck fist and the kill |
| C7 | the gauntlet excelling against large enemies | **ON (heavy41).** On a tank, WARDEN, CHARGER or THE ICEBREAKER a gauntlet hit freezes 0.14 s and knocks harder. | the CHARGER held 0.19 -> 0.34 s; boss reels clean with it on |
| C8 | performance with the new views | **Measured (indicative).** Level 2 with 24 enemies: median 3.3 ms a frame (~300 fps), the slowest 1 % ~10.7-11.1 ms; the new enemy models cost nothing measurable. | 3 bench runs on the freeze build (not fully alone: the Editor was finishing a test batch) |
| C9 | level select previews | **Built, OFF (select41).** Each slot names its boss in red-orange; slot 6 says "REBUILDING". No silhouette art exists, so the name stands in. | the level select check passes with it on; no frame of it was looked at |
| C10 | the arc and the pad's reticle overlapping | **Built (behind aim41).** The lock box's lower corners keep clear of the arc. | from the screen layout, not a frame |

## Decisions for you (DECIDE41.md)
- D-8 (new): boss rooms keep the warm sun under look41 (red-orange tells stay red-orange); or the themed sun everywhere.
- D-9 (new): THE GANTRY's no-ride band looks like the wall: give it the no-ride look, or leave it unmarked.
- D-10 (new): THE WICK's and THE STAG's rule lines were written tonight from their damage rules ("RIDE A WALL, HOLD {fire}: SHOOT THROUGH
  A CRACK", "CATCH IT ON THE ICE: {punch} ITS LEGS"): your wording.
- D-3 (carried): level 6: react to the beat sheet, the greybox and THE RIVAL page, then the build.

## Not done
- aim41, models41, select41 are built and OFF (unseen on a frame tonight). A11 is a test limit on level 3. THE WICK's fight reel still
  does not kill it in a Player (as since NIGHT37; the harness does).
