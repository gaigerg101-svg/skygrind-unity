// NIGHT27 (builder W; design.n27.pulse27, chargeShots27): the two weapon rows' HUD. UNCOMPILED in the Editor and UNRUN when written (no Editor in the
// builder's worktree; dn-check compiles it); the rules it draws are Core/PulseCharge27.cs and Core/Guns/ChargeShots27.cs.
// - THE GLOVE: five pips in a row on the lower right, above the weapon's name (GG: "5 glowing pips on the glove", not a mid-screen bar). A pip is lit
//   (cyan) per kill; with a full glove and the blast off cooldown ALL five glow white-hot and "PULSE READY" (24 px) shows; with a full glove on cooldown
//   the words are "PULSE n S"; with fewer, "PULSE n/5 KILLS". The words show with the gauntlet in hand (and always when READY); the pips dim to
//   Weapons27PipsAwayAlpha with another gun in hand. When LT is pressed and the blast is refused the words say WHY for Weapons27DenyShow s
//   ("NEED 5 KILLS" / "COOLDOWN n S"), never red (cyan is player-usable; red-orange is danger). Text is the theme's S18 / S24 (18 px+ at 1080p).
// - THE CHARGE ARC: a thin arc (UiArc24, the NIGHT24 reticle arc's mesh) under the reticle arc: while RT charges the pistol or shotgun it FILLS with the
//   charge (cyan, whole alpha); after a charged shot it REFILLS with the cooldown (dim cyan, an empty track while it runs). Nothing is drawn when
//   nothing is charging or cooling (the central 40 % stays clear). The weapon glow of the viewmodel is NOT built tonight (the lead's reel decides).
// Wiring (the lead, Ui.cs): BuildHud calls BuildWeapons27Hud() after BuildN26Hud(); Render calls RenderWeapons27Hud(frameDt) after RenderN26Hud(frameDt).
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform w27Root, w27ArcRoot;
        CanvasGroup w27Group, w27ArcGroup;
        UiShape[] w27Pips;
        TextMeshProUGUI w27Text;
        UiArc24 w27Arc;
        float w27Alpha;
        int w27TextKey = -999;
        float w27ArcFillShown = -1f, w27ArcAlphaShown = -1f;

        /// <summary>What the widgets draw now, for the reel's scenario (the Ui instance is private to SurvivalGame).</summary>
        public static bool Weapons27Built;
        public static int Pulse27PipsLitNow;
        public static string Pulse27TextNow = "";
        public static bool Pulse27ReadyNow;
        public static float Charge27ArcFillNow, Charge27ArcAlphaNow;

        void BuildWeapons27Hud()
        {
            Weapons27Built = true;
            int n = 5; // design.n27.pulse27.pips: the glove's pips; the widget draws at most this many and reads the real count at render time
            float pip = UnityTuning.Weapons27PipSize, gap = UnityTuning.Weapons27PipGap;
            float w = n * pip + (n - 1) * gap;
            w27Root = Hud17Rect(values, "Pulse glove 27", new Vector2(1, 0), new Vector2(1, 0), new Vector2(UnityTuning.Weapons27PipsX, UnityTuning.Weapons27PipsY), new Vector2(UnityTuning.Weapons27TextW, pip + UnityTuning.Weapons27TextH + 6f));
            w27Group = w27Root.gameObject.AddComponent<CanvasGroup>();
            w27Group.blocksRaycasts = w27Group.interactable = false;
            w27Group.alpha = 0;
            w27Pips = new UiShape[n];
            for (int i = 0; i < n; i++)
            {
                w27Pips[i] = Hud17Shape(w27Root, "Pip " + i, UiTheme.InkSoft, new Vector2(1, 0), new Vector2(0, 0), new Vector2(-w + i * (pip + gap), 0), new Vector2(pip, pip));
            }
            w27Text = Hud17Text(w27Root, "Words", "", UiTheme.Px(UiTheme.Size.S18), UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Right, new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(0, pip + 6f), new Vector2(UnityTuning.Weapons27TextW, UnityTuning.Weapons27TextH), true);
            w27Root.gameObject.SetActive(false);

            // the charge arc: one wide cell, a smile under the NIGHT24 reticle arc (the reticle arc ends 28 px under the crosshair, its middle 42)
            float sag = UnityTuning.Weapons27ArcMidBelow - UnityTuning.Weapons27ArcEndsBelow;
            float h = sag + UnityTuning.Weapons27ArcThick + 2f;
            w27ArcRoot = Hud17Rect(sight, "Charge arc 27", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0, -UnityTuning.Weapons27ArcEndsBelow), new Vector2(UnityTuning.Weapons27ArcWidth + 8f, h));
            w27ArcGroup = w27ArcRoot.gameObject.AddComponent<CanvasGroup>();
            w27ArcGroup.blocksRaycasts = w27ArcGroup.interactable = false;
            w27ArcGroup.alpha = 0;
            var go = new GameObject("Arc", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiArc24));
            var rt = (RectTransform)go.transform;
            rt.SetParent(w27ArcRoot, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = w27ArcRoot.sizeDelta;
            w27Arc = go.GetComponent<UiArc24>();
            w27Arc.raycastTarget = false;
            w27Arc.radius = UnityTuning.Weapons27ArcRadius;
            w27Arc.thick = UnityTuning.Weapons27ArcThick;
            w27ArcRoot.gameObject.SetActive(false);
            game.OnNewRun += () =>
            {
                w27Alpha = 0;
                w27TextKey = -999;
                w27ArcFillShown = -1f;
            };
        }

        void RenderWeapons27Hud(double frameDt)
        {
            if (w27Root == null) return;
            SurvivalSim sim = game.sim;
            PulseCharge27 pc = sim?.pulse27;
            ChargeShots27 cs = sim?.loadout?.charge27; // NIGHT31 W: the loadout's charge (design.n27.chargeShots27's, or design.n31.charge's: SurvivalSim.Weapons31.cs)
            bool playing = sim != null && game.flow == Flow.Playing;
            bool pulseOn = playing && sim.n27.pulse27 && pc != null && pc.active;
            bool chargeOn = playing && (sim.n27.chargeShots27 || sim.n31.charge) && cs != null && cs.active;
            string weapon = sim != null ? sim.loadout.Weapon : null;

            // --- the glove -------------------------------------------------------------------------------------------------------------
            bool gloveShown = pulseOn && !hud30On; // NIGHT30 P0 e / item 2 (design.n30.hud30): the BLAST slot carries the pips; "PULSE READY" goes (its reads below still run)
            if (w27Root.gameObject.activeSelf != gloveShown) w27Root.gameObject.SetActive(gloveShown);
            Pulse27ReadyNow = false;
            if (pulseOn)
            {
                bool inHand = weapon == "gauntlet";
                bool ready = pc.Ready;
                Pulse27ReadyNow = ready;
                int lit = Mathf.Clamp(pc.pips, 0, w27Pips.Length);
                Pulse27PipsLitNow = lit;
                float pulse = ready ? 0.82f + 0.18f * Mathf.Sin(FeelClock.UnscaledTime * UnityTuning.Weapons27ReadyHz * 6.2831853f) : 1f;
                for (int i = 0; i < w27Pips.Length; i++)
                {
                    Color c = i < lit ? (ready ? UiTheme.CyanHot : UiTheme.Cyan) : UiTheme.InkSoft;
                    c.a = i < lit ? pulse : UnityTuning.Weapons27PipEmptyAlpha;
                    if (w27Pips[i].color != c) w27Pips[i].color = c;
                }
                // the words: READY, the refusal's reason (briefly), the cooldown, or the kill count
                bool denied = pc.deniedWhy != null && pc.stepNow - pc.deniedAtStep < (int)(UnityTuning.Weapons27DenyShow / sim.step);
                int cdSecs = Mathf.CeilToInt((float)(pc.cooldownSteps * sim.step));
                int key;
                string words;
                if (ready) { key = 1000; words = "PULSE READY"; }
                else if (denied && pc.deniedWhy == "pips") { key = 2000 + lit; words = "NEED " + (pc.p.pips - lit) + " KILL" + (pc.p.pips - lit == 1 ? "" : "S"); }
                else if (denied) { key = 3000 + cdSecs; words = "COOLDOWN " + cdSecs + " S"; }
                else if (pc.Full) { key = 4000 + cdSecs; words = "PULSE " + cdSecs + " S"; }
                else { key = 5000 + lit; words = "PULSE " + lit + "/" + pc.p.pips + " KILLS"; }
                if (key != w27TextKey)
                {
                    w27TextKey = key;
                    w27Text.SetText(words);
                    Pulse27TextNow = words;
                    w27Text.color = ready ? UiTheme.CyanHot : UiTheme.CyanPale;
                    w27Text.fontSize = ready ? UiTheme.Px(UiTheme.Size.S24) : UiTheme.Px(UiTheme.Size.S18);
                }
                bool showWords = inHand || ready || denied;
                float wantAlpha = (inHand || ready ? 1f : UnityTuning.Weapons27PipsAwayAlpha);
                w27Alpha = Mathf.MoveTowards(w27Alpha, wantAlpha, (float)(frameDt / UnityTuning.Weapons27FadeIn)); // no pop-in
                if (w27Group.alpha != w27Alpha) w27Group.alpha = w27Alpha;
                if (w27Text.gameObject.activeSelf != showWords) w27Text.gameObject.SetActive(showWords);
            }
            else
            {
                w27Alpha = 0;
                Pulse27PipsLitNow = 0;
                Pulse27TextNow = "";
            }

            // --- the charge arc ---------------------------------------------------------------------------------------------------------
            int gi = chargeOn ? ChargeShots27.Index(weapon) : -1;
            float fill = 0f, alpha = 0f;
            bool cooling = false;
            if (gi >= 0)
            {
                if (cs.charging) { fill = (float)cs.Level(gi); alpha = 1f; }
                else if (cs.cooldown[gi] > 0) { fill = 1f - (float)cs.CooldownShare(gi); alpha = UnityTuning.Weapons27ArcCoolAlpha; cooling = true; }
            }
            bool arcShow = alpha > 0f;
            if (w27ArcRoot.gameObject.activeSelf != arcShow) w27ArcRoot.gameObject.SetActive(arcShow);
            Charge27ArcFillNow = fill;
            Charge27ArcAlphaNow = alpha;
            if (arcShow)
            {
                float qf = Mathf.Round(fill * 64f) / 64f;
                if (qf != w27ArcFillShown || alpha != w27ArcAlphaShown)
                {
                    w27ArcFillShown = qf;
                    w27ArcAlphaShown = alpha;
                    Color cy = UiTheme.A(UiTheme.Cyan, alpha);
                    w27Arc.cells.Clear();
                    float half = UnityTuning.Weapons27ArcWidth * 0.5f;
                    w27Arc.cells.Add(new UiArc24.Cell { x0 = -half, x1 = half, fill = qf, track = UiTheme.A(UiTheme.InkSolid, cooling ? 0.45f : 0.3f), bar = cy });
                    w27Arc.Changed();
                }
                if (w27ArcGroup.alpha != 1f) w27ArcGroup.alpha = 1f;
            }
        }
    }

    public static partial class UnityTuning
    {
        // --- NIGHT27 weapons HUD (UiWeapons.N27.cs): Unity-only render / layout numbers (CLAUDE.md rule 5) ---------------------------------------
        // The glove's pips: a square of this many px (1080p) with this gap, the row's right edge X px from the right edge and Y px above the bottom
        // (above the weapon's name at the lower right); the words' box.
        public const float Weapons27PipSize = 16f, Weapons27PipGap = 6f, Weapons27PipsX = -40f, Weapons27PipsY = 150f, Weapons27TextW = 320f, Weapons27TextH = 30f;
        // An empty pip's alpha (the INK-soft square), the pips' alpha with another gun in hand, the READY pulse's rate (Hz), how long a refusal's reason
        // shows (s), the fade-in (s: no pop-in).
        public const float Weapons27PipEmptyAlpha = 0.55f, Weapons27PipsAwayAlpha = 0.5f, Weapons27ReadyHz = 1.6f, Weapons27DenyShow = 1.5f, Weapons27FadeIn = 0.2f;
        // The charge arc: width / thickness / radius like the NIGHT24 reticle arc, its ends and lowest point under the crosshair (below the reticle arc's
        // 28 / 42 px), and the cooldown's alpha.
        public const float Weapons27ArcWidth = 110f, Weapons27ArcThick = 4f, Weapons27ArcRadius = 180f, Weapons27ArcEndsBelow = 50f, Weapons27ArcMidBelow = 60f, Weapons27ArcCoolAlpha = 0.45f;
    }
}
