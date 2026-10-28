// NIGHT57 builder G (G2 press57, sim row design.n57.press57; Core/Press57.cs): the sim side. NIGHT57's row hooks (SurvivalSim.Rows.cs:
// Night57StartRun / Night57Step, empty partials until tonight) are implemented HERE and dispatch to press57; a later NIGHT57 sim row adds its
// own call in these two bodies. At a run's start the row's bit and numbers (the acid's marks cleared); every step (after the enemies stepped)
// the acid's on57 = campaignOn (a CAMPAIGN run only, as tokens56: survival keeps the web's spitter; campaignOn is known only after N24StartRun)
// and the marks age. A spitter's commit ("spitterTell", once subscribed, the first run with the row on) marks the spot under its target's
// feet (the nearest player's, as Husks.StepPool picks it) and draws the telegraph's red-orange danger circle there (TelegraphView: the Game's
// Telegraph20; null in the harness). Off (the bit false, --off57 / -skygrindOff57): acid.on57 false, the handler returns at its first test:
// NIGHT56's game bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        internal int idPress57 = -1;
        bool press57On, press57Hooked;
        Press57Params press57P;

        /// <summary>press57 on for this run (the harness and the reels read it).</summary>
        public bool Press57On => press57On;

        partial void Night57StartRun()
        {
            N57PressStartRun(); // G2 press57
        }

        partial void Night57Step(double dt)
        {
            N57PressStep(dt); // G2 press57
        }

        void N57PressStartRun()
        {
            idPress57 = rowsRegistry.IndexOf("press57");
            press57On = rows.On(idPress57);
            SpitterAcid acid = husks?.acid;
            if (acid == null) return;
            acid.on57 = false;
            acid.Clear57(TelegraphView);
            if (!press57On) return;
            press57P ??= Press57Params.From(cfg);
            acid.p57 = press57P;
            acid.Alloc57();
            if (press57Hooked) return;
            press57Hooked = true;
            events.On("spitterTell", N57PressTell);
        }

        void N57PressStep(double dt)
        {
            if (!press57On || husks == null) return;
            husks.acid.on57 = campaignOn;
            if (campaignOn) husks.acid.Step57(dt, TelegraphView);
        }

        /// <summary>A spitter's commit (its tell's first step): MARK the spot under its target's feet (Core/Press57.cs).</summary>
        void N57PressTell(object o)
        {
            if (!press57On || !campaignOn || husks == null || !(o is int id)) return;
            foreach (Husk h in husks.PoolOf("spitter"))
            {
                if (h.id != id || !h.alive || !(h is Spitter sp)) continue;
                Vec3 feet = players.Count > 0 ? players.Nearest(sp.position).Feet : mover.position;
                husks.acid.Mark57At(sp, feet, sp.stateSteps * step, TelegraphView);
                return;
            }
        }
    }
}
