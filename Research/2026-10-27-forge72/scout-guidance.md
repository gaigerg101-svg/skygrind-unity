# Scout: in-the-moment player direction (2026-10-27, forge72)

Brief: GG said "way too much instruction at the start of the level", then "more text on the screen that explains like OK now you have to go up ... gamified and easy". Read: short, in-the-moment lines, not a front-loaded tutorial.
Method: web only. "Fetched" = I opened the page and read it. "Summary only" = the page blocked my fetch (403/402) or the claim comes from a search-result summary, so treat it as a lead. Lines marked INFERENCE are mine, not a source's.
Search-engine summaries were wrong about some things (e.g. a "blue = unparryable" rule), so I fetched each load-bearing page.

## Ranked: what helps a confused first-timer in a 30-second room

1. **Put the instruction on the thing, and make it change.** Fixed corner text is ignored.
   - Eye tracking (Naraka: Bladepoint): more players were stuck at "see" and "know" than at "do". Players missed guidance even when told to follow prompts. A fixed text prompt on the left of the screen "will be instinctively ignored"; a bright red range circle or a screen colour/brightness change draws attention. Text next to the thing it describes was viewed longer and remembered better. Fetched: https://www.gamedeveloper.com/design/why-do-players-turn-a-blind-eye-tutorial-design-and-selective-attention-
   - 64-player eye-tracking study of subtitled instructions in a game: subtitle skipping was about 73-87%, word skipping 84-92%. Words tied to immediate action got more fixations. Its advice: highlight key action words (directions, targets), keep text up long enough to re-read, make clear who the instruction is for. Fetched: https://pmc.ncbi.nlm.nih.gov/articles/PMC12452717/
   - Roblox's onboarding doc prefers visual highlights and arrows over long text (clearer, less likely to be misread). Fetched: https://create.roblox.com/docs/en-us/production/game-design/onboarding-techniques
   - INFERENCE: for "now go up", the line should be drawn at or pointing to the next ledge or bell, in the player's usable colour, and should appear (pulse or slide in) when the player enters the trigger, not sit in a fixed HUD slot.

2. **Hints that fire on stuck or failure, once, then retire on success.**
   - Roblox: timed hints for players who are struggling. Playtest how long most players take (their example: most press the button within 10 s, so a highlight at 11 s helps only the rest); A/B 5 s vs 10 s. Rules: show once, only after most players would have finished, only to those who need it. Fetched: https://create.roblox.com/docs/en-us/production/game-design/onboarding-techniques
   - Stuck timer: start it when the objective becomes active; reset it only on real progress, not on any input (flailing would hold it off forever). The page gives no second values. Fetched: https://bugnet.io/blog/how-to-fix-contextual-hint-that-never-fires-when-the-player-is-stuck
   - Valve's Game Instructor (Left 4 Dead 2, Portal 2 era): a lesson has a timeout in seconds, a display limit (how many times it may show), and a success limit (after N "learned" events it stops showing). Counts are saved and can be reset. The icon follows the player's rebinding. Summary only (Valve wiki returned 403): https://developer.valvesoftware.com/wiki/L4D2_Level_Design/Instructor_Hints and https://developer.valvesoftware.com/wiki/Env_instructor_hint
   - Nintendo Super Guide: after 8 failed tries at one stage a block offers a CPU run; it applies to that stage only. Summary only: https://giantbomb.com/wiki/Concepts/Super_Guide
   - Hades God Mode: starts at 20% less damage, +2% per death, capped at 80%. Kasavin: make the player a bit tougher, leave enemies alone so patterns can be learned. Fetched: https://inverse.com/gaming/hades-god-mode-interview
   - A patent describes hint strength (nudge to walkthrough) scaled by repeated failure at one obstacle, with a per-player frustration threshold. A patent, not a shipped-game source. Summary only: https://www.patentarcade.com/video-game-patent/US10441886/dynamic-allocation-of-contextual-assistance-during-game-play
   - Game Accessibility Guidelines: reminders of objectives and controls, and a way to practise without failing, are listed as intermediate items. Summary only: https://gameaccessibilityguidelines.com/?p=2854
   - INFERENCE for a 30 s room: first nudge at about 8-10 s without progress, a stronger one (arrow or highlight) at about 20 s, never a repeat after the verb succeeded once. After 2-3 deaths in the same spot, show the cause in one line (what killed or what was missed).

