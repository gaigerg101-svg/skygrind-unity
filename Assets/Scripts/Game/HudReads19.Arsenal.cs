// NIGHT20 U3-34.3 / U3-34.8 (builder G): the HUD's reads that HudReads19.cs declares (H's file) for the guns' sim:
// - ReadAlt, the gun in hand's alt meter: the THUNDERCLAP's shockwave with design.shockwave (its charges of 1, or 2 forged,
//   the next charge's share of the 18 s refilled: Loadout.shockwave), NIGHT19's KESTREL gust with its block and the
//   shockwave off (the ground charge after its cooldown plus the airtime's charge: KestrelGust.ChargesReady, of 2);
// - ReadFreshTier, the tier of a gun on ULTRAKILL's freshness table (design.freshness, or design.arsenal.n20 which turns
//   it on: Arsenal.fresh19On), 0 FRESH .. 3 DULL from Freshness19Params' thresholds.
// Read every frame: no allocation (numbers only; the weapon id is the hotbar's string).
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class HudReads19
    {
        static partial void ReadAlt(SurvivalGame g, string weapon, ref Alt a)
        {
            Loadout lo = g.loadout;
            if (lo == null) return;
            if (weapon == "shotgun" && lo.ShockwaveOn)
            {
                Shockwave sw = lo.shockwave;
                a.valid = true;
                a.charges = sw.Charges;
                a.max = sw.MaxCharges;
                double cd = sw.Cooldown;
                a.next = sw.CooldownLeft <= 0 || cd <= 0 ? 1 : 1 - sw.CooldownLeft / cd;
                return;
            }
            if (weapon == "revolver" && lo.GustOn && lo.gust != null && lo.gust.p != null)
            {
                a.valid = true;
                a.charges = lo.gust.ChargesReady;
                a.max = 2;
                a.next = lo.gust.Refilled(g.cfg.Dt);
            }
        }

        static partial void ReadFreshTier(SurvivalGame g, string weapon, ref int tier)
        {
            Loadout lo = g.loadout;
            Arsenal ar = lo?.arsenal;
            if (ar == null || !ar.fresh19On || ar.f19 == null) return;
            double v = ar.Value19(weapon);
            Freshness19Params p = ar.f19;
            tier = v >= p.freshAt ? 0 : v >= p.usedAt ? 1 : v >= p.staleAt ? 2 : 3;
        }
    }
}
