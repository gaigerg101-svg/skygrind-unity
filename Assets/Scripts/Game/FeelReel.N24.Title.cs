// NIGHT24 remove-daily-weekly (builder R): the reel scenario `title`. TITLE is built (ToTitle, a settle) and every text under the TITLE and
// RESULTS screens is read. The MODE row shows only the mode picked now, so the scenario reads the row's whole wrap from Ui.ReelModeLabels (the
// labels its cycle visits, in order; no mode is picked, because picking CAMPAIGN reloads the scene) plus the label shown now. PASS iff
// SURVIVAL is in the wrap, THE GRIND is in it exactly when design.n21.grind is on, no DAILY / WEEKLY / REPLAY text exists on either screen,
// and CAMPAIGN is in it exactly when design.n24.campaign is on and the map list holds a campaign level (SurvivalGame.HasCampaignMap).
// Every result line starts with FeelReel.Preconditions(g, pos) from SafeSpot(g).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N24TitleBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = TitleScenario(g, dir, res, offs, pre);
        }

        static IEnumerator TitleScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap, the sandbox spot)");
            g.ToTitle();
            yield return Frames(40);
            Start(g, "title", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            yield return Frames(30);
            Ui ui = g.UiView;
            List<string> wrap = ui != null ? ui.ReelModeLabels() : new List<string>();
            bool rowShown = ui != null && ui.ReelModeRowShown(out _);
            ui.ReelModeRowShown(out string current);
            yield return Stop();
            var seen = new List<string>();
            List<string> bad = ScreenTexts(new Regex("\\bDAILY\\b|\\bWEEKLY\\b|REPLAY", RegexOptions.IgnoreCase), seen);
            bool campaignOn = g.cfg.Design("n24.campaign.enabled") && g.HasCampaignMap;
            bool grindOn = g.cfg.Design("n21.grind.enabled");
            bool hasCampaign = wrap.Contains(UnityTuning.CampaignLabel);
            bool hasGrind = wrap.Contains(UnityTuning.GrindLabel);
            bool ok = wrap.Contains("SURVIVAL") && hasGrind == grindOn && hasCampaign == campaignOn && bad.Count == 0 && (rowShown || wrap.Count < 2);
            res[0] = pc + $" | title {(ok ? "PASS" : "RED")}: MODE wrap [{string.Join(" > ", wrap)}] (row shown {rowShown}, now '{current}'), DAILY / WEEKLY / REPLAY texts [{string.Join("; ", bad)}], "
                + $"CAMPAIGN {(hasCampaign ? "in" : "absent from")} the wrap with design.n24.campaign {(campaignOn ? "on and a level" : "off or no level")}, THE GRIND {(hasGrind ? "in" : "absent from")} the wrap (design.n21.grind {(grindOn ? "on" : "off")}); {seen.Count} texts on TITLE and RESULTS";
        }
    }
}