3. **Make the room carry the route; text only confirms.**
   - Neon White: red doors mark the main path, green ivy marks direction, low-detail look with negative space so the route reads at a glance; "the player always knowing where to go next"; no level needs platforming, shooting and card management at once. Esposito: players "don't seem to have a difficult time figuring out the abilities" from card art plus a tutorial image. Fetched: https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity
   - Ghostrunner 2: each setting reserves one colour for steering; yellow marks places to focus. Summary only (article body blocked): https://creativebloq.com/news/ghostrunner-2-level-design-secrets
   - Mirror's Edge: red objects show the route. Level designer Silli at GDC 2010: red is "an odd choice" because it reads as danger or stop; she once stopped at a red door; for a sequel she would lean on flashes less and make the route clear from the start. Fetched: https://www.engadget.com/2010-08-17-overheard-gdc-mirrors-edges-odd-go-here-color.html
   - Catalyst reworked it: a waypoint plus a path that recalculates "like a GPS". Players pick Off, Classic or Full View; when you leave the path it plots a route that keeps momentum rather than a reversal; routes come from playtest telemetry. Fetched: https://segmentnext.com/2015/08/22/mirrors-edge-catalyst-open-world-demanded-a-redesigned-runner-vision/ and https://www.ea.com/amp/news/runners-vision-in-mirrors-edge-catalyst
   - Dead Space: a floor locator line to the objective replaced a holographic 3D map that did not work for players. Fetched: https://www.gamedeveloper.com/design/game-ui-discoveries-what-players-want
   - Complaint to avoid: Doom Eternal's colour-coded "designated climbing areas" were called "the worst part" by one blogger (opinion). Fetched: https://www.siliconsasquatch.com/blog/2020/11/21/apparently-conscious-changes-to-doom-eternal-that-cost-time-and-money-many-of-which-were-a-mistake-that-appear-in-the-first-eighty-five-minutes-of-play
   - INFERENCE: SKYGRIND reserves red-orange for danger, so the Mirror's Edge red-reads-as-stop problem does not apply if the guide hue stays cyan (player-usable, per CLAUDE.md). Not checked in-game.

4. **One-line verb prompt, with the glyph, in a room that forces the verb.** ULTRAKILL 0-1 is the closest model:
   - Exact prompts: "Press 'F' to PUNCH." (a boarded door), "Hold 'Left Control' to SLIDE." (two jammed doors with a gap under each), "Press 'Left Shift' to DASH through danger." (collapsed walkway plus an electric field; the dash gives brief invincibility), a wall-jump line (broken ladder, three uses), "Press 'Left Control' in the air to GROUND SLAM." (a shaft of planks). Each prompt is 4-8 words, imperative, verb in capitals, and the room cannot be left without doing it. Fetched: https://ultrakill.wiki.gg/wiki/0-1
   - Titanfall 2 Gauntlet: a holographic ghost runs the obstacle first (wall run, slide), then you do it; the course ends in a timed run. Summary only (Prima returned 403): https://primagames.com/eguides/titanfall-2-eguide/single-player-campaign
   - Celeste: review pages in a search summary say the first screens teach jump, climb and dash with no extra explanation, and a collapsing bridge plus a bird show the dash. I did not fetch a source for it, so it is a lead only (no URL).
   - Complaint: ULTRAKILL's parry colour rule. A Steam player took "blue flash = cannot parry" to mean unflashed attacks were parryable and found otherwise; the wiki states yellow flash = parryable, blue = not, plus a sound cue. Teach one rule that never has exceptions. Fetched: https://steamcommunity.com/app/1229490/discussions/2/3734079155590153286 and https://ultrakill.wiki.gg/wiki/Parry

