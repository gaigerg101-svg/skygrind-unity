// NIGHT25 (builder C): the sim's half of the three combat rows (design.n25.meleeFire, pulseCharge, shockwaveOff; the rules are Core/Guns/Pulse25.cs,
// Fist24's pulse mode and the shockwaveOff gates in Combat.cs / Shockwave.cs / Zoom20.cs). The hook N25CombatStartRun is the lead's
// (SurvivalSim.N25.cs); N25PulseWire is called from N24FistStartRun (the fist's object exists there, after the NIGHT25 hook ran).
// Off (--off25, a row off, a tape without the bit): every flag false and the loadout is NIGHT24's bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        partial void N25CombatStartRun()
        {
            loadout.meleeFire25 = n25.meleeFire;
            loadout.shockwaveOff25 = n25.shockwaveOff;
            loadout.shotgunZoom25 = n25.shockwaveOff ? cfg.Num("design.n25.shockwaveOff.zoom") : 0;
            loadout.ResetPulse25();
            loadout.Apply26(cfg, n26.pulseLT, n26.weaponIdentity); // NIGHT26 (builder W): the PULSE's blast on LT, the identity rows (Core/Guns/Weapon26.cs); both off: NIGHT25's loadout
        }

        /// <summary>After the fist's Reset: pulse mode on (the charge is the PULSE's) or off (NIGHT24's fist).</summary>
        void N25PulseWire()
        {
            bool on = (n25.pulseCharge || n26.pulseLT) && fist24 != null; // NIGHT26: pulseLT needs the charge too
            loadout.pulse25 = on;
            loadout.pulseFist = on ? fist24 : null;
            loadout.pulseP = on ? Pulse25Params.From(cfg) : null;
            fist24?.SetPulse(on, on ? loadout.pulseP.hitGain : 0);
        }
    }
}
