# NIGHT15-REPORT: map night 1, for GG (2026-10-05, 11:14 AM - 1:30 PM)

Your asks were a map overhaul with personality, aim assist that's too strong, and controller support (Steam, hopefully
Switch). All nine items closed (`NIGHT15-LOG.md` has the close table). The old arena plays exactly as before: its
fingerprint is EQUAL, golden 0 mismatches, 724 / 724 tests pass.

## 1. The map: three concepts, one recommended (MAP-DESIGN.md; **D-59 is yours**)

- **DRYDOCK (recommended): "Finish the ship, or fight on its bones."** A cliffside sky-shipyard at dusk. The hub is a
  training ring around an unfinished hull with a 64 m mast, and each side zone is one verb built into its geometry: the
  RIBS (a wall-ride canyon), the SLIPWAY (40 m slide ramps with a pad back up), the BOILERWORKS (close quarters under a
  4.5 m roof), the GANTRY (ARCLINE between cranes). The PROTOCOL relays are the ship's launch sequence.
- **TETHER: "Something came down the cable. Climb up and cut it."** Tiers stacked around a space-elevator cable. The
  purest movement map, but hard to read, and the walkers can't reach the high tiers.
- **THE DIAL: "The sun won't set until you hold every hour."** Eight wedge zones around a sundial whose face is the old
  core grid. The best home for the core, but radially symmetric: the old map's blandness again.

The plan: `Logs/shots/n15-plan.png`. Every distance is sized from the game's real movement, measured from the web's own
trajectories (a dash-jump carries 21.2 m, a wall-ride climbs 7.6 m, a wall-jump carries 12.2 m).

## 2. DRYDOCK's heart is playable (greybox)

Pick **MAP: DRYDOCK** on TITLE. Four zones are built: the GANGWAY, the CRADLE, the RIBS and the SLIPWAY.
- **Measured on the real Mover:** the slide down the slipway peaks at **28.8 m/s**; a rib wall-ride lasts its full
  1.2 s and climbs 7.7 m; a slide-jump off the kicker lands on LAUNCH RAIL (3 of 6 take-off points); a dash-jump from the
  deck's lip reaches the hull deck (3 of 4 timings); every pad lands within 7 cm of its target.
- **The AUTOPILOT** clears 5 rounds on it in 300 s, twice, with identical hashes. The validator finds 0 problems.
- **To edit it:** open `Assets/Scenes/Maps/drydock-authoring.unity`, move a marker, then `SKYGRIND/Map/Export`. Or
  change `Design/maps/drydock.plan.json` and run `SKYGRIND/Map/Build DRYDOCK`.
- **What it looks like** (`Logs/shots/n15-b3/`):
  - `n15-drydock-above.png`: the plan, in the engine.
  - `n15-drydock-hub.png`: the hull's arches and the mast from inside the ring.
  - `n15-drydock-landmark.png`: the mast from 66 m away.
- **Honest problems:**
  - Everything is the same purple stone.
  - The mast hides once you're inside the rib alleys.
  - The hub is mirror-symmetric.
  - The slipway's slope barely reads.
  - Night 2 builds the other four zones; night 3 is the look.

## 3. Aim assist: calmed

- **New defaults:** the mouse is **OFF** and the pad is **LIGHT** (the web's mildest mode); strength is unchanged.
- **Your profile on this machine:** your saved STRONG (the old default) moved to the new defaults once. Pick STRONG again
  and it sticks.
- **SETTINGS:** the rows now read AIM ASSIST: OFF / LIGHT / STRONG / LOCK.

## 4. The pad

- **Weapon wheel:** hold **Y** for 0.25 s, point with the right stick, and let go to take that weapon. A tap of Y is still
  NEXT WEAPON (now on the release). The smoke drives a whole run, the wheel, PAUSED and SETTINGS with a virtual pad and no
  mouse.
- **Nintendo glyphs:** picked by the pad's name.
- **Rumble:** on by default, with a RUMBLE strength row.
- **PLATFORM.md:** a checklist for Steam, the Steam Deck and Switch 2, with 11 decisions for you.

## 5. Questions for you (none of them blocks night 2)

1. **D-59:** DRYDOCK, TETHER, THE DIAL, or a mix? Night 2 (`NIGHT16.md`) builds DRYDOCK's other four zones unless you
   say otherwise.
2. **The zone verbs** (MAP-DESIGN.md section 3.1): are the restrictions right? The ribs have no ARCLINE anchors; the
   boilerworks has a 4.5 m roof.
3. **The old arena:** keep it as "ARENA" in the MAP row, or make it an ENDLESS map whose core reshapes every wave (the
   Cyber Grind)?
4. **The run rules in MAP-DESIGN.md section 6 (Proposals):**
   - the PROTOCOL opens THE FORGE
   - finishing the PROTOCOL turns the hull deck into the exfil (the ship launches)
   - per-run variation from the seed
5. **PLATFORM.md's 11 decisions**, the most pressing two:
   - Steam Input: opt PlayStation and Nintendo pads out first?
   - Set the company and product names before any public build: they move every player's save folder.

**One open risk:** the smoke's NIGHT14 residual (steps 18 and 21) failed on builds 1 and 3 and passed on build 2. It's
intermittent and still not understood.
