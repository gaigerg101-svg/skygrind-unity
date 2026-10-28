# Research 2026-10-06: zoom vs unique alts, the shockwave's cooldown, round-end regen, RB cycling, a balance loop

One read-only agent, for the next night's prompt. GG, after playing NIGHT19's build:
- the wind gust / shockwave belongs on the **THUNDERCLAP's** LT, not the KESTREL's (the KESTREL keeps its zoom), and it
  is spammable: "there needs to be a long long cooldown like maybe every 15 seconds or even longer";
- "most weapons should zoom via simple zoom" on LT / right mouse, "besides a few that have unique" alt-fires;
- "the regen happens way too quickly and should only really be at the end of the round; the game needs to be balanced a
  lot more";
- the right bumper should scroll through weapons.

Tags: **[read]** = read on the cited page today; **[snip]** = a search-result snippet only (the page wasn't opened, or
refused: several wikis answered 402 / 403 today); **(unverified)** = from memory or not confirmed; **[rec]** = this note's
recommendation, not a sourced number; **[repo]** = read in this repo (or the frozen web repo) today. Not repeated here:
2026-10-06-weapon-sandbox.md (gun niches, combos, freshness, ULTRAKILL's arsenal), 2026-10-05-overhaul.md (stamina
models), 2026-10-04-controller.md (glyphs, Steam Input).

---

## 0. The recommendation in one table [rec]

| Item | Today [repo] | Recommended |
| --- | --- | --- |
| Shockwave owner | KESTREL LT (design.gust) | **THUNDERCLAP LT**; the KESTREL's LT zooms again (web ads.weapons.revolver 1.5) |
| Shockwave cooldown | 0.5 s on the ground + one air charge per airtime | **18 s, one charge**, ticks while holstered, no air charge |
| Shockwave effect | push self 12 m/s + 30 deg / 8 m cone (push, stagger 1 s), no damage | the same, **plus the web's own THUNDERCLAP double blast** (16 pellets x 8, 10 deg) on the same press |
| Shockwave upgrade | none | a **second charge** (FORGE sidegrade or perk), never a cooldown below 12 s |
| Zoom guns (LT = zoom) | KESTREL only in the web; none in Unity with design.gust on | KESTREL 1.5, STITCHER 1.25 (new), LONGWAVE 3.5 scope; when ported ZAPPER 1.3, RATTLER 1.4, PRISM 1.4 (web), CHOIR 1.25 (new) |
| Unique LT (no zoom) | — | **THUNDERCLAP** (shockwave), **COMET** (steer, held); when ported **VORTEX** (repel) and **TORCH** (fire wall) |
| Zoom guns' specials | — | on the SPECIAL key (keyboard B, pad Y): the web's own routeSpecial rule |
| In-round regen | 4 s delay (2 s while moving above 8 m/s or after dealing damage), then 25 HP/s to full | **segments**: only back to the top of the current 25 HP segment, after 5 s untouched, at 10 HP/s, no movement speed-up |
| Round end | nothing (regen already full) | **refill to full at roundClear**, over 2 s, in the 10 s break |
| Earned heals in a round | parry +15, perfect parry full, BLOOD PRICE +3 / kill / stack | the same, plus **+4 HP per kill within 6 m** and **+10 per punch kill** |
| Pad | RB = SPECIAL + ARCLINE, Y = NEXT WEAPON (hold = wheel) | **RB = NEXT WEAPON (hold 0.25 s = wheel), Y = SPECIAL + ARCLINE** (a swap of two rows) |
| Balance loop | judges exist, S1-S8 FAIL | section 7: intent table -> RANGE iterations (big steps, then bisect) -> CHASE / parity farm -> free farm -> GG's blind A/B |

Every new number goes in a Unity-side block (`Design/unity-config.json`, rule 5): `design.gust` (moved), a new
`design.ads` (the zoom table for guns the web never zoomed) and a new `design.regen`. With each flag off, the web's
behaviour.

---

## 1. What the build does today [repo]

**The gust (NIGHT19 U3-33.5):** `Assets/Scripts/Core/Guns/Gust.cs` + `Design/unity-config.json` `design.gust`:
`selfPush` 12 m/s opposite the aim through `Mover.Impulse`; `cooldown` **0.5 s**; one gust on the ground per cooldown
and **one more per airtime** (refilled on landing); no gust during a dash; `pushTime` 0.3 s; `lightKinds` husk, swarmer;
`staminaCost` 0; `kestrelHipSpreadDeg` 0 (the zoom went, so the primary became perfectly precise). The cone is the web's
`weapons.specials.shotgun.shock` (30 deg, 8 m, push 12 m/s, stagger 1 s). No damage. Event `N19Events.Gust`.
THE RANGE's probe (final build): the gust looking down lifts the player **+2.62 m** (the COMET's rocket jump +1.12 m); 3
of 8 SWARMERs pushed 3 m or more (`Logs/sandbox/final/RANGE.md`). At 0.5 s that is a free double jump plus a crowd
shove every half second: GG's "spammable".

**The zoom:** `Combat.cs:969-981` `StepAds` zooms **only the KESTREL** (`Weapon == "revolver"`), and not at all with
`design.gust` on. `AltIsSpecial` (`:960`) routes LT to the special on the KESTREL (gust) and, with `design.arsenal`
(off), on the LONGWAVE and ZAPPER. The web (`PortExport/config.json` `ads`) has the whole system:
- `ads.weapons`: revolver zoom **1.5** (ring), rattler **1.4** (spreadMult 0.3), zapper **1.3** (chainRange 1.15), prism
  **1.4** (bounce preview), rail **3.5** (scope);
- `easeTime` 0.12 s, `classEase` light 0.13 / medium 0.18 / heavy 0.25 s; `walkMult` **0.85**; zoomed `headshotMult`
  **x1.2**; `hipSpreadDeg.revolver` 0.8 (0 zoomed); `suspendStates` DASH, SWING; zoomed ricochet 12 m; scope vignette.
