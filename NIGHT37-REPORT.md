# NIGHT37 report for GG: the boss night

**Play:** `Builds/n37-r7/SKYGRIND.exe`, the level select: levels 1, 2, 4 and 5 end in the new bosses. `-skygrindOff37` gives last
night's game (the old bosses) to compare. THE KILN WYRM (level 3) is untouched, as you asked; level 6 still has the old Stag until THE
RIVAL (your D-1).

## What you asked for, and what's there
You asked for every boss but the worm restarted from scratch, arenas and AI included, as moving fights that use each level's mechanic
and weapon, not "a boss fight in a room". All four were rebuilt from zero on one new shared brain, and none of them is fought standing
in a room:

| boss | the fight | the level's verb + weapon | in a Player tonight |
|---|---|---|---|
| **THE WICK** (level 1, replaces THE LANTERN) | a climb up a 40 m furnace stack away from rising slag; only cyan panels hold you, the bare wall is hot; you hurt it from the walls | the wall ride + the look-up jump-off, the parry, the KESTREL's pierce | climbed to the cap leg, 3 parries, 3 plates cracked, a pierce, the window; **not finished in the Player run** |
| **THE STAG** | a chase around a frozen canal loop; ride its slipstream, change lanes to dodge, catch it and punch its legs; the ice breaks into floes; it charges you head-on at the end | skating, the PULSE GAUNTLET, the launch, the parry | killed in 24.2 s; the floes didn't drop in the final run (they did in the run before) |
| **THE LOOM** | no floor: you cross a canyon on the grapple while it cuts the threads behind you; nail its knees and pull away to tear them out | the grapple, the STITCHER (nails + detonate), the bend | killed at 21.7 s, 6 knees torn |
| **THE CROWN** | a descent down a breaking spire: orbiting fragments, a broken ramp, the plaza; jump-dash THROUGH its ring to burn its gems | the jump-dash, the TORCH, the parry | killed; every attack seen |

## How the new brain works (all four share it)
Parts of it come from ULTRAKILL's boss AI, read from your copy: numbers and rule shapes only, no code copied.
- **Stagger, not a health sponge.** The right move drains a meter; when it empties the boss opens CYAN for 2.5 s and takes full damage.
  Each fight lands at ~20-22 s for the bot (THE WICK ~34 s, ~21 s of which is the climb).
- **A rhythm you can read.** Two attacks, then a rest. An attack budget forces a breather. A parry refunds the budget.
- **Wind-ups.** Every attack winds up 1.0-1.5 s: YELLOW = punch it back, RED = get out of the way, CYAN = your window.
- **It reacts to how you play.**
  - It never repeats an attack.
  - It punishes the habit you fall into: too long on the ground, or too long in the air.
  - Hide or camp for 3 s and it comes to you: THE LOOM severs your tower, THE CROWN drops your platform.
- **A fall restarts the leg, not the fight.**

## Things I measured and changed on the way (worth knowing)
- **THE STAG and a casual player.** Someone who just sprint-glides (no dash pumping) could never catch it. It now slows and looks back
  when you fall 25 m behind: a casual pilot catches it every time and kills it in 27-37 s.
- **THE WICK's climb.** It was tuned for a person, measured on the real movement: looking up and letting go gives you 7.2 m of each 8 m
  panel to release from. Looking level mostly falls short.
- **No WICK attack had ever fired** until a builder found the attack ids didn't match the config.
- **Nothing you had was lost.** Every verb check that failed was traced to the check itself, with last night's levels as the control.
  Level 2 passes 30 / 30 and level 4 32 / 32 in the Player.

## Decisions for you
- **D-7, THE WICK's length:** the bot takes ~34 s because 21 s of it is the 40 m climb. I kept the climb rather than shrink it to fit the
  15-22 s band (that band was set against health sponges). Shorter climb, or keep it?
- **The climb's strictness:** a level leap can still catch the next panel from part of it (2.4 m of 8). Raise the panels so you *must*
  look up, at the cost of a harder climb?

## Not done or not proven
- THE WICK's kill in a Player, and its CYAN window frame (the camera faced the wall). The harness kills it 10/10.
- THE STAG's floes in the final run (shown in the run before).
- THE CROWN's ring passes in the Player run were scripted, so they show the look, not the timing; the harness's real passes do the timing.
- No person has fought any of the four. The bot times say little about yours.
- The level-5 jump pads don't fire in the moves check (the same on last night's spire; a headless probe launches all three).
- Level 6's boss (THE RIVAL) is next; the rooms before each boss in levels 2, 4 and 5 are still the old ones.
