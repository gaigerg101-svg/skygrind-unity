# ULTRAKILL bosses, read from its shipped code (2026-10-12, GG's own installed copy)

Source: the ilspycmd decompile of `ULTRAKILL_Data/Managed/Assembly-CSharp.dll` in the session scratchpad (`...\scratchpad\uk`,
1,538 .cs files). Citations are `File.cs:line` in that decompile. Numbers and rule shapes only; no code copied. Anything
set in the inspector or in a scene (most health values, phase-change health, move speeds) is **not checked**.
Already covered in `Research/2026-10-12-wave36/research/rea-ultrakill-combat.md` and not repeated here: V2 (whole kit),
Mindflayer (camp punish, beam preview), Cerberus/StatueBoss (tackle-chance swing), the parry buffer.

**Difficulty index** used by every boss (GameDifficulty.cs:3-9; read by `Enemy.InitializeDifficulty`, Enemy.cs:3073-3080):
0 Harmless, 1 Lenient, 2 Standard, 3 Violent, 4 Brutal, 5 UKMD. A per-enemy `difficultyOverride` wins over the prefs value.

---

## Minos Prime (MinosPrime.cs)

1. **Attack set and choice**
   - Grounded set: Boxing, Combo, Dropkick, Uppercut, picked uniformly from 4 (`PickAttack`, :477-515).
   - **Anti-repeat:** if the roll equals the last attack, take the next one in the list (wrapping) (:488-491).
   - **Forced staple:** after 5 attacks without Boxing, the next one is Boxing (:484-487).
   - Player in the air and closer than 25 m: Jump to follow (once, not twice in a row); farther than 25 m: ProjectilePunch
     (not twice in a row) (:410-437).
   - Air set (when falling): DropAttack if the player is under 5 m and below; DownSwing (max 2 per airtime) if within 10 m
     above; RiderKick at under 10 m or when the player is more than 90 degrees off straight up; after a RiderKick, the next
     air pick teleports and DownSwings (:439-475).
   - Enraged only: 25 % chance of an air ProjectilePunch when more than 6 m away and more than 4 m above ground (:445).
   - **Attack budget:** each attack adds points (Dropkick 2, Combo 3, Boxing 2, the rest 1: :520, :539, :573, :592, :613,
     :634, :718, :740). When the total reaches the cap, it resets and forces a cooldown (:342-353):
     - cap 6 (Harmless / Lenient always; Standard unenraged), 12 (Standard enraged), 10 (Violent unenraged); Violent
       enraged and Brutal+ have no cap in this line.
     - cooldown after the cap: 4 s Harmless, 3 s Lenient, 2 s Standard and up.
   - Dropkick adds 1.25 s cooldown (0.25 s enraged) (:531). Start cooldown 2 s (:27).
   - **A parry refunds the budget:** `attackAmount -= 5` (:1125).
   - Wind-ups: RiderKick locks tracking at 0.25 s (0.5 s on Standard+) and fires at 0.75 s, both divided by animation speed
     (:602-603); DropAttack fires at 0.75 s (:623).
   - **Parryable:** whatever calls `Parryable()` flashes a 10x parryable flash; on Violent+ while enraged it no longer
     opens parry windows at all (:1112-1119). A parried explosion attack on Standard or lower (unenraged) skips its explosion
     (:894-898).
2. **Movement**
   - Walks the navmesh to the player when farther than 2.5 m (:303-306).
   - **Teleports to the player:** the teleport point is on the line from where it is toward the target, clamped to 3 m
     from the target (2.5 m while boxing) (:983-990). `TeleportSide` puts it to the player's right or left front within
     4 m or 10 m (:955-980).
   - During a swing it lunges 75 m/s forward, **125 m/s if the player is sliding** (:383-386): sliding away does not
     outrun it.
3. **Phases:** enrage once below 50 % of its starting health (:252-255, Enrage :268-283): subtitle, flame effect, the
   budget cap doubles on Standard, the air projectile unlocks, dropkick recovery 1.25 -> 0.25 s.
