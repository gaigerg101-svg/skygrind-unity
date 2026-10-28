// NIGHT47 builder F (design.n47.hud47, a view row: no sim bit; NIGHT47.md G5 system, G16, G18, G19; GG 2026-10-18, verbatim: "The health bar should
// be a single continuous bar instead of a series of tiny segments, glowing green and transitioning smoothly through gradients, green to lighter green
// to lime to yellow to orange to red ... The enemy count in the top left needs to be larger and visually cleaner because the current tiny bars look
// bad ... Remove the strange blue bar under the aim reticle and the charged attack indicator because it's unclear and misplaced"). With the row on
// (and hud33's layer drawing; the spec and the measured baselines: Research/2026-10-18-wave47/F-cards.md section 2):
//   - the health bar: hud43's ten cells hide; one continuous fill in the same bracket, its colour by the health share through six stops interpolated in
//     OKLab, a soft glow in its own colour, a lighter ghost of lost health (0.4 s, then drains), and below 25 % a pulse and a brighter number (not hue
//     alone); hud43's number, bracket, notches and heal glow stay;
//   - the counter: "6 LEFT" larger (48 / 30 px), the per-enemy pips gone, one thin line under it that drains per kill, "LAST ONE" at 1, CLEAR at 0;
//   - the reticle alone: the arc under it (hud38 / arc40 / meter42) and the dot's two charge arcs (hud33) are not drawn; what they said stays readable
//     elsewhere (PRINCIPLES 1): the KESTREL / THUNDERCLAP charge on the gun (Viewmodel.Charge35's barrel glow and tip flash, existing) and the rail's
//     hand bar + CHARGING / CHARGED label (existing); the cooldowns on the rail's slot lines (existing); NEW here: the PULSE's five kill pips and the
//     BLAST's cooldown line over the gauntlet's slot, the STITCHER's heat line over its slot, and the grapple's hook + charges row moved from under the
//     reticle to the weapon panel;
//   - text: the fight / teach / banner / corner sizes of F-cards 2a and a dark TMP outline on hud33's labels.
// Off (the row off, -skygrindOff47 or any older switch): nothing here is built and every older HUD row draws as NIGHT46. Numbers: UnityTuning.F47.cs.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>hud47's statics for the reels (a647 prints them).</summary>
    public static class Hud47Probe
    {
        public static bool Live;
        public static float Frac = -1f, Ghost = -1f;
        public static Color BarColor;
        public static int CounterShown = -1, PipsLit = -1;
        public static string CounterWord = "";
        /// <summary>The fight line shown now ("" none) and the lines shown this session; the a647 reel's forces (a teach card, a banner).</summary>
        public static string FightLine = "";
        public static int FightLines;
        /// <summary>n47-r1e's guard: the text objects drawing the fight line (want 1) and whether its drawn text still holds a raw tag (want false).</summary>
        public static int FightObjects;
        public static bool FightRawTag;
        public static string ForceTeach, ForceBanner;
        public static bool ForceTeachOff;
        /// <summary>The a647 reel only: forces the row (null: the config's design.n47.hud47). Read once, when the HUD is built.</summary>
        public static bool? Force;
    }

    /// <summary>hud47's pure rules (EditMode: N47FeelTests).</summary>
    public static class Hud47Math
    {
        static float Lin(float c) => c <= 0.04045f ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);
        static float Srgb(float c) => c <= 0.0031308f ? 12.92f * c : 1.055f * Mathf.Pow(Mathf.Max(0f, c), 1f / 2.4f) - 0.055f;

        /// <summary>sRGB (0..1, gamma) to OKLab (Bjorn Ottosson, 2021 matrices; Research/2026-10-18-wave47/F.md R1).</summary>
        public static Vector3 ToOklab(Color c)
        {
            float r = Lin(c.r), g = Lin(c.g), b = Lin(c.b);
            float l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
            float m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
            float s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;
            l = Cbrt(l); m = Cbrt(m); s = Cbrt(s);
            return new Vector3(
                0.2104542553f * l + 0.7936177850f * m - 0.0040720468f * s,
                1.9779984951f * l - 2.4285922050f * m + 0.4505937099f * s,
                0.0259040371f * l + 0.7827717662f * m - 0.8086757660f * s);
        }

        /// <summary>OKLab to sRGB (0..1, gamma; clamped), alpha 1.</summary>
        public static Color FromOklab(Vector3 lab)
        {
            float l = lab.x + 0.3963377774f * lab.y + 0.2158037573f * lab.z;
            float m = lab.x - 0.1055613458f * lab.y - 0.0638541728f * lab.z;
            float s = lab.x - 0.0894841775f * lab.y - 1.2914855480f * lab.z;
            l = l * l * l; m = m * m * m; s = s * s * s;
            float r = 4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
            float g = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
            float b = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;
            return new Color(Mathf.Clamp01(Srgb(r)), Mathf.Clamp01(Srgb(g)), Mathf.Clamp01(Srgb(b)), 1f);
        }

        static float Cbrt(float x) => x < 0 ? -Mathf.Pow(-x, 1f / 3f) : Mathf.Pow(x, 1f / 3f);

        /// <summary>OKLab hue in degrees (0..360).</summary>
        public static float HueDeg(Color c)
        {
            Vector3 lab = ToOklab(c);
            float h = Mathf.Atan2(lab.z, lab.y) * Mathf.Rad2Deg;
            return h < 0 ? h + 360f : h;
        }

        /// <summary>The bar's colour at health share `share` (0..1): the six stops (UnityTuning.Hud47HealthStops, high to low), mixed in OKLab.</summary>
        public static Color Health(float share)
        {
            float[] st = UnityTuning.Hud47HealthStops;
            uint[] hx = UnityTuning.Hud47HealthHex;
            share = Mathf.Clamp01(share);
            for (int i = 0; i < st.Length - 1; i++)
            {
                if (share >= st[i + 1])
                {
                    float t = (st[i] - share) / (st[i] - st[i + 1]);
                    return Mix(Look.Hex(hx[i]), Look.Hex(hx[i + 1]), t);
                }
            }
            return Look.Hex(hx[hx.Length - 1]);
        }

        /// <summary>The overheal layer's colour at `over` (0 at 100 health .. 1 at the cap): blue, deeper blue, purple, mixed in OKLab.</summary>
        public static Color Over(float over)
        {
            uint[] hx = UnityTuning.Hud47OverHex;
            over = Mathf.Clamp01(over) * (hx.Length - 1);
            int i = Mathf.Min(hx.Length - 2, Mathf.FloorToInt(over));
            return Mix(Look.Hex(hx[i]), Look.Hex(hx[i + 1]), over - i);
        }

        public static Color Mix(Color a, Color b, float t) => FromOklab(Vector3.Lerp(ToOklab(a), ToOklab(b), Mathf.Clamp01(t)));

        /// <summary>The ghost (lost health, lighter): holds `wait` then drains at `rate` of the max per s; a new hit restarts the hold.</summary>
        public static float Ghost(float ghost, float last, float fill, ref double wait, double dt)
        {
            if (ghost < 0f) return fill;
            if (fill < last - 1e-4f)
            {
                wait = UnityTuning.Hud47GhostWait;
                return Mathf.Max(ghost, last);
            }
            if (fill > ghost) return fill;
            if (wait > 0) { wait -= dt; return ghost; }
            return Mathf.Max(fill, ghost - (float)(UnityTuning.Hud47GhostRate * dt));
        }

        /// <summary>The counter's word: CLEAR at 0, LAST ONE at 1, LEFT otherwise.</summary>
        public static string CounterWord(int n) => n == 0 ? UnityTuning.Counter43ClearWord : n == 1 ? UnityTuning.Hud47LastOneWord : UnityTuning.Counter43LeftWord;
    }

    public sealed partial class Ui
    {
        bool hud47On, hud47Read, hud47Built;

        /// <summary>design.n47.hud47 (or the reel's Force), read once a session.</summary>
        bool Hud47On
        {
            get
            {
                if (!hud47Read && cfg != null) { hud47Read = true; hud47On = Hud47Probe.Force ?? NightRows.ViewOn(cfg, "hud47"); }
                return hud47On;
            }
        }

        // --- the bar ---
        bool hp47Built;
        Image hp47Track, hp47Glow, hp47GhostImg, hp47Fill, hp47Over;
        float hp47Ghost = -1f, hp47Last = -1f;
        double hp47Wait, hp47Clock;

        // --- the counter ---
        bool c47Built;
        Image c47Line, c47LineTrack;
        int c47Peak;
        float c47LineShown = -1f, c47LineTo = 1f, c47LineFrom = 1f;
        double c47LineAge = double.MaxValue, c47Clock;

        // --- the rail's reads ---
        Image[] r47Pips;
        Image r47Blast, r47Heat;

        /// <summary>From the end of BuildHud33Now with hud47 on (every older layer exists by then).</summary>
        void BuildHud47()
        {
            hud47Built = true;
            Hud47Probe.Live = true;
            if (hp43Bar != null) BuildHealth47();
            if (c43Root != null) BuildCounter47();
            if (r34Root != null) BuildRail47();
            if (h38GrRt != null)
            {
                // the grapple's hook + charges: off the reticle, over the weapon panel (bottom-right)
                var br = new Vector2(1, 0);
                h38GrRt.SetParent(hud33, false);
                h38GrRt.anchorMin = h38GrRt.anchorMax = br;
                h38GrRt.anchoredPosition = new Vector2(UnityTuning.Hud47GrappleX - UnityTuning.Hud38PipX0 - UnityTuning.Hud38PipPitch, UnityTuning.Hud47GrappleY);
            }
            // the lead's n47-r0 frame: "a small slanted cyan tick still sits under the reticle" = the BEND meter's slash (UiRail34.BuildBendMeter34: it
            // joins the grapple's hook row at Hud38BendX under the reticle). It moves with the row to the weapon panel (its read, the BEND meter, kept).
            if (r34MeterFill != null)
            {
                Transform host = h38GrRt != null ? (Transform)h38GrRt : hud33;
                foreach (Image im in new[] { r34MeterTrack, r34MeterFill })
                {
                    if (im == null) continue;
                    RectTransform rt = im.rectTransform;
                    rt.SetParent(host, false);
                    if (h38GrRt != null) rt.anchoredPosition = new Vector2(UnityTuning.Hud38BendX, 0);
                    else { rt.anchorMin = rt.anchorMax = new Vector2(1, 0); rt.anchoredPosition = new Vector2(UnityTuning.Hud47GrappleX, UnityTuning.Hud47GrappleY); }
                }
            }
            Text47();
            // W's hand-off (G7): the first crack-open of a run puts up the big "PUNCH THE CORE" line (the teach card's head: 44 px under hud47), held
            // 2.0 s + 0.3 s a word (the text system's timing), then dropped
            game.events.On("wick47CrackOpen", o =>
            {
                if (core47Shown) return;
                core47Shown = true;
                ShowLesson25(UnityTuning.Hud47CoreLine);
                Fight47Post(ResolveLesson25(UnityTuning.Hud47CoreLine, null), 2); // its {punch} glyph filled in (the bar's hints are resolved the same way) // with the boss bar up, the one fight line carries it (a boss line: it outranks a card)
                core47Left = UnityTuning.Hud47LineHoldS + UnityTuning.Hud47LineHoldPerWord * UnityTuning.Hud47CoreLine.Split(' ').Length;
            });
            game.OnNewRun += () => { core47Shown = false; core47Left = 0; };
            Finale47Build(); // R4 finale47 (UiHud47.Finale.cs): the boss's name comes down in big type on bossDown
            BossRule61Wire(); // NIGHT61 G bossRule61 (UiBossRule61.cs): a campaign boss's rise arms its rule line (nothing is armed with the row off)
        }

        bool core47Shown;
        double core47Left;

        /// <summary>Each HUD frame (from Health47Bar): the core line's hold.</summary>
        void Line47Tick(double frameDt)
        {
            Finale47Tick(frameDt);
            if (core47Left <= 0) return;
            core47Left -= frameDt;
            if (core47Left <= 0) HideLesson25();
        }

        // ---- text sizes and the outline (F-cards 2a) ----
        void Text47()
        {
            Size47(h33Slot, UnityTuning.Hud47SlotPx);
            Size47(h33Weapon, UnityTuning.Hud47NamePx);
            Size47(h33Lt, UnityTuning.Hud47CornerPx);
            Size47(h33Round, UnityTuning.Hud47RoundPx);
            if (h33RunLines != null) foreach (UiStyle33.Text33 t in h33RunLines) Size47(t, UnityTuning.Hud47CornerPx);
            Size47(s42Name, UnityTuning.Hud47CornerPx);
            Size47(s42Award, UnityTuning.Hud47CornerPx);
            Size47(b43Name, UnityTuning.Hud47BossNamePx);
            Size47(b43Leg, UnityTuning.Hud47BossLabelPx);
            Size47(b43Tag, UnityTuning.Hud47BossLabelPx);
            Size47(b43Hint, UnityTuning.Hud47BossHintPx); // the boss bar's hint carries the fight lines (wick43.hints and W's): >= 36
            Size47(h33BossName, UnityTuning.Hud47BossNamePx);
            foreach (UiStyle33.Text33 t in new[] { h33Health, h33Weapon, h33Lt, h33Points, h33Rank, h33Round, c43Num, c43Label, s42Combo, s42Name, s42Award, b43Name, b43Leg, b43Tag, b43Hint, h33BossName })
                Outline47(t);
        }

        // TEXT SIZE (XAG 101: scaling to 200 %; F-cards 2a): each hud47 label's base size, rescaled when the setting changes
        readonly System.Collections.Generic.List<(UiStyle33.Text33 t, float px)> sized47 = new System.Collections.Generic.List<(UiStyle33.Text33, float)>();
        float textScale47 = 1f;

        /// <summary>A label at `px` (never smaller than it was) x TEXT SIZE, its box grown to fit the new height; remembered for a rescale.</summary>
        void Size47(UiStyle33.Text33 t, float px)
        {
            if (t == null || t.main == null) return;
            float basePx = Mathf.Max(px, t.main.fontSize);
            sized47.Add((t, basePx));
            Apply47(t, basePx * textScale47);
        }

        static void Apply47(UiStyle33.Text33 t, float px)
        {
            float grow = px - t.main.fontSize;
            if (Mathf.Abs(grow) < 0.01f) return;
            t.main.fontSize = px;
            if (t.back != null) t.back.fontSize = px;
            Vector2 sd = t.rt.sizeDelta;
            if (sd.y > 0) t.rt.sizeDelta = new Vector2(sd.x, Mathf.Max(1f, sd.y + grow));
        }

        /// <summary>Each HUD frame: TEXT SIZE read (1..2); on a change every hud47 label is resized (the teach card reads TextScale47 itself).</summary>
        void TextScale47Tick()
        {
            float s = game.settings != null && game.settings.Has(UnityTuning.TextSize47Setting) ? Mathf.Clamp((float)game.settings.Num(UnityTuning.TextSize47Setting), (float)UnityTuning.TextSize47Min, (float)UnityTuning.TextSize47Max) : 1f;
            if (Mathf.Abs(s - textScale47) < 1e-3f) return;
            textScale47 = s;
            foreach (var e in sized47) Apply47(e.t, e.px * s);
        }

        static void Outline47(UiStyle33.Text33 t)
        {
            if (t == null || t.main == null) return;
            Color so = UiStyle33.Shadow;
            t.main.outlineColor = new Color32((byte)(so.r * 255), (byte)(so.g * 255), (byte)(so.b * 255), 255);
            t.main.outlineWidth = UnityTuning.Hud47Outline;
        }

        /// <summary>copy47 (docs/COPY47.md; XAG: "lines of text in sentence case", a 1-2 word label exempt): the HUD's labels draw in forced capitals
        /// (UiStyle33.Label's caps: FontStyles.UpperCase), so copy47's sentence-case lines read in caps anyway (n47-r1e's teach card: "IN THE AIR,
        /// LOOK STRAIGHT DOWN"). With copy47 on, a label showing a line of 3+ words drops the forced capitals (its own caps stay: names, a line typed
        /// in caps); a 1-2 word label keeps them. Off: the forced capitals as NIGHT46.</summary>
        void Case47(UiStyle33.Text33 t, string shown)
        {
            if (t == null || t.main == null) return;
            bool sentence = false;
            if (Copy47.On(cfg) && !string.IsNullOrEmpty(shown))
            {
                string plain = System.Text.RegularExpressions.Regex.Replace(shown, "<[^>]*>", " ");
                sentence = plain.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length >= 3;
            }
            FontStyles want = sentence ? FontStyles.Normal : FontStyles.UpperCase;
            if (t.main.fontStyle != want) t.main.fontStyle = want;
            if (t.back != null && t.back.fontStyle != want) t.back.fontStyle = want;
        }

        /// <summary>n47-r1e: a drop shadow drawn by the text itself (TMP's SDF underlay on this label's own material instance), for a label whose
        /// shadow twin cannot be drawn (rich text with a glyph sprite: the twin would print the markup or double the sprite).</summary>
        static void Underlay47(TextMeshProUGUI t)
        {
            if (t == null) return;
            Material m = t.fontMaterial; // the label's own instance (the shared font material is untouched)
            if (m == null || !m.HasProperty(ShaderUtilities.ID_UnderlayColor)) return;
            m.EnableKeyword(ShaderUtilities.Keyword_Underlay);
            Color so = UiStyle33.Shadow;
            m.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(so.r, so.g, so.b, UnityTuning.Hud47UnderlayAlpha));
            m.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, UnityTuning.Hud47UnderlayOffset);
            m.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -UnityTuning.Hud47UnderlayOffset);
            m.SetFloat(ShaderUtilities.ID_UnderlaySoftness, UnityTuning.Hud47UnderlaySoftness);
            t.fontMaterial = m;
        }

        /// <summary>The teach card's line sizes (UiTeach43's Teach43Fit): hud47's larger sizes and floors, or hud43's.</summary>
        // NIGHT51 declutter51: smaller (N51Teach*; the text-size setting still scales them)
        float Teach47HeadPx => Night51.On ? UnityTuning.N51TeachHeadPx * (hud47Built ? textScale47 : 1) : hud47Built ? UnityTuning.Hud47TeachHeadPx * textScale47 : UnityTuning.Teach43HeadPx;
        float Teach47HeadMinPx => Night51.On ? UnityTuning.N51TeachHeadMinPx * (hud47Built ? textScale47 : 1) : hud47Built ? UnityTuning.Hud47TeachHeadMinPx * textScale47 : UnityTuning.Teach43HeadMinPx;
        float Teach47SubPx => Night51.On ? UnityTuning.N51TeachSubPx * (hud47Built ? textScale47 : 1) : hud47Built ? UnityTuning.Hud47TeachSubPx * textScale47 : UnityTuning.Teach43SubPx;
        float Teach47SubMinPx => Night51.On ? UnityTuning.N51TeachSubMinPx * (hud47Built ? textScale47 : 1) : hud47Built ? UnityTuning.Hud47TeachSubMinPx * textScale47 : UnityTuning.Teach43SubMinPx;

        // ---- the continuous bar (F-cards 2b) ----
        void BuildHealth47()
        {
            hp47Built = true;
            foreach (Image[] cells in new[] { hp43Track, hp43After, hp43Fill, hp43Glow })
                if (cells != null) foreach (Image c in cells) if (c != null) c.gameObject.SetActive(false);
            Vector2 bl = Vector2.zero;
            Vector2 size = hp43Bar.sizeDelta;
            hp47Track = UiKit43.Blade(hp43Bar, "Track 47", UiTheme.A(UiKit43.Muted, UnityTuning.Hud43TrackAlpha), bl, bl, Vector2.zero, size);
            float gh = size.y * UnityTuning.Hud47GlowGrow;
            hp47Glow = UiKit43.BladeFill(hp43Bar, "Glow 47", UiTheme.A(Color.white, 0f), bl, bl, new Vector2(0, -(gh - size.y) * 0.5f), new Vector2(size.x, gh));
            hp47GhostImg = UiKit43.BladeFill(hp43Bar, "Ghost 47", Color.white, bl, bl, Vector2.zero, size);
            hp47Fill = UiKit43.BladeFill(hp43Bar, "Fill 47", Color.white, bl, bl, Vector2.zero, size);
            hp47Over = UiKit43.BladeFill(hp43Bar, "Over 47", UiTheme.A(Color.white, 0f), bl, bl, Vector2.zero, size);
            hp47Over.fillAmount = 0f;
            UiSkew.Apply(UiKit43.Scan(hp47Fill.rectTransform, "Scan", UnityTuning.Ui43ScanAlpha), UiKit43.SlantTan);
            // the notches and the bracket (hud43's) stay over the fill
            if (hp43Notch != null) foreach (Image n in hp43Notch) if (n != null) n.transform.SetAsLastSibling();
            if (hp43Bracket != null && hp43Bracket.Length > 0 && hp43Bracket[0] != null) hp43Bracket[0].transform.parent.SetAsLastSibling();
            game.OnNewRun += () => { hp47Ghost = hp47Last = -1f; hp47Wait = 0; };
        }

        /// <summary>From Health43Draw in place of the cells (the number and the bracket are hud43's). `over`: 0 at 100 health .. 1 at the overheal cap
        /// (0 until overheal47 lands).</summary>
        void Health47Bar(float frac, float over, double frameDt)
        {
            hp47Clock += frameDt;
            Line47Tick(frameDt);
            TextScale47Tick();
            hp47Ghost = Hud47Math.Ghost(hp47Ghost, hp47Last < 0 ? frac : hp47Last, frac, ref hp47Wait, frameDt);
            hp47Last = frac;
            bool l52 = H52Live; // NIGHT52 U hud52 (UiHud52.cs): the palette's colours (bone / red-orange below 30 %, the ghost red-orange, the overheal cyan, no soft glow); hud52's edge breathes for low
            Color c = l52 ? Fill52(frac) : Hud47Math.Health(frac);
            bool low = !l52 && frac < UnityTuning.Hud47LowFrac;
            if (low)
            {
                float p = 0.5f + 0.5f * Mathf.Cos((float)(hp47Clock * UnityTuning.Hud47LowPulseHz * 2 * Math.PI));
                c = Color.Lerp(c, Color.Lerp(c, Color.white, UnityTuning.Hud47LowPulseWhite), p); // the lead's n47-r0 frame: the pulse dimmed 20 % to brown #A75A1B; it now only ever brightens (hotter, never muddier)
                c.a = 1f;
                h33Health.Color = Color.Lerp(h33Health.Color, Color.white, (UnityTuning.Hud47LowNumberGain - 1f) * p); // brighter with the pulse
            }
            float f = Mathf.Round(frac * 400f) / 400f, g = Mathf.Round(hp47Ghost * 400f) / 400f;
            if (hp47Fill.fillAmount != f) hp47Fill.fillAmount = f;
            if (hp47Fill.color != c) hp47Fill.color = c;
            if (hp47GhostImg.fillAmount != g) hp47GhostImg.fillAmount = g;
            Color gc = l52 ? Ghost52 : Color.Lerp(c, Color.white, UnityTuning.Hud47GhostWhite);
            if (hp47GhostImg.color != gc) hp47GhostImg.color = gc;
            hp47Glow.fillAmount = f;
            Color glow = UiTheme.A(c, l52 ? 0f : UnityTuning.Hud47GlowAlpha);
            if (hp47Glow.color != glow) hp47Glow.color = glow;
            float o = Mathf.Round(Mathf.Clamp01(over) * 400f) / 400f;
            if (hp47Over.fillAmount != o) hp47Over.fillAmount = o;
            Color oc = o > 0f ? (l52 ? Over52 : Hud47Math.Over(o)) : UiTheme.A(Color.white, 0f);
            if (hp47Over.color != oc) hp47Over.color = oc;
            if (o > 0f && !l52) h33Health.Color = Color.Lerp(Hud47Math.Over(o), Color.white, 0.25f); // the number reads "130" in the overheal's colour
            Hud47Probe.Frac = frac;
            Hud47Probe.Ghost = hp47Ghost;
            Hud47Probe.BarColor = c;
        }

        /// <summary>The overheal layer's share (0 at 100 .. 1 at the cap): overheal47's pool once it lands (round 2); 0 until then.</summary>
        float Over47Share(Health h)
        {
            double cap = game.sim != null ? game.sim.Overheal47Max : 0;
            return cap > 0 ? Mathf.Clamp01((float)(h.over / cap)) : 0f;
        }

        // ---- the counter (F-cards 2d) ----
        void BuildCounter47()
        {
            c47Built = true;
            float grow = UnityTuning.Hud47CounterNumPx - c43Num.main.fontSize;
            c43Num.main.fontSize = c43Num.back.fontSize = UnityTuning.Hud47CounterNumPx;
            c43Num.rt.sizeDelta = new Vector2(c43Num.rt.sizeDelta.x * UnityTuning.Hud47CounterNumPx / Mathf.Max(1f, UnityTuning.Counter43NumPx), UnityTuning.Hud47CounterNumPx);
            c43Label.main.fontSize = c43Label.back.fontSize = UnityTuning.Hud47CounterWordPx;
            c43Label.rt.sizeDelta = new Vector2(c43Label.rt.sizeDelta.x * 2.2f, UnityTuning.Hud47CounterWordPx + 6);
            c43Label.rt.anchoredPosition = new Vector2(UnityTuning.Counter43AccentW + UnityTuning.Counter43Gap + c43Num.rt.sizeDelta.x + UnityTuning.Counter43Gap, -(UnityTuning.Hud47CounterNumPx - UnityTuning.Hud47CounterWordPx) * 0.6f);
            c43Accent.rectTransform.sizeDelta = new Vector2(c43Accent.rectTransform.sizeDelta.x, UnityTuning.Hud47CounterNumPx);
            c43Root.sizeDelta = new Vector2(Mathf.Max(c43Root.sizeDelta.x, UnityTuning.Hud47CounterLineW + 40f), c43Root.sizeDelta.y + grow + 10f);
            foreach (Image p in c43Pips) if (p != null) p.gameObject.SetActive(false);
            var tl = new Vector2(0, 1);
            var at = new Vector2(UnityTuning.Counter43AccentW + UnityTuning.Counter43Gap, -(UnityTuning.Hud47CounterNumPx + 6f));
            c47LineTrack = UiStyle33.Line(c43Root, "Line track 47", UiTheme.A(UiKit43.Muted, 0.35f), tl, tl, at, new Vector2(UnityTuning.Hud47CounterLineW, UnityTuning.Hud47CounterLineH));
            c47Line = UiStyle33.Line(c43Root, "Line 47", UiStyle33.Danger, tl, tl, at, new Vector2(UnityTuning.Hud47CounterLineW, UnityTuning.Hud47CounterLineH));
            c47Line.sprite = UiStyle33.WhiteSprite();
            c47Line.type = Image.Type.Filled;
            c47Line.fillMethod = Image.FillMethod.Horizontal;
            c47Line.fillOrigin = (int)Image.OriginHorizontal.Left;
            game.OnNewRun += () => { c47Peak = 0; c47LineShown = -1f; c47LineAge = double.MaxValue; };
        }

        /// <summary>From Counter43 each frame it is shown (after the count and word are set): the word, the line, LAST ONE's pulse.</summary>
        void Counter47(int shown, bool opened, double frameDt)
        {
            c47Clock += frameDt;
            if (opened || shown > c47Peak) c47Peak = Math.Max(1, shown);
            string word = Hud47Math.CounterWord(shown);
            if (c43Label.Shown != word) c43Label.Set(word);
            Color wc = shown == 0 ? UiStyle33.Cyan : shown == 1 ? UiStyle33.Danger : UiKit43.Muted;
            if (shown == 1) wc = Color.Lerp(UiTheme.A(UiStyle33.Danger, 0.55f), UiStyle33.Danger, 0.5f + 0.5f * Mathf.Cos((float)(c47Clock * UnityTuning.Hud47LastOnePulseHz * 2 * Math.PI)));
            c43Label.Color = wc;
            float to = c47Peak > 0 ? Mathf.Clamp01(shown / (float)c47Peak) : 0f;
            if (c47LineShown < 0f) { c47LineShown = c47LineFrom = c47LineTo = to; }
            if (Mathf.Abs(to - c47LineTo) > 1e-4f) { c47LineFrom = c47LineShown; c47LineTo = to; c47LineAge = 0; }
            if (c47LineAge < UnityTuning.Hud47CounterLineEase)
            {
                c47LineAge += frameDt;
                c47LineShown = Mathf.Lerp(c47LineFrom, c47LineTo, UiTween.OutCubic((float)Math.Min(1.0, c47LineAge / UnityTuning.Hud47CounterLineEase)));
            }
            else c47LineShown = c47LineTo;
            c47Line.fillAmount = c47LineShown;
            c47Line.color = shown == 0 ? UiStyle33.Cyan : UiStyle33.Danger;
            Hud47Probe.CounterShown = shown;
            Hud47Probe.CounterWord = word;
        }

        // ---- the rail: the PULSE's pips + the BLAST's cooldown, the STITCHER's heat (F-cards 2e) ----
        void BuildRail47()
        {
            var br = new Vector2(1, 0);
            var bc = new Vector2(0.5f, 0);
            r47Pips = new Image[5];
            Color pink = Look.Hex(UnityTuning.Hud47GauntletPinkHex);
            for (int i = 0; i < r47Pips.Length; i++)
            {
                r47Pips[i] = UiKit43.Blade(r34Root, "PULSE pip " + i, pink, br, bc, Vector2.zero, new Vector2(UnityTuning.Hud47PipW, UnityTuning.Hud47PipH));
                r47Pips[i].gameObject.SetActive(false);
            }
            r47Blast = UiStyle33.Line(r34Root, "BLAST cooldown 47", pink, br, bc, Vector2.zero, new Vector2(5 * UnityTuning.Hud47PipPitch, UnityTuning.Hud47BlastLineH));
            r47Blast.sprite = UiStyle33.WhiteSprite();
            r47Blast.type = Image.Type.Filled;
            r47Blast.fillMethod = Image.FillMethod.Horizontal;
            r47Blast.gameObject.SetActive(false);
            r47Heat = UiStyle33.Line(r34Root, "Heat 47", UiStyle33.Cyan, br, bc, Vector2.zero, new Vector2(UnityTuning.Rail34LineW, UnityTuning.Hud47HeatLineH));
            r47Heat.sprite = UiStyle33.WhiteSprite();
            r47Heat.type = Image.Type.Filled;
            r47Heat.fillMethod = Image.FillMethod.Horizontal;
            r47Heat.gameObject.SetActive(false);
        }

        /// <summary>From Weapon33 right after RenderRail34 (the slots are placed).</summary>
        void Rail47()
        {
            if (r47Pips == null) return;
            SurvivalSim sim = game.sim;
            Loadout lo = game.loadout;
            Rail34Slot gs = null, ns = null;
            for (int i = 0; i < r34Slots.Count; i++)
            {
                Rail34Slot s = r34Slots[i];
                if (!s.rt.gameObject.activeSelf) continue;
                if (s.id == "gauntlet") gs = s;
                else if (s.id == "nailgun") ns = s;
            }
            float top = UnityTuning.Rail34HandBarH + UnityTuning.Rail34LineGap + UnityTuning.Rail34IconH + UnityTuning.Hud47PipUp;
            // GG 2026-10-26: over the rail the pips shared the LT label's band (Hud33Layout.RailLt: "ZL ZOOM" drawn on top of them); they now hang
            // under the rail's hand bar, in the bottom margin (the BLAST cooldown line under them)
            if (UnityTuning.Hud47PipsUnderRail) top = -(UnityTuning.Hud47PipH + UnityTuning.Hud47PipUnderGap);
            PulseCharge27 pc = sim != null ? sim.pulse27 : null;
            bool pulse = gs != null && pc != null && pc.active;
            int lit = pulse ? Mathf.Clamp(pc.pips, 0, 5) : -1;
            for (int i = 0; i < r47Pips.Length; i++)
            {
                Image p = r47Pips[i];
                if (p.gameObject.activeSelf != pulse) p.gameObject.SetActive(pulse);
                if (!pulse) continue;
                float x = gs.rt.anchoredPosition.x + (i - 2) * UnityTuning.Hud47PipPitch;
                p.rectTransform.anchoredPosition = new Vector2(x, top);
                bool ready = pc.Ready;
                Color pink = Look.Hex(UnityTuning.Hud47GauntletPinkHex);
                Color c = i < lit ? (ready ? Color.Lerp(pink, Color.white, 0.35f) : pink) : UiTheme.A(pink, UnityTuning.Hud47PipDimAlpha);
                if (pc.cooldownSteps > 0 && i < lit) c = UiTheme.A(pink, 0.6f);
                if (p.color != c) p.color = c;
            }
            bool cooling = pulse && pc.cooldownSteps > 0;
            if (r47Blast.gameObject.activeSelf != cooling) r47Blast.gameObject.SetActive(cooling);
            if (cooling)
            {
                r47Blast.rectTransform.anchoredPosition = new Vector2(gs.rt.anchoredPosition.x, UnityTuning.Hud47PipsUnderRail ? top - 3f - UnityTuning.Hud47BlastLineH : top + UnityTuning.Hud47PipH + 3f);
                r47Blast.fillAmount = (float)(1.0 - pc.CooldownShare);
            }
            bool heat = ns != null && lo != null;
            if (r47Heat.gameObject.activeSelf != heat) r47Heat.gameObject.SetActive(heat);
            if (heat)
            {
                r47Heat.rectTransform.anchoredPosition = new Vector2(ns.rt.anchoredPosition.x, top);
                r47Heat.fillAmount = (float)Mathf.Clamp01((float)lo.Heat);
                r47Heat.color = lo.Overheated ? UiStyle33.Danger : Color.Lerp(UiStyle33.Cyan, UiStyle33.Danger, (float)lo.Heat);
            }
            Hud47Probe.PipsLit = lit;
        }
    }
}
