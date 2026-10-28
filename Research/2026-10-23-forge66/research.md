# Making the verb the best answer: research for FORGE66 (2026-10-23)

Scout: one agent, web reading plus the repo. The problem (NIGHT65-REPORT.md section 1): "walking and shooting still clears half
the rooms as well as using their verb" (skip test RED in a2 19.7 vs 18.9 s, a3 33.7 vs 24.2 s, a8 23.0 vs 14.5 s); GG: "The rooms
still don't incentivize wall-riding or creative use of the weapons", "more elements that are not just decoration"; a dash-jump
crosses any gap a room can hold (DECIDE65 D-5). Previous research: Research/2026-10-23-forge65/research-rooms.md (15 room rules).

Legend: **[read]** I opened the page. **[s]** search-result summary only (lower trust). **derived** my arithmetic on
PortExport/config.json. **proposal** my synthesis, no source. **repo** read in this repo. Drive search was attempted for
SKYGRIND material (the Drive session had expired); Gmail, Calendar and Linear were not searched (a game project, not INVERSA work).

## 0. The core idea, from three sources
- "Given the opportunity, players will optimize the fun out of a game"; "a single dominant strategy actually takes away choice
  from a game" (Soren Johnson) [read] https://designer-notes.com/game-developer-column-17-water-finds-a-crack . The walk bot is
  that optimizer. A room's job is to make the verb the optimum, not to hope a player chooses fun over speed.
