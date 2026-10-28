// NIGHT25 (builder F): "no two HUD text rects overlap at any frame". Reads a recording's telemetry.jsonl (the probe's ui[] per frame: path, rect [x, y, w, h] in screen px,
// alpha, text) and lists every frame where two visible TEXT elements (a non-empty text, alpha at least MinAlpha) share more than MinOverlapPx2 of area. A parent / child
// pair (one path is the other's prefix) and two copies of one label (the italic display faces' "(offset)" shadow, "Clear (offset)") are not overlaps. Pure C#, no UnityEngine:
// the reels `cue` and `lit` call Check after Stop(); EditMode tests drive Frame / Overlaps directly. The found case: "DOOR OPEN / 36 m" on top of the room-title banner.
using System;
using System.Collections.Generic;
using System.IO;

namespace Skygrind.Game
{
    public static class HudOverlap25
    {
        public const double MinAlpha = 0.3, MinOverlapPx2 = 24;

        /// <summary>The campaign's own HUD elements (a pair counts only when one of its two is one of these: the old HUD's own pairs, such as the banner over the round mark,
        /// are not this check's business): the door cue, the lesson line, the room banner, the boss bar, the campaign results.</summary>
        public static readonly string[] Own = { "UI/HUD values/Door cue 24", "UI/Meta overlay/Lesson25", "UI/HUD values/Room banner 24", "UI/HUD values/Threat bar 20", "UI/Menus/Results/Campaign results" };

        public static bool IsOwn(string path)
        {
            foreach (string o in Own) if (path.StartsWith(o, StringComparison.Ordinal)) return true;
            return false;
        }

        public struct Item
        {
            public string path, text;
            public double x, y, w, h, alpha;
        }

        /// <summary>The overlap area of two items (px squared).</summary>
        public static double Area(Item a, Item b)
        {
            double w = Math.Min(a.x + a.w, b.x + b.w) - Math.Max(a.x, b.x), h = Math.Min(a.y + a.h, b.y + b.h) - Math.Max(a.y, b.y);
            return w > 0 && h > 0 ? w * h : 0;
        }

        static bool Related(Item a, Item b)
        {
            if (a.path.StartsWith(b.path + "/", StringComparison.Ordinal) || b.path.StartsWith(a.path + "/", StringComparison.Ordinal)) return true;
            string pa = a.path.Replace(" (offset)", ""), pb = b.path.Replace(" (offset)", "");
            return pa == pb; // a label and its offset copy
        }

        /// <summary>The overlapping pairs among one frame's items ("pathA [text] x pathB [text]").</summary>
        public static List<string> Overlaps(List<Item> items)
        {
            var found = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].text) || items[i].alpha < MinAlpha) continue;
                for (int j = i + 1; j < items.Count; j++)
                {
                    if (string.IsNullOrEmpty(items[j].text) || items[j].alpha < MinAlpha || Related(items[i], items[j])) continue;
                    if (!IsOwn(items[i].path) && !IsOwn(items[j].path)) continue;
                    if (Area(items[i], items[j]) > MinOverlapPx2) found.Add($"{items[i].path} [{items[i].text}] x {items[j].path} [{items[j].text}]");
                }
            }
            return found;
        }

        /// <summary>Check a telemetry file: the number of frames with an overlap, and the first few (frame: pair) lines.</summary>
        public static int Check(string telemetryPath, List<string> first, int keep = 4)
        {
            int bad = 0;
            if (!File.Exists(telemetryPath)) return -1;
            var seen = new HashSet<string>();
            foreach (string line in File.ReadLines(telemetryPath))
            {
                if (line.IndexOf("\"type\":\"frame\"", StringComparison.Ordinal) < 0) continue;
                if (!(Skygrind.Core.JsJson.Parse(line) is Dictionary<string, object> f) || !f.TryGetValue("ui", out object uo) || !(uo is List<object> ui)) continue;
                var items = new List<Item>(ui.Count);
                foreach (object o in ui)
                {
                    if (!(o is Dictionary<string, object> d) || !d.TryGetValue("text", out object t) || !(t is string text) || text.Length == 0) continue;
                    if (!(d["rect"] is List<object> r) || r.Count < 4) continue;
                    items.Add(new Item
                    {
                        path = (string)d["path"], text = text, alpha = Convert.ToDouble(d["alpha"], System.Globalization.CultureInfo.InvariantCulture),
                        x = Convert.ToDouble(r[0], System.Globalization.CultureInfo.InvariantCulture), y = Convert.ToDouble(r[1], System.Globalization.CultureInfo.InvariantCulture),
                        w = Convert.ToDouble(r[2], System.Globalization.CultureInfo.InvariantCulture), h = Convert.ToDouble(r[3], System.Globalization.CultureInfo.InvariantCulture),
                    });
                }
                List<string> pairs = Overlaps(items);
                if (pairs.Count == 0) continue;
                bad++;
                foreach (string p in pairs) if (seen.Add(p) && first.Count < keep) first.Add($"f{f["f"]}: {p}");
            }
            return bad;
        }

        /// <summary>The result line's part: "HUD text overlaps: none" or the count and the first pairs (a RED reason when any).</summary>
        public static string Report(string dir, out bool ok)
        {
            var first = new List<string>();
            int bad = Check(Path.Combine(dir, "telemetry.jsonl"), first);
            ok = bad == 0;
            return bad < 0 ? "HUD text overlaps: not checked (no telemetry)" : bad == 0 ? "HUD text overlaps: none" : $"HUD text overlaps in {bad} frame(s): {string.Join(" ;; ", first)}";
        }
    }
}
