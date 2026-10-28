# NIGHT10 report: the ARMORY guns, the Mystery Box, the ULTRAKILL flags (`u3-n10`)

For GG, 2026-10-03. You cut the run to 6 hours at its start (10:03 AM); it closed at about noon. Every NIGHT10 item in
`tasks.json` (U3-13.1, .3, .4, .5, .8, .9, .6, .7) is passing, none cut, and U3-13 passes. Branch `port/stage3`, local
tag `u3-n10`. Nothing is pushed or merged. Each item's checks are in `NIGHT10-LOG.md`.

## What shipped

| Commit | What |
|---|---|
| `748ae8a` | **Projectiles + the three guns' sim + homing (U3-13.3, .4, .5).** projectiles.js ported line for line (nails that stick and pierce, rockets with splash, the rocket jump, fragments, steering); STITCHER (heat, detonate on right mouse), COMET (steer on right mouse), LONGWAVE (charge, pierce, overcharge, through walls at speed) as weapons.js fires them; nails and rockets home on your V lock. Two new golden sets from the web (`projectiles`, `weapons`: 2,765 recorded ops), bit-exact. |
| `da23e7c` | **The Mystery Box sim (U3-13.8).** 950 points; CoD's teddy ladder (pulls 1-3 0%, 4-7 15%, 8-12 30%, then 50%); a teddy refunds and moves the box; 3 spots (LANDING, THE GRINDER, SLAB GARDENS) picked by casting the collider; a seeded roll from every ported gun you don't own. |
| `58cc73f` + `5465598` | **In the game:** the three guns on the ARMORY wall with their models and hands rigs; nails, rockets and smoke drawn; the box (a dark crate with cyan trim, a lid, the gun rising and cycling, a teddy); the prompt `HOLD E · MYSTERY BOX 950`; the smoke now rolls the box and kills with each new gun. |
| `7844541` | **The ULTRAKILL flags (U3-13.9), ON per your D-17:** FRESH pays points and style but not damage; style doesn't drain in a break or with nobody near; a parry freezes 0.25 s; a parry refills the dash (the web already did). |
| `1266a39` | **FX (U3-13.6):** LONGWAVE's charge laser and dot, its beam flash, the KESTREL's ricochet line; impact decals on hitscan hits. |

## The numbers (tonight's runs)

- Tests: `368 / 368 passed, 0 failed`; `replays: 22 / 22`; `golden tests: 86 / 86`. NIGHT9 closed at 339 / 22 / 84.
- The Player (built 11:48, 26 s): smoke `[smoke] 32.1s DONE: all steps passed`, 0 exceptions, including `PASS Mystery
  Box rolled ... paid 950, offered shotgun, taken` and a husk killed by STITCHER (0.7 s), COMET (0.5 s) and LONGWAVE
  (0.8 s); `-skygrindGolden` `30 vector sets (30529 records), 22 replays (22 exact); 0 failing, 0 mismatches`.
- Bench, 60 s, 24 husks, 1920x1080, on the Balanced plan on AC: median 2.91 ms, worst 1% 8.69 ms, 309 fps mean.
  NIGHT9's ran on Silent (2.49 / 4.49 ms), so they don't compare. Managed memory grew more (+103.9 MB vs +75.7 MB in
  the 60 s); the cause isn't measured yet (HANDOFF risk; it needs a profiling build).
- Captures: `Logs/shots/n10-box-roll-2.png` (the box mid-roll), `n10-armory-wall.png`, `n10-rail-beam.png`.

## What is partial, and why

- **Box:** no Fire Sale; the teddy only moves to a zone you've opened. A taken gun adds a slot like a wall buy (the port
  has no two-gun limit), it doesn't replace the gun in hand.
- **Guns:** the 12 other web guns and the KESTREL / THUNDERCLAP specials are next (projectiles.js's grenades, discs,
  vortexes and darts are stubbed for them). The model bevels aren't drawn (outlines 2-8 mm smaller).
- **FX:** one decal per THUNDERCLAP shot (the web leaves one per pellet); no Prism preview (no PRISM yet).
- **Player[]'s rest** moved to the co-op night, as the re-plan said.

## Your 10-minute play checklist (`Builds/Stage1/SKYGRIND.exe`)

Keys as before, plus **B: the special** (right mouse is the special too on these three guns). Buy ARMORY (750), then the
wall guns. One question per point; a yes / no and a word is enough.

1. **STITCHER** (1250): hold fire until it overheats. Do the nails and the heat bar read clearly, and does right mouse
   (detonate the stuck nails) feel worth using?
2. **COMET** (2000): rocket a group, then rocket the floor at your feet and jump. Is the blast strong enough, and does
   the rocket jump feel like a tool?
3. **LONGWAVE** (3000): hold to charge (0.6 s) through a line of husks. Does the charge laser sell the wait?
4. **Homing:** press V on a husk, then fire STITCHER off to one side. Do the nails bend onto it enough? Too much?
5. **The box:** at the LANDING, hold E (950). Does the rise-and-cycle reveal read, and is 8 s enough to take the gun?
   Roll until a teddy (pull 4+): does the refund and the move feel fair?
6. **Freshness on style, not damage:** spam one gun for a while. Does it feel better that damage stays the same and
   only points / style drop?
7. **The break pause:** build style, then wait out a break. Is keeping your rank between rounds better?
8. **Parry:** punch a husk's wind-up. Does the 0.25 s freeze and the full dash feel right, or too long?

## Decisions waiting for you

- **D-18:** hard damage and a close-kill blood heal (ULTRAKILL): off tonight; NIGHT11 does it on your yes.
- **D-19:** the research's night order. NIGHT11.md follows it (the run's shape: power switch, special round, exfil,
  pause / save-quit, RATTLER + TORCH); say no and NIGHT11 does PORT-PLAN's POWER guns instead.
- **D-23 / D-24 (new):** keep the ULTRAKILL defaults and the box's numbers after your playtest, or tune them.
- The rest are unchanged (HANDOFF section 5).
