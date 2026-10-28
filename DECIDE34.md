# DECIDE34: GG's calls for NIGHT34 (each has a default the night uses if GG says nothing)

| id | question | default |
|---|---|---|
| R-1 | REA (the Reverse Engineer Anything skill): which targets may it read? Its `rea` server timed out on connect in the forge session, so check `/mcp` first. Options: (a) none; (b) only open-source builds and our own Player (for example, timing); (c) also installed builds of games you own (for example ULTRAKILL), numbers and structure only, no code copied, every finding logged with its source. | (a) none: the night runs on the web research set. **Answered 2026-10-11 ~17:40:** GG asked to use it. REA connected after MCP_TIMEOUT 90 s (the server needs 2.5-8 s to start, and timed out at 30 s at session start). Target: GG's installed ULTRAKILL, numbers and rule shapes only, no code copied: Research/2026-10-11-wave34/research/rea-ultrakill.md. REA's .NET reader stops at its 64 MB metadata budget on ULTRAKILL's assembly, so the code was read with ILSpy in the scratch folder |
| B-1 | The bend's default: SPLIT (the world slows, you stay at full speed) or GLOBAL (everything slows, swaps instant). | the lead picks from frames and logs why; both stay switches |
| B-2 | The bend's cost: every latch gives a free 1 s SHORT BEND; the 2.5-5 s FULL BEND is paid by a meter that only kills refill (about 15 s of fighting from empty). Alternatives: every latch a full bend with no meter, or a fixed cooldown. | short bend free, full bend on the meter |
| B-3 | The player's own rockets and grenades slow with the world inside a bend (a Max Payne read). | yes |
| H-1 | The arsenal rail's place: bottom right in one row (where the gun name is now), or a column on the right edge. | bottom right, one row |
| A-1 | calm34 removes at least 40 % of dressing props per level (props with no collider and no purpose). | 40 % |
| M-1 | flow34 may edit `Mover.cs` inside its own switch (rule 3 allows it when a task names it; off stays bit-exact). | yes |

## New calls from the night (2026-10-11 17:05; each is a switch, so a "no" costs one config line)
| id | question | what the night shipped (the default) |
|---|---|---|
| N-1 | The bend's speed: the world at a quarter (scale 0.25). Slower (0.2) or faster (0.33)? No game publishes this number. | 0.25 |
| N-2 | Should the free 1 s short bend fire on EVERY grapple latch, or only when the meter is full (rarer, heavier)? | every latch |
| N-3 | The bend default is SPLIT (the world slows, you don't). GLOBAL (everything slower, swaps instant) is `design.n34.bend34.split: false`. | split |
| N-4 | Your own rockets and grenades slow with the world inside a bend (B-3). | yes |
| N-5 | The air multiplier: up to 3x points and style while airborne or sliding (ULTRAKILL's rule). Too generous? | 3x, on |
| N-6 | The combo names and bonuses (BEND BREAKER +150, TRIPLE BEND +250, FULL ARSENAL +400 style) were the builder's choice. | as listed |
| N-7 | The calm pass cut 73-81 % of the decoration props, against the plan's 40 % floor. Keep it this bare, or add kinds back (one config list, `design.n34.calm34.keepKinds`)? | 73-81 % cut |
| N-8 | The air lurch costs speed (kept 90.6 % instead of 102.8 % on a 90 degree steer). Keep it, or turn `design.n34.flow34.lurch` off? | on |
| N-9 | Levels 4 and 5 (span, spire) were not rebuilt for movement lines tonight (span with nodes broke the walk bot, 8/10). Next night? | NIGHT33's rooms + the calm pass |
| N-11 | The bend shortens boss fights: THE STAG died in 10.1 s (Player bot; NIGHT33 17.6 s; band 15-22 s). Options: leave it (bosses were "still way too tanky"), make the world share inside a boss fight 0.5 instead of 0.25, or allow only the short bend in boss rooms. | left as is (not tuned without your read). Evidence from ULTRAKILL's code: it never gives the player slow motion in boss fights (only scripted boss moments), which supports "only the short bend in boss rooms" |
| N-10 | A project-wide Input System "run in background" setting (carried from NIGHT33; the shop reel now passes without it). | not set |

## Parked threads (age in waves; each waits for GG or a later night)
| thread | age | note |
|---|---|---|
| T-18 map night 2 (U3-32) | 15+ | deferred again: lines34 rebuilds the five campaign levels instead |
| T-20 UnityConnectSettings | 10+ | GG decides; never staged |
| T-11 RESULTS' grade card over POINTS; T-12 M9 pop-ins | 16 | not this wave |
| T-04 / T-05 / T-06 S2 niches, S4 SWAP-CANCEL 19 %, S6 shotgun 60 % | 15 | the bend's instant swaps may move S4: the run reports it if THE RANGE runs |
| T-46 lava orange metric; T-47 hold-still tell; T-48 lava edge pressing; T-49 the allocation test | 1 | carried in NIGHT34.draft.md; leftovers after B1 / H1 / A1 / M1 |
| T-43 nobody has played n33-r7 | 1 | answered by GG's 2026-10-11 request (he played n33 in the Editor) |