- The web's zoom is optical on the vertical FOV: `zoomedFov = 2 atan(tan(base / 2) / zoom)`, look sensitivity scaled by
  the tangent ratio (`skygrind/src/player/ads.js:12-25`); base `camera.fovDeg` 74.
- **The web's routing rule** (`ads.js:1-3, 34-37`): "Right mouse on a zoom weapon ... zooms while held, or toggles
  (settings.adsMode). On every other weapon right mouse is still its special; the SPECIAL key is always the special (it is
  the only one on the zoom weapons)." GG's ask is this rule with more guns in the zoom list.

**The THUNDERCLAP's own alt in the web** (`skygrind/src/combat/weapons.js:237-247, 606-659`; not ported:
`Combat.cs:13` "Not ported yet ... the THUNDERCLAP's compress special and variant"):
- hold alt to **compress** 0..1 over `compressTime` 0.6 s; letting go at 0.25 or more fires the **double blast**:
  `pellets` 16 x 8 x compress in a 10 deg cone, every enemy hit pushed 16 x compress m/s for 0.3 s, the player pushed
  `selfPush` 22 x compress opposite the aim (x0.35 and horizontal only on the ground), `cooldown` 1.4 s; event
  `doubleBlast`;
- the **cone wave** (`shock`: 30 deg, 8 m, push 12 for 0.25 s, stagger 1 s, event `coneWave`) fires on the momentum
  variant's primary shot, not on the alt.
So the gust already *is* half of the web's THUNDERCLAP; moving it there puts the cone back on its own gun.

**Regen:** `config.json` `regen`: `delay` 4 s, `rate` **25 HP/s**, `fastSpeed` 8 m/s, `fastMult` 2, `dealtWindow` 1 s,
`rankPerRank` 0.08. `Health.cs` `RegenMath.Step`: the delay counts down x2 while moving above 8 m/s **or** within 1 s of
dealing damage, then HP climbs at 25 HP/s to the cap. The sprint (10.5 m/s) and the slide floor (11.6 m/s) are both above
8, so in play the delay is ~2 s: **from 1 HP to 100 in about 6 s**. `health.max` 100; MEND (2000 pts) delay x0.6, rate
x1.5; PLATE (2500) max 150; `punch.parryHeal` 15; `design.parry` perfect parry heals to max; mod BLOOD PRICE: kills heal
3 HP per stack (max 2); mutator NO REGEN (from round 15). `survival.breakTime` 10 s.
- **Enemy damage:** husk melee 15 (0.5 s wind-up, 1.2 s cooldown), swarmer bite 8, spitter glob 12, acid puddle 10 HP/s.
  100 HP = **7 husk hits**.
- **THE CHASE (final, round 10):** FIGHT survives 300 s (1 of 10 died) while taking 96.3 HP/min; KITE 133.7 s; STAND
  9.6 s. Round 15: FIGHT 259.7 s, KITE 67.1 s (`Logs/sandbox/final/CHASE.md`). The regen hands back ~a full bar every few
  seconds of not being hit, so only standing still kills.
- **The farm (NIGHT17's 100 runs, `Logs/farm-n17-b2/farm-normal-1.jsonl`; an older build, not tonight's):** median round
  reached 9, median 660 s, **~49 s per round** including the 10 s break, ~20 kills per round; causes HUSK 39, SWARMER 24,
  ACID 19, timeout 12, SPITTER GLOB 6.
- `Director.RoundHpMult` = 1 + max(0, round - 10) x 0.08 (x1.4 at round 15, x1.8 at round 20).

**The pad (v2, `Core/Gamepad.cs:277-295`):** RT fire, LT alt-fire, LB boost, **RB SPECIAL + ARCLINE**, **Y NEXT WEAPON**,
L3 sprint, R3 punch, d-pad left previous weapon, right quick swap, up lock-on. `design.pad.wheelHold` 0.25 s: holding the
NEXT WEAPON button 0.25 s opens the wheel (a tap stays next weapon). LB / RB switch tabs in SETTINGS, CONTROLS, RESULTS
and the run setup (NIGHT19-REPORT.md).

---

## 2. Zoom (ADS) conventions

**Magnification per class**
- **Destiny 2** [read: d2.destinygamewiki.com/wiki/Zoom; snip for the /10 rule]: magnification = zoom stat / 10;
  base zoom by archetype: SMG / shotgun / sidearm **1.25**, hand cannon **1.5**, auto rifle **1.6**, pulse **1.7**, scout
  **2.1**, sniper **5.1-8.0**. "Higher zoom reduces the effective (angular) recoil", and zoom stretches damage falloff in
  proportion (a 20 m falloff start becomes 30 m at 1.5x).
- **Halo Infinite** [snip, halopedia via search; read: halopedia Commando]: most unscoped weapons get the default
  **1.4x** "smart-link" zoom (the Sidekick pistol 1.4x); BR 2x; Commando 3x (the Impact variant 2x -> 1.4x); sniper 5x /
  10x. **Descope** (knocked out of zoom when shot) applies to scoped weapons only, not to the default 1.4x zoom [snip:
  steam / Waypoint threads].
- **Apex Legends** [snip, edpi-calculator]: FOV scale per optic 1x 1.00, 2x 0.90, 3x 0.75, 4x 0.55 (their FOV slider
  makes "2x" etc. nominal). **ADS move speed by class** [snip, Dexerto]: pistols **1.0**, SMGs **0.75**, ARs **0.5**,
  marksman **0.425**, LMGs **0.4**, snipers **0.35**.
- **Call of Duty** [snip, hone.gg / yawsens]: ADS-in time SMG ~180 ms, AR ~250 ms, sniper ~400 ms, LMG ~450 ms; iron-sight
  sensitivity coefficients handgun 1.14, shotgun 1.20, SMG 1.29, AR / LMG 1.47 (which CoD title: not checked); Hybrid
  sight 1.2x / 2.4x magnified (fandom snippet).
