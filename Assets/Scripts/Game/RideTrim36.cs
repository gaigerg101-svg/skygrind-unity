// NIGHT36 builder M (NIGHT36.md M1, REVIEW.md shot 155909: "The ride wall is the same dark purple as every other wall: nothing marks it as rideable"):
// THE RIDE TRIM. Every wall-ride panel of a campaign level gets a cyan trim (cyan = player-usable, CLAUDE.md rule 8): on each long face a stripe
// under its top edge and a stripe at eye height (the ride's own band), and a lit cap down each end ("ride from here to here"). NIGHT26's strips go
// on every long "wall" box (room walls too), so they never told a ride panel from a room wall; this marks only the panels.
// A ride panel is a box of kind "wall" in the level's campaign box list (json "campaign"."boxes", where the generators keep each box's reason) whose
// `ride` is true (builder L's n36 tag; a level with any such tag is read by the tag alone), or, on a level with no tag, whose note / why names a ride
// panel or a ride wall (NIGHT35's generators: "ride panel A"). View only: nothing here touches the sim. Shown with design.n36.wallgrip36 on (the card's view half) and not
// -skygrindOff36. Numbers: UnityTuning.Move36.cs (RideTrim36*).
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class RideTrim36
    {
        static readonly Regex RideNote = new Regex(@"ride (panel|wall)", RegexOptions.IgnoreCase);

        /// <summary>For the reels: the panels trimmed on the last build.</summary>
        public static int Panels;

        /// <summary>Is this campaign box a wall-ride panel?</summary>
        public static bool IsRidePanel(Dictionary<string, object> b, bool tagged)
        {
            if (b == null || !b.TryGetValue("kind", out object k) || !(k is string kind) || kind != "wall") return false;
            if (b.TryGetValue("ride", out object r) && r is bool rb && rb) return true;
            if (tagged) return false; // builder L (n36 levels): `ride: true` ONLY (its job "line" is on decks and hull walls too)
            foreach (string key in new[] { "note", "why" })
                if (b.TryGetValue(key, out object n) && n is string s && RideNote.IsMatch(s)) return true;
            return false;
        }

        /// <summary>The trim for the level's ride panels under `parent`; returns the panels trimmed (0 with the row off or none marked).</summary>
        public static int Build(SurvivalGame g, Transform parent, SurvivalMaterials mats)
        {
            Panels = 0;
            GameConfig cfg = g.cfg;
            if (!cfg.Design("n36.wallgrip36.enabled") || N36Blocks.ForceOff || !N36Fixes.On || g.map == null || g.map.json == null) return 0;
            if (!g.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp) || !camp.TryGetValue("boxes", out object bo) || !(bo is List<object> boxes)) return 0;
            var m = new Material(mats.neon);
            m.SetColor("_BaseColor", Look.C(cfg, "colors.usable") * UnityTuning.RideTrim36Brightness);
            var root = new GameObject("Ride trim 36");
            root.transform.SetParent(parent, false);
            float th = UnityTuning.RideTrim36Height, off = UnityTuning.RideTrim36Offset, cap = UnityTuning.RideTrim36EndW;
            bool tagged = false; // a level that tags any box `ride: true` (n36) is read by the tag alone; older levels by their notes
            foreach (object o in boxes)
                if (o is Dictionary<string, object> d && d.TryGetValue("ride", out object rv) && rv is bool rt && rt) { tagged = true; break; }
            foreach (object o in boxes)
            {
                var b = o as Dictionary<string, object>;
                if (!IsRidePanel(b, tagged) || !b.ContainsKey("min") || !b.ContainsKey("max")) continue;
                List<object> mn = PortData.Arr(b["min"]), mx = PortData.Arr(b["max"]);
                float x0 = (float)PortData.Num(mn[0]), y0 = (float)PortData.Num(mn[1]), z0 = (float)PortData.Num(mn[2]);
                float x1 = (float)PortData.Num(mx[0]), y1 = (float)PortData.Num(mx[1]), z1 = (float)PortData.Num(mx[2]);
                float dx = x1 - x0, dz = z1 - z0;
                if (dx <= 0 || dz <= 0 || y1 <= y0) continue;
                bool alongX = dx >= dz;
                float len = alongX ? dx : dz;
                float base0 = Mathf.Max(y0, 0f); // a panel sunk into the floor (or a lava pit): its band from the floor up
                float top = y1 - UnityTuning.RideTrim36TopDrop, low = Mathf.Min(base0 + UnityTuning.RideTrim36LowY, top - th * 2);
                float midY = (base0 + y1) / 2, hgt = y1 - base0;
                for (int side = 0; side < 2; side++)
                {
                    if (alongX)
                    {
                        float z = side == 0 ? z0 - off : z1 + off;
                        Piece(root.transform, m, (x0 + x1) / 2, top, z, len, th, th);
                        Piece(root.transform, m, (x0 + x1) / 2, low, z, len, th, th);
                        Piece(root.transform, m, x0 + cap / 2, midY, z, cap, hgt, th);
                        Piece(root.transform, m, x1 - cap / 2, midY, z, cap, hgt, th);
                    }
                    else
                    {
                        float x = side == 0 ? x0 - off : x1 + off;
                        Piece(root.transform, m, x, top, (z0 + z1) / 2, th, th, len);
                        Piece(root.transform, m, x, low, (z0 + z1) / 2, th, th, len);
                        Piece(root.transform, m, x, midY, z0 + cap / 2, th, hgt, cap);
                        Piece(root.transform, m, x, midY, z1 - cap / 2, th, hgt, cap);
                    }
                }
                Panels++;
            }
            return Panels;
        }

        static void Piece(Transform parent, Material m, float cx, float cy, float cz, float sx, float sy, float sz)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Cube);
            s.name = "Ride trim";
            Object.Destroy(s.GetComponent<Collider>());
            s.transform.SetParent(parent, false);
            s.transform.position = new Vector3(cx, cy, cz); // the map builder uses the web coordinates as they are
            s.transform.localScale = new Vector3(sx, sy, sz);
            s.GetComponent<MeshRenderer>().sharedMaterial = m;
        }
    }
}
