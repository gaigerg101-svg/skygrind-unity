// NIGHT20 U3-34.5 (builder E owns this file): design.regen.n20 in the sim (Core/Health.cs: RegenN20's rules and Regen's
// block path), through the shared commit's hook (SurvivalSim.N20.cs: N20RegenStartRun at the run's start, right after the
// run takes its blocks; regen.Step runs the block's rules itself, so N20RegenStep has nothing to add):
// - the run hands Regen the block (UseN20: on with the run's n20.regen, off otherwise: NIGHT19's regen, bit for bit);
// - `roundClear` starts the refill to full over refillTime (Regen.StartRefill: `roundHeal`, the HUD's refill under the round
//   banner); nothing with the block off or with HEAT's NO REGEN (regen disabled);
// - a player's kill earns its heal (RegenN20.KillHeal: the punch's punchKillHeal, else closeKillHeal within closeKillRange m
//   of the player's eye, as the stamina rule measures a kill: Regen.Earn: the web's `heal`, then `earnedHeal`), past the
//   segments' lines and with NO REGEN too. The parry's heals and BLOOD PRICE are unchanged (Combat.cs, Mods.cs).
// The handlers are made once (no allocation per step); each reads the run's block through Regen.N20.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT20: design.regen.n20's numbers, read once (the block's rules need them all; absent: never on).</summary>
        RegenN20.Params regen20;

        bool regen20Known, regen20Wired;

        void Regen20Wire()
        {
            regen20Wired = true;
            regen20Known = cfg.Has("design.regen.n20.segment");
            if (regen20Known) regen20 = RegenN20.Params.From(cfg);
            events.On<RoundEvent>("roundClear", e => regen.StartRefill(e.number)); // nothing unless the block is on (Regen.N20)
            events.On<KillEvent>("kill", Regen20OnKill);
        }

        void Regen20OnKill(KillEvent k)
        {
            if (!regen.N20 || k.owner != "player") return;
            Vec3 p = mover.position;
            double dx = k.position.x - p.x, dy = k.position.y - (p.y + cfg.movement.body.eyeHeight), dz = k.position.z - p.z;
            double heal = RegenN20.KillHeal(regen20, k.weapon, JsMath.Hypot(dx, dy, dz), out string kind);
            if (heal > 0) regen.Earn(heal, kind);
        }

        partial void N20RegenStartRun()
        {
            if (!regen20Wired) Regen20Wire();
            regen.UseN20(n20.regen && regen20Known, regen20);
        }
    }
}
