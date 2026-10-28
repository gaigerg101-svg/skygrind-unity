# Wave 35 audit: GG's five complaints after playing n34 (read-only)

Branch port/stage3, HEAD 6733d8be (NIGHT34 close + addendum). Read with git log / grep / reading only. No Editor, no Player,
no build. Where something was not traced, it says "not checked". Config values are from `Design/unity-config.json`, and the
same values are in `Assets/StreamingAssets/Design/unity-config.json` and in `Builds/n34-r5/SKYGRIND_Data/StreamingAssets/...`
(read with python: n27.chargeShots27 false, n31.charge true, n33.pierce33 true, n31.unlocks true).

## 1. "The pistol lost the ability to charge and fire, same with the shotgun"

**Where it lives**
- Rule: `Assets/Scripts/Core/Guns/ChargeShots27.cs` (NIGHT27 6b3e2ddc). The params come from a config prefix (`:32-38`, `:80-85`).
- Fire gate: `Assets/Scripts/Core/Combat.cs:903`. On the KESTREL or THUNDERCLAP, `StepCharge27` runs before the normal trigger
  path (`:904`). The THUNDERCLAP's compress gate is `:885`; it only runs under arsenal19 and with shockwaveOff25 off (`:795`).
- `StepCharge27` (`ChargeShots27.cs:155-205`):
  - a melee press cancels (`:160`);
  - a release fires a full charge;
  - a new press edge restarts the hold (`:184`);
  - while the 4 s / 6 s cooldown runs, no charge can start and the normal fire runs (`:185-189`);
  - until 0.4 s of unbroken hold, it returns false and the normal shot fires (`:191`);
  - a full charge held 0.5 s more fires itself (`:200`).
- KESTREL charged shot: pierce33's numbers (24 bodies, x9) replace n31's (`ChargeShots27.cs:227`; Pierce33.cs, NIGHT33 W 4f92416d).
- THUNDERCLAP charged shot: the slug (`FireSlug31`, `:236`; NIGHT31 W 72b69035).
- Wiring: `SurvivalSim.Weapons31.cs:39-45` puts `charge31 = new ChargeShots27(cfg, "design.n31.charge.")` on `loadout.charge27`.
  This runs after N27's StartRun, so n27's row (off, `Weapons27.cs:39` sets null) is overwritten. The clock and the swap guard
  are at `Weapons31.cs:114-115`.
- Config rows today:
  - `design.n27.chargeShots27.enabled` = **false** (`unity-config.json:1999`). This is the old NIGHT27 rule; it is meant to stay
    off.
  - `design.n31.charge.enabled` = **true** (`:2516`): hold 0.4 s, autoRelease 0.5 s, KESTREL full 0.5 s / cooldown 4 s,
    THUNDERCLAP full 0.7 s / 6 s, slug true. NIGHT31 W 41ac5550.
  - `design.n33.pierce33.enabled` = true (`:3242`).
  - Parse: `Core/N31Blocks.cs:36`. `GameConfig.Design` (`GameConfig.cs:50`) needs only the leaf bool.
- Input:
  - mouse: `CombatReader.cs:130` (the latch) and `:175` (held = LMB isPressed);
  - pad: RT maps to PadAction.Fire (`Core/Gamepad.cs:250`, `:279`), merged by `Gamepad.cs:858` / `:865-870` (held + latched);
  - LT is zoom / alt, not the charge: `LtAlways31`, `Combat.cs:729-733`.
  - Nothing NIGHT32-34 touched this path.

**Did NIGHT32-34 break it?** The code shows no break:
- pierce33 changes only the shot's numbers (`:227`). It does not replace the charge.
- bend34's instant swap (eb87d5a4, `Combat.cs:363`, StepSwap `:1121`) does not touch the fire path.
- combo34 (eae0acb6) added only `PunchCooldownSteps` / `ReadyPunch34` to Combat.cs (diff read).
- reach34 (f01c4544) changed only the gauntlet's target list (`Combat.cs:765`).
- flow34 is Mover-only (config `:3399` _why).
- The rail (rail34) is view-only.

The bend34 split world-step path was not checked for the loadout's step order.

**Most likely root cause: the charge works but cannot be felt or seen.** Evidence:
1. Holding RT first fires normal shots for 0.4 s (`Combat.cs:904` runs while `holdSteps <= HoldStepsNeeded`,
   `ChargeShots27.cs:191`). Then it charges for 0.5 s and auto-fires at 1.4 s. A held trigger feels like plain auto-fire.
2. The only charge cue in n34 is HUD33's two crosshair arcs (`UiHud33.cs:683-694`). The viewmodel (`Viewmodel*.cs`) and
   `WeaponFx` never read `charge27` and never listen to `chargeShot27Start`: a grep over Game finds charge readers only in
   HudWeapons30, UiHud33, UiWeapons.N27 and the sim. The ARSENAL RAIL has no charging state (UiRail34.cs: no "Charg"), and with
   rail34 on it replaces hud33's pip row (`UiRail34.cs:84-116`).
