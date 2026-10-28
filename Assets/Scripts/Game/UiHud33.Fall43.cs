// NIGHT43 builder U1 (with hud43; the lead's round-2 ask): the view of E1's fallReturn43 (rooms43: a fall below a room's belowY returns the player to
// the lip with damage, DECIDE43 D-5 "a short fade, never a death"). The event fires on the step the player is already back on the lip, so the fade is
// a BLINK, not a fade-out of the new spot: the screen goes to dusk in Fall43Out (near a cut), holds Fall43Hold, comes back over Fall43In (the lead's
// ~0.25 s out + 0.25 s in envelope). A full-screen DUSK sheet with scanlines at the back of the HUD layer (the HUD reads over it), plus the health
// bracket's red-orange hit pulse (hud43) even when the fall did no damage. The event is read by its name (the bus's object form), so this file needs
// none of E1's types. Off (hud43 off): nothing is built. Numbers: UnityTuning.Ui43.cs.
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 fallReturn43's view statics for the reels.</summary>
    public static class Fall43Probe
    {
        public static int Fades;
        public static float AlphaNow;
    }

    public sealed partial class Ui
    {
        Image f43Sheet;
        RawImage f43Scan;
        double f43Age = double.MaxValue;
        float f43Shown = -1f;

        /// <summary>From BuildHud43: the sheet at the back of the HUD layer and the event.</summary>
        void BuildFall43()
        {
            Color dusk = Look.Hex(UnityTuning.Blur43FlatHex);
            f43Sheet = UiStyle33.Line(hud33, "Fall fade 43", new Color(dusk.r, dusk.g, dusk.b, 0f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiKit43.Stretch(f43Sheet.rectTransform);
            f43Sheet.rectTransform.SetAsFirstSibling(); // behind every HUD element: the health, the counter and the card read over it
            f43Scan = UiKit43.Scan(f43Sheet.rectTransform, "Scan", 0f);
            f43Sheet.gameObject.SetActive(false);
            game.events.On("fallReturn43", (object o) =>
            {
                f43Age = 0;
                hp43HitAge = 0; // the bracket's red-orange pulse (a fall with no damage still reads as a hit)
                Fall43Probe.Fades++;
            });
            game.OnNewRun += () => { f43Age = double.MaxValue; f43Shown = -1f; if (f43Sheet != null) f43Sheet.gameObject.SetActive(false); };
        }

        /// <summary>The blink's alpha `age` s after the event (0 outside it).</summary>
        public static float Fall43Alpha(double age)
        {
            double o = UnityTuning.Fall43Out, h = UnityTuning.Fall43Hold, i = UnityTuning.Fall43In;
            if (age < 0 || age >= o + h + i) return 0f;
            float peak = UnityTuning.Fall43Peak;
            if (age < o) return peak * UiTween.OutCubic(age / o);
            if (age < o + h) return peak;
            return peak * (1f - UiTween.OutCubic((age - o - h) / i));
        }

        /// <summary>Every frame hud43 draws (from Health43Draw).</summary>
        void Fall43Draw(double frameDt)
        {
            if (f43Sheet == null) return;
            float a = f43Age < double.MaxValue ? Mathf.Round(Fall43Alpha(f43Age) * 50f) / 50f : 0f;
            if (f43Age < double.MaxValue)
            {
                f43Age += frameDt;
                if (f43Age > UnityTuning.Fall43Out + UnityTuning.Fall43Hold + UnityTuning.Fall43In) f43Age = double.MaxValue;
            }
            Fall43Probe.AlphaNow = a;
            if (a == f43Shown) return;
            f43Shown = a;
            bool on = a > 0f;
            if (f43Sheet.gameObject.activeSelf != on) f43Sheet.gameObject.SetActive(on);
            if (!on) return;
            Color c = f43Sheet.color;
            f43Sheet.color = new Color(c.r, c.g, c.b, a);
            f43Scan.color = new Color(1, 1, 1, UnityTuning.Ui43ScanAlpha * a);
        }
    }
}
