// NIGHT71 builder B (slate B1, design.n71.voice71; the rule and the counter: Voice71.cs). The HUD's half, read (never written): does the HUD
// draw the boss's window order this frame? The order is the bar's hint while it holds the window's text (design.n43.wick43.bosses.window,
// the sim's hint for THE CROWN while its window is open); it is drawn by hud47's fight line (drawn, its text the bar's order: Order64.Same,
// the test order64 uses) or by the bar's hint itself (its holder drawn: order64 and bossRule61 hide it on their frames). Called from THE
// CROWN's LateUpdate (Crown37View.Voice71.cs), after this frame's HUD (Ui.Render in SurvivalGame.Update: Fight47Late, Order64Late and
// FightLine64Late have written), so it reads what the frame shows. Allocation-free.
using System;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        string v71Window;
        bool v71Read;

        /// <summary>The HUD draws the boss's window order this frame (`line`: by the fight line, `bar`: by the bar's hint).</summary>
        public bool Voice71HudOrder(out bool line, out bool bar)
        {
            line = bar = false;
            if (!v71Read)
            {
                v71Read = true;
                try { v71Window = cfg?.Str(Voice71.WindowPath); }
                catch (Exception) { v71Window = null; }
            }
            if (string.IsNullOrEmpty(v71Window) || b43Root == null || b43Hint == null || !b43Root.gameObject.activeSelf) return false;
            if (!string.Equals(b43ShownHintRaw, v71Window, StringComparison.Ordinal)) return false; // the bar's order is not the window's now
            string order = b43Hint.Shown;
            if (order.Length == 0) return false;
            bar = b43Hint.rt.gameObject.activeInHierarchy;
            line = fight47 != null && fight47.rt.gameObject.activeInHierarchy && fight47.Alpha > 0f && Order64.Same(fight47Shown, order);
            return line || bar;
        }
    }
}