5. **Objective line with a counter.** Thin evidence; one number from outside games.
   - Doom Eternal shows a meter of combat encounters left in the stage and "secret encounters" (defeat N enemies in a time limit). Fetched: https://en.wikipedia.org/wiki/Doom_Eternal
   - Neon White levels have two goals: kill every demon, then reach the exit (reviews). Whether its HUD prints a count I could not confirm. Summary only: https://www.nintendoworldreport.com/review/60703
   - Endowed progress effect (Nunes and Dreze 2006, car-wash loyalty cards): a task shown as "10 steps, 2 done" was completed more often and faster than "8 steps, 0 done". Outside games, but it supports starting a counter above zero or showing the first step as already ticked. Fetched: https://ideas.repec.org/a/oup/jconrs/v32y2006i4p504-512.html
   - Not found: any source on check-mark animation or completion stings in shooters. Do not cite one.
   - INFERENCE: "CLIMB TO THE BELL" (verb + landmark) beats "OBJECTIVE 2/3"; use a counter only where the player can act on it (KILL 3/12). Update the line when it changes (a changing element draws gaze, source 1).

6. **Boss and fight text: signal state with the body, name the verb once.**
   - Doom Eternal glow: blue while a staggered enemy regains balance, orange when a Glory Kill is available. Fetched: https://www.gameshedge.com/doom-eternal-glory-kills-guide/
   - Doom Eternal text boxes: a pop-up tutorial before each new enemy. Reviewers: "completely ruins the sense of discovery"; mechanics "pause the game ... counter to the fast pace"; a grey box states the weak point and sometimes a full attack guide. Fetched: https://zatu.com/en-us/blogs/reviews/doom-eternal-review and https://www.vgchartz.com/game/222183/doom-eternal/reviews/442875/. A blogger also disliked tutorials that lock you in a featureless room and repeating pop-ups (same siliconsasquatch URL as above).
   - id on why Doom's UI is loud: bright elements are "essential for visibility during fast fights"; "signage ... big and bold" like driving fast; if information is subtle, "it's the game screwing me". Fetched: https://gameinformer.com/preview/2019/06/13/id-talks-mod-support-integrating-more-traversal-and-the-controversial-neon-ui
   - ULTRAKILL: yellow/blue flash plus sound for parry; a successful parry briefly freezes the game and flashes white. Fetched: https://ultrakill.wiki.gg/wiki/Parry
   - Sekiro: one red kanji over the player before a perilous attack; it is the same symbol for every type, so the animation tells you which response. Players warn kanji-only timing fails. Summary only: https://sekiroshadowsdietwice.wiki.fextralife.com/Combat
   - God of War (2018): an R3 prompt appears over dazed enemies (official PlayStation guide). Whether it is drawn in world space is not confirmed. Summary only: https://www.playstation.com/en-sg/editorial/the-playstation-guide-to-god-of-war/
   - Not found: primary sources for Furi, Hollow Knight or Cuphead on "weak point is open" cues. Cuphead (GMTK transcript, summary only) says not every attack can be reacted to on first sight: https://amara.org/subtitles/TbmQ6Ac3Z6pb/en/1/download/How%20Cupheads%20Bosses%20Try%20to%20Kill%20You%20%20Game%20Makers%20Toolkit.en.sbv
   - INFERENCE: "go up" mid-fight = a pulse on the target ledge plus a 2-3 word line the first time the state starts; the "open" state needs a body colour or shape change, not a sentence.

