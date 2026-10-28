// NIGHT30 builder V (item 3; design.n30.lava, D-6): the player's LAVA HURT FLASH. On builder S's `lavaBurn` event (a touch: 25 % of max health and the
// upward knock) the HUD's frame layer shows a red-orange INSET EDGE (the power-up edge's sprite: the screen's border, the centre clear: R2's "under 25 % of
// the screen") that fades over design.n30.lava.hurtFlashSeconds (0.6 s, S's config row: never typed here). Through game.flashGate (at most 3 flashes a
// second) and x FLASH INTENSITY (0: none). Hidden with the HUD under a menu (the frame layer). With -skygrindOff30 or the run's lava block off nothing
// subscribes. The event is read by name (any payload).
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        UnityEngine.UI.Image lavaEdge30;
        double lavaFlashLeft30, lavaFlashLen30;
        float lavaEdgeShown30 = -1;

        /// <summary>Lava hurt flashes started (the reel `lava30` reads it).</summary>
        public static int LavaFlashes30;

        void BuildLava30()
        {
            if (!N30Fixes.On || game.events == null) return;
            lavaEdge30 = Img(frame, "Lava edge 30", UiTheme.A(UiTheme.Danger, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            lavaEdge30.sprite = InsetShadowSprite(UnityTuning.ChargeEdgeBlur, UnityTuning.ChargeEdgeSpread, 1);
            lavaEdge30.type = UnityEngine.UI.Image.Type.Sliced;
            lavaEdge30.raycastTarget = false;
            lavaEdge30.transform.SetAsFirstSibling(); // under the HUD's plates
            lavaEdge30.enabled = false;
            game.events.On("lavaBurn", o =>
            {
                if (game.sim == null || !game.sim.n30.lava) return;
                if (!(game.settings.Num("flashIntensity") > 0) || !game.flashGate.TryFlash(SurvivalGame.RealNow)) return;
                lavaFlashLen30 = cfg.Has("design.n30.lava.hurtFlashSeconds") ? cfg.Num("design.n30.lava.hurtFlashSeconds") : 0;
                lavaFlashLeft30 = lavaFlashLen30;
                LavaFlashes30++;
            });
        }

        void RenderLava30(double frameDt)
        {
            if (lavaEdge30 == null) return;
            lavaFlashLeft30 = System.Math.Max(0, lavaFlashLeft30 - frameDt);
            float a = lavaFlashLen30 > 0 ? (float)(lavaFlashLeft30 / lavaFlashLen30) * UnityTuning.Lava30FlashAlpha * Mathf.Clamp01((float)game.settings.Num("flashIntensity")) : 0f;
            a = Mathf.Round(a * 100f) / 100f;
            if (a == lavaEdgeShown30) return;
            lavaEdgeShown30 = a;
            lavaEdge30.enabled = a > 0;
            lavaEdge30.color = UiTheme.A(UiTheme.Danger, a);
        }
    }
}
