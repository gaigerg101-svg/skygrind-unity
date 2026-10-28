# PLAY74.md: what changed tonight and how to compare it (NIGHT74, ROOMS)

Build: Builds/n74-play (made at the close). Each line: the change, where to see it, its off switch, keep / drop.
To compare a level's rooms with the old ones: `SKYGRIND.exe -skygrindRows72 <row>:0` (for example `kiln67:0`). A row: `-skygrindRows72 <row>:0`.

## Rooms: your NIGHT71 rooms are back, and 9 more rooms were rebuilt tonight (smaller, raised, a reason to move)
- [ ] keep [ ] drop **Level 1 (foundry65):** your NIGHT71 rooms b1 THE YARD (launch pad at the entry, a raised deck) and a8 (the wall-ride
  corridor), the 1.3 RIDE roof gone; rebuilt tonight: **a2 THE RANGE** (a free cartridge stack you can loop, the ramp up to the deck; its
  floor mark now cyan), **a5 THE POUR** (two pads back, a crate that keeps the wall-ride test honest) and **a3 THE RUNNER** (roofless, your
  1.3 RIDE line kept over its new ramp, less lava, low blocks to hop, a hoist pad). Off: `-skygrindRows72 foundry65:0`.
- [ ] keep [ ] drop **Level 2 (iceline67):** rebuilt tonight: **g1** (a ride face, a gallery tier, cover on a floor that had none), **g4 THE ORB
  YARD** (your NIGHT71 room with its ice raised into a stage with two ramps), **g6 THE ARMOURY** (a shelf with a lift pad, rack and cover),
  **g2 THE SHIELD** (a flank wall-ride onto a gantry, tiers, THE ANVIL), **g7 THE MIX** (no lava; a sluice wall ride, a tower, a stage),
  **g3 THE TIPPLE RIDES** and **g5 THE CRANE YARD** (NIGHT71's versions, fixed so they pass), **g8 THE STAMPEDE** (a gallery, a signal box,
  hoists; the lava pool gone). Every fight room on level 2 is new. Off: `-skygrindRows72 iceline67:0`.
- [ ] keep [ ] drop **Level 3 (kiln67):** your NIGHT71 rooms kb2, kb3c, kb3d; **kb3 THE PRESS HALL** (your room, its entry opened: the first
  wave stands in view from the door instead of a wall of presses), plus **kb2c THE ASH FLOOR** rebuilt tonight (raised plates, the kiln hood,
  low cover). Off: `-skygrindRows72 kiln67:0`.
- [ ] keep [ ] drop **Level 5 (spire67):** your NIGHT71 room v6, plus **v5 THE BELL WALLS** (a bell stage and ramp; its wall ride now works)
  and **v2 THE NAVE** (three deck levels, cover; the first wave stands across the crypt so you see it from the door). Off: `-skygrindRows72 spire67:0`.

## Bosses
- [ ] keep [ ] drop **weak74 (rule W):** on a boss, cyan now means "shoot here" only: THE CROWN's ring is ivory while shut, cyan when open.
  Off: `-skygrindRows72 weak74:0`.
- [ ] keep [ ] drop **target74:** when a boss's weak point is open but off screen, the bar reads "OPEN 2.9 - BEHIND" (or LEFT / RIGHT /
  ABOVE) and a bold cyan arrow points at it. Off: `-skygrindRows72 target74:0`.
- [ ] keep [ ] drop **kill74:** the boss kill flashes the boss white for two frames and holds a beat (0.35 s) before the zoom. Off:
  `-skygrindRows72 kill74:0`.

## Enemies, light and sight
- [ ] keep [ ] drop **spring74:** a hit squashes the enemy down and wide for three frames, then it springs back. Off: `-skygrindRows72 spring74:0`.
- [ ] keep [ ] drop **reveal74:** when the counter says N LEFT and none are in view for 3 s, a red-orange double arrow at the screen edge points
  at the nearest one. Off: `-skygrindRows72 reveal74:0`.
