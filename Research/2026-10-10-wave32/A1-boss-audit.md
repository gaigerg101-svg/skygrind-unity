# A1: boss HP / damage gates and the grapple spam (NIGHT31, HEAD c4c86280)

Read-only audit. Numbers were read from code or config; line numbers (`Lnnnn`) refer to Design/unity-config.json. DPS and TTK figures are arithmetic, not measured. "Realistic" means a 50 % hit rate plus the stated success rate on the boss's verb. Momentum (guns up to x1.35, melee up to x3) is left out. Campaign fights start at round 0, so `BossScale.hp` = 1 (BossMath.cs:117-122).

## Weapons as the sim deals them

| Weapon (level) | Hit | Rate | DPS |
| --- | --- | --- | --- |
| KESTREL (L1) | 25 (weapons.revolver) | 0.4 s | 62.5 |
| KESTREL charged | 100, pierce 3 | 0.5 s hold, 4 s cooldown, auto-fires 0.5 s after full (L2506-2522) | about 22 |
| PULSE (L2) | 32 x 6 = 192 (L1967) | one swing per 2.5 s (L1965) | 76.8 |
| THUNDERCLAP (L3) | 8 x 8 x 2.5 x 1.3 = 208 (L555, n24.shotgunTune) | 0.684 s | 304 at point blank (falloff 5-14 m to 25 %) |
| Slug | 624 | 0.7 s hold, 6 s cooldown | about 93 |
| NAILGUN (L4) | 9 x 0.55 = 4.95 (L559) | 12/s, heat 3 s then lock 1.5 s | 59 peak, about 40 sustained |

The PULSE's 2.5 s cooldown is as long as or longer than every punch window (the Stag's 1.5 / 2 / 2.5 s, the Wyrm's 2.5 s stagger), so each window fits one punch at most.

Levels (design.n30.levels.order; routing in SurvivalSim.Boss25.cs:108, :123-176): 1 n30-foundry THE LANTERN; 2 n30-iceline THE STAG; 3 n30-kiln THE KILN WYRM; 4 n30-span THE LOOM.

## THE LANTERN (level 1, KESTREL only)

**HP:** 1800 (L2013) x lanternMult 0.285 (L1857; SurvivalSim.Boss29.cs:60-65) = **513**.

**Phases:** the bar clamps at 0.66 and 0.33 and the rest of that hit is lost (Lantern27.cs:678-679). Each phase change has a 1.2 s beat with no damage; the intro lasts at least 2.5 s (Hurtable at :223).

| State (Lantern27.cs:641, Lantern31.Scale :73-95, wired SurvivalSim.Boss31.cs:203) | Share | Per shot |
| --- | --- | --- |
| On a platform (the break shot too) | **0** (platformScale L2265; Lantern31.cs:78) | 0 |
| Shield up, on the floor | 0.06 x 0.5 x 0.6 = **1.8 %** (lightDamage L1793, floorScale L2285, lightMult L2637) | 0.45 |
| Shield up, riding or in the air | **3.6 %** | 0.9 |
| Charged KESTREL (100 or more) while riding, or within 0.6 s of leaving the wall | breaks the shield; the core is open **4 s** at 100 % (L2633-2639) | 25 |
| Finisher (last 18 %) | guns **x0.4** (finisherGunMult L2013; Lantern27.cs:639). The x3 bonus is for the PULSE, which level 1 doesn't have. | 10 |

**Hurtbox:** a 2.4 m oriented cube, 3 m up, tested by a ray against the box (Lantern27.cs:594-617). It spans about 9 degrees from the wall at 15 m. Platforms blocking line of sight: not checked.

**Time with no real damage:** about 45 s a minute (core shut 75 % for a human breaking once every 16 s). True zero on the platforms, in the intro and in the beats.

**TTK:**
- Theoretical: 3 breaks of 350 raw each: about 20 s.
- Realistic, half the break attempts landing: 225 per break every 16 s: 55-70 s.
- With no breaks: about 230 s.

**Measured:** Player pilot **25.1 s** (n31-r5); n31-r4 not killed in 200 s with 0 breaks; harness 137.8 s.

The fight is bimodal: easy if you break the shield, close to unkillable if you can't.

## THE STAG (level 2, + PULSE)

**HP:** **1800** (L2657; Stag31.cs:207). One phase, intro at least 2 s; at half HP the banks melt and charges come in pairs.

| Hit (Stag31.Filter :305-338) | Share | Per hit |
| --- | --- | --- |
| Any gun, any part | **gunScale 0.05** (L2713) | KESTREL 1.25 |
| Charged KESTREL on a crystal | 0.05, plus a 1.5 s stun with the back open; the crystal regrows in 15 s | 5 |
| PULSE on the side, or on a shut back | 0.05 | 9.6 |
| PULSE from behind on an open back | 100 % | 192 |
| Parry | 100 % | 18 |

