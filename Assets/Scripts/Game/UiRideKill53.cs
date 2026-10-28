// EXT53 builder N (row N, RIDE KILL; design.n53.rideKill53): the HUD's RIDE KILL callout. The sim's `rideKill53` event (SurvivalSim.N53.
// RideKill.cs: a kill on a wall ride or in its grace paid a boost back) slams the word in over the reticle: x RideKill53Punch on the kill's
// frame back to x1 over RideKill53PunchS (an overshoot), white-hot (CyanHot) for RideKill53HotS cooling to the theme's cyan (player-usable),
// the DISPLAY face with its INK shadow at 32 px (1080p), held RideKill53LifeS and faded over its last RideKill53FadeS. REDUCED MOTION: no punch.
// With the row off the event never fires: the text is built hidden and never shown (NIGHT53's HUD). Nothing allocates after the build.
// Numbers: UnityTuning.RideKill53.cs.
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        TextMeshProUGUI rk53Text;
        double rk53Age = -1; // s of HUD time since the last ride kill (-1: idle)
        bool rk53Shown;
        float rk53ShownScale = -1f, rk53ShownAlpha = -1f, rk53ShownHot = -1f;

        /// <summary>The reel's probe (a253): callouts started this session, the last one's frame, the callout's alpha and scale now.</summary>
        public static int RideKill53Callouts, RideKill53LastFrame = -1;
        public static float RideKill53Alpha, RideKill53Scale = 1f;
        /// <summary>The rendered word's box in screen px (bottom-left origin) at its last drawn frame (the reel's pixel count).</summary>
        public static Rect RideKill53ScreenRect;

        /// <summary>After BuildKillPop53: the word, hidden, and the bus's rideKill53 (the sim's own event: the row's switch).</summary>
        void BuildRideKill53()
        {
            rk53Text = Text(sight, "Ride kill callout", UnityTuning.RideKill53Word, UnityTuning.RideKill53Px, UiTheme.Cyan, TextAlignmentOptions.Center,
                new Vector2(0, UnityTuning.RideKill53Y), new Vector2(UnityTuning.RideKill53BoxW, UnityTuning.RideKill53BoxH), new Vector2(0.5f, 0.5f), Face.Display);
            UiTheme.Style(rk53Text, UnityTuning.RideKill53Px, UiTheme.Cyan, UiTheme.Display, true);
            rk53Text.enabled = false;
            rk53Text.richText = true;
            game.events.On<RideKill53Event>(RideKill53.Event, e =>
            {
                string word = e.after > e.before + 1e-9 ? UnityTuning.RideKill53WordPaid : UnityTuning.RideKill53Word; // two constants: no allocation
                if (!ReferenceEquals(rk53Text.text, word)) rk53Text.text = word;
                rk53Age = 0;
                RideKill53Callouts++;
                RideKill53LastFrame = Time.frameCount;
            });
        }

        /// <summary>Each frame (after RenderKillPop53): the word's punch, colour and fade while it lives.</summary>
        void RenderRideKill53(double frameDt, bool playing)
        {
            if (rk53Text == null) return;
            bool live = playing && rk53Age >= 0 && rk53Age < UnityTuning.RideKill53LifeS;
            if (!live)
            {
                if (rk53Shown) { rk53Text.enabled = false; rk53Shown = false; }
                if (!playing || rk53Age >= UnityTuning.RideKill53LifeS) rk53Age = -1;
                RideKill53Alpha = 0f;
                RideKill53Scale = 1f;
                return;
            }
            float age = (float)rk53Age;
            float s = 1f;
            if (!game.settings.View.reducedMotion && age < UnityTuning.RideKill53PunchS)
            {
                // OutBack from the punch to x1: 1 + (punch - 1) x (1 - eased), the overshoot dipping just under x1 before it settles
                float t = age / UnityTuning.RideKill53PunchS, c1 = 1.70158f, c3 = c1 + 1f, u = t - 1f;
                float eased = 1f + c3 * u * u * u + c1 * u * u;
                s = 1f + (UnityTuning.RideKill53Punch - 1f) * (1f - eased);
            }
            float fadeFrom = UnityTuning.RideKill53LifeS - UnityTuning.RideKill53FadeS;
            float a = age <= fadeFrom ? 1f : Mathf.Clamp01(1f - (age - fadeFrom) / UnityTuning.RideKill53FadeS);
            float hot = age < UnityTuning.RideKill53HotS ? 1f - age / UnityTuning.RideKill53HotS : 0f;
            // n53-r3 a253 (the lead's frame shot-N30-N53-ridekill-callout.png: no word on screen while the probe read alpha 1): with hud33 on, the
            // NIGHT32 layers (frame, values, sight) are kept running under a CanvasGroup at Hud33HiddenAlpha 0 (UiHud33.BuildHud33Now); what must
            // stay on screen is moved into HUD 33's "Kept" rect (Keep33: full-screen rects alike, so the place holds), drawn last
            if (hud33Kept != null && rk53Text.transform.parent != hud33Kept)
            {
                Keep33(rk53Text.transform);
                rk53Text.transform.SetAsLastSibling();
            }
            if (!rk53Shown) { rk53Text.enabled = true; rk53Shown = true; }
            if (s != rk53ShownScale)
            {
                rk53ShownScale = s;
                rk53Text.rectTransform.localScale = new Vector3(s, s, 1f);
            }
            if (a != rk53ShownAlpha || hot != rk53ShownHot)
            {
                rk53ShownAlpha = a;
                rk53ShownHot = hot;
                rk53Text.color = UiTheme.A(Color.Lerp(UiTheme.Cyan, UiTheme.CyanHot, hot), a);
            }
            // the probe reads what reaches the screen: the alpha through every parent CanvasGroup (0 under a hidden layer), and the rendered
            // word's screen box (an overlay canvas: world = screen px; the reel counts the callout's pixels in it)
            RideKill53Alpha = rk53Text.gameObject.activeInHierarchy ? a * rk53Text.canvasRenderer.GetInheritedAlpha() : 0f;
            RideKill53Scale = s;
            Vector2 rv = rk53Text.GetRenderedValues(true);
            Vector3 ls = rk53Text.rectTransform.lossyScale;
            Vector3 c = rk53Text.rectTransform.position;
            if (rv.x <= 0f || rv.y <= 0f) rv = new Vector2(UnityTuning.RideKill53BoxW, UnityTuning.RideKill53BoxH); // a mesh not built yet: the text's box
            float w = Mathf.Abs(rv.x * ls.x), h = Mathf.Abs(rv.y * ls.y);
            RideKill53ScreenRect = new Rect(c.x - w * 0.5f, c.y - h * 0.5f, w, h);
            rk53Age += frameDt;
        }
    }
}
