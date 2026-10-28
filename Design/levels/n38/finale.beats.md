# LEVEL 6 THE LAST LINE, n38: the beat sheet (NIGHT38 builder F, card F; written before any geometry)

n35-finale is rebuilt whole (DECIDE38 D-5; GG 2026-10-13: "I want overhauls on all the levels"; old rooms "with just random boxes and lava ... no
meaning" must go). No n35 room, generator or prop survives except THE STAG's room row, kept as it is until THE RIVAL (D-5). Generator:
`Tools/night/gen-n38-finale.cjs` (requires only `n36-lib.cjs`); plan `Design/levels/n38/n38-finale.plan.json`; level `Design/levels/n38-finale.json`,
id `n38-finale`; the campaign plays it behind `design.n38.finale38` (levelSwap `n35-finale` -> `n38-finale`). Regenerate:
`bash Tools/night/n38f-regen.sh finale`.

The level is the EXAM (PRINCIPLES 4; teach.md rule 9): no new idea, every verb of levels 1-5, the weapons of n 1-5 (KESTREL, PULSE gauntlet,
THUNDERCLAP, NAILGUN, TORCH) and the grapple. PLAN.md section 2: "mixed gates, one per verb, in sequence: ride, boost, grapple, dash jump",
two open arenas.

| # | room | size (w x l x h) | job | verb | time |
|---|---|---|---|---|---|
| 1 | x1 6.1 THRESHOLD | 10 x 12 x 6 | link (the name) | walk | 4 s |
| 2 | x2 6.2 THE FLOOR | 36 x 40 x 14 | combine (open arena 1) | ride + shoot + hook, on the move | 40-60 s |
| 3 | x3 6.3 THE RIDE | 20 x 46 x 14 | test (gate: ride) | the ride chain | 8-15 s |
| 4 | x4 6.4 THE STEP | 16 x 24 x 10 | test (gate: boost) | the THUNDERCLAP's mid-air boost | 5-10 s |
| 5 | x5 6.5 THE SPINE | 40 x 40 x 16 | combine (open arena 2) | everything, one rule-breaker | 50-70 s |
| 6 | x6 6.6 THE VOID | 20 x 56 x 16 | test (gate: grapple) | the hook over a void | 8-15 s |
| 7 | x7 6.7 THE LEAP | 14 x 34 x (low roof) | test (gate: dash jump) | a far jump + dash under a ceiling | 5-10 s |
| 8 | x8 6.8 REST | 6 x 10 x 4 | rest | (health, the checkpoint) | 4 s |
| 9 | x9 6.9 THE STAG | 28 x 52 x 12 | boss | n35-finale's x6 row as is (boss `glacier`) | |

## Rules this sheet is held to
- NIGHT36's: one job per room; every solid `job` + `why`; a walkers' climb to every raised enemy spot; lava only where an enemy can be
  launched into it (this level has none).
- NIGHT38's: open arenas (24-40 m across, roofs 10-16 m, at most three floor planes, the exit seen from the door) with enemies switched on
  AHEAD on the line (arena36 flow); each gate tagged `gate: {verb, from, to}` for builder E's checker; a gate ships only when the checker
  passes it (the intended verb with >= 10 % margin, no other single verb or combo, D-6); a fall costs a climb back to the gate, never a death.
- The walls a gate's verb must not use (boost, grapple, dash-jump rooms) are `noride` (dull, no cyan trim: builder M's flag) so the held ride
  (ride38) cannot climb or carry past them; only the gantry-style ride gate has cyan ride walls.
- KNOWN RISK, measured when E's checker lands: the grapple (design.n35.grapple35: a bare wall is never refused, ~40 m) can hook past every
  gate but its own. If the checker shows it, the boost / dash-jump / ride gates stay RED and the report says so (a per-room no-hook rule is
  the lead's call, not this card's).

## x1 6.1 THRESHOLD (link)
- The door; the level's name: "THE LAST LINE. EVERYTHING YOU LEARNED." (lesson `reached`). Cyan trims, the next door's cyan pool.

## x2 6.2 THE FLOOR (combine: open arena 1)
- **Job / verb:** the first open arena of the exam: ride the zig-zag of two long walls up the middle while husks and swarmers run at the line;
  spitters on the north catwalk; two grapple rings over the floor for a hooked line to the catwalk.
- **Line:** spawn (0, 2) > wall A's east face (plane lx -5, lz 8..22) > off across onto wall B's west face (plane lx +5, lz 18..32; planes 10 m
  apart) > off onto the CATWALK (lz 34..40, 3 m, full width) or the floor > the exit (0, 38) on the catwalk's middle (its ramps at either end).