**Windows:**
- The back opens 2 s after a slam, 2.5 s after a miss and 1.5 s after a crystal hit (L2701, L2703, L2729).
- "Behind" means more than 1.5 m behind the centre (:299-303).
- Every window is shorter than the PULSE's 2.5 s cooldown: one back punch per window.

**Hurtbox:** a 3.2 x 6 x 3.2 m box from the ice up, a head sphere of radius 0.78 m and two crystal spheres of radius 0.7 m (:281-296). The box is larger than the drawn torso (Boss31View.cs:370-392), so the problem is the damage gate, not aim.

**Time with no real damage:** about 45 s a minute at 5 %.

**TTK:**
- It needs 9.4 back punches.
- Theoretical: one per 5.8 s charge cycle: about 52 s.
- Realistic, one punch in two windows: about 100 s.
- KESTREL only: about 580 s.

**Measured:** Player pilot **115.4 s** (7 back punches); n31-r4 not killed in 180 s with 0 of 101 punches landing; harness 76.6 s. The two camp-table campers took shard hits but lost 0 % health: not traced.

## THE KILN WYRM (level 3, + THUNDERCLAP)

**HP:** 7800 (L1113) x aim 1.1 (L1234; Wyrm.cs:490) x wormMult 0.7 (L1861; SurvivalSim.Boss29.cs:104-113) = **6006**. Clamps and 1.2 s beats at 0.66 and 0.33 (Wyrm.cs:1293-1311).

**Parts:** body x1 x wyrm26.body.mult **0.5** (L1379), head x2, core x3 (open 3.5 s every 14 s).

| Hit (Wyrm31.Filter :100-111) | Share | KESTREL / THUNDERCLAP / PULSE |
| --- | --- | --- |
| Body, not staggered | 0.5 x **gunScale 0.03** (L2770) = 1.5 % | 0.4 / 3.1 / 2.9 |
| Head, not staggered | 6 % | 1.5 / 12.5 / 11.5 |
| The punch that staggers (0.4 s contact window, aggro charges only) | swing 11.5, plus a 96 bonus (SurvivalSim.Boss30.cs:294-299) | **about 108** |
| During the 2.5 s stagger (L2772) | THUNDERCLAP and PULSE 100 % x part | body 104 / head 416; the PULSE can't swing again in time |

**Bug: slugMult never applies.** Wyrm31.cs:107 looks for "slug" in the weapon id, but FireSlug31 sends `"shotgun"` (ChargeShots27.cs:277), so slugMult 2 (L2774) does nothing.

**Stagger supply:** aggro charges come every 6 s (L2808) and only within 14 m; one in three is a feint that can't be punched. After the punch the recoil throws the head 6 m back and 2.5 m up.

**Hurtbox:** 24 oriented boxes (head 3 x 3 x 4 m) and the core sphere of radius 0.95 m. **Buried pieces can't be hit** (Wyrm.cs:1210); the burrow time per minute: not checked.

**Time with no real damage:** about 54 s a minute at 1.5-6 %.

**TTK:**
- Theoretical: about 55 s.
- Realistic: about 350 s.

**Measured:** Player: 31 % HP left after 240 s (5 staggers); harness 475 s.

## THE LOOM (level 4, + NAILGUN)

**HP:** **2400** (L2411; Loom30.cs:226). One phase, intro at least 3 s; at half HP the deck breaks into a pit.

| Hit (Loom30.Filter :292-304; hooks Loom31.cs:89-97) | Share |
| --- | --- |
| NAILGUN on a joint, chest within **6 m** (L2820) or roped to that leg | 100 % |
| NAILGUN anywhere else, or the PULSE | **0.12** (L2848) |
| KESTREL or THUNDERCLAP | **0.01** (L2846) |
| Parry | 100 % |

**Geometry** (Loom30.cs:170-175):
- The knees are 9 m up and 3 m out, so never in reach from the deck.
- From a deck corner the hips are about 6.02 m away: just out of reach.
- So full damage means hanging on the rope, which shakes you off after 3 s (L2826); the cut comes every 6 s. Standing on the deck for 3 s brings a strike drop.

**Hurtbox:** 8 joint spheres of radius 1 m (drawn at 1.6 m) and a body box 8 x 4 x 8 m under the deck.

**Time with no real damage:** about 40 s a minute.

**TTK:**
- Theoretical, full uptime: about 60 s.
- Realistic (30 % full uptime): about 310 s.

**Measured:** harness 102-124 s; no Player kill time.

## Retune for 45-75 s

The NIGHT32 working-tree row `design.n32.bosses32` already asks for no zero-damage state over 1.5 s and at least 25 % chip. Values that sit on shared n27/n29/n30 keys should go under `design.n32.bosses32.<boss>.*`, so that `--off32` stays NIGHT31's game.

