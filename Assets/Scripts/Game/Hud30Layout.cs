// NIGHT30 builder V (P0 e; design.n30.hud30): the HUD LAYOUT CHECK. "No text over icons, no chip over a plate": GG's frames showed "PULSE READY"
// drawn over the weapon icons and "TRIGGER STALE" (170047), and the "x1.2 AIR" chip over the rank plate (170227). NIGHT25's HudOverlap25 checks only
// the campaign's own texts in a recording; this check reads the LIVE HUD of one frame (every overlay canvas: the HUD, the meta layer, Hud29's canvas)
// and reports every pair that collides:
//   - TEXT x TEXT: two visible texts whose drawn glyph boxes (TMP textBounds, not the layout box) share more than Hud30LayoutMinArea px squared;
//   - TEXT x ICON: a text over a weapon icon (a graphic named "Silhouette" or "Icon");
//   - CHIP x PLATE: a chip (a rect named "... chip", its plate) over another widget's plate (a graphic named "Plate" / "... plate").
// A parent / child pair and a label with its "(offset)" shadow twin are one element, not a pair. Visible: active in the hierarchy, enabled, scale not
// zero, alpha (its colour x every CanvasGroup above it) at least Hud30LayoutMinAlpha, and a non-empty text for a text.
// The pure part (Item, Overlaps) is engine-free (EditMode tests drive it); Collect reads the scene. The reel `layout30` calls Report on each HUD state.
using System;
using System.Collections.Generic;
using System.Text;

namespace Skygrind.Game
{
    public static class Hud30Layout
    {
        public enum Kind { Text, Icon, Plate, Chip }

        public struct Item
        {
            public string path, text;
            public Kind kind;
            public double x, y, w, h, alpha;
        }

        /// <summary>The shared area of two items' rects (px squared).</summary>
        public static double Area(Item a, Item b)
        {
            double w = Math.Min(a.x + a.w, b.x + b.w) - Math.Max(a.x, b.x), h = Math.Min(a.y + a.h, b.y + b.h) - Math.Max(a.y, b.y);
            return w > 0 && h > 0 ? w * h : 0;
        }

        /// <summary>One element: a parent and its child, or a label and its "(offset)" twin.</summary>
        public static bool Related(Item a, Item b)
        {
            string pa = a.path.Replace(" (offset)", ""), pb = b.path.Replace(" (offset)", "");
            return pa == pb || pa.StartsWith(pb + "/", StringComparison.Ordinal) || pb.StartsWith(pa + "/", StringComparison.Ordinal);
        }

        /// <summary>Does the pair (a, b) count as a collision kind? text x text, text x icon, chip x plate (either order).</summary>
        public static bool Counts(Kind a, Kind b)
        {
            if (a == Kind.Text && b == Kind.Text) return true;
            if ((a == Kind.Text && b == Kind.Icon) || (a == Kind.Icon && b == Kind.Text)) return true;
            return (a == Kind.Chip && b == Kind.Plate) || (a == Kind.Plate && b == Kind.Chip);
        }

        /// <summary>A chip's own plate (a "Plate" under the chip's rect) is part of the chip: never a CHIP x PLATE pair.</summary>
        static bool ChipOwnsPlate(Item a, Item b)
        {
            Item chip = a.kind == Kind.Chip ? a : b, plate = a.kind == Kind.Chip ? b : a;
            return chip.kind == Kind.Chip && plate.kind == Kind.Plate && plate.path.StartsWith(chip.path + "/", StringComparison.Ordinal);
        }

        /// <summary>Every colliding pair of one frame's items: "kindA pathA [text] x kindB pathB [text] (area px2)".</summary>
        public static List<string> Overlaps(List<Item> items, double minArea = UnityTuning.Hud30LayoutMinArea, double minAlpha = UnityTuning.Hud30LayoutMinAlpha)
        {
            var found = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                Item a = items[i];
                if (a.alpha < minAlpha || (a.kind == Kind.Text && string.IsNullOrEmpty(a.text))) continue;
                for (int j = i + 1; j < items.Count; j++)
                {
                    Item b = items[j];
                    if (b.alpha < minAlpha || (b.kind == Kind.Text && string.IsNullOrEmpty(b.text))) continue;
                    if (!Counts(a.kind, b.kind) || Related(a, b) || ChipOwnsPlate(a, b)) continue;
                    double area = Area(a, b);
                    if (area > minArea) found.Add($"{a.kind} {a.path}{Quote(a)} x {b.kind} {b.path}{Quote(b)} ({Math.Round(area)} px2)");
                }
            }
            return found;
        }

        static string Quote(Item it) => string.IsNullOrEmpty(it.text) ? "" : " [" + it.text + "]";