3. After one charged shot, the next 4 s (KESTREL) or 6 s (THUNDERCLAP) give only normal fire (`ChargeShots27.cs:185-189`).
4. In the campaign, the THUNDERCLAP is owned only from level 3 (n34-kiln row, `design.n31.unlocks.levels`). Levels 1-2 have no
   shotgun to charge.

**Would a test have caught it?**
- The last Player check of the charge is NIGHT33's PIERCE reel: `FeelReel.N33.Weapons.cs:115-130` holds the virtual pad's RT
  for hold + full + 0.15 s at a test spot, not in a campaign level (NIGHT33-PROOF.md:25, "PIERCE: 1 beam, 5 / 5").
- The THUNDERCLAP charge's last reel is `FeelReel.N31.Weapons.cs:34` (Charge31Scenario).
- NIGHT34-PROOF.md and NIGHT34-LOG.md have no charge line: nothing re-checked it after bend34 / rail34 / lines34.
- No reel measures whether the charge is *visible*.

**Fix (proposal):**
- (a) Make the charge readable on the gun: a viewmodel glow / pose and a WeaponFx hum from `chargeShot27Start` to the release,
  and a CHARGING state on the rail slot.
- (b) Decide the trigger shape with GG: either "press = shot, hold after the shot = charge" with no normal auto-fire during the
  hold, or keep the 0.4 s threshold and make it obvious.
- (c) Add a Player reel on n34-foundry (KESTREL) and n34-kiln (THUNDERCLAP): hold RT 1.5 s, count `chargeShot27Start` and
  `fired[i]`, RED once with `-skygrindOff31` (L-014).

If GG means the charge *never* fires: that is not reproduced here, and the code path is intact. A Player check is needed
(not checked).

## 2. Weapon pickup / unlock: "you don't even see it"

