# PORT-PLAN: everything from the browser build, rebuilt in Unity (stage 3, NIGHT9-NIGHT20)

> **Update note (2026-10-03): GG's direction is "CoD Zombies meets ULTRAKILL as the main".**
> `ZOMBIES-ULTRAKILL-RESEARCH.md` section 6 proposes a Survival-first night order aimed at a Next Fest demo. It also
> proposes adding the Zombies rituals the web never had: the Mystery Box, a power switch, special rounds, exfil, a
> wonder weapon and secrets. Adopting the order is HANDOFF D-19.
> - **NIGHT10 already follows it:** the ARMORY guns, the Mystery Box, the ULTRAKILL alignment flags.
> - **Web modules still port line for line** against the oracle. Only the order and the additions change.
> - **Section 3's table** stays as the parity reference until GG answers D-19.

**Goal:** the Unity build has everything the web game (`skygrind`, `p17-night6`) has, behaving the same. That means:
- all 18 weapons with their specials and momentum variants
- the arsenal, momentum and the ARCLINE
- all 7 enemy kinds with affixes
- the 5 bosses and RIVAL
- all 7 modes plus daily, seeds, mutators, challenges and the kiosk
- the whole Survival layer
- the style meter, the full UI and input (rebinding, gamepad)
- the look

Unity is the only build from here on. The web build stays frozen as the reference that every port is checked against.

**Why this way:** PORT-RESEARCH.md.
- Ports that kept the old build as the oracle and judged by behaviour converged (Vampire Survivors, the TS → Go
  compiler).
- Rewrites without that reference slipped for years (AQW) or shipped missing features (Habbo).
- AI-driven migrations reach 75-97% only with small units, an oracle first, machine checks, and a rulebook that is fixed
  when a mistake repeats.

**Inputs:**
- `PORT-RESEARCH.md`: the evidence and the inventory.
- `PORT-RULES.md`: how to translate.
- `tasks.json`: items U3-xx, the state.
- `PORT-STATUS.md`: per system.
- CLAUDE.md's hard rules hold throughout.

## 1. The method (every night)

1. **One web module per commit, leaves first** (PORT-RULES.md section 1). A module waits until what it imports is
   ported.
2. **The oracle before the translation.**
   - Port the module's web tests, and see them fail.
   - Consume its golden vectors. If it is pure and has none, export them first in `port-export-p18` (or a later pNN).
   - Only V8 produces expected values.
3. **Gates before every commit** (PORT-RULES.md section 10): compile clean; the unit's tests; the full suite (0 failed,
   every golden test, 16 / 16 replays and the new trajectories) when Core changed; the smoke when the flow changed.
4. **The rulebook grows.** The same translation mistake seen twice becomes a sentence in PORT-RULES.md, and the files
   with that pattern are re-checked.
5. **Engine features never replace web logic:** no PhysX, NavMesh, Animator, Behavior or VFX Graph standing in for a
   web system.
6. **Unity-only ideas stay out of the port.** They go in `Design/unity-config.json` behind a flag that is off (or the
   LAB), or in an ideas list. GG decides on them.
7. **Each night ends the same way:**
   - a Player build, its smoke, the golden check inside the Player (from N9 on), the bench
   - a tag `u3-nNN`
   - HANDOFF.md rewritten, `NIGHTnn-REPORT.md` (one page, with a 10-minute play checklist for that night's features)
   - **`NIGHT(n+1).md` written from this plan's next row**, re-checked against the code (untracked, as every
     NIGHTn.md)
   - the ticks in section 2 updated
8. **GG reviews results, not code:** the report, the checklist, the decide list.

## 2. The parity checklist ("everything")

Tick a row only when it plays as the web does: its tests or vectors pass and it was seen in a run. Owner of every
looks question: GG (CLAUDE.md rule 7).

