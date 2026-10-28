// NIGHT17 U3-30.3 (the shared commit; builder M owns this file from now): the one place the views read the stamina bar.
// The HUD's bar (builder H: UiStaminaBar), the sprint's camera cue (builder X: CameraFeel) and the sprint pose (builder V:
// the viewmodel) read StaminaHud.Read, never the Mover's fields, so they build against this contract while M builds
// Core/Stamina.cs. NIGHT17 U3-30.4 (builder M): with design.stamina's bar on (the default) it reads the bar (Mover.stamina:
// fill 0 .. StaminaMax, 1 bar or 2 with VIGOR; boostCost = the full boost's cost, boostFloor = the least fill a boost fires
// with; sprinting from the sprint's start on the ground to its end, a hop or a slide included); with the rules off (a
// format 1-2 tape's replay, the web's config) the web's pips (max = MaxPips, cost = dash.pipCost, never sprinting). No
// allocation (a struct by ref).
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class StaminaHud
    {
        /// <summary>What a view needs to draw the bar and the movement cues.</summary>
        public struct Info
        {
            /// <summary>True when there is a mover to read.</summary>
            public bool valid;
            /// <summary>The bar now, 0 .. max (bars; the web's pips when the rules are off).</summary>
            public double fill;
            /// <summary>The bar's size (1 bar, 2 with VIGOR; MaxPips when the rules are off).</summary>
            public double max;
            /// <summary>A full boost's cost, and the least fill a boost fires with (its floor x cost; = cost for pips).</summary>
            public double boostCost, boostFloor;
            /// <summary>Sprinting now (always false with the pips), boosting now (the dash).</summary>
            public bool sprinting, boosting;
            /// <summary>NIGHT17 round 5: the sim's refill ran on the last step (StaminaBarLook.Refilling: past its delay, not boosting
            /// or sprinting, under the max; the pips: under MaxPips, not dashing).</summary>
            public bool refilling;
            /// <summary>The design.stamina rules are on for this run (false: the web's pips).</summary>
            public bool bar;
            /// <summary>NIGHT18 U3-31.5: the boost key held on since its press with the sprint rule on (one key: the sprint follows
            /// the boost on the ground), so the camera's sprint widen starts at the press (Mover.stamina.armed).</summary>
            public bool sprintArmed;
        }

        public static void Read(SurvivalGame g, ref Info i)
        {
            Mover m = g != null && g.sim != null ? g.mover : null;
            i.valid = m != null;
            if (m == null) return;
            i.boosting = m.state == MoveState.Dash;
            i.refilling = StaminaBarLook.Refilling(m);
            if (m.stamina.rules.Any)
            {
                // NIGHT17 U3-30.4 (builder M): design.stamina's bar (Core/Stamina.cs)
                Stamina.Params p = m.stamina.rules;
                i.fill = m.stamina.bar;
                i.max = m.StaminaMax;
                i.boostCost = p.cost;
                i.boostFloor = Stamina.FloorFill(p);
                i.sprinting = m.stamina.sprinting;
                i.sprintArmed = p.sprint && m.stamina.armed;
                i.bar = true;
                return;
            }
            double cost = m.Config.dash.pipCost;
            i.fill = m.pips;
            i.max = m.MaxPips;
            i.boostCost = cost;
            i.boostFloor = cost;
            i.sprinting = false;
            i.sprintArmed = false;
            i.bar = false;
        }
    }
}
