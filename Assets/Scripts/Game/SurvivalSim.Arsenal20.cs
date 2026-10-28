// NIGHT20 U3-34.3 / U3-34.8 / U3-34.9 (builder G): the sim's half of tonight's guns, the hooks the shared commit placed in
// SurvivalSim.N20.cs. N20ArsenalStartRun runs in N20StartRun (the run's NIGHT20 blocks are set and handed to the loadout,
// before the loadout's and the forge's resets); N20ArsenalStep right after the loadout's step (and N19ArsenalStep).
// - design.arsenal.n20: NIGHT19 W's sandbox (Sandbox19, the ZAPPER / BOUNCER ports, the forge's sidegrade, the freshness
//   table) on for the run (Loadout.Arsenal19On / Forge19On / Fresh19On), applied to the forge and the box (Apply19).
// - design.shockwave: the loadout learns which guns are forged (Loadout.forgedAny = Forge.IsForged): the THUNDERCLAP's
//   second charge.
// - design.adds.b3 (B3, THE FORGE in tiers): the run's tier switch (Forge.tiered) and its clock (Forge.now, FORGE II's proc
//   cooldown); THE FORGE's cabinet sells the gun in hand's next tier (Forge.NextPrice / CanUpgrade / Upgrade). Untiered,
//   those three are NIGHT19's cabinet exactly (forge.cost, once per gun, Forge.Give).
// With every NIGHT20 block off nothing here changes a run (Apply19 repeats what N19ArsenalStartRun set; the cabinet's
// answers are the old ones).
// No UnityEngine use (SimHarness compiles SurvivalSim*.cs).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        bool forgeTiersWired;

        partial void N20ArsenalStartRun()
        {
            Apply19(); // design.arsenal.n20 turns W's sandbox on: the forge's sidegrade, the box's ports
            loadout.forgedAny = forgedOf; // design.shockwave: a forged THUNDERCLAP holds a second charge (read only with the block on)
            forge.tiered = n20.Add(cfg, "b3"); // B3: THE FORGE in tiers this run
            if (box != null) box.ports20 = n20.Add(cfg, "b5"); // B5: the TORCH and the VORTEX in the box
            if (!forgeTiersWired)
            {
                forgeTiersWired = true;
                foreach (WallGun gun in wallGuns)
                {
                    if (!Loadout.IsPort20(gun.id)) continue;
                    string id = gun.id;
                    gun.item.available = () => loadout.B5On && !loadout.owned.Contains(id); // B5: for sale only in a run with the row on
                }
                foreach (Machine m in machines)
                {
                    if (!m.forge) continue;
                    Machine fm = m;
                    // the cabinet: the gun in hand's next tier (untiered: forge.cost, once per gun, as before)
                    fm.item.price = () => forge.NextPrice(loadout.Weapon);
                    fm.item.available = () => (fm.zone < 0 || !zones.LockedTag(fm.zone)) && forge.CanUpgrade(loadout.Weapon);
                    fm.item.buy = () => forge.Upgrade(loadout.Weapon, forge.NextPrice(loadout.Weapon));
                }
            }
        }

        partial void N20ArsenalStep(double dt) => forge.now = simStep * step; // B3: FORGE II's proc clock
    }
}