| Boss | Key (line) | Now | To |
| --- | --- | --- | --- |
| LANTERN | n29.bossHp.lanternMult (L1857) | 0.285 | **0.6** (pool 1080) |
| | n29.boss1.lightDamage (L1793) / lantern31.lightMult (L2637) | 0.06 / 0.6 | **0.25 / 1.0** (25 % chip when riding) |
| | lantern30.floorScale (L2285) / platformScale (L2265) | 0.5 / 0 | **1.0 / 0.25** |
| | lantern31.coreOpenSeconds (L2633) / rideGraceSeconds (L2639) | 4 / 0.6 | **6 / 1.2** |
| | n27.boss1.hp.finisherGunMult (L2013) | 0.4 | **1.0** (level 1 has no PULSE) |
| | Lantern27.cs:678-679 | clamp | keep the beat, drop the overflow loss |
| STAG | stag31.pool (L2657) | 1800 | **1300** |
| | stag31.gunScale (L2713) | 0.05 | **0.25** |
| | wallStunSeconds / openSeconds / crystalStunSeconds | 2 / 2.5 / 1.5 | **3 / 3 / 3** (longer than the PULSE cooldown) |
| | crystalRegrowSeconds (L2731) | 15 | **8** |
| | Stag31.cs:325 | x gunScale | full damage for a charged crystal hit |
| WYRM | n29.bossHp.wormMult (L1861) | 0.7 | **0.25** (pool 2145) |
| | wyrm31.gunScale (L2770) | 0.03 | **0.3** (0.5 for the 25 % rule) |
| | wyrm31.staggerSeconds (L2772) / aggroCooldown (L2808) | 2.5 / 6 | **4 / 5** |
| | Wyrm31.cs:107 or ChargeShots27.cs:277 | dead | detect the slug, or set slugMult to 1 |
| LOOM | n30.span.loom.pool (L2411) | 2400 | **1200** |
| | loom31.otherScale (L2846) / nailLightScale (L2848) | 0.01 / 0.12 | **0.25 / 0.35** |
| | loom31.fullReach (L2820) / hangSeconds (L2826) | 6 / 3 | **7.5 / 4** |

**Estimates with the new numbers** (to be measured):

| Boss | Realistic | Theoretical |
| --- | --- | --- |
| LANTERN | chip 7.8 + breaks about 18 = 26 DPS: **about 47 s**; 138 s with no breaks | about 15 s |
| STAG | back punch every 12 s (16) + chip 6 = 22 DPS: **about 59 s** | about 32 s |
| WYRM | chip about 10 + staggers about 24: **about 67 s** | about 15 s |
| LOOM | NAILGUN about 12 + shotgun in the heat lock about 6: **about 63 s** | about 24 s |

## The grapple

| Item | Value | Where |
| --- | --- | --- |
| Pull cooldown | **0.4 s** after any release, the only limit | PortExport arcline.cooldown; Mover.cs:498, :525 |
| Re-hook the same node | 0.25 s | Mover.cs:500, :529 |
| Range | 60 m, cone 15 degrees, probe 1.5 m, maxLen 55 | GrappleAim.cs:77-104 |
| Targets | **any solid surface or a node** | GrappleAim.cs:85-104 |
| Tap window | 0.2 s (L2495) | GrappleTap31.cs:122-145 |
| Speed | **44 m/s** (L2493; the web is 34) | SurvivalSim.Grapple31.cs:52 |
| Arrival | release at 2.6 m or after 1.2 s; horizontal speed clamped to 12 m/s (L2499), +5 m/s up (L2497) | GrappleTap31.cs:40-54 |
| Cancels | jump, dash or a second press keep **all** velocity (x1.08 while rising) and skip the 12 m/s clamp | Mover.cs:497; GrappleTap31.cs:107-115 |

**Why it spams:**
1. The only gate is 0.4 s.
2. Every wall is a target.
3. Nothing is spent.
4. A jump-cancel carries about 44-47 m/s into the next hook.

**Knobs** (put them under the existing `design.n32.grapple32` row; Mover.cs untouched):

| Knob | Suggested | Hook |
| --- | --- | --- |
| `charges` | 2 | GrappleTap31.Apply: with 0 charges, clear `grapple.pressed` before mover.Step |
| `rechargeSeconds` | 3 per charge | N31GrappleStep |
| `refillOnGround` | true | N31GrappleStep (mover.grounded) |
| `killRefill` | 1 per non-boss kill | kill listener |
| `minGapSeconds` | 0.8 | the same gate |
| `cancelKeepMax` | 20 m/s | G31OnMover on a jump or dash release while pulling |
| `surfacePull` | optional false (taps pull only to nodes) | GrappleTap31.Apply |

Spend a charge on `RopeFire` (G31OnMover, SurvivalSim.Grapple31.cs:105), for taps and holds alike, and reset at run start so tapes replay.
