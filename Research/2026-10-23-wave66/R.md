# NIGHT66 builder R: notes (a2 THE RANGE, a3 THE RUNNER, a5 THE POUR)

Branch n66-R from e37083cf (the t0 sha; t0 2026-10-23 22:26:11 MST), in the reused worktree .claude/worktrees/agent-a9d0ffb6d0dfdb2be.
Branch C: sim only, no Player. The usage limit stopped every agent from about 22:46 to 00:41 (the lead's message); this note resumes at 00:42.

## Verb plans (22:29-22:45, finished 00:42-00:50)
Read: the builder brief, CLAUDE.md, docs/PRINCIPLES.md, docs/VERBS.md, NIGHT66.md (main tree), Logs/n66/slate.json (read only), the forge's
n65-data.md, research.md, history.md section 3, DECISIONS.md (D-4, D-5, D-8, R-1..R-14), redteam.md findings 3 and 9, my NIGHT65 cards and
notes (Research/2026-10-23-wave65/cards/a2.md, a3.md, a5.md, R.md), the t0 room files, NIGHT65's red skip output (T/verb-skip-verbs65-red.txt).

| plan | the lever | round 1's pieces |
|---|---|---|
| plans/a2.md | the rack's 8 s hold, broken by one pierce only from THE RANGE LINE (the entry run), then wave 1's file down the same line | THE RACK moved into a depth file on the line; THE FEED RAMP re-laid onto it; THE MARK to the floor; line65 (tour65 WARN frame 7) |
| plans/a3.md | THE LANDING holds the ride's lesson (a refuser pays the 8 s cap or the stair); THE PICKETS, stand-ins only a ride shot kills, hold wave 1 | the lesson rect moved; two stand-ins on the ride's line |
| plans/a5.md | wave 2's lip held by launchable spitters with THE POUR behind them; round 2: THE SPRUE GATE files wave 1 for one launch | wave 2 recomposed (no new kind); the lip nodes; the pickup; line65 (tour65 WARN frame 4) |

## Code facts read for the plans (data for the critic)
- The holds (Campaign24.cs:844-907; Design/unity-config.json design.n46.teach46.numbers): stand-ins hold the next wave at most 8 s after the
  entry; an afterLesson43 wave waits at most 8 s after the entry; a lesson not done when the enemies are dead is capped 8 s later. A room
  clears only with its waves dead and its lessons done.
- Stand-ins die only to a shot fired from a wall ride, or to a charged KESTREL shot in a standIns31 room (Combat.cs:1464).
- The charged shot: pierce 24, x9 damage (225 a body), no falloff (design.n33.pierce33); it is a ray against body capsules (husk radius 0.4 m).
- The verb-aware pilot's pierce trigger counts a second body within 2.0 m of the line (Harness.N65.VerbPilot.cs:203): wider than the 0.4 m a
  shot actually needs. It is T's tool; I note it, not change it.
- The brain fires only when its look is settled on its target (Assets/Scripts/Core/Bot/BotBrain.cs:451-456); the verb layer sets the look
  along the face during a ride, so a rider shoots only what stands ahead on the ride's line.
- Launch36.Wants takes any Husk-class body: husk, swarmer, spitter (one pool class), never a stand-in or a tank (Launch36.cs:96).
- Level 1's kinds today: husk (a2, a3, a5, b1, b2, a8), swarmer (a3), spitter (a4, a5, b1). "tank" is a kind level 1 does not have: a new
  type, GG's call (D-12), not used.
- spawns33's trigger lines (a wave waits for a rect) live in config (design.n33.spawns33.triggers33, keyed by room id across levels), not in a
  room file: not mine.

## Disk
C: 8 GB free at 22:29 and 22:43; 12 GB at 00:42 (`df -BG /c | tail -1`).
