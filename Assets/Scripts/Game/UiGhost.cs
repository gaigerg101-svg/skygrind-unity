// NIGHT14 U3-27.11 (agent V2): SPLITS (design.ghost; ULTRAKILL's clock on CoD's rounds; port-only). At each round's start
// on a seed with a stored best, one HUD line under the round line and EXPOSED: "ROUND 5 · -12.4 s" (this run's start of the
// round against the best's; negative = ahead, in colors.usable (cyan); level or behind with a "+", in the theme's muted
// text: never red-orange, the danger's). It shows for UnityTuning.GhostSplitShow s, fading in and out over GhostSplitFade s.
// The text is set once per split (TMP SetText with the numbers: no string), the alpha only when it changes; it lives in the
// HUD's values layer, so it hides with the HUD. design.ghost (or design.route) off: nothing is built (the old HUD).
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        TextMeshProUGUI ghostSplitLabel;
        int ghostSplitShownVersion;
        double ghostSplitAge;
        float ghostSplitAlpha = -1, ghostSplitBase = 1;

        /// <summary>Automation: the split line is showing, and its text (built on read).</summary>
        public bool GhostSplitVisible => ghostSplitLabel != null && ghostSplitLabel.gameObject.activeInHierarchy;
        public string GhostSplitShownText => ghostSplitLabel != null && ghostSplitLabel.gameObject.activeSelf ? ghostSplitLabel.text : null;

        partial void BuildGhostSplits()
        {
            if (!cfg.Design("route.enabled") || !cfg.Design("ghost.enabled")) return;
            ghostSplitLabel = Text(values, "Ghost split", "", tH3, muted, TextAlignmentOptions.Center, new Vector2(0, -margin - UnityTuning.GhostSplitBelow),
                new Vector2(UnityTuning.GhostSplitWidth, UnityTuning.GhostSplitHeight), new Vector2(0.5f, 1), Face.Display);
            ghostSplitLabel.gameObject.SetActive(false);
        }

        partial void RenderGhostSplits(double frameDt)
        {
            if (ghostSplitLabel == null) return;
            int version = game.GhostSplitVersion;
            if (version != ghostSplitShownVersion)
            {
                ghostSplitShownVersion = version;
                float seconds = (float)game.GhostSplitSeconds;
                bool ahead = seconds < 0;
                if (ahead) ghostSplitLabel.SetText("ROUND {0:0} · {1:1} s", game.GhostSplitRound, seconds);
                else ghostSplitLabel.SetText("ROUND {0:0} · +{1:1} s", game.GhostSplitRound, seconds);
                ghostSplitLabel.color = ahead ? usable : muted;
                ghostSplitBase = ghostSplitLabel.color.a; // muted is the text at its muted alpha: the fade scales it
                ghostSplitAge = 0;
                ghostSplitAlpha = -1;
                if (!ghostSplitLabel.gameObject.activeSelf) ghostSplitLabel.gameObject.SetActive(true);
            }
            if (!ghostSplitLabel.gameObject.activeSelf) return;
            ghostSplitAge += frameDt;
            if (ghostSplitAge >= UnityTuning.GhostSplitShow)
            {
                ghostSplitLabel.gameObject.SetActive(false);
                return;
            }
            double fade = UnityTuning.GhostSplitFade;
            float a = (float)System.Math.Min(1, System.Math.Min(ghostSplitAge / fade, (UnityTuning.GhostSplitShow - ghostSplitAge) / fade));
            if (a == ghostSplitAlpha) return;
            ghostSplitAlpha = a;
            ghostSplitLabel.alpha = a * ghostSplitBase;
        }

    }
}
