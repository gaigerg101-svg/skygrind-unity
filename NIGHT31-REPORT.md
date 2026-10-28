# NIGHT31 REPORT (for GG): bosses you cannot camp, a grapple that pulls

**Outcome:** everything you asked for is built, switched ON, and checked in a Player build (n31-r5, the freeze), with frames looked at: the grapple pulls you in, the charge shot is back, every weapon's LT does something, level 1 is the pistol only with one new weapon per level, THE STAG replaces the level-2 boss, and the Wyrm, Lantern and Loom have new anti-camp rules. The camp test (four or five scripted campers per boss) ran in the Player on THE STAG: 3 of 5 campers lose; the two that survive (island top, half a lap away) also deal no damage, and a shard-damage discrepancy there is not solved. Nobody has played it with hands. The run was paused for about two hours by the usage limit (12:55-14:51), so the last round of boss tuning did not happen.

**Why the charge shot was gone:** NIGHT27 built it, then switched it off the same night because the automated smoke test failed with it on. Cause (found tonight): the charge fired only when you let go, so a trigger held down (the smoke's virtual pad, or you holding fire) never fired again. Now a full charge you keep holding fires itself 0.5 s later, then the trigger auto-fires. The smoke passes every step with it on.

**The grapple (level 4):** TAP = pull in, straight at the ring at 44 m/s; on arrival you keep about a dash of speed (12 m/s; uncapped, one tap crossed every gap in the level) plus a small hop. HOLD = the old swing. A jump or dash mid-pull keeps full speed. A cyan diamond shows the ring the hook will take. Measured: a tap moved you 0.0 m/s toward the ring before, 44 m/s now; 3 of 3 pulls arrive in the chain reel; room 4.6 was rebuilt so TAP, RIDE THE WALL, TAP AGAIN is the fast line.

**Weapons (RT tap / RT hold / LT):**
- KESTREL pistol (level 1): shot / a charged shot that pierces 3 bodies, 4 s cooldown / zoom 1.5x.
- PULSE gauntlet (level 2, picked up in 2.1): punch / heavy swing / a PARRY (a quarter second: a boss orb, shard or spit flies back cyan and hurts the boss) until five punch kills earn the BLAST.
- THUNDERCLAP shotgun (level 3): blast / a single-line SLUG, 6 s cooldown (no shockwave) / zoom 1.25x.
- NAILGUN (level 4): stream / - / zoom, then DETONATE once nails stick. A charged pistol shot through a body full of nails sets them off.
- Order changed from my prompt: level 2 gives the gauntlet and level 3 the shotgun (level 2's rooms and its tank lesson were built around the gauntlet).

**The bosses (Player, freeze build):**
- THE LANTERN: only a charged pistol shot fired from a wall panel breaks its shield; the platforms sink into the lava at half health. Pilot kill 25.1 s: now too easy (band 45-75 s).
- THE STAG (new): a charging ice ram. Stand still or run far and it charges; dodge sideways on the ice; if it slams a wall it is stunned with its back open: punch it. Pilot kill 115.4 s (band 80-130), 12 wall slams, 7 back punches.
- THE KILN WYRM: one charge in three is a feint; guns barely hurt it unless its head was punched first (then the shotgun slug); stand still 3 s and it spits; platforms crumble at half health. Pilot: 5 staggers, 69 % of its health in 240 s (slower than the 3-4 min band).
- THE LOOM: tap a joint to zip and hang; only the nailgun near a joint does real damage; hang over 3 s and it shakes you off; stand on the deck 3 s and a strike drops on you (a deck camper died at 18.3 s); the deck's middle breaks at half health.

**Also fixed:** no enemy spawns beside a door (32 spawn points moved); the low-health red only at the screen edges; an old room's lesson no longer lingers; a level-1 stall (room 1.8 never cleared once the pierce dummies went in) found and fixed; at the freeze the level-select check found that a new pickup row broke level 2's lighting and props view (an error every frame): fixed, and all 20 level-select presses pass on the final build (n31-r7) with 0 errors.

**Switches:** every change is a `design.n31.<row>` in Design/unity-config.json; `-skygrindOff31` gives NIGHT30's game.

**Not verified:** a human playing it; THE STAG's two surviving campers (shards register hits but no damage: unexplained); camp tests in the Player for the Lantern, Wyrm and Loom (harness only); the Loom's hip joints (too low to hang; the knees work); the grapple's 0.2 s tap window (a guess); final suite and smoke lines are in the close table.

**Felt worse / risks:** the Lantern is now short; the Wyrm is long; the parry window may be too tight; the charge hold may feel late.

**DECIDE31 questions that matter:** D-2 THE STAG instead of the Glacier, D-3 tap to pull / hold to swing, D-11 the pull's 0.4 s cooldown, and D-1 (I changed the weapon order).
