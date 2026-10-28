# NIGHT47 builder B: research notes (levels 2 and 3: the CHARGER, the stampede room, THE ICEBREAKER, THE KILN WYRM)

Written 2026-10-18 ~23:05-23:30 by builder B (round 0). Each line is marked **Verified** (I read the page tonight, or NIGHT47.md section 10
says the lead read it) or **Reported** (a search snippet, a secondary page, or a page that refused me). Pages that refused me are named, so
the next reader does not try them again.

## R-B1 Chargers in other games (the CHARGER foe card, G26)

- **Doom 2016 Pinky** (doomwiki.org, Verified by the lead, NIGHT47.md section 10; doomwiki refused me with HTTP 403 tonight): an armored
  front, a vulnerable back and sides, poor turning mid-charge, stunned by walls, can be led off ledges.
- **Helldivers 2 Charger** (helldivers.wiki.gg/wiki/Charger, **Verified**): it roars before charging, then keeps going until it hits a
  player, solid terrain or another Charger; charge speed 9 m/s; heavy front armour (the head 1,200 HP, torso 800 HP), the unarmoured rear
  abdomen takes 150 % to main health; a miss does not stop it: the post-charge recovery was lengthened by one second and its turning arc was
  widened "to make it easier to avoid"; ramming a large wall or rock stuns it briefly; the counter is to move perpendicular (or diagonally
  towards it) late, not early, because it can still turn; after a miss, shoot the rear in its recovery. Takeaways for ours: (1) the
  recovery after a miss is the punish window, and the designers lengthened it on purpose; (2) "dodge late, not early" is the skill,
  which only works if the turn rate is low enough that a late dodge beats it; (3) the back is the weak spot and the front is armour.
- **Left 4 Dead 2 Charger** (left4dead fandom snippet + Valve Developer wiki snippet, **Reported**: developer.valvesoftware.com refused
  me with HTTP 403): it cannot stop or turn during the charge until it hits a wall or covers its distance; the dev wiki's ConVars list a
  charge duration of 2.5 s and an interval of 12 s. Takeaway: the no-turn rule is what makes the charger a positioning puzzle, and its
  long interval keeps it a punctuation, not a constant.
- **ULTRAKILL Cerberus** (ultrakill.wiki.gg/wiki/Cerberus, **Verified**): its dash from close range does 25 with big knockback and cannot
  be parried; shortly before charging it can turn instantly to face where the player is going; it will not dash off ledges unless enraged;
  when one dies the others enrage (faster attacks and movement). Takeaways: a "commit" moment (the last turn) then no more steering is the
  read; the enrage-on-partner-death rule is a ready-made escalation for a room with 3+ chargers (G27): kill order matters.
- **Monster Hunter World Diablos** (monsterhunterworld.wiki.fextralife.com/Diablos, **Verified**): its charge "can be baited into colliding
  with a wildspire, momentarily getting Diablos stuck". Takeaway: the arena's pillars are the CHARGER's counter (the stampede room needs
  pillars to bait it into).

## R-B2 Burrowing bosses (THE KILN WYRM's burrow and tells, G31 / G34)

- **Monster Hunter World Diablos burrow** (fextralife page above, **Verified**): it "can burrow and traverse underground quickly, attacking
  from different angles", "resurfaces with a powerful lunge"; the page advises reading "visual cues when Diablos is underground" and an
  "audio cue just before he emerges"; loud sounds (a Screamer Pod, Sonic Waves) force it to the surface, and it "can form an immunity" to
  the trick. Samurai-gamers' Iceborne guide (search snippet, **Reported**): "you can tell when it is approaching by the pattern of dust
  spouting from the ground"; forced out early it surfaces in pain, a safe window. Takeaways: (1) the travel under the floor shows as a
  moving surface trail (ours: a crack line of glowing slag that follows it); (2) the breach spot glows before it breaks (ours: 0.8-1.5 s,
  G31); (3) a player action that forces it up early and stuns it is the burrow's counterplay (ours: the THUNDERCLAP's boost slam or a
  slug into the breach glow).
- **The Wind Waker's Molgera** (zeldawiki.wiki refused me with HTTP 403; zelda.fandom HTTP 402): not read tonight; from memory only, so not
  used as a source.
- **Our own old DIVE** (git, Wyrm.cs's history, read tonight: the baseline below): "a floor crack, then it burrows" is the in-house
  precedent GG remembers.

## R-B3 Boss tells, windows and animation (G28-G31)

- **Boss design from a combat designer** (Game Developer, Verified by the lead, NIGHT47.md section 10): wind-ups 1-1.5 s, at most ~2 s
  for heavies; tells distinct and never overlapping; punish windows longer than one combo plus the travel; weak points obvious, exposed
  only in their window.
- **Doom Eternal Marauder** (Verified by the lead): one colour, one moment, one answer (its green eye flash); ours is yellow.
- **ULTRAKILL Gabriel** (Verified by the lead): phase 2 at half health with a colour change, a line, improved versions of the same attacks.
- **David Rosen, "An Indie Approach to Procedural Animation"** (GDC 2014 Animation Bootcamp; gamedeveloper.com and a Unity forum thread,
  search snippets, **Reported**; the GDC Vault video not watched): fluid interactive animation from very few key poses blended by
  procedural springs (Overgrowth). Takeaway for G29: THE ICEBREAKER's view should be key poses (idle, wind-up, strike, recover, stagger)
  blended by the NIGHT5 closed-form springs, never linear lerps between states.
- **Hit feedback tiers** (uhiyama-lab, Verified by the lead): flash 80 ms; hitstop 0-30 / 50-80 / 100-150 ms; restraint on light hits so
  heavy ones stand out (G30: each ICEBREAKER attack's impact gets its own tier).

## R-B4 Encounters that ask for more than one verb (the stampede room, G27)

- **Helldivers 2 Charger** (above, Verified): the front armour shrugs off the default answer; the back is the answer, so the room asks for
  a position (get behind) rather than a weapon.
- **ULTRAKILL Cerberus enrage** (above, Verified): killing one makes the rest faster, so a room of several makes the kill order a
  decision.
- Research/2026-10-17-wave46/arena-flow-web.md Part D (Verified by the NIGHT46 author, read again tonight): chargers on the long straight
  (R-4); never two chaser types at once (point 8); cover is jump-height blocks, one per quadrant (point 5).

## R-B5 Special rounds (round 4's stampede47)
- **Call of Duty Zombies' hellhound / special rounds** (from memory and the lead's list; no page opened tonight, so **Reported**): every few
  rounds the normal wave is replaced or joined by a pack of one fast enemy type that arrives together, announced, as a rhythm beat in the
  round loop. Takeaway: THE CHARGER's herd as SURVIVAL's beat, in the CHARGER's own language (the lit lines, the late dodge, the bursts).

## Not opened
- doomwiki.org (HTTP 403), zelda.fandom.com (HTTP 402), zeldawiki.wiki (HTTP 403), developer.valvesoftware.com (HTTP 403), the GDC Vault
  (members only).