        /// <summary>The kind a graphic's name gives it (null: not checked). Texts are decided by their component, not here.</summary>
        public static Kind? KindOfName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (name.IndexOf("Silhouette", StringComparison.Ordinal) >= 0 || name.IndexOf("Icon", StringComparison.Ordinal) >= 0) return Kind.Icon;
            if (name.EndsWith(" chip", StringComparison.OrdinalIgnoreCase) || name.IndexOf(" chip ", StringComparison.OrdinalIgnoreCase) >= 0) return Kind.Chip;
            if (name == "Plate" || name.EndsWith(" plate", StringComparison.OrdinalIgnoreCase)) return Kind.Plate;
            return null;
        }

        /// <summary>The live HUD's items of this frame (every root overlay / camera canvas and its graphics).</summary>
        public static List<Item> Collect()
        {
            var items = new List<Item>();
            var corners = new UnityEngine.Vector3[4];
            foreach (UnityEngine.Canvas c in UnityEngine.Object.FindObjectsByType<UnityEngine.Canvas>(UnityEngine.FindObjectsSortMode.None))
            {
                if (c == null || !c.isRootCanvas || !c.isActiveAndEnabled) continue;
                UnityEngine.Camera cam = c.renderMode == UnityEngine.RenderMode.ScreenSpaceOverlay ? null : c.worldCamera;
                if (c.renderMode == UnityEngine.RenderMode.WorldSpace) continue;
                foreach (UnityEngine.UI.Graphic g in c.GetComponentsInChildren<UnityEngine.UI.Graphic>(false))
                {
                    if (g == null || !g.isActiveAndEnabled || g.canvas == null || !g.canvas.enabled) continue;
                    UnityEngine.RectTransform rt = g.rectTransform;
                    if (Math.Abs(rt.lossyScale.x) < 1e-4f || Math.Abs(rt.lossyScale.y) < 1e-4f) continue;
                    Kind kind;
                    string text = null;
                    bool chipByParent = false;
                    var tmp = g as TMPro.TMP_Text;
                    if (tmp != null)
                    {
                        kind = Kind.Text;
                        text = tmp.text;
                        if (string.IsNullOrEmpty(text)) continue;
                    }
                    else
                    {
                        // a chip is its rect (a plain RectTransform holding the plate): its plate stands for it
                        Kind? byName = KindOfName(g.gameObject.name);
                        UnityEngine.Transform parent = rt.parent;
                        if (byName == Kind.Plate && parent != null && KindOfName(parent.name) == Kind.Chip) { byName = Kind.Chip; chipByParent = true; }
                        if (byName == null) continue;
                        kind = byName.Value;
                    }
                    double alpha = g.color.a * GroupAlpha(rt);
                    if (tmp != null) alpha = tmp.alpha * GroupAlpha(rt);
                    if (alpha < UnityTuning.Hud30LayoutMinAlpha) continue;
                    UnityEngine.Rect r = ScreenRect(g, tmp, cam, corners);
                    if (r.width <= 0 || r.height <= 0) continue;
                    string path = Path(g.transform, c.transform);
                    if (chipByParent) path = Path(g.transform.parent, c.transform); // the chip's rect, so its own texts are its children
                    items.Add(new Item { path = path, text = text, kind = kind, x = r.x, y = r.y, w = r.width, h = r.height, alpha = alpha });
                }
            }
            return items;
        }

        /// <summary>One frame's check: "HUD layout: 0 collisions (n items)" or the count and the first `keep` pairs; ok is false when any pair collides.</summary>
        public static string Report(out bool ok, out int count, int keep = 6)
        {
            List<Item> items = Collect();
            List<string> pairs = Overlaps(items);
            count = pairs.Count;
            ok = count == 0;
            if (ok) return $"HUD layout: 0 collisions ({items.Count} items)";
            var sb = new StringBuilder();
            sb.Append("HUD layout: ").Append(count).Append(" collision(s) (").Append(items.Count).Append(" items): ");
            for (int i = 0; i < pairs.Count && i < keep; i++) sb.Append(i > 0 ? " ;; " : "").Append(pairs[i]);
            return sb.ToString();
        }

        static double GroupAlpha(UnityEngine.Transform t)
        {
            double a = 1;
            for (UnityEngine.Transform p = t; p != null; p = p.parent)
            {
                var g = p.GetComponent<UnityEngine.CanvasGroup>();
                if (g == null || !g.enabled) continue;
                a *= g.alpha;
                if (g.ignoreParentGroups) break;
            }
            return a;
        }

        /// <summary>A graphic's rect in screen px: a text's drawn glyph box (textBounds), any other graphic's rect.</summary>
        static UnityEngine.Rect ScreenRect(UnityEngine.UI.Graphic g, TMPro.TMP_Text tmp, UnityEngine.Camera cam, UnityEngine.Vector3[] corners)
        {
            UnityEngine.RectTransform rt = g.rectTransform;
            if (tmp != null)
            {
                UnityEngine.Bounds b = tmp.textBounds;
                if (b.size.x <= 0 || b.size.y <= 0) return default;
                corners[0] = rt.TransformPoint(new UnityEngine.Vector3(b.min.x, b.min.y, 0));
                corners[1] = rt.TransformPoint(new UnityEngine.Vector3(b.min.x, b.max.y, 0));
                corners[2] = rt.TransformPoint(new UnityEngine.Vector3(b.max.x, b.max.y, 0));
                corners[3] = rt.TransformPoint(new UnityEngine.Vector3(b.max.x, b.min.y, 0));
            }
            else rt.GetWorldCorners(corners);
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                UnityEngine.Vector2 s = UnityEngine.RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
                x0 = Math.Min(x0, s.x); y0 = Math.Min(y0, s.y); x1 = Math.Max(x1, s.x); y1 = Math.Max(y1, s.y);
            }
            return UnityEngine.Rect.MinMaxRect(x0, y0, x1, y1);
        }

        static string Path(UnityEngine.Transform t, UnityEngine.Transform root)
        {
            var sb = new StringBuilder();
            for (UnityEngine.Transform p = t; p != null; p = p.parent)
            {
                sb.Insert(0, p.name);
                if (p == root) break;
                sb.Insert(0, "/");
            }
            return sb.ToString();
        }
    }
}
