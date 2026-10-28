# NIGHT14-RESEARCH.md (was NIGHT13-RESEARCH.md): the facts behind the POWER guns, the hotbar and radial, THE FORGE tiers, save-and-quit

> **Renamed 2026-10-04:** the POWER-guns night moved to NIGHT14 (`NIGHT14.md`). Its sections 2 (the hotbar and radial)
> and 4 (save-and-quit) are used by NIGHT13 (`NIGHT13.md`, `NIGHT13-RESEARCH.md`), which builds those two.

Read-only research by a background agent during NIGHT11 (2026-10-03), kept by the NIGHT11 close as the evidence for
`NIGHT13.md` (untracked; it was NIGHT12 until NEXT-WAVE-RESEARCH.md moved it). Its section 5 is the split in `tasks.json` U3-14.1..U3-14.10.

Read-only research, 2026-10-03. Unity repo `C:\Users\gaige\dev\SKYGRIND-unity` (branch `port/stage3`, read while NIGHT11
was running: HEAD moved 9b41520 -> 4728ba2 during the read, and `Core/Power.cs` / `Core/Exfil.cs` appeared; re-check
every Unity line number). Web repo `C:\Users\gaige\dev\skygrind` at `p17-night6` (read only). Material: these two repos
only; no connected systems (Drive / Gmail / Linear) were relevant or searched.
Tags: **[V]** verified by reading the cited line(s); **[U]** uncertain / inferred / not checked.

## 0. Where the plan stands

- PORT-PLAN row "RATTLER, TORCH, BOUNCER, ZAPPER, DISCUS, VORTEX, CHOIR, PRISM; 12 specials; a momentum variant per gun",
  unticked (PORT-PLAN.md:72); hotbar + radial unticked (:99); N11 row: "combat/guns.js (8 guns, 12 specials, momentum
  variants), gunFx, arcs; the remaining wall buys; the hotbar and the radial wheel (keyboardMouse.js wheel / Tab)", closed by
  "gunMath vectors complete; one kill with each gun in the smoke's armory step" (:117) [V].
- The research order moved these to N12: "the rest of guns.js; the hotbar and wheel; THE FORGE tiers 5k / 15k / 30k"
  (ZOMBIES-ULTRAKILL-RESEARCH.md:353); save-and-quit "N12 or the Steam night" (NIGHT11-RESEARCH.md:148-149, :170-171;
  HANDOFF.md:11-12) [V].
- tasks.json: U3-14 "NIGHT11: the POWER guns" is `not_started` with exactly this scope (tasks.json:316-319); U3-15 is still
  titled "NIGHT12: melee, perks, mods, traps, ARCLINE" (:321-324) and needs re-titling [V]. Suggestion: reuse U3-14 as
  NIGHT12's parent and split it into U3-14.1..10 (section 5) [U: GG / lead's call].
