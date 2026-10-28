// NIGHT33 P (DESIGN33 card P, D-6, row design.n33.shop33): the sim half of the shop. N33ShopAfterStart (the end of StartRun: the economy, the health,
// the stamina, the grapple and the guns are set by then) applies the run's equipped upgrades (shop33Owned, Core/Shop33.cs's encoding). The run's own
// points stay NIGHT32's (the lead, 11:04: the wallet is a separate campaign currency the game banks at layerClear, SurvivalGame.Shop33.cs), so
// bank33 is not read here: it stays 0 on the tape. At layerClear the sim notes the level's earned points (shop33Earned, the harness's).
// Only while n33.shop33 is on; with it off, or nothing owned, the run is
// NIGHT32's bit for bit: the fields set every run (loadout.slugPierce33, projectiles.detonateMult33) are set to their neutral values, and the numbers
// changed in place on objects that live across runs (the charge's and the parry's seconds, the grapple's charges) are put back before the next run
// reads them (shop33Saved*), so a run never inherits another run's upgrades.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The catalogue (design.n33.shop33), read once.</summary>
        public Shop33Catalogue shop33Catalogue;

        /// <summary>This run's upgrade effects (Neutral with the row off or nothing equipped).</summary>
        public Shop33Effects shop33Effects = Shop33Effects.Neutral;

        /// <summary>The points this run's level earned by its layerClear (Economy.earned, whole); -1 until layerClear in a campaign run with the row on.</summary>
        public int shop33Earned = -1;

        /// <summary>The harness's counter: HP the kill heal restored this run.</summary>
        public double shop33Healed;

        bool shop33Wired;
        // The in-place changes to undo before the next run (NaN / -1: nothing changed).
        ChargeShots27 shop33ChargeObj;
        double shop33SavedCharge = double.NaN;
        LtAlways31 shop33LtObj;
        double shop33SavedParry = double.NaN;
        int shop33SavedGrapple = -1;

        partial void N33ShopStartRun()
        {
            shop33Catalogue ??= Shop33Catalogue.From(cfg);
            if (!shop33Wired)
            {
                shop33Wired = true;
                events.On<KillEvent>("kill", Shop33OnKill);
                events.On<RoomEvent>("layerClear", Shop33OnLayerClear);
            }
            shop33Earned = -1;
            shop33Healed = 0;
        }

        partial void N33ShopAfterStart()
        {
            Shop33Restore();
            bool on = n33.shop33;
            shop33Effects = on ? shop33Catalogue.Effects(shop33Owned) : Shop33Effects.Neutral;
            Shop33Effects e = shop33Effects;
            // set every run (neutral values are the defaults: x 1.0 and + 0 change nothing)
            loadout.slugPierce33 = e.slugPierce;
            if (projectiles != null) projectiles.detonateMult33 = e.detonateMult;
            if (!on || !e.Any) return;
            // the KESTREL's charge (NIGHT31's charge, or NIGHT27's with the n31 row off)
            ChargeShots27 c = loadout.charge27;
            if (e.kestrelChargeMult != 1 && c != null)
            {
                shop33ChargeObj = c;
                shop33SavedCharge = c.p.fullSeconds[0];
                c.p.fullSeconds[0] = shop33SavedCharge * e.kestrelChargeMult;
            }
            // the PULSE GAUNTLET's LT parry window (NIGHT31 ltAlways)
            if (e.parryWindowMult != 1 && lt31 != null && loadout.lt31 != null)
            {
                shop33LtObj = lt31;
                shop33SavedParry = lt31.p.parryWindowSeconds;
                lt31.p.parryWindowSeconds = shop33SavedParry * e.parryWindowMult;
            }
            // the grapple's charges (NIGHT32 grapple32): the max and the full count the run starts with
            if (e.grappleCharges != 0 && Grapple32On)
            {
                shop33SavedGrapple = grapple32Rules.charges;
                grapple32Rules.charges = shop33SavedGrapple + e.grappleCharges;
                grappleCharges32 = grapple32Rules.charges;
            }
            // the dash's recharge: the stamina bar's refill delays x mult, its rate / mult (StaminaStartRun set fresh rules this run)
            if (e.dashRecoverMult != 1 && mover.stamina.rules.Any)
            {
                Stamina.Params r = mover.stamina.rules;
                double dt = step;
                r.delay *= e.dashRecoverMult;
                r.emptyDelay *= e.dashRecoverMult;
                r.rate /= e.dashRecoverMult;
                r.delaySteps = MovementMath.StepsFor(r.delay, dt);
                r.emptyDelaySteps = MovementMath.StepsFor(r.emptyDelay, dt);
                mover.SetStamina(r);
            }
            // max health, the bar full (ResetWorld sets health.max from config each run, so it never stacks)
            if (e.maxHealth != 0)
            {
                health.max += e.maxHealth;
                health.hp = health.max;
            }
        }

        /// <summary>Undo the last run's in-place changes (objects that live across runs), before this run applies its own.</summary>
        void Shop33Restore()
        {
            if (!double.IsNaN(shop33SavedCharge) && shop33ChargeObj != null) shop33ChargeObj.p.fullSeconds[0] = shop33SavedCharge;
            shop33SavedCharge = double.NaN;
            shop33ChargeObj = null;
            if (!double.IsNaN(shop33SavedParry) && shop33LtObj != null) shop33LtObj.p.parryWindowSeconds = shop33SavedParry;
            shop33SavedParry = double.NaN;
            shop33LtObj = null;
            if (shop33SavedGrapple >= 0 && grapple32Rules != null)
            {
                // N32GrappleStartRun already filled this run's charges from the raised max: back to the config's
                grapple32Rules.charges = shop33SavedGrapple;
                grappleCharges32 = shop33SavedGrapple;
            }
            shop33SavedGrapple = -1;
        }

        void Shop33OnKill(KillEvent e)
        {
            if (!n33.shop33 || shop33Effects.killHeal <= 0 || e.owner != "player") return;
            shop33Healed += health.Heal(shop33Effects.killHeal);
        }

        void Shop33OnLayerClear(RoomEvent e)
        {
            if (!n33.shop33 || !campaignOn) return;
            shop33Earned = Shop33.Whole(economy.earned);
        }
    }
}
