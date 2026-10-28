# NIGHT47: what changed after your playtest (GG's page)

**Play build: `Builds/n47-r3/SKYGRIND.exe`** (the night's freeze, port/stage3 c75fbc6c, tag `u3-n47`): every verb check passed on
levels 1-5 (26 / 20 / 25 / 30 / 30) and the smoke passed ("DONE: all steps passed", no errors). Every change tonight switches off with `-skygrindOff47` (that is NIGHT46's game). The full list of your 45 asks
with each one's state is `Logs/n47/ASKS.md`: **26 GREEN, 19 SHORT, none skipped**.

One honest note first: the night ran 22:46-07:16, but nothing happened from 03:28 to 06:28 (the usage limit stopped the session until a
re-login). The four builders had their work committed before that; the last 45 minutes merged it, turned it on and checked it.

## Level 1: THE WICK
- **Three phases.** P1 on a round floor (the climb panels folded into the wall); at 45 % the panels and steps swing out and the fight
  becomes the climb; at the top a **glass floor slides over the shaft** (you can't fall back down) and P3 starts.
- **The climb.** The bottom is now five plain steps, each 1.4 m (half the double jump), no wall-ride needed. Then 8 ride panels. A wall jump
  off a climb wall now SETS your upward speed instead of adding to a falling one, with a little coyote time and a jump buffer: the
  "slipping further down each time" is gone (five wall jumps gain the same height). A miss costs one section at most. A sloppy bot reaches
  the top 39 times in 40, in about 30 s. **SHORT:** the top third still punishes a late release (58-78 % first tries, target 80 %).
- **Its HP isn't the problem any more:** outside a window your shots now do 35 % (was 10 %), so 10 s of pistol fire moves the bar 17.5 %
  (was 4.4 %). A human-like bot kills it in a median 95 s (was 72 s, and with guns alone it never died).
- **New attacks:** slag geysers from floor vents, an ember curtain when you camp a ledge, a spinning plate ring, and P3's **crack-open
  charge**: it charges, cracks open, the core goes yellow with a ring clock and arrows on the glass, **"Punch the core!"**; a punch takes
  22 % of the bar and throws you ~15 m back with slow motion and a hard camera hit.
- Built but OFF (not seen in a Player yet): a parry from a wall-ride cracks two plates; a name card at the start; a finale beat.

## Level 2: the CHARGER and THE ICEBREAKER
- The CHARGER now shows its path on the floor, **locks it in the last 0.25 s**, and can barely turn: dodge late and it misses, dodge early
  and it follows you. It bursts anything in its path; it stuns on walls with its back vents glowing yellow (punch it there).
- It appears **in the first room, alone**, then in 3 more rooms (9 in all, was 4 in one room). **THE STAMPEDE** room: 6 chargers + spitters
  on a gallery; your glove does nothing to their armoured fronts, the KESTREL through their backs does triple: glove-only takes 85 s, mixing
  weapons 46 s.
- **THE ICEBREAKER:** three phases with armour that a window cracks; phase 3 sends chargers you can bait into it. A median 92 s fight
  (was 51 s). SHORT: its animation and per-attack hit effects were built but not judged on frames.

## Level 3: THE KILN WYRM
- Longer tells that say where (a crack in the floor before it dives, a glowing trail while it's under you, a circle where it'll breach),
  the burrow no longer cancelled by its charge, slag islands only the shotgun boost reaches, the molten yellow-red-orange colours on the
  blocky worm you chose, and **three appearances before the fight** (a glimpse through a window, a pass under a hall, a cameo where it
  breaches and teaches "When the floor glows, jump").
- With the boost (as you play it) the fight is a median 97 s. **SHORT:** its head charge still has a 0.5 s tell (every longer one made the
  punch miss), the burrow is 1.5 a minute (target 2), and the islands don't yet double how often a player boosts.

## Level 4: THE LOOM
- A new arena: the loom hangs in its frame over a pit; a **net of its own threads catches you** (falls that restart dropped from 21 to 3 a
  fight). Hook a lit knee: slow motion, shoot the yellow seam, tear it. Tears and parries refill the BEND, so the slow motion keeps coming.
  7 attacks over 3 phases; it cuts its own frame posts at phase 2. A median 92 s fight (it often didn't die before).
- **SHORT:** its new bronze-and-brass shuttle body was committed after the last frames I looked at.

## Level 5: THE CROWN and the level
- The level has a story now (a cathedral on a chasm; the crown of blades fell into the bell shaft), and every room's far end shows a lit
  way on (seen from 10 of 10 rooms, was 0). **THE CROWN:** 8 attacks, a real flame (a dense stream with smoke, red-orange), falls caught on
  the floor below instead of restarting, a median 92 s fight (was 44 s), deaths down from 38 to 8 per 40 fights.

## Everywhere
- **The pink door is fixed at its cause** (the door's shimmer kept a deleted material after a level change: 200 passes clean on levels 1-5).
- **Lava light:** fixed per field, no more popping (the moving lights now fade).
- **HUD:** one continuous health bar, green -> lime -> yellow -> orange -> red, **blue to purple overheal** (close kills fill it up to 150,
  it drains back slowly), a bigger "6 LEFT / LAST ONE / CLEAR" counter, the reticle with nothing under it (the charge and the PULSE pips are on
  the gun and the weapon panel now), fight text one line at a time at readable size, and a TEXT SIZE option.
- **Words:** 75 lines rewritten to sound like a person (docs/COPY47.md), e.g. the gauntlet: "Punch with {fire}. {altFire} parries, and
  after five kills it blasts."
- **Weapons:** the LT blast throws a husk 14 m (was 1.4 m) with pink shockwave rings; the THUNDERCLAP's LT is a real shouldered zoom; the
  grapple is one glowing line; punches glow cyan-blue (fist) / pink (gauntlet) / red-orange (enemies); hit sparks, kill flares, tracers.
  **SHORT:** the blind pairs between look variants were not run; the screen distortion and heat haze were cut (a new shader is the kind of
  thing that turns pink).
- **Enemies:** sharper angular visors, the WARDEN's shield held on its chest, the small swarmer redesigned as a pill on four pointed legs.
  **SHORT:** built, but I did not look at their frames.

## What to look at first
Level 1's boss (the climb and the core punch), level 2's first room and THE STAMPEDE, then any door after a level change.