- HANDOFF §9: "The box's pool and the wall read `Loadout.Fires`: a newly ported gun joins both by being added there"
  (HANDOFF.md:204); "New vectors come from a new untracked port-export-pNN/" (:203). p20 now exists (NIGHT11's), so NIGHT12's
  vectors go in **port-export-p21** [V].

## 1. The eight POWER guns

Shared facts [V]:
- guns.js is 636 lines: `GUN_IDS` :30, `createGuns(config, kit)` :39-636. Its imports: combatMath `sampleCone`, gunMath
  `burstRoundAt / chainTargets / reflectDir / sweepTargets`, weaponMath `fireInterval / heatStep`, momentumMath
  `momentumRadius`, `stepsFor`, `MoveState` (guns.js:22-28).
- Its state: per-gun `cooldowns` / `altCooldowns` (steps), `heats` for torch / zapper / choir, `burst`, `torchSteps`,
  `choir {spin, carry, burstLeft, burstSteps, magnet}`, `paint`, `vortexAlt`, `walls` (guns.js:43-52). Helpers: `fresh`,
  `dashing`, `M()` = loadout.aimMomentum, `variant(id)` (M > momentum.variantAt 0.6) (:69-77).
- `step(weapon, input, dt, ready)` (:441-581): cooldowns tick, heat for the 3 heat guns whatever is in hand (:448-452),
  stepWalls (:453), the burst (:455-462), choir spin and burst (:465-475), the specials (:477-508), then the fire switch
  (:513-580). Also `meter` :584-594, `heat` :597, `locked` :600, `spin` :604, `cooldownFraction` :608-612, `reset` :615-634.
- weapons.js calls it at :229-235: `motion.sinceDash / sinceSlamLand / wasSlam` (:230-233), then `guns.step`, then
  `melee.step`, **before** the loadout's own specials (:236-275). The kit is weapons.js:419-437.
- **Math.random draws:** rattlerRound 2 per round (guns.js:86), choirRound 2 per round (:222); projectiles.js `arm` 1 per
  launched projectile (spin, :222) and `volley` 3 per dart (:283-285).
- The 8 hint labels: config `ui.specialHints` (config.js:2228): rattler PAINT + VOLLEY, torch FIRE WALL, bouncer AIRBURST,
  zapper DISCHARGE, discus RECALL, vortex COLLAPSE, choir BURST; prism has none (ui.test.js:86-88).
- Zoom guns: `ads.weapons` has revolver, rattler (zoom 1.4, spreadMult 0.3), zapper (1.3, chainRange 1.15), prism (1.4,
  preview), rail (3.5). On a zoom gun right mouse zooms and the special is B only (ads.js routeSpecial :37-42, `spec`
  :93-95) [V].
- Families (config.js:491-502): rattler kinetic, torch fire, bouncer blast, zapper volt, discus edge, vortex gravity, choir
  kinetic, prism pierce [V].
- Models: weaponModels.js rattler :215, torch :239, bouncer :261, zapper :287, discus :317, vortex :342, choir :364, prism
  :391-415. Viewmodel parts: viewmodel.js :449-510, events :578-580, :657 [V].

**Unity has none of the 8 [V]:**
- `Loadout.Fires` = revolver, shotgun, gauntlet, nailgun, rocket, rail (Combat.cs:602-603).
- There is no Guns class and no `guns.step` call in `Loadout.Step` (Combat.cs:458-596).
- `Projectiles` stubs `Launch` (:401), `Volley` (:416), `Airburst` (:450), `Recall` (:467) and `VortexAlt` (:470). The
  Step skips grenades / discs / vortexes / darts (header :10-14). The `Projectile` class lacks bounces, maxBounces, rest,
  spike, open, openSteps, hitAt, pulled, recall, repel, pullMult and target (:97-117).
- Already ported and golden: gunMath (GunMath.cs; checks in GoldenPorted.cs:145-208), CombatMath.SweepTargets / SampleCone,
  MomentumMath.MomentumRadius (MomentumMath.cs:114), Arsenal Status / HasStatus / Use (Arsenal.cs:133-177), Enemy.Stun /
  Knockback (Combat.cs:60-85).

| Gun (id) | Price | Config (config.js) | guns.js | Projectile / FX | Web tests and presets |
|---|---|---|---|---|---|
| RATTLER (rattler) | 1000 | weapons.rattler :393-397 (9 dmg, 3 rounds, roundInterval 0.06, burstInterval 0.36, dash tight 0.35 / x0.75 cadence); specials.rattler :445 (paint cone 12 / every 0.2 / max 4 / range 60; darts speed 40, dmg 18, turn 540; cd 1.6; variantRounds 5) | rattlerRound :79-94; fire :514-527; burst :455-462; rattlerAlt :308-343 (+:478-479) | darts: projectiles.js volley :276-291, stepDarts :567-589 (life hard-coded stepsFor(3)); paint markers main.js:1955-1966 | gunMath.test.js:65-69 (burstRoundAt); ads.test.js:38-39; preset newGuns powerScripts.js:245-252, pass at :888 (6 rounds, one gap > 0.2 s, >= 5 hits); gunSpecials nightScripts.js:127, pass :466 (1 volley, >= 2 painted, >= 2 dart hits); momentumVariants :227, pass :472 (5 rounds) |
| TORCH (torch) | 1250 | weapons.torch :398-403 (range 7, cone 18, tick 0.1, 6.5 dmg, pointScale 0.3, heat 4 / lock 1.5 / cool 0.7, slide x1.5 cone x1.25 dmg); specials.torch :448 (wall 7 m long, 2.5 ahead, width 1.2, life 3, dps 20, tick 0.25, cd 4, heat 0.35, variantReach 1.4) | flame :96-118; fire :528-536; torchAlt :345-370; stepWalls :372-398 | no projectile; BURN; `groupShot('torch')` (Unity has a nail-only GroupNailShot, Combat.cs:379-389); gunFx `flame` :77-85, `fireWall` :44 | gunMath.test.js:9-21 (sweepTargets), :85-91 (a tick pays 0.3 of a hit); newGuns pass :889 (3 bodies burning, overheat 3.5-4.5 s); gunSpecials (1 wall, burn > 0); momentumVariants (reach x1.4) |
| BOUNCER (bouncer) | 1500 | weapons.bouncer :404-411 (speed 22, gravity x0.8, bounce 0.55, friction 0.7, fuse 1.6, 68 dmg, splash 3.5, knockback 13, pool 12, spike dirY 0.8 / fuse 0.12); specials.bouncer :450 (variantRadius 1.3) | lob :168-183; blastMult :165-166; fire :543-548; airburst :482 | grenade: launch :257-265, stepGrenades :592-644, airburst :312-322, blast :146-175 (grenadeJump); web bug: the airburst event puts the count in `owner` (:320) | gunMath.test.js:38-42 (bounceVelocity); newGuns pass :890-891 (fuse 1.6 +/- 0.05, contact < 0.6 s kills, jump > 1 m); gunSpecials (1 airburst, < 0.5 s) |
| ZAPPER (zapper) | 1750 | weapons.zapper :412-417 (26 dmg, 0.3 s, range 32, 3 chains, chainRange 7, falloff 0.7, +1 airborne, heat 3.5 / 1.5 / 0.8); specials.zapper :451 (stun 1.5, 10 dmg, range 32, cd 3, variantChains 2) | zap :120-163; fire :537-542; zapperAlt :400-422 (needs the arsenal on) | hitscan chain (GunMath.ChainTargets ported); SHOCK; gunFx `zap` :86-95 and `discharge` :57-64 on fx/arcs.js (179 lines) | gunMath.test.js:23-35 (chainTargets); newGuns pass :892 (4 bodies, falling damage, 4 SHOCKED); gunSpecials (>= 2 stunned) |
| DISCUS (discus) | 2250 | weapons.discus :418-424 (speed 34, 52 dmg, 0.65 s, 4 bounces, life 2.5, pierceHp 60, +15% per bounce, rehit 0.3, slide +1 bounce); specials.discus :453 (recall 40 m/s, catch 1.4, variantBounces 2) | throwDisc :185-201; fire :549-554; recall :484-486 | disc: stepDiscs :647-726, recall :335-344; RICOSHOT via `bounced` (:703). Husk 40 / swarmer 10 / spitter 60 HP are all <= pierceHp, so discs cut every ported kind | gunMath.test.js:43-47 (reflectDir, discDamage); newGuns pass :893 (cuts 2, >= 1 bounce); gunSpecials (1 caught) |
| VORTEX (vortex) | 2500 | weapons.vortex :425-432 (speed 14, flight 1.2 s, pull 7 m / 1.5 s / 9 m/s, burst 95 in 5 m, 2.4 s, pool 3, dash x1.6 speed); specials.vortex :454 (repelHold 0.25, repelSpeed 12, variantPull 1.4) | orb :203-215; fire :555-560; alt (tap collapse / hold repel) :495-507 | vortex: stepVortexes :729-767, openVortex :182-189, burstVortex :191-197, vortexAlt :346-357; PULL; orbs, rings and motes in render :521-553 | gunMath.test.js:50-56 (pullSpeed); newGuns pass :894 (spread falls > 1.5 m, 1 burst, 3 hit); gunSpecials (1 collapse > 0.3 s early, 1 repel, pushed > 0.5 m) |
| CHOIR (choir) | 3500 | weapons.choir :434-437 (6 dmg, 5 -> 18 rounds/s, spin up 0.8 / down 0.5, spread 2.2, range 80, heat 5 / 2 / 0.6); specials.choir :456 (burst 12 at 0.03 s, spread 3.5, cd 2.5, variantConeDeg 3) | choirRound :217-235; spin :465-475; fire :561-572; burst special :487-493 | hitscan; `groupShot('choir')`; viewmodel barrels and collar :487-495 | gunSpecials (12 rounds); momentumVariants (choir > 0) |
| PRISM (prism) | 2750 | weapons.prism :438-440 (34 dmg, 0.55 s, range 60, 3 bounces, split 3 x 14 deg at 0.6); specials.prism :457 (variantBounces 1; no special) | prismSegment :237-268; prismBeam :270-305; fire :573-578 | beam; RICOSHOT `bounced` (:256); gunFx `prismBeam` :45-56; the zoomed preview weaponFx.js:41-46, :82+ (not ported, PORT-STATUS.md:34) | gunSpecials (>= 1 bounce); momentumVariants (prism > 0) |

Wall slots, worked out from `wallBuys.js:167-182` + `PortExport/maps/survival.json` [V]: the guns in `weapons.prices` order
(melee filtered out) take `armoryWall.zs` = [46.6 .. 26.6] (11 slots, x 24):
- shotgun 46.6, **rattler 44.6**, **torch 42.6**, nailgun 40.6, **bouncer 38.6**, **zapper 36.6**, rocket 34.6,
  **discus 32.6**, **vortex 30.6**, rail 28.6, **prism 26.6**.
- **CHOIR** goes to `armoryBack` (z 48, x 17.7).
- wallBuys.js:180's comment says PRISM and CHOIR hang on the back wall. With 11 slots only CHOIR does (the data wins).
- Unity's SurvivalMap parses no `armoryBack` (only armoryWall, SurvivalMap.cs:169-175). WallBuys loops `armoryWallZs`
  (WallBuys.cs:46-49), and the smoke indexes `armoryWallZs[names.IndexOf(gun)]` (SliceSmoke.cs:169-174). So CHOIR needs
  the back wall in all three [V].

Also missing in Unity, which the 8 guns need [V]:
- **`bounced` (RICOSHOT):** `Loadout.Damage` has no bounced opt (Combat.cs:1009), and the hit / kill payloads have no
  `bounced` field (Economy.cs:13-46). StyleCore.Kill and StyleMath.KillAwards already accept it (StyleCore.cs:165,
  Style.cs:106-112).
- **ADS:** `StepAds` zooms the revolver only (`zoomable = Weapon == "revolver"`, Combat.cs:816), while routeSpecial
  skips every `ads.weapons` gun (:463). So on RATTLER / ZAPPER / PRISM (and on today's LONGWAVE) right mouse neither zooms
  nor fires the special. This comes from reading the code; it was not checked in play.
- **The hit test:** `enemies.hitEnemy(pos, r)` (grenades, discs) exists only as Spitter's private static HitEnemy
  (Spitter.cs:322).
- **The weapon bar:** `Loadout.Meter` has no caller in Game (no HUD weapon bar; hud.js:839-843), and it knows only
  nailgun / rail / rocket (Combat.cs:415-438).
- **Models:** BlockyKit.ForWeapon builds revolver, shotgun, nailgun, rocket, rail (and the gauntlet); the Viewmodel rigs
  animate nailgun, rail and rocket parts (Viewmodel.cs:387-411). `viewmodel.kitOffsets` and `recoil` in config already
  hold all 8 guns.
- **The rest of weapons.js, not ported** (Combat.cs:13-14):
  - the KESTREL's pierce variant (weapons.js:557-570)
  - the THUNDERCLAP's variant cone + coneWave (:585-588, :641-656), compress + double blast (:242-248, :292-293,
    :609-638), the pump (:210-212)
  - `motion` (:230-233)

**Oracle status [V]:**
- `port-export-p20/vectors/gunMath.json` has all 9 gunMath functions (sweepTargets 3, chainTargets 3, bounceVelocity 4,
  reflectDir 4, discDamage 4, pullSpeed 6, nextCombo 5, burstRoundAt 23, thumperWave 4 records), and every one is
  checked. "gunMath vectors complete" is true by function, but chainTargets and sweepTargets are thin.
- **guns.js has no vectors at all.** projectiles.json has only nails / rockets / lifetimes / clear sessions; weapons.json
  only rail / railSpecial.
- No vitest imports guns.js or projectiles.js. The behavioural oracles are the browser presets above (newGuns,
  gunSpecials, momentumVariants, plus armoryHall at powerScripts.js:430 / :904).
- gunMath.test.js (105 lines) is already ported (Tests/EditMode/GunMathTests.cs, 13 tests).

## 2. The hotbar and the radial wheel

**Web [V]:**
- `input/keyboardMouse.js` (411 lines):
  - `WEAPON_KEYS` Digit1..Digit9, Digit0 -> slots 0-9 (:20)
  - `selectWeapon` ignores unowned and current slots, remembers `lastWeapon` (:81-85)
  - quickSwap (:169)
  - the radial opens on the `radial` action's keydown (:170-176) and picks on keyup (:124-129, :200)
  - the wheel: `(weapon + sign(deltaY) + n) % n` with `weapons.wheelCooldown` 0.08 s (:184-194; not `cycleSlot`)
  - while the radial is open the mouse moves the pointer offset, not the view: clamped to `radial.maxOffset`, then
    `radialSector(x, y, slotCount, deadZone)` (:221-236)
  - `setSlotCount` :262, `selectWeapon(slot, {resetLast})` :266-273, `frame.weapon` :297
  - gamepad next / prev weapon :399-402
- Bindings: `input.bindings` (config.js:1182+): quickSwap KeyQ, radial Tab, special KeyB, interact KeyE, punch KeyF,
  lockOn KeyV.
- Config (config.js:548-550): `wheelCooldown 0.08`, `radial {deadZone 40, maxOffset 160, radius 170}`, `hotbarShown 12`.
- Pure maths: arsenalMath.js `slotForDigit` :85, `cycleSlot` :91, `radialSector` :100.
- `ui/hud.js` (942 lines):
  - CSS: hotbar :145-156, radial :88-95; markup :307, :332; elements :347-351; glyph :356-357
  - the hotbar render (:620-645): 12 shown, a window that scrolls to keep the gun in hand in view, the key label
    `(n + 1) % 10`, a FRESH bar per icon, dimmed under `arsenal.staleAt` 0.4, the family colour, "+N" for more
  - the radial (:646-667): at most 24 options; 0 is up, clockwise, at radius 170; the highlighted one; the middle reads
    `POINT · RELEASE TAB`
  - slot label "KEY n" / "TAB" :833; weapon bar :839-843; special hint :615-619
- Glyphs: `ui/icons.js` `weaponIcon` :175-179, SVG paths for all 8 guns at :43-99 (a name fallback `.ph` when there's none).
- main.js wiring: `setSlotCount` :746, `hudInfo.owned / slot / radial` :2382-2386, freshness :2237, `specialHint`
  :2226-2235, the aim card for the radial's highlighted weapon :2102-2116 (tooltips: N19).
- Web tests: arsenal.test.js:125-143 (digits, wheel, radial); ads.test.js:52-70 (right mouse zooms a zoom gun and never
  presses the special; the SPECIAL key does); nightScripts.js:315-321 / :455 (the radial's card names ZAPPER).

**Unity [V]:**
- `Game/CombatReader.cs` (65 lines):
  - digits 1-9 only, no Digit0 (:28-32)
  - the wheel with no `wheelCooldown` (:33-34), Q (:35)
  - keys hard-coded (B / F / E), not read from `input.bindings` (:23-27)
  - no Tab, no radial
- `InputReader.cs:44-49` always turns mouse delta into look (nothing suspends it). Unity's `delta.y` is + up where the
  web's movementY is + down (:48).
- `ArsenalMath.SlotForDigit / CycleSlot / RadialSector` are ported and golden (ArsenalMath.cs:272-298,
  GoldenPorted.cs:513-520), and arsenal.test.js's ordering tests are ported (ArsenalMathTests.cs header).
- The HUD has no hotbar, radial, weapon bar or special hint; only the weapon name + its freshness tier (UiFeel.cs:110-126).
  TITLE's controls line still says "1-3 WEAPONS" (Ui.cs:279).
- The Loadout already takes `CombatInput.weapon` as a slot request and runs the web's swap timeline (Combat.cs:460-472).

## 3. THE FORGE tiers

- **The web has one tier [V]** (forge.js, 141 lines):
  - `forgeMult` :17-26; one `forged` Set (:38); once per gun per run (`forge(id)` :93-98)
  - `after` rolls the family status on the run's stream (:106-111)
  - config `forge` :627-637: cost 5000, damage x1.8, gem 1.9, glow 1.6, seedSalt 0x0f09e, toughKinds, families
    (kinetic head x1.25, edge STAGGER 25%, volt SHOCK 30%, gravity PULL 30%, impact x1.2, pierce tough x1.3,
    fire BURN 50%, blast x1.25)
  - web test: night6.test.js:9-33 (forgeMult)
  - **No tiers, no 15k / 30k anywhere in the web.**
- **Unity [V]:** Forge.cs (151 lines) ports it all: ForgeMath.Mult, `forged` HashSet + `order` (:90-91), `Give` (the
  PROTOCOL's free forge), rng `runSeed ^ seedSalt` (:120); ForgeTests (8).
- **The tiers are new design [V source]:** "Pack-a-Punch: 5000 (BO1-BO3); 5k / 15k / 30k tiers (Cold War, BO6)"
  (ZOMBIES-ULTRAKILL-RESEARCH.md:113); recommendation #9 "THE FORGE tiers 5k / 15k / 30k" (:311); the N12 row (:353).
- **Numbers per tier: none in our data [U].** CoD's per-tier damage multipliers aren't in the research, so they need a
  research pass or GG's numbers in `design.forge` (with a `_why`).
- What a tier touches [V]:
  - `forged` becomes a per-gun level
  - the "+" name (Ui ForgedName, UiFeel.cs:118); the results' FORGED row (SurvivalGame EndRun `forge.order`)
  - the gem glow (BlockyKit); the `forge` event's payload {owner, weapon, price, free} (Forge.cs:76-82)
  - the mod lock "FORGE 3 GUNS IN ONE RUN" (config.js:569)
  - the PROTOCOL's free forge (tier 1 only? [U])
  - Off must give the web's single tier.

## 4. Save-and-quit

**The research's BO6 rules [V NIGHT11-RESEARCH.md:94-97; ZOMBIES-ULTRAKILL-RESEARCH.md:141-142, :312]:**
- solo only, one slot
- you can save after round 1, at full health, not while downed or in an exfil
- loading restores the start of the last completed round, with weapons and perks
- it autosaves each round; pause is capped at 15 minutes
- "a feature players have been asking for since 2008"; Brotato resumes at the last shop
- The web has no save or suspend (grep of src: only settings / records / challenges in localStorage) [V], so this is new
  design behind a flag (say `design.save`).
- tasks.json U5-01 "EXTRACT, the full results screen, overtime, suspend" is blocked on D-03 (:431-434) and overlaps [V].
- Our reading of "the start of the last completed round" as SKYGRIND's `roundBreak` (all enemies dead, the next round not
  begun) is an interpretation [U].

**The natural snapshot point [V]:** `SurvivalRun.ToBreak` (SurvivalRun.cs:122-128, from Step :179-185) after
`roundClear`. At a break the spawner is done, so no enemies are alive. Spitter acid puddles, grenades, discs and vortexes
can still be live then [U: not measured]. Projectiles, puddles and decals should be cleared or saved (log the choice).

**State a round-start snapshot needs (Unity, as read) [V unless marked]:**
- **SurvivalRun** (SurvivalRun.cs:67-73): state, round, cleared, steps, phaseSteps, seed, `stats` (RunStats :24-55: kills,
  headshots, shots, zones, forged, exfil...), readyFrom.
- **Economy** (Economy.cs:114-119, :201): enabled, total, earned, spent, boost, perkPoints, perkHeadshot, discount.
- **Zones** (Zones.cs:37-45): `locked[]`, `seen[]`, here, `padEnabled[]`, Version (the views refresh on it).
- **Loadout** (Combat.cs:181-214):
  - owned, slot / targetSlot, shotSeq, curShot, the nail-group clock, clock, the cooldowns dictionary
  - heat, railCharge / overcharge, the special cooldowns, the stats counters
  - damageMult and the QUICK multipliers; headshotMult
  - its rng; `arsenal` (Arsenal.cs:66-76: fresh per weapon, time, lastSwapAt, FLIP mods, overclock)
  - `momentum` (Momentum.cs:14-19: ring, at, filled); impact cooldown
  - plus NIGHT12's Guns state
- **Perks** (Perks.cs `owned` in purchase order, `tokens`): re-applied through `apply`, so restore by re-giving them in
  order [U: check that Give is idempotent with effects].
- **Mods** (Mods.cs:128-138): stacks, offer / offerRound / rerollsLeft, pending, active, lastFamily, stats, version, both
  rngs.
- **PowerUps** (PowerUps.cs:96-102): drops[], timers (DOUBLE / OVERCLOCK / STASIS), refund, styleDrop state, clock,
  lastDropAt (the pity ramp), counters, both rngs.
- **Forge:** forged, order, rng. **MysteryBox** (MysteryBox.cs:109-125): state, spot, fromSpot, pulls, totalPulls, owner,
  paid, rng (and any offer in progress).
- **Director:** DirectorState (Director.cs:23-30: intensity, phase, phaseTime, relaxTime, camp, lingerTime, anchor, home,
  hunted, exposed), dealt, kills, time, active, fullPace (:292-310). NewRun resets them (:408-417); StartWave doesn't.
- **Health / Regen:** hp, max (PLATE), shieldSteps (Health.cs:31), the regen timers.
- **Style:** StyleMeter / StyleCore (rank, meter, tokens, total, bestRank, freshness) [U: fields not listed].
- **Mover:** position, velocity, state, stamina pips, timers; look yaw / pitch.
- **Enemy ids:** `EnemyRegistry.nextId` (Combat.cs:101). Ids continue across rounds, and stuck nails / homing check them.
- **NIGHT11's systems:** Objectives (Objectives.cs:70-80), Protocol (awake, surge, lit relays, Version: Protocol.cs:122-130),
  Traps, Power, Exfil, RunMoments' tracker [U: still landing during the read].
- `simStep`, `runSeed`. Records and settings live in PlayerPrefs already (SurvivalGame EndRun; GameSettings.cs:37-71).

**Every Mulberry32 stream in a run [V]** (seeded in SurvivalGame.StartRun, about :582-609):

| # | Stream | Seed | Where | Over a run |
|---|---|---|---|---|
| 1 | Loadout.rng (hip spread, pellets, nail spread; future RATTLER / CHOIR cones) | `(runSeed ^ 0x6b357) >> 0` | Combat.cs:181, :335 | continuous |
| 2 | Projectiles.rng (arm's spin, fragments; future volley fan) | `runSeed ^ 0x9e3779b9` | Projectiles.cs:144, :216 | continuous |
| 3 | Director.rng + paceRng | `seed + n * seedStride` (+ paceSeedOffset) | Director.cs:298, :531-532 | **reseeded every round** (restore-exact for free) |
| 4 | HuskBrain.rng | `(seed ^ 0xb7a1) >> 0` | HuskBrain.cs:439, :519 | continuous |
| 5 | SpitterAcid.rng (miss sides) | `(seed ^ 0x5917) >> 0` | Husks.cs:47, Spitter.cs:234 | continuous |
| 6 | PowerUps.killRng / rng | `seed ^ powerUps salt` / `seed ^ 0x9e37` | PowerUps.cs:101, :241-242 | continuous |
| 7 | Mods draftRng / rollRng | `seed ^ 0x6d0d5` / `seed ^ 0x5eed5` | Mods.cs:134, :212-214 | continuous |
| 8 | Forge.rng | `runSeed ^ 0x0f09e` | Forge.cs:93, :120 | continuous |
| 9 | MysteryBox.rng (design.box) | `runSeed ^ design.box.seedSalt` | MysteryBox.cs:109, :188 | continuous |
| 10 | Decals.rng (cosmetic) | `runSeed` | Game/Decals.cs:51, :63 | cosmetic, not sim |
| - | Core heights (`PresetHeights flat, new Mulberry32(1)`) | constant | SurvivalGame ResetWorld | constant |

- Objectives, Traps and Protocol hold no Mulberry32 (grep; Protocol.cs header: "No randomness"). Power.cs / Exfil.cs were
  not checked [U].
- **Mulberry32's state is a private `uint a`** (CoreGrid.cs:12-30), and each draw is just `a += 0x6D2B79F5`. So the state
  is seed + draws x 0x6D2B79F5 (mod 2^32). Saving needs a `State` get / set (or a draw count to fast-forward): a small Core
  change that must keep golden + replays exact [V].
- What can't be restored bit-exactly without more work:
  - continuous streams without the State accessor
  - live projectiles, acid puddles and decals
  - Game-side view state (HUD queues, the box / relay animations: cosmetic)
  - doubles written as text: use exact round-trip printing, as JsJson / RunMoments' JsNumber already do [V NIGHT11 fd94a26]
- The testable meaning of "exact": snapshot -> restore -> snapshot gives identical bytes, and a seeded scripted run
  continued after a restore equals the same run uninterrupted [U: proposal].
- The alternative: replay the run's inputs from the seed (the "whole-run replays" after N20, PORT-PLAN.md:130). Its load
  time at round 20 is not measured [U].

## 5. Proposed NIGHT12 split (8 h, lead time about 460 min; port first, then design behind flags)

Parallel agents as in NIGHT10 / NIGHT11 (memory `night-parallel-agents`): a p21 recorder (web repo, untracked folder
only), a Core agent in a worktree (Projectiles), a Game agent on named files. The lead alone drives the Editor and git.

| # | Item | Lead budget | Done when |
|---|---|---|---|
| U3-14.1 | Verify on arrival (HANDOFF §1 at u3-n11) | 10 min | RECEIVED rows as expected |
| U3-14.2 | **The p21 oracle** (recorder agent, about 60 min in parallel). Projectiles sessions on fake loadouts (the p19 pattern): grenade (bounce, rest, spike, contact, fuse, airburst, radiusMult), disc (ricochet, weak pierce / tough bounce, rehit, recall, catch, slide / variant bounces), vortex (open on contact / at flight end, pull, repel, collapse, burst, pullMult), darts (volley fan, homing, hit). Real `createLoadout` sessions for the 8 guns + specials + variants (Math.random seeded after construction). Lead: brief it + the checkers | 35 min | p21 `verify.mjs` 0 mismatches; every new file has a checker, and a one-ulp mutation fails |
| U3-14.3 | **Projectiles.cs POWER kinds** (Core agent, worktree, cherry-pick): projectiles.js :182-197, :253-265, :275-291, :311-357, :374-376, :566-767; the Projectile fields; a public `enemies.hitEnemy` | 20 min (review) | p21 projectile sessions bit-exact; ZeroAllocTests line |
| U3-14.4 | **Core/Guns.cs** (guns.js line for line). Wired at weapons.js :229-235's place with `motion`. `groupShot(weapon)` generalized; `bounced` through Damage -> hit / kill -> StyleMeter; StepAds generalized to `ads.weapons` (spreadMult, chainRange); Meter / CooldownFraction / Heat / Spin; `Loadout.Fires` + 8. Tests: the newGuns / gunSpecials / momentumVariants pass criteria as EditMode tests | 110 min | p21 loadout sessions bit-exact; golden + 22 replays unchanged; no allocation per step |
| U3-14.5 | weapons.js remainder: KESTREL pierce variant, THUNDERCLAP variant cone + coneWave, compress + double blast, pump | 35 min | sessions or tests; `coneWave` / `doubleBlast` / `shotgunPump` events |
| U3-14.6 | **Game half** (Game agent: BlockyKit 8 models from weaponModels.js :215-415, Viewmodel rigs :449-510, ProjectileViews for grenades / discs / orbs / rings / darts, GunFx.cs + Arcs.cs from gunFx.js / fx/arcs.js, SurvivalMap `armoryBack`, WallBuys back wall). Lead: wire it; smoke buys + kills with each of the 8 | 40 min | Editor smoke PASS x8 (the VORTEX needs about 2.7 s: flight 1.2 + pull 1.5) |
| U3-14.7 | **Hotbar + radial**: CombatReader (Digit0, wheelCooldown, Tab radial with RadialSector, look paused while open, y sign), Ui hotbar (12, scroll, keys, FRESH bars, stale dim, family colour, name glyph), radial ring (24, 0 = up), weapon bar (Meter), special hint, controls line | 55 min | a capture with the radial open; tests for the selection rules |
| U3-14.8 | THE FORGE tiers (`design.forge.tiers`, off = the web): 5k / 15k / 30k levels, numbers with a `_why` | 35 min | tests on / off; golden unchanged |
| U3-14.9 | Save-and-quit (`design.save`, solo, one slot, at a round break; Mulberry32 State; the section 4 snapshot; RESUME on TITLE; consumed on load [U design]) | 70 min | snapshot round-trip identical; restore-then-continue equals uninterrupted; smoke step |
| U3-14.10 | Close (Player, smoke, `-skygrindGolden`, bench, docs, NIGHT13.md, tag u3-n12) | 50 min | INV-* pass |

- **Cut order:** FORGE tiers -> the radial (keep digits / wheel / hotbar) -> save-and-quit's TITLE UI (keep the Core
  snapshot + tests) -> arcs drawn simply -> U3-14.5.
- **Never cut:** .1, .2, .3, .4, .6, .10, and "every new flag off gives the web's game".
- **Caps:** at most 5 Play checks; at most 4 captures (the wall with 12 guns, the radial open, a ZAPPER chain or VORTEX
  pull, RESUME); 1-2 Player builds.

## 6. Risks and unknowns

1. **One Math.random in the web vs split streams in the port** (Loadout.rng, Projectiles.rng). A session that draws from
   both (a RATTLER round and a dart volley, CHOIR with a grenade in flight) can't line up unless the golden harness
   injects one shared stream into both (a test-only hook). In the game the split is a logged difference (the web's is
   unseeded) [V streams; U impact].
2. **guns.js allocates per shot:** `zapPoints.slice()`, discharge's `points`, `new Set()` / `new Vector3()` per prism beam
   and segment, `walls.push({...})`, `paint.list.filter`. PORT-RULES' no-allocation rule means pooled equivalents plus
   ZeroAllocTests [V guns.js:161, :262, :278, :329, :364, :406].
3. **Step order.** weapons.js runs `guns.step` before the loadout's own specials and their cooldown tick (:234-236); the
   Unity Step interleaves StepAds and StepPunch differently (Combat.cs:468-506). Only sessions on the real loadout will
   prove the order.
4. **Web bugs to port as they are:** the airburst payload `{owner: n}` (projectiles.js:320); dart life `stepsFor(3)`
   hard-coded (:569); `stats.shocks++` on a missing key (weapons.js:654, debug only).
5. **The ADS gap** (Combat.cs:816) affects three new guns and possibly today's LONGWAVE: check in play [U].
6. **Hotbar glyphs are SVG** (icons.js): TMP can't draw them. Use the name fallback the web itself uses, or build meshes
   from the mostly-rect paths: GG's call (look night).
7. **Save-and-quit has no web oracle:**
   - BO6's "start of the last completed round" is ambiguous
   - live projectiles and puddles at a break are not measured
   - NIGHT11's systems are still landing, so the snapshot list will grow
   - save-scumming policy (consume on load?) is a design decision for GG
8. **FORGE tier numbers are unknown** (no CoD per-tier data in our research) [U].
9. **The smoke gets longer** (8 buys + kills), and CHOIR's back-wall slot breaks the smoke's `IndexOf` lookup.
10. **Bench:** CHOIR at 18 rounds/s and TORCH at 10 ticks/s on 30 enemies, plus 32 arcs, are not measured.
11. **The repo moved during this read** (NIGHT11 is live): re-check Unity line numbers before quoting them in NIGHT12.md.