4. **Arena:** none in code (flat arena; the outro's light shafts are scripted, :1060-1075).
5. **Difficulty:** animation speed 0.8 / 0.9 / 1 / 1 / 1.125 / 1.25 for Harmless..UKMD (:197-216); explosions on Harmless
   / Lenient are 0.5 / 0.6x speed and 0.5 / 0.75x size (:908-912). Health: not checked.

## Sisyphus Prime (SisyphusPrime.cs)

1. **Attack set and choice**
   - Primary (3): UppercutCombo, StompCombo, ExplodeAttack. Secondary (4 aerial / teleport attacks: Chop, Clap, AirStomp,
     AirKick; :602-680).
   - **Primary anti-repeat + spacing:** never the same primary twice; the Explosion needs at least 2 other primaries since
     the last one (:445-468).
   - **Primary / secondary rhythm:** after a primary, it does 1-2 secondaries (2-3 enraged) before the next primary; past 1
     secondary it is a coin flip (:429-439, :419).
   - **Anti-air rule:** if the player has no ground within 15 m below (11 m while rising), only secondaries (aerial
     attacks) are picked (:408-415). When it has spent its secondaries in the air while enraged, it teleports to the ground
     (:423-426).
   - **Attack budget** like Minos: cap 6 (Harmless / Lenient; Standard unenraged), 10 (Standard enraged), 8 / 16
     (Violent unenraged / enraged); then 2-4 s cooldown (:365-376). Costs: combos 2-3, singles 1 (:562-680). Parry
     refunds 5 (:1181).
   - Taunts after 10 primaries (15 enraged) if health > 20 during a cooldown (:317).
   - **Telegraphs:** parryable flash 30x (:1166-1171); a separate **unparryable warning flash 15x** (:1173-1176).
   - Aims at the player's predicted position 0.5 s ahead on Standard+ (:945, :965).
2. **Movement**
   - Walks toward the player only beyond 10 m; within 10 m during cooldown it stops and faces the player (:310-325).
   - While in cooldown and under 20 m it walks at half speed (:347-349): the cooldown is readable as a slow walk.
   - Teleports to the side of the player, clamped to 6 m from the target (:1027), optionally to the predicted position
     (:986-991).
3. **Phases:** enrage below 50 % of starting health (:255-258); `onPhaseChange` event for the scene (:287).
4. **Arena:** none in code.
5. **Difficulty:** same animation-speed table as Minos (:201-217); explosion shrink on Harmless / Lenient (:870-874).

## Gabriel (GabrielBase.cs, Gabriel.cs, GabrielSecond.cs)

1. **Attack set and choice** (both fights share the same picker shape)
   - First fight: AxeThrow, SpearCombo, StingerCombo, ZweiDash (Gabriel.cs:110-123).
   - **Weighted roll with a pity bonus:** each allowed move rolls 0-1 plus its bonus; the highest wins. The picked move's
     bonus resets to 0; every other move gains +0.25 (Gabriel.cs:74-128). The previous move is never allowed.
   - **Distance gates:** under 5 m, no axe throw or spear combo; over 10 m (20 m in the second fight), no stinger combo;
     the axe throw once per burst (Gabriel.cs:72-93; GabrielSecond.cs:98-99).
   - **Bursts then rest:** attacks chain without cooldown in bursts of 2 (3 on Violent+), then `attackCooldown` 3 s on
     Standard / Violent, 1 s Brutal, 0 s UKMD (Gabriel.cs:132-142; GabrielSecond.cs:160-166). After phase 2 the burst
     length is the difficulty number (GabrielBase.cs:792, :859).
   - Second phase on Violent+: summoned swords every 15 s, with a 1 s wind-up (GabrielBase.cs:337-345).
2. **Movement**
   - Flies: forward at 7.5 m/s when farther than 5 m (scaled down to 0 as it nears 5 m), plus a 5 m/s strafe that flips
     side when a wall is within 3 m (GabrielBase.cs:414-434).
   - **Teleports around the player:** a random direction on the upper hemisphere, 8-15 m from the player's head (5-8 m
     close, 15-20 m long), backed off 3 m from walls (GabrielBase.cs:569-645).
   - **Anti-hide:** when the player is out of sight, more than 20 m away or more than 15 m below, a timer fills at 1/s
     (drains at 2/s); at 3 s it teleports (GabrielBase.cs:355-363). With no line of sight when ready to attack it teleports
     at most every 0.5 s (:305-311).
   - Brutal+ attack lunges are 1.25x faster (:438-443).
