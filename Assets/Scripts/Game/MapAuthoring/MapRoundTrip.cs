// NIGHT15 U3-28.4 (builder P): the round trip's comparison (import a map JSON, export it, compare). The sim-relevant keys
// are every key the game reads (SurvivalMap.FromExport, PortData.BuildMapOctree, NavGrid.FromExport, NavMap.FromExport,
// MysteryBox / Power / Exfil's spots, the bench's core.presets, GoldenBot's navGrid.corePreset) plus the whole level data
// and the nav grid's other arrays; each must print the same text (MapJsonWriter: values, key order and the -0 / undefined
// encodings). The diagnostics are the keys the export drops or rewrites (MapModelJson's header); with them stripped from
// both sides, the two files must print the same text whole. Pure (no UnityEngine).
using System.Collections.Generic;
using System.Text;

namespace Skygrind.Game.MapAuthoring
{
    public struct MapKeyCompare
    {
        public string path;
        public bool inSource, inExport, same;
        public int sourceChars, exportChars;
    }

    public static class MapRoundTrip
    {
        /// <summary>Every key the game reads, and the level data whole ("data"), as dotted paths.</summary>
        public static readonly string[] SimKeys =
        {
            "name", "floorSize", "killPlaneY", "collision.boxes", "collision.barriers", "collision.wedges", "core.spec", "core.presets",
            "spawns.playerSpawn", "spawns.duelSpawns", "spawns.ground", "spawns.high", "pads", "grappleNodes",
            "zoneParts.zoneIds", "zoneParts.startZone", "zoneParts.parts",
            "navGrid.cell", "navGrid.size", "navGrid.origin", "navGrid.count", "navGrid.linkKinds", "navGrid.linkCounts",
            "navGrid.nodes.x", "navGrid.nodes.z", "navGrid.nodes.y", "navGrid.nodes.clear", "navGrid.nodes.tag", "navGrid.nodes.edge",
            "navGrid.nodes.col", "navGrid.nodes.blocked", "navGrid.links.linkStart", "navGrid.links.linkTo", "navGrid.links.linkKind",
            "navGrid.links.linkCost", "navGrid.corePreset", "data", "spots",
        };

        /// <summary>The diagnostics: dropped by the export (notes, Octree dumps, the Spire's Duel graph) or its own (generatedBy, source).</summary>
        public static readonly string[] DiagnosticKeys =
        {
            "generatedBy", "source", "collision.note", "collision.wedgeNote", "collision.triangles", "collision.droppedByOctree",
            "core.note", "core.randomNote", "padNote", "duelNavGraph", "dataNote",
        };

        const string Missing = "\u0000missing";

        /// <summary>The value at a dotted path, or a marker when a key on the way is missing (a null on the way reads as null).</summary>
        public static object At(Dictionary<string, object> json, string path, out bool found)
        {
            object cur = json;
            found = true;
            foreach (string key in path.Split('.'))
            {
                if (cur == null) return null; // e.g. zoneParts null: its keys read as null on both sides
                if (!(cur is Dictionary<string, object> d) || !d.TryGetValue(key, out cur))
                {
                    found = false;
                    return Missing;
                }
            }
            return cur;
        }

        /// <summary>True when every key on the dotted path exists (a null on the way ends it: training's "core": null has no core.note).</summary>
        public static bool Has(Dictionary<string, object> json, string path)
        {
            object cur = json;
            foreach (string key in path.Split('.'))
                if (!(cur is Dictionary<string, object> d) || !d.TryGetValue(key, out cur)) return false;
            return true;
        }

        /// <summary>Each sim key's text on both sides.</summary>
        public static List<MapKeyCompare> Compare(Dictionary<string, object> source, Dictionary<string, object> exported)
        {
            var list = new List<MapKeyCompare>();
            foreach (string path in SimKeys)
            {
                object a = At(source, path, out bool fa), b = At(exported, path, out bool fb);
                string ta = fa ? MapJsonWriter.Write(a) : Missing, tb = fb ? MapJsonWriter.Write(b) : Missing;
                list.Add(new MapKeyCompare { path = path, inSource = fa, inExport = fb, same = ta == tb, sourceChars = fa ? ta.Length : 0, exportChars = fb ? tb.Length : 0 });
            }
            return list;
        }

        /// <summary>A copy of the map without its diagnostics (DiagnosticKeys), key order kept.</summary>
        public static Dictionary<string, object> StripDiagnostics(Dictionary<string, object> json)
        {
            var copy = Copy(json);
            foreach (string path in DiagnosticKeys)
            {
                string[] keys = path.Split('.');
                object cur = copy;
                for (int i = 0; i < keys.Length - 1 && cur != null; i++) cur = cur is Dictionary<string, object> d && d.TryGetValue(keys[i], out object next) ? next : null;
                if (cur is Dictionary<string, object> parent) parent.Remove(keys[keys.Length - 1]);
            }
            return copy;
        }

        static Dictionary<string, object> Copy(Dictionary<string, object> d)
        {
            var c = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> kv in d) c[kv.Key] = kv.Value is Dictionary<string, object> sub ? Copy(sub) : kv.Value;
            return c;
        }

        /// <summary>True when both maps print the same text once their diagnostics are stripped.</summary>
        public static bool SameWithoutDiagnostics(Dictionary<string, object> source, Dictionary<string, object> exported, out int firstDifference)
        {
            string a = MapJsonWriter.Write(StripDiagnostics(source)), b = MapJsonWriter.Write(StripDiagnostics(exported));
            firstDifference = -1;
            if (a == b) return true;
            int i = 0;
            while (i < a.Length && i < b.Length && a[i] == b[i]) i++;
            firstDifference = i;
            return false;
        }

        /// <summary>One line per sim key ("SAME path (n chars)" / "DIFF path ...").</summary>
        public static string Describe(List<MapKeyCompare> list)
        {
            var sb = new StringBuilder();
            foreach (MapKeyCompare c in list)
            {
                if (c.same) sb.Append($"SAME {c.path} ({(c.inSource ? c.sourceChars + " chars" : "absent on both sides")})\n");
                else sb.Append($"DIFF {c.path} (source {(c.inSource ? c.sourceChars + " chars" : "absent")}, export {(c.inExport ? c.exportChars + " chars" : "absent")})\n");
            }
            return sb.ToString();
        }
    }
}
