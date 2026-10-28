// NIGHT22 leftover 4 (builder M): TRICK CHAIN in the run (design.n22.trickChain; the rule is Core/Trick22.cs). A sim change, entirely behind
// sim.n22.trickChain: with the row off every handler returns at once and nothing is drawn, so the run is NIGHT21's bit for bit (the handlers
// are registered once per sim; registering changes nothing). With it on, a new distinct trick inside the window emits one `styleBonus` named
// "CHAIN xN" (the style meter's own named-bonus path, as SLIDE BITE): the smallest blast radius, no points rule and no Mover change.
// Listens to the existing events only: "trick" (the mover's tricks as SurvivalSim maps them, and the slingshot), "slamUse", "ropeAttach".
// Not saved by the run snapshot (a break loses the open chain, like the yank's cooldown).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The run's chain (null until the first run with the row on).</summary>
        public Trick22Chain trick22;

        bool trick22Wired;

        partial void N22TrickStartRun()
        {
            if (!n22.trickChain)
            {
                trick22?.Reset();
                return;
            }
            trick22 ??= new Trick22Chain(cfg, step);
            trick22.Reset();
            if (trick22Wired) return;
            trick22Wired = true;
            events.On("trick", o => Trick22Hit((string)o));
            events.On("slamUse", _ => Trick22Hit("slam"));
            events.On("ropeAttach", _ => Trick22Hit("swing"));
        }

        /// <summary>The style meter's own test for a trick to pay: a live enemy within style.trickEnemyRange of the player.</summary>
        bool Trick22EnemyNear()
        {
            double r = cfg.Num("style.trickEnemyRange");
            foreach (Enemy e in enemies.all)
            {
                if (!e.alive) continue;
                double dx = e.position.x - mover.position.x, dy = e.position.y - mover.position.y, dz = e.position.z - mover.position.z;
                if (dx * dx + dy * dy + dz * dz <= r * r) return true;
            }
            return false;
        }

        partial void N22TrickStep(double dt)
        {
            if (n22.trickChain) trick22.Step();
        }

        void Trick22Hit(string name)
        {
            if (!n22.trickChain || trick22 == null) return;
            double pts = trick22.Trick(name);
            if (pts <= 0 || !Trick22EnemyNear()) return; // like the web's tricks: they pay near an enemy (style.trickEnemyRange)
            events.Emit("styleBonus", new StyleBonusEvent { owner = "player", name = "CHAIN x" + trick22.Links, points = pts });
        }
    }
}