- Hakita: "The main challenge is creating enemies that actually pose a threat to the player" [read]
  https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/ . The lever is mostly
  enemies and economy; geometry alone cannot do it (D-5's finding agrees).
- id (DOOM Eternal): "the solution to every problem is to be aggressive, and there's no way around that" (Hugo Martin); "As long
  as there are demons on the battlefield, you've got the resources you need" (Marty Stratton) [read]
  https://gamedeveloper.com/design/the-aggressive-resource-management-of-i-doom-eternal-i- . DOOM forces a style by its economy,
  not by walls. GDC 2018, "Embracing Push Forward Combat in DOOM" (Loudy, Campbell): mechanics are judged by a filter; reload was
  cut because it left the player unable to attack, the double jump kept because it improves mobility [s]
  https://gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in ; summary [read]
  https://www.gamedeveloper.com/design/video-the-combat-design-of-i-doom-i- .

## 1. Fourteen checkable levers (each: source, SKYGRIND hook, sim measure)
Sim measures use the night's instruments (NIGHT65-REPORT.md section 3): the verb pilots `n65verb launch|pierce|parry|skip`, the
verb-aware autopilot `n46flow --verbs65`, air65, and the walk bot as the "refused" baseline. "Skip margin" = time with the verb
refused / time with it, on the same seeds, refusal proven fired (T's 4:58 fix).

1. **The verb refills what the alternative spends.** Neon White: "Players can sacrifice weapons (soul cards) for parkour
   abilities" [read] https://www.gamedeveloper.com/road-to-igf-2023/-neon-white-and-designing-for-player-creativity ; DOOM:
   fodder chainsawed for ammo, the first fuel pip regenerates on its own [s] https://doom.fandom.com/wiki/Chainsaw/Doom_Eternal ;
   ULTRAKILL: a parry heals 999 (full, up to Hard Damage), a punch 10-30, health comes from "attacking an enemy at close range"
   [read] https://ultrakill.wiki.gg/wiki/Health . SKYGRIND hook (repo): a dash-jump costs 2 of 3 pips (dash.pipCost 1 +
   dashJump.extraPipCost 1, MovementConfig.cs:249), regen 0.7 pip/s, and `mover.regenMult` already scales regen by style rank
   (SurvivalSim.Run.cs:433). Proposal: a ride (or a launch kill) refunds a pip, so the ride is free and the dash-jump costs the
   dash you need for the next threat. Not health: GG retired passive in-round regen (CORE.md). A new number goes in a Unity-side
   config and touches Skygrind.Core: a default-off row, goldens 0 mismatches (CLAUDE.md rules 3, 5). Measure: pips on landing
   after each crossing method; damage taken in the 5 s after landing, verb vs refused.
2. **A threat that hits a standing or grounded player.** ULTRAKILL's Virtue: the insignia "follows and surrounds the player for
   about 3 seconds, before stopping at their current location"; "constant horizontal movement is encouraged" [read]
   https://ultrakill.wiki.gg/wiki/Virtue . The Insurrectionist lands with a "Horizontal" shockwave, speed 35 units/s [read]
   https://ultrakill.wiki.gg/wiki/Insurrectionist ; Cerberus's stomp is a ground shockwave [s]
   https://ultrakill.fandom.com/wiki/Cerberus . SKYGRIND hook (repo): spitter puddles (puddleRadius 3 m, puddleTime 4 s, 10 dps)
   and slam's 5 m shockwave exist. Proposal: a room's lane floor is where puddles or a floor wave land, so the walk line costs HP.
   Measure: damage taken while grounded vs airborne per room; the walk bot's deaths and HP lost vs the ride pilot's.
3. **An enemy efficiently killed by one verb.** DOOM Eternal weak points: a grenade into the Cacodemon's mouth, the Arachnotron's
   turret, the Carcass shield overloads under the Plasma Rifle [read, a third-party guide]
   https://www.thumbsticks.com/how-to-exploit-demon-weaknesses-doom-eternal/ ; weak spots "take extra damage and disable enemy
   attacks" [s] https://doomwiki.org/wiki/Destructible_demons . Hook (repo): charger.weakMult 3 (what it multiplies: not checked);
   points.punchKill 130 vs kill 60. Measure: time to kill the room's key enemy with vs without its verb; aim 2x or more.
4. **A window only the verb opens.** id's Marauder: "his eyes briefly flash green" before the close attack; "Attack too soon and
   the Marauder will deflect it with his shield" [read] https://slayersclub.bethesda.net/en-AU/article/doom-eternal-boss-tips .
   Hook: parry (a4) and style.base.returnToSender 60. Proposal: a shielded spitter that takes damage only from its returned glob.
   Measure: share of a4's kills by parry; a4's clear time with parry refused (today's GREEN was never proven refused).
5. **The score counts the verb, not only time.** ULTRAKILL: each hit drops that weapon's freshness by 0.7 (FRESH 1.5x, USED 1.0x,
   STALE 0.5x, DULL 0x); Airshot +50, Air Slam +160; a movement multiplier "rises at 2.0x per second up to 3x" while sliding or
   airborne [read] https://ultrakill.wiki.gg/wiki/Style ; Hard Damage drains faster at high rank and is zeroed at ULTRAKILL rank
   [read, Health page]. Bulletstorm: "A headshot is a much faster kill than leashing an enemy toward you, then kicking him into a
   cactus", and playtesters still chose the creative kill because skillshots paid [read]
   https://gamedeveloper.com/audio/classic-postmortem-people-can-fly-s-i-bulletstorm-i- . Hook (repo): style.bonus wallkill 20,
   airshot 12, impact 50, trapped 30; points.rankMult up to 2. Measure: points per minute and STYLE rank, verb pilot vs walk bot;
   a room "rank" = time and style together, so a tie on time is a loss on rank.
6. **Anti-camping that catches the walk line, not only standing still.** Hook (repo): the director's camp rule is radius 10 m,
   lingerTime 8 s, exposedAt 0.5; EXPOSED halves points (points.campMult 0.5) and pauses regen (SurvivalSim.cs:190-191). A walker
   in a 30 m lane leaves a 10 m circle, so the walk-and-shoot line is probably never EXPOSED (proposal; Director.cs:160's
   "lingering" not read in full). Halo pushes back on the player who charges: "it waits to see what you will do, how hard you will
   push, and then it pushes back at just the right resistance" (Griesemer) [read]
   https://www.engadget.com/2011-07-14-half-minute-halo-an-interview-with-jaime-griesemer.html . Measure first: the walk bot's
   EXPOSED-time share per room (expect about 0 in lane rooms: then this lever does not bite yet). Proposal: a grounded-time share.
7. **Movement is defence.** Severed Steel: "While Steel is doing a stunt, all incoming enemy fire misses" (Larrabee) [read]
   https://blog.playstation.com/?p=347121 ; ULTRAKILL's Virtue page recommends sliding, which can outrun the beam [read].
   Proposal: ranged husks lead a grounded target and not a riding one (a new rule: GG's call). Measure: hits taken per second
   riding vs walking under the same spitter volley.
8. **The height that matters is reached fast only by the verb.** "It is easier to shoot down, than up" (Pears, forge65 rule 2)
   [read in forge65]; DOOM Eternal's early arenas pair monkey bars with double jumps [s] https://blog.playstation.com/?p=220711 ;
   Titanfall War Games: wallrun paths placed "to connect important areas together" [s]
   https://primagames.com/eguides/titanfall-eguide/maps/war-games/map-overview . Measure: time to reach the vantage by the verb
   vs by the walk route; count of wave-2 spawns in line of sight from the vantage vs from the floor.
9. **The next wave arrives on the verb's landing.** Neon White's rule 2: "The player always has the right tool for the next
   encounter" [read, Game Developer above]; the LDB: "Begin the encounter with no enemies, so the player will wander into the
   middle of the arena" [read] https://book.leveldesignbook.com/process/combat/encounter . NIGHT65's own second lesson: the targets
   in the verb's reach at its moment (report section 7). Measure: distance from the ride's exit to wave 2's centroid; enemies
   inside punch / slam reach (2 m / 5 m) within 1 s of landing.
10. **Hazards are weapons the verb loads.** Bulletstorm: kick an enemy into a cactus ("Pricked"); cacti in almost every outdoor
   section [s] https://gamesradar.com/bulletstorm-skillshot-guide/2 ; "Kick to push an enemy away. Kick to destroy an enemy's
   armor" [read, postmortem]. Hook (repo): traps.kill 99999, style.bonus.trapped 30, a5's POUR (no launch has filled it: report
   section 4). Measure: hazard kills per room (launch36 count); the share of a wave killed by the room's hazard.
11. **Refusing costs time, not a wall.** Neon White medals: Gold is the intended path with no mistakes, Ace uses the designed
   shortcut, which "saved a significant amount of time even if you didn't execute it perfectly"; the developers' ghosts got
   "small errors throughout the run to simulate hesitations" [read]
   https://vice.com/en/article/neon-whites-developers-being-too-good-at-their-own-game-caused-real-problems . Measure: the skip
   margin with a jittered pilot (late inputs, a missed first ride); proposal bar: the verb still wins by 20 % or more.
12. **Ranged enemies make the pieces matter.** Mike Stout via the LDB: "When you have Near Enemies, since they can't attack you
   from far away, your environment doesn't matter at all"; with ranged ones "your level design is going to start to matter" [read]
   https://book.leveldesignbook.com/process/combat/enemy . A husk-only room makes its pieces decoration. Measure: the share of
   damage from ranged enemies per room; pieces that break line of sight to a ranged enemy on the walk line.
13. **The same 30 seconds in a new context.** Griesemer: the cut half of the quote is "taking that 30 seconds of fun and playing
   it in different environments, with different weapons ... against different combinations of enemies"; "No 30 second stretch of
   Halo is ever repeated" [read, Engadget above]. Measure: each room names one verb and one enemy mix no other room on the level
   uses (a card line, checked by a script).
14. **Weapon freshness forces the swap.** ULTRAKILL freshness (lever 5) [read]; GG's "creative use of the weapons". Hook (repo):
   style.diminish window 4, repeat 0.6, floor 0.15; style.bonus.swap 10. Measure: per room, the share of kills by each weapon and
   verb (an entropy or the top weapon's share); the walk bot's top-weapon share is the red.

## 2. Hand-offs and flow: chaining air pieces
- **A hand-off must not cost speed.** Respawn after Titanfall 2's tech test: "Players should once again accumulate and retain more
  speed when chaining wallruns"; they had found "unintended side effects that were causing players to lose momentum while
  wallrunning" [read] https://press-start.com.au/news/playstation/2016/08/25/titanfall-2-getting-serious-changes-due-negative-feedback/ .
  Testers had said the beta maps gave too few chances to wall run; some final maps are "very heavily focused in chaining wall runs
  together" (Dusty Welch) [read] https://gamingbolt.com/titanfall-2-will-have-maps-that-will-heavily-focus-on-chaining-wall-runs .
- **Mark the surfaces and test with heatmaps.** War Games: "those colored areas encourage wall running"; "much more long-wall-heavy"
  [read] https://www.slashgear.com/titanfall-dlc-map-detailed-war-games-for-parkour-24326398/ ; heatmaps of where players
  wallrun, arrows, "obvious paths to help encourage players that haven't embraced this new way of traversing maps" [s] (Prima above).
- **The next piece is in view when you need it.** "Having to stop mid-run to look around destroys the level's pacing and flow";
  "Don't place it behind them and outside player's view" [read]
  https://worldofleveldesign.com/categories/level_design_tutorials/one-mistake-in-mirrors-edge.php ; DICE: momentum is "like the
  fuel that drives you through the game" [read] https://www.gamedeveloper.com/design/the-philosophy-of-faith-a-mirror-s-edge-interview .
  Mirror's Edge's level designer later said a sequel should show the route ahead instead of relying on red flashes [s]
  https://www.engadget.com/2010-08-17-overheard-gdc-mirrors-edges-odd-go-here-color.html .
- **The chain uses only pieces on the main path, and is not hidden.** Neon White rules 4 and 5: "The shortcut path cannot be
  hidden"; "must only use cards from the main path"; encounters are blocked out alone, then assembled, which "often creates
  opportunities for shortcuts" [read, Game Developer above]. Turbo Overkill chains pad, wall-run, dash, wall-run into an arena
  (forge65 rule 15) [read in forge65].
- **A fall drops you to the slower line, not to death.** Sonic: on Starlight Act 2 "a fall merely means the player falls to a lower
  path" [read, a fan analysis] https://griminpink.substack.com/p/a-quick-and-dirty-path-analysis-of . For SKYGRIND: the floor is the
  slow line and stays survivable, but costs (levers 2, 6, 11).
- **Spacing from SKYGRIND's numbers (derived; apex hang ignored; not checked against Mover.cs):** jump v0 = 2 x 1.5 / 0.36 =
  8.3 m/s, rise gravity 23.1 m/s2, fall gravity x1.35 = 31.2. A wall jump (v0 x0.9 = 7.5 m/s, out 8 m/s) rises 1.2 m and is back at
  its height after about 0.6 s: about 4.8 m out from the wall, and 4.8 m along it at 8 m/s or 14 m at 24 m/s. So facing ride walls
  more than about 5 m apart need the double jump. A pad (apex 8 m) flies about 1.55 s: about 12 m at walk speed, 37 m at 24 m/s.
  A ride lasts at most 1.2 s (9.6 m at 8 m/s, 29 m at 24 m/s); at most 3 wall jumps per airtime. The ride's sink is NOT derivable
  by hand: Mover.cs:626-642 applies flow34's ridePhase, ride35's rise gravity and Grip36 on top of wallRide.gravityMult 0.15;
  use air65's graph for heights. The dash-jump (v0 x0.8, gravity x0.5, 24 m/s) flies roughly 25 m flat (if the x0.5 applies on
  the fall too: not checked), which is why no small room's gap forces a ride.
- **Readable chain (proposal from the above):** every next piece is cyan (CLAUDE.md rule 8: cyan is player-usable), lit, and
  inside the camera's view at the previous jump-off; the chain's first piece is visible from the room's entry. Check: air65's
  edges plus a view test at each jump-off (the next piece's centre within, say, 40 degrees of the velocity direction).

## 3. Dressing with jobs, not decoration
- The LDB: "landmarks have to feel relevant and useful !! otherwise they don't function as landmarks"; "Actual landmarks (what the
  player notices during play) vs. fake set dressing landmarks"; "Sightlines are tools that players use when it seems relevant"; and
  a warning: "Leading lines routinely fail if you actually bother to playtest" [read]
  https://book.leveldesignbook.com/process/blockout/massing/composition .
- Cover: "Often less cover is better"; "Dodging is often a better strategy than standing still behind cover"; "In general, low
  cover is better than tall cover" (Epic) [read] https://book.leveldesignbook.com/process/combat/cover .
- Every piece is a move: "In the Tony Hawk world, everything is a ramp or a grind rail"; "Everything is a tool for executing moves
  and trying to create the perfect line" [read] https://haywiremag.com/features/no-rules-skate-everything/ .
- Pieces that kill: Bulletstorm's cacti and spikes are placed to be kicked into (section 1, lever 10) [s].
- Pieces that tell: Neon White: "Doors are always red and always on the main path, green ivy means you're going in the right
  direction" [read, Game Developer above]; Apex's drop towers: "if you see a drop tower you go 'oh there's probably people around'"
  (Mackey McCandish) [read] https://press-start.com.au/news/playstation/2019/02/05/heres-why-apex-legends-doesnt-have-titans-and-wall-running/ .
- Pieces that open: Severed Steel's destructible voxels let the player "open shortcuts" [read, PlayStation Blog above].
- **The "not just decoration" test (proposal, built on the above):** a piece passes if it does at least one job a script can see:
  (a) it can be stood on, ridden or bounced, and the verb pilot touched it on a clear (forge65 rule 10's touch count); (b) it
  blocks a ranged enemy's line of sight to the walk line; (c) a pushed or launched enemy dies or is trapped on it; (d) it marks the
  next chain piece or the exit and is visible from the fight floor. Report the share of pieces with a job, and the share with a
  collider: NIGHT65's density round added a3 13 props without a collider (report section 1), which fails all four.

## 4. Methods for an 8-hour autonomous run
- **/goal is a prompt-based Stop hook judged by the small fast model** on "the condition and the conversation so far"; "It doesn't
  run commands or read files independently"; up to 4,000 characters; good conditions have "One measurable end state", "A stated
  check", and constraints; a bound such as "or stop after 20 turns"; verdicts Not yet met / Met / Impossible ("Impossible" clears
  the goal) [read] https://code.claude.com/docs/en/goal . "No tool use for several turns in a row" stops the loop with the goal
  still set. Background work skips evaluation; check-ins at 30 min, then doubling to every 2 h; a usage limit pauses the goal and
  it resumes when the session auto-continues; resume restores the goal (every route since v2.1.239); a context overflow that
  auto-compaction cannot clear clears the goal [read, same page].
- **The block cap:** "Claude Code overrides a Stop hook after it blocks eight times in a row with no tool call from Claude in
  between"; hooks read `stop_hook_active`; `CLAUDE_CODE_STOP_HOOK_BLOCK_CAP` raises the cap; a prompt hook returns `ok`, `reason`
  and optionally `impossible` [read] https://code.claude.com/docs/en/hooks-guide .
- **What a judge can check unambiguously:** "A good task is one where two domain experts would independently reach the same
  pass/fail verdict"; "it's often better to grade what the agent produced, not the path it took"; checking "a sequence of tool calls
  in the right order" is "too rigid"; give the judge a way out ("Unknown") and grade each dimension separately [read]
  https://www.anthropic.com/engineering/demystifying-evals-for-ai-agents . "Be empirical or specific", e.g. output only 'correct'
  or 'incorrect' [read] https://platform.claude.com/docs/en/test-and-evaluate/develop-tests .
- **What went wrong on NIGHT65 (repo):** the goal (Research/2026-10-23-forge65/GOAL.txt) listed nine items "in order", "a printed
  clock line reading 6:00 or later before the close starts", and "printed after every background job". At 6:32 "the Stop hook
  asked to see the close files committed after the close began" (NIGHT65-LOG.md:296-297): the judge had to reconstruct history.
  The tag could not follow (the guard refuses moving a tag, L-107), so any condition tying the tag to the last commit cannot be met.
- **Recommendation for NIGHT66's goal (proposal):** move every history and order check into one script, e.g.
  `node Tools/night/n66-close-check.cjs`, that prints one PASS / FAIL line per item (it reads the clock, `git status`, `git log`
  ancestry, the tag, the slate, the saved gate outputs, and accepts "logged under Blocked" as resolved) and ends with one line:
  `CLOSE-CHECK n66: PASS 9/9` or `CLOSE-CHECK n66: MINIMUM PASS` (the cap path). The condition then reads: "Met when the final
  assistant message contains the output of `node Tools/night/n66-close-check.cjs` ending in `CLOSE-CHECK n66: PASS` or
  `CLOSE-CHECK n66: MINIMUM PASS`, followed by the line `NIGHT66 COMPLETE`." No "first", "before", "after", "in order" or "every
  turn" clause; working rules stay in NIGHT66.md (the forge65 goal already split rules from conditions: keep that). Every item that
  a guard can refuse (builds, reels, smoke) must have its Blocked alternative inside the script, or the judge may rule Impossible.
- **Avoiding a judge loop:** (a) final-state conditions only (above); (b) while waiting, end each turn with a tool call (the clock
  print rule does this), never a text-only reply, so neither the "no tool use" stop nor the 8-block cap fires; (c) run the check
  script after the last commit, so its output is the newest evidence; (d) a bound in the condition (the clock's phase CAP).
- **Keeping a long run on track (Anthropic):** a progress file and a JSON feature list where agents may only change a `passes`
  field ("It is unacceptable to remove or edit tests"); "work on only one feature at a time"; end each session with a commit and a
  progress update; the failure modes are "Claude declares victory on the entire project too early" and "marks features as done
  prematurely" [read] https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents . Structured state in JSON,
  notes in text, git as the log; starting fresh can beat compaction ("Review progress.txt, tests.json, and the git logs"); a
  sample line telling Claude not to stop early for token budget [read]
  https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/claude-prompting-best-practices . Compaction, a NOTES.md,
  sub-agents that return "a condensed, distilled summary", and context rot [read]
  https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents . "Each subagent needs an objective, an output
  format, guidance on the tools and sources to use, and clear task boundaries"; judge the final state; "minor system failures can
  be catastrophic for agents" [read] https://www.anthropic.com/engineering/multi-agent-research-system . Give Claude "a check it
  can run"; show evidence, not assertions; an adversarial reviewer in a fresh context, told to "flag only gaps that affect
  correctness or the stated requirements"; after two failed corrections start fresh [read]
  https://code.claude.com/docs/en/best-practices . A subagent in a worktree does not load the worktree's CLAUDE.md, and worktrees
  branch from the default branch unless told (FORGE65's read of the docs, research-run-methods.md section 4; not re-checked).
- **Mapped to SKYGRIND (proposal):** slate.json is the `passes` list; each room's row passes only on its verb instrument (the
  skip margin with the refusal proven, lever 11), never on the walk bot (report section 7); critics get the requirement-only rule.

## 5. Not found or not checked
- No published threshold for how much faster the verb route must be (lever 11's 20 % is a proposal); no Titanfall wall spacing in
  metres; no id quote on weak points as design intent (GameStar's German report was 403); the GDC 2018 DOOM talk and Noclip's DOOM
  video were not watched (video); the Halo "Illusion of Intelligence" slides were fetched but not readable here (no PDF renderer),
  so their 3-minute / 30-second split is [s] https://www.jmeiners.com/shamans/papers/ai/the_illusion_of_intelligence.pdf .
- Not checked in the repo: what charger.weakMult multiplies; how regen.fastMult and director "lingering" are computed; every
  derived flight number against Mover.cs.