7. **Pad-first prompt rules (numbers).**
   - Xbox XAG 101 minimum default body height (ascender to descender): console 26 px at 1080p, 52 px at 4K; PC/VR 18 px at 1080p, 36 px at 4K. Text inside a glyph (the "X" in a button) must meet the same minimum; glyphs scale with text up to 200%; players can scale text to 200%. Line width at most 80 characters, line spacing 1.5. Fetched: https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/101
   - FLAG: CLAUDE.md sets UI text at 16 px at 1080p, below Xbox's PC 18 px and console 26 px. Body height is not the same unit as a font-size, so measure the built prompt before deciding. Not checked.
   - Safe area: Xbox title-safe is 1720 x 1000 at 1080p (inner 90%), action-safe 1792 x 1016; the outer 32 px sides / 8 px top and bottom may hold system UI. Fetched: https://devdocs.xbox.com/build/core-features/graphics/overviews/screen-areas
   - Steam Deck Verified: smallest on-screen font at least 9 px tall at 1280x800; glyphs must match the active input (Deck, Steam Controller, Xbox), no keyboard glyphs on pad. Fetched: https://partner.steamgames.com/doc/steamdeck/compat
   - IGDA: test at your normal distance doubled; subtitles no more than 38 characters a line. Fetched: https://igda-gasig.org/how/game-accessibility-top-ten-se/
   - Xbox's own example shows prompt text with a black outline over the world (Fenyx: glyph plus "Collect"), regardless of background. Fetched: same XAG 101 URL.
   - Valve's icon follows the player's rebinding. Summary only: https://developer.valvesoftware.com/wiki/L4D2_Level_Design/Instructor_Hints

## Text length and time on screen (best published numbers; none is a shooter-specific rule)
- Netflix: 2 lines maximum, each subtitle event 5/6 s minimum and 7 s maximum. Fetched: https://partnerhelp.netflixstudios.com/hc/en-us/articles/215758617-Timed-Text-Style-Guide-General-Requirements. 42 characters per line is from the language guides (summary only): https://backlothelp.netflix.com/hc/en-us/articles/217349997-Castilian-Latin-American-Spanish-Timed-Text-Style-Guide
- Study above: 12 characters per second, up to 63 characters per line, new instruction every 5 s. Fetched: https://pmc.ncbi.nlm.nih.gov/articles/PMC12452717/
- INFERENCE, starting values to test (not published rules): one line, at most 6 words or 40 characters; one prompt on screen at a time; 40 characters is about 3.5 s at 12 cps, so 4 s for a pop-up, but a verb prompt stays until the verb succeeds.
- Not found: a GDC or Game Developer rule with a words-per-line or prompts-at-once number for shooters. The Roblox doc says it gives no word limit.

## What players praised and complained about (short)
- Praised: rooms that force the verb (ULTRAKILL 0-1, Gauntlet, Celeste); one reserved colour for the route (Neon White, Ghostrunner); loud, readable signage in fast play (id); optional guidance with a dial (Catalyst's Off/Classic/Full).
- Complained: pop-ups that pause fast play and take away discovery (Doom Eternal); text that states the answer; guidance that points through walls or has no use indoors (Mirror's Edge; a user review says rooms that are all red make it useless, summary only: https://giantbomb.com/user-reviews/id/162125/mirrors-edge-is-a-flawed-gem); a rule with exceptions (ULTRAKILL parry colour); fixed corner prompts nobody looks at.

## Gaps
- Hades (first game) and Celeste's Assist Mode as hint systems: nothing primary found. Celeste's Assist Mode is a difficulty option, not a hint (Thorson on why: https://wireframe.raspberrypi.com/articles/the-new-normal-the-indie-developers-changing-our-conceptions-of-difficulty, summary only).
- Doom Eternal objective markers and automap: no useful source beyond the Doom Wiki's automap note (https://doomwiki.org/wiki/Automap_(Doom_2016)); not read in full.
- GDC Vault and Valve wiki pages could not be fetched; the Mushroom 11 tutorial talk is a lead only: https://gdcvault.com/play/1024187/Teaching-by-Design-Tips-for