- **SKYGRIND's web** [repo]: optical zoom 1.3-1.5 on light guns, 3.5 on the rail; ease 0.13 / 0.18 / 0.25 s by class;
  walk x0.85 zoomed.

**Who zooms and who gets a unique alt instead**
- **CoD Zombies, Killing Floor 2, Halo:** everything zooms (Halo: LT = zoom on every gun; abilities on LB / B) [snip:
  Halo Infinite default layout]. KF2 / CoD Zombies: ADS on every gun (unverified as a rule; true of the games played).
- **DOOM Eternal:** LT = the gun's mod (two per gun), no plain zoom; the Heavy Cannon's **Precision Bolt** is the scope
  mod, a slow precise shot that breaks weak points (Arachnotron turret, Mancubus cannons, Revenant rockets); "The shots can
  fire before the zoom is fully activated"; a Mobility upgrade gives +15 % move speed while scoped [snip, TheGamer /
  fandom].
- **ULTRAKILL:** no ADS; every variant's alt is a unique verb (unverified; consistent with 2026-10-06-weapon-sandbox.md
  section 1).
- **Roboquest:** "avoids ADS on everything but sniper rifles"; right mouse is the alt-fire where a gun has one [snip,
  Steam "No ADS?" thread].
- **Deep Rock Galactic:** no ADS; the M1000's focus shot zooms "a bit" on a held, released shot [snip, Steam].
- **Gunfire Reborn:** right mouse aims on guns; some guns have a secondary fire instead (snip; not confirmed per gun).

**How a zoom stays useful in a horde shooter [rec, from the above]**
1. It must buy **precision that the horde pays for:** head / weak-point damage (the web's zoomed x1.2 head bonus, the
   KESTREL's ricochet), zero spread, a longer falloff (Destiny's rule), the far kinds (SPITTER at 22 m, DRONEs).
2. It must **cost awareness:** the narrower FOV hides the flankers a horde always sends (SWARMERs at 10 m/s), plus the
   slower walk. That trade is the whole point; don't refund it with aim assist on mouse.
