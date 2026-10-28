# NIGHT9 report: the oracle and the arsenal (`u3-n9`)

For GG, 2026-10-03. Every NIGHT9 item in `tasks.json` (U3-06..U3-12) is passing, none cut, and the "if time" item
landed too. Branch `port/stage3`, local tag `u3-n9`. Nothing is pushed or merged. Times are local, from `date`. Each
item's checks are in `NIGHT9-LOG.md`.

## What shipped

| Commit | What |
|---|---|
| `ff72de9` | **The oracle + determinism (U3-07, U3-04).** 28 golden vector sets from the web (14 new + `jsMath`), every one checked by C# code that the Editor tests, the Player and a dotnet harness share; the 6 trial routes replayed bit for bit (22 replays); V8's maths (atan2, exp, tan, acos, asin, atan, tanh, log, pow) in JsMath, no System.Math left in the sim. The vectors found eight port bugs, all fixed (logged). |
| `7b9104c` | **The arsenal (U3-08).** FRESH per weapon (damage and points x0.65 to x1.25), PRIMED swaps, CROSS / COLLAPSE / OVERLOAD combos, the five statuses (burn ticks), the SWAP kill, FLIP's machine; the forge's statuses now land. |
| `3bba47f` | **Momentum (U3-09).** Speed is damage (melee up to x3, guns up to x1.35), IMPACT, a fast punch / GAUNTLET hit sends a ring wave and a short hitstop; a momentum ring round the crosshair. |
| `250930e` | **The V lock and the lunge (U3-10).** V locks the enemy nearest the crosshair, V again cycles, alone it releases; the punch and the GAUNTLET lunge to a body just out of reach. |
| `b749414` | **Four perk machines (U3-11):** BOUNTY, DEADEYE, FLOW, SLIPSTREAM. Ten of the web's 14 perks now. |
| `926ebb8` | A Core harness: the oracle outside Unity in about a second (a tool, not a gate). |
| `c818416` | **`Player[]`, first slice (the "if time" item):** enemies hunt the nearest living player; the flow field seeds every player; events carry their owner. One player plays exactly as before. |

## The numbers (tonight's runs)

- Tests: `339 / 339 passed, 0 failed`; `replays: 22 / 22`; `golden tests: 84 / 84` (1:49 AM). The NIGHT8 close had 237 /
  16 / 55.
- The Player (built 1:39 AM, 25 s): smoke `[smoke] 25.1s DONE: all steps passed`, 0 exceptions; `-skygrindGolden`
  `28 vector sets (30523 records), 22 replays (22 exact); 0 failing, 0 mismatches` in 4.4 s.
- Bench, 60 s with 24 husks at 1920x1080: median 2.49 ms, worst 1% 4.49 ms, 381 fps mean, on the Silent plan on AC.
  NIGHT8's was 3.35 / 8.37 ms on Balanced, so the two aren't a fair comparison.
- Two players in a test, 86.3 m apart: each husk reaches and hits its nearest player; when one dies, its husk crosses
  85.6 m to the other in 15.4 s.

## What is partial, and why

- **Impact waves:** only the ring (GAUNTLET, punch). The crescent / echo / quake / crack belong to melee weapons not
  ported yet (N12). The FX is a simple expanding ring; the web's full look is N12.
- **Statuses have no look:** the web draws none either (nothing listens to them).
- **Perks:** STEADY, ANVIL, STATIC, PHOENIX need systems from N12.
- **`Player[]`:** the enemies' half is in. The director, the wallet, buys, power-ups and zones still serve one player
  (N10). A second player's input, camera and HUD are U4-02 (your call, D-02).
- **Math.pow** with a non-integer exponent can't be matched exactly: V8 uses the host's `pow`, which isn't the same
  everywhere. The port uses a fixed formula there (HANDOFF risks).

## Your 10-minute play checklist (`Builds/Stage1/SKYGRIND.exe`)

Keys as NIGHT8's report, plus **V: lock on**. One question per point; a yes / no and a word is enough.

1. **FRESH:** fire one gun for a while, then swap. Does the weapon name's freshness tag fall with use and refill
   while the gun is holstered, and does the drop in damage feel fair?
2. **PRIMED:** hit with the KESTREL, swap to the THUNDERCLAP and fire within 1 s (the primed window). Do you notice
   the primed shot hit harder?
3. **CROSS:** the same swap: does "CROSS" pay in the style meter (it should pay 8), and is it readable in the fight?
4. **Momentum:** dash, then punch or swing the GAUNTLET at full speed. Does the ring round the crosshair fill with
   speed, and does a fast hit's ring wave and short freeze feel strong enough? Too strong?
5. **The V lock:** press V with two husks in view, V again, then V with one. Does it lock, cycle and release where
   you expect? Does the lunge on a punch just out of reach feel helpful or grabby?
6. **The perks:** buy BOUNTY (near the start, at (15.4, 0, 47.3); the spawn is (0, 0, 40)), DEADEYE (22, 0, -22),
   FLOW (-40, 29.2, 40), SLIPSTREAM (-25.5, 0, 32.5), FLIP (20, 0, 47.3). Which one would you skip, and is
   SLIPSTREAM's longer slide (x1.25, less friction) worth its price?
7. **Overall:** after one run to round 5, does the game feel faster to play than on Oct 2?

## Decisions waiting for you

Every one is in HANDOFF.md section 5 with options and a recommendation. New tonight: **D-16**, the co-op scaling
numbers (spawn budget, alive cap, elite / boss HP by living players; solo stays exactly as now). D-10 gained three
proposed CLAUDE.md lines. None of them blocks NIGHT10.

Next: `NIGHT10.md` (players and projectiles: STITCHER, COMET, LONGWAVE, homing, the rest of `Player[]`).
