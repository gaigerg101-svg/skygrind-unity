// NIGHT29 item 3 (builder V): two small HUD parts on a canvas of their own (no edit in Ui.cs):
//  - the BOSS GAP BAR (the Glacier's chase): a bar under the boss bar, text "GAP 23 M", filled by gap / scale; cyan while the gap is
//    inside the hurt line (player-usable), pale violet-white otherwise, red-orange (danger) only while `BossGapDanger` is set;
//  - the DOORS OPEN tag (builder S's DOORFREE): a small corner tag while `DoorsOpenTag` is true.
// The lead and builders WRITE the statics each frame (or on change); this view only reads them. Text is 20 px at 1080p (rule 8:
// 16 px or more). View only; it installs itself after the first scene loads and shows nothing until a static is set.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>The values the HUD parts read. A boss's driver sets BossGapMetres each frame it fights (and -1 when it stops); DoorFree sets DoorsOpenTag.</summary>
    public static class Hud29
    {
        /// <summary>The gap to the boss in metres; under 0 hides the bar.</summary>
        public static double BossGapMetres = -1;
        /// <summary>true while DOORFREE holds every door open (the corner tag shows).</summary>
        public static bool DoorsOpenTag;
        /// <summary>The bar's full length in metres (the gap where the chase is lost: the boss row's number); 0 = UnityTuning.Hud29GapFallbackMax.</summary>
        public static double BossGapMax;
        /// <summary>The gap at or under which the player can hurt the boss (a hurt line is drawn there); under 0 = none.</summary>
        public static double BossGapHurt = -1;
        /// <summary>true: the fill is red-orange (the lost-chase zone: danger only).</summary>
        public static bool BossGapDanger;
        /// <summary>NIGHT30 P0 c (builder V, design.n30.hud30): true while TITLE / PAUSE / RESULTS shows; the whole canvas hides (the run's rows never
        /// draw over a menu: shot 190310). The Ui writes it every frame with hud30 on (Hud30.cs); off it stays false (NIGHT29's view).</summary>
        public static bool MenuHidden;
        /// <summary>NIGHT30 (the reel `menus30` only): both rows forced up while it runs (the sim's own writers reset DoorsOpenTag / the gap each
        /// frame), so the hide-under-a-menu check has rows to hide. False in play; the reel clears it.</summary>
        public static bool ReelForce;
        /// <summary>NIGHT33 U: the DOORS OPEN tag's text (the hud33 reel checks its rect with the HUD's); null until built.</summary>
        public static TMPro.TextMeshProUGUI TagTextNow;

        /// <summary>The bar's fill share, 0 to 1 (pure: Hud29Tests).</summary>
        public static float FillShare(double gap, double max)
        {
            if (!(max > 0)) max = UnityTuning.Hud29GapFallbackMax;
            if (!(gap > 0)) return 0;
            return gap >= max ? 1f : (float)(gap / max);
        }

        /// <summary>The label: "GAP 23 M" (rounded to whole metres), with the hurt line when there is one.</summary>
        public static string Label(double gap, double hurt) =>
            "GAP " + (int)System.Math.Round(gap) + " M" + (hurt >= 0 ? "   HURT AT " + (int)System.Math.Round(hurt) + " M" : "");

        /// <summary>Back to a clean state (a new run or scene).</summary>
        public static void Reset()
        {
            BossGapMetres = -1;
            DoorsOpenTag = false;
            BossGapMax = 0;
            BossGapHurt = -1;
            BossGapDanger = false;
        }
    }

    public sealed class Hud29View : MonoBehaviour
    {
        /// <summary>NIGHT30 (the reel `menus30` reads it): the gap bar or the DOORS OPEN tag is drawn this frame.</summary>
        public static bool DrawnNow;
        Canvas canvas;
        RectTransform gap, fill, hurtLine, tag;
        Image fillImage, hurtImage;
        TextMeshProUGUI gapText, tagText;
        float shown = -1;
        bool scaleRead;
        string shownLabel;
        // NIGHT30: the Glacier's patience gauge
        RectTransform gauge, gaugeFill;
        Image gaugeFillImage;
        TextMeshProUGUI gaugeText;
        float gaugeShown = -1;
        string gaugeLabel;
        SurvivalGame game30;
        /// <summary>For the reels: the gauge is drawn, its fill (0..1) and its words now.</summary>
        public static bool GaugeDrawn;
        public static float GaugeFillNow;
        public static string GaugeWordsNow = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Hud29View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Hud29View>();
        }

        static Color Cyan => UiTheme.Ready ? UiTheme.Cyan : Color.cyan;
        static Color White => UiTheme.Ready ? UiTheme.White : Color.white;
        static Color Danger => UiTheme.Ready ? UiTheme.Danger : new Color(1f, 0.35f, 0.2f);
        static Color Ink => UiTheme.Ready ? UiTheme.Ink : new Color(0.05f, 0.04f, 0.1f, 0.85f);

        void Build()
        {
            var go = new GameObject("Hud29 canvas", typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UnityTuning.Hud29Sorting;
            var cs = go.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(UnityTuning.Hud29RefW, UnityTuning.Hud29RefH);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;

            // the gap bar: top centre
            gap = MakeRect("Gap bar", go.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -UnityTuning.Hud29GapTop), new Vector2(UnityTuning.Hud29GapW + 24, 60));
            MakeImg(gap, "Plate", Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.Hud29GapW + 24, 60));
            MakeImg(gap, "Track", Color.Lerp(Ink, White, 0.18f), new Vector2(0, 0), new Vector2(0, 0), new Vector2(12, 10), new Vector2(UnityTuning.Hud29GapW, UnityTuning.Hud29GapBarH));
            fill = MakeImg(gap, "Fill", Cyan, new Vector2(0, 0), new Vector2(0, 0), new Vector2(12, 10), new Vector2(0, UnityTuning.Hud29GapBarH), out fillImage);
            hurtLine = MakeImg(gap, "Hurt line", White, new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(12, 6), new Vector2(4, UnityTuning.Hud29GapBarH + 8), out hurtImage);
            gapText = MakeText(gap, "Gap text", UnityTuning.Hud29GapTextPx, TextAlignmentOptions.BottomLeft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -4), new Vector2(UnityTuning.Hud29GapW, 30));
            // NIGHT30 (builder V; design.n30.glacier30): THE GLACIER's patience gauge under the gap bar (GG: "it needs to shoot ... or at least target you in a
            // meaningful way": the player SEES it filling while too far or out of sight, then the volley), with the volley / sweep tells as words
            gauge = MakeRect("Shards gauge", gap, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -UnityTuning.Hud30GaugeGap), new Vector2(UnityTuning.Hud29GapW + 24, UnityTuning.Hud30GaugeH));
            gauge.pivot = new Vector2(0.5f, 1);
            gauge.anchoredPosition = new Vector2(0, -UnityTuning.Hud30GaugeGap);
            MakeImg(gauge, "Plate", Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, gauge.sizeDelta);
            MakeImg(gauge, "Track", Color.Lerp(Ink, White, 0.18f), new Vector2(0, 0), new Vector2(0, 0), new Vector2(12, 8), new Vector2(UnityTuning.Hud29GapW, UnityTuning.Hud30GaugeBarH));
            gaugeFill = MakeImg(gauge, "Fill", Danger, new Vector2(0, 0), new Vector2(0, 0), new Vector2(12, 8), new Vector2(0, UnityTuning.Hud30GaugeBarH), out gaugeFillImage);
            gaugeText = MakeText(gauge, "Gauge text", UnityTuning.Hud29GapTextPx, TextAlignmentOptions.BottomLeft, new Vector2(0, 0), new Vector2(0, 0), new Vector2(12, 8 + UnityTuning.Hud30GaugeBarH + 2), new Vector2(UnityTuning.Hud29GapW, 26));
            gauge.gameObject.SetActive(false);
            gap.gameObject.SetActive(false);

            // the DOORS OPEN tag: top-left
            tag = MakeRect("Doors open tag", go.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(UnityTuning.Hud29TagMargin, -UnityTuning.Hud29TagMargin), new Vector2(190, UnityTuning.Hud29TagTextPx + 2 * UnityTuning.Hud29TagPadY));
            MakeImg(tag, "Plate", Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, tag.sizeDelta);
            tagText = MakeText(tag, "Tag text", UnityTuning.Hud29TagTextPx, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, tag.sizeDelta);
            tagText.text = UnityTuning.Hud29TagText;
            tagText.color = UiTheme.Ready ? UiTheme.CyanPale : White; // pale: not a boss colour, not danger
            tag.gameObject.SetActive(false);
            Hud29.TagTextNow = tagText;
            tag33 = -1; // a rebuilt tag is placed again
        }

        static RectTransform MakeRect(string name, Transform parent, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.pivot = new Vector2(aMin.x == 0.5f ? 0.5f : aMin.x, aMin.y == 0.5f ? 0.5f : aMin.y);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            return rt;
        }

        static RectTransform MakeImg(RectTransform parent, string name, Color c, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size) => MakeImg(parent, name, c, aMin, aMax, pos, size, out _);

        static RectTransform MakeImg(RectTransform parent, string name, Color c, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, out Image img)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.pivot = new Vector2(aMin.x == 0 ? 0 : 0.5f, aMin.y == 0 ? 0 : 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            img = go.GetComponent<Image>();
            img.color = c;
            img.raycastTarget = false;
            return rt;
        }

        static TextMeshProUGUI MakeText(RectTransform parent, string name, float px, TextAlignmentOptions align, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            rt.pivot = new Vector2(align == TextAlignmentOptions.Center ? 0.5f : 0, align == TextAlignmentOptions.Center ? 0.5f : 0);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var t = go.GetComponent<TextMeshProUGUI>();
            t.fontSize = px;
            t.alignment = align;
            t.color = White;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            if (UiTheme.Ready && UiTheme.BodyBold != null) t.font = UiTheme.BodyBold;
            return t;
        }

        /// <summary>NIGHT30: THE GLACIER's patience gauge (Glacier30.GaugeFrac), shown under the gap bar while the rebuilt fight runs; its words say why it
        /// fills (too far, out of sight) and what comes (the volley: dash sideways; the sweep: change lane). Off: -skygrindOff30 or the run's glacier30 block off.</summary>
        void Gauge30(bool bar)
        {
            if (game30 == null) game30 = FindFirstObjectByType<SurvivalGame>();
            SurvivalSim sim = game30 != null ? game30.sim : null;
            Skygrind.Core.Glacier30 gl = bar && N30Fixes.On && sim != null && sim.Glacier30On && sim.glacier28 != null && sim.glacier28.Alive ? sim.glacier30 : null;
            bool on = gl != null;
            GaugeDrawn = on && canvas.enabled;
            if (gauge.gameObject.activeSelf != on) gauge.gameObject.SetActive(on);
            if (!on) { gaugeShown = -1; GaugeFillNow = 0; GaugeWordsNow = ""; return; }
            float target = Mathf.Clamp01((float)gl.GaugeFrac);
            if (gl.VolleyTelling) target = 1f;
            gaugeShown = gaugeShown < 0 ? target : Mathf.Lerp(gaugeShown, target, 1f - Mathf.Exp(-UnityTuning.Hud29GapEase * FeelClock.UnscaledDelta));
            gaugeFill.sizeDelta = new Vector2(Mathf.Round(gaugeShown * UnityTuning.Hud29GapW * 2) / 2f, UnityTuning.Hud30GaugeBarH);
            GaugeFillNow = gaugeShown;
            string words = gl.VolleyTelling ? UnityTuning.Hud30GaugeVolley
                : gl.SweepTelling ? UnityTuning.Hud30GaugeSweep
                : gl.Hidden ? UnityTuning.Hud30GaugeHidden
                : gl.Far ? UnityTuning.Hud30GaugeFar
                : UnityTuning.Hud30GaugeRest;
            if (!ReferenceEquals(words, gaugeLabel))
            {
                gaugeLabel = words;
                GaugeWordsNow = words;
                gaugeText.SetText(words);
                bool alarm = gl.VolleyTelling || gl.SweepTelling;
                gaugeText.color = alarm ? Danger : ReferenceEquals(words, UnityTuning.Hud30GaugeRest) ? (UiTheme.Ready ? UiTheme.Muted : White) : White;
            }
        }

        int tag33 = -1;

        /// <summary>NIGHT33 U (design.n33.hud33, the lead's round-3 note): with HUD33 drawn the DOORS OPEN tag moves inside the 54 px margin, under the
        /// top-left run lines (Hud33Layout.DoorsOpenTag), its ink plate gone (text with the theme's shadow); off: NIGHT29's corner tag. On a change only.</summary>
        void Tag33(bool on)
        {
            int want = on ? 1 : 0;
            if (want == tag33 || tag == null) return;
            tag33 = want;
            Hud33Layout.Box b = Hud33Layout.DoorsOpenTag;
            tag.anchoredPosition = on ? new Vector2((float)b.x, (float)b.y) : new Vector2(UnityTuning.Hud29TagMargin, -UnityTuning.Hud29TagMargin);
            Transform plate = tag.Find("Plate");
            if (plate != null && plate.TryGetComponent(out Image img)) img.enabled = !on;
            if (tagText != null)
            {
                tagText.alignment = on ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
                Material sm = on ? UiTheme.ShadowFor(tagText.font) : null;
                if (sm != null) tagText.fontSharedMaterial = sm;
            }
        }

        void Update()
        {
            if (canvas == null) Build();
            if (canvas.enabled == Hud29.MenuHidden) canvas.enabled = !Hud29.MenuHidden; // NIGHT30 P0 c: hidden under a menu
            bool doors = Hud29.DoorsOpenTag || Hud29.ReelForce;
            if (tag.gameObject.activeSelf != doors) tag.gameObject.SetActive(doors);
            Tag33(Ui.Hud33Live);
            if (Hud29.ReelForce && Hud29.BossGapMetres < 0) Hud29.BossGapMetres = 20; // the reel's stand-in gap
            bool bar = Hud29.BossGapMetres >= 0;
            if (gap.gameObject.activeSelf != bar) gap.gameObject.SetActive(bar);
            DrawnNow = canvas.enabled && (bar || doors);
            Gauge30(bar);
            if (!bar) { shown = -1; return; }
            if ((Hud29.BossGapMax <= 0 || Hud29.BossGapHurt < 0) && !scaleRead)
            {
                // nobody set the scale: take THE GLACIER's own gap numbers (design.n27.boss2.gap: the chase is lost at failAt, a pulse window opens at windowAt)
                SurvivalGame g = FindFirstObjectByType<SurvivalGame>();
                if (g != null && g.cfg != null)
                {
                    scaleRead = true;
                    if (Hud29.BossGapMax <= 0 && g.cfg.Has("design.n27.boss2.gap.failAt")) Hud29.BossGapMax = g.cfg.Num("design.n27.boss2.gap.failAt");
                    if (Hud29.BossGapHurt < 0 && g.cfg.Has("design.n27.boss2.gap.windowAt")) Hud29.BossGapHurt = g.cfg.Num("design.n27.boss2.gap.windowAt");
                }
            }
            float target = Hud29.FillShare(Hud29.BossGapMetres, Hud29.BossGapMax);
            shown = shown < 0 ? target : Mathf.Lerp(shown, target, 1f - Mathf.Exp(-UnityTuning.Hud29GapEase * FeelClock.UnscaledDelta));
            fill.sizeDelta = new Vector2(Mathf.Round(shown * UnityTuning.Hud29GapW * 2) / 2f, UnityTuning.Hud29GapBarH);
            bool hurtable = Hud29.BossGapHurt >= 0 && Hud29.BossGapMetres <= Hud29.BossGapHurt;
            fillImage.color = Hud29.BossGapDanger ? Danger : hurtable ? Cyan : White;
            bool line = Hud29.BossGapHurt >= 0;
            if (hurtImage.enabled != line) hurtImage.enabled = line;
            if (line) hurtLine.anchoredPosition = new Vector2(12 + Hud29.FillShare(Hud29.BossGapHurt, Hud29.BossGapMax) * UnityTuning.Hud29GapW, 6);
            string label = Hud29.Label(Hud29.BossGapMetres, Hud29.BossGapHurt);
            if (label != shownLabel) { shownLabel = label; gapText.SetText(label); }
        }
    }
}
