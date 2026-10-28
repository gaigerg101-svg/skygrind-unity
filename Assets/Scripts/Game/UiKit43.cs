// NIGHT43 builder U1: the dusk instrument's kit (Research/2026-10-16-wave43/U1.md section 1). Two parts:
//   - Ui43Math: the language's rules in pure C# (no Unity object; EditMode tests: Ui43Tests): the CRT pop, the after-image, the tick punch, the
//     teach card's two lines, the boss bar's state word, cached number strings (no allocation per frame once warm).
//   - UiKit43: the builders every NIGHT43 view draws with: the blade (a 12 deg parallelogram), the bracket (two L's), the scanline overlay, the
//     cut plate. Unity objects are made on first use only (L-035: nothing here runs in a field initialiser).
// Numbers: UnityTuning.Ui43.cs.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>The NIGHT43 UI's rules (pure; Ui43Tests).</summary>
    public static class Ui43Math
    {
        static float Lerp(float a, float b, float k) => a + (b - a) * k;

        /// <summary>The CRT pop's scale `age` s after an open (X grows first, then Y; HudOpenEffect's shape). Both 1 when done.</summary>
        public static void Pop(double age, out float sx, out float sy)
        {
            float f = UnityTuning.Ui43OpenFrom;
            if (age < UnityTuning.Ui43OpenX) { sx = Lerp(f, 1f, UiTween.OutCubic(age / UnityTuning.Ui43OpenX)); sy = f; return; }
            double a = age - UnityTuning.Ui43OpenX;
            sx = 1f;
            sy = a < UnityTuning.Ui43OpenY ? Lerp(f, 1f, UiTween.OutCubic(a / UnityTuning.Ui43OpenY)) : 1f;
        }

        /// <summary>The close's scale `age` s after it began (Y collapses, then X); `done` once both are at the pop's floor.</summary>
        public static void Close(double age, out float sx, out float sy, out bool done)
        {
            float f = UnityTuning.Ui43OpenFrom;
            done = false;
            if (age < UnityTuning.Ui43CloseY) { sx = 1f; sy = Lerp(1f, f, UiTween.InCubic(age / UnityTuning.Ui43CloseY)); return; }
            double a = age - UnityTuning.Ui43CloseY;
            sy = f;
            if (a < UnityTuning.Ui43CloseX) { sx = Lerp(1f, f, UiTween.InCubic(a / UnityTuning.Ui43CloseX)); return; }
            sx = f;
            done = true;
        }

        /// <summary>The time the pop takes to full (s).</summary>
        public static double PopTime => UnityTuning.Ui43OpenX + UnityTuning.Ui43OpenY;

        /// <summary>One frame of an after-image (a share 0..1): a drop of `fill` under `last` holds the after-image at the old level and restarts its
        /// wait; when the wait runs out it drains at (gap x rate + base) per s toward the fill; it never sits under the fill (a heal lifts it).</summary>
        public static float After(float after, float last, float fill, ref double wait, double dt)
        {
            if (fill < last - 1e-4f)
            {
                after = Math.Max(after, last);
                wait = UnityTuning.Ui43AfterWait;
                return Math.Max(after, fill);
            }
            if (wait > 0) wait -= dt;
            else if (after > fill)
            {
                double gap = after - fill;
                after -= (float)((gap * UnityTuning.Ui43AfterRate + UnityTuning.Ui43AfterBase) * dt);
            }
            return Math.Max(after, fill);
        }

        /// <summary>A tick's punch scale `age` s after it (1 -> Ui43TickScale -> 1 over Ui43TickTime); 1 outside it.</summary>
        public static float Tick(double age) =>
            age < 0 || age >= UnityTuning.Ui43TickTime ? 1f : 1f + (UnityTuning.Ui43TickScale - 1f) * UiTween.Punch(age / UnityTuning.Ui43TickTime);

        /// <summary>The teach card's two lines from one teaching line: the verb phrase and the rest (the rule). The split is at the first sentence break
        /// whose head is a verb phrase (3 words or more, or it carries a tag: the button); a chain of short verbs stays on one line ("MOVE. LOOK. (B)
        /// JUMP." is one head). "HOLD RT to CHARGE the KESTREL. Release to fire." -> ("HOLD RT to CHARGE the KESTREL", "Release to fire.").</summary>
        public static void TeachSplit(string s, out string head, out string sub)
        {
            head = s ?? "";
            sub = "";
            if (string.IsNullOrEmpty(s)) return;
            int depth = 0, words = 0;
            bool inWord = false, tag = false;
            for (int i = 0; i < s.Length - 1; i++)
            {
                char c = s[i];
                if (c == '<') { depth++; tag = true; if (!inWord) { words++; inWord = true; } }
                else if (c == '>' && depth > 0) depth--;
                else if (depth == 0 && c == ' ') inWord = false;
                else if (depth == 0)
                {
                    if (!inWord && c != '.' && c != ':' && c != ',') { words++; inWord = true; }
                    if ((c == '.' || c == '!' || c == '?') && s[i + 1] == ' ' && (words >= 3 || tag))
                    {
                        string rest = s.Substring(i + 2).Trim();
                        if (rest.Length == 0) break;
                        head = s.Substring(0, i).Trim();
                        sub = rest;
                        return;
                    }
                }
            }
            if (head.EndsWith(".", StringComparison.Ordinal)) head = head.Substring(0, head.Length - 1); // a one-sentence verb line has no full stop on the card
        }

        /// <summary>The boss bar's state, in the order the bar reads it: OPEN (the player can hurt it most now) over LOCKED (it cannot be hurt) over
        /// the plain fight.</summary>
        public enum BossMode { None = 0, Normal = 1, Locked = 2, Open = 3 }

        public static BossMode Mode(bool valid, bool open, bool locked) => !valid ? BossMode.None : open ? BossMode.Open : locked ? BossMode.Locked : BossMode.Normal;

        static readonly Dictionary<int, string> nums = new Dictionary<int, string>(); // pure C#: safe in an initialiser

        /// <summary>A small non-negative int as a string, cached (no allocation per frame for a count or a timer).</summary>
        public static string Num(int n)
        {
            if (n < 0) n = 0;
            if (!nums.TryGetValue(n, out string s)) nums[n] = s = n.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return s;
        }

        static readonly Dictionary<int, string> legs = new Dictionary<int, string>();

        /// <summary>"LEG 1 / 3" (cached); "" when the boss has no legs.</summary>
        public static string Leg(int leg, int of)
        {
            if (of <= 0 || leg <= 0) return "";
            int key = leg * 100 + of;
            if (!legs.TryGetValue(key, out string s)) legs[key] = s = UnityTuning.Bar43LegWord + " " + Num(leg) + " / " + Num(of);
            return s;
        }

        /// <summary>A timer's text in tenths ("1.4"), cached by the tenth.</summary>
        public static string Tenths(double seconds)
        {
            int t = (int)Math.Ceiling(Math.Max(0, seconds) * 10 - 1e-6);
            if (!tenths.TryGetValue(t, out string s)) tenths[t] = s = (t / 10).ToString(System.Globalization.CultureInfo.InvariantCulture) + "." + (t % 10).ToString(System.Globalization.CultureInfo.InvariantCulture);
            return s;
        }

        static readonly Dictionary<int, string> tenths = new Dictionary<int, string>();

        /// <summary>hud43 (T-64): a style award is drawn only when it is worth a point on screen ("+0 WALL JUMP" is not).</summary>
        public static bool ShowAward(double points) => Math.Round(points) != 0;

        /// <summary>The counter's pips: how many to draw for `n` enemies left (capped; the number carries the rest).</summary>
        public static int Pips(int n) => Math.Max(0, Math.Min(n, UnityTuning.Counter43MaxPips));
    }

    /// <summary>The NIGHT43 builders (U1.md section 1.3): blades, brackets, scanlines, cut plates.</summary>
    public static class UiKit43
    {
        static Texture2D scan;
        static Color steel;
        static bool steelRead;

        public static float SlantTan => Mathf.Tan(UnityTuning.Ui43SlantDeg * Mathf.Deg2Rad);

        /// <summary>STEEL (U1.md 1.2): a boss that cannot be hurt now.</summary>
        public static Color Steel
        {
            get
            {
                if (!steelRead) { steel = Look.Hex(UnityTuning.Ui43SteelHex); steel.a = 1f; steelRead = true; }
                return steel;
            }
        }

        /// <summary>MUTED (U1.md 1.2): BONE toward INK.</summary>
        public static Color Muted
        {
            get
            {
                Color ink = UiTheme.Ready ? UiTheme.InkSolid : new Color(0.047f, 0.043f, 0.086f, 1f);
                Color m = Color.Lerp(UiStyle33.Bone, ink, UnityTuning.Ui43MutedMix);
                m.a = 1f;
                return m;
            }
        }

        /// <summary>A blade: a plain Image leaning 12 deg (its top to the right).</summary>
        public static Image Blade(Transform parent, string name, Color c, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            Image img = UiStyle33.Line(parent, name, c, anchor, pivot, pos, size);
            UiSkew.Apply(img, SlantTan);
            return img;
        }

        /// <summary>A blade that fills from the left (a meter's cell or a bar).</summary>
        public static Image BladeFill(Transform parent, string name, Color c, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            Image img = Blade(parent, name, c, anchor, pivot, pos, size);
            img.sprite = UiStyle33.WhiteSprite();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillOrigin = (int)Image.OriginHorizontal.Left;
            return img;
        }

        /// <summary>Stretch a rect over its parent.</summary>
        public static void Stretch(RectTransform rt, float pad = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-pad, -pad);
            rt.offsetMax = new Vector2(pad, pad);
        }

        /// <summary>The HUD bracket around `parent`'s rect, `pad` px outside it: an L at the top-left, a mirrored L at the bottom-right (four 2 px lines,
        /// returned so the caller can colour them). Never a fill (w33).</summary>
        public static Image[] Bracket(RectTransform parent, string name, Color c, float pad)
        {
            RectTransform holder = UiStyle33.Rect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Stretch(holder, pad);
            float arm = UnityTuning.Ui43BracketArm, w = UnityTuning.Ui43BracketW;
            var l = new Image[4];
            l[0] = UiStyle33.Line(holder, "TL h", c, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(arm, w));
            l[1] = UiStyle33.Line(holder, "TL v", c, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(w, arm));
            l[2] = UiStyle33.Line(holder, "BR h", c, new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, new Vector2(arm, w));
            l[3] = UiStyle33.Line(holder, "BR v", c, new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, new Vector2(w, arm));
            return l;
        }

        /// <summary>The scanline overlay over `parent` (U1.md 1.4): a RawImage tiling a 1 x pitch texture (one dark row), point-sampled.</summary>
        public static RawImage Scan(RectTransform parent, string name, float alpha)
        {
            if (scan == null)
            {
                int p = Math.Max(2, UnityTuning.Ui43ScanPitch);
                scan = new Texture2D(1, p, TextureFormat.RGBA32, false) { name = "UiKit43 scan", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point };
                var px = new Color32[p];
                for (int i = 0; i < p; i++) px[i] = i == 0 ? new Color32(0, 0, 0, 255) : new Color32(0, 0, 0, 0);
                scan.SetPixels32(px);
                scan.Apply(false, true);
            }
            RectTransform rt = UiStyle33.Rect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Stretch(rt);
            var ri = rt.gameObject.AddComponent<RawImage>();
            ri.texture = scan;
            ri.raycastTarget = false;
            ri.color = new Color(1, 1, 1, alpha);
            FitScan(ri);
            return ri;
        }

        /// <summary>Re-tile a scanline overlay to its rect's height (call after a resize).</summary>
        public static void FitScan(RawImage ri)
        {
            float h = Mathf.Max(1f, ri.rectTransform.rect.height);
            float p = Math.Max(2, UnityTuning.Ui43ScanPitch);
            ri.uvRect = new Rect(0, 0, 1, h / p);
        }

        /// <summary>A cut plate (full screens, the teach card): the theme's 45 deg cut sprite, 9-sliced, in `c`.</summary>
        public static Image Plate(Transform parent, string name, Color c, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            Image img = UiStyle33.Line(parent, name, c, anchor, pivot, pos, size);
            if (UiTheme.Cut != null)
            {
                img.sprite = UiTheme.Cut;
                img.type = Image.Type.Sliced;
            }
            return img;
        }

        /// <summary>Apply a pop / close scale to a rect.</summary>
        public static void Scale(RectTransform rt, float sx, float sy)
        {
            var s = new Vector3(sx, sy, 1f);
            if (rt.localScale != s) rt.localScale = s;
        }
    }
}