3. **No descope on hit** at low zoom (Halo's own exception); a flinch at most. Descope only on the 3.5x scope.
4. **Cheap to enter, cheap to leave:** 0.13-0.25 s eases (the web's), suspended by a dash or a swing and resumed after
   (the web's `suspendStates`), so zooming never fights the movement.

---

## 3. Long-cooldown movement / crowd-control abilities

| Game | Ability | Cooldown | Source |
| --- | --- | --- | --- |
| Overwatch 2 | Lúcio Soundwave (boop: 17 m/s knockback, 7 m, 45 dmg) | **5 s** (raised from 4 on 2026-01-08) | [read: overwatch.weirdgloop.org] |
| Overwatch 2 | Pharah Concussive Blast (8 m radius, 20.6-31.1 m/s) | **8 s** (9 -> 7 in Season 9, Feb 2024; 8 now) | [read: weirdgloop; read: Dexerto S9 notes] |
| Overwatch 2 | Pharah Jump Jet (8.4 m up) / Jet Dash | **14 s** / 9 s | [read: weirdgloop] |
| Apex | Octane Stim / Pathfinder Grapple / Wraith Into the Void | 1 s / **10-30 s** (longer the further you swing) / 25 s | [read: apexlegends.wiki.gg] |
| Apex | Horizon Gravity Lift | 30 s (wiki.gg) vs 15 s (Dexerto list): patch-dependent | [read / snip] |
| Apex | Fuse Knuckle Cluster / Bangalore smoke | 20 s / 30 s, **2 charges** each | [read: apexlegends.wiki.gg] |
| Titanfall 2 | Grapple | 10 s per charge, cut to **1 charge** (was 16.7 s) in Tricks and Treats | [snip, fandom; read: titanfall.wiki.gg says "one charge and recharges faster"] |
| Titanfall 2 | Pulse Blade | **25 s** (pulse lasts 4.5 s) | [read: titanfall.wiki.gg] |
| DOOM Eternal | Frag grenade / ice bomb / flame belch | **30 / 45 / 25 s**; Praetor upgrades 20 / 35 / 12.5 s; Frag Stock Up = a **2nd charge** | [snip: doomwiki via search; the pages 403'd] |
| Risk of Rain 2 | Commando Tactical Dive / Huntress Blink / Phase Blink | 4 s / 7 s / 2 s per charge | [snip] |
| CoD Cold War Zombies | Field upgrades | **charged by kills**, not time: "Each one is balanced with an appropriate number of zombie kills required to charge it"; Healing Aura needs double the kills of Energy Mine | [read: Treyarch guide; read: diamondlobby] |
| CoD BO6 Zombies | Field upgrades | "useable when its ring icon at the bottom right of the screen is filled" (seconds not found) | [read: game8] |

**What the table says [rec]**
- **PvP knockbacks sit at 5-8 s** because a fight is ~10 s long, there is one Lúcio, and the target can fight back.
  **PvE crowd control and big displacement sit at 15-45 s** (DOOM's equipment, Apex's lifts and grapples, the Pulse
  Blade), because a horde can't punish the user and a free reset every few seconds deletes the threat.
- **The longer the cooldown, the stronger the effect may be.** At 18 s the gust's 2.6 m lift (NIGHT19's open question 1)
  stops being a problem: it's a rare escape, like Pharah's 14 s Jump Jet, not a second jump.
- **Charges vs one cooldown:** one charge makes each use a decision; Titanfall 2 moved the grapple *from* charges *to* one
  faster charge. **Charges are the upgrade** (DOOM's Frag Stock Up, Apex's 2-charge kits): they allow a panic double-use
  without raising the average rate much.
- **Cooldown reduction as a reward:** DOOM sells -22 % to -50 % per ability; Cold War recharges by kills. Kill-charging
  rewards aggression (on-brand) but in a horde it speeds up exactly when the screen is full, the moment GG wants it
  rare, so keep it off the base ability and offer it as a mod at most.
- **Showing it:** every reference shows the ability's own icon with a fill (Apex / Overwatch: icon + seconds; CoD: a ring
  that fills; DOOM: the equipment icon). The ready moment gets a flash and a sound (GG's), never a persistent centre
  widget (S14: nothing persistent in the middle).

---

## 4. Health regen in wave / round shooters

| Game | Model | Numbers | Source |
| --- | --- | --- | --- |
| CoD BO1 Zombies | fast full regen | regen delay **2.4 s** (5 s at low health), then **full in 0.5 s** (the stock values a mod changed) | [read: BO1-Reimagined README] |
| CoD Zombies, lethality | few hits | BO3: **3 hits** down without Juggernog, **5** with (100 -> 250 HP) | [snip] |
| CoD Cold War / BO6 | regen + armour plates | Jugg +50 max HP (Cold War); +100 (BO6); BO6 Quick Revive halves the regen delay; armour is "a finite resource", repaired by shards / bought | [read: Treyarch guide; snip: BO6] |
| CoD MW3 (MP, for scale) | delayed regen | 5 s, then 1 HP per 0.05 s (20 HP/s) | [snip] |
| Killing Floor 2 | no passive regen; heal at wave end | "players are completely healed at the start of trader time" (60-90 s); syringe self-heal 50 HP, 15 s recharge (20 HP / 7.5 s on a teammate); healing lands at 10 HP/s | [snip: KF2 wiki; the pages 403'd / SSL-failed] |
| Brotato | heal at wave end | "Your hit points are reset between waves"; in-wave regen is a stat (1 point = 0.2 HP/s) | [snip] |
| Left 4 Dead | no regen | medkit heals **80 % of missing** HP; pills 50 temporary HP that decays | [snip: fandom] |
| Darktide (and Halo CE's shield) | two layers | toughness regens (coherency, melee kills); health comes back only at medicae stations | [snip] |
| Deep Rock Galactic | no regen | red sugar up to ~60 HP per vein; resupply pod **50 %** (60-75 % with a perk) | [snip] |
| Risk of Rain 2 | slow passive | **1 HP/s** base (Loader / Acrid 2.5), +20 % per level | [read: riskofrain2.wiki.gg Health] |
| ULTRAKILL | heal only up close | punch torso 10 / limb 20 / crit 30, gib 120, **parry = full** (to the hard-damage cap); hard damage = 35 % of damage taken, 1 s delay, drains 15 HP/s (faster at high rank) | [read: ultrakill.wiki.gg Health] |
| DOOM Eternal | glory kills | glory kills drop health, more when you're low (amounts not found) | [snip] |
| Returnal | pickups that flip | at full integrity, healing (Silphium) turns into max-HP pieces (Resin); take damage and they turn back | [snip: fandom] |
| Ninja Gaiden Sigma 2 | hybrid | temporary damage heals after each fight; part of every hit is permanent until a save point | [read: Game Developer, Vanden Bossche 2009] |

**What creates tension [read: Game Developer, "Video Games' Regeneration Station", Andrew Vanden Bossche, 2009]:**
- health packs are "regeneration by a different name. It's simply that regeneration only occurs when the designers want
  the player to";
- the static model "creates a tension of endurance. You know that as long as you press forward, there will be more health,
  but you don't know when or where it will be";
- "Players act differently when they're close to death. It forces more careful behavior."
- [snip, the search summary of the same piece] "If gameplay is focused on attrition ... having no regen makes sense, as if
  you instantly regenerate, it makes efficiency gains pointless."

**Why SKYGRIND's regen feels too fast, in numbers [repo + read]:** CoD Zombies' regen is as fast as SKYGRIND's (2.4 s,
then full), but a zombie downs you in **3 hits**; SKYGRIND's husk needs **7**. Fast regen plus low lethality = a hit
never matters. Either lever fixes it: cut the regen (GG's choice) or raise the damage. Do one at a time.

**Death spirals [snip: Enter the Gungeon / roguelite threads; read: Returnal snip above]:** cross-room attrition with
no heal is the classic spiral ("after getting hit twice there are no more heals until the next level"). The cures seen:
a reset at a boundary (KF2, Brotato: the wave), earned heals that reward the right play (ULTRAKILL close range, DOOM glory
kills, the parry), and a small regenerating layer for chip damage only (Darktide's toughness, Halo CE's shield,
Resistance's segments, NGS2's temporary damage).

**Full vs partial at the round's end [rec]:** **full.** SKYGRIND's runs are endless (rounds 9-22 in the farm); a partial
refill (L4D's 80 % of missing, DRG's 50 %) compounds across rounds into the spiral above. The round is CoD Zombies' unit
of tension; refilling at its end makes each round its own fight, and earned heals inside it decide whether you finish it.

---

## 5. The pad: weapon cycling

- **Y / Triangle = switch weapon:** CoD BO6 default ("Switch Weapon", hold = armour plate) [snip, NME / keymap];
  Halo Infinite (Y switch, hold Y switch equipment; RB is melee) [snip, Shacknews]; Destiny 2 (tap Y kinetic / energy,
  hold Y heavy) [snip, Twinfinite]; Apex (unverified).
- **RB = switch weapon:** **DOOM Eternal** (RB switch, hold = wheel; Y flame belch; LB equipment) [read earlier:
  2026-10-06-weapon-sandbox.md section 5, Shacknews]; ULTRAKILL's pad puts the variation cycle on RB [read earlier, with
  that note's caveat].
- **Menus:** LB / RB switch top tabs on Xbox and PlayStation (unverified; Microsoft's TV guidance not opened). This
  doesn't clash with RB = next weapon in play: menus and play are separate contexts (DOOM Eternal does both).
- **Why RB fits SKYGRIND [rec]:** the right thumb is busy aiming; a bumper cycles without leaving the stick (the reason
  DOOM moved it). Y then holds the gun's SPECIAL, as DOOM puts its flame belch on Y.

---

## 6. Balance methods that work

- **Sirlin [read: sirlin.net, Balancing Multiplayer Games part 1]:** "A multiplayer game is balanced if a reasonably large
  number of options available to the player are viable"; imbalance is when "an expert player can consistently beat other
  experts by just doing one move or one tactic"; options that "all accomplish the same thing, or nothing, or all lose to
  the dominant move" don't count. For SKYGRIND: a gun is viable if it's the best answer *somewhere* (THE RANGE's S2).
- **Schreiber's cost curve [read: gamebalanceconcepts.wordpress.com, Level 3]:** put every cost and benefit in one
  currency ("We want the costs and benefits to be equal, numerically"), find the curve from the simple, common items,
  then call each outlier "overpowered", "undercosted" or "above / below the curve"; "Figure out what you want to stay the
  same ... and then change the other thing." SKYGRIND's currency already exists: **seconds to clear** (THE RANGE).
- **Halo 3's sniper, 0.5 -> 0.7 s (Jaime Griesemer, GDC 2010)** [read: alexx-kay's talk notes; snip: Game Developer]:
  the problem was seen in play (snipers used up close with "no viable counter-strategies"); strengths were protected
  ("Can't touch strength knobs"), so the fix went to the rhythm (time between shots); "Change drastically when tuning!
  Small changes get lost in the noise"; "Balance requires strengths — all tools must be strong"; "Fix things players can
  directly perceive, not mathematical averages"; "Be wary of fairness. It's not balance". Halo 3 had 40+ weapons, the
  sniper alone 200+ tunable fields [snip: GDC Vault abstract].
- **Halo 3's lab [read: ABC News, "How Microsoft Labs Invented a New Science of Play"]:** "more than 3,000 hours of Halo 3
  played by some 600 everyday gamers", deaths tracked "down to the square foot"; a gun Brute killing new players in three
  shots was flagged as "not going to be a fun experience". Death heatmaps went public later [snip].
- **Riot's champion framework [read: /dev Balance Framework Update; snip: PC Gamer for the numbers]:** "If a champion is
  overperforming in ANY of the four groups, they're in need of nerfs, and if they're underperforming in ALL of the four
  groups, they're ready for buffs"; the original average-play bands: overpowered above 54.5 % win rate (lower with more
  bans), underpowered at 49 % or less [snip].
- **Valve (Mike Ambinder, GDC 2009 "Valve's Approach to Playtesting: the Application of Empiricism") [snip; the slide PDF
  was too large to load]:** designs are hypotheses, playtests are experiments; observation, verbal reports, stats,
  design experiments, surveys, later biometrics.
- **Sakurai [snip, Senkohome / GoNintendo]:** gameplay is "risk and reward"; "Give Yourself a Handicap When Balancing Your
  Game" (the developer is too good at his own game). For SKYGRIND: the bots and GG play far better than a new player.
- **Juice:** "Juice it or lose it" (Jonasson & Purho, GDC Europe 2012) and "The art of screenshake" (Nijman, 2013) are
  feel talks, not balance (not opened today; 2026-10-05-overhaul.md covers feel). Keep feel and numbers in separate
  loops: a feel change is judged by FeelReel and blind A/B, a number by the judges (memory: "feel done = measured").

---

## 7. Recommendations for SKYGRIND [rec]

### 7.1 The shockwave on the THUNDERCLAP

1. **Move it:** LT on the THUNDERCLAP fires it; LT on the KESTREL zooms (web `ads.weapons.revolver` 1.5) and its hip
   spread goes back to the web's 0.8 deg (`design.gust.kestrelHipSpreadDeg` retired). `AltIsSpecial` loses the KESTREL.
2. **Cooldown 18 s, one charge.** GG's floor is 15 s; 18 s gives 2-3 uses in a ~40 s round of combat (the farm's ~49 s
   per round minus the 10 s break), the PvE band of section 3. It ticks while the shotgun is holstered (DOOM's equipment
   timers do), so "swap to the THUNDERCLAP, LT" is the panic button and the shotgun earns its slot. Drop the air charge
   (one cooldown is the whole rule). Never in a dash (today's rule).
3. **Full strength on the press** (no compress hold: a 0.6 s hold on an 18 s ability is a second tax, and a half-charged
   blast would waste the cooldown). One press = the web's double blast (16 x 8 pellets, 10 deg; enemies hit pushed 16 m/s
   for 0.3 s) + the shock cone (30 deg, 8 m, push 12, stagger 1 s on `lightKinds`) + the self push 12 m/s opposite the
   aim (keep the measured +2.62 m lift: at 18 s it is a rare escape; this answers NIGHT19's question 1 unless GG
   overrules). Emit the web's names `doubleBlast` and `coneWave` (rule 7) as well as `gust` for the views.
4. **HUD:** a ring fills around the LT glyph on the THUNDERCLAP's weapon card (visible on the hotbar while holstered),
   the seconds left beside it while above 3 s (16 px minimum), a 0.2 s cyan flash on ready plus a `gustReady` event for
   GG's sound. LT while not ready: nothing fires, the ring pulses once.
5. **Upgrade:** the THUNDERCLAP's FORGE sidegrade grants a **second charge** (same 18 s each), never a shorter cooldown;
   if a mod ever reduces it, floor it at 12 s.
6. **Numbers' home:** `design.gust` (cooldown 18, charges 1, owner "shotgun", the rest unchanged); the web's
   `weapons.specials.shotgun` stays the web's (its 1.4 s cooldown is overridden, not edited).
7. **Checks:** a Core test (two presses 17.9 s apart: one gust; 18.0 s: two); THE CHASE KITE survival with the shockwave
   on vs off at most +25 % (S10's third part, never measured yet); the farm's gusts per round 1.5-3.

### 7.2 Zoom on most guns, unique alts on a few

| Gun | LT | Zoom | Ease (s) | Walk x | Zoomed extras | SPECIAL key (B / pad Y) |
| --- | --- | --- | --- | --- | --- | --- |
| KESTREL | zoom | **1.5** (web) | 0.13 (light) | 0.85 (web) | spread 0, head x1.2, ricochet 12 m (web) | — (web: pierce variant only) |
| STITCHER | zoom | **1.25** (new; Destiny SMG 1.25, Halo default 1.4) | 0.18 (medium) | 0.85 | spread x0.5 (new; the web's RATTLER uses 0.3) | detonate (web, 1 s cooldown) |
| LONGWAVE | scope | **3.5** (web) | 0.25 (heavy) | **0.6** (new; Apex snipers 0.35, Apex pistols 1.0) | hold fire past a full charge = overcharge (web POLISH V2) | overcharge (web) |
| ZAPPER (when ported) | zoom | 1.3 (web) | 0.18 | 0.85 | chain range x1.15 (web) | discharge stun (web, 3 s) |
| RATTLER / PRISM (when ported) | zoom | 1.4 / 1.4 (web) | 0.13 / 0.18 | 0.85 | spread x0.3 / bounce preview (web) | darts / — (web) |
| CHOIR (when ported) | zoom | 1.25 (new) | 0.25 | 0.85 | spread x0.5 (new) | burst (web, 2.5 s) |
| **THUNDERCLAP** | **shockwave** | — | — | — | — | the same shockwave |
| **COMET** | **steer** (held, web) | — | — | — | — | steer |
| **VORTEX** (when ported) | **repel** (web) | — | — | — | — | repel |
| **TORCH** (when ported) | **fire wall** (web, 4 s) | — | — | — | — | fire wall |

- So with today's five ranged guns, three zoom (KESTREL, STITCHER, LONGWAVE) and two keep unique LTs (THUNDERCLAP,
  COMET): "most zoom". The full web arsenal adds four zoom guns and two unique ones (VORTEX, TORCH); BOUNCER, DISCUS
  (recall) and PRISM are a later call. The PULSE GAUNTLET and the melee weapons: not checked (their LT today unknown).
- **Zoomed FOV at base 74 deg (vertical; 16:9 horizontal in brackets), the web's optical formula:** 1.25 -> 62.2 deg
  (94.0), 1.3 -> 60.2 (91.7), 1.4 -> 56.6 (87.5), 1.5 -> 53.3 (83.5), 3.5 -> 24.3 (41.9) [computed today]. Sensitivity
  follows the tangent ratio (the web's).
- **Rules:** zoom ends the sprint (CoD); a dash or swing suspends it and it resumes if LT is still held (web); no descope
  on hit below 2x; only the 3.5x scope descopes (Halo's split). When `design.arsenal`'s falloffs land, a zoomed gun's
  falloff start / end scale by its zoom (Destiny's rule), so the STITCHER reaches the SPITTERs zoomed and stays a hip gun
  up close.
- **Numbers' home:** the web's `ads.*` stays as is; a new `design.ads` holds the STITCHER / CHOIR zooms, the zoomed spread
  multipliers and the LONGWAVE's 0.6 walk (the web never zoomed those guns, or had one walk multiplier for all).
  `StepAds` reads "does this gun zoom" from `ads.weapons` plus `design.ads`, and the web's `routeSpecial` routing.

### 7.3 Regen: mostly at the round's end

**Model A (recommended default): round-end refill + segments + earned heals**
1. **Round end:** at `roundClear`, refill to full max HP over 2 s (inside the 10 s break), the web's `heal` event, the HP
   bar filling under the round banner.
2. **In the round, segments only:** the bar is cut into **25 HP segments** (4 at 100 HP, 6 with PLATE); passive regen
   only climbs back to the top of the segment you're in: **5 s after the last hit, at 10 HP/s, no speed-up** for moving
   or dealing damage (`fastMult` 1). A swarmer bite (8) or a glob (12) comes back; a combo that crosses a line stays until
   the round ends or you earn it back. Tick marks on the vitals plate show the lines.
3. **Earned heals (they ignore the segments):** **+4 HP per kill within 6 m** (the stamina rule's `nearRange`; ULTRAKILL's
   close-range logic), **+10 per punch kill** (ULTRAKILL's torso punch), the parry's +15 (web) and the perfect parry's full
   heal (`design.parry`) unchanged, BLOOD PRICE's +3 per kill per stack (web) unchanged (it becomes a real pick).
4. **Perks and modes:** MEND keeps its web multipliers on the segment regen (3 s delay, 15 HP/s) [rec: test whether it
   should also widen the segment to 33 HP]; the style rank keeps x(1 + 0.08 x rank) on the rate; **NO REGEN** = no
   segments and no round-end refill (earned heals only).
5. **Numbers' home:** a new `design.regen` (segment 25, delay 5, rate 10, fastMult 1, refillTime 2, killNearHeal 4,
   punchKillHeal 10); with it off, the web's `regen.*` exactly.

**Model B (the strict switch, for GG's A/B):** the same without segments: no passive HP in a round at all.

**Why A first:** at 7 husk hits per bar and ~96 HP/min taken by the FIGHT bot at round 10, B makes every round a race
against chip damage the player can't avoid; A keeps chip damage forgiven but makes real mistakes stick until the round
ends (NGS2's temporary / permanent split, Darktide's two layers), which is GG's "only really at the end of the round".
The KITE bot, which never kills up close, earns nothing back: S8 should finally pass.

### 7.4 The pad

- **RB = NEXT WEAPON** (tap), held 0.25 s = the wheel (`design.pad.wheelHold`, already wired to the NEXT WEAPON
  button); **Y = SPECIAL + ARCLINE** (the two rows of v2 swapped; the grapple must stay bound: REQUIRED). D-pad left
  previous, right last weapon, unchanged. LB / RB keep switching tabs in menus.
- Ship it as layout v3 with a migration from v2 that swaps RB and Y only if the player never edited either (the v1 -> v2
  pattern, `MigrateV1`); the prompts follow through `PromptText`.

### 7.5 A balance procedure with THE RANGE and the farm

The judges exist and are deterministic (`dotnet run --project Tools/SimHarness -c Release -- range | chase | pfarm |
sbreport`); what's missing is a loop that changes numbers on purpose.

1. **Re-baseline after 7.1-7.3** (b1): the shockwave, the zooms and the regen change every cell. Raise THE RANGE's
   `damageWeight` from 0.05 to **0.1** (10 HP = 1 s) before the baseline, because with no full regen HP is now a
   round-long resource. That's an edit to the lead's frozen `Tools/sandbox-cells.json`: the lead does it once at the
   night's start, never mid-loop.
2. **Write the intent table before touching a number** (Sirlin: viable somewhere; Schreiber: the curve first). Each gun's
   2-3 home cells and its weak cells, e.g. KESTREL C1 / C4 / C9 (heads, far kinds, drones); THUNDERCLAP C2 / C8 / C3 at
   close range (with the shockwave); STITCHER C3 / C6 / C11 (sustained, swarms); COMET C6 / C7 (knots); LONGWAVE C5 /
   C10 / C4 (lines, shields, far). The bot's preferred-gun list and the table must agree.
3. **The curve per cell is the median gun's score, not the best** (COMET is the outlier: best in 75 % of cells). A gun is
   above the curve in a cell at 0.8 x the median or better, below at 1.25 x or worse. Targets (the existing S-rows): no
   gun dominated (S1), each gun near-best in 2 or more cells and best in at most 35 % (S2), worst / best in C7 at most
   1.6 (S3).
4. **One knob per gun per iteration, the knob that fits its identity, in big steps** (Griesemer: small changes get lost;
   protect strengths, change rhythm). First +-30-50 %, then bisect. **Nerf the outlier first** (COMET: splash 4 m -> 3 m
   or `edgeDamage` 0.3 -> 0.15, or interval 0.9 -> 1.1 s), then lift the losers (THUNDERCLAP: interval 0.9 -> 0.75 s or a
   x1.5 multiplier inside 5 m; LONGWAVE: cooldown 3 -> 2 s). Every value in `design.arsenal` (rule 5), never the web's
   config. Log "knob, from -> to, cells moved, verdict" per iteration (Halo's 200 fields per gun: keep the ledger).
5. **Cadence:** RANGE every iteration (all cells, 10 seeds); THE CHASE and the parity farm (S5: each one-gun farm median
   within +-20 %) every third accepted iteration; the free farm (S6: top gun 30 % or less of kills, 5 or more guns at 5 %
   or more) once RANGE passes; then GG's blind paired A/B (NIGHT17's lesson: feel is measured, not self-scored).
6. **Riot's rule, with bots as the skill groups:** nerf a gun that is above the curve for **any** bot profile (aim sigma
   0.6 / 1.5 deg; STAND / STRAFE / BOOST scripts), buff one that is below for **all** of them.
7. **A paper sanity check before each run (TTK from config, single husk, every pellet / nail hitting):** KESTREL 62.5 DPS
   (125 to the head), THUNDERCLAP 71, STITCHER 108 until it overheats at 3 s (then a 1.5 s lock), COMET 78 plus 4 m
   splash, LONGWAVE 42 (150 per 3.6 s, but through 24 bodies). The THUNDERCLAP's 71 only lands point blank (the 8 deg
   cone is 0.56 m wide at 4 m): it loses even C2 (2.56 vs the COMET's 0.10) because nothing pays it for being close. At
   round 20 (HP x1.8: 72) the COMET's 70 direct no longer one-shots a husk: expect the late cells to reorder.

**Regen and difficulty metrics (the solo game's "win rate") [rec]; the farm needs two new fields, marked (new):**
- **R1 HP at round clear** (new field), rounds 5-15: median **35-70 %** of max (the refill must matter), p10 at least 5 %.
- **R2 heal by source** (new field): the round-end refill **50 % or more** of all HP restored (GG's "mostly at the end").
- **R3 death timing** (new field: seconds into the round at death): **25 % or fewer** of deaths in a round's first
  quarter; more means a bad start spirals.
- **R4 THE CHASE:** FIGHT at round 10 survives **120 s or more** (two rounds' combat), KITE **60 s or less** at rounds 10
  and 15 (S8 tightened), STAND dies fast (today 9.6 s: fine).
- **R5 the farm's median round reached:** report it next to today's 9; the target is GG's call ("balanced a lot more"
  doesn't say harder or easier), so print it, don't tune to it.
- Keep THE CHASE's "first hit (median s)" and the farm's causes table: if ACID's share grows past HUSK's once regen is
  slow, the puddles (10 HP/s) are the next knob, not the guns.

---

## 8. Questions for GG

1. Shockwave: 18 s with one charge (a second charge from the FORGE) — or a flat 15 s with no upgrade?
2. Regen: model A (segments: chip damage comes back, big losses wait for the round's end) or model B (nothing in the
   round; only kills up close, punches and parries heal)?
3. Keep the shockwave's 2.6 m lift now that it's every 18 s (it's higher than the COMET's rocket jump)?
4. The STITCHER zooms (its detonate moves to Y / B): OK, or keep detonate on its LT as a third unique gun?

---

## Sources

- Zoom: Destiny zoom https://d2.destinygamewiki.com/wiki/Zoom [read]; Halopedia Commando https://www.halopedia.org/Commando
  [read]; Sidekick https://www.halopedia.org/Sidekick [snip]; Halo descope threads
  https://steamcommunity.com/app/1240440/discussions/0/7179373376269097347/ [snip]; Apex optics
  https://edpi-calculator.org/tools/apex-ads-sensitivity [snip]; Apex ADS move speed
  https://www.dexerto.com/apex-legends/apex-legends-strafe-ads-speed-383308/ [snip]; CoD ADS times
  https://hone.gg/blog/ads-in-call-of-duty/ [snip]; CoD coefficients https://yawsens.com/coefficient [snip; the page body
  didn't load]; DOOM Eternal mods https://www.thegamer.com/doom-eternal-weapon-mods-guide-upgrades-mastery/ [snip];
  Roboquest "No ADS?" https://steamcommunity.com/app/692890/discussions/1/3954784199553243968/ [snip]; DRG M1000
  https://steamcommunity.com/app/548430/discussions/1/1845819570024717288/ [snip].
- Cooldowns: Lúcio https://overwatch.weirdgloop.org/w/L%C3%BAcio [read]; Pharah https://overwatch.weirdgloop.org/w/Pharah
  [read]; Overwatch Season 9 notes https://www.dexerto.com/overwatch/overwatch-2-season-9-patch-notes-2502496/ [read]; Apex
  tacticals https://apexlegends.wiki.gg/wiki/Tactical [read]; Dexerto's list
  https://www.dexerto.com/apex-legends/apex-legends-ultimate-tactical-ability-cooldowns-all-legends-1512776/ [snip];
  Pulse Blade https://titanfall.wiki.gg/wiki/Pulse_Blade [read]; Titanfall Tricks and Treats
  https://titanfall.wiki.gg/wiki/Tricks_and_Treats [snip]; DOOM Eternal frag / ice / belch https://doomwiki.org/wiki/Ice_bomb ,
  https://doomwiki.org/wiki/Frag_grenade_(Doom_Eternal) , https://doomwiki.org/wiki/Flame_belch [snip; 403]; RoR2 Commando /
  Huntress https://riskofrain2.wiki.gg/wiki/Commando [snip]; Cold War field upgrades
  https://www.treyarch.com/game-intel/2020/10/Black_Ops_Cold_War_Zombies_Gameplay_Guide [read],
  https://diamondlobby.com/call-of-duty/black-ops-cold-war/best-field-upgrades-in-cold-war-zombies/ [read]; BO6 field
  upgrades https://game8.co/games/Call-of-Duty-Black-Ops-6/archives/480265 [read].
- Health: BO1-Reimagined https://github.com/Jbleezy/BO1-Reimagined [read]; CoD health / Juggernog
  https://callofduty.fandom.com/wiki/Health_System , https://callofduty.fandom.com/wiki/Juggernog [snip; 402]; BO6 perks
  https://www.gameleap.com/articles/call-of-duty-black-ops-6-all-perk-augments [snip]; KF2 syringe / mechanics
  http://www.wiki.killingfloor2.com/index.php?title=Medical_Syringe ,
  https://wiki.killingfloor2.com/index.php?title=Mechanics_(Killing_Floor_2) [snip; 403 / SSL]; Brotato
  https://brotato.wiki.spellsandguns.com/HP_Regeneration , https://steamcommunity.com/app/1942280/discussions/0/3488628040296662341/
  [snip]; L4D https://left4dead.fandom.com/wiki/Healing_items [snip]; Darktide
  https://darktide.wiki.fextralife.com/Mechanics [snip]; DRG https://deeprockgalactic.wiki.gg/wiki/Red_Sugar [snip]; RoR2
  https://riskofrain2.wiki.gg/wiki/Health [read]; ULTRAKILL https://ultrakill.wiki.gg/wiki/Health [read]; Returnal
  https://returnal.fandom.com/wiki/Resin [snip]; Game Developer, Regeneration Station
  https://www.gamedeveloper.com/game-platforms/analysis-video-games-regeneration-station [read]; Enter the Gungeon thread
  https://steamcommunity.com/app/311690/discussions/0/4628107182184886726 [snip].
- Pad: Halo Infinite controls https://www.shacknews.com/article/127720/halo-infinite-controls-and-keybindings [snip];
  Destiny 2 https://twinfinite.net/ps4/destiny-2-change-weapons-how/ [snip]; BO6
  https://www.nme.com/guides/gaming-guides/call-of-duty-black-ops-6-controller-settings-3789479 [snip]; DOOM Eternal
  https://www.shacknews.com/article/117015/doom-eternal-controls-and-keybindings [read in the earlier note].
- Balance: Sirlin https://www.sirlin.net/articles/balancing-multiplayer-games-part-1-definitions [read]; Schreiber
  https://gamebalanceconcepts.wordpress.com/2010/07/21/level-3-transitive-mechanics-and-cost-curves/ [read]; Griesemer
  https://www.gdcvault.com/play/1012211/ [snip], notes https://alexx-kay.livejournal.com/284146.html [read], Game Developer
  https://www.gamedeveloper.com/design/video-0-2-seconds-to-balancing-i-halo-3-i-s-sniper-rifle [read]; Halo 3 lab
  https://abcnews.com/Technology/story?id=3506473 [read]; Riot https://www.leagueoflegends.com/en-us/news/dev/dev-balance-framework-update/
  [read], https://www.pcgamer.com/league-of-legends-team-reveals-new-method-to-decide-if-champions-are-overpowered/ [snip];
  Valve https://media.gdcvault.com/gdc09/slides/GDC09_Ambinder_ValvesApproach.pdf [snip; too large to load]; Sakurai
  https://gonintendo.com/contents/15531-sakurai-goes-over-game-balance-in-a-new-dev-video ,
  https://en.senkohome.com/sakurai-game-dev-gameplay/ [snip].
- Repo: `Assets/Scripts/Core/Guns/Gust.cs`, `Assets/Scripts/Core/Combat.cs` (:13, :594-660, :955-981),
  `Assets/Scripts/Core/Health.cs`, `Assets/Scripts/Core/Gamepad.cs` (:248-295), `Assets/Scripts/Core/Director.cs:176`,
  `Design/unity-config.json` (design.gust, design.stamina, design.pad, design.parry, design.arsenal),
  `PortExport/config.json` (regen, health, ads, weapons.*, punch, survival, director.rounds, husk / swarmer / spitter),
  `Logs/sandbox/final/{RANGE,CHASE,SANDBOX}.md`, `Logs/farm-n17-b2/farm-normal-1.jsonl`, `Tools/sandbox-cells.json`,
  `NIGHT19-REPORT.md`, `HANDOFF.md`; web (read only) `skygrind/src/player/ads.js`, `skygrind/src/combat/weapons.js`.
