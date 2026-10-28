// NIGHT34 builder B (leftover 1, design.n34.air34): THE AIR MULTIPLIER in the sim (the rule is Core/Air34.cs). Called from B's own hooks
// (SurvivalSim.Bend34.cs): Air34StartRun from N34BendStartRun, Air34Step from N34BendBeginStep (the top of every Step, before anything moves:
// it reads the Mover as the last step left it, so this step's kills pay the value the player sees). Airborne = !mover.grounded; sliding =
// MoveState.Slide. The value goes to the style meter (StyleCore.mult34, every award; the web's flat stateMult off while on) and to the combat
// points (Economy boost "air34": hits, kills, parries). Damage never reads it.
// Off (the row, --off34 and every older off switch): the meter's air34 false, mult34 1, no boost: NIGHT33's points and style bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT34: design.n34.air34's numbers (null until a run starts with the row on).</summary>
        public Air34Rules air34Rules;

        /// <summary>NIGHT34: the air multiplier's state this run.</summary>
        public readonly Air34 air34 = new Air34();

        /// <summary>NIGHT34: the air multiplier now (1 with the row off): the HUD's read.</summary>
        public double Air34Mult => n34.air34 && air34Rules != null ? air34.mult : 1;

        bool a34Boosted;

        void Air34StartRun()
        {
            if (a34Boosted)
            {
                economy.SetBoost("air34", 1);
                a34Boosted = false;
            }
            style.core.air34 = false;
            style.core.mult34 = 1;
            if (!n34.air34) return;
            air34Rules ??= Air34Rules.From(cfg);
            air34.Reset(air34Rules);
            style.core.air34 = true;
        }

        void Air34Step()
        {
            if (!n34.air34 || air34Rules == null) return;
            double was = air34.mult;
            air34.Step(!mover.grounded || mover.state == MoveState.Slide, step);
            style.core.mult34 = air34.mult;
            if (air34.mult == was && a34Boosted == (air34.mult != 1)) return;
            economy.SetBoost("air34", air34.mult);
            a34Boosted = air34.mult != 1;
        }
    }
}