3. **Phases:** at `phaseChangeHealth` (inspector, not checked) it enters a **juggle**: the player's damage launches it
   upward (5 m/s per health lost; 10 in the second fight, capped at 35 m/s). The juggle ends after 5 s, after 15 health lost
   (7.5 in the second fight) or when it lands; then it enrages, attacks after 1 s and teleports (GabrielBase.cs:282-289,
   724-816). On Violent+ the enrage animation auto-ends after 3 s (:821-828).
4. **Arena:** none in code.
5. **Difficulty:** animation speed 0.75 / 0.85 / 1 for Harmless / Lenient / Standard+ (GabrielBase.cs:226-240).

## Leviathan (LeviathanController.cs, LeviathanHead.cs)

1. **Attack set and choice (head)**
   - ProjectileBurst, Bite, Beam. **Inside 50 m it always bites** (LeviathanHead.cs:350-355); otherwise burst or bite at
     random with anti-repeat (+1 wrap) (:357-383).
   - **3 attacks per surfacing**, then it descends (:336-347). In phase 2 it stays up and every 4th attack is a forced beam
     (:338-342).
   - Burst: 80 shots on Standard+ (60 Lenient, 40 Harmless), one every 0.025 s (0.0375 / 0.05), every 20th shot explosive
     on Standard+, and the spread sweeps across then back over the burst (:179-190, 394-436).
   - Cooldowns: after surfacing 1 + (2 - difficulty) s on Standard and below, 3 s in phase 2 (:516-533); after a bite
     0.2 + (2 - difficulty) s (:556-559).
2. **Movement:** the head surfaces at a random spawn point, never the same one twice and never within 10 m of the tail
   (:478-496). Head turn rate is capped at 270 deg/s (:279).
3. **Phases**
   - Main / sub cycle: head attacks, then a sub-phase of **2 tail attacks**, then back; the weak point swaps between head
     and tail with the phase (LeviathanController.cs:153-246).
   - At `tailAddHealth` the tail stops being a phase and attacks on its own timer: (10 - 2.5 x difficulty) s on Standard
     and below (:125-135, 241-244).
   - At `phaseChangeHealth` it **takes half damage** until the transition plays (:137-142, 163-170), then the head
     centres and forces a beam (LeviathanHead.cs:327-331).
4. **Arena:** the fight is the player on a platform against a body in the water; geometry is scene-side, not checked.
5. **Difficulty:** burst size, fire rate and cooldowns above; health not checked.

## Flesh Prison and Flesh Panopticon (FleshPrison.cs; the Panopticon is `altVersion`, :12, :363)

1. **Attack set and choice:** Insignia (cooldown after 4 s), homing projectiles (1 s), black hole (2 s; only when none
   exists and not on Harmless). Uniform with anti-repeat (+1 wrap) (:314-352). **Every attack has a 1 s wind-up flare
   coloured by attack** (white insignia; cyan / orange homing; magenta black hole) (:322-350). Attack cooldown drains at
   0.75x Harmless, 0.9x Lenient (:299-311). Start: 5 s (:75).
2. **Movement:** static; it spins (rotation speed set to +/-45 after each drone wave, :496).
3. **Drone heal cycle (the core rule)**
   - Every 25 s (30 s Panopticon, :501) it spawns 10 flesh drones (12 below half health), halved on Harmless, /1.5 on
     Lenient, halved again for the Panopticon, minimum 3 (:420-436). On Violent every 5th, Brutal every 3rd, UKMD every
     drone is a skull drone (:476).
   - **Damage to the boss speeds the next wave**, scaled by how many drones are alive: 1.5 s per health with 9+ drones,
     0.4 with 6-8, 0.2 with 1-5, 0.133 with none (:188-231). So ignoring the drones to burst the boss brings the next wave
     fast.
   - At the heal, every drone still alive heals the boss (`HealFromDrone` after 5 s) and the boss takes **0.1x damage**
     while healing on Lenient+ (:506-540). With no drones alive it skips straight to the next spawn.
   - The secondary boss bar shows the heal progress (:232-239).
