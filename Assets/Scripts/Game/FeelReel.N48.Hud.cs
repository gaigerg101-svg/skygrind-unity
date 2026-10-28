// NIGHT48 builder F: the a648 phase, G5's HUD text side (W owns the fight lines; F checks the HUD's own text at 720p). Run the same Player twice:
//   reel.sh + -skygrindMap <a campaign level> + -skygrindReelOnly a648 + -screen-width 1280 -screen-height 720 -screen-fullscreen 0
//   and again at -screen-width 1920 -screen-height 1080
// The CAMPAIGN on the booted map, enemies frozen, God; the HUD as it plays (a room's counter, the health bar, the weapon panel), then the
// gauntlet in hand (its rail), then a boss hint posted (the fight line's slot). Per moment, every TMP text the frame draws (its canvas enabled
// and in the hierarchy, its alpha > 0, its text not empty) is measured: its font size, its DRAWN em in px (font size x the root canvas's scale
// factor: the canvas scales with the height from 1920 x 1080, so 1 unit is 1 px at 1080p and 0.667 px at 720p), its cap height in px (the
// em x the font's cap line / its point size, from the font asset's face info), and its 1080p equivalent. Rules (CLAUDE.md rule 8; NIGHT48.md
// G5): every HUD text >= 16 px at 1080p (a 720p em >= 10.7 px), a fight line >= 36 px at 1080p and >= 24 px at 720p. Frames
// shot-N30-H48-<moment>-<w>x<h>.png. GREEN when no drawn text is under its rule.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N48Hud(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Hud48Scenario(g, dir, res, offs, pre);

        /// <summary>Every TMP text drawn now: (name, text's first 24 chars, font size, drawn em px, cap px, the 1080p em).</summary>
        static List<(string name, string text, float size, float em, float cap, float em1080)> Texts48()
        {
            var r = new List<(string, string, float, float, float, float)>();
            foreach (TMPro.TMP_Text t in UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (t == null || !t.isActiveAndEnabled || string.IsNullOrEmpty(t.text) || t.alpha <= 0.01f) continue;
                Canvas c = t.canvas;
                if (c == null || !c.enabled) continue;
                Canvas root = c.rootCanvas;
                if (root == null || !root.enabled || root.renderMode == RenderMode.WorldSpace) continue;
                CanvasGroup grp = t.GetComponentInParent<CanvasGroup>();
                if (grp != null && grp.alpha <= 0.01f) continue;
                if (t.rectTransform.lossyScale.y <= 1e-4f) continue; // a scaled-away layer (Hud30's menu hide)
                float scale = root.scaleFactor > 0 ? root.scaleFactor : 1f;
                float size = t.fontSize * (t.rectTransform.lossyScale.y / Mathf.Max(1e-4f, root.transform.lossyScale.y));
                float em = size * scale;
                float capShare = 0.7f;
                if (t.font != null && t.font.faceInfo.pointSize > 0) capShare = t.font.faceInfo.capLine / t.font.faceInfo.pointSize;
                string s = t.GetParsedText();
                if (string.IsNullOrEmpty(s)) s = t.text;
                s = s.Replace("\n", " ");
                if (s.Length > 24) s = s.Substring(0, 24);
                r.Add((t.gameObject.name, s, size, em, em * capShare, em * 1080f / Mathf.Max(1, Screen.height)));
            }
            return r;
        }

        static IEnumerator Hud48Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string why = N24CampaignGate(g);
            if (why != null) { res[0] = why; yield break; }
            N24StartCampaign(g, offs);
            SurvivalSim sim = g.sim;
            offs.Add(() => { g.husks.frozen = false; g.forcedCombat = IdleCombat; });
            if (!sim.campaignOn) { res[0] = "RED a648: the run did not start as a CAMPAIGN"; yield break; }
            float wallEnd = Time.realtimeSinceStartup + 200;
            while ((sim.campaign == null || sim.campaign.room < 0) && Time.realtimeSinceStartup < wallEnd - 180) { AutopilotInput.TakeDraft(sim); yield return null; }
            yield return Frames(20);
            DoorFree41(g);
            God(g);
            if (sim.input is AutopilotInput) sim.UseInput(g.playerInput);
            g.forcedMove = new InputFrame();
            g.forcedCombat = IdleCombat;
            g.husks.frozen = true;
            string pc = Preconditions(g, g.Feet);
            Start(g, "a648-hud48", "camera", dir);
            yield return Frames(pre);
            string res0 = $"{Screen.width}x{Screen.height}";
            var lines = new List<string> { $"frame {res0}, hud47 {(Hud47Probe.Live ? "drawn" : "NOT drawn")}" };
            int under = 0, seen = 0;
            int[] mag = new int[1];
            bool forceWas = Bar43Probe.ForceOn;
            Boss43State stateWas = Bar43Probe.ForceState;
            offs.Add(() => { Bar43Probe.ForceOn = forceWas; Bar43Probe.ForceState = stateWas; });
            foreach (string moment in new[] { "play", "gauntlet", "boss" })
            {
                if (Time.realtimeSinceStartup > wallEnd) break;
                if (moment == "gauntlet") { yield return Equip27(g, "gauntlet"); g.forcedCombat = IdleCombat; }
                if (moment == "boss")
                {
                    // a boss bar with its hint (the fight line's slot: a647's forced state, a glyph in it)
                    Bar43Probe.ForceOn = true;
                    Bar43Probe.ForceState = new Boss43State { valid = true, name = "THE WICK", hp01 = 0.62, leg = 1, legs = 3, stagger01 = 0.4, hint = "{punch} the core!" };
                }
                for (int i = 0; i < 45; i++) { God(g); yield return null; }
                yield return Capture47(dir, $"H48-{moment}-{res0}", mag);
                var sb = new System.Text.StringBuilder($"{moment}:");
                foreach (var (name, text, size, em, cap, em1080) in Texts48())
                {
                    seen++;
                    bool fight = em1080 >= 34f; // a fight line / a big title (the 36 px class)
                    float need = fight ? (Screen.height <= 800 ? 24f : 36f) : 16f * Screen.height / 1080f;
                    bool bad = em + 0.01f < need;
                    if (bad) under++;
                    sb.Append($" [{name} '{text}' size {size.ToString("0", ci)} em {em.ToString("0.0", ci)} px cap {cap.ToString("0.0", ci)} px (1080p em {em1080.ToString("0", ci)}){(bad ? " UNDER " + need.ToString("0", ci) : "")}]");
                }
                lines.Add(sb.ToString());
            }
            yield return Stop();
            res[0] = $"{(under == 0 && seen > 0 ? "GREEN" : "RED")} a648 hud48 | {res0}: {seen} texts drawn, {under} under their rule (16 px at 1080p; a fight line 36 at 1080p / 24 at 720p) || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