- Campaign weapon pickups (`weaponPickup` props and NIGHT31's `pickup31` rows) are drawn by `Game/PropsView29.cs:145-178`. The
  pickup is a cyan glow pillar plus a spinning, bobbing **plain cube**: `mk.Cube(...)` at `:164`, size 0.6 m
  (`UnityTuning.Weapons31.cs:10`). The header says so (`PropsView29.cs:5`).
- No silhouette or gun model is drawn. Commits: NIGHT29 55f81af5 (the prop) and NIGHT31 W 4271a610 (the i1a pickup31 drawn the
  same way).
- On take: `Combat.AddWeapon` (`Combat.cs` "weaponBought", the gun goes in hand through pendingSlot). Listeners:
  - UiOnboarding (`UiOnboarding.cs:140`, a lesson tick);
  - the campaign RESULTS line "UNLOCKED <name>" (`UiCampaignResults25.cs:214`).
  - No in-level reveal card, model or banner listens to `weaponBought` / `weaponEarned` (grep over Game; FeelReel excluded).
- The models already exist:
  - `BlockyKit.ForWeapon(id, kit, ...)` (`Game/BlockyKit.cs`) builds each gun's kit mesh;
  - Survival's ARMORY wall buys already show the gun's model as a turning hologram (`Game/WallBuys.cs:1-9`, `:76`, `:151-152`);
  - the viewmodel builds the same meshes (`Viewmodel.cs:193-198`: BlockyKit.Kestrel / Thunderclap / ...).
- To show the model: replace the cube at `PropsView29.cs:163-165` with `BlockyKit.ForWeapon(<the pickup's weapon id>, mk,
  Color.white)` at pickup scale. The prop row needs the weapon id (a pickup31 row carries it; the plain props' rows were not
  checked). Optionally add a HUD reveal on `weaponBought` (the name plus the rail slot sliding in).

## 3. The grapple "goes limp, too fluid"

- **Tap vs hold** (`Core/GrappleTap31.cs:128-157`):
  - a press that comes up within `tapWindow` 0.2 s (`unity-config.json:2508`) is a TAP: the zip pull, straight at the node;
  - still held after 0.2 s, it falls through to **NIGHT30's pendulum swing** (`:135-139`, "the hold passed the window: the
    swing").
  - A natural press-and-hold therefore gives the rope swing.
- **Taps on walls refused**: `design.n32.grapple32.tapNodesOnly` = true (`:3084`; code `GrappleTap31.cs:145-148`). A tap on a
  bare surface does **not pull**: "the Mover lets go" and the charge comes back. Only a HOLD on a wall does something, and that
  is the swing (`:3083` _why).
  - Commit: NIGHT32 G cb12ffff ("a TAP on a bare surface does not pull (a HOLD still swings from it)").
  - So firing at a wall can never "take you to wherever you fire". This is a design that replaced NIGHT31's tap-pull to any
    surface, not a bug.
- **Arrival clamp**: `design.n31.grapplePull.arriveKeepMax` = 12 m/s (`:2512`). The pull's carry is cut at the anchor
  (NIGHT31 G), and `grapple32.cancelKeepMax` is 20.
- Other rows: grapplePull speed 44, arrivePop 5 (`:2510`); grapple32 charges 2, recharge 4 s, minGap 0.8 s, range 40.
- whip33 (`:3225`) is the enemy latch. It stops 1.5 m short; it is not the surface pull.
- Fix: set tapNodesOnly false and/or make any press a straight zip to the hit point, and drop the hold-swing (or move it to a
  separate input). arriveKeepMax decides how "dead" the arrival feels. These are row switches plus GrappleTap31; Mover.cs is
  not needed (the zip is the Mover's own).
- **Unlocked at**: the grapple is not a `unlocks` weapon row. `SurvivalSim.Systems30.cs:198-204` owns it from the first step
  of any campaign level whose map has grapple nodes (`n30.span && n21.arcline && map.grappleNodes.Count > 0`).
  - Node counts (python over Design/levels): n33-foundry / iceline / kiln have 0; n33-span has 15; **n34-foundry has 15,
    n34-iceline 9, n34-kiln 8**.
  - With `design.n34.lines34` on (levelSwap n33 -> n34 for foundry / iceline / kiln), the grapple is owned **from level 1**.
    Before lines34 it was owned from level 4 (span).
  - That `map.grappleNodes` is filled from the level file's top-level array was assumed, not traced.
  - lines34 shared row: e7bead72. The level-file commit: not checked.

## 4. The healing item "looks weird"

- Same code as the weapon pickup: `PropsView29.cs:145-178` with `kind == "healthPickup"`. It is a spinning **plain cube** in
  green `FixtureGreenHex` 0x3CFF9A (`UnityTuning.Lights29.cs:15`) inside a green glow pillar at half the weapon's height
  (`PickupGlowHealthShare` 0.5, `UnityTuning.Props29.cs:22`).
- It was dimmed in NIGHT30 look30 because it "bloomed to a white human-shaped figure" (comment at `PropsView29.cs:166`).
- No cross, heart or plus shape exists anywhere in it. Its only difference from the weapon pickup is green instead of cyan.
- NIGHT29 55f81af5. A "MEND" power-up model: not found in the view code (MEND appears in Health.cs / Perks.cs / PerkMachines.cs,
  not checked).
- Fix: a readable shape (a plus or a cross block from MeshKit boxes) plus a slow pulse, and a distinct silhouette from the
  weapon pickup.

## 5. The lights' "weird little triangle displays"

- Fixtures:
  - `Game/Fixtures33.cs` (header `:1-4`) builds the six NIGHT33 L fixture types from `LightPlan33`'s **boxes** (sconce, strip,
    pole lamp, ceiling panel, brazier, trims). The fixture choice is `LightPlan33.cs:407`.
  - The dress kinds are also boxes (`Kit33Props.cs`): `lamp` (`:252-254`), `lampPost` (`:341-345`), `lintelLight` /
    `edgeLight` (`:218-219`), `halo` (8 box segments, `:347-355`).
  - No triangle, cone or pyramid mesh is drawn at a light. `MeshKit.Triangle` callers are only BlockyKit (guns) and the boss
    views (grep).
- Candidates for the "triangles" (not confirmed on screen):
  - (a) Vfx33's **untextured particle quads**: DUST MOTES under every room's ceiling panel (`Vfx33.cs:91`, billboard) and
    SPARKS at every brazier (`Vfx33.cs:105-112`, Stretch mode, cone emitter). Both use `Emitter` (`:117-135`). A stretched or
    small billboard quad on the neon material can read as a sliver or triangle.
  - (b) the HDR emissive lamp cores (`G("kit lamp", ...)`) blooming into shapes.
  - (c) AffixFx20's Kenney flare sprites (`AffixFx20.cs:123`, `:210`). These are drawn on affixed enemies, not lights.
- Commit: NIGHT33 L (look33, FEATURES.md:66); exact sha not checked.
- Next step: a Player frame at a lit wall in n34-foundry with Vfx33 off vs on (and `-skygrindOff33`). That shows which one
  GG means before any change.

## Summary table

| # | Verdict | Where | Commit |
|---|---------|-------|--------|
| 1 | Not a code break found. Charge ON (n31.charge, pierce33). Hidden by 0.4 s of auto-fire first, no gun/rail cue, 4/6 s cooldown. Shotgun only from level 3 | Combat.cs:903-904, ChargeShots27.cs:155-205, UiHud33.cs:683 | 41ac5550 (N31), 4f92416d (N33) |
| 2 | Design gap: the pickup is a plain cube, no model or reveal | PropsView29.cs:145-178 | 55f81af5 (N29), 4271a610 (N31) |
| 3 | Design (row): hold = pendulum swing; a tap on a wall refused; arrival clamped to 12 m/s. Grapple owned from level 1 (n34 maps have nodes) | GrappleTap31.cs:135-148, config :2508 :3084, Systems30.cs:200-204 | cb12ffff (N32 G), lines34 |
| 4 | Design gap: a green spinning cube, no health shape | PropsView29.cs:145-178 | 55f81af5 (N29) |
| 5 | Not found as a mesh. Likely Vfx33 motes/sparks quads or bloom (unverified) | Vfx33.cs:91-135, Kit33Props.cs:218-355 | NIGHT33 L (sha not checked) |