4. **Arena:** none in code.
5. **Difficulty:** drone count, skull-drone ratio and cooldown speed above. Health: not checked.

## Minotaur (Minotaur.cs)

1. **Attack set and choice**
   - HammerTantrum, HammerSmash, Meat (pool or cloud), plus the Ram. Uniform with anti-repeat (+1 wrap), only within 20 m
     and with line of sight (:363-413).
   - **Ram every 3 attacks or every 15 s**, whichever first (:368-375).
   - HammerSmash only under 15 m or if the player is moving toward it (:393-397).
   - **Air-habit read:** `playerAirBias` drifts toward 1 while the player is airborne and 0 while grounded, over 20 s
     (:294-296). The meat attack rolls against it: below the bias it throws a **cloud** (air denial), above it a **floor
     pool** (ground denial), and then the bias snaps to the opposite extreme (:399-410).
   - **The Ram is parryable** (`ParryableCheck` at ram start, :477). A parried ram deals it 20 damage, bonks it and pushes
     the ram cooldown to 30 s (:673-688).
2. **Movement:** navmesh run; turn rate (45 + angle) x 5 per s on Standard+ (x3 Lenient, x1.5 Harmless) (:261-275).
3. **Phases:** none in code beyond the death sequence.
4. **Arena:** the meat pool and cloud are area denial; Brutal+ uses longer-lasting versions (:542, :558).
5. **Difficulty:** turn-rate multipliers above; speed per difficulty in `GetSpeed` (:164-190, values not read).

## Ferryman (Ferryman.cs)

1. **Attack set and choice:** Downslam, BackstepAttack, Stinger, Vault/VaultSwing, KickCombo, OarCombo, Uppercut, Roll,
   plus a lightning bolt on Brutal+ (:518-610, 655-723).
   - **It reads the player's intent from velocity:** horizontal speed under 1 m/s = neutral; moving within 80 deg of
     "away from the Ferryman" = retreating; more than 135 deg = approaching (:1087-1104).
   - Player above it (>5 m, or rising faster than 5 m/s) and under 8 m: Roll if retreating, else Uppercut under 5 m
     (:533-543).
   - Beyond 8 m (or the player slide-retreats): Vault at 30-35 m if not approaching (2 s cooldown); Stinger or Roll under
     14 m when the player retreats (:544-571).
   - Approaching player: 25 % roll / sidestep to the player's side; else Overhead or another melee (:572-599).
   - **Alternation chances:** stinger / overhead / kick-combo chances start at 0.5, then snap to 0.25 after being picked
     and to at least 0.75 after not being picked (:560-568, 589-597).
   - Roll cooldown 5.5 - 2 x difficulty s (Standard and below) (:739-742); lightning cooldown 8 - 2 x difficulty, max 6
     (:749-757).
   - Parryable / unparryable flash per swing (:1052-1067).
2. **Movement:** navmesh; leaps 100 m/s up (+ forward toward the player if the ledge is safe) (:411-412).
3. **Phases:** boss version: below `phaseChangeHealth` it runs a scripted lightning phase between fixed positions
   (:390-393, 456-500); lightning recharges at full rate only then (0.4x otherwise, :444).
4. **Arena:** the phase-change positions are scene points (not checked).

## Gutterman (Gutterman.cs) and Guttertank (Guttertank.cs)

- **Gutterman's minigun:** a wind-up value climbs to 1 at `windupSpeed` per s (1 Standard+, 0.75 Lenient, 0.5 Harmless)
  and its **sound pitch is wind-up x 3**: the audible telegraph (:398-462). Once firing, its aim point chases the player
  with a speed that ramps by 2/s shielded, 5/s unshielded (x0.8 on Standard, x0.5 Lenient, x0.35 Harmless) (:405-424,
  560-575): **a player who keeps moving laterally outruns the aim; one who stops gets caught.** Shield bash only under
  12 m, after 0.5 s in sight, and not within 5 s of being parried (:586).
