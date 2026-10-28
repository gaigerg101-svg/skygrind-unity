# DECIDE26.md: questions only GG can answer (defaults act; blank Answer = default)

| id | question | default | Answer |
| --- | --- | --- | --- |
| D-1 | The easy first boss | THE SLAG HULK (new, 1500 HP, a slam ring and a charge, back / head weak points) | |
| D-2 | Level order | 1 FOUNDRY (tutorial + platforming + HULK), 2 ICE LINE (chase, pistol only), 3 THE KILN (worm) | |
| D-3 | Weapon unlock order | THUNDERCLAP after level 1, NAILGUN after 2, RATTLER after 3, then ZAPPER, RAIL, BOUNCER, ROCKET (data in unlocks.json) | |
| D-4 | PULSE blast | LT hold charges by use (hits), release launches + small damage, 3 s lockout; the click is a plain swing; R3 punch | |
| D-5 | Rank formula | TIME / KILLS / STYLE D-S per level; aggregate S5 A4 B3 C2 D1 minus 1 per checkpoint restart; P = S in all three, no restarts | |
| D-6 | Ice friction | `groundFrictionMult` 0.25 on ice; boost strips push 6 m/s through `Mover.Impulse` | |
| D-7 | Worm damage split | body x0.5, head x2, core x3 (config); proximity aggro radius 14 m, cooldown 6 s | |
| D-8 | Rocket and BOUNCER nerfs | rocket damage -30 %, splash radius -35 %, self-damage on, reload 2.2 s; BOUNCER fuse 2.4 s, damage -35 %, pool 3 | |
| D-9 | Survival modes | keep BIG PICK and the mystery box; the campaign has neither | |

