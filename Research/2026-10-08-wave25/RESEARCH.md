# RESEARCH.md (wave 25): how shooters teach, and what it means for SKYGRIND's tutorial, ten levels and boss pass

Done 2026-10-08, four web searches and two page reads (30 minutes box). Every claim carries its source; "us" marks our inference.

## S1. ULTRAKILL's first level is a tutorial with a fixed order of one-thing beats, then a main level of one-new-thing arenas
Source: https://ultrakill.wiki.gg/wiki/0-1 (read), https://ultrakill.wiki.gg/wiki/Levels (search result).
- **0-1's tutorial, in order (8 beats):** punch (break boarded planks) > slide (under two jammed doors) > dash (cross a gap, dodge electric fields) > blood healing (kill three helpless Filth) > wall jump (a tall ledge with a broken ladder) > ground slam (break planks, drop through) > soul orb (a secret) > exit (an elevator). Each beat is a physical obstacle that only that verb passes.
- **The main level (11 sequential arenas):** it hands over the revolver in the first arena against one enemy kind (Filth); a glass floor teaches the charged piercing shot; hazards (grinder pit, turbine) come as terrain; **Strays (the first ranged enemy) appear only in arena 7**; a hallway teaches projectile parrying with melee; hard damage appears in arena 9; the slam shows its offensive use in arena 10; **the boss (a stronger Malicious Face: 25 HP vs the common 15) closes the level behind a skull door**; checkpoints sit between sections.
- **Enemy ladder across the Prelude:** Filth (intro) > Strays (0-1) > Schisms (0-3) > the boss face becomes a common enemy (0-4). One new enemy per level; the earlier boss is recycled as a normal enemy later.
- **Us:** copy the shape: tutorial = a row of single-verb obstacles in one hall each; the first fight has ONE enemy kind; the first ranged enemy arrives deep in level 1; every boss later becomes a regular enemy (the WYRM's tail pieces, the MONOLITH's husks).

## S2. A general rule set for tutorial moments
Source: search results summarising https://gdcvault.com/play/1022444/Level-Design-Fundamentals and https://bugnet.io/blog/how-to-design-a-tutorial-moment-within-a-level (the second page's fetch returned only an index, so its rules are from the search snippet, not read in full).
- Teach in isolation: no enemies while teaching jump and traversal (they distract).
- **Three stages: Exposition (show), Validation (do it safely), Challenge (prove it).**
- A tutorial moment is a designed safe situation inside the level, not a separate screen.
- DOOM's rule: present the problem, then the solution (show the shield enemy, then give the tool that beats it).
- Titanfall 2's Gauntlet is a separate course; it works because every obstacle needs exactly one verb and the course is timed (a built-in reason to replay).
- **Us:** each of our six tutorial stages is Exposition (a prompt line and the obstacle in view), Validation (the safe attempt: the door stays shut until the lesson clears), Challenge (a short gated test: 3 targets, a gap that needs the charged flick). The pistol stage presents the problem first (husks behind glass or a pit) and hands the tool second.

## S3. CoD Zombies teaches by room gating, not by text
Source: search results (low quality: several mirrors); Nacht der Untoten is a few rooms and narrow corridors with no perks and no box, the later maps added perks and the power switch (https://thunderstore.io/c/h3vr/p/Kodeman/CodZombies/v/0.7.0/ and other results).
- The first map is deliberately small and bare: guns on walls and doors that cost points are the only systems; perks and the power switch arrive in later maps.
- **Us:** this matches GG's "slowly builds up": the early levels must be small with few systems; the points economy returns only if GG asks (DECIDE24 D-11: no wall buys in the campaign: weapons are earned). Rooms open as you progress (our locked doors), and the pace comes from the doors, not a wave counter.

## S4. Boss tuning: harder by shortening reaction time and shrinking safe answers, never by hiding the tell
Source: search results summarising https://www.wayline.io/blog/crafting-legendary-boss-battles and Game Maker's Toolkit's "How Cuphead's Bosses Try to Kill You".
- Telegraph length scales with the attack's size; fairness comes from consistent tells.
- Difficulty axes beyond HP and damage: more simultaneous considerations, shorter reaction windows, fewer safe solutions; keep the telegraph.
- A good fight teaches recognition on early attempts, execution on mid attempts, mastery on later ones.
- **Us (for "a little harder, clearly aimed at you"):** keep the 47 attacks and the >= 0.5 s tells; (1) lead the target (spit aimed at the player's predicted position over the flight time); (2) lock the lunge line at the tell's start and re-aim once at its midpoint; (3) shrink the safe window by 10-15 % per phase (shorter gaps between attacks, not shorter tells); (4) add overlap in phase 3 (two compatible attacks, never two lethal ones in the same 0.5 s); measure player hits per minute before and after.

## What changed in NIGHT25.md because of this
- The tutorial stages follow ULTRAKILL's order of single-verb obstacles and the Exposition / Validation / Challenge triplet (stage template in CURRICULUM.md).
- The first fight has one enemy kind (husk); the first ranged enemy (spitter) arrives in level 3, not the tutorial.
- Each defeated boss returns as a regular enemy in a later level (recycle).
- The boss pass uses the four tuning moves above and prints hits per minute.