| Area | Items (web) | In Unity (`u2a-demo`; NIGHT9 ticks at `u3-n9`; NIGHT10 at `u3-n10`; NIGHT11 at `u3-n11`; NIGHT12 at `u3-n12`) | Night |
|---|---|---|---|
| Oracle | every pure module's golden vectors consumed (28 sets incl. jsMath), 16 + 6 trial-route trajectories, V8's maths in JsMath, the Player's `-skygrindGolden` | ✓ (N9; 34 sets at N11: + roundType, runMoments, the objectives and traps sessions from p20) | N9 |
| Guns | KESTREL, THUNDERCLAP | ✓ | - |
| Guns | STITCHER, COMET, LONGWAVE (projectiles) | ✓ (N10: projectiles.js golden, weapons.js fireRail golden on the real loadout, each kills in the smoke) | N10 |
| Guns | RATTLER, TORCH, BOUNCER, ZAPPER, DISCUS, VORTEX, CHOIR, PRISM; 12 specials; a momentum variant per gun | | N12 (moved from N11 by D-19) |
| Melee | PULSE GAUNTLET, punch / parry, glob reflect | ✓ | - |
| Melee | SUNDER BLADE, FLICKER DAGGERS, THUMPER HAMMER, TETHER WHIP | | N12 |
| Wall buys | 16 (11 armory wall, 4 rack, 1 back) | 4 (THUNDERCLAP; N10: STITCHER, COMET, LONGWAVE) | N10-N12 |
| Combat systems | arsenal (FRESH, PRIMED, CROSS / COLLAPSE / OVERLOAD, 5 statuses), momentum + impact waves, momentum arc | ✓ (N9; the impact wave's crescent / echo / quake / crack come with their melee weapons, N12; the full impactFx look N12) | N9 |
| Combat systems | aim assist V-lock + lunge / homing | ✓ V-lock + lunge (N9); ✓ homing (N10) | N9 / N10 |
| Combat systems | static aura, health orbs | | N12 / N16 |
| Co-op readiness (code) | `Player[]` in Core: per-player systems, enemies at the nearest living player, the flow field seeded per player, `owner` on every event (COOP-RESEARCH; new design, no web module) | part (N9: Core `Players`, the enemies / acid / brain / flow field per player, the loadout's owner on its payload events, a 2-player sim test); the director, wallet, buyables, power-ups, zones, mods and the bare-string events: moved to the co-op night (U3-13.2, 2026-10-03 re-plan) | N9 / co-op night |
| Movement | mover, swing math, pads (bit-exact) | ✓ | - |
| Movement | ARCLINE gadget (5500 buy, the hook's target pick, enemy pull, rope visual) | | N12 |
| Enemies | husk, swarmer, spitter | ✓ | - |
| Enemies | charger, drone (+ orbs), lancer, warden; 5 affixes; squads / slots / flankers; debut cards | | N13 |
| Bosses | BULWARK, MONOLITH | | N14 |
| Bosses | ORBITER, HIVE; the boss rotation in Survival | | N15 |
| Bosses | RIVAL PRIME | | N16 |
| Modes | SURVIVAL | ✓ | - |
| Modes | BOSS RUSH | | N15 |
| Modes | DUEL (3 levels, first to 3 / 5) | | N16 |
| Modes | CLASSIC ARENA, TRAINING, TUTORIAL (10 steps), DAILY, seeded runs, mutators (4), challenges (24), the kiosk | part (N14: the kiosk (kiosk.js: -skygrindKiosk, the hints, the idle reset; the attract plays SURVIVAL on the AUTOPILOT); N12: DAILY (daily.js: today's seed, the twist, the counted run, the streak; a DAILY on HEAT 1 in the smoke), seeded runs (seeds.js; the seed code on RESULTS, RETRY SAME SEED), mutators 3 of 4 (LOW GRAVITY needs a Mover gravity factor), challenges (24; 11 can't progress until bosses / ARCLINE / affixes); their web tests ported) | N12 / N17 |
| Modes | TRIALS (6 courses, ghosts, finishes) | part (N12: finishes.js with trialMath's medal times; a finish worn on the viewmodel and the wall) | N18 |
| Survival | 5 power-ups, the forge, READY, zones, records | ✓ | - |
| Survival | perks 5 / 14 | ✓ (10: + FLIP, BOUNTY, DEADEYE, FLOW, SLIPSTREAM in N9) | N9 (+5), N12 (+4) |
| Survival | mods 10 / 19 | ✓ (10) | N12 |
| Survival | 4 traps | ✓ (N11: traps.js, a golden session, each trap's kill and cooldown in tests, a SHOCK PLATE kill in the smoke) | N11 |
| Survival | HOLD / GOLD RUSH objectives, the PROTOCOL, tips, unlock flags, run moments + the full results | part (N11: objectives.js with golden sessions, roundType golden, protocol.js, runMoments.js golden: the grade, the moments, the score line on RESULTS; a HOLD held and THE PROTOCOL completed in the smoke). N12: tips (tips.js), unlock flags (unlocks.js in the JSON profile), the finishes (the PROTOCOL's, the secret relay's), a contract completed with its unlock in the smoke. Left: the rest of the results screen (kills by weapon / type, the board, COPY SCORE LINE) | N11 / N12 / N18 |
| Style | styleCore / styleMath / styleMeter (+ styleCore and styleSim vectors) | ✓ (N9: StyleCore / StyleMeter split as the web's, styleCore session + styleSim consumed, the arsenal's FRESH) | N9 |
| UI | TITLE, PAUSED, DOWN, RESULTS, SETTINGS (17 of 41), MODS draft, most of the HUD | ✓ | - |
| UI | weapon hotbar + radial wheel | | N12 |
| UI | boss card + boss bar, kill feed, award pops | | N14 / N13 |
| UI | mode cards + every end screen | | N15-N18 |
| UI | CONTROLS + rebinding (18 actions), gamepad, 41 settings, tooltips, damage numbers, how-to-play, credits, loading, theme fonts | part (N12: the theme fonts (seen in captures), damage numbers (damageNumbers.js, off by default), SETTINGS 22 of 41 (+ QUALITY, TONE MAPPING, FILM GRAIN, ENEMY X-RAY, DAMAGE NUMBERS), killConfirm.js, the hitmarker grammar; **N13: CONTROLS + rebinding (bindings.js, the swap, UNBINDABLE) on keys and the pad, gamepad.js, holdToggle.js, the GAMEPAD tab and SLIDE / ARCLINE / UI SCALE / RENDER SCALE rows (SETTINGS' shown list names 42 keys: the web's rows that work, the Unity-only VSYNC / WINDOW MODE / RESOLUTION / RUN LOG, the 6 LAB toggles), pad menu navigation, the HUD hotbar and the radial, a version stamp, COPY REPORT; seen in captures n13-*.png**. Left: tooltips, how-to-play, credits, loading) | N12 / N13 / N19 |
| Look | world material, AO volume, sky, skyline, motes, grade (three's ACES + fog), barrier, quality levels, x-ray, rim glow, dissolve, blob shadows | part (N12, seen in captures: motes, the grade (split tone), the height fog, light pools and the muzzle / pop lights, rim glow, the dissolve, blob shadows; in code: x-ray, quality's grain / bloom / SMAA; URP SSAO stands in for the AO volume. **N13: quality.js / qualityMath.js (AUTO, render scale, per-preset shadows) on a runtime copy of the URP asset; the perf switches (-skygrindPerfOff).** Left: world material, skyline, barrier ripples, god rays) | N12 / N13 / N20 |
| Look | viewmodels for all 18 weapons, ADS rigs, finishes | part: 6 kit models (KESTREL, THUNDERCLAP, GAUNTLET; N10: STITCHER, COMET, LONGWAVE with their animated parts); N12: viewmodel.js's motion, finishes by palette (BlockyKit.Finish) | N10-N12 (models), N20 (polish) |
| *New design (D-20, D-17)* | *the Mystery Box; the ULTRAKILL alignment flags (not web parity: Unity-only, behind design flags; off is the web)* | ✓ (N10: design.box, design.ultra) | N10 |
| *New design (D-19, NIGHT11)* | *the power switch, the swarm round's banner and drop, exfil (design.power, design.zombies.swarmDrop, design.exfil: ON); ULTRA HEALTH (hard damage + blood heal) as a LAB toggle, off (D-18)* | ✓ (N11: each in tests on and off; power, exfil in the smoke) | N11 |
| *New design (NIGHT12, NEXT-WAVE-RESEARCH)* | *CONTRACTS to 39 (design.contracts), the PAYOFF strip (design.payoff), HEAT rungs 1-3 (design.heat, opt-in), MASTERY with finishes (design.mastery), the P grade (design.grade.p), the first-run script (design.onboarding), the ceremonies and the living title (design.ceremony); the JSON profile* | ✓ (N12: unit tests; the golden / replay config has every flag off (DesignTests); a contract and a DAILY on HEAT 1 in the smoke; TITLE, CONTRACTS, RESULTS' tally + strip, the round burn-in in captures) | N12 |

## 3. The nights (dependency order; about 2,000-2,500 web LOC each)

The web LOC are the inventory's estimates (PORT-RESEARCH.md §0), ±15%. NIGHT8 ported about 1,900 web LOC plus UI in 6 h
19 min, so a 12 h cap leaves room for the checks and the close. If a night overruns, its last item moves to the next
night: the order holds, the night boundaries don't.

| Night | Theme | Web modules (src/...) | Est. web LOC | Parity check that closes it |
|---|---|---|---|---|
| **N9** | **The oracle and the arsenal** | `port-export-p18` (new vectors and trajectories, every exported set consumed); JsMath ← V8's transcendentals + `-skygrindGolden` (U3-04); combat/arsenal.js + arsenalMath (the style meter's `getFresh`); combat/momentum.js + momentumMath, impact.js, ui/momentumArc.js; aim assist's V-lock and lunge; perks BOUNTY, DEADEYE, FLOW, SLIPSTREAM, FLIP | ~1,100 + tooling | arsenalMath, momentumMath, weaponMath, styleCore, styleSim, gunMath and spawnMath vectors all consumed; 0 System.Math transcendentals in Core; the Player's golden run 0 mismatches |
| **N10** | **Players and projectiles** | the `Player[]` refactor (the code half of U4-01: per-player systems, nearest-living-player targeting, `owner` everywhere); combat/projectiles.js; aim assist homing; STITCHER, COMET, LONGWAVE (weapons.js), combat/weaponFx.js, effects' decals; their models and wall buys | ~1,800 + refactor | a 2-player sim test; projectile session vectors (nails, rockets, rails); golden and replays unchanged with one player |
| **N11** | **The POWER guns** | combat/guns.js (8 guns, 12 specials, momentum variants), gunFx, arcs; the remaining wall buys; the hotbar and the radial wheel (keyboardMouse.js wheel / Tab) | ~1,600 + selection UI | gunMath vectors complete; one kill with each gun in the smoke's armory step |
| **N12** | **Melee, perks, mods, traps, ARCLINE** | combat/melee.js (4 weapons), impactFx; perks STEADY, ANVIL, STATIC (staticAura + fx/aura), PHOENIX; the 9 remaining mods; survival/traps.js; survival/gadgets.js + combat/arcline.js + fx/rope.js (the ARCLINE buy and enemy pull, U3-03) | ~1,950 | 14 / 14 perks, 19 / 19 mods; a trap kill and an ARCLINE pull in the smoke |
| **N13** | **The enemy roster** | enemies/charger, drone (+ orbs), lancer (aim beam), warden; enemyKind base remainder; ai/brain squads, slots, flankers; enemies/affixes; debut cards; fx/killConfirm; the nav grid's runtime region rebuild (U3-02) | ~2,400 | tokens / slots / aim / crowd vectors; each kind and affix spawned by the director in its debut round |
| *(GG's call)* | *co-op split-screen (U4-02)* | *PlayerInputManager, per-player camera / HUD / viewmodel, downed and revive* | *-* | *recommended here, so Oct 14's checkpoint has co-op with every gun and enemy (D-02)* |
| **N14** | **Bosses I** | enemies/bossKit + bossMath (new vectors), ui/bossCard, boss telegraphs, fx/lightPool, the HUD boss bar, the boss box layer; BULWARK (enemies/bulwark.js), MONOLITH (enemies/boss.js) | ~2,100 | bossMath vectors; each boss beaten by a scripted run in the smoke (god mode) |
| **N15** | **Bosses II and Boss Rush** | enemies/orbiter.js, hive.js (the abyss rule); the boss rotation in Survival (r5 / r10 / r20 / r25); `IMode` in Core with SurvivalGame as the mode host; modes/bossRush.js + its end screen | ~1,800 | Boss Rush clears with the bosses in order; Survival's boss rounds spawn their boss |
| **N16** | **RIVAL, Duel, RIVAL PRIME** (**N14 did the bot and nav half**: botBrain, astar, reach, follower, navGraph ported with p22 vectors; the spire graph equal to the export) | bot/botBrain, rival, rivalPrime; nav/astar, follower, reach (the exported duel nav graph); modes/duel.js; combat/healthOrbs; the Spire map (core `towers`); endDuel | ~2,400 | astar / reach vectors; a seeded duel replays its RIVAL InputFrames bit-exact |
| **N17** | **The other modes** | modes/endless + enemies/spawner + spawnMath (CLASSIC ARENA), TRAINING (training map), modes/tutorial (10 steps), mutators, daily, seeds, challenges (24), kiosk; the title's mode cards | ~1,700 | each mode starts from the title and ends on its screen in the smoke |
| **N18** | **Survival depth and Trials** | survival/objectives, protocol; ui/tips; modes/unlocks, runMoments + the full results; modes/trials + trialMath + fx/trialCourse; ghosts + ghostCodec + fx/ghostView; finishes | ~2,000 | the 6 trial routes replay to their par times; ghostCodec round-trips the web's ghost strings |
| **N19** | **Input, settings, menus** (**N13 did the input half**: bindings, CONTROLS, gamepad, holdToggle ported with their web tests; framePace superseded by targetFrameRate; a virtual pad plays TITLE -> PLAY -> pause in the smoke; a full run on a real pad needs GG) | ~~input/bindings (rebinding, CONTROLS), gamepad, holdToggle, core/framePace;~~ the 24 remaining settings; ui/menus' remaining screens (how-to-play, credits), tooltips + tooltipText, damageNumbers, icons, loading, signs; the theme fonts (woff2 → TMP SDF, OFL) | ~2,300 | gamepad / holdToggle / tooltipText vectors; a full run on a gamepad |
| **N20** | **Look and viewmodel parity** | world/worldMaterial, aoVolume, sky, skyline; fx/motes, grade + post (three's ACES and fog), barrier + barrierMath, quality + qualityMath, xray, rimGlow, dissolve, blobShadows; player/viewmodel's gun states for 18 weapons, ADS rigs, finishes; U3-05 (the enemy shader / GPU Resident Drawer) | ~2,600 (shader-heavy) | same-seed, same-step screenshots web vs Unity for 6 views; bench median and worst 1% recorded |

**After N20:**
- whole-run replays (a seeded Survival round's state hash every N steps)
- the online half of co-op (U4)
- the run shape (U5)
- Steam (U6)

## 4. Calendar (one night a day from Oct 3)

| Night | Date | Notes |
|---|---|---|
| N9-N13 | Oct 3-7 | **Demo Day (Oct 7) plays `u2a-demo`**, which stays tagged. Stage 3 works on `port/stage3`, so Demo Day never depends on an unfinished night. |
| U4-02 | Oct 8 | only if GG says so (D-02): local split-screen with every gun and enemy |
| N14-N17 | Oct 9-12 | |
| N18-N20 | Oct 13-15 | Oct 14 is the co-op checkpoint |
| | | Game 3 (co-op) is due Oct 28 |

## 5. What we won't do (and why)

- **Transpile the JS, or run it inside Unity:** there is no maintained transpiler; interpreting the JS was too slow for
  CrossCode; consoles forbid a JIT (PORT-RESEARCH.md A.2). JS runs only at export time.
- **Switch the UI to UI Toolkit:** split-screen needs per-player focus, which only uGUI has today; and USS lacks the
  web's grid, skew, gradients and shadows anyway (B).
- **Replace web logic with engine features:** that is the trap Vampire Survivors had to throw a build away for (A.1).
- **Improve while porting:** a port that changes behaviour can't be checked against the oracle. Changes go behind design
  flags after the system is ported.

## 6. Risks

- **The pace assumption:** NIGHT8's rate came from mostly pure modules. Bosses, FX and the look are three.js-heavy and
  slower. Mitigation: a night moves its last item forward and keeps the order.
- **The arsenal changes Survival's numbers:** damage and points x lerp(0.65, 1.25, FRESH), PRIMED, CROSS. That is the
  web's balance, so Unity is behind until N9. `u2a-demo` keeps the old numbers for Demo Day.
- **Movement exactness:** momentum, the ARCLINE buy, SLIPSTREAM and the trial routes all touch the mover's inputs.
  Mover.cs changes only when an item names it; the replays (16 + the trial routes) gate every Core commit.
- **The look is underestimated:** 10 ShaderMaterials, 4 shader patches, a grade pass. It gets its own night (N20), and
  screenshots compare it.
- **Co-op's date:** if U4-02 waits for N20, the Oct 14 checkpoint has no co-op (D-02).
- **Debug keys:** F8 / F9 work in shipped builds today. Gate them behind a dev flag before peers play a stage 3 build.

## 7. GG's decisions for this plan (also in HANDOFF.md section 5)

| ID | Question | Recommendation | Default if no answer |
|---|---|---|---|
| D-02 | When does local split-screen co-op (U4-02) land? | after N13 (Oct 8), so the Oct 14 checkpoint has co-op with every gun and enemy | the port continues (N14); U4-02 runs when GG says (N10's `Player[]` makes it a one-night job either way) |
| D-11 | "Better movement": the web's movement features ported exactly (ARCLINE buy, momentum, trials, pads), or changes to how it moves? | port exactly first (N9, N12, N18); any change to feel goes behind a design flag / the LAB afterwards | port exactly |
| D-12 | The UI system | stay uGUI + TMP (split-screen) | uGUI + TMP |
| D-13 | Theme fonts: convert the web's OFL woff2 fonts (Chakra Petch, Barlow Semi Condensed) to TMP SDF | yes, in N19 | yes |
| D-14 | Look parity: port three's ACES curve and smoothstep fog | yes, in N20 | yes |
| D-15 | Debug keys (F3 / F8 / F9) in shipped builds | behind a `-skygrindDev` flag (the Editor keeps them) | unchanged (keys on) until GG answers |
