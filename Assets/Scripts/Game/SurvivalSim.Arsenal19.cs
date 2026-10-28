// NIGHT19 U3-33.5 / U3-33.6 (builder W): the sim's half of the weapon sandbox, the hooks the shared commit placed in
// SurvivalSim.N19.cs. N19ArsenalStartRun runs in N19StartRun (the run's blocks are set, before the loadout's and the
// forge's resets); N19ArsenalStep right after the loadout's step. With every block off nothing here changes a run:
// - the ARMORY wall's ports (ZAPPER, BOUNCER: Loadout.Fires lists them when the design's arsenal block is on at boot)
//   are for sale only in a run with the sandbox on (Loadout.Arsenal19On); the box's pool takes them on the same
//   condition (MysteryBox.ports);
// - design.forge: the FORGE's sidegrade (a flat x1.4, the gun's variant on; Forge.n19Sidegrade, Loadout.forgedVariant);
// - each step: launched enemies that landed come down, nails stuck in each enemy are counted (Loadout.Step19).
// NIGHT20 U3-34.8 (builder G): W's work is merged behind design.arsenal.n20 as well (Loadout.Arsenal19On / Forge19On /
// Fresh19On: the NIGHT19 block or the NIGHT20 one), so the run's flags are applied again in N20ArsenalStartRun
// (SurvivalSim.Arsenal20.cs) once the NIGHT20 blocks are set: Apply19 below.
// No UnityEngine use (SimHarness compiles SurvivalSim*.cs).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        bool arsenal19Wired;
        Func<string, bool> forgedOf; // forge.IsForged, made once

        partial void N19ArsenalStartRun()
        {
            if (!arsenal19Wired)
            {
                arsenal19Wired = true;
                forgedOf = forge.IsForged;
                foreach (WallGun gun in wallGuns)
                {
                    if (!Loadout.IsPort19(gun.id)) continue;
                    string id = gun.id;
                    gun.item.available = () => loadout.Arsenal19On && !loadout.owned.Contains(id);
                }
            }
            Apply19();
        }

        /// <summary>The run's sandbox flags into the forge, the loadout and the box (N19ArsenalStartRun, then again in
        /// N20ArsenalStartRun once the NIGHT20 blocks are set).</summary>
        void Apply19()
        {
            forge.n19Sidegrade = loadout.Forge19On;
            loadout.forgedVariant = loadout.Forge19On ? forgedOf : null;
            if (box != null) box.ports = loadout.Arsenal19On;
        }

        partial void N19ArsenalStep(double dt) => loadout.Step19();
    }
}
