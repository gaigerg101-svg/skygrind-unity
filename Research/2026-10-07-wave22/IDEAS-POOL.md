# IDEAS-POOL (wave 22 forge): the slate is mostly NIGHT21's unfinished rows, so the pool is short

The owner's priorities for this night: building, iteration, polishing, accuracy (all four). NIGHT21 left 7 contracts unproven and four
"beyond" rows unbuilt, so the wave-21 pool (`Research/2026-10-07-wave21/IDEAS-POOL.md`, 28 cards, scored through 9 lenses including a
persona panel) is reused, not regenerated. This file holds the slate and the new cards, each grepped against FEATURES.md and the code.

## Slate (9 seeded contracts; see NIGHT22-FEATURES.seed.json)

| row | card | status before tonight (grepped) | tier |
| --- | --- | --- | --- |
| arcline | NIGHT21 U3-35.3 closure + polish | Core + view exist; never seen; no chip, no reticle states | A |
| stamp | NIGHT21 U3-35.5 closure + T-11 | grades exist; no entrance, card over POINTS | A |
| deadline | NIGHT21 U3-35.6 closure | Core + tuned; no HUD bar | A |
| grind | NIGHT21 U3-35.7 closure | Core + TITLE / banner / RESULTS compile; no smoke step, no persistence | A |
| replay | C-01, the moonshot | Core ring only; the 0 m check is by construction | B |
| challenge | C-04 | Core only; no UI; mutators unproven | B |
| ghost-daily | C-03 (46 / 60) | NIGHT14's ghost exists (`Game/Route/Ghost*.cs`); the DAILY gate is not built | B |
| dusk-clock | C-14 (53 / 60) | not built; `UnityTuning` sky numbers exist; toward violet and cyan, never magenta or red-orange | B |
| haptics | C-23 (45 / 60) | partly built: `Game/ImpactFrames.cs` calls `Rumble.Play` for impact tiers; the shockwave, ARCLINE and DOWN have no pattern, and nothing logs | B |

## Seeded leftovers (approved, in this order; a run never ends at its list)

1. Directed steps for THE HUNT / THE BEACON (G-1; BO6 quest completion 4 -> 8.23 %).
2. T-12: M9 pop-in FAIL rows on the older screens (slide, roundend, gust, ui-weapons, zoom, special-round, regen) through the shared motion.
3. T-10: the words where glyphs belong ("E START ROUND", "RMB", "C SLIDE") routed through PromptText.
4. C-17 TRICK CHAIN (50 / 60) behind `design.n22.trickChain`, a sim change (one more fingerprint re-record).
5. C-18 OVERDRIVE (50 / 60) behind its own switch, after C-17.
6. THE GRIND's Cyber-Grind-style golden-beam spawn telegraph (research R6).
7. The Player-safe replacement for the rank letter's lost shine (DECIDE22 D-7).

## New cards from this forge (lens: the accuracy audit of NIGHT21's own claims)

| id | card | why | proof |
| --- | --- | --- | --- |
| A-1 | A measured replay error | "0.0000 m by construction" is not evidence | ring every 2nd step, interpolated, error vs the live path |
| A-2 | A virtual-pad contract | the pad pass used forced input, so the Y press was never tested | a virtual pad press of Y fires the yank |
| A-3 | Mutators in a code do something | no reader of `setup.mutators` was found | a test reads the run state |
| A-4 | A round P is reachable | the bot never reached SS | a scripted SS run prints `p=true` |
| A-5 | DEADLINE at both rounds, 30 seeds | tuned on round 10 with 10 seeds (48 %) | chase --twist deadline at 10 and 15 |
| A-6 | The bench, alone, three runs | never ran in w20 or w21 (CPU 100 %) | median of three, CPU under 30 % |
| A-7 | The allocation in `PerStep_AllocatesNothing` | 1 allocation over 21,600 steps under load | find it or document its source |

Dropped from the pool with the reason: wall-run and rail grind (a big Core change, 6 h each); persona bots as a build (C-16: the triage
is a subagent job already); the wonder weapon (GG names it first); map night 2 (a separate night).
