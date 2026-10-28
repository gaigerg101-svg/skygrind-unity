// NIGHT33 builder U (design.n33.hud33): the HUD33 visual language as one small reusable kit, so the shop (builder P) and the front screens draw in
// the same hand (U1 spec section 6, DECIDE33 D-1): no panel fills; text on the world with a hard 2 px shadow (a twin text behind it, offset exactly
// Hud33ShadowPx at every size, which TMP's underlay cannot do: its offset scales with the font); one ornament, the 20-degree slash tick (an Image
// skewed by UiSkew); bone white for neutral values, cyan only for what the player can use now, red-orange only for danger (CLAUDE.md rule 8;
// magenta stays RIVAL's and is not here). Faces: the theme's converted fonts (UiTheme: Archivo ExtraCondensed Black Italic for numbers, Archivo
// ExtraBold for labels; A3 section 1), TMP's default when missing. Every number is in UnityTuning.Hud33.cs.
// Unity objects are made on first use only (L-035): nothing here runs in a field initialiser.
//
// Use (any screen):
//   UiStyle33.Text33 t = UiStyle33.Label(parent, "Name", UnityTuning.Hud33Name, UiStyle33.Bone, UiStyle33.FaceLabel, TextAlignmentOptions.Right,
//                                        new Vector2(1, 0), new Vector2(1, 0), new Vector2(-72, 54), new Vector2(320, 30));
//   t.Set("KESTREL"); t.Color = UiStyle33.Cyan;
//   UnityEngine.UI.Image tick = UiStyle33.Slash(parent, "Tick", UiStyle33.Bone, anchor, pivot, pos, new Vector2(6, 14));
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public static class UiStyle33
    {
        static bool ready;
        static Color bone, cyan, cyanDeep, danger, shadow;
        static Sprite ring;

        /// <summary>Neutral values and labels (U1: #E6ECEF).</summary>
        public static Color Bone { get { Init(); return bone; } }
        /// <summary>Usable / ready (colors.usable through UiTheme).</summary>
        public static Color Cyan { get { Init(); return cyan; } }
        /// <summary>A usable thing still coming back (a spent charge refilling).</summary>
        public static Color CyanDeep { get { Init(); return cyanDeep; } }
        /// <summary>Danger and enemies only (colors.enemy through UiTheme).</summary>
        public static Color Danger { get { Init(); return danger; } }
        /// <summary>The 2 px shadow (#0A0E14 at 80 %).</summary>
        public static Color Shadow { get { Init(); return shadow; } }

        /// <summary>Numbers (health, points, rank): the theme's display face.</summary>
        public static TMP_FontAsset FaceNumber => UiTheme.Display;
        /// <summary>Caps labels (the weapon, the LT action, the message slot): the theme's bold body face.</summary>
        public static TMP_FontAsset FaceLabel => UiTheme.BodyBold;
        /// <summary>The rank letter: the theme's rank face.</summary>
        public static TMP_FontAsset FaceRank => UiTheme.Rank;

        /// <summary>tan of the slash's skew.</summary>
        public static float SlashTan => Mathf.Tan(UnityTuning.Hud33SlashDeg * Mathf.Deg2Rad);

        static void Init()
        {
            if (ready) return;
            bone = Look.Hex(UnityTuning.Hud33BoneHex);
            bone.a = 1f;
            cyan = UiTheme.Ready ? UiTheme.Cyan : new Color(0.21f, 0.88f, 0.95f, 1f); // fallback only before UiTheme.Init (U1's #36E0F2)
            cyanDeep = UiTheme.Ready ? UiTheme.CyanDeep : Color.Lerp(cyan, Color.black, 0.55f);
            danger = UiTheme.Ready ? UiTheme.Danger : new Color(1f, 0.35f, 0.17f, 1f); // fallback only before UiTheme.Init (U1's #FF5A2B)
            shadow = Look.Hex(UnityTuning.Hud33ShadowHex);
            shadow.a = UnityTuning.Hud33ShadowAlpha;
            ready = UiTheme.Ready;
        }

        /// <summary>A text with its hard shadow: `rt` is the holder (move / scale / hide it), `main` the text, `back` its shadow twin.</summary>
        public sealed class Text33
        {
            public RectTransform rt;
            public TextMeshProUGUI main, back;
            string shown;
            Color color;
            float alpha = 1f;

            /// <summary>Set the string (only when it changed); an empty string hides the holder.</summary>
            public void Set(string s)
            {
                s ??= string.Empty;
                if (s == shown) return;
                shown = s;
                main.SetText(s);
                back.SetText(s);
                bool on = s.Length > 0;
                if (rt.gameObject.activeSelf != on) rt.gameObject.SetActive(on);
            }

            public string Shown => shown ?? string.Empty;

            public Color Color
            {
                get => color;
                set
                {
                    if (value == color) return;
                    color = value;
                    Apply();
                }
            }

            /// <summary>The whole label's opacity (text and shadow together).</summary>
            public float Alpha
            {
                get => alpha;
                set
                {
                    if (value == alpha) return;
                    alpha = value;
                    Apply();
                }
            }

            void Apply()
            {
                main.color = new Color(color.r, color.g, color.b, color.a * alpha);
                Color sh = Shadow;
                back.color = new Color(sh.r, sh.g, sh.b, sh.a * alpha);
            }

            public float FontPx => main.fontSize;
        }

        /// <summary>An empty rect (no graphic).</summary>
        public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>A full-screen rect with its own nested canvas (so its changes rebuild only its batch).</summary>
        public static RectTransform Layer(Transform parent, string name)
        {
            var rt = new GameObject(name, typeof(RectTransform), typeof(Canvas)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        static TextMeshProUGUI Tmp(Transform parent, string name, float px, TMP_FontAsset face, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var t = go.GetComponent<TextMeshProUGUI>();
            if (face != null) t.font = face;
            t.fontSize = Mathf.Max(UnityTuning.Hud33TextMin, px);
            t.enableAutoSizing = false;
            t.alignment = align;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false; // U1 rule 10
            t.richText = false;
            t.fontStyle = FontStyles.Normal;
            return t;
        }

        /// <summary>A label with its 2 px shadow, at `pos` from `anchor`, its box `size` (the box is the checked rect: keep text inside it).</summary>
        public static Text33 Label(Transform parent, string name, float px, Color c, TMP_FontAsset face, TextAlignmentOptions align, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, bool caps = true)
        {
            var l = new Text33 { rt = Rect(parent, name, anchor, pivot, pos, size) };
            l.back = Tmp(l.rt, "Shadow", px, face, align);
            l.back.rectTransform.anchoredPosition = new Vector2(UnityTuning.Hud33ShadowPx, -UnityTuning.Hud33ShadowPx);
            l.main = Tmp(l.rt, "Text", px, face, align);
            if (caps)
            {
                l.main.characterSpacing = l.back.characterSpacing = UnityTuning.Hud33Tracking;
                l.main.fontStyle = l.back.fontStyle = FontStyles.UpperCase;
            }
            l.Color = c;
            l.main.color = c;
            Color sh = Shadow;
            l.back.color = sh;
            l.rt.gameObject.SetActive(false); // shown by its first non-empty Set
            return l;
        }

        /// <summary>A plain bar (the boss bar, a 3 px meter line): an Image with no sprite.</summary>
        public static Image Line(Transform parent, string name, Color c, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            RectTransform rt = Rect(parent, name, anchor, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = c;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>The motif: a tick skewed 20 degrees (its top leans right), as a plain Image so it can fill.</summary>
        public static Image Slash(Transform parent, string name, Color c, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            Image img = Line(parent, name, c, anchor, pivot, pos, size);
            UiSkew.Apply(img, SlashTan);
            return img;
        }

        /// <summary>A slash tick that fills from the bottom (a charge coming back): a track and a vertically filled Image over it.</summary>
        public static Image SlashFill(Transform parent, string name, Color c, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, out Image track, Color trackColor)
        {
            track = Slash(parent, name + " track", trackColor, anchor, pivot, pos, size);
            Image fill = Slash(parent, name, c, anchor, pivot, pos, size);
            fill.sprite = WhiteSprite();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Vertical;
            fill.fillOrigin = (int)Image.OriginVertical.Bottom;
            return fill;
        }

        /// <summary>A horizontally filled slash bar (the stamina): a track and a fill from the left.</summary>
        public static Image SlashBar(Transform parent, string name, Color c, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, out Image track, Color trackColor)
        {
            track = Slash(parent, name + " track", trackColor, anchor, pivot, pos, size);
            Image fill = Slash(parent, name, c, anchor, pivot, pos, size);
            fill.sprite = WhiteSprite();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            return fill;
        }

        /// <summary>A ring arc (only the reticle's charge arcs use it: U1 rule 6), radially filled from `originTop` (true: top, clockwise down the
        /// right; false: bottom, clockwise up the left), so two of them at fill f each close a circle at f = 0.5.</summary>
        public static Image Arc(Transform parent, string name, Color c, float radius, float width, bool originTop)
        {
            RectTransform rt = Rect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = RingSprite(radius, width);
            rt.sizeDelta = img.sprite.rect.size;
            img.color = c;
            img.raycastTarget = false;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillOrigin = originTop ? (int)Image.Origin360.Top : (int)Image.Origin360.Bottom;
            img.fillClockwise = true;
            img.fillAmount = 0f;
            return img;
        }

        static Sprite scrim;

        /// <summary>A soft dark wash behind a line of text on a bright world (the message slot under a lit ceiling panel): no edges, no box, its alpha
        /// falling to 0 at every side (a smooth ellipse-like gradient), so it reads as shade, not a panel. First sibling of `parent`, centred on it.</summary>
        public static Image Scrim(Transform parent, string name, Vector2 size)
        {
            RectTransform rt = Rect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            rt.SetAsFirstSibling();
            var img = rt.gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            if (scrim == null)
            {
                const int w = 128, h = 32;
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = "UiStyle33 scrim", wrapMode = TextureWrapMode.Clamp };
                var px = new Color32[w * h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        float u = Mathf.Abs((x + 0.5f) / w * 2 - 1), v = Mathf.Abs((y + 0.5f) / h * 2 - 1);
                        float a = (1 - Mathf.SmoothStep(0.55f, 1f, u)) * (1 - Mathf.SmoothStep(0.45f, 1f, v)); // a flat core over the text's height, then the fall-off
                        px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255));
                    }
                tex.SetPixels32(px);
                tex.Apply(false, true);
                scrim = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            }
            img.sprite = scrim;
            Color sh = Shadow;
            img.color = new Color(sh.r, sh.g, sh.b, 0f);
            return img;
        }

        static Sprite white;

        /// <summary>A 4 px white sprite (a Filled Image needs one).</summary>
        public static Sprite WhiteSprite()
        {
            if (white != null) return white;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { name = "UiStyle33 white", wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[16];
            for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px);
            tex.Apply(false, true);
            white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
            return white;
        }

        static Sprite RingSprite(float radius, float width)
        {
            if (ring != null) return ring;
            int n = Mathf.CeilToInt((radius + width + 2) * 2);
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "UiStyle33 ring", wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[n * n];
            float c = n * 0.5f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - c) * (x + 0.5f - c) + (y + 0.5f - c) * (y + 0.5f - c));
                    float a = Mathf.Clamp01(0.5f - (Mathf.Abs(d - radius) - width * 0.5f));
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            ring = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f));
            return ring;
        }
    }
}
