# WEAPON-IDENTITY.md (NIGHT26, builder W)

GG, 2026-10-08: "almost all the weapons should really have a left trigger that either zooms in or does something else ... every single weapon has a positive a weakness and they work in certain scenarios ... you really don't feel incentivized at all to switch weapons ... the grenade and rocket launcher are a little too broken".
Switch: `design.n26.weaponIdentity` (LT rows, vulnerabilities, the rocket / BOUNCER cost) and `design.n26.pulseLT` (the PULSE's blast). Off (`--off26`): NIGHT25's arsenal. Numbers: `design.n26.weaponIdentity.{identity,weapons,vulnerability}` in `Design/unity-config.json`, each with a `_why`.
Only the guns Unity fires are listed (revolver, gauntlet, shotgun, nailgun, rocket, rail, zapper, bouncer, torch, vortex). The RATTLER, DISCUS, CHOIR, PRISM, SUNDER, FLICKER, THUMPER and TETHER are not ported (`Loadout.Fires` is false): no LT to build.

## The table

| gun | strength | weakness / cost | LT | beats | bad against |
| --- | --- | --- | --- | --- | --- |
| KESTREL (revolver) | accurate hitscan, headshots x2, the ricochet when zoomed | one body per shot; low damage | zoom 1.5 (the web's). A charged shot was in the brief: NOT built tonight | CHARGER in its wind-up (x1.5), DRONES | SWARMERS (single shot x0.6), WARDEN shield (x0.2) |
| PULSE GAUNTLET (gauntlet) | melee, its hits charge the blast, breaks the WARDEN's shield whole | range 2.4 m | **the blast**: hold LT (it swings), the landed hits fill 3 notches (7 hits each), release launches 0.5 / 1.0 / 2.25 jumps along the aim with a little damage; 3 s lockout, one air flick, capped on the collider | WARDEN (shield ignored), husks in reach | DRONES (out of reach), SWARMERS (single x0.6) |
| THUNDERCLAP (shotgun) | close and fat, falloff past point blank | weak at range; no shockwave (NIGHT25) | zoom 1.25 | SWARMERS (spread x1.2), husks close, CHARGER wind-up (x1.5) | DRONES at range (no damage past the falloff) |
| STITCHER (nailgun) | sustained mid-range, nails stick | heat then a lock; single target | **detonate** every stuck nail (was SPECIAL; zoom 0) | SWARMERS in bursts with a detonate | CHARGERS, WARDEN |
| COMET (rocket) | big splash, rocket jumps | **damage -30 %, blast radius -35 %, a 2.2 s reload, its blast hurts you (30 HP at point blank)** | steer in flight (hold) | SWARMERS and husk packs | DRONES (slow shot x0.5), CHARGERS |
| LONGWAVE (rail) | pierces everything in line, breaks shields | 3 s cooldown, charge | zoom 3.5 scope (the overcharge stays on SPECIAL) | WARDEN, DRONES (x1.25), CHARGER wind-up (x1.5), lines of bodies | swarms (one shot per line) |
| ZAPPER (zapper) | chains between bodies, shocks | heat; low damage per body | **overload**: discharge every shocked body in sight (zoom 0) | SWARMERS (chain x1.2), DRONES (x1.25) | WARDEN, CHARGERS |
| BOUNCER (bouncer) | lobbed grenades, the grenade jump | **damage -35 %, fuse 2.4 s, 3 in the air** (was 12) | press: airburst the live grenades; hold with none in the air: **cook** the next one (up to 1.6 s into its fuse) | clumps behind cover, SWARMERS | DRONES (slow x0.5), moving targets |
| TORCH (torch) | burning area | heat | fire wall (NIGHT20) | SWARMERS (area x1.2) | DRONES, CHARGERS |
| VORTEX (vortex) | pulls bodies together | needs a follow-up | tap collapse / hold repel (NIGHT20) | SWARMERS (area x1.2), crowds before a blast | single tough bodies |

Multipliers are `design.n26.weaponIdentity.vulnerability` (class single / pierce / spread / chain / area / slow; per-kind rows; the CHARGER's `windup` row; the WARDEN: front hit x0.2 instead of blocked, whole from rail / gauntlet / punch).

## Final numbers (harness `weapons`, THE RANGE's aim model, 3 seeds, score = clear seconds + 0.05 x HP taken, cap 30; the WARDEN is not spawnable in THE RANGE: n/a)

First numbers (rocket x0.7 damage, x0.65 splash, 2.2 s reload; bouncer x0.65 damage): rocket ROOM 45.7 s, bouncer 49.5 s against 11-20 s for the rest (4x): useless. GG: "not broken, not useless". Retune (config only): rocket x0.8 damage, x0.8 splash, 1.4 s reload; bouncer x0.8 damage, x1.31 splash, fuse 2.4 s, pool 3; the DRONE's `slow` multiplier 0.5 -> 0.9.

| gun | K-husk | K-swarmer | K-charger | K-drone | ROOM |
| --- | --- | --- | --- | --- | --- |
| revolver | 3.2 | 4.4 | 26.5 | 9.8 | 17.9 |
| shotgun | 3.7 | 6.1 | 21.8 | 64.0 | 26.8 |
| nailgun | 9.6 | 3.6 | 20.0 | 17.6 | 23.4 |
| rocket | 5.8 | 4.2 | 60.8 | 14.5 | 27.2 |
| rail | 4.4 | 7.5 | 5.7 | 6.4 | 11.2 |
| zapper | 3.4 | 0.8 | 35.0 | 3.0 | 13.5 |
| bouncer | 5.9 | 2.8 | 11.5 | 51.0 | 26.3 |
| torch | 5.9 | 1.2 | 18.5 | 70.0 | 50.5 |
| gauntlet | 15.4 | 6.6 | 43.8 | 70.0 | 51.2 |

Median ROOM of the other seven guns 23.4 s: rocket 1.17x, bouncer 1.12x (band 1.0-1.5x); neither is the fastest on any kind row; no gun wins every row (zapper 2 of 4). With the identity row OFF the rocket is the fastest on 2 of 4 rows and 0.56x the median of the rest (the RED side). The bouncer's room time is a cliff on its splash (1.29 to 1.6 swings 26 to 9 s on 3 seeds): re-run `weapons` after any change to it. The bot under-uses lobbed shots on drones (bouncer K-drone 51 s).
