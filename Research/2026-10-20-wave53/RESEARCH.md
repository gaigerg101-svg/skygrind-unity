# Wave 53 research (2026-10-20 ~07:10; three web-only agents, every claim with its URL; marks: [v] verified on the page, [s] search snippet only)

## A. Arena design: what proven sources say (and what they do not)
No primary source gives arena sizes in metres, tier counts, sightline lengths or cover spacing for DOOM 2016 / Eternal, ULTRAKILL, Halo or
Titanfall; the talks speak in principles. Our numeric contract (NIGHT52 section 2) stays our own measured hypothesis.
- A-1 [v] DOOM's push-forward rule: "If the player had to hide behind a wall, run away, or wait for something to recover, then that needed to
  be changed." Resources come from combat. https://www.gamedeveloper.com/game-platforms/pushing-push-forward-combat-with-gameplay
- A-2 [v] DOOM's Lazarus Labs: a room whose flow failed was fixed by joining two dead-end halls into a loop (portals), not rebuilt; "the line
  of sight breaks, the ins and outs"; "You realise how fast you're going when you start to shrink the space" (Jerry Keehan).
  https://blog.playstation.com/archive/2017/05/12/classic-levels-deconstructed-the-beautiful-brutality-of-dooms-lazarus-labs
- A-3 [v] Arenas as "skate parks": height to "get height on the demons and fire down" (Marty Stratton).
  https://www.gamewatcher.com/2015-07-07-doom-s-arenas-are-almost-like-skate-parks-according-to-exec-producer
- A-4 [s] DOOM Eternal (Hugo Martin): no hidden corner to pick demons apart for a whole fight; enemies push you off ledges; arena pickups are
  deliberately not enough to finish the fight. https://www.fandom.com/articles/doom-eternal-marty-stratton-hugo-martin
- A-5 [v] Halo (Jaime Griesemer): "a 3-second loop inside of a 30-second loop inside of a 3-minute loop that is always different"; "No 30
  second stretch of Halo is ever repeated; the missions are constantly changing the context on you."
  https://www.engadget.com/2011-07-14-half-minute-halo-an-interview-with-jaime-griesemer.html
- A-6 [v] Halo AI: enemies hold territories; the encounter moves as the player widens a safe zone (enter, plan, execute).
  https://gamedeveloper.com/game-platforms/in-depth-bungie-on-eight-years-of-i-halo-i-ai
- A-7 [v] ULTRAKILL (Hakita): close-quarters spaces stop long slides; tanky enemies "would detract from the high tension do-or-die quick
  action". https://intothebluesky.com/2020/12/19/interview-with-arsi-hakita-patala/ ; [s] simple, clean geometry that reads at a glance:
  https://80.lv/articles/ultrakill-devs-on-the-game-s-mechanics-npc-ai-and-early-access-experience/
- A-8 [v] The Level Design Book, encounters: the "door problem" (give the player a look over the arena, or they fight from the doorway);
  three openings (the player ambushes; the enemies ambush past a midfield line; a vista with a one-way entrance); enemy types at once
  1 rest, 2 normal, 3 complex, 4 brawl, 5+ chaos. https://book.leveldesignbook.com/process/combat/encounter
- A-9 [v] Doom (1993) community metrics: rooms 256-512 / 512-1024 / 1024-1536 units (about 10-19 / 19-38 / 38-58 m); monster corridors at
  least 128 x 128 units. https://book.leveldesignbook.com/process/blockout/metrics/doom
- A-10 [v] Iteration by data: Halo 3 kill / death heatmaps filtered by weapon. https://www.coolinfographics.com/blog/2009/1/12/halo-3-heatmaps.html ;
  [v exists] Respawn's "action blocks" for Titanfall 2: https://80.lv/articles/gdc18-designing-titanfall-levels-with-action-blocks
- Not found: Apex arena metrics; published time-to-kill or "where players stall" numbers.
**For tonight (rooms that pass numbers but read as templates):** each room changes its context (A-5: its opening, its pairing, what you do
there); a route earns its place by being the only fast link between two places that matter (A-2: the ride is not a lap of the ring); give
each room a different opening and ground the enemies hold (A-6, A-8). -> NIGHT53.md section 2, I1-I7.

