// NIGHT14 U3-27.10 (agent V2): THE ROUTE on RESULTS and the SHARE CARD (design.route; port-only: the web's RESULTS has no
// map, no image and no seed button). Ui hooks: BuildRoute, RenderRoute, SaveRouteCard, CopyRouteSeed.
// RESULTS: a plate at the bottom right (x +40..+900, y -262..-530 at 1080p from the screen centre: right of the tally's
// buttons, under the PAYOFF strip; plain RESULTS: from x +240) holds the map (RouteMap: the arena from above, the path on a
// cyan speed ramp, round ticks, kills, the start, the exfil, the DOWN), a legend, and RESULTS' SAVE CARD and COPY SEED
// (Ui.cs pre-placed them hidden: they move into the plate and show with it). The map is drawn once when RESULTS opens on
// new stats (the frame only compares a reference); the plate shows with the tally's buttons.
// SAVE CARD: the card (1200 x 630) laid out on an off-screen world-space canvas (built on the first press) and written as
// a PNG by RouteCard (My Pictures\SKYGRIND\SKYGRIND-<seed>-<time>.png); a hint says where. COPY SEED: the seed code to the
// clipboard, a hint. Flag off (design.route.enabled false): nothing is built, the buttons stay hidden, RESULTS as before.
using System;
using System.Globalization;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        bool routeOn, routeShown;
        RouteMap routeMap, routeCardMap;
        RectTransform routePlate;
        TextMeshProUGUI routeKillsLabel, routeEndLabel;
        UnityEngine.UI.RawImage routeEndIcon;
        Texture2D routeIconDown, routeIconExfil;
        RunStats routeStats;
        int routeVersion = int.MinValue;
        // The card's rig (built on the first SAVE CARD)
        GameObject routeCardRig;
        Camera routeCardCam;
        RenderTexture routeCardRt;
        UnityEngine.UI.RawImage routeCardMapImage;
        TextMeshProUGUI routeCardOutcome, routeCardRound, routeCardScore, routeCardRank, routeCardTime, routeCardGun, routeCardLine, routeCardSeed, routeCardStamp;

        /// <summary>Automation: THE ROUTE was drawn for the RESULTS showing (false off RESULTS), and its point count.</summary>
        public bool RouteDrawn { get; private set; }
        public int RouteDrawnPoints { get; private set; }

        /// <summary>Automation: the RESULTS map (its texture and draw count).</summary>
        public RouteMap RouteMapView => routeMap;

        /// <summary>Automation (the smoke): SAVE CARD and COPY SEED as their buttons do (RouteCard.LastPath / LastError tell).</summary>
        public void RouteSaveCardNow() => SaveRouteCard();

        public void RouteCopySeedNow() => CopyRouteSeed();

        partial void BuildRoute()
        {
            routeOn = cfg.Design("route.enabled");
            if (!routeOn) return; // the old RESULTS: SAVE CARD and COPY SEED stay hidden
            routeMap = new RouteMap(cfg, game.map, UnityTuning.RouteMapTexture);
            float left = resultsTally ? UnityTuning.RoutePlateX : UnityTuning.RoutePlateXPlain;
            float width = UnityTuning.ResultsRouteRight - left, height = UnityTuning.RoutePlateTop - UnityTuning.RoutePlateBottom; // NIGHT17 round 1: inside the right inset at 16:10
            float pad = UnityTuning.RoutePad, mapPx = UnityTuning.RouteMapPx;
            UnityEngine.UI.Image back = Img(results.transform, "Route", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, height));
            routePlate = back.rectTransform;
            routePlate.pivot = new Vector2(0, 1);
            routePlate.anchoredPosition = new Vector2(left, UnityTuning.RoutePlateTop);
            Img(routePlate, "Edge", usable, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2((float)cfg.Num("ui.theme.edgeWidth"), 0)).rectTransform.pivot = new Vector2(0, 0.5f);
            var mapGo = new GameObject("Map", typeof(RectTransform), typeof(UnityEngine.UI.RawImage));
            var mapRt = (RectTransform)mapGo.transform;
            mapRt.SetParent(routePlate, false);
            mapRt.anchorMin = mapRt.anchorMax = mapRt.pivot = new Vector2(0, 1);
            mapRt.anchoredPosition = new Vector2(pad, -pad);
            mapRt.sizeDelta = new Vector2(mapPx, mapPx);
            var mapImg = mapGo.GetComponent<UnityEngine.UI.RawImage>();
            mapImg.texture = routeMap.texture;
            mapImg.raycastTarget = false;

            // SAVE CARD and COPY SEED: into the plate's right, stacked.
            float bw = UnityTuning.RouteButtonWidth, bh = UnityTuning.RouteButtonHeight;
            RoutePlaceButton(resultsSaveCard, new Vector2(-pad, -pad), bw, bh);
            RoutePlaceButton(resultsCopySeed, new Vector2(-pad, -pad - bh - UnityTuning.RouteButtonGap), bw, bh);

            // The legend between the map and the buttons (only when it has the room: not beside plain RESULTS' buttons).
            float lx = pad + mapPx + UnityTuning.RouteLegendGap;
            float lw = width - lx - pad - bw - UnityTuning.RouteLegendGap;
            if (lw >= UnityTuning.RouteLegendMinWidth)
            {
                float line = UnityTuning.RouteLegendLine, icon = UnityTuning.RouteLegendIcon, y = -pad;
                TextMeshProUGUI head = Text(routePlate, "Head", "THE ROUTE", tLabel, muted, TextAlignmentOptions.Left, new Vector2(lx, y), new Vector2(lw, line), new Vector2(0, 1));
                head.characterSpacing = UnityTuning.LabelSpacing;
                y -= line + 4;
                RouteLegendRow(lx, y, lw, routeMap.RampStrip(64), UnityTuning.RouteRampWidth, UnityTuning.RouteRampHeight, "SPEED");
                y -= line;
                routeKillsLabel = RouteLegendRow(lx, y, lw, routeMap.Icon(RouteIconKind.Kill, 36), icon, icon, "KILLS");
                y -= line;
                RouteLegendRow(lx, y, lw, routeMap.Icon(RouteIconKind.Tick, 36), icon, icon, "ROUND STARTS");
                y -= line;
                RouteLegendRow(lx, y, lw, routeMap.Icon(RouteIconKind.Start, 36), icon, icon, "START");
                y -= line;
                routeIconDown = routeMap.Icon(RouteIconKind.Down, 36);
                routeIconExfil = routeMap.Icon(RouteIconKind.Exfil, 36);
                routeEndLabel = RouteLegendRow(lx, y, lw, routeIconDown, icon, icon, "DOWN");
                routeEndIcon = routeEndLabel.transform.parent.GetComponentInChildren<UnityEngine.UI.RawImage>();
            }
            routePlate.gameObject.SetActive(false);
        }

        /// <summary>One of RESULTS' route buttons into the plate's top-right, at `pos` from it, `w` x `h`.</summary>
        void RoutePlaceButton(GameObject button, Vector2 pos, float w, float h)
        {
            var rt = (RectTransform)button.transform;
            rt.SetParent(routePlate, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(w, h);
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null) label.rectTransform.sizeDelta = new Vector2(w, h);
            button.SetActive(game.devMode); // NIGHT17 rule 7: SAVE CARD / COPY SEED are a developer's (the plate decides the rest)
        }

        /// <summary>A legend row at (x, y) in the plate: an icon (w x h) then its label; returns the label.</summary>
        TextMeshProUGUI RouteLegendRow(float x, float y, float width, Texture2D icon, float w, float h, string label)
        {
            var row = new GameObject("Legend " + label, typeof(RectTransform));
            var rowRt = (RectTransform)row.transform;
            rowRt.SetParent(routePlate, false);
            rowRt.anchorMin = rowRt.anchorMax = rowRt.pivot = new Vector2(0, 1);
            rowRt.anchoredPosition = new Vector2(x, y);
            rowRt.sizeDelta = new Vector2(width, UnityTuning.RouteLegendLine);
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(UnityEngine.UI.RawImage));
            var iconRt = (RectTransform)iconGo.transform;
            iconRt.SetParent(rowRt, false);
            iconRt.anchorMin = iconRt.anchorMax = iconRt.pivot = new Vector2(0, 0.5f); // the row's middle, at its left
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(w, h);
            var img = iconGo.GetComponent<UnityEngine.UI.RawImage>();
            img.texture = icon;
            img.raycastTarget = false;
            float tx = Mathf.Max(w, UnityTuning.RouteLegendIcon) + 10;
            TextMeshProUGUI t = Text(rowRt, "Label", label, tSmall, text, TextAlignmentOptions.Left, new Vector2(tx, 0), new Vector2(width - tx, UnityTuning.RouteLegendLine), new Vector2(0, 0.5f));
            t.overflowMode = TextOverflowModes.Ellipsis;
            return t;
        }

        partial void RenderRoute(double frameDt)
        {
            if (!routeOn) return;
            RouteTrack t = game.RouteTrackView;
            RunStats s = game.lastStats;
            bool onResults = game.UiFlow == Flow.Results && t != null && s != null && t.count > 0;
            if (onResults && (!ReferenceEquals(s, routeStats) || t.Version != routeVersion))
            {
                // RESULTS opened on a run: its map, once (the texture work happens here, never per frame).
                routeStats = s;
                routeVersion = t.Version;
                routeMap.Draw(t);
                RouteDrawn = true;
                RouteDrawnPoints = t.count;
                if (routeKillsLabel != null) routeKillsLabel.text = t.kills.ToString(CultureInfo.InvariantCulture) + (t.kills == 1 ? " KILL" : " KILLS");
                if (routeEndLabel != null)
                {
                    bool end = t.exfilWin || t.down;
                    routeEndLabel.transform.parent.gameObject.SetActive(end);
                    if (end)
                    {
                        routeEndLabel.text = t.exfilWin ? "EXFIL" : "DOWN";
                        routeEndIcon.texture = t.exfilWin ? routeIconExfil : routeIconDown;
                    }
                }
            }
            if (!onResults && RouteDrawn) RouteDrawn = false;
            bool on = onResults && restartButton.activeSelf && resultsDetailsOpen; // NIGHT17: with RESULTS' DETAILS
            if (on == routeShown) return;
            routeShown = on;
            routePlate.gameObject.SetActive(on);
        }

        /// <summary>RESULTS' COPY SEED: the run's seed code to the system clipboard, then the hint.</summary>
        partial void CopyRouteSeed()
        {
            string code = game.lastStats?.seedCode;
            if (string.IsNullOrEmpty(code)) return;
            GUIUtility.systemCopyBuffer = code;
            ShowHint("SEED " + code + " COPIED");
        }

        /// <summary>RESULTS' SAVE CARD: the card as a PNG in My Pictures\SKYGRIND, then a hint with where.</summary>
        partial void SaveRouteCard()
        {
            RunStats s = game.lastStats;
            RouteTrack t = game.RouteTrackView;
            if (s == null || t == null) return;
            try
            {
                RouteCardRig();
                RouteCardFill(s, t);
                byte[] png = RouteCard.Render(routeCardRig, routeCardCam, routeCardRt);
                string path = RouteCard.Write(png, s.seedCode);
                Debug.Log("[route] card saved: " + path);
                ShowHint("CARD SAVED TO " + RouteCard.ShortPath(System.IO.Path.GetDirectoryName(path)).ToUpperInvariant());
            }
            catch (Exception e)
            {
                RouteCard.Failed(e.GetType().Name + ": " + e.Message);
                Debug.LogWarning("[route] the card could not be saved: " + e);
                ShowHint("THE CARD COULD NOT BE SAVED");
            }
        }

        /// <summary>The card's off-screen rig: a world-space canvas (1 px = 1 cm) and its own camera, far below the map.</summary>
        void RouteCardRig()
        {
            if (routeCardRig != null) return;
            int w = UnityTuning.RouteCardWidth, h = UnityTuning.RouteCardHeight;
            float pad = UnityTuning.RouteCardPad, mapPx = UnityTuning.RouteCardMapPx;
            routeCardMap = new RouteMap(cfg, game.map, UnityTuning.RouteCardMapPx);
            routeCardRig = new GameObject("Route card rig");
            routeCardRig.SetActive(false);
            routeCardRig.transform.position = new Vector3(0, UnityTuning.RouteCardDepth, 0);
            var canvasGo = new GameObject("Route card", typeof(RectTransform), typeof(Canvas));
            var root = (RectTransform)canvasGo.transform;
            root.SetParent(routeCardRig.transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            root.sizeDelta = new Vector2(w, h);
            root.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            root.localPosition = Vector3.zero;
            Color plate = panel;
            plate.a = 1;
            Img(root, "Plate", plate, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Img(root, "Edge", usable, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2((float)cfg.Num("ui.theme.edgeWidth") * 2, 0)).rectTransform.pivot = new Vector2(0, 0.5f);
            var mapGo = new GameObject("Map", typeof(RectTransform), typeof(UnityEngine.UI.RawImage));
            var mapRt = (RectTransform)mapGo.transform;
            mapRt.SetParent(root, false);
            mapRt.anchorMin = mapRt.anchorMax = mapRt.pivot = new Vector2(0, 1);
            mapRt.anchoredPosition = new Vector2(pad, -pad);
            mapRt.sizeDelta = new Vector2(mapPx, mapPx);
            routeCardMapImage = mapGo.GetComponent<UnityEngine.UI.RawImage>();
            routeCardMapImage.texture = routeCardMap.texture;
            routeCardMapImage.raycastTarget = false;

            // The text column: the wordmark (TITLE's: cyan over a red-orange copy), the run's outcome, the round, four numbers,
            // a line, the seed code, the build stamp.
            float x = pad + mapPx + UnityTuning.RouteCardGap, cw = w - x - pad, half = cw * 0.5f;
            var top = new Vector2(0, 1);
            float wm = UnityTuning.RouteCardWordmark, shadow = UnityTuning.WordmarkShadow * wm / UnityTuning.WordmarkSize;
            TextMeshProUGUI back = Text(root, "Wordmark (back)", "SKYGRIND", wm, danger, TextAlignmentOptions.Left, new Vector2(x + shadow, -pad + 6 - shadow), new Vector2(cw, wm + 16), top, Face.Heading);
            back.fontStyle = FontStyles.Bold | FontStyles.Italic;
            TextMeshProUGUI mark = Text(root, "Wordmark", "SKYGRIND", wm, usable, TextAlignmentOptions.Left, new Vector2(x, -pad + 6), new Vector2(cw, wm + 16), top, Face.Heading);
            mark.fontStyle = FontStyles.Bold | FontStyles.Italic;
            float y = -pad - wm - 18;
            routeCardOutcome = Text(root, "Outcome", "", UnityTuning.RouteCardLabel, muted, TextAlignmentOptions.Left, new Vector2(x, y), new Vector2(cw, 28), top);
            routeCardOutcome.characterSpacing = UnityTuning.LabelSpacing;
            y -= 34;
            routeCardRound = Text(root, "Round", "", UnityTuning.RouteCardBig, text, TextAlignmentOptions.Left, new Vector2(x, y), new Vector2(cw, UnityTuning.RouteCardBig + 14), top, Face.Heading);
            routeCardRound.richText = true;
            y -= UnityTuning.RouteCardBig + 26;
            routeCardScore = RouteCardCell(root, "SCORE", x, y, half, gold);
            routeCardRank = RouteCardCell(root, "BEST RANK", x + half, y, half, text);
            y -= UnityTuning.RouteCardRow;
            routeCardTime = RouteCardCell(root, "TIME", x, y, half, text);
            routeCardGun = RouteCardCell(root, "TOP GUN", x + half, y, half, text);
            y -= UnityTuning.RouteCardRow + 4;
            routeCardLine = Text(root, "Line", "", UnityTuning.RouteCardSmall, muted, TextAlignmentOptions.Left, new Vector2(x, y), new Vector2(cw, 28), top);
            routeCardLine.overflowMode = TextOverflowModes.Ellipsis;
            y -= 40;
            routeCardSeed = Text(root, "Seed", "", UnityTuning.RouteCardValue, text, TextAlignmentOptions.Left, new Vector2(x, y), new Vector2(cw, UnityTuning.RouteCardValue + 12), top, Face.Display);
            routeCardSeed.richText = true;
            routeCardStamp = Text(root, "Stamp", BuildStamp.Line, UnityTuning.RouteCardSmall, muted, TextAlignmentOptions.Left, new Vector2(x, -h + pad + 28), new Vector2(cw, 28), top, Face.Body);
            routeCardStamp.overflowMode = TextOverflowModes.Ellipsis;

            foreach (Transform tr in routeCardRig.GetComponentsInChildren<Transform>(true)) tr.gameObject.layer = UnityTuning.RouteCardLayer;
            routeCardCam = RouteCard.MakeCamera(routeCardRig.transform, w * 0.01f, h * 0.01f, 5, plate, UnityTuning.RouteCardLayer);
            routeCardRt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { name = "Route card", antiAliasing = 1 };
            routeCardRt.Create();
        }

        /// <summary>A card number: its label (small, muted) over its value.</summary>
        TextMeshProUGUI RouteCardCell(RectTransform root, string label, float x, float y, float w, Color valueColor)
        {
            var top = new Vector2(0, 1);
            TextMeshProUGUI l = Text(root, label, label, UnityTuning.RouteCardLabel, muted, TextAlignmentOptions.Left, new Vector2(x, y), new Vector2(w, 26), top);
            l.characterSpacing = UnityTuning.LabelSpacing;
            TextMeshProUGUI v = Text(root, label + " value", "", UnityTuning.RouteCardValue, valueColor, TextAlignmentOptions.Left, new Vector2(x, y - 26), new Vector2(w - 10, UnityTuning.RouteCardValue + 12), top, Face.Display);
            v.overflowMode = TextOverflowModes.Ellipsis;
            return v;
        }

        /// <summary>The card's texts and map for a run (on the button only).</summary>
        void RouteCardFill(RunStats s, RouteTrack t)
        {
            routeCardMap.Draw(t);
            var inv = CultureInfo.InvariantCulture;
            string outcome = s.exfil ?? "RUN OVER";
            if (s.heat > 0) outcome += " · " + HeatBadgeText(s.heat);
            routeCardOutcome.text = outcome;
            string grade = "";
            if (s.grade != null) grade = "  <color=#" + (gradeHex.TryGetValue(s.grade, out string gh) ? gh : goldHex) + ">" + s.grade + "</color>";
            routeCardRound.text = "ROUND " + s.round.ToString(inv) + grade;
            routeCardScore.text = Math.Round(s.earned).ToString("N0", inv);
            routeCardRank.text = s.bestRank ?? "-";
            StyleMeter st = game.style;
            routeCardRank.color = st != null && s.bestRankIndex >= 0 && s.bestRankIndex < st.Params.colors.Count ? Look.Hex(st.Params.colors[s.bestRankIndex]) : text;
            var span = TimeSpan.FromSeconds(Math.Max(0, s.time));
            routeCardTime.text = span.TotalHours >= 1
                ? ((int)span.TotalHours).ToString(inv) + ":" + span.Minutes.ToString("00", inv) + ":" + span.Seconds.ToString("00", inv)
                : span.Minutes.ToString(inv) + ":" + span.Seconds.ToString("00", inv);
            string gun = t.TopGun(out int gunKills);
            routeCardGun.text = gun != null ? WeaponName(gun) + " · " + gunKills.ToString(inv) : "-";
            routeCardLine.text = s.kills.ToString(inv) + " KILLS · " + s.headshots.ToString(inv) + " HEADSHOTS · ACCURACY " + Math.Round(s.Accuracy * 100).ToString(inv) + "%";
            routeCardSeed.text = "<color=#" + mutedHex + ">SEED</color> " + (s.seedCode ?? "-");
            routeCardStamp.text = BuildStamp.Line;
        }
    }
}
