# NIGHT57 REPORT (for GG; SHIP AND DEEPEN, 2026-10-21 09:34 - 14:3x MST)

You couldn't play, so the night started from a gap analysis (Research/2026-10-21-wave57/GAPS.md: game-design research with URLs, a
GitHub scout, our own numbers). Its main finding: most of what the research recommends was already BUILT and switched off, the late
levels were thin (levels 3-5 had 3 combat rooms each), and no judge had ever been checked against your taste. So tonight: a new shipping
rule to get built work to you, level 3 rebuilt from scratch, enemies that come at you, and a calibrated judge. Three builders (L level 3,
G enemies / kills, V colour) and one batched critic per build. No usage-limit stop this time.

**Your play build: `Builds/n57-freeze`** (tag `u3-n57`). Fallback: `Builds/n57-r1` (tag `u3-n57-r1`). NIGHT56's is `Builds/n56-freeze`.

## The new shipping rule (your "you decide")
A feature turns ON when its main number beats the old version by more than the run-to-run noise, none of its guard numbers gets worse
beyond a band, nothing is lost (verbs37 on every level), and a fresh critic confirms the evidence is real. Your play is the veto: every
ON row below has an off switch. Result: **7 rows ON tonight** (NIGHT55-56 together shipped 2 under the old "every line passes" rule).

## What is ON in n57-freeze
| feature | what you should notice | measured (bot = the walk / boss pilot, not a person) | off switch |
|---|---|---|---|
| **Level 3 rebuilt (kiln57)** | THE KILN is five new combat rooms, each built from an identity card: kb2 THE CHARGING FLOOR, kb2c THE CLINKER BRIDGES, kb3 THE PRESS HALL, kb3c THE TAP RUN, kb3d THE CRACKED FLOOR. The worm, its arena and its lead-in beats are unchanged | rooms 3 -> 5 (all new); level time 92 -> 168-172 s; stall 25.5 -> 19.0 %; deaths 8 -> 2 per 10 walks; verbs37 31 / 31 | `-skygrindOff57` |
| THE KILN WYRM pacing (wyrm55) | fewer deaths, a shorter tail | deaths per fight 1.70 -> 0.80; median 122.6 -> 86.8 s; worst 10 % 256.9 -> 178.3 s | `-skygrindOff55` |
| Enemies come at you (tokens56) | enemies attack from where you can see them; idle ones walk into view | attacks that start on screen 44.7 -> 85.0 %; deaths down; level time +2-8 % | `-skygrindOff56` |
| Short lessons (brief55) | the lesson lines are short and name your button; the card never covers the room counter | every line passes; overlap 1 frame -> 0 | `-skygrindOff55` |
| THE WICK in windows (wick55) | more of the fight is punishing its open windows, less chip | damage in windows 6 -> 57 % (part of that jump is the row counting the bare core as a window; on one measure chip still falls 51 -> 43 %); deaths unchanged | `-skygrindOff55` |
| Boss palettes (grade55) | THE WICK's grade + hot wall; THE CROWN's ring tells now kept | CROWN tells kept 0.68 -> 0.99; WICK ride panels carry the cyan cue (13 %) | `-skygrindOff55` |
| Level colour (colour56) | each level has its own colour key and accent; the world never out-shines the danger colours | key / accent on 36 / 36 frames (22 off); world over the tells 3 / 36 (NIGHT56's version 32 / 36); greyscale and colour-blind checks hold | `-skygrindOff56` |

## Look at these first
- Level 3: play it (it is level 3 in the build). Frames: `Logs/feel/n57-freeze/l2on/a452/` (each room's entry / mid / high).
- Two honest weak spots in level 3 (not gate lines): at the door, 3 of 6 enemies are visible in kb3c and 2 of 5 in kb3d; and the bot got
  stuck at doors more (the harness moved it on 15 / 17 times vs 3 / 4 on the old kiln): the new rooms' exits may be hard to find. Not
  checked: whether a person rides onto the 4.5 m tiers in kb2, kb2c and kb3.
- THE KILN WYRM's best fight is now 27 s for the bot (under your 1-minute floor): too quick?

## Built and OFF (why)
- **press57** (standing still loses: a spitter's volley marks the spot you stood on with a red-orange circle): the bot's stall did not drop,
  because its "still" steps are mostly the bot blocked with the stick pushed, not choosing to stand; kiln deaths rose with it alone.
- **kill55** (hit marks, chained-kill holds): the chain holds work, but the frame after a hit is 97 % white flash, so the marks never show.
- **rim56** (enemies brightest): 75 % of sightings (90 % wanted; foundry 29 %). **yellow56**: one real non-parry yellow frame on foundry.
- **rank56**: retired (it never changed how often you leave DULL).

## The judge
A critic run blind outside the repo picked your "awful" build (n35) over your "a little bit better" build (n37), in both orders. So no AI
decided taste tonight; critics only checked that evidence was real. (Caveat: the frames were stills of different rooms;
Research/2026-10-21-wave57/calib.md.) Next: calibrate on played reels of the same room.

## Fixed along the way
- The Editor's wedge this morning (a stuck test run) and the suite runner: it now waits for an idle Editor and never re-runs a timeout.
- A teach-text checker that failed every keyboard line; verbs37's staging that tested verbs before the level hands you its gun (D-16).
- T-137: a test fixture switched NIGHT36's blocks off for every Game test (rank56's test "failed" in the Editor only).

## Your calls
- Level 3: is it the "new arena that encourages far more mechanics" you asked for? (If not: `-skygrindOff57` brings back NIGHT56's kiln.)
- THE KILN WYRM at 27 s for a perfect fight.
- press57's circle as a tell: want it tried with a different measure (a person's play, not the bot's)?

## Files
NIGHT57.md (the prompt), NIGHT57-LOG.md (close table), DECIDE57.md, Research/2026-10-21-wave57/ (GAPS, calib, gate-r1, gate-r3, gate-r3b,
builder notes L / G / V), Logs/n57/.
