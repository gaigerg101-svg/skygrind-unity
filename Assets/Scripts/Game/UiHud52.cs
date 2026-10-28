// NIGHT52 builder U, hud52 (design.n52.hud52, a view row: no sim bit; NIGHT52.md section 4; GG on the HUD: "looks so AI generated", "too generic and
// cheap"; the green health bar was outside the palette). With the row on (and hud33 / hud43 / hud47 drawing, as on every default build), the two
// bottom corners become one family of hard-edged ink panels (ULTRAKILL / DOOM: a dark plate leaning with the display face, a hard offset shadow,
// a lit rule on its top edge, an accent blade on its outer edge):
//   - THE HEALTH BLOCK (bottom-left: the number, hud47's continuous bar, the boost charges under it) sits in its panel; the bar is BONE, the
//     overheal CYAN over it, RED-ORANGE only below 30 % (Hud52LowFrac: the fill, the number, the rule and the accent breathe); hud47's soft glow is
//     gone (hard edges), its ghost of lost health is red-orange; hud43's bracket gives way to the panel's edge (the 25 % notches stay).
//   - A HIT shakes the whole panel by decaying trauma (game-feel: shake = trauma^2 along three smooth sines, roll about the panel's centre; trauma
//     grows with the share lost; scaled by the CAMERA SHAKE setting) and flashes the rule red-orange; hud43's chroma split on the number stays.
//   - A HEAL pops the panel (1 -> 1.06 -> 1, the punch curve), flashes the healed segment, the fill and the rule cyan, and settles (ease-out).
//   - THE WEAPON BLOCK (bottom-right: the name, the LT label, the rail, the grapple row) sits in the mirrored panel (accent on the right edge),
//     sized to its contents; a swap kicks it outward and flashes its rule cyan.
// Nothing here is over the reticle: both panels hang off the bottom corners. Off (the row off, -skygrindOff52 or any older switch): nothing is built
// and hud43 / hud47 draw as NIGHT51 (every hook below is guarded by h52Built / H52Live). The a152 reel's Hud52Probe.ForceOff draws NIGHT51's
// colours and hides the panels in the same session (its before / after pairs). Numbers: UnityTuning.U52.cs.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>hud52's statics for the a152 reel.</summary>
    public static class Hud52Probe
    {
        /// <summary>hud52 was built this session (the row on).</summary>
        public static bool Built;
        /// <summary>The reel's "before" frame: NIGHT51's colours, the panels hidden (the row still built).</summary>
        public static bool ForceOff;
        /// <summary>The reel only: forces the row (null: the config's design.n52.hud52). Read once, when the HUD is built.</summary>
        public static bool? Force;
        public static float Frac = -1f, Trauma, ShakePx, PeakShakePx, Scale = 1f, PeakScale = 1f, KickPx, PeakKickPx;
        public static Color FillColor, EdgeColor, NumberColor, WeaponEdgeColor;
        public static int Hits, Heals, Swaps;
        /// <summary>The panels' rects this frame (canvas units at 1080p: x, y from the bottom-left / bottom-right corner, w, h).</summary>
        public static Rect HealthPanel, WeaponPanel;

        public static void ResetPeaks()
        {
            PeakShakePx = 0f;
            PeakScale = 1f;
            PeakKickPx = 0f;
        }
    }

    /// <summary>hud52's pure rules (EditMode: N52UTests).</summary>
    public static class Hud52Math
    {
        /// <summary>The fill's role at a health share: danger below Hud52LowFrac, else bone (never a green; never a hue outside the palette).</summary>
        public static bool Low(float share) => share < UnityTuning.Hud52LowFrac;

        /// <summary>A hit's trauma: the old trauma + the base + the share lost x the gain, clamped to 0..1 (hits add, they never reset).</summary>
        public static float AddTrauma(float trauma, float lostShare) =>
            Mathf.Clamp01(trauma + UnityTuning.Hud52TraumaBase + UnityTuning.Hud52TraumaPerShare * Mathf.Max(0f, lostShare));

        /// <summary>Trauma after `dt` s of decay.</summary>
        public static float Decay(float trauma, double dt) => Mathf.Max(0f, trauma - (float)(UnityTuning.Hud52TraumaDecay * dt));

        /// <summary>The panel's shake at trauma `trauma` and phase time `t` (s): offset (px at 1080p, before the setting's scale) and roll (deg).
        /// shake = trauma^2 (gentle low, sharp high); three incommensurate sines so it never repeats a buzz.</summary>
        public static Vector2 Shake(float trauma, double t, out float rollDeg)
        {
            float k = trauma * trauma;
            double w = t * UnityTuning.Hud52ShakeRate;
            rollDeg = UnityTuning.Hud52ShakeRollDeg * k * (float)Math.Sin(w * 1.1);
            return new Vector2(UnityTuning.Hud52ShakePx * k * (float)Math.Sin(w * 1.7), 0.7f * UnityTuning.Hud52ShakePx * k * (float)Math.Sin(w * 2.3));
        }

        /// <summary>The heal pop's scale `age` s after a heal (1 outside it).</summary>
        public static float Pop(double age) =>
            age < 0 || age >= UnityTuning.Hud52PopTime ? 1f : 1f + (UnityTuning.Hud52PopScale - 1f) * UiTween.Punch(age / UnityTuning.Hud52PopTime);

        /// <summary>A flash's strength `age` s into a `time` s window: 1 -> 0, eased out (fast, then settles); 0 outside.</summary>
        public static float Fade(double age, double time) => age < 0 || age >= time ? 0f : 1f - UiTween.OutCubic(age / time);

        /// <summary>The swap kick (px outward) `age` s after a swap: out at once, back with an overshoot (OutBack); 0 outside.</summary>
        public static float Kick(double age) => age < 0 || age >= UnityTuning.Hud52SwapTime ? 0f : UnityTuning.Hud52KickPx * (1f - UiTween.OutBack(age / UnityTuning.Hud52SwapTime));
    }

    public sealed partial class Ui
    {
        bool hud52On, hud52Read, h52Built;

        /// <summary>design.n52.hud52 (or the reel's Force), read once a session.</summary>
        bool Hud52On
        {
            get
            {
                if (!hud52Read && cfg != null) { hud52Read = true; hud52On = Hud52Probe.Force ?? NightRows.ViewOn(cfg, "hud52"); Hud52Probe.Built = false; } // Built: BuildHud52 sets it (a statics' leftover from an earlier session never reads as built)
                return hud52On;
            }
        }

        /// <summary>hud52 draws its look this frame (built, and not the reel's NIGHT51 frame).</summary>
        bool H52Live => h52Built && !Hud52Probe.ForceOff;

        // --- the health panel ---
        RectTransform h52HRoot;
        Image h52HShadow, h52HPlate, h52HEdge, h52HAccent, h52Heal;
        Vector2 h52HSize;
        float h52Last = -1f, h52Trauma, h52HealLo, h52HealHi;
        double h52ShakeT, h52HitAge = double.MaxValue, h52HealAge = double.MaxValue, h52Clock;
        bool h52ShownLive = true;

        // --- the weapon panel ---
        RectTransform h52WRoot;
        Image h52WShadow, h52WPlate, h52WEdge, h52WAccent;
        Vector2 h52WSize, h52WHome;
        float h52WContentW = -1f, h52WRail = -1f, h52WWant;
        string h52WName, h52WLt;
        double h52SwapAge = double.MaxValue;

        /// <summary>From the end of BuildHud33Now with hud52 on; needs hud43's bar and hud47's continuous bar (the default HUD); the weapon panel needs
        /// the rail (rail34). Any missing: that panel is not built (logged), the rest of the HUD as NIGHT51.</summary>
        void BuildHud52()
        {
            if (hp43Bar == null || !hp47Built)
            {
                Debug.LogWarning("[hud52] hud43 / hud47 are not drawing: the health panel is not built (NIGHT51's HUD)");
                return;
            }
            h52Built = true;
            Hud52Probe.Built = true;
            BuildHealth52();
            if (r34Root != null) BuildWeapon52();
            else Debug.LogWarning("[hud52] rail34 is off: the weapon panel is not built");
            BuildTop52(); // round 11 (UiHud52.Top.cs): the counter, the score and the room title in the same family
            game.OnNewRun += () =>
            {
                h52Last = -1f;
                h52Trauma = 0f;
                h52HitAge = h52HealAge = h52SwapAge = double.MaxValue;
                h52WName = null;
                Hud52Probe.Hits = Hud52Probe.Heals = Hud52Probe.Swaps = 0;
                Hud52Probe.ResetPeaks();
            };
        }

        /// <summary>A holder with its own nested canvas (its shake rebatches only itself), anchored at a bottom corner, `size` big, its corner on the
        /// screen's corner (so the elements moved into it keep their places exactly) and its pivot at `centre` (the panel's centre: the roll and the
        /// pop turn about it).</summary>
        RectTransform Holder52(string name, Vector2 corner, Vector2 size, Vector2 centre)
        {
            var rt = new GameObject(name, typeof(RectTransform), typeof(Canvas)).GetComponent<RectTransform>();
            rt.SetParent(hud33, false);
            rt.anchorMin = rt.anchorMax = corner;
            rt.sizeDelta = size;
            // the pivot as a share of the holder; the holder's corner sits on the screen's corner (bottom-left: (0,0); bottom-right: (W,0) from the left)
            float px = corner.x < 0.5f ? centre.x / size.x : 1f + centre.x / size.x;
            rt.pivot = new Vector2(px, centre.y / size.y);
            rt.anchoredPosition = corner.x < 0.5f ? new Vector2(rt.pivot.x * size.x, rt.pivot.y * size.y) : new Vector2(-(1f - rt.pivot.x) * size.x, rt.pivot.y * size.y);
            return rt;
        }

        /// <summary>Moves `t` into the holder keeping its anchored place (the holder's corner is the screen's corner).</summary>
        static void Into52(Transform t, RectTransform holder)
        {
            if (t == null) return;
            t.SetParent(holder, false);
        }

        /// <summary>The lean of a panel: the health panel leans with the display face (12 deg, its top to the right); the weapon panel is its mirror
        /// (its top to the left), so the two outer edges frame the screen's bottom corners alike (/ ... \).</summary>
        static float Lean52(bool right) => right ? -UiKit43.SlantTan : UiKit43.SlantTan;

        /// <summary>A plain Image leaning `tan` (UiSkew about its own centre).</summary>
        static Image Leaned52(Transform parent, string name, Color c, Vector2 corner, Vector2 pos, Vector2 size, float tan)
        {
            Image img = UiStyle33.Line(parent, name, c, corner, Vector2.zero, pos, size);
            UiSkew.Apply(img, tan);
            img.raycastTarget = false;
            return img;
        }

        /// <summary>A panel's four parts in `holder` (shadow, plate, edge rule, accent), `x0..x1` / `y0..y1` from the holder's corner, behind the
        /// elements moved into it.</summary>
        static void Panel52(RectTransform holder, Vector2 corner, float x0, float x1, float y0, float y1, bool accentRight, out Image shadow, out Image plate, out Image edge, out Image accent) =>
            Panel52(holder, corner, x0, x1, y0, y1, accentRight, UnityTuning.Hud52ShadowPx, UnityTuning.Hud52EdgeH, UnityTuning.Hud52AccentW, out shadow, out plate, out edge, out accent);

        /// <summary>Round 11: a panel with its own shadow offset, rule height and accent width (the room title's smaller family).</summary>
        static void Panel52(RectTransform holder, Vector2 corner, float x0, float x1, float y0, float y1, bool accentRight, float shadowPx, float eh, float aw, out Image shadow, out Image plate, out Image edge, out Image accent)
        {
            float tan = Lean52(accentRight), w = x1 - x0, h = y1 - y0;
            Color ink = Ink52;
            var at = new Vector2(x0, y0);
            shadow = Leaned52(holder, "Shadow 52", UiTheme.A(ink, UnityTuning.Hud52ShadowAlpha), corner, at + new Vector2(shadowPx, -shadowPx), new Vector2(w, h), tan);
            plate = Leaned52(holder, "Plate 52", UiTheme.A(ink, UnityTuning.Hud52PlateAlpha), corner, at, new Vector2(w, h), tan);
            // the rule on the top edge: the plate leans about its centre, so its top edge sits (h - eh) / 2 x tan aside
            edge = Leaned52(holder, "Edge 52", UiStyle33.Bone, corner, new Vector2(x0 + (h * 0.5f - eh * 0.5f) * tan, y1 - eh), new Vector2(w, eh), tan);
            accent = Leaned52(holder, "Accent 52", UiStyle33.Bone, corner, new Vector2(accentRight ? x1 - aw : x0, y0), new Vector2(aw, h), tan);
            // draw order: the shadow, the plate, the rule, the accent, then the moved elements
            shadow.transform.SetSiblingIndex(0);
            plate.transform.SetSiblingIndex(1);
            edge.transform.SetSiblingIndex(2);
            accent.transform.SetSiblingIndex(3);
        }

        /// <summary>The panels' ink (UiTheme's solid ink; a fallback before the theme is ready).</summary>
        static Color Ink52 => UiTheme.Ready ? UiTheme.InkSolid : new Color(0.047f, 0.043f, 0.086f, 1f);

        /// <summary>Round 11: a built panel's four parts moved to `x0..x1` / `y0..y1` (a panel fitted to text that changes).</summary>
        static void Place52(Image shadow, Image plate, Image edge, Image accent, float x0, float x1, float y0, float y1, bool accentRight, float shadowPx, float eh, float aw)
        {
            float tan = Lean52(accentRight), w = x1 - x0, h = y1 - y0;
            shadow.rectTransform.anchoredPosition = new Vector2(x0 + shadowPx, y0 - shadowPx);
            shadow.rectTransform.sizeDelta = new Vector2(w, h);
            plate.rectTransform.anchoredPosition = new Vector2(x0, y0);
            plate.rectTransform.sizeDelta = new Vector2(w, h);
            edge.rectTransform.anchoredPosition = new Vector2(x0 + (h * 0.5f - eh * 0.5f) * tan, y1 - eh);
            edge.rectTransform.sizeDelta = new Vector2(w, eh);
            accent.rectTransform.anchoredPosition = new Vector2(accentRight ? x1 - aw : x0, y0);
            accent.rectTransform.sizeDelta = new Vector2(aw, h);
        }

        // ---- the health block ----
        void BuildHealth52()
        {
            var bl = Vector2.zero;
            float side = UnityTuning.Hud33SideMargin, edge = UnityTuning.Hud33EdgeMargin;
            Vector2 barPos = hp43Bar.anchoredPosition, barSize = hp43Bar.sizeDelta;
            float numRight = side + UnityTuning.Hud43NumW;
            float fitW = h33Health.main.GetPreferredValues(UnityTuning.Hud52NumFit).x;
            float x0 = numRight - fitW - UnityTuning.Hud52PadL;
            float x1 = barPos.x + barSize.x + barSize.y * UiKit43.SlantTan * 0.5f + UnityTuning.Hud52PadR;
            float y0 = edge - UnityTuning.Hud52PadB;
            float notchTop = barPos.y + barSize.y + UnityTuning.Hud43NotchGap + UnityTuning.Hud43NotchH;
            float y1 = Mathf.Max(edge + UnityTuning.Hud43NumPx * UnityTuning.Hud52NumCapShare, notchTop) + UnityTuning.Hud52PadT;
            h52HSize = new Vector2(x1 + 2f * UnityTuning.Hud52ShadowPx + 40f, y1 + 40f); // the holder: the panel and room for the shake
            h52HRoot = Holder52("Health 52", bl, h52HSize, new Vector2((x0 + x1) * 0.5f, (y0 + y1) * 0.5f));
            h52HRoot.SetSiblingIndex(Mathf.Min(h33Health.rt.GetSiblingIndex(), hp43Bar.GetSiblingIndex()));
            Into52(h33Health.rt, h52HRoot);
            Into52(h33StamFill != null && h33StamFill.Length > 0 && h33StamFill[0] != null ? h33StamFill[0].rectTransform.parent : null, h52HRoot);
            Into52(hp43Bar, h52HRoot);
            Panel52(h52HRoot, bl, x0, x1, y0, y1, false, out h52HShadow, out h52HPlate, out h52HEdge, out h52HAccent);
            Hud52Probe.HealthPanel = new Rect(x0, y0, x1 - x0, y1 - y0);
            // the healed segment's cyan flash, over the fill (under the notches)
            h52Heal = UiKit43.Blade(hp43Bar, "Heal 52", UiTheme.A(UiStyle33.Cyan, 0f), bl, Vector2.zero, Vector2.zero, new Vector2(1f, barSize.y));
            h52Heal.raycastTarget = false;
            h52Heal.transform.SetSiblingIndex(hp47Fill.transform.GetSiblingIndex() + 1);
            if (hp47Over != null) hp47Over.transform.SetSiblingIndex(h52Heal.transform.GetSiblingIndex() + 1);
        }

        /// <summary>From the end of Health43Draw with hud52 built (every older layer has set this frame's colours): the panel's state, the shake,
        /// the pop, the heal flash; the number's and the fill's colours in the palette.</summary>
        void Health52Draw(Health h, float frac, double frameDt)
        {
            bool live = H52Live;
            if (live != h52ShownLive)
            {
                h52ShownLive = live;
                foreach (Image im in new[] { h52HShadow, h52HPlate, h52HEdge, h52HAccent, h52Heal }) if (im != null) im.gameObject.SetActive(live);
                if (hp43Bracket != null && hp43Bracket.Length > 0 && hp43Bracket[0] != null) hp43Bracket[0].transform.parent.gameObject.SetActive(!live);
                if (h52WRoot != null) foreach (Image im in new[] { h52WShadow, h52WPlate, h52WEdge, h52WAccent }) if (im != null) im.gameObject.SetActive(live);
                if (!live)
                {
                    h52HRoot.anchoredPosition = HomeOf52(h52HRoot);
                    h52HRoot.localRotation = Quaternion.identity;
                    h52HRoot.localScale = Vector3.one;
                    if (h52WRoot != null) h52WRoot.anchoredPosition = h52WHome;
                }
            }
            if (hp43Bracket != null && hp43Bracket.Length > 0 && hp43Bracket[0] != null && hp43Bracket[0].transform.parent.gameObject.activeSelf == live)
                hp43Bracket[0].transform.parent.gameObject.SetActive(!live); // hud43 may have shown it again
            if (!live) return;
            h52Clock += frameDt;
            float tot = (float)((h.hp + h.over) / Math.Max(1, h.max)); // the overheal counts: a hit on the overheal still shakes
            if (h52Last < 0f) h52Last = tot;
            if (tot < h52Last - 1e-4f)
            {
                h52Trauma = Hud52Math.AddTrauma(h52Trauma, h52Last - tot);
                h52HitAge = 0;
                Hud52Probe.Hits++;
            }
            else if (tot >= h52Last + UnityTuning.Health42HealMin)
            {
                h52HealLo = h52HealAge < UnityTuning.Hud52HealTime ? Mathf.Min(h52HealLo, h52Last) : h52Last;
                h52HealHi = tot;
                h52HealAge = 0;
                Hud52Probe.Heals++;
            }
            h52Last = tot;

            // the shake (decaying trauma) and the pop (heal), about the panel's centre
            h52Trauma = Hud52Math.Decay(h52Trauma, frameDt);
            h52ShakeT += frameDt;
            float ss = (float)CameraFeel.ShakeScale(game.settings);
            Vector2 off = Hud52Math.Shake(h52Trauma, h52ShakeT, out float roll) * ss;
            off = new Vector2(Mathf.Round(off.x), Mathf.Round(off.y)); // whole px: the hard edge never smears
            Vector2 home = HomeOf52(h52HRoot);
            if (h52HRoot.anchoredPosition != home + off) h52HRoot.anchoredPosition = home + off;
            Quaternion rq = Quaternion.Euler(0, 0, roll * ss);
            if (h52HRoot.localRotation != rq) h52HRoot.localRotation = rq;
            float pop = Hud52Math.Pop(h52HealAge);
            var ps = new Vector3(pop, pop, 1f);
            if (h52HRoot.localScale != ps) h52HRoot.localScale = ps;

            float hitK = Hud52Math.Fade(h52HitAge, UnityTuning.Hud52HitTime), healK = Hud52Math.Fade(h52HealAge, UnityTuning.Hud52HealTime);
            if (h52HitAge < double.MaxValue) h52HitAge = h52HitAge > UnityTuning.Hud52HitTime ? double.MaxValue : h52HitAge + frameDt;
            if (h52HealAge < double.MaxValue) h52HealAge = h52HealAge > UnityTuning.Hud52HealTime + UnityTuning.Hud52PopTime ? double.MaxValue : h52HealAge + frameDt;
            bool low = Hud52Math.Low(frac);
            float breathe = low ? 0.5f + 0.5f * Mathf.Cos((float)(h52Clock * UnityTuning.Hud52BreatheHz * 2 * Math.PI)) : 0f;
            Color bone = UiStyle33.Bone, cyan = UiStyle33.Cyan, danger = UiStyle33.Danger;

            // the fill (bone; red-orange low; cyan at a heal's peak); the number in the same role
            Color fill = low ? danger : Color.Lerp(bone, cyan, healK * UnityTuning.Hud52HealFillMix);
            fill.a = 1f;
            if (hp47Fill.color != fill) hp47Fill.color = fill;
            Color num = low ? danger : healK > 0f ? Color.Lerp(bone, cyan, healK) : h.over > 0 ? cyan : bone;
            h33Health.Color = num;
            // the healed segment
            float bw = hp43Bar.sizeDelta.x;
            if (healK > 0f)
            {
                float lo = Mathf.Clamp01(h52HealLo), hi = Mathf.Clamp01(Mathf.Min(h52HealHi, 1f));
                RectTransform rt = h52Heal.rectTransform;
                rt.anchoredPosition = new Vector2(Mathf.Round(lo * bw), 0);
                rt.sizeDelta = new Vector2(Mathf.Max(1f, Mathf.Round((hi - lo) * bw)), rt.sizeDelta.y);
            }
            Color hc = UiTheme.A(Color.Lerp(cyan, Color.white, 0.35f), Mathf.Round(healK * 50f) / 50f);
            if (h52Heal.color != hc) h52Heal.color = hc;

            // the edge rule and the accent: bone at rest, red-orange on a hit and breathing low, cyan on a heal
            Color ec = UiTheme.A(bone, UnityTuning.Hud52EdgeRestAlpha), ac = bone;
            if (low)
            {
                ec = Color.Lerp(UiTheme.A(danger, UnityTuning.Hud52LowDim), danger, breathe);
                ac = ec;
            }
            if (healK > 0f) { ec = Color.Lerp(ec, cyan, healK); ac = Color.Lerp(ac, cyan, healK); }
            if (hitK > 0f) { ec = Color.Lerp(ec, danger, hitK); ac = Color.Lerp(ac, danger, hitK); }
            if (h52HEdge.color != ec) h52HEdge.color = ec;
            if (h52HAccent.color != ac) h52HAccent.color = ac;

            Hud52Probe.Frac = frac;
            Hud52Probe.Trauma = h52Trauma;
            Hud52Probe.ShakePx = off.magnitude;
            Hud52Probe.PeakShakePx = Mathf.Max(Hud52Probe.PeakShakePx, off.magnitude);
            Hud52Probe.Scale = pop;
            Hud52Probe.PeakScale = Mathf.Max(Hud52Probe.PeakScale, pop);
            Hud52Probe.FillColor = fill;
            Hud52Probe.EdgeColor = ec;
            Hud52Probe.NumberColor = num;
        }

        /// <summary>A holder's resting anchoredPosition (its corner on the screen's corner).</summary>
        static Vector2 HomeOf52(RectTransform rt) => rt.anchorMin.x < 0.5f ? new Vector2(rt.pivot.x * rt.sizeDelta.x, rt.pivot.y * rt.sizeDelta.y) : new Vector2(-(1f - rt.pivot.x) * rt.sizeDelta.x, rt.pivot.y * rt.sizeDelta.y);

        /// <summary>hud47's colours under hud52 (Health47Bar asks): the fill's base (bone / red-orange), the ghost (red-orange), the overheal (cyan),
        /// no soft glow, no hud47 low pulse (hud52's edge breathes instead).</summary>
        Color Fill52(float frac) => Hud52Math.Low(frac) ? UiStyle33.Danger : UiStyle33.Bone;
        Color Ghost52 => UiTheme.A(UiStyle33.Danger, UnityTuning.Hud52GhostAlpha);
        Color Over52 => Color.Lerp(UiStyle33.Cyan, Color.white, UnityTuning.Hud52OverWhite);

        // ---- the weapon block ----
        void BuildWeapon52()
        {
            var br = new Vector2(1, 0);
            float side = UnityTuning.Hud33SideMargin, edge = UnityTuning.Hud33EdgeMargin;
            Vector2 nameAt = h33Weapon.rt.anchoredPosition;
            float y0 = edge - UnityTuning.Hud52WPadB;
            float y1 = nameAt.y + UnityTuning.Hud47NamePx + UnityTuning.Hud52WPadT;
            float grappleLow = UnityTuning.Hud47GrappleY - UnityTuning.Hud52WTopGap - 8f; // the grapple row's lowest px (its ticks are ~16 px tall about their centre)
            if (h38GrRt != null) y1 = Mathf.Min(y1, grappleLow);
            float maxW = (float)UnityTuning.Hud33WeaponBoxW + UnityTuning.Hud52WPadL + UnityTuning.Hud52WPadR;
            h52WSize = new Vector2(maxW + 2f * UnityTuning.Hud52ShadowPx + 40f + side, y1 + 60f);
            // the pivot at the panel's right part's centre (the kick only moves it; no roll or pop on the weapon block)
            h52WRoot = Holder52("Weapon 52", br, h52WSize, new Vector2(-side - 120f, (y0 + y1) * 0.5f));
            h52WHome = h52WRoot.anchoredPosition;
            h52WRoot.SetSiblingIndex(Mathf.Min(h33Weapon.rt.GetSiblingIndex(), Mathf.Min(h33Lt.rt.GetSiblingIndex(), r34Root.GetSiblingIndex())));
            Into52(h33Weapon.rt, h52WRoot);
            Into52(h33Lt.rt, h52WRoot);
            Into52(r34Root, h52WRoot);
            if (h38GrRt != null) Into52(h38GrRt, h52WRoot);
            // the panel at its widest; Weapon52 fits its width to the contents when the name or the rail changes
            float x1 = -side + UnityTuning.Hud52WPadR, x0 = -side - (float)UnityTuning.Hud33WeaponBoxW - UnityTuning.Hud52WPadL;
            Panel52(h52WRoot, br, x0, x1, y0, y1, true, out h52WShadow, out h52WPlate, out h52WEdge, out h52WAccent);
            Hud52Probe.WeaponPanel = new Rect(x0, y0, x1 - x0, y1 - y0);
            h52WContentW = -1f;
        }

        /// <summary>From the end of Weapon33 with hud52 built: the panel fitted to the name, the LT label and the rail; the swap's kick and flash.</summary>
        void Weapon52(double frameDt)
        {
            if (h52WRoot == null || !H52Live) return;
            string name = h33Weapon.Shown, lt = h33Lt.Shown;
            float railW = r34Root.gameObject.activeInHierarchy ? r34Root.sizeDelta.x : 0f;
            bool nameNew = !string.Equals(name, h52WName, StringComparison.Ordinal);
            if (nameNew && h52WName != null && name.Length > 0) { h52SwapAge = 0; Hud52Probe.Swaps++; }
            // the contents' width: measured only when the name, the LT label or the rail changed (a TMP layout pass is not a per-frame cost)
            if (nameNew || !string.Equals(lt, h52WLt, StringComparison.Ordinal) || Mathf.Abs(railW - h52WRail) > 0.5f)
            {
                h52WName = name;
                h52WLt = lt;
                h52WRail = railW;
                float nameW = name.Length > 0 ? h33Weapon.main.GetPreferredValues(name).x : 0f;
                float ltW = lt.Length > 0 ? h33Lt.main.GetPreferredValues(lt).x : 0f;
                h52WWant = Mathf.Min((float)UnityTuning.Hud33WeaponBoxW, Mathf.Ceil(Mathf.Max(railW, Mathf.Max(nameW, ltW))));
            }
            float cw = h52WWant;
            if (Mathf.Abs(cw - h52WContentW) > 0.5f)
            {
                h52WContentW = cw;
                float side = UnityTuning.Hud33SideMargin;
                float x1 = -side + UnityTuning.Hud52WPadR, x0 = -side - cw - UnityTuning.Hud52WPadL;
                Rect r = Hud52Probe.WeaponPanel;
                float w = x1 - x0, hgt = r.height, tan = Lean52(true), eh = UnityTuning.Hud52EdgeH;
                h52WShadow.rectTransform.anchoredPosition = new Vector2(x0 + UnityTuning.Hud52ShadowPx, r.y - UnityTuning.Hud52ShadowPx);
                h52WShadow.rectTransform.sizeDelta = new Vector2(w, hgt);
                h52WPlate.rectTransform.anchoredPosition = new Vector2(x0, r.y);
                h52WPlate.rectTransform.sizeDelta = new Vector2(w, hgt);
                h52WEdge.rectTransform.anchoredPosition = new Vector2(x0 + (hgt * 0.5f - eh * 0.5f) * tan, r.y + hgt - eh);
                h52WEdge.rectTransform.sizeDelta = new Vector2(w, eh);
                Hud52Probe.WeaponPanel = new Rect(x0, r.y, w, hgt);
            }
            float kick = Mathf.Round(Hud52Math.Kick(h52SwapAge));
            float swapK = Hud52Math.Fade(h52SwapAge, UnityTuning.Hud52SwapTime);
            if (h52SwapAge < double.MaxValue) h52SwapAge = h52SwapAge > UnityTuning.Hud52SwapTime ? double.MaxValue : h52SwapAge + frameDt;
            Vector2 at = h52WHome + new Vector2(kick, 0);
            if (h52WRoot.anchoredPosition != at) h52WRoot.anchoredPosition = at;
            Color ec = Color.Lerp(UiTheme.A(UiStyle33.Bone, UnityTuning.Hud52EdgeRestAlpha), UiStyle33.Cyan, swapK);
            if (h52WEdge.color != ec) h52WEdge.color = ec;
            Color ac = UiStyle33.Cyan; // the held gun is the player's usable: the accent is cyan (the rail's held icon's colour)
            if (h52WAccent.color != ac) h52WAccent.color = ac;
            Hud52Probe.KickPx = kick;
            Hud52Probe.PeakKickPx = Mathf.Max(Hud52Probe.PeakKickPx, Mathf.Abs(kick));
            Hud52Probe.WeaponEdgeColor = ec;
        }
    }
}
