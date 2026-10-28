// NIGHT17 U3-30.3 (the shared commit): Ui's shared building helpers, moved here unchanged from Ui.cs (the lead's file:
// the identity restyles every screen through these: Text, Button, Img, Screen, the face). Layer / Twin make the nested
// canvases; SetInt / SetFill / SetActive set a value only when it changed (hud.js write(key, value, apply)).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>NIGHT17 U3-30.3: the SETTINGS tabs shown now, in order (the feel reel captures each). Builder N keeps
        /// `tabButtons[i].id` (UiSettings.cs) or changes this line with it.</summary>
        public string[] ReelSettingsTabs()
        {
            var ids = new string[tabButtons.Count];
            for (int i = 0; i < ids.Length; i++) ids[i] = tabButtons[i].id;
            return ids;
        }

        RectTransform Layer(string name, bool interactive)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas));
            if (interactive) go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(root, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        /// <summary>An empty rect in another layer with the same anchors, pivot, position and size (so values sit on their frame).</summary>
        static RectTransform Twin(RectTransform of, Transform parent)
        {
            var rt = new GameObject(of.name + " (values)", typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = of.anchorMin;
            rt.anchorMax = of.anchorMax;
            rt.pivot = of.pivot;
            rt.anchoredPosition = of.anchoredPosition;
            rt.sizeDelta = of.sizeDelta;
            return rt;
        }

        GameObject Screen(string name, Color back)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(menus, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = back;
            img.raycastTarget = true;
            go.SetActive(false);
            return go;
        }

        static UnityEngine.UI.Image Img(Transform parent, string name, Color c, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.color = c;
            img.raycastTarget = false;
            return img;
        }

        static TextMeshProUGUI Text(Transform parent, string name, string s, float size, Color c, TextAlignmentOptions align, Vector2 pos, Vector2 box, Vector2 anchor, Face face = Face.Auto)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = box;
            var t = go.GetComponent<TextMeshProUGUI>();
            ApplyFace(t, size, face);
            t.text = s;
            t.fontSize = Mathf.Max(UnityTuning.ThemeTextFloor, size); // NIGHT17 rule 2: the floor from 16 to 18 px (UiTheme's token)
            t.color = c;
            t.alignment = align;
            t.raycastTarget = false;
            t.enableWordWrapping = false;
            return t;
        }

        /// <summary>
        /// NIGHT12 U3-25.6: the theme face for a label (theme.js): Barlow Semi Condensed under h2, Chakra Petch from h2, its
        /// 700 italic from h1 (the web's h1 / h2 / .big / banner are display italic 700). Nothing when the asset is missing.
        /// </summary>
        static void ApplyFace(TMP_Text t, float size, Face face)
        {
            // NIGHT17 U3-30.2: the identity's faces (UiTheme): DISPLAY from UnityTuning.ThemeDisplayFrom (heads and Heading
            // alike: the display face is a black italic already, so nothing synthetic is added), BODY below it.
            if (face == Face.Auto) face = size >= UnityTuning.ThemeDisplayFrom ? Face.Display : Face.Body;
            TMP_FontAsset f = face == Face.Body ? UiTheme.Body : UiTheme.Display;
            if (f == null) f = face == Face.Body ? fontBody : fontDisplay;
            if (f == null) return;
            t.font = f;
            if (face == Face.Heading && f == fontDisplay) t.fontStyle |= FontStyles.Bold | FontStyles.Italic;
        }

        TextMeshProUGUI Button(Transform parent, string label, Vector2 pos, Action onClick)
        {
            // NIGHT17 U3-30.2: a solid INK-SOFT plate with the theme's corner cuts and a 4 px cyan accent bar (no translucent grey).
            var img = Img(parent, label, UiTheme.InkSoft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(420, 54));
            img.sprite = UiTheme.Cut;
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.raycastTarget = true;
            Img(img.transform, "Edge", UiTheme.Cyan, new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(UnityTuning.ThemeAccentBar, 0)).rectTransform.pivot = new Vector2(0, 0.5f);
            var b = img.gameObject.AddComponent<UnityEngine.UI.Button>();
            var colors = b.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.75f, 1, 1, 1);
            colors.pressedColor = new Color(0.6f, 0.9f, 0.95f, 1);
            b.colors = colors;
            b.onClick.AddListener(() => onClick());
            img.gameObject.AddComponent<UiPress>(); // NIGHT16 polish #8: the press dips the button (visual only)
            // NIGHT17 U3-30.2: the label in the display face at 32 px, white with the hard ink shadow, no tracking.
            TextMeshProUGUI t = Text(img.transform, "Label", label, UiTheme.Px(UiTheme.Size.S32), UiTheme.White, TextAlignmentOptions.Center, Vector2.zero, new Vector2(420, 54), new Vector2(0.5f, 0.5f), Face.Display);
            UiTheme.Style(t, UiTheme.Px(UiTheme.Size.S32), UiTheme.White, UiTheme.Display, true);
            return t;
        }

        // --- set only on change (hud.js write(key, value, apply)) ---------------------------------------------------------

        static void SetInt(TMP_Text t, ref int shown, int v, string format = "{0:0}")
        {
            if (v == shown) return;
            shown = v;
            t.SetText(format, v);
        }

        static void SetFill(UnityEngine.UI.Image fill, ref float shown, float v)
        {
            if (v == shown) return;
            shown = v;
            fill.rectTransform.anchorMax = new Vector2(v, 1);
        }

        static void SetActive(GameObject go, ref bool shown, bool v)
        {
            if (v == shown) return;
            shown = v;
            go.SetActive(v);
        }
    }
}