- **Enemies:** wave 1: 4 husks + 4 swarmers on the floor ahead (lz 14-30). Wave 2: 2 spitters on the catwalk + 4 husks on the line. Wave 3:
  6 swarmers + 2 spitters.
- **Reward:** a health pickup on the catwalk; the doors unlock on the last kill.

## x3 6.3 THE RIDE (gate: ride)
- One gap of RIDE_GAP (34 m) over a 4 m dry pit, spanned by five ride walls whose faces alternate (planes 8 m apart). Recovery steps at the
  pit's south end. Gate: verb `ride`, from the lip to the far floor.

## x4 6.4 THE STEP (gate: boost)
- A wall-to-wall STEP of STEP_H (3.6 m) at lz 12..24: higher than the jump + double jump (about 2.7 m), under the jump + double jump + one
  THUNDERCLAP shot fired down in the air (about +2 m). Its face and the room's walls are `noride`. No ceiling (the boost needs the height).
  Gate: verb `boost`, from the floor (0, 0, 10) to the step's top (0, STEP_H, 15).

## x5 6.5 THE SPINE (combine: open arena 2)
- **Job / verb:** the second open arena: a long double-sided ride wall (THE SPINE, lz 8..30, 7 m) splits the floor into two lanes; two decks
  (2.5 m) at the north corners with ramps; two grapple rings over the lanes; the last wave holds the level's one rule-breaker (a heavy).
- **Line:** spawn (0, 2) > either lane > ride the spine (either face), fire from it > off onto a deck > the exit (0, 38).
- **Enemies:** wave 1: 4 husks + 2 spitters (decks). Wave 2: 6 swarmers + 2 husks. Wave 3: 1 heavy + 4 husks.
- **Reward:** a health pickup on the west deck.

## x6 6.6 THE VOID (gate: grapple)
- One gap of VOID_GAP (44 m) over a 6 m dry pit: longer than any jump, no wall to ride (the room's walls `noride`); two grapple rings over the
  void at 8 m. Recovery steps at the pit's south end. Gate: verb `grapple`, from the lip to the far floor.

## x7 6.7 THE LEAP (gate: dash jump)
- A gap of LEAP_GAP (18 m) over a 3 m dry pit, under the room's LOW ROOF (LEAP_ROOF 3.6 m: the capsule's 1.8 m + a jump's 1.5 m clear it,
  the double jump's extra height does not), walls `noride`. Beyond the slide-hop (12.8 m), inside the jump-dash (21 m). Gate: verb
  `dashJump`, from the lip to the far floor.

## x8 6.8 REST and x9 6.9 THE STAG
- The rest: a health pickup, the checkpoint, the boss hint "IT CHARGES. DODGE. PUNCH ITS BACK." The boss room: n35-finale's x6 row (boss
  `glacier`, the Stag's rink) with the same size, lights and proof.

## Round 2 changes (measured on builder E's checker)
- Gates tagged on their LANDING decks (`runUp` 6, `owns38` every verb incl. boost and grapple); x3 on the gantry's two-wall layout
  (RIDE_GAP 30); the boost / grapple / dash rooms lined with 0.2 m `noride` liners (M's flag reads campaign boxes only), landing faces noride.
- THE LEAP 13.5 m (was 18): under the 3.6 m roof every apex is capped at 1.80 m; the best dash try (jump, dash, double) reaches 16.52 m, the
  best try without the dash 10.78 m. The roof opens the dash window.
- The `reached` lessons past a gate dropped (x4, x6, x7, x8): levelgen's player graph does not make the real Mover's crossings. The links
  clear on reaching their exits; level 6 is the exam (no text but the threshold's).
- Checker: RED 0 / 4. The THUNDERCLAP boost crosses x3 (jump + boost reaches 22-60 m) and x7; the grapple (line of sight within 40 m)
  crosses x3, x4, x7; x6's 44.5 m void needs two pulls, which the checker's grapple model (one pull to the landing) does not fly. On the
  exam level, where every verb is owned, D-6 cannot hold for a horizontal gap without a rule that takes the boost and the hook away at the
  gate (a lead / GG decision). finale38 stays off.

## Checks
1. n36-jobs GREEN; levelgen 0 errors.
2. E's gate checker on x3, x4, x6, x7: each GREEN, or RED with its measured reason in the report; the row turns on only when all are GREEN.
3. The walk bot (`n38f walk finale`): the arenas by the autopilot, each gate by a scripted run of its verb.