- **Guttertank:** punch when the player (or the player 0.5 s ahead) is within 10 m; rocket when the 1 s-predicted position
  is beyond 15 m; mines every 2-3 s (slowed to half while it has line of sight), max 5 placed, never within 15 m of
  another mine (Guttertank.cs:376-405, 553-586).

## Corpse of King Minos (MinosBoss.cs) and the Insurrectionist (Sisyphus.cs)

- **Minos (the giant):** attack by **the arena zone the player stands in**: right zone -> right slam, left -> left slam,
  middle -> middle slam, overlapping zones 50/50 (:200-226). Cooldown 2 s in phase 1; phase 2 (below 50 % health,
  :156-160) is 3 s rest after each slam, Brutal: 2 slams then 3 s, UKMD: no rest (:179-199). Black hole every 5-10 s
  (:152-167, 373-375).
- **Insurrectionist:** 4 attacks, re-rolled until it is not the previous one (and not the jump-attack twice) (:672-680),
  then the first one that passes a line-of-fire test (:681-712). After 2-3 attacks, or when the player is over 100 m
  away, it **jumps onto the player's position** (:666-671). Cooldown 3 - difficulty / 2 s (:721), **draining 3x faster
  when the player is within 10 m** (:618-629): standing close makes it attack faster.

## Hideous Mass (Mass.cs)

- Two modes, switching every 8-12 s (:445-450). Being within 7 m of it for 1.5 s forces battle mode (:286, :342-345).
  Spear every 2-4 s when it has sight from its tail (:299-309); explosives and swings every 3-5 s, 1.5x faster on Brutal+
  (:280-285, 425-439). `crazyMode` under an inspector health value (not checked) (:318).

## Geryon (Geryon.cs; found while searching, newer boss)

- Never repeats either of its last 2 attacks (:386-414). A proximity blast pushes the player away when within 22 m
  (3 s cooldown) and **adds heat**; at max heat it is stunned, takes fire damage on its weak point while stunned, and the
  stun drains 1.5x faster in phase 2 (below 50 %, :243-253, 276-299). The heat bar is the boss bar's second bar (:315).

## Earthmover / Centaur

- Not in code as an AI class: only `CentaurDeath.cs` (77 lines) exists; the Earthmover fight is scene / animation-driven
  (not checked).

---

## The 10 most transferable rules

1. **Attack budget:** each attack costs 1-3 points; at the cap (6 unenraged, 10-12 enraged) a forced 2 s rest
   (MinosPrime.cs:342-353; SisyphusPrime.cs:365-376).
2. **A parry refunds 5 budget points**, so parrying shortens the boss's next rest (MinosPrime.cs:1125).
3. **Weighted pick with pity:** roll 0-1 + bonus; unpicked moves +0.25 each, picked resets to 0, no repeats
   (Gabriel.cs:74-128).
4. **Bursts then rest:** 2-3 chained attacks, then 3 s (1 s Brutal) (Gabriel.cs:132-142).
5. **Teleport to the player, clamped to 3 m** (Minos) / 6 m (Sisyphus) from them (MinosPrime.cs:983-990;
   SisyphusPrime.cs:1027).
6. **Out-of-sight / far / below timer:** fills 1/s, drains 2/s, teleport at 3 s (GabrielBase.cs:355-363).
7. **Read the player's habit:** airborne bias over 20 s chooses floor vs air denial, then flips (Minotaur.cs:294-296,
   399-410); velocity angle <80 deg = retreating, >135 = approaching (Ferryman.cs:1087-1104).
8. **Sliding doesn't escape a lunge:** 75 -> 125 m/s when the player slides (MinosPrime.cs:383-386).
9. **Adds tie into boss damage:** the next add wave comes 1.5 s sooner per health dealt while 9+ adds live, 0.133 with
   none; surviving adds heal it at 0.1x damage taken (FleshPrison.cs:188-231, 506-540).
10. **Close range speeds the boss:** cooldown drains 3x within 10 m (Sisyphus.cs:618-629); a lagging aim ramps 2-5/s so
    lateral movement outruns it (Gutterman.cs:560-575).