## B. Open-source tools (GitHub API, 2026-10-20: stars, licence, last push)
- B-1 ikpil/DotRecast | 954 | zlib | 2026-10-03: pure C# Recast/Detour; can take TriangleWorld's triangles (no PhysX) for reachable-area
  maps, path lengths (I2 / I3) and coverage heatmaps. Est. 4-6 h for a first slice (not measured). https://github.com/ikpil/DotRecast
- B-2 mob-sakai/UIEffect | 7,692 | MIT | 2026-08-27: shiny / dissolve / hit-flash on uGUI + TMP (the project already has
  Assets/ProjectSettings/UIEffectProjectSettings.asset: likely installed). https://github.com/mob-sakai/UIEffect
- B-3 mob-sakai/ParticleEffectForUGUI | 6,024 | MIT | 2026-10-16: particles in the HUD canvas (kill pops, hit markers).
- B-4 annulusgames/LitMotion | 2,343 | MIT | 2026-09-18: zero-allocation tweens for HUD punch / shake (view layer only; sim timers stay in steps).
- B-5 Tayx94/graphy | 2,910 | MIT | 2026-08-29: an in-Player FPS / memory overlay (the first-shot hitch, T-122).
- B-6 mxgmn/WaveFunctionCollapse | 25,418 | MIT | 2026-10-15; BorisTheBrave/DeBroglie | 527 | MIT (3D WFC with path constraints);
  OndrejNepozitek/Edgar-Unity | 912 | MIT (graph room generator, mainly 2D); radiatoryang/scopa | 158 | MIT (.MAP into Unity) with
  TrenchBroom | 2,872 | GPL-3.0 editor (its map files are not GPL); LogicalError/realtime-CSG-for-unity | 1,012 | MIT. Each needs an
  exporter into our plan JSON first: a later night (the TrenchBroom pipeline is parked for GG, NIGHT52.md section 5).
- B-7 Unity-Technologies/ml-agents | 19,776 | Apache-2.0: learned explorers need a Python trainer and hours: not tonight.
- Not found: a maintained open-source equivalent of MoreMountains Feel.

## C. Long autonomous runs with Claude Code and Opus 5.5 (official docs)
- C-1 /goal "is a wrapper around a session-scoped prompt-based Stop hook"; the checker "doesn't run commands or read files", so the
  condition names proof printed in the conversation; at most 4,000 characters; a running subagent or background command skips the check
  that turn; a check-in after 30 minutes; a usage limit pauses it ("Goal paused"). https://code.claude.com/docs/en/goal
- C-2 /loop with a prompt and no interval is self-paced (a delay between one minute and one hour); it ends with ScheduleWakeup stop:true
  or a fallback wakeup about 20 minutes later; resume does not restore it. https://code.claude.com/docs/en/scheduled-tasks
- C-3 Worktree subagents branch "by default from your default branch": this repo's default is main, not port/stage3, so builders reset
  their branch onto port/stage3 first. https://code.claude.com/docs/en/sub-agents
- C-4 Long-run harness: a feature list file, a progress file and git; "only one feature at a time", commit each change.
  https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents ; a separate evaluator agent with sprint contracts:
  https://www.anthropic.com/engineering/harness-design-long-running-apps
- C-5 Opus 5.5: a text-only progress turn is "a report rather than proof the task is done"; keep a checklist file; an elapsed / budget line
  in each message helps pacing; default effort medium matched Opus 5 at high on coding; set effort before the kickoff, do not change it
  mid-run. https://platform.claude.com/docs/en/build-with-claude/prompt-engineering/prompting-claude-opus-5-5 ;
  https://platform.claude.com/docs/en/models/opus-5-5/whats-new-opus-5-5
**For tonight:** the /goal names printed proof only; the /loop reads the log, the queue file and `git log` first, does one item, commits,
stops itself by name; every log line and STOP POINT carries "elapsed H:MM / 3:30"; builders base on port/stage3.
