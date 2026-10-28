// NIGHT20 U3-34.10 row 5 (builder H): the hook surface for the HUD's package effects (UIEffect 5.9.7 and LitMotion v2.0.2,
// both MIT, pinned by the lead: Packages/manifest.json). Skygrind.Game doesn't reference the packages (its asmdef is the
// lead's); a small assembly of H's, Skygrind.HudFx20 (Assets/Scripts/HudFx20: its asmdef references Skygrind.Game, LitMotion,
// LitMotion.Extensions and Coffee.UIEffect), installs these at start-up ([RuntimeInitializeOnLoadMethod]). Without it every
// hook is null and the HUD is as before (nothing else depends on them).
// - Tick: once a frame from Ui.Render with the HUD's frame time: the effects run on LitMotion's manual dispatcher, so they
//   keep the HUD's frame clock (the reel's fixed one included), in every flow (TITLE, RESULTS).
// - Reveal: a per-character reveal on a text just set (the announcer's slab, the debut card's name): each character fades
//   and rises into place in turn (UnityTuning.Hud20FxCharStagger apart).
// - EdgeGlow: a plate's edge glow, made once when the plate is built (the slab's and the debut card's plates only: banned
//   pattern 3, "glows on everything").
// - Shine: one shine sweep across a graphic now (the style meter's letter on a rank-up only).
// UnityTuning.Hud20FxOn switches all of it off in one place.
using System;
using TMPro;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public static class HudFx20
    {
        public static Action<double> Tick;
        public static Action<TMP_Text> Reveal;
        public static Action<Graphic> EdgeGlow;
        public static Action<Graphic> Shine;

        /// <summary>The effects are installed and switched on.</summary>
        public static bool On => UnityTuning.Hud20FxOn && Tick != null;

        public static void DoTick(double frameDt)
        {
            if (On) Tick(frameDt);
        }

        public static void DoReveal(TMP_Text t, bool reducedMotion)
        {
            if (On && t != null && !reducedMotion) Reveal?.Invoke(t);
        }

        public static void DoEdgeGlow(Graphic g)
        {
            if (On && g != null) EdgeGlow?.Invoke(g);
        }

        public static void DoShine(Graphic g, bool reducedMotion)
        {
            if (On && g != null && !reducedMotion) Shine?.Invoke(g);
        }
    }
}
